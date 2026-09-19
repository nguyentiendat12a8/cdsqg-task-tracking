<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 space-y-4 font-sans max-h-[92vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div class="flex items-center gap-2">
          <div class="w-9 h-9 rounded-xl bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-lg">
            🔔
          </div>
          <div>
            <h3 class="text-base font-extrabold text-slate-800">Gửi Thông Báo / Nhắc Nhở Nhiệm Vụ</h3>
            <span class="text-xs text-slate-500 font-semibold">Gửi thông báo tới Tài khoản Đầu mối Chủ trì và các Đơn vị Phối hợp</span>
          </div>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <!-- Main Scrollable Content -->
      <div class="flex-1 overflow-y-auto space-y-4 pr-1 custom-scrollbar">

        <!-- Task Info Banner -->
        <div class="p-3.5 bg-blue-50/60 border border-blue-200/80 rounded-xl space-y-1">
          <div class="flex items-center gap-2 text-xs font-bold text-blue-900">
            <span class="px-2 py-0.5 bg-blue-600 text-white rounded-md text-[11px] font-black">{{ itemCode || 'Mã NV' }}</span>
            <span class="truncate">{{ itemTitle || 'Tên Mục tiêu / Nhiệm vụ' }}</span>
          </div>
          <div class="text-[11px] text-slate-600 flex flex-wrap gap-x-4 gap-y-1 pt-1 border-t border-blue-100/80">
            <span>🏛️ Đơn vị chủ trì: <strong class="text-slate-800">{{ leadAgencyName || 'Chưa phân công' }}</strong></span>
            <span v-if="coordinatingNames">🤝 Đơn vị phối hợp: <strong class="text-slate-800">{{ coordinatingNames }}</strong></span>
          </div>
        </div>

        <!-- Target Recipients Selection Block -->
        <div class="p-4 bg-slate-50 border border-slate-200 rounded-xl space-y-3">
          <label class="text-xs font-extrabold text-slate-700 uppercase block tracking-wider">
            1. Đơn Vị Nhận Thông Báo <span class="text-rose-500">*</span>
          </label>

          <!-- Lead Agency Recipient Option -->
          <div class="flex items-center justify-between bg-white p-3 rounded-xl border border-slate-200">
            <label class="flex items-center gap-2.5 cursor-pointer text-xs font-bold text-slate-800">
              <input 
                type="checkbox" 
                v-model="form.sendToLeadAgency" 
                class="w-4 h-4 rounded text-blue-600 border-slate-300 focus:ring-blue-500 cursor-pointer" 
              />
              <span>Gửi đến Tài khoản Đầu mối Chủ trì</span>
            </label>
            <span class="px-2.5 py-1 bg-blue-100 text-blue-800 rounded-lg text-xs font-black">
              {{ leadAgencyName || 'Đơn vị Chủ trì' }}
            </span>
          </div>

          <!-- Coordinating Agencies Multi-Select -->
          <div>
            <SearchableSelect 
              v-model="form.coordinatingAgencyIds" 
              :options="agencyOptions" 
              :isMulti="true" 
              label="Đơn Vị Phối Hợp Nhận Thông Báo" 
              placeholder="-- Chọn các đơn vị phối hợp --"
            />
          </div>
        </div>

        <!-- Notification Details Form -->
        <div class="space-y-3">
          <label class="text-xs font-extrabold text-slate-700 uppercase block tracking-wider">
            2. Nội Dung Thông Báo
          </label>

          <!-- Notification Title -->
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Tiêu Đề Thông Báo <span class="text-rose-500">*</span></label>
            <input 
              v-model="form.title" 
              type="text" 
              required 
              placeholder="Nhập tiêu đề thông báo..." 
              class="w-full text-xs font-bold bg-white border border-slate-300 rounded-xl px-3 py-2 min-h-[38px] focus:ring-2 focus:ring-blue-500" 
            />
          </div>

          <!-- Notification Message Content -->
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Nội Dung Chi Tiết <span class="text-rose-500">*</span></label>
            
            <RichTextEditor 
              v-model="form.message" 
              :showRestoreBtn="false" 
            />
          </div>
        </div>

      </div>

      <!-- Modal Footer -->
      <div class="flex justify-end gap-2.5 border-t border-slate-100 pt-3 shrink-0">
        <button 
          type="button" 
          @click="close" 
          class="px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition"
        >
          Hủy
        </button>
        <button 
          type="button" 
          @click="submitNotification" 
          :disabled="isSubmitting" 
          class="px-5 py-2.5 text-xs font-extrabold text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-xl shadow-sm transition flex items-center gap-1.5 cursor-pointer"
        >
          <span v-if="isSubmitting" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          <span>{{ isSubmitting ? 'Đang gửi...' : '🚀 Gửi Thông Báo' }}</span>
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { toast } from 'vue3-toastify';
import SearchableSelect from './SearchableSelect.vue';
import RichTextEditor from './RichTextEditor.vue';
import { getApiUrl } from '../config/api';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  itemId: { type: String, required: true },
  itemCode: { type: String, default: '' },
  itemTitle: { type: String, default: '' },
  leadAgencyId: { type: String, default: '' },
  leadAgencyName: { type: String, default: '' },
  coordinatingAgencyIds: { type: Array, default: () => [] },
  coordinatingNames: { type: String, default: '' },
  agencies: { type: Array, default: () => [] }
});

