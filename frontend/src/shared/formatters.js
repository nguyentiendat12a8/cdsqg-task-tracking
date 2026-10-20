export function formatNumber(val) {
  if (val === null || val === undefined || val === '') return '—';
  const num = Number(val);
  if (Number.isNaN(num)) return String(val);
  return new Intl.NumberFormat('vi-VN').format(num);
}

export function formatMeasuredValue(value, unit) {
  if (value === null || value === undefined || value === '') return '—';
  const formatted = formatNumber(value);
  return unit ? `${formatted} ${unit}` : formatted;
}

export function formatPercent(val) {
  if (val === null || val === undefined || val === '') return '—';
  const num = Number(val);
  if (Number.isNaN(num)) return String(val);
  return `${new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 1 }).format(num)}%`;
}

export function formatDate(val) {
  if (!val) return '—';
  try {
    const d = new Date(val);
    if (Number.isNaN(d.getTime())) return '—';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  } catch {
    return '—';
  }
}

export function formatDateTime(val) {
  if (!val) return '—';
  try {
    const d = new Date(val);
    if (Number.isNaN(d.getTime())) return '—';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes} ${day}/${month}/${year}`;
  } catch {
    return '—';
  }
}

export function formatDateRange(start, end) {
  return !start && !end ? '—' : `${start ? formatDate(start) : '...'} ➔ ${end ? formatDate(end) : '...'}`;
}

export function formatFileName(path) {
  if (!path) return 'Tệp đính kèm';
  if (path.includes('/api/ai-inbox/')) return 'Tài liệu minh chứng (AI)';
  const name = String(path).split('?')[0].split(/[/\\]/).pop();
  if (name === 'file') return 'Tài liệu minh chứng';
  return name?.replace(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_/i, '') || 'Tệp đính kèm';
}
