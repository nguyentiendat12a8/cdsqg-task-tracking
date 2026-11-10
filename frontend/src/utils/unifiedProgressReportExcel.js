import { loadExcelWriter, loadExcelReader } from './excelRuntime.js';
import { downloadProgressWorkbook } from './progressWorkbook.js';
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

const SUBHEADER_YELLOW_BRIGHT_STYLE = {
  font: { name: 'Segoe UI', sz: 9.5, bold: true, color: { rgb: '0F172A' } },
  fill: { fgColor: { rgb: 'FFFF00' } },
  alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
  border: THIN_BORDER
};

const SECTION_STYLE = {
  font: { name: 'Segoe UI', sz: 10.5, bold: true, color: { rgb: 'FFFFFF' } },
  fill: { fgColor: { rgb: '1E3A8A' } },
  alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
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

function formatExcelTarget(val) {
  if (val == null || val === '' || val === '—') return '';
  const s = String(val).trim();
  if (s.endsWith('.0')) return s.slice(0, -2);
  const n = Number(s);
  if (!isNaN(n) && n % 1 === 0) return Math.round(n);
  return s;
}

/**
 * Builds Sheet 1: Mục tiêu
 */
function buildGoalsWorksheet(goals, agencyName, dateStr, XLSX) {
  const rows = [];
  const merges = [];

  // Row 1: Title
  rows.push(['BIỂU MẪU BÁO CÁO MỤC TIÊU DO CƠ QUAN CHỦ TRÌ THEO DÕI']);
  merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: 12 } });

  // Row 2: Subtitle
  rows.push([`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`]);
  merges.push({ s: { r: 1, c: 0 }, e: { r: 1, c: 12 } });

  // Row 3: Căn cứ & Hướng dẫn
  rows.push([
    'CĂN CỨ: Kế hoạch mục tiêu được xây dựng căn cứ theo Kế hoạch chuyển đổi số 5 năm của cơ quan, đơn vị đã được cấp có thẩm quyền ban hành. Trường hợp chưa ban hành kế hoạch thì khẩn trương rà soát, xây dựng và ban hành theo đúng quy định.\nHƯỚNG DẪN: Điền vào ô màu vàng; không điền vào ô màu xám (—).'
  ]);
  merges.push({ s: { r: 2, c: 0 }, e: { r: 2, c: 12 } });

  // Row 4: Level 1 Header
  rows.push([
    'STT',
    'Mã MT',
    'Tên mục tiêu / Chỉ tiêu',
    'Đơn vị tính',
    'Kết quả theo QĐ',
    '',
    'Kết quả thực hiện',
    '',
    'Kế hoạch (dự kiến)',
    '',
    '',
    '',
    'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]'
  ]);

  // Row 5: Level 2 Header
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

  // Row 6: Level 3 Empty for merged headers
  rows.push(['', '', '', '', '', '', '', '', '', '', '', '', '']);

  // Header Merges
  merges.push({ s: { r: 3, c: 0 }, e: { r: 5, c: 0 } }); // STT (A4:A6)
  merges.push({ s: { r: 3, c: 1 }, e: { r: 5, c: 1 } }); // Mã MT (B4:B6)
  merges.push({ s: { r: 3, c: 2 }, e: { r: 5, c: 2 } }); // Tên MT (C4:C6)
  merges.push({ s: { r: 3, c: 3 }, e: { r: 5, c: 3 } }); // Đơn vị tính (D4:D6)
  merges.push({ s: { r: 3, c: 4 }, e: { r: 3, c: 5 } }); // Kết quả theo QĐ (E4:F4)
  merges.push({ s: { r: 4, c: 4 }, e: { r: 5, c: 4 } }); // Giai đoạn 1 (E5:E6)
  merges.push({ s: { r: 4, c: 5 }, e: { r: 5, c: 5 } }); // Giai đoạn 2 (F5:F6)
  merges.push({ s: { r: 3, c: 6 }, e: { r: 3, c: 7 } }); // Kết quả thực hiện (G4:H4)
  merges.push({ s: { r: 4, c: 6 }, e: { r: 5, c: 6 } }); // Kết quả 2025 (G5:G6)
  merges.push({ s: { r: 4, c: 7 }, e: { r: 5, c: 7 } }); // Ước tính 2026 (H5:H6)
  merges.push({ s: { r: 3, c: 8 }, e: { r: 3, c: 11 } }); // Kế hoạch dự kiến (I4:L4)
  merges.push({ s: { r: 4, c: 8 }, e: { r: 5, c: 8 } }); // 2027 (I5:I6)
  merges.push({ s: { r: 4, c: 9 }, e: { r: 5, c: 9 } }); // 2028 (J5:J6)
  merges.push({ s: { r: 4, c: 10 }, e: { r: 5, c: 10 } }); // 2029 (K5:K6)
  merges.push({ s: { r: 4, c: 11 }, e: { r: 5, c: 11 } }); // 2030 (L5:L6)
  merges.push({ s: { r: 3, c: 12 }, e: { r: 5, c: 12 } }); // Ghi chú (M4:M6)

  const goals2629 = (goals || []).filter(g => String(g.documentNumber).includes('2629'));
  const goals1266 = (goals || []).filter(g => String(g.documentNumber).includes('1266'));

  const sectionRows = [];
  const editableCellCoords = [];
  const disabledCellCoords = [];

  // Helper to add goals of a section
  function appendGoalsSection(sectionNumberRoman, docLabel, sectionGoals, colEHeader, colFHeader) {
    const secRowIdx = rows.length;
    sectionRows.push(secRowIdx);
    const secRow = [sectionNumberRoman, docLabel, '', '', colEHeader, colFHeader, '', '', '', '', '', '', ''];
    rows.push(secRow);
    merges.push({ s: { r: secRowIdx, c: 1 }, e: { r: secRowIdx, c: 3 } });

    sectionGoals.forEach((g, idx) => {
      const rIdx = rows.length;
      const appYears = g.applicableYears || [];

      const valAct2025 = g.actual2025 != null ? g.actual2025 : (appYears.includes(2025) ? '' : '—');
      const valEst2026 = g.estimate2026 != null ? g.estimate2026 : (appYears.includes(2026) ? '' : '—');
      const valPlan2027 = g.plan2027 != null && g.plan2027 !== '' ? g.plan2027 : (appYears.includes(2027) ? '' : '—');
      const valPlan2028 = g.plan2028 != null && g.plan2028 !== '' ? g.plan2028 : (appYears.includes(2028) ? '' : '—');
      const valPlan2029 = g.plan2029 != null && g.plan2029 !== '' ? g.plan2029 : (appYears.includes(2029) ? '' : '—');
      const valPlan2030 = g.plan2030 != null && g.plan2030 !== '' ? g.plan2030 : (appYears.includes(2030) ? '' : '—');

      const row = [
        idx + 1,
        g.code || '',
        g.title || '',
        g.unitName || '%',
        formatExcelTarget(g.targetPhase1),
        formatExcelTarget(g.targetPhase2),
        valAct2025,
        valEst2026,
        valPlan2027,
        valPlan2028,
        valPlan2029,
        valPlan2030,
        g.notes || ''
      ];
      rows.push(row);

      // Mark editable vs disabled cells
      const checkCell = (val, col) => {
        if (val === '—') disabledCellCoords.push({ r: rIdx, c: col });
        else editableCellCoords.push({ r: rIdx, c: col });
      };

      checkCell(valEst2026, 7);
      checkCell(valPlan2027, 8);
      checkCell(valPlan2028, 9);
      checkCell(valPlan2029, 10);
      checkCell(valPlan2030, 11);
    });
  }

  // Section I: 2629
  appendGoalsSection('I', 'QĐ 2629/QĐ-TTG', goals2629, '2025 - 2027', '2028 - 2030');

  // Section II: 1266
  appendGoalsSection('II', 'QĐ 1266/QĐ-TTG', goals1266, '2026 - 2030', '2031 - 2045');

  const ws = XLSX.utils.aoa_to_sheet(rows);
  ws['!merges'] = merges;

  // Column Widths matching reference
  ws['!cols'] = [
    { wch: 6 },  // STT (A)
    { wch: 10 }, // Mã MT (B)
    { wch: 46 }, // Tên mục tiêu (C)
    { wch: 12 }, // ĐVT (D)
    { wch: 14 }, // Kết quả 1 (E)
    { wch: 14 }, // Kết quả 2 (F)
    { wch: 15 }, // Thực hiện 2025 (G)
    { wch: 16 }, // Ước tính 2026 (H)
    { wch: 16 }, // KH 2027 (I)
    { wch: 16 }, // KH 2028 (J)
    { wch: 16 }, // KH 2029 (K)
    { wch: 16 }, // KH 2030 (L)
    { wch: 45 }  // Ghi chú (M)
  ];

  // Set Row Heights
  ws['!rows'] = [
    { hpt: 28 }, // Title
    { hpt: 22 }, // Subtitle
    { hpt: 48 }, // Guide
    { hpt: 24 }, // Hdr 1
    { hpt: 24 }, // Hdr 2
    { hpt: 10 }  // Hdr 3
  ];

  // Apply Styles
  applyStyleToRange(ws, 0, 0, 0, 12, TITLE_STYLE, XLSX);
  applyStyleToRange(ws, 1, 0, 1, 12, SUBTITLE_STYLE, XLSX);
  applyStyleToRange(ws, 2, 0, 2, 12, GUIDE_STYLE, XLSX);

  // Headers styling
  applyStyleToRange(ws, 3, 0, 5, 5, HEADER_STYLE, XLSX);
  applyStyleToRange(ws, 3, 6, 3, 7, HEADER_STYLE, XLSX);
  applyStyleToRange(ws, 4, 6, 5, 6, SUBHEADER_YELLOW_BRIGHT_STYLE, XLSX);
  applyStyleToRange(ws, 4, 7, 5, 7, SUBHEADER_YELLOW_STYLE, XLSX);
  applyStyleToRange(ws, 3, 8, 3, 11, HEADER_STYLE, XLSX);
  applyStyleToRange(ws, 4, 8, 5, 11, SUBHEADER_YELLOW_STYLE, XLSX);
  applyStyleToRange(ws, 3, 12, 5, 12, HEADER_STYLE, XLSX);

  // Section styling
  sectionRows.forEach(r => {
    applyStyleToRange(ws, r, 0, r, 12, SECTION_STYLE, XLSX);
  });

  // Base Data Rows styling
  const headerCount = 6;
  for (let r = headerCount; r < rows.length; r++) {
    if (sectionRows.includes(r)) continue;
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

  return ws;
}