const emit = defineEmits(['close', 'sent']);

const isSubmitting = ref(false);

const form = ref({
  sendToLeadAgency: true,
  coordinatingAgencyIds: [],
  type: 'TaskReminder',
  title: '',
  message: ''
});

const agencyOptions = computed(() => {
  return props.agencies.map(ag => ({ value: ag.id, label: ag.name }));
});

const typeOptions = ref([
  { value: 'TaskReminder', label: '1. Nhắc nhở tiến độ thực hiện' },
  { value: 'UrgeNotice', label: '2. Văn bản chỉ đạo thực hiện' },
  { value: 'CoordinationNotice', label: '3. Thông báo phối hợp thực hiện' },
  { value: 'StatusUpdate', label: '4. Cập nhật trạng thái nhiệm vụ' }
]);

function generateTemplateContent() {
  const code = props.itemCode || 'Mã nhiệm vụ';
  const title = props.itemTitle || 'Tên nhiệm vụ';
  const leadName = props.leadAgencyName || 'Đơn vị Chủ trì';

  form.value.title = `[Thông báo] V/v Thực hiện nhiệm vụ: ${code} - ${title}`;
  form.value.message = `<p><strong>THÔNG BÁO THỰC HIỆN NHIỆM VỤ CĐS QUỐC GIA</strong></p>
<p><strong>Kính gửi:</strong> ${leadName} (Đơn vị chủ trì) và các Đơn vị Phối hợp liên quan</p>
<p>Văn phòng Ủy ban Chuyển đổi số Quốc gia đề nghị các đơn vị thực hiện các nội dung sau đối với nhiệm vụ:</p>
<ul>
  <li><strong>Mã số nhiệm vụ:</strong> ${code}</li>
  <li><strong>Tên chi tiết:</strong> ${title}</li>
</ul>
<p>Đề nghị Đơn vị chủ trì phối hợp chặt chẽ với các đơn vị liên quan để triển khai đúng tiến độ và cập nhật báo cáo kết quả lên hệ thống theo dõi.</p>`;
}

watch(() => [props.isOpen, props.itemId], ([isOpen, itemId]) => {
  if (isOpen && itemId) {
    form.value = {
      sendToLeadAgency: true,
      coordinatingAgencyIds: Array.isArray(props.coordinatingAgencyIds) ? [...props.coordinatingAgencyIds] : [],
      type: 'TaskReminder',
      title: `[Thông báo] V/v Thực hiện nhiệm vụ: ${props.itemCode} - ${props.itemTitle}`,
      message: ''
    };
    generateTemplateContent();
  }
}, { immediate: true });

function close() {
  emit('close');
}

function stripHtml(html) {
  if (!html) return '';
  const tmp = document.createElement('div');
  tmp.innerHTML = html;
  return tmp.textContent || tmp.innerText || '';
}

async function submitNotification() {
  if (!form.value.sendToLeadAgency && (!form.value.coordinatingAgencyIds || form.value.coordinatingAgencyIds.length === 0)) {
    toast.error("Vui lòng chọn ít nhất 1 đơn vị nhận thông báo (Chủ trì hoặc Phối hợp)!");
    return;
  }

  if (!form.value.title.trim()) {
    toast.error("Vui lòng nhập tiêu đề thông báo!");
    return;
  }

  if (!stripHtml(form.value.message).trim()) {
    toast.error("Vui lòng nhập nội dung thông báo!");
    return;
  }

  isSubmitting.value = true;
  try {
    const payload = {
      taskId: props.itemId,
      title: form.value.title,
      message: form.value.message,
      type: form.value.type,
      leadAgencyId: props.leadAgencyId || null,
      coordinatingAgencyIds: form.value.coordinatingAgencyIds || [],
      sendToLeadAgency: form.value.sendToLeadAgency
    };

    const res = await fetch(getApiUrl('/api/notification/send'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      const data = await res.json();
      toast.success(data.message || "Đã gửi thông báo thành công!");
      emit('sent', data);
      close();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || "Lỗi khi gửi thông báo.");
    }
  } catch (e) {
    toast.error("Không thể kết nối máy chủ khi gửi thông báo.");
  } finally {
    isSubmitting.value = false;
  }
}
</script>
