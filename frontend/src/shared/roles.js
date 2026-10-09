export function isAdminRole(role) { return role === 1 || role === '1' || String(role).toLowerCase() === 'admin'; }
