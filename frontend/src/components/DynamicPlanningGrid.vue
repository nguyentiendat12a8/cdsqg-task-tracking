<template>
  <div class="w-full space-y-5 font-sans relative">
    
    <!-- Subtle Floating Toast Notification -->
    <transition name="fade">
      <div v-if="toastMessage" class="fixed bottom-6 right-6 z-50 bg-slate-900/90 text-white text-xs font-bold px-4 py-3 rounded-2xl shadow-xl backdrop-blur-sm flex items-center gap-2 border border-slate-700 animate-in fade-in slide-in-from-bottom-2">
        <span class="text-emerald-400 font-black text-sm">✓</span>
        <span>{{ toastMessage }}</span>
      </div>
    </transition>

    <!-- ADVANCED FILTER BAR WITH OVERLAY PANEL -->
    <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-slate-50/60 p-3 w-full rounded-2xl border border-slate-200/80">
      <div class="flex items-center gap-2 flex-1 max-w-xl">
        <!-- Quick Search Input -->
        <div class="relative flex-1">
          <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <input 
            :value="filterDraft.searchQuery" 
            @input="filterDraft.searchQuery = $event.target.value"
            placeholder="Tìm theo mã, tên chỉ tiêu kế hoạch..." 
            class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none h-[34px]"
          />
        </div>

        <!-- OverlayPanel Advanced Filter Popover -->
        <OverlayPanel 
          title="Bộ Lọc Ma Trận Kế Hoạch Nâng Cao"
          buttonText="Lọc Nâng Cao"
          :activeCount="activeFilterCount"
          widthClass="w-[340px] sm:w-[500px]"
          @apply="execGridFilterSearch"
          @reset="resetGridFilterSearch"
        >
          <div class="space-y-3">
            <!-- Agency Filter -->
            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedAgencyIds" 
                :options="agencyOptions" 
                :isMulti="true" 
                label="Cơ Quan Chủ Trì" 
                placeholder="Tất cả cơ quan"
              />
            </div>

            <!-- Scope Filter -->
            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedScopes" 
                :options="gridScopeOptions" 
                :isMulti="true" 
                label="Phạm Vi (Chung - Riêng)" 
                placeholder="Tất cả phạm vi"
              />
            </div>

            <!-- Section Filter -->
            <div v-if="filterItemType === 'Goal' || (!filterItemType && activeSubTab === 'goals')">
              <SearchableSelect 
                v-model="filterDraft.selectedSections" 
                :options="sectionFilterOptions" 
                :isMulti="true" 
                label="Mục (Phụ lục)" 
                placeholder="Tất cả mục"
              />
            </div>

            <!-- Group Filter -->
            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedGroups" 
                :options="groupFilterOptions" 
                :isMulti="true" 
                label="Nhóm Trọng Tâm" 
                placeholder="Tất cả nhóm"
              />
            </div>

            <!-- Year Range Filter -->
            <div>
              <div class="flex items-center justify-between mb-1">
                <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block">Giai Đoạn</label>
                <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-extrabold text-blue-700">
                  <input type="checkbox" v-model="filterDraft.onlyOngoing" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3 h-3">
                  Thường xuyên
                </label>
              </div>
              <div class="flex items-center gap-1">
                <SearchableSelect 
                  v-model="filterDraft.fromYear" 
                  :options="yearRangeOptions" 
                  :isMulti="false" 
                  placeholder="Từ năm" 
                  class="w-full"
                />
                <span class="text-xs font-bold text-slate-400">➔</span>
                <SearchableSelect 
                  v-model="filterDraft.toYear" 
                  :options="yearRangeOptions" 
                  :isMulti="false" 
                  placeholder="Đến năm" 
                  class="w-full"
                />
              </div>
            </div>

            <!-- Progress/Alert Status Filter -->
            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedStatuses" 
                :options="gridStatusOptions" 
                :isMulti="true" 
                label="Trạng Thái Tiến Độ" 
                placeholder="Tất cả trạng thái"
              />
            </div>
          </div>
        </OverlayPanel>
      </div>
    </div>

    <!-- Loading State -->
    <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu chỉ tiêu kế hoạch..." />

    <!-- Spreadsheet Grid Table Container with Attached Pagination Bar -->
    <div v-else class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col w-full">
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-340px)] custom-scrollbar w-full max-w-full relative">
        <table class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200">
            <tr>
              <!-- STICKY FROZEN COLUMNS 1, 2, 3 -->
              <th class="px-3 py-2.5 border-r border-slate-200 min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-30 bg-slate-100 shadow-[1px_0_0_0_#e2e8f0]">Mã</th>
              <th class="px-3 py-2.5 border-r border-slate-200 min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-30 bg-slate-100 shadow-[1px_0_0_0_#e2e8f0]">
                {{ filterItemType === 'Goal' ? 'Tên Mục Tiêu' : (filterItemType === 'Task' ? 'Tên Nhiệm Vụ' : (activeSubTab === 'goals' ? 'Tên Mục Tiêu' : 'Tên Nhiệm Vụ')) }}
              </th>
              <th class="px-3 py-2.5 border-r border-slate-200 min-w-[130px] w-[130px] max-w-[130px] sticky left-[355px] z-30 bg-slate-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">Đơn Vị Chủ Trì</th>
              
              <!-- SCROLLABLE COLUMNS -->
              
              <!-- Dynamic Year Columns -->
              <th 
                v-for="year in gridData.dynamicYears" 
                :key="year" 
                class="px-4 py-3 border-r border-slate-200 text-center min-w-[120px] bg-blue-50/70 text-blue-900 font-extrabold"
              >
                Năm {{ year }}
              </th>
              <th class="px-4 py-3 text-center min-w-[120px] w-[120px]">Cấu Hình Mốc</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            
            <!-- SUB-TAB 1: GOALS -->
            <template v-if="activeSubTab === 'goals'">
              <tr v-for="(item, itemIdx) in paginatedGridList" :key="item.taskId" class="hover:bg-purple-50/40 transition group">
                <!-- STICKY FROZEN CELLS -->
                <td class="px-3 py-3 border-r border-slate-200 font-extrabold text-purple-900 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[1px_0_0_0_#e2e8f0]">
                  {{ item.code }}
                </td>

                <td class="px-4 py-3 border-r border-slate-200 sticky left-[75px] z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[1px_0_0_0_#e2e8f0] min-w-[280px] w-[280px] max-w-[280px]">
                  <VTooltip 
                    theme="custom-dark"
                    placement="top"
                    :delay="{ show: 1500, hide: 0 }"
                  >
                    <div class="line-clamp-5 font-semibold text-slate-800 text-xs leading-relaxed cursor-help">
                      {{ item.title }}
                    </div>

                    <template #popper>
                      <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                        <span class="font-extrabold text-purple-300 block mb-1 text-[11px] uppercase tracking-wider">🎯 Chi Tiết Mục Tiêu</span>
                        {{ item.title }}
                      </div>
                    </template>
                  </VTooltip>
                </td>

                <td class="px-4 py-3 border-r border-slate-200 font-bold text-slate-700 min-w-[130px] w-[130px] max-w-[130px] sticky left-[355px] z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">
                  {{ item.leadAgencyName || item.leadAgencyCode || 'N/A' }}
                </td>

                <!-- SCROLLABLE CELLS -->

                <!-- Dynamic Year Target Cells with INLINE AUTO-SAVE -->
                <td 
                  v-for="year in gridData.dynamicYears" 
                  :key="year" 
                  class="px-3 py-2 border-r border-slate-200 text-center transition"
                  :class="{ 'bg-slate-100/80 opacity-40': !isYearEnabledForItem(item, year) }"
                >
                  <template v-if="isQuantitative(item)">
                    <div class="relative flex items-center">
                      <input 
                        type="number" 
                        v-model.number="item.yearlyTargets[year]" 
                        :disabled="!isYearEnabledForItem(item, year)"
                        @change="saveYearlyTarget(item, year, $event.target.value)"
                        placeholder="—"
                        class="w-full text-center font-bold text-slate-800 bg-white border border-slate-300 rounded-lg py-1 pl-2 pr-6 focus:ring-2 focus:ring-blue-500 focus:outline-none transition hover:border-blue-400 disabled:bg-slate-100 disabled:text-slate-400 disabled:cursor-not-allowed disabled:border-slate-200 [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                      />
                      <span v-if="isYearEnabledForItem(item, year) && (!item.unitName || item.unitName === '%' || item.unit?.name === '%')" class="absolute right-2 top-1/2 -translate-y-1/2 text-[10px] font-bold text-slate-400 pointer-events-none">%</span>
                    </div>
                  </template>
                  <template v-else>
                    <SearchableSelect 
                      v-model="item.yearlyTargets[year]" 
                      :options="gridStatusCellOptions" 
                      :isMulti="false" 
                      :disabled="!isYearEnabledForItem(item, year)"
                      :clearable="false"
                      @change="val => saveYearlyTarget(item, year, val)" 
                      class="w-full text-xs"
                    />
                  </template>
                </td>

                <!-- Custom Baseline Action -->
                <td class="px-3 py-3 text-center min-w-[120px] w-[120px]">
                  <div class="flex items-center justify-center w-full">
                    <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                      <button 
                        @click="openBaselineModal(item)" 
                        class="w-7 h-7 rounded-lg bg-amber-50 hover:bg-amber-100 border border-amber-200 text-amber-800 flex items-center justify-center transition shadow-2xs"
                      >
                        <svg class="w-3.5 h-3.5 text-amber-700 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                        </svg>
                      </button>
                      <template #popper>
                        <div class="text-[11px] font-bold whitespace-nowrap">
                          ⚙️ Cấu hình / chia chỉ tiêu theo quý (Custom Baseline)
                        </div>
                      </template>
                    </VTooltip>
                  </div>
                </td>
              </tr>

              <tr v-if="filteredGoalsList.length === 0">
                <td :colspan="4 + gridData.dynamicYears.length" class="px-4 py-8 text-center text-slate-400 italic text-xs bg-slate-50/50">
                  Chưa có Mục tiêu (1A) nào trong văn bản này.
                </td>
              </tr>
            </template>

            <!-- SUB-TAB 2: LEVEL 1B TASKS -->
            <template v-else-if="activeSubTab === 'tasks'">
              <tr v-for="(item, itemIdx) in paginatedGridList" :key="item.taskId" class="hover:bg-blue-50/40 transition group">
                <!-- STICKY FROZEN CELLS -->
                <td class="px-3 py-3 border-r border-slate-200 font-extrabold text-blue-900 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[1px_0_0_0_#e2e8f0]">
                  {{ item.code }}
                </td>

                <td class="px-4 py-3 border-r border-slate-200 sticky left-[75px] z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[1px_0_0_0_#e2e8f0] min-w-[280px] w-[280px] max-w-[280px]">
                  <VTooltip 
                    theme="custom-dark"
                    placement="top"
                    :delay="{ show: 1500, hide: 0 }"
                  >
                    <div class="line-clamp-5 font-semibold text-slate-800 text-xs leading-relaxed cursor-help">
                      {{ item.title }}
                    </div>

                    <template #popper>
                      <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                        <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">📋 Chi Tiết Nhiệm Vụ</span>
                        {{ item.title }}
                      </div>
                    </template>
                  </VTooltip>
                </td>

                <td class="px-4 py-3 border-r border-slate-200 font-bold text-slate-700 min-w-[130px] w-[130px] max-w-[130px] sticky left-[355px] z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">
                  {{ item.leadAgencyName || item.leadAgencyCode || 'N/A' }}
                </td>

                <!-- SCROLLABLE CELLS -->

                <!-- Dynamic Year Target Cells with INLINE AUTO-SAVE -->
                <td 
                  v-for="year in gridData.dynamicYears" 
                  :key="year" 
                  class="px-3 py-2 border-r border-slate-200 text-center transition"
                  :class="{ 'bg-slate-100/80 opacity-40': !isYearEnabledForItem(item, year) }"
                >
                  <template v-if="isQuantitative(item)">
                    <div class="relative flex items-center">
                      <input 
                        type="number" 
                        v-model.number="item.yearlyTargets[year]" 
                        :disabled="!isYearEnabledForItem(item, year)"
                        @change="saveYearlyTarget(item, year, $event.target.value)"
                        placeholder="—"
                        class="w-full text-center font-bold text-slate-800 bg-white border border-slate-300 rounded-lg py-1 pl-2 pr-6 focus:ring-2 focus:ring-blue-500 focus:outline-none transition hover:border-blue-400 disabled:bg-slate-100 disabled:text-slate-400 disabled:cursor-not-allowed disabled:border-slate-200 [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                      />
                      <span v-if="isYearEnabledForItem(item, year) && (!item.unitName || item.unitName === '%' || item.unit?.name === '%')" class="absolute right-2 top-1/2 -translate-y-1/2 text-[10px] font-bold text-slate-400 pointer-events-none">%</span>
                    </div>
                  </template>
                  <template v-else>
                    <SearchableSelect 
                      v-model="item.yearlyTargets[year]" 
                      :options="gridStatusCellOptions" 
                      :isMulti="false" 
                      :disabled="!isYearEnabledForItem(item, year)"
                      :clearable="false"
                      @change="val => saveYearlyTarget(item, year, val)" 
                      class="w-full text-xs"
                    />
                  </template>
                </td>

                <!-- Custom Baseline Action -->
                <td class="px-3 py-3 text-center min-w-[120px] w-[120px]">
                  <div class="flex items-center justify-center w-full">
                    <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                      <button 
                        @click="openBaselineModal(item)" 
                        class="w-7 h-7 rounded-lg bg-amber-50 hover:bg-amber-100 border border-amber-200 text-amber-800 flex items-center justify-center transition shadow-2xs"
                      >
                        <svg class="w-3.5 h-3.5 text-amber-700 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                        </svg>
                      </button>
                      <template #popper>
                        <div class="text-[11px] font-bold whitespace-nowrap">
                          ⚙️ Cấu hình / chia chỉ tiêu theo quý (Custom Baseline)
                        </div>
                      </template>
                    </VTooltip>
                  </div>
                </td>
              </tr>

              <tr v-if="filteredTasksList.length === 0">
                <td :colspan="4 + gridData.dynamicYears.length" class="px-4 py-8 text-center text-slate-400 italic text-xs bg-slate-50/50">
                  Chưa có Nhiệm vụ (1B) nào trong văn bản này.
                </td>
              </tr>
            </template>

          </tbody>
        </table>
      </div>

      <!-- Attached Pagination Controls -->
      <div v-if="totalGridItems > 0" class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold w-full">
        <div>
          Hiển thị <span class="font-extrabold text-slate-900">{{ gridPageStart }} - {{ gridPageEnd }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalGridItems }}</span> {{ activeSubTab === 'goals' ? 'Mục tiêu' : 'Nhiệm vụ' }}
        </div>

        <div class="flex items-center gap-2">
          <button 
            @click="gridCurrentPage--" 
            :disabled="gridCurrentPage <= 1"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 disabled:hover:bg-white font-bold transition shadow-2xs"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-black shadow-2xs">
            Trang {{ gridCurrentPage }} / {{ totalGridPages }}
          </span>

          <button 
            @click="gridCurrentPage++" 
            :disabled="gridCurrentPage >= totalGridPages"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 disabled:hover:bg-white font-bold transition shadow-2xs"
          >
            Trang sau ›
          </button>
        </div>
      </div>
    </div>

    <!-- Custom Baseline Modal -->
    <BaselineOverrideModal 
      v-if="selectedTaskForOverride"
      :isOpen="isBaselineModalOpen"
      :taskId="selectedTaskForOverride.taskId"
      :taskCode="selectedTaskForOverride.code"
      :taskTitle="selectedTaskForOverride.title"
      :evaluationType="selectedTaskForOverride.evaluationType"
      :unitName="selectedTaskForOverride.unitName || selectedTaskForOverride.unit?.name || '%'"
      :dynamicYears="gridData.dynamicYears"
      :existingCustomBaseline="selectedTaskForOverride.customBaseline"
      :yearlyTargets="selectedTaskForOverride.yearlyTargets"
      @close="isBaselineModalOpen = false"
      @saved="onBaselineSaved"
    />

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import { authState } from '../services/auth';
import BaselineOverrideModal from './BaselineOverrideModal.vue';
import LoadingSpinner from './LoadingSpinner.vue';
import SearchableSelect from './SearchableSelect.vue';
import OverlayPanel from './OverlayPanel.vue';
import { getApiUrl } from '../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';

