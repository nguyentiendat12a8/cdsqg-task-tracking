import { fetchWithAuth } from '../../services/auth';
import { ref, computed } from 'vue';
import { toast } from 'vue3-toastify';
import { getApiUrl } from '../../config/api';
import { authState } from '../../services/auth';

export function useLegalDashboard({ agencies, isSpecialAgencyCode }) {
  const legalStats = ref(null);
  const isLegalStatsLoading = ref(false);

  const legalFilterDocumentType = ref(null);
  const legalFilterIssuedFromDate = ref(null);
  const legalFilterIssuedToDate = ref(null);
  const legalFilterEffectiveFromDate = ref(null);
  const legalFilterEffectiveToDate = ref(null);
  const legalFilterIssuingAgencyId = ref(null);
  const legalFilterDraftingAgencyId = ref(null);

  const legalMinistriesStats = computed(() => {
    let list = [];
    if (legalStats.value?.ministriesStats && legalStats.value.ministriesStats.length > 0) {
      list = legalStats.value.ministriesStats;
    } else if (legalStats.value?.agencyStats) {
      list = legalStats.value.agencyStats.filter(a => a.agencyType !== 'Province');
    }
    return list.filter(a => a.agencyName !== 'Các bộ, ngành, địa phương' && !a.agencyName?.includes('Các bộ, ngành, địa phương'));
  });

  const legalProvincesStats = computed(() => {
    let list = [];
    if (legalStats.value?.provincesStats && legalStats.value.provincesStats.length > 0) {
      list = legalStats.value.provincesStats;
    } else if (legalStats.value?.agencyStats) {
      list = legalStats.value.agencyStats.filter(a => a.agencyType === 'Province');
    }
    return list.filter(a => a.agencyName !== 'Các bộ, ngành, địa phương' && !a.agencyName?.includes('Các bộ, ngành, địa phương'));
  });

  const isLegalViewerOpen = ref(false);
  const selectedLegalViewerFile = ref(null);

  const legalDocumentTypeOptions = ref([
    { value: 'Thông tư', label: 'Thông tư' },
    { value: 'Nghị định', label: 'Nghị định' },
    { value: 'Quyết định', label: 'Quyết định' },
    { value: 'Nghị quyết', label: 'Nghị quyết' },
    { value: 'Chỉ thị', label: 'Chỉ thị' },
    { value: 'Quy chế', label: 'Quy chế' },
    { value: 'Luật', label: 'Luật' },
    { value: 'Khác', label: 'Khác' }
  ]);

  const legalAgencyOptions = computed(() => {
    return (agencies.value || [])
      .filter(ag => !isSpecialAgencyCode(ag.code))
      .map(ag => ({
        value: ag.id,
        label: ag.name
      }));
  });

  const activeLegalFilterCount = computed(() => {
    let cnt = 0;
    if (legalFilterDocumentType.value) cnt++;
    if (legalFilterIssuedFromDate.value || legalFilterIssuedToDate.value) cnt++;
    if (legalFilterEffectiveFromDate.value || legalFilterEffectiveToDate.value) cnt++;
    if (legalFilterIssuingAgencyId.value) cnt++;
    if (legalFilterDraftingAgencyId.value) cnt++;
    return cnt;
  });

  function resetLegalFilters() {
    legalFilterDocumentType.value = null;
    legalFilterIssuedFromDate.value = null;
    legalFilterIssuedToDate.value = null;
    legalFilterEffectiveFromDate.value = null;
    legalFilterEffectiveToDate.value = null;
    legalFilterIssuingAgencyId.value = null;
    legalFilterDraftingAgencyId.value = null;
    loadLegalDashboardStats();
  }

  async function loadLegalDashboardStats() {
    isLegalStatsLoading.value = true;
    try {
      const params = new URLSearchParams();
      if (legalFilterDocumentType.value) params.append('documentType', legalFilterDocumentType.value);
      if (legalFilterIssuedFromDate.value) params.append('issuedFromDate', legalFilterIssuedFromDate.value);
      if (legalFilterIssuedToDate.value) params.append('issuedToDate', legalFilterIssuedToDate.value);
      if (legalFilterEffectiveFromDate.value) params.append('effectiveFromDate', legalFilterEffectiveFromDate.value);
      if (legalFilterEffectiveToDate.value) params.append('effectiveToDate', legalFilterEffectiveToDate.value);
      if (legalFilterIssuingAgencyId.value) params.append('issuingAgencyId', legalFilterIssuingAgencyId.value);
      if (legalFilterDraftingAgencyId.value) params.append('draftingAgencyId', legalFilterDraftingAgencyId.value);

      // Pass role and agencyId for 3-tier data access scoping
      const userRole = authState.user.value?.role || (authState.isAdmin.value ? 'Admin' : 'Level2');
      if (userRole) params.append('userRole', userRole);
      if (authState.user.value?.agencyId) params.append('userAgencyId', authState.user.value.agencyId);

      const queryString = params.toString();
      const url = getApiUrl(`/api/legaldocuments/dashboard-stats${queryString ? '?' + queryString : ''}`);
      const res = await fetchWithAuth(url);
      if (res.ok) {
        legalStats.value = await res.json();
      }
    } catch (err) {
      console.error("Lỗi khi tải thống kê VB QPPL:", err);
    } finally {
      isLegalStatsLoading.value = false;
    }
  }

  function getLegalAgencyBarPercent(count) {
    if (!legalStats.value || !legalStats.value.agencyStats || legalStats.value.agencyStats.length === 0) return 0;
    const maxCount = Math.max(...legalStats.value.agencyStats.map(a => a.totalCount || 0), 1);
    return Math.round((count / maxCount) * 100);
  }

  function openLegalViewerForDoc(doc) {
    if (doc && doc.attachments && doc.attachments.length > 0) {
      const file = doc.attachments[0];
      selectedLegalViewerFile.value = {
        fileName: file.fileName,
        cleanName: `${doc.documentNumber} - ${doc.title}`,
        fileType: doc.documentType,
        fileUrl: file.fileUrl
      };
      isLegalViewerOpen.value = true;
    } else {
      toast.info("Văn bản này không có tệp đính kèm nào.");
    }
  }

  function getEffectStatusBadgeClass(status) {
    switch (status) {
      case 'Còn hiệu lực': return 'bg-emerald-50 text-emerald-700 border-emerald-200';
      case 'Hết hiệu lực': return 'bg-rose-50 text-rose-700 border-rose-200';
      case 'Chưa có hiệu lực': return 'bg-amber-50 text-amber-700 border-amber-200';
      default: return 'bg-slate-100 text-slate-700 border-slate-200';
    }
  }

  return {
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
};
}
