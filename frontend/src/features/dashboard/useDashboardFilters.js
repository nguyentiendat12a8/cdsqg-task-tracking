import { defaultDashboardTimeFilters } from './dashboardTimeFilters';
import { activeDocument } from '../../services/documentContext';
import { ref, computed } from 'vue';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../../config/planningStructureConfig';

export function useDashboardFilters({ agencies, loadDashboardMetrics }) {
  const dashboardFilter = ref('all'); // Both types are loaded; each visualization switches independently.

  function setDashboardFilter(val) {
    dashboardFilter.value = val;
    loadDashboardMetrics();
  }

  const dashboardTitleText = computed(() => {
    if (dashboardFilter.value === 'goals') return 'Trang chủ theo dõi tiến độ mục tiêu';
    return 'Trang chủ theo dõi tiến độ nhiệm vụ';
  });

  const dashboardItemNoun = computed(() => {
    if (dashboardFilter.value === 'goals') return 'mục tiêu';
    return 'nhiệm vụ';
  });

  const dashboardItemNounCap = computed(() => {
    if (dashboardFilter.value === 'goals') return 'Mục Tiêu';
    return 'Nhiệm Vụ';
  });

  const assignmentRole = ref('Lead');
  const selectedAgencyIds = ref([]);
  const selectedSubAgencyIds = ref([]);
  const selectedScopes = ref([]);
  const selectedSections = ref([]);
  const selectedGroups = ref([]);
  const fromYear = ref(null);
  const toYear = ref(null);
  const defaults = defaultDashboardTimeFilters();
  const isOngoingOnly = ref(defaults.isOngoingOnly);
  const reportFromYear = ref(defaults.reportFromYear), reportToYear = ref(defaults.reportToYear);
  const executionFrom = ref(defaults.executionFrom), executionTo = ref(defaults.executionTo);
  const timeModels = { reportFromYear, reportToYear, executionFrom, executionTo, isOngoingOnly };

  const dashboardFilterOptions = ref([
    { value: 'goals', label: '🎯 Mục tiêu' },
    { value: 'tasks', label: '📋 Nhiệm vụ' }
  ]);

  const scopeOptions = ref([
    { value: 'general', label: 'Phạm vi chung (tất cả đơn vị)' },
    { value: 'specific', label: 'Phạm vi riêng (đơn vị cụ thể)' }
  ]);

  const activeDashboardFilterCount = computed(() => {
    let count = 0;
    if (reportFromYear.value || reportToYear.value) count++;
    if (executionFrom.value || executionTo.value) count++;
    if (isOngoingOnly.value) count++;
    return count;
  });

  function resetDashboardFilters() {
    assignmentRole.value = 'Lead';
    selectedAgencyIds.value = [];
    selectedSubAgencyIds.value = [];
    selectedScopes.value = [];
    selectedSections.value = [];
    selectedGroups.value = [];
    fromYear.value = null;
    toYear.value = null;
    for (const [key, value] of Object.entries(defaultDashboardTimeFilters())) timeModels[key].value = value;
    dashboardFilter.value = 'all';
    loadDashboardMetrics();
  }

  const isSpecialAgencyCode = (code) => code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND' || code === 'ALL_MINISTRIES_DIRECT';

  const leadAgencyOptions = computed(() => {
    return agencies.value.map(ag => {
      if (isSpecialAgencyCode(ag.code)) {
        return { value: ag.id, label: `🌐 ${ag.name}` };
      }
      if (ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000') {
        const parentAg = agencies.value.find(p => p.id === ag.parentId);
        return { value: ag.id, label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name };
      }
      return { value: ag.id, label: ag.name };
    });
  });

  const subAgencyOptions = computed(() => {
    return agencies.value
      .filter(ag => ag.parentId != null && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000')
      .map(ag => {
        const parentAg = agencies.value.find(p => p.id === ag.parentId);
        const parentSuffix = parentAg ? ` (Trực thuộc ${parentAg.name})` : '';
        return { value: ag.id, label: `${ag.name}${parentSuffix}` };
      });
  });

  const agencyOptions = computed(() => leadAgencyOptions.value);

  const yearOptions = computed(() => Array.from({ length: activeDocument.value.endYear - activeDocument.value.startYear + 1 }, (_, i) => activeDocument.value.startYear + i).map(y => ({ value: y, label: String(y) })));

  const sectionOptions = computed(() => [...GOAL_SECTIONS, ...TASK_SECTIONS]);
  const groupOptions = computed(() => [...GOAL_GROUPS, ...TASK_GROUPS]);

  return {
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
  isOngoingOnly, reportFromYear, reportToYear, executionFrom, executionTo,
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
};
}
