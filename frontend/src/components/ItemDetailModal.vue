<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl w-full p-6 space-y-4 font-sans max-h-[92vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div class="flex items-center gap-3">
          <span class="px-2.5 py-1 bg-blue-600 text-white rounded-lg text-xs font-black shadow-sm">
            {{ item?.code || 'MÃ' }}
          </span>
          <div>
            <h3 class="text-base font-extrabold text-slate-800 leading-snug">
              {{ item?.title || 'Chi tiết Mục Tiêu / Nhiệm Vụ' }}
            </h3>
            <span class="text-xs text-slate-500 font-semibold">
              {{ item?.itemType === 'Goal' ? 'Mục Tiêu Chiến Lược' : 'Nhiệm Vụ Thực Hiện' }} - Quyết định 1266/QĐ-TTg
            </span>
          </div>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1 cursor-pointer">✕</button>
      </div>

      <!-- Navigation Tabs -->
      <div class="flex items-center gap-2 border-b border-slate-200 shrink-0">
        <button 
          @click="activeTab = 'info'" 
          :class="['px-4 py-2 text-xs font-extrabold transition border-b-2 cursor-pointer', activeTab === 'info' ? 'border-blue-600 text-blue-700 bg-blue-50/50 rounded-t-lg' : 'border-transparent text-slate-500 hover:text-slate-800']"
        >
          📋 Thông Tin Chi Tiết
        </button>
        <button 
          @click="activeTab = 'reports'" 
          :class="['px-4 py-2 text-xs font-extrabold transition border-b-2 cursor-pointer flex items-center gap-1.5', activeTab === 'reports' ? 'border-blue-600 text-blue-700 bg-blue-50/50 rounded-t-lg' : 'border-transparent text-slate-500 hover:text-slate-800']"
        >
          <span>📊 Lịch Sử Báo Cáo Tiến Độ</span>
          <span v-if="reportHistory.length > 0" class="px-1.5 py-0.2 bg-blue-100 text-blue-800 rounded-full text-[10px] font-black">
            {{ reportHistory.length }}
          </span>
        </button>
        <button 
          @click="activeTab = 'notifications'" 
          :class="['px-4 py-2 text-xs font-extrabold transition border-b-2 cursor-pointer flex items-center gap-1.5', activeTab === 'notifications' ? 'border-blue-600 text-blue-700 bg-blue-50/50 rounded-t-lg' : 'border-transparent text-slate-500 hover:text-slate-800']"
        >
          <span>🔔 Lịch Sử Gửi Thông Báo</span>
          <span v-if="notificationHistory.length > 0" class="px-1.5 py-0.2 bg-amber-100 text-amber-800 rounded-full text-[10px] font-black">
            {{ notificationHistory.length }}
          </span>
        </button>
      </div>

      <!-- Scrollable Tab Content Container -->
      <div class="flex-1 overflow-y-auto pr-1 custom-scrollbar space-y-4">

        <!-- TAB 1: General Info -->
        <div v-if="activeTab === 'info'" class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3.5 bg-slate-50/80 p-4 rounded-2xl border border-slate-200 text-xs">
            <div>
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Mã Số Nhiệm Vụ</span>
              <span class="font-extrabold text-blue-800 text-sm">{{ item?.code || '—' }}</span>
            </div>

            <div>
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Phạm Vi Triển Khai</span>
              <span :class="['px-2.5 py-0.5 rounded-full font-extrabold text-[11px] inline-block mt-0.5', item?.isGeneralTask ? 'bg-purple-100 text-purple-800' : 'bg-slate-200 text-slate-700']">
                {{ item?.isGeneralTask ? 'Nhiệm vụ chung' : 'Nhiệm vụ riêng' }}
              </span>
            </div>

            <div class="md:col-span-2 border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Tên Chi Tiết</span>
              <p class="font-bold text-slate-900 text-xs leading-relaxed mt-0.5">{{ item?.title || '—' }}</p>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Đơn Vị Chủ Trì</span>
              <span class="font-extrabold text-slate-800 mt-0.5 block">
                🏛️ {{ item?.leadAgencyName || '—' }} <span v-if="item?.leadAgencyCode">({{ item.leadAgencyCode }})</span>
              </span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Đơn Vị Phối Hợp</span>
              <span class="font-extrabold text-slate-800 mt-0.5 block">
                🤝 {{ coordinatingNamesDisplay }}
              </span>
            </div>

            <div v-if="item?.itemType === 'Goal'" class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Mục (Phụ Lục QĐ 1266)</span>
              <span class="font-bold text-slate-800 mt-0.5 block">{{ item?.section || '—' }}</span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Nhóm Trọng Tâm</span>
              <span class="font-bold text-slate-800 mt-0.5 block">{{ item?.group || '—' }}</span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Thời Gian Thực Hiện</span>
              <span class="font-bold text-slate-700 mt-0.5 block">
                📅 {{ formatDateRange(item?.startDate, item?.dueDate) }}
              </span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-400 font-bold uppercase block text-[10px]">Trạng Thái Hiện Tại</span>
              <span :class="['px-2.5 py-0.5 rounded-full text-xs font-bold inline-block mt-0.5', getStatusBadgeClass(item?.calculatedStatus)]">
                {{ getStatusLabel(item?.calculatedStatus) }}
              </span>
            </div>
          </div>
        </div>

        <!-- TAB 2: Report History -->
        <div v-if="activeTab === 'reports'" class="space-y-3">
          <LoadingSpinner v-if="isLoadingReports" text="Đang tải lịch sử báo cáo..." padding="py-6" />

          <div v-else-if="reportHistory.length === 0" class="py-10 text-center bg-slate-50 rounded-2xl border border-slate-200 text-slate-400 text-xs font-semibold italic">
            📭 Chưa có lượt báo cáo tiến độ nào được ghi nhận cho nhiệm vụ này.
          </div>

          <div v-else class="space-y-2.5">
            <div 
              v-for="rep in reportHistory" 
              :key="rep.progressLogId || rep.id"
              class="bg-slate-50/90 border border-slate-200 rounded-xl p-3.5 space-y-2 text-xs hover:border-blue-300 transition"
            >
              <div class="flex justify-between items-center border-b border-slate-200/80 pb-2">
                <div class="flex items-center gap-2">
                  <span class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded font-black">
                    Kỳ Báo Cáo Quý {{ rep.quarter || rep.periodQuarter || 1 }}/{{ rep.year || 2026 }}
                  </span>
                  <span class="text-slate-500 font-semibold">
                    🕒 {{ formatDate(rep.logDate) }}
                  </span>
                </div>
                <span class="text-slate-600 font-bold">
                  Người báo cáo: <strong class="text-slate-800">{{ rep.createdBy || 'Đơn vị chủ trì' }}</strong>
                </span>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 pt-1">
                <div>
                  <span class="text-slate-400 font-bold block text-[10px] uppercase">Kết Quả Tiến Độ</span>
                  <span v-if="rep.actualValue !== null && rep.actualValue !== undefined" class="font-black text-blue-700 text-sm">
                    {{ rep.actualValue }} %
                  </span>
                  <span v-else class="font-bold text-slate-800 text-xs">
                    {{ rep.qualitativeStatus || 'Đã cập nhật báo cáo' }}
                  </span>
                </div>

                <div v-if="rep.attachmentFileUrl">
                  <span class="text-slate-400 font-bold block text-[10px] uppercase">File Minh Chứng Đính Kèm</span>
                  <a 
                    :href="getApiUrl(rep.attachmentFileUrl)" 
                    target="_blank" 
                    class="inline-flex items-center gap-1 text-blue-600 hover:text-blue-800 font-extrabold text-xs underline mt-0.5"
                  >
                    📎 {{ rep.attachmentFileName || 'Tải file minh chứng' }}
                  </a>
                </div>
              </div>

              <div v-if="rep.notes" class="bg-white p-2.5 rounded-lg border border-slate-200 text-slate-700 font-medium">
                📌 <strong>Ghi chú / Nội dung:</strong> {{ rep.notes }}
              </div>
            </div>
          </div>
        </div>

        <!-- TAB 3: Notifications / Urge History -->
        <div v-if="activeTab === 'notifications'" class="space-y-3">
          <LoadingSpinner v-if="isLoadingNotifications" text="Đang tải lịch sử gửi thông báo..." padding="py-6" />

          <div v-else-if="notificationHistory.length === 0" class="py-10 text-center bg-slate-50 rounded-2xl border border-slate-200 text-slate-400 text-xs font-semibold italic">
            🔕 Chưa có đợt gửi thông báo nào đối với nhiệm vụ này.
          </div>

          <div v-else class="space-y-3">
            <div 
              v-for="notif in notificationHistory" 
              :key="notif.id"
              class="bg-amber-50/50 border border-amber-200 rounded-xl p-4 space-y-2 text-xs"
            >
              <div class="flex justify-between items-center border-b border-amber-200/70 pb-2">
                <div class="flex items-center gap-2">
                  <span class="px-2.5 py-0.5 bg-amber-200 text-amber-900 rounded-full font-black">
                    🔔 Thông Báo
                  </span>
                  <span class="text-slate-600 font-bold">
                    🕒 {{ formatDate(notif.createdAt) }}
                  </span>
                </div>
                <span class="text-slate-600 font-bold">
                  Người gửi: <strong class="text-slate-800">{{ notif.createdBy || 'Chuyên viên CĐS' }}</strong>
                </span>
              </div>

              <div class="prose max-w-none text-xs text-slate-800 bg-white p-3 rounded-lg border border-amber-100" v-html="notif.urgeContent"></div>
            </div>
          </div>
        </div>

      </div>

      <!-- Modal Footer -->
      <div class="flex justify-end border-t border-slate-100 pt-3 shrink-0">
        <button 
          type="button" 
          @click="close" 
          class="px-5 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition cursor-pointer"
        >
          Đóng
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import LoadingSpinner from './LoadingSpinner.vue';
import { getApiUrl } from '../config/api';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  item: { type: Object, default: null }
});

