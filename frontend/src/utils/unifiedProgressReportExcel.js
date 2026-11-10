import { loadExcelWriter, loadExcelReader } from './excelRuntime.js';
import { downloadProgressWorkbook } from './progressWorkbook.js';
import { isProgressReportYearApplicable } from '../shared/progressReportCells.js';
import { toast } from 'vue3-toastify';

const THIN_BORDER = {
  top: { style: 'thin', color: { rgb: 'CBD5E1' } },
  bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
  left: { style: 'thin', color: { rgb: 'CBD5E1' } },
  right: { style: 'thin', color: { rgb: 'CBD5E1' } }
};

const HEADER_STYLE = {
  font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: 'FFFFFF' } },
  fill: { fgColor: { rgb: '1E3A8A' } },
  alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

const SUBHEADER_YELLOW_STYLE = {
  font: { name: 'Segoe UI', sz: 9.5, bold: true, color: { rgb: '0F172A' } },
  fill: { fgColor: { rgb: 'FEF08A' } },
  alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

const SECTION_STYLE = {
  font: { name: 'Segoe UI', sz: 10.5, bold: true, color: { rgb: 'FFFFFF' } },
  fill: { fgColor: { rgb: '1E3A8A' } },
  alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

const SECTION_PHASE_STYLE = {
  font: { name: 'Segoe UI', sz: 9.5, bold: true, color: { rgb: 'FFFFFF' } },
  fill: { fgColor: { rgb: '2563EB' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const TITLE_STYLE = {
  font: { name: 'Segoe UI', sz: 13, bold: true, color: { rgb: 'FFFFFF' } },
  fill: { fgColor: { rgb: '1E3A8A' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const SUBTITLE_STYLE = {
  font: { name: 'Segoe UI', sz: 10, italic: true, color: { rgb: '334155' } },
  fill: { fgColor: { rgb: 'F1F5F9' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const GUIDE_STYLE = {
  font: { name: 'Segoe UI', sz: 9.5, italic: true, color: { rgb: '78350F' } },
  fill: { fgColor: { rgb: 'FFFBEB' } },
  alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

const CELL_TEXT_LEFT = {
  font: { name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' } },
  fill: { fgColor: { rgb: 'F8FAFC' } },
  alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

const CELL_TEXT_CENTER = {
  font: { name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' } },
  fill: { fgColor: { rgb: 'F8FAFC' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const CELL_CODE = {
  font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '1E3A8A' } },
  fill: { fgColor: { rgb: 'F8FAFC' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const CELL_EDITABLE = {
  font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '047857' } },
  fill: { fgColor: { rgb: 'FEFCE8' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const CELL_DISABLED = {
  font: { name: 'Segoe UI', sz: 10, color: { rgb: '94A3B8' } },
  fill: { fgColor: { rgb: 'F1F5F9' } },
  alignment: { horizontal: 'center', vertical: 'center' },
  border: THIN_BORDER
};

const CELL_NOTES = {
  font: { name: 'Segoe UI', sz: 9.5, color: { rgb: '0F172A' } },
  fill: { fgColor: { rgb: 'FFFFFF' } },
  alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

function applyStyleToRange(ws, r1, c1, r2, c2, style, XLSX) {
  for (let r = r1; r <= r2; r++) {
    for (let c = c1; c <= c2; c++) {
      const cellRef = XLSX.utils.encode_cell({ r, c });
      if (!ws[cellRef]) {
        ws[cellRef] = { t: 's', v: '' };
      }
      ws[cellRef].s = { ...style };
    }
  }
}

export function parseVNNumber(val) {
  if (val == null || val === '' || val === '—') return null;
  if (typeof val === 'number') return isNaN(val) ? null : val;
  let s = String(val).trim().replace(/%/g, '').trim();
  if (!s || s === '—') return null;

  // Format with thousand dots: e.g. "10.000.000" or "10.000.000,5"
  if (/^-?\d{1,3}(\.\d{3})+(,\d+)?$/.test(s)) {
    s = s.replace(/\./g, '').replace(/,/g, '.');
    const n = parseFloat(s);
    return isNaN(n) ? null : n;
  }

  // Format with thousand commas: e.g. "10,000,000" or "10,000,000.5"
  if (/^-?\d{1,3}(,\d{3})+(\.\d+)?$/.test(s)) {
    s = s.replace(/,/g, '');
    const n = parseFloat(s);
    return isNaN(n) ? null : n;
  }

  // Simple comma decimal: e.g. "41,2"
  if (/^-?\d+,\d+$/.test(s)) {
    s = s.replace(/,/g, '.');
    const n = parseFloat(s);
    return isNaN(n) ? null : n;
  }

  const n = parseFloat(s);
  return isNaN(n) ? null : n;
}

function formatExcelTarget(val) {
  if (val == null || val === '' || val === '—') return '';
  const s = String(val).trim();
  let clean = s;
  if (clean.endsWith('.0')) clean = clean.slice(0, -2);
  else if (clean.endsWith('.00')) clean = clean.slice(0, -3);

  const parsed = parseVNNumber(clean);
  if (parsed != null) {
    return parsed;
  }
  return clean;
}

function formatExcelNumericVal(val, isApplicable) {
  if (!isApplicable) return '—';
  if (val == null || val === '') {
    return isApplicable ? '' : '—';
  }
  if (val === '—') return '—';
  const parsed = parseVNNumber(val);
  if (parsed != null) {
    return parsed;
  }
  const s = String(val).trim();
  if (s === '—' || !s) return isApplicable ? '' : '—';
  return s;
}

/**
 * Builds Sheet 1: Mục tiêu
 */
export function buildGoalsWorksheet(goals, agencyName, dateStr, XLSX) {
  const rows = [];
  const merges = [];

  // Row 1: Title (Row 0)
  rows.push(['BIỂU MẪU BÁO CÁO MỤC TIÊU DO CƠ QUAN CHỦ TRÌ THEO DÕI']);
  merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: 12 } });

  // Row 2: Subtitle (Row 1)
  rows.push([`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`]);
  merges.push({ s: { r: 1, c: 0 }, e: { r: 1, c: 12 } });

  // Row 3: Căn cứ & Hướng dẫn (Row 2)
  rows.push([
    'CĂN CỨ: Kế hoạch mục tiêu được xây dựng căn cứ theo Kế hoạch chuyển đổi số 5 năm của cơ quan, đơn vị đã được cấp có thẩm quyền ban hành. Trường hợp chưa ban hành kế hoạch thì khẩn trương rà soát, xây dựng và ban hành theo đúng quy định.\nHƯỚNG DẪN: Điền vào ô màu vàng (ước tính 2026 và kế hoạch 2027-2030); không điền vào ô màu xám (—).'
  ]);
  merges.push({ s: { r: 2, c: 0 }, e: { r: 2, c: 12 } });

  // Row 4: Level 1 Header (Row 3)
  rows.push([
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    'ĐVT',
    'Kết quả theo QĐ',
    '',
    'Kết quả thực hiện',
    '',
    'Mục tiêu theo từng năm (đến 2030)',
    '',
    '',
    '',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]'
  ]);

  // Row 5: Level 2 Header (Row 4)
  rows.push([
    '', '', '', '',
    'Giai đoạn 1',
    'Giai đoạn 2',
    'Kết quả 2025',
    'Ước tính 2026',
    '2027',
    '2028',
    '2029',
    '2030',
    ''
  ]);

  // Header Merges (2 rows total: r:3 and r:4)
  merges.push({ s: { r: 3, c: 0 }, e: { r: 4, c: 0 } }); // STT (A4:A5)
  merges.push({ s: { r: 3, c: 1 }, e: { r: 4, c: 1 } }); // Mã MT (B4:B5)
  merges.push({ s: { r: 3, c: 2 }, e: { r: 4, c: 2 } }); // Tên MT (C4:C5)
  merges.push({ s: { r: 3, c: 3 }, e: { r: 4, c: 3 } }); // ĐVT (D4:D5)
  merges.push({ s: { r: 3, c: 4 }, e: { r: 3, c: 5 } }); // Kết quả theo QĐ (E4:F4)
  merges.push({ s: { r: 3, c: 6 }, e: { r: 3, c: 7 } }); // Kết quả thực hiện (G4:H4)
  merges.push({ s: { r: 3, c: 8 }, e: { r: 3, c: 11 } }); // Mục tiêu theo từng năm (đến 2030) (I4:L4)
  merges.push({ s: { r: 3, c: 12 }, e: { r: 4, c: 12 } }); // Ghi chú (M4:M5)

  const goals2629 = (goals || []).filter(g => String(g.documentNumber).includes('2629'));
  const goals1266 = (goals || []).filter(g => String(g.documentNumber).includes('1266'));

  const sectionRows = [];
  const editableCellCoords = [];
  const disabledCellCoords = [];
  const numericCellCoords = [];

  // Helper to add goals of a section
  function appendGoalsSection(sectionNumberRoman, docLabel, sectionGoals, colEHeader, colFHeader) {
    const secRowIdx = rows.length;
    sectionRows.push({ r: secRowIdx, colE: colEHeader, colF: colFHeader });
    const secRow = [sectionNumberRoman, docLabel, '', '', colEHeader, colFHeader, '', '', '', '', '', '', ''];
    rows.push(secRow);
    merges.push({ s: { r: secRowIdx, c: 1 }, e: { r: secRowIdx, c: 3 } });

    sectionGoals.forEach((g, idx) => {
      const rIdx = rows.length;

      const valAct2025 = formatExcelNumericVal(g.actual2025, isProgressReportYearApplicable(g, 2025));
      const valEst2026 = formatExcelNumericVal(g.estimate2026, isProgressReportYearApplicable(g, 2026));
      const valPlan2027 = formatExcelNumericVal(g.plan2027, isProgressReportYearApplicable(g, 2027));
      const valPlan2028 = formatExcelNumericVal(g.plan2028, isProgressReportYearApplicable(g, 2028));
      const valPlan2029 = formatExcelNumericVal(g.plan2029, isProgressReportYearApplicable(g, 2029));
      const valPlan2030 = formatExcelNumericVal(g.plan2030, isProgressReportYearApplicable(g, 2030));

      const valTarget1 = formatExcelTarget(g.targetPhase1);
      const valTarget2 = formatExcelTarget(g.targetPhase2);

      const row = [
        g.sequenceNumber || (idx + 1),
        g.code || '',
        g.title || '',
        g.unitName || '%',
        valTarget1,
        valTarget2,
        valAct2025,
        valEst2026,
        valPlan2027,
        valPlan2028,
        valPlan2029,
        valPlan2030,
        g.notes || ''
      ];
      rows.push(row);

      // Track numeric cells for number format z: '#,##0' / '#,##0.##'
      [
        { val: valTarget1, col: 4 },
        { val: valTarget2, col: 5 },
        { val: valAct2025, col: 6 },
        { val: valEst2026, col: 7 },
        { val: valPlan2027, col: 8 },
        { val: valPlan2028, col: 9 },
        { val: valPlan2029, col: 10 },
        { val: valPlan2030, col: 11 }
      ].forEach(item => {
        if (typeof item.val === 'number') {
          numericCellCoords.push({ r: rIdx, c: item.col, val: item.val });
        }
      });

      // Mark editable vs disabled cells
      const checkCell = (val, col) => {
        if (val === '—') disabledCellCoords.push({ r: rIdx, c: col });
        else editableCellCoords.push({ r: rIdx, c: col });
      };

      if (valAct2025 === '—') {
        disabledCellCoords.push({ r: rIdx, c: 6 });
      }

      checkCell(valEst2026, 7);
      checkCell(valPlan2027, 8);
      checkCell(valPlan2028, 9);
      checkCell(valPlan2029, 10);
      checkCell(valPlan2030, 11);
    });
  }

  // Section I: 2629
  appendGoalsSection('I', 'QUYẾT ĐỊNH SỐ 2629/QĐ-TTg NGÀY 10/12/2024', goals2629, '2025 - 2027', '2028 - 2030');

  // Section II: 1266
  appendGoalsSection('II', 'QUYẾT ĐỊNH SỐ 1266/QĐ-TTg NGÀY 24/10/2024', goals1266, '2026 - 2030', '2031 - 2045');

  const ws = XLSX.utils.aoa_to_sheet(rows);
  ws['!merges'] = merges;

  // Column Widths
  ws['!cols'] = [
    { wch: 6 },  // STT (A)
    { wch: 10 }, // Mã MT (B)
    { wch: 46 }, // Tên mục tiêu (C)
    { wch: 10 }, // ĐVT (D)
    { wch: 14 }, // Kết quả 1 (E)
    { wch: 14 }, // Kết quả 2 (F)
    { wch: 15 }, // Thực hiện 2025 (G)
    { wch: 16 }, // Ước tính 2026 (H)
    { wch: 14 }, // 2027 (I)
    { wch: 14 }, // 2028 (J)
    { wch: 14 }, // 2029 (K)
    { wch: 14 }, // 2030 (L)
    { wch: 45 }  // Ghi chú (M)
  ];

  // Set Row Heights
  ws['!rows'] = [
    { hpt: 28 }, // Title
    { hpt: 22 }, // Subtitle
    { hpt: 48 }, // Guide
    { hpt: 24 }, // Hdr 1
    { hpt: 24 }  // Hdr 2
  ];

  // Apply Styles
  applyStyleToRange(ws, 0, 0, 0, 12, TITLE_STYLE, XLSX);
  applyStyleToRange(ws, 1, 0, 1, 12, SUBTITLE_STYLE, XLSX);
  applyStyleToRange(ws, 2, 0, 2, 12, GUIDE_STYLE, XLSX);

  // Headers styling (Row 3 and Row 4)
  applyStyleToRange(ws, 3, 0, 4, 3, HEADER_STYLE, XLSX); // STT, Mã, Tên, ĐVT
  applyStyleToRange(ws, 3, 4, 4, 5, HEADER_STYLE, XLSX); // Kết quả theo QĐ
  applyStyleToRange(ws, 3, 6, 4, 6, HEADER_STYLE, XLSX); // Kết quả 2025
  applyStyleToRange(ws, 3, 7, 3, 7, HEADER_STYLE, XLSX);
  applyStyleToRange(ws, 4, 7, 4, 7, SUBHEADER_YELLOW_STYLE, XLSX); // Ước tính 2026
  applyStyleToRange(ws, 3, 8, 3, 11, HEADER_STYLE, XLSX);          // Mục tiêu từng năm
  applyStyleToRange(ws, 4, 8, 4, 11, SUBHEADER_YELLOW_STYLE, XLSX); // 2027-2030
  applyStyleToRange(ws, 3, 12, 4, 12, HEADER_STYLE, XLSX);         // Ghi chú

  // Section styling
  const secRowIndices = sectionRows.map(s => s.r);
  sectionRows.forEach(({ r }) => {
    applyStyleToRange(ws, r, 0, r, 3, SECTION_STYLE, XLSX);
    applyStyleToRange(ws, r, 4, r, 5, SECTION_PHASE_STYLE, XLSX);
    applyStyleToRange(ws, r, 6, r, 12, SECTION_STYLE, XLSX);
  });

  // Base Data Rows styling
  const headerCount = 5;
  for (let r = headerCount; r < rows.length; r++) {
    if (secRowIndices.includes(r)) continue;
    applyStyleToRange(ws, r, 0, r, 0, CELL_TEXT_CENTER, XLSX); // STT
    applyStyleToRange(ws, r, 1, r, 1, CELL_CODE, XLSX);        // Mã
    applyStyleToRange(ws, r, 2, r, 2, CELL_TEXT_LEFT, XLSX);   // Tên
    applyStyleToRange(ws, r, 3, r, 3, CELL_TEXT_CENTER, XLSX); // ĐVT
    applyStyleToRange(ws, r, 4, r, 5, CELL_TEXT_CENTER, XLSX); // Kết quả QĐ
    applyStyleToRange(ws, r, 6, r, 6, CELL_TEXT_CENTER, XLSX); // 2025
    applyStyleToRange(ws, r, 12, r, 12, CELL_NOTES, XLSX);     // Ghi chú
  }

  // Highlight editable & disabled cells
  editableCellCoords.forEach(({ r, c }) => {
    const ref = XLSX.utils.encode_cell({ r, c });
    if (ws[ref]) ws[ref].s = { ...CELL_EDITABLE };
  });

  disabledCellCoords.forEach(({ r, c }) => {
    const ref = XLSX.utils.encode_cell({ r, c });
    if (ws[ref]) ws[ref].s = { ...CELL_DISABLED };
  });

  // Number format on numeric cells
  numericCellCoords.forEach(({ r, c, val }) => {
    const ref = XLSX.utils.encode_cell({ r, c });
    if (ws[ref]) {
      ws[ref].t = 'n';
      ws[ref].v = val;
      ws[ref].z = Number.isInteger(val) ? '#,##0' : '#,##0.##';
    }
  });

  return ws;
}

/**
 * Builds Sheet 2: Nhiệm vụ
 */
export function buildTasksWorksheet(tasks, agencyName, dateStr, XLSX) {
  const rows = [];
  const merges = [];
  const statusCells = [];

  // Title (Row 0)
  rows.push(['BIỂU MẪU BÁO CÁO NHIỆM VỤ DO CƠ QUAN CHỦ TRÌ THEO DÕI']);
  merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: 7 } });

  // Subtitle (Row 1)
  rows.push([`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`]);
  merges.push({ s: { r: 1, c: 0 }, e: { r: 1, c: 7 } });

  // Guide (Row 2)
  rows.push([
    'HƯỚNG DẪN: Chọn Trạng thái thực hiện tại cột F (màu vàng); Ghi chú khó khăn vướng mắc tại cột H.'
  ]);
  merges.push({ s: { r: 2, c: 0 }, e: { r: 2, c: 7 } });

  // Headers (Row 3)
  const headers = [
    'STT',
    'Mã NV',
    'Tên nhiệm vụ',
    'Cơ quan phối hợp',
    'Hạn hoàn thành',
    'TRẠNG THÁI THỰC HIỆN [CẦN CHỌN]',
    'Sản phẩm bàn giao',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]',
    'ID Nhiệm Vụ (Ẩn)',
    'ID Quyết Định (Ẩn)'
  ];
  rows.push(headers);

  const sectionRows = [];
  const tasks2629 = (tasks || []).filter(t => String(t.documentNumber).includes('2629'));
  const tasks1266 = (tasks || []).filter(t => String(t.documentNumber).includes('1266'));

  function appendTasksSection(sectionNumberRoman, docLabel, sectionTasks) {
    const secRowIdx = rows.length;
    sectionRows.push(secRowIdx);
    const secRow = [sectionNumberRoman, docLabel, '', '', '', '', '', '', '', ''];
    rows.push(secRow);
    merges.push({ s: { r: secRowIdx, c: 1 }, e: { r: secRowIdx, c: 7 } });

    sectionTasks.forEach((t, idx) => {
      const rIdx = rows.length;
      const row = [
        t.sequenceNumber || (idx + 1),
        t.code || '',
        t.title || '',
        t.coordinatingAgencies || '',
        t.dueDate || '—',
        t.statusLabel || 'Chưa thực hiện',
        t.deliverables || '',
        t.notes || '',
        t.id || '',
        t.documentId || ''
      ];
      rows.push(row);

      // Track cell for Excel data validation dropdown (Column F)
      statusCells.push(`F${rIdx + 1}`);
    });
  }

  appendTasksSection('I', 'QUYẾT ĐỊNH SỐ 2629/QĐ-TTg NGÀY 10/12/2024', tasks2629);
  appendTasksSection('II', 'QUYẾT ĐỊNH SỐ 1266/QĐ-TTg NGÀY 24/10/2024', tasks1266);

  const ws = XLSX.utils.aoa_to_sheet(rows);
  ws['!merges'] = merges;

  ws['!cols'] = [
    { wch: 6 },  // STT (A)
    { wch: 12 }, // Mã NV (B)
    { wch: 45 }, // Tên NV (C)
    { wch: 25 }, // Phối hợp (D)
    { wch: 14 }, // Hạn (E)
    { wch: 28 }, // Trạng thái (F) - Editable
    { wch: 30 }, // Sản phẩm (G)
    { wch: 40 }, // Ghi chú (H) - Editable
    { wch: 36, hidden: true }, // ID NV (I)
    { wch: 36, hidden: true }  // ID QĐ (J)
  ];

  ws['!rows'] = [
    { hpt: 28 }, // Title
    { hpt: 22 }, // Subtitle
    { hpt: 32 }, // Guide
    { hpt: 26 }  // Header
  ];

  applyStyleToRange(ws, 0, 0, 0, 7, TITLE_STYLE, XLSX);
  applyStyleToRange(ws, 1, 0, 1, 7, SUBTITLE_STYLE, XLSX);
  applyStyleToRange(ws, 2, 0, 2, 7, GUIDE_STYLE, XLSX);

  // Headers
  applyStyleToRange(ws, 3, 0, 3, 4, HEADER_STYLE, XLSX);
  applyStyleToRange(ws, 3, 5, 3, 5, SUBHEADER_YELLOW_STYLE, XLSX);
  applyStyleToRange(ws, 3, 6, 3, 7, HEADER_STYLE, XLSX);

  // Section Rows
  sectionRows.forEach(r => {
    applyStyleToRange(ws, r, 0, r, 7, SECTION_STYLE, XLSX);
  });

  // Data rows
  for (let r = 4; r < rows.length; r++) {
    if (sectionRows.includes(r)) continue;
    applyStyleToRange(ws, r, 0, r, 0, CELL_TEXT_CENTER, XLSX);
    applyStyleToRange(ws, r, 1, r, 1, CELL_CODE, XLSX);
    applyStyleToRange(ws, r, 2, r, 2, CELL_TEXT_LEFT, XLSX);
    applyStyleToRange(ws, r, 3, r, 3, CELL_TEXT_LEFT, XLSX);
    applyStyleToRange(ws, r, 4, r, 4, CELL_TEXT_CENTER, XLSX);
    applyStyleToRange(ws, r, 5, r, 5, CELL_EDITABLE, XLSX); // Status
    applyStyleToRange(ws, r, 6, r, 6, CELL_TEXT_LEFT, XLSX);
    applyStyleToRange(ws, r, 7, r, 7, CELL_NOTES, XLSX);
  }

  return { ws, statusCells };
}

/**
 * Main export function: creates 1 file with 2 sheets
 */
export async function exportUnifiedProgressReport({ reportData, agencyName, reportYear = 2026 }) {
  if (!reportData) {
    toast.warning('Không có dữ liệu báo cáo để xuất!');
    return;
  }

  const XLSX = await loadExcelWriter();
  const wb = XLSX.utils.book_new();

  const now = new Date();
  const dateStr = now.toLocaleDateString('vi-VN');
  const dateFileStr = now.toISOString().slice(0, 10);

  // 1. Build Sheet 1: Mục tiêu
  const goalsWs = buildGoalsWorksheet(reportData.goals || [], agencyName || reportData.agencyName || 'Cơ quan', dateStr, XLSX);
  XLSX.utils.book_append_sheet(wb, goalsWs, 'Mục tiêu');

  // 2. Build Sheet 2: Nhiệm vụ
  const { ws: tasksWs, statusCells } = buildTasksWorksheet(reportData.tasks || [], agencyName || reportData.agencyName || 'Cơ quan', dateStr, XLSX);
  XLSX.utils.book_append_sheet(wb, tasksWs, 'Nhiệm vụ');

  const safeAgencyCode = (reportData.agencyCode || reportData.agencyName || 'Don_Vi').replace(/\s+/g, '_').slice(0, 20);
  const fileName = `Bao_Cao_Tien_Do_${safeAgencyCode}_${dateFileStr}.xlsx`;

  // Download with dropdown validations on sheet 2
  downloadProgressWorkbook(wb, [[], statusCells], fileName, XLSX);
  toast.success(`Đã xuất báo cáo tiến độ 2 sheet cho ${agencyName}!`);
}

/**
 * Parser for unified Excel file containing Sheet 1 (Mục tiêu) & Sheet 2 (Nhiệm vụ)
 */
export async function parseUnifiedProgressReportFile(file, { existingGoals = [], existingTasks = [], reportYear = 2026 }) {
  const XLSX = await loadExcelReader(file.name);
  const data = await file.arrayBuffer();
  const workbook = XLSX.read(data, { type: 'array' });

  if (!workbook.SheetNames || workbook.SheetNames.length === 0) {
    throw new Error('File Excel không có sheet nào.');
  }

  const parsedGoals = [];
  const parsedPlans = [];
  const parsedTasks = [];
  const warnings = [];

  // 1. Parse Sheet 1 (Mục tiêu)
  const goalsSheetName = workbook.SheetNames.find(n => n.toLowerCase().includes('mục tiêu') || n.toLowerCase().includes('muctieu')) || workbook.SheetNames[0];
  const goalsWs = workbook.Sheets[goalsSheetName];
  if (goalsWs) {
    const rows = XLSX.utils.sheet_to_json(goalsWs, { header: 1 });
    let headerRowIdx = -1;
    for (let r = 0; r < Math.min(rows.length, 10); r++) {
      const row = rows[r] || [];
      if (row.some(c => String(c || '').toLowerCase().includes('mã mt') || String(c || '').toLowerCase().includes('chỉ tiêu'))) {
        headerRowIdx = r;
        break;
      }
    }

    if (headerRowIdx !== -1) {
      const headerRow = rows[headerRowIdx] || [];
      const subheaderRow = rows[headerRowIdx + 1] || [];

      // Helper to find column index by matchers
      const findCol = (predicate, fallback) => {
        let idx = subheaderRow.findIndex(predicate);
        if (idx !== -1) return idx;
        idx = headerRow.findIndex(predicate);
        return idx !== -1 ? idx : fallback;
      };

      const colMap = {
        code: findCol(c => String(c || '').toLowerCase().includes('mã'), 1),
        notes: findCol(c => String(c || '').toLowerCase().includes('ghi chú'), 12),
        est2026: findCol(c => String(c || '').includes('2026'), 7),
        plan2027: findCol(c => String(c || '').includes('2027'), 8),
        plan2028: findCol(c => String(c || '').includes('2028'), 9),
        plan2029: findCol(c => String(c || '').includes('2029'), 10),
        plan2030: findCol(c => String(c || '').includes('2030'), 11)
      };

      const startDataRow = headerRowIdx + (rows[headerRowIdx + 2] && rows[headerRowIdx + 2].some(c => String(c || '').includes('2027')) ? 3 : 2);

      for (let r = startDataRow; r < rows.length; r++) {
        const row = rows[r];
        if (!row || !row[colMap.code]) continue;
        const rawCode = String(row[colMap.code]).trim();
        if (!rawCode || rawCode.startsWith('I') || rawCode.startsWith('QĐ') || rawCode.toLowerCase().startsWith('mã')) continue;

        const rowCodes = rawCode.split(/[\r\n,\/]+/).map(c => c.trim()).filter(Boolean);

        const matchedGoal = existingGoals.find(g => {
          if (g.code && g.code.trim().toLowerCase() === rawCode.toLowerCase()) return true;
          if (g.subCodes && g.subCodes.some(sc => rowCodes.some(rc => rc.toLowerCase() === sc.toLowerCase()))) return true;
          const gCodes = (g.code || '').split(/[\r\n,\/]+/).map(c => c.trim().toLowerCase()).filter(Boolean);
          if (rowCodes.some(rc => gCodes.includes(rc.toLowerCase()))) return true;
          return false;
        });
        if (!matchedGoal) continue;

        const est2026Raw = row[colMap.est2026];
        const notesRaw = row[colMap.notes] !== undefined && row[colMap.notes] !== null ? String(row[colMap.notes]).trim() : '';

        // If progress value provided
        const numVal = parseVNNumber(est2026Raw);
        if (numVal != null) {
          const goalCode = matchedGoal.subCodes?.[0] || rowCodes[0] || rawCode;
          const goalId = matchedGoal.subGoalIds?.[0] || matchedGoal.id;
          parsedGoals.push({
            code: goalCode,
            id: goalId,
            documentId: matchedGoal.documentId,
            title: matchedGoal.title,
            value: numVal,
            summaryNotes: notesRaw || null,
            periodYear: reportYear
          });
        }

        // Check plans 2027-2030
        const checkPlan = (year, colIdx) => {
          const raw = row[colIdx];
          const num = parseVNNumber(raw);
          if (num != null) {
            let targetTaskId = matchedGoal.id;
            let targetCode = rawCode;
            if (matchedGoal.subGoalIds && matchedGoal.subGoalIds.length > 1) {
              if (year <= 2027) {
                targetTaskId = matchedGoal.subGoalIds[0];
                targetCode = matchedGoal.subCodes?.[0] || rawCode;
              } else {
                targetTaskId = matchedGoal.subGoalIds[1];
                targetCode = matchedGoal.subCodes?.[1] || rawCode;
              }
            }
            parsedPlans.push({
              taskId: targetTaskId,
              code: targetCode,
              title: matchedGoal.title,
              year,
              targetQuantity: num,
              documentId: matchedGoal.documentId
            });
          }
        };

        checkPlan(2027, colMap.plan2027);
        checkPlan(2028, colMap.plan2028);
        checkPlan(2029, colMap.plan2029);
        checkPlan(2030, colMap.plan2030);
      }
    }
  }

  // 2. Parse Sheet 2 (Nhiệm vụ)
  const tasksSheetName = workbook.SheetNames.find(n => n.toLowerCase().includes('nhiệm vụ') || n.toLowerCase().includes('nhiemvu')) || (workbook.SheetNames.length > 1 ? workbook.SheetNames[1] : null);
  if (tasksSheetName && workbook.Sheets[tasksSheetName]) {
    const tasksWs = workbook.Sheets[tasksSheetName];
    const rows = XLSX.utils.sheet_to_json(tasksWs, { header: 1 });
    let headerRowIdx = -1;
    for (let r = 0; r < Math.min(rows.length, 10); r++) {
      const row = rows[r] || [];
      if (row.some(c => String(c || '').toLowerCase().includes('mã nv'))) {
        headerRowIdx = r;
        break;
      }
    }

    if (headerRowIdx !== -1) {
      // Read by header so new exports (8 columns) and older templates both import safely.
      const headers = rows[headerRowIdx].map(value => String(value || '').trim().toLowerCase());
      const colMap = {
        code: headers.findIndex(h => h.includes('mã nv')),
        status: headers.findIndex(h => h.includes('trạng thái')),
        percent: headers.findIndex(h => h.includes('tiến độ') && h.includes('%')),
        notes: headers.findIndex(h => h.includes('ghi chú'))
      };

      for (let r = headerRowIdx + 1; r < rows.length; r++) {
        const row = rows[r];
        if (!row || !row[colMap.code]) continue;
        const code = String(row[colMap.code]).trim();
        if (!code || code.startsWith('I') || code.startsWith('QĐ') || code.toLowerCase().startsWith('mã')) continue;

        const matchedTask = existingTasks.find(t => t.code && t.code.trim().toLowerCase() === code.toLowerCase());
        if (!matchedTask) continue;

        const statusRaw = row[colMap.status] !== undefined && row[colMap.status] !== null ? String(row[colMap.status]).trim() : '';
        const percentRaw = row[colMap.percent];
        const notesRaw = row[colMap.notes] !== undefined && row[colMap.notes] !== null ? String(row[colMap.notes]).trim() : '';

        const sLower = statusRaw.toLowerCase();
        let mappedStatus = null;
        if (sLower.includes('hoàn thành') || statusRaw === 'Completed') mappedStatus = 'Completed';
        else if (sLower.includes('đang') || statusRaw === 'Drafting' || statusRaw === 'InProgress') mappedStatus = 'Drafting';
        else if (sLower.includes('chưa') || statusRaw === 'NotStarted') mappedStatus = 'NotStarted';

        const numPercent = parseVNNumber(percentRaw);

        if (mappedStatus || numPercent != null || notesRaw) {
          parsedTasks.push({
            code,
            id: matchedTask.id,
            documentId: matchedTask.documentId,
            title: matchedTask.title,
            status: mappedStatus || matchedTask.status,
            value: numPercent,
            summaryNotes: notesRaw || null,
            periodYear: reportYear
          });
        }
      }
    }
  }

  return {
    goals: parsedGoals,
    plans: parsedPlans,
    tasks: parsedTasks,
    warnings
  };
}
