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
            <h2 class="text-sm sm:text-base font-bold text-slate-800">
              Trung Tâm Báo Cáo & Xuất Dữ Liệu Excel
            </h2>
          </div>
        </div>
      </div>

      <button 
        @click="exportCurrentReportToExcel" 
        class="px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center gap-2 shrink-0"
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
          'p-3 rounded-2xl border text-left transition space-y-0.5 cursor-pointer',
          activeReportType === 'summary' ? 'bg-blue-600 text-white border-blue-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-bold uppercase opacity-80">Báo cáo 1</div>
        <div class="text-xs font-bold truncate">📊 Tiến độ các Bộ, Ngành & Địa phương</div>
      </button>

      <button 
        @click="activeReportType = 'urgent'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5 cursor-pointer',
          activeReportType === 'urgent' ? 'bg-rose-600 text-white border-rose-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-bold uppercase opacity-80">Báo cáo 2</div>
        <div class="text-xs font-bold truncate">⚠️ Cảnh báo Sắp hết hạn & Quá hạn</div>
      </button>

      <button 
        @click="activeReportType = 'governance'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5 cursor-pointer',
          activeReportType === 'governance' ? 'bg-indigo-600 text-white border-indigo-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-bold uppercase opacity-80">Báo cáo 3</div>
        <div class="text-xs font-bold truncate">🏛️ Thống kê Đôn đốc & Tuân thủ Báo cáo</div>
      </button>

      <button 
        @click="activeReportType = 'domain'"
        :class="[
          'p-3 rounded-2xl border text-left transition space-y-0.5 cursor-pointer',
          activeReportType === 'domain' ? 'bg-purple-600 text-white border-purple-600 shadow-md' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50'
        ]"
      >
        <div class="text-[10px] font-bold uppercase opacity-80">Báo cáo 4</div>
        <div class="text-xs font-bold truncate">📈 Tiến độ theo Lĩnh vực & Nhóm trọng tâm</div>
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
          class="py-2 px-3 text-xs bg-slate-50 hover:bg-slate-100 border border-slate-200 rounded-xl font-bold text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer min-h-[36px] shadow-2xs shrink-0"
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
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <SearchableSelect 
                  v-model="filterDraft.selectedItemTypes" 
                  :options="itemTypeOptions" 
                  :isMulti="true" 
                  label="Loại Đối Tượng" 
                  placeholder="Tất cả loại đối tượng"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="filterDraft.selectedScopes" 
                  :options="reportScopeOptions" 
                  :isMulti="true" 
                  label="Phạm Vi" 
                  placeholder="Tất cả phạm vi"
                />
              </div>
            </div>

            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedAgencyIds" 
                :options="leadAgencyOptions" 
                :isMulti="true" 
                label="Cơ Quan Chủ Trì" 
                placeholder="Tất cả cơ quan chủ trì"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="filterDraft.selectedSubAgencyIds" 
                :options="subAgencyOptions" 
                :isMulti="true" 
                label="Đơn Vị Trực Thuộc" 
                placeholder="Tất cả đơn vị trực thuộc"
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
                <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wider block">Giai Đoạn</label>
                <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-bold text-blue-700">
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
          <thead class="bg-slate-100 font-bold text-slate-600 border-b border-slate-200">
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
              <td class="p-3 text-center font-bold text-blue-800">
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
          <thead class="bg-rose-50 text-rose-900 font-bold border-b border-rose-200">
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
              <td class="p-3 font-bold text-rose-800">{{ item.code }}</td>
              <td class="p-3">
                <span :class="['px-2 py-0.5 rounded text-[10px] font-bold', item.itemType === 'Goal' ? 'bg-purple-100 text-purple-800' : 'bg-slate-100 text-slate-700']">
                  {{ item.itemType === 'Goal' ? '🎯 Mục tiêu' : '📋 Nhiệm vụ' }}
                </span>
              </td>
              <td class="p-3 font-semibold text-slate-900">
                <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1500, hide: 0 }">
                  <div class="line-clamp-2 font-semibold text-slate-900 leading-relaxed cursor-help">
                    {{ item.title }}
                  </div>
                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[260px] max-w-[420px] p-1">
                      <span class="font-bold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">📋 Nội dung chi tiết</span>
                      {{ item.title }}
                    </div>
                  </template>
                </VTooltip>
              </td>
              <td class="p-3 font-bold text-slate-800">{{ item.leadAgencyName }}</td>
              <td class="p-3 font-semibold text-slate-600">{{ formatDate(item.dueDate) }}</td>
              <td class="p-3 text-center">
                <span :class="['px-2.5 py-1 rounded-full text-[11px] font-bold', item.calculatedStatus === 'ExpiringSoon' ? 'bg-amber-100 text-amber-900 border border-amber-300' : 'bg-rose-100 text-rose-900 border border-rose-300']">
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

      <!-- BÁO CÁO 3 MỚI: THỐNG KÊ ĐÔN ĐỐC & TÍNH TUÂN THỦ BÁO CÁO -->
      <div v-else-if="activeReportType === 'governance'" class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-700 border-collapse">
          <thead class="bg-indigo-50 text-indigo-900 font-bold border-b border-indigo-200">
            <tr>
              <th class="p-3 text-center w-12">STT</th>
              <th class="p-3">Tên Cơ Quan / Địa Phương</th>
              <th class="p-3 text-center min-w-[120px]">Tổng NV Được Giao</th>
              <th class="p-3 text-center text-amber-800 min-w-[110px]">Số Lần Đã Đôn Đốc</th>
              <th class="p-3 text-center min-w-[120px]">Báo Cáo Đã Nạp</th>
              <th class="p-3 text-center text-emerald-800 min-w-[120px]">Báo Cáo Đúng Hạn</th>
              <th class="p-3 text-center text-rose-800 min-w-[120px]">Trễ Báo Cáo / Chưa Nạp</th>
              <th class="p-3 text-center min-w-[140px]">Tỷ Lệ Tuân Thủ (%)</th>
              <th class="p-3 text-center min-w-[130px]">Đánh Giá Tuân Thủ</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="(ag, idx) in paginatedGovernanceSummaries" :key="ag.agencyId" class="hover:bg-slate-50 transition">
              <td class="p-3 text-center font-bold text-slate-500">{{ (currentPage - 1) * pageSize + idx + 1 }}</td>
              <td class="p-3 font-bold text-slate-900">{{ ag.name }}</td>
              <td class="p-3 text-center font-bold">{{ ag.totalItems }}</td>
              <td class="p-3 text-center font-bold text-amber-700 bg-amber-50/40">{{ ag.urgedCount }}</td>
              <td class="p-3 text-center font-bold text-blue-700">{{ ag.totalReportsSubmitted }}</td>
              <td class="p-3 text-center font-bold text-emerald-700">{{ ag.onTimeReports }}</td>
              <td class="p-3 text-center font-bold text-rose-700 bg-rose-50/40">{{ ag.lateOrPendingReports }}</td>
              <td class="p-3 text-center font-bold text-slate-900">
                <div class="flex items-center justify-center gap-2">
                  <div class="w-16 bg-slate-200 rounded-full h-2 overflow-hidden">
                    <div class="h-full bg-indigo-600 rounded-full" :style="{ width: ag.complianceRate + '%' }"></div>
                  </div>
                  <span>{{ ag.complianceRate }}%</span>
                </div>
              </td>
              <td class="p-3 text-center">
                <span :class="['px-2.5 py-1 rounded-full text-[11px] font-bold border inline-block whitespace-nowrap', ag.complianceBadgeClass]">
                  {{ ag.complianceBadgeText }}
                </span>
              </td>
            </tr>
            <tr v-if="filteredGovernanceSummaries.length === 0">
              <td colspan="9" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- BÁO CÁO 4 MỚI: PHÂN LOẠI THEO LĨNH VỰC & TRỤ CỘT CHIẾN LƯỢC -->
      <div v-else-if="activeReportType === 'domain'" class="overflow-x-auto">
        <table class="w-full text-left text-xs text-slate-700 border-collapse">
          <thead class="bg-purple-50 text-purple-900 font-bold border-b border-purple-200">
            <tr>
              <th class="p-3 text-center w-12">STT</th>
              <th class="p-3 min-w-[200px]">Lĩnh Vực / Trụ Cột Chiến Lược</th>
              <th class="p-3 text-center min-w-[100px]">Tổng Số</th>
              <th class="p-3 text-center text-emerald-800 min-w-[110px]">Đã Hoàn Thành</th>
              <th class="p-3 text-center text-blue-800 min-w-[110px]">Đang Thực Hiện</th>
              <th class="p-3 text-center text-amber-800 min-w-[100px]">Sắp Hết Hạn</th>
              <th class="p-3 text-center text-rose-800 min-w-[100px]">Quá Hạn</th>
              <th class="p-3 text-center text-slate-500 min-w-[100px]">Chưa Thực Hiện</th>
              <th class="p-3 text-center min-w-[150px]">Tỷ Lệ Hoàn Thành (%)</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="(dom, idx) in paginatedDomainSummaries" :key="dom.domainName" class="hover:bg-slate-50 transition">
              <td class="p-3 text-center font-bold text-slate-500">{{ (currentPage - 1) * pageSize + idx + 1 }}</td>
              <td class="p-3 font-bold text-purple-900 flex items-center gap-2">
                <span class="w-2 h-2 rounded-full bg-purple-600 shrink-0"></span>
                <span>{{ dom.domainName }}</span>
              </td>
              <td class="p-3 text-center font-bold text-slate-900">{{ dom.totalItems }}</td>
              <td class="p-3 text-center font-bold text-emerald-700">{{ dom.completedOnTime + dom.completedOverdue }}</td>
              <td class="p-3 text-center font-bold text-blue-700">{{ dom.inProgressOnTime }}</td>
              <td class="p-3 text-center font-bold text-amber-700">{{ dom.expiringSoon }}</td>
              <td class="p-3 text-center font-bold text-rose-700 bg-rose-50/30">{{ dom.inProgressOverdue }}</td>
              <td class="p-3 text-center font-bold text-slate-500">{{ dom.notStarted }}</td>
              <td class="p-3 text-center font-bold text-slate-900">
                <div class="flex items-center justify-center gap-2">
                  <div class="w-16 bg-slate-200 rounded-full h-2 overflow-hidden">
                    <div class="h-full bg-purple-600 rounded-full" :style="{ width: dom.completionRate + '%' }"></div>
                  </div>
                  <span>{{ dom.completionRate }}%</span>
                </div>
              </td>
            </tr>
            <tr v-if="filteredDomainSummaries.length === 0">
              <td colspan="9" class="p-8 text-center text-slate-400 italic font-medium">Không tìm thấy bản ghi nào phù hợp với bộ lọc.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Server Pagination Controls Footer -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div>
          Hiển thị <span class="font-bold text-slate-900">{{ currentActiveTotalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, currentActiveTotalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ currentActiveTotalCount }}</span> bản ghi
        </div>

        <div class="flex items-center gap-2">
          <button 
            @click="changePage(currentPage - 1)" 
            :disabled="currentPage <= 1"
            class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer disabled:cursor-not-allowed"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-xl font-bold">
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
  selectedSubAgencyIds: [],
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
  selectedSubAgencyIds: [],
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
  if (filterDraft.value.selectedSubAgencyIds?.length) count++;
  if (filterDraft.value.selectedScopes?.length) count++;
  if (filterDraft.value.selectedSections?.length) count++;
  if (filterDraft.value.selectedGroups?.length) count++;
  if (filterDraft.value.fromYear || filterDraft.value.toYear || filterDraft.value.onlyOngoing) count++;
  if (filterDraft.value.selectedItemTypes?.length) count++;
  return count;
});

