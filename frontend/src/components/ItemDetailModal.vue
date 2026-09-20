<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-6xl sm:max-w-7xl w-full p-6 space-y-4 font-sans max-h-[92vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-center border-b border-slate-100 pb-2.5 shrink-0">
        <div class="flex items-center gap-2.5">
          <h3 class="text-sm font-semibold text-slate-700">
            Chi tiết {{ item?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }}
          </h3>

        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-lg font-normal p-1 cursor-pointer">✕</button>
      </div>

      <!-- Navigation Tabs -->
      <div class="flex items-center gap-2 border-b border-slate-200 shrink-0">
        <button 
          @click="activeTab = 'info'" 
          :class="['px-4 py-2 text-xs font-semibold transition border-b-2 cursor-pointer', activeTab === 'info' ? 'border-blue-600 text-blue-700 bg-blue-50/50 rounded-t-lg' : 'border-transparent text-slate-500 hover:text-slate-800']"
        >
          📋 Thông Tin Chi Tiết
        </button>
        <button 
          @click="activeTab = 'reports'" 
          :class="['px-4 py-2 text-xs font-semibold transition border-b-2 cursor-pointer flex items-center gap-1.5', activeTab === 'reports' ? 'border-blue-600 text-blue-700 bg-blue-50/50 rounded-t-lg' : 'border-transparent text-slate-500 hover:text-slate-800']"
        >
          <span>📊 Lịch Sử Báo Cáo Tiến Độ</span>
          <span v-if="reportHistory.length > 0" class="px-1.5 py-0.2 bg-blue-100 text-blue-800 rounded-full text-[10px] font-bold">
            {{ reportHistory.length }}
          </span>
        </button>
        <button 
          @click="activeTab = 'notifications'" 
          :class="['px-4 py-2 text-xs font-semibold transition border-b-2 cursor-pointer flex items-center gap-1.5', activeTab === 'notifications' ? 'border-blue-600 text-blue-700 bg-blue-50/50 rounded-t-lg' : 'border-transparent text-slate-500 hover:text-slate-800']"
        >
          <span>🔔 Lịch Sử Gửi Thông Báo</span>
          <span v-if="notificationHistory.length > 0" class="px-1.5 py-0.2 bg-amber-100 text-amber-800 rounded-full text-[10px] font-bold">
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
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Mã Số {{ item?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }}</span>
              <span class="font-semibold text-blue-700 text-xs mt-0.5 block">{{ item?.code || '—' }}</span>
            </div>

            <div>
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Phạm Vi Triển Khai</span>
              <span :class="['px-2.5 py-0.5 rounded-full font-medium text-[11px] inline-block mt-0.5', item?.isGeneralTask ? 'bg-purple-100 text-purple-800' : 'bg-slate-200 text-slate-700']">
                {{ item?.isGeneralTask ? 'Nhiệm vụ chung' : 'Nhiệm vụ riêng' }}
              </span>
            </div>

            <div class="md:col-span-2 border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Tên Chi Tiết</span>
              <p class="font-normal text-slate-800 text-xs leading-relaxed mt-0.5">{{ item?.title || '—' }}</p>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Đơn Vị Chủ Trì</span>
              <span class="font-medium text-slate-800 mt-0.5 block">
                🏛️ {{ item?.leadAgencyName || '—' }}
              </span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Đơn Vị Phối Hợp</span>
              <span class="font-medium text-slate-800 mt-0.5 block">
                🤝 {{ coordinatingNamesDisplay }}
              </span>
            </div>

            <div v-if="item?.itemType === 'Goal'" class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Mục (Phụ Lục QĐ 1266)</span>
              <span class="font-normal text-slate-800 mt-0.5 block">{{ item?.section || '—' }}</span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Nhóm Trọng Tâm</span>
              <span class="font-normal text-slate-800 mt-0.5 block">{{ item?.group || '—' }}</span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Thời Gian Thực Hiện</span>
              <span v-if="item?.isOngoing" class="font-medium text-blue-700 mt-0.5 inline-flex items-center gap-1 bg-blue-50 px-2.5 py-0.5 rounded-full border border-blue-200 text-xs">
                Thường xuyên
              </span>
              <span v-else class="font-normal text-slate-700 mt-0.5 block">
                📅 {{ formatDateRange(item?.startDate, item?.dueDate) }}
              </span>
            </div>

            <div class="border-t border-slate-200/60 pt-2">
              <span class="text-slate-500 font-semibold uppercase block text-[10px]">Trạng Thái Hiện Tại</span>
              <span :class="['px-2.5 py-0.5 rounded-full text-xs font-medium inline-block mt-0.5', getStatusBadgeClass(item?.calculatedStatus)]">
                {{ getStatusLabel(item?.calculatedStatus) }}
              </span>
            </div>

            <!-- Multi-Deliverables Checklist Display -->
            <div v-if="item?.deliverables && item.deliverables.length > 0" class="border-t border-slate-200/80 pt-3 space-y-2">
              <h4 class="text-xs font-extrabold text-blue-900 uppercase flex items-center gap-1.5">
                <span>📋 Danh Mục Sản Phẩm Đầu Ra Dự Kiến ({{ item.deliverables.length }} sản phẩm)</span>
              </h4>
              <div class="space-y-2">
                <div 
                  v-for="(del, idx) in item.deliverables" 
                  :key="idx" 
                  class="bg-slate-50 p-2.5 rounded-xl border border-slate-200/80 space-y-1 text-xs"
                >
                  <div class="flex items-center justify-between font-bold text-slate-800">
                    <span class="text-slate-900 font-extrabold">{{ idx + 1 }}. {{ del.title }}</span>
                    <span :class="['px-2 py-0.5 rounded-full text-[10px] font-black', getDeliverableStatusClass(del.currentStatus)]">
                      {{ getDeliverableStatusLabel(del.currentStatus) }}
                    </span>
                  </div>
                  <div class="flex flex-wrap items-center gap-3 text-[11px] text-slate-600 font-semibold pt-0.5">
                    <span v-if="del.dueDate" class="flex items-center gap-1">
                      📅 Hạn chót: <strong class="text-slate-800">{{ formatDate(del.dueDate) }}</strong>
                    </span>
                    <span v-if="del.documentNumber" class="flex items-center gap-1 text-blue-700 font-bold">
                      📄 Văn bản: <strong>{{ del.documentNumber }}</strong>
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- TAB 2: Report History (2-Column Before vs After Update Layout) -->
        <div v-if="activeTab === 'reports'" class="space-y-4">
          
          <!-- Agency Filter Bar (Shown for Admin or General Tasks when multiple agencies have reported) -->
          <div v-if="(authState.isAdmin.value || item?.isGeneralTask) && availableAgenciesInHistory.length > 1" class="flex flex-wrap items-center justify-between bg-slate-50 p-3 rounded-xl border border-slate-200 gap-2 text-xs">
            <div class="flex items-center gap-2 font-bold text-slate-700">
              <span>🏛️ Lọc theo đơn vị báo cáo:</span>
            </div>
            <select 
              v-model="selectedAgencyFilter" 
              @change="loadHistories" 
              class="text-xs font-bold text-slate-800 bg-white border border-slate-300 rounded-lg px-3 py-1.5 focus:ring-2 focus:ring-blue-500 focus:outline-none cursor-pointer"
            >
              <option value="">Tất cả các đơn vị ({{ rawReportHistory.length }} lượt báo cáo)</option>
              <option v-for="ag in availableAgenciesInHistory" :key="ag.id" :value="ag.id">
                {{ ag.name }} ({{ ag.count }} lượt)
              </option>
            </select>
          </div>

          <LoadingSpinner v-if="isLoadingReports" text="Đang tải lịch sử báo cáo..." padding="py-6" />

          <div v-else-if="reportHistory.length === 0" class="py-10 text-center bg-slate-50 rounded-2xl border border-slate-200 text-slate-400 text-xs font-semibold italic">
            📭 Chưa có lượt báo cáo tiến độ nào được ghi nhận cho nhiệm vụ này.
          </div>

          <div v-else class="space-y-4">
            <div 
              v-for="(rep, idx) in reportHistory" 
              :key="rep.progressLogId || rep.id || idx"
              class="bg-white border border-slate-200 rounded-2xl p-4 space-y-3 text-xs shadow-xs hover:border-blue-300 transition"
            >
              <!-- History Log Card Header -->
              <div class="flex flex-wrap justify-between items-center border-b border-slate-100 pb-2.5 gap-2">
                <div class="flex items-center gap-2">
                  <span class="px-2.5 py-0.5 bg-blue-600 text-white rounded-md font-extrabold text-[11px]">
                    Lần {{ reportHistory.length - idx }}
                  </span>
                  <span class="px-2 py-0.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-md font-bold text-[11px]">
                    Kỳ: {{ formatPeriodLabel(rep) }}
                  </span>
                  <span class="text-slate-500 font-semibold text-[11px]">
                    🕒 {{ formatDate(rep.logDate) }}
                  </span>
                </div>
                <div class="text-slate-600 font-semibold text-xs">
                  ✍️ Cán bộ báo cáo: <strong class="text-slate-900 font-bold">{{ rep.createdBy || 'Đơn vị chủ trì' }}</strong>
                </div>
              </div>

              <!-- Single Clean Updated Report Info Card -->
              <div class="bg-blue-50/40 border border-blue-200/80 rounded-xl p-3.5 space-y-2.5">
                <!-- Quantitative value or qualitative status after update -->
                <div class="space-y-1">
                  <span class="text-[10px] font-bold text-blue-700 uppercase block">Trạng Thái / Tiến Độ Ghi Nhận</span>
                  <div class="font-black text-blue-900 text-sm">
                    {{ formatReportProgressValue(rep) }}
                  </div>
                </div>

                <!-- Deliverables status after update -->
                <div v-if="rep.deliverables && rep.deliverables.length > 0" class="space-y-1 pt-1.5 border-t border-blue-100">
                  <span class="text-[10px] font-bold text-blue-800 uppercase block">📋 Tiến Độ Danh Mục Sản Phẩm Đầu Ra ({{ rep.deliverables.length }})</span>
                  <div class="space-y-1 max-h-44 overflow-y-auto pr-1 custom-scrollbar">
                    <div 
                      v-for="(del, dIdx) in rep.deliverables" 
                      :key="dIdx"
                      class="bg-white p-2 rounded-lg border border-blue-100 text-[11px] space-y-0.5 shadow-2xs"
                    >
                      <div class="flex justify-between font-bold text-slate-900">
                        <span class="truncate pr-1">{{ dIdx + 1 }}. {{ del.title }}</span>
                        <span :class="['px-1.5 py-0.2 rounded text-[10px] shrink-0 font-bold', getDeliverableStatusClass(del.currentStatus)]">
                          {{ getDeliverableStatusLabel(del.currentStatus) }}
                        </span>
                      </div>
                      <div v-if="del.documentNumber" class="text-[10px] text-blue-800 font-semibold">
                        📄 Số VB: <strong class="text-blue-950 font-bold">{{ del.documentNumber }}</strong>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Notes / Explanation after update -->
                <div v-if="rep.summaryNotes || rep.notes" class="bg-white p-2.5 rounded-lg border border-blue-100 text-[11px] text-slate-800 font-medium leading-relaxed">
                  📌 <strong>Ghi chú / Giải trình:</strong> {{ rep.summaryNotes || rep.notes }}
                </div>

                <!-- Evidence file attachments -->
                <div v-if="rep.attachmentFileUrls && rep.attachmentFileUrls.length > 0" class="pt-1 space-y-1">
                  <span class="text-[10px] font-bold text-purple-900 uppercase block">📄 File Minh Chứng Đính Kèm ({{ rep.attachmentFileUrls.length }})</span>
                  <div class="space-y-1">
                    <div v-for="(fileUrl, fIdx) in rep.attachmentFileUrls" :key="fIdx" class="flex items-center justify-between text-[11px] bg-white p-1.5 rounded-lg border border-purple-100">
                      <a 
                        :href="getApiUrl(fileUrl)" 
                        target="_blank" 
                        class="text-purple-700 hover:underline font-semibold truncate flex items-center gap-1 min-w-0 flex-1"
                      >
                        <span>📎</span>
                        <span class="truncate">{{ formatFileName(fileUrl) }}</span>
                      </a>
                      <a 
                        :href="getApiUrl(fileUrl)" 
                        target="_blank" 
                        download
                        class="text-[10px] font-bold text-purple-700 bg-purple-50 hover:bg-purple-600 hover:text-white px-2 py-0.5 rounded transition cursor-pointer shrink-0 ml-1"
                      >
                        Tải về
                      </a>
                    </div>
                  </div>
                </div>
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
              <div class="flex flex-wrap items-center justify-between border-b border-amber-200/70 pb-2 gap-2">
                <div class="flex items-center gap-2">
                  <span class="px-2.5 py-0.5 bg-amber-200 text-amber-900 rounded-full font-black">
                    🔔 Thông Báo
                  </span>
                  <span class="text-slate-600 font-bold">
                    🕒 {{ formatDate(notif.createdAt) }}
                  </span>
                </div>
                <span class="text-slate-600 font-bold">
                  ✍️ Người gửi: <strong class="text-slate-900">{{ notif.createdBy || 'Chuyên viên CĐS' }}</strong>
                </span>
              </div>

              <div v-if="notif.title" class="text-xs text-amber-950 bg-amber-100/60 px-3 py-2 rounded-lg font-bold border border-amber-200/80 flex items-start gap-1.5 leading-snug">
                <span class="shrink-0 text-amber-900">📌 <strong>Tiêu đề:</strong></span>
                <span class="font-extrabold text-slate-900">{{ notif.title }}</span>
              </div>

              <div v-if="notif.recipientsSummary" class="text-[11px] text-amber-950 bg-amber-100/70 px-3 py-1.5 rounded-lg font-bold flex items-start gap-1.5 leading-snug">
                <span class="shrink-0">👥 <strong>Đầu mối / Người nhận:</strong></span>
                <span>{{ notif.recipientsSummary }}</span>
              </div>

              <div class="prose max-w-none text-xs text-slate-800 bg-white p-3 rounded-lg border border-amber-100" v-html="notif.urgeContent"></div>
            </div>
          </div>
        </div>

      </div>

      <!-- Modal Footer -->
      <div class="flex items-center justify-between border-t border-slate-100 pt-3 shrink-0">
        <button 
          v-if="canEditItem(item)"
          type="button" 
          @click="onEditClick" 
          class="px-4 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition cursor-pointer flex items-center gap-1.5 shadow-2xs"
        >
          ✏️ Chỉnh Sửa {{ item?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }}
        </button>
        <div v-else></div>
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
import { authState } from '../services/auth';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  item: { type: Object, default: null },
  initialTab: { type: String, default: 'info' }
});

