import { parseReportStatus } from '../shared/reportStatuses.js';
export function readProductProgress(workbook, task, XLSX) {
  const inlineSheet = workbook.Sheets[workbook.SheetNames?.[0]];
  const inlineRows = inlineSheet ? XLSX.utils.sheet_to_json(inlineSheet, { header: 1 }) : [];
  const inlineHeader = inlineRows.findIndex(row => row.some(cell => String(cell).trim() === 'Mã Sản Phẩm'));
  const inline = inlineHeader >= 0;
  const ws = inline ? inlineSheet : workbook.Sheets['Sản phẩm đầu ra'];
  if (!ws) return { error: 'Thiếu sheet Sản phẩm đầu ra' };
  const rows = XLSX.utils.sheet_to_json(ws, { header: 1 });
  const header = rows.findIndex(row => row.some(cell => String(cell).trim() === 'Mã Sản Phẩm'));
  if (header < 0) return { error: 'Thiếu cột Mã Sản Phẩm' };
  const columns = rows[header].map(cell => String(cell).trim().toLowerCase());
  const codeColumn = columns.indexOf(inline ? 'mã' : 'mã nhiệm vụ'), idColumn = columns.indexOf('mã sản phẩm');
  const statusColumn = columns.findIndex(cell => inline ? cell.includes('tiến độ') && cell.includes('mới') : cell.includes('trạng thái mới'));
  if (codeColumn < 0 || idColumn < 0 || statusColumn < 0) return { error: 'Thiếu cột báo cáo sản phẩm' };
  if (inline) {
    // Only inherit codes within actual merged task cells; unrelated blank rows stay blank.
    for (const merge of ws['!merges'] || []) {
      if (merge.s.c !== codeColumn || merge.e.c !== codeColumn || merge.s.r <= header) continue;
      for (let r = merge.s.r + 1; r <= merge.e.r; r++) {
        rows[r] ||= [];
        rows[r][codeColumn] = rows[merge.s.r]?.[codeColumn];
      }
    }
  }
  const report = rows.slice(header + 1).filter(row => String(row[codeColumn] || '').trim().toLowerCase() === task.code.toLowerCase());
  if (report.length && report.every(row => String(row[statusColumn] ?? '').trim() === '')) return { skipped: true };
  const definitions = task.deliverables || [];
  const ids = report.map(row => String(row[idColumn] || '').trim());
  if (report.length !== definitions.length || new Set(ids).size !== ids.length || ids.some(id => !definitions.some(product => product.id === id))) return { error: 'Thiếu, trùng hoặc sai mã sản phẩm' };
  const deliverables = report.map(row => ({ id: String(row[idColumn]).trim(), currentStatus: parseReportStatus(row[statusColumn]) }));
  if (deliverables.some(product => !product.currentStatus)) return { error: 'Chọn trạng thái hợp lệ cho tất cả sản phẩm' };
  return { deliverables };
}
