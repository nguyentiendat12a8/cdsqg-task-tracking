// The API supplies applicableYears; missing values are different from excluded years.
export function isProgressReportYearApplicable(goal, year) {
  if (year === 2025 && !String(goal.documentNumber || '').includes('2629')) return false;
  if (Array.isArray(goal.applicableYears)) return goal.applicableYears.map(Number).includes(year);
  const key = year === 2025 ? 'actual2025' : year === 2026 ? 'estimate2026' : `plan${year}`;
  return goal[key] !== '—';
}
export function progressReportYearValue(goal, year) {
  if (!isProgressReportYearApplicable(goal, year)) return '—';
  const key = year === 2025 ? 'actual2025' : year === 2026 ? 'estimate2026' : `plan${year}`;
  return goal[key] ?? '';
}