const emit = defineEmits(['close', 'edit']);

function onEditClick() {
  close();
  emit('edit', props.item);
}

function canEditItem(item) {
  if (!item) return false;
  if (!authState.isAdmin.value) return false;

  const st = item.calculatedStatus || item.status;
  if (st && st !== 'NotStarted' && st !== '1. Chưa thực hiện') {
    return false;
  }

  if (reportHistory.value && reportHistory.value.length > 0) {
    return false;
  }

  if (item.progressLogs && item.progressLogs.length > 0) {
    return false;
  }

  return true;
}

const activeTab = ref('info');
const reportHistory = ref([]);
const rawReportHistory = ref([]);
const availableAgenciesInHistory = ref([]);
const selectedAgencyFilter = ref('');
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

function formatPeriodLabel(rep) {
  if (!rep) return 'Toàn thời gian';
  if (rep.periodQuarter && rep.periodQuarter > 0) {
    return `Quý ${rep.periodQuarter}/${rep.periodYear || rep.year || 2026}`;
  }
  return `Năm ${rep.periodYear || rep.year || 2026}`;
}

function formatReportProgressValue(rep) {
  if (!rep) return '—';
  const unitName = props.item?.unitName || props.item?.unit?.name || '%';
  const val = (rep.actualValue !== null && rep.actualValue !== undefined)
    ? rep.actualValue
    : (rep.completionPercentage !== null && rep.completionPercentage !== undefined ? rep.completionPercentage : null);

  if (val !== null && val !== undefined) {
    if (unitName === 'Số lượng') {
      return `${val}`;
    }
    return `${val}%`;
  }

  if (rep.status) {
    return getStatusLabel(rep.status);
  }

  return '—';
}

