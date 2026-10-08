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

  const yearOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));
  const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

  const scopeOptions = computed(() => [
    { value: 'general', label: props.filterItemType === 'Goal' ? 'Mục tiêu chung (Tất cả đơn vị)' : 'Nhiệm vụ chung (Tất cả đơn vị)' },
    { value: 'specific', label: props.filterItemType === 'Goal' ? 'Mục tiêu riêng (Đơn vị cụ thể)' : 'Nhiệm vụ riêng (Đơn vị cụ thể)' }
  ]);

  const statusOptions = ref([
    { value: 'NotStarted', label: 'Chưa thực hiện' },
    { value: 'InProgressOnTime', label: 'Đang thực hiện (trong hạn)' },
    { value: 'InProgressOverdue', label: 'Đang thực hiện (quá hạn)' },
    { value: 'CompletedOnTime', label: 'Hoàn thành (đúng hạn)' },
    { value: 'CompletedOverdue', label: 'Hoàn thành (quá hạn)' },
    { value: 'ExpiringSoon', label: 'Sắp hết hạn' }
  ]);

  const filteredList = computed(() => {
    let list = rawItemsList.value;

    // Filter for Non-Admin Focal Point Accounts:
    // - Sub-Agency (ParentId != null): ONLY show items assigned to this Sub-Agency (cannot view parent agency data)
    // - Parent Agency (ParentId == null): Show items assigned to Parent Agency AND all of its Sub-Agencies (views all)
    if (!authState.isAdmin.value && authState.user.value?.agencyId) {
      const userAgencyId = String(authState.user.value.agencyId).toLowerCase();
      const userAgency = agencies.value.find(a => String(a.id).toLowerCase() === userAgencyId);
      const isParentAgency = !userAgency || !userAgency.parentId;

      const scopedAgencyIds = [userAgencyId];
      if (userAgency && !userAgency.parentId) {
        const childIds = agencies.value
          .filter(a => a.parentId && String(a.parentId).toLowerCase() === userAgencyId)
          .map(a => String(a.id).toLowerCase());
        scopedAgencyIds.push(...childIds);
      }

      list = list.filter(i => {
        const itemLeadId = i.leadAgencyId ? String(i.leadAgencyId).toLowerCase() : '';
        const itemAssignedId = i.assignedAgencyId ? String(i.assignedAgencyId).toLowerCase() : '';
        const itemCoordIds = (i.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());

        const isGeneral = isParentAgency && isGeneralTaskOrAllAgencies(i);
        const isLead = scopedAgencyIds.includes(itemLeadId);
        const isAssigned = itemAssignedId && scopedAgencyIds.includes(itemAssignedId);
        const isCoord = itemCoordIds.some(id => scopedAgencyIds.includes(id));
        return isGeneral || isLead || isAssigned || isCoord;
      });
    }

    // Search Query
    const searchQ = (appliedFilters.value.searchQuery || '').trim().toLowerCase();
    if (searchQ) {
      list = list.filter(i => {
        const matchCode = i.code?.toLowerCase().includes(searchQ);
        const matchTitle = i.title?.toLowerCase().includes(searchQ);
        const matchLeadAgency = i.leadAgencyName?.toLowerCase().includes(searchQ);
        const matchCoopAgencies = i.coordinatingAgencyNames?.some(c => c.toLowerCase().includes(searchQ)) || (typeof i.coordinatingAgencies === 'string' && i.coordinatingAgencies.toLowerCase().includes(searchQ));
        return matchCode || matchTitle || matchLeadAgency || matchCoopAgencies;
      });
    }

    // Agency Filter (Multi-select)
    if (appliedFilters.value.selectedAgencyIds && appliedFilters.value.selectedAgencyIds.length > 0) {
      list = list.filter(i => appliedFilters.value.selectedAgencyIds.includes(i.leadAgencyId));
    }

    // Subordinate Agency Filter (Multi-select)
    if (appliedFilters.value.selectedSubAgencyIds && appliedFilters.value.selectedSubAgencyIds.length > 0) {
      list = list.filter(i => appliedFilters.value.selectedSubAgencyIds.includes(i.assignedAgencyId));
    }

    // Section Filter (Multi-select)
    if (appliedFilters.value.selectedSections && appliedFilters.value.selectedSections.length > 0) {
      list = list.filter(i => appliedFilters.value.selectedSections.includes(i.section));
    }

    // Group Filter (Multi-select)
    if (appliedFilters.value.selectedGroups && appliedFilters.value.selectedGroups.length > 0) {
      list = list.filter(i => appliedFilters.value.selectedGroups.includes(i.group));
    }

    // Ongoing Tasks Filter (Thường xuyên)
    if (appliedFilters.value.onlyOngoing) {
      list = list.filter(i => i.isOngoing);
    }

    // Year Range Filter (From Year -> To Year)
    if (appliedFilters.value.fromYear || appliedFilters.value.toYear) {
      const fYr = appliedFilters.value.fromYear ? Number(appliedFilters.value.fromYear) : 2026;
      const tYr = appliedFilters.value.toYear ? Number(appliedFilters.value.toYear) : 2030;
      list = list.filter(i => {
        if (i.isOngoing) return true;
        const startY = i.startDate ? new Date(i.startDate).getFullYear() : 2026;
        const dueY = i.dueDate ? new Date(i.dueDate).getFullYear() : startY;
        return (startY <= tYr && dueY >= fYr);
      });
    }

    // Scope Filter (Multi-select)
    if (appliedFilters.value.selectedScopes && appliedFilters.value.selectedScopes.length > 0) {
      list = list.filter(i => {
        if (appliedFilters.value.selectedScopes.includes('general') && i.isGeneralTask) return true;
        if (appliedFilters.value.selectedScopes.includes('specific') && !i.isGeneralTask) return true;
        return false;
      });
    }

    // Status Filter (Multi-select)
    if (appliedFilters.value.selectedStatuses && appliedFilters.value.selectedStatuses.length > 0) {
      list = list.filter(i => appliedFilters.value.selectedStatuses.includes(i.calculatedStatus));
    }

    // Sort by Most Recently Updated / Newly Created First
    return list.slice().sort((a, b) => {
      const getItemTime = (item) => {
        let t1 = item.lastUpdated ? new Date(item.lastUpdated).getTime() : 0;
        let t2 = item.createdAt ? new Date(item.createdAt).getTime() : 0;
        let maxT = Math.max(t1, t2);
        return maxT;
      };

      const timeA = getItemTime(a);
      const timeB = getItemTime(b);

      if (timeA !== timeB) {
        return timeB - timeA; // Descending (latest first)
      }

      return (a.code || '').localeCompare(b.code || '', undefined, { numeric: true, sensitivity: 'base' });
    });
  });

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
