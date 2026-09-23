<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl w-full max-h-[92vh] flex flex-col overflow-hidden">
      
      <!-- Modal Header -->
      <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
        <h3 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <span>📜</span>
          <span>{{ isEditing ? 'Chỉnh Sửa Văn Bản Quy Phạm Pháp Luật' : 'Thêm Mới Văn Bản Quy Phạm Pháp Luật' }}</span>
        </h3>
        <button @click="close" class="text-slate-400 hover:text-slate-700 font-bold text-lg cursor-pointer">✕</button>
      </div>

      <!-- Modal Form Body -->
      <form @submit.prevent="save" class="p-6 space-y-4 overflow-y-auto flex-1 custom-scrollbar text-xs">
        
        <!-- Row 1: Số ký hiệu & Loại văn bản -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label class="block font-bold text-slate-700 mb-1">
              Số Ký Hiệu Văn Bản <span class="text-rose-500">*</span>
            </label>
            <input 
              v-model="form.code" 
              required 
              placeholder="e.g. 1266/QĐ-TTg, 15/2026/NĐ-CP" 
              class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>

          <div>
            <SearchableSelect 
              v-model="form.documentType" 
              :options="documentTypeOptions" 
              :isMulti="false" 
              :clearable="false"
              label="Loại Văn Bản *" 
            />
          </div>

          <div>
            <SearchableSelect 
              v-model="form.effectStatus" 
              :options="effectStatusOptions" 
              :isMulti="false" 
              :clearable="false"
              label="Trạng Thái Hiệu Lực *" 
            />
          </div>
        </div>

        <!-- Row 2: Tên / Trích yếu văn bản -->
        <div>
          <label class="block font-bold text-slate-700 mb-1">
            Tên / Trích Yếu Nội Dung Văn Bản <span class="text-rose-500">*</span>
          </label>
          <textarea 
            v-model="form.title" 
            required 
            rows="2" 
            placeholder="Nhập trích yếu hoặc tên đầy đủ của văn bản quy phạm pháp luật..." 
            class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
          ></textarea>
        </div>

        <!-- Row 3: Cơ Quan Ban Hành (3-Tier Enforcement) & Cơ Quan Dự Thảo -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <SearchableSelect 
              v-model="form.issuingAgencyId" 
              :options="issuingAgencyOptions" 
              :isMulti="false" 
              :disabled="isIssuingAgencyLocked"
              label="Cơ Quan Ban Hành *" 
              placeholder="Chọn cơ quan ban hành"
            />
            <p v-if="isIssuingAgencyLocked" class="text-[10px] text-amber-700 mt-1 italic font-medium">
              🔒 Tài khoản Cấp 3: Cơ quan ban hành mặc định cố định là cơ quan của bạn.
            </p>
            <p v-else-if="isLevel2" class="text-[10px] text-blue-700 mt-1 italic font-medium">
              🏢 Tài khoản Cấp 2: Chỉ được chọn Bộ/Ngành của bạn hoặc các đơn vị trực thuộc.
            </p>
          </div>

          <div>
            <SearchableSelect 
              v-model="form.draftingAgencyId" 
              :options="agencySelectOptions" 
              :isMulti="false" 
              label="Cơ Quan Dự Thảo" 
              placeholder="Chọn cơ quan dự thảo văn bản"
            />
          </div>
        </div>

        <!-- Row 4: Người ký, Chức danh, Ngày ban hành & Có hiệu lực -->
        <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
          <div>
            <label class="block font-bold text-slate-700 mb-1">Người Ký</label>
            <input 
              v-model="form.signerName" 
              placeholder="e.g. Trần Lưu Quang" 
              class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>

          <div>
            <label class="block font-bold text-slate-700 mb-1">Chức Danh Người Ký</label>
            <input 
              v-model="form.signerTitle" 
              placeholder="e.g. Phó Thủ tướng, Bộ trưởng" 
              class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>

          <div>
            <label class="block font-bold text-slate-700 mb-1">Ngày Ban Hành</label>
            <input 
              type="date" 
              v-model="form.issuedDate" 
              class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>

          <div>
            <label class="block font-bold text-slate-700 mb-1">Ngày Có Hiệu Lực</label>
            <input 
              type="date" 
              v-model="form.effectiveDate" 
              class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>
        </div>

        <!-- Row 5: Lĩnh vực & Phạm vi áp dụng -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <SearchableSelect 
              v-model="form.field" 
              :options="fieldOptions" 
              :isMulti="false" 
              label="Lĩnh Vực / Nhóm Chuyển Đổi Số" 
              placeholder="Chọn lĩnh vực"
            />
          </div>

          <div>
            <SearchableSelect 
              v-model="form.scope" 
              :options="scopeOptions" 
              :isMulti="false" 
              label="Phạm Vi Áp Dụng" 
            />
          </div>
        </div>

        <!-- Row 6: Multi-File Attachments Section -->
        <div class="border border-slate-200 rounded-xl p-4 bg-slate-50/60 space-y-3">
          <div class="flex items-center justify-between">
            <label class="font-bold text-slate-800 flex items-center gap-1.5 uppercase tracking-wider text-[11px]">
              📎 Danh Sách Tệp Văn Bản Đính Kèm (Hỗ trợ nhiều file)
            </label>
          </div>

          <!-- Upload Bar -->
          <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-2 bg-white p-2.5 rounded-xl border border-slate-200">
            <input 
              type="file" 
              ref="fileInputRef" 
              @change="handleFileSelected" 
              class="text-xs text-slate-600 file:mr-2 file:py-1 file:px-2.5 file:rounded-lg file:border-0 file:text-xs file:font-bold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 cursor-pointer flex-1"
            />
            
            <SearchableSelect 
              v-model="uploadFileType" 
              :options="attachmentTypeOptions" 
              :isMulti="false" 
              :clearable="false"
              class="w-full sm:w-44" 
            />

            <button 
              type="button" 
              @click="uploadAttachment" 
              :disabled="!selectedFileToUpload || isUploading" 
              class="px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-40 text-white font-bold text-xs rounded-xl shadow-2xs transition flex items-center justify-center gap-1 cursor-pointer shrink-0"
            >
              <span>{{ isUploading ? 'Đang tải...' : '+ Tải tệp lên' }}</span>
            </button>
          </div>

          <!-- Attachment List Table -->
          <div v-if="form.attachments && form.attachments.length > 0" class="space-y-1.5 max-h-48 overflow-y-auto pr-1">
            <div 
              v-for="(att, idx) in form.attachments" 
              :key="idx" 
              class="flex items-center justify-between p-2.5 bg-white rounded-xl border border-slate-200 text-xs shadow-2xs"
            >
              <div class="flex items-center gap-2 min-w-0 pr-2">
                <span class="text-blue-500">📄</span>
                <button 
                  type="button" 
                  @click="openFileInNewWindow(att)" 
                  class="font-normal text-slate-800 hover:text-blue-700 hover:underline truncate cursor-pointer text-left" 
                  :title="`Click để mở xem tệp: ${att.cleanName || att.fileName}`"
                >
                  {{ att.cleanName || att.fileName }}
                </button>
                <span class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-blue-50 text-blue-700 border border-blue-200 shrink-0">
                  {{ att.fileType }}
                </span>
                <span class="text-[10px] text-slate-400 shrink-0">
                  ({{ formatBytes(att.fileSize) }})
                </span>
              </div>

              <button 
                type="button" 
                @click="removeAttachment(idx)" 
                class="text-rose-500 hover:text-rose-700 font-bold px-2 py-1 hover:bg-rose-50 rounded transition text-xs shrink-0 cursor-pointer"
              >
                ✕ Xóa
              </button>
            </div>
          </div>

          <p v-else class="text-center text-slate-400 italic text-xs py-2">
            Chưa có tệp văn bản đính kèm nào. Vui lòng chọn tệp và nhấn nút Tải tệp lên ở trên.
          </p>
        </div>

        <!-- Row 7: Ghi chú -->
        <div>
          <label class="block font-bold text-slate-700 mb-1">Ghi Chú Chi Tiết</label>
          <textarea 
            v-model="form.notes" 
            rows="2" 
            placeholder="Ghi chú thêm thông tin văn bản..." 
            class="w-full font-normal p-2.5 bg-slate-50 border border-slate-300 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
          ></textarea>
        </div>

        <!-- Modal Footer Actions -->
        <div class="pt-3 border-t border-slate-200 flex items-center justify-end gap-3">
          <button 
            type="button" 
            @click="close" 
            class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold rounded-xl transition cursor-pointer"
          >
            Hủy
          </button>
          <button 
            type="submit" 
            :disabled="isSubmitting"
            class="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl shadow-sm transition disabled:opacity-50 cursor-pointer"
          >
            {{ isSubmitting ? 'Đang lưu...' : (isEditing ? 'Cập Nhật Văn Bản' : 'Tạo Mới Văn Bản') }}
          </button>
        </div>

      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import SearchableSelect from './SearchableSelect.vue';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';