const isSpecialAgencyCode = (code) => code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND' || code === 'ALL_MINISTRIES_DIRECT';

const formatAgencyLabel = (ag, list) => {
  if (isSpecialAgencyCode(ag.code)) {
    return `🌐 ${ag.name}`;
  }
  if (ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000') {
    const parentAg = list.find(p => p.id === ag.parentId);
    return parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name;
  }
  return ag.name;
};

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: formatAgencyLabel(ag, agencies.value) }));
});

const leadAgencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: formatAgencyLabel(ag, agencies.value) }));
});

const subAgencyOptions = computed(() => {
  return agencies.value
    .filter(ag => ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000')
    .map(ag => ({
      value: ag.id,
      label: formatAgencyLabel(ag, agencies.value)
    }));
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
    selectedSubAgencyIds: [],
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
        .filter(a => a.parentId && String(a.parentId).toLowerCase() === userAgencyId && a.type !== 4 && a.type !== 'Other')
        .map(a => String(a.id).toLowerCase());
      scopedAgencyIds.push(...childIds);
    }

    const itemLeadId = i.leadAgencyId ? String(i.leadAgencyId).toLowerCase() : '';
    const itemAssignedId = i.assignedAgencyId ? String(i.assignedAgencyId).toLowerCase() : '';
    const itemCoordIds = (i.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());

    const isParentAgency = !userAgency || !userAgency.parentId;
    const isGeneral = isParentAgency && (i.isGeneralTask || isSpecialAgencyCode(i.leadAgencyCode) || ['00000000-0000-0000-0000-000000009999', '00000000-0000-0000-0000-000000009998', '00000000-0000-0000-0000-000000009997', '00000000-0000-0000-0000-000000009996', '00000000-0000-0000-0000-000000009995'].includes(itemLeadId) || (i.leadAgencyName && (i.leadAgencyName.toLowerCase().includes('các bộ, ngành') || i.leadAgencyName.toLowerCase().includes('các địa phương'))));
    const isLead = scopedAgencyIds.includes(itemLeadId);
    const isAssigned = itemAssignedId && scopedAgencyIds.includes(itemAssignedId);
    const isCoord = itemCoordIds.some(id => scopedAgencyIds.includes(id));
    const isSubMatch = i.subItems?.some(s => {
      const sLeadId = s.leadAgencyId ? String(s.leadAgencyId).toLowerCase() : '';
      const sAssignedId = s.assignedAgencyId ? String(s.assignedAgencyId).toLowerCase() : '';
      const sCoordIds = (s.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());
      const sIsGeneral = isParentAgency && (s.isGeneralTask || isSpecialAgencyCode(s.leadAgencyCode) || ['00000000-0000-0000-0000-000000009999', '00000000-0000-0000-0000-000000009998', '00000000-0000-0000-0000-000000009997', '00000000-0000-0000-0000-000000009996', '00000000-0000-0000-0000-000000009995'].includes(sLeadId));
      return sIsGeneral || scopedAgencyIds.includes(sLeadId) || (sAssignedId && scopedAgencyIds.includes(sAssignedId)) || sCoordIds.some(id => scopedAgencyIds.includes(id));
    });

    if (!isGeneral && !isLead && !isAssigned && !isCoord && !isSubMatch) return false;
  }

  if (appliedFilters.value.selectedAgencyIds?.length > 0 && !appliedFilters.value.selectedAgencyIds.includes(i.leadAgencyId)) return false;
  if (appliedFilters.value.selectedSubAgencyIds?.length > 0 && !appliedFilters.value.selectedSubAgencyIds.includes(i.assignedAgencyId)) return false;
  if (appliedFilters.value.selectedScopes?.length > 0) {
    const isGeneral = i.isGeneralTask || isSpecialAgencyCode(i.leadAgencyCode) || ['00000000-0000-0000-0000-000000009999', '00000000-0000-0000-0000-000000009998', '00000000-0000-0000-0000-000000009997', '00000000-0000-0000-0000-000000009996', '00000000-0000-0000-0000-000000009995'].includes(i.leadAgencyId);
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
  let list = [...(metrics.value.ministriesPerformance || []), ...(metrics.value.provincesPerformance || [])];
  if (appliedFilters.value.selectedAgencyIds && appliedFilters.value.selectedAgencyIds.length > 0) {
    list = list.filter(a => appliedFilters.value.selectedAgencyIds.includes(a.agencyId));
  }
  if (appliedFilters.value.selectedSubAgencyIds && appliedFilters.value.selectedSubAgencyIds.length > 0) {
    list = list.filter(a => appliedFilters.value.selectedSubAgencyIds.includes(a.agencyId));
  }
  return list;
});

