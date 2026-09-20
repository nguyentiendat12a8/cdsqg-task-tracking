<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-6 sm:p-7 max-h-[92vh] flex flex-col font-sans">
      
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div>
          <span class="text-xs font-bold uppercase tracking-wider block" :class="itemType === 'Goal' ? 'text-purple-600' : 'text-blue-600'">
            Thêm {{ itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }}
          </span>
          <h3 class="text-lg font-bold text-slate-800">Thành Phần Mới Trong Quyết Định</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1 cursor-pointer">✕</button>
      </div>

      <form @submit.prevent="submitItem" class="flex-1 flex flex-col min-h-0 pt-3">
        <div class="flex-1 overflow-y-auto custom-scrollbar space-y-4 pr-1">
          <div v-if="errorMessage" class="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs font-bold">
            {{ errorMessage }}
          </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Mã <span class="text-rose-500">*</span></label>
            <input v-model="form.code" required placeholder="e.g. MT-01, NV-05" class="w-full text-xs font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2" />
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Phân Nhóm</label>
            <input v-model="form.category" placeholder="Chính phủ số, Kinh tế số..." class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2" />
          </div>
        </div>

        <div>
          <label class="text-xs font-bold text-slate-700 uppercase">
            {{ itemType === 'Goal' ? 'Tên Mục Tiêu' : 'Tên Nhiệm Vụ' }} <span class="text-rose-500">*</span>
          </label>
          <textarea 
            v-model="form.title" 
            required 
            rows="2" 
            :placeholder="itemType === 'Goal' ? 'Nhập tên chi tiết mục tiêu...' : 'Nhập tên chi tiết nhiệm vụ...'" 
            class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-blue-500"
          ></textarea>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <SearchableSelect 
              v-model="form.leadAgencyId" 
              :options="leadAgencyOptions" 
              :isMulti="false" 
              label="Đơn Vị Chủ Trì" 
              placeholder="-- Chọn Đơn vị Chủ trì --"
            />
          </div>

          <div>
            <SearchableSelect 
              v-model="form.unitId" 
              :options="unitOptions" 
              :isMulti="false" 
              label="Đơn Vị Tính" 
              placeholder="-- Chọn Đơn vị tính --"
              @change="onUnitChanged"
            />
          </div>
        </div>

        <div>
          <SearchableSelect 
            v-model="form.coordinatingAgencyIds" 
            :options="coordinatingAgencyOptions" 
            :isMulti="true" 
            label="Cơ Quan Phối Hợp" 
            placeholder="-- Chọn các cơ quan phối hợp --"
          />
        </div>

        <!-- Is Ongoing / Continuous Task Toggle -->
        <div class="flex items-center gap-2.5 p-3 bg-blue-50/70 border border-blue-200 rounded-xl">
          <input 
            type="checkbox" 
            id="isOngoingToggle" 
            v-model="form.isOngoing" 
            class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer"
          />
          <label for="isOngoingToggle" class="text-xs font-bold text-blue-950 cursor-pointer select-none">
            Thời hạn thực hiện: Thường xuyên (Nhiệm vụ duy trì liên tục, báo cáo theo kỳ)
          </label>
        </div>

        <!-- Multi-Deliverables Section for Tasks -->
        <div v-if="form.itemType === 'Task'" class="border border-slate-200 rounded-xl p-3.5 bg-slate-50/50 space-y-3">
          <div class="flex items-center justify-between">
            <label class="text-xs font-extrabold text-slate-800 uppercase flex items-center gap-1.5">
              <span>📋 Danh Mục Sản Phẩm Đầu Ra Dự Kiến (Phụ Lục II)</span>
            </label>
            <button 
              type="button" 
              @click="addDeliverable" 
              class="px-2.5 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-xs rounded-lg transition border border-blue-200 flex items-center gap-1 cursor-pointer"
            >
              + Thêm sản phẩm đầu ra
            </button>
          </div>

          <div v-if="!form.deliverables || form.deliverables.length === 0" class="text-xs text-slate-400 italic text-center py-2">
            Chưa khai báo sản phẩm đầu ra. Nhấn nút trên để thêm sản phẩm cụ thể.
          </div>

          <div v-else class="space-y-2.5 max-h-48 overflow-y-auto pr-1">
            <div 
              v-for="(del, idx) in form.deliverables" 
              :key="idx" 
              class="bg-white p-2.5 rounded-xl border border-slate-200 shadow-2xs space-y-2 relative"
            >
              <div class="flex items-center justify-between border-b border-slate-100 pb-1">
                <span class="text-[11px] font-extrabold text-blue-800">Sản phẩm đầu ra #{{ idx + 1 }}</span>
                <button 
                  type="button" 
                  @click="removeDeliverable(idx)" 
                  class="text-rose-500 hover:text-rose-700 text-xs font-bold hover:bg-rose-50 px-2 py-0.5 rounded transition cursor-pointer"
                >
                  ✕ Xóa
                </button>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
                <div class="sm:col-span-2">
                  <label class="text-[10px] font-bold text-slate-600">Tên sản phẩm đầu ra / Tên văn bản <span class="text-rose-500">*</span></label>
                  <input 
                    v-model="del.title" 
                    required 
                    placeholder="Ví dụ: Nghị định quy định về Dữ liệu số / Nền tảng chia sẻ..." 
                    class="w-full text-xs font-semibold bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                  />
                </div>
                <div>
                  <label class="text-[10px] font-bold text-slate-600">Hạn chót sản phẩm</label>
                  <DatePicker v-model="del.dueDate" placeholder="dd/mm/yyyy" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="flex justify-end gap-3 border-t border-slate-100 pt-3 shrink-0">
          <button type="button" @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl cursor-pointer">Hủy</button>
          <button type="submit" :disabled="isSubmitting" class="px-5 py-2.5 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm transition disabled:opacity-50 flex items-center gap-1.5 cursor-pointer">
            <span v-if="isSubmitting" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>{{ isSubmitting ? 'Đang lưu...' : (itemType === 'Goal' ? 'Thêm Mục Tiêu' : 'Thêm Nhiệm Vụ') }}</span>
          </button>
        </div>
      </form>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import SearchableSelect from './SearchableSelect.vue';