import { openFileInNewWindow } from '../utils/fileViewer';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  editingDocument: { type: Object, default: null },
  agencies: { type: Array, default: () => [] }
});

const emit = defineEmits(['close', 'saved']);

const isEditing = computed(() => !!props.editingDocument);
const isSubmitting = ref(false);
const isUploading = ref(false);

const fileInputRef = ref(null);
const selectedFileToUpload = ref(null);
const uploadFileType = ref('Văn bản chính');

const documentTypeOptions = [
  { value: 'Luật', label: 'Luật' },
  { value: 'Nghị định', label: 'Nghị định' },
  { value: 'Thông tư', label: 'Thông tư' },
  { value: 'Quyết định', label: 'Quyết định' },
  { value: 'Nghị quyết', label: 'Nghị quyết' },
  { value: 'Chỉ thị', label: 'Chỉ thị' },
  { value: 'Khác', label: 'Khác' }
];

const effectStatusOptions = [
  { value: 'Còn hiệu lực', label: 'Còn hiệu lực' },
  { value: 'Hết hiệu lực toàn bộ', label: 'Hết hiệu lực toàn bộ' },
  { value: 'Hết hiệu lực một phần', label: 'Hết hiệu lực một phần' },
  { value: 'Chưa có hiệu lực', label: 'Chưa có hiệu lực' }
];

