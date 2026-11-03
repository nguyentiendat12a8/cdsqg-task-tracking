import XLSX from 'xlsx-js-style/dist/xlsx.bundle.js';
import { toast } from 'vue3-toastify';
import { styleWorksheet } from './worksheetStyle.js';
import { formatMeasuredValue } from '../shared/formatters.js';
import { reportStatusLabel, reportStatusOptions } from '../shared/reportStatuses.js';
import { downloadProgressWorkbook } from './progressWorkbook.js';
import { GOAL_GUIDANCE_2629 } from '../config/goalGuidance2629.js';

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
function createGoalWorksheet({
  items = [],
  sheetTitle = 'BÁO CÁO TIẾN ĐỘ THỰC HIỆN CÁC MỤC TIÊU CHIẾN LƯỢC',
  agencyName = 'Bộ, Ngành, Địa phương',
  dateStr = '',
  guideText = 'HƯỚNG DẪN: Điền kết quả thực hiện mới vào cột J (màu vàng) theo công thức tính tại cột G; đánh giá tại cột K; nêu rõ khó khăn, vướng mắc tại cột L (màu xanh).'
}) {
  const headers = [
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    'Cơ quan chủ trì / Đầu mối TƯ',
    'Đơn vị tính',
    'Chỉ tiêu 2026',
    'Công thức toán học & Hướng dẫn tính',
    'Nguồn dữ liệu / Căn cứ kiểm chứng',
    'Tiến độ hiện tại',
    'TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]',
    'ĐÁNH GIÁ [Đạt / Chưa đạt / Đang thực hiện]',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]'
  ];

  const rows = [
    [sheetTitle.toUpperCase()],
    [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
    [guideText],
    headers
  ];

  const statusCells = [];

  items.forEach((item, idx) => {
    const guidance = GOAL_GUIDANCE_2629[item.code] || {};
    const unit = guidance.unit || item.unitName || item.unit?.name || '%';
    const target = guidance.target2026Display || (item.targetValue != null ? String(item.targetValue) : '—');
    const leadAgency = guidance.leadAgency || item.leadAgencyName || item.leadAgency?.name || '—';
    const source = guidance.source || 'Hệ thống thông tin của Bộ, ngành, địa phương';

    let formulaAndGuidance = '';
    if (guidance.formula) {
      formulaAndGuidance = `Công thức: ${guidance.formula}`;
      if (guidance.numerator) formulaAndGuidance += `\n- Tử số (A): ${guidance.numerator}`;
      if (guidance.denominator) formulaAndGuidance += `\n- Mẫu số (B): ${guidance.denominator}`;
      if (guidance.guidance) formulaAndGuidance += `\n- Hướng dẫn: ${guidance.guidance}`;
    } else {
      formulaAndGuidance = item.description || 'Theo dõi và tổng hợp kết quả định kỳ theo quy định.';
    }

    const qualitative = ['Qualitative', '2', 2].includes(item.evaluationType);
    let currentVal = 'Chưa cập nhật';
    if (qualitative) {
      currentVal = item.latestQualitativeStatus ? reportStatusLabel(item.latestQualitativeStatus) : 'Chưa cập nhật';
    } else if (item.latestProgressValue != null) {
      currentVal = formatMeasuredValue(item.latestProgressValue, unit);
    }

    const rowIdx = rows.length + 1; // 1-based index in Excel
    rows.push([
      idx + 1,
      item.code || '',
      item.title || '',
      leadAgency,
      unit,
      target,
      formulaAndGuidance,
      source,
      currentVal,
      '', // TIẾN ĐỘ MỚI (Col 9 / J)
      '', // ĐÁNH GIÁ (Col 10 / K)
      ''  // GHI CHÚ (Col 11 / L)
    ]);

    statusCells.push(`K${rowIdx}`);
  });

  const ws = XLSX.utils.aoa_to_sheet(rows);
  const numCols = headers.length;

  // Set explicit column widths
  ws['!cols'] = [
    { wch: 6 },  // STT
    { wch: 12 }, // Mã MT
    { wch: 44 }, // Tên MT
    { wch: 28 }, // Cơ quan chủ trì
    { wch: 14 }, // ĐVT
    { wch: 16 }, // Chỉ tiêu 2026
    { wch: 54 }, // Công thức & Hướng dẫn
    { wch: 38 }, // Nguồn dữ liệu
    { wch: 18 }, // Tiến độ hiện tại
    { wch: 28 }, // Tiến độ mới [CẦN ĐIỀN]
    { wch: 22 }, // Đánh giá
    { wch: 42 }  // Ghi chú & Kiến nghị
  ];

  styleWorksheet(ws, { numCols, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);

  // Extend banner merges across all columns
  for (const merge of ws['!merges'] || []) {
    if (merge.s.r < 3) merge.e.c = numCols - 1;
  }

  // Row heights
  ws['!rows'] = ws['!rows'] || [];
  ws['!rows'][0] = { hpt: 32 };
  ws['!rows'][1] = { hpt: 22 };
  ws['!rows'][2] = { hpt: 38 };
  ws['!rows'][3] = { hpt: 30 };

  // Style input headers: Col 9 (Yellow), Col 11 (Soft Blue)
  const thinBorder = {
    top: { style: 'thin', color: { rgb: 'CBD5E1' } },
    bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
    left: { style: 'thin', color: { rgb: 'CBD5E1' } },
    right: { style: 'thin', color: { rgb: 'CBD5E1' } }
  };

  const refInputHdr = XLSX.utils.encode_cell({ r: 3, c: 9 });
  if (ws[refInputHdr]) {
    ws[refInputHdr].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'FEF08A' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: thinBorder
    };
  }

  const refNotesHdr = XLSX.utils.encode_cell({ r: 3, c: 11 });
  if (ws[refNotesHdr]) {
    ws[refNotesHdr].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'BAE6FD' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: thinBorder
    };
  }

  // Format and color data input rows
  for (let r = 4; r < rows.length; r++) {
    // Dynamic height based on formula text lines
    const formulaText = String(rows[r][6] || '');
    const lines = formulaText.split('\n').reduce((sum, line) => sum + Math.max(1, Math.ceil(line.length / 48)), 0);
    ws['!rows'][r] = { hpt: Math.max(34, lines * 15 + 10) };

    // Column 9 (Input Progress - soft yellow tint)
    const c9 = XLSX.utils.encode_cell({ r, c: 9 });
    if (ws[c9]) {
      ws[c9].s = {
        font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '047857' } },
        fill: { fgColor: { rgb: 'FEFCE8' } },
        alignment: { horizontal: 'center', vertical: 'center' },
        border: thinBorder
      };
    }

    // Column 10 (Evaluation - centered)
    const c10 = XLSX.utils.encode_cell({ r, c: 10 });
    if (ws[c10]) {
      ws[c10].s = {
        font: { name: 'Segoe UI', sz: 10, color: { rgb: '1E293B' } },
        alignment: { horizontal: 'center', vertical: 'center' },
        border: thinBorder
      };
    }

    // Column 11 (Notes - soft blue tint)
    const c11 = XLSX.utils.encode_cell({ r, c: 11 });
    if (ws[c11]) {
      ws[c11].s = {
        font: { name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' } },
        fill: { fgColor: { rgb: 'F0F9FF' } },
        alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
        border: thinBorder
      };
    }
  }

  return { ws, statusCells };
}

