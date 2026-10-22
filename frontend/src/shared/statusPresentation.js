export const executionStatuses = Object.freeze({
  NotStarted: { label: 'Chưa thực hiện', tone: 'slate' },
  InProgress: { label: 'Đang thực hiện', tone: 'blue' },
  Completed: { label: 'Đã hoàn thành', tone: 'emerald' }
});
// Read-only compatibility for historical API/cache values; never use these as options.
export function normalizeExecutionStatus(status) {
  return ({ InProgressOnTime: 'InProgress', InProgressOverdue: 'InProgress', ExpiringSoon: 'InProgress',
    CompletedOnTime: 'Completed', CompletedOverdue: 'Completed', Drafting: 'InProgress', Reviewing: 'InProgress', Submitted: 'InProgress' })[status] || status;
}
export const executionStatusSeries = Object.entries(executionStatuses).map(([value, { label, tone }]) => ({
  value, label, tone, key: value[0].toLowerCase() + value.slice(1), color: ({ slate: 'slate-400', blue: 'blue-500', emerald: 'emerald-500' })[tone]
}));
const labels = {
  Pending: 'Chờ phê duyệt', PendingApproval: 'Chờ phê duyệt', Approved: 'Đã phê duyệt', Rejected: 'Bị từ chối',
  NotRelevant: 'Không liên quan'
};
const badges = {
  slate: 'border bg-slate-100 text-slate-700 border-slate-200',
  emerald: 'border bg-emerald-50 text-emerald-700 border-emerald-200',
  rose: 'border bg-rose-50 text-rose-700 border-rose-200',
  amber: 'border bg-amber-50 text-amber-700 border-amber-200',
  blue: 'border bg-blue-50 text-blue-700 border-blue-200',
  teal: 'border bg-teal-50 text-teal-700 border-teal-200',
  orange: 'border bg-teal-50 text-teal-700 border-teal-200'
};
export function getStatusLabel(status) {
  return executionStatuses[normalizeExecutionStatus(status)]?.label ?? labels[status] ?? (status || executionStatuses.NotStarted.label);
}
export function getStatusBadgeClass(status) {
  const approvalTone = { Pending: 'amber', PendingApproval: 'amber', Approved: 'emerald', Rejected: 'rose', NotRelevant: 'rose' };
  return badges[executionStatuses[normalizeExecutionStatus(status)]?.tone ?? approvalTone[status] ?? 'slate'];
}
export const executionStatusOptions = Object.entries(executionStatuses).map(([value, { label }]) => ({ value, label }));
