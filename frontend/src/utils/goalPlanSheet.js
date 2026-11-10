// Sheet "Kế hoạch mục tiêu" của biểu mẫu báo cáo mục tiêu: dùng chung cho xuất và nhập Excel.
// API là nơi kiểm tra quyền và ghi kế hoạch; module này chỉ chọn mục tiêu, năm và đọc số.
import { isGeneralTaskItem } from '../shared/agencyPresentation.js';
import { parseQuantityInput } from '../shared/numericInput.js';

export const GOAL_PLAN_SHEET_NAME = 'Kế hoạch mục tiêu';
const PLAN_HEADER = /^KẾ HOẠCH\s+(\d{4})/i;

export function planYearHeader(year) {
  return `KẾ HOẠCH ${year} [CẦN ĐIỀN]`;
}

export function parsePlanHeaderYear(text) {
  const match = PLAN_HEADER.exec(String(text ?? '').trim());
  return match ? Number(match[1]) : null;
}

const isGoal = item => ['Goal', '1', 1].includes(item?.itemType);
const isQuantitative = item => ['Quantitative', '1', 1].includes(item?.evaluationType);
const sameId = (a, b) => a != null && b != null && String(a).toLowerCase() === String(b).toLowerCase();

// Kế hoạch riêng chỉ lập cho mục tiêu định lượng có cơ quan chủ trì cụ thể.
// Mục tiêu giao chung (vd. MT-24/1266) không có kế hoạch riêng trong biểu mẫu.
export function isPlanEligibleGoal(item) {
  return isGoal(item) && isQuantitative(item) && !isGeneralTaskItem(item);
}

export function leadsGoal(item, agencyId) {
  if (!agencyId) return false;
  return sameId(item.leadAgencyId, agencyId) || (item.leadAssignmentAgencyIds || []).some(id => sameId(id, agencyId));
}

export function planYears(items) {
  const years = new Set();
  for (const item of items) Object.keys(item.yearlyTargets || {}).forEach(year => /^\d{4}$/.test(year) && years.add(Number(year)));
  if (years.size === 0) for (let year = 2026; year <= 2030; year++) years.add(year);
  return [...years].sort((a, b) => a - b);
}

// Cùng quy tắc PlanningYears.Contains của API: khoảng StartDate–DueDate, thiếu thì theo văn bản.
export function isPlanYearInRange(item, year, years) {
  const start = item.startDate ? new Date(item.startDate).getFullYear() : Math.min(...years);
  const end = item.dueDate ? new Date(item.dueDate).getFullYear() : Math.max(...years);
  return year >= start && year <= end;
}

export function currentPlanValue(item, year) {
  const value = item.yearlyTargets?.[year] ?? item.yearlyTargets?.[String(year)];
  return typeof value === 'number' && Number.isFinite(value) ? value : null;
}

export function configuredPlanValue(item, year) {
  const value = item.configuredYearlyTargets?.[year] ?? item.configuredYearlyTargets?.[String(year)];
  return typeof value === 'number' && Number.isFinite(value) ? value : null;
}

export function readPlanCell(value) {
  if (value === undefined || value === null) return null;
  if (typeof value === 'number') return Number.isFinite(value) ? value : NaN;
  const text = String(value).trim();
  if (!text || text === '—' || text === '-') return null;
  return parseQuantityInput(text);
}

export function isGoalPlanRows(rows) {
  return (rows || []).slice(0, 12).some(row => (row || []).some(cell => parsePlanHeaderYear(cell)));
}

// rows: mảng hàng của sheet (sheet_to_json header:1). Trả về các ô kế hoạch khác với kế hoạch hiện hành.
export function readGoalPlanRows(rows, items) {
  const headerIndex = rows.findIndex(row => (row || []).some(cell => parsePlanHeaderYear(cell)));
  if (headerIndex < 0) return [];
  const header = rows[headerIndex];
  const codeCol = header.findIndex(cell => /^mã/i.test(String(cell ?? '').trim()));
  const yearCols = header.map((cell, col) => ({ col, year: parsePlanHeaderYear(cell) })).filter(entry => entry.year);
  if (codeCol < 0) return [];
  const byCode = new Map(items.map(item => [String(item.code || '').trim().toLowerCase(), item]));
  const result = [];
  for (const row of rows.slice(headerIndex + 1)) {
    const code = String(row?.[codeCol] ?? '').trim();
    if (!code) continue;
    const item = byCode.get(code.toLowerCase());
    for (const { col, year } of yearCols) {
      const value = readPlanCell(row[col]);
      if (value === null) continue;
      const current = item ? configuredPlanValue(item, year) : null;
      if (item && current !== null && value === current) continue;
      let error = null;
      if (!item) error = 'Mã không tồn tại';
      else if (!isPlanEligibleGoal(item)) error = 'Mục tiêu không lập kế hoạch riêng';
      else if (!Number.isFinite(value) || value <= 0) error = 'Kế hoạch phải là số lớn hơn 0';
      else if (!isPlanYearInRange(item, year, yearCols.map(entry => entry.year))) error = 'Năm ngoài thời gian thực hiện';
      result.push({ code, year, value, current, item, error, title: item?.title || '', unitName: item?.unitName || '' });
    }
  }
  return result;
}
