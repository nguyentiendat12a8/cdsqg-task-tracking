import { reportStatusOptions } from '../shared/reportStatuses';

// The installed XLSX writer omits Excel validation. Add standard worksheet XML
// through its public CFB ZIP API, retaining every original workbook entry.
export function encodeProgressWorkbook(workbook, statusCellsBySheet, XLSX) {
  const bytes = new Uint8Array(XLSX.write(workbook, { type: 'array', bookType: 'xlsx' }));
  const zip = XLSX.CFB.read(bytes, { type: 'array' });
  const encoder = new TextEncoder(), decoder = new TextDecoder();
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
