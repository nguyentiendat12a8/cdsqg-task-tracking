<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-6 space-y-4 font-sans max-h-[92vh] flex flex-col">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3 shrink-0">
        <div class="flex items-center gap-2">
          <div class="w-9 h-9 rounded-xl bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-lg">
            🔔
          </div>
          <div>
            <h3 class="text-base font-extrabold text-slate-800">
              Gửi Thông Báo / Nhắc Nhở Nhiệm Vụ (Chủ Trì, Phối Hợp & Trực Thuộc)
            </h3>
            <span class="text-xs text-slate-500 font-semibold">
              Gửi thông báo đến Đơn vị Chủ trì, các Đơn vị Phối hợp và các Đơn vị trực thuộc / phụ thuộc
            </span>
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
          <div class="flex items-center justify-between border-b border-slate-200 pb-2">
            <div>
              <label class="text-xs font-extrabold text-slate-800 uppercase tracking-wider block">
                1. Danh Sách Đơn Vị Nhận Thông Báo <span class="text-rose-500">*</span>
              </label>
              <span class="text-[11px] text-slate-500 font-semibold block mt-0.5">
                Chọn Đơn vị Chủ trì, Đơn vị Phối hợp và các Đơn vị trực thuộc / phụ thuộc để gửi thông báo
              </span>
            </div>
            
            <button 
              v-if="recipientOptions.length > 0"
              type="button" 
              @click="toggleSelectAll"
              class="px-2.5 py-1 text-[11px] font-bold text-blue-700 bg-blue-100 hover:bg-blue-200 rounded-lg transition cursor-pointer"
            >
              {{ isAllSelected ? '✕ Bỏ chọn tất cả' : '✓ Chọn tất cả' }}
            </button>
          </div>

          <LoadingSpinner v-if="isLoadingRecipients" text="Đang tải danh sách đơn vị nhận thông báo..." padding="py-4" />

          <div v-else-if="recipientOptions.length === 0" class="p-4 text-center bg-white rounded-xl border border-slate-200 text-slate-400 text-xs font-semibold italic">
            ⚠️ Không tìm thấy đơn vị phù hợp nào để gửi thông báo.
          </div>

          <div v-else class="max-h-56 overflow-y-auto space-y-1.5 custom-scrollbar pr-1">
            <div 
              v-for="rec in recipientOptions" 
              :key="rec.id"
              class="flex items-center justify-between p-2.5 bg-white hover:bg-blue-50/60 rounded-xl border border-slate-200 transition cursor-pointer"
              @click="toggleUserSelection(rec.id)"
            >
              <label class="flex items-center gap-2.5 cursor-pointer text-xs font-bold text-slate-800 flex-1 min-w-0 pointer-events-none">
                <input 
                  type="checkbox" 
                  :value="rec.id" 
                  v-model="selectedUserIds"
                  class="w-4 h-4 rounded text-blue-600 border-slate-300 focus:ring-blue-500 cursor-pointer pointer-events-auto" 
                  @click.stop
                />
                <span class="truncate">
                  <strong class="text-slate-900">{{ rec.agencyName || rec.name }}</strong>
                  <span v-if="rec.infoSummary" class="text-slate-600 font-medium ml-1.5">({{ rec.infoSummary }})</span>
                </span>
              </label>

              <div class="flex items-center gap-1.5 shrink-0 ml-2">
                <span 
                  :class="[
                    'px-2 py-0.5 rounded-md text-[10px] font-black',
                    rec.roleTag === 'Đơn vị chủ trì' ? 'bg-blue-100 text-blue-800' : 
                    rec.roleTag === 'Đơn vị phối hợp' ? 'bg-amber-100 text-amber-800' : 'bg-purple-100 text-purple-800'
                  ]"
                >
                  {{ rec.roleTag }}
                </span>
              </div>
            </div>
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
          class="px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition cursor-pointer"
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
import LoadingSpinner from './LoadingSpinner.vue';
import RichTextEditor from './RichTextEditor.vue';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';

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
const isLoadingRecipients = ref(false);
const recipientOptions = ref([]);
const selectedUserIds = ref([]);

