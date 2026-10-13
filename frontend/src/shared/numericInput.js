// Editable Vietnamese quantities: dots group thousands, comma separates decimals.
export function parseQuantityInput(text) {
  const value = String(text ?? '').trim();
  if (!value) return null;
  if (!/^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value)) return NaN;
  const number = Number(value.replaceAll('.', '').replace(',', '.'));
  return Number.isFinite(number) ? number : NaN;
}
export function formatQuantityInput(value) {
  if (value == null || value === '') return '';
  // Preserve fractional digits of the canonical value; don't round on blur.
  const parts = String(value).split('.');
  if (!/^-?\d+$/.test(parts[0])) return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 20 }).format(Number(value));
  return parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.') + (parts[1] ? `,${parts[1]}` : '');
}
