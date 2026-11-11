export const SPECIAL_AGENCY_CODES = Object.freeze([
  'ALL_AGENCIES',
  'ALL_MINISTRIES',
  'ALL_PROVINCES',
  'ALL_PROVINCES_UBND',
  'ALL_MINISTRIES_DIRECT'
]);

const generalCodes = new Set(SPECIAL_AGENCY_CODES);
const generalIds = new Set(['9999', '9998', '9997', '9996', '9995'].map(suffix => `00000000-0000-0000-0000-00000000${suffix}`));

export function isSpecialAgencyCode(code) {
  if (!code) return false;
  const upper = String(code).trim().toUpperCase();
  return generalCodes.has(upper) || upper.startsWith('ALL_');
}

export function isSpecialAgency(agency) {
  if (!agency) return false;
  const code = String(agency.code || agency.Code || '').trim().toUpperCase();
  if (isSpecialAgencyCode(code)) return true;
  const type = agency.type ?? agency.Type;
  if (type === 'Special' || type === 5 || type === '5') return true;
  const name = String(agency.name || agency.Name || agency.label || '').trim().toLowerCase();
  return name.startsWith('tất cả') || name.startsWith('khối ') || code === 'BNDP';
}

export function isProvinceAgency(agency) {
  if (!agency) return false;
  if (isSpecialAgency(agency)) return false;
  const type = agency.type ?? agency.Type;
  if (type === 'Province' || type === 2 || type === '2') return true;
  const name = String(agency.name || agency.Name || agency.label || '').trim().toLowerCase();
  return (
    name.startsWith('tỉnh ') ||
    name.startsWith('thành phố ') ||
    name.startsWith('ubnd tỉnh') ||
    name.startsWith('ubnd thành phố') ||
    name.startsWith('tp.')
  );
}

export function isMinistryAgency(agency) {
  if (!agency) return false;
  if (isSpecialAgency(agency) || isProvinceAgency(agency)) return false;
  const hasParent = Boolean(agency.parentId && agency.parentId !== '' && String(agency.parentId) !== '00000000-0000-0000-0000-000000000000');
  if (hasParent) return false;
  const type = agency.type ?? agency.Type;
  if (type === 'Ministry' || type === 1 || type === '1') return true;
  const name = String(agency.name || agency.Name || agency.label || '').trim().toLowerCase();
  return (
    name.startsWith('bộ ') ||
    name.startsWith('ban ') ||
    name.startsWith('viện ') ||
    name.startsWith('văn phòng chính phủ') ||
    name.startsWith('ngân hàng nhà nước') ||
    name.startsWith('thanh tra chính phủ') ||
    name.startsWith('toà án nhân dân') ||
    name.startsWith('viện kiểm sát nhân dân') ||
    name.startsWith('học viện chính trị')
  );
}

/**
 * Phân cấp thứ tự cơ quan theo tiêu chuẩn toàn hệ thống:
 * 1: Đặc biệt (Special, ALL_*, Tất cả / Khối)
 * 2: Bộ ngành (Ministry, cơ quan ngang bộ, cơ quan trung ương)
 * 3: Địa phương (Province, tỉnh / thành phố trực thuộc trung ương)
 * 4: Đơn vị trực thuộc / Nội bộ / Khác
 */
export function getAgencyTypeRank(agency) {
  if (!agency) return 99;
  if (isSpecialAgency(agency)) return 1;
  const hasParent = Boolean(agency.parentId && agency.parentId !== '' && String(agency.parentId) !== '00000000-0000-0000-0000-000000000000');
  if (!hasParent) {
    if (isMinistryAgency(agency)) return 2;
    if (isProvinceAgency(agency)) return 3;
  }
  return 4;
}

export function compareAgenciesByStandardOrder(a, b) {
  const rankA = getAgencyTypeRank(a);
  const rankB = getAgencyTypeRank(b);
  if (rankA !== rankB) return rankA - rankB;
  const nameA = String(a?.name || a?.label || a?.Name || '').replace(/^🌐\s*/, '').trim();
  const nameB = String(b?.name || b?.label || b?.Name || '').replace(/^🌐\s*/, '').trim();
  return nameA.localeCompare(nameB, 'vi', { sensitivity: 'base' });
}

export function sortAgenciesByStandardOrder(agencies) {
  if (!Array.isArray(agencies)) return [];
  return [...agencies].sort(compareAgenciesByStandardOrder);
}

export function formatAgencyOption(ag, list = []) {
  if (!ag) return null;
  const isSpecial = isSpecialAgency(ag);
  let rawName = String(ag.name || ag.label || '').replace(/^🌐\s*/, '').trim();
  let label = rawName;
  if (isSpecial) {
    label = `🌐 ${rawName}`;
  } else if (ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000') {
    const parentAg = Array.isArray(list) ? list.find(p => p.id === ag.parentId) : null;
    if (parentAg) {
      const pName = String(parentAg.name || parentAg.label || '').replace(/^🌐\s*/, '').trim();
      label = `${rawName} (Trực thuộc ${pName})`;
    }
  }
  return {
    value: ag.id ?? ag.value,
    label,
    code: ag.code,
    type: ag.type,
    parentId: ag.parentId
  };
}

// Display compatibility only. Authorization always belongs to the API.
export function isGeneralTaskItem(item) {
  if (!item) return false;
  if (item.isGeneralTask || generalCodes.has(String(item.leadAgencyCode || '').toUpperCase()) || generalIds.has(String(item.leadAgencyId || '').toLowerCase())) return true;
  const name = String(item.leadAgencyName || '').toLowerCase();
  return ['các bộ, ngành', 'các địa phương', 'ubnd tỉnh, thành phố'].some(part => name.includes(part));
}

// Mirrors ProgressCalculator.SumsAcrossAgencies: shared quantitative goals counted in absolute
// units (e.g. MT-24/1266, "lượt người") show the sum of every agency's total, not an average.
export function isAgencySumGoal(item) {
  if (!item || !isGeneralTaskItem(item)) return false;
  if (!['Quantitative', '1', 1].includes(item.evaluationType)) return false;
  const unit = String(item.unitName || item.unit?.name || '').trim().toLowerCase();
  return Boolean(unit) && !unit.includes('%') && unit !== 'percent' && !['hạng', 'thứ hạng', 'rank'].includes(unit);
}

export const assignmentRoleOptions = Object.freeze([{ value: 'Lead', label: 'Chủ trì' }, { value: 'Coordinating', label: 'Phối hợp' }]);
