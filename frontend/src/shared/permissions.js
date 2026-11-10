// Bảng phân quyền giao diện, đối ứng backend/Api/Security/RolePermissions.cs.
// Chỉ dùng để ẩn/hiện chức năng; API mới là nơi chặn quyền thật. Đổi quyền thì sửa cả hai nơi.
import { ROLE, normalizeRole } from './roles.js';

export const MODULE = Object.freeze({
  // Tiện ích: văn bản pháp lý, kế hoạch CĐS đơn vị, tin tức CĐS.
  utilities: 'utilities',
  // Mục tiêu, nhiệm vụ, kế hoạch mục tiêu, báo cáo, dashboard.
  planning: 'planning',
  // Thiết lập chung, người dùng, dữ liệu AI.
  administration: 'administration'
});

export const ACTION = Object.freeze({ view: 'view', edit: 'edit' });

// Cán bộ đơn vị: quyền báo cáo tiến độ và sửa kế hoạch CĐS của đơn vị mình vẫn do từng màn hình
// kiểm tra theo cơ quan được giao, không nằm trong bảng này.
const MATRIX = Object.freeze({
  [ROLE.Admin]: { utilities: ['view', 'edit'], planning: ['view', 'edit'], administration: ['view', 'edit'] },
  [ROLE.Clerk]: { utilities: ['view', 'edit'], planning: ['view'], administration: [] },
  [ROLE.AgencyUser]: { utilities: ['view'], planning: ['view'], administration: [] }
});

export function can(role, module, action = ACTION.view) {
  return MATRIX[normalizeRole(role)]?.[module]?.includes(action) ?? false;
}

// Báo cáo tiến độ: Admin và cán bộ đơn vị (API kiểm tra cơ quan được giao); Văn thư chỉ xem.
export function canReportProgress(role) {
  return normalizeRole(role) !== ROLE.Clerk;
}

// Admin và Văn thư xem dữ liệu toàn hệ thống; cán bộ đơn vị chỉ xem phạm vi cơ quan mình.
export function seesAllAgencies(role) {
  const normalized = normalizeRole(role);
  return normalized === ROLE.Admin || normalized === ROLE.Clerk;
}