const props = defineProps({
  documentId: { type: String, required: true },
  filterItemType: { type: String, default: null } // 'Goal' or 'Task'
});

const activeSubTab = ref('goals');
const isLoading = ref(false);
const isBaselineModalOpen = ref(false);
const selectedTaskForOverride = ref(null);
const agencies = ref([]);
const toastMessage = ref('');
let toastTimeout = null;

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: ag.name }));
});

const sectionFilterOptions = computed(() => {
  return props.filterItemType === 'Goal' ? GOAL_SECTIONS : (props.filterItemType === 'Task' ? TASK_SECTIONS : [...GOAL_SECTIONS, ...TASK_SECTIONS]);
});

const groupFilterOptions = computed(() => {
  return props.filterItemType === 'Goal' ? GOAL_GROUPS : (props.filterItemType === 'Task' ? TASK_GROUPS : [...GOAL_GROUPS, ...TASK_GROUPS]);
});

const gridStatusOptions = ref([
  { value: 'Completed', label: 'Hoàn thành (≥100%)' },
  { value: 'OnTrack', label: 'Đạt kế hoạch (≥80%)' },
  { value: 'Lagging', label: 'Chậm tiến độ (<80%)' },
  { value: 'NoReport', label: 'Chưa có Báo cáo' }
]);

const yearRangeOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));

const gridStatusCellOptions = ref([
  { value: 'NotStarted', label: 'Chưa thực hiện' },
  { value: 'Drafting', label: 'Đang soạn thảo' },
  { value: 'Reviewing', label: 'Đang xin ý kiến' },
  { value: 'Completed', label: 'Hoàn thành' }
]);

const yearOptions = ref([
  { value: 2026, label: 'Năm 2026' },
  { value: 2027, label: 'Năm 2027' },
  { value: 2028, label: 'Năm 2028' },
  { value: 2029, label: 'Năm 2029' },
  { value: 2030, label: 'Năm 2030' }
]);

const gridData = ref({
  documentId: props.documentId,
  documentNumber: '',
  documentName: '',
  startYear: 2026,
  endYear: 2030,
  dynamicYears: [2026, 2027, 2028, 2029, 2030],
  items: []
});

// Advanced Filter States & Draft
const STORAGE_KEY = computed(() => `cdsqg_grid_filters_${props.documentId}`);

const filterDraft = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedScopes: [],
  selectedStatuses: [],
  selectedYears: [],
  selectedSections: [],
  selectedGroups: [],
  fromYear: null,
  toYear: null,
  onlyOngoing: false
});

const appliedFilters = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedScopes: [],
  selectedStatuses: [],
  selectedYears: [],
  selectedSections: [],
  selectedGroups: [],
  fromYear: null,
  toYear: null,
  onlyOngoing: false
});

