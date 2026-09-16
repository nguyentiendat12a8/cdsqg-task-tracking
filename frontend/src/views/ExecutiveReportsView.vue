<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Header Bar -->
    <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 flex flex-col md:flex-row md:items-center justify-between gap-3 w-full">
      <div>
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          </span>
          <div>
            <h2 class="text-sm sm:text-base font-extrabold tracking-tight text-slate-800">
              Trung Tâm Báo Cáo & Xuất Dữ Liệu Excel
            </h2>
          </div>
        </div>
      </div>

      <button 
        @click="exportCurrentReportToExcel" 
        class="px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-extrabold text-xs rounded-xl shadow-md transition flex items-center gap-2 shrink-0"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
        Xuất File Excel (.xlsx / .csv)
      </button>
    </div>

    <!-- REPORT TYPE SELECTOR TABS -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-2.5 w-full">
      <button 
        @click="activeReportType = 'summary'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5',
          activeReportType === 'summary' ? 'bg-blue-600 text-white border-blue-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-extrabold uppercase opacity-80">Báo cáo 1</div>
        <div class="text-xs font-black truncate">📊 Tổng hợp Tiến độ Bộ & Địa phương</div>
      </button>

      <button 
        @click="activeReportType = 'urgent'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5',
          activeReportType === 'urgent' ? 'bg-rose-600 text-white border-rose-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-extrabold uppercase opacity-80">Báo cáo 2</div>
        <div class="text-xs font-black truncate">⚠️ Nhiệm vụ Sắp hết hạn & Quá hạn</div>
      </button>

      <button 
        @click="activeReportType = 'detail'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5',
          activeReportType === 'detail' ? 'bg-indigo-600 text-white border-indigo-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-extrabold uppercase opacity-80">Báo cáo 3</div>
        <div class="text-xs font-black truncate">📄 Chi tiết Tiến độ & File Minh chứng</div>
      </button>

      <button 
        @click="activeReportType = 'scope'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5',
          activeReportType === 'scope' ? 'bg-purple-600 text-white border-purple-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-extrabold uppercase opacity-80">Báo cáo 4</div>
        <div class="text-xs font-black truncate">🌐 Phân loại Nhiệm vụ Chung & Riêng</div>
      </button>
    </div>

    <!-- FILTER BAR -->
    <div class="bg-white p-3 rounded-2xl shadow-sm border border-slate-200/80 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 xl:grid-cols-7 gap-2 items-end w-full">
      <div>
        <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Từ Khóa</label>
        <input v-model="searchQuery" placeholder="Mã, tiêu đề..." class="w-full text-xs font-semibold px-2.5 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none min-h-[34px]" />
      </div>

      <div>
        <SearchableSelect 
          v-model="selectedAgencyIds" 
          :options="agencyOptions" 
          :isMulti="true" 
          label="Cơ quan / Đơn vị" 
          placeholder="Tất cả cơ quan / đơn vị"
        />
      </div>

      <div>
        <SearchableSelect 
          v-model="selectedSections" 
          :options="sectionOptions" 
          :isMulti="true" 
          label="Mục (Phụ lục)" 
          placeholder="Tất cả mục"
        />
      </div>

      <div>
        <SearchableSelect 
          v-model="selectedGroups" 
          :options="groupOptions" 
          :isMulti="true" 
          label="Nhóm Trọng Tâm" 
          placeholder="Tất cả nhóm"
        />
      </div>

      <div>
        <div class="flex items-center justify-between mb-1">
          <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block">Giai Đoạn</label>
          <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-extrabold text-blue-700">
            <input type="checkbox" v-model="onlyOngoing" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3 h-3">
            Thường xuyên
          </label>
        </div>
        <div class="flex items-center gap-1">
          <SearchableSelect 
            v-model="fromYear" 
            :options="yearOptions" 
            :isMulti="false" 
            placeholder="Từ năm" 
            class="w-full"
          />
          <span class="text-xs font-bold text-slate-400">➔</span>
          <SearchableSelect 
            v-model="toYear" 
            :options="yearOptions" 
            :isMulti="false" 
            placeholder="Đến năm" 
            class="w-full"
          />
        </div>
      </div>

      <div>
        <SearchableSelect 
          v-model="selectedItemTypes" 
          :options="itemTypeOptions" 
          :isMulti="true" 
          label="Loại Đối Tượng" 
          placeholder="Tất cả"
        />
      </div>

      <div class="flex items-center gap-1.5 col-span-1 xl:col-start-7 ml-auto w-full justify-end">
        <button @click="loadReportData" class="w-full sm:w-auto px-5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl transition min-h-[34px] cursor-pointer whitespace-nowrap">Lọc Báo Cáo</button>
        <button @click="resetReportFilters" class="px-3 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition shrink-0 min-h-[34px] cursor-pointer" title="Đặt lại bộ lọc">↺</button>
      </div>
    </div>

    <!-- REPORT TABLE VIEW CONTAINER -->
    <div class="bg-white rounded-2xl shadow-sm border border-slate-200/80 overflow-hidden w-full">
      <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu báo cáo từ máy chủ..." />
      <template v-else>
      
      <!-- BÁO CÁO 1: TỔNG HỢP TIẾN ĐỘ THEO BỘ / ĐỊA PHƯƠNG -->
      <div v-if="activeReportType === 'summary'" class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-700 border-collapse">
          <thead class="bg-slate-100 font-extrabold text-slate-600 border-b border-slate-200">
            <tr>
              <th class="p-3">STT</th>
              <th class="p-3">Mã Đơn Vị</th>
              <th class="p-3">Tên Cơ Quan / Địa Phương</th>
              <th class="p-3 text-center">Phân Loại</th>
              <th class="p-3 text-center">Tổng Mục Tiêu</th>
              <th class="p-3 text-center">Tổng Nhiệm Vụ</th>
              <th class="p-3 text-center">Đã Hoàn Thành</th>
              <th class="p-3 text-center">Đang Thực Hiện</th>
              <th class="p-3 text-center text-amber-700">Sắp Hết Hạn</th>
              <th class="p-3 text-center text-rose-700">Quá Hạn</th>
              <th class="p-3 text-center">Tỷ Lệ Hoàn Thành</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="(ag, idx) in paginatedAgencySummaries" :key="ag.agencyId" class="hover:bg-slate-50 transition">
              <td class="p-3 font-bold text-slate-500">{{ (currentPage - 1) * pageSize + idx + 1 }}</td>
              <td class="p-3 font-black text-blue-700">{{ ag.code }}</td>
              <td class="p-3 font-bold text-slate-900">{{ ag.name }}</td>
              <td class="p-3 text-center">
                <span :class="['px-2 py-0.5 rounded text-[10px] font-extrabold', ag.type === 'Ministry' ? 'bg-blue-100 text-blue-800' : 'bg-emerald-100 text-emerald-800']">
                  {{ ag.type === 'Ministry' ? 'Bộ / Ngành' : 'Địa phương' }}
                </span>
              </td>
              <td class="p-3 text-center font-bold">{{ ag.totalGoals }}</td>
              <td class="p-3 text-center font-bold">{{ ag.totalTasks }}</td>
              <td class="p-3 text-center font-bold text-emerald-700">{{ ag.completedOnTime + ag.completedOverdue }}</td>
              <td class="p-3 text-center font-bold text-blue-700">{{ ag.inProgressOnTime }}</td>
              <td class="p-3 text-center font-bold text-amber-700">{{ ag.expiringSoon }}</td>
              <td class="p-3 text-center font-bold text-rose-700">{{ ag.inProgressOverdue }}</td>
              <td class="p-3 text-center font-extrabold text-blue-800">
                {{ getPct(ag.completedOnTime + ag.completedOverdue, ag.totalItems) }}%
              </td>
            </tr>
            <tr v-if="filteredAgencySummaries.length === 0">
              <td colspan="11" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- BÁO CÁO 2: DANH SÁCH NHIỆM VỤ SẮP HẾT HẠN & QUÁ HẠN -->
      <div v-else-if="activeReportType === 'urgent'" class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-700 border-collapse">
          <thead class="bg-rose-50 text-rose-900 font-extrabold border-b border-rose-200">
            <tr>
              <th class="p-3">Mã</th>
              <th class="p-3">Phân Loại</th>
              <th class="p-3">Tiêu Đề Mục Tiêu / Nhiệm Vụ</th>
              <th class="p-3">Đơn Vị Chủ Trì</th>
              <th class="p-3">Hạn Chót</th>
              <th class="p-3 text-center">Trạng Thái Cảnh Báo</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="item in paginatedUrgentItems" :key="item.taskId" class="hover:bg-rose-50/40 transition">
              <td class="p-3 font-extrabold text-rose-800">{{ item.code }}</td>
              <td class="p-3">
                <span :class="['px-2 py-0.5 rounded text-[10px] font-bold', item.itemType === 'Goal' ? 'bg-purple-100 text-purple-800' : 'bg-slate-100 text-slate-700']">
                  {{ item.itemType === 'Goal' ? '🎯 Mục tiêu' : '📋 Nhiệm vụ' }}
                </span>
              </td>
              <td class="p-3 font-semibold text-slate-900">{{ item.title }}</td>
              <td class="p-3 font-bold text-slate-800">{{ item.leadAgencyName }}</td>
              <td class="p-3 font-semibold text-slate-600">{{ formatDate(item.dueDate) }}</td>
              <td class="p-3 text-center">
                <span :class="['px-2.5 py-1 rounded-full text-[11px] font-extrabold', item.calculatedStatus === 'ExpiringSoon' ? 'bg-amber-100 text-amber-900 border border-amber-300' : 'bg-rose-100 text-rose-900 border border-rose-300']">
                  {{ item.calculatedStatus === 'ExpiringSoon' ? '⏰ Sắp Hết Hạn' : '🚨 Quá Hạn Thực Hiện' }}
                </span>
              </td>
            </tr>
            <tr v-if="urgentItems.length === 0">
              <td colspan="6" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- BÁO CÁO 3: CHI TIẾT TIẾN ĐỘ & FILE MINH CHỨNG -->
      <div v-else-if="activeReportType === 'detail'" class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-700 border-collapse">
          <thead class="bg-indigo-50 text-indigo-900 font-extrabold border-b border-indigo-200">
            <tr>
              <th class="p-3">Mã</th>
              <th class="p-3">Tiêu Đề</th>
              <th class="p-3">Đơn Vị Chủ Trì</th>
              <th class="p-3 text-center">Tiến Độ Mới Nhất</th>
              <th class="p-3 text-center">Trạng Thái</th>
              <th class="p-3">File Minh Chứng Đính Kèm</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="item in paginatedDetailItems" :key="item.taskId" class="hover:bg-slate-50 transition">
              <td class="p-3 font-extrabold text-indigo-700">{{ item.code }}</td>
              <td class="p-3 font-semibold text-slate-900">{{ item.title }}</td>
              <td class="p-3 font-bold text-slate-800">{{ item.leadAgencyName }}</td>
              <td class="p-3 text-center font-extrabold text-blue-700">
                {{ item.latestProgressValue !== null ? `${item.latestProgressValue} ${item.unitName}` : 'Chưa cập nhật' }}
              </td>
              <td class="p-3 text-center font-bold">
                {{ getStatusLabel(item.calculatedStatus) }}
              </td>
              <td class="p-3">
                <span v-if="!item.progressLogs?.length" class="text-slate-400 italic">Chưa có minh chứng</span>
                <div v-else class="space-y-1">
                  <template v-for="log in item.progressLogs" :key="log.id">
                    <a v-for="url in log.attachmentFileUrls" :key="url" :href="url" target="_blank" class="text-blue-600 underline font-semibold block text-[11px] hover:text-blue-800">
                      📎 {{ getFileName(url) }}
                    </a>
                  </template>
                </div>
              </td>
            </tr>
            <tr v-if="filteredDetailItems.length === 0">
              <td colspan="6" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- BÁO CÁO 4: PHÂN LOẠI NHIỆM VỤ CHUNG VS RIÊNG -->
      <div v-else-if="activeReportType === 'scope'" class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-700 border-collapse">
          <thead class="bg-purple-50 text-purple-900 font-extrabold border-b border-purple-200">
            <tr>
              <th class="p-3">Mã</th>
              <th class="p-3">Phạm Vi</th>
              <th class="p-3">Nội Dung Thực Hiện</th>
              <th class="p-3">Đơn Vị Đầu Mối</th>
              <th class="p-3">Thời Gian</th>
              <th class="p-3 text-center">Báo Cáo</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="item in paginatedScopeItems" :key="item.taskId" class="hover:bg-slate-50 transition">
              <td class="p-3 font-black text-purple-800">{{ item.code }}</td>
              <td class="p-3">
                <span :class="['px-2 py-0.5 rounded text-[10px] font-extrabold', item.isGeneralTask ? 'bg-purple-100 text-purple-800' : 'bg-slate-100 text-slate-700']">
                  {{ item.isGeneralTask ? '🌐 Nhiệm vụ Chung' : '🏢 Nhiệm vụ Riêng' }}
                </span>
              </td>
              <td class="p-3 font-semibold text-slate-900">{{ item.title }}</td>
              <td class="p-3 font-bold text-slate-800">{{ item.leadAgencyName }}</td>
              <td class="p-3 font-semibold text-slate-600">{{ formatDateRange(item.startDate, item.dueDate) }}</td>
              <td class="p-3 text-center font-bold">{{ getStatusLabel(item.calculatedStatus) }}</td>
            </tr>
            <tr v-if="filteredScopeItems.length === 0">
              <td colspan="6" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Server Pagination Controls Footer -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div>
          Hiển thị <span class="font-extrabold text-slate-900">{{ currentActiveTotalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, currentActiveTotalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ currentActiveTotalCount }}</span> bản ghi
        </div>

        <div class="flex items-center gap-2">
          <button 
            @click="changePage(currentPage - 1)" 
            :disabled="currentPage <= 1"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer disabled:cursor-not-allowed"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-xl font-black">
            Trang {{ currentPage }} / {{ currentActiveTotalPages }}
          </span>

          <button 
            @click="changePage(currentPage + 1)" 
            :disabled="currentPage >= currentActiveTotalPages"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer disabled:cursor-not-allowed"
          >
            Trang sau ›
          </button>
        </div>
      </div>

      </template>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import { getApiUrl } from '../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';

