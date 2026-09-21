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
        Xuất File Excel
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
        <div class="text-xs font-black truncate">📊 Tiến độ các bộ & địa phương</div>
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

    <!-- FILTER BAR WITH OVERLAY PANEL -->
    <div class="bg-white p-3 rounded-2xl shadow-sm border border-slate-200/80 flex flex-col sm:flex-row items-center justify-between gap-3 w-full">
      <div class="flex flex-wrap items-center gap-2 w-full sm:w-auto flex-1">
        <div class="relative flex-1 min-w-[200px] max-w-md">
          <input 
            :value="filterDraft.searchQuery" 
            @input="filterDraft.searchQuery = $event.target.value"
            placeholder="Tìm theo mã, tên mục tiêu / nhiệm vụ..." 
            class="w-full text-xs font-semibold pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none min-h-[36px]" 
          />
          <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
        </div>

        <!-- Quick Item Type Select Dropdown -->
        <select 
          v-model="quickItemType" 
          @change="onQuickItemTypeChange"
          class="py-2 px-3 text-xs bg-slate-50 hover:bg-slate-100 border border-slate-200 rounded-xl font-extrabold text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer min-h-[36px] shadow-2xs shrink-0"
          title="Lọc loại đối tượng (Tất cả / Mục tiêu / Nhiệm vụ)"
        >
          <option value="all">Tất cả (Mục tiêu & Nhiệm vụ)</option>
          <option value="Goal">🎯 Chỉ Mục tiêu</option>
          <option value="Task">📋 Chỉ Nhiệm vụ</option>
        </select>

        <!-- OverlayPanel Advanced Filter Popover -->
        <OverlayPanel 
          title="Bộ Lọc Báo Cáo Nâng Cao"
          buttonText="Lọc Nâng Cao"
          :activeCount="activeFilterCount"
          widthClass="w-[340px] sm:w-[500px]"
          @apply="execFilterSearch"
          @reset="resetReportFilters"
        >
          <div class="space-y-3">
            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedAgencyIds" 
                :options="agencyOptions" 
                :isMulti="true" 
                label="Cơ Quan / Đơn Vị" 
                placeholder="Tất cả cơ quan / đơn vị"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedScopes" 
                :options="reportScopeOptions" 
                :isMulti="true" 
                label="Phạm Vi (Chung - Riêng)" 
                placeholder="Tất cả phạm vi"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedSections" 
                :options="sectionOptions" 
                :isMulti="true" 
                label="Mục (Phụ lục)" 
                placeholder="Tất cả mục"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedGroups" 
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
                  <input type="checkbox" v-model="filterDraft.onlyOngoing" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3.5 h-3.5">
                  Thường xuyên
                </label>
              </div>
              <div class="flex items-center gap-2">
                <SearchableSelect 
                  v-model="filterDraft.fromYear" 
                  :options="yearOptions" 
                  :isMulti="false" 
                  placeholder="Từ năm" 
                  class="w-full"
                />
                <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                <SearchableSelect 
                  v-model="filterDraft.toYear" 
                  :options="yearOptions" 
                  :isMulti="false" 
                  placeholder="Đến năm" 
                  class="w-full"
                />
              </div>
            </div>

            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedItemTypes" 
                :options="itemTypeOptions" 
                :isMulti="true" 
                label="Loại Đối Tượng (Mục tiêu / Nhiệm vụ)" 
                placeholder="Tất cả loại đối tượng"
              />
            </div>
          </div>
        </OverlayPanel>
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
              <th class="p-3">Tên Cơ Quan / Địa Phương</th>
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
              <td class="p-3 font-bold text-slate-900">{{ ag.name }}</td>
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
              <td colspan="9" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
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
              <td class="p-3 font-semibold text-slate-900">
                <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1500, hide: 0 }">
                  <div class="line-clamp-5 font-semibold text-slate-900 leading-relaxed cursor-help">
                    {{ item.title }}
                  </div>
                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[260px] max-w-[420px] p-1">
                      <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">📋 Nội dung chi tiết</span>
                      {{ item.title }}
                    </div>
                  </template>
                </VTooltip>
              </td>
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
              <td class="p-3 font-semibold text-slate-900">
                <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1500, hide: 0 }">
                  <div class="line-clamp-5 font-semibold text-slate-900 leading-relaxed cursor-help">
                    {{ item.title }}
                  </div>
                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[260px] max-w-[420px] p-1">
                      <span class="font-extrabold text-indigo-300 block mb-1 text-[11px] uppercase tracking-wider">📄 Chi tiết tiêu đề</span>
                      {{ item.title }}
                    </div>
                  </template>
                </VTooltip>
              </td>
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
              <td class="p-3 font-semibold text-slate-900">
                <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1500, hide: 0 }">
                  <div class="line-clamp-5 font-semibold text-slate-900 leading-relaxed cursor-help">
                    {{ item.title }}
                  </div>
                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[260px] max-w-[420px] p-1">
                      <span class="font-extrabold text-purple-300 block mb-1 text-[11px] uppercase tracking-wider">🌐 Nội dung nhiệm vụ</span>
                      {{ item.title }}
                    </div>
                  </template>
                </VTooltip>
              </td>
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
import OverlayPanel from '../components/OverlayPanel.vue';
import { getApiUrl } from '../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';
import { exportToExcel, exportFormattedReportExcel } from '../utils/excelExport';
import { authState } from '../services/auth';

