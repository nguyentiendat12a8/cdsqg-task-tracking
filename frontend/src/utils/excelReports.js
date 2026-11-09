import XLSX from 'xlsx-js-style/dist/xlsx.bundle.js';
import { toast } from 'vue3-toastify';
import { styleWorksheet } from './worksheetStyle.js';
import { formatDateRange, formatMeasuredValue } from '../shared/formatters.js';
import { reportStatusLabel, reportStatusOptions } from '../shared/reportStatuses.js';
import { downloadProgressWorkbook } from './progressWorkbook.js';
import { GOAL_GUIDANCE_2629 } from '../config/goalGuidance2629.js';
import { GOAL_GUIDANCE_1266 } from '../config/goalGuidance1266.js';

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
 * Check if an agency is a Central Ministry or National Agency.
 */
export function isMinistryAgency(agency) {
  if (!agency) return false;
  const type = String(agency.type || '').toLowerCase();
  const code = String(agency.code || '').toUpperCase();
  const name = String(agency.name || '').toLowerCase();

  if (type === 'ministry' || type === '1') return true;
  if (type === 'province' || type === '2') return false;

  const ministryCodes = [
    'VPCP', 'BCA', 'BNV', 'BKHCN', 'BTC', 'BYT', 'BTP', 'BCYCP', 'BTTTT', 'BCT',
    'BGTVT', 'BNNPTNT', 'BTNMT', 'BXD', 'BLDTBXH', 'BVHTTDL', 'BGDDT', 'BNG', 'BQP',
    'NHNN', 'TTCP', 'UBDT', 'VKSNDTC', 'TANDTC'
  ];
  if (ministryCodes.includes(code)) return true;

  if (
    name.startsWith('bộ ') ||
    name.startsWith('ban ') ||
    name.startsWith('văn phòng chính phủ') ||
    name.startsWith('ngân hàng nhà nước') ||
    name.startsWith('thanh tra chính phủ') ||
    name.startsWith('viện kiểm sát') ||
    name.startsWith('tòa án nhân dân')
  ) {
    return true;
  }
  return false;
}

/**
 * Check if an agency is a Locality / Province.
 */
export function isProvinceAgency(agency) {
  if (!agency) return false;
  const type = String(agency.type || '').toLowerCase();
  const name = String(agency.name || '').toLowerCase();

  if (type === 'province' || type === '2') return true;
  if (
    name.startsWith('tỉnh ') ||
    name.startsWith('thành phố ') ||
    name.startsWith('ubnd tỉnh') ||
    name.startsWith('ubnd thành phố')
  ) {
    return true;
  }
  return false;
}

/**
 * Helper to build and style a Goal worksheet
 */
/**
 * Helper to build and style the clean Goal Reporting worksheet (to send to units)
 * Cột nhập liệu gọn gàng: Bỏ cột Chỉ tiêu, bỏ cột Công thức, bỏ cột Đánh giá đạt
 */
