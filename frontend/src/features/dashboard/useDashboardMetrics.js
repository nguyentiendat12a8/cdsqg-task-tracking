import { fetchWithAuth } from '../../services/auth';
import { ref, computed, onUnmounted } from 'vue';
import { buildDashboardQuery } from './queries';
import { getApiUrl } from '../../config/api';
import { authState } from '../../services/auth';

export function useDashboardMetrics({ dashboardFilter, selectedAgencyIds, selectedSubAgencyIds, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, isSpecialAgencyCode }) {
  const isLoading = ref(false);
  const metrics = ref({
    totalGoals: 0,
    completedGoals: 0,
    totalTasks: 0,
    completedTasks: 0,
    createdMetrics: null,
    statusSummary: {},
    goalStatusSummary: {},
    taskStatusSummary: {},
    ministriesPerformance: [],
    provincesPerformance: [],
    othersPerformance: []
  });

  const activeStatusSummary = computed(() => {
    if (dashboardFilter.value === 'goals') return metrics.value.goalStatusSummary || {};
    if (dashboardFilter.value === 'tasks') return metrics.value.taskStatusSummary || {};
    return metrics.value.statusSummary || {};
  });

  const activeStatusTotal = computed(() => {
    const s = activeStatusSummary.value || {};
    return (s.notStarted || 0) +
           (s.inProgressOnTime || 0) +
           (s.inProgressOverdue || 0) +
           (s.completedOnTime || 0) +
           (s.completedOverdue || 0) +
           (s.expiringSoon || 0);
  });

  const overallDonutStats = computed(() => {
    return {
      ...activeStatusSummary.value,
      totalItems: activeStatusTotal.value
    };
  });

  const createdMetrics = computed(() => metrics.value.createdMetrics || {});

  const activeCreatedStatusSummary = computed(() => {
    const cm = createdMetrics.value;
    if (!cm) return {};
    if (dashboardFilter.value === 'goals') return cm.goalStatusSummary || {};
    if (dashboardFilter.value === 'tasks') return cm.taskStatusSummary || {};
    return cm.statusSummary || {};
  });

  const activeCreatedTotals = computed(() => {
    const cm = createdMetrics.value;
    if (!cm) return { total: 0, specific: 0, general: 0 };
    if (dashboardFilter.value === 'goals') {
      return {
        total: cm.totalGoals || 0,
        specific: cm.goalsSpecific || 0,
        general: cm.goalsGeneral || 0
      };
    } else if (dashboardFilter.value === 'tasks') {
      return {
        total: cm.totalTasks || 0,
        specific: cm.tasksSpecific || 0,
        general: cm.tasksGeneral || 0
      };
    }
    return {
      total: (cm.totalGoals || 0) + (cm.totalTasks || 0),
      specific: (cm.goalsSpecific || 0) + (cm.tasksSpecific || 0),
      general: (cm.goalsGeneral || 0) + (cm.tasksGeneral || 0)
    };
  });

  const createdDonutStats = computed(() => {
    return {
      ...activeCreatedStatusSummary.value,
      totalItems: activeCreatedTotals.value.specific
    };
  });

  const userSubAgenciesPerformance = ref([]);
  const isMinistriesExpanded = ref(false);
  const isProvincesExpanded = ref(false);
  const isOthersExpanded = ref(false);
  const isSubAgenciesExpanded = ref(false);

  const isSection1Collapsed = ref(true);
  const isSection2Collapsed = ref(true);
  const isSection3Collapsed = ref(true);
  const isSection4Collapsed = ref(true);

  function filterOutSpecialAgencies(list) {
    return (list || []).filter(item => {
      if (!item) return false;
      if (isSpecialAgencyCode(item.code)) return false;
      if (item.type === 5 || item.type === '5' || item.type === 'Special') return false;
      const code = (item.code || '').toUpperCase();
      if (['ALL_AGENCIES', 'ALL_MINISTRIES', 'ALL_PROVINCES', 'ALL_PROVINCES_UBND', 'ALL_MINISTRIES_DIRECT'].includes(code)) return false;
      const name = (item.name || '').toLowerCase();
      if (name.includes('ubnd tỉnh, thành phố trực thuộc trung ương') || name.startsWith('các bộ, ngành') || name.startsWith('các địa phương')) return false;
      return true;
    });
  }

  function getAgencySortCount(item) {
    if (!item) return 0;
    if (dashboardFilter.value === 'goals') return item.totalGoals ?? item.totalItems ?? 0;
    if (dashboardFilter.value === 'tasks') return item.totalTasks ?? item.totalItems ?? 0;
    return item.totalItems ?? ((item.totalGoals || 0) + (item.totalTasks || 0));
  }

  function sortAgenciesByCount(list) {
    const filtered = filterOutSpecialAgencies(list);
    return [...filtered].sort((a, b) => {
      const countA = getAgencySortCount(a);
      const countB = getAgencySortCount(b);
      if (countB !== countA) {
        return countB - countA;
      }
      return (a.name || '').localeCompare(b.name || '', 'vi');
    });
  }

  const filteredMinistriesPerformance = computed(() => sortAgenciesByCount(metrics.value.ministriesPerformance));
  const filteredProvincesPerformance = computed(() => sortAgenciesByCount(metrics.value.provincesPerformance));
  const filteredOthersPerformance = computed(() => sortAgenciesByCount(metrics.value.othersPerformance));

  const visibleMinistries = computed(() => {
    const list = filteredMinistriesPerformance.value;
    if (isMinistriesExpanded.value || list.length <= 8) return list;
    return list.slice(0, 8);
  });

  const visibleProvinces = computed(() => {
    const list = filteredProvincesPerformance.value;
    if (isProvincesExpanded.value || list.length <= 8) return list;
    return list.slice(0, 8);
  });

  const visibleOthers = computed(() => {
    const list = filteredOthersPerformance.value;
    if (isOthersExpanded.value || list.length <= 8) return list;
    return list.slice(0, 8);
  });

  const visibleSubAgencies = computed(() => {
    const list = userSubAgenciesPerformance.value || [];
    if (isSubAgenciesExpanded.value || list.length <= 8) return list;
    return list.slice(0, 8);
  });

  const mostSubAgenciesPerformance = ref([]);
  const isMostExpanded = ref(false);

  const filteredMostSubAgenciesPerformance = computed(() => sortAgenciesByCount(mostSubAgenciesPerformance.value));
  const visibleMostSubAgencies = computed(() => {
    const list = filteredMostSubAgenciesPerformance.value;
    if (isMostExpanded.value || list.length <= 8) return list;
    return list.slice(0, 8);
  });
  const userAgencyName = computed(() => {
    return authState.user.value?.agencyName || 'Cơ quan/Bộ/Địa phương';
  });

  const loggedUserAgencyId = computed(() => authState.user.value?.agencyId ? String(authState.user.value.agencyId).toLowerCase() : '');
  const loggedUserAgency = computed(() => agencies.value.find(a => String(a.id).toLowerCase() === loggedUserAgencyId.value));
  const isSubAgencyUser = computed(() => !authState.isAdmin.value && !!loggedUserAgencyId.value);
  const isLevel3User = computed(() => {
    if (authState.isAdmin.value) return false;
    if (loggedUserAgency.value) {
      return !!loggedUserAgency.value.parentId;
    }
    const userObjAg = authState.user.value?.agency;
    return !!(userObjAg && userObjAg.parentId);
  });

  const isBKHCNAgency = computed(() => {
    const ag = loggedUserAgency.value || authState.user.value?.agency;
    const name = ((ag?.name || authState.user.value?.agencyName) || '').toLowerCase();
    const code = ((ag?.code || authState.user.value?.agencyCode) || '').toLowerCase();
    return code === 'bkhcn' || name.includes('khoa học và công nghệ') || name.includes('khoa học & công nghệ') || name.includes('khoa học công nghệ');
  });

  function isBKHCNItem(agency) {
    if (!agency) return false;
    const name = String(agency.name || '').toLowerCase();
    const code = String(agency.code || '').toLowerCase();
    return code === 'bkhcn' || name.includes('khoa học và công nghệ') || name.includes('khoa học & công nghệ') || name.includes('khoa học công nghệ');
  }

  const singleSubAgencyPerformance = computed(() => {
    const allPerf = [
      ...(metrics.value.ministriesPerformance || []),
      ...(metrics.value.provincesPerformance || []),
      ...(metrics.value.othersPerformance || [])
    ];
    let found = null;
    if (loggedUserAgencyId.value) {
      found = allPerf.find(p => String(p.agencyId).toLowerCase() === loggedUserAgencyId.value);
    }
    if (!found) {
      found = allPerf[0] || null;
    }
    if (!found) return null;

    const filter = dashboardFilter.value;
    if (filter === 'goals') {
      return {
        ...found,
        totalItems: found.totalGoals ?? found.totalItems ?? 0,
        notStarted: found.goalNotStarted ?? found.notStarted ?? 0,
        inProgressOnTime: found.goalInProgressOnTime ?? found.inProgressOnTime ?? 0,
        inProgressOverdue: found.goalInProgressOverdue ?? found.inProgressOverdue ?? 0,
        completedOnTime: found.goalCompletedOnTime ?? found.completedOnTime ?? 0,
        completedOverdue: found.goalCompletedOverdue ?? found.completedOverdue ?? 0,
        expiringSoon: found.goalExpiringSoon ?? found.expiringSoon ?? 0,
      };
    } else if (filter === 'tasks') {
      return {
        ...found,
        totalItems: found.totalTasks ?? found.totalItems ?? 0,
        notStarted: found.taskNotStarted ?? found.notStarted ?? 0,
        inProgressOnTime: found.taskInProgressOnTime ?? found.inProgressOnTime ?? 0,
        inProgressOverdue: found.taskInProgressOverdue ?? found.inProgressOverdue ?? 0,
        completedOnTime: found.taskCompletedOnTime ?? found.completedOnTime ?? 0,
        completedOverdue: found.taskCompletedOverdue ?? found.completedOverdue ?? 0,
        expiringSoon: found.taskExpiringSoon ?? found.expiringSoon ?? 0,
      };
    }
    return found;
  });

  function getPct(val, total) {
    if (!total || total <= 0) return 0;
    return Math.round(((val || 0) / total) * 100);
  }

  async function loadAgencies() {
    try {
      const res = await fetchWithAuth(getApiUrl('/api/agencies'));
      if (res.ok) {
        const data = await res.json();
        agencies.value = Array.isArray(data) ? data : (data.items || []);
      }
    } catch (e) {}
  }

  const loadError = ref('');
  let requestVersion = 0;
  onUnmounted(() => { requestVersion++; });

  async function loadDashboardMetrics() {
    const version = ++requestVersion;
    isLoading.value = true;
    loadError.value = '';
    const filters = {
      dashboardFilter: dashboardFilter.value,
      selectedScopes: selectedScopes.value,
      selectedSections: selectedSections.value,
      selectedGroups: selectedGroups.value,
      fromYear: fromYear.value,
      toYear: toYear.value,
      isOngoingOnly: isOngoingOnly.value
    };
    const ids = [...selectedAgencyIds.value, ...selectedSubAgencyIds.value];
    const userId = authState.user.value?.agencyId;
    if (!authState.isAdmin.value && userId && ids.length === 0) ids.push(userId);
    async function fetchMetrics(options) {
      const query = buildDashboardQuery(filters, options);
      const response = await fetchWithAuth(getApiUrl('/api/dashboard/metrics?' + query));
      if (!response.ok) throw new Error('Cannot load dashboard metrics');
      return response.json();
    }
    try {
      const mostAgency = agencies.value.find(a => a.name?.toLowerCase().includes('khoa học và công nghệ') || a.code === 'AG-77c4bef6');
      const [main, sub, most] = await Promise.all([
        fetchMetrics({ agencyIds: ids }),
        !authState.isAdmin.value && userId ? fetchMetrics({ parentAgencyId: userId }) : null,
        authState.isAdmin.value ? fetchMetrics({ parentAgencyId: mostAgency?.id || '3e6c7022-b1b8-42ae-bbeb-0acdd30735f4' }) : null
      ]);
      if (version !== requestVersion) return;
      const performance = data => [...(data?.ministriesPerformance || []), ...(data?.provincesPerformance || []), ...(data?.othersPerformance || [])];
      metrics.value = main;
      userSubAgenciesPerformance.value = performance(sub).sort((a, b) => {
        const totalA = a.totalItems ?? ((a.totalGoals || 0) + (a.totalTasks || 0));
        const totalB = b.totalItems ?? ((b.totalGoals || 0) + (b.totalTasks || 0));
        return totalB - totalA || (a.name || '').localeCompare(b.name || '', 'vi');
      });
      mostSubAgenciesPerformance.value = performance(most);
    } catch (error) {
      if (version === requestVersion) loadError.value = 'Không tải được thống kê. Vui lòng thử lại.';
    } finally {
      if (version === requestVersion) isLoading.value = false;
    }
  }

  return {
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
};
}
