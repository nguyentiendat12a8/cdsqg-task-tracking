<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 space-y-5 animate-in fade-in duration-150 font-sans max-h-[85vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div>
          <span class="text-xs font-bold text-blue-600 uppercase tracking-wider block">Nhật Ký & Lịch Sử Chỉ Đạo</span>
          <h3 class="text-lg font-extrabold text-slate-800">📜 Lịch Sử Các Văn Bản Đôn Đốc Tiến Độ</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <!-- History Content -->
      <div v-if="isLoading" class="p-8 text-center text-slate-400 text-xs font-semibold">
        Đang tải lịch sử đôn đốc...
      </div>

      <div v-else-if="logs.length === 0" class="p-8 text-center bg-slate-50 rounded-xl border border-slate-200 text-slate-400 text-xs font-semibold italic">
        Chưa có nhật ký đôn đốc nào được lưu.
      </div>

      <div v-else class="flex-1 overflow-y-auto space-y-4 pr-1">
        <div 
          v-for="log in logs" 
          :key="log.id"
          class="p-4 bg-slate-50/80 rounded-xl border border-slate-200/90 space-y-2 hover:border-slate-300 transition"
        >
          <div class="flex items-center justify-between border-b border-slate-200/60 pb-2">
            <div class="flex items-center gap-2">
              <span class="px-2.5 py-1 bg-rose-100 text-rose-800 font-extrabold text-xs rounded-lg">
                {{ log.taskCode }}
              </span>
              <span class="text-xs font-bold text-slate-800">{{ log.taskTitle }}</span>
            </div>

            <div class="text-right text-[11px] text-slate-400 font-semibold">
              🕒 {{ formatDate(log.createdAt) }} bởi <span class="text-slate-700 font-bold">{{ log.createdBy }}</span>
            </div>
          </div>

          <div class="text-xs text-slate-700 font-medium whitespace-pre-line bg-white p-3 rounded-lg border border-slate-200/60">
            {{ log.urgeContent }}
          </div>

          <div class="flex items-center justify-between text-[11px] text-slate-500 font-semibold pt-1">
            <span>Cơ quan nhận đôn đốc: <span class="font-bold text-slate-800">{{ log.leadAgencyName || log.leadAgencyCode }}</span></span>
            <span class="text-emerald-700 font-bold bg-emerald-50 border border-emerald-200 px-2 py-0.5 rounded">Đã lưu CSDL</span>
          </div>
        </div>
      </div>

      <!-- Footer Actions -->
      <div class="flex justify-end border-t border-slate-100 pt-3 shrink-0">
        <button type="button" @click="close" class="px-5 py-2 text-xs font-bold text-white bg-slate-800 hover:bg-slate-900 rounded-xl">Đóng</button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  taskId: { type: String, default: null }
});

const emit = defineEmits(['close']);

const logs = ref([]);
const isLoading = ref(true);

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    const d = new Date(dateStr);
    return `${d.toLocaleDateString('vi-VN')} ${d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
  } catch {
    return dateStr;
  }
}

async function loadLogs() {
  isLoading.value = true;
  try {
    const url = props.taskId 
      ? `http://localhost:5000/api/execution/tasks/${props.taskId}/urge-history`
      : 'http://localhost:5000/api/execution/urge-logs';
      
    const res = await fetch(url);
    if (res.ok) {
      logs.value = await res.json();
    }
  } catch (e) {
    console.error('Failed to load urge logs:', e);
  } finally {
    isLoading.value = false;
  }
}

watch(() => [props.isOpen, props.taskId], ([isOpen]) => {
  if (isOpen) {
    loadLogs();
  }
}, { immediate: true });

function close() {
  emit('close');
}
</script>