import DatePicker from './DatePicker.vue';
import { getApiUrl } from '../config/api';
import { parseApiError } from '../utils/errorUtils';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  documentId: { type: String, required: true },
  itemType: { type: String, default: 'Task' }
});

const emit = defineEmits(['close', 'created']);

const form = ref({
  documentId: props.documentId,
  itemType: props.itemType,
  code: '',
  title: '',
  category: 'Chính phủ số',
  leadAgencyId: '',
  coordinatingAgencyIds: [],
  unitId: '',
  evaluationType: 'Quantitative',
  calculationMethod: 'LatestValue',
  isOngoing: false,
  isGeneralTask: false,
  deliverables: []
});

function addDeliverable() {
  if (!form.value.deliverables) form.value.deliverables = [];
  form.value.deliverables.push({ title: '', dueDate: null, currentStatus: 'NotStarted' });
}

function removeDeliverable(index) {
  if (form.value.deliverables && index >= 0 && index < form.value.deliverables.length) {
    form.value.deliverables.splice(index, 1);
  }
}

const agencies = ref([]);
const units = ref([]);
const isSubmitting = ref(false);
const errorMessage = ref(null);
const isCoordinatingDropdownOpen = ref(false);

const leadAgencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (ag.code === 'ALL_AGENCIES') {
      return { value: ag.id, label: `🌐 ${ag.name}` };
    }
    return { value: ag.id, label: ag.name };
  });
});

const coordinatingAgencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (ag.code === 'ALL_AGENCIES') {
      return { value: ag.id, label: `🌐 ${ag.name}` };
    }
    return { value: ag.id, label: ag.name };
  });
});

watch(() => form.value.leadAgencyId, (newId) => {
  const selected = agencies.value.find(a => a.id === newId);
  if (selected && selected.code === 'ALL_AGENCIES') {
    form.value.isGeneralTask = true;
  } else {
    form.value.isGeneralTask = false;
  }
}, { immediate: true });