const gridScopeOptions = computed(() => [
  { value: 'general', label: 'Phạm vi Chung (Tất cả đơn vị)' },
  { value: 'specific', label: 'Phạm vi Riêng (Đơn vị cụ thể)' }
]);

const activeFilterCount = computed(() => {
  let count = 0;
  if (filterDraft.value.selectedAgencyIds?.length) count++;
  if (filterDraft.value.selectedScopes?.length) count++;
  if (filterDraft.value.selectedSections?.length) count++;
  if (filterDraft.value.selectedGroups?.length) count++;
  if (filterDraft.value.fromYear || filterDraft.value.toYear || filterDraft.value.onlyOngoing) count++;
  if (filterDraft.value.selectedStatuses?.length) count++;
  return count;
});

// Pagination States
const gridCurrentPage = ref(1);
const gridPageSize = ref(10);

function loadSavedFilters() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY.value);
    if (saved) {
      const parsed = JSON.parse(saved);
      filterDraft.value = { ...parsed };
      appliedFilters.value = { ...parsed };
    }
  } catch (e) {
    console.error('Error reading localStorage for grid filters:', e);
  }
}

let gridSearchTimer = null;
let gridSearchRequestId = 0;

function execGridFilterSearch() {
  if (gridSearchTimer) clearTimeout(gridSearchTimer);
  gridSearchRequestId++;
  appliedFilters.value = { ...filterDraft.value };
  gridCurrentPage.value = 1;
  try {
    localStorage.setItem(STORAGE_KEY.value, JSON.stringify(appliedFilters.value));
  } catch (e) {
    console.error('Error saving grid filters to localStorage:', e);
  }
}

