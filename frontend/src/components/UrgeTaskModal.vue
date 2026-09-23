<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-5xl sm:max-w-6xl w-full p-6 space-y-5 animate-in fade-in duration-150 font-sans max-h-[92vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div>
          <span class="text-xs font-bold text-rose-600 uppercase tracking-wider block">Thông Báo Tiến Độ Thực Hiện</span>
          <h3 class="text-base font-bold text-slate-800 mt-0.5">{{ taskCode }} - {{ taskTitle }}</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <!-- Main Scrollable Body Container -->
      <div class="flex-1 overflow-y-auto space-y-5 pr-1 custom-scrollbar">

        <!-- Progress Forecast Summary Card -->
        <div class="p-4 bg-rose-50/70 border border-rose-200 rounded-xl space-y-2 shrink-0">
          <div class="flex justify-between items-center text-xs font-bold text-rose-900">
            <span>📊 Đơn Vị Chủ Trì: <span class="font-bold text-slate-900">{{ leadAgencyName || 'Bộ/Ngành' }}</span></span>
            <span class="px-2.5 py-0.5 bg-rose-200 text-rose-900 rounded-full font-bold">Cảnh Báo Chậm Tiến Độ</span>
          </div>

          <div class="grid grid-cols-3 gap-3 text-center pt-1">
            <div class="bg-white p-2.5 rounded-lg border border-rose-100 shadow-2xs">
              <span class="text-[10px] text-slate-400 font-bold block uppercase">Tiến Độ Thực Tế</span>
              <span v-if="!hasReport" class="text-xs font-bold text-amber-700 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 inline-block mt-0.5">Chưa báo cáo</span>
              <span v-else class="text-base font-bold text-rose-700">{{ actualProgressPct }}%</span>
            </div>

            <div class="bg-white p-2.5 rounded-lg border border-rose-100 shadow-2xs">
              <span class="text-[10px] text-slate-400 font-bold block uppercase">Chỉ Tiêu Kế Hoạch</span>
              <span class="text-base font-bold text-blue-700">{{ expectedTargetPct }}%</span>
            </div>

            <div class="bg-white p-2.5 rounded-lg border border-rose-100 shadow-2xs">
              <span class="text-[10px] text-slate-400 font-bold block uppercase">Chậm Kế Hoạch</span>
              <span class="text-base font-bold text-rose-600">-{{ Math.abs(laggingDeltaPct) }}%</span>
            </div>
          </div>
        </div>

        <!-- Section: Past Urge History Logs (If any) -->
        <div v-if="historyLogs.length > 0" class="bg-slate-50 border border-slate-200/90 rounded-xl p-4 space-y-3">
          <div class="flex items-center justify-between">
            <span class="text-xs font-bold text-slate-800 flex items-center gap-1.5 uppercase">
              📜 Lịch Sử Thông Báo Trước Đây (<span class="text-rose-600 font-bold">{{ historyLogs.length }}</span> lần)
            </span>
            <span class="text-[11px] text-slate-500 italic">Bấm "Xem chi tiết" để mở cửa sổ Lịch sử thông báo</span>
          </div>

          <div class="space-y-2 max-h-44 overflow-y-auto pr-1 custom-scrollbar">
            <div 
              v-for="log in historyLogs" 
              :key="log.id"
              class="bg-white p-3 rounded-xl border border-slate-200 hover:border-rose-200 transition shadow-2xs flex items-center justify-between gap-3"
            >
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 text-[11px] text-slate-500 font-semibold">
                  <span class="font-bold text-rose-800">🕒 {{ formatDate(log.createdAt) }}</span>
                  <span>| Người gửi: <strong class="text-slate-700">{{ log.createdBy || 'Lãnh đạo' }}</strong></span>
                </div>
                <div class="text-xs text-slate-800 font-medium truncate mt-0.5">
                  📌 {{ formatSnippet(log.urgeContent) }}
                </div>
              </div>

              <button 
                type="button"
                @click="openUrgeDetailInNewWindow(log.id)"
                class="px-3 py-1.5 bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 rounded-lg text-xs font-bold transition shrink-0 flex items-center gap-1 shadow-2xs"
              >
                👁️ Xem chi tiết
              </button>
            </div>
          </div>
        </div>

        <!-- Section: Draft Urge Content with Rich Text Editor -->
        <div class="space-y-2">
          <label class="text-xs font-bold text-slate-700 uppercase flex items-center justify-between">
            <span>Dự Thảo Văn Bản Thông Báo</span>
          </label>

          <RichTextEditor 
            v-model="form.urgeContent" 
            :showRestoreBtn="true"
            @restoreTemplate="generateDraftText" 
          />
        </div>

      </div>

      <!-- Footer Actions -->
      <div class="flex justify-end gap-3 border-t border-slate-100 pt-3 shrink-0">
        <button type="button" @click="close" class="px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition">
          Hủy
        </button>
        <button 
          type="button" 
          @click="submitUrge"
          :disabled="isSubmitting"
          class="px-5 py-2.5 text-xs font-bold text-white bg-rose-600 hover:bg-rose-700 disabled:opacity-50 rounded-xl shadow-sm transition flex items-center gap-1.5"
        >
          <span v-if="isSubmitting" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          {{ isSubmitting ? 'Đang lưu...' : 'Lưu Văn Bản Thông Báo Vào Lịch Sử' }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import RichTextEditor from './RichTextEditor.vue';
import { getApiUrl } from '../config/api';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  taskId: { type: String, required: true },
  taskCode: { type: String, default: '' },
  taskTitle: { type: String, default: '' },
  leadAgencyName: { type: String, default: '' },
  actualProgressPct: { type: Number, default: 0 },
  expectedTargetPct: { type: Number, default: 100 },
  laggingDeltaPct: { type: Number, default: 0 },
  hasReport: { type: Boolean, default: true }
});