const fieldOptions = [
  { value: 'Thể chế số', label: 'Thể chế số' },
  { value: 'Chính phủ số', label: 'Chính phủ số' },
  { value: 'Kinh tế số', label: 'Kinh tế số' },
  { value: 'Xã hội số', label: 'Xã hội số' },
  { value: 'Hạ tầng số', label: 'Hạ tầng số' },
  { value: 'Dữ liệu số', label: 'Dữ liệu số' },
  { value: 'Khác', label: 'Khác' }
];

const scopeOptions = [
  { value: 'Toàn quốc', label: 'Toàn quốc' },
  { value: 'Bộ/Ngành', label: 'Bộ/Ngành' },
  { value: 'Địa phương', label: 'Địa phương' }
];

const attachmentTypeOptions = [
  { value: 'Văn bản chính', label: 'Văn bản chính' },
  { value: 'Phụ lục', label: 'Phụ lục' },
  { value: 'Báo cáo giải trình', label: 'Báo cáo giải trình' },
  { value: 'Tờ trình', label: 'Tờ trình' }
];

const agencySelectOptions = computed(() => {
  return (props.agencies || [])
    .filter(a => a.code !== 'ALL_AGENCIES')
    .map(a => ({
      value: a.id,
      label: a.name
    }));
});

// Role permission checks for Issuing Agency dropdown
const userAgencyId = computed(() => authState.user.value?.agencyId || authState.user.value?.agency?.id || null);
const userAgencyObj = computed(() => props.agencies?.find(a => String(a.id).toLowerCase() === String(userAgencyId.value).toLowerCase()) || null);
const isAdmin = computed(() => authState.isAdmin.value);
const isLevel2 = computed(() => !isAdmin.value && userAgencyObj.value && !userAgencyObj.value.parentId);
const isLevel3 = computed(() => !isAdmin.value && userAgencyObj.value && userAgencyObj.value.parentId);

const isIssuingAgencyLocked = computed(() => isLevel3.value);

const issuingAgencyOptions = computed(() => {
  if (isAdmin.value) {
    return agencySelectOptions.value;
  }
  if (isLevel2.value && userAgencyId.value) {
    // Level 2: Ministry itself + direct sub-agencies
    const userAgIdStr = String(userAgencyId.value).toLowerCase();
    return (props.agencies || [])
      .filter(a => String(a.id).toLowerCase() === userAgIdStr || (a.parentId && String(a.parentId).toLowerCase() === userAgIdStr))
      .map(a => ({ value: a.id, label: a.name }));
  }
  if (isLevel3.value && userAgencyId.value) {
    // Level 3: Locked to user's agency
    const myAg = props.agencies?.find(a => String(a.id).toLowerCase() === String(userAgencyId.value).toLowerCase());
    return myAg ? [{ value: myAg.id, label: myAg.name }] : [];
  }
  return agencySelectOptions.value;
});

const form = reactive({
  code: '',
  title: '',
  documentType: 'Quyết định',
  issuingAgencyId: null,
  draftingAgencyId: null,
  signerName: '',
  signerTitle: '',
  issuedDate: '',
  effectiveDate: '',
  effectStatus: 'Còn hiệu lực',
  field: 'Thể chế số',
  scope: 'Toàn quốc',
  notes: '',
  attachments: []
});

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    resetForm();
    if (props.editingDocument) {
      const doc = props.editingDocument;
      form.code = doc.code || '';
      form.title = doc.title || '';
      form.documentType = doc.documentType || 'Quyết định';
      form.issuingAgencyId = doc.issuingAgencyId || null;
      form.draftingAgencyId = doc.draftingAgencyId || null;
      form.signerName = doc.signerName || '';
      form.signerTitle = doc.signerTitle || '';
      form.issuedDate = doc.issuedDate ? doc.issuedDate.split('T')[0] : '';
      form.effectiveDate = doc.effectiveDate ? doc.effectiveDate.split('T')[0] : '';
      form.effectStatus = doc.effectStatus || 'Còn hiệu lực';
      form.field = doc.field || 'Thể chế số';
      form.scope = doc.scope || 'Toàn quốc';
      form.notes = doc.notes || '';
      form.attachments = doc.attachments ? JSON.parse(JSON.stringify(doc.attachments)) : [];
    } else {
      // Auto set issuing agency for Level 2 and Level 3
      if (userAgencyId.value) {
        form.issuingAgencyId = userAgencyId.value;
        form.draftingAgencyId = userAgencyId.value;
      }
    }
  }
});

