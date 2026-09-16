<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-lg w-full p-6 space-y-5 animate-in fade-in duration-150">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3">
        <div>
          <span class="text-xs font-bold text-blue-700 uppercase tracking-wider block">Báo Cáo Tiến Độ Thực Hiện</span>
          <h3 class="text-lg font-bold text-slate-800 mt-0.5">{{ taskCode }} - {{ taskTitle }}</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <form @submit.prevent="submitProgress" class="space-y-4">
        
        <!-- Period Selection Bar (Yearly default, Quarterly, Monthly) -->
        <div class="space-y-2 bg-slate-50 p-3.5 rounded-xl border border-slate-200">
          <label class="text-xs font-bold text-slate-700 uppercase block">Tần Suất & Kỳ Báo Cáo <span class="text-rose-500">*</span></label>
          
          <div class="flex items-center gap-1.5 bg-slate-200/80 p-1 rounded-xl">
            <button 
              type="button"
              @click="setPeriodType('yearly')" 
              :class="['flex-1 py-1.5 rounded-lg text-xs font-extrabold transition text-center', periodType === 'yearly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900']"
            >
              🗓 Theo Năm (Mặc định)
            </button>

            <button 
              v-if="showQuarterOption"
              type="button"
              @click="setPeriodType('quarterly')" 
              :class="['flex-1 py-1.5 rounded-lg text-xs font-extrabold transition text-center', periodType === 'quarterly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900']"
            >
              📅 Theo Quý (4 Quý)
            </button>

            <button 
              v-if="showMonthOption"
              type="button"
              @click="setPeriodType('monthly')" 
              :class="['flex-1 py-1.5 rounded-lg text-xs font-extrabold transition text-center', periodType === 'monthly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900']"
            >
              📆 Theo Tháng (12 Tháng)
            </button>
          </div>

          <div :class="['grid gap-3 pt-1', periodType === 'yearly' ? 'grid-cols-1' : 'grid-cols-2']">
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Năm Báo Cáo <span class="text-rose-500">*</span></label>
              <SearchableSelect 
                v-model="form.periodYear" 
                :options="yearOptions" 
                :isMulti="false" 
                :clearable="false"
                placeholder="Chọn năm"
              />
            </div>

            <!-- Dynamic Sub-Period Selection -->
            <div v-if="periodType === 'quarterly'" class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Chọn Quý Báo Cáo <span class="text-rose-500">*</span></label>
              <SearchableSelect 
                v-model="form.periodQuarter" 
                :options="quarterOptions" 
                :isMulti="false" 
                :clearable="false"
                placeholder="Chọn quý"
              />
            </div>

            <div v-else-if="periodType === 'monthly'" class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Chọn Tháng Báo Cáo <span class="text-rose-500">*</span></label>
              <SearchableSelect 
                v-model="form.periodMonth" 
                :options="monthOptions" 
                :isMulti="false" 
                :clearable="false"
                placeholder="Chọn tháng"
              />
            </div>
          </div>
        </div>

        <!-- Quantitative vs Qualitative Progress Inputs -->
        <div v-if="evaluationType === 'Quantitative'" class="space-y-1">
          <label class="text-xs font-bold text-slate-700 uppercase">GIÁ TRỊ ĐẠT ĐƯỢC <span class="text-rose-500">*</span></label>
          <div class="relative">
            <input 
              type="number" 
              step="0.01"
              v-model.number="form.value" 
              required 
              placeholder="Nhập con số thực tế..." 
              class="w-full text-base font-bold text-slate-800 bg-slate-50 border border-slate-300 rounded-xl pl-3.5 pr-10 py-2.5 focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
            />
            <span class="absolute right-3.5 top-3 text-sm font-bold text-slate-400 pointer-events-none">{{ unitName || '%' }}</span>
          </div>
        </div>

        <div v-else class="space-y-1">
          <label class="text-xs font-bold text-slate-700 uppercase">Trạng Thái Thực Tế Văn Bản <span class="text-rose-500">*</span></label>
          <SearchableSelect 
            v-model="form.status" 
            :options="qualitativeStatusOptions" 
            :isMulti="false" 
            placeholder="Chọn trạng thái"
          />
        </div>

        <!-- Evidence Multi-File Picker (IFormFile Array) -->
        <div class="space-y-2">
          <label class="text-xs font-bold text-slate-700 uppercase flex items-center justify-between">
            <span>Văn Bản Minh Chứng (File PDF/Word/Excel)</span>
            <span v-if="selectedFiles.length > 0" class="text-[11px] text-blue-600 font-semibold">Đã chọn {{ selectedFiles.length }} file</span>
          </label>
          
          <input 
            type="file" 
            ref="fileInput"
            multiple
            @change="handleFilesChange" 
            accept=".pdf,.doc,.docx,.xls,.xlsx"
            class="w-full text-xs text-slate-600 bg-slate-50 border border-slate-300 rounded-xl p-2.5 focus:bg-white file:mr-3 file:py-1 file:px-3 file:rounded-lg file:border-0 file:text-xs file:font-bold file:bg-blue-100 file:text-blue-800 hover:file:bg-blue-200 cursor-pointer"
          />

          <!-- Selected Files List with Removal Buttons -->
          <div v-if="selectedFiles.length > 0" class="space-y-1.5 pt-1 max-h-36 overflow-y-auto pr-1">
            <div 
              v-for="(file, index) in selectedFiles" 
              :key="index"
              class="flex items-center justify-between bg-slate-50 rounded-lg px-3 py-1.5 border border-slate-200 text-xs text-slate-700 hover:border-slate-300 transition"
            >
              <div class="flex items-center gap-2 truncate pr-2">
                <span class="text-blue-600 font-bold">📄</span>
                <span class="truncate font-medium text-slate-800">{{ file.name }}</span>
                <span class="text-[10px] text-slate-400 font-mono">({{ (file.size / 1024 / 1024).toFixed(2) }} MB)</span>
              </div>
              <button 
                type="button" 
                @click="removeFile(index)" 
                class="text-slate-400 hover:text-rose-600 font-bold px-1.5 py-0.5 rounded-md hover:bg-rose-50 transition text-sm"
                title="Xóa file này khỏi danh sách"
              >
                ✕
              </button>
            </div>
          </div>
        </div>

        <!-- Existing Attached Files Section -->
        <div v-if="existingFiles.length > 0" class="space-y-1.5 p-3 bg-purple-50/80 rounded-xl border border-purple-200">
          <label class="text-[11px] font-bold text-purple-900 uppercase block">
            📄 File Minh Chứng Đã Đính Kèm ({{ existingFiles.length }} file)
          </label>
          <div class="space-y-1.5">
            <div v-for="(fileUrl, idx) in existingFiles" :key="idx" class="flex items-center justify-between text-xs gap-2 bg-white p-2 rounded-xl border border-purple-100 shadow-2xs">
              <a 
                :href="getFileUrl(fileUrl)" 
                target="_blank" 
                class="text-purple-700 hover:text-purple-900 hover:underline font-semibold truncate flex items-center gap-1.5 min-w-0 flex-1"
              >
                <span class="shrink-0">📎</span>
                <span class="truncate">{{ formatFileName(fileUrl) }}</span>
              </a>

              <div class="flex items-center gap-1.5 shrink-0">
                <a 
                  :href="getFileUrl(fileUrl)" 
                  target="_blank" 
                  download
                  class="inline-flex items-center gap-1 px-2.5 py-1 text-[11px] font-bold text-purple-700 hover:text-white bg-purple-100 hover:bg-purple-600 rounded-lg transition-all cursor-pointer"
                  title="Tải file minh chứng về máy"
                >
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                  </svg>
                  <span>Tải về</span>
                </a>

                <button 
                  type="button" 
                  @click="removeExistingFile(idx)" 
                  class="inline-flex items-center gap-1 px-2.5 py-1 text-[11px] font-bold text-rose-600 hover:text-white bg-rose-50 hover:bg-rose-600 rounded-lg transition-all border border-rose-200 cursor-pointer"
                  title="Xóa file đính kèm này"
                >
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                  <span>Xóa</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Notes Area -->
        <div class="space-y-1">
          <label class="text-xs font-bold text-slate-700 uppercase">Nội dung giải trình / Ghi chú</label>
          <textarea 
            v-model="form.notes" 
            rows="3" 
            placeholder="Tóm tắt kết quả triển khai hoặc khó khăn vướng mắc..."
            class="w-full text-xs text-slate-800 bg-slate-50 border border-slate-300 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
          ></textarea>
        </div>

        <!-- Footer Actions -->
        <div class="flex justify-end gap-3 border-t border-slate-100 pt-3">
          <button type="button" @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl">Hủy</button>
          <button 
            type="submit" 
            :disabled="isSubmitting"
            class="px-5 py-2.5 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-xl shadow-sm transition flex items-center gap-1.5"
          >
            <span v-if="isSubmitting" class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            {{ isSubmitting ? 'Đang gửi...' : 'Gửi Báo Cáo Tiến Độ' }}
          </button>
        </div>

      </form>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import SearchableSelect from './SearchableSelect.vue';
