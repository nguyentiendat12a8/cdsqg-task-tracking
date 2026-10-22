import XLSX from 'xlsx-js-style/dist/xlsx.bundle.js';
import { toast } from 'vue3-toastify';
import { styleWorksheet } from './worksheetStyle';
import { formatMeasuredValue } from '../shared/formatters';
import { reportStatusLabel } from '../shared/reportStatuses';
import { downloadProgressWorkbook } from './progressWorkbook';

/**
 * Export structured Excel report with Title Banner, Subtitle, KPI section, and Data Table.
 */
export function exportFormattedReportExcel({
  title,
  subtitle,
  kpiTitle = "1. CHỈ SỐ TỔNG QUAN VÀ THỐNG KÊ",
  kpiSection = [],
  tableTitle = "2. DANH SÁCH CHI TIẾT DỮ LIỆU BÁO CÁO",
  headers = [],
  rows = [],
  fileName = 'Bao_Cao_Du_Lieu',
  sheetName = 'Báo cáo',
  minColWidths = {}
}) {
  if (!rows || rows.length === 0) {
    toast.warning("Không có dữ liệu để xuất file Excel!");
    return;
  }

  const now = new Date();
  const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;

  const numCols = Math.max(headers ? headers.length : 1, 3);
  const data = [];

  // Title & Subtitle
  data.push([(title || 'Báo cáo dữ liệu').toUpperCase()]);
  data.push([subtitle || `Thời gian xuất báo cáo: ${timeStr}`]);
  data.push([]); // blank separator

  const merges = [];
  if (numCols > 1) {
    merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: numCols - 1 } });
    merges.push({ s: { r: 1, c: 0 }, e: { r: 1, c: numCols - 1 } });
  }

  // Section 1: KPI Overview Section
  if (kpiSection && kpiSection.length > 0) {
    const kpiTitleRowIdx = data.length;
    data.push([kpiTitle]);
    if (numCols > 1) {
      merges.push({ s: { r: kpiTitleRowIdx, c: 0 }, e: { r: kpiTitleRowIdx, c: numCols - 1 } });
    }
    kpiSection.forEach(kpi => {
      data.push([kpi[0], kpi[1]]);
    });
    data.push([]); // blank separator
  }

  // Section 2: Data Table Section
  const tableTitleRowIdx = data.length;
  data.push([tableTitle]);
  if (numCols > 1) {
    merges.push({ s: { r: tableTitleRowIdx, c: 0 }, e: { r: tableTitleRowIdx, c: numCols - 1 } });
  }

  const headerRowIdx = data.length;
  data.push(headers);

  rows.forEach(r => data.push(r));

  const ws = XLSX.utils.aoa_to_sheet(data);
  ws['!merges'] = merges;

  // Auto calculate fit column widths (!cols)
  const colWidths = headers.map((hdr, colIdx) => {
    let maxLen = hdr ? hdr.toString().length : 10;
    
    for (let r = headerRowIdx + 1; r < data.length; r++) {
      const cellVal = data[r][colIdx] !== undefined && data[r][colIdx] !== null ? data[r][colIdx].toString() : '';
      if (cellVal.length > maxLen) {
        maxLen = cellVal.length;
      }
    }

    let calculatedWidth = Math.min(Math.max(maxLen + 4, 12), 75);
    if (minColWidths[colIdx]) {
      calculatedWidth = Math.max(calculatedWidth, minColWidths[colIdx]);
    }
    return calculatedWidth;
  });

  ws['!cols'] = colWidths.map(w => ({ wch: w }));

  styleWorksheet(ws, { numCols, headerRowIndex: headerRowIdx, titleRowIndex: 0 }, XLSX);

  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, sheetName);

  const dateFileStr = now.toISOString().slice(0, 10);
  const fullFileName = `${fileName}_${dateFileStr}.xlsx`;
  
  XLSX.writeFile(wb, fullFileName);
  toast.success("Đã xuất file Excel báo cáo thành công!");
}

/**
 * Cleanly export basic data to formatted Excel (.xlsx).
 */
export function exportToExcel({
  title,
  subtitle,
  headers,
  rows,
  fileName = 'Bao_Cao_Du_Lieu',
  sheetName = 'Báo cáo',
  minColWidths = {}
}) {
  exportFormattedReportExcel({
    title,
    subtitle,
    kpiSection: [],
    tableTitle: "DANH SÁCH CHI TIẾT DỮ LIỆU",
    headers,
    rows,
    fileName,
    sheetName,
    minColWidths
  });
}

/**
 * Export specialized Progress Report template file for users to update & import back.
 */
