using Cdsqg.Application.DTOs;
using Cdsqg.Core.Entities;
namespace Cdsqg.Application.Services;

// Single source for document listing and export: filter/sort before pagination.
public static class DocumentItemQuery
{
    // Derived status/progress filters evaluate the matching set; simple list sorts page in SQL.
    public static bool CanPageInDatabase(DocumentItemsQueryDto dto) => string.IsNullOrWhiteSpace(dto.Search)
        && dto.SelectedStatuses?.Count is not > 0 && (dto.SortBy.ToLowerInvariant() is "code" or "title");

    public static IQueryable<GoalTaskItem> ApplyDatabase(IQueryable<GoalTaskItem> query, DocumentItemsQueryDto dto)
    {
        query = ApplyCommon(query, dto);
        bool asc = !string.Equals(dto.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return dto.SortBy.Equals("title", StringComparison.OrdinalIgnoreCase)
            ? asc ? query.OrderBy(i => i.Title).ThenBy(i => i.Id) : query.OrderByDescending(i => i.Title).ThenBy(i => i.Id)
            : asc ? query.OrderBy(i => i.Code.ToLower()).ThenBy(i => i.Id) : query.OrderByDescending(i => i.Code.ToLower()).ThenBy(i => i.Id);
    }

    private static IQueryable<T> ApplyCommon<T>(IQueryable<T> query, DocumentItemsQueryDto dto) where T : IPlanningFilterItem
    {
        if (dto.SelectedSubAgencyIds?.Count > 0) query = query.Where(i => i.AssignedAgencyId.HasValue && dto.SelectedSubAgencyIds.Contains(i.AssignedAgencyId.Value));
        if (dto.SelectedSections?.Count > 0) query = query.Where(i => i.Section != null && dto.SelectedSections.Contains(i.Section));
        if (dto.SelectedGroups?.Count > 0) query = query.Where(i => i.Group != null && dto.SelectedGroups.Contains(i.Group));
        if (dto.SelectedScopes?.Count > 0)
        {
            bool general = dto.SelectedScopes.Contains("general"), specific = dto.SelectedScopes.Contains("specific");
            query = query.Where(i => general && i.IsGeneralTask || specific && !i.IsGeneralTask);
        }
        var reportFrom = dto.ReportFromYear ?? dto.FromYear;
        var reportTo = dto.ReportToYear ?? dto.ToYear;
        if (reportFrom.HasValue || reportTo.HasValue)
        {
            int from = reportFrom ?? 1900, to = reportTo ?? 9999;
            query = query.Where(i => i.IsOngoing && dto.OnlyOngoing || !i.IsOngoing &&
                (i.DueDate.HasValue ? i.DueDate.Value.Year : i.StartDate.HasValue ? i.StartDate.Value.Year : 2026) >= from
                && (i.DueDate.HasValue ? i.DueDate.Value.Year : i.StartDate.HasValue ? i.StartDate.Value.Year : 2026) <= to);
        }
        else if (dto.OnlyOngoing) query = query.Where(i => i.IsOngoing);

        if (dto.ExecutionFrom.HasValue)
        {
            var from = dto.ExecutionFrom.Value.Date;
            query = query.Where(i => i.DueDate.HasValue && i.DueDate.Value >= from);
        }
        if (dto.ExecutionTo.HasValue)
        {
            var toExclusive = dto.ExecutionTo.Value.Date.AddDays(1);
            query = query.Where(i => i.DueDate.HasValue && i.DueDate.Value < toExclusive);
        }
        return query;
    }

    public static List<PlanningGridItemDto> Apply(List<PlanningGridItemDto> list, DocumentItemsQueryDto queryDto)
    {
            // 2. ItemType filter (Goal vs Task)
            if (!string.IsNullOrWhiteSpace(queryDto.ItemType))
            {
                var targetType = queryDto.ItemType.Trim();
                if (targetType.Equals("Goal", StringComparison.OrdinalIgnoreCase) || targetType == "1")
                {
                    list = list.Where(i => string.Equals(i.ItemType, "Goal", StringComparison.OrdinalIgnoreCase) || i.ItemType == "1").ToList();
                }
                else if (targetType.Equals("Task", StringComparison.OrdinalIgnoreCase) || targetType == "2")
                {
                    list = list.Where(i => string.Equals(i.ItemType, "Task", StringComparison.OrdinalIgnoreCase) || i.ItemType == "2").ToList();
                }
            }

            // 3. Search query
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim().ToLower();
                list = list.Where(i =>
                    (i.Code != null && i.Code.ToLower().Contains(s)) ||
                    (i.Title != null && i.Title.ToLower().Contains(s)) ||
                    (i.LeadAgencyName != null && i.LeadAgencyName.ToLower().Contains(s)) ||
                    (i.AssignedAgencyName != null && i.AssignedAgencyName.ToLower().Contains(s)) ||
                    (i.CoordinatingAgencyNames != null && i.CoordinatingAgencyNames.Any(c => c.ToLower().Contains(s)))
                ).ToList();
            }

            // 4. Filters
            if (queryDto.SelectedAgencyIds != null && queryDto.SelectedAgencyIds.Count > 0)
            {
                var ids = queryDto.SelectedAgencyIds.ToHashSet();
                list = list.Where(i => (AgencyAssignments.Coordinating(queryDto.AssignmentRole) ? i.CoordinatingAssignmentAgencyIds : i.LeadAssignmentAgencyIds).Any(ids.Contains)).ToList();
            }

            list = ApplyCommon(list.AsQueryable(), queryDto).ToList();

            if (queryDto.SelectedStatuses != null && queryDto.SelectedStatuses.Count > 0)
            {
                bool filterOverdue = queryDto.SelectedStatuses.Any(s => string.Equals(s, "Overdue", StringComparison.OrdinalIgnoreCase));
                var statuses = queryDto.SelectedStatuses
                    .Where(s => !string.Equals(s, "Overdue", StringComparison.OrdinalIgnoreCase))
                    .Select(ExecutionStatuses.Normalize)
                    .ToHashSet();

                list = list.Where(i =>
                    (filterOverdue && string.Equals(i.DeadlineStatus, "Overdue", StringComparison.OrdinalIgnoreCase)) ||
                    (statuses.Count > 0 && statuses.Contains(ExecutionStatuses.Normalize(i.CalculatedStatus)))
                ).ToList();
            }

            // 5. Column Sorting
            bool isAsc = string.Equals(queryDto.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
            string sortByKey = (queryDto.SortBy ?? "code").ToLower();

            int GetStatusRank(string? status) => ExecutionStatuses.Normalize(status ?? "NotStarted") switch
            {
                "InProgress" => 2,
                "Completed" => 3,
                _ => 1
            };

            switch (sortByKey)
            {
                case "code":
                    list = isAsc
                        ? list.OrderBy(i => i.Code, StringComparer.OrdinalIgnoreCase).ToList()
                        : list.OrderByDescending(i => i.Code, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "title":
                    list = isAsc
                        ? list.OrderBy(i => i.Title).ToList()
                        : list.OrderByDescending(i => i.Title).ToList();
                    break;
                case "leadagency":
                case "leadagencyname":
                    list = isAsc
                        ? list.OrderBy(i => i.LeadAgencyName).ToList()
                        : list.OrderByDescending(i => i.LeadAgencyName).ToList();
                    break;
                case "assignedagency":
                case "assignedagencyname":
                    list = isAsc
                        ? list.OrderBy(i => i.AssignedAgencyName ?? "").ToList()
                        : list.OrderByDescending(i => i.AssignedAgencyName ?? "").ToList();
                    break;
                case "coordinatingagency":
                    list = isAsc
                        ? list.OrderBy(i => i.CoordinatingAgencyNames?.FirstOrDefault() ?? "").ToList()
                        : list.OrderByDescending(i => i.CoordinatingAgencyNames?.FirstOrDefault() ?? "").ToList();
                    break;
                case "period":
                    list = isAsc
                        ? list.OrderByDescending(i => i.IsOngoing).ThenBy(i => i.StartDate).ThenBy(i => i.DueDate).ToList()
                        : list.OrderBy(i => i.IsOngoing).ThenByDescending(i => i.DueDate).ThenByDescending(i => i.StartDate).ToList();
                    break;
                case "progress":
                    list = isAsc
                        ? list.OrderBy(i => i.LatestProgressValue ?? -1).ToList()
                        : list.OrderByDescending(i => i.LatestProgressValue ?? -1).ToList();
                    break;
                case "status":
                    list = isAsc
                        ? list.OrderBy(i => GetStatusRank(i.CalculatedStatus)).ToList()
                        : list.OrderByDescending(i => GetStatusRank(i.CalculatedStatus)).ToList();
                    break;
                case "lastupdated":
                case "createdat":
                default:
                    list = isAsc
                        ? list.OrderBy(i => i.LastUpdated ?? i.CreatedAt).ThenBy(i => i.Code).ToList()
                        : list.OrderByDescending(i => i.LastUpdated ?? i.CreatedAt).ThenBy(i => i.Code).ToList();
                    break;
            }

            // 6. Server-side Pagination
        return list;
    }
}

