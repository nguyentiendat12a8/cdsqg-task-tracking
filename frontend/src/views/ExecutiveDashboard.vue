<template>
  <div class="executive-dashboard w-full space-y-3 min-w-0" :aria-busy="isLoading">
    <DashboardHeader :active-dashboard-filter-count="activeDashboardFilterCount" :reset-dashboard-filters="resetDashboardFilters" :year-options="dashboardYearOptions" :load-dashboard-metrics="loadDashboardMetrics" :is-exporting-excel="isExportingExcel" :is-loading="isLoading" :export-dashboard-excel-report="exportDashboardExcelReport"
      v-model:scope="dashboardScope" v-model:reportFromYear="reportFromYear" v-model:reportToYear="reportToYear" v-model:executionFrom="executionFrom" v-model:executionTo="executionTo" v-model:isOngoingOnly="isOngoingOnly" />


    <div v-if="loadError" role="alert" class="flex items-center justify-between gap-3 rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">{{ loadError }}<button type="button" class="ui-btn ui-btn-secondary" @click="loadDashboardMetrics">Thử lại</button></div>
    <div v-if="isLoading && !hasDashboardData" class="space-y-4" role="status" aria-label="Đang tải bảng điều hành"><div class="grid grid-cols-2 xl:grid-cols-4 gap-3"><div v-for="index in 4" :key="index" class="h-40 rounded-2xl border border-slate-200 bg-white p-5"><div class="executive-skeleton h-3 w-24 rounded"></div><div class="executive-skeleton mt-6 h-8 w-16 rounded"></div></div></div><div class="executive-skeleton h-96 rounded-2xl"></div><span class="sr-only">Đang tải dữ liệu bảng điều hành.</span></div>
    <p v-else-if="isLoading" role="status" class="text-xs text-blue-700">Đang cập nhật dữ liệu theo bộ lọc...</p>
    <main v-if="hasDashboardData && !loadError" :inert="isLoading" class="space-y-3" :class="{'opacity-60':isLoading}">

      <div class="grid grid-cols-1 xl:grid-cols-2 gap-3 items-start" data-testid="dashboard-kpi-groups">
      <LeadershipOverview :show-assignments="authState.isAdmin.value" v-for="kind in dashboardKinds" :key="kind.value" :summary="summaryFor(kind.value)" :assignment-summary="metrics[kind.prefix+'StatusSummary'] || {}" :total="totalFor(kind.value)" :noun="kind.noun" :unit-count="performanceAgencies.filter(a=>a[kind.total]>0).length" :scope-counts="authState.isAdmin.value?{specific:metrics.createdMetrics?.[kind.prefix+'sSpecific']||0,general:metrics.createdMetrics?.[kind.prefix+'sGeneral']||0}:null" @select-statistic="openStatistic({...$event,itemType:kind.itemType})" />
      </div>
      <div v-if="authState.isAdmin.value" class="grid grid-cols-1 lg:grid-cols-[minmax(0,1.8fr)_minmax(0,1fr)] gap-3 items-stretch">
        <section v-if="!isSubAgencyUser" class="min-w-0 executive-map" aria-label="Theo dõi địa phương trên bản đồ"><ProvinceProgressMap :agencies="filteredProvincesPerformance" :filter-type="mapFilter" fit-height @select-agency="openAgency($event,mapFilter)" data-testid="province-map-panel"><template #type-switch><DashboardTypeSwitch v-model="mapFilter" label="Loại dữ liệu bản đồ" /></template></ProvinceProgressMap></section>

        <LeadershipWatchlist :agencies="performanceAgencies" :leaders="leadingAgencies" noun="mục tiêu và nhiệm vụ" @select-agency="openAgency($event,'all')" @select-overdue-agency="openOverdueAgency" />
      </div>
      <div v-if="authState.isAdmin.value" class="grid grid-cols-1 lg:grid-cols-[minmax(0,1.8fr)_minmax(0,1fr)] gap-3 items-stretch" data-testid="dashboard-comparison-groups">
        <AgencyPerformanceBoard :agencies="performanceAgencies" @select-agency="openAgency" />
        <div class="executive-groups min-w-0"><GroupPerformanceBoard :statistics="metrics.groupStatistics || []" /></div>
      </div>
      <div v-if="!authState.isAdmin.value" class="agency-dashboard-grid grid grid-cols-1 lg:grid-cols-[minmax(0,1.8fr)_minmax(0,1fr)] gap-3 items-stretch" data-testid="agency-dashboard-content">
        <AgencyWorkQueue :agency-id="authState.user.value?.agencyId || ''" :agency-name="singleSubAgencyPerformance?.name || loggedUserAgency?.name || authState.user.value?.agencyName || ''" :query="metricsQuery" @select-item="selectedDetailItem=$event" />
        <div class="agency-dashboard-sidebar min-h-0 min-w-0 flex flex-col gap-3"><LeadershipWatchlist :show-coverage="false" :agencies="singleSubAgencyPerformance ? [singleSubAgencyPerformance] : []" noun="mục tiêu và nhiệm vụ" @select-overdue-agency="openOverdueAgency" /><GroupPerformanceBoard :statistics="metrics.groupStatistics || []" /></div>
      </div>
      <AgencyPerformanceBoard v-if="subordinatePerformance.length" :show-agency-groups="authState.isAdmin.value" :agencies="subordinatePerformance" title="Kết quả các đơn vị trực thuộc" @select-agency="openAgency" />
    </main>
    <!-- Drilldown Sub-agencies, Tasks List & Contact Persons Modal -->
    <AgencyDrilldownDialog
      :getStatusBadgeClass="getStatusBadgeClass"
      :getStatusLabel="getStatusLabel"
      :formatDate="formatDate"
      :formatItemProgressDisplay="formatItemProgressDisplay"
      :agencies="agencies"
      :dashboardFilter="drilldownFilter"
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
      v-model:modalDeadlineFilter="modalDeadlineFilter"
      v-model:selectedDrilldownAgency="selectedDrilldownAgency"
      v-model:selectedDetailItem="selectedDetailItem"
      v-if="selectedDrilldownAgency"
    />



    <SummaryStatusDialog v-if="summarySelection" :selection="summarySelection" :query="metricsQuery" :item-type="summarySelection.itemType" @close="summarySelection = null" @select-item="selectedDetailItem = $event" />
    <!-- Item Detail Modal -->
    <ItemDetailModal v-if="selectedDetailItem != null"
      :isOpen="selectedDetailItem != null"
      :item="selectedDetailItem"
      @close="selectedDetailItem = null"
    />

  </div>
