import assert from 'node:assert/strict';
import { ROLE, normalizeRole, isAdminRole, isClerkRole } from '../src/shared/roles.js';
import { MODULE, ACTION, can, canReportProgress, seesAllAgencies } from '../src/shared/permissions.js';

// Vai trò từ API có thể là số (enum) hoặc chuỗi.
assert.equal(normalizeRole(1), ROLE.Admin);
assert.equal(normalizeRole('Admin'), ROLE.Admin);
assert.equal(normalizeRole(3), ROLE.Clerk);
assert.equal(normalizeRole('clerk'), ROLE.Clerk);
assert.equal(normalizeRole(2), ROLE.AgencyUser);
assert.equal(normalizeRole(undefined), ROLE.AgencyUser);
assert.equal(isAdminRole('3'), false);
assert.equal(isClerkRole('Clerk'), true);

// Văn thư: thao tác Tiện ích, chỉ xem mục tiêu/nhiệm vụ/báo cáo, không vào quản trị.
assert.equal(can('Clerk', MODULE.utilities, ACTION.edit), true);
assert.equal(can('Clerk', MODULE.planning, ACTION.view), true);
assert.equal(can('Clerk', MODULE.planning, ACTION.edit), false);
assert.equal(can('Clerk', MODULE.administration, ACTION.view), false);
assert.equal(canReportProgress('Clerk'), false);
assert.equal(seesAllAgencies('Clerk'), true);

// Admin toàn quyền; cán bộ đơn vị giữ quy tắc cũ (báo cáo theo cơ quan được giao, API kiểm tra).
for (const module of Object.values(MODULE)) assert.equal(can('Admin', module, ACTION.edit), true);
assert.equal(can('AgencyUser', MODULE.utilities, ACTION.edit), false);
assert.equal(can('AgencyUser', MODULE.administration, ACTION.view), false);
assert.equal(canReportProgress('AgencyUser'), true);
assert.equal(seesAllAgencies('AgencyUser'), false);
console.log('Passed: role normalization and Admin / Văn thư / cán bộ đơn vị permission matrix.');