const emit = defineEmits(['close', 'submitted', 'urged']);

const form = ref({
  urgeContent: ''
});

const isSubmitting = ref(false);
const historyLogs = ref([]);

function generateDraftText() {
  const actualText = !props.hasReport ? 'Chưa báo cáo' : `${props.actualProgressPct}%`;
  form.value.urgeContent = `<p><strong>VĂN BẢN THÔNG BÁO TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ</strong></p>
<p><strong>Kính gửi:</strong> ${props.leadAgencyName || 'Đơn vị Chủ trì'}</p>
<p>Căn cứ theo dõi kế hoạch giao chỉ tiêu Chuyển đổi số Quốc gia:</p>
<ul>
  <li><strong>Nhiệm vụ:</strong> ${props.taskCode} - ${props.taskTitle}</li>
  <li><strong>Tiến độ thực tế đạt được:</strong> <span style="color: #e11d48;"><strong>${actualText}</strong></span></li>
  <li><strong>Chỉ tiêu kế hoạch yêu cầu:</strong> <span style="color: #2563eb;"><strong>${props.expectedTargetPct}%</strong></span></li>
  <li><strong>Mức độ chậm tiến độ:</strong> <span style="color: #e11d48;"><strong>-${Math.abs(props.laggingDeltaPct)}%</strong></span></li>
</ul>
<p>Văn phòng Ủy ban Chuyển đổi số Quốc gia đề nghị Đơn vị khẩn trương rà soát các vướng mắc, tập trung nguồn lực đẩy nhanh tiến độ và cập nhật báo cáo tiến độ mới nhất.</p>`;
}

async function loadTaskUrgeHistory() {
  if (!props.taskId) return;
  try {
    const res = await fetch(getApiUrl(`/api/execution/tasks/${props.taskId}/urge-history`));
    if (res.ok) {
      historyLogs.value = await res.json();
    }
  } catch (e) {
    console.error('Failed to load task urge history:', e);
  }
}

function stripHtml(html) {
  if (!html) return '';
  const tmp = document.createElement('div');
  tmp.innerHTML = html;
  return tmp.textContent || tmp.innerText || '';
}

function formatSnippet(content) {
  return stripHtml(content);
}

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    let str = String(dateStr).trim();
    if (str.includes('T') && !str.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(str)) {
      str += 'Z';
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return dateStr;
    return d.toLocaleString('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh' });
  } catch {
    return dateStr;
  }
}

function openUrgeDetailInNewWindow(logId) {
  const url = `${window.location.origin}${window.location.pathname}#urge-history?logId=${logId}`;
  window.open(url, '_blank');
}

watch(() => [props.isOpen, props.taskId], ([isOpen, taskId]) => {
  if (isOpen && taskId) {
    generateDraftText();
    loadTaskUrgeHistory();
  }
}, { immediate: true });

function close() {
  emit('close');
}

async function submitUrge() {
  if (!form.value.urgeContent || !stripHtml(form.value.urgeContent).trim()) {
    toast.error("Vui lòng nhập nội dung văn bản thông báo!");
    return;
  }

  isSubmitting.value = true;
  try {
    const payload = {
      GoalTaskId: props.taskId,
      UrgeContent: form.value.urgeContent,
      ActualProgressPct: props.actualProgressPct,
      ExpectedTargetPct: props.expectedTargetPct,
      LaggingDeltaPct: props.laggingDeltaPct,
      CreatedBy: 'Chuyên viên CĐS'
    };

    const res = await fetch(getApiUrl(`/api/execution/tasks/${props.taskId}/urge`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      const result = await res.json();
      toast.success("Đã lưu văn bản thông báo vào lịch sử thành công!");
      window.dispatchEvent(new CustomEvent('notification-sent'));
      emit('submitted', result);
      emit('urged', result);
      close();
    } else {
      const err = await res.text();
      toast.error("Lỗi khi lưu thông báo: " + err);
    }
  } catch (e) {
    toast.error("Không thể kết nối máy chủ khi lưu thông báo: " + e.message);
  } finally {
    isSubmitting.value = false;
  }
}
</script>