const unitOptions = computed(() => {
  return units.value.map(u => ({ value: u.id, label: `${u.name} (${formatUnitDataType(u.dataType)})` }));
});

function formatUnitDataType(dataType) {
  if (dataType === 1 || dataType === 'Integer') return 'Định lượng số nguyên';
  if (dataType === 2 || dataType === 'Decimal') return 'Định lượng số thập phân';
  if (dataType === 3 || dataType === 'Boolean') return 'Định tính Có/Không';
  if (dataType === 4 || dataType === 'Text_Status') return 'Định tính Văn bản/Trạng thái';
  return dataType || 'N/A';
}

function onUnitChanged() {
  const selectedUnit = units.value.find(u => u.id === form.value.unitId);
  if (selectedUnit) {
    const dt = selectedUnit.dataType;
    if (dt === 1 || dt === 2 || dt === 'Integer' || dt === 'Decimal') {
      form.value.evaluationType = 'Quantitative';
    } else {
      form.value.evaluationType = 'Qualitative';
    }
  }
}

function resetForm() {
  form.value = {
    documentId: props.documentId,
    itemType: props.itemType,
    code: '',
    title: '',
    category: 'Chính phủ số',
    leadAgencyId: agencies.value.length > 0 ? agencies.value[0].id : '',
    coordinatingAgencyIds: [],
    unitId: units.value.length > 0 ? units.value[0].id : '',
    evaluationType: 'Quantitative',
    calculationMethod: 'LatestValue',
    deliverables: []
  };
  if (units.value.length > 0) {
    onUnitChanged();
  }
  errorMessage.value = null;
  isCoordinatingDropdownOpen.value = false;
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    resetForm();
  }
}, { immediate: true });

watch(() => props.itemType, (newVal) => {
  form.value.itemType = newVal;
});

watch(() => props.documentId, (newVal) => {
  form.value.documentId = newVal;
});

async function loadCatalogs() {
  try {
    const [agencyRes, unitRes] = await Promise.all([
      fetch(getApiUrl('/api/agencies')),
      fetch(getApiUrl('/api/units'))
    ]);
    if (agencyRes.ok) agencies.value = await agencyRes.json();
    if (unitRes.ok) units.value = await unitRes.json();

    if (agencies.value.length > 0 && !form.value.leadAgencyId) {
      form.value.leadAgencyId = agencies.value[0].id;
    }
    if (units.value.length > 0 && !form.value.unitId) {
      form.value.unitId = units.value[0].id;
      onUnitChanged();
    }
  } catch (e) {
    console.error('Failed loading agencies or units catalogs:', e);
  }
}

function close() {
  resetForm();
  emit('close');
}

async function submitItem() {
  errorMessage.value = null;

  if (!form.value.title || !form.value.title.trim()) {
    errorMessage.value = 'Tên mục tiêu / nhiệm vụ không được để trống.';
    return;
  }

  if (!form.value.leadAgencyId) {
    errorMessage.value = 'Vui lòng chọn đơn vị chủ trì.';
    return;
  }

  if (form.value.deliverables && form.value.deliverables.length > 0) {
    for (let idx = 0; idx < form.value.deliverables.length; idx++) {
      const del = form.value.deliverables[idx];
      if (!del.title || !del.title.trim()) {
        errorMessage.value = `Sản phẩm đầu ra #${idx + 1}: Tên sản phẩm / tên văn bản không được để trống.`;
        return;
      }
    }
  }

  isSubmitting.value = true;

  try {
    const response = await fetch(getApiUrl('/api/planning/items'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form.value)
    });

    if (response.ok) {
      const data = await response.json();
      emit('created', data);
      close();
    } else {
      const err = await response.json().catch(() => ({}));
      errorMessage.value = parseApiError(err, 'Thêm mới thất bại. Vui lòng kiểm tra lại.');
    }
  } catch (e) {
    errorMessage.value = 'Không thể kết nối máy chủ API: ' + e.message;
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(() => {
  loadCatalogs();
});
</script>
