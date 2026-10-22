// Adapt the API shape only. Official statuses and completion rates come from BE.
export function agencyForDashboard(agency, filter) {
  const prefix = filter === 'goals' ? 'goal' : 'task';
  const rateKey = `${prefix}CompletionRate`;
  return {
    ...agency,
    totalItems: agency[filter === 'goals' ? 'totalGoals' : 'totalTasks'] ?? agency.totalItems ?? 0,
    notStarted: agency[`${prefix}NotStarted`] ?? agency.notStarted ?? 0,
    inProgress: agency[`${prefix}InProgress`] ?? agency.inProgress ?? 0,
    completed: agency[`${prefix}Completed`] ?? agency.completed ?? 0,
    completionRate: rateKey in agency ? agency[rateKey] : agency.completionRateOverall ?? null
  };
}
export function visualShare(value, total) {
  return total > 0 ? Math.max(0, Math.min(100, value / total * 100)) : 0;
}