function createGoalReportingWorksheet({
  items = [],
  sheetTitle = 'BÁO CÁO TIẾN ĐỘ THỰC HIỆN CÁC MỤC TIÊU CHIẾN LƯỢC',
  agencyName = 'Bộ, Ngành, Địa phương',
  dateStr = '',
  guideText = 'HƯỚNG DẪN: Điền kết quả thực hiện mới vào cột G (màu vàng); nêu rõ khó khăn, vướng mắc hoặc kiến nghị tại cột H (màu xanh). Chi tiết công thức tính và căn cứ kiểm chứng xem tại Sheet "Hướng dẫn tính toán".',
  guidanceMap = GOAL_GUIDANCE_2629
}) {
  const headers = [
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    'Cơ quan chủ trì',
    'Đơn vị tính',
    'Tiến độ hiện tại',
    'TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]'
  ];

  const rows = [
    [sheetTitle.toUpperCase()],
    [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
    [guideText],
    headers
  ];

  items.forEach((item, idx) => {
    const guidance = guidanceMap[item.code] || {};
    const unit = guidance.unit || item.unitName || item.unit?.name || '%';
    const leadAgency = guidance.leadAgency || item.leadAgencyName || item.leadAgency?.name || '—';

    const qualitative = ['Qualitative', '2', 2].includes(item.evaluationType);
    let currentVal = 'Chưa cập nhật';
    if (qualitative) {
      currentVal = item.latestQualitativeStatus ? reportStatusLabel(item.latestQualitativeStatus) : 'Chưa cập nhật';
    } else if (item.latestProgressValue != null) {
      currentVal = formatMeasuredValue(item.latestProgressValue, unit);
    }

    rows.push([
      idx + 1,
      item.code || '',
      item.title || '',
      leadAgency,
      unit,
      currentVal,
      '', // TIẾN ĐỘ THỰC HIỆN MỚI (Col 6 / G)
      ''  // GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ (Col 7 / H)
    ]);
  });

  const ws = XLSX.utils.aoa_to_sheet(rows);
  const numCols = headers.length;

  // Explicit column widths (8 columns)
  ws['!cols'] = [
    { wch: 6 },  // STT
    { wch: 12 }, // Mã MT
    { wch: 50 }, // Tên mục tiêu / Chỉ tiêu
    { wch: 28 }, // Cơ quan chủ trì
    { wch: 14 }, // Đơn vị tính
    { wch: 18 }, // Tiến độ hiện tại
    { wch: 32 }, // Tiến độ mới [CẦN ĐIỀN] (Yellow)
    { wch: 46 }  // Ghi chú & Kiến nghị [CẦN ĐIỀN] (Blue)
  ];

  styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);

  // Extend banner merges across all 8 columns
  for (const merge of ws['!merges'] || []) {
    if (merge.s.r < 3) merge.e.c = numCols - 1;
  }

  // Row heights
  ws['!rows'] = ws['!rows'] || [];
  ws['!rows'][0] = { hpt: 32 };
  ws['!rows'][1] = { hpt: 22 };
  ws['!rows'][2] = { hpt: 36 };
  ws['!rows'][3] = { hpt: 30 };

  const thinBorder = {
    top: { style: 'thin', color: { rgb: 'CBD5E1' } },
    bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
    left: { style: 'thin', color: { rgb: 'CBD5E1' } },
    right: { style: 'thin', color: { rgb: 'CBD5E1' } }
  };

  // Header Col 6: TIẾN ĐỘ THỰC HIỆN MỚI (Yellow)
  const refInputHdr = XLSX.utils.encode_cell({ r: 3, c: 6 });
  if (ws[refInputHdr]) {
    ws[refInputHdr].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'FEF08A' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: thinBorder
    };
  }

  // Header Col 7: GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ (Soft Blue)
  const refNotesHdr = XLSX.utils.encode_cell({ r: 3, c: 7 });
  if (ws[refNotesHdr]) {
    ws[refNotesHdr].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'BAE6FD' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: thinBorder
    };
  }

  // Format data input rows
  for (let r = 4; r < rows.length; r++) {
    const titleText = String(rows[r][2] || '');
    const lines = Math.max(1, Math.ceil(titleText.length / 45));
    ws['!rows'][r] = { hpt: Math.max(30, lines * 16 + 10) };

    // Column 6 (Input Progress - soft yellow tint)
    const c6 = XLSX.utils.encode_cell({ r, c: 6 });
    if (ws[c6]) {
      ws[c6].s = {
        font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '047857' } },
        fill: { fgColor: { rgb: 'FEFCE8' } },
        alignment: { horizontal: 'center', vertical: 'center' },
        border: thinBorder
      };
    }

    // Column 7 (Notes - soft blue tint)
    const c7 = XLSX.utils.encode_cell({ r, c: 7 });
    if (ws[c7]) {
      ws[c7].s = {
        font: { name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' } },
        fill: { fgColor: { rgb: 'F0F9FF' } },
        alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
        border: thinBorder
      };
    }
  }

  return ws;
}

/**
 * Helper to build the separate Guidance worksheet (Sheet "Hướng dẫn tính toán")
 * Trình bày công thức toán học, tử số, mẫu số, chỉ tiêu 2026 và căn cứ kiểm chứng
 */
function createGoalGuidanceWorksheet({
  items = [],
  sheetTitle = 'HƯỚNG DẪN CÔNG THỨC TOÁN HỌC CÁC MỤC TIÊU CHIẾN LƯỢC',
  docSubTitle = '',
  dateStr = '',
  guidanceMap = GOAL_GUIDANCE_2629
}) {
  const headers = [
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    'Cơ quan chủ trì',
    'Công thức toán học',
    'Tử số (A)',
    'Mẫu số (B)'
  ];

  const rows = [
    [sheetTitle.toUpperCase()],
    [docSubTitle || (guidanceMap === GOAL_GUIDANCE_1266
      ? `Căn cứ Quyết định số 1266/QĐ-TTg của Thủ tướng Chính phủ | Ngày xuất file: ${dateStr}`
      : `Căn cứ Quyết định số 2629/QĐ-TTg của Thủ tướng Chính phủ | Ngày xuất file: ${dateStr}`)],
    ['TRA CỨU CÔNG THỨC TOÁN HỌC, ĐỊNH NGHĨA TỬ SỐ (A) VÀ MẪU SỐ (B)'],
    headers
  ];

  items.forEach((item, idx) => {
    const guidance = guidanceMap[item.code] || {};
    const leadAgency = guidance.leadAgency || item.leadAgencyName || item.leadAgency?.name || '—';

    rows.push([
      idx + 1,
      item.code || '',
      item.title || '',
      leadAgency,
      guidance.formula || 'Theo dõi định kỳ',
      guidance.numerator || '—',
      guidance.denominator || '—'
    ]);
  });

  const ws = XLSX.utils.aoa_to_sheet(rows);
  const numCols = headers.length;

  ws['!cols'] = [
    { wch: 6 },  // STT
    { wch: 12 }, // Mã MT
    { wch: 48 }, // Tên mục tiêu
    { wch: 28 }, // Cơ quan chủ trì
    { wch: 26 }, // Công thức toán học
    { wch: 52 }, // Tử số (A)
    { wch: 52 }  // Mẫu số (B)
  ];

  styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);

  for (const merge of ws['!merges'] || []) {
    if (merge.s.r < 3) merge.e.c = numCols - 1;
  }

  ws['!rows'] = ws['!rows'] || [];
  ws['!rows'][0] = { hpt: 32 };
  ws['!rows'][1] = { hpt: 22 };
  ws['!rows'][2] = { hpt: 30 };
  ws['!rows'][3] = { hpt: 30 };

  const thinBorder = {
    top: { style: 'thin', color: { rgb: 'CBD5E1' } },
    bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
    left: { style: 'thin', color: { rgb: 'CBD5E1' } },
    right: { style: 'thin', color: { rgb: 'CBD5E1' } }
  };

  for (let r = 4; r < rows.length; r++) {
    const titleText = String(rows[r][2] || '');
    const numText = String(rows[r][5] || '');
    const denText = String(rows[r][6] || '');
    const maxLen = Math.max(titleText.length, numText.length, denText.length);
    const lines = Math.max(1, Math.ceil(maxLen / 46));
    ws['!rows'][r] = { hpt: Math.max(34, lines * 15 + 10) };

    for (let c = 0; c < numCols; c++) {
      const cellRef = XLSX.utils.encode_cell({ r, c });
      if (ws[cellRef]) {
        const alignH = (c === 0 || c === 1 || c === 4) ? 'center' : 'left';
        ws[cellRef].s = {
          ...ws[cellRef].s,
          alignment: { horizontal: alignH, vertical: 'center', wrapText: true },
          border: thinBorder
        };
      }
    }
  }

  return ws;
}

