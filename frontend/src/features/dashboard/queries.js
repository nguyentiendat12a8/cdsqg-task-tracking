export function buildDashboardQuery(filters, { agencyIds = [], parentAgencyId, includeItemType = true } = {}) {
  const params = new URLSearchParams();
  if (includeItemType && filters.dashboardFilter && filters.dashboardFilter !== 'all') {
    params.set('itemType', filters.dashboardFilter === 'goals' ? 'Goal' : 'Task');
  }
  for (const id of agencyIds) params.append('agencyId', id);
  if (parentAgencyId) params.set('parentAgencyId', parentAgencyId);
  if (filters.selectedScopes?.length === 1) params.set('scope', filters.selectedScopes[0]);
  for (const section of filters.selectedSections || []) params.append('section', section);
  for (const group of filters.selectedGroups || []) params.append('group', group);
  if (filters.fromYear) params.set('fromYear', filters.fromYear);
  if (filters.toYear) params.set('toYear', filters.toYear);
  if (filters.isOngoingOnly) params.set('isOngoing', 'true');
  return params;
}
