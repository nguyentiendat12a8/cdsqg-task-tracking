import { ref } from 'vue';
import { currentReportingYear } from '../config/reporting.js';

export const REPORT_YEAR_MIN = 2025;
export const REPORT_YEAR_MAX = 2045;

export function getReportYearMin(docOrCode) {
  if (!docOrCode) return 2026;
  if (typeof docOrCode === 'number') return docOrCode;
  if (typeof docOrCode === 'string') {
    return docOrCode.includes('2629') ? 2025 : 2026;
  }
  if (docOrCode.code === '2629' || docOrCode.documentNumber?.includes('2629') || docOrCode.startYear === 2025) {
    return 2025;
  }
  return docOrCode.startYear ?? 2026;
}

export function getReportYearOptions(docOrCodeOrMinYear) {
  let minYear = 2026;
  if (typeof docOrCodeOrMinYear === 'number') {
    minYear = docOrCodeOrMinYear;
  } else {
    minYear = getReportYearMin(docOrCodeOrMinYear);
  }
  return Array.from(
    { length: REPORT_YEAR_MAX - minYear + 1 },
    (_, i) => {
      const y = minYear + i;
      return { value: y, label: `Năm ${y}` };
    }
  );
}

export const reportYearOptions = getReportYearOptions(2026);

export const reportYear = ref(currentReportingYear());