import { getApiUrl } from '../config/api';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  taskId: { type: String, required: true },
  taskCode: { type: String, default: '' },
  taskTitle: { type: String, default: '' },
  evaluationType: { type: String, default: 'Quantitative' },
  unitName: { type: String, default: '%' },
  customBaseline: { type: Object, default: () => ({}) }
});

const emit = defineEmits(['close', 'submitted']);

const yearOptions = [
  { value: 2026, label: 'Năm 2026' },
  { value: 2027, label: 'Năm 2027' },
  { value: 2028, label: 'Năm 2028' },
  { value: 2029, label: 'Năm 2029' },
  { value: 2030, label: 'Năm 2030' }
];

const quarterOptions = [
  { value: 1, label: 'Quý I (Q1)' },
  { value: 2, label: 'Quý II (Q2)' },
  { value: 3, label: 'Quý III (Q3)' },
  { value: 4, label: 'Quý IV (Q4)' }
];

const monthOptions = Array.from({ length: 12 }, (_, i) => ({
  value: i + 1,
  label: `Tháng ${i + 1} (T${i + 1})`
}));

const qualitativeStatusOptions = [
  { value: 'NotStarted', label: 'Chưa thực hiện' },
  { value: 'Drafting', label: 'Đang xây dựng / Soạn thảo' },
  { value: 'Reviewing', label: 'Đang xin ý kiến / Báo cáo' },
  { value: 'Completed', label: 'Đã hoàn thành ban hành' }
];