const form = ref({
  type: 'TaskReminder',
  title: '',
  message: ''
});

const isAllSelected = computed(() => {
  return recipientOptions.value.length > 0 && selectedUserIds.value.length === recipientOptions.value.length;
});

function toggleSelectAll() {
  if (isAllSelected.value) {
    selectedUserIds.value = [];
  } else {
    selectedUserIds.value = recipientOptions.value.map(r => r.id);
  }
}

function toggleUserSelection(id) {
  const idx = selectedUserIds.value.indexOf(id);
  if (idx >= 0) {
    selectedUserIds.value.splice(idx, 1);
  } else {
    selectedUserIds.value.push(id);
  }
}

function computeAgencyRecipientOptions() {
  const options = [];
  const addedIds = new Set();
  const allAgencies = props.agencies || [];

  function addAgency(ag, roleTag, infoSummary) {
    if (!ag || !ag.id) return;
    const key = String(ag.id).toLowerCase();
    if (addedIds.has(key)) return;
    addedIds.add(key);
    options.push({
      id: ag.id,
      name: ag.name,
      agencyName: ag.name,
      agencyCode: ag.code,
      roleTag: roleTag,
      infoSummary: infoSummary || (ag.parentName ? `Trực thuộc ${ag.parentName}` : roleTag)
    });
  }

  function addAgencyAndHierarchy(agId, mainRoleTag, defaultSummary) {
    if (!agId) return;
    const agIdStr = String(agId).toLowerCase();
    const ag = allAgencies.find(a => String(a.id).toLowerCase() === agIdStr);
    if (!ag) {
      if (props.leadAgencyName && mainRoleTag === 'Đơn vị chủ trì') {
        addAgency({ id: agId, name: props.leadAgencyName, code: 'CHỦ_TRÌ' }, mainRoleTag, defaultSummary);
      }
      return;
    }

    // 1. Target Agency
    const summary = ag.parentName ? `Trực thuộc ${ag.parentName}` : defaultSummary;
    addAgency(ag, mainRoleTag, summary);

    // 2. Parent Ministry & Sibling Agencies
    if (ag.parentId) {
      const parentIdStr = String(ag.parentId).toLowerCase();
      const parentAg = allAgencies.find(a => String(a.id).toLowerCase() === parentIdStr);
      if (parentAg) {
        addAgency(parentAg, 'Cơ quan cấp trên', 'Bộ/Cơ quan quản lý trực tiếp');

        const siblings = allAgencies.filter(a => a.parentId && String(a.parentId).toLowerCase() === parentIdStr && String(a.id).toLowerCase() !== agIdStr);
        siblings.forEach(sib => {
          addAgency(sib, 'Đơn vị thuộc Bộ', `Trực thuộc ${parentAg.name}`);
        });
      }
    }

    // 3. Child Sub-Agencies directly under target agency
    const childSubs = allAgencies.filter(a => a.parentId && String(a.parentId).toLowerCase() === agIdStr);
    childSubs.forEach(sub => {
      addAgency(sub, 'Đơn vị trực thuộc', `Đơn vị trực thuộc ${ag.name}`);
    });
  }

  const leadId = props.leadAgencyId ? String(props.leadAgencyId).toLowerCase() : '';
  if (leadId) {
    addAgencyAndHierarchy(leadId, 'Đơn vị chủ trì', 'Cơ quan chủ trì chính');
  }

  const coordIds = (props.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());
  coordIds.forEach(cId => {
    if (cId) addAgencyAndHierarchy(cId, 'Đơn vị phối hợp', 'Cơ quan phối hợp');
  });

  return options;
}