/**
 * Export specialized Progress Report template file for users to update & import back.
 */
/**
 * Helper to build and style the clean Task Reporting worksheet
 */
function createTaskReportingWorksheet({
  items = [],
  sheetTitle = 'BÁO CÁO TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ CHIẾN LƯỢC',
  agencyName = 'Bộ, Ngành, Địa phương',
  dateStr = '',
  guideText = 'HƯỚNG DẪN: Điền tổng kết quả đã đạt đến năm báo cáo hoặc chọn trạng thái sản phẩm ở cột H (màu vàng); nêu rõ khó khăn, vướng mắc hoặc kiến nghị tại cột I (màu xanh). Mỗi sản phẩm đầu ra chọn một trạng thái riêng.'
}) {
  const numCols = 9;
  const inputCol = 7, notesCol = 8;
  const headers = [
    'STT',
    'Mã NV',
    'Tên Nhiệm Vụ',
    'Cơ quan chủ trì',
    'Thời gian thực hiện',
    'Sản Phẩm Đầu Ra',
    'Tiến độ hiện tại',
    'TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]',
    'Mã Sản Phẩm'
  ];

  const rows = [
    [sheetTitle.toUpperCase()],
    [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
    [guideText],
    headers
  ];

  const statusCells = [], merges = [];
  items.forEach((item, index) => {
    const qualitative = ['Qualitative', '2', 2].includes(item.evaluationType);
    const products = qualitative ? (item.deliverables || []) : [];
    const unit = qualitative ? 'Trạng thái' : (item.unitName || item.unit?.name || '%');
    const current = qualitative
      ? (item.latestQualitativeStatus ? reportStatusLabel(item.latestQualitativeStatus) : 'Chưa cập nhật')
      : (item.latestProgressValue != null ? formatMeasuredValue(item.latestProgressValue, unit) : 'Chưa cập nhật');
    const start = rows.length;
    (products.length ? products : [null]).forEach((product, productIndex) => {
      const row = productIndex === 0
        ? [index + 1, item.code || '', item.title || '', item.leadAgencyName || item.leadAgency?.name || '—', item.isOngoing ? 'Thường xuyên' : formatDateRange(item.startDate, item.dueDate)]
        : ['', '', '', '', ''];
      row.push(product?.title || '—');
      row.push(product ? reportStatusLabel(product.currentStatus) : current, '', '');
      row.push(product?.id || '');
      rows.push(row);
      if (qualitative) statusCells.push(`${XLSX.utils.encode_col(inputCol)}${rows.length}`);
    });
    if (products.length > 1) {
      for (const c of [0, 1, 2, 3, 4, notesCol]) merges.push({ s: { r: start, c }, e: { r: rows.length - 1, c } });
    }
  });

  const ws = XLSX.utils.aoa_to_sheet(rows);
  ws['!cols'] = [
    { wch: 6 },  // STT
    { wch: 14 }, // Mã NV
    { wch: 48 }, // Tên Nhiệm Vụ
    { wch: 28 }, // Cơ quan chủ trì
    { wch: 25 }, // Thời gian thực hiện
    { wch: 46 }, // Sản Phẩm Đầu Ra
    { wch: 20 }, // Tiến độ hiện tại
    { wch: 34 }, // TIẾN ĐỘ THỰC HIỆN MỚI (Yellow)
    { wch: 46 }  // GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ (Soft Blue)
  ];
  ws['!cols'].push({ wch: 36, hidden: true }); // Col 9: Mã Sản Phẩm hidden

  styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);

  for (const merge of ws['!merges'] || []) {
    if (merge.s.r < 3) merge.e.c = numCols - 1;
  }
  ws['!merges'] = [...(ws['!merges'] || []), ...merges];

  for (const merge of merges.filter(merge => merge.s.c === 2)) {
    const height = Math.max(30, (ws['!rows'][merge.s.r]?.hpt || 30) / (merge.e.r - merge.s.r + 1));
    for (let r = merge.s.r; r <= merge.e.r; r++) {
      const productLines = String(rows[r][5] || '').split('\n').reduce((sum, line) => sum + Math.max(1, Math.ceil(line.length / 42)), 0);
      ws['!rows'][r] = { hpt: Math.max(height, productLines * 15 + 10, r === merge.s.r ? 30 : ws['!rows'][r]?.hpt || 30) };
    }
  }

  ws['!rows'] = ws['!rows'] || [];
  ws['!rows'][0] = { hpt: 32 };
  ws['!rows'][1] = { hpt: 22 };
  ws['!rows'][2] = { hpt: 36 };
  ws['!rows'][3] = { hpt: 30 };

  const thinBorder = {
    top: { style: 'thin', color: { rgb: 'CBD5E1' } },
    bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
    left: { style: 'thin', color: { rgb: 'CBD5E1' } },
    right: { style: 'thin', color: { rgb: 'CBD5E1' } }
  };

  // Header Col 7: TIẾN ĐỘ THỰC HIỆN MỚI (Yellow)
  const refInputHdr = XLSX.utils.encode_cell({ r: 3, c: inputCol });
  if (ws[refInputHdr]) {
    ws[refInputHdr].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'FEF08A' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: thinBorder
    };
  }

  // Header Col 8: GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ (Soft Blue)
  const refNotesHdr = XLSX.utils.encode_cell({ r: 3, c: notesCol });
  if (ws[refNotesHdr]) {
    ws[refNotesHdr].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'BAE6FD' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: thinBorder
    };
  }

  for (let r = 4; r < rows.length; r++) {
    const cInput = XLSX.utils.encode_cell({ r, c: inputCol });
    if (ws[cInput]) {
      ws[cInput].s = {
        font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '047857' } },
        fill: { fgColor: { rgb: 'FEFCE8' } },
        alignment: { horizontal: 'center', vertical: 'center' },
        border: thinBorder
      };
    }

    const cNotes = XLSX.utils.encode_cell({ r, c: notesCol });
    if (ws[cNotes]) {
      ws[cNotes].s = {
        font: { name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' } },
        fill: { fgColor: { rgb: 'F0F9FF' } },
        alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
        border: thinBorder
      };
    }
  }

  return { ws, statusCells };
}


