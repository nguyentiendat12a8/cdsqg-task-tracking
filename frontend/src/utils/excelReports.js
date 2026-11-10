import XLSX from 'xlsx-js-style/dist/xlsx.bundle.js';
import { toast } from 'vue3-toastify';
import { styleWorksheet } from './worksheetStyle.js';
import { formatDateRange, formatMeasuredValue } from '../shared/formatters.js';
import { reportStatusLabel, reportStatusOptions } from '../shared/reportStatuses.js';
import { downloadProgressWorkbook } from './progressWorkbook.js';
import { GOAL_GUIDANCE_2629 } from '../config/goalGuidance2629.js';
import { GOAL_GUIDANCE_1266 } from '../config/goalGuidance1266.js';
import { isAgencySumGoal } from '../shared/agencyPresentation.js';
import {
  GOAL_PLAN_SHEET_NAME, planYearHeader, isPlanEligibleGoal, leadsGoal, planYears, isPlanYearInRange, configuredPlanValue
} from './goalPlanSheet.js';

// Lời dặn chung đặt trên biểu báo cáo tiến độ mục tiêu.
const GOAL_REPORT_ACCURACY_NOTE = 'LƯU Ý: Các cơ quan khi tiến hành báo cáo tiến độ cần bảo đảm tính chính xác, khách quan, trung thực, có căn cứ và nguồn số liệu kiểm chứng rõ ràng; phương pháp tính do cơ quan chủ trì tự xác định, chịu trách nhiệm về số liệu báo cáo, làm cơ sở để tổng hợp, đánh giá đúng thực chất kết quả thực hiện và báo cáo cấp có thẩm quyền.';
const GOAL_REPORT_GUIDE = 'HƯỚNG DẪN: Điền vào ô màu vàng (tiến độ ước tính — số liệu định lượng ghi tổng kết quả đạt được đến năm báo cáo) và ô màu xanh (ghi chú, khó khăn, kiến nghị); không điền vào các ô khác.';
const GOAL_SUM_NOTE = 'Mục tiêu giao chung tính bằng số lượng (như MT-24): mỗi cơ quan báo cáo số liệu của cơ quan mình; hệ thống tự cộng tổng các cơ quan.';

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

