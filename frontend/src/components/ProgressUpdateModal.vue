<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-5 sm:p-6 font-sans max-h-[90vh] flex flex-col animate-in fade-in duration-150">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 gap-3 shrink-0">
        <div class="min-w-0 flex-1">
          <span class="text-xs font-bold text-blue-700 uppercase tracking-wider block">Báo Cáo Tiến Độ Thực Hiện</span>
          <h3 class="text-xs sm:text-sm font-semibold text-slate-700 mt-1 leading-relaxed break-words">{{ taskCode }} - {{ taskTitle }}</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1 cursor-pointer shrink-0">✕</button>
      </div>

      <form @submit.prevent="submitProgress" class="flex-1 flex flex-col min-h-0 pt-3">
        
        <div class="flex-1 overflow-y-auto custom-scrollbar pr-1 space-y-4">
          <!-- Pending Approval Warning Banner -->
          <div v-if="hasPendingApproval" class="p-3.5 bg-amber-50 text-amber-900 border border-amber-300/80 rounded-xl text-xs font-bold flex items-start gap-2.5 leading-relaxed shadow-2xs">
            <span class="text-base leading-none">⏳</span>
            <div>
              <strong class="font-bold">Nhiệm vụ này đang ở trạng thái Chờ duyệt:</strong>
              <span class="font-medium text-amber-950 block mt-0.5"> Báo cáo tiến độ trước đó đang chờ Cấp 2 xem xét phê duyệt hoặc từ chối. Bạn không thể gửi báo cáo tiến độ mới cho tới khi cấp trên duyệt xong.</span>
            </div>
          </div>

          <!-- Period Selection Bar (Yearly default, Quarterly, Monthly) -->
          <div class="space-y-2 bg-slate-50 p-3.5 rounded-xl border border-slate-200 relative">
            <div v-if="isLoadingExisting" class="absolute inset-0 bg-white/80 backdrop-blur-2xs rounded-xl flex items-center justify-center gap-2 z-10 text-xs font-bold text-blue-700">
              <span class="w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin"></span>
              <span>Đang tải dữ liệu kỳ báo cáo...</span>
            </div>
            <label class="text-xs font-bold text-slate-700 uppercase block">Tần Suất & Kỳ Báo Cáo <span class="text-rose-500">*</span></label>
            
            <div class="flex items-center gap-1.5 bg-slate-200/80 p-1 rounded-xl">
              <button 
                type="button"
                @click="setPeriodType('yearly')" 
                :class="['flex-1 py-1.5 rounded-lg text-xs font-bold transition text-center', periodType === 'yearly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900']"
              >
                🗓 Theo Năm (Mặc định)
              </button>

              <button 
                v-if="showQuarterOption"
                type="button"
                @click="setPeriodType('quarterly')" 
                :class="['flex-1 py-1.5 rounded-lg text-xs font-bold transition text-center', periodType === 'quarterly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900']"
              >
                📅 Theo Quý (4 Quý)
              </button>

              <button 
                v-if="showMonthOption"
                type="button"
                @click="setPeriodType('monthly')" 
                :class="['flex-1 py-1.5 rounded-lg text-xs font-bold transition text-center', periodType === 'monthly' ? 'bg-white text-blue-700 shadow-xs' : 'text-slate-600 hover:text-slate-900']"
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
              <span v-if="!unitName || unitName === '%'" class="absolute right-3.5 top-3 text-sm font-bold text-slate-400 pointer-events-none">%</span>
            </div>
          </div>

          <!-- Qualitative Progress Inputs (Only shown when task has NO deliverables) -->
          <div v-if="evaluationType !== 'Quantitative' && (!localDeliverables || localDeliverables.length === 0)" class="space-y-1">
            <label class="text-xs font-bold text-slate-700 uppercase">Trạng Thái Thực Tế Văn Bản <span class="text-rose-500">*</span></label>
            <SearchableSelect 
              v-model="form.status" 
              :options="qualitativeStatusOptions" 
              :isMulti="false" 
              placeholder="Chọn trạng thái"
            />
          </div>

          <!-- Multi-Deliverables Checklist Progress Updates -->
          <div v-if="localDeliverables && localDeliverables.length > 0" class="border border-slate-200 rounded-xl p-3 bg-slate-50/70 space-y-3">
            <div class="flex items-center justify-between gap-2 flex-wrap border-b border-slate-200/80 pb-2">
              <div class="flex items-center gap-2">
                <span class="text-xs font-bold text-blue-900 uppercase">📋 Cập Nhật Tiến Độ Danh Mục Sản Phẩm Đầu Ra</span>
                <span class="text-[11px] font-bold text-blue-700 bg-blue-100 px-2 py-0.5 rounded-full">{{ localDeliverables.length }} Sản phẩm</span>
              </div>
            </div>

            <div class="space-y-2.5 max-h-56 overflow-y-auto pr-1 custom-scrollbar">
              <div 
                v-for="(del, idx) in localDeliverables" 
                :key="idx"
                class="bg-white p-3 rounded-xl border border-slate-200 shadow-2xs space-y-2"
              >
                <div class="flex items-center justify-between border-b border-slate-100 pb-1.5">
                  <span class="text-xs font-bold text-slate-900">{{ idx + 1 }}. {{ del.title }}</span>
                  <span v-if="del.dueDate" class="text-[10px] font-bold text-slate-500 bg-slate-100 px-1.5 py-0.5 rounded">Hạn: {{ formatDate(del.dueDate) }}</span>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <div>
                    <label class="text-[10px] font-bold text-slate-600">Trạng Thái Mốc Sản Phẩm</label>
                    <SearchableSelect 
                      v-model="del.currentStatus" 
                      :options="qualitativeStatusOptions" 
                      :isMulti="false" 
                      :clearable="false"
                    />
                  </div>

                  <div>
                    <label class="text-[10px] font-bold text-slate-600">Số / Ký Hiệu Văn Bản</label>
                    <input 
                      v-model="del.documentNumber" 
                      placeholder="VD: 45/2026/NĐ-CP" 
                      class="w-full text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                </div>
              </div>
            </div>
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
        </div>

        <!-- Fixed Footer Actions -->
        <div class="flex justify-end gap-3 border-t border-slate-100 pt-3 shrink-0 mt-3">
          <button type="button" @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl">Hủy</button>
          <button 
            type="submit" 
            :disabled="isSubmitting || hasPendingApproval"
            :class="['px-5 py-2.5 text-xs font-bold text-white rounded-xl shadow-2xs transition flex items-center gap-1.5 cursor-pointer', (isSubmitting || hasPendingApproval) ? 'bg-slate-400 cursor-not-allowed opacity-60' : 'bg-blue-600 hover:bg-blue-700']"
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
import { authState } from '../services/auth';
import { parseApiError } from '../utils/errorUtils';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  taskId: { type: String, required: true },
  taskCode: { type: String, default: '' },
  taskTitle: { type: String, default: '' },
  evaluationType: { type: String, default: 'Quantitative' },
  unitName: { type: String, default: '%' },
  customBaseline: { type: Object, default: () => ({}) },
  deliverables: { type: Array, default: () => [] },
  hasPendingApproval: { type: Boolean, default: false }
});

