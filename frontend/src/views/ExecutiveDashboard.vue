<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Top Header -->
    <header class="flex flex-col lg:flex-row lg:items-center justify-between gap-3 bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div>
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
          </span>
          <div>
            <h2 class="text-sm sm:text-base font-extrabold tracking-tight text-slate-800">
              Trang chủ theo dõi tiến độ
            </h2>
          </div>
        </div>
      </div>

      <!-- Overview Goal vs Task Badges & View Mode Selector -->
      <div class="flex flex-wrap items-center gap-2">
        <div class="bg-slate-100 p-1 rounded-xl border border-slate-200/80 flex items-center text-xs font-extrabold">
          <button 
            @click="dashboardFilter = 'all'" 
            :class="['px-3 py-1 rounded-lg transition', dashboardFilter === 'all' ? 'bg-white text-slate-900 shadow-xs' : 'text-slate-500 hover:text-slate-800']"
          >
            Tất cả
          </button>
          <button 
            @click="dashboardFilter = 'goals'" 
            :class="['px-3 py-1 rounded-lg transition flex items-center gap-1', dashboardFilter === 'goals' ? 'bg-purple-600 text-white shadow-xs' : 'text-purple-700 hover:bg-purple-50']"
          >
            🎯 Mục tiêu ({{ metrics.totalGoals ?? 0 }})
          </button>
          <button 
            @click="dashboardFilter = 'tasks'" 
            :class="['px-3 py-1 rounded-lg transition flex items-center gap-1', dashboardFilter === 'tasks' ? 'bg-blue-600 text-white shadow-xs' : 'text-blue-700 hover:bg-blue-50']"
          >
            📋 Nhiệm vụ ({{ metrics.totalTasks ?? 0 }})
          </button>
        </div>
      </div>
    </header>

    <!-- UNIFIED ADVANCED FILTER BAR -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 xl:grid-cols-7 gap-2 bg-slate-50/60 p-3 rounded-2xl border border-slate-200/80 items-end w-full shadow-sm">
      <div class="sm:col-span-2 xl:col-span-2">
        <SearchableSelect 
          v-model="selectedAgencyIds" 
          :options="agencyOptions" 
          :isMulti="true" 
          label="Cơ Quan / Đơn Vị" 
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

      <!-- Từ năm ➔ Đến năm -->
      <div>
        <div class="flex items-center justify-between mb-1">
          <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block">Giai Đoạn</label>
          <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-extrabold text-blue-700">
            <input type="checkbox" v-model="isOngoingOnly" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3 h-3">
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

      <div class="flex items-center gap-1.5 col-span-1">
        <button 
          type="button" 
          @click="loadDashboardMetrics" 
          class="w-full py-1.5 px-3 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm transition min-h-[34px] cursor-pointer"
        >
          Tìm Kiếm
        </button>
        <button 
          type="button" 
          @click="resetDashboardFilters" 
          class="px-3 py-1.5 bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition min-h-[34px] cursor-pointer"
          title="Đặt lại bộ lọc"
        >
          ↺
        </button>
      </div>
    </div>

    <!-- 6 Execution Status Grid Cards (Filtered by Goal / Task / All) -->
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-2.5 w-full">
      <!-- 1. Chưa thực hiện -->
      <div class="bg-white p-3 rounded-2xl border border-slate-200 shadow-2xs space-y-0.5">
        <div class="text-[10px] font-bold text-slate-500 uppercase">1. Chưa thực hiện</div>
        <div class="text-xl font-black text-slate-700">{{ activeStatusSummary.notStarted ?? 0 }}</div>
      </div>

      <!-- 2. Đang thực hiện (trong hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-blue-200 bg-blue-50/40 shadow-2xs space-y-0.5">
        <div class="text-[10px] font-bold text-blue-700 uppercase">2. Đang thực hiện (trong hạn)</div>
        <div class="text-xl font-black text-blue-800">{{ activeStatusSummary.inProgressOnTime ?? 0 }}</div>
      </div>

      <!-- 3. Đang thực hiện (quá hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-rose-200 bg-rose-50/40 shadow-2xs space-y-0.5">
        <div class="text-[10px] font-bold text-rose-700 uppercase">3. Đang thực hiện (quá hạn)</div>
        <div class="text-xl font-black text-rose-800">{{ activeStatusSummary.inProgressOverdue ?? 0 }}</div>
      </div>

      <!-- 4. Hoàn thành (đúng hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-emerald-200 bg-emerald-50/40 shadow-2xs space-y-0.5">
        <div class="text-[10px] font-bold text-emerald-700 uppercase">4. Hoàn thành (đúng hạn)</div>
        <div class="text-xl font-black text-emerald-800">{{ activeStatusSummary.completedOnTime ?? 0 }}</div>
      </div>

      <!-- 5. Hoàn thành (quá hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-teal-200 bg-teal-50/40 shadow-2xs space-y-0.5">
        <div class="text-[10px] font-bold text-teal-700 uppercase">5. Hoàn thành (quá hạn)</div>
        <div class="text-xl font-black text-teal-800">{{ activeStatusSummary.completedOverdue ?? 0 }}</div>
      </div>

      <!-- 6. Sắp hết hạn -->
      <div class="bg-white p-3 rounded-2xl border border-amber-200 bg-amber-50/40 shadow-2xs space-y-0.5">
        <div class="text-[10px] font-bold text-amber-700 uppercase">6. Sắp hết hạn</div>
        <div class="text-xl font-black text-amber-800">{{ activeStatusSummary.expiringSoon ?? 0 }}</div>
      </div>
    </div>

    <!-- 2 Main Blocks: Khối Bộ / Ngành & Khối Địa Phương (Admin View) -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 w-full">
      
      <!-- Block 1: Khối Bộ / Ngành -->
      <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-900 flex items-center gap-2">
            🏢 Khối Các Bộ / Ngành Trung Ương
          </h3>
          <span class="text-xs font-bold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-lg">
            {{ metrics.ministriesPerformance?.length ?? 0 }} Bộ/Ngành
          </span>
        </div>

        <div class="space-y-3 max-h-96 overflow-y-auto custom-scrollbar pr-1">
          <div 
            v-for="item in metrics.ministriesPerformance" 
            :key="item.agencyId"
            @click="drilldownAgency(item)"
            class="p-3 bg-slate-50 hover:bg-slate-100/80 rounded-xl border border-slate-200/60 transition cursor-pointer space-y-2"
          >
            <div class="flex items-center justify-between text-xs font-bold">
              <span class="text-slate-900">{{ item.name }} ({{ item.code }})</span>
              <div class="flex items-center gap-1.5 text-[11px]">
                <span class="text-purple-800 bg-purple-100 px-1.5 py-0.5 rounded">🎯 {{ item.totalGoals ?? 0 }}</span>
                <span class="text-blue-800 bg-blue-100 px-1.5 py-0.5 rounded">📋 {{ item.totalTasks ?? 0 }}</span>
              </div>
            </div>

            <!-- 6-status mini progress bar -->
            <div class="flex h-2.5 rounded-full overflow-hidden bg-slate-200">
              <div :style="{ width: `${getPct(item.completedOnTime, item.totalItems)}%` }" class="bg-emerald-500" title="Hoàn thành (đúng hạn)"></div>
              <div :style="{ width: `${getPct(item.completedOverdue, item.totalItems)}%` }" class="bg-teal-500" title="Hoàn thành (quá hạn)"></div>
              <div :style="{ width: `${getPct(item.inProgressOnTime, item.totalItems)}%` }" class="bg-blue-500" title="Đang thực hiện (trong hạn)"></div>
              <div :style="{ width: `${getPct(item.expiringSoon, item.totalItems)}%` }" class="bg-amber-500" title="Sắp hết hạn"></div>
              <div :style="{ width: `${getPct(item.inProgressOverdue, item.totalItems)}%` }" class="bg-rose-500" title="Đang thực hiện (quá hạn)"></div>
              <div :style="{ width: `${getPct(item.notStarted, item.totalItems)}%` }" class="bg-slate-400" title="Chưa thực hiện"></div>
            </div>

            <div class="flex items-center justify-between text-[10px] text-slate-500 font-semibold pt-1">
              <span>Đúng hạn: {{ item.completedOnTime + item.inProgressOnTime }}</span>
              <span>Sắp hết hạn: {{ item.expiringSoon }}</span>
              <span class="text-rose-600">Quá hạn: {{ item.inProgressOverdue }}</span>
            </div>
          </div>

          <div v-if="!metrics.ministriesPerformance?.length" class="p-6 text-center text-xs text-slate-400">
            Không có dữ liệu Bộ/Ngành.
          </div>
        </div>
      </div>

      <!-- Block 2: Khối Địa Phương -->
      <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-900 flex items-center gap-2">
            🏛️ Khối Các Tỉnh / Thành Phố
          </h3>
          <span class="text-xs font-bold text-emerald-600 bg-emerald-50 px-2.5 py-1 rounded-lg">
            {{ metrics.provincesPerformance?.length ?? 0 }} Địa phương
          </span>
        </div>

        <div class="space-y-3 max-h-96 overflow-y-auto custom-scrollbar pr-1">
          <div 
            v-for="item in metrics.provincesPerformance" 
            :key="item.agencyId"
            @click="drilldownAgency(item)"
            class="p-3 bg-slate-50 hover:bg-slate-100/80 rounded-xl border border-slate-200/60 transition cursor-pointer space-y-2"
          >
            <div class="flex items-center justify-between text-xs font-bold">
              <span class="text-slate-900">{{ item.name }} ({{ item.code }})</span>
              <span class="text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded-md text-[11px]">
                Tổng: {{ item.totalItems }}
              </span>
            </div>

            <div class="flex h-2.5 rounded-full overflow-hidden bg-slate-200">
              <div :style="{ width: `${getPct(item.completedOnTime, item.totalItems)}%` }" class="bg-emerald-500"></div>
              <div :style="{ width: `${getPct(item.completedOverdue, item.totalItems)}%` }" class="bg-teal-500"></div>
              <div :style="{ width: `${getPct(item.inProgressOnTime, item.totalItems)}%` }" class="bg-blue-500"></div>
              <div :style="{ width: `${getPct(item.expiringSoon, item.totalItems)}%` }" class="bg-amber-500"></div>
              <div :style="{ width: `${getPct(item.inProgressOverdue, item.totalItems)}%` }" class="bg-rose-500"></div>
              <div :style="{ width: `${getPct(item.notStarted, item.totalItems)}%` }" class="bg-slate-400"></div>
            </div>

            <div class="flex items-center justify-between text-[10px] text-slate-500 font-semibold pt-1">
              <span>Đúng hạn: {{ item.completedOnTime + item.inProgressOnTime }}</span>
              <span>Sắp hết hạn: {{ item.expiringSoon }}</span>
              <span class="text-rose-600">Quá hạn: {{ item.inProgressOverdue }}</span>
            </div>
          </div>

          <div v-if="!metrics.provincesPerformance?.length" class="p-6 text-center text-xs text-slate-400">
            Không có dữ liệu Địa phương.
          </div>
        </div>
      </div>

    </div>

    <!-- Drilldown Sub-agencies Modal -->
    <div v-if="selectedDrilldownAgency" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-2xl w-full p-6 space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base font-extrabold text-slate-900">
              Chi Tiết Đơn Vị Trực Thuộc: {{ selectedDrilldownAgency.name }}
            </h3>
            <p class="text-xs text-slate-500">Thống kê theo 6 trạng thái thực hiện của các đơn vị trực thuộc</p>
          </div>
          <button @click="selectedDrilldownAgency = null" class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-lg">✕</button>
        </div>

        <div class="space-y-3 max-h-96 overflow-y-auto custom-scrollbar">
          <div v-for="child in subAgenciesList" :key="child.agencyId" class="p-3 bg-slate-50 rounded-xl border border-slate-200 space-y-2">
            <div class="flex items-center justify-between text-xs font-bold">
              <span>{{ child.name }} ({{ child.code }})</span>
              <span class="text-blue-700">Tổng: {{ child.totalItems }}</span>
            </div>
            <div class="flex h-2 rounded-full overflow-hidden bg-slate-200">
              <div :style="{ width: `${getPct(child.completedOnTime, child.totalItems)}%` }" class="bg-emerald-500"></div>
              <div :style="{ width: `${getPct(child.completedOverdue, child.totalItems)}%` }" class="bg-teal-500"></div>
              <div :style="{ width: `${getPct(child.inProgressOnTime, child.totalItems)}%` }" class="bg-blue-500"></div>
              <div :style="{ width: `${getPct(child.expiringSoon, child.totalItems)}%` }" class="bg-amber-500"></div>
              <div :style="{ width: `${getPct(child.inProgressOverdue, child.totalItems)}%` }" class="bg-rose-500"></div>
              <div :style="{ width: `${getPct(child.notStarted, child.totalItems)}%` }" class="bg-slate-400"></div>
            </div>
          </div>

          <div v-if="subAgenciesList.length === 0" class="p-8 text-center text-xs text-slate-400 font-semibold">
            Không có đơn vị trực thuộc nào.
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';

const dashboardFilter = ref('all'); // 'all', 'goals', 'tasks'

const selectedAgencyIds = ref([]);
const selectedSections = ref([]);
const selectedGroups = ref([]);
const fromYear = ref(null);
const toYear = ref(null);
const isOngoingOnly = ref(false);
const agencies = ref([]);

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: `${ag.code} - ${ag.name}` }));
});

const yearOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));

const sectionOptions = computed(() => [...GOAL_SECTIONS, ...TASK_SECTIONS]);
const groupOptions = computed(() => [...GOAL_GROUPS, ...TASK_GROUPS]);

const metrics = ref({
  totalGoals: 0,
  completedGoals: 0,
  totalTasks: 0,
  completedTasks: 0,
  statusSummary: {},
  goalStatusSummary: {},
  taskStatusSummary: {},
  ministriesPerformance: [],
  provincesPerformance: []
});

const activeStatusSummary = computed(() => {
  if (dashboardFilter.value === 'goals') return metrics.value.goalStatusSummary || {};
  if (dashboardFilter.value === 'tasks') return metrics.value.taskStatusSummary || {};
  return metrics.value.statusSummary || {};
});

const selectedDrilldownAgency = ref(null);
const subAgenciesList = ref([]);

const userAgencyName = computed(() => {
  return authState.user.value?.agencyName || 'Cơ quan/Bộ/Địa phương';
});

function getPct(val, total) {
  if (!total || total <= 0) return 0;
  return Math.round(((val || 0) / total) * 100);
}

function resetDashboardFilters() {
  selectedAgencyIds.value = [];
  selectedSections.value = [];
  selectedGroups.value = [];
  fromYear.value = null;
  toYear.value = null;
  isOngoingOnly.value = false;
  dashboardFilter.value = 'all';
  loadDashboardMetrics();
}