const activeReportType = ref('summary');
const isLoading = ref(false);
const agencies = ref([]);
const allItems = ref([]);
const metrics = ref({});

const selectedAgencyIds = ref([]);
const selectedItemTypes = ref([]);
const selectedSections = ref([]);
const selectedGroups = ref([]);
const fromYear = ref(null);
const toYear = ref(null);
const onlyOngoing = ref(false);
const searchQuery = ref('');

const currentPage = ref(1);
const pageSize = ref(10);

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: `${ag.code} - ${ag.name}` }));
});

const yearOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));

const sectionOptions = computed(() => [...GOAL_SECTIONS, ...TASK_SECTIONS]);
const groupOptions = computed(() => [...GOAL_GROUPS, ...TASK_GROUPS]);

const itemTypeOptions = ref([
  { value: 'Goal', label: '🎯 Chỉ Mục tiêu' },
  { value: 'Task', label: '📋 Chỉ Nhiệm vụ' }
]);

function getPct(val, total) {
  if (!total || total <= 0) return 0;
  return Math.round(((val || 0) / total) * 100);
}

function formatDate(d) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('vi-VN');
}

function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? new Date(sDate).toLocaleDateString('vi-VN') : '...';
  const d = dDate ? new Date(dDate).toLocaleDateString('vi-VN') : '...';
  return `${s} ➔ ${d}`;
}