watch(() => filterDraft.value.searchQuery, (newVal) => {
  if (gridSearchTimer) clearTimeout(gridSearchTimer);
  const currentId = ++gridSearchRequestId;
  gridSearchTimer = setTimeout(() => {
    if (currentId !== gridSearchRequestId) return;
    appliedFilters.value.searchQuery = newVal || '';
    gridCurrentPage.value = 1;
  }, 300);
});

function resetGridFilterSearch() {
  if (gridSearchTimer) clearTimeout(gridSearchTimer);
  gridSearchRequestId++;
  filterDraft.value = {
    searchQuery: '',
    selectedAgencyIds: [],
    selectedScopes: [],
    selectedStatuses: [],
    selectedYears: [],
    selectedSections: [],
    selectedGroups: [],
    fromYear: null,
    toYear: null,
    onlyOngoing: false
  };
  appliedFilters.value = { ...filterDraft.value };
  gridCurrentPage.value = 1;
  try {
    localStorage.removeItem(STORAGE_KEY.value);
  } catch (e) {
    console.error('Error removing grid filters from localStorage:', e);
  }
}

watch(activeSubTab, () => {
  gridCurrentPage.value = 1;
});

function showToast(msg) {
  toastMessage.value = msg;
  if (toastTimeout) clearTimeout(toastTimeout);
  toastTimeout = setTimeout(() => {
    toastMessage.value = '';
  }, 3000);
}

