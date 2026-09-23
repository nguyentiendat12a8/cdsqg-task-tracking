import { getApiUrl } from '../config/api';

export function openFileInNewWindow(fileItem) {
  if (!fileItem || !fileItem.fileUrl) return;

  let fullUrl = fileItem.fileUrl;
  if (!fullUrl.startsWith('http://') && !fullUrl.startsWith('https://')) {
    fullUrl = getApiUrl(fullUrl);
  }

  const fileName = fileItem.cleanName || fileItem.fileName || 'Văn bản đính kèm';
  const ext = (fileName.split('.').pop() || '').toLowerCase();
  const isPdf = ext === 'pdf';
  const isImage = ['png', 'jpg', 'jpeg', 'gif', 'webp', 'svg'].includes(ext);

  if (isPdf || isImage) {
    window.open(fullUrl, '_blank');
    return;
  }

  const win = window.open('about:blank', '_blank');
  if (!win) return;

  win.document.write(`
    <!DOCTYPE html>
    <html lang="vi">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0">
      <title>${fileName}</title>
      <script src="https://cdnjs.cloudflare.com/ajax/libs/mammoth/1.6.0/mammoth.browser.min.js"></script>
      <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; background: #0f172a; color: #334155; min-height: 100vh; display: flex; flex-direction: column; }
        .top-bar { background: #1e293b; color: white; padding: 12px 24px; display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid #334155; position: sticky; top: 0; z-index: 50; }
        .doc-title { font-size: 15px; font-weight: 700; display: flex; align-items: center; gap: 8px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .btn-download { background: #2563eb; color: white; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 700; text-decoration: none; transition: background 0.15s; display: inline-flex; align-items: center; gap: 6px; }
        .btn-download:hover { background: #1d4ed8; }
        .content-area { flex: 1; padding: 32px 16px; display: flex; justify-content: center; overflow-y: auto; background: #0f172a; }
        .paper { background: white; width: 100%; max-width: 900px; min-height: 1000px; padding: 48px 56px; border-radius: 12px; box-shadow: 0 20px 25px -5px rgba(0,0,0,0.3); line-height: 1.7; font-size: 14px; color: #1e293b; }
        .paper h1, .paper h2, .paper h3 { margin-top: 1.5em; margin-bottom: 0.5em; font-weight: 700; color: #0f172a; }
        .paper p { margin-bottom: 1em; text-align: justify; }
        .paper table { width: 100%; border-collapse: collapse; margin: 1.5em 0; }
        .paper th, .paper td { border: 1px solid #cbd5e1; padding: 8px 12px; }
        .paper th { background: #f8fafc; font-weight: 700; }
        .loading { text-align: center; color: #94a3b8; font-size: 15px; padding: 60px 0; font-weight: 600; }
        .error-msg { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; padding: 16px 20px; border-radius: 10px; font-size: 13px; text-align: center; }
      </style>
    </head>
    <body>
      <div class="top-bar">
        <div class="doc-title">📜 ${fileName}</div>
        <a href="${fullUrl}" download class="btn-download">📥 Tải tệp gốc về máy</a>
      </div>
      <div class="content-area">
        <div class="paper">
          <div id="output" class="loading">⏳ Đang nạp và đọc dữ liệu văn bản...</div>
        </div>
      </div>
      <script>
        async function loadDoc() {
          const out = document.getElementById('output');
          try {
            const res = await fetch('${fullUrl}');
            if (!res.ok) throw new Error('Không thể tải tệp từ máy chủ (Mã ' + res.status + ')');
            const arrayBuffer = await res.arrayBuffer();
            if (typeof mammoth !== 'undefined' && mammoth.convertToHtml) {
              const result = await mammoth.convertToHtml({ arrayBuffer: arrayBuffer });
              out.innerHTML = result.value || '<p class="loading">Văn bản không có nội dung chữ hoặc là tệp đặc biệt.</p>';
            } else {
              out.innerHTML = '<div class="error-msg">Không thể nạp thư viện đọc file Word. Bạn có thể nhấn nút "Tải tệp gốc về máy" ở góc trên để xem chi tiết.</div>';
            }
          } catch (err) {
            out.innerHTML = '<div class="error-msg">⚠️ Không thể hiển thị trực tuyến: ' + err.message + '. Vui lòng nhấn nút "Tải tệp gốc về máy" để mở bằng ứng dụng trên máy.</div>';
          }
        }
        loadDoc();
      </script>
    </body>
    </html>
  `);
}
