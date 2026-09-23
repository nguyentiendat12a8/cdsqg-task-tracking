<template>
  <div class="w-full space-y-6 font-sans">
    
    <!-- Top Header -->
    <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 flex flex-col md:flex-row md:items-center justify-between gap-4 w-full">
      <div>
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-rose-600 text-white rounded-xl shadow-sm font-bold text-base">
            ⚡
          </span>
          <h1 class="text-xl font-bold text-slate-800">Lịch Sử Thông Báo & Chỉ Đạo Tiến Độ</h1>
        </div>
        <p class="text-xs text-slate-500 mt-1">Tổng hợp nhật ký chỉ đạo, thông báo nhiệm vụ từ Lãnh đạo cấp cao</p>
      </div>

      <div class="flex items-center gap-2">
        <button 
          @click="exportExcel"
          class="px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer shadow-xs"
          title="Xuất lịch sử thông báo ra file Excel"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          Xuất Excel
        </button>

        <button 
          @click="loadLogs"
          class="px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer"
        >
          🔄 Làm Mới
        </button>
      </div>
    </div>

    <!-- Stats Summary Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 w-full">
      <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-rose-100 text-rose-700 flex items-center justify-center font-bold text-lg">
          📜
        </div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase block">Tổng Văn Bản Thông Báo</span>
          <span class="text-xl font-bold text-slate-800">{{ logs.length }}</span>
        </div>
      </div>

      <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-purple-100 text-purple-700 flex items-center justify-center font-bold text-lg">
          🏢
        </div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase block">Cơ Quan Nhận Thông Báo</span>
          <span class="text-xl font-bold text-slate-800">{{ uniqueAgenciesCount }}</span>
        </div>
      </div>

      <div class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-emerald-100 text-emerald-700 flex items-center justify-center font-bold text-lg">
          🕒
        </div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase block">Lần Thông Báo Gần Nhất</span>
          <span class="text-xs font-bold text-slate-800">{{ latestLogDate }}</span>
        </div>
      </div>
    </div>

    <!-- Unified Search, List & Pagination Card Block -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden w-full flex flex-col">
      
      <!-- 1. Top Header: Search & Advanced Filter Bar -->
      <div class="p-3 border-b border-slate-200/80 bg-slate-50/60 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 w-full">
        <div class="flex items-center gap-2 flex-1 max-w-xl">
          <!-- Text Search Input -->
          <div class="relative flex-1">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              :value="searchQueryDraft" 
              @input="searchQueryDraft = $event.target.value"
              placeholder="Tìm theo mã, tên nhiệm vụ, nội dung..." 
              class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-rose-500 focus:outline-none h-[34px]"
            />
          </div>

          <!-- OverlayPanel Advanced Filter Popover -->
          <OverlayPanel
            title="Lọc Lịch Sử Thông Báo"
            buttonText="Lọc Nâng Cao"
            :activeCount="activeFilterCount"
            widthClass="w-[340px] sm:w-[460px]"
            @apply="execSearch"
            @reset="resetSearch"
          >
            <div class="space-y-3">
              <div>
                <SearchableSelect 
                  v-model="selectedAgencyIds" 
                  :options="leadAgencyOptions" 
                  :isMulti="true" 
                  label="Cơ Quan Chủ Trì" 
                  placeholder="Tất cả cơ quan chủ trì"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="selectedSubAgencyIds" 
                  :options="subAgencyOptions" 
                  :isMulti="true" 
                  label="Đơn Vị Trực Thuộc" 
                  placeholder="Tất cả đơn vị trực thuộc"
                />
              </div>

              <div>
                <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wider block mb-1">Khoảng Thời Gian Gửi</label>
                <div class="grid grid-cols-2 gap-2">
                  <div>
                    <label class="text-[10px] font-bold text-slate-400 block mb-0.5">Từ ngày</label>
                    <DatePicker v-model="fromDate" placeholder="dd/mm/yyyy" />
                  </div>
                  <div>
                    <label class="text-[10px] font-bold text-slate-400 block mb-0.5">Đến ngày</label>
                    <DatePicker v-model="toDate" placeholder="dd/mm/yyyy" />
                  </div>
                </div>
              </div>
            </div>
          </OverlayPanel>
        <span class="text-xs font-bold text-slate-500 shrink-0">
          Hiển thị {{ logs.length }} / {{ totalCount }} nhật ký thông báo
        </span>
      </div>

      <!-- 2. Middle Body: Main Urge Logs List Table -->
      <div class="overflow-y-auto max-h-[calc(100vh-420px)] custom-scrollbar flex-1 bg-white">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh sách lịch sử thông báo từ máy chủ..." />

        <div v-else-if="logs.length === 0" class="p-12 text-center text-xs font-semibold text-slate-400 italic space-y-2">
          <div>Chưa phát sinh nhật ký thông báo nào phù hợp từ khóa và bộ lọc tìm kiếm.</div>
          <button @click="resetSearch" class="px-3 py-1.5 bg-rose-50 text-rose-700 hover:bg-rose-100 rounded-lg text-xs font-bold border border-rose-200 transition not-italic">
            🔄 Đặt lại tất cả bộ lọc
          </button>
        </div>

        <div v-else class="divide-y divide-slate-100">
          <div 
            v-for="log in logs" 
            :key="log.id" 
            class="p-5 hover:bg-slate-50/80 transition space-y-3"
          >
            <!-- Header info row -->
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-100 pb-2">
              <div class="flex items-center gap-2">
                <span class="px-2.5 py-1 bg-rose-100 text-rose-800 font-bold text-xs rounded-lg shadow-2xs">
                  {{ log.taskCode }}
                </span>
                <h3 class="text-sm font-bold text-slate-800">{{ log.taskTitle }}</h3>
              </div>

              <div class="text-[11px] text-slate-400 font-semibold shrink-0">
                🕒 {{ formatDate(log.createdAt) }} | Người chỉ đạo: <span class="text-slate-700 font-bold">{{ log.createdBy || 'Lãnh đạo' }}</span>
              </div>
            </div>

            <!-- Urge Directive Text Content -->
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-slate-50 p-3.5 rounded-xl border border-slate-200/80">
              <div class="text-xs text-slate-800 font-medium leading-relaxed truncate flex-1 min-w-0">
                📌 <strong class="text-rose-900">Nội dung chỉ đạo:</strong> {{ formatSnippet(log.urgeContent) }}
              </div>

              <button 
                @click="openDetail(log)"
                class="px-3.5 py-1.5 bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs rounded-xl shadow-xs transition shrink-0 flex items-center gap-1 cursor-pointer"
              >
                👁️ Xem chi tiết
              </button>
            </div>

            <!-- Agency & Completion Status -->
            <div class="flex items-center justify-between text-xs font-bold pt-1">
              <div class="flex items-center gap-2">
                <span class="text-slate-500">Cơ quan nhận thông báo:</span>
                <span class="px-2.5 py-0.5 bg-blue-50 text-blue-800 rounded-lg border border-blue-200">
                  🏢 {{ log.leadAgencyName || log.leadAgencyCode || 'Cơ quan chủ trì' }}
                </span>
              </div>

              <span class="text-emerald-700 bg-emerald-50 border border-emerald-200 px-3 py-1 rounded-lg text-[11px] font-bold">
                ✓ Đã lưu lịch sử CSDL
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 3. Bottom Footer: Server Pagination Controls -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div>
          Hiển thị <span class="font-bold text-slate-900">{{ totalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, totalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ totalCount }}</span> văn bản thông báo
        </div>

        <div class="flex items-center gap-2">
          <button 
            @click="changePage(currentPage - 1)" 
            :disabled="currentPage <= 1"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer disabled:cursor-not-allowed"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-rose-50 text-rose-800 border border-rose-200 rounded-xl font-bold">
            Trang {{ currentPage }} / {{ totalPages }}
          </span>

          <button 
            @click="changePage(currentPage + 1)" 
            :disabled="currentPage >= totalPages"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer disabled:cursor-not-allowed"
          >
            Trang sau ›
          </button>
        </div>
      </div>

    </div>

    <!-- Detail Popup Modal -->
    <UrgeDetailModal 
      :isOpen="isDetailModalOpen"
      :log="selectedLog"
      @close="isDetailModalOpen = false"
    />

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import { exportToExcel } from '../utils/excelExport';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import UrgeDetailModal from '../components/UrgeDetailModal.vue';
import OverlayPanel from '../components/OverlayPanel.vue';
import DatePicker from '../components/DatePicker.vue';
import { getApiUrl } from '../config/api';

