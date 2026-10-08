import { fetchWithAuth } from '../../services/auth';
import { ref, computed, watch } from 'vue';
import { toast } from 'vue3-toastify';
import { getApiUrl } from '../../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../../config/planningStructureConfig';
import { parseApiError } from '../../utils/errorUtils';

export function useCreateItem({ planningGridRef, agencies, rawItemsList, loadData, isSpecialAgencyCode }) {
  const isCreateModalOpen = ref(false);

  const createItemType = ref('Task');


  const createForm = ref({
    code: '',
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

  const createErrorMessage = ref('');

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

  const createAssignedAgencyOptions = computed(() => {
    if (!createForm.value.leadAgencyId) return [];
    return agencies.value
      .filter(ag => ag.parentId === createForm.value.leadAgencyId)
      .map(ag => ({ value: ag.id, label: ag.name }));
  });

  watch(() => createForm.value.leadAgencyId, (newId) => {
    const selected = agencies.value.find(a => a.id === newId);
    if (selected && isSpecialAgencyCode(selected.code)) {
      createForm.value.isGeneralTask = true;
    } else {
      createForm.value.isGeneralTask = false;
    }
    if (createForm.value.assignedAgencyId) {
      const isChild = agencies.value.some(a => a.id === createForm.value.assignedAgencyId && a.parentId === newId);
      if (!isChild) createForm.value.assignedAgencyId = '';
    }
  }, { immediate: true });

  function openCreateModal(type) {
    createItemType.value = type;
    const count = rawItemsList.value.filter(i => i.itemType === type || (type === 'Goal' && (i.itemType === 1 || i.itemType === '1')) || (type === 'Task' && (i.itemType === 2 || i.itemType === '2'))).length;
    const prefix = type === 'Goal' ? 'MT' : 'NV';
    const autoCode = `${prefix}-${String(count + 1).padStart(2, '0')}`;

    createForm.value = {
      code: autoCode,
      title: '',
      section: type === 'Goal' ? 'Mục A' : '',
      group: '',
      unitName: '%',
      isOngoing: false,
      isGeneralTask: false,
      startYear: 2026,
      dueYear: 2030,
      startDate: '',
      dueDate: '',
      leadAgencyId: agencies.value[0]?.id || '',
      assignedAgencyId: '',
      coordinatingAgencyIds: [],
      deliverables: []
    };
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
      const docId = '12660000-0000-0000-0000-000000001266';
      let startDateIso = null;
      let dueDateIso = null;

      if (createForm.value.isOngoing) {
        startDateIso = new Date('2026-01-01T00:00:00.000Z').toISOString();
        dueDateIso = new Date('2030-12-31T23:59:59.000Z').toISOString();
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
        isOngoing: createForm.value.isOngoing,
        isGeneralTask: createForm.value.isGeneralTask,
        startDate: startDateIso,
        dueDate: dueDateIso,
        leadAgencyId: createForm.value.leadAgencyId,
        assignedAgencyId: createForm.value.assignedAgencyId || null,
        coordinatingAgencyIds: createForm.value.coordinatingAgencyIds || [],
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
