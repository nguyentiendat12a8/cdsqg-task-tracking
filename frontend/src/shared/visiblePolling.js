// Polling chỉ khi tab đang hiển thị: không gửi request khi người dùng chuyển tab/thu nhỏ cửa sổ,
// bỏ lượt nếu lượt trước chưa xong, và tải lại ngay khi người dùng quay lại tab.
export function createVisiblePoller(task, intervalMs) {
  let timer = null;
  let running = false;
  const isVisible = () => typeof document === 'undefined' || document.visibilityState !== 'hidden';
  async function run() {
    if (running || !isVisible()) return;
    running = true;
    try { await task(); } catch { /* lỗi đã được task tự xử lý */ } finally { running = false; }
  }
  function onVisibility() { if (isVisible()) run(); }
  return {
    start() {
      if (timer) return;
      timer = setInterval(run, intervalMs);
      document.addEventListener('visibilitychange', onVisibility);
    },
    stop() {
      if (timer) clearInterval(timer);
      timer = null;
      document.removeEventListener('visibilitychange', onVisibility);
    },
    run
  };
}