const localDeliverables = ref([]);

function formatDate(dStr) {
  if (!dStr) return '';
  const d = new Date(dStr);
  if (isNaN(d.getTime())) return dStr;
  return d.toLocaleDateString('vi-VN');
}

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
  { value: 'Reviewing', label: 'Đang xin ý kiến / Thẩm định' },
  { value: 'Completed', label: 'Đã hoàn thành / Ban hành' }
];

function getQualitativeStatusLabel(val) {
  const found = qualitativeStatusOptions.find(o => o.value === val);
  return found ? found.label : (val || 'Chưa thực hiện');
}

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
const initialSnapshot = ref(null);

function captureSnapshot() {
  const fileNames = selectedFiles.value.map(f => f.name).sort();
  const existingUrls = [...existingFiles.value].sort();
  const dels = (localDeliverables.value || []).map(d => ({
    id: d.id || d.Id || '',
    status: d.currentStatus || d.CurrentStatus || 'NotStarted',
    docNum: (d.documentNumber || d.DocumentNumber || '').trim()
  }));
  return JSON.stringify({
    periodType: periodType.value,
    periodYear: form.value.periodYear,
    periodQuarter: form.value.periodQuarter,
    periodMonth: form.value.periodMonth,
    value: form.value.value,
    status: form.value.status,
    notes: (form.value.notes || '').trim(),
    existingFiles: existingUrls,
    selectedFiles: fileNames,
    deliverables: dels
  });
}

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
    const userAgencyId = authState.user.value?.agencyId || '';
    const agencyQuery = userAgencyId ? `&agencyId=${userAgencyId}` : '';
    const res = await fetch(getApiUrl(`/api/execution/tasks/${props.taskId}/progress?year=${form.value.periodYear}&period=${qParam}${agencyQuery}`));
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
          if (Array.isArray(data.deliverables) && data.deliverables.length > 0) {
            localDeliverables.value = data.deliverables.map(d => ({
              id: d.id || d.Id || '',
              title: d.title || d.Title || '',
              dueDate: d.dueDate || d.DueDate || null,
              currentStatus: d.currentStatus || d.CurrentStatus || 'NotStarted',
              documentNumber: d.documentNumber || d.DocumentNumber || '',
              promulgationDate: d.promulgationDate || d.PromulgationDate || null
            }));
          }
          initialSnapshot.value = captureSnapshot();
          return;
        }
      }
    }
    resetFormFields();
    initialSnapshot.value = captureSnapshot();
  } catch (e) {
    console.error('Failed to fetch existing progress log:', e);
    resetFormFields();
    initialSnapshot.value = captureSnapshot();
  } finally {
    isLoadingExisting.value = false;
  }
}

