import { vTooltip } from 'floating-vue';

// Tooltip chung của hệ thống: cùng giao diện tối "custom-dark" với tooltip chi tiết mục tiêu.
// Dùng thay cho thuộc tính title của trình duyệt: v-tip="'Nội dung'" hoặc v-tip="biểu thức".
// Giá trị rỗng/null thì không hiện tooltip.
export const TOOLTIP_THEME = 'custom-dark';

function toOptions(value) {
  if (value == null || value === false || value === '') return { content: false };
  const options = typeof value === 'object' ? { ...value } : { content: String(value) };
  return {
    theme: TOOLTIP_THEME,
    placement: 'top',
    delay: { show: 300, hide: 0 },
    popperClass: 'app-tip',
    ...options
  };
}

// Phần tử chỉ có biểu tượng vẫn cần tên cho trình đọc màn hình (trước đây do title cung cấp).
function syncAccessibleName(el, options) {
  const text = typeof options.content === 'string' ? options.content : '';
  if (el.dataset.tipLabel === 'auto') {
    if (text) el.setAttribute('aria-label', text);
    else { el.removeAttribute('aria-label'); delete el.dataset.tipLabel; }
    return;
  }
  if (!text || el.hasAttribute('aria-label') || el.hasAttribute('aria-labelledby')) return;
  if ((el.textContent || '').trim()) return;
  el.setAttribute('aria-label', text);
  el.dataset.tipLabel = 'auto';
}

function bind(hook) {
  return (el, binding) => {
    const options = toOptions(binding.value);
    syncAccessibleName(el, options);
    hook(el, { ...binding, value: options });
  };
}

export const appTooltip = {
  beforeMount: bind(vTooltip.beforeMount),
  updated: bind(vTooltip.updated),
  beforeUnmount: (el, binding) => vTooltip.beforeUnmount(el, binding)
};
