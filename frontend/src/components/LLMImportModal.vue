<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4 overflow-y-auto">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl w-full p-6 space-y-6 animate-in fade-in duration-150">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-4">
        <div>
          <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-extrabold text-purple-800 bg-purple-100 mb-1">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>
            AI Data Extraction - Human-in-the-Loop Review
          </span>
          <h3 class="text-xl font-bold text-slate-800 tracking-tight">Đồng Bộ Dữ Liệu Tự Động Từ LLM JSON</h3>
          <p class="text-xs text-slate-500 mt-0.5">Kiểm tra, điều chỉnh các chỉ tiêu trích xuất từ Quyết định PDF trước khi ghi vào PostgreSQL</p>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <!-- JSON Input Area or Upload Button -->
      <div v-if="!payload" class="space-y-4">
        <div class="space-y-1.5">
          <label class="text-xs font-bold text-slate-700 uppercase">Dán chuỗi JSON trích xuất từ LLM (Qwen2.5 / Gemma 4):</label>
          <textarea 
            v-model="rawJsonInput" 
            rows="8" 
            placeholder='{"document": {"documentNumber": "749/QĐ-TTg", "title": "..."}, "items": [...]}'
            class="w-full text-xs font-mono bg-slate-900 text-emerald-400 p-4 rounded-xl border border-slate-800 focus:outline-none"
          ></textarea>
        </div>

        <div class="flex justify-between items-center">
          <button @click="loadSamplePayload" class="text-xs font-bold text-blue-600 hover:underline">
            + Tải Dữ Liệu JSON Mẫu Quyết Định 749/QĐ-TTg
          </button>
          <button @click="parseJson" class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm">
            Xem Trước & Kiểm Tra (Preview)
          </button>
        </div>
      </div>

      <!-- Human-in-the-Loop Editable Preview Table -->
      <div v-else class="space-y-5">
        
        <!-- Document Meta Preview -->
        <div class="grid grid-cols-3 gap-4 bg-purple-50/70 p-4 rounded-xl border border-purple-100">
          <div>
            <span class="text-[11px] font-bold text-purple-700 uppercase block">Số Hiệu Quyết Định</span>
            <input v-model="payload.document.documentNumber" class="w-full text-sm font-bold bg-white border border-purple-200 rounded-lg px-2.5 py-1 text-slate-800" />
          </div>
          <div>
            <span class="text-[11px] font-bold text-purple-700 uppercase block">Khung Thời Gian</span>
            <span class="text-sm font-extrabold text-purple-900 block mt-1">{{ payload.document.startYear }} - {{ payload.document.endYear }}</span>
          </div>
          <div>
            <span class="text-[11px] font-bold text-purple-700 uppercase block">Số Cơ Quan Phát Hiện</span>
            <span class="text-sm font-extrabold text-purple-900 block mt-1">{{ payload.agencies.length }} Cơ quan</span>
          </div>
        </div>

        <!-- Editable Items Table -->
        <div class="overflow-x-auto border border-slate-200 rounded-xl max-h-72 overflow-y-auto">
          <table class="w-full text-left text-xs text-slate-700">
            <thead class="bg-slate-100 uppercase text-slate-500 font-bold sticky top-0 border-b border-slate-200">
              <tr>
                <th class="px-3 py-2.5">Cấp / Mã</th>
                <th class="px-3 py-2.5">Tên Nhiệm Vụ / Mục Tiêu</th>
                <th class="px-3 py-2.5">Chủ Trì</th>
                <th class="px-3 py-2.5">Loại</th>
                <th class="px-3 py-2.5 text-center">Chỉ Tiêu Năm 2026</th>
                <th class="px-3 py-2.5 text-center">Thao Tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="(item, idx) in payload.items" :key="idx" class="hover:bg-slate-50">
                <td class="px-3 py-2 border-r border-slate-100">
                  <span :class="['px-1.5 py-0.5 rounded font-extrabold text-[10px]', item.itemType === 'Goal' ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800']">
                    {{ item.itemType }}
                  </span>
                  <input v-model="item.code" class="w-16 font-bold bg-white border border-slate-300 rounded px-1.5 py-0.5 mt-1 block" />
                </td>
                <td class="px-3 py-2 border-r border-slate-100">
                  <input v-model="item.title" class="w-full font-semibold bg-white border border-slate-300 rounded px-2 py-1 text-slate-800" />
                </td>
                <td class="px-3 py-2 border-r border-slate-100">
                  <input v-model="item.leadAgencyCode" class="w-16 font-bold uppercase bg-white border border-slate-300 rounded px-1.5 py-0.5" />
                </td>
                <td class="px-3 py-2 border-r border-slate-100">
                  <select v-model="item.evaluationType" class="bg-white border border-slate-300 rounded px-1.5 py-0.5 font-bold">
                    <option value="Quantitative">Quantitative</option>
                    <option value="Qualitative">Qualitative</option>
                  </select>
                </td>
                <td class="px-3 py-2 border-r border-slate-100 text-center">
                  <input 
                    v-if="item.baselines && item.baselines.length > 0" 
                    type="number" 
                    v-model.number="item.baselines[0].targetQuantity" 
                    class="w-16 text-center font-bold bg-white border border-slate-300 rounded px-1 py-0.5" 
                  />
                  <span v-else class="text-slate-400">—</span>
                </td>
                <td class="px-3 py-2 text-center">
                  <button @click="removeItem(idx)" class="text-rose-600 font-bold hover:text-rose-800">Xóa</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Footer Actions -->
        <div class="flex justify-between items-center border-t border-slate-100 pt-4">
          <button @click="payload = null" class="text-xs font-bold text-slate-500 hover:text-slate-700">
            ← Nhập Lại Chuỗi JSON
          </button>

          <div class="flex gap-3">
            <button @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl">Hủy</button>
            <button 
              @click="syncToDatabase" 
              :disabled="isSyncing"
              class="px-6 py-2.5 text-xs font-extrabold text-white bg-purple-600 hover:bg-purple-700 disabled:opacity-50 rounded-xl shadow-sm transition flex items-center gap-2"
            >
              <span v-if="isSyncing" class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              {{ isSyncing ? 'Đang Đồng Bộ...' : 'Đồng Bộ Vào PostgreSQL' }}
            </button>
          </div>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { toast } from 'vue3-toastify';
