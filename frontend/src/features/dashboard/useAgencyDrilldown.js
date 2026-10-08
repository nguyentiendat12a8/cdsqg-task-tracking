import { fetchWithAuth } from '../../services/auth';
import { ref, computed } from 'vue';
import { getApiUrl } from '../../config/api';

export function useAgencyDrilldown({ dashboardFilter, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, metrics }) {
  const selectedDrilldownAgency = ref(null);
  const subAgenciesList = ref([]);
  const isSubAgenciesLoading = ref(false);
  const drilldownTab = ref('goals'); // 'goals' | 'tasks' | 'sub-agencies' | 'contacts'

  const selectedDetailItem = ref(null);
  const agencyItemsList = ref([]);
  const isAgencyItemsLoading = ref(false);
  const modalSearchKeyword = ref('');
  const modalItemTypeFilter = ref('all');
  const modalStatusFilter = ref('all');

  const modalGoalsList = computed(() => (agencyItemsList.value || []).filter(i => i.itemType === 'Goal'));
  const modalTasksList = computed(() => (agencyItemsList.value || []).filter(i => i.itemType === 'Task'));

  const filteredAgencyItems = computed(() => {
    let list = agencyItemsList.value || [];

    if (drilldownTab.value === 'goals') {
      list = list.filter(i => i.itemType === 'Goal');
    } else if (drilldownTab.value === 'tasks') {
      list = list.filter(i => i.itemType === 'Task');
    } else if (modalItemTypeFilter.value && modalItemTypeFilter.value !== 'all') {
      list = list.filter(i => i.itemType === modalItemTypeFilter.value);
    }

    if (modalStatusFilter.value && modalStatusFilter.value !== 'all') {
      list = list.filter(i => i.status === modalStatusFilter.value);
    }

    if (modalSearchKeyword.value && modalSearchKeyword.value.trim() !== '') {
      const q = modalSearchKeyword.value.trim().toLowerCase();
      list = list.filter(i =>
        (i.code && i.code.toLowerCase().includes(q)) ||
        (i.title && i.title.toLowerCase().includes(q)) ||
        (i.category && i.category.toLowerCase().includes(q)) ||
        (i.section && i.section.toLowerCase().includes(q)) ||
        (i.group && i.group.toLowerCase().includes(q)) ||
        (i.leadAgencyName && i.leadAgencyName.toLowerCase().includes(q))
      );
    }

    return list;
  });

  const allDrilldownContacts = computed(() => {
    if (!selectedDrilldownAgency.value) return [];
    const list = [];

    // Contacts from root parent agency
    if (selectedDrilldownAgency.value.contactPersons?.length) {
      selectedDrilldownAgency.value.contactPersons.forEach(c => {
        list.push({
          ...c,
          unitName: selectedDrilldownAgency.value.name,
          unitCode: selectedDrilldownAgency.value.code,
          isParent: true
        });
      });
    }

    // Contacts from child agencies
    if (subAgenciesList.value?.length) {
      subAgenciesList.value.forEach(sub => {
        if (sub.contactPersons?.length) {
          sub.contactPersons.forEach(c => {
            list.push({
              ...c,
              unitName: sub.name,
              unitCode: sub.code,
              isParent: false
            });
          });
        }
      });
    }

    return list;
  });

  async function loadAgencyItems(agencyId) {
    isAgencyItemsLoading.value = true;
    try {
      const params = new URLSearchParams();
      params.append('agencyId', agencyId);
      if (selectedScopes.value && selectedScopes.value.length === 1) {
        params.append('scope', selectedScopes.value[0]);
      }
      if (selectedSections.value && selectedSections.value.length > 0) {
        selectedSections.value.forEach(sec => params.append('section', sec));
      }
      if (selectedGroups.value && selectedGroups.value.length > 0) {
        selectedGroups.value.forEach(grp => params.append('group', grp));
      }
      if (fromYear.value) params.append('fromYear', fromYear.value);
      if (toYear.value) params.append('toYear', toYear.value);
      if (isOngoingOnly.value) params.append('isOngoing', 'true');

      const res = await fetchWithAuth(getApiUrl(`/api/dashboard/agency-items?${params.toString()}`));
      if (res.ok) {
        agencyItemsList.value = await res.json();
      }
    } catch (e) {
      console.error('Lỗi khi tải danh sách mục tiêu/nhiệm vụ của đơn vị:', e);
    } finally {
      isAgencyItemsLoading.value = false;
    }
  }

  async function loadSubAgencies(parentAgencyId) {
    isSubAgenciesLoading.value = true;
    try {
      const params = new URLSearchParams();
      params.append('parentAgencyId', parentAgencyId);
      if (dashboardFilter.value && dashboardFilter.value !== 'all') {
        const itemType = dashboardFilter.value === 'goals' ? 'Goal' : 'Task';
        params.append('itemType', itemType);
      }
      if (selectedScopes.value && selectedScopes.value.length === 1) {
        params.append('scope', selectedScopes.value[0]);
      }
      if (selectedSections.value && selectedSections.value.length > 0) {
        selectedSections.value.forEach(sec => params.append('section', sec));
      }
      if (selectedGroups.value && selectedGroups.value.length > 0) {
        selectedGroups.value.forEach(grp => params.append('group', grp));
      }
      if (fromYear.value) params.append('fromYear', fromYear.value);
      if (toYear.value) params.append('toYear', toYear.value);
      if (isOngoingOnly.value) params.append('isOngoing', 'true');

      const res = await fetchWithAuth(getApiUrl(`/api/dashboard/metrics?${params.toString()}`));
      if (res.ok) {
        const data = await res.json();
        subAgenciesList.value = [...(data.ministriesPerformance || []), ...(data.provincesPerformance || []), ...(data.othersPerformance || [])];
      }
    } catch (e) {
      console.error('Lỗi khi tải đơn vị trực thuộc:', e);
    } finally {
      isSubAgenciesLoading.value = false;
    }
  }

  async function switchDrilldownTab(tab) {
    drilldownTab.value = tab;
    if (!selectedDrilldownAgency.value) return;

    if (tab === 'goals' || tab === 'tasks') {
      if (agencyItemsList.value.length === 0 && !isAgencyItemsLoading.value) {
        await loadAgencyItems(selectedDrilldownAgency.value.agencyId);
      }
    } else if (tab === 'sub-agencies' || tab === 'contacts') {
      if (subAgenciesList.value.length === 0 && !isSubAgenciesLoading.value) {
        await loadSubAgencies(selectedDrilldownAgency.value.agencyId);
      }
    }
  }

  async function drilldownAgency(agency) {
    selectedDrilldownAgency.value = agency;
    subAgenciesList.value = [];
    agencyItemsList.value = [];
    modalSearchKeyword.value = '';
    modalItemTypeFilter.value = 'all';
    modalStatusFilter.value = 'all';

    if (dashboardFilter.value === 'tasks') {
      drilldownTab.value = 'tasks';
    } else {
      drilldownTab.value = 'goals';
    }

    // Set loading states immediately so header tab badges display (...) loading indicator instead of (0)
    isAgencyItemsLoading.value = true;
    isSubAgenciesLoading.value = true;

    // Immediately load both assigned items list and sub-agencies metrics concurrently
    // so all tab totals (Goals, Tasks, Sub-agencies, Contacts) are accurate right on popup open
    await Promise.all([
      loadAgencyItems(agency.agencyId),
      loadSubAgencies(agency.agencyId)
    ]);

    if (drilldownTab.value === 'goals' && modalGoalsList.value.length === 0 && modalTasksList.value.length > 0) {
      drilldownTab.value = 'tasks';
    }
  }

  return {
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
};
}
