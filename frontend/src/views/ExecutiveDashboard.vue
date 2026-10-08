<template>
  <div class="w-full space-y-4 font-sans">

    <!-- Top Header Bar -->
    <DashboardHeader
      :authState="authState"
      :setDashboardFilter="setDashboardFilter"
      :dashboardTitleText="dashboardTitleText"
      :dashboardFilterOptions="dashboardFilterOptions"
      :scopeOptions="scopeOptions"
      :activeDashboardFilterCount="activeDashboardFilterCount"
      :resetDashboardFilters="resetDashboardFilters"
      :leadAgencyOptions="leadAgencyOptions"
      :subAgencyOptions="subAgencyOptions"
      :yearOptions="yearOptions"
      :sectionOptions="sectionOptions"
      :groupOptions="groupOptions"
      :isBKHCNAgency="isBKHCNAgency"
      :loadDashboardMetrics="loadDashboardMetrics"
      :isExportingExcel="isExportingExcel"
      :exportDashboardExcelReport="exportDashboardExcelReport"
      v-model:dashboardFilter="dashboardFilter"
      v-model:selectedScopes="selectedScopes"
      v-model:selectedAgencyIds="selectedAgencyIds"
      v-model:selectedSubAgencyIds="selectedSubAgencyIds"
      v-model:selectedSections="selectedSections"
      v-model:selectedGroups="selectedGroups"
      v-model:isOngoingOnly="isOngoingOnly"
      v-model:fromYear="fromYear"
      v-model:toYear="toYear"
    />

    <div v-if="loadError" role="alert" class="p-3 text-rose-700 bg-rose-50 rounded-xl">
      {{ loadError }} <button type="button" class="underline" @click="loadDashboardMetrics">Thử lại</button>
    </div>
    <!-- Loading Spinner -->
    <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu tổng quan bảng điều khiển..." />

    <!-- Main Dashboard Container -->
    <div v-else class="space-y-3.5 w-full">

      <!-- 2 Biểu đồ Thống kê (Chia đôi màn hình 50%-50% trên Desktop, 1 hàng trên màn hình nhỏ) -->
      <DashboardSummary
      :dashboardFilter="dashboardFilter"
      :dashboardItemNoun="dashboardItemNoun"
      :activeStatusSummary="activeStatusSummary"
      :activeStatusTotal="activeStatusTotal"
      :overallDonutStats="overallDonutStats"
      :activeCreatedStatusSummary="activeCreatedStatusSummary"
      :activeCreatedTotals="activeCreatedTotals"
      :createdDonutStats="createdDonutStats"
      v-if="!isSubAgencyUser"
    />

    <!-- Sub-Agency Dedicated Progress Dashboard Section (When logged in as Sub-Agency / Child Unit) -->
    <div v-if="isSubAgencyUser" class="space-y-4 w-full">
      <!-- 1. Progress of Level 2 Agency Itself -->
      <div v-if="singleSubAgencyPerformance" class="bg-white p-5 rounded-2xl shadow-sm border border-slate-200/80 space-y-4 w-full">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
              🏢 <span class="text-blue-700 font-bold">{{ singleSubAgencyPerformance.name }}</span>
            </h3>
            <span v-if="loggedUserAgency?.parentName" class="text-xs text-slate-500 font-semibold mt-0.5 block">
              Cơ quan quản lý trực tiếp: <strong>{{ loggedUserAgency.parentName }}</strong>
            </span>
          </div>

          <button
            @click="drilldownAgency(singleSubAgencyPerformance)"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-2xs transition flex items-center gap-1.5 cursor-pointer shrink-0"
          >
            <span>📋 Xem Danh Sách Chi Tiết {{ dashboardItemNounCap }}</span>
            <span>→</span>
          </button>
        </div>

        <!-- Big Circle Donut Chart & Status Breakdown -->
        <div class="bg-gradient-to-br from-slate-50 to-blue-50/40 rounded-2xl border border-slate-200 p-5 shadow-2xs space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-12 gap-6 items-center">

            <!-- Big Donut Circle Chart on Left -->
            <div class="md:col-span-5 flex flex-col items-center justify-center p-4 bg-white rounded-2xl border border-slate-200/80 shadow-2xs space-y-2">
              <MiniStatusDonut :stats="singleSubAgencyPerformance" :size="160" :innerSize="105" :fontSize="32" />
              <div class="text-center pt-1">
                <div class="text-xs font-bold text-slate-800">Tổng số: {{ (dashboardFilter === 'goals' ? singleSubAgencyPerformance.totalGoals : singleSubAgencyPerformance.totalTasks) || 0 }} {{ dashboardItemNoun }}</div>
              </div>
            </div>

            <!-- 6 Status Legend Breakdown List on Right -->
            <div class="md:col-span-7 space-y-2 font-bold text-xs">
              <div class="flex items-center justify-between p-2.5 rounded-xl bg-white border border-slate-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-slate-400"></span>
                  <span class="text-slate-700">Chưa thực hiện</span>
                </div>
                <span class="font-bold text-slate-900 text-sm">{{ singleSubAgencyPerformance.notStarted || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-blue-50/70 border border-blue-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-blue-500"></span>
                  <span class="text-blue-800">Đang thực hiện (trong hạn)</span>
                </div>
                <span class="font-bold text-blue-900 text-sm">{{ singleSubAgencyPerformance.inProgressOnTime || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-rose-50/70 border border-rose-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-rose-500"></span>
                  <span class="text-rose-800">Đang thực hiện (quá hạn)</span>
                </div>
                <span class="font-bold text-rose-900 text-sm">{{ singleSubAgencyPerformance.inProgressOverdue || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-emerald-50/70 border border-emerald-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-emerald-500"></span>
                  <span class="text-emerald-800">Hoàn thành (đúng hạn)</span>
                </div>
                <span class="font-bold text-emerald-900 text-sm">{{ singleSubAgencyPerformance.completedOnTime || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-teal-50/70 border border-teal-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-teal-500"></span>
                  <span class="text-teal-800">Hoàn thành (quá hạn)</span>
                </div>
                <span class="font-bold text-teal-900 text-sm">{{ singleSubAgencyPerformance.completedOverdue || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-amber-50/70 border border-amber-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-amber-500"></span>
                  <span class="text-amber-800">Sắp hết hạn</span>
                </div>
                <span class="font-bold text-amber-900 text-sm">{{ singleSubAgencyPerformance.expiringSoon || 0 }}</span>
              </div>
            </div>

          </div>
        </div>
      </div>

      <!-- 2. Subordinate Child Agencies Progress Block (Khối Các Đơn Vị Trực Thuộc - Chỉ hiển thị cho BKHCN đối với Cấp 2) -->
      <div v-if="!isLevel3User && isBKHCNAgency" class="bg-white p-4.5 rounded-2xl shadow-sm border border-slate-200/80 space-y-4 w-full">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
              🏛️ Khối Các Đơn Vị Trực Thuộc
            </h3>
          </div>
        </div>

        <div v-if="userSubAgenciesPerformance?.length" class="space-y-3">
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
            <AgencyProgressCard v-for="(item, index) in visibleSubAgencies" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="blue" @select="drilldownAgency" />
          </div>

          <!-- Expand / Collapse Button -->
          <div v-if="(userSubAgenciesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
            <button
              @click="isSubAgenciesExpanded = !isSubAgenciesExpanded"
              class="px-5 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
            >
              <span>{{ isSubAgenciesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${userSubAgenciesPerformance.length - 8} Đơn vị trực thuộc khác)` }}</span>
            </button>
          </div>
        </div>

        <div v-else class="p-8 text-center text-xs text-slate-400 italic font-semibold bg-slate-50/50 rounded-xl border border-slate-200/60">
          Chưa có đơn vị trực thuộc nào được giao nhiệm vụ.
        </div>
      </div>
    </div>

    <!-- Main Sections: Admin / Parent Agency View -->
    <div v-else class="space-y-6 w-full">

      <!-- Collapsible Overall Agencies Ranking Line & Stacked Chart -->
      <AllAgenciesProgressChart
        :ministriesData="metrics.ministriesPerformance"
        :provincesData="metrics.provincesPerformance"
        :othersData="metrics.othersPerformance"
        :filterType="dashboardFilter"
        @select-agency="drilldownAgency"
      />

      <!-- Section 1: Khối Bộ / Ngành -->
      <div class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div
          @click="isSection1Collapsed = !isSection1Collapsed"
          class="flex items-center justify-between border-b border-slate-100 pb-3 cursor-pointer select-none hover:opacity-80 transition"
        >
          <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
            <span>🏢 Khối Các Bộ / Ngành Trung Ương</span>
            <span class="text-xs font-normal text-slate-400">({{ isSection1Collapsed ? 'Nhấp để mở rộng' : 'Nhấp để thu gọn' }})</span>
          </h3>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-blue-600 bg-blue-50 px-3 py-1 rounded-xl border border-blue-200/60">
              {{ filteredMinistriesPerformance?.length ?? 0 }} Bộ/Ngành
            </span>
            <span class="text-slate-500 text-xs font-bold px-2.5 py-1 bg-slate-100 rounded-lg">
              {{ isSection1Collapsed ? '▼ Mở rộng' : '▲ Thu gọn' }}
            </span>
          </div>
        </div>

        <div v-show="!isSection1Collapsed" class="space-y-4 pt-1">
          <div v-if="filteredMinistriesPerformance?.length" class="space-y-3">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleMinistries" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="blue" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredMinistriesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isMinistriesExpanded = !isMinistriesExpanded"
                class="px-5 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isMinistriesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredMinistriesPerformance.length - 8} Bộ/Ngành khác)` }}</span>
              </button>
            </div>
          </div>

          <div v-if="!filteredMinistriesPerformance?.length" class="p-8 text-center text-xs text-slate-400 italic font-semibold">
            Không có dữ liệu Bộ/Ngành.
          </div>
        </div>
      </div>

      <!-- Section 2: Khối Địa Phương -->
      <div class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div
          @click="isSection2Collapsed = !isSection2Collapsed"
          class="flex items-center justify-between border-b border-slate-100 pb-3 cursor-pointer select-none hover:opacity-80 transition"
        >
          <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
            <span>🏛️ Khối Các Tỉnh / Thành Phố</span>
            <span class="text-xs font-normal text-slate-400">({{ isSection2Collapsed ? 'Nhấp để mở rộng' : 'Nhấp để thu gọn' }})</span>
          </h3>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-emerald-600 bg-emerald-50 px-3 py-1 rounded-xl border border-emerald-200/60">
              {{ filteredProvincesPerformance?.length ?? 0 }} Địa phương
            </span>
            <span class="text-slate-500 text-xs font-bold px-2.5 py-1 bg-slate-100 rounded-lg">
              {{ isSection2Collapsed ? '▼ Mở rộng' : '▲ Thu gọn' }}
            </span>
          </div>
        </div>

        <div v-show="!isSection2Collapsed" class="space-y-4 pt-1">
          <div v-if="filteredProvincesPerformance?.length" class="space-y-3">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleProvinces" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="blue" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredProvincesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isProvincesExpanded = !isProvincesExpanded"
                class="px-5 py-2 text-xs font-bold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isProvincesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredProvincesPerformance.length - 8} Địa phương khác)` }}</span>
              </button>
            </div>
          </div>

          <div v-if="!filteredProvincesPerformance?.length" class="col-span-full p-8 text-center text-xs text-slate-400 italic font-semibold">
            Không có dữ liệu Địa phương.
          </div>
        </div>
      </div>

      <!-- Section 3: Khối Các Cơ Quan / Đơn Vị Khác (Hiển thị khi có dữ liệu được gán chủ trì) -->
      <div v-if="filteredOthersPerformance?.length" class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div
          @click="isSection3Collapsed = !isSection3Collapsed"
          class="flex items-center justify-between border-b border-slate-100 pb-3 cursor-pointer select-none hover:opacity-80 transition"
        >
          <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
            <span>🏢 Khối Các Đơn Vị Khác</span>
            <span class="text-xs font-normal text-slate-400">({{ isSection3Collapsed ? 'Nhấp để mở rộng' : 'Nhấp để thu gọn' }})</span>
          </h3>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-purple-600 bg-purple-50 px-3 py-1 rounded-xl border border-purple-200/60">
              {{ filteredOthersPerformance?.length ?? 0 }} Cơ quan / Đơn vị
            </span>
            <span class="text-slate-500 text-xs font-bold px-2.5 py-1 bg-slate-100 rounded-lg">
              {{ isSection3Collapsed ? '▼ Mở rộng' : '▲ Thu gọn' }}
            </span>
          </div>
        </div>

        <div v-show="!isSection3Collapsed" class="space-y-4 pt-1">
          <div class="space-y-3">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleOthers" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="purple" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredOthersPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isOthersExpanded = !isOthersExpanded"
                class="px-5 py-2 text-xs font-bold text-purple-700 bg-purple-50 hover:bg-purple-100 border border-purple-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isOthersExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredOthersPerformance.length - 8} Đơn vị khác)` }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Section 4 (Admin View Only): Khối Các Đơn Vị Trực Thuộc Bộ Khoa học và Công nghệ -->
      <div v-if="authState.isAdmin.value && filteredMostSubAgenciesPerformance?.length" class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div
          @click="isSection4Collapsed = !isSection4Collapsed"
          class="flex items-center justify-between border-b border-slate-100 pb-3 cursor-pointer select-none hover:opacity-80 transition"
        >
          <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
            <span>🏛️ Các Đơn Vị Trực Thuộc Bộ Khoa Học và Công Nghệ</span>
            <span class="text-xs font-normal text-slate-400">({{ isSection4Collapsed ? 'Nhấp để mở rộng' : 'Nhấp để thu gọn' }})</span>
          </h3>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-indigo-600 bg-indigo-50 px-3 py-1 rounded-xl border border-indigo-200/60">
              {{ filteredMostSubAgenciesPerformance?.length ?? 0 }} Đơn vị trực thuộc
            </span>
            <span class="text-slate-500 text-xs font-bold px-2.5 py-1 bg-slate-100 rounded-lg">
              {{ isSection4Collapsed ? '▼ Mở rộng' : '▲ Thu gọn' }}
            </span>
          </div>
        </div>

        <div v-show="!isSection4Collapsed" class="space-y-4 pt-1">
          <div class="space-y-3">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleMostSubAgencies" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="indigo" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredMostSubAgenciesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isMostExpanded = !isMostExpanded"
                class="px-5 py-2 text-xs font-bold text-indigo-700 bg-indigo-50 hover:bg-indigo-100 border border-indigo-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isMostExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredMostSubAgenciesPerformance.length - 8} Đơn vị khác)` }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>

    </div>
    </div>

    <!-- Drilldown Sub-agencies, Tasks List & Contact Persons Modal -->
    <AgencyDrilldownDialog
      :getStatusBadgeClass="getStatusBadgeClass"
      :getStatusLabel="getStatusLabel"
      :formatDate="formatDate"
      :formatItemProgressDisplay="formatItemProgressDisplay"
      :agencies="agencies"
      :dashboardFilter="dashboardFilter"
      :isBKHCNItem="isBKHCNItem"
      :subAgenciesList="subAgenciesList"
      :isSubAgenciesLoading="isSubAgenciesLoading"
      :drilldownTab="drilldownTab"
      :isAgencyItemsLoading="isAgencyItemsLoading"
      :modalGoalsList="modalGoalsList"
      :modalTasksList="modalTasksList"
      :filteredAgencyItems="filteredAgencyItems"
      :allDrilldownContacts="allDrilldownContacts"
      :switchDrilldownTab="switchDrilldownTab"
      v-model:modalSearchKeyword="modalSearchKeyword"
      v-model:modalStatusFilter="modalStatusFilter"
      v-model:selectedDrilldownAgency="selectedDrilldownAgency"
      v-model:selectedDetailItem="selectedDetailItem"
      v-if="selectedDrilldownAgency"
    />



    <!-- Item Detail Modal -->
    <ItemDetailModal v-if="selectedDetailItem != null"
      :isOpen="selectedDetailItem != null"
      :item="selectedDetailItem"
      @close="selectedDetailItem = null"
    />

    <!-- Legal File Viewer Modal -->
    <LegalFileViewerModal v-if="isLegalViewerOpen"
      :isOpen="isLegalViewerOpen"
      :fileItem="selectedLegalViewerFile"
      @close="isLegalViewerOpen = false"
    />
  </div>
</template>

<script setup>
import AgencyProgressCard from '../features/dashboard/AgencyProgressCard.vue';
import DashboardHeader from '../features/dashboard/DashboardHeader.vue';
import DashboardSummary from '../features/dashboard/DashboardSummary.vue';
const AgencyDrilldownDialog = defineAsyncComponent(() => import('../features/dashboard/AgencyDrilldownDialog.vue'));
import { defineAsyncComponent, ref, onMounted } from 'vue';
import { authState } from '../services/auth';
import { toast } from 'vue3-toastify';

import LoadingSpinner from '../components/LoadingSpinner.vue';
import MiniStatusDonut from '../components/MiniStatusDonut.vue';

const ItemDetailModal = defineAsyncComponent(() => import('../components/ItemDetailModal.vue'));
const LegalFileViewerModal = defineAsyncComponent(() => import('../components/LegalFileViewerModal.vue'));

import AllAgenciesProgressChart from '../components/AllAgenciesProgressChart.vue';
import { getStatusBadgeClass, getStatusLabel, formatDate, formatItemProgressDisplay } from '../features/dashboard/presentation';
import { useDashboardFilters } from '../features/dashboard/useDashboardFilters';
import { useDashboardMetrics } from '../features/dashboard/useDashboardMetrics';
import { useAgencyDrilldown } from '../features/dashboard/useAgencyDrilldown';
import { useLegalDashboard } from '../features/dashboard/useLegalDashboard';

const agencies = ref([]);
const {
  dashboardFilter,
  setDashboardFilter,
  dashboardTitleText,
  dashboardItemNoun,
  dashboardItemNounCap,
  selectedAgencyIds,
  selectedSubAgencyIds,
  selectedScopes,
  selectedSections,
  selectedGroups,
  fromYear,
  toYear,
  isOngoingOnly,
  dashboardFilterOptions,
  scopeOptions,
  activeDashboardFilterCount,
  resetDashboardFilters,
  isSpecialAgencyCode,
  leadAgencyOptions,
  subAgencyOptions,
  agencyOptions,
  yearOptions,
  sectionOptions,
  groupOptions
} = useDashboardFilters({ agencies, loadDashboardMetrics: () => loadDashboardMetrics() });

const {
  isLoading,
  loadError,
  metrics,
  activeStatusSummary,
  activeStatusTotal,
  overallDonutStats,
  createdMetrics,
  activeCreatedStatusSummary,
  activeCreatedTotals,
  createdDonutStats,
  userSubAgenciesPerformance,
  isMinistriesExpanded,
  isProvincesExpanded,
  isOthersExpanded,
  isSubAgenciesExpanded,
  isSection1Collapsed,
  isSection2Collapsed,
  isSection3Collapsed,
  isSection4Collapsed,
  filterOutSpecialAgencies,
  getAgencySortCount,
  sortAgenciesByCount,
  filteredMinistriesPerformance,
  filteredProvincesPerformance,
  filteredOthersPerformance,
  visibleMinistries,
  visibleProvinces,
  visibleOthers,
  visibleSubAgencies,
  mostSubAgenciesPerformance,
  isMostExpanded,
  filteredMostSubAgenciesPerformance,
  visibleMostSubAgencies,
  userAgencyName,
  loggedUserAgencyId,
  loggedUserAgency,
  isSubAgencyUser,
  isLevel3User,
  isBKHCNAgency,
  isBKHCNItem,
  singleSubAgencyPerformance,
  getPct,
  loadAgencies,
  loadDashboardMetrics
} = useDashboardMetrics({ dashboardFilter, selectedAgencyIds, selectedSubAgencyIds, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, isSpecialAgencyCode });

const {
  selectedDrilldownAgency,
  subAgenciesList,
  isSubAgenciesLoading,
  drilldownTab,
  selectedDetailItem,
  agencyItemsList,
  isAgencyItemsLoading,
  modalSearchKeyword,
  modalItemTypeFilter,
  modalStatusFilter,
  modalGoalsList,
  modalTasksList,
  filteredAgencyItems,
  allDrilldownContacts,
  loadAgencyItems,
  loadSubAgencies,
  switchDrilldownTab,
  drilldownAgency
} = useAgencyDrilldown({ dashboardFilter, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, metrics });

const {
  legalStats,
  isLegalStatsLoading,
  legalFilterDocumentType,
  legalFilterIssuedFromDate,
  legalFilterIssuedToDate,
  legalFilterEffectiveFromDate,
  legalFilterEffectiveToDate,
  legalFilterIssuingAgencyId,
  legalFilterDraftingAgencyId,
  legalMinistriesStats,
  legalProvincesStats,
  isLegalViewerOpen,
  selectedLegalViewerFile,
  legalDocumentTypeOptions,
  legalAgencyOptions,
  activeLegalFilterCount,
  resetLegalFilters,
  loadLegalDashboardStats,
  getLegalAgencyBarPercent,
  openLegalViewerForDoc,
  getEffectStatusBadgeClass
} = useLegalDashboard({ agencies, isSpecialAgencyCode });

const isExportingExcel = ref(false);
const isExportingLegalExcel = ref(false);
async function exportDashboardExcelReport() {
  if (isExportingExcel.value) return;
  isExportingExcel.value = true;
  try {
    const { exportDashboardReport } = await import('../features/dashboard/dashboardReport');
    await exportDashboardReport({ dashboardFilter, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, metrics, filterOutSpecialAgencies, filteredMinistriesPerformance, filteredProvincesPerformance, filteredOthersPerformance, formatDate, formatItemProgressDisplay, isExportingExcel });
  } catch (error) {
    toast.error('Không thể tải chức năng xuất Excel. Vui lòng thử lại.');
  } finally { isExportingExcel.value = false; }
}
async function exportLegalStatsExcelReport() {
  if (isExportingLegalExcel.value) return;
  isExportingLegalExcel.value = true;
  try {
    const { exportLegalReport } = await import('../features/dashboard/legalReport');
    await exportLegalReport({ formatDate, legalStats, legalFilterDocumentType, legalFilterIssuedFromDate, legalFilterIssuedToDate, legalFilterIssuingAgencyId, legalFilterDraftingAgencyId, legalMinistriesStats, legalProvincesStats, isExportingLegalExcel });
  } catch (error) {
    toast.error('Không thể tải chức năng xuất Excel. Vui lòng thử lại.');
  } finally { isExportingLegalExcel.value = false; }
}

onMounted(async () => {
  await loadAgencies();
  await Promise.all([loadDashboardMetrics(), loadLegalDashboardStats()]);
});
</script>