import { useTrackingStore } from '../stores/useTrackingStore';

const props = defineProps({
  isOpen: { type: Boolean, default: false }
});

const emit = defineEmits(['close', 'imported']);

const store = useTrackingStore();

const rawJsonInput = ref('');
const payload = ref(null);
const isSyncing = ref(false);

function close() {
  emit('close');
}

function loadSamplePayload() {
  rawJsonInput.value = JSON.stringify({
    document: {
      documentNumber: "749/QĐ-TTg",
      title: "Chương trình Chuyển đổi số quốc gia đến năm 2025, định hướng đến năm 2030",
      issueDate: "2020-06-03",
      timeResolution: "YearRange",
      startYear: 2026,
      endYear: 2030
    },
    agencies: [
      { code: "BTTTT", name: "Bộ Thông tin và Truyền thông", agencyType: "Ministry" },
      { code: "BCA", name: "Bộ Công an", agencyType: "Ministry" },
      { code: "BKHĐT", name: "Bộ Kế hoạch và Đầu tư", agencyType: "Ministry" }
    ],
    items: [
      {
        itemType: "Task",
        code: "NV-01",
        title: "Phát triển Nền tảng Định danh và Xác thực điện tử quốc gia",
        category: "Chính phủ số",
        leadAgencyCode: "BCA",
        coordinatingAgencyCodes: ["BTTTT"],
        evaluationType: "Quantitative",
        calculationMethod: "LatestValue",
        unitCode: "PERCENT",
        baselines: [{ year: 2026, quarter: 1, targetQuantity: 90.0 }]
      },
      {
        itemType: "Task",
        code: "NV-05",
        title: "Xây dựng Nghị định quy định về cơ sở dữ liệu dùng chung",
        category: "Thể chế CĐS",
        leadAgencyCode: "BTTTT",
        coordinatingAgencyCodes: ["BKHĐT"],
        evaluationType: "Qualitative",
        calculationMethod: "LatestValue",
        unitCode: "DOC",
        baselines: [{ year: 2026, quarter: 4, targetStatus: "Completed" }]
      }
    ]
  }, null, 2);
}

function parseJson() {
  try {
    payload.value = JSON.parse(rawJsonInput.value);
    toast.success('Phân tích cú pháp JSON hợp lệ!');
  } catch (err) {
    toast.error('Định dạng JSON không hợp lệ. Vui lòng kiểm tra lại: ' + err.message);
  }
}

function removeItem(idx) {
  if (payload.value && payload.value.items) {
    payload.value.items.splice(idx, 1);
  }
}

async function syncToDatabase() {
  if (!payload.value) return;
  isSyncing.value = true;
  try {
    const result = await store.importLlmBootstrapPayload(payload.value);
    toast.success(`Đã đồng bộ vào PostgreSQL thành công! Quyết định: ${result.documentNumber}, ${result.importedTasksCount} nhiệm vụ.`);
    emit('imported', result);
    close();
  } catch (err) {
    toast.error('Lỗi khi đồng bộ: ' + err.message);
  } finally {
    isSyncing.value = false;
  }
}
</script>
