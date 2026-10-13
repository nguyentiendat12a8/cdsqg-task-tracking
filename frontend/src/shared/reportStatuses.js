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