// ---------------------------------------------------------------------------
// Biểu mẫu mục tiêu: hai sheet "Báo cáo" và "Kế hoạch mục tiêu" dùng chung một khung trình bày.
// Ô vàng / xanh là ô cần điền; ô xám "—" không điền; còn lại là thông tin tham chiếu.
// ---------------------------------------------------------------------------
const GOAL_SHEET_COLORS = {
  inputHeader: 'FEF08A', inputCell: 'FEFCE8', inputText: '047857',
  notesHeader: 'BAE6FD', notesCell: 'F0F9FF',
  noteFill: 'FFFBEB', noteText: '78350F',
  disabledFill: 'F1F5F9', disabledText: '94A3B8',
  border: 'CBD5E1'
};
const goalBorder = () => {
  const side = { style: 'thin', color: { rgb: GOAL_SHEET_COLORS.border } };
  return { top: side, bottom: side, left: side, right: side };
};
const goalFont = (extra = {}) => ({ name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' }, ...extra });

/**
 * Khung chung: tiêu đề (dòng 1), đơn vị/ngày (dòng 2), hướng dẫn (dòng 3), tiêu đề cột (dòng 4).
 * inputs: [{ col, kind: 'input' | 'notes' }] — các cột cần điền.
 */
function styleGoalSheet(ws, { numCols, rowCount, noteLines, inputs, titleCol = 2, centerCols = [] }) {
  styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);
  for (const merge of ws['!merges'] || []) {
    if (merge.s.r < 3) merge.e.c = numCols - 1;
  }
  const border = goalBorder();
  const rows = ws['!rows'] = ws['!rows'] || [];
  rows[0] = { hpt: 34 };
  rows[1] = { hpt: 22 };
  rows[2] = { hpt: Math.max(30, noteLines * 17 + 12) };
  rows[3] = { hpt: 36 };

  const note = ws[XLSX.utils.encode_cell({ r: 2, c: 0 })];
  if (note) {
    note.s = {
      font: goalFont({ color: { rgb: GOAL_SHEET_COLORS.noteText } }),
      fill: { fgColor: { rgb: GOAL_SHEET_COLORS.noteFill } },
      alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
      border
    };
  }
  for (const { col, kind } of inputs) {
    const header = ws[XLSX.utils.encode_cell({ r: 3, c: col })];
    if (header) header.s = {
      font: goalFont({ bold: true }),
      fill: { fgColor: { rgb: kind === 'notes' ? GOAL_SHEET_COLORS.notesHeader : GOAL_SHEET_COLORS.inputHeader } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border
    };
  }

  for (let r = 4; r < rowCount; r++) {
    const title = String(ws[XLSX.utils.encode_cell({ r, c: titleCol })]?.v ?? '');
    const width = ws['!cols']?.[titleCol]?.wch ?? 50;
    rows[r] = { hpt: Math.max(32, Math.ceil(title.length / (width * 0.95)) * 15 + 12) };
    for (let c = 0; c < numCols; c++) {
      const cell = ws[XLSX.utils.encode_cell({ r, c })];
      if (!cell) continue;
      const input = inputs.find(entry => entry.col === c);
      if (input && cell.v === '—') {
        cell.s = { font: goalFont({ color: { rgb: GOAL_SHEET_COLORS.disabledText } }), fill: { fgColor: { rgb: GOAL_SHEET_COLORS.disabledFill } }, alignment: { horizontal: 'center', vertical: 'center' }, border };
      } else if (input?.kind === 'notes') {
        cell.s = { font: goalFont(), fill: { fgColor: { rgb: GOAL_SHEET_COLORS.notesCell } }, alignment: { horizontal: 'left', vertical: 'center', wrapText: true }, border };
      } else if (input) {
        cell.s = { font: goalFont({ bold: true, color: { rgb: GOAL_SHEET_COLORS.inputText } }), fill: { fgColor: { rgb: GOAL_SHEET_COLORS.inputCell } }, alignment: { horizontal: 'center', vertical: 'center' }, border };
        if (typeof cell.v === 'number') cell.z = Number.isInteger(cell.v) ? '#,##0' : '#,##0.0##'; // tránh hiện "33."
      } else {
        cell.s = {
          ...cell.s,
          font: goalFont({ bold: c === 1, color: { rgb: c === 1 ? '1E3A8A' : '0F172A' } }),
          alignment: { horizontal: c === 0 || c === 1 || centerCols.includes(c) ? 'center' : 'left', vertical: 'center', wrapText: true },
          border
        };
      }
    }
  }
}

/**
 * Sheet 1 — Báo cáo tiến độ mục tiêu (gửi đơn vị điền rồi nhập lại).
 */
function createGoalReportingWorksheet({
  items = [],
  sheetTitle = 'BÁO CÁO TIẾN ĐỘ THỰC HIỆN CÁC MỤC TIÊU CHIẾN LƯỢC',
  agencyName = 'Bộ, Ngành, Địa phương',
  dateStr = '',
  guideText = GOAL_REPORT_GUIDE,
  guidanceMap = GOAL_GUIDANCE_2629
}) {
  const headers = [
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    'Cơ quan chủ trì',
    'Đơn vị tính',
    'Tiến độ hiện tại',
    'TIẾN ĐỘ ƯỚC TÍNH [CẦN ĐIỀN]',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]'
  ];

  const notes = [guideText, items.some(isAgencySumGoal) ? GOAL_SUM_NOTE : '', GOAL_REPORT_ACCURACY_NOTE].filter(Boolean);
  const rows = [
    [sheetTitle.toUpperCase()],
    [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
    [notes.join('\n')],
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
      '', // TIẾN ĐỘ ƯỚC TÍNH (G)
      ''  // GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ (H)
    ]);
  });

  const ws = XLSX.utils.aoa_to_sheet(rows);
  const numCols = headers.length;
  ws['!cols'] = [
    { wch: 6 },  // STT
    { wch: 10 }, // Mã MT
    { wch: 52 }, // Tên mục tiêu / Chỉ tiêu
    { wch: 26 }, // Cơ quan chủ trì
    { wch: 14 }, // Đơn vị tính
    { wch: 18 }, // Tiến độ hiện tại
    { wch: 24 }, // Tiến độ ước tính (vàng)
    { wch: 44 }  // Ghi chú (xanh)
  ];
  styleGoalSheet(ws, {
    numCols,
    rowCount: rows.length,
    noteLines: notes.length + 2,
    inputs: [{ col: 6, kind: 'input' }, { col: 7, kind: 'notes' }],
    centerCols: [4, 5]
  });
  return ws;
}

/**
 * Sheet "Kế hoạch mục tiêu": cơ quan chủ trì điền kế hoạch (chỉ tiêu cần đạt đến từng năm)
 * căn cứ kế hoạch chuyển đổi số 5 năm đã ban hành. Chỉ điền sẵn bản thiết lập;
 * dữ liệu mặc định không ghi vào Excel. Số khác bản thiết lập cũ cần xác nhận ghi đè.
 * Mục tiêu giao chung (như MT-24) không có kế hoạch riêng nên không xuất hiện ở sheet này.
 */