async function fetchRecipientOptions() {
  isLoadingRecipients.value = true;
  recipientOptions.value = [];
  selectedUserIds.value = [];

  try {
    const coordsStr = Array.isArray(props.coordinatingAgencyIds) ? props.coordinatingAgencyIds.join(',') : '';
    const isAdmin = authState.isAdmin.value;
    const url = getApiUrl(`/api/notification/recipients-for-task?leadAgencyId=${props.leadAgencyId || ''}&coordinatingAgencyIds=${coordsStr}&isAdmin=${isAdmin}`);
    
    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (Array.isArray(data) && data.length > 0) {
        const uniqueData = [];
        const seen = new Set();
        data.forEach(item => {
          const k = String(item.id || item.agencyId).toLowerCase();
          if (!seen.has(k)) {
            seen.add(k);
            uniqueData.push(item);
          }
        });
        recipientOptions.value = uniqueData;
      } else {
        recipientOptions.value = computeAgencyRecipientOptions();
      }
    } else {
      recipientOptions.value = computeAgencyRecipientOptions();
    }
  } catch (e) {
    recipientOptions.value = computeAgencyRecipientOptions();
  } finally {
    // Mặc định tick chọn gửi hết
    selectedUserIds.value = recipientOptions.value.map(r => r.id);
    isLoadingRecipients.value = false;
  }
}

function generateTemplateContent() {
  const code = props.itemCode || 'Mã nhiệm vụ';
  const title = props.itemTitle || 'Tên nhiệm vụ';
  const leadName = props.leadAgencyName || 'Đơn vị Chủ trì';

  form.value.title = `[Thông báo] V/v Thực hiện nhiệm vụ: ${code} - ${title}`;
  
  form.value.message = `<p><strong>THÔNG BÁO THỰC HIỆN NHIỆM VỤ CĐS QUỐC GIA</strong></p>
<p><strong>Kính gửi:</strong> Các Cán bộ Đầu mối Đơn vị Chủ trì (${leadName}), các Đơn vị Phối hợp và các Đơn vị trực thuộc</p>
<p>Văn phòng Ủy ban Chuyển đổi số Quốc gia trân trọng đề nghị các Đơn vị triển khai các nội dung sau đối với nhiệm vụ:</p>
<ul>
  <li><strong>Mã số nhiệm vụ:</strong> ${code}</li>
  <li><strong>Tên chi tiết:</strong> ${title}</li>
</ul>
<p>Đề nghị các Đơn vị chủ trì và phối hợp chỉ đạo triển khai đúng tiến độ và cập nhật báo cáo kết quả lên hệ thống theo dõi.</p>`;
}

watch(() => [props.isOpen, props.itemId], ([isOpen, itemId]) => {
  if (isOpen && itemId) {
    form.value = {
      type: 'TaskReminder',
      title: `[Thông báo] V/v Thực hiện nhiệm vụ: ${props.itemCode} - ${props.itemTitle}`,
      message: ''
    };
    generateTemplateContent();
    fetchRecipientOptions();
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
  if (!selectedUserIds.value || selectedUserIds.value.length === 0) {
    toast.error("Vui lòng chọn ít nhất 1 đơn vị để gửi thông báo!");
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

  const selectedRecs = recipientOptions.value.filter(r => selectedUserIds.value.includes(r.id));
  const recipientsSummaryStr = selectedRecs.map(r => `${r.agencyName || r.name} (${r.roleTag})`).join(', ');

  isSubmitting.value = true;
  try {
    const payload = {
      taskId: props.itemId,
      title: form.value.title,
      message: form.value.message,
      type: form.value.type,
      leadAgencyId: props.leadAgencyId || null,
      targetUserIds: selectedUserIds.value,
      createdBy: authState.user.value?.fullName || authState.user.value?.username || (authState.isAdmin.value ? 'Quản trị viên Hệ thống' : 'Đầu mối cơ quan'),
      recipientsSummary: recipientsSummaryStr
    };

    const res = await fetch(getApiUrl('/api/notification/send'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      const data = await res.json();
      toast.success("Gửi thông báo thành công.");
      window.dispatchEvent(new CustomEvent('notification-sent'));
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