const showQuarterOption = computed(() => {
  const cb = props.customBaseline || {};
  return cb.hasQuarter === 'true' || cb.hasQuarter === true || Object.keys(cb).some(k => k.startsWith('Q'));
});

const showMonthOption = computed(() => {
  const cb = props.customBaseline || {};
  return cb.hasMonth === 'true' || cb.hasMonth === true || Object.keys(cb).some(k => k.startsWith('M'));
});

const periodType = ref('yearly'); // 'yearly' | 'quarterly' | 'monthly'

const form = ref({
  periodYear: 2026,
  periodQuarter: 0,
  periodMonth: 1,
  value: null,
  status: 'Drafting',
  notes: ''
});

function setPeriodType(type) {
  periodType.value = type;
  if (type === 'yearly') {
    form.value.periodQuarter = 0;
    form.value.periodMonth = 0;
  } else if (type === 'quarterly') {
    if (form.value.periodQuarter === 0) form.value.periodQuarter = 1;
    form.value.periodMonth = 0;
  } else if (type === 'monthly') {
    form.value.periodQuarter = 0;
    if (!form.value.periodMonth) form.value.periodMonth = 1;
  }
}

const fileInput = ref(null);
const selectedFiles = ref([]);
const existingFiles = ref([]);
const isSubmitting = ref(false);
const isLoadingExisting = ref(false);

function handleFilesChange(e) {
  if (e.target.files && e.target.files.length > 0) {
    const newFiles = Array.from(e.target.files);
    for (const file of newFiles) {
      if (!selectedFiles.value.some(f => f.name === file.name && f.size === file.size)) {
        selectedFiles.value.push(file);
      }
    }
  }
  if (fileInput.value) fileInput.value.value = '';
}

function removeFile(index) {
  selectedFiles.value.splice(index, 1);
}

function removeExistingFile(index) {
  existingFiles.value.splice(index, 1);
}

function resetFormFields() {
  form.value.value = null;
  form.value.status = 'Drafting';
  form.value.notes = '';
  existingFiles.value = [];
  selectedFiles.value = [];
  if (fileInput.value) fileInput.value.value = '';
}

function formatFileName(fullPath) {
  if (!fullPath) return 'File minh chứng';
  const rawFileName = fullPath.split(/[/\\]/).pop() || fullPath;
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}_/i;
  return rawFileName.replace(guidRegex, '');
}

function getFileUrl(path) {
  if (!path) return '#';
  return getApiUrl(path.trim());
}

