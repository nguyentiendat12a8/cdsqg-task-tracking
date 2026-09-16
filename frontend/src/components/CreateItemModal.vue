<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 sm:p-7 space-y-4">
      
      <div class="flex justify-between items-start border-b border-slate-100 pb-3">
        <div>
          <span class="text-xs font-bold uppercase tracking-wider block" :class="itemType === 'Goal' ? 'text-purple-600' : 'text-blue-600'">
            Thêm {{ itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }}
          </span>
          <h3 class="text-lg font-bold text-slate-800">Thành Phần Mới Trong Quyết Định</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <form @submit.prevent="submitItem" class="space-y-4">
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
              :options="agencyOptions" 
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
            :options="agencyOptions" 
            :isMulti="true" 
            label="Cơ Quan Phối Hợp" 
            placeholder="-- Chọn các cơ quan phối hợp --"
          />
        </div>

        <div class="flex justify-end gap-3 border-t border-slate-100 pt-3">
          <button type="button" @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl">Hủy</button>
          <button type="submit" :disabled="isSubmitting" class="px-5 py-2.5 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm transition disabled:opacity-50">
            {{ isSubmitting ? 'Đang lưu...' : (itemType === 'Goal' ? 'Thêm Mục Tiêu' : 'Thêm Nhiệm Vụ') }}
          </button>
        </div>
      </form>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import SearchableSelect from './SearchableSelect.vue';
import { getApiUrl } from '../config/api';

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
  calculationMethod: 'LatestValue'
});

const agencies = ref([]);
const units = ref([]);
const isSubmitting = ref(false);
const errorMessage = ref(null);
const isCoordinatingDropdownOpen = ref(false);

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: `${ag.code} - ${ag.name}` }));
});

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
  const prefix = props.itemType === 'Goal' ? 'MT' : 'NV';
  const defaultNum = String(Math.floor(Math.random() * 9) + 1).padStart(2, '0');

  form.value = {
    documentId: props.documentId,
    itemType: props.itemType,
    code: `${prefix}-${defaultNum}`,
    title: '',
    category: 'Chính phủ số',
    leadAgencyId: agencies.value.length > 0 ? agencies.value[0].id : '',
    coordinatingAgencyIds: [],
    unitId: units.value.length > 0 ? units.value[0].id : '',
    evaluationType: 'Quantitative',
    calculationMethod: 'LatestValue'
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
  if (!form.value.title) {
    const prefix = newVal === 'Goal' ? 'MT' : 'NV';
    const defaultNum = String(Math.floor(Math.random() * 9) + 1).padStart(2, '0');
    form.value.code = `${prefix}-${defaultNum}`;
  }
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
  isSubmitting.value = true;
  errorMessage.value = null;

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
      const err = await response.json();
      errorMessage.value = err.error || 'Thêm mới thất bại. Vui lòng kiểm tra lại.';
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
