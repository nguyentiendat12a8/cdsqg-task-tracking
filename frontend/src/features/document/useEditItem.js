import { fetchWithAuth } from '../../services/auth';
import { ref, computed, watch } from 'vue';
import { toast } from 'vue3-toastify';
import { authState } from '../../services/auth';
import { getApiUrl } from '../../config/api';
import { parseApiError } from '../../utils/errorUtils';

export function useEditItem({ planningGridRef, agencies, loadData, hasProgress }) {
  const isEditModalOpen = ref(false);

  const editErrorMessage = ref('');

  const editingItem = ref(null);

  const editForm = ref({
    title: '',
    section: '',
    group: '',
    isOngoing: false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: '',
    dueDate: '',
    leadAgencyId: '',
    assignedAgencyId: '',
    coordinatingAgencyIds: [],
    deliverables: []
  });

  watch(() => editForm.value.isOngoing, (val) => {
    if (val) {
      editForm.value.startYear = 2026;
      editForm.value.dueYear = 2030;
      editForm.value.startDate = '2026-01-01';
      editForm.value.dueDate = '2030-12-31';
    }
  });

  const editAssignedAgencyOptions = computed(() => {
    if (!editForm.value.leadAgencyId) return [];
    return agencies.value
      .filter(ag => ag.parentId === editForm.value.leadAgencyId)
      .map(ag => ({ value: ag.id, label: ag.name }));
  });

  watch(() => editForm.value.leadAgencyId, (newId) => {
    if (editForm.value.assignedAgencyId) {
      const isChild = agencies.value.some(a => a.id === editForm.value.assignedAgencyId && a.parentId === newId);
      if (!isChild) editForm.value.assignedAgencyId = '';
    }
  });

  function openEditModal(item) {
    if (!item) return;

    if (!authState.isAdmin.value) {
      toast.error("Chỉ có tài khoản Quản trị viên (Admin) mới có quyền chỉnh sửa Mục tiêu / Nhiệm vụ.");
      return;
    }

    if (hasProgress(item)) {
      toast.warning("Chỉ được phép chỉnh sửa Mục tiêu / Nhiệm vụ khi ở trạng thái Chưa bắt đầu.");
      return;
    }

    editErrorMessage.value = '';
    editingItem.value = item;

    let sDate = '';
    let dDate = '';
    let sYear = 2026;
    let dYear = 2030;

    if (item.startDate) {
      const dt = new Date(item.startDate);
      if (!isNaN(dt.getTime())) {
        sDate = dt.toISOString().split('T')[0];
        sYear = dt.getFullYear();
      }
    }
    if (item.dueDate) {
      const dt = new Date(item.dueDate);
      if (!isNaN(dt.getTime())) {
        dDate = dt.toISOString().split('T')[0];
        dYear = dt.getFullYear();
      }
    }

    editForm.value = {
      code: item.code || '',
      title: item.title || '',
      section: item.section || '',
      group: item.group || '',
      unitName: item.unitName || '%',
      isOngoing: !!item.isOngoing,
      isGeneralTask: !!item.isGeneralTask,
      startYear: sYear,
      dueYear: dYear,
      startDate: sDate,
      dueDate: dDate,
      leadAgencyId: item.leadAgencyId || agencies.value[0]?.id || '',
      assignedAgencyId: item.assignedAgencyId || '',
      coordinatingAgencyIds: item.coordinatingAgencyIds ? [...item.coordinatingAgencyIds] : [],
      deliverables: item.deliverables ? JSON.parse(JSON.stringify(item.deliverables)) : []
    };

    isEditModalOpen.value = true;
  }

  function addEditDeliverable() {
    if (!editForm.value.deliverables) {
      editForm.value.deliverables = [];
    }
    const defaultDueDate = editForm.value.dueDate || '2030-12-31';
    editForm.value.deliverables.push({
      title: '',
      dueDate: defaultDueDate,
      currentStatus: 'NotStarted'
    });
  }

  function removeEditDeliverable(idx) {
    if (editForm.value.deliverables) {
      editForm.value.deliverables.splice(idx, 1);
    }
  }

  async function submitEditItem() {
    if (!editingItem.value) return;

    editErrorMessage.value = '';

    if (!editForm.value.title || !editForm.value.title.trim()) {
      editErrorMessage.value = 'Tên mục tiêu / nhiệm vụ không được để trống.';
      toast.error(editErrorMessage.value);
      return;
    }

    const isGoal = editingItem.value?.itemType === 'Goal' || editingItem.value?.itemType === 1 || editingItem.value?.itemType === '1';

    if (isGoal && !editForm.value.section) {
      editErrorMessage.value = 'Vui lòng chọn Mục (Phụ lục I).';
      toast.error(editErrorMessage.value);
      return;
    }

    if (isGoal && !editForm.value.unitName) {
      editErrorMessage.value = 'Vui lòng chọn Đơn vị tính.';
      toast.error(editErrorMessage.value);
      return;
    }

    if (!editForm.value.leadAgencyId) {
      editErrorMessage.value = 'Vui lòng chọn đơn vị chủ trì.';
      toast.error(editErrorMessage.value);
      return;
    }

    if (editingItem.value.itemType !== 'Goal' && editForm.value.deliverables && editForm.value.deliverables.length > 0) {
      for (let idx = 0; idx < editForm.value.deliverables.length; idx++) {
        const del = editForm.value.deliverables[idx];
        if (!del.title || !del.title.trim()) {
          editErrorMessage.value = `Sản phẩm đầu ra #${idx + 1}: Tên sản phẩm / tên văn bản không được để trống.`;
          toast.error(editErrorMessage.value);
          return;
        }
      }
    }

    try {
      let startDateIso = null;
      let dueDateIso = null;

      if (editForm.value.isOngoing) {
        startDateIso = new Date('2026-01-01T00:00:00.000Z').toISOString();
        dueDateIso = new Date('2030-12-31T23:59:59.000Z').toISOString();
      } else if (editForm.value.startDate || editForm.value.dueDate) {
        startDateIso = editForm.value.startDate ? new Date(editForm.value.startDate).toISOString() : null;
        dueDateIso = editForm.value.dueDate ? new Date(editForm.value.dueDate).toISOString() : null;
      } else {
        const sYear = editForm.value.startYear || 2026;
        const dYear = editForm.value.dueYear || 2030;
        startDateIso = new Date(`${sYear}-01-01T00:00:00.000Z`).toISOString();
        dueDateIso = new Date(`${dYear}-12-31T12:00:00.000Z`).toISOString();
      }

      const payload = {
        code: editForm.value.code ? editForm.value.code.trim() : '',
        title: editForm.value.title,
        section: editForm.value.section,
        group: editForm.value.group,
        unitName: editForm.value.unitName || '%',
        isOngoing: editForm.value.isOngoing,
        isGeneralTask: editForm.value.isGeneralTask,
        startDate: startDateIso,
        dueDate: dueDateIso,
        leadAgencyId: editForm.value.leadAgencyId,
        assignedAgencyId: editForm.value.assignedAgencyId || null,
        coordinatingAgencyIds: editForm.value.coordinatingAgencyIds || [],
        deliverables: editForm.value.deliverables || []
      };

      const targetId = editingItem.value.taskId || editingItem.value.id;
      const res = await fetchWithAuth(getApiUrl(`/api/planning/items/${targetId}`), {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (res.ok) {
        toast.success("Cập nhật thông tin thành công!");
        isEditModalOpen.value = false;
        await loadData();
        planningGridRef.value?.loadGridData();
      } else {
        const err = await res.json().catch(() => ({}));
        const msg = parseApiError(err, 'Lỗi khi cập nhật mục tiêu/nhiệm vụ.');
        editErrorMessage.value = msg;
        toast.error(msg);
      }
    } catch (e) {
      editErrorMessage.value = 'Không thể kết nối máy chủ.';
      toast.error('Không thể kết nối máy chủ.');
    }
  }

  return {
    isEditModalOpen,
    editErrorMessage,
    editingItem,
    editForm,
    editAssignedAgencyOptions,
    openEditModal,
    addEditDeliverable,
    removeEditDeliverable,
    submitEditItem
  };
}
