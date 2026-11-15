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

// Mục tiêu do tổ chức quốc tế đánh giá (thứ hạng EGDI của Liên Hợp Quốc: STT gốc MT-12, MT-25 của QĐ 1266).
// Số liệu lấy từ báo cáo của Liên Hợp Quốc nên các cơ quan, đơn vị trong nước không báo cáo: không đưa vào biểu mẫu Excel.
const EXTERNALLY_ASSESSED_1266 = ['MT-12', 'MT-25'];
export function isExternallyAssessedGoal(goal) {
  return String(goal?.documentNumber || '').includes('1266')
    && EXTERNALLY_ASSESSED_1266.includes(String(goal?.originalCode || '').trim().toUpperCase());
}