watch(() => [props.isOpen, props.customBaseline, props.deliverables], ([newOpen]) => {
  if (newOpen) {
    if (Array.isArray(props.deliverables)) {
      localDeliverables.value = props.deliverables.map(d => ({
        id: d.id || d.Id || '',
        title: d.title || d.Title || '',
        dueDate: d.dueDate || d.DueDate || null,
        currentStatus: d.currentStatus || d.CurrentStatus || 'NotStarted',
        documentNumber: d.documentNumber || d.DocumentNumber || '',
        promulgationDate: d.promulgationDate || d.PromulgationDate || null
      }));
    } else {
      localDeliverables.value = [];
    }
    if (showQuarterOption.value) {
      setPeriodType('quarterly');
    } else if (showMonthOption.value) {
      setPeriodType('monthly');
    } else {
      setPeriodType('yearly');
    }
    initialSnapshot.value = captureSnapshot();
  }
}, { immediate: true });

watch(() => [form.value.periodYear, form.value.periodQuarter, form.value.periodMonth, periodType.value, props.isOpen, props.taskId], async () => {
  if (props.isOpen && props.taskId) {
    await fetchExistingProgress();
  }
}, { immediate: true });

watch(() => localDeliverables.value, (newDels) => {
  if (newDels && newDels.length > 0) {
    const statuses = newDels.map(d => d.currentStatus || d.CurrentStatus || 'NotStarted');
    if (statuses.every(s => s === 'Completed')) {
      form.value.status = 'Completed';
    } else if (statuses.some(s => s === 'Reviewing')) {
      form.value.status = 'Reviewing';
    } else if (statuses.some(s => s === 'Drafting')) {
      form.value.status = 'Drafting';
    } else {
      form.value.status = 'NotStarted';
    }
  }
}, { deep: true, immediate: true });

function close() {
  resetFormFields();
  emit('close');
}

async function submitProgress() {
  if (authState.isAdmin.value) {
    toast.warning("Tài khoản Quản trị viên (Admin) không thực hiện cập nhật tiến độ. Thao tác này dành cho tài khoản cán bộ đầu mối của các Cơ quan / Bộ / Ngành.");
    return;
  }
  if (props.hasPendingApproval) {
    toast.warning("Nhiệm vụ này đang ở trạng thái Chờ duyệt. Vui lòng chờ Cấp 2 phê duyệt hoặc từ chối trước khi gửi báo cáo mới.");
    return;
  }

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
    if (!localDeliverables.value || localDeliverables.value.length === 0) {
      if (!form.value.status) {
        toast.error("Vui lòng chọn trạng thái thực tế văn bản!");
        return;
      }
    }
  }

  // Dirty check: check if any field or file has changed compared to initial state
  const currentSnapshot = captureSnapshot();
  if (initialSnapshot.value && currentSnapshot === initialSnapshot.value) {
    toast.info("Không có thay đổi nào trong báo cáo tiến độ.");
    return;
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
      formData.append('Status', form.value.status || 'Drafting');
    }

    if (localDeliverables.value && localDeliverables.value.length > 0) {
      localDeliverables.value.forEach((d, idx) => {
        formData.append(`Deliverables[${idx}].Id`, d.id || d.Id || '');
        formData.append(`Deliverables[${idx}].Title`, d.title || d.Title || '');
        if (d.dueDate || d.DueDate) formData.append(`Deliverables[${idx}].DueDate`, d.dueDate || d.DueDate);
        formData.append(`Deliverables[${idx}].CurrentStatus`, d.currentStatus || d.CurrentStatus || 'NotStarted');
        if (d.documentNumber || d.DocumentNumber) formData.append(`Deliverables[${idx}].DocumentNumber`, d.documentNumber || d.DocumentNumber);
        if (d.promulgationDate || d.PromulgationDate) formData.append(`Deliverables[${idx}].PromulgationDate`, d.promulgationDate || d.PromulgationDate);
      });
    }
    
    if (form.value.notes) {
      formData.append('SummaryNotes', form.value.notes);
    }

    const agencyName = authState.user.value?.agencyName || '';
    const userName = authState.user.value?.fullName || authState.user.value?.username || '';
    const creatorLabel = agencyName ? (userName ? `${agencyName} (${userName})` : agencyName) : (userName || 'Đơn vị chủ trì');
    formData.append('CreatedBy', creatorLabel);
    if (authState.user.value?.agencyId) {
      formData.append('AgencyId', authState.user.value.agencyId);
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
      toast.success("Lưu thành công!");
      emit('submitted', data);
      close();
    } else {
      let errorMsg = 'Lưu thất bại.';
      try {
        const errJson = await response.json();
        errorMsg = parseApiError(errJson, 'Lưu thất bại.');
      } catch (e) {
        errorMsg = 'Lưu thất bại.';
      }
      toast.error(errorMsg);
    }
  } catch (error) {
    toast.error('Lưu thất bại.');
  } finally {
    isSubmitting.value = false;
  }
}
</script>