const urgentItems = computed(() => {
  return allItems.value.filter(i => (i.calculatedStatus === 'ExpiringSoon' || i.calculatedStatus === 'InProgressOverdue') && passesCommonFilters(i));
});

const filteredGovernanceSummaries = computed(() => {
  return filteredAgencySummaries.value.map(ag => {
    const totalItems = ag.totalItems || 0;
    const totalReportsSubmitted = (ag.completedOnTime || 0) + (ag.completedOverdue || 0) + (ag.inProgressOnTime || 0) + (ag.inProgressOverdue || 0);
    const onTimeReports = (ag.completedOnTime || 0) + (ag.inProgressOnTime || 0);
    const lateOrPendingReports = (ag.inProgressOverdue || 0) + (ag.expiringSoon || 0) + (ag.notStarted || 0);
    const urgedCount = (ag.inProgressOverdue || 0) + (ag.expiringSoon || 0);

    const complianceRate = totalItems > 0 ? Math.round((onTimeReports / totalItems) * 100) : 100;

    let complianceBadgeText = 'Thực hiện tốt';
    let complianceBadgeClass = 'bg-emerald-50 text-emerald-700 border-emerald-200';

    if (complianceRate < 50) {
      complianceBadgeText = 'Trễ hạn / Cần đôn đốc';
      complianceBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
    } else if (complianceRate < 80) {
      complianceBadgeText = 'Cần đẩy nhanh';
      complianceBadgeClass = 'bg-amber-50 text-amber-700 border-amber-200';
    }

    return {
      agencyId: ag.agencyId,
      name: ag.name,
      totalItems,
      urgedCount,
      totalReportsSubmitted,
      onTimeReports,
      lateOrPendingReports,
      complianceRate,
      complianceBadgeText,
      complianceBadgeClass
    };
  });
});

