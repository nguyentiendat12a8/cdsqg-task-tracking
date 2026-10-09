const numberFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 });
const timestampFormatter = new Intl.DateTimeFormat('en-GB', {
  timeZone: 'Asia/Ho_Chi_Minh', day: '2-digit', month: '2-digit', year: 'numeric',
  hour: '2-digit', minute: '2-digit', hourCycle: 'h23'
});

export function formatNumber(value) {
  if (value == null || value === '' || !Number.isFinite(Number(value))) return '—';
  return numberFormatter.format(Number(value));
}
export function formatPercent(value) {
  const text = formatNumber(value);
  return text === '—' ? text : `${text}%`;
}
export function formatDate(value) {
  if (!value) return '—';
  const text = String(value).trim();
  const match = /^(\d{4})-(\d{2})-(\d{2})(?:$|T| )/.exec(text);
  if (match) {
    const [, y, m, d] = match;
    const date = new Date(`${y}-${m}-${d}T00:00:00Z`);
    if (!Number.isNaN(date.getTime()) && date.toISOString().slice(0, 10) === `${y}-${m}-${d}`) return `${d}/${m}/${y}`;
    return text;
  }
  const date = new Date(text);
  if (Number.isNaN(date.getTime())) return text || '—';
  return `${String(date.getUTCDate()).padStart(2, '0')}/${String(date.getUTCMonth() + 1).padStart(2, '0')}/${date.getUTCFullYear()}`;
}
export function formatDateTime(value) {
  if (!value) return '—';
  let text = String(value).trim();
  // Compatibility adapter: legacy API timestamps without an offset represent UTC.
  // Date-only fields never pass through this timestamp adapter.
  if (/^\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}/.test(text) && !/(Z|[+-]\d{2}:?\d{2})$/i.test(text)) text = text.replace(' ', 'T') + 'Z';
  const date = new Date(text);
  if (Number.isNaN(date.getTime())) return String(value);
  const parts = Object.fromEntries(timestampFormatter.formatToParts(date).map(p => [p.type, p.value]));
  return `${parts.hour}:${parts.minute} ${parts.day}/${parts.month}/${parts.year}`;
}
export function formatDateRange(start, end) {
  return !start && !end ? '—' : `${start ? formatDate(start) : '...'} ➔ ${end ? formatDate(end) : '...'}`;
}
export function formatFileName(path) {
  if (!path) return 'Tệp đính kèm';
  const name = String(path).split('?')[0].split(/[/\\]/).pop();
  return name?.replace(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_/i, '') || 'Tệp đính kèm';
}
