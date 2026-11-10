import { reportStatusOptions } from '../shared/reportStatuses.js';

// Opt-in report protection: the writer does not serialize per-cell unlocked styles.
// Clone serialized styles so editable report cells remain editable in protected Excel sheets.
function protectReportSheets(workbook, zip, XLSX, encoder, decoder) {
  const targets = workbook.SheetNames.map((name, index) => ({ index, cells: workbook.Sheets[name]['!reportEditableCells'] })).filter(item => Array.isArray(item.cells));
  if (!targets.length) return;
  const stylesPath = zip.FullPaths.find(path => path.endsWith('/xl/styles.xml'));
  if (!stylesPath) throw new Error('Không tìm thấy styles.xml để khóa ô báo cáo.');
  let styles = decoder.decode(XLSX.CFB.find(zip, stylesPath).content);
  const block = /<cellXfs\b[^>]*>([\s\S]*?)<\/cellXfs>/.exec(styles);
  const xfs = block?.[1].match(/<xf\b[^>]*?\/>|<xf\b[^>]*>[\s\S]*?<\/xf>/g);
  if (!xfs) throw new Error('Không đọc được định dạng ô báo cáo.');
  const unlockedStyles = new Map();
  for (const {index,cells} of targets) {
    const editable = new Set(cells);
    const path = zip.FullPaths.find(path => path.endsWith(`/xl/worksheets/sheet${index+1}.xml`));
    if (!path) throw new Error('Không tìm thấy sheet báo cáo để khóa ô.');
    let xml = decoder.decode(XLSX.CFB.find(zip, path).content);
    const seen = new Set();
    xml = xml.replace(/<c\b[^>]*>/g, tag => {
      const ref = /\br="([^"]+)"/.exec(tag)?.[1];
      if (!editable.has(ref)) return tag;
      seen.add(ref);
      const base = Number(/\bs="(\d+)"/.exec(tag)?.[1] || 0);
      if (!unlockedStyles.has(base)) {
        let xf = xfs[base];
        if (!xf) throw new Error('Định dạng ô nhập báo cáo không hợp lệ.');
        xf = xf.replace(/\sapplyProtection="[^"]*"/g, '').replace(/<protection\b[^>]*\/>/g, '');
        if (xf.endsWith('/>')) xf = xf.slice(0,-2) + ' applyProtection="1"><protection locked="0"/></xf>';
        else xf = xf.replace(/^<xf\b/, '<xf applyProtection="1"').replace('</xf>', '<protection locked="0"/></xf>');
        unlockedStyles.set(base,xfs.length); xfs.push(xf);
      }
      return tag.replace(/\ss="\d+"/g,'').replace(/\/?>(?=$)/, end=>` s="${unlockedStyles.get(base)}"${end}`);
    });
    if (seen.size !== editable.size) throw new Error('Có ô nhập chưa được ghi vào sheet; không xuất file bị khóa sai.');
    xml = xml.replace(/<sheetProtection\b[^>]*\/>/g,'');
    xml = xml.replace('</sheetData>', '</sheetData><sheetProtection password="FABF" sheet="1" objects="1" scenarios="1" selectLockedCells="0" selectUnlockedCells="0"/>');
    XLSX.CFB.utils.cfb_add(zip,path,encoder.encode(xml));
  }
  styles = styles.replace(block[0], `<cellXfs count="${xfs.length}">${xfs.join('')}</cellXfs>`);
  XLSX.CFB.utils.cfb_add(zip,stylesPath,encoder.encode(styles));
}

// The installed XLSX writer omits Excel validation. Add standard worksheet XML
// through its public CFB ZIP API, retaining every original workbook entry.
export function encodeProgressWorkbook(workbook, statusCellsBySheet, XLSX) {
  const bytes = new Uint8Array(XLSX.write(workbook, { type: 'array', bookType: 'xlsx' }));
  const zip = XLSX.CFB.read(bytes, { type: 'array' });
  const encoder = new TextEncoder(), decoder = new TextDecoder();
  protectReportSheets(workbook, zip, XLSX, encoder, decoder);
  statusCellsBySheet.forEach((cells, index) => {
    if (!cells.length) return;
    const path = zip.FullPaths.find(path => path.endsWith(`/xl/worksheets/sheet${index + 1}.xml`));
    if (!path) throw new Error('Không tìm thấy sheet để tạo danh sách trạng thái.');
    const xml = decoder.decode(XLSX.CFB.find(zip, path).content);
    const formula = `&quot;${reportStatusOptions.map(option => option.label).join(',')}&quot;`;
    const validations = cells.map(cell => `<dataValidation type="list" errorStyle="stop" allowBlank="1" showInputMessage="1" showErrorMessage="1" showDropDown="0" sqref="${cell}" errorTitle="Trạng thái không hợp lệ" error="Chọn một trong ba trạng thái trong danh sách."><formula1>${formula}</formula1></dataValidation>`).join('');
    // Schema order: validations follow merged cells and precede page options.
    const block = `<dataValidations count="${cells.length}">${validations}</dataValidations>`;
    const insertion = /<(?:hyperlinks|printOptions|pageMargins|pageSetup|headerFooter|rowBreaks|colBreaks|customProperties|cellWatches|ignoredErrors|smartTags|drawing|legacyDrawing|legacyDrawingHF|picture|oleObjects|controls|webPublishItems|tableParts|extLst)\b/.exec(xml);
    const offset = insertion?.index ?? xml.indexOf('</worksheet>');
    XLSX.CFB.utils.cfb_add(zip, path, encoder.encode(xml.slice(0, offset) + block + xml.slice(offset)));
  });
  return new Uint8Array(XLSX.CFB.write(zip, { type: 'array', fileType: 'zip' }));
}
export function downloadProgressWorkbook(workbook, statusCellsBySheet, fileName, XLSX) {
  const bytes = encodeProgressWorkbook(workbook, statusCellsBySheet, XLSX);
  const url = URL.createObjectURL(new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }));
  const link = document.createElement('a'); link.href = url; link.download = fileName;
  document.body.append(link); link.click(); link.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