</template>

<script setup>
import { computed, defineAsyncComponent, onMounted, ref } from 'vue';
import { toast } from 'vue3-toastify';
import { authState } from '../services/auth';
import { activeDocumentId, dashboardDocumentScope, SUPPORTED_DOCUMENTS } from '../services/documentContext';
import { formatNumber } from '../shared/formatters';
import DashboardHeader from '../features/dashboard/DashboardHeader.vue';
import DashboardTypeSwitch from '../features/dashboard/DashboardTypeSwitch.vue';
import AgencyWorkQueue from '../features/dashboard/AgencyWorkQueue.vue';
import LeadershipOverview from '../features/dashboard/LeadershipOverview.vue';
import LeadershipWatchlist from '../features/dashboard/LeadershipWatchlist.vue';
import GroupPerformanceBoard from '../features/dashboard/GroupPerformanceBoard.vue';
import AgencyPerformanceBoard from '../features/dashboard/AgencyPerformanceBoard.vue';
import { agencyForDashboard } from '../features/dashboard/leadershipPresentation';
import { getStatusBadgeClass, getStatusLabel, formatDate, formatItemProgressDisplay } from '../features/dashboard/presentation';
import { useDashboardFilters } from '../features/dashboard/useDashboardFilters';
import { useDashboardMetrics } from '../features/dashboard/useDashboardMetrics';
import { useAgencyDrilldown } from '../features/dashboard/useAgencyDrilldown';
const ProvinceProgressMap=defineAsyncComponent(()=>import('../features/dashboard/ProvinceProgressMap.vue'));
const SummaryStatusDialog=defineAsyncComponent(()=>import('../features/dashboard/SummaryStatusDialog.vue'));
const AgencyDrilldownDialog=defineAsyncComponent(()=>import('../features/dashboard/AgencyDrilldownDialog.vue'));
const ItemDetailModal=defineAsyncComponent(()=>import('../components/ItemDetailModal.vue'));
const mapFilter=ref('tasks'),drilldownFilter=ref('tasks');
const dashboardScope=dashboardDocumentScope;
const documentIds=computed(()=>dashboardScope.value==='all'?SUPPORTED_DOCUMENTS.map(doc=>doc.id):[activeDocumentId.value]);
const dashboardYearOptions=computed(()=>dashboardScope.value==='all'?Array.from({length:Math.max(...SUPPORTED_DOCUMENTS.map(doc=>doc.endYear))-Math.min(...SUPPORTED_DOCUMENTS.map(doc=>doc.startYear))+1},(_,index)=>{const year=Math.min(...SUPPORTED_DOCUMENTS.map(doc=>doc.startYear))+index;return {value:year,label:String(year)};}):yearOptions.value);
const dashboardKinds=[{value:'goals',prefix:'goal',total:'totalGoals',noun:'mục tiêu',itemType:'Goal'},{value:'tasks',prefix:'task',total:'totalTasks',noun:'nhiệm vụ',itemType:'Task'}];
const agencies=ref([]),summarySelection=ref(null),isExportingExcel=ref(false);
const {
 assignmentRole,dashboardFilter,setDashboardFilter,dashboardTitleText,dashboardItemNoun,
 selectedAgencyIds,selectedSubAgencyIds,selectedScopes,selectedSections,selectedGroups,fromYear,toYear,isOngoingOnly,reportFromYear,reportToYear,executionFrom,executionTo,
 dashboardFilterOptions,scopeOptions,activeDashboardFilterCount,resetDashboardFilters,isSpecialAgencyCode,
 leadAgencyOptions,subAgencyOptions,yearOptions,sectionOptions,groupOptions
}=useDashboardFilters({agencies,loadDashboardMetrics:()=>loadDashboardMetrics()});
const {
 isLoading,loadError,metrics,metricsQuery,activeStatusSummary,activeCreatedTotals,
 userSubAgenciesPerformance,filteredMinistriesPerformance,filteredProvincesPerformance,filteredOthersPerformance,
 filteredMostSubAgenciesPerformance,loggedUserAgency,isSubAgencyUser,isBKHCNAgency,isBKHCNItem,
 singleSubAgencyPerformance,filterOutSpecialAgencies,loadAgencies,loadDashboardMetrics
}=useDashboardMetrics({assignmentRole,dashboardFilter,documentIds,selectedAgencyIds,selectedSubAgencyIds,selectedScopes,selectedSections,selectedGroups,fromYear,toYear,isOngoingOnly,reportFromYear,reportToYear,executionFrom,executionTo,agencies,isSpecialAgencyCode});
const {
 selectedDrilldownAgency,subAgenciesList,isSubAgenciesLoading,drilldownTab,selectedDetailItem,isAgencyItemsLoading,
 modalSearchKeyword,modalStatusFilter,modalDeadlineFilter,modalGoalsList,modalTasksList,filteredAgencyItems,allDrilldownContacts,switchDrilldownTab,drilldownAgency
}=useAgencyDrilldown({assignmentRole,dashboardFilter:drilldownFilter,documentIds,selectedScopes,selectedSections,selectedGroups,fromYear,toYear,isOngoingOnly,reportFromYear,reportToYear,executionFrom,executionTo,agencies,metrics});
const hasDashboardData=computed(()=>!!metrics.value.overviewMetrics||!!metrics.value.createdMetrics);
const performanceAgencies=computed(()=>[...filteredMinistriesPerformance.value,...filteredProvincesPerformance.value,...filteredOthersPerformance.value].map(a=>({...a,completionRate:a.completionRateOverall})));
const assignedAgencies=computed(()=>performanceAgencies.value.filter(a=>a.totalItems>0));
function summaryFor(type){return !authState.isAdmin.value?agencyForDashboard(singleSubAgencyPerformance.value||{},type):metrics.value.overviewMetrics?.[type==='goals'?'goalStatusSummary':'taskStatusSummary']||{};}
function totalFor(type){const summary=summaryFor(type);return ['notStarted','inProgress','completed'].reduce((sum,key)=>sum+(summary[key]||0),0);}
async function openOverdueAgency(agency){drilldownFilter.value='all';await drilldownAgency(agency,{deadlineStatus:'Overdue'});}
async function openAgency(agency,type='goals'){drilldownFilter.value=type;await drilldownAgency(agency);}
const leadingAgencies=computed(()=>[...assignedAgencies.value].sort((a,b)=>b.totalItems-a.totalItems||a.name.localeCompare(b.name,'vi')).slice(0,2));
const subordinatePerformance=computed(()=> (authState.isAdmin.value?filteredMostSubAgenciesPerformance.value:userSubAgenciesPerformance.value).filter(a=>authState.isAdmin.value||a.totalItems>0).map(a=>({...a,completionRate:a.completionRateOverall})));
async function openStatistic(selection){
 if(authState.isAdmin.value){summarySelection.value=selection;return;}
 if(singleSubAgencyPerformance.value){await openAgency(singleSubAgencyPerformance.value,selection.itemType==='Task'?'tasks':'goals');modalStatusFilter.value=selection.status||'all';}
}
async function exportDashboardExcelReport(){
 if(isExportingExcel.value||isLoading.value||loadError.value)return;
 isExportingExcel.value=true;
 try{const {exportDashboardReport}=await import('../features/dashboard/dashboardReport');await exportDashboardReport({assignmentRole,dashboardFilter,documentIds,selectedScopes,selectedSections,selectedGroups,fromYear,toYear,isOngoingOnly,reportFromYear,reportToYear,executionFrom,executionTo,agencies,metrics,metricsQuery,filterOutSpecialAgencies,filteredMinistriesPerformance,filteredProvincesPerformance,filteredOthersPerformance,formatDate,formatItemProgressDisplay,isExportingExcel});}
 catch{toast.error('Không thể tải chức năng xuất Excel. Vui lòng thử lại.');}
 finally{isExportingExcel.value=false;}
}
onMounted(async()=>{const ready=loadAgencies();await Promise.all([ready,loadDashboardMetrics(ready)]);});
</script>