const activeReportType = ref('summary');
const isLoading = ref(false);
const agencies = ref([]);
const allItems = ref([]);
const metrics = ref({});

const filterDraft = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedScopes: [],
  selectedItemTypes: [],
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
  selectedItemTypes: [],
  selectedSections: [],
  selectedGroups: [],
  fromYear: null,
  toYear: null,
  onlyOngoing: false
});

const reportScopeOptions = computed(() => [
  { value: 'general', label: 'Phạm vi Chung (Tất cả đơn vị)' },
  { value: 'specific', label: 'Phạm vi Riêng (Đơn vị cụ thể)' }
]);

const currentPage = ref(1);
const pageSize = ref(10);

const activeFilterCount = computed(() => {
  let count = 0;
  if (filterDraft.value.selectedAgencyIds?.length) count++;
  if (filterDraft.value.selectedScopes?.length) count++;
  if (filterDraft.value.selectedSections?.length) count++;
  if (filterDraft.value.selectedGroups?.length) count++;
  if (filterDraft.value.fromYear || filterDraft.value.toYear || filterDraft.value.onlyOngoing) count++;
  if (filterDraft.value.selectedItemTypes?.length) count++;
  return count;
});

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: ag.name }));
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
  try {
    let str = String(d).trim();
    if (!str) return '—';
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
      const [y, m, day] = str.slice(0, 10).split('-');
      return `${day}/${m}/${y}`;
    }
    const dateObj = new Date(str);
    if (isNaN(dateObj.getTime())) return '—';
    const day = String(dateObj.getUTCDate()).padStart(2, '0');
    const month = String(dateObj.getUTCMonth() + 1).padStart(2, '0');
    const year = dateObj.getUTCFullYear();
    return `${day}/${month}/${year}`;
  } catch {
    return '—';
  }
}

function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? formatDate(sDate) : '...';
  const d = dDate ? formatDate(dDate) : '...';
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

const quickItemType = ref('all');

function onQuickItemTypeChange() {
  if (quickItemType.value === 'all') {
    filterDraft.value.selectedItemTypes = [];
  } else {
    filterDraft.value.selectedItemTypes = [quickItemType.value];
  }
  execFilterSearch();
}

let reportSearchTimer = null;
let reportSearchRequestId = 0;
let reportFetchRequestId = 0;

function execFilterSearch() {
  if (reportSearchTimer) clearTimeout(reportSearchTimer);
  reportSearchRequestId++;
  if (filterDraft.value.selectedItemTypes?.length === 1) {
    quickItemType.value = filterDraft.value.selectedItemTypes[0];
  } else if (!filterDraft.value.selectedItemTypes?.length) {
    quickItemType.value = 'all';
  }
  appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
  currentPage.value = 1;
  loadReportData();
}

watch(() => filterDraft.value.searchQuery, (newVal) => {
  if (reportSearchTimer) clearTimeout(reportSearchTimer);
  const currentId = ++reportSearchRequestId;
  reportSearchTimer = setTimeout(() => {
    if (currentId !== reportSearchRequestId) return;
    appliedFilters.value.searchQuery = newVal || '';
    currentPage.value = 1;
    loadReportData();
  }, 300);
});

