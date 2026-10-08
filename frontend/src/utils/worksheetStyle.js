export function styleWorksheet(ws, { numCols = 8, headerRowIndex = 3, titleRowIndex = 0 } = {}, XLSX) {
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