function getFileName(path) {
  if (!path) return 'File đính kèm';
  return path.split('/').pop() || path;
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

function resetReportFilters() {
  selectedAgencyIds.value = [];
  selectedItemTypes.value = [];
  selectedSections.value = [];
  selectedGroups.value = [];
  fromYear.value = null;
  toYear.value = null;
  onlyOngoing.value = false;
  searchQuery.value = '';
  currentPage.value = 1;
  loadReportData();
}

function passesCommonFilters(i) {
  if (selectedAgencyIds.value?.length > 0 && !selectedAgencyIds.value.includes(i.leadAgencyId)) return false;
  if (selectedItemTypes.value?.length > 0 && !selectedItemTypes.value.includes(i.itemType)) return false;
  if (selectedSections.value?.length > 0 && !selectedSections.value.includes(i.section)) return false;
  if (selectedGroups.value?.length > 0 && !selectedGroups.value.includes(i.group)) return false;
  if (onlyOngoing.value && !i.isOngoing) return false;
  if (fromYear.value || toYear.value) {
    const fY = fromYear.value ? Number(fromYear.value) : 2026;
    const tY = toYear.value ? Number(toYear.value) : 2030;
    if (!i.isOngoing) {
      const sY = i.startDate ? new Date(i.startDate).getFullYear() : 2026;
      const dY = i.dueDate ? new Date(i.dueDate).getFullYear() : sY;
      if (sY > tY || dY < fY) return false;
    }
  }
  if (searchQuery.value && searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase();
    const match = (i.title && i.title.toLowerCase().includes(q)) || (i.code && i.code.toLowerCase().includes(q));
    if (!match) return false;
  }
  return true;
}

const filteredAgencySummaries = computed(() => {
  const list = [...(metrics.value.ministriesPerformance || []), ...(metrics.value.provincesPerformance || [])];
  if (selectedAgencyIds.value && selectedAgencyIds.value.length > 0) {
    return list.filter(a => selectedAgencyIds.value.includes(a.agencyId));
  }
  return list;
});

const urgentItems = computed(() => {
  return allItems.value.filter(i => (i.calculatedStatus === 'ExpiringSoon' || i.calculatedStatus === 'InProgressOverdue') && passesCommonFilters(i));
});

const filteredDetailItems = computed(() => {
  return allItems.value.filter(i => passesCommonFilters(i));
});

const filteredScopeItems = computed(() => {
  return allItems.value.filter(i => passesCommonFilters(i));
});

const currentActiveTotalCount = computed(() => {
  if (activeReportType.value === 'summary') return filteredAgencySummaries.value.length;
  if (activeReportType.value === 'urgent') return urgentItems.value.length;
  if (activeReportType.value === 'detail') return filteredDetailItems.value.length;
  if (activeReportType.value === 'scope') return filteredScopeItems.value.length;
  return 0;
});

const currentActiveTotalPages = computed(() => {
  const total = currentActiveTotalCount.value;
  if (total <= 0) return 1;
  return Math.ceil(total / pageSize.value);
});

function changePage(page) {
  if (page < 1 || page > currentActiveTotalPages.value) return;
  currentPage.value = page;
}

const paginatedAgencySummaries = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredAgencySummaries.value.slice(start, start + pageSize.value);
});

const paginatedUrgentItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return urgentItems.value.slice(start, start + pageSize.value);
});

const paginatedDetailItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredDetailItems.value.slice(start, start + pageSize.value);
});

const paginatedScopeItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredScopeItems.value.slice(start, start + pageSize.value);
});

watch([activeReportType, searchQuery, selectedAgencyIds, selectedItemTypes, selectedSections, selectedGroups, fromYear, toYear, onlyOngoing], () => {
  currentPage.value = 1;
});

async function loadReportData() {
  isLoading.value = true;
  try {
    const agRes = await fetch(getApiUrl('/api/agencies'));
    if (agRes.ok) agencies.value = await agRes.json();

    const params = new URLSearchParams();
    if (selectedAgencyIds.value && selectedAgencyIds.value.length > 0) {
      selectedAgencyIds.value.forEach(id => params.append('agencyId', id));
    }
    if (selectedSections.value && selectedSections.value.length > 0) {
      selectedSections.value.forEach(sec => params.append('section', sec));
    }
    if (selectedGroups.value && selectedGroups.value.length > 0) {
      selectedGroups.value.forEach(grp => params.append('group', grp));
    }
    if (fromYear.value) params.append('fromYear', fromYear.value);
    if (toYear.value) params.append('toYear', toYear.value);
    if (onlyOngoing.value) params.append('isOngoing', 'true');

    const mRes = await fetch(getApiUrl(`/api/dashboard/metrics${params.toString() ? '?' + params.toString() : ''}`));
    if (mRes.ok) metrics.value = await mRes.json();

    const docId = '12660000-0000-0000-0000-000000001266';
    const gridRes = await fetch(getApiUrl(`/api/planning/documents/${docId}/grid`));
    if (gridRes.ok) {
      const data = await gridRes.json();
      allItems.value = data.items || [];
    }
  } catch (e) {
    console.error('Lỗi tải dữ liệu báo cáo:', e);
  } finally {
    isLoading.value = false;
  }
}

