import assert from 'node:assert/strict';
import { executionStatusOptions, executionStatusSeries, getStatusLabel, getStatusBadgeClass } from '../src/shared/statusPresentation.js';
assert.deepEqual(executionStatusOptions, [
  { value: 'NotStarted', label: 'Chưa thực hiện' },
  { value: 'InProgress', label: 'Đang thực hiện' },
  { value: 'Completed', label: 'Đã hoàn thành' }
]);
assert.deepEqual(executionStatusSeries.map(s => s.key), ['notStarted', 'inProgress', 'completed']);
for (const legacy of ['InProgressOnTime', 'InProgressOverdue', 'ExpiringSoon', 'Drafting', 'Reviewing', 'Submitted']) {
  assert.equal(getStatusLabel(legacy), 'Đang thực hiện');
  assert.equal(getStatusBadgeClass(legacy), getStatusBadgeClass('InProgress'));
}
for (const legacy of ['CompletedOnTime', 'CompletedOverdue']) {
  assert.equal(getStatusLabel(legacy), 'Đã hoàn thành');
  assert.equal(getStatusBadgeClass(legacy), getStatusBadgeClass('Completed'));
}
assert.equal(getStatusLabel('Approved'), 'Đã phê duyệt');
console.log('PASS execution status options, legacy labels, colors and approval separation');
