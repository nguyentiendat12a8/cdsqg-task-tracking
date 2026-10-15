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
      :isLoading="isLoading"
      :exportDashboardExcelReport="exportDashboardExcelReport"
      v-model:assignmentRole="assignmentRole"
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

      <div v-if="!isSubAgencyUser" class="grid grid-cols-1 lg:grid-cols-2 gap-4 items-stretch" data-testid="admin-overview-layout">
      <div class="min-w-0 space-y-3.5" data-testid="admin-overview-left">

      <DashboardSummary
      :dashboardFilter="dashboardFilter"
      :dashboardItemNoun="dashboardItemNoun"
      :activeStatusSummary="activeStatusSummary"
      :activeStatusTotal="activeStatusTotal"
      :overallDonutStats="overallDonutStats"
      :activeCreatedStatusSummary="activeCreatedStatusSummary"
      :activeCreatedTotals="activeCreatedTotals"
      :createdDonutStats="createdDonutStats"
      @select-statistic="summarySelection = $event"
    />
      </div>
      <div class="relative min-w-0">
      <ProvinceProgressMap :agencies="filteredProvincesPerformance" :filterType="dashboardFilter" fit-height @select-agency="drilldownAgency" class="min-w-0 lg:absolute lg:inset-0" data-testid="province-map-panel" />
      </div>
      </div>

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
            class="ui-single-line px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-2xs transition flex items-center gap-1.5 cursor-pointer shrink-0"
          >
            <span>📋 Xem danh sách chi tiết {{ dashboardItemNoun }}</span>
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
                <div class="text-xs font-bold text-slate-800">Tổng số: {{ formatNumber((dashboardFilter === 'goals' ? singleSubAgencyPerformance.totalGoals : singleSubAgencyPerformance.totalTasks) || 0) }} {{ dashboardItemNoun }}</div>
              </div>
            </div>

            <!-- 6 Status Legend Breakdown List on Right -->
            <div class="md:col-span-7 space-y-2 font-bold text-xs">
              <div class="flex items-center justify-between p-2.5 rounded-xl bg-white border border-slate-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-slate-400"></span>
                  <span class="text-slate-700">Chưa thực hiện</span>
                </div>
                <span class="font-bold text-slate-900 text-sm">{{ formatNumber(singleSubAgencyPerformance.notStarted || 0) }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-blue-50/70 border border-blue-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-blue-500"></span>
                  <span class="text-blue-800">Đang thực hiện (trong hạn)</span>
                </div>
                <span class="font-bold text-blue-900 text-sm">{{ formatNumber(singleSubAgencyPerformance.inProgressOnTime || 0) }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-rose-50/70 border border-rose-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-rose-500"></span>
                  <span class="text-rose-800">Đang thực hiện (quá hạn)</span>
                </div>
                <span class="font-bold text-rose-900 text-sm">{{ formatNumber(singleSubAgencyPerformance.inProgressOverdue || 0) }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-emerald-50/70 border border-emerald-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-emerald-500"></span>
                  <span class="text-emerald-800">Hoàn thành (đúng hạn)</span>
                </div>
                <span class="font-bold text-emerald-900 text-sm">{{ formatNumber(singleSubAgencyPerformance.completedOnTime || 0) }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-teal-50/70 border border-teal-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-teal-500"></span>
                  <span class="text-teal-800">Hoàn thành (quá hạn)</span>
                </div>
                <span class="font-bold text-teal-900 text-sm">{{ formatNumber(singleSubAgencyPerformance.completedOverdue || 0) }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-amber-50/70 border border-amber-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-amber-500"></span>
                  <span class="text-amber-800">Sắp hết hạn</span>
                </div>
                <span class="font-bold text-amber-900 text-sm">{{ formatNumber(singleSubAgencyPerformance.expiringSoon || 0) }}</span>
              </div>
            </div>

          </div>
        </div>
      </div>

      <!-- 2. Subordinate Child Agencies Progress Block (Khối các đơn vị trực thuộc - Chỉ hiển thị cho BKHCN đối với Cấp 2) -->
      <div v-if="!isLevel3User && isBKHCNAgency" class="bg-white p-4.5 rounded-2xl shadow-sm border border-slate-200/80 space-y-4 w-full">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
              🏛️ Khối các đơn vị trực thuộc
            </h3>
          </div>
        </div>

        <div v-if="userSubAgenciesPerformance?.length" class="space-y-3">
          <div class="agency-card-scroll grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
            <AgencyProgressCard v-for="(item, index) in visibleSubAgencies" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="blue" @select="drilldownAgency" />
          </div>

          <!-- Expand / Collapse Button -->
          <div v-if="(userSubAgenciesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
            <button
              @click="isSubAgenciesExpanded = !isSubAgenciesExpanded"
              class="ui-single-line px-5 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
            >
              <span>{{ isSubAgenciesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${userSubAgenciesPerformance.length - 8} đơn vị trực thuộc khác)` }}</span>
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

      <section class="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden" aria-labelledby="agency-block-heading">
        <div class="px-4 pt-4 sm:px-5 sm:pt-5 space-y-3">
          <div>
            <h2 id="agency-block-heading" class="text-base font-semibold text-slate-900">Tiến độ theo khối cơ quan</h2>
            <p class="mt-1 text-xs text-slate-600">Chọn khối để xem tiến độ và mở chi tiết từng đơn vị. Số lượng theo bộ lọc hiện tại.</p>
          </div>
          <div role="tablist" aria-label="Khối cơ quan" class="flex gap-1 rounded-xl bg-slate-100 p-1 overflow-x-auto">
            <button v-for="(tab, index) in agencyBlockTabs" :key="tab.value" :id="`agency-tab-${tab.value}`" type="button" role="tab" :aria-selected="activeAgencyBlock === tab.value" :aria-controls="visitedAgencyBlocks.has(tab.value) ? `agency-panel-${tab.value}` : undefined" :tabindex="activeAgencyBlock === tab.value ? 0 : -1" @click="selectAgencyBlock(tab.value)" @keydown="handleAgencyTabKey($event, index)" :class="['ui-single-line flex-1 inline-flex items-center justify-center gap-2 px-3 text-sm font-semibold whitespace-nowrap rounded-lg transition', activeAgencyBlock === tab.value ? 'bg-white text-blue-700 shadow-sm' : 'text-slate-600 hover:bg-white/60 hover:text-slate-900']">
              {{ tab.label }} <span :class="['rounded-full px-2 py-0.5 text-xs tabular-nums', activeAgencyBlock === tab.value ? 'bg-blue-50 text-blue-700' : 'bg-slate-200/70 text-slate-600']">{{ formatNumber(tab.count) }}</span>
            </button>
          </div>
        </div>
      <!-- Section 1: Khối Bộ / Ngành -->
      <div v-if="visitedAgencyBlocks.has('ministries')" v-show="activeAgencyBlock === 'ministries'" id="agency-panel-ministries" role="tabpanel" aria-labelledby="agency-tab-ministries" tabindex="0" class="p-4 sm:p-5 space-y-4">


        <div class="rounded-2xl border border-slate-200 bg-white p-3 space-y-3">
          <div class="flex flex-wrap gap-2" role="group" aria-label="Chọn loại biểu đồ">
            <button v-for="tab in agencyChartTabs" :key="tab.value" type="button" :aria-pressed="agencyChartViews.ministries === tab.value" @click="agencyChartViews.ministries = tab.value" :class="['ui-single-line px-4 text-sm font-semibold rounded-lg border transition', agencyChartViews.ministries === tab.value ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50']">{{ tab.label }}</button>
          </div>
          <AllAgenciesProgressChart v-if="agencyChartViews.ministries === 'bar'" :ministriesData="filteredMinistriesPerformance" :filterType="dashboardFilter" @select-agency="drilldownAgency" />
          <div v-if="agencyChartViews.ministries === 'pie' && filteredMinistriesPerformance?.length" class="space-y-3">
            <div class="agency-card-scroll grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleMinistries" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="blue" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredMinistriesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isMinistriesExpanded = !isMinistriesExpanded"
                class="ui-single-line px-5 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isMinistriesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredMinistriesPerformance.length - 8} bộ, ngành khác)` }}</span>
              </button>
            </div>
          </div>

          <div v-if="agencyChartViews.ministries === 'pie' && !filteredMinistriesPerformance?.length" class="p-8 text-center text-xs text-slate-400 italic font-semibold">
            Không có dữ liệu bộ, ngành.
          </div>
        </div>
      </div>

      <!-- Section 2: Khối Địa Phương -->
      <div v-if="visitedAgencyBlocks.has('provinces')" v-show="activeAgencyBlock === 'provinces'" id="agency-panel-provinces" role="tabpanel" aria-labelledby="agency-tab-provinces" tabindex="0" class="p-4 sm:p-5 space-y-4">


        <div class="rounded-2xl border border-slate-200 bg-white p-3 space-y-3" data-testid="province-visualization-block">
          <div class="flex flex-wrap gap-2" role="group" aria-label="Chọn loại biểu đồ">
            <button v-for="tab in provinceChartTabs" :key="tab.value" type="button" :aria-pressed="agencyChartViews.provinces === tab.value" @click="agencyChartViews.provinces = tab.value" :class="['ui-single-line px-4 text-sm font-semibold rounded-lg border transition', agencyChartViews.provinces === tab.value ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50']">{{ tab.label }}</button>
          </div>
          <div class="grid grid-cols-1 gap-3 items-start">
          <div class="min-w-0 space-y-3" data-testid="province-chart-panel">
          <AllAgenciesProgressChart v-if="agencyChartViews.provinces === 'bar'" :ministriesData="filteredProvincesPerformance" :filterType="dashboardFilter" @select-agency="drilldownAgency" />
          <div v-if="agencyChartViews.provinces === 'pie' && filteredProvincesPerformance?.length" class="space-y-3">
            <div class="agency-card-scroll grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleProvinces" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="blue" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredProvincesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isProvincesExpanded = !isProvincesExpanded"
                class="ui-single-line px-5 py-2 text-xs font-bold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isProvincesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredProvincesPerformance.length - 8} địa phương khác)` }}</span>
              </button>
            </div>
          </div>

          <div v-if="agencyChartViews.provinces === 'pie' && !filteredProvincesPerformance?.length" class="col-span-full p-8 text-center text-xs text-slate-400 italic font-semibold">
            Không có dữ liệu địa phương.
          </div>
          </div>
          </div>
        </div>
      </div>

      <!-- Section 3: Khối Các Cơ Quan / Đơn Vị Khác (Hiển thị khi có dữ liệu được gán chủ trì) -->
      <div v-if="visitedAgencyBlocks.has('others')" v-show="activeAgencyBlock === 'others'" id="agency-panel-others" role="tabpanel" aria-labelledby="agency-tab-others" tabindex="0" class="p-4 sm:p-5 space-y-4">


        <div class="rounded-2xl border border-slate-200 bg-white p-3 space-y-3">
          <div class="flex flex-wrap gap-2" role="group" aria-label="Chọn loại biểu đồ">
            <button v-for="tab in agencyChartTabs" :key="tab.value" type="button" :aria-pressed="agencyChartViews.others === tab.value" @click="agencyChartViews.others = tab.value" :class="['ui-single-line px-4 text-sm font-semibold rounded-lg border transition', agencyChartViews.others === tab.value ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50']">{{ tab.label }}</button>
          </div>
          <AllAgenciesProgressChart v-if="agencyChartViews.others === 'bar'" :ministriesData="filteredOthersPerformance" :filterType="dashboardFilter" @select-agency="drilldownAgency" />
          <div v-if="agencyChartViews.others === 'pie'" class="space-y-3">
            <div class="agency-card-scroll grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleOthers" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="purple" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredOthersPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isOthersExpanded = !isOthersExpanded"
                class="ui-single-line px-5 py-2 text-xs font-bold text-purple-700 bg-purple-50 hover:bg-purple-100 border border-purple-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isOthersExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredOthersPerformance.length - 8} đơn vị khác)` }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      </section>

      <!-- Section 4 (Admin View Only): Khối các đơn vị trực thuộc Bộ Khoa học và Công nghệ -->
      <div v-if="authState.isAdmin.value && filteredMostSubAgenciesPerformance?.length" class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div
          class="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 pb-3"
        >
          <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
            <span>🏛️ Các đơn vị trực thuộc Bộ Khoa học và Công nghệ</span>
          </h3>
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-indigo-600 bg-indigo-50 px-3 py-1 rounded-xl border border-indigo-200/60">
              {{ filteredMostSubAgenciesPerformance?.length ?? 0 }} đơn vị trực thuộc
            </span>
          </div>
        </div>

        <div class="space-y-4 pt-1">
          <div class="flex flex-wrap gap-2" role="group" aria-label="Chọn loại biểu đồ">
            <button v-for="tab in agencyChartTabs" :key="tab.value" type="button" :aria-pressed="agencyChartViews.subAgencies === tab.value" @click="agencyChartViews.subAgencies = tab.value" :class="['ui-single-line px-4 text-sm font-semibold rounded-lg border transition', agencyChartViews.subAgencies === tab.value ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-slate-700 border-slate-200 hover:bg-slate-50']">{{ tab.label }}</button>
          </div>
          <AllAgenciesProgressChart v-if="agencyChartViews.subAgencies === 'bar'" :ministriesData="filteredMostSubAgenciesPerformance" :filterType="dashboardFilter" @select-agency="drilldownAgency" />
          <div v-if="agencyChartViews.subAgencies === 'pie'" class="space-y-3">
            <div class="agency-card-scroll grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
              <AgencyProgressCard v-for="(item, index) in visibleMostSubAgencies" :key="item.agencyId" :item="item" :index="index" :dashboardFilter="dashboardFilter" tone="indigo" @select="drilldownAgency" />
            </div>

            <!-- Expand / Collapse Button -->
            <div v-if="(filteredMostSubAgenciesPerformance?.length || 0) > 8" class="pt-2 text-center border-t border-slate-100">
              <button
                @click="isMostExpanded = !isMostExpanded"
                class="ui-single-line px-5 py-2 text-xs font-bold text-indigo-700 bg-indigo-50 hover:bg-indigo-100 border border-indigo-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
              >
                <span>{{ isMostExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${filteredMostSubAgenciesPerformance.length - 8} đơn vị khác)` }}</span>
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



    <SummaryStatusDialog v-if="summarySelection" :selection="summarySelection" :query="metricsQuery" :item-type="dashboardFilter === 'goals' ? 'Goal' : 'Task'" @close="summarySelection = null" @select-item="selectedDetailItem = $event" />
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
import { formatNumber } from "../shared/formatters";
import AgencyProgressCard from '../features/dashboard/AgencyProgressCard.vue';
import DashboardHeader from '../features/dashboard/DashboardHeader.vue';
import DashboardSummary from '../features/dashboard/DashboardSummary.vue';
const SummaryStatusDialog = defineAsyncComponent(() => import('../features/dashboard/SummaryStatusDialog.vue'));
const summarySelection = ref(null);
const AgencyDrilldownDialog = defineAsyncComponent(() => import('../features/dashboard/AgencyDrilldownDialog.vue'));
import { defineAsyncComponent, ref, computed, onMounted } from 'vue';
import { authState } from '../services/auth';
import { toast } from 'vue3-toastify';

import LoadingSpinner from '../components/LoadingSpinner.vue';
import MiniStatusDonut from '../components/MiniStatusDonut.vue';

const ItemDetailModal = defineAsyncComponent(() => import('../components/ItemDetailModal.vue'));
const LegalFileViewerModal = defineAsyncComponent(() => import('../components/LegalFileViewerModal.vue'));

import AllAgenciesProgressChart from '../components/AllAgenciesProgressChart.vue';
const ProvinceProgressMap = defineAsyncComponent(() => import('../features/dashboard/ProvinceProgressMap.vue'));
const provinceChartTabs = [{ value: 'bar', label: 'Biểu đồ cột' }, { value: 'pie', label: 'Biểu đồ tròn' }];
const agencyChartViews = ref({ ministries: 'bar', provinces: 'bar', others: 'bar', subAgencies: 'bar' });
const activeAgencyBlock = ref('ministries');
const visitedAgencyBlocks = ref(new Set(['ministries']));
const agencyBlockTabs = computed(() => [
  { value: 'ministries', label: 'Khối các bộ, ngành Trung ương', count: filteredMinistriesPerformance.value?.length || 0 },
  { value: 'provinces', label: 'Khối các tỉnh, thành phố', count: filteredProvincesPerformance.value?.length || 0 },
  { value: 'others', label: 'Khối các đơn vị khác', count: filteredOthersPerformance.value?.length || 0 }
]);
function selectAgencyBlock(value) {
  visitedAgencyBlocks.value.add(value);
  activeAgencyBlock.value = value;
}
function handleAgencyTabKey(event, index) {
  const keys = ['ArrowLeft', 'ArrowRight', 'Home', 'End'];
  if (!keys.includes(event.key)) return;
  event.preventDefault();
  const tabs = agencyBlockTabs.value;
  const next = event.key === 'Home' ? 0 : event.key === 'End' ? tabs.length - 1 : (index + (event.key === 'ArrowRight' ? 1 : -1) + tabs.length) % tabs.length;
  selectAgencyBlock(tabs[next].value);
  document.getElementById(`agency-tab-${tabs[next].value}`)?.focus();
}
const agencyChartTabs = [{ value: 'bar', label: 'Biểu đồ cột' }, { value: 'pie', label: 'Biểu đồ tròn' }];
import { getStatusBadgeClass, getStatusLabel, formatDate, formatItemProgressDisplay } from '../features/dashboard/presentation';
import { useDashboardFilters } from '../features/dashboard/useDashboardFilters';
import { useDashboardMetrics } from '../features/dashboard/useDashboardMetrics';
import { useAgencyDrilldown } from '../features/dashboard/useAgencyDrilldown';
import { useLegalDashboard } from '../features/dashboard/useLegalDashboard';

const agencies = ref([]);
const {
  assignmentRole,
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
  metricsQuery,
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
} = useDashboardMetrics({ assignmentRole, dashboardFilter, selectedAgencyIds, selectedSubAgencyIds, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, isSpecialAgencyCode });

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
} = useAgencyDrilldown({ assignmentRole, dashboardFilter, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, metrics });

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
  if (isExportingExcel.value || isLoading.value) return;
  isExportingExcel.value = true;
  try {
    const { exportDashboardReport } = await import('../features/dashboard/dashboardReport');
    await exportDashboardReport({ assignmentRole, dashboardFilter, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, metrics, filterOutSpecialAgencies, filteredMinistriesPerformance, filteredProvincesPerformance, filteredOthersPerformance, formatDate, formatItemProgressDisplay, isExportingExcel });
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
  const agenciesReady = loadAgencies();
  await Promise.all([agenciesReady, loadDashboardMetrics(agenciesReady), loadLegalDashboardStats()]);
});
</script>

<style scoped>
.agency-card-scroll {
  max-height: min(640px, 70vh);
  overflow-y: auto;
  overscroll-behavior-y: contain;
  scrollbar-gutter: stable;
  padding: 2px 4px 2px 2px;
  align-content: start;
}
</style>