export function exportProgressReportTemplate({ items = [], itemType, currentAgency = null, fileName = 'Bao_Cao_Tien_Do_Thuc_Hien', documentTitle = '', documentNumber = '', documentShortTitle = '' }) {
  const now = new Date();
  const isTask = (itemType || items[0]?.itemType) !== 'Goal';
  const noun = isTask ? 'Nhiệm Vụ' : 'Mục Tiêu';
  const numCols = isTask ? 9 : 8;
  const currentCol = isTask ? 6 : 5, inputCol = currentCol + 1, notesCol = inputCol + 1;
  const headers = ['STT', 'Mã', `Tên ${noun}`, 'Cơ Quan Chủ Trì', 'Đơn Vị Tính'];
  if (isTask) headers.push('Sản Phẩm Đầu Ra');
  headers.push('Tiến Độ Hiện Tại', 'TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]', 'GHI CHÚ [CẦN ĐIỀN]');
  if (isTask) headers.push('Mã Sản Phẩm');
  const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
  const docSuffix = documentTitle
    ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
    : '';
  const rows = [
    [`BÁO CÁO TIẾN ĐỘ THỰC HIỆN ${noun.toUpperCase()}${docSuffix}`],
    [`Đơn vị báo cáo: ${currentAgency?.name || 'Bộ, Ngành, Địa phương'} | Ngày xuất file: ${now.toLocaleDateString('vi-VN')}`],
    [`HƯỚNG DẪN: Điền tổng kết quả đã đạt đến năm báo cáo hoặc chọn trạng thái ở cột [${isTask ? 'H' : 'G'}]; ghi chú ở cột [${isTask ? 'I' : 'H'}]. Mỗi sản phẩm đầu ra chọn một trạng thái riêng. Chọn năm báo cáo khi import.`],
    headers
  ];
  const statusCells = [], merges = [];
  items.forEach((item, index) => {
    const qualitative = isTask || ['Qualitative', '2', 2].includes(item.evaluationType);
    const products = isTask && qualitative ? (item.deliverables || []) : [];
    const unit = qualitative ? 'Trạng thái' : (item.unitName || item.unit?.name || '%');
    const current = qualitative ? (item.latestQualitativeStatus ? reportStatusLabel(item.latestQualitativeStatus) : 'Chưa cập nhật') : (item.latestProgressValue != null ? formatMeasuredValue(item.latestProgressValue, unit) : 'Chưa cập nhật');
    const start = rows.length;
    (products.length ? products : [null]).forEach((product, productIndex) => {
      const row = productIndex === 0 ? [index + 1, item.code || '', item.title || '', item.leadAgencyName || item.leadAgency?.name || '—', unit] : ['', '', '', '', ''];
      if (isTask) row.push(product?.title || '—');
      row.push(product ? reportStatusLabel(product.currentStatus) : current, '', '');
      if (isTask) row.push(product?.id || '');
      rows.push(row);
      if (qualitative) statusCells.push(`${XLSX.utils.encode_col(inputCol)}${rows.length}`);
    });
    if (products.length > 1) {
      for (const c of [0, 1, 2, 3, 4, notesCol]) merges.push({ s: { r: start, c }, e: { r: rows.length - 1, c } });
    }
  });
  const ws = XLSX.utils.aoa_to_sheet(rows);
  ws['!cols'] = (isTask ? [6, 14, 50, 30, 16, 50, 22, 34, 40] : [6, 14, 50, 30, 16, 22, 34, 40]).map(wch => ({ wch }));
  if (isTask) ws['!cols'].push({ wch: 36, hidden: true });
  styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);
  for (const merge of ws['!merges'] || []) {
    if (merge.s.r < 3) merge.e.c = numCols - 1;
  }
  ws['!merges'] = [...(ws['!merges'] || []), ...merges];
  // Distribute the height of long shared task text across its merged product rows.
  for (const merge of merges.filter(merge => merge.s.c === 2)) {
    const height = Math.max(30, (ws['!rows'][merge.s.r]?.hpt || 30) / (merge.e.r - merge.s.r + 1));
    for (let r = merge.s.r; r <= merge.e.r; r++) {
      const productLines = String(rows[r][5] || '').split('\n').reduce((sum, line) => sum + Math.max(1, Math.ceil(line.length / 42)), 0);
      ws['!rows'][r] = { hpt: Math.max(height, productLines * 15 + 10, r === merge.s.r ? 30 : ws['!rows'][r]?.hpt || 30) };
    }
  }
  ws['!rows'][2] = { hpt: 42 };
  ws['!rows'][3] = { hpt: 36 };
  for (const [column, color] of [[inputCol, 'FEF08A'], [notesCol, 'BAE6FD']]) {
    const ref = XLSX.utils.encode_cell({ r: 3, c: column });
    ws[ref].s = { ...ws[ref].s, font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } }, fill: { fgColor: { rgb: color } } };
  }
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'Báo Cáo Tiến Độ');
  downloadProgressWorkbook(wb, [statusCells], `${fileName}_${now.toISOString().slice(0, 10)}.xlsx`, XLSX);
  toast.success('Đã xuất file Excel Báo Cáo Tiến Độ thành công!');
}
