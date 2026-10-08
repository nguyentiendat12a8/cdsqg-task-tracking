import { fetchWithAuth } from '../../services/auth';
import { ref } from 'vue';
import { toast } from 'vue3-toastify';
import { authState } from '../../services/auth';
import { getApiUrl } from '../../config/api';

export function usePendingApprovals({ loadData }) {
  const globalPendingLogs = ref([]);

  const isPendingApprovalsModalOpen = ref(false);

  const isLoadingPendingLogs = ref(false);

  function isGoalItem(log) {
    if (!log) return false;
    if (log.itemType === 'Goal') return true;
    if (log.taskCode && String(log.taskCode).toUpperCase().startsWith('MT')) return true;
    if (log.taskTitle && String(log.taskTitle).toLowerCase().startsWith('mục tiêu')) return true;
    return false;
  }

  const isRejectModalOpen = ref(false);

  const rejectLogId = ref(null);

  const rejectionReason = ref('');

  async function loadGlobalPendingLogs(isSilent = false) {
    if (!authState.isAdmin.value) return;
    const showSpinner = !isSilent && (!globalPendingLogs.value || globalPendingLogs.value.length === 0);
    if (showSpinner) {
      isLoadingPendingLogs.value = true;
    }
    try {
      const res = await fetchWithAuth(getApiUrl('/api/execution/pending-approvals'));
      if (res.ok) {
        globalPendingLogs.value = await res.json();
      } else {
        globalPendingLogs.value = [];
      }
    } catch {
      globalPendingLogs.value = [];
    } finally {
      if (showSpinner) {
        isLoadingPendingLogs.value = false;
      }
    }
  }

  function openPendingApprovalsModal() {
    isPendingApprovalsModalOpen.value = true;
    loadGlobalPendingLogs(false);
  }

  async function handleApproveFromGlobalList(logId) {
    if (!logId) return;
    try {
      const res = await fetchWithAuth(getApiUrl(`/api/execution/approve/${logId}`), { method: 'POST' });
      if (res.ok) {
        toast.success('Đã phê duyệt báo cáo tiến độ thành công!');
        try {
          await loadGlobalPendingLogs(true);
          if (typeof loadData === 'function') await loadData();
        } catch (e) {
          console.error('Error refreshing after approve:', e);
        }
      } else {
        toast.error('Phê duyệt thất bại.');
      }
    } catch (err) {
      console.error('Approve error:', err);
      toast.error('Lỗi khi phê duyệt báo cáo.');
    }
  }

  function openRejectModalFromGlobalList(logId) {
    if (!logId) return;
    rejectLogId.value = logId;
    rejectionReason.value = 'Chưa đạt yêu cầu';
    isRejectModalOpen.value = true;
  }

  async function confirmRejectFromGlobalList() {
    if (!rejectLogId.value) return;
    try {
      const res = await fetchWithAuth(getApiUrl(`/api/execution/reject/${rejectLogId.value}`), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reason: rejectionReason.value || 'Chưa đạt yêu cầu' })
      });
      if (res.ok) {
        toast.info('Đã từ chối báo cáo tiến độ.');
        isRejectModalOpen.value = false;
        rejectLogId.value = null;
        try {
          await loadGlobalPendingLogs(true);
          if (typeof loadData === 'function') await loadData();
        } catch (e) {
          console.error('Error refreshing after reject:', e);
        }
      } else {
        toast.error('Từ chối thất bại.');
      }
    } catch (err) {
      console.error('Reject error:', err);
      toast.error('Lỗi khi từ chối báo cáo.');
    }
  }

  return {
    globalPendingLogs,
    isPendingApprovalsModalOpen,
    isLoadingPendingLogs,
    isGoalItem,
    isRejectModalOpen,
    rejectLogId,
    rejectionReason,
    loadGlobalPendingLogs,
    openPendingApprovalsModal,
    handleApproveFromGlobalList,
    openRejectModalFromGlobalList,
    confirmRejectFromGlobalList
  };
}