function resetReportFilters() {
  if (reportSearchTimer) clearTimeout(reportSearchTimer);
  reportSearchRequestId++;
  quickItemType.value = 'all';
  filterDraft.value = {
    searchQuery: '',
    selectedAgencyIds: [],
    selectedScopes: [],
    selectedItemTypes: [],
    selectedSections: [],
    selectedGroups: [],
    fromYear: null,
    toYear: null,
    onlyOngoing: false
  };
  appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
  currentPage.value = 1;
  loadReportData();
}

function passesCommonFilters(i) {
  // Non-Admin Focal Point Scoping Filter:
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

    const itemLeadId = i.leadAgencyId ? String(i.leadAgencyId).toLowerCase() : '';
    const itemAssignedId = i.assignedAgencyId ? String(i.assignedAgencyId).toLowerCase() : '';
    const itemCoordIds = (i.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());

    const isParentAgency = !userAgency || !userAgency.parentId;
    const isGeneral = isParentAgency && (i.isGeneralTask || i.leadAgencyCode === 'ALL_AGENCIES' || itemLeadId === '00000000-0000-0000-0000-000000009999' || (i.leadAgencyName && i.leadAgencyName.toLowerCase().trim() === 'các bộ, ngành, địa phương'));
    const isLead = scopedAgencyIds.includes(itemLeadId);
    const isAssigned = itemAssignedId && scopedAgencyIds.includes(itemAssignedId);
    const isCoord = itemCoordIds.some(id => scopedAgencyIds.includes(id));
    const isSubMatch = i.subItems?.some(s => {
      const sLeadId = s.leadAgencyId ? String(s.leadAgencyId).toLowerCase() : '';
      const sAssignedId = s.assignedAgencyId ? String(s.assignedAgencyId).toLowerCase() : '';
      const sCoordIds = (s.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());
      const sIsGeneral = isParentAgency && (s.isGeneralTask || s.leadAgencyCode === 'ALL_AGENCIES' || sLeadId === '00000000-0000-0000-0000-000000009999');
      return sIsGeneral || scopedAgencyIds.includes(sLeadId) || (sAssignedId && scopedAgencyIds.includes(sAssignedId)) || sCoordIds.some(id => scopedAgencyIds.includes(id));
    });

    if (!isGeneral && !isLead && !isAssigned && !isCoord && !isSubMatch) return false;
  }

  if (appliedFilters.value.selectedAgencyIds?.length > 0 && !appliedFilters.value.selectedAgencyIds.includes(i.leadAgencyId)) return false;
  if (appliedFilters.value.selectedScopes?.length > 0) {
    const isGeneral = i.isGeneralTask || i.leadAgencyCode === 'ALL_AGENCIES' || i.leadAgencyId === '00000000-0000-0000-0000-000000009999';
    const matchGen = appliedFilters.value.selectedScopes.includes('general') && isGeneral;
    const matchSpec = appliedFilters.value.selectedScopes.includes('specific') && !isGeneral;
    if (!matchGen && !matchSpec) return false;
  }
  if (appliedFilters.value.selectedItemTypes?.length > 0 && !appliedFilters.value.selectedItemTypes.includes(i.itemType)) return false;
  if (appliedFilters.value.selectedSections?.length > 0 && !appliedFilters.value.selectedSections.includes(i.section)) return false;
  if (appliedFilters.value.selectedGroups?.length > 0 && !appliedFilters.value.selectedGroups.includes(i.group)) return false;
  if (appliedFilters.value.onlyOngoing && !i.isOngoing) return false;
  if (appliedFilters.value.fromYear || appliedFilters.value.toYear) {
    const fY = appliedFilters.value.fromYear ? Number(appliedFilters.value.fromYear) : 2026;
    const tY = appliedFilters.value.toYear ? Number(appliedFilters.value.toYear) : 2030;
    if (!i.isOngoing) {
      const sY = i.startDate ? new Date(i.startDate).getFullYear() : 2026;
      const dY = i.dueDate ? new Date(i.dueDate).getFullYear() : sY;
      if (sY > tY || dY < fY) return false;
    }
  }

  const q = (appliedFilters.value.searchQuery || '').trim().toLowerCase();
  if (q) {
    const match = (i.title && i.title.toLowerCase().includes(q)) || 
                  (i.code && i.code.toLowerCase().includes(q)) ||
                  (i.leadAgencyName && i.leadAgencyName.toLowerCase().includes(q));
    if (!match) return false;
  }
  return true;
}

