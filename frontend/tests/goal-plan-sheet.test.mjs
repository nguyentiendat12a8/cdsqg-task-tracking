import assert from 'node:assert/strict';
import { readGoalPlanRows, isGoalPlanRows, isPlanEligibleGoal, leadsGoal, planYearHeader, configuredPlanValue } from '../src/utils/goalPlanSheet.js';
import { isAgencySumGoal } from '../src/shared/agencyPresentation.js';

const goal = { taskId: 't1', code: 'MT-03', title: 'Tỷ lệ', itemType: 'Goal', evaluationType: 'Quantitative', unitName: '%', leadAgencyId: 'A1', leadAssignmentAgencyIds: ['a1'], startDate: '2026-01-01', dueDate: '2028-12-31', yearlyTargets: { 2026: 90, 2027: null, 2028: null }, configuredYearlyTargets: { 2026: 90 } };
const shared = { taskId: 't2', code: 'MT-24', title: 'Kỹ năng số', itemType: 'Goal', evaluationType: 'Quantitative', unitName: 'Lượt người', isGeneralTask: true, leadAgencyCode: 'ALL_AGENCIES', yearlyTargets: { 2026: 10000000 } };
const percentShared = { ...shared, code: 'MT-99', unitName: '%' };

assert.equal(isPlanEligibleGoal(goal), true);
assert.equal(isPlanEligibleGoal(shared), false, 'Shared goals have no per-agency plan');
assert.equal(isAgencySumGoal(shared), true);
assert.equal(isAgencySumGoal(percentShared), false);
assert.equal(isAgencySumGoal(goal), false);
assert.equal(leadsGoal(goal, 'a1'), true);
assert.equal(leadsGoal(goal, 'b2'), false);
assert.equal(configuredPlanValue(goal, 2026), 90);
assert.equal(configuredPlanValue({ yearlyTargets: { 2026: 90 } }, 2026), null);
const defaultOnly = { ...goal, configuredYearlyTargets: { 2026: null }, defaultYearlyTargets: { 2026: 90 } };
assert.equal(configuredPlanValue(defaultOnly, 2026), null, 'Excel must leave a default-only plan empty');
assert.deepEqual(readGoalPlanRows([
  ['Mã MT', planYearHeader(2026)],
  ['MT-03', 95]
], [defaultOnly]).map(row => [row.current, row.value, row.error]), [[null, 95, null]]);

const rows = [
  ['KẾ HOẠCH MỤC TIÊU'], ['Đơn vị'], ['CĂN CỨ ...'],
  ['STT', 'Mã MT', 'Tên', 'Đơn vị tính', planYearHeader(2025), planYearHeader(2026), planYearHeader(2027), planYearHeader(2028)],
  [1, 'MT-03', 'Tỷ lệ', '%', '—', 90, '95,5', ''],
  [2, 'MT-24', 'Kỹ năng số', 'Lượt người', '', 12000000, '', ''],
  [3, 'MT-77', 'Không có', '%', '', 5, '', ''],
  [4, 'MT-03', 'Tỷ lệ', '%', 3, 'abc', '', 0]
];
assert.equal(isGoalPlanRows(rows), true);
assert.equal(isGoalPlanRows([['STT', 'Mã MT', 'TIẾN ĐỘ ƯỚC TÍNH [CẦN ĐIỀN]']]), false);
const parsed = readGoalPlanRows(rows, [goal, shared]);
assert.deepEqual(parsed.map(r => [r.code, r.year, r.error]), [
  ['MT-03', 2027, null],                         // 90 unchanged is skipped; 95,5 is new
  ['MT-24', 2026, 'Mục tiêu không lập kế hoạch riêng'],
  ['MT-77', 2026, 'Mã không tồn tại'],
  ['MT-03', 2025, 'Năm ngoài thời gian thực hiện'],
  ['MT-03', 2026, 'Kế hoạch phải là số lớn hơn 0'],
  ['MT-03', 2028, 'Kế hoạch phải là số lớn hơn 0']
]);
assert.equal(parsed[0].value, 95.5);
console.log('Passed: goal plan sheet selection, unchanged cells, shared MT-24 exclusion, year range and number validation.');
