<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 sm:p-7 space-y-4 font-sans max-h-[90vh] overflow-y-auto">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3">
        <div>
          <span class="text-xs font-bold text-blue-600 uppercase tracking-wider block">
            {{ isEditing ? 'Chỉnh Sửa Văn Bản Chỉ Đạo' : 'Thêm Mới Quyết Định / Văn Bản Chỉ Đạo' }}
          </span>
          <h3 class="text-lg font-extrabold text-slate-800">
            {{ isEditing ? 'Cập Nhật Quyết Định / Văn Bản Chỉ Đạo' : 'Thêm Quyết Định' }}
          </h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <form @submit.prevent="submitDocument" class="space-y-4">
        <div v-if="errorMessage" class="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs font-bold flex items-center gap-2">
          <svg class="w-4 h-4 text-rose-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
          <span>{{ errorMessage }}</span>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Số Hiệu Văn Bản / Quyết Định <span class="text-rose-500">*</span></label>
            <input v-model="form.documentNumber" required placeholder="VD: 749/QĐ-TTg, 06/QĐ-TTg..." class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 focus:bg-white focus:ring-2 focus:ring-blue-500" />
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Người Ký / Chức Vụ</label>
            <input v-model="form.signer" placeholder="VD: Thủ tướng Phạm Minh Chính" class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 focus:bg-white focus:ring-2 focus:ring-blue-500" />
          </div>
        </div>

        <div>
          <label class="text-xs font-bold text-slate-700 uppercase">Tên Văn Bản / Quyết Định <span class="text-rose-500">*</span></label>
          <textarea v-model="form.name" required rows="2" placeholder="VD: Chương trình Chuyển đổi số quốc gia đến năm 2025, định hướng đến năm 2030..." class="w-full text-sm font-semibold bg-slate-50 border border-slate-300 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-blue-500"></textarea>
        </div>

        <div>
          <label class="text-xs font-bold text-slate-700 uppercase">Trích Yếu Nội Dung Chính</label>
          <textarea v-model="form.summary" rows="2" placeholder="Tóm tắt trích yếu các nội dung quan trọng của quyết định..." class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-blue-500"></textarea>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Ngày Ban Hành <span class="text-rose-500">*</span></label>
            <input type="date" v-model="form.issueDate" required class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2" />
          </div>

          <div>
            <SearchableSelect 
              v-model="form.timeResolution" 
              :options="timeResolutionOptions" 
              :isMulti="false" 
              label="Thời Hạn Thực Hiện" 
            />
          </div>
        </div>

        <div v-if="form.timeResolution === 'Range'" class="grid grid-cols-2 gap-4 p-3.5 bg-blue-50/60 rounded-xl border border-blue-100">
          <div>
            <label class="text-xs font-bold text-blue-900 uppercase">Năm Bắt Đầu</label>
            <input type="number" v-model.number="form.startYear" class="w-full text-sm font-bold bg-white border border-blue-200 rounded-xl px-3 py-1.5" />
          </div>
          <div>
            <label class="text-xs font-bold text-blue-900 uppercase">Năm Kết Thúc</label>
            <input type="number" v-model.number="form.endYear" class="w-full text-sm font-bold bg-white border border-blue-200 rounded-xl px-3 py-1.5" />
          </div>
        </div>

        <!-- MULTI-FILE ATTACHMENT FIELD (PDF, DOCX) -->
        <div class="space-y-2">
          <label class="text-xs font-bold text-slate-700 uppercase flex items-center justify-between">
            <span>File Văn Bản Đính Kèm (PDF, DOCX - Có thể chọn nhiều file)</span>
            <span v-if="selectedFiles.length > 0" class="text-[11px] text-blue-600 font-bold">Đã chọn {{ selectedFiles.length }} file</span>
          </label>
          
          <input 
            type="file" 
            ref="fileInput" 
            multiple
            accept=".pdf,.doc,.docx"
            @change="onFilesSelected"
            class="w-full text-xs font-bold text-slate-600 bg-slate-50 border border-slate-300 rounded-xl p-2.5 cursor-pointer file:mr-3 file:py-1 file:px-3 file:rounded-lg file:border-0 file:text-xs file:font-bold file:bg-blue-600 file:text-white hover:file:bg-blue-700" 
          />

          <!-- Selected Files List -->
          <div v-if="selectedFiles.length > 0" class="space-y-1.5 max-h-32 overflow-y-auto pr-1">
            <div 
              v-for="(file, idx) in selectedFiles" 
              :key="idx"
              class="flex items-center justify-between bg-slate-100 rounded-lg px-3 py-1.5 text-xs text-slate-700 border border-slate-200"
            >
              <div class="flex items-center gap-2 truncate">
                <span class="text-blue-600 font-bold">📄</span>
                <span class="truncate font-semibold">{{ file.name }}</span>
                <span class="text-[10px] text-slate-400 font-mono">({{ (file.size / 1024 / 1024).toFixed(2) }} MB)</span>
              </div>
              <button type="button" @click="removeFile(idx)" class="text-slate-400 hover:text-rose-600 font-bold px-1.5 py-0.5 rounded hover:bg-rose-50 text-xs">✕</button>
            </div>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex justify-end gap-3 border-t border-slate-100 pt-3">
          <button type="button" @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl">Hủy</button>
          <button type="submit" :disabled="isSubmitting" class="px-5 py-2.5 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm transition disabled:opacity-50 flex items-center gap-1.5 cursor-pointer">
            <span v-if="isSubmitting" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>{{ isSubmitting ? (isEditing ? 'Đang lưu...' : 'Đang tạo...') : (isEditing ? 'Lưu Cập Nhật' : 'Thêm Quyết Định') }}</span>
          </button>
        </div>
      </form>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import SearchableSelect from './SearchableSelect.vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { getApiUrl } from '../config/api';

