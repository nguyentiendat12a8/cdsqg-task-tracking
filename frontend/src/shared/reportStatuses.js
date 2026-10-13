export const reportStatusOptions = Object.freeze([
  { value: 'NotStarted', label: 'Chưa thực hiện' },
  { value: 'Drafting', label: 'Đang thực hiện' },
  { value: 'Completed', label: 'Đã hoàn thành' }
]);
export function parseReportStatus(text) {
  const normalized = String(text || '').trim().toLowerCase();
  return reportStatusOptions.find(option => [option.label.toLowerCase(), option.value.toLowerCase()].includes(normalized))?.value ?? null;
}
export function reportStatusLabel(value) {
  if (['Reviewing', 'Submitted', '2', '3'].includes(value)) return 'Đang thực hiện';
  return reportStatusOptions.find(option => option.value === value)?.label || 'Chưa thực hiện';
}
import { getStatusLabel } from './statusPresentation';


// Compatibility presentation for history returned by older API versions.
// Never derive deadlines or percentages here; the BE remains authoritative.
export function taskReportStatusLabel(report) {
  if (report?.calculatedStatus) return getStatusLabel(report.calculatedStatus);
  const products = report?.deliverables;
  if (products?.length) {
    const states = products.map(product => String(product.currentStatus ?? 'NotStarted').trim().toLowerCase());
    if (states.every(state => ['completed', '4'].includes(state))) return 'Đã hoàn thành';
    if (states.some(state => ['drafting', 'reviewing', 'submitted', 'completed', '2', '3', '4'].includes(state))) return 'Đang thực hiện';
    if (states.every(state => ['notstarted', '1'].includes(state))) return 'Chưa thực hiện';
    return 'Chưa có trạng thái';
  }
  return report?.status ? getStatusLabel(report.status) : 'Chưa có trạng thái';
}