async function fetchExistingProgress() {
  if (!props.taskId || !props.isOpen) return;
  isLoadingExisting.value = true;
  try {
    const qParam = periodType.value === 'quarterly' ? form.value.periodQuarter : (periodType.value === 'monthly' ? form.value.periodMonth : 0);
    const res = await fetch(getApiUrl(`/api/execution/tasks/${props.taskId}/progress?year=${form.value.periodYear}&period=${qParam}`));
    if (res.ok && res.status !== 204) {
      const text = await res.text();
      if (text && text.trim().length > 0) {
        const data = JSON.parse(text);
        if (data) {
          form.value.value = data.actualValue !== undefined ? data.actualValue : (data.value !== undefined ? data.value : null);
          form.value.status = data.status || 'Drafting';
          form.value.notes = data.summaryNotes || '';
          existingFiles.value = data.attachmentFileUrls || [];
          selectedFiles.value = [];
          return;
        }
      }
    }
    resetFormFields();
  } catch (e) {
    console.error('Failed to fetch existing progress log:', e);
    resetFormFields();
  } finally {
    isLoadingExisting.value = false;
  }
}

watch(() => [props.isOpen, props.customBaseline], ([newOpen]) => {
  if (newOpen) {
    if (showQuarterOption.value) {
      setPeriodType('quarterly');
    } else if (showMonthOption.value) {
      setPeriodType('monthly');
    } else {
      setPeriodType('yearly');
    }
  }
}, { immediate: true });

watch(() => [form.value.periodYear, form.value.periodQuarter, form.value.periodMonth, periodType.value, props.isOpen, props.taskId], async () => {
  if (props.isOpen && props.taskId) {
    await fetchExistingProgress();
  }
}, { immediate: true });

function close() {
  resetFormFields();
  emit('close');
}

async function submitProgress() {
  if (!form.value.periodYear) {
    toast.error("Vui lòng chọn Năm Báo Cáo!");
    return;
  }
  if (periodType.value === 'quarterly' && !form.value.periodQuarter) {
    toast.error("Vui lòng chọn Quý Báo Cáo!");
    return;
  }
  if (periodType.value === 'monthly' && !form.value.periodMonth) {
    toast.error("Vui lòng chọn Tháng Báo Cáo!");
    return;
  }
  if (props.evaluationType === 'Quantitative') {
    if (form.value.value === null || form.value.value === '' || isNaN(form.value.value)) {
      toast.error("Vui lòng nhập con số giá trị đạt được!");
      return;
    }
  } else {
    if (!form.value.status) {
      toast.error("Vui lòng chọn trạng thái thực tế văn bản!");
      return;
    }
  }

  isSubmitting.value = true;
  try {
    const formData = new FormData();
    formData.append('PeriodYear', form.value.periodYear);
    formData.append('PeriodType', periodType.value === 'monthly' ? 'Monthly' : (periodType.value === 'quarterly' ? 'Quarterly' : 'Yearly'));
    formData.append('PeriodQuarter', periodType.value === 'quarterly' ? form.value.periodQuarter : 0);
    formData.append('PeriodMonth', periodType.value === 'monthly' ? form.value.periodMonth : 0);
    
    if (props.evaluationType === 'Quantitative') {
      formData.append('Value', form.value.value);
      formData.append('ActualValue', form.value.value);
    } else {
      formData.append('Status', form.value.status);
    }
    
    if (form.value.notes) {
      formData.append('SummaryNotes', form.value.notes);
    }

    if (existingFiles.value.length > 0) {
      for (const fileUrl of existingFiles.value) {
        formData.append('ExistingFiles', fileUrl);
      }
    }

    if (selectedFiles.value.length > 0) {
      for (const file of selectedFiles.value) {
        formData.append('EvidenceFiles', file);
      }
    }

    const response = await fetch(getApiUrl(`/api/execution/tasks/${props.taskId}/progress`), {
      method: 'POST',
      body: formData
    });

    if (response.ok) {
      const data = await response.json();
      toast.success("Cập nhật tiến độ thành công!");
      emit('submitted', data);
      close();
    } else {
      let errorMsg = 'Vui lòng kiểm tra lại thông tin.';
      try {
        const errJson = await response.json();
        errorMsg = errJson.message || errJson.error || errJson.title || JSON.stringify(errJson);
      } catch (e) {
        errorMsg = await response.text();
      }
      toast.error('Lỗi: ' + errorMsg, { autoClose: 4000 });
    }
  } catch (error) {
    toast.error('Lỗi kết nối Server API: ' + error.message, { autoClose: 4000 });
  } finally {
    isSubmitting.value = false;
  }
}
</script>
