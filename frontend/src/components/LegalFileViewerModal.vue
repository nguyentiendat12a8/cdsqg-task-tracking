<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/85 backdrop-blur-md flex items-center justify-center p-0 sm:p-2 animate-in fade-in duration-200">
    <div class="bg-white sm:rounded-2xl shadow-2xl border border-slate-200 w-full sm:w-[98vw] h-full sm:h-[96vh] max-w-none flex flex-col overflow-hidden">
      
      <!-- Modal Header -->
      <div class="px-5 py-3.5 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
        <div class="flex items-center gap-2.5 min-w-0 pr-4">
          <span class="p-2 bg-blue-100 text-blue-700 rounded-xl font-bold text-sm shrink-0">📄</span>
          <div class="truncate">
            <h3 class="text-sm sm:text-base font-bold text-slate-800 truncate" :title="fileItem?.cleanName || fileItem?.fileName">
              {{ fileItem?.cleanName || fileItem?.fileName || 'Xem Văn Bản Đính Kèm' }}
            </h3>
            <p class="text-[11px] text-slate-500 truncate" v-if="fileItem?.fileType">
              Phân loại: <span class="font-bold text-blue-700">{{ fileItem.fileType }}</span>
            </p>
          </div>
        </div>

        <div class="flex items-center gap-2 shrink-0">
          <button 
            v-if="fileItem?.fileUrl" 
            @click="openInNewWindow"
            class="px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer shadow-sm"
            title="Mở trong cửa sổ mới (Full screen)"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/></svg>
            <span>Mở Cửa Sổ Mới (Full)</span>
          </button>

          <a 
            v-if="fileItem?.fileUrl" 
            :href="fullFileUrl" 
            download 
            class="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer"
            title="Tải tệp về máy"
          >
            <svg class="w-4 h-4 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
            <span>Tải về</span>
          </a>

          <button 
            @click="close" 
            class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 font-bold flex items-center justify-center transition cursor-pointer text-base"
          >
            ✕
          </button>
        </div>
      </div>

      <!-- Viewer Body -->
      <div class="flex-1 bg-slate-100 relative overflow-hidden flex items-center justify-center">
        <!-- PDF File -->
        <iframe 
          v-if="isPdf" 
          :src="fullFileUrl" 
          class="w-full h-full border-none"
        ></iframe>

        <!-- Image File -->
        <div v-else-if="isImage" class="p-4 w-full h-full flex items-center justify-center overflow-auto">
          <img :src="fullFileUrl" class="max-w-full max-h-full object-contain rounded-lg shadow-md" alt="Document preview" />
        </div>

        <!-- Office Document / Other Files -->
        <div v-else class="p-8 text-center max-w-lg space-y-4">
          <div class="w-16 h-16 bg-blue-50 text-blue-600 rounded-2xl flex items-center justify-center mx-auto text-3xl font-bold shadow-sm">
            📑
          </div>
          <div>
            <h4 class="text-base font-bold text-slate-800 mb-1">
              {{ fileItem?.cleanName || fileItem?.fileName }}
            </h4>
            <p class="text-xs text-slate-500 leading-relaxed">
              Tệp tài liệu văn bản. Bạn có thể mở xem trực tuyến toàn màn hình trong cửa sổ mới hoặc tải tệp về máy.
            </p>
          </div>

          <div class="flex items-center justify-center gap-3 pt-2">
            <button 
              @click="openInNewWindow" 
              class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl transition shadow-sm inline-flex items-center gap-2 cursor-pointer"
            >
              🚀 Mở cửa sổ mới (Full screen)
            </button>
            <a 
              :href="fullFileUrl" 
              download 
              class="px-4 py-2 bg-slate-200 hover:bg-slate-300 text-slate-800 font-bold text-xs rounded-xl transition inline-flex items-center gap-2"
            >
              📥 Tải về máy
            </a>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { getApiUrl } from '../config/api';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  fileItem: { type: Object, default: null }
});

const emit = defineEmits(['close']);

function close() {
  emit('close');
}

const fullFileUrl = computed(() => {
  if (!props.fileItem?.fileUrl) return '';
  const url = props.fileItem.fileUrl;
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return getApiUrl(url);
});

const isPdf = computed(() => {
  const url = props.fileItem?.fileUrl || props.fileItem?.fileName || '';
  return url.toLowerCase().endsWith('.pdf');
});

const isImage = computed(() => {
  const url = props.fileItem?.fileUrl || props.fileItem?.fileName || '';
  return /\.(png|jpe?g|gif|webp|svg)$/i.test(url);
});

function openInNewWindow() {
  if (!fullFileUrl.value) return;
  
  if (isPdf.value || isImage.value) {
    window.open(fullFileUrl.value, '_blank');
  } else {
    const win = window.open('about:blank', '_blank');
    if (!win) return;
    const title = props.fileItem?.cleanName || props.fileItem?.fileName || 'Xem Văn Bản';
    win.document.write(`
      <!DOCTYPE html>
      <html>
      <head>
        <title>${title}</title>
        <meta charset="utf-8">
        <style>
          body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; margin: 0; padding: 0; background: #0f172a; color: #f8fafc; height: 100vh; display: flex; flex-direction: column; }
          .header { background: #1e293b; color: white; padding: 12px 24px; display: flex; align-items: center; justify-content: space-between; border-b: 1px solid #334155; }
          .title { font-size: 15px; font-weight: 700; }
          .btn { background: #2563eb; color: white; border: none; padding: 8px 16px; border-radius: 8px; font-weight: 700; cursor: pointer; text-decoration: none; font-size: 12px; }
          .btn:hover { background: #1d4ed8; }
          .viewer { flex: 1; width: 100%; height: 100%; border: none; }
        </style>
      </head>
      <body>
        <div class="header">
          <div class="title">📄 ${title}</div>
          <a href="${fullFileUrl.value}" download class="btn">📥 Tải về máy</a>
        </div>
        <iframe src="https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(fullFileUrl.value)}" class="viewer"></iframe>
      </body>
      </html>
    `);
  }
}
</script>