const filteredDomainSummaries = computed(() => {
  const domainMap = {};

  allItems.value.forEach(item => {
    if (!passesCommonFilters(item)) return;

    let dName = item.section || item.group || 'Các nhiệm vụ / mục tiêu chung';
    if (!domainMap[dName]) {
      domainMap[dName] = {
        domainName: dName,
        totalItems: 0,
        completedOnTime: 0,
        completedOverdue: 0,
        inProgressOnTime: 0,
        inProgressOverdue: 0,
        expiringSoon: 0,
        notStarted: 0
      };
    }

    const d = domainMap[dName];
    d.totalItems++;

    const st = item.calculatedStatus;
    if (st === 'CompletedOnTime') d.completedOnTime++;
    else if (st === 'CompletedOverdue') d.completedOverdue++;
    else if (st === 'InProgressOnTime') d.inProgressOnTime++;
    else if (st === 'InProgressOverdue') d.inProgressOverdue++;
    else if (st === 'ExpiringSoon') d.expiringSoon++;
    else d.notStarted++;
  });

  return Object.values(domainMap).map(d => {
    const totalComp = d.completedOnTime + d.completedOverdue;
    const completionRate = d.totalItems > 0 ? Math.round((totalComp / d.totalItems) * 100) : 0;
    return {
      ...d,
      completionRate
    };
  }).sort((a, b) => b.totalItems - a.totalItems);
});