function exportCurrentReportToExcel() {
  let csvContent = "\uFEFF"; // Add UTF-8 BOM for Microsoft Excel Vietnamese font support
  
  if (activeReportType.value === 'summary') {
    csvContent += "Mã Đơn Vị,Tên Cơ Quan,Phân Loại,Tổng Mục Tiêu,Tổng Nhiệm Vụ,Đã Hoàn Thành,Đang Thực Hiện,Sắp Hết Hạn,Quá Hạn,Tỷ Lệ Hoàn Thành (%)\n";
    filteredAgencySummaries.value.forEach(ag => {
      const pct = getPct(ag.completedOnTime + ag.completedOverdue, ag.totalItems);
      csvContent += `"${ag.code}","${ag.name}","${ag.type}","${ag.totalGoals}","${ag.totalTasks}","${ag.completedOnTime + ag.completedOverdue}","${ag.inProgressOnTime}","${ag.expiringSoon}","${ag.inProgressOverdue}","${pct}"\n`;
    });
  } else if (activeReportType.value === 'urgent') {
    csvContent += "Mã,Phân Loại,Tiêu Đề,Đơn Vị Chủ Trì,Hạn Chót,Trạng Thái\n";
    urgentItems.value.forEach(i => {
      csvContent += `"${i.code}","${i.itemType}","${i.title}","${i.leadAgencyName}","${formatDate(i.dueDate)}","${getStatusLabel(i.calculatedStatus)}"\n`;
    });
  } else if (activeReportType.value === 'detail') {
    csvContent += "Mã,Tiêu Đề,Đơn Vị Chủ Trì,Tiến Độ Mới Nhất,Trạng Thái\n";
    filteredDetailItems.value.forEach(i => {
      csvContent += `"${i.code}","${i.title}","${i.leadAgencyName}","${i.latestProgressValue !== null ? i.latestProgressValue : 'Chưa cập nhật'}","${getStatusLabel(i.calculatedStatus)}"\n`;
    });
  } else if (activeReportType.value === 'scope') {
    csvContent += "Mã,Phạm Vi,Nội Dung Thực Hiện,Đơn Vị Đầu Mối,Thời Gian,Trạng Thái\n";
    filteredScopeItems.value.forEach(i => {
      const scopeStr = i.isGeneralTask ? "Nhiệm vụ Chung" : "Nhiệm vụ Riêng";
      csvContent += `"${i.code}","${scopeStr}","${i.title}","${i.leadAgencyName}","${formatDateRange(i.startDate, i.dueDate)}","${getStatusLabel(i.calculatedStatus)}"\n`;
    });
  }

  const blob = new Blob([csvContent], { type: 'type: text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.setAttribute('download', `Bao_Cao_Quyet_Dinh_1266_${activeReportType.value}_${new Date().toISOString().slice(0,10)}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

onMounted(() => {
  loadReportData();
});
</script>
