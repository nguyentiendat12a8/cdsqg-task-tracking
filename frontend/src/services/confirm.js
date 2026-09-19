import { ref } from 'vue';

const isVisible = ref(false);
const title = ref('Xác nhận');
const message = ref('');
const confirmText = ref('Đồng ý');
const cancelText = ref('Hủy');
const type = ref('danger'); // 'danger' | 'warning' | 'info'
let resolvePromise = null;

export function confirmModal(options) {
  if (typeof options === 'string') {
    options = { message: options };
  }
  
  title.value = options.title || 'Xác nhận thao tác';
  message.value = options.message || 'Bạn có chắc chắn muốn thực hiện thao tác này?';
  confirmText.value = options.confirmText || 'Đồng ý';
  cancelText.value = options.cancelText || 'Hủy';
  type.value = options.type || 'danger';
  isVisible.value = true;

  return new Promise((resolve) => {
    resolvePromise = resolve;
  });
}

export function handleConfirmResponse(result) {
  isVisible.value = false;
  if (resolvePromise) {
    resolvePromise(result);
    resolvePromise = null;
  }
}

export const confirmState = {
  isVisible,
  title,
  message,
  confirmText,
  cancelText,
  type
};
