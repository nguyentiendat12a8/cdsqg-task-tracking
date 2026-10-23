<template>
  <header class="flex flex-wrap items-center justify-between gap-x-6 gap-y-3 rounded-2xl border border-blue-200 bg-blue-700 px-4 py-3">
    <div class="flex min-w-0 items-center gap-3">
      <span class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-blue-600 text-white"><svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M4 19V9m5 10V5m6 14v-7m5 7V3M2 21h20" stroke-linecap="round" /></svg></span>
      <div class="min-w-0"><h2 class="ui-page-title text-white">Toàn cảnh thực hiện {{ scope === 'all' ? 'Chiến lược & Chương trình' : activeDocument.shortTitle }}</h2></div>
    </div>
    <div class="flex min-w-0 flex-wrap items-center gap-2">
      <ActionButton kind="export" @click="exportDashboardExcelReport" :disabled="isExportingExcel || isLoading">{{ isExportingExcel ? 'Đang xuất...' : 'Xuất báo cáo Excel' }}</ActionButton>
      <OverlayPanel title="Lọc dữ liệu bảng điều khiển" button-text="Bộ lọc nâng cao" :active-count="activeDashboardFilterCount" placement="bottom-end" :overflow-padding="16" width-class="w-[calc(100vw-2rem)] sm:w-[480px]" :apply-disabled="!!filterError" @show="prepareFilterDraft" @apply="applyFilterDraft" @reset="resetFilterDraft">
        <div class="space-y-4" data-testid="dashboard-time-filters">
          <div>
            <label class="text-xs font-semibold text-slate-700 block mb-1">Thời gian báo cáo (từ năm ➔ đến năm)</label>
            <div class="flex items-center gap-2">
              <SearchableSelect v-model="filterDraft.reportFromYear" :options="yearOptions" :is-multi="false" label="Từ năm" label-class="sr-only" placeholder="Từ năm" class="w-full" />
              <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
              <SearchableSelect v-model="filterDraft.reportToYear" :options="yearOptions" :is-multi="false" label="Đến năm" label-class="sr-only" placeholder="Đến năm" class="w-full" />
            </div>
          </div>
          <div>
            <label class="text-xs font-semibold text-slate-700 block mb-1">Thời gian hoàn thành (từ ngày ➔ đến ngày)</label>
            <div class="flex items-center gap-2">
              <div class="w-full">
                <label :for="`${filterId}-from`" class="sr-only">Từ ngày</label>
                <DatePicker v-model="filterDraft.executionFrom" :input-id="`${filterId}-from`" placeholder="Từ ngày" />
              </div>
              <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
              <div class="w-full">
                <label :for="`${filterId}-to`" class="sr-only">Đến ngày</label>
                <DatePicker v-model="filterDraft.executionTo" :input-id="`${filterId}-to`" placeholder="Đến ngày" />
              </div>
            </div>
          </div>
          <label class="flex cursor-pointer items-center gap-2 text-sm text-slate-700"><input v-model="filterDraft.isOngoingOnly" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-blue-600" />Nhiệm vụ thường xuyên</label>
          <p v-if="filterError" role="alert" class="text-xs text-rose-700">{{ filterError }}</p>
        </div>
      </OverlayPanel>
    </div>
  </header>
</template>
<script setup>
import { computed, ref, useId } from 'vue';
import { defaultDashboardTimeFilters } from './dashboardTimeFilters';
import { activeDocument } from '../../services/documentContext';
import ActionButton from '../../components/ActionButton.vue';
import SearchableSelect from '../../components/SearchableSelect.vue';
import DatePicker from '../../components/DatePicker.vue';
import OverlayPanel from '../../components/OverlayPanel.vue';
const props = defineProps(['activeDashboardFilterCount', 'resetDashboardFilters', 'yearOptions', 'loadDashboardMetrics', 'isExportingExcel', 'isLoading', 'exportDashboardExcelReport']);
const scope = defineModel('scope', { default: 'current' });
const reportFromYear = defineModel('reportFromYear', { default: null });
const reportToYear = defineModel('reportToYear', { default: null });
const executionFrom = defineModel('executionFrom', { default: '' });
const executionTo = defineModel('executionTo', { default: '' });
const isOngoingOnly = defineModel('isOngoingOnly', { default: false });
const filterId = `dashboard-filter-${useId()}`;
const filterDraft = ref({});
const filterModels = { reportFromYear, reportToYear, executionFrom, executionTo, isOngoingOnly };
const filterError = computed(() => {
  const draft = filterDraft.value;
  if (draft.reportFromYear && draft.reportToYear && Number(draft.reportFromYear) > Number(draft.reportToYear)) return 'Năm báo cáo từ không được lớn hơn năm đến.';
  if (draft.executionFrom && draft.executionTo && draft.executionFrom > draft.executionTo) return 'Ngày hoàn thành từ không được lớn hơn ngày đến.';
  return '';
});
function prepareFilterDraft() { filterDraft.value = Object.fromEntries(Object.entries(filterModels).map(([key, model]) => [key, model.value])); }
function applyFilterDraft() {
  if (filterError.value) return;
  for (const [key, model] of Object.entries(filterModels)) model.value = filterDraft.value[key];
  props.loadDashboardMetrics();
}
function resetFilterDraft() { props.resetDashboardFilters(); filterDraft.value = defaultDashboardTimeFilters(); }
</script>