/**
 * Export specialized Progress Report template file for users to update & import back.
 */
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
  // BRANCH 1: TASK PROGRESS REPORT (Preserve existing logic)
  // -------------------------------------------------------------
  if (isTask) {
    const noun = 'Nhiệm Vụ';
    const numCols = 9;
    const currentCol = 6, inputCol = currentCol + 1, notesCol = inputCol + 1;
    const headers = ['STT', 'Mã', `Tên ${noun}`, 'Cơ Quan Chủ Trì', 'Đơn Vị Tính', 'Sản Phẩm Đầu Ra', 'Tiến Độ Hiện Tại', 'TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]', 'GHI CHÚ [CẦN ĐIỀN]', 'Mã Sản Phẩm'];
    const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
    const docSuffix = documentTitle
      ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
      : '';
    const agencyName = targetAgency?.name || currentAgency?.name || 'Bộ, Ngành, Địa phương';
    const rows = [
      [`BÁO CÁO TIẾN ĐỘ THỰC HIỆN ${noun.toUpperCase()}${docSuffix}`],
      [`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`],
      [`HƯỚNG DẪN: Điền tổng kết quả đã đạt đến năm báo cáo hoặc chọn trạng thái ở cột [H]; ghi chú ở cột [I]. Mỗi sản phẩm đầu ra chọn một trạng thái riêng. Chọn năm báo cáo khi import.`],
      headers
    ];
    const statusCells = [], merges = [];
    items.forEach((item, index) => {
      const qualitative = ['Qualitative', '2', 2].includes(item.evaluationType);
      const products = qualitative ? (item.deliverables || []) : [];
      const unit = qualitative ? 'Trạng thái' : (item.unitName || item.unit?.name || '%');
      const current = qualitative ? (item.latestQualitativeStatus ? reportStatusLabel(item.latestQualitativeStatus) : 'Chưa cập nhật') : (item.latestProgressValue != null ? formatMeasuredValue(item.latestProgressValue, unit) : 'Chưa cập nhật');
      const start = rows.length;
      (products.length ? products : [null]).forEach((product, productIndex) => {
        const row = productIndex === 0 ? [index + 1, item.code || '', item.title || '', item.leadAgencyName || item.leadAgency?.name || '—', unit] : ['', '', '', '', ''];
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
    ws['!cols'] = [6, 14, 50, 30, 16, 50, 22, 34, 40].map(wch => ({ wch }));
    ws['!cols'].push({ wch: 36, hidden: true });
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
  // BRANCH 2A: GENERIC GOAL REPORT (For non-2629 / non-specialized)
  // -------------------------------------------------------------
  const isGoal = !isTask;
  const isSpecializedGoalReport = isGoal && (
    templateMode === 'ministry' ||
    templateMode === 'province' ||
    Boolean(targetAgency) ||
    documentCode === '2629' ||
    String(documentNumber || '').includes('2629') ||
    String(documentTitle || '').includes('2629') ||
    items.some(i => GOAL_GUIDANCE_2629[i.code])
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
  // BRANCH 2B: SPECIALIZED GOAL PROGRESS REPORT (With 2629 formulas & Agency routing)
  // -------------------------------------------------------------
  const wb = XLSX.utils.book_new();
  const statusCellsBySheet = [];
  const docCode = documentCode || (documentNumber ? documentNumber.replace(/\D/g, '') : '2629');
  const typeText = documentShortTitle ? ` ${documentShortTitle.toUpperCase()}` : '';
  const docSuffix = documentTitle
    ? ` THEO DÕI${typeText} - ${documentTitle.toUpperCase()}${documentNumber ? ` (${documentNumber})` : ''}`
    : '';

  const effectiveAgency = targetAgency || (currentAgency && String(userRole).toLowerCase() !== 'admin' ? currentAgency : null);

  // Helper to filter goals led by a specific ministry
  const filterGoalsForMinistry = (goalList, agency) => {
    if (!agency) return goalList;
    const agId = agency.id ? String(agency.id).toLowerCase() : '';
    const agCode = (agency.code || '').toUpperCase();
    const agName = (agency.name || '').toLowerCase();

    const matched = goalList.filter(item => {
      if (item.leadAgencyId && agId && String(item.leadAgencyId).toLowerCase() === agId) return true;
      if (item.leadAgencyCode && agCode && item.leadAgencyCode.toUpperCase() === agCode) return true;
      const g = GOAL_GUIDANCE_2629[item.code];
      if (g && g.leadAgency) {
        const lead = g.leadAgency.toLowerCase();
        if (agCode === 'VPCP' && lead.includes('văn phòng chính phủ')) return true;
        if (agCode === 'BKHCN' && lead.includes('khoa học và công nghệ')) return true;
        if (agCode === 'BCA' && lead.includes('công an')) return true;
        if (agCode === 'BNV' && lead.includes('nội vụ')) return true;
        if (lead.includes(agName.replace('bộ ', '').replace('ban ', '').trim())) return true;
      }
      return false;
    });
    return matched.length > 0 ? matched : goalList;
  };

  // Helper to filter goals that localities measure & report
  const filterGoalsForLocality = (goalList) => {
    return goalList.filter(item => {
      const g = GOAL_GUIDANCE_2629[item.code];
      if (g) return g.isLocalReporting !== false;
      return true;
    });
  };

  // 1. SPECIFIC SINGLE AGENCY REPORT (When filtered to 1 agency)
  if (effectiveAgency && templateMode === 'auto') {
    const isMinistry = isMinistryAgency(effectiveAgency);
    const isProvince = isProvinceAgency(effectiveAgency);

    let filteredItems = items;
    let agencyRoleTag = 'Bộ, Ngành, Địa phương';

    if (isMinistry) {
      filteredItems = filterGoalsForMinistry(items, effectiveAgency);
      agencyRoleTag = 'Bộ, Cơ quan chủ trì mục tiêu';
    } else if (isProvince) {
      filteredItems = filterGoalsForLocality(items);
      agencyRoleTag = 'Ủy ban nhân dân Tỉnh, Thành phố';
    }

    const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN CÁC MỤC TIÊU${docSuffix}`;
    const { ws, statusCells } = createGoalWorksheet({
      items: filteredItems,
      sheetTitle,
      agencyName: `${effectiveAgency.name} (${agencyRoleTag})`,
      dateStr,
      guideText: `HƯỚNG DẪN: Điền kết quả thực hiện mới của đơn vị vào cột J (màu vàng) theo công thức hướng dẫn tại cột G; chọn đánh giá tại cột K; nêu rõ khó khăn, vướng mắc tại cột L (màu xanh).`
    });

    const sheetName = `Báo Cáo - ${(effectiveAgency.code || effectiveAgency.name).slice(0, 20)}`;
    XLSX.utils.book_append_sheet(wb, ws, sheetName);
    statusCellsBySheet.push(statusCells);

    const safeAgencyCode = (effectiveAgency.code || effectiveAgency.name.replace(/\s+/g, '_')).slice(0, 15);
    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_${safeAgencyCode}_${dateFileStr}.xlsx`;

    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success(`Đã xuất biểu mẫu báo cáo cho ${effectiveAgency.name}!`);
    return;
  }

  // 2. EXPLICIT MINISTRY REPORT
  if (templateMode === 'ministry') {
    const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN MỤC TIÊU CHIẾN LƯỢC - DÀNH CHO BỘ, NGÀNH CHỦ TRÌ${docSuffix}`;
    const { ws, statusCells } = createGoalWorksheet({
      items,
      sheetTitle,
      agencyName: 'Các Bộ, Cơ quan ngang Bộ, Cơ quan thuộc Chính phủ chủ trì mục tiêu',
      dateStr,
      guideText: `HƯỚNG DẪN DÀNH CHO BỘ, NGÀNH CHỦ TRÌ: Cập nhật số liệu đo lường tập trung toàn quốc vào cột J (màu vàng) theo đúng công thức tại cột G; đánh giá tại cột K; nêu giải pháp thúc đẩy tại cột L (màu xanh).`
    });

    XLSX.utils.book_append_sheet(wb, ws, 'Bộ ngành báo cáo');
    statusCellsBySheet.push(statusCells);

    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Bo_Nganh_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo dành cho Bộ, ngành chủ trì!');
    return;
  }

  // 3. EXPLICIT PROVINCE REPORT
  if (templateMode === 'province') {
    const localityItems = filterGoalsForLocality(items);
    const sheetTitle = `BÁO CÁO TIẾN ĐỘ THỰC HIỆN MỤC TIÊU CHIẾN LƯỢC - DÀNH CHO ĐỊA PHƯƠNG${docSuffix}`;
    const { ws, statusCells } = createGoalWorksheet({
      items: localityItems,
      sheetTitle,
      agencyName: 'Ủy ban nhân dân các Tỉnh, Thành phố trực thuộc Trung ương',
      dateStr,
      guideText: `HƯỚNG DẪN DÀNH CHO ĐỊA PHƯƠNG: Cập nhật kết quả triển khai trên địa bàn tỉnh/thành phố vào cột J (màu vàng) theo hướng dẫn tại cột G; đánh giá tại cột K; nêu khó khăn, kiến nghị tại cột L (màu xanh).`
    });

    XLSX.utils.book_append_sheet(wb, ws, 'Địa phương báo cáo');
    statusCellsBySheet.push(statusCells);

    const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Dia_Phuong_${dateFileStr}.xlsx`;
    downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
    toast.success('Đã xuất biểu mẫu báo cáo dành cho Địa phương!');
    return;
  }

  // 4. DUAL-SHEET COMPREHENSIVE REPORT (Default when all agencies selected)
  // Sheet 1: Bộ ngành báo cáo
  const ministryTitle = `BÁO CÁO TIẾN ĐỘ MỤC TIÊU - DÀNH CHO CÁC BỘ, NGÀNH CHỦ TRÌ${docSuffix}`;
  const ministryResult = createGoalWorksheet({
    items,
    sheetTitle: ministryTitle,
    agencyName: 'Các Bộ, cơ quan chủ trì theo dõi mục tiêu toàn quốc',
    dateStr,
    guideText: `HƯỚNG DẪN: Biểu mẫu dành cho các Bộ, cơ quan chủ trì đầu mối cập nhật số liệu toàn quốc vào cột J (màu vàng); đánh giá tại cột K; ghi chú tại cột L.`
  });
  XLSX.utils.book_append_sheet(wb, ministryResult.ws, 'Bộ ngành báo cáo');
  statusCellsBySheet.push(ministryResult.statusCells);

  // Sheet 2: Địa phương báo cáo
  const localityItems = filterGoalsForLocality(items);
  const localityTitle = `BÁO CÁO TIẾN ĐỘ MỤC TIÊU - DÀNH CHO ĐỊA PHƯƠNG THỰC HIỆN${docSuffix}`;
  const localityResult = createGoalWorksheet({
    items: localityItems,
    sheetTitle: localityTitle,
    agencyName: 'Ủy ban nhân dân các Tỉnh, Thành phố trực thuộc Trung ương',
    dateStr,
    guideText: `HƯỚNG DẪN: Biểu mẫu dành cho các Địa phương (Tỉnh/Thành phố) tự đo lường và đối soát số liệu địa phương vào cột J (màu vàng); đánh giá tại cột K; ghi chú khó khăn tại cột L.`
  });
  XLSX.utils.book_append_sheet(wb, localityResult.ws, 'Địa phương báo cáo');
  statusCellsBySheet.push(localityResult.statusCells);

  const finalFileName = `Bao_Cao_Tien_Do_Muc_Tieu_${docCode}_Tong_Hop_${dateFileStr}.xlsx`;
  downloadProgressWorkbook(wb, statusCellsBySheet, finalFileName, XLSX);
  toast.success('Đã xuất biểu mẫu báo cáo tiến độ mục tiêu tổng hợp (gồm tab Bộ ngành & Địa phương)!');
}