<style scoped>
.executive-dashboard :deep(.executive-eyebrow){font-size:var(--ui-font-body, .75rem);font-weight:600;letter-spacing:.06em;color:var(--color-slate-500);}
.executive-dashboard :deep(.executive-pill){display:inline-flex;align-items:center;border:1px solid var(--color-slate-200);border-radius:999px;padding:.25rem .625rem;font-size:var(--ui-font-body,.75rem);color:var(--color-slate-600);background:var(--color-slate-50);white-space:nowrap;}
.executive-skeleton{background:var(--color-slate-200);animation:dashboard-pulse 1.5s ease-in-out infinite;}
.executive-map{min-width:0;}
.agency-dashboard-sidebar :deep(.group-board){max-height:340px;}
.executive-map :deep(.province-map-container){height:100%;border-color:var(--color-blue-200);box-shadow:0 2px 8px rgb(15 23 42 / .05);}
.executive-map :deep(.province-map-container > :first-child){background:var(--color-blue-50);border-color:var(--color-blue-100);}
.executive-map :deep(.province-map-container > :first-child h4){color:var(--color-blue-900);}
@media(min-width:1024px){
  .agency-dashboard-grid{height:clamp(560px,calc(100dvh - 280px),700px);grid-template-rows:minmax(0,1fr);}
  .agency-dashboard-sidebar > :first-child{flex-shrink:0;}
  .agency-dashboard-sidebar :deep(.group-board){flex:1;max-height:none;min-height:0;}
  .agency-dashboard-grid :deep(.agency-work-items){max-height:none;}
  .executive-map{position:relative;min-height:528px;}
  .executive-groups{position:relative;min-height:400px;}
  .executive-groups :deep(.group-board){position:absolute;inset:0;}
  .executive-map :deep(.province-map-container){position:absolute;inset:0;height:100%;}
}
@media(prefers-reduced-motion:reduce){.executive-skeleton{animation:none;}}
@keyframes dashboard-pulse{50%{opacity:.5;}}
</style>