function createGoalPlanWorksheet({
  items = [],
  sheetTitle = 'KẾ HOẠCH MỤC TIÊU DO CƠ QUAN CHỦ TRÌ',
  agencyName = 'Bộ, Ngành, Địa phương',
  dateStr = '',
  showLeadAgency = false,
  guidanceMap = GOAL_GUIDANCE_2629
}) {
  const goals = items.filter(isPlanEligibleGoal);
  const years = planYears(goals.length ? goals : items);
  const headers = [
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    ...(showLeadAgency ? ['Cơ quan chủ trì'] : []),
    'Đơn vị tính',
    ...years.map(planYearHeader)
  ];
  const firstYearCol = headers.length - years.length;
  const notes = [
    'CĂN CỨ: Kế hoạch mục tiêu được xây dựng căn cứ theo Kế hoạch chuyển đổi số 5 năm của cơ quan, đơn vị đã được cấp có thẩm quyền ban hành. Trường hợp chưa ban hành kế hoạch thì khẩn trương rà soát, xây dựng và ban hành theo đúng quy định.',
    'HƯỚNG DẪN: Điền vào ô màu vàng; không điền vào ô màu xám (—).'
  ];
  const rows = [
    [sheetTitle.toUpperCase()],
    [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
    [notes.join('\n')],
    headers
  ];

  goals.forEach((item, idx) => {
    const guidance = guidanceMap[item.code] || {};
    rows.push([
      idx + 1,
      item.code || '',
      item.title || '',
      ...(showLeadAgency ? [guidance.leadAgency || item.leadAgencyName || item.leadAgency?.name || '—'] : []),
      guidance.unit || item.unitName || item.unit?.name || '%',
      ...years.map(year => isPlanYearInRange(item, year, years) ? (configuredPlanValue(item, year) ?? '') : '—')
    ]);
  });
  if (goals.length === 0) rows.push(['Không có mục tiêu định lượng do cơ quan chủ trì cần lập kế hoạch trong văn bản này.']);

  const ws = XLSX.utils.aoa_to_sheet(rows);
  const numCols = headers.length;
  ws['!cols'] = [
    { wch: 6 },
    { wch: 10 },
    { wch: 52 },
    ...(showLeadAgency ? [{ wch: 26 }] : []),
    { wch: 14 },
    ...years.map(() => ({ wch: 15 }))
  ];
  styleGoalSheet(ws, {
    numCols,
    rowCount: 4 + goals.length,
    noteLines: 4,
    inputs: years.map((_, index) => ({ col: firstYearCol + index, kind: 'input' })),
    centerCols: [firstYearCol - 1]
  });
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
    const headers = ['STT', 'Mã', `Tên ${noun}`, 'Cơ Quan Chủ Trì', 'Đơn Vị Tính', 'Tiến Độ Hiện Tại', 'TIẾN ĐỘ ƯỚC TÍNH [CẦN ĐIỀN]', 'GHI CHÚ [CẦN ĐIỀN]'];
    const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
    const docSuffix = documentTitle
      ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
      : '';
    const agencyName = targetAgency?.name || currentAgency?.name || 'Bộ, Ngành, Địa phương';
    const rows = [
      [`BÁO CÁO TIẾN ĐỘ THỰC HIỆN ${noun.toUpperCase()}${docSuffix}`],
      [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
      [`HƯỚNG DẪN: Điền tiến độ ước tính (tổng kết quả đã đạt đến năm báo cáo) hoặc chọn trạng thái ở cột [G]; ghi chú ở cột [H]. Chọn năm báo cáo khi import. Kế hoạch mục tiêu do cơ quan chủ trì điền tại sheet "Kế hoạch mục tiêu".\n${GOAL_REPORT_ACCURACY_NOTE}`],
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
    ws['!rows'][2] = { hpt: Math.max(ws['!rows'][2]?.hpt ?? 0, 84) };
    ws['!rows'][3] = { hpt: 36 };
    for (const [column, color] of [[inputCol, 'FEF08A'], [notesCol, 'BAE6FD']]) {
      const ref = XLSX.utils.encode_cell({ r: 3, c: column });
      ws[ref].s = { ...ws[ref].s, font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } }, fill: { fgColor: { rgb: color } } };
    }
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Báo Cáo Tiến Độ');
    const planAgency = targetAgency || (currentAgency && String(userRole).toLowerCase() !== 'admin' ? currentAgency : null);
    const wsPlan = createGoalPlanWorksheet({
      items: planAgency ? items.filter(item => leadsGoal(item, planAgency.id)) : items,
      sheetTitle: `KẾ HOẠCH MỤC TIÊU DO CƠ QUAN CHỦ TRÌ${docSuffix}`,
      agencyName,
      dateStr,
      showLeadAgency: !planAgency,
      guidanceMap: {}
    });
    XLSX.utils.book_append_sheet(wb, wsPlan, GOAL_PLAN_SHEET_NAME);
    downloadProgressWorkbook(wb, [statusCells, []], `${fileName}_${dateFileStr}.xlsx`, XLSX);
    toast.success('Đã xuất file Excel Báo Cáo Tiến Độ thành công!');
    return;
  }

  // -------------------------------------------------------------
  // BRANCH 2B: SPECIALIZED GOAL PROGRESS REPORT (Báo cáo tiến độ + Kế hoạch mục tiêu, theo cơ quan)
  // -------------------------------------------------------------
  const wb = XLSX.utils.book_new();
  const statusCellsBySheet = [];
  const is1266 = String(documentCode || '').includes('1266') ||
                 String(documentNumber || '').includes('1266') ||
                 String(documentTitle || '').includes('1266') ||
                 items.some(i => i.code && GOAL_GUIDANCE_1266[i.code] && !GOAL_GUIDANCE_2629[i.code]);
  const activeGuidanceMap = is1266 ? GOAL_GUIDANCE_1266 : GOAL_GUIDANCE_2629;
  const docCode = is1266 ? '1266' : (documentCode || (documentNumber ? documentNumber.replace(/\D/g, '') : '2629'));
  const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
  const docSuffix = documentTitle
    ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
    : '';

  const effectiveAgency = targetAgency || (currentAgency && String(userRole).toLowerCase() !== 'admin' ? currentAgency : null);

  const planTitle = `KẾ HOẠCH MỤC TIÊU DO CƠ QUAN CHỦ TRÌ${docSuffix}`;
  const leadOf = item => ({ name: item.leadAgencyName || item.leadAgency?.name || '', code: item.leadAgencyCode || '' });
  const appendPlanSheet = (planItems, agencyLabel, showLeadAgency) => {
    const wsPlan = createGoalPlanWorksheet({
      items: planItems,
      sheetTitle: planTitle,
      agencyName: agencyLabel,
      dateStr,
      showLeadAgency,
      guidanceMap: activeGuidanceMap
    });
    XLSX.utils.book_append_sheet(wb, wsPlan, GOAL_PLAN_SHEET_NAME);
    statusCellsBySheet.push([]);
  };

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
      guidanceMap: activeGuidanceMap
    });

    const sheetName = `Báo Cáo - ${(effectiveAgency.code || effectiveAgency.name).slice(0, 20)}`;
    XLSX.utils.book_append_sheet(wb, wsReport, sheetName);
    statusCellsBySheet.push([]);

    // Sheet 2: Kế hoạch mục tiêu do chính cơ quan chủ trì (phương pháp tính do cơ quan chủ trì tự xác định)
    appendPlanSheet(filteredItems.filter(item => leadsGoal(item, effectiveAgency.id)), effectiveAgency.name, false);

    const safeAgencyCode = (effectiveAgency.code || effectiveAgency.name.replace(/\s+/g, '_')).slice(0, 15);
    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_${safeAgencyCode}_${dateFileStr}.xlsx`;

    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success(`Đã xuất biểu mẫu báo cáo tiến độ và kế hoạch mục tiêu cho ${effectiveAgency.name}!`);
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
      guidanceMap: activeGuidanceMap
    });

    XLSX.utils.book_append_sheet(wb, wsReport, 'Bộ ngành báo cáo');
    statusCellsBySheet.push([]);

    appendPlanSheet(items.filter(item => !isProvinceAgency(leadOf(item))), 'Các Bộ, cơ quan chủ trì mục tiêu', true);

    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Bo_Nganh_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo tiến độ và kế hoạch mục tiêu dành cho Bộ, ngành chủ trì!');
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
      guidanceMap: activeGuidanceMap
    });

    XLSX.utils.book_append_sheet(wb, wsReport, 'Địa phương báo cáo');
    statusCellsBySheet.push([]);

    appendPlanSheet(localityItems.filter(item => isProvinceAgency(leadOf(item))), 'Ủy ban nhân dân các Tỉnh, Thành phố chủ trì mục tiêu', true);

    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Dia_Phuong_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo tiến độ và kế hoạch mục tiêu dành cho Địa phương!');
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
    guidanceMap: activeGuidanceMap
  });
  XLSX.utils.book_append_sheet(wb, wsLocality, 'Địa phương báo cáo');
  statusCellsBySheet.push([]);

  // Sheet 3: Kế hoạch mục tiêu của các cơ quan chủ trì
  appendPlanSheet(items, 'Các Bộ, ngành, địa phương chủ trì mục tiêu', true);

  const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Tong_Hop_${dateFileStr}.xlsx`;
  downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
  toast.success('Đã xuất biểu mẫu báo cáo tiến độ mục tiêu tổng hợp (gồm tab Bộ ngành, Địa phương & Kế hoạch mục tiêu)!');
}