const emit = defineEmits(['close']);

const activeTab = ref('info');
const reportHistory = ref([]);
const notificationHistory = ref([]);
const isLoadingReports = ref(false);
const isLoadingNotifications = ref(false);

const coordinatingNamesDisplay = computed(() => {
  if (!props.item) return '—';
  if (props.item.coordinatingAgencyNames && props.item.coordinatingAgencyNames.length > 0) {
    return props.item.coordinatingAgencyNames.join(', ');
  }
  if (props.item.coordinatingAgencyCodes && props.item.coordinatingAgencyCodes.length > 0) {
    return props.item.coordinatingAgencyCodes.join(', ');
  }
  return '—';
});

function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? new Date(sDate).toLocaleDateString('vi-VN') : '...';
  const d = dDate ? new Date(dDate).toLocaleDateString('vi-VN') : '...';
  return `${s} ➔ ${d}`;
}

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

function getStatusLabel(st) {
  const map = {
    'NotStarted': '1. Chưa thực hiện',
    'InProgressOnTime': '2. Đang thực hiện (trong hạn)',
    'InProgressOverdue': '3. Đang thực hiện (quá hạn)',
    'CompletedOnTime': '4. Hoàn thành (đúng hạn)',
    'CompletedOverdue': '5. Hoàn thành (quá hạn)',
    'ExpiringSoon': '6. Sắp hết hạn'
  };
  return map[st] || st || 'Chưa thực hiện';
}