const filteredAgencySummaries = computed(() => {
  const list = [...(metrics.value.ministriesPerformance || []), ...(metrics.value.provincesPerformance || [])];
  if (appliedFilters.value.selectedAgencyIds && appliedFilters.value.selectedAgencyIds.length > 0) {
    return list.filter(a => appliedFilters.value.selectedAgencyIds.includes(a.agencyId));
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

watch(activeReportType, () => {
  currentPage.value = 1;
});

async function loadReportData() {
  const currentId = ++reportFetchRequestId;
  isLoading.value = true;
  try {
    const agRes = await fetch(getApiUrl('/api/agencies'));
    if (agRes.ok) {
      const agData = await agRes.json();
      if (currentId !== reportFetchRequestId) return;
      agencies.value = agData;
    }

    const params = new URLSearchParams();
    if (appliedFilters.value.selectedAgencyIds && appliedFilters.value.selectedAgencyIds.length > 0) {
      appliedFilters.value.selectedAgencyIds.forEach(id => params.append('agencyId', id));
    } else if (!authState.isAdmin.value && authState.user.value?.agencyId) {
      params.append('agencyId', authState.user.value.agencyId);
    }
    if (appliedFilters.value.selectedScopes && appliedFilters.value.selectedScopes.length === 1) {
      params.append('scope', appliedFilters.value.selectedScopes[0]);
    }
    if (appliedFilters.value.selectedItemTypes && appliedFilters.value.selectedItemTypes.length === 1) {
      params.append('itemType', appliedFilters.value.selectedItemTypes[0]);
    }
    if (appliedFilters.value.selectedSections && appliedFilters.value.selectedSections.length > 0) {
      appliedFilters.value.selectedSections.forEach(sec => params.append('section', sec));
    }
    if (appliedFilters.value.selectedGroups && appliedFilters.value.selectedGroups.length > 0) {
      appliedFilters.value.selectedGroups.forEach(grp => params.append('group', grp));
    }
    if (appliedFilters.value.fromYear) params.append('fromYear', appliedFilters.value.fromYear);
    if (appliedFilters.value.toYear) params.append('toYear', appliedFilters.value.toYear);
    if (appliedFilters.value.onlyOngoing) params.append('isOngoing', 'true');

    const mRes = await fetch(getApiUrl(`/api/dashboard/metrics${params.toString() ? '?' + params.toString() : ''}`));
    if (mRes.ok) {
      const mData = await mRes.json();
      if (currentId !== reportFetchRequestId) return;
      metrics.value = mData;
    }

    const docId = '12660000-0000-0000-0000-000000001266';
    const gridRes = await fetch(getApiUrl(`/api/planning/documents/${docId}/grid`));
    if (gridRes.ok) {
      const data = await gridRes.json();
      if (currentId !== reportFetchRequestId) return;
      allItems.value = data.items || [];
    }
  } catch (e) {
    if (currentId !== reportFetchRequestId) return;
    console.error('Lỗi tải dữ liệu báo cáo:', e);
  } finally {
    if (currentId === reportFetchRequestId) {
      isLoading.value = false;
    }
  }
}

function exportCurrentReportToExcel() {
  let title = '';
  let subtitle = '';
  let kpiTitle = '';
  let kpiSection = [];
  let tableTitle = '';
  let headers = [];
  let rows = [];
  let fileName = '';
  let sheetName = '';
  let minColWidths = {};

  const now = new Date();
  const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;

  if (activeReportType.value === 'summary') {
    title = "BÁO CÁO TỔNG HỢP TIẾN ĐỘ THEO CƠ QUAN / ĐƠN VỊ - QUYẾT ĐỊNH 1266/QĐ-TTg";
    subtitle = `Thời gian xuất báo cáo: ${timeStr} | Tổng số cơ quan / địa phương: ${filteredAgencySummaries.value.length}`;
    kpiTitle = "1. CHỈ SỐ TỔNG QUAN HỆ THỐNG CƠ QUAN / ĐƠN VỊ";
    kpiSection = [
      ["Tổng số cơ quan / địa phương theo dõi", filteredAgencySummaries.value.length],
      ["Tổng số mục tiêu chiến lược", metrics.value.totalGoals || 0],
      ["Tổng số nhiệm vụ thực thi", metrics.value.totalTasks || 0],
      ["Tổng số hạng mục hợp nhất", (metrics.value.totalGoals || 0) + (metrics.value.totalTasks || 0)]
    ];
    tableTitle = "2. DANH SÁCH BỘ, NGÀNH, ĐỊA PHƯƠNG VÀ TIẾN ĐỘ THỰC HIỆN";
    fileName = "Bao_Cao_Tong_Hop_Tien_Do_Co_Quan";
    sheetName = "Tổng hợp tiến độ";
    headers = ["STT", "Tên Cơ Quan / Địa Phương", "Tổng Hạng Mục", "Mục Tiêu", "Nhiệm Vụ", "Đã Hoàn Thành", "Đang T/H (Trong Hạn)", "Sắp Tới Hạn", "Đang T/H (Quá Hạn)", "Tỷ Lệ Hoàn Thành (%)"];
    minColWidths = { 0: 8, 1: 38, 2: 15, 3: 12, 4: 12, 5: 15, 6: 20, 7: 15, 8: 20, 9: 22 };

    rows = filteredAgencySummaries.value.map((ag, idx) => {
      const pct = getPct(ag.completedOnTime + ag.completedOverdue, ag.totalItems);
      return [
        idx + 1,
        ag.name || '',
        ag.totalItems || 0,
        ag.totalGoals || 0,
        ag.totalTasks || 0,
        (ag.completedOnTime || 0) + (ag.completedOverdue || 0),
        ag.inProgressOnTime || 0,
        ag.expiringSoon || 0,
        ag.inProgressOverdue || 0,
        `${pct}%`
      ];
    });
  } else if (activeReportType.value === 'urgent') {
    title = "BÁO CÁO NHIỆM VỤ CẦN GỬI THÔNG BÁO (SẮP HẾT HẠN & QUÁ HẠN) - QUYẾT ĐỊNH 1266/QĐ-TTg";
    subtitle = `Thời gian xuất báo cáo: ${timeStr} | Tổng số nhiệm vụ cần chú ý: ${urgentItems.value.length}`;
    kpiTitle = "1. CHỈ SỐ CẢNH BÁO TIẾN ĐỘ THỰC HIỆN";
    kpiSection = [
      ["Tổng số nhiệm vụ cần gửi thông báo", urgentItems.value.length],
      ["Số lượng nhiệm vụ đang thực hiện quá hạn (🔴)", urgentItems.value.filter(i => i.calculatedStatus === 'InProgressOverdue').length],
      ["Số lượng nhiệm vụ sắp tới hạn (🟣)", urgentItems.value.filter(i => i.calculatedStatus === 'ExpiringSoon').length]
    ];
    tableTitle = "2. DANH SÁCH CHI TIẾT NHIỆM VỤ CẦN GỬI THÔNG BÁO";
    fileName = "Bao_Cao_Nhiem_Vu_Can_Gui_Thong_Bao";
    sheetName = "Sắp hết hạn & Quá hạn";
    headers = ["STT", "Mã Hạng Mục", "Loại Hạng Mục", "Tên Mục Tiêu / Nhiệm Vụ", "Đơn Vị Chủ Trì", "Hạn Chót", "Trạng Thái Cảnh Báo"];
    minColWidths = { 0: 8, 1: 15, 2: 15, 3: 50, 4: 32, 5: 20, 6: 25 };

    rows = urgentItems.value.map((i, idx) => [
      idx + 1,
      i.code || '',
      i.itemType === 'Goal' ? 'Mục tiêu' : 'Nhiệm vụ',
      i.title || '',
      i.leadAgencyName || '',
      formatDate(i.dueDate),
      getStatusLabel(i.calculatedStatus)
    ]);
  } else if (activeReportType.value === 'detail') {
    title = "BÁO CÁO CHI TIẾT TIẾN ĐỘ VÀ FILE MINH CHỨNG - QUYẾT ĐỊNH 1266/QĐ-TTg";
    subtitle = `Thời gian xuất báo cáo: ${timeStr} | Tổng số hạng mục: ${filteredDetailItems.value.length}`;
    kpiTitle = "1. CHỈ SỐ CẬP NHẬT TIẾN ĐỘ VÀ MINH CHỨNG";
    kpiSection = [
      ["Tổng số mục tiêu & nhiệm vụ theo dõi", filteredDetailItems.value.length],
      ["Số lượng hạng mục đã có cập nhật tiến độ", filteredDetailItems.value.filter(i => i.latestProgressValue !== null && i.latestProgressValue !== undefined).length],
      ["Số lượng hạng mục đã đính kèm file minh chứng", filteredDetailItems.value.filter(i => (i.evidenceFilesCount || 0) > 0).length]
    ];
    tableTitle = "2. DANH SÁCH CHI TIẾT TIẾN ĐỘ VÀ FILE MINH CHỨNG THEO HẠNG MỤC";
    fileName = "Bao_Cao_Chi_Tiet_Tien_Do_Minh_Chung";
    sheetName = "Chi tiết tiến độ";
    headers = ["STT", "Mã Hạng Mục", "Tên Mục Tiêu / Nhiệm Vụ", "Đơn Vị Chủ Trì", "Tiến Độ Mới Nhất (%)", "Trạng Thái Thực Hiện", "Số File Minh Chứng"];
    minColWidths = { 0: 8, 1: 15, 2: 50, 3: 32, 4: 22, 5: 25, 6: 20 };

    rows = filteredDetailItems.value.map((i, idx) => [
      idx + 1,
      i.code || '',
      i.title || '',
      i.leadAgencyName || '',
      i.latestProgressValue !== null && i.latestProgressValue !== undefined ? `${i.latestProgressValue}%` : 'Chưa cập nhật',
      getStatusLabel(i.calculatedStatus),
      i.evidenceFilesCount || 0
    ]);
  } else if (activeReportType.value === 'scope') {
    title = "BÁO CÁO PHÂN LOẠI NHIỆM VỤ CHUNG VÀ RIÊNG - QUYẾT ĐỊNH 1266/QĐ-TTg";
    subtitle = `Thời gian xuất báo cáo: ${timeStr} | Tổng số nhiệm vụ: ${filteredScopeItems.value.length}`;
    kpiTitle = "1. THỐNG KÊ PHÂN LOẠI PHẠM VI NHIỆM VỤ";
    kpiSection = [
      ["Tổng số nhiệm vụ theo dõi", filteredScopeItems.value.length],
      ["Số lượng nhiệm vụ phạm vi chung (Các bộ, ngành, địa phương)", filteredScopeItems.value.filter(i => i.isGeneralTask).length],
      ["Số lượng nhiệm vụ phạm vi riêng (Giao đơn vị cụ thể)", filteredScopeItems.value.filter(i => !i.isGeneralTask).length]
    ];
    tableTitle = "2. DANH SÁCH PHÂN LOẠI CHI TIẾT NHIỆM VỤ THEO PHẠM VI";
    fileName = "Bao_Cao_Phan_Loai_Nhiem_Vu_Chung_Rieng";
    sheetName = "Nhiệm vụ chung & riêng";
    headers = ["STT", "Mã Hạng Mục", "Phạm Vi Nhiệm Vụ", "Nội Dung Thực Hiện", "Đơn Vị Đầu Mối", "Thời Gian Thực Hiện", "Trạng Thái Thực Hiện"];
    minColWidths = { 0: 8, 1: 15, 2: 20, 3: 50, 4: 32, 5: 22, 6: 25 };

    rows = filteredScopeItems.value.map((i, idx) => [
      idx + 1,
      i.code || '',
      i.isGeneralTask ? "Nhiệm vụ Chung" : "Nhiệm vụ Riêng",
      i.title || '',
      i.leadAgencyName || '',
      formatDateRange(i.startDate, i.dueDate),
      getStatusLabel(i.calculatedStatus)
    ]);
  }

  exportFormattedReportExcel({
    title,
    subtitle,
    kpiTitle,
    kpiSection,
    tableTitle,
    headers,
    rows,
    fileName,
    sheetName,
    minColWidths
  });
}

onMounted(() => {
  loadReportData();
});
</script>
