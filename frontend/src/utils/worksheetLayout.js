// Merge presentation rows only. Data/table rows and existing complex merges stay intact.
export function fitPresentationRows(ws, { numCols, headerRowIndex, titleRowIndex = 0 }, XLSX) {
  const range = XLSX.utils.decode_range(ws['!ref']);
  const lastCol = Math.max(numCols - 1, range.e.c);
  const merges = ws['!merges'] ||= [];
  const heights = ws['!rows'] ||= [];
  const nonempty = cell => cell && (cell.f || (cell.v !== undefined && cell.v !== null && String(cell.v).trim() !== ''));
  const overlaps = (a, b) => a.s.r <= b.e.r && a.e.r >= b.s.r && a.s.c <= b.e.c && a.e.c >= b.s.c;
  const add = (r, start, end) => {
    if (end <= start) return;
    const candidate = { s: { r, c: start }, e: { r, c: end } };
    if (!merges.some(m => overlaps(m, candidate))) merges.push(candidate);
  };
  for (let r = range.s.r; r <= range.e.r; r++) {
    const cells = [];
    for (let c = 0; c <= lastCol; c++) if (nonempty(ws[XLSX.utils.encode_cell({ r, c })])) cells.push(c);
    const label = ws[XLSX.utils.encode_cell({ r, c: 0 })];
    if (!label || typeof label.v !== 'string' || r === headerRowIndex) continue;
    const beforeTable = r < headerRowIndex;
    const section = /^(\d+\.\s|CHỈ SỐ|DANH SÁCH|BẢNG TỔNG HỢP|THỐNG KÊ|HƯỚNG DẪN|Không có|Chưa có|Lưu ý:|Ghi chú:)/i.test(label.v);
    if (cells.length === 1 && cells[0] === 0 && (beforeTable || section)) {
      const old = merges.find(m => m.s.r === r && m.e.r === r && m.s.c === 0);
      const candidate = { s: { r, c: 0 }, e: { r, c: lastCol } };
      if (old && !merges.some(m => m !== old && overlaps(m, candidate))) old.e.c = lastCol;
      else add(r, 0, lastCol);
    } else if (beforeTable && r > titleRowIndex + 1 && cells.length === 2 && cells[0] === 0 && cells[1] === 1 && lastCol >= 2 && !merges.some(m => m.s.r <= r && m.e.r >= r)) {
      // Spread a label/value pair over unused columns, keeping both values.
      const labelEnd = Math.min(2, lastCol - 1);
      const valueCell = ws[XLSX.utils.encode_cell({ r, c: 1 })];
      delete ws[XLSX.utils.encode_cell({ r, c: 1 })];
      ws[XLSX.utils.encode_cell({ r, c: labelEnd + 1 })] = valueCell;
      add(r, 0, labelEnd); add(r, labelEnd + 1, lastCol);
    }
    const rowMerges = merges.filter(m => m.s.r === r && m.e.r === r);
    for (const merge of rowMerges) {
      const cell = ws[XLSX.utils.encode_cell(merge.s)];
      if (!nonempty(cell)) continue;
      let width = 0;
      for (let c = merge.s.c; c <= merge.e.c; c++) width += ws['!cols']?.[c]?.wch ?? 16;
      const lines = String(cell.v ?? '').split('\n').reduce((sum, line) => sum + Math.max(1, Math.ceil(line.length / Math.max(1, width * .85))), 0);
      const min = r === titleRowIndex ? 32 : 24;
      heights[r] = { ...heights[r], hpt: Math.max(heights[r]?.hpt ?? 0, min, lines * 15 + 8) };
    }
  }
  range.e.c = lastCol;
  ws['!ref'] = XLSX.utils.encode_range(range);
}
