// Charts read the same tokens as buttons, badges and status cards.
export function themeColor(name) {
  return getComputedStyle(document.documentElement).getPropertyValue(`--color-${name}`).trim();
}
