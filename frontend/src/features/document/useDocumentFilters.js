import { executionStatusOptions } from '../../shared/statusPresentation';
import { REPORTING_YEARS } from '../../config/reporting';
import { ref, computed, watch, onUnmounted } from 'vue';
import { authState } from '../../services/auth';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../../config/planningStructureConfig';

export function useDocumentFilters({ props, userAgencyId, agencies, rawItemsList, serverTotalCount, serverTotalPages, isGeneralTaskOrAllAgencies, loadData }) {
  const filterDraft = ref({
    searchQuery: '',
    selectedAgencyIds: [],
    selectedSubAgencyIds: [],
    selectedScopes: [],
    selectedStatuses: [],
    selectedSections: [],
    selectedGroups: [],
    fromYear: null,
    toYear: null,
    onlyOngoing: false
  });

  const activeFilterCount = computed(() => {
    let count = 0;
    if (filterDraft.value.selectedAgencyIds?.length) count++;
    if (filterDraft.value.selectedSubAgencyIds?.length) count++;
    if (filterDraft.value.selectedScopes?.length) count++;
    if (filterDraft.value.selectedSections?.length) count++;
    if (filterDraft.value.selectedGroups?.length) count++;
    if (filterDraft.value.fromYear || filterDraft.value.toYear || filterDraft.value.onlyOngoing) count++;
    if (filterDraft.value.selectedStatuses?.length) count++;
    return count;
  });

  const appliedFilters = ref({
    searchQuery: '',
    selectedAgencyIds: [],
    selectedSubAgencyIds: [],
    selectedScopes: [],
    selectedStatuses: [],
    selectedSections: [],
    selectedGroups: [],
    fromYear: null,
    toYear: null,
    onlyOngoing: false
  });

  const currentPage = ref(1);
  const pageSize = ref(10);
  watch(pageSize, () => {
    currentPage.value = 1;
    loadData();
  });
  const sortBy = ref('code');
  const sortOrder = ref('asc');

  function handleSort(columnKey) {
    if (sortBy.value === columnKey) {
      sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
    } else {
      sortBy.value = columnKey;
      if (columnKey === 'progress' || columnKey === 'lastUpdated') {
        sortOrder.value = 'desc';
      } else {
        sortOrder.value = 'asc';
      }
    }
    currentPage.value = 1;
    loadData();
  }

  let docDetailSearchTimer = null;
  let docDetailSearchRequestId = 0;

  function execFilterSearch() {
    if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer);
    docDetailSearchRequestId++;
    appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
    currentPage.value = 1;
    loadData();
  }

  watch(() => filterDraft.value.searchQuery, (newVal) => {
    if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer);
    const currentId = ++docDetailSearchRequestId;
    docDetailSearchTimer = setTimeout(() => {
      if (currentId !== docDetailSearchRequestId) return;
      appliedFilters.value.searchQuery = newVal || '';
      currentPage.value = 1;
      loadData();
    }, 300);
  });

  function resetFilterSearch() {
    if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer);
    docDetailSearchRequestId++;
    filterDraft.value = {
      searchQuery: '',
      selectedAgencyIds: [],
      selectedSubAgencyIds: [],
      selectedScopes: [],
      selectedStatuses: [],
      selectedSections: [],
      selectedGroups: [],
      fromYear: null,
      toYear: null,
      onlyOngoing: false
    };
    appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
    currentPage.value = 1;
    loadData();
  }

  function changePage(newPage) {
    if (newPage < 1 || newPage > totalPages.value) return;
    currentPage.value = newPage;
    loadData();
  }

  const sectionFilterOptions = computed(() => {
    return props.filterItemType === 'Goal' ? GOAL_SECTIONS : TASK_SECTIONS;
  });

  const groupFilterOptions = computed(() => {
    return props.filterItemType === 'Goal' ? GOAL_GROUPS : TASK_GROUPS;
  });

  const agencyOptions = computed(() => {
    return agencies.value.map(ag => {
      if (isSpecialAgencyCode(ag.code)) {
        return { value: ag.id, label: `🌐 ${ag.name}` };
      }
      if (ag.parentId) {
        const parentAg = agencies.value.find(p => p.id === ag.parentId);
        return { value: ag.id, label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name };
      }
      return { value: ag.id, label: ag.name };
    });
  });

  const isSpecialAgencyCode = (code) => code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND' || code === 'ALL_MINISTRIES_DIRECT';

  const leadAgencyOptions = computed(() => {
    return agencies.value.map(ag => {
      if (isSpecialAgencyCode(ag.code)) {
        return { value: ag.id, label: `🌐 ${ag.name}` };
      }
      if (ag.parentId) {
        const parentAg = agencies.value.find(p => p.id === ag.parentId);
        return { value: ag.id, label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name };
      }
      return { value: ag.id, label: ag.name };
    });
  });

  const subAgencyOptions = computed(() => {
    return agencies.value
      .filter(ag => ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000')
      .map(ag => {
        const parentAg = agencies.value.find(p => p.id === ag.parentId);
        return {
          value: ag.id,
          label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name
        };
      });
  });

  const yearOptions = computed(() => REPORTING_YEARS.map(y => ({ value: y, label: String(y) })));
  const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

  const scopeOptions = computed(() => [
    { value: 'general', label: props.filterItemType === 'Goal' ? 'Mục tiêu chung (Tất cả đơn vị)' : 'Nhiệm vụ chung (Tất cả đơn vị)' },
    { value: 'specific', label: props.filterItemType === 'Goal' ? 'Mục tiêu riêng (Đơn vị cụ thể)' : 'Nhiệm vụ riêng (Đơn vị cụ thể)' }
  ]);

  const statusOptions = ref(executionStatusOptions);

  // Filtering and sorting happen before pagination on the server.
  const filteredList = computed(() => rawItemsList.value);

  const totalCount = computed(() => serverTotalCount.value);
  const totalPages = computed(() => serverTotalPages.value);

  const paginatedPrimaryList = computed(() => rawItemsList.value);

  onUnmounted(() => { if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer); });

  return {
  filterDraft,
  activeFilterCount,
  appliedFilters,
  currentPage,
  pageSize,
  sortBy,
  sortOrder,
  handleSort,
  execFilterSearch,
  resetFilterSearch,
  changePage,
  sectionFilterOptions,
  groupFilterOptions,
  agencyOptions,
  isSpecialAgencyCode,
  leadAgencyOptions,
  subAgencyOptions,
  yearOptions,
  pageSizeOptions,
  scopeOptions,
  statusOptions,
  filteredList,
  totalCount,
  totalPages,
  paginatedPrimaryList
};
}


