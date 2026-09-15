<template>
  <div class="w-full space-y-5 font-sans relative">
    
    <!-- Subtle Floating Toast Notification -->
    <transition name="fade">
      <div v-if="toastMessage" class="fixed bottom-6 right-6 z-50 bg-slate-900/90 text-white text-xs font-bold px-4 py-3 rounded-2xl shadow-xl backdrop-blur-sm flex items-center gap-2 border border-slate-700 animate-in fade-in slide-in-from-bottom-2">
        <span class="text-emerald-400 font-black text-sm">✓</span>
        <span>{{ toastMessage }}</span>
      </div>
    </transition>

    <!-- ADVANCED FILTER BAR WITH SEARCH BUTTON & LOCALSTORAGE PERSISTENCE -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 bg-slate-50 p-4 rounded-2xl border border-slate-200 w-full items-end">
      <!-- Search Query -->
      <div>
        <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Tìm Kiếm Từ Khóa</label>
        <div class="relative">
          <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <input 
            v-model="filterDraft.searchQuery" 
            @keyup.enter="execGridFilterSearch"
            placeholder="Mã, tên, phân nhóm..." 
            class="w-full text-xs font-semibold pl-9 pr-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
          />
        </div>
      </div>

      <!-- Agency Filter -->
      <div>
        <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Cơ Quan Chủ Trì</label>
        <select 
          v-model="filterDraft.selectedAgencyId" 
          class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none"
        >
          <option value="all">Tất cả cơ quan / Bộ ngành</option>
          <option v-for="ag in agencies" :key="ag.id" :value="ag.id">
            {{ ag.code }} - {{ ag.name }}
          </option>
        </select>
      </div>

      <!-- Progress/Alert Status Filter -->
      <div>
        <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Trạng Thái Tiến Độ</label>
        <select 
          v-model="filterDraft.selectedStatusFilter" 
          class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none"
        >
          <option value="all">Tất cả trạng thái</option>
          <option value="Completed">Hoàn thành (≥100%)</option>
          <option value="OnTrack">Đạt kế hoạch (≥80%)</option>
          <option value="Lagging">Chậm tiến độ (<80%)</option>
          <option value="NoReport">Chưa có Báo cáo</option>
        </select>
      </div>

      <!-- Year Filter -->
      <div>
        <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Năm Chỉ Tiêu</label>
        <select 
          v-model="filterDraft.selectedYearFilter" 
          class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none"
        >
          <option value="all">Tất cả các năm (2026-2030)</option>
          <option v-for="y in [2026, 2027, 2028, 2029, 2030]" :key="y" :value="y">Năm {{ y }}</option>
        </select>
      </div>

      <!-- Filter Action Buttons -->
      <div class="flex items-center gap-2">
        <button 
          type="button" 
          @click="execGridFilterSearch" 
          class="w-full py-2 px-3 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm transition flex items-center justify-center gap-1.5"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <span>Tìm Kiếm</span>
        </button>

        <button 
          type="button" 
          @click="resetGridFilterSearch" 
          class="px-3 py-2 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition shrink-0"
          title="Đặt lại bộ lọc"
        >
          ↺
        </button>
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
              <th class="px-3 py-3 border-r border-slate-200 min-w-[85px] w-[85px] max-w-[85px] sticky left-0 z-30 bg-slate-100 shadow-[1px_0_0_0_#e2e8f0]">Mã</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[295px] w-[295px] max-w-[295px] sticky left-[85px] z-30 bg-slate-100 shadow-[1px_0_0_0_#e2e8f0]">Tên Mục Tiêu / Nhiệm Vụ</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[130px] w-[130px] max-w-[130px] sticky left-[380px] z-30 bg-slate-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">Đơn Vị Chủ Trì</th>
              
              <!-- SCROLLABLE COLUMNS -->
              <th class="px-4 py-3 border-r border-slate-200 min-w-[140px] bg-purple-50/50 text-purple-900 font-extrabold">Cơ Quan Phối Hợp</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[110px]">Loại Đánh Giá</th>
              
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
                <td class="px-3 py-3 border-r border-slate-200 font-extrabold text-purple-900 whitespace-nowrap min-w-[85px] w-[85px] max-w-[85px] sticky left-0 z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[1px_0_0_0_#e2e8f0]">
                  {{ item.code }}
                </td>

                <td class="px-4 py-3 border-r border-slate-200 sticky left-[85px] z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[1px_0_0_0_#e2e8f0] min-w-[295px] w-[295px] max-w-[295px]">
                  <VTooltip 
                    v-if="item.title && item.title.length > 40"
                    theme="custom-dark"
                    placement="top"
                    :delay="{ show: 1000, hide: 0 }"
                  >
                    <div class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed cursor-help">
                      {{ item.title }}
                    </div>

                    <template #popper>
                      <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[400px]">
                        <span class="font-extrabold text-purple-300 block mb-1 text-[11px] uppercase tracking-wider">🎯 Chi Tiết Mục Tiêu</span>
                        {{ item.title }}
                      </div>
                    </template>
                  </VTooltip>
                  <div v-else class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed">
                    {{ item.title }}
                  </div>
                </td>

                <td class="px-4 py-3 border-r border-slate-200 font-bold text-slate-700 min-w-[130px] w-[130px] max-w-[130px] sticky left-[380px] z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">
                  {{ item.leadAgencyCode || 'N/A' }}
                </td>

                <!-- SCROLLABLE CELLS -->
                <td class="px-4 py-3 border-r border-slate-200">
                  <div class="flex flex-wrap gap-1">
                    <span v-for="coord in item.coordinatingAgencyCodes" :key="coord" class="px-2 py-0.5 bg-purple-100 text-purple-800 rounded font-extrabold text-[10px]">
                      {{ coord }}
                    </span>
                    <span v-if="!item.coordinatingAgencyCodes || item.coordinatingAgencyCodes.length === 0" class="text-slate-400 italic text-xs">—</span>
                  </div>
                </td>

                <td class="px-4 py-3 border-r border-slate-200">
                  <span :class="['px-2 py-0.5 rounded text-[11px] font-semibold', isQuantitative(item) ? 'bg-emerald-50 text-emerald-700' : 'bg-indigo-50 text-indigo-700']">
                    {{ isQuantitative(item) ? 'Định lượng' : 'Định tính' }}
                  </span>
                </td>

                <!-- Dynamic Year Target Cells with INLINE AUTO-SAVE -->
                <td v-for="year in gridData.dynamicYears" :key="year" class="px-3 py-2 border-r border-slate-200 text-center">
                  <template v-if="isQuantitative(item)">
                    <div class="relative">
                      <input 
                        type="number" 
                        v-model.number="item.yearlyTargets[year]" 
                        @change="saveYearlyTarget(item, year, $event.target.value)"
                        placeholder="— %"
                        class="w-full text-center font-bold text-slate-800 bg-white border border-slate-300 rounded-lg py-1 px-2 focus:ring-2 focus:ring-blue-500 focus:outline-none transition hover:border-blue-400"
                      />
                    </div>
                  </template>
                  <template v-else>
                    <select 
                      v-model="item.yearlyTargets[year]" 
                      @change="saveYearlyTarget(item, year, $event.target.value)"
                      class="w-full text-xs font-bold bg-white border border-slate-300 rounded-lg py-1 px-1 focus:ring-2 focus:ring-blue-500 transition hover:border-blue-400"
                    >
                      <option value="NotStarted">Chưa thực hiện</option>
                      <option value="Drafting">Đang soạn thảo</option>
                      <option value="Reviewing">Đang xin ý kiến</option>
                      <option value="Completed">Hoàn thành</option>
                    </select>
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
                <td :colspan="6 + gridData.dynamicYears.length" class="px-4 py-8 text-center text-slate-400 italic text-xs bg-slate-50/50">
                  Chưa có Mục tiêu (1A) nào trong văn bản này.
                </td>
              </tr>
            </template>

            <!-- SUB-TAB 2: LEVEL 1B TASKS -->
            <template v-else-if="activeSubTab === 'tasks'">
              <tr v-for="(item, itemIdx) in paginatedGridList" :key="item.taskId" class="hover:bg-blue-50/40 transition group">
                <!-- STICKY FROZEN CELLS -->
                <td class="px-3 py-3 border-r border-slate-200 font-extrabold text-blue-900 whitespace-nowrap min-w-[85px] w-[85px] max-w-[85px] sticky left-0 z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[1px_0_0_0_#e2e8f0]">
                  {{ item.code }}
                </td>

                <td class="px-4 py-3 border-r border-slate-200 sticky left-[85px] z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[1px_0_0_0_#e2e8f0] min-w-[295px] w-[295px] max-w-[295px]">
                  <VTooltip 
                    v-if="item.title && item.title.length > 40"
                    theme="custom-dark"
                    placement="top"
                    :delay="{ show: 1000, hide: 0 }"
                  >
                    <div class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed cursor-help">
                      {{ item.title }}
                    </div>

                    <template #popper>
                      <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[400px]">
                        <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">📋 Chi Tiết Nhiệm Vụ</span>
                        {{ item.title }}
                      </div>
                    </template>
                  </VTooltip>
                  <div v-else class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed">
                    {{ item.title }}
                  </div>
                </td>

                <td class="px-4 py-3 border-r border-slate-200 font-bold text-slate-700 min-w-[130px] w-[130px] max-w-[130px] sticky left-[380px] z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">
                  {{ item.leadAgencyCode || 'N/A' }}
                </td>

                <!-- SCROLLABLE CELLS -->
                <td class="px-4 py-3 border-r border-slate-200">
                  <div class="flex flex-wrap gap-1">
                    <span v-for="coord in item.coordinatingAgencyCodes" :key="coord" class="px-2 py-0.5 bg-purple-100 text-purple-800 rounded font-extrabold text-[10px]">
                      {{ coord }}
                    </span>
                    <span v-if="!item.coordinatingAgencyCodes || item.coordinatingAgencyCodes.length === 0" class="text-slate-400 italic text-xs">—</span>
                  </div>
                </td>

                <td class="px-4 py-3 border-r border-slate-200">
                  <span :class="['px-2 py-0.5 rounded text-[11px] font-semibold', isQuantitative(item) ? 'bg-emerald-50 text-emerald-700' : 'bg-indigo-50 text-indigo-700']">
                    {{ isQuantitative(item) ? 'Định lượng' : 'Định tính' }}
                  </span>
                </td>

                <!-- Dynamic Year Target Cells with INLINE AUTO-SAVE -->
                <td v-for="year in gridData.dynamicYears" :key="year" class="px-3 py-2 border-r border-slate-200 text-center">
                  <template v-if="isQuantitative(item)">
                    <div class="relative">
                      <input 
                        type="number" 
                        v-model.number="item.yearlyTargets[year]" 
                        @change="saveYearlyTarget(item, year, $event.target.value)"
                        placeholder="— %"
                        class="w-full text-center font-bold text-slate-800 bg-white border border-slate-300 rounded-lg py-1 px-2 focus:ring-2 focus:ring-blue-500 focus:outline-none transition hover:border-blue-400"
                      />
                    </div>
                  </template>
                  <template v-else>
                    <select 
                      v-model="item.yearlyTargets[year]" 
                      @change="saveYearlyTarget(item, year, $event.target.value)"
                      class="w-full text-xs font-bold bg-white border border-slate-300 rounded-lg py-1 px-1 focus:ring-2 focus:ring-blue-500 transition hover:border-blue-400"
                    >
                      <option value="NotStarted">Chưa thực hiện</option>
                      <option value="Drafting">Đang soạn thảo</option>
                      <option value="Reviewing">Đang xin ý kiến</option>
                      <option value="Completed">Hoàn thành</option>
                    </select>
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
                <td :colspan="6 + gridData.dynamicYears.length" class="px-4 py-8 text-center text-slate-400 italic text-xs bg-slate-50/50">
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
      :unitName="selectedTaskForOverride.unitName || '%'"
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
import BaselineOverrideModal from './BaselineOverrideModal.vue';
import LoadingSpinner from './LoadingSpinner.vue';

const props = defineProps({
  documentId: { type: String, required: true },
  filterItemType: { type: String, default: null } // 'Goal' or 'Task'
});

const activeSubTab = ref('goals');

watch(() => props.filterItemType, (newType) => {
  if (newType === 'Goal' || newType === 'goals' || newType === '1') {
    activeSubTab.value = 'goals';
  } else if (newType === 'Task' || newType === 'tasks' || newType === '2') {
    activeSubTab.value = 'tasks';
  }
  loadGridData();
}, { immediate: true });

const gridData = ref({
  documentId: props.documentId,
  documentNumber: '',
  documentName: '',
  startYear: 2026,
  endYear: 2030,
  dynamicYears: [2026, 2027, 2028, 2029, 2030],
  items: []
});

const isLoading = ref(false);
const isBaselineModalOpen = ref(false);
const selectedTaskForOverride = ref(null);

// Advanced Filter States & Draft
const STORAGE_KEY = computed(() => `cdsqg_grid_filters_${props.documentId}`);

const filterDraft = ref({
  searchQuery: '',
  selectedAgencyId: 'all',
  selectedStatusFilter: 'all',
  selectedYearFilter: 'all'
});

const appliedFilters = ref({
  searchQuery: '',
  selectedAgencyId: 'all',
  selectedStatusFilter: 'all',
  selectedYearFilter: 'all'
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

function execGridFilterSearch() {
  appliedFilters.value = { ...filterDraft.value };
  gridCurrentPage.value = 1;
  try {
    localStorage.setItem(STORAGE_KEY.value, JSON.stringify(appliedFilters.value));
  } catch (e) {
    console.error('Error saving grid filters to localStorage:', e);
  }
}

function resetGridFilterSearch() {
  filterDraft.value = {
    searchQuery: '',
    selectedAgencyId: 'all',
    selectedStatusFilter: 'all',
    selectedYearFilter: 'all'
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

const agencies = ref([]);

const toastMessage = ref('');
let toastTimeout = null;

function showToast(msg) {
  toastMessage.value = msg;
  if (toastTimeout) clearTimeout(toastTimeout);
  toastTimeout = setTimeout(() => {
    toastMessage.value = '';
  }, 3000);
}

async function loadAgencies() {
  try {
    const res = await fetch('http://localhost:5000/api/planning/agencies');
    if (res.ok) {
      agencies.value = await res.json();
    }
  } catch (e) {
    console.error('Error loading agencies:', e);
  }
}

const goalsList = computed(() => {
  if (!gridData.value || !gridData.value.items) return [];
  return gridData.value.items.filter(x => x.itemType === 'Goal' || x.itemType === 1 || x.itemType === '1');
});

const tasksList = computed(() => {
  if (!gridData.value || !gridData.value.items) return [];
  return gridData.value.items.filter(x => x.itemType === 'Task' || x.itemType === 2 || x.itemType === '2');
});

function filterGridItem(item) {
  // 1. Search Query Filter
  if (appliedFilters.value.searchQuery && appliedFilters.value.searchQuery.trim()) {
    const q = appliedFilters.value.searchQuery.toLowerCase().trim();
    const matchQ = (item.code && item.code.toLowerCase().includes(q)) ||
                   (item.title && item.title.toLowerCase().includes(q)) ||
                   (item.leadAgencyCode && item.leadAgencyCode.toLowerCase().includes(q)) ||
                   (item.leadAgencyName && item.leadAgencyName.toLowerCase().includes(q)) ||
                   (item.category && item.category.toLowerCase().includes(q));
    if (!matchQ) return false;
  }

  // 2. Agency Filter
  if (appliedFilters.value.selectedAgencyId !== 'all') {
    const agId = appliedFilters.value.selectedAgencyId;
    const selectedAg = agencies.value.find(a => a.id === agId);
    if (selectedAg) {
      const matchAg = item.leadAgencyId === selectedAg.id || 
                      item.leadAgencyCode === selectedAg.code || 
                      item.leadAgencyName === selectedAg.name;
      if (!matchAg) return false;
    }
  }

  // 3. Progress / Alert Status Filter
  if (appliedFilters.value.selectedStatusFilter !== 'all') {
    const statusFilter = appliedFilters.value.selectedStatusFilter;
    const latestVal = item.latestProgressValue;
    const latestStatus = item.latestProgressStatus;
    if (statusFilter === 'Completed') {
      if (latestVal !== null && latestVal !== undefined) {
        if (latestVal < 100) return false;
      } else if (latestStatus !== 'Completed') {
        return false;
      }
    } else if (statusFilter === 'OnTrack') {
      if (latestVal !== null && latestVal !== undefined) {
        if (latestVal < 80 || latestVal >= 100) return false;
      } else if (latestStatus !== 'OnTrack' && latestStatus !== 'Reviewing') {
        return false;
      }
    } else if (statusFilter === 'Lagging') {
      if (latestVal !== null && latestVal !== undefined) {
        if (latestVal >= 80) return false;
      } else if (latestStatus !== 'Lagging' && latestStatus !== 'NotStarted') {
        return false;
      }
    } else if (statusFilter === 'NoReport') {
      if (latestVal !== null || latestStatus !== null) return false;
    }
  }

  // 4. Year Filter
  if (appliedFilters.value.selectedYearFilter !== 'all') {
    const yr = Number(appliedFilters.value.selectedYearFilter);
    if (item.yearlyTargets && (item.yearlyTargets[yr] === undefined || item.yearlyTargets[yr] === null || item.yearlyTargets[yr] === '')) {
      return false;
    }
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
  return item.evaluationType === 'Quantitative' || item.evaluationType === 1 || item.evaluationType === '1';
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
    const res = await fetch(`http://localhost:5000/api/planning/tasks/${item.taskId}/yearly-target`, {
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
    const res = await fetch(`http://localhost:5000/api/planning/documents/${props.documentId}/grid`);
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
