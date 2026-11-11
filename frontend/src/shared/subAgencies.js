// Đơn vị trực thuộc (Cấp 3) được giao cho mục tiêu/nhiệm vụ. Một mục có thể giao nhiều đơn vị,
// độc lập với cơ quan đồng chủ trì (assignedAgencyId).

const BKHCN_NAMES = ['khoa học và công nghệ', 'khoa học & công nghệ', 'khoa học công nghệ'];

export function isBKHCN(agency) {
  if (!agency) return false;
  const code = String(agency.code || agency.leadAgencyCode || '').toLowerCase();
  const name = String(agency.name || agency.leadAgencyName || '').toLowerCase();
  return code === 'bkhcn' || BKHCN_NAMES.some(n => name.includes(n));
}

const sameId = (a, b) => a != null && b != null && String(a).toLowerCase() === String(b).toLowerCase();

export function findBKHCN(agencies = []) {
  return agencies.find(a => !a.parentId && isBKHCN(a)) || null;
}

// Bộ KH&CN tham gia chủ trì: chủ trì, đồng chủ trì, hoặc thuộc nhóm "Các bộ, ngành..." của nhiệm vụ chung.
export function itemInvolvesBKHCN(item, agencies = []) {
  if (!item) return false;
  if (isBKHCN({ code: item.leadAgencyCode, name: item.leadAgencyName })) return true;
  const bkhcn = findBKHCN(agencies);
  if (bkhcn && (sameId(item.leadAgencyId, bkhcn.id) || sameId(item.assignedAgencyId, bkhcn.id))) return true;
  return !!item.isGeneralTask;
}

// Nhóm đơn vị (ví dụ "Tất cả cơ quan, đơn vị thuộc Bộ"): giao cho mọi đơn vị trực thuộc của Bộ.
export function isUnitGroup(agency) {
  if (!agency) return false;
  return String(agency.code || '').toUpperCase().startsWith('ALL_') || agency.type === 5 || agency.type === '5' || agency.type === 'Special';
}

export function bkhcnSubAgencies(agencies = []) {
  const bkhcn = findBKHCN(agencies);
  if (!bkhcn) return [];
  return agencies
    .filter(a => sameId(a.parentId, bkhcn.id) && a.type !== 4 && a.type !== 'Other')
    .sort((a, b) => (isUnitGroup(b) - isUnitGroup(a)) || String(a.name).localeCompare(String(b.name), 'vi'));
}

export function subAgencyIdsOf(item) {
  return Array.isArray(item?.subAgencyIds) ? item.subAgencyIds.map(String) : [];
}

export function subAgencyLabel(item) {
  const names = Array.isArray(item?.subAgencyNames) ? item.subAgencyNames.filter(Boolean) : [];
  return names.join(', ');
}

// Đơn vị đã giao nhưng chưa có trong danh mục đang tải (ví dụ danh mục được bổ sung sau khi mở trang):
// thêm vào lựa chọn với tên lấy từ chính mục, để ô chọn không bao giờ hiển thị mã.
export function withAssignedSubAgencies(options = [], item) {
  const ids = subAgencyIdsOf(item);
  const names = Array.isArray(item?.subAgencyNames) ? item.subAgencyNames : [];
  const missing = ids.filter(id => !options.some(o => sameId(o.id, id)));
  if (!missing.length) return options;
  return [...options, ...missing.map(id => ({
    id,
    name: (names.length === ids.length ? names[ids.indexOf(id)] : '') || 'Đơn vị đã giao (không còn trong danh mục)'
  }))];
}

// Cần tải lại danh mục khi mục đang giao cho đơn vị chưa có trong danh mục hiện tại.
export function catalogMissesSubAgencies(agencies = [], item) {
  return subAgencyIdsOf(item).some(id => !agencies.some(a => sameId(a.id, id)));
}

export function hasSubAgency(item, agencyId) {
  return subAgencyIdsOf(item).some(id => sameId(id, agencyId));
}

// Lựa chọn đơn vị trực thuộc trong form tạo/sửa: đơn vị trực thuộc cơ quan chủ trì,
// cộng thêm đơn vị trực thuộc Bộ KH&CN khi Bộ đồng chủ trì hoặc mục là nhiệm vụ chung.
export function subAgencyOptionsFor(agencies = [], { leadAgencyId, assignedAgencyId, isGeneralTask } = {}) {
  const lead = agencies.find(a => sameId(a.id, leadAgencyId));
  const list = agencies.filter(a => leadAgencyId && sameId(a.parentId, leadAgencyId) && a.type !== 4 && a.type !== 'Other');
  const bkhcn = findBKHCN(agencies);
  const coLeadIsBKHCN = bkhcn && sameId(assignedAgencyId, bkhcn.id);
  const leadIsGroup = isGeneralTask || String(lead?.code || '').toUpperCase().startsWith('ALL_');
  if (coLeadIsBKHCN || leadIsGroup) {
    for (const sub of bkhcnSubAgencies(agencies)) if (!list.some(a => sameId(a.id, sub.id))) list.push(sub);
  }
  return list.map(a => ({ value: a.id, label: String(a.name || '').trim() }));
}
