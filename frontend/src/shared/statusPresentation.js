export const executionStatuses = Object.freeze({
  NotStarted: { label: 'Chưa thực hiện', tone: 'slate' },
  InProgressOnTime: { label: 'Đang thực hiện (trong hạn)', tone: 'blue' },
  InProgressOverdue: { label: 'Đang thực hiện (quá hạn)', tone: 'rose' },
  ExpiringSoon: { label: 'Sắp hết hạn', tone: 'amber' },
  CompletedOnTime: { label: 'Hoàn thành (đúng hạn)', tone: 'emerald' },
  CompletedOverdue: { label: 'Hoàn thành (quá hạn)', tone: 'teal' }
});
const labels = {
  InProgress: 'Đang thực hiện', Completed: 'Hoàn thành', Drafting: 'Đang xây dựng / soạn thảo',
  Reviewing: 'Đang thẩm định / xin ý kiến', Submitted: 'Đã trình',
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
  return executionStatuses[status]?.label ?? labels[status] ?? (status || executionStatuses.NotStarted.label);
}
export function getStatusBadgeClass(status) {
  const approvalTone = { Pending: 'amber', PendingApproval: 'amber', Approved: 'emerald', Rejected: 'rose', NotRelevant: 'rose' };
  return badges[executionStatuses[status]?.tone ?? approvalTone[status] ?? 'slate'];
}
export const executionStatusOptions = Object.entries(executionStatuses).map(([value, { label }]) => ({ value, label }));
