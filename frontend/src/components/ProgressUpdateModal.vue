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

          <div class="grid grid-cols-2 gap-3 pt-1">
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Năm Báo Cáo</label>
              <select v-model.number="form.periodYear" class="w-full text-xs font-bold bg-white border border-slate-300 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500">
                <option :value="2026">Năm 2026</option>
                <option :value="2027">Năm 2027</option>
                <option :value="2028">Năm 2028</option>
                <option :value="2029">Năm 2029</option>
                <option :value="2030">Năm 2030</option>
              </select>
            </div>

            <!-- Dynamic Sub-Period Selection -->
            <div v-if="periodType === 'quarterly'" class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Chọn Quý Báo Cáo</label>
              <select v-model.number="form.periodQuarter" class="w-full text-xs font-bold bg-white border border-slate-300 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500">
                <option :value="1">Quý I (Q1)</option>
                <option :value="2">Quý II (Q2)</option>
                <option :value="3">Quý III (Q3)</option>
                <option :value="4">Quý IV (Q4)</option>
              </select>
            </div>

            <div v-else-if="periodType === 'monthly'" class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Chọn Tháng Báo Cáo</label>
              <select v-model.number="form.periodMonth" class="w-full text-xs font-bold bg-white border border-slate-300 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500">
                <option v-for="m in 12" :key="m" :value="m">Tháng {{ m }} (T{{ m }})</option>
              </select>
            </div>

            <div v-else class="space-y-1">
              <label class="text-[11px] font-bold text-slate-600">Kỳ Tổng Hợp</label>
              <div class="text-xs font-bold text-slate-700 bg-white border border-slate-200 rounded-xl px-3 py-2 text-center">
                Báo cáo Cả Năm {{ form.periodYear }}
              </div>
            </div>
          </div>
        </div>

        <!-- Quantitative vs Qualitative Inputs -->
        <div v-if="evaluationType === 'Quantitative'" class="space-y-1">
          <label class="text-xs font-bold text-slate-700 uppercase">Giá Trị Thực Tế Đạt Được <span class="text-rose-500">*</span></label>
          <div class="relative">
            <input 
              type="number" 
              step="0.1" 
              v-model.number="form.value" 
              required 
              placeholder="Nhập con số thực tế..." 
              class="w-full text-base font-bold text-slate-800 bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
            <span class="absolute right-3.5 top-3 text-sm font-bold text-slate-400">{{ unitName || '%' }}</span>
          </div>
        </div>

        <div v-else class="space-y-1">
          <label class="text-xs font-bold text-slate-700 uppercase">Trạng Thái Thực Tế Văn Bản <span class="text-rose-500">*</span></label>
          <select 
            v-model="form.status" 
            required 
            class="w-full text-sm font-bold text-slate-800 bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
          >
            <option value="NotStarted">Chưa thực hiện</option>
            <option value="Drafting">Đang xây dựng / Soạn thảo</option>
            <option value="Reviewing">Đang xin ý kiến / Đánh giá</option>
            <option value="Completed">Đã hoàn thành ban hành</option>
          </select>
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
            📄 File Minh Chứng Đã Đính Kèm Trước Đó ({{ existingFiles.length }} file)
          </label>
          <div class="space-y-1.5">
            <div v-for="(fileUrl, idx) in existingFiles" :key="idx" class="flex items-center justify-between text-xs gap-2 bg-white/60 p-1.5 rounded-lg border border-purple-100">
              <a 
                :href="getFileUrl(fileUrl)" 
                target="_blank" 
                class="text-purple-700 hover:text-purple-900 hover:underline font-semibold truncate flex items-center gap-1.5 min-w-0"
              >
                <span class="shrink-0">📎</span>
                <span class="truncate">{{ formatFileName(fileUrl) }}</span>
              </a>
              <a 
                :href="getFileUrl(fileUrl)" 
                target="_blank" 
                download
                class="inline-flex items-center gap-1 px-2.5 py-1 text-[11px] font-bold text-purple-700 hover:text-white bg-purple-100 hover:bg-purple-600 rounded-lg transition-all shadow-2xs shrink-0 cursor-pointer"
                title="Tải file minh chứng về máy"
              >
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                </svg>
                <span>Tải xuống</span>
              </a>
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
    if (res.ok) {
      const data = await res.json();
      if (data) {
        form.value.value = data.actualValue !== undefined ? data.actualValue : null;
        form.value.status = data.status || 'Drafting';
        form.value.notes = data.summaryNotes || '';
        existingFiles.value = data.attachmentFileUrls || [];
        selectedFiles.value = [];
        return;
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
  isSubmitting.value = true;
  try {
    const formData = new FormData();
    formData.append('PeriodYear', form.value.periodYear);
    formData.append('PeriodType', periodType.value === 'monthly' ? 'Monthly' : (periodType.value === 'quarterly' ? 'Quarterly' : 'Yearly'));
    formData.append('PeriodQuarter', periodType.value === 'quarterly' ? form.value.periodQuarter : 0);
    formData.append('PeriodMonth', periodType.value === 'monthly' ? form.value.periodMonth : 0);
    
    if (props.evaluationType === 'Quantitative') {
      if (form.value.value !== null && form.value.value !== '') {
        formData.append('Value', form.value.value);
        formData.append('ActualValue', form.value.value);
      }
    } else {
      if (form.value.status) {
        formData.append('Status', form.value.status);
      }
    }
    
    if (form.value.notes) {
      formData.append('SummaryNotes', form.value.notes);
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
