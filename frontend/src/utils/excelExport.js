import XLSX from 'xlsx-js-style';
import { toast } from 'vue3-toastify';

/**
 * Apply cell borders, colors, fonts, alignment, and row heights to a worksheet.
 */
export function styleWorksheet(ws, { numCols = 8, headerRowIndex = 3, titleRowIndex = 0 } = {}) {
  if (!ws || !ws['!ref']) return;
  const range = XLSX.utils.decode_range(ws['!ref']);
  const thinBorder = {
    top: { style: 'thin', color: { rgb: 'CBD5E1' } },
    bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
    left: { style: 'thin', color: { rgb: 'CBD5E1' } },
    right: { style: 'thin', color: { rgb: 'CBD5E1' } }
  };

  const headerStyle = {
    font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: 'FFFFFF' } },
    fill: { fgColor: { rgb: '1E3A8A' } }, // Navy Blue
    alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
    border: {
      top: { style: 'medium', color: { rgb: '0F172A' } },
      bottom: { style: 'medium', color: { rgb: '0F172A' } },
      left: { style: 'thin', color: { rgb: '334155' } },
      right: { style: 'thin', color: { rgb: '334155' } }
    }
  };

  const sectionStyle = {
    font: { name: 'Segoe UI', sz: 11, bold: true, color: { rgb: '0F172A' } },
    fill: { fgColor: { rgb: 'E2E8F0' } }, // Slate 200
    alignment: { horizontal: 'left', vertical: 'center' },
    border: {
      top: { style: 'thin', color: { rgb: '94A3B8' } },
      bottom: { style: 'thin', color: { rgb: '94A3B8' } },
      left: thinBorder.left,
      right: thinBorder.right
    }
  };

  const titleStyle = {
    font: { name: 'Segoe UI', sz: 13, bold: true, color: { rgb: 'FFFFFF' } },
    fill: { fgColor: { rgb: '1E3A8A' } },
    alignment: { horizontal: 'center', vertical: 'center' },
    border: thinBorder
  };

  const subtitleStyle = {
    font: { name: 'Segoe UI', sz: 10, italic: true, color: { rgb: '475569' } },
    fill: { fgColor: { rgb: 'F1F5F9' } },
    alignment: { horizontal: 'center', vertical: 'center' }
  };

  const kpiLabelStyle = {
    font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '1E293B' } },
    fill: { fgColor: { rgb: 'F8FAFC' } },
    alignment: { horizontal: 'left', vertical: 'center' },
    border: thinBorder
  };

  const kpiValueStyle = {
    font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '1E3A8A' } },
    fill: { fgColor: { rgb: 'FFFFFF' } },
    alignment: { horizontal: 'left', vertical: 'center' },
    border: thinBorder
  };

  for (let r = range.s.r; r <= range.e.r; r++) {
    const firstColVal = ws[XLSX.utils.encode_cell({ r, c: 0 })]?.v;
    const isHeaderRow = r === headerRowIndex || 
                        firstColVal === 'STT' || 
                        firstColVal === 'Mã Hạng Mục' || 
                        firstColVal === 'Phạm Vi Nhiệm Vụ' ||
                        firstColVal === 'Tổng số hạng mục' || 
                        firstColVal === 'Loại hình';

    for (let c = range.s.c; c <= range.e.c; c++) {
      const cellRef = XLSX.utils.encode_cell({ r, c });
      let cell = ws[cellRef];
      if (!cell) {
        cell = { t: 's', v: '' };
        ws[cellRef] = cell;
      }

      const valStr = cell.v !== undefined && cell.v !== null ? String(cell.v).trim() : '';

      // 1. Main Title Row
      if (r === titleRowIndex) {
        cell.s = titleStyle;
      }
      // 2. Subtitle Row
      else if (r === titleRowIndex + 1) {
        cell.s = subtitleStyle;
      }
      // 3. Section Title Rows
      else if (/^(\d+\.|CHỈ SỐ|DANH SÁCH|BẢNG TỔNG HỢP|THỐNG KÊ)/i.test(valStr)) {
        cell.s = sectionStyle;
      }
      // 4. Column Header Rows
      else if (isHeaderRow) {
        cell.s = headerStyle;
      }
      // 5. KPI Overview rows (between title and header row when not section title)
      else if (r > 2 && r < headerRowIndex && valStr !== '' && !/^(\d+\.|CHỈ SỐ|DANH SÁCH)/i.test(firstColVal)) {
        cell.s = c === 0 ? kpiLabelStyle : (c === 1 ? kpiValueStyle : { font: { name: 'Segoe UI', sz: 10 }, border: thinBorder });
      }
      // 6. Data Rows
      else if (valStr !== '') {
        const isEven = r % 2 === 0;
        const isNumber = typeof cell.v === 'number';
        cell.s = {
          font: { name: 'Segoe UI', sz: 10, color: { rgb: '0F172A' } },
          fill: { fgColor: { rgb: isEven ? 'F8FAFC' : 'FFFFFF' } },
          alignment: {
            horizontal: c === 0 ? 'center' : (isNumber ? 'right' : 'left'),
            vertical: 'center',
            wrapText: true
          },
          border: thinBorder
        };
      } else {
        const isEven = r % 2 === 0;
        cell.s = {
          font: { name: 'Segoe UI', sz: 10 },
          fill: { fgColor: { rgb: isEven ? 'F8FAFC' : 'FFFFFF' } },
          border: thinBorder
        };
      }
    }
  }
}

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
  if (title) data.push([title.toUpperCase()]);
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

  styleWorksheet(ws, { numCols, headerRowIndex: headerRowIdx, titleRowIndex: 0 });

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
export function exportProgressReportTemplate({
  items = [],
  currentAgency = null,
  userRole = '',
  fileName = 'Bao_Cao_Tien_Do_Thuc_Hien'
}) {
  const now = new Date();
  const day = String(now.getDate()).padStart(2, '0');
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const year = now.getFullYear();
  const dateStr = `${day}/${month}/${year}`;

  const agencyName = currentAgency?.name || 'Bộ, Ngành, Địa phương';

  const rows = [];
  // Title row
  rows.push(["BÁO CÁO TIẾN ĐỘ THỰC HIỆN MỤC TIÊU & NHIỆM VỤ"]);
  // Subtitle row
  rows.push([`Đơn vị báo cáo: ${agencyName} | Ngày xuất file: ${dateStr}`]);
  // Instruction row
  rows.push([`HƯỚNG DẪN: Điền con số thực tế (nếu % hoặc Số lượng) hoặc chọn/nhập trạng thái văn bản vào cột [I] "TIẾN ĐỘ THỰC HIỆN MỚI", điền ghi chú/trích yếu vào cột [J]. Sau khi điền xong, chọn nút "Import Báo Cáo Tiến Độ" trên phần mềm để tải file này lên.`]);

  // Headers
  const headers = [
    "STT",
    "Mã",
    "Tên Mục Tiêu / Nhiệm Vụ",
    "Loại Hạng Mục",
    "Cơ Quan Chủ Trì",
    "Giao Đơn Vị Trực Thuộc",
    "Đơn Vị Tính & Loại Đánh Giá",
    "Chỉ Tiêu Kế Hoạch",
    "Tiến Độ Hiện Tại",
    "TIẾN ĐỘ THỰC HIỆN MỚI [CẦN ĐIỀN]",
    "GHI CHÚ / TRÍCH YẾU [CẦN ĐIỀN]",
    "Quyền Cập Nhật"
  ];
  rows.push(headers);

  const roleStr = userRole !== null && userRole !== undefined ? String(userRole).toLowerCase() : '';
  const isAdmin = roleStr === 'admin' || roleStr === '1';
  const currAgId = currentAgency?.id ? String(currentAgency.id).toLowerCase() : null;
  const isLevel2 = currentAgency && !currentAgency.parentId;
  const isLevel3 = currentAgency && currentAgency.parentId;

  items.forEach((item, index) => {
    const code = item.code || '';
    const title = item.title || '';
    const itemType = item.itemType === 'Goal' ? 'Mục tiêu' : 'Nhiệm vụ';
    const leadAgName = item.leadAgencyName || item.leadAgency?.name || '—';
    const assignedAgName = item.assignedAgencyName || '—';
    const unitName = item.unitName || item.unit?.name || '%';
    const evalType = item.evaluationType === 'Quantitative' ? 'Định lượng' : 'Văn bản';
    const unitDisplay = `${unitName} (${evalType})`;

    const targetVal = item.targetQuantity !== null && item.targetQuantity !== undefined ? item.targetQuantity : '—';
    
    // Progress display
    let curProgressStr = 'Chưa cập nhật';
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      curProgressStr = `${item.latestProgressValue} ${unitName}`;
    } else if (item.latestQualitativeStatus) {
      curProgressStr = item.latestQualitativeStatus;
    }

    // Permission check for user
    let permStr = 'Không có quyền';
    const leadAgId = item.leadAgencyId ? String(item.leadAgencyId).toLowerCase() : null;
    const assignedAgId = item.assignedAgencyId ? String(item.assignedAgencyId).toLowerCase() : null;
    const isGeneral = item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES';

    if (isAdmin) {
      permStr = 'Có quyền (Duyệt ngay)';
    } else if (isLevel2) {
      if (leadAgId === currAgId || isGeneral || (item.leadAgency?.parentId && String(item.leadAgency.parentId).toLowerCase() === currAgId)) {
        permStr = 'Có quyền (Duyệt ngay)';
      } else if (assignedAgId && currentAgency?.children?.some(c => String(c.id).toLowerCase() === assignedAgId)) {
        permStr = 'Có quyền (Duyệt ngay)';
      }
    } else if (isLevel3) {
      if (assignedAgId === currAgId || leadAgId === currAgId) {
        permStr = 'Có quyền (Chờ Cấp 2 duyệt)';
      }
    }

    // Suggested new value column
    let fillValue = '';
    if (item.evaluationType === 'Quantitative') {
      fillValue = item.latestProgressValue !== null && item.latestProgressValue !== undefined ? item.latestProgressValue : '';
    } else {
      fillValue = item.latestQualitativeStatus || 'Đang soạn thảo';
    }

    rows.push([
      index + 1,
      code,
      title,
      itemType,
      leadAgName,
      assignedAgName,
      unitDisplay,
      targetVal,
      curProgressStr,
      fillValue,
      '',
      permStr
    ]);
  });

  const ws = XLSX.utils.aoa_to_sheet(rows);

  // Apply styling
  styleWorksheet(ws, { numCols: 12, headerRowIndex: 3, titleRowIndex: 0 });

  // Custom column widths
  ws['!cols'] = [
    { wch: 6 },  // STT
    { wch: 14 }, // Mã
    { wch: 45 }, // Tên
    { wch: 12 }, // Loại
    { wch: 28 }, // Chủ trì
    { wch: 28 }, // Giao đơn vị trực thuộc
    { wch: 22 }, // Đơn vị tính
    { wch: 16 }, // Mục tiêu
    { wch: 18 }, // Tiến độ hiện tại
    { wch: 32 }, // Tiến độ mới [CẦN ĐIỀN]
    { wch: 35 }, // Ghi chú [CẦN ĐIỀN]
    { wch: 24 }  // Quyền
  ];

  // Highlight column I and J headers (indices 9 and 10)
  const headerRowIdx = 3;
  const colI_Ref = XLSX.utils.encode_cell({ r: headerRowIdx, c: 9 });
  const colJ_Ref = XLSX.utils.encode_cell({ r: headerRowIdx, c: 10 });

  if (ws[colI_Ref]) {
    ws[colI_Ref].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'FEF08A' } }, // Yellow Highlight
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: ws[colI_Ref].s?.border
    };
  }

  if (ws[colJ_Ref]) {
    ws[colJ_Ref].s = {
      font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: '0F172A' } },
      fill: { fgColor: { rgb: 'BAE6FD' } }, // Light Blue Highlight
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: ws[colJ_Ref].s?.border
    };
  }

  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, "Báo Cáo Tiến Độ");

  const dateFileStr = now.toISOString().slice(0, 10);
  const fullFileName = `${fileName}_${dateFileStr}.xlsx`;
  XLSX.writeFile(wb, fullFileName);
  toast.success("Đã xuất file Excel Báo Cáo Tiến Độ thành công!");
}
