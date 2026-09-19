import * as XLSX from 'xlsx';
import { toast } from 'vue3-toastify';

/**
 * Cleanly export data to formatted Excel (.xlsx) with auto-fit column widths & report header.
 * 
 * @param {Object} options
 * @param {string} options.title - Main report title
 * @param {string} [options.subtitle] - Optional subtitle (defaults to timestamp)
 * @param {Array<string>} options.headers - Column header titles
 * @param {Array<Array<any>>} options.rows - Data rows
 * @param {string} options.fileName - File name prefix (without extension)
 * @param {string} [options.sheetName="Báo cáo"] - Excel sheet tab title
 * @param {Object} [options.minColWidths={}] - Map of column index to minimum character width e.g. { 1: 30, 2: 50 }
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
  if (!rows || rows.length === 0) {
    toast.warning("Không có dữ liệu để xuất file Excel!");
    return;
  }

  const now = new Date();
  const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}`;

  const data = [];
  
  if (title) {
    data.push([title.toUpperCase()]);
  }
  data.push([subtitle || `Thời gian xuất báo cáo: ${timeStr}`]);
  data.push([]); // Empty row separator
  data.push(headers);

  const headerRowIdx = data.length - 1;

  rows.forEach(row => {
    data.push(row);
  });

  const ws = XLSX.utils.aoa_to_sheet(data);

  // Configure merged rows across all data columns for Title & Subtitle
  const numCols = headers ? headers.length : 1;
  const merges = [];

  if (numCols > 1) {
    if (title) {
      merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: numCols - 1 } }); // Title merged A1:X1
      merges.push({ s: { r: 1, c: 0 }, e: { r: 1, c: numCols - 1 } }); // Subtitle merged A2:X2
    } else {
      merges.push({ s: { r: 0, c: 0 }, e: { r: 0, c: numCols - 1 } }); // Subtitle merged A1:X1
    }
    ws['!merges'] = merges;
  }

  // Auto calculate fit column widths (!cols)
  const colWidths = headers.map((hdr, colIdx) => {
    let maxLen = hdr ? hdr.toString().length : 10;
    
    for (let r = headerRowIdx + 1; r < data.length; r++) {
      const cellVal = data[r][colIdx] !== undefined && data[r][colIdx] !== null ? data[r][colIdx].toString() : '';
      if (cellVal.length > maxLen) {
        maxLen = cellVal.length;
      }
    }

    // Standard width calculation with padding + min/max bounds
    let calculatedWidth = Math.min(Math.max(maxLen + 4, 12), 75);
    
    if (minColWidths[colIdx]) {
      calculatedWidth = Math.max(calculatedWidth, minColWidths[colIdx]);
    }
    
    return calculatedWidth;
  });

  ws['!cols'] = colWidths.map(w => ({ wch: w }));

  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, sheetName);

  const dateFileStr = now.toISOString().slice(0, 10);
  const fullFileName = `${fileName}_${dateFileStr}.xlsx`;
  
  XLSX.writeFile(wb, fullFileName);
  toast.success("Đã xuất file Excel chuẩn định dạng thành công!");
}
