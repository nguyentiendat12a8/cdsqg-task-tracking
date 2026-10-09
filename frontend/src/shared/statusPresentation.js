export const executionStatuses = Object.freeze({
  NotStarted: { label: 'Chưa thực hiện', tone: 'slate' },
  InProgressOnTime: { label: 'Đang thực hiện (trong hạn)', tone: 'emerald' },
  InProgressOverdue: { label: 'Đang thực hiện (quá hạn)', tone: 'rose' },
  ExpiringSoon: { label: 'Sắp hết hạn', tone: 'amber' },
  CompletedOnTime: { label: 'Hoàn thành (đúng hạn)', tone: 'blue' },
  CompletedOverdue: { label: 'Hoàn thành (quá hạn)', tone: 'orange' }
});
const labels = {
  InProgress: 'Đang thực hiện', Completed: 'Hoàn thành', Drafting: 'Đang xây dựng / soạn thảo',
  Reviewing: 'Đang thẩm định / xin ý kiến', Submitted: 'Đã trình',
  Pending: 'Chờ phê duyệt', PendingApproval: 'Chờ phê duyệt', Approved: 'Đã phê duyệt', Rejected: 'Bị từ chối'
};
const badges = {
  slate: 'border bg-slate-100 text-slate-700 border-slate-200',
  emerald: 'border bg-emerald-50 text-emerald-700 border-emerald-200',
  rose: 'border bg-rose-50 text-rose-700 border-rose-200',
  amber: 'border bg-amber-50 text-amber-700 border-amber-200',
  blue: 'border bg-blue-50 text-blue-700 border-blue-200',
  orange: 'border bg-orange-50 text-orange-700 border-orange-200'
};
export function getStatusLabel(status) {
  return executionStatuses[status]?.label ?? labels[status] ?? (status || executionStatuses.NotStarted.label);
}
export function getStatusBadgeClass(status) {
  const approvalTone = { Pending: 'amber', PendingApproval: 'amber', Approved: 'emerald', Rejected: 'rose' };
  return badges[executionStatuses[status]?.tone ?? approvalTone[status] ?? 'slate'];
}
export const executionStatusOptions = Object.entries(executionStatuses).map(([value, { label }]) => ({ value, label }));
