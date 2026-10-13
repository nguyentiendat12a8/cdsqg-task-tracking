// Explicit defaults for the current decision; deployment can select another document.
export const REPORTING_DOCUMENT_ID = import.meta.env.VITE_REPORTING_DOCUMENT_ID || '12660000-0000-0000-0000-000000001266';
export const REPORTING_YEARS = Object.freeze([2026, 2027, 2028, 2029, 2030]);

export function currentReportingYear() {
  return Number(new Intl.DateTimeFormat('en', { timeZone: 'Asia/Ho_Chi_Minh', year: 'numeric' }).format(new Date()));
}
export function isValidReportingYear(year) {
  return Number.isInteger(Number(year)) && Number(year) >= 1900 && Number(year) <= currentReportingYear();
}
export function availableReportingYears() {
  return [...new Set([...REPORTING_YEARS, currentReportingYear()])].filter(isValidReportingYear).sort((a, b) => a - b);
}