async function loadAgencies() {
  try {
    const res = await fetch(getApiUrl('/api/agencies'));
    if (res.ok) {
      const data = await res.json();
      agencies.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (e) {}
}

async function loadDashboardMetrics() {
  try {
    const params = new URLSearchParams();
    if (dashboardFilter.value && dashboardFilter.value !== 'all') {
      const itemType = dashboardFilter.value === 'goals' ? 'Goal' : 'Task';
      params.append('itemType', itemType);
    }
    if (selectedAgencyIds.value && selectedAgencyIds.value.length > 0) {
      selectedAgencyIds.value.forEach(id => params.append('agencyId', id));
    } else if (!authState.isAdmin.value && authState.user.value?.agencyId) {
      params.append('agencyId', authState.user.value.agencyId);
    }
    if (selectedSections.value && selectedSections.value.length > 0) {
      selectedSections.value.forEach(sec => params.append('section', sec));
    }
    if (selectedGroups.value && selectedGroups.value.length > 0) {
      selectedGroups.value.forEach(grp => params.append('group', grp));
    }
    if (fromYear.value) params.append('fromYear', fromYear.value);
    if (toYear.value) params.append('toYear', toYear.value);
    if (isOngoingOnly.value) params.append('isOngoing', 'true');

    const queryString = params.toString();
    const url = getApiUrl(`/api/dashboard/metrics${queryString ? '?' + queryString : ''}`);
    const res = await fetch(url);
    if (res.ok) {
      metrics.value = await res.json();
    }
  } catch (e) {
    // Silent catch
  }
}

async function drilldownAgency(agency) {
  selectedDrilldownAgency.value = agency;
  subAgenciesList.value = [];
  try {
    const params = new URLSearchParams();
    params.append('parentAgencyId', agency.agencyId);
    if (dashboardFilter.value && dashboardFilter.value !== 'all') {
      const itemType = dashboardFilter.value === 'goals' ? 'Goal' : 'Task';
      params.append('itemType', itemType);
    }
    if (selectedSections.value && selectedSections.value.length > 0) {
      selectedSections.value.forEach(sec => params.append('section', sec));
    }
    if (selectedGroups.value && selectedGroups.value.length > 0) {
      selectedGroups.value.forEach(grp => params.append('group', grp));
    }
    if (fromYear.value) params.append('fromYear', fromYear.value);
    if (toYear.value) params.append('toYear', toYear.value);
    if (isOngoingOnly.value) params.append('isOngoing', 'true');

    const res = await fetch(getApiUrl(`/api/dashboard/metrics?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      subAgenciesList.value = [...(data.ministriesPerformance || []), ...(data.provincesPerformance || [])];
    }
  } catch (e) {}
}

watch([dashboardFilter, selectedAgencyIds, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly], () => {
  loadDashboardMetrics();
}, { deep: true });

onMounted(() => {
  loadAgencies();
  loadDashboardMetrics();
});
</script>
