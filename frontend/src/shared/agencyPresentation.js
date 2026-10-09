const generalCodes = new Set(['ALL_AGENCIES', 'ALL_MINISTRIES', 'ALL_PROVINCES', 'ALL_PROVINCES_UBND', 'ALL_MINISTRIES_DIRECT']);
const generalIds = new Set(['9999', '9998', '9997', '9996', '9995'].map(suffix => `00000000-0000-0000-0000-00000000${suffix}`));
// Display compatibility only. Authorization always belongs to the API.
export function isGeneralTaskItem(item) {
  if (!item) return false;
  if (item.isGeneralTask || generalCodes.has(String(item.leadAgencyCode || '').toUpperCase()) || generalIds.has(String(item.leadAgencyId || '').toLowerCase())) return true;
  const name = String(item.leadAgencyName || '').toLowerCase();
  return ['các bộ, ngành', 'các địa phương', 'ubnd tỉnh, thành phố'].some(part => name.includes(part));
}