async function loadAgencies() {
  try {
    const res = await fetch(getApiUrl('/api/agencies'));
    if (res.ok) {
      const data = await res.json();
      agencies.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (e) {
    // Silent catch
  }
}

const goalsList = computed(() => {
  if (!gridData.value || !gridData.value.items) return [];
  const list = gridData.value.items.filter(x => x.itemType === 'Goal' || x.itemType === 1 || x.itemType === '1');
  return list.sort((a, b) => new Date(b.lastUpdated || b.createdAt || 0) - new Date(a.lastUpdated || a.createdAt || 0));
});

const tasksList = computed(() => {
  if (!gridData.value || !gridData.value.items) return [];
  const list = gridData.value.items.filter(x => x.itemType === 'Task' || x.itemType === 2 || x.itemType === '2');
  return list.sort((a, b) => new Date(b.lastUpdated || b.createdAt || 0) - new Date(a.lastUpdated || a.createdAt || 0));
});

function filterGridItem(item) {
  // 0. Non-Admin Focal Point Agency Scoping Filter:
  // - Sub-Agency (ParentId != null): ONLY show items assigned to this Sub-Agency (cannot view parent agency data)
  // - Parent Agency (ParentId == null): Show items assigned to Parent Agency AND all of its Sub-Agencies (views all)
  if (!authState.isAdmin.value && authState.user.value?.agencyId) {
    const userAgencyId = String(authState.user.value.agencyId).toLowerCase();
    const userAgency = agencies.value.find(a => String(a.id).toLowerCase() === userAgencyId);

    const scopedAgencyIds = [userAgencyId];
    if (userAgency && !userAgency.parentId) {
      const childIds = agencies.value
        .filter(a => a.parentId && String(a.parentId).toLowerCase() === userAgencyId)
        .map(a => String(a.id).toLowerCase());
      scopedAgencyIds.push(...childIds);
    }

    const itemLeadId = item.leadAgencyId ? String(item.leadAgencyId).toLowerCase() : '';
    const itemAssignedId = item.assignedAgencyId ? String(item.assignedAgencyId).toLowerCase() : '';
    const itemCoordIds = (item.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());

    const isParentAgency = !userAgency || !userAgency.parentId;
    const isGeneral = isParentAgency && (item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES' || itemLeadId === '00000000-0000-0000-0000-000000009999' || (item.leadAgencyName && item.leadAgencyName.toLowerCase().trim() === 'các bộ, ngành, địa phương'));
    const isLead = scopedAgencyIds.includes(itemLeadId);
    const isAssigned = itemAssignedId && scopedAgencyIds.includes(itemAssignedId);
    const isCoord = itemCoordIds.some(id => scopedAgencyIds.includes(id));
    const isSubMatch = item.subItems?.some(s => {
      const sLeadId = s.leadAgencyId ? String(s.leadAgencyId).toLowerCase() : '';
      const sAssignedId = s.assignedAgencyId ? String(s.assignedAgencyId).toLowerCase() : '';
      const sCoordIds = (s.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());
      const sIsGeneral = isParentAgency && (s.isGeneralTask || s.leadAgencyCode === 'ALL_AGENCIES' || sLeadId === '00000000-0000-0000-0000-000000009999');
      return sIsGeneral || scopedAgencyIds.includes(sLeadId) || (sAssignedId && scopedAgencyIds.includes(sAssignedId)) || sCoordIds.some(id => scopedAgencyIds.includes(id));
    });

    if (!isGeneral && !isLead && !isAssigned && !isCoord && !isSubMatch) return false;
  }

  // 1. Search Query Filter - Real-time debounced matching
  const q = (appliedFilters.value.searchQuery || '').toLowerCase().trim();
  if (q) {
    const matchQ = (item.code && item.code.toLowerCase().includes(q)) ||
                   (item.title && item.title.toLowerCase().includes(q)) ||
                   (item.leadAgencyCode && item.leadAgencyCode.toLowerCase().includes(q)) ||
                   (item.leadAgencyName && item.leadAgencyName.toLowerCase().includes(q)) ||
                   (item.category && item.category.toLowerCase().includes(q));
    if (!matchQ) return false;
  }

  // 2. Agency Filter (Multi-select)
  if (appliedFilters.value.selectedAgencyIds && appliedFilters.value.selectedAgencyIds.length > 0) {
    const selectedAgencies = agencies.value.filter(a => appliedFilters.value.selectedAgencyIds.includes(a.id));
    const matchAg = selectedAgencies.some(ag => 
      item.leadAgencyId === ag.id || 
      item.leadAgencyCode === ag.code || 
      item.leadAgencyName === ag.name
    );
    if (!matchAg) return false;
  }

  // 2.5 Scope Filter (Multi-select)
  if (appliedFilters.value.selectedScopes && appliedFilters.value.selectedScopes.length > 0) {
    const isGeneral = item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES' || item.leadAgencyId === '00000000-0000-0000-0000-000000009999';
    const matchGen = appliedFilters.value.selectedScopes.includes('general') && isGeneral;
    const matchSpec = appliedFilters.value.selectedScopes.includes('specific') && !isGeneral;
    if (!matchGen && !matchSpec) return false;
  }

  // 3. Section Filter (Multi-select)
  if (appliedFilters.value.selectedSections && appliedFilters.value.selectedSections.length > 0) {
    if (!appliedFilters.value.selectedSections.includes(item.section)) return false;
  }

  // 4. Group Filter (Multi-select)
  if (appliedFilters.value.selectedGroups && appliedFilters.value.selectedGroups.length > 0) {
    if (!appliedFilters.value.selectedGroups.includes(item.group)) return false;
  }

  // 5. Ongoing Tasks / Goals Filter
  if (appliedFilters.value.onlyOngoing && !item.isOngoing) {
    return false;
  }

  // 6. Year Range Filter (From Year -> To Year)
  if (appliedFilters.value.fromYear || appliedFilters.value.toYear) {
    const fYr = appliedFilters.value.fromYear ? Number(appliedFilters.value.fromYear) : 2026;
    const tYr = appliedFilters.value.toYear ? Number(appliedFilters.value.toYear) : 2030;
    if (!item.isOngoing) {
      const startY = item.startDate ? new Date(item.startDate).getFullYear() : 2026;
      const dueY = item.dueDate ? new Date(item.dueDate).getFullYear() : startY;
      if (startY > tYr || dueY < fYr) return false;
    }
  }

  // 7. Progress / Alert Status Filter (Multi-select)
  if (appliedFilters.value.selectedStatuses && appliedFilters.value.selectedStatuses.length > 0) {
    const selectedStatuses = appliedFilters.value.selectedStatuses;
    const latestVal = item.latestProgressValue;
    const latestStatus = item.latestProgressStatus;
    
    let matchStatus = false;
    for (const statusFilter of selectedStatuses) {
      if (statusFilter === 'Completed') {
        if ((latestVal !== null && latestVal !== undefined && latestVal >= 100) || latestStatus === 'Completed') {
          matchStatus = true; break;
        }
      } else if (statusFilter === 'OnTrack') {
        if ((latestVal !== null && latestVal !== undefined && latestVal >= 80 && latestVal < 100) || latestStatus === 'OnTrack' || latestStatus === 'Reviewing') {
          matchStatus = true; break;
        }
      } else if (statusFilter === 'Lagging') {
        if ((latestVal !== null && latestVal !== undefined && latestVal < 80) || latestStatus === 'Lagging' || latestStatus === 'NotStarted') {
          matchStatus = true; break;
        }
      } else if (statusFilter === 'NoReport') {
        if (latestVal === null && latestStatus === null) {
          matchStatus = true; break;
        }
      }
    }
    if (!matchStatus) return false;
  }

  // 8. Year Filter (Multi-select)
  if (appliedFilters.value.selectedYears && appliedFilters.value.selectedYears.length > 0) {
    const matchYr = appliedFilters.value.selectedYears.some(yr => {
      const yrNum = Number(yr);
      return item.yearlyTargets && item.yearlyTargets[yrNum] !== undefined && item.yearlyTargets[yrNum] !== null && item.yearlyTargets[yrNum] !== '';
    });
    if (!matchYr) return false;
  }

  return true;
}

const filteredGoalsList = computed(() => {
  return goalsList.value.filter(filterGridItem);
});

const filteredTasksList = computed(() => {
  return tasksList.value.filter(filterGridItem);
});

const activeGridList = computed(() => {
  return activeSubTab.value === 'goals' ? filteredGoalsList.value : filteredTasksList.value;
});

const totalGridItems = computed(() => activeGridList.value.length);
const totalGridPages = computed(() => Math.ceil(totalGridItems.value / gridPageSize.value) || 1);

const paginatedGridList = computed(() => {
  const start = (gridCurrentPage.value - 1) * gridPageSize.value;
  return activeGridList.value.slice(start, start + gridPageSize.value);
});

const gridPageStart = computed(() => totalGridItems.value === 0 ? 0 : (gridCurrentPage.value - 1) * gridPageSize.value + 1);
const gridPageEnd = computed(() => Math.min(gridCurrentPage.value * gridPageSize.value, totalGridItems.value));

function isQuantitative(item) {
  if (item.itemType === 'Task' || item.itemType === 2 || item.itemType === '2') {
    return false;
  }
  return item.evaluationType === 'Quantitative' || item.evaluationType === 1 || item.evaluationType === '1';
}

function isYearEnabledForItem(item, year) {
  if (!item) return true;
  if (item.isOngoing) return true;

  const yr = Number(year);
  let startYr = 2026;
  let dueYr = 2030;

  if (item.startDate) {
    const dt = new Date(item.startDate);
    if (!isNaN(dt.getTime())) startYr = dt.getFullYear();
  } else if (item.startYear) {
    startYr = Number(item.startYear);
  }

  if (item.dueDate) {
    const dt = new Date(item.dueDate);
    if (!isNaN(dt.getTime())) dueYr = dt.getFullYear();
  } else if (item.dueYear) {
    dueYr = Number(item.dueYear);
  }

  return yr >= startYr && yr <= dueYr;
}

function openBaselineModal(item) {
  selectedTaskForOverride.value = item;
  isBaselineModalOpen.value = true;
}

function onBaselineSaved(newMilestones) {
  if (selectedTaskForOverride.value) {
    selectedTaskForOverride.value.customBaseline = { ...newMilestones };
  }
  showToast(`Đã lưu mốc chỉ tiêu Custom Baseline thành công!`);
}

async function saveYearlyTarget(item, year, val) {
  const isQuant = isQuantitative(item);
  const payload = {
    year: Number(year),
    targetQuantity: isQuant ? (val !== '' && val !== null ? Number(val) : null) : null,
    targetQualitativeStatus: !isQuant ? val : null
  };

  try {
    const res = await fetch(getApiUrl(`/api/planning/tasks/${item.taskId}/yearly-target`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      showToast(`Đã tự động lưu chỉ tiêu ${item.code} năm ${year}: ${val}`);
    } else {
      showToast(`Lỗi khi tự động lưu chỉ tiêu năm ${year}`);
    }
  } catch (e) {
    showToast(`Không thể kết nối máy chủ để lưu chỉ tiêu`);
  }
}

async function loadGridData() {
  if (!props.documentId) return;
  isLoading.value = true;
  try {
    const currentAgencyId = authState.user.value?.agencyId || '';
    const agencyParam = currentAgencyId ? `?agencyId=${currentAgencyId}` : '';
    const res = await fetch(getApiUrl(`/api/planning/documents/${props.documentId}/grid${agencyParam}`));
    if (res.ok) {
      const data = await res.json();
      gridData.value = {
        documentId: data.documentId || props.documentId,
        documentNumber: data.documentNumber || '',
        documentName: data.documentName || '',
        startYear: data.startYear || 2026,
        endYear: data.endYear || 2030,
        dynamicYears: data.dynamicYears || [2026, 2027, 2028, 2029, 2030],
        items: data.items || []
      };
    }
  } catch (e) {
    console.error('Error fetching planning grid data from backend API:', e);
  } finally {
    isLoading.value = false;
  }
}

watch(() => props.filterItemType, (newType) => {
  if (newType === 'Goal' || newType === 'goals' || newType === '1') {
    activeSubTab.value = 'goals';
  } else if (newType === 'Task' || newType === 'tasks' || newType === '2') {
    activeSubTab.value = 'tasks';
  }
  loadGridData();
}, { immediate: true });

watch(() => props.documentId, () => {
  loadGridData();
}, { immediate: true });

onMounted(() => {
  loadSavedFilters();
  loadAgencies();
  loadGridData();
});

defineExpose({
  loadGridData
});
</script>
