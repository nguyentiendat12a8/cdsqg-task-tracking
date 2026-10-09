using Cdsqg.Application.DTOs;
namespace Cdsqg.Application.Services;

// Single source for document listing and export: filter/sort before pagination.
public static class DocumentItemQuery
{
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
                var expandedAgencyIds = new HashSet<Guid>(queryDto.SelectedAgencyIds);
                var ministriesGuid1 = Guid.Parse("00000000-0000-0000-0000-000000009998");
                var ministriesGuid2 = Guid.Parse("00000000-0000-0000-0000-000000009995");
                var provincesGuid1  = Guid.Parse("00000000-0000-0000-0000-000000009997");
                var provincesGuid2  = Guid.Parse("00000000-0000-0000-0000-000000009996");

                if (expandedAgencyIds.Contains(ministriesGuid1) || expandedAgencyIds.Contains(ministriesGuid2))
                {
                    expandedAgencyIds.Add(ministriesGuid1);
                    expandedAgencyIds.Add(ministriesGuid2);
                }
                if (expandedAgencyIds.Contains(provincesGuid1) || expandedAgencyIds.Contains(provincesGuid2))
                {
                    expandedAgencyIds.Add(provincesGuid1);
                    expandedAgencyIds.Add(provincesGuid2);
                }

                list = list.Where(i => expandedAgencyIds.Contains(i.LeadAgencyId)).ToList();
            }

            if (queryDto.SelectedSubAgencyIds != null && queryDto.SelectedSubAgencyIds.Count > 0)
            {
                list = list.Where(i => i.AssignedAgencyId.HasValue && queryDto.SelectedSubAgencyIds.Contains(i.AssignedAgencyId.Value)).ToList();
            }

            if (queryDto.SelectedSections != null && queryDto.SelectedSections.Count > 0)
            {
                list = list.Where(i => queryDto.SelectedSections.Contains(i.Section)).ToList();
            }

            if (queryDto.SelectedGroups != null && queryDto.SelectedGroups.Count > 0)
            {
                list = list.Where(i => queryDto.SelectedGroups.Contains(i.Group)).ToList();
            }

            bool wantOngoing = queryDto.OnlyOngoing;
            bool hasYearFilter = queryDto.FromYear.HasValue || queryDto.ToYear.HasValue;

            if (wantOngoing && hasYearFilter)
            {
                int fYr = queryDto.FromYear ?? 2026;
                int tYr = queryDto.ToYear ?? 2030;
                list = list.Where(i =>
                {
                    if (i.IsOngoing) return true;
                    int targetYear = i.DueDate.HasValue ? i.DueDate.Value.Year : (i.StartDate.HasValue ? i.StartDate.Value.Year : 2026);
                    if (queryDto.FromYear.HasValue && targetYear < fYr) return false;
                    if (queryDto.ToYear.HasValue && targetYear > tYr) return false;
                    return true;
                }).ToList();
            }
            else if (wantOngoing)
            {
                list = list.Where(i => i.IsOngoing).ToList();
            }
            else if (hasYearFilter)
            {
                int fYr = queryDto.FromYear ?? 2026;
                int tYr = queryDto.ToYear ?? 2030;
                list = list.Where(i =>
                {
                    if (i.IsOngoing) return false;
                    int targetYear = i.DueDate.HasValue ? i.DueDate.Value.Year : (i.StartDate.HasValue ? i.StartDate.Value.Year : 2026);
                    if (queryDto.FromYear.HasValue && targetYear < fYr) return false;
                    if (queryDto.ToYear.HasValue && targetYear > tYr) return false;
                    return true;
                }).ToList();
            }

            if (queryDto.SelectedScopes != null && queryDto.SelectedScopes.Count > 0)
            {
                list = list.Where(i =>
                {
                    if (queryDto.SelectedScopes.Contains("general") && i.IsGeneralTask) return true;
                    if (queryDto.SelectedScopes.Contains("specific") && !i.IsGeneralTask) return true;
                    return false;
                }).ToList();
            }

            if (queryDto.SelectedStatuses != null && queryDto.SelectedStatuses.Count > 0)
            {
                list = list.Where(i => queryDto.SelectedStatuses.Contains(i.CalculatedStatus)).ToList();
            }

            // 5. Column Sorting
            bool isAsc = string.Equals(queryDto.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
            string sortByKey = (queryDto.SortBy ?? "code").ToLower();

            int GetStatusRank(string? status) => status switch
            {
                "NotStarted" => 1,
                "InProgressOnTime" => 2,
                "InProgressOverdue" => 3,
                "ExpiringSoon" => 4,
                "CompletedOnTime" => 5,
                "CompletedOverdue" => 6,
                _ => 0
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

