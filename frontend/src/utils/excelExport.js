// Small public facade; the Excel engine loads only when an export is requested.
export { styleWorksheet } from './worksheetStyle';

export async function exportFormattedReportExcel(options) {
  const reports = await import('./excelReports');
  return reports.exportFormattedReportExcel(options);
}

export async function exportToExcel(options) {
  const reports = await import('./excelReports');
  return reports.exportToExcel(options);
}

export async function exportProgressReportTemplate(options) {
  const reports = await import('./excelReports');
  return reports.exportProgressReportTemplate(options);
}