const STORAGE_KEY = 'cdsqg_urge_history_query';

const logs = ref([]);
const agencies = ref([]);
const isLoading = ref(true);

const searchQueryDraft = ref('');
const appliedSearchQuery = ref('');

const selectedAgencyIds = ref([]);
const appliedAgencyIds = ref([]);
const selectedSubAgencyIds = ref([]);
const appliedSubAgencyIds = ref([]);

const leadAgencyOptions = computed(() => {
  return agencies.value
    .filter(ag => ag.code === 'ALL_AGENCIES' || (ag.type !== 3 && ag.type !== 4 && ag.type !== 'Other' && !ag.parentId))
    .map(ag => {
      if (ag.code === 'ALL_AGENCIES') {
        return { value: ag.id, label: `🌐 ${ag.name} (Tất cả đơn vị)` };
      }
      return { value: ag.id, label: ag.name };
    });
});

const subAgencyOptions = computed(() => {
  return agencies.value
    .filter(ag => ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000' && ag.type !== 4 && ag.type !== 'Other')
    .map(ag => {
      const parentAg = agencies.value.find(p => p.id === ag.parentId);
      return {
        value: ag.id,
        label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name
      };
    });
});

const fromDate = ref('');
const appliedFromDate = ref('');

const toDate = ref('');
const appliedToDate = ref('');

const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedAgencyIds.value?.length) count++;
  if (selectedSubAgencyIds.value?.length) count++;
  if (fromDate.value || toDate.value) count++;
  return count;
});

