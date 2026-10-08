// Opening a view never downloads Excel. Promises are cached by the module loader.
export async function loadExcelWriter() {
  return (await import('xlsx-js-style/dist/xlsx.bundle.js')).default;
}

export async function loadExcelReader(fileName = '') {
  if (/\.xlsx$/i.test(fileName)) return loadExcelWriter();
  // Keep the full codepage tables for .xls and older Vietnamese workbooks.
  return (await import('xlsx-js-style')).default;
}
