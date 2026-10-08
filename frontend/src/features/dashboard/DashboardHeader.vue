<template>
<header class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-white p-4 sm:p-5 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div class="flex items-center gap-2.5">
        <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
        </span>
        <div>
          <h2 class="text-sm sm:text-base font-bold text-slate-800">
            {{ dashboardTitleText }}
          </h2>
        </div>
      </div>

      <!-- Action Buttons & Advanced Filter Popover -->
      <div class="flex flex-wrap items-center gap-2 min-w-0">
        <!-- Segmented Tab Filter: Mục tiêu | Nhiệm vụ -->
        <div class="inline-flex p-1 bg-slate-100 rounded-xl border border-slate-200/90 shrink-0 h-[34px] items-center">
          <button
            type="button"
            @click="setDashboardFilter('goals')"
            :class="[
              'px-3.5 py-1 text-xs font-bold rounded-lg transition cursor-pointer whitespace-nowrap h-full flex items-center gap-1.5',
              dashboardFilter === 'goals' ? 'bg-purple-600 text-white shadow-2xs' : 'text-slate-600 hover:text-slate-900'
            ]"
          >
            <span>🎯</span> Mục tiêu
          </button>
          <button
            type="button"
            @click="setDashboardFilter('tasks')"
            :class="[
              'px-3.5 py-1 text-xs font-bold rounded-lg transition cursor-pointer whitespace-nowrap h-full flex items-center gap-1.5',
              dashboardFilter === 'tasks' ? 'bg-blue-600 text-white shadow-2xs' : 'text-slate-600 hover:text-slate-900'
            ]"
          >
            <span>📋</span> Nhiệm vụ
          </button>
        </div>

        <button
          type="button"
          @click="exportDashboardExcelReport"
          :disabled="isExportingExcel"
          class="px-3.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-2xs transition h-[34px] flex items-center gap-1.5 cursor-pointer whitespace-nowrap shrink-0"
          title="Xuất file báo cáo Excel theo bộ lọc (Mỗi Bộ/Ngành/Địa phương 1 Sheet)"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          <span>{{ isExportingExcel ? 'Đang xuất...' : 'Xuất Báo Cáo Excel' }}</span>
        </button>

        <!-- OverlayPanel Filter Popover -->
        <OverlayPanel
          title="Lọc Dữ Liệu Bảng Điều Khiển"
          buttonText="Bộ Lọc Nâng Cao"
          :activeCount="activeDashboardFilterCount"
          widthClass="w-[340px] sm:w-[500px]"
          @apply="loadDashboardMetrics"
          @reset="resetDashboardFilters"
        >
          <div class="space-y-3">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <SearchableSelect
                  v-model="dashboardFilter"
                  :options="dashboardFilterOptions"
                  :isMulti="false"
                  label="Loại Đối Tượng"
                  placeholder="Tất cả (Mục tiêu & Nhiệm vụ)"
                />
              </div>

              <div>
                <SearchableSelect
                  v-model="selectedScopes"
                  :options="scopeOptions"
                  :isMulti="true"
                  label="Phạm Vi"
                  placeholder="Tất cả phạm vi"
                />
              </div>
            </div>

            <div>
              <SearchableSelect
                v-model="selectedAgencyIds"
                :options="leadAgencyOptions"
                :isMulti="true"
                label="Cơ Quan Chủ Trì"
                placeholder="Tất cả cơ quan chủ trì"
              />
            </div>

            <div v-if="authState.isAdmin.value || isBKHCNAgency">
              <SearchableSelect
                v-model="selectedSubAgencyIds"
                :options="subAgencyOptions"
                :isMulti="true"
                label="Đơn Vị Trực Thuộc"
                placeholder="Tất cả đơn vị trực thuộc"
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
                <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wider block">Giai Đoạn (Từ năm ➔ Đến năm)</label>
                <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-bold text-blue-700">
                  <input type="checkbox" v-model="isOngoingOnly" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3 h-3">
                  Thường xuyên
                </label>
              </div>
              <div class="flex items-center gap-2">
                <SearchableSelect
                  v-model="fromYear"
                  :options="yearOptions"
                  :isMulti="false"
                  placeholder="Từ năm"
                  class="w-full"
                />
                <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                <SearchableSelect
                  v-model="toYear"
                  :options="yearOptions"
                  :isMulti="false"
                  placeholder="Đến năm"
                  class="w-full"
                />
              </div>
            </div>
          </div>
        </OverlayPanel>
      </div>
    </header>
</template>

<script setup>
import SearchableSelect from '../../components/SearchableSelect.vue';
import OverlayPanel from '../../components/OverlayPanel.vue';

defineProps([
  'authState',
  'setDashboardFilter',
  'dashboardTitleText',
  'dashboardFilterOptions',
  'scopeOptions',
  'activeDashboardFilterCount',
  'resetDashboardFilters',
  'leadAgencyOptions',
  'subAgencyOptions',
  'yearOptions',
  'sectionOptions',
  'groupOptions',
  'isBKHCNAgency',
  'loadDashboardMetrics',
  'isExportingExcel',
  'exportDashboardExcelReport'
]);
const dashboardFilter = defineModel('dashboardFilter', { required: true });
const selectedScopes = defineModel('selectedScopes', { required: true });
const selectedAgencyIds = defineModel('selectedAgencyIds', { required: true });
const selectedSubAgencyIds = defineModel('selectedSubAgencyIds', { required: true });
const selectedSections = defineModel('selectedSections', { required: true });
const selectedGroups = defineModel('selectedGroups', { required: true });
const isOngoingOnly = defineModel('isOngoingOnly', { required: true });
const fromYear = defineModel('fromYear', { required: true });
const toYear = defineModel('toYear', { required: true });
</script>
