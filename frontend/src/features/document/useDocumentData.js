import { REPORTING_DOCUMENT_ID } from '../../config/reporting';
import { fetchWithAuth } from '../../services/auth';
import { ref, watch, onUnmounted } from 'vue';
import { authState } from '../../services/auth';
import { getApiUrl } from '../../config/api';

export function useDocumentData({ props, getQueryState }) {
  const isLoading = ref(false);
  const agencies = ref([]);
  const rawItemsList = ref([]);
  const serverTotalCount = ref(0);
  const serverTotalPages = ref(1);
  const loadError = ref('');
  let requestVersion = 0;
  let agenciesLoaded = false;
  onUnmounted(() => { requestVersion++; });
  function buildQuery() {
    const { appliedFilters, currentPage, pageSize, sortBy, sortOrder, userRoleStr } = getQueryState();
    const currentAgencyId = authState.user.value?.agencyId || '';
    const userRole = userRoleStr.value;
      const params = new URLSearchParams();
      params.append('itemType', props.filterItemType || 'Task');
      params.append('pageNumber', String(currentPage.value));
      params.append('pageSize', String(pageSize.value));
      params.append('sortBy', sortBy.value);
      params.append('sortOrder', sortOrder.value);

      if (currentAgencyId) params.append('agencyId', currentAgencyId);
      if (userRole) params.append('userRole', userRole);

      if (appliedFilters.value.searchQuery) {
        params.append('search', appliedFilters.value.searchQuery.trim());
      }
      if (appliedFilters.value.selectedAgencyIds?.length) {
        params.append('selectedAgencyIds', appliedFilters.value.selectedAgencyIds.join(','));
      }
      if (appliedFilters.value.selectedSubAgencyIds?.length) {
        params.append('selectedSubAgencyIds', appliedFilters.value.selectedSubAgencyIds.join(','));
      }
      if (appliedFilters.value.selectedScopes?.length) {
        params.append('selectedScopes', appliedFilters.value.selectedScopes.join(','));
      }
      if (appliedFilters.value.selectedStatuses?.length) {
        params.append('selectedStatuses', appliedFilters.value.selectedStatuses.join(','));
      }
      if (appliedFilters.value.selectedSections?.length) {
        params.append('selectedSections', appliedFilters.value.selectedSections.join(','));
      }
      if (appliedFilters.value.selectedGroups?.length) {
        params.append('selectedGroups', appliedFilters.value.selectedGroups.join(','));
      }
      if (appliedFilters.value.fromYear) {
        params.append('fromYear', String(appliedFilters.value.fromYear));
      }
      if (appliedFilters.value.toYear) {
        params.append('toYear', String(appliedFilters.value.toYear));
      }
      if (appliedFilters.value.onlyOngoing) {
        params.append('onlyOngoing', 'true');
      }

    return params;
  }
  async function loadExportItems() {
    const params = buildQuery();
    params.set('pageSize', '100');
    const items = [];
    for (let page = 1; ; page++) {
      params.set('pageNumber', String(page));
      const response = await fetchWithAuth(getApiUrl('/api/documents/' + REPORTING_DOCUMENT_ID + '/items?' + params.toString()));
      if (!response.ok) throw new Error('Không tải được dữ liệu xuất Excel.');
      const data = await response.json();
      if (!Number.isInteger(data.totalCount) || data.totalCount < 0 || !Array.isArray(data.items)) throw new Error('Dữ liệu phân trang xuất Excel không hợp lệ.');
      items.push(...(data.items || []));
      if (items.length >= data.totalCount) break;
      if (!data.items.length) throw new Error('Danh sách đã thay đổi khi xuất Excel. Vui lòng thử lại.');
    }
    return items;
  }
  async function loadData() {
    const version = ++requestVersion;
    isLoading.value = true;
    loadError.value = '';
    try {
      const docId = REPORTING_DOCUMENT_ID;

      const params = buildQuery();

      const [itemsRes, agRes] = await Promise.all([
        fetchWithAuth(getApiUrl(`/api/documents/${docId}/items?${params.toString()}`)),
        agenciesLoaded ? Promise.resolve(null) : fetchWithAuth(getApiUrl('/api/agencies'))
      ]);

      if (!itemsRes.ok || (agRes && !agRes.ok)) throw new Error('Cannot load document data');
      const [data, agData] = await Promise.all([itemsRes.json(), agRes?.json()]);
      if (version !== requestVersion) return;
      if (agData) {
        agencies.value = Array.isArray(agData) ? agData : (agData.items || []);
        agenciesLoaded = true;
      }
      rawItemsList.value = data.items || [];
      serverTotalCount.value = typeof data.totalCount === 'number' ? data.totalCount : rawItemsList.value.length;
      serverTotalPages.value = typeof data.totalPages === 'number' ? data.totalPages : (Math.ceil(serverTotalCount.value / pageSize.value) || 1);
    } catch (e) {
      if (version === requestVersion) loadError.value = 'Không tải được danh sách. Vui lòng thử lại.';
    } finally {
      if (version === requestVersion) isLoading.value = false;
    }
  }

  watch(() => props.filterItemType, () => {
    const { currentPage } = getQueryState();
    currentPage.value = 1;
    loadData();
  });

  return { isLoading, loadError, agencies, rawItemsList, serverTotalCount, serverTotalPages, loadData, loadExportItems };
}

