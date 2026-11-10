// Vai trò tài khoản (khớp UserRoleEnum ở backend: Admin = 1, AgencyUser = 2, Clerk = 3).
export const ROLE = Object.freeze({ Admin: 'Admin', AgencyUser: 'AgencyUser', Clerk: 'Clerk' });

export const ROLE_LABELS = Object.freeze({
  [ROLE.Admin]: 'Quản trị viên (Admin)',
  [ROLE.Clerk]: 'Văn thư',
  [ROLE.AgencyUser]: 'Cán bộ đơn vị'
});

export function normalizeRole(role) {
  const value = String(role ?? '').trim().toLowerCase();
  if (value === '1' || value === 'admin') return ROLE.Admin;
  if (value === '3' || value === 'clerk') return ROLE.Clerk;
  return ROLE.AgencyUser;
}

export function isAdminRole(role) { return normalizeRole(role) === ROLE.Admin; }
export function isClerkRole(role) { return normalizeRole(role) === ROLE.Clerk; }