/**
 * Builds Sheet 2: Nhiệm vụ
 */
function buildTasksWorksheet(tasks, agencyName, dateStr, XLSX) {
  const rows = [];
  const merges = [];
  const statusCells = [];

  // Title
  rows.push(['BIỂU MẪU BÁO CÁO NHIỆM VỤ DO CƠ QUAN CHỦ TRÌ THEO DÕI']);
  merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: 8 } });

  // Subtitle
  rows.push([`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`]);
  merges.push({ s: { r: 1, c: 0 }, e: { r: 1, c: 8 } });

  // Guide
  rows.push([
    'HƯỚNG DẪN: Chọn Trạng thái thực hiện tại cột G (màu vàng); Ghi chú khó khăn vướng mắc tại cột I.'
  ]);
  merges.push({ s: { r: 2, c: 0 }, e: { r: 2, c: 8 } });

  // Headers
  const headers = [
    'STT',
    'Căn cứ QĐ',
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
    const secRow = [sectionNumberRoman, docLabel, '', '', '', '', '', '', '', '', ''];
    rows.push(secRow);
    merges.push({ s: { r: secRowIdx, c: 1 }, e: { r: secRowIdx, c: 8 } });

    sectionTasks.forEach((t, idx) => {
      const rIdx = rows.length;
      const row = [
        idx + 1,
        t.documentNumber || '',
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

      // Track cell for Excel data validation dropdown
      statusCells.push(`G${rIdx + 1}`);
    });
  }

  appendTasksSection('I', 'QĐ 2629/QĐ-TTG - CÁC NHIỆM VỤ CHỦ TRÌ', tasks2629);
  appendTasksSection('II', 'QĐ 1266/QĐ-TTG - CÁC NHIỆM VỤ CHỦ TRÌ', tasks1266);

  const ws = XLSX.utils.aoa_to_sheet(rows);
  ws['!merges'] = merges;

  ws['!cols'] = [
    { wch: 6 },  // STT (A)
    { wch: 15 }, // Căn cứ QĐ (B)
    { wch: 12 }, // Mã NV (C)
    { wch: 45 }, // Tên NV (D)
    { wch: 25 }, // Phối hợp (E)
    { wch: 14 }, // Hạn (F)
    { wch: 28 }, // Trạng thái (G) - Editable
    { wch: 30 }, // Sản phẩm (H)
    { wch: 40 }, // Ghi chú (I) - Editable
    { wch: 36, hidden: true }, // ID NV (J)
    { wch: 36, hidden: true }  // ID QĐ (K)
  ];

  ws['!rows'] = [
    { hpt: 28 }, // Title
    { hpt: 22 }, // Subtitle
    { hpt: 32 }, // Guide
    { hpt: 26 }  // Header
  ];

  applyStyleToRange(ws, 0, 0, 0, 8, TITLE_STYLE, XLSX);
  applyStyleToRange(ws, 1, 0, 1, 8, SUBTITLE_STYLE, XLSX);
  applyStyleToRange(ws, 2, 0, 2, 8, GUIDE_STYLE, XLSX);

  // Headers
  applyStyleToRange(ws, 3, 0, 3, 5, HEADER_STYLE, XLSX);
  applyStyleToRange(ws, 3, 6, 3, 6, SUBHEADER_YELLOW_STYLE, XLSX);
  applyStyleToRange(ws, 3, 7, 3, 8, HEADER_STYLE, XLSX);

  // Section Rows
  sectionRows.forEach(r => {
    applyStyleToRange(ws, r, 0, r, 8, SECTION_STYLE, XLSX);
  });

  // Data rows
  for (let r = 4; r < rows.length; r++) {
    if (sectionRows.includes(r)) continue;
    applyStyleToRange(ws, r, 0, r, 0, CELL_TEXT_CENTER, XLSX);
    applyStyleToRange(ws, r, 1, r, 1, CELL_TEXT_CENTER, XLSX);
    applyStyleToRange(ws, r, 2, r, 2, CELL_CODE, XLSX);
    applyStyleToRange(ws, r, 3, r, 3, CELL_TEXT_LEFT, XLSX);
    applyStyleToRange(ws, r, 4, r, 4, CELL_TEXT_LEFT, XLSX);
    applyStyleToRange(ws, r, 5, r, 5, CELL_TEXT_CENTER, XLSX);
    applyStyleToRange(ws, r, 6, r, 6, CELL_EDITABLE, XLSX); // Status
    applyStyleToRange(ws, r, 7, r, 7, CELL_TEXT_LEFT, XLSX);
    applyStyleToRange(ws, r, 8, r, 8, CELL_NOTES, XLSX);
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
      // Find columns
      const colMap = { code: 1, act2025: 6, est2026: 7, plan2027: 8, plan2028: 9, plan2029: 10, plan2030: 11, notes: 12 };

      for (let r = headerRowIdx + 1; r < rows.length; r++) {
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

        const est2026Raw = row[colMap.est2026] !== undefined && row[colMap.est2026] !== null ? String(row[colMap.est2026]).trim() : '';
        const notesRaw = row[colMap.notes] !== undefined && row[colMap.notes] !== null ? String(row[colMap.notes]).trim() : '';

        // If progress value provided
        if (est2026Raw !== '' && est2026Raw !== '—') {
          const numVal = parseFloat(est2026Raw.replace(/%/g, '').replace(/,/g, '.'));
          if (!isNaN(numVal)) {
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
        }

        // Check plans 2027-2030
        const checkPlan = (year, colIdx) => {
          const raw = row[colIdx] !== undefined && row[colIdx] !== null ? String(row[colIdx]).trim() : '';
          if (raw !== '' && raw !== '—') {
            const num = parseFloat(raw.replace(/%/g, '').replace(/,/g, '.'));
            if (!isNaN(num)) {
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
      // Read by header so new exports and older percentage templates both import safely.
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
        const percentRaw = row[colMap.percent] !== undefined && row[colMap.percent] !== null ? String(row[colMap.percent]).trim() : '';
        const notesRaw = row[colMap.notes] !== undefined && row[colMap.notes] !== null ? String(row[colMap.notes]).trim() : '';

        const sLower = statusRaw.toLowerCase();
        let mappedStatus = null;
        if (sLower.includes('hoàn thành') || statusRaw === 'Completed') mappedStatus = 'Completed';
        else if (sLower.includes('đang') || statusRaw === 'Drafting' || statusRaw === 'InProgress') mappedStatus = 'Drafting';
        else if (sLower.includes('chưa') || statusRaw === 'NotStarted') mappedStatus = 'NotStarted';

        let numPercent = null;
        if (percentRaw !== '' && percentRaw !== '—') {
          const parsed = parseFloat(percentRaw.replace(/%/g, '').replace(/,/g, '.'));
          if (!isNaN(parsed)) numPercent = parsed;
        }

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
