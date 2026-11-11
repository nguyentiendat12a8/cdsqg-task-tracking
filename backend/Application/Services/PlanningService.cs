using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Application.Services
{
    public interface IPlanningService
    {
        Task<PlanningGridResponseDto> GetDocumentPlanningGridAsync(Guid documentId, Guid? agencyId = null, int? reportYear = null, DocumentItemsQueryDto? filterQuery = null, bool paginate = false);
        Task<PagedResultDto<PlanningGridItemDto>> GetDocumentItemsPagedAsync(Guid documentId, DocumentItemsQueryDto queryDto);
        Task<GoalTaskItem> CreateGoalTaskItemAsync(CreateGoalTaskItemRequestDto dto);
        Task<bool> UpdateTaskCustomBaselineAsync(Guid taskId, UpdateCustomBaselineDto dto);
        Task<bool> UpdateYearlyTargetAsync(Guid taskId, UpdateYearlyTargetDto dto);
        Task<ImportYearlyTargetsResultDto> ImportYearlyTargetsAsync(Guid documentId, ImportYearlyTargetsRequestDto dto, Guid? userAgencyId, bool isAdmin);
        Task<AgencyProgressReportDto> GetAgencyProgressReportAsync(Guid agencyId, int reportYear, Guid? userAgencyId, string? userRole, string? tab = null, int page = 1, int pageSize = 15, bool fetchAll = false);
    }

    public class PlanningService : IPlanningService
    {
        private readonly AppDbContext _context;

        public PlanningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlanningGridResponseDto> GetDocumentPlanningGridAsync(Guid documentId, Guid? agencyId = null, int? reportYear = null, DocumentItemsQueryDto? filterQuery = null, bool paginate = false)
        {
            var doc = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId)
                ?? await _context.Documents.FirstOrDefaultAsync();

            if (doc == null)
            {
                doc = new Document
                {
                    Id = Guid.Parse("12660000-0000-0000-0000-000000001266"),
                    DocumentNumber = "1266/QĐ-TTg",
                    Name = "Quyết định số 1266/QĐ-TTg ngày 14/07/2026 của Thủ tướng Chính phủ",
                    StartYear = 2026,
                    EndYear = 2030,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Documents.Add(doc);
                await _context.SaveChangesAsync();
            }

            int startYear = doc.StartYear ?? 2026;
            int endYear = doc.EndYear ?? 2030;
            var dynamicYears = new List<int>();
            for (int y = startYear; y <= endYear; y++)
            {
                dynamicYears.Add(y);
            }

            var agenciesMap = await _context.Agencies.AsNoTracking().ToDictionaryAsync(a => a.Id);
            if (filterQuery?.SelectedAgencyIds?.Count > 0) filterQuery.SelectedAgencyIds = AgencyAssignments.ExpandSelection(filterQuery.SelectedAgencyIds, agenciesMap.Values.ToList());
            var query = _context.GoalTaskItems
                .Include(i => i.LeadAgency)
                .Include(i => i.Unit)
                .Include(i => i.Baselines)
                .Include(i => i.ProgressLogs)
                .Include(i => i.AgencyExecutions)
                .AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .AsQueryable();

            query = query.Where(i => i.DocumentId == doc.Id);
            if (filterQuery?.SelectedAgencyIds?.Count > 0)
                query = AgencyAssignments.Filter(query, filterQuery.SelectedAgencyIds, filterQuery.AssignmentRole);
            if (filterQuery?.AgencyId.HasValue == true && !string.Equals(filterQuery.UserRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var view = await AgencyScopePolicy.ResolveViewScopeAsync(_context, filterQuery.AgencyId.Value);
                query = AgencyAssignments.Filter(query, view.AgencyIds.ToList(), filterQuery.AssignmentRole);
            }
            if (AgencyAssignments.Coordinating(filterQuery?.AssignmentRole)) query = query.Where(i => i.AgencyExecutions.Any(e => e.IsCoordinatingAssignment));
            if (!string.IsNullOrWhiteSpace(filterQuery?.ItemType) && Enum.TryParse<ItemTypeEnum>(filterQuery.ItemType, true, out var kind)) query = query.Where(i => i.ItemType == kind);
            int? pagedTotalCount = null;
            if (paginate && filterQuery != null && DocumentItemQuery.CanPageInDatabase(filterQuery))
            {
                query = DocumentItemQuery.ApplyDatabase(query, filterQuery);
                pagedTotalCount = await query.CountAsync();
                var sqlPageSize = Math.Clamp(filterQuery.PageSize, 1, 1000);
                query = query.Skip((Math.Max(1, filterQuery.PageNumber) - 1) * sqlPageSize).Take(sqlPageSize);
            }
            var items = await query.ToListAsync();
            if (!items.Any())
            {
                items = await query.ToListAsync();
            }

            int gCount = 1;
            int tCount = 1;

            var itemsMap = items.ToDictionary(i => i.Id);
            // SQL-paged rows already have the requested order; do not reorder a page by update time.
            var rootItems = pagedTotalCount.HasValue ? items : items
                .OrderByDescending(i => i.LastUpdated ?? i.CreatedAt)
                .ThenByDescending(i => i.CreatedAt)
                .ToList();

            PlanningGridItemDto MapItemDto(GoalTaskItem item)
            {
                string safeCode = item.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(safeCode) || safeCode == "MT-" || safeCode == "NV-" || safeCode.EndsWith("-") || safeCode.Length <= 3)
                {
                    safeCode = item.ItemType == ItemTypeEnum.Goal ? $"MT-{gCount:D2}" : $"NV-{tCount:D2}";
                }
                if (item.ItemType == ItemTypeEnum.Goal) gCount++;
                else tCount++;

                var coordAgencies = item.CoordinatingAgencyIds
                    .Where(id => agenciesMap.ContainsKey(id))
                    .Select(id => agenciesMap[id])
                    .ToList();

                var yearlyTargets = new Dictionary<int, object?>();
                var defaultYearlyTargets = new Dictionary<int, object?>();
                var configuredYearlyTargets = new Dictionary<int, decimal?>();
                foreach (var year in dynamicYears)
                {
                    if (!PlanningYears.Contains(item, year))
                    {
                        yearlyTargets[year] = null;
                        defaultYearlyTargets[year] = null;
                        configuredYearlyTargets[year] = null;
                        continue;
                    }
                    var baseline = item.Baselines.FirstOrDefault(b => b.Year == year);
                    if (item.EvaluationType == EvaluationTypeEnum.Quantitative)
                    {
                        yearlyTargets[year] = ProgressCalculator.ResolveBaseline(item, year).Target;
                        defaultYearlyTargets[year] = ProgressCalculator.ResolveDefaultBaseline(item, year);
                        configuredYearlyTargets[year] = baseline?.ConfiguredTargetQuantity;
                    }
                    else
                    {
                        yearlyTargets[year] = baseline?.TargetQualitativeStatus?.ToString();
                        defaultYearlyTargets[year] = yearlyTargets[year];
                        configuredYearlyTargets[year] = null;
                    }
                }

                var role = filterQuery?.AssignmentRole ?? "Lead";
                var contextIds = filterQuery?.SelectedAgencyIds?.Count > 0
                    ? item.AgencyExecutions.Where(e => AgencyAssignments.HasRole(e, role) && filterQuery.SelectedAgencyIds.Contains(e.AgencyId)).Select(e => e.AgencyId).Distinct().ToList()
                    : agencyId.HasValue ? new List<Guid> { agencyId.Value } : AgencyAssignments.Coordinating(role) ? item.AgencyExecutions.Where(e => e.IsCoordinatingAssignment).Select(e => e.AgencyId).Distinct().ToList() : new List<Guid>();
                Guid? contextId = contextIds.Count == 1 ? contextIds[0] : agencyId;
                var latestLog = ProgressCalculator.AnnualReport(item, contextId, reportYear);
                var agencyDeliverables = reportYear.HasValue ? ProgressCalculator.AnnualDeliverables(item, latestLog) : GetDeliverablesForAgency(item, contextId);
                var progress = contextIds.Count > 0 ? ProgressCalculator.EvaluateRecipients(item, contextIds, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear))
                    : item.IsGeneralTask || item.AgencyExecutions.Any(e => e.IsLeadAssignment && e.IsLeadGroupAssignment) ? ProgressCalculator.EvaluateOverall(item, agenciesMap.Values, ProgressCalculator.AnnualEvaluationDate(reportYear), reportYear)
                    : ProgressCalculator.Evaluate(item, latestLog, agencyDeliverables, contextId, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear));

                return new PlanningGridItemDto
                {
                    LeadAssignmentAgencyIds = item.AgencyExecutions.Where(e => e.IsLeadAssignment).Select(e => e.AgencyId).ToList(),
                    CoordinatingAssignmentAgencyIds = item.AgencyExecutions.Where(e => e.IsCoordinatingAssignment).Select(e => e.AgencyId).ToList(),
                    ProgressAgencyId = contextId,
                    ProgressAgencyName = contextId.HasValue && agenciesMap.TryGetValue(contextId.Value, out var contextAgency) ? ProvinceCatalog.DisplayName(contextAgency) : null,
                    AssignmentKind = contextId.HasValue ? (AgencyAssignments.Coordinating(role) ? item.AgencyExecutions.FirstOrDefault(e => e.AgencyId == contextId)?.IsCoordinatingGroupAssignment : item.AgencyExecutions.FirstOrDefault(e => e.AgencyId == contextId)?.IsLeadGroupAssignment) == true ? "Group" : "Direct" : null,
                    TaskId = item.Id,
                    ItemType = item.ItemType.ToString(),
                    Code = safeCode,
                    Title = item.Title,
                    Category = item.Category,
                    Section = item.Section ?? string.Empty,
                    Group = item.Group ?? string.Empty,
                    IsOngoing = item.IsOngoing,
                    IsGeneralTask = item.IsGeneralTask,
                    StartDate = item.StartDate,
                    DueDate = item.DueDate,
                    LeadAgencyId = item.LeadAgencyId,
                    LeadAgencyCode = item.LeadAgency?.Code ?? string.Empty,
                    LeadAgencyName = item.LeadAgency?.Name ?? string.Empty,
                    LeadAgencyDisplayText = item.LeadAgencyDisplayText,
                    AssignedAgencyId = item.AssignedAgencyId,
                    AssignedAgencyCode = item.AssignedAgencyId.HasValue && agenciesMap.ContainsKey(item.AssignedAgencyId.Value)
                        ? agenciesMap[item.AssignedAgencyId.Value].Code
                        : null,
                    AssignedAgencyName = item.AssignedAgencyId.HasValue && agenciesMap.ContainsKey(item.AssignedAgencyId.Value)
                        ? agenciesMap[item.AssignedAgencyId.Value].Name
                        : null,
                    SubAgencyIds = item.SubAgencyIds,
                    SubAgencyNames = item.SubAgencyIds.Where(agenciesMap.ContainsKey).Select(id => agenciesMap[id].Name.Trim()).ToList(),
                    CoordinatingAgencyIds = item.CoordinatingAgencyIds,
                    CoordinatingAgencyCodes = coordAgencies.Select(a => a.Code).ToList(),
                    CoordinatingAgencyNames = coordAgencies.Select(a => a.Name).ToList(),
                    CoordinatingAgenciesDisplayText = item.CoordinatingAgenciesDisplayText,
                    ResultTarget = item.ResultTarget,
                    YearlyAllocations = GoalResultPlan.AnnualAllocations(item),
                    UnitId = item.UnitId,
                    EvaluationType = item.EvaluationType.ToString(),
                    UnitName = item.Unit?.Name ?? "%",
                    CalculationMethod = item.CalculationMethod.ToString(),
                    LatestProgressValue = progress.ActualValue,
                    CompletionPercentage = progress.Percentage,
                    ExpectedBaselineTarget = progress.Target,
                    LatestProgressStatus = latestLog?.QualitativeStatus?.ToString(),
                    LastUpdated = latestLog?.LogDate ?? item.LastUpdated ?? item.CreatedAt,
                    LatestReportAgencyName = latestLog?.AgencyId is Guid reportAgencyId && agenciesMap.TryGetValue(reportAgencyId, out var reportAgency)
                        ? ProvinceCatalog.DisplayName(reportAgency) : null,
                    LatestReportedBy = latestLog?.CreatedBy,
                    CreatedAt = item.CreatedAt,
                    HasPendingApproval = item.ProgressLogs != null && item.ProgressLogs.Any(l => l.ApprovalStatus == ApprovalStatusEnum.Pending && (!contextId.HasValue || l.AgencyId == contextId) && (!reportYear.HasValue || l.PeriodYear == reportYear)),
                    CalculatedStatus = progress.Status.ToString(),
                    DeadlineStatus = progress.DeadlineStatus,
                    CalculatedAlert = progress.Alert.ToString(),
                    CustomBaseline = item.CustomBaseline ?? new Dictionary<string, string>(),
                    Deliverables = agencyDeliverables ?? new List<TaskDeliverable>(),
                    YearlyTargets = yearlyTargets,
                    DefaultYearlyTargets = defaultYearlyTargets,
                    ConfiguredYearlyTargets = configuredYearlyTargets
                };
            }

            var gridItems = rootItems.Select(MapItemDto).ToList();

            return new PlanningGridResponseDto
            {
                PagedTotalCount = pagedTotalCount,
                DocumentId = doc.Id,
                DocumentNumber = doc.DocumentNumber,
                DocumentName = doc.Name,
                TimeResolution = doc.TimeResolution.ToString(),
                StartYear = startYear,
                EndYear = endYear,
                DynamicYears = dynamicYears,
                Items = gridItems
            };
        }

        public async Task<PagedResultDto<PlanningGridItemDto>> GetDocumentItemsPagedAsync(Guid documentId, DocumentItemsQueryDto queryDto)
        {
            queryDto.PageNumber = Math.Max(1, queryDto.PageNumber);
            queryDto.PageSize = queryDto.PageSize > 0 ? Math.Min(1000, queryDto.PageSize) : 10;
            // Admin's account agency must not scope the official progress of
            // assignments performed by other agencies. Agency filters below
            // select rows; they do not change the reporting owner.
            var progressAgencyId = string.Equals(queryDto.UserRole, "Admin", StringComparison.OrdinalIgnoreCase)
                ? (Guid?)null : queryDto.AgencyId;
            var gridResponse = await GetDocumentPlanningGridAsync(documentId, progressAgencyId, filterQuery: queryDto, paginate: true);
            var list = gridResponse.Items ?? new List<PlanningGridItemDto>();

            // 1. Scoping for non-admin user accounts
            if (!string.Equals(queryDto.UserRole, "Admin", StringComparison.OrdinalIgnoreCase) && queryDto.AgencyId.HasValue && queryDto.AgencyId.Value != Guid.Empty)
            {
                var scope = await AgencyScopePolicy.ResolveViewScopeAsync(_context, queryDto.AgencyId.Value);
                list = list.Where(scope.CanView).ToList();
            }

            if (!gridResponse.PagedTotalCount.HasValue) list = DocumentItemQuery.Apply(list, queryDto);

            int totalCount = gridResponse.PagedTotalCount ?? list.Count;
            int pageNum = queryDto.PageNumber > 0 ? queryDto.PageNumber : 1;
            int pageSize = queryDto.PageSize > 0 ? queryDto.PageSize : 10;
            var pagedItems = gridResponse.PagedTotalCount.HasValue ? list : list.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultDto<PlanningGridItemDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = pageNum,
                PageSize = pageSize
            };
        }

        public async Task<GoalTaskItem> CreateGoalTaskItemAsync(CreateGoalTaskItemRequestDto dto)
        {
            Enum.TryParse<ItemTypeEnum>(dto.ItemType, true, out var itemType);
            Enum.TryParse<EvaluationTypeEnum>(dto.EvaluationType, true, out var evalType);
            Enum.TryParse<CalculationMethodEnum>(dto.CalculationMethod, true, out var calcMethod);

            // Auto-increment code generation logic
            string code = dto.Code?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(code) || code.StartsWith("MT-") || code.StartsWith("NV-") || code.Length <= 4)
            {
                if (itemType == ItemTypeEnum.Goal)
                {
                    var existingCodes = await _context.GoalTaskItems
                        .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Goal)
                        .Select(i => i.Code)
                        .ToListAsync();

                    int maxNum = 0;
                    foreach (var c in existingCodes)
                    {
                        if (!string.IsNullOrEmpty(c) && c.StartsWith("MT-"))
                        {
                            if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                        }
                    }
                    code = $"MT-{(maxNum + 1):D2}";
                }
                else
                {
                    var existingCodes = await _context.GoalTaskItems
                        .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Task)
                        .Select(i => i.Code)
                        .ToListAsync();

                    int maxNum = 0;
                    foreach (var c in existingCodes)
                    {
                        if (!string.IsNullOrEmpty(c) && c.StartsWith("NV-"))
                        {
                            if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                        }
                    }
                    code = $"NV-{(maxNum + 1):D2}";
                }
            }
            else
            {
                // Check if user provided code is already used by another item
                bool exists = await _context.GoalTaskItems.AnyAsync(i => i.DocumentId == dto.DocumentId && i.Code == code);
                if (exists)
                {
                    if (itemType == ItemTypeEnum.Goal)
                    {
                        var existingCodes = await _context.GoalTaskItems
                            .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Goal)
                            .Select(i => i.Code)
                            .ToListAsync();
                        int maxNum = 0;
                        foreach (var c in existingCodes)
                        {
                            if (!string.IsNullOrEmpty(c) && c.StartsWith("MT-"))
                            {
                                if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                            }
                        }
                        code = $"MT-{(maxNum + 1):D2}";
                    }
                    else
                    {
                        var existingCodes = await _context.GoalTaskItems
                            .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Task)
                            .Select(i => i.Code)
                            .ToListAsync();
                        int maxNum = 0;
                        foreach (var c in existingCodes)
                        {
                            if (!string.IsNullOrEmpty(c) && c.StartsWith("NV-"))
                            {
                                if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                            }
                        }
                        code = $"NV-{(maxNum + 1):D2}";
                    }
                }
            }

            Guid? unitIdToAssign = dto.UnitId;
            if (!unitIdToAssign.HasValue && !string.IsNullOrWhiteSpace(dto.UnitName))
            {
                var targetUnit = await _context.Units.FirstOrDefaultAsync(u => u.Name == dto.UnitName || (dto.UnitName == "%" && u.Code == "PERCENT"));
                if (targetUnit != null) unitIdToAssign = targetUnit.Id;
            }

            var item = new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                DocumentId = dto.DocumentId,
                ItemType = itemType,
                Code = code,
                Title = dto.Title,
                Category = dto.Category ?? "Chính phủ số",
                Section = dto.Section ?? string.Empty,
                Group = dto.Group ?? string.Empty,
                IsOngoing = dto.IsOngoing,
                IsGeneralTask = dto.IsGeneralTask,
                StartDate = dto.IsOngoing ? (dto.StartDate ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) : dto.StartDate,
                DueDate = dto.IsOngoing ? (dto.DueDate ?? new DateTime(2030, 12, 31, 0, 0, 0, DateTimeKind.Utc)) : dto.DueDate,
                LeadAgencyId = dto.LeadAgencyId,
                CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>(),
                ResultTarget = dto.ResultTarget,
                UnitId = unitIdToAssign,
                EvaluationType = evalType,
                CalculationMethod = calcMethod,
                CreatedAt = DateTime.UtcNow
            };

            GoalResultPlan.Validate(item);
            _context.GoalTaskItems.Add(item);
            await AgencyAssignments.SyncAsync(_context, item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateTaskCustomBaselineAsync(Guid taskId, UpdateCustomBaselineDto dto)
        {
            var task = await _context.GoalTaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ/Mục tiêu với ID: {taskId}");
            }

            ProgressCalculator.ValidateAnnualBaselines(dto.Milestones);
            PlanningYears.ValidateTargets(task, dto.Milestones?.Keys.Select(int.Parse) ?? Enumerable.Empty<int>());
            task.CustomBaseline = dto.Milestones ?? new Dictionary<string, string>();
            await ProgressCalculator.RefreshCachesAsync(_context, task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateYearlyTargetAsync(Guid taskId, UpdateYearlyTargetDto dto)
        {
            if (dto.Year is < 1900 or > 9999 || dto.TargetQuantity <= 0)
                throw new InvalidOperationException("Năm không hợp lệ hoặc chỉ tiêu không lớn hơn 0.");
            var task = await _context.GoalTaskItems
                .Include(t => t.Baselines)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ/Mục tiêu với ID: {taskId}");
            }

            PlanningYears.ValidateTargets(task, new[] { dto.Year });

            var baseline = task.Baselines.FirstOrDefault(b => b.Year == dto.Year);
            if (baseline == null)
            {
                baseline = new TargetBaseline
                {
                    Id = Guid.NewGuid(),
                    GoalTaskId = taskId,
                    Year = dto.Year
                };
                _context.TargetBaselines.Add(baseline);
            }

            if (task.ItemType == ItemTypeEnum.Goal && task.EvaluationType == EvaluationTypeEnum.Quantitative)
                baseline.ConfiguredTargetQuantity = dto.TargetQuantity;
            else
                baseline.TargetQuantity = dto.TargetQuantity;
            // Editing the annual grid explicitly replaces that year's custom override.
            var overrides = new Dictionary<string, string>(task.CustomBaseline);
            overrides.Remove(dto.Year.ToString(System.Globalization.CultureInfo.InvariantCulture));
            task.CustomBaseline = overrides;

            if (!string.IsNullOrEmpty(dto.TargetQualitativeStatus))
            {
                if (Enum.TryParse<TextStatusEnum>(dto.TargetQualitativeStatus, true, out var parsedStatus))
                {
                    baseline.TargetQualitativeStatus = parsedStatus;
                }
            }

            await ProgressCalculator.RefreshCachesAsync(_context, task);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Nhập kế hoạch mục tiêu theo năm từ Excel. Đơn vị chỉ nhập cho mục tiêu định lượng mình trực tiếp
        /// chủ trì; mục tiêu giao chung (vd. MT-24/1266) không có kế hoạch riêng. Kế hoạch đã có khác số mới
        /// chỉ được ghi đè khi ConfirmOverwrite = true. Không lưu gì nếu có dòng lỗi.
        /// </summary>
        public async Task<ImportYearlyTargetsResultDto> ImportYearlyTargetsAsync(Guid documentId, ImportYearlyTargetsRequestDto dto, Guid? userAgencyId, bool isAdmin)
        {
            var rows = dto.Rows ?? new List<ImportYearlyTargetRowDto>();
            if (rows.Count == 0) throw new InvalidOperationException("File không có số liệu kế hoạch mục tiêu để nhập.");
            if (!isAdmin && (!userAgencyId.HasValue || userAgencyId == Guid.Empty))
                throw new UnauthorizedAccessException("Tài khoản chưa gắn với cơ quan.");
            if (rows.GroupBy(r => (r.TaskId, r.Year)).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Mỗi mục tiêu chỉ được nhập một kế hoạch cho mỗi năm.");

            if (dto.ReportingAgencyId.HasValue)
            {
                var destination = await _context.Agencies.FindAsync(dto.ReportingAgencyId.Value);
                if (destination == null || !destination.IsActive || destination.ParentId.HasValue || ReportingAgencyGroups.IsGroup(destination))
                    throw new InvalidOperationException("Đơn vị nhập không được cập nhật kế hoạch mục tiêu của cơ quan chủ trì.");
                if (!isAdmin && !(await AgencyScopePolicy.ResolveViewScopeAsync(_context, userAgencyId!.Value)).AgencyIds.Contains(destination.Id))
                    throw new UnauthorizedAccessException("Đơn vị nhập nằm ngoài phạm vi được phép.");
                var assignedIds = await AgencyAssignments.Filter(_context.GoalTaskItems.Where(t => t.DocumentId == documentId), new[] { destination.Id })
                    .Select(t => t.Id).ToListAsync();
                if (rows.Any(r => !assignedIds.Contains(r.TaskId)))
                    throw new InvalidOperationException("Có mục tiêu không được phân giao cho đơn vị nhập đã chọn.");
            }

            var ids = rows.Select(r => r.TaskId).Distinct().ToList();
            var tasks = await _context.GoalTaskItems
                .Include(t => t.Baselines).Include(t => t.Unit).Include(t => t.Document).Include(t => t.AgencyExecutions)
                .Where(t => t.DocumentId == documentId && ids.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);
            var ownAgencyIds = new HashSet<Guid>();
            if (!isAdmin)
            {
                ownAgencyIds.Add(userAgencyId!.Value);
                ownAgencyIds.UnionWith(await _context.Agencies.Where(a => a.ParentId == userAgencyId).Select(a => a.Id).ToListAsync());
            }

            var errors = new List<string>();
            var result = new ImportYearlyTargetsResultDto();
            var changes = new List<(GoalTaskItem Task, ImportYearlyTargetRowDto Row)>();
            foreach (var row in rows.OrderBy(r => r.TaskId).ThenBy(r => r.Year))
            {
                if (!tasks.TryGetValue(row.TaskId, out var task)) { errors.Add("Có mục tiêu không thuộc văn bản đang chọn."); continue; }
                var label = $"{task.Code} năm {row.Year}";
                if (task.ItemType != ItemTypeEnum.Goal || task.EvaluationType != EvaluationTypeEnum.Quantitative)
                { errors.Add($"{label}: chỉ nhập kế hoạch cho mục tiêu định lượng."); continue; }
                if (task.IsGeneralTask || task.AgencyExecutions.Any(e => e.IsLeadAssignment && e.IsLeadGroupAssignment))
                { errors.Add($"{label}: mục tiêu giao chung không nhập kế hoạch riêng."); continue; }
                if (!isAdmin && !task.AgencyExecutions.Any(e => e.IsLeadAssignment && ownAgencyIds.Contains(e.AgencyId)))
                { errors.Add($"{label}: cơ quan không chủ trì mục tiêu này."); continue; }
                if (row.Year is < 1900 or > 9999 || !PlanningYears.Contains(task, row.Year))
                { errors.Add($"{label}: năm nằm ngoài thời gian thực hiện."); continue; }
                if (row.TargetQuantity <= 0) { errors.Add($"{label}: kế hoạch phải lớn hơn 0."); continue; }

                var yearKey = row.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var baseline = task.Baselines.FirstOrDefault(b => b.Year == row.Year);
                var hasExplicit = task.CustomBaseline.ContainsKey(yearKey) || baseline?.ConfiguredTargetQuantity > 0;
                var current = hasExplicit ? ProgressCalculator.ResolveBaseline(task, row.Year).Target : null;
                if (current == row.TargetQuantity) { result.UnchangedCount++; continue; }
                if (current.HasValue)
                    result.Conflicts.Add(new YearlyTargetConflictDto { TaskId = task.Id, Code = task.Code, Title = task.Title, Year = row.Year, CurrentTarget = current, NewTarget = row.TargetQuantity });
                changes.Add((task, row));
            }
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(" ", errors.Distinct().Take(20)));
            if (result.Conflicts.Count > 0 && !dto.ConfirmOverwrite)
            {
                result.RequiresConfirmation = true;
                return result;
            }

            foreach (var (task, row) in changes)
            {
                var baseline = task.Baselines.FirstOrDefault(b => b.Year == row.Year);
                if (baseline == null)
                {
                    baseline = new TargetBaseline { Id = Guid.NewGuid(), GoalTaskId = task.Id, Year = row.Year };
                    _context.TargetBaselines.Add(baseline);
                    task.Baselines.Add(baseline);
                }
                baseline.ConfiguredTargetQuantity = row.TargetQuantity;
                var overrides = new Dictionary<string, string>(task.CustomBaseline);
                overrides.Remove(row.Year.ToString(System.Globalization.CultureInfo.InvariantCulture));
                task.CustomBaseline = overrides;
            }
            foreach (var task in changes.Select(c => c.Task).Distinct())
                await ProgressCalculator.RefreshCachesAsync(_context, task);
            await _context.SaveChangesAsync();
            result.SavedCount = changes.Count;
            return result;
        }

        public static List<TaskDeliverable> GetDeliverablesForAgency(GoalTaskItem item, Guid? agencyId)
        {
            if (item == null) return new List<TaskDeliverable>();
            var masterList = item.Deliverables ?? new List<TaskDeliverable>();

            var approved = ProgressCalculator.LatestApproved(item, agencyId);
            if (approved?.Deliverables is { Count: > 0 }) return approved.Deliverables;
            if (!item.IsGeneralTask) return masterList;

            return masterList.Select(d => new TaskDeliverable
            {
                Id = d.Id,
                Title = d.Title,
                DueDate = d.DueDate,
                CurrentStatus = "NotStarted",
                DocumentNumber = null,
                PromulgationDate = null,
                AttachmentUrl = null,
                AttachmentName = null
            }).ToList();
        }

        public static ProgressLog? GetLatestProgressLogForAgency(GoalTaskItem item, Guid? agencyId)
            => ProgressCalculator.LatestApproved(item, agencyId);

        public static ExecutionStatusEnum CalculateExecutionStatus(GoalTaskItem item, ProgressLog? latestLog, List<TaskDeliverable>? customDeliverables = null)
            => ProgressCalculator.Evaluate(item, latestLog, customDeliverables, latestLog?.AgencyId).Status;

        private static string FormatTargetClean(decimal? val, string? fallback)
        {
            if (val.HasValue)
            {
                return val.Value % 1 == 0
                    ? ((long)val.Value).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : val.Value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrWhiteSpace(fallback) && decimal.TryParse(fallback, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                return d % 1 == 0
                    ? ((long)d).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : d.ToString("G29", System.Globalization.CultureInfo.InvariantCulture);
            }
            return fallback ?? "";
        }

        public async Task<AgencyProgressReportDto> GetAgencyProgressReportAsync(
            Guid agencyId, 
            int reportYear, 
            Guid? userAgencyId, 
            string? userRole,
            string? tab = null,
            int page = 1,
            int pageSize = 15,
            bool fetchAll = false)
        {
            var agency = await _context.Agencies.AsNoTracking().FirstOrDefaultAsync(a => a.Id == agencyId);
            if (agency == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ quan với ID: {agencyId}");
            }

            if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!userAgencyId.HasValue || userAgencyId.Value == Guid.Empty)
                    throw new UnauthorizedAccessException("Người dùng chưa được gán cơ quan.");
                var scope = await AgencyScopePolicy.ResolveViewScopeAsync(_context, userAgencyId.Value);
                if (!scope.AgencyIds.Contains(agencyId))
                    throw new UnauthorizedAccessException("Bạn không có quyền truy cập báo cáo của cơ quan này.");
            }

            var doc1266Id = Guid.Parse("12660000-0000-0000-0000-000000001266");
            var doc2629Id = Guid.Parse("26290000-0000-0000-0000-000000002629");

            var reportItemsQuery = _context.GoalTaskItems
                .Include(i => i.Document)
                .Include(i => i.Unit)
                .Include(i => i.Baselines)
                .Include(i => i.ProgressLogs)
                .Include(i => i.AgencyExecutions)
                .Where(i => i.DocumentId == doc1266Id || i.DocumentId == doc2629Id);

            // Use persisted lead assignments, never a substring in a display label.
            var agencyItems = await AgencyAssignments.Filter(reportItemsQuery, new[] { agencyId })
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync();

            var agenciesMap = await _context.Agencies.AsNoTracking().ToDictionaryAsync(a => a.Id);

            bool loadAll = fetchAll || string.IsNullOrWhiteSpace(tab);
            bool loadGoals = loadAll || string.Equals(tab, "goals", StringComparison.OrdinalIgnoreCase);
            bool loadTasks = loadAll || string.Equals(tab, "tasks", StringComparison.OrdinalIgnoreCase);
            string activeTab = string.Equals(tab, "tasks", StringComparison.OrdinalIgnoreCase) ? "tasks" : "goals";

            static string NormalizeTitle(string? t)
            {
                if (string.IsNullOrWhiteSpace(t)) return "";
                var s = t.Trim().TrimEnd('.').ToLowerInvariant();
                return System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ");
            }

            var allGoalItems = agencyItems
                .Where(i => i.ItemType == ItemTypeEnum.Goal)
                .OrderByDescending(i => i.DocumentId == doc2629Id)
                .ThenBy(i => i.Code)
                .ToList();

            var mergedGoalsList = new List<AgencyProgressGoalItemDto>();
            int totalGoals = 0;

            if (loadGoals)
            {
                var goalsList = new List<AgencyProgressGoalItemDto>();

            var sortedGoals = agencyItems
                .Where(i => i.ItemType == ItemTypeEnum.Goal)
                .OrderByDescending(i => i.DocumentId == doc2629Id)
                .ThenBy(i => i.Code)
                .ToList();

            foreach (var g in sortedGoals)
            {
                bool is2629 = g.DocumentId == doc2629Id;
                string docNum = is2629 ? "2629/QĐ-TTg" : "1266/QĐ-TTg";

                var baselines = g.Baselines ?? new List<TargetBaseline>();
                // The decision targets and the configured annual plan are separate data sources.
                var bMap = baselines.ToDictionary(b => b.Year, b => b.TargetQuantity);

                var pLogs = g.ProgressLogs?.Where(p => p.AgencyId == agencyId && (p.PeriodYear == 2025 || p.PeriodYear == 2026))
                    .OrderByDescending(p => p.LogDate)
                    .ToList() ?? new List<ProgressLog>();

                var p2025 = pLogs.FirstOrDefault(p => p.PeriodYear == 2025)?.QuantitativeValue;
                var p2026 = pLogs.FirstOrDefault(p => p.PeriodYear == 2026)?.QuantitativeValue;
                var latestNote = pLogs.FirstOrDefault()?.SummaryNotes;

                var cb = g.CustomBaseline ?? new Dictionary<string, string>();

                string? tPhase1 = null;
                string? tPhase2 = null;
                var applicableYears = new List<int>();

                if (is2629)
                {
                    bool isPhase1 = (g.Section != null && g.Section.Contains("2025-2027")) ||
                                    (g.DueDate.HasValue && g.DueDate.Value.Year <= 2027) ||
                                    bMap.ContainsKey(2027);

                    if (isPhase1)
                    {
                        tPhase1 = FormatTargetClean(bMap.ContainsKey(2027) ? bMap[2027] : null, cb.ContainsKey("2027") ? cb["2027"] : g.ResultTarget?.ToString());
                        applicableYears.AddRange(new[] { 2025, 2026, 2027 });
                    }
                    else
                    {
                        tPhase2 = FormatTargetClean(bMap.ContainsKey(2030) ? bMap[2030] : null, cb.ContainsKey("2030") ? cb["2030"] : g.ResultTarget?.ToString());
                        applicableYears.AddRange(new[] { 2028, 2029, 2030 });
                    }
                }
                else
                {
                    bool isPhase2Only = (g.Section != null && g.Section.Contains("2045")) ||
                                        (g.DueDate.HasValue && g.DueDate.Value.Year > 2030);

                    tPhase1 = FormatTargetClean(bMap.ContainsKey(2030) ? bMap[2030] : null, cb.ContainsKey("2030") ? cb["2030"] : (isPhase2Only ? null : g.ResultTarget?.ToString()));
                    if (isPhase2Only)
                    {
                        tPhase2 = FormatTargetClean(bMap.ContainsKey(2045) ? bMap[2045] : null, cb.ContainsKey("2045") ? cb["2045"] : g.ResultTarget?.ToString());
                    }
                    applicableYears.AddRange(new[] { 2026, 2027, 2028, 2029, 2030 });
                }

                string? GetPlanVal(int yr)
                {
                    if (!applicableYears.Contains(yr)) return "—";
                    var configured = baselines.FirstOrDefault(b => b.Year == yr)?.ConfiguredTargetQuantity;
                    if (configured.HasValue) return FormatTargetClean(configured, null);
                    return "";
                }

                goalsList.Add(new AgencyProgressGoalItemDto
                {
                    Id = g.Id,
                    DocumentId = g.DocumentId,
                    DocumentNumber = docNum,
                    Code = g.Code,
                    Title = g.Title,
                    UnitName = g.Unit?.Name ?? "%",
                    Section = g.Section,
                    TargetPhase1 = tPhase1,
                    TargetPhase2 = tPhase2,
                    Actual2025 = applicableYears.Contains(2025) ? p2025 : null,
                    Estimate2026 = applicableYears.Contains(2026) ? p2026 : null,
                    Plan2027 = GetPlanVal(2027),
                    Plan2028 = GetPlanVal(2028),
                    Plan2029 = GetPlanVal(2029),
                    Plan2030 = GetPlanVal(2030),
                    Notes = latestNote,
                    ApplicableYears = applicableYears
                });
            }

            // Group and merge multi-phase goals that share the same title
            var goalGroups = goalsList.GroupBy(g => (g.DocumentId, NormalizeTitle(g.Title))).ToList();

            foreach (var group in goalGroups)
            {
                var items = group.OrderBy(i => i.Code).ToList();
                if (items.Count == 1)
                {
                    items[0].SubCodes = new List<string> { items[0].Code };
                    items[0].SubGoalIds = new List<Guid> { items[0].Id };
                    mergedGoalsList.Add(items[0]);
                }
                else
                {
                    var p1 = items.FirstOrDefault(x => !string.IsNullOrEmpty(x.TargetPhase1) && x.TargetPhase1 != "—") ?? items[0];
                    var p2 = items.FirstOrDefault(x => x != p1) ?? items[items.Count - 1];

                    var merged = new AgencyProgressGoalItemDto
                    {
                        Id = p1.Id,
                        DocumentId = p1.DocumentId,
                        DocumentNumber = p1.DocumentNumber,
                        Code = $"{p1.Code}\n{p2.Code}",
                        SubCodes = items.Select(x => x.Code).Distinct().ToList(),
                        SubGoalIds = items.Select(x => x.Id).Distinct().ToList(),
                        Title = p1.Title,
                        UnitName = !string.IsNullOrEmpty(p1.UnitName) ? p1.UnitName : (p2.UnitName ?? "%"),
                        Section = p1.Section,
                        TargetPhase1 = !string.IsNullOrEmpty(p1.TargetPhase1) && p1.TargetPhase1 != "—" ? p1.TargetPhase1 : p2.TargetPhase1,
                        TargetPhase2 = !string.IsNullOrEmpty(p2.TargetPhase2) && p2.TargetPhase2 != "—" ? p2.TargetPhase2 : p1.TargetPhase2,
                        Actual2025 = p1.Actual2025 ?? p2.Actual2025,
                        Estimate2026 = p1.Estimate2026 ?? p2.Estimate2026,
                        Plan2027 = p1.Plan2027 != null && p1.Plan2027 != "—" ? p1.Plan2027 : p2.Plan2027,
                        Plan2028 = p2.Plan2028 != null && p2.Plan2028 != "—" ? p2.Plan2028 : p1.Plan2028,
                        Plan2029 = p2.Plan2029 != null && p2.Plan2029 != "—" ? p2.Plan2029 : p1.Plan2029,
                        Plan2030 = p2.Plan2030 != null && p2.Plan2030 != "—" ? p2.Plan2030 : p1.Plan2030,
                        Notes = string.Join("; ", items.Select(x => x.Notes).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct()),
                        ApplicableYears = items.SelectMany(x => x.ApplicableYears).Distinct().OrderBy(y => y).ToList()
                    };
                    mergedGoalsList.Add(merged);
                }
            }

            int seq2629 = 1;
            int seq1266 = 1;
            foreach (var mg in mergedGoalsList)
            {
                if (mg.DocumentNumber.Contains("2629")) mg.SequenceNumber = seq2629++;
                else mg.SequenceNumber = seq1266++;
            }

            totalGoals = mergedGoalsList.Count;
        }
        else
        {
            totalGoals = allGoalItems.GroupBy(g => (g.DocumentId, NormalizeTitle(g.Title))).Count();
        }

        var allTaskItems = agencyItems
            .Where(i => i.ItemType == ItemTypeEnum.Task)
            .OrderByDescending(i => i.DocumentId == doc2629Id)
            .ThenBy(i => i.Code)
            .ToList();

        int totalTasks = allTaskItems.Count;
        var finalTasksList = new List<AgencyProgressTaskItemDto>();

        if (loadTasks)
        {
            var taskSeqMap = new Dictionary<Guid, int>();
            int tSeq2629 = 1;
            int tSeq1266 = 1;
            foreach (var t in allTaskItems)
            {
                if (t.DocumentId == doc2629Id) taskSeqMap[t.Id] = tSeq2629++;
                else taskSeqMap[t.Id] = tSeq1266++;
            }

            var tasksToProcess = fetchAll
                ? allTaskItems
                : allTaskItems.Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToList();

            foreach (var t in tasksToProcess)
            {
                bool is2629 = t.DocumentId == doc2629Id;
                string docNum = is2629 ? "2629/QĐ-TTg" : "1266/QĐ-TTg";

                var latestLog = PlanningService.GetLatestProgressLogForAgency(t, agencyId);
                var eval = ProgressCalculator.Evaluate(t, latestLog, null, agencyId);

                string statusKey = eval.Status.ToString();
                string statusLabel = statusKey switch
                {
                    "Completed" => "Đã hoàn thành",
                    "InProgress" => "Đang thực hiện",
                    _ => "Chưa thực hiện"
                };

                var coordNames = t.CoordinatingAgencyIds
                    .Where(id => agenciesMap.ContainsKey(id))
                    .Select(id => agenciesMap[id].Name)
                    .ToList();
                string coordText = !string.IsNullOrWhiteSpace(t.CoordinatingAgenciesDisplayText)
                    ? t.CoordinatingAgenciesDisplayText
                    : string.Join(", ", coordNames);

                string deliverablesText = t.Deliverables != null && t.Deliverables.Any()
                    ? string.Join("; ", t.Deliverables.Select(d => d.Title))
                    : "";

                finalTasksList.Add(new AgencyProgressTaskItemDto
                {
                    SequenceNumber = taskSeqMap.ContainsKey(t.Id) ? taskSeqMap[t.Id] : 0,
                    Id = t.Id,
                    DocumentId = t.DocumentId,
                    DocumentNumber = docNum,
                    Code = t.Code,
                    Title = t.Title,
                    CoordinatingAgencies = coordText,
                    DueDate = t.DueDate?.ToString("dd/MM/yyyy"),
                    Status = statusKey,
                    StatusLabel = statusLabel,
                    CompletionPercentage = eval.Percentage,
                    Deliverables = deliverablesText,
                    Notes = latestLog?.SummaryNotes
                });
            }
        }

        var pagedGoals = fetchAll || !loadGoals
            ? mergedGoalsList
            : mergedGoalsList.Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToList();

        int totalCount = activeTab == "tasks" ? totalTasks : totalGoals;
        int actualPageSize = pageSize > 0 ? pageSize : 15;
        int totalPages = fetchAll ? 1 : Math.Max(1, (int)Math.Ceiling((double)totalCount / actualPageSize));
        int currentPage = fetchAll ? 1 : Math.Max(1, page);

        return new AgencyProgressReportDto
        {
            AgencyId = agency.Id,
            AgencyName = agency.Name,
            AgencyCode = agency.Code,
            AgencyType = agency.Type.ToString(),
            ReportYear = reportYear,
            Page = currentPage,
            PageSize = actualPageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            TotalGoals = totalGoals,
            TotalTasks = totalTasks,
            ActiveTab = activeTab,
            Goals = pagedGoals,
            Tasks = finalTasksList
        };
        }

    }
}