function resetForm() {
  form.code = '';
  form.title = '';
  form.documentType = 'Quyết định';
  form.issuingAgencyId = null;
  form.draftingAgencyId = null;
  form.signerName = '';
  form.signerTitle = '';
  form.issuedDate = '';
  form.effectiveDate = '';
  form.effectStatus = 'Còn hiệu lực';
  form.field = 'Thể chế số';
  form.scope = 'Toàn quốc';
  form.notes = '';
  form.attachments = [];
  selectedFileToUpload.value = null;
  if (fileInputRef.value) fileInputRef.value.value = '';
}

async function handleFileSelected(e) {
  const files = e.target.files;
  if (files && files.length > 0) {
    selectedFileToUpload.value = files[0];
    await uploadAttachment();
  } else {
    selectedFileToUpload.value = null;
  }
}

async function uploadAttachment() {
  if (!selectedFileToUpload.value) return;
  isUploading.value = true;

  try {
    const formData = new FormData();
    formData.append('file', selectedFileToUpload.value);

    const res = await fetch(getApiUrl(`/api/legaldocuments/upload-file?fileType=${encodeURIComponent(uploadFileType.value)}`), {
      method: 'POST',
      body: formData
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.message || 'Lỗi khi tải file lên máy chủ.');
    }

    const data = await res.json();
    form.attachments.push(data);
    toast.success(`Đã đính kèm tệp '${data.cleanName || data.fileName}' thành công!`);

    selectedFileToUpload.value = null;
    if (fileInputRef.value) fileInputRef.value.value = '';
  } catch (err) {
    toast.error(err.message || 'Không thể tải tệp lên.');
  } finally {
    isUploading.value = false;
  }
}

function removeAttachment(idx) {
  form.attachments.splice(idx, 1);
}

function close() {
  emit('close');
}

function formatBytes(bytes) {
  if (!bytes) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

async function save() {
  if (!form.code || !form.title) {
    toast.error('Vui lòng điền đầy đủ Số ký hiệu và Trích yếu văn bản.');
    return;
  }

  isSubmitting.value = true;
  try {
    // If a file is selected but not yet uploaded, upload it automatically before saving
    if (selectedFileToUpload.value) {
      await uploadAttachment();
    }

    const issuingAg = props.agencies?.find(a => String(a.id).toLowerCase() === String(form.issuingAgencyId).toLowerCase());
    const draftingAg = props.agencies?.find(a => String(a.id).toLowerCase() === String(form.draftingAgencyId).toLowerCase());

    const payload = {
      code: form.code,
      title: form.title,
      documentType: form.documentType,
      issuingAgencyId: form.issuingAgencyId || null,
      issuingAgencyName: issuingAg?.name || null,
      draftingAgencyId: form.draftingAgencyId || null,
      draftingAgencyName: draftingAg?.name || null,
      signerName: form.signerName,
      signerTitle: form.signerTitle,
      issuedDate: form.issuedDate ? new Date(form.issuedDate).toISOString() : null,
      effectiveDate: form.effectiveDate ? new Date(form.effectiveDate).toISOString() : null,
      effectStatus: form.effectStatus,
      field: form.field,
      scope: form.scope,
      notes: form.notes,
      attachments: form.attachments,
      createdByAgencyId: userAgencyId.value || null,
      createdByUserId: authState.user.value?.id || null
    };

    let url = getApiUrl('/api/legaldocuments');
    let method = 'POST';

    if (isEditing.value) {
      url = getApiUrl(`/api/legaldocuments/${props.editingDocument.id}`);
      method = 'PUT';
    }

    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.message || 'Lỗi khi lưu thông tin văn bản.');
    }

    toast.success(isEditing.value ? 'Đã cập nhật văn bản QPPL thành công!' : 'Đã tạo mới văn bản QPPL thành công!');
    emit('saved');
    close();
  } catch (err) {
    toast.error(err.message || 'Không thể lưu văn bản.');
  } finally {
    isSubmitting.value = false;
  }
}
</script>