function formatFileName(fullPath) {
  if (!fullPath) return 'File minh chứng';
  const rawFileName = fullPath.split(/[/\\]/).pop() || fullPath;
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}_/i;
  return rawFileName.replace(guidRegex, '');
}

function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? formatDateOnly(sDate) : '...';
  const d = dDate ? formatDateOnly(dDate) : '...';
  return `${s} ➔ ${d}`;
}

function formatDateOnly(dateStr) {
  if (!dateStr) return '—';
  try {
    let str = String(dateStr).trim();
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
      const [y, m, d] = str.slice(0, 10).split('-');
      return `${d}/${m}/${y}`;
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return dateStr;
    const day = String(d.getUTCDate()).padStart(2, '0');
    const month = String(d.getUTCMonth() + 1).padStart(2, '0');
    const year = d.getUTCFullYear();
    return `${day}/${month}/${year}`;
  } catch {
    return dateStr;
  }
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

function getDeliverableStatusLabel(st) {
  const map = {
    'NotStarted': 'Chưa thực hiện',
    'Drafting': 'Đang soạn thảo',
    'Reviewing': 'Đang trình/Thẩm định',
    'Submitted': 'Đã trình ban hành',
    'Completed': 'Đã hoàn thành/Ban hành'
  };
  return map[st] || st || 'Chưa thực hiện';
}

function getDeliverableStatusClass(st) {
  const map = {
    'NotStarted': 'bg-slate-100 text-slate-600',
    'Drafting': 'bg-sky-100 text-sky-800',
    'Reviewing': 'bg-amber-100 text-amber-800',
    'Submitted': 'bg-indigo-100 text-indigo-800',
    'Completed': 'bg-emerald-100 text-emerald-800'
  };
  return map[st] || 'bg-slate-100 text-slate-600';
}

async function loadHistories() {
  if (!props.item) return;
  const taskId = props.item.taskId || props.item.id;
  if (!taskId) return;

  // 1. Fetch Report History
  isLoadingReports.value = true;
  try {
    // Admin reads all agencies' progress history unless a specific filter is selected
    // Non-admin agency users default to their own agencyId
    let userAgId = authState.isAdmin.value 
      ? selectedAgencyFilter.value 
      : (selectedAgencyFilter.value || authState.user.value?.agencyId || '');

    const queryStr = userAgId ? `?agencyId=${userAgId}` : '';
    const res = await fetch(getApiUrl(`/api/execution/tasks/${taskId}/progress-history${queryStr}`));
    if (res.ok) {
      const data = await res.json();
      reportHistory.value = data;

      // Extract available unique agencies for the filter dropdown if viewing all
      if (!selectedAgencyFilter.value) {
        rawReportHistory.value = data;
        const agencyMap = new Map();
        data.forEach(rep => {
          if (rep.agencyId) {
            const agName = rep.createdBy || 'Đơn vị báo cáo';
            if (!agencyMap.has(rep.agencyId)) {
              agencyMap.set(rep.agencyId, { id: rep.agencyId, name: agName, count: 1 });
            } else {
              agencyMap.get(rep.agencyId).count++;
            }
          }
        });
        availableAgenciesInHistory.value = Array.from(agencyMap.values());
      }
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

watch(() => [props.isOpen, props.item, props.initialTab], ([isOpen, item, tab]) => {
  if (isOpen && item) {
    selectedAgencyFilter.value = '';
    activeTab.value = tab || 'info';
    loadHistories();
  }
}, { immediate: true });

function close() {
  emit('close');
}
</script>