const currentPage = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const selectedLog = ref(null);
const isDetailModalOpen = ref(false);

function stripHtml(html) {
  if (!html) return '';
  const tmp = document.createElement('div');
  tmp.innerHTML = html;
  return tmp.textContent || tmp.innerText || '';
}

function formatSnippet(content) {
  return stripHtml(content);
}

function openDetail(log) {
  selectedLog.value = log;
  isDetailModalOpen.value = true;
}

function loadSavedQuery() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      searchQueryDraft.value = saved;
      appliedSearchQuery.value = saved;
    }
  } catch (e) {
    console.error('Error reading localStorage:', e);
  }
}

async function loadAgencies() {
  try {
    const res = await fetch(getApiUrl('/api/agencies'));
    if (res.ok) {
      const data = await res.json();
      agencies.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (e) {
    console.error('Failed to load agencies:', e);
  }
}

let urgeFetchRequestId = 0;
let urgeSearchDebounceTimer = null;

function execSearch() {
  if (urgeSearchDebounceTimer) clearTimeout(urgeSearchDebounceTimer);
  urgeFetchRequestId++;
  appliedSearchQuery.value = searchQueryDraft.value;
  appliedAgencyIds.value = [...selectedAgencyIds.value];
  appliedSubAgencyIds.value = [...selectedSubAgencyIds.value];
  appliedFromDate.value = fromDate.value;
  appliedToDate.value = toDate.value;
  currentPage.value = 1;
  try {
    localStorage.setItem(STORAGE_KEY, appliedSearchQuery.value);
  } catch (e) {
    console.error('Error saving to localStorage:', e);
  }
  loadLogs();
}

watch(searchQueryDraft, () => {
  if (urgeSearchDebounceTimer) clearTimeout(urgeSearchDebounceTimer);
  urgeSearchDebounceTimer = setTimeout(() => {
    execSearch();
  }, 300);
});

function resetSearch() {
  if (urgeSearchDebounceTimer) clearTimeout(urgeSearchDebounceTimer);
  urgeFetchRequestId++;
  searchQueryDraft.value = '';
  appliedSearchQuery.value = '';
  selectedAgencyIds.value = [];
  appliedAgencyIds.value = [];
  selectedSubAgencyIds.value = [];
  appliedSubAgencyIds.value = [];
  fromDate.value = '';
  appliedFromDate.value = '';
  toDate.value = '';
  appliedToDate.value = '';
  currentPage.value = 1;
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch (e) {
    console.error('Error removing from localStorage:', e);
  }
  loadLogs();
}

function exportExcel() {
  const headers = ["STT", "Mã Nhiệm Vụ", "Nhiệm Vụ", "Cơ Quan Nhận Thông Báo", "Chỉ Số Dự Báo (%)", "Nội Dung Thông Báo", "Thời Gian Gửi"];
  const minColWidths = { 0: 8, 1: 15, 2: 45, 3: 28, 4: 18, 5: 50, 6: 20 };

  const rows = logs.value.map((log, idx) => [
    idx + 1,
    log.taskCode || '',
    log.taskTitle || '',
    log.leadAgencyName || log.leadAgencyCode || 'Chưa xác định',
    log.forecastValue !== undefined && log.forecastValue !== null ? `${log.forecastValue}%` : '—',
    log.urgeContent || '',
    formatDate(log.createdAt)
  ]);

  exportToExcel({
    title: "BÁO CÁO LỊCH SỬ THÔNG BÁO & CHỈ ĐẠO TIẾN ĐỘ",
    headers,
    rows,
    fileName: "Lich_Su_Thong_Bao",
    sheetName: "Lịch sử thông báo",
    minColWidths
  });
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  currentPage.value = newPage;
  loadLogs();
}

const uniqueAgenciesCount = computed(() => {
  const set = new Set();
  logs.value.forEach(l => {
    if (l.leadAgencyName) set.add(l.leadAgencyName);
    else if (l.leadAgencyCode) set.add(l.leadAgencyCode);
  });
  return set.size;
});

const latestLogDate = computed(() => {
  if (!logs.value || logs.value.length === 0) return 'Chưa có';
  const latest = logs.value[0];
  return formatDate(latest.createdAt);
});

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

async function checkAndOpenLogFromHash() {
  const hash = window.location.hash || '';
  if (hash.includes('logId=')) {
    const match = hash.match(/logId=([a-f0-9-]+)/i);
    if (match && match[1]) {
      const targetLogId = match[1];
      try {
        const res = await fetch(getApiUrl(`/api/execution/urge-logs/${targetLogId}`));
        if (res.ok) {
          const logData = await res.json();
          if (logData) {
            openDetail(logData);
          }
        }
      } catch (e) {
        console.error('Failed to load log detail from hash:', e);
      }
    }
  }
}

async function loadLogs() {
  const currentRequestId = ++urgeFetchRequestId;
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/execution/urge-logs'));
    url.searchParams.append('pageNumber', currentPage.value);
    url.searchParams.append('pageSize', pageSize.value);
    
    if (appliedSearchQuery.value.trim()) {
      url.searchParams.append('search', appliedSearchQuery.value.trim());
    }
    if (appliedAgencyIds.value && appliedAgencyIds.value.length > 0) {
      url.searchParams.append('agencyId', appliedAgencyIds.value[0]);
    }
    if (appliedSubAgencyIds.value && appliedSubAgencyIds.value.length > 0) {
      url.searchParams.append('subAgencyId', appliedSubAgencyIds.value[0]);
    }
    if (appliedFromDate.value) {
      url.searchParams.append('fromDate', appliedFromDate.value);
    }
    if (appliedToDate.value) {
      url.searchParams.append('toDate', appliedToDate.value);
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (currentRequestId !== urgeFetchRequestId) return;
      if (data.items) {
        logs.value = data.items;
        totalCount.value = data.totalCount || data.items.length;
        currentPage.value = data.pageNumber || 1;
        pageSize.value = data.pageSize || 10;
        totalPages.value = data.totalPages || 1;
      } else {
        logs.value = data;
        totalCount.value = data.length;
        totalPages.value = 1;
      }
    }
  } catch (e) {
    if (currentRequestId !== urgeFetchRequestId) return;
    console.error('Failed to fetch urge logs:', e);
  } finally {
    if (currentRequestId === urgeFetchRequestId) {
      isLoading.value = false;
      checkAndOpenLogFromHash();
    }
  }
}

watch(() => window.location.hash, () => {
  checkAndOpenLogFromHash();
});

onMounted(() => {
  loadSavedQuery();
  loadAgencies();
  loadLogs();
});
</script>
