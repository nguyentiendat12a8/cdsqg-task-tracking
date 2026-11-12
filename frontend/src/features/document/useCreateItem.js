import { initializeAgencyDisplayNames, useAgencyDisplayNames } from './useAgencyDisplayNames';
import { fetchWithAuth } from '../../services/auth';
import { ref, computed, watch } from 'vue';
import { toast } from 'vue3-toastify';
import { getApiUrl } from '../../config/api';
import { activeDocumentId } from '../../services/documentContext';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../../config/planningStructureConfig';
import { parseApiError } from '../../utils/errorUtils';
import { subAgencyOptionsFor } from '../../shared/subAgencies';

export function useCreateItem({ planningGridRef, agencies, rawItemsList, loadData, isSpecialAgencyCode }) {
  const isCreateModalOpen = ref(false);

  const createItemType = ref('Task');


  const createForm = ref({
    code: '',
    title: '',
    resultTarget: null,
    section: '',
    group: '',
    isOngoing: false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: '',
    dueDate: '',
    leadAgencyId: '',
    leadAgencyDisplayText: '',
    assignedAgencyId: '',
    subAgencyIds: [],
    coordinatingAgencyIds: [],
    coordinatingAgenciesDisplayText: '',
    deliverables: []
  });

  const createErrorMessage = ref('');

  useAgencyDisplayNames(createForm, agencies);

  watch(() => createForm.value.isOngoing, (val) => {
    if (val) {
      createForm.value.startYear = 2026;
      createForm.value.dueYear = 2030;
      createForm.value.startDate = '2026-01-01';
      createForm.value.dueDate = '2030-12-31';
    }
  });

  function addDeliverable() {
    if (!createForm.value.deliverables) {
      createForm.value.deliverables = [];
    }
    const defaultDueDate = createForm.value.dueDate || '2030-12-31';
    createForm.value.deliverables.push({
      title: '',
    resultTarget: null,
      dueDate: defaultDueDate,
      notes: ''
    });
  }

  function removeDeliverable(idx) {
    if (createForm.value.deliverables) {
      createForm.value.deliverables.splice(idx, 1);
    }
  }

  const currentFormSections = computed(() => {
    return createItemType.value === 'Goal' ? GOAL_SECTIONS : TASK_SECTIONS;
  });

  const currentFormGroups = computed(() => {
    if (createItemType.value === 'Task') {
      return TASK_GROUPS;
    }
    const groups = GOAL_GROUPS;
    if (!createForm.value.section) return groups;
    return groups.filter(g => g.section === createForm.value.section);
  });

  // Đơn vị trực thuộc (chọn nhiều); assignedAgencyId là cơ quan đồng chủ trì, giữ nguyên khi sửa.
  const createAssignedAgencyOptions = computed(() => subAgencyOptionsFor(agencies.value, {
    leadAgencyId: createForm.value.leadAgencyId,
    assignedAgencyId: createForm.value.assignedAgencyId,
    isGeneralTask: createForm.value.isGeneralTask
  }));

  watch(() => createForm.value.leadAgencyId, (newId) => {
    const selected = agencies.value.find(a => a.id === newId);
    if (selected && isSpecialAgencyCode(selected.code)) {
      createForm.value.isGeneralTask = true;
    } else {
      createForm.value.isGeneralTask = false;
    }
    const allowed = new Set(createAssignedAgencyOptions.value.map(o => String(o.value)));
    if (createForm.value.subAgencyIds?.length) {
      createForm.value.subAgencyIds = createForm.value.subAgencyIds.filter(id => allowed.has(String(id)));
    }
  }, { immediate: true });

  function openCreateModal(type) {
    createItemType.value = type;
    // Để trống: hệ thống tự cấp mã theo cơ quan chủ trì (NV-BCA-01; giao chung/đồng chủ trì: NV-C-01).
    createForm.value = {
      code: '',
      title: '',
    resultTarget: null,
      section: type === 'Goal' ? 'Mục A' : '',
      group: '',
      unitName: '%',
      resultTarget: null,
      isOngoing: false,
      isGeneralTask: false,
      startYear: 2026,
      dueYear: 2030,
      startDate: '',
      dueDate: '',
      leadAgencyId: agencies.value[0]?.id || '',
      leadAgencyDisplayText: '',
      assignedAgencyId: '',
      subAgencyIds: [],
      coordinatingAgencyIds: [],
      coordinatingAgenciesDisplayText: '',
      deliverables: []
    };
    initializeAgencyDisplayNames(createForm.value, agencies.value);
    isCreateModalOpen.value = true;
  }

  async function submitCreateItem() {
    createErrorMessage.value = '';

    if (!createForm.value.title || !createForm.value.title.trim()) {
      createErrorMessage.value = 'Tên mục tiêu / nhiệm vụ không được để trống.';
      toast.error(createErrorMessage.value);
      return;
    }

    if (createItemType.value === 'Goal' && !createForm.value.section) {
      createErrorMessage.value = 'Vui lòng chọn Mục (Phụ lục I).';
      toast.error(createErrorMessage.value);
      return;
    }

    if (createItemType.value === 'Goal' && !createForm.value.unitName) {
      createErrorMessage.value = 'Vui lòng chọn Đơn vị tính.';
      toast.error(createErrorMessage.value);
      return;
    }

    if (!createForm.value.leadAgencyId) {
      createErrorMessage.value = 'Vui lòng chọn đơn vị chủ trì.';
      toast.error(createErrorMessage.value);
      return;
    }

    if (createItemType.value !== 'Goal' && createForm.value.deliverables && createForm.value.deliverables.length > 0) {
      for (let idx = 0; idx < createForm.value.deliverables.length; idx++) {
        const del = createForm.value.deliverables[idx];
        if (!del.title || !del.title.trim()) {
          createErrorMessage.value = `Sản phẩm đầu ra #${idx + 1}: Tên sản phẩm / tên văn bản không được để trống.`;
          toast.error(createErrorMessage.value);
          return;
        }
      }
    }

    try {
      const docId = activeDocumentId.value || '12660000-0000-0000-0000-000000001266';
      let startDateIso = null;
      let dueDateIso = null;

      if (createForm.value.isOngoing) {
        startDateIso = new Date('2026-01-01T00:00:00.000Z').toISOString();
        dueDateIso = new Date('2030-12-31T23:59:59.000Z').toISOString();
      } else if (createForm.value.startDate || createForm.value.dueDate) {
        startDateIso = createForm.value.startDate ? new Date(createForm.value.startDate).toISOString() : null;
        dueDateIso = createForm.value.dueDate ? new Date(createForm.value.dueDate).toISOString() : null;
      } else {
        const sYear = createForm.value.startYear || 2026;
        const dYear = createForm.value.dueYear || 2030;
        startDateIso = new Date(`${sYear}-01-01T00:00:00.000Z`).toISOString();
        dueDateIso = new Date(`${dYear}-12-31T12:00:00.000Z`).toISOString();
      }

      const payload = {
        documentId: docId,
        itemType: createItemType.value,
        code: createForm.value.code ? createForm.value.code.trim() : '',
        title: createForm.value.title,
        section: createItemType.value === 'Goal' ? createForm.value.section : '',
        group: createForm.value.group,
        unitName: createForm.value.unitName || '%',
        resultTarget: createItemType.value === 'Goal' && createForm.value.resultTarget != null && createForm.value.resultTarget !== '' ? Number(createForm.value.resultTarget) : null,
        isOngoing: createForm.value.isOngoing,
        isGeneralTask: createForm.value.isGeneralTask,
        startDate: startDateIso,
        dueDate: dueDateIso,
        leadAgencyId: createForm.value.leadAgencyId,
        leadAgencyDisplayText: createForm.value.leadAgencyDisplayText?.trim() || null,
        assignedAgencyId: createForm.value.assignedAgencyId || null,
        subAgencyIds: [...(createForm.value.subAgencyIds || [])],
        coordinatingAgencyIds: createForm.value.coordinatingAgencyIds || [],
        coordinatingAgenciesDisplayText: createForm.value.coordinatingAgenciesDisplayText?.trim() || null,
        evaluationType: createItemType.value === 'Goal' ? 'Quantitative' : 'Qualitative',
        deliverables: createForm.value.deliverables || []
      };

      const res = await fetchWithAuth(getApiUrl('/api/planning/items'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (res.ok) {
        toast.success("Thêm mới thành công!");
        isCreateModalOpen.value = false;
        await loadData();
        planningGridRef.value?.loadGridData();
      } else {
        const err = await res.json().catch(() => ({}));
        const msg = parseApiError(err, 'Lỗi khi thêm mới mục tiêu/nhiệm vụ.');
        createErrorMessage.value = msg;
        toast.error(msg);
      }
    } catch (e) {
      createErrorMessage.value = 'Không thể kết nối máy chủ.';
      toast.error('Không thể kết nối máy chủ.');
    }
  }

  return {
    isCreateModalOpen,
    createItemType,
    createForm,
    createErrorMessage,
    addDeliverable,
    removeDeliverable,
    currentFormSections,
    currentFormGroups,
    createAssignedAgencyOptions,
    openCreateModal,
    submitCreateItem
  };
}
