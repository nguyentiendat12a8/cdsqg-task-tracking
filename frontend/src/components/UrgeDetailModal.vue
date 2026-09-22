<template>
  <div v-if="isOpen && log" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-6 space-y-5 animate-in fade-in duration-150 font-sans max-h-[90vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div>
          <span class="text-xs font-bold text-rose-600 uppercase tracking-wider block">Chi Tiết Văn Bản Thông Báo</span>
          <h3 class="text-base font-bold text-slate-800 mt-0.5">
            {{ log.taskCode }} - {{ log.taskTitle }}
          </h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <!-- Main Content Container -->
      <div class="flex-1 overflow-y-auto space-y-4 pr-1 custom-scrollbar">

        <!-- Info & Metadata Card -->
        <div class="p-4 bg-slate-50 border border-slate-200 rounded-xl flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-xs">
          <div>
            <span class="text-slate-500 font-medium">👥 Đầu mối / Người nhận: </span>
            <span class="font-extrabold text-slate-900 px-2 py-0.5 bg-blue-100 text-blue-900 rounded border border-blue-200">
              {{ log.recipientsSummary || log.leadAgencyName || log.leadAgencyCode || 'Đơn vị nhận' }}
            </span>
          </div>

          <div class="text-slate-500 font-medium text-[11px]">
            🕒 Thời gian lập: <strong class="text-slate-800">{{ formatDate(log.createdAt) }}</strong> | Bởi: <strong class="text-slate-800">{{ log.createdBy || 'Lãnh đạo' }}</strong>
          </div>
        </div>

        <!-- Full Directive Content -->
        <div class="space-y-1.5">
          <label class="text-xs font-extrabold text-slate-700 uppercase">Nội Dung Văn Bản Thông Báo</label>
          <div 
            v-html="log.urgeContent"
            class="p-5 bg-white border border-slate-200 rounded-xl text-xs text-slate-800 font-medium leading-relaxed shadow-2xs space-y-2 whitespace-pre-line"
          ></div>
        </div>

      </div>

      <!-- Footer Actions -->
      <div class="flex justify-end border-t border-slate-100 pt-3 shrink-0">
        <button type="button" @click="close" class="px-5 py-2 text-xs font-bold text-white bg-slate-800 hover:bg-slate-900 rounded-xl transition">
          Đóng
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  log: { type: Object, default: null }
});

const emit = defineEmits(['close']);

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    return `${d.toLocaleDateString('vi-VN')} ${d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
  } catch {
    return dateStr;
  }
}

function close() {
  emit('close');
}
</script>

<style scoped>
:deep(ul) {
  list-style-type: disc;
  padding-left: 1.25rem;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(ol) {
  list-style-type: decimal;
  padding-left: 1.25rem;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
</style>