function getStatusBadgeClass(st) {
  const map = {
    'NotStarted': 'bg-slate-100 text-slate-700 border border-slate-200',
    'InProgressOnTime': 'bg-blue-50 text-blue-800 border border-blue-200',
    'InProgressOverdue': 'bg-rose-50 text-rose-800 border border-rose-200',
    'CompletedOnTime': 'bg-emerald-50 text-emerald-800 border border-emerald-200',
    'CompletedOverdue': 'bg-teal-50 text-teal-800 border border-teal-200',
    'ExpiringSoon': 'bg-amber-50 text-amber-800 border border-amber-200'
  };
  return map[st] || 'bg-slate-100 text-slate-700';
}

async function loadHistories() {
  if (!props.item) return;
  const taskId = props.item.taskId || props.item.id;
  if (!taskId) return;

  // 1. Fetch Report History
  isLoadingReports.value = true;
  try {
    const res = await fetch(getApiUrl(`/api/execution/tasks/${taskId}/progress-history`));
    if (res.ok) {
      reportHistory.value = await res.json();
    } else {
      reportHistory.value = [];
    }
  } catch (e) {
    reportHistory.value = [];
  } finally {
    isLoadingReports.value = false;
  }

  // 2. Fetch Notification / Urge History
  isLoadingNotifications.value = true;
  try {
    const res = await fetch(getApiUrl(`/api/execution/tasks/${taskId}/urge-history`));
    if (res.ok) {
      notificationHistory.value = await res.json();
    } else {
      notificationHistory.value = [];
    }
  } catch (e) {
    notificationHistory.value = [];
  } finally {
    isLoadingNotifications.value = false;
  }
}

watch(() => [props.isOpen, props.item], ([isOpen, item]) => {
  if (isOpen && item) {
    activeTab.value = 'info';
    loadHistories();
  }
}, { immediate: true });

function close() {
  emit('close');
}
</script>