export function exportProgressReportTemplate({
  items = [],
  itemType,
  currentAgency = null,
  targetAgency = null,
  templateMode = 'auto',
  userRole = '',
  fileName = 'Bao_Cao_Tien_Do_Thuc_Hien',
  documentTitle = '',
  documentNumber = '',
  documentShortTitle = '',
  documentCode = ''
}) {
  const now = new Date();
  const dateStr = now.toLocaleDateString('vi-VN');
  const dateFileStr = now.toISOString().slice(0, 10);
  const isTask = (itemType || items[0]?.itemType) !== 'Goal';

  // -------------------------------------------------------------
  // BRANCH 1: TASK PROGRESS REPORT (Enhanced with Agency Routing & Multi-sheet)
  // -------------------------------------------------------------
  if (isTask) {
    const wb = XLSX.utils.book_new();
    const statusCellsBySheet = [];
    const is1266 = String(documentCode || '').includes('1266') ||
                   String(documentNumber || '').includes('1266') ||
                   String(documentTitle || '').includes('1266');
    const docCode = is1266 ? '1266' : (documentCode || (documentNumber ? documentNumber.replace(/\D/g, '') : '2629'));
    const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
    const docSuffix = documentTitle
      ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
      : '';

    const effectiveAgency = targetAgency || (currentAgency && String(userRole).toLowerCase() !== 'admin' ? currentAgency : null);

    const filterTasksForMinistry = (taskList, agency) => {
      if (agency) {
        const agId = agency.id ? String(agency.id).toLowerCase() : '';
        const agCode = (agency.code || '').toUpperCase();
        const agName = (agency.name || '').toLowerCase();
        return taskList.filter(item => {
          if (item.leadAgencyId && agId && String(item.leadAgencyId).toLowerCase() === agId) return true;
          if (item.leadAgencyCode && agCode && item.leadAgencyCode.toUpperCase() === agCode) return true;
          if (item.assignedAgencyId && agId && String(item.assignedAgencyId).toLowerCase() === agId) return true;
          if (item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES' || item.leadAgencyCode === 'ALL_MINISTRIES') return true;
          const lead = (item.leadAgencyName || '').toLowerCase();
          if (lead.includes(agName.replace('bộ ', '').replace('ban ', '').trim())) return true;
          return false;
        });
      }
      return taskList.filter(item => {
        if (item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES' || item.leadAgencyCode === 'ALL_MINISTRIES') return true;
        return isMinistryAgency(item.leadAgency || { code: item.leadAgencyCode, name: item.leadAgencyName });
      });
    };

    const filterTasksForLocality = (taskList, agency) => {
      if (agency) {
        const agId = agency.id ? String(agency.id).toLowerCase() : '';
        const agCode = (agency.code || '').toUpperCase();
        const agName = (agency.name || '').toLowerCase();
        return taskList.filter(item => {
          if (item.leadAgencyId && agId && String(item.leadAgencyId).toLowerCase() === agId) return true;
          if (item.leadAgencyCode && agCode && item.leadAgencyCode.toUpperCase() === agCode) return true;
          if (item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES' || item.leadAgencyCode === 'ALL_PROVINCES') return true;
          if (item.coordinatingAgencyIds && agId && item.coordinatingAgencyIds.map(String).includes(agId)) return true;
          const lead = (item.leadAgencyName || '').toLowerCase();
          if (lead.includes(agName.replace('tỉnh ', '').replace('thành phố ', '').trim())) return true;
          return false;
        });
      }
      return taskList.filter(item => {
        if (item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES' || item.leadAgencyCode === 'ALL_PROVINCES') return true;
        if (isProvinceAgency(item.leadAgency || { code: item.leadAgencyCode, name: item.leadAgencyName })) return true;
        if (item.coordinatingAgencyNames?.some(name => name.toLowerCase().includes('địa phương') || name.toLowerCase().includes('tỉnh'))) return true;
        return false;
      });
    };

    // Case 1: Specific Single Agency Report
    if (effectiveAgency && templateMode === 'auto') {
      const isMinistry = isMinistryAgency(effectiveAgency);
      const isProvince = isProvinceAgency(effectiveAgency);
      let filteredTasks = items;
      let agencyRoleTag = 'Bộ, Ngành, Địa phương';

      if (isMinistry) {
        filteredTasks = filterTasksForMinistry(items, effectiveAgency);
        agencyRoleTag = 'Bộ, Cơ quan chủ trì nhiệm vụ';
      } else if (isProvince) {
        filteredTasks = filterTasksForLocality(items, effectiveAgency);
        agencyRoleTag = 'Ủy ban nhân dân Tỉnh, Thành phố';
      }

      const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ${docSuffix}`;
      const { ws, statusCells } = createTaskReportingWorksheet({
        items: filteredTasks,
        sheetTitle,
        agencyName: `${effectiveAgency.name} (${agencyRoleTag})`,
        dateStr
      });
      const sheetName = `Báo Cáo - ${(effectiveAgency.code || effectiveAgency.name).slice(0, 20)}`;
      XLSX.utils.book_append_sheet(wb, ws, sheetName);
      statusCellsBySheet.push(statusCells);

      const safeAgencyCode = (effectiveAgency.code || effectiveAgency.name.replace(/\s+/g, '_')).slice(0, 15);
      const finalFileName = `Bao_Cao_Tien_Do_Nhiem_Vu_${docCode}_${safeAgencyCode}_${dateFileStr}.xlsx`;
      downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
      toast.success(`Đã xuất biểu mẫu báo cáo nhiệm vụ cho ${effectiveAgency.name}!`);
      return;
    }

    // Case 2: Explicit Ministry mode
    if (templateMode === 'ministry') {
      const ministryTasks = filterTasksForMinistry(items);
      const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ CHIẾN LƯỢC - DÀNH CHO BỘ, NGÀNH CHỦ TRÌ${docSuffix}`;
      const { ws, statusCells } = createTaskReportingWorksheet({
        items: ministryTasks,
        sheetTitle,
        agencyName: 'Các Bộ, Cơ quan ngang Bộ, Cơ quan thuộc Chính phủ chủ trì nhiệm vụ',
        dateStr
      });
      XLSX.utils.book_append_sheet(wb, ws, 'Bộ ngành báo cáo');
      statusCellsBySheet.push(statusCells);

      const finalFileName = `Bao_Cao_Tien_Do_Nhiem_Vu_${docCode}_Bo_Nganh_${dateFileStr}.xlsx`;
      downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
      toast.success('Đã xuất biểu mẫu báo cáo nhiệm vụ dành cho Bộ, ngành chủ trì!');
      return;
    }

    // Case 3: Explicit Province mode
    if (templateMode === 'province') {
      const provinceTasks = filterTasksForLocality(items);
      const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ CHIẾN LƯỢC - DÀNH CHO ĐỊA PHƯƠNG${docSuffix}`;
      const { ws, statusCells } = createTaskReportingWorksheet({
        items: provinceTasks,
        sheetTitle,
        agencyName: 'Ủy ban nhân dân các Tỉnh, Thành phố trực thuộc Trung ương',
        dateStr
      });
      XLSX.utils.book_append_sheet(wb, ws, 'Địa phương báo cáo');
      statusCellsBySheet.push(statusCells);

      const finalFileName = `Bao_Cao_Tien_Do_Nhiem_Vu_${docCode}_Dia_Phuong_${dateFileStr}.xlsx`;
      downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
      toast.success('Đã xuất biểu mẫu báo cáo nhiệm vụ dành cho Địa phương!');
      return;
    }

    // Case 4: Default dual-sheet report (when all agencies selected)
    const ministryTasks = filterTasksForMinistry(items);
    const { ws: wsMin, statusCells: scMin } = createTaskReportingWorksheet({
      items: ministryTasks,
      sheetTitle: `BÁO CÁO TIẾN ĐỘ NHIỆM VỤ - DÀNH CHO CÁC BỘ, NGÀNH CHỦ TRÌ${docSuffix}`,
      agencyName: 'Các Bộ, cơ quan chủ trì theo dõi nhiệm vụ',
      dateStr
    });
    XLSX.utils.book_append_sheet(wb, wsMin, 'Bộ ngành báo cáo');
    statusCellsBySheet.push(scMin);

    const provinceTasks = filterTasksForLocality(items);
    const { ws: wsPrv, statusCells: scPrv } = createTaskReportingWorksheet({
      items: provinceTasks,
      sheetTitle: `BÁO CÁO TIẾN ĐỘ NHIỆM VỤ - DÀNH CHO ĐỊA PHƯƠNG THỰC HIỆN${docSuffix}`,
      agencyName: 'Ủy ban nhân dân các Tỉnh, Thành phố trực thuộc Trung ương',
      dateStr
    });
    XLSX.utils.book_append_sheet(wb, wsPrv, 'Địa phương báo cáo');
    statusCellsBySheet.push(scPrv);

    const finalFileName = `Bao_Cao_Tien_Do_Nhiem_Vu_${docCode}_Tong_Hop_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo tiến độ nhiệm vụ tổng hợp (gồm tab Bộ ngành và Địa phương)!');
    return;
  }


  // -------------------------------------------------------------
  // BRANCH 2A: GENERIC GOAL REPORT (For non-2629 / non-specialized)
  // -------------------------------------------------------------
  const isGoal = !isTask;
  const isSpecializedGoalReport = isGoal && (
    templateMode === 'ministry' ||
    templateMode === 'province' ||
    Boolean(targetAgency) ||
    documentCode === '2629' ||
    documentCode === '1266' ||
    String(documentNumber || '').includes('2629') ||
    String(documentNumber || '').includes('1266') ||
    String(documentTitle || '').includes('2629') ||
    String(documentTitle || '').includes('1266') ||
    items.some(i => GOAL_GUIDANCE_2629[i.code] || GOAL_GUIDANCE_1266[i.code])
  );

  if (isGoal && !isSpecializedGoalReport) {
    const noun = 'Mục Tiêu';
    const numCols = 8;
    const currentCol = 5, inputCol = currentCol + 1, notesCol = inputCol + 1;
    const headers = ['STT', 'Mã', `Tên ${noun}`, 'Cơ Quan Chủ Trì', 'Đơn Vị Tính', 'Tiến Độ Hiện Tại', 'TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]', 'GHI CHÚ [CẦN ĐIỀN]'];
    const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
    const docSuffix = documentTitle
      ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
      : '';
    const agencyName = targetAgency?.name || currentAgency?.name || 'Bộ, Ngành, Địa phương';
    const rows = [
      [`BÁO CÁO TIẾN ĐỘ THỰC HIỆN ${noun.toUpperCase()}${docSuffix}`],
      [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
      [`HƯỚNG DẪN: Điền tổng kết quả đã đạt đến năm báo cáo hoặc chọn trạng thái ở cột [G]; ghi chú ở cột [H]. Chọn năm báo cáo khi import.`],
      headers
    ];
    const statusCells = [];
    items.forEach((item, index) => {
      const qualitative = ['Qualitative', '2', 2].includes(item.evaluationType);
      const unit = qualitative ? 'Trạng thái' : (item.unitName || item.unit?.name || '%');
      const current = qualitative ? (item.latestQualitativeStatus ? reportStatusLabel(item.latestQualitativeStatus) : 'Chưa cập nhật') : (item.latestProgressValue != null ? formatMeasuredValue(item.latestProgressValue, unit) : 'Chưa cập nhật');
      rows.push([index + 1, item.code || '', item.title || '', item.leadAgencyName || item.leadAgency?.name || '—', unit, current, '', '']);
      if (qualitative) statusCells.push(`${XLSX.utils.encode_col(inputCol)}${rows.length}`);
    });
    const ws = XLSX.utils.aoa_to_sheet(rows);
    ws['!cols'] = [6, 14, 50, 30, 16, 22, 34, 40].map(wch => ({ wch }));
    styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);
    for (const merge of ws['!merges'] || []) {
      if (merge.s.r < 3) merge.e.c = numCols - 1;
    }
    ws['!rows'] = ws['!rows'] || [];
    ws['!rows'][2] = { hpt: 42 };
    ws['!rows'][3] = { hpt: 36 };
    for (const [column, color] of [[inputCol, 'FEF08A'], [notesCol, 'BAE6FD']]) {
      const ref = XLSX.utils.encode_cell({ r: 3, c: column });
      ws[ref].s = { ...ws[ref].s, font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } }, fill: { fgColor: { rgb: color } } };
    }
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Báo Cáo Tiến Độ');
    downloadProgressWorkbook(wb, [statusCells], `${fileName}_${dateFileStr}.xlsx`, XLSX);
    toast.success('Đã xuất file Excel Báo Cáo Tiến Độ thành công!');
    return;
  }

  // -------------------------------------------------------------
  // BRANCH 2B: SPECIALIZED GOAL PROGRESS REPORT (With 2629 & 1266 formulas & Agency routing)
  // -------------------------------------------------------------
  const wb = XLSX.utils.book_new();
  const statusCellsBySheet = [];
  const is1266 = String(documentCode || '').includes('1266') ||
                 String(documentNumber || '').includes('1266') ||
                 String(documentTitle || '').includes('1266') ||
                 items.some(i => i.code && GOAL_GUIDANCE_1266[i.code] && !GOAL_GUIDANCE_2629[i.code]);
  const activeGuidanceMap = is1266 ? GOAL_GUIDANCE_1266 : GOAL_GUIDANCE_2629;
  const docCode = is1266 ? '1266' : (documentCode || (documentNumber ? documentNumber.replace(/\D/g, '') : '2629'));
  const docNumberStr = is1266 ? '1266/QĐ-TTg' : (documentNumber || '2629/QĐ-TTg');
  const totalGoals = is1266 ? 26 : 22;
  const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
  const docSuffix = documentTitle
    ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
    : '';

  const effectiveAgency = targetAgency || (currentAgency && String(userRole).toLowerCase() !== 'admin' ? currentAgency : null);

  // Helper to filter goals that localities measure & report
  const filterGoalsForLocality = (goalList) => {
    return goalList.filter(item => {
      const g = activeGuidanceMap[item.code];
      if (g) return g.isLocalReporting !== false;
      return true;
    });
  };

  // 1. SPECIFIC SINGLE AGENCY REPORT (When filtered to 1 agency, e.g. Bộ Công an)
  if (effectiveAgency && templateMode === 'auto') {
    const isMinistry = isMinistryAgency(effectiveAgency);
    const isProvince = isProvinceAgency(effectiveAgency);

    let filteredItems = items;
    let agencyRoleTag = 'Bộ, Ngành, Địa phương';

    if (isMinistry) {
      agencyRoleTag = 'Bộ, Cơ quan chủ trì mục tiêu';
    } else if (isProvince) {
      agencyRoleTag = 'Ủy ban nhân dân Tỉnh, Thành phố';
    }

    // Sheet 1: Báo Cáo - [Đơn vị] (Chỉ giữ các cột cần thiết cho đơn vị điền kết quả)
    const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN CÁC MỤC TIÊU${docSuffix}`;
    const wsReport = createGoalReportingWorksheet({
      items: filteredItems,
      sheetTitle,
      agencyName: `${effectiveAgency.name} (${agencyRoleTag})`,
      dateStr,
      guideText: 'HƯỚNG DẪN: Điền kết quả thực hiện mới vào cột G (màu vàng); nêu rõ khó khăn, vướng mắc hoặc kiến nghị tại cột H (màu xanh). Chi tiết công thức tính và căn cứ kiểm chứng xem tại Sheet "Hướng dẫn tính toán".',
      guidanceMap: activeGuidanceMap
    });

    const sheetName = `Báo Cáo - ${(effectiveAgency.code || effectiveAgency.name).slice(0, 20)}`;
    XLSX.utils.book_append_sheet(wb, wsReport, sheetName);
    statusCellsBySheet.push([]);

    // Sheet 2: Hướng dẫn tính toán (Tách riêng công thức, tử số, mẫu số, căn cứ kiểm chứng)
    const wsGuide = createGoalGuidanceWorksheet({
      items: filteredItems,
      sheetTitle: `HƯỚNG DẪN CÔNG THỨC TOÁN HỌC VÀ CĂN CỨ ĐO LƯỜNG - ${effectiveAgency.name.toUpperCase()}`,
      docSubTitle: `Căn cứ Quyết định số ${docNumberStr} của Thủ tướng Chính phủ | Ngày xuất file: ${dateStr}`,
      dateStr,
      guidanceMap: activeGuidanceMap
    });
    XLSX.utils.book_append_sheet(wb, wsGuide, 'Hướng dẫn tính toán');
    statusCellsBySheet.push([]);

    const safeAgencyCode = (effectiveAgency.code || effectiveAgency.name.replace(/\s+/g, '_')).slice(0, 15);
    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_${safeAgencyCode}_${dateFileStr}.xlsx`;

    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success(`Đã xuất biểu mẫu báo cáo và hướng dẫn tính toán cho ${effectiveAgency.name}!`);
    return;
  }

  // 2. EXPLICIT MINISTRY REPORT
  if (templateMode === 'ministry') {
    const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN MỤC TIÊU CHIẾN LƯỢC - DÀNH CHO BỘ, NGÀNH CHỦ TRÌ${docSuffix}`;
    const wsReport = createGoalReportingWorksheet({
      items,
      sheetTitle,
      agencyName: 'Các Bộ, Cơ quan ngang Bộ, Cơ quan thuộc Chính phủ chủ trì mục tiêu',
      dateStr,
      guideText: 'HƯỚNG DẪN: Điền kết quả đo lường toàn quốc vào cột G (màu vàng); nêu giải pháp, kiến nghị tại cột H (màu xanh). Xem công thức tại Sheet "Hướng dẫn tính toán".',
      guidanceMap: activeGuidanceMap
    });

    XLSX.utils.book_append_sheet(wb, wsReport, 'Bộ ngành báo cáo');
    statusCellsBySheet.push([]);

    const wsGuide = createGoalGuidanceWorksheet({
      items,
      sheetTitle: 'HƯỚNG DẪN CÔNG THỨC VÀ CĂN CỨ ĐO LƯỜNG - DÀNH CHO BỘ, NGÀNH CHỦ TRÌ',
      docSubTitle: `Căn cứ Quyết định số ${docNumberStr} của Thủ tướng Chính phủ | Ngày xuất file: ${dateStr}`,
      dateStr,
      guidanceMap: activeGuidanceMap
    });
    XLSX.utils.book_append_sheet(wb, wsGuide, 'Hướng dẫn tính toán');
    statusCellsBySheet.push([]);

    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Bo_Nganh_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo kèm hướng dẫn dành cho Bộ, ngành chủ trì!');
    return;
  }

  // 3. EXPLICIT PROVINCE REPORT
  if (templateMode === 'province') {
    const localityItems = filterGoalsForLocality(items);
    const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN MỤC TIÊU CHIẾN LƯỢC - DÀNH CHO ĐỊA PHƯƠNG${docSuffix}`;
    const wsReport = createGoalReportingWorksheet({
      items: localityItems,
      sheetTitle,
      agencyName: 'Ủy ban nhân dân các Tỉnh, Thành phố trực thuộc Trung ương',
      dateStr,
      guideText: 'HƯỚNG DẪN: Điền kết quả thực hiện của địa phương vào cột G (màu vàng); nêu khó khăn, kiến nghị tại cột H (màu xanh). Xem công thức tại Sheet "Hướng dẫn tính toán".',
      guidanceMap: activeGuidanceMap
    });

    XLSX.utils.book_append_sheet(wb, wsReport, 'Địa phương báo cáo');
    statusCellsBySheet.push([]);

    const wsGuide = createGoalGuidanceWorksheet({
      items: localityItems,
      sheetTitle: 'HƯỚNG DẪN CÔNG THỨC VÀ CĂN CỨ ĐO LƯỜNG - DÀNH CHO ĐỊA PHƯƠNG',
      docSubTitle: `Căn cứ Quyết định số ${docNumberStr} của Thủ tướng Chính phủ | Ngày xuất file: ${dateStr}`,
      dateStr,
      guidanceMap: activeGuidanceMap
    });
    XLSX.utils.book_append_sheet(wb, wsGuide, 'Hướng dẫn tính toán');
    statusCellsBySheet.push([]);

    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Dia_Phuong_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo kèm hướng dẫn dành cho Địa phương!');
    return;
  }

  // 4. DUAL-SHEET COMPREHENSIVE REPORT + GUIDANCE SHEET (Default when all agencies selected)
  // Sheet 1: Bộ ngành báo cáo
  const ministryTitle = `BÁO CÁO TIẾN ĐỘ MỤC TIÊU - DÀNH CHO CÁC BỘ, NGÀNH CHỦ TRÌ${docSuffix}`;
  const wsMinistry = createGoalReportingWorksheet({
    items,
    sheetTitle: ministryTitle,
    agencyName: 'Các Bộ, cơ quan chủ trì theo dõi mục tiêu toàn quốc',
    dateStr,
    guideText: 'HƯỚNG DẪN: Biểu mẫu dành cho các Bộ, cơ quan chủ trì đầu mối cập nhật số liệu toàn quốc vào cột G (màu vàng); ghi chú tại cột H.',
    guidanceMap: activeGuidanceMap
  });
  XLSX.utils.book_append_sheet(wb, wsMinistry, 'Bộ ngành báo cáo');
  statusCellsBySheet.push([]);

  // Sheet 2: Địa phương báo cáo
  const localityItems = filterGoalsForLocality(items);
  const localityTitle = `BÁO CÁO TIẾN ĐỘ MỤC TIÊU - DÀNH CHO ĐỊA PHƯƠNG THỰC HIỆN${docSuffix}`;
  const wsLocality = createGoalReportingWorksheet({
    items: localityItems,
    sheetTitle: localityTitle,
    agencyName: 'Ủy ban nhân dân các Tỉnh, Thành phố trực thuộc Trung ương',
    dateStr,
    guideText: 'HƯỚNG DẪN: Biểu mẫu dành cho các Địa phương (Tỉnh/Thành phố) tự đo lường và đối soát số liệu địa phương vào cột G (màu vàng); ghi chú tại cột H.',
    guidanceMap: activeGuidanceMap
  });
  XLSX.utils.book_append_sheet(wb, wsLocality, 'Địa phương báo cáo');
  statusCellsBySheet.push([]);

  // Sheet 3: Hướng dẫn tính toán
  const wsGuideAll = createGoalGuidanceWorksheet({
    items,
    sheetTitle: `HƯỚNG DẪN CÔNG THỨC TOÁN HỌC VÀ CĂN CỨ ĐO LƯỜNG TOÀN BỘ ${totalGoals} MỤC TIÊU (${docNumberStr})`,
    docSubTitle: `Căn cứ Quyết định số ${docNumberStr} của Thủ tướng Chính phủ | Ngày xuất file: ${dateStr}`,
    dateStr,
    guidanceMap: activeGuidanceMap
  });
  XLSX.utils.book_append_sheet(wb, wsGuideAll, 'Hướng dẫn tính toán');
  statusCellsBySheet.push([]);

  const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Tong_Hop_${dateFileStr}.xlsx`;
  downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
  toast.success('Đã xuất biểu mẫu báo cáo tiến độ mục tiêu tổng hợp (gồm tab Bộ ngành, Địa phương & Hướng dẫn tính toán)!');
}