const currentActiveTotalCount = computed(() => {
  if (activeReportType.value === 'summary') return filteredAgencySummaries.value.length;
  if (activeReportType.value === 'urgent') return urgentItems.value.length;
  if (activeReportType.value === 'governance') return filteredGovernanceSummaries.value.length;
  if (activeReportType.value === 'domain') return filteredDomainSummaries.value.length;
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

const paginatedGovernanceSummaries = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredGovernanceSummaries.value.slice(start, start + pageSize.value);
});

const paginatedDomainSummaries = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredDomainSummaries.value.slice(start, start + pageSize.value);
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
    headers = ["STT", "Mã Hạng Mục", "Loại Hạng Mục", "Tên Mục Tiêu / Nhiệm Vụ", "Đơn Vị Chủ Trì", "Giao Đơn Vị Trực Thuộc", "Hạn Chót", "Trạng Thái Cảnh Báo"];
    minColWidths = { 0: 8, 1: 15, 2: 15, 3: 45, 4: 28, 5: 28, 6: 20, 7: 25 };

    rows = urgentItems.value.map((i, idx) => [
      idx + 1,
      i.code || '',
      i.itemType === 'Goal' ? 'Mục tiêu' : 'Nhiệm vụ',
      i.title || '',
      i.leadAgencyName || '',
      i.assignedAgencyName || '—',
      formatDate(i.dueDate),
      getStatusLabel(i.calculatedStatus)
    ]);
  } else if (activeReportType.value === 'governance') {
    title = "BÁO CÁO THỐNG KÊ ĐÔN ĐỐC VÀ TÍNH TUÂN THỦ BÁO CÁO - QUYẾT ĐỊNH 1266/QĐ-TTg";
    subtitle = `Thời gian xuất báo cáo: ${timeStr} | Tổng số cơ quan / địa phương: ${filteredGovernanceSummaries.value.length}`;
    kpiTitle = "1. CHỈ SỐ TỔNG QUAN TÍNH TUÂN THỦ BÁO CÁO";
    kpiSection = [
      ["Tổng số cơ quan / địa phương theo dõi", filteredGovernanceSummaries.value.length],
      ["Số đơn vị đạt tỷ lệ tuân thủ tốt (≥80%)", filteredGovernanceSummaries.value.filter(a => a.complianceRate >= 80).length],
      ["Số đơn vị cần đôn đốc (<50%)", filteredGovernanceSummaries.value.filter(a => a.complianceRate < 50).length]
    ];
    tableTitle = "2. DANH SÁCH THỐNG KÊ ĐÔN ĐỐC VÀ TÍNH TUÂN THỦ CỦA TỪNG BỘ, NGÀNH, ĐỊA PHURƠNG";
    fileName = "Bao_Cao_Thong_Ke_Don_Doc_Tuan_Thu_Bao_Cao";
    sheetName = "Đôn đốc & Tuân thủ";
    headers = ["STT", "Tên Cơ Quan / Địa Phương", "Tổng NV Được Giao", "Số Lần Đã Đôn Đốc", "Báo Cáo Đã Nạp", "Báo Cáo Đúng Hạn", "Trễ Báo Cáo / Chưa Nạp", "Tỷ Lệ Tuân Thủ (%)", "Đánh Giá Tuân Thủ"];
    minColWidths = { 0: 8, 1: 38, 2: 18, 3: 18, 4: 18, 5: 18, 6: 22, 7: 20, 8: 22 };

    rows = filteredGovernanceSummaries.value.map((ag, idx) => [
      idx + 1,
      ag.name || '',
      ag.totalItems,
      ag.urgedCount,
      ag.totalReportsSubmitted,
      ag.onTimeReports,
      ag.lateOrPendingReports,
      `${ag.complianceRate}%`,
      ag.complianceBadgeText
    ]);
  } else if (activeReportType.value === 'domain') {
    title = "BÁO CÁO THỐNG KÊ TIẾN ĐỘ THEO LĨNH VỰC VÀ TRỤ CỘT CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg";
    subtitle = `Thời gian xuất báo cáo: ${timeStr} | Tổng số lĩnh vực / trụ cột: ${filteredDomainSummaries.value.length}`;
    kpiTitle = "1. CHỈ SỐ TỔNG QUAN TIẾN ĐỘ THEO TRỤ CỘT CHIẾN LƯỢC";
    kpiSection = [
      ["Tổng số lĩnh vực / nhóm trọng tâm theo dõi", filteredDomainSummaries.value.length],
      ["Tổng số mục tiêu & nhiệm vụ hợp nhất", allItems.value.length],
      ["Số lĩnh vực đạt tỷ lệ hoàn thành trên 50%", filteredDomainSummaries.value.filter(d => d.completionRate >= 50).length]
    ];
    tableTitle = "2. DANH SÁCH CHI TIẾT TIẾN ĐỘ THEO LĨNH VỰC VÀ TRỤ CỘT CHIẾN LƯỢC";
    fileName = "Bao_Cao_Tien_Do_Theo_Linh_Vuc_Tru_Cot";
    sheetName = "Tiến độ theo lĩnh vực";
    headers = ["STT", "Lĩnh Vực / Trụ Cột Chiến Lược", "Tổng Hạng Mục", "Đã Hoàn Thành", "Đang Thực Hiện (Trong Hạn)", "Sắp Tới Hạn", "Đang T/H (Quá Hạn)", "Chưa Thực Hiện", "Tỷ Lệ Hoàn Thành (%)"];
    minColWidths = { 0: 8, 1: 45, 2: 15, 3: 15, 4: 22, 5: 15, 6: 20, 7: 15, 8: 22 };

    rows = filteredDomainSummaries.value.map((dom, idx) => [
      idx + 1,
      dom.domainName,
      dom.totalItems,
      dom.completedOnTime + dom.completedOverdue,
      dom.inProgressOnTime,
      dom.expiringSoon,
      dom.inProgressOverdue,
      dom.notStarted,
      `${dom.completionRate}%`
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