const timeResolutionOptions = ref([
  { value: 'Range', label: 'Giai đoạn nhiều năm (Ví dụ 2026-2030)' },
  { value: 'SpecificYear', label: 'Theo năm cụ thể' },
  { value: 'Continuous', label: 'Thường xuyên hàng năm' }
]);

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  documentToEdit: { type: Object, default: null }
});

const emit = defineEmits(['close', 'created', 'updated']);

const isEditing = computed(() => !!props.documentToEdit);
const fileInput = ref(null);
const selectedFiles = ref([]);
const isSubmitting = ref(false);
const errorMessage = ref(null);

const form = ref({
  documentNumber: '',
  name: '',
  summary: '',
  signer: '',
  issueDate: new Date().toISOString().split('T')[0],
  timeResolution: 'Range',
  startYear: 2026,
  endYear: 2030
});

watch(() => [props.isOpen, props.documentToEdit], ([newOpen, doc]) => {
  if (newOpen) {
    if (doc) {
      let formattedDate = new Date().toISOString().split('T')[0];
      if (doc.issueDate) {
        try {
          formattedDate = new Date(doc.issueDate).toISOString().split('T')[0];
        } catch (e) {}
      }
      form.value = {
        documentNumber: doc.documentNumber || '',
        name: doc.name || '',
        summary: doc.summary || '',
        signer: doc.signer || '',
        issueDate: formattedDate,
        timeResolution: doc.timeResolution || 'Range',
        startYear: doc.startYear || 2026,
        endYear: doc.endYear || 2030
      };
      selectedFiles.value = [];
    } else {
      form.value = {
        documentNumber: '',
        name: '',
        summary: '',
        signer: '',
        issueDate: new Date().toISOString().split('T')[0],
        timeResolution: 'Range',
        startYear: 2026,
        endYear: 2030
      };
      selectedFiles.value = [];
    }
    errorMessage.value = null;
  }
}, { immediate: true });

function onFilesSelected(event) {
  if (event.target.files && event.target.files.length > 0) {
    const newFiles = Array.from(event.target.files);
    for (const f of newFiles) {
      if (!selectedFiles.value.some(exist => exist.name === f.name && exist.size === f.size)) {
        selectedFiles.value.push(f);
      }
    }
  }
  if (fileInput.value) fileInput.value.value = '';
}

function removeFile(index) {
  selectedFiles.value.splice(index, 1);
}

function close() {
  errorMessage.value = null;
  selectedFiles.value = [];
  if (fileInput.value) fileInput.value.value = '';
  emit('close');
}

async function submitDocument() {
  isSubmitting.value = true;
  errorMessage.value = null;

  try {
    const formData = new FormData();
    formData.append('DocumentNumber', form.value.documentNumber);
    formData.append('Name', form.value.name);
    if (form.value.summary) formData.append('Summary', form.value.summary);
    if (form.value.signer) formData.append('Signer', form.value.signer);
    formData.append('IssueDate', form.value.issueDate);
    formData.append('TimeResolution', form.value.timeResolution);
    formData.append('StartYear', form.value.startYear);
    formData.append('EndYear', form.value.endYear);

    if (selectedFiles.value.length > 0) {
      for (const file of selectedFiles.value) {
        formData.append('AttachmentFiles', file);
      }
      formData.append('AttachmentFile', selectedFiles.value[0]);
    }

    const url = isEditing.value 
      ? getApiUrl(`/api/documents/${props.documentToEdit.id}`)
      : getApiUrl('/api/documents');

    const method = isEditing.value ? 'PUT' : 'POST';

    const response = await fetch(url, {
      method,
      body: formData
    });

    if (response.ok) {
      const data = await response.json();
      if (isEditing.value) {
        toast.success(`Đã cập nhật thành công văn bản ${data.documentNumber}`);
        emit('updated', data);
      } else {
        toast.success(`Tạo thành công văn bản: ${data.documentNumber}`);
        emit('created', data);
      }
      close();
    } else {
      let errText = 'Khởi tạo văn bản thất bại. Vui lòng kiểm tra lại.';
      try {
        const errJson = await response.json();
        errText = errJson.error || errText;
      } catch (e) {
        errText = await response.text();
      }
      errorMessage.value = errText;
      toast.error(errText);
    }
  } catch (error) {
    errorMessage.value = 'Không thể kết nối Server API: ' + error.message;
    toast.error(errorMessage.value);
  } finally {
    isSubmitting.value = false;
  }
}
</script>
