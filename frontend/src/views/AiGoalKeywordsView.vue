<template>
  <div class="ai-workspace ai-admin-shell">
    <AiPageHeader title="Thiết lập từ khóa" />

    <div class="ai-admin-body space-y-4">
      <p v-if="error" role="alert" class="p-3 rounded-xl bg-rose-50 text-rose-800 border border-rose-200 text-xs">
        {{ error }}
      </p>
      <p v-if="message" role="status" class="p-3 rounded-xl bg-blue-50 text-blue-900 border border-blue-200 text-xs font-medium">
        {{ message }}
      </p>

      <!-- Khối chỉ dẫn trực quan: Cơ chế đọc 1 mục tiêu vs Tất cả mục tiêu (Mặc định thu gọn) -->
      <section class="bg-gradient-to-r from-blue-50/90 via-slate-50 to-indigo-50/80 border border-blue-200/80 rounded-2xl p-4 sm:p-5 shadow-2xs">
        <div class="flex items-center justify-between gap-3" :class="{ 'mb-3': showGuideDetails }">
          <div class="flex items-center gap-2">
            <span class="w-7 h-7 rounded-lg bg-blue-600 text-white flex items-center justify-center text-sm font-bold shadow-xs">
              AI
            </span>
            <h3 class="ui-section-title text-slate-900 font-bold">
              Quy trình hoạt động: Đọc 1 mục tiêu đơn lẻ vs Tất cả mục tiêu
            </h3>
          </div>
          <button
            type="button"
            class="text-xs font-semibold text-blue-700 hover:text-blue-900 underline flex items-center gap-1 cursor-pointer"
            @click="showGuideDetails = !showGuideDetails"
          >
            {{ showGuideDetails ? 'Thu gọn chỉ dẫn ▲' : 'Xem chi tiết ▼' }}
          </button>
        </div>

        <div v-show="showGuideDetails" class="grid grid-cols-1 md:grid-cols-3 gap-3 text-xs">
          <!-- Bước 1 -->
          <div class="bg-white/90 p-3.5 rounded-xl border border-slate-200/80 shadow-2xs space-y-1.5">
            <div class="flex items-center gap-1.5 font-bold text-blue-900">
              <span class="w-5 h-5 rounded-full bg-blue-100 text-blue-800 flex items-center justify-center font-bold text-xs">1</span>
              <span>Thiết lập từ khóa (Tại màn này)</span>
            </div>
            <p class="text-slate-600 leading-relaxed">
              Bạn thiết lập tập <em>Từ khóa nhận diện</em> và <em>Câu lệnh prompt</em> cho từng mục tiêu của văn bản <strong>{{ activeDocument.documentNumber }}</strong> (thay đổi văn bản ở thanh chọn trên đầu trang).
            </p>
          </div>

          <!-- Bước 2 -->
          <div class="bg-white/90 p-3.5 rounded-xl border border-blue-200 shadow-2xs space-y-1.5 relative overflow-hidden">
            <div class="flex items-center gap-1.5 font-bold text-indigo-900">
              <span class="w-5 h-5 rounded-full bg-indigo-100 text-indigo-800 flex items-center justify-center font-bold text-xs">2</span>
              <span>Chọn phạm vi đọc khi Nạp tài liệu</span>
            </div>
            <p class="text-slate-600 leading-relaxed">
              Tại màn <strong>Nạp file trực tiếp</strong> (hoặc bấm <em>Đọc lại</em>), bạn có 2 chế độ:
            </p>
            <ul class="text-slate-700 space-y-1 pl-1 font-medium">
              <li>• <strong>🎯 Đọc 1 mục tiêu:</strong> AI chỉ tập trung tìm số liệu cho riêng mục tiêu bạn chọn.</li>
              <li>• <strong>✨ Tất cả mục tiêu:</strong> AI tự động nạp toàn bộ bộ từ khóa của văn bản đang đọc để tìm mọi chỉ số!</li>
            </ul>
          </div>

          <!-- Bước 3 -->
          <div class="bg-white/90 p-3.5 rounded-xl border border-slate-200/80 shadow-2xs space-y-1.5">
            <div class="flex items-center gap-1.5 font-bold text-emerald-900">
              <span class="w-5 h-5 rounded-full bg-emerald-100 text-emerald-800 flex items-center justify-center font-bold text-xs">3</span>
              <span>Nhận kết quả bóc tách</span>
            </div>
            <p class="text-slate-600 leading-relaxed">
              Thuật toán thông minh sẽ lọc trước các trang tài liệu có chứa từ khóa rồi mới gọi AI. Dữ liệu tìm thấy sẽ gắn trực tiếp vào mục tiêu tương ứng để kiểm chứng.
            </p>
          </div>
        </div>
      </section>

      <!-- Toolbar Bộ lọc tìm kiếm: Chuẩn đồng bộ, gọn gàng -->
      <section class="bg-white border border-slate-200 rounded-2xl shadow-xs overflow-hidden">
        <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-slate-50/70 p-3 border-b border-slate-200 w-full">
          <div class="flex flex-wrap items-center gap-2.5 flex-1 min-w-0">
            <!-- Ô tìm kiếm nhanh chuẩn DocumentFilters -->
            <div class="relative flex-1 min-w-[240px]">
              <svg class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <input
                v-model="searchTerm"
                type="text"
                class="ui-single-line w-full text-xs font-semibold pl-9 pr-8 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
                placeholder="Tìm theo mã mục tiêu, tên mục tiêu, từ khóa..."
                @input="onSearchInput"
              />
              <button
                v-if="searchTerm"
                type="button"
                class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 p-0.5"
                aria-label="Xóa tìm kiếm"
                @click="searchTerm = ''; page = 1; load()"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <!-- Bộ lọc đơn vị đo chuẩn SearchableSelect -->
            <div class="w-48 shrink-0">
              <SearchableSelect
                v-model="unitFilter"
                :options="unitFilterOptions"
                label="Đơn vị đo"
                label-class="sr-only"
                placeholder="Tất cả đơn vị"
                :is-multi="false"
                :clearable="true"
                @update:model-value="onUnitFilterChange"
              />
            </div>
          </div>


        </div>

        <LoadingSpinner v-if="loading" text="Đang tải danh sách từ khóa AI..." />

        <!-- Bảng danh sách cân đối đẹp mắt (Desktop) -->
        <div v-else class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
          <table class="w-full min-w-[1200px] text-left text-xs text-slate-700 border-collapse">
            <thead class="bg-[#f1f5fc] text-xs text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-20 shadow-2xs select-none">
              <tr>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center w-[50px] min-w-[50px] max-w-[50px]">
                  STT
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center w-[90px] min-w-[90px] max-w-[90px]">
                  Mã
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 w-[29%] min-w-[280px]">
                  Tiêu đề mục tiêu
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 w-[29%] min-w-[280px]">
                  Từ khóa nhận diện AI
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center w-[90px] min-w-[90px] max-w-[90px]">
                  Đơn vị
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 w-[27%] min-w-[260px]">
                  Chỉ dẫn bóc tách (AI Prompt)
                </th>
                <th class="px-3 py-2.5 text-center w-[110px] min-w-[110px] max-w-[110px]">
                  Thao tác
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200 bg-white">
              <tr
                v-for="(item, index) in items"
                :key="item.id"
                class="hover:bg-blue-50/30 transition-colors"
              >
                <!-- STT -->
                <td class="px-3 py-3 border-r border-slate-200 text-center text-slate-500 font-medium">
                  {{ (page - 1) * pageSize + index + 1 }}
                </td>

                <!-- Mã -->
                <td class="px-3 py-3 border-r border-slate-200 text-center">
                  <span class="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-bold font-mono bg-blue-50 text-blue-700 border border-blue-200 shadow-2xs">
                    {{ item.goalCode }}
                  </span>
                </td>

                <!-- Tiêu đề mục tiêu -->
                <td class="px-3 py-3 border-r border-slate-200">
                  <p class="font-semibold text-slate-900 leading-snug" :title="'Cập nhật: ' + formatDateTime(item.updatedAt)">
                    {{ item.goalTitle }}
                  </p>
                </td>

                <!-- Từ khóa nhận diện AI -->
                <td class="px-3 py-3 border-r border-slate-200">
                  <div class="space-y-1.5">
                    <div class="flex flex-wrap gap-1">
                      <span
                        v-for="(kw, ki) in splitKeywords(item.keywordsVi)"
                        :key="ki"
                        class="inline-flex items-center px-2 py-0.5 rounded-md text-xs bg-slate-100 text-slate-700 border border-slate-200/80 font-medium"
                      >
                        {{ kw }}
                      </span>
                    </div>
                    <!-- Chỉ hiển thị tiếng Anh khi mục tiêu thực sự có dữ liệu (ví dụ chỉ tiêu quốc tế UN EGDI) -->
                    <div v-if="item.keywordsEn && item.keywordsEn.trim()" class="text-xs text-indigo-700 bg-indigo-50/70 p-1.5 rounded-md border border-indigo-200/70 leading-tight">
                      <span class="font-bold text-indigo-800 uppercase text-xs mr-1">EN:</span>
                      <span class="italic">{{ item.keywordsEn }}</span>
                    </div>
                  </div>
                </td>

                <!-- Đơn vị -->
                <td class="px-3 py-3 border-r border-slate-200 text-center">
                  <span class="inline-flex items-center justify-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-emerald-50 text-emerald-800 border border-emerald-200 shadow-2xs">
                    {{ item.expectedUnit || '—' }}
                  </span>
                </td>

                <!-- Chỉ dẫn bóc tách AI -->
                <td class="px-3 py-3 border-r border-slate-200">
                  <div
                    class="p-2 rounded-lg bg-slate-50 border border-slate-200/80 text-xs text-slate-700 leading-relaxed max-h-24 overflow-y-auto custom-scrollbar"
                    :title="item.extractionPrompt"
                  >
                    {{ item.extractionPrompt }}
                  </div>
                </td>

                <!-- Thao tác -->
                <td class="px-3 py-3 text-center">
                  <div class="flex items-center justify-center gap-1.5">
                    <button
                      type="button"
                      class="ui-btn ui-btn-secondary !px-2.5 !py-1 text-xs font-semibold"
                      :disabled="busy"
                      title="Sửa bộ từ khóa"
                      @click="openEdit(item)"
                    >
                      Sửa
                    </button>
                    <button
                      type="button"
                      class="ui-btn ui-btn-danger !px-2.5 !py-1 text-xs font-semibold"
                      :disabled="busy"
                      title="Xóa bộ từ khóa"
                      @click="handleDelete(item)"
                    >
                      Xóa
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <div v-if="!items.length" class="p-8 text-center text-slate-500">
            <p class="font-semibold text-slate-800 text-sm">
              {{ searchTerm || unitFilter ? 'Không tìm thấy bộ từ khóa phù hợp với bộ lọc' : 'Chưa có bộ từ khóa nào cho văn bản này' }}
            </p>
            <p class="text-xs text-slate-500 mt-1">
              Thêm bộ từ khóa để hướng dẫn AI đối soát và bóc tách dữ liệu theo mục tiêu của {{ activeDocument.documentNumber }}.
            </p>
            <button
              v-if="searchTerm || unitFilter"
              type="button"
              class="ui-btn ui-btn-secondary mt-3"
              @click="resetFilters"
            >
              Xóa bộ lọc
            </button>
            <ActionButton v-else kind="create" class="mt-3" @click="openCreate">
              Thêm bộ từ khóa
            </ActionButton>
          </div>
        </div>

        <div class="p-3 border-t border-slate-200 bg-slate-50/50">
          <AiListPagination
            v-model:page="page"
            v-model:page-size="pageSize"
            :total="total"
            :disabled="loading || busy"
            item-label="bộ từ khóa"
          />
        </div>
      </section>
    </div>

    <!-- Modal Thêm / Sửa bộ từ khóa -->
    <div
      v-if="modalOpen"
      v-accessible-dialog="closeModal"
      class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3 sm:p-4"
      @click.self="closeModal"
    >
      <form
        class="bg-white rounded-2xl w-full max-w-3xl max-h-[92dvh] flex flex-col shadow-2xl overflow-hidden border border-slate-200"
        @submit.prevent="saveForm"
      >
        <header class="p-5 sm:p-6 border-b border-slate-200 flex justify-between items-center gap-3 bg-slate-50/70">
          <div>
            <h3 class="ui-title text-xl font-bold text-slate-900">
              {{ form.id ? 'Sửa bộ từ khóa mục tiêu' : 'Thêm bộ từ khóa mục tiêu' }}
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">
              Văn bản: <strong class="text-slate-800">{{ activeDocument.documentNumber }}</strong>
            </p>
          </div>
          <button
            type="button"
            class="text-slate-400 hover:text-slate-700 p-2 rounded-lg hover:bg-slate-200/60 transition"
            :disabled="busy"
            aria-label="Đóng"
            @click="closeModal"
          >
            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </header>

        <div class="p-5 sm:p-6 overflow-y-auto min-h-0 space-y-4">
          <p v-if="formError" role="alert" class="p-3 rounded-xl bg-rose-50 text-rose-800 border border-rose-200 text-xs">
            {{ formError }}
          </p>

          <!-- Chọn nhanh mục tiêu từ danh sách mục tiêu của văn bản đang làm việc -->
          <div v-if="!form.id && modalPresetGoalOptions.length > 0">
            <SearchableSelect
              v-model="selectedPresetGoal"
              :options="modalPresetGoalOptions"
              label="Gợi ý: Chọn nhanh từ danh sách mục tiêu chiến lược"
              :is-multi="false"
              :clearable="true"
              @update:model-value="applyPresetGoal"
            />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label for="kw-goal-code" class="block font-semibold text-xs text-slate-700 mb-1">
                Mã mục tiêu <span class="text-rose-600">*</span>
              </label>
              <input
                id="kw-goal-code"
                v-model.trim="form.goalCode"
                required
                maxlength="20"
                class="w-full text-xs font-mono font-bold px-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
                placeholder="Ví dụ: MT-13"
              />
            </div>
            <div class="sm:col-span-2">
              <label for="kw-goal-unit" class="block font-semibold text-xs text-slate-700 mb-1">
                Đơn vị đo lường kỳ vọng <span class="text-rose-600">*</span>
              </label>
              <input
                id="kw-goal-unit"
                v-model.trim="form.expectedUnit"
                required
                maxlength="50"
                class="w-full text-xs px-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
                placeholder="%, rank, doanh nghiệp, sàn, lần GDP..."
              />
              <div class="flex flex-wrap gap-1.5 mt-1.5">
                <button
                  v-for="u in quickUnits"
                  :key="u"
                  type="button"
                  class="text-xs px-2 py-0.5 rounded-full border border-slate-200 bg-slate-50 text-slate-600 hover:bg-slate-100 transition cursor-pointer"
                  @click="form.expectedUnit = u"
                >
                  {{ u }}
                </button>
              </div>
            </div>
          </div>

          <div>
            <label for="kw-goal-title" class="block font-semibold text-xs text-slate-700 mb-1">
              Tiêu đề mục tiêu <span class="text-rose-600">*</span>
            </label>
            <input
              id="kw-goal-title"
              v-model.trim="form.goalTitle"
              required
              maxlength="500"
              class="w-full text-xs px-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
              placeholder="Ví dụ: Quy mô kinh tế số đạt tối thiểu 30% GDP"
            />
          </div>

          <div>
            <label for="kw-vi" class="block font-semibold text-xs text-slate-700 mb-1">
              Từ khóa tiếng Việt <span class="text-rose-600">*</span>
            </label>
            <textarea
              id="kw-vi"
              v-model.trim="form.keywordsVi"
              required
              rows="3"
              class="w-full text-xs font-normal px-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
              placeholder="Ví dụ: kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng"
            ></textarea>

          </div>

          <div>
            <label for="kw-en" class="block font-semibold text-xs text-slate-700 mb-1">
              Từ khóa tiếng Anh (chỉ dùng cho nguồn quốc tế)
            </label>
            <input
              id="kw-en"
              v-model.trim="form.keywordsEn"
              class="w-full text-xs px-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
              placeholder="Ví dụ: e-government, egdi, ranking, survey"
            />

          </div>

          <div>
            <label for="kw-prompt" class="block font-semibold text-xs text-slate-700 mb-1">
              Chỉ dẫn bóc tách AI (Extraction Prompt) <span class="text-rose-600">*</span>
            </label>
            <textarea
              id="kw-prompt"
              v-model.trim="form.extractionPrompt"
              required
              rows="4"
              class="w-full text-xs font-normal px-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
              placeholder="Ví dụ: Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%). Đơn vị là %. Không lấy số liệu dự báo hay ngưỡng mục tiêu."
            ></textarea>

          </div>
        </div>

        <footer class="p-4 sm:px-6 border-t border-slate-200 bg-slate-50 flex justify-end gap-2.5">
          <button
            type="button"
            class="ui-btn ui-btn-secondary"
            :disabled="busy"
            @click="closeModal"
          >
            Hủy
          </button>
          <button
            type="submit"
            class="ui-btn ui-btn-primary"
            :disabled="busy"
          >
            {{ busy ? 'Đang lưu…' : form.id ? 'Cập nhật' : 'Thêm mới' }}
          </button>
        </footer>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { fetchWithAuth } from '../services/auth';
import { getApiUrl } from '../config/api';
import { formatDateTime } from '../shared/formatters';
import { confirmModal } from '../services/confirm';
import { activeDocumentId, activeDocument } from '../services/documentContext';
import SearchableSelect from '../components/SearchableSelect.vue';
import AiPageHeader from '../components/AiPageHeader.vue';
import ActionButton from '../components/ActionButton.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import AiListPagination from '../components/AiListPagination.vue';
import '../features/ai/ai.css';

defineEmits(['navigate']);

const items = ref([]);
const total = ref(0);
const page = ref(1);
const pageSize = ref(10);
const loading = ref(true);
const busy = ref(false);
const error = ref('');
const message = ref('');
const searchTerm = ref('');
const unitFilter = ref('');
const availableGoals = ref([]);
const showGuideDetails = ref(false);

const quickUnits = ['%', 'rank', 'doanh nghiệp', 'sàn', 'lần GDP', 'lượt người'];

const unitFilterOptions = [
  { value: '', label: 'Tất cả đơn vị' },
  { value: '%', label: 'Tỷ lệ (%)' },
  { value: 'rank', label: 'Thứ hạng (rank)' },
  { value: 'doanh nghiệp', label: 'Doanh nghiệp' },
  { value: 'sàn', label: 'Sàn dữ liệu' },
  { value: 'lần GDP', label: 'Lần GDP' },
  { value: 'lượt người', label: 'Lượt người' }
];

const modalPresetGoalOptions = computed(() => {
  const docId = activeDocumentId.value;
  const filtered = docId ? availableGoals.value.filter(g => g.documentId === docId) : availableGoals.value;
  return filtered.map(g => ({
    value: g.code,
    label: `${g.code} · ${g.title} (${g.unit || g.evaluationType})`
  }));
});

const modalOpen = ref(false);
const formError = ref('');
const selectedPresetGoal = ref('');
const form = ref({
  id: null,
  documentId: activeDocumentId.value || '12660000-0000-0000-0000-000000001266',
  goalCode: '',
  goalTitle: '',
  keywordsVi: '',
  keywordsEn: '',
  expectedUnit: '%',
  extractionPrompt: ''
});

function splitKeywords(text) {
  if (!text) return [];
  return text
    .split(/[,;\n]/)
    .map(s => s.trim())
    .filter(Boolean);
}

let searchTimer = null;
function onSearchInput() {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    page.value = 1;
    load();
  }, 300);
}

function onUnitFilterChange() {
  page.value = 1;
  load();
}

// Khi người dùng đổi văn bản ở header, tự động tải lại danh sách từ khóa theo văn bản đó
watch(activeDocumentId, () => {
  page.value = 1;
  load();
});

function resetFilters() {
  searchTerm.value = '';
  unitFilter.value = '';
  page.value = 1;
  load();
}

async function load() {
  loading.value = true;
  error.value = '';
  try {
    const docId = activeDocumentId.value || '12660000-0000-0000-0000-000000001266';
    const params = new URLSearchParams({
      page: String(page.value),
      pageSize: String(pageSize.value),
      search: searchTerm.value.trim(),
      unit: unitFilter.value.trim(),
      documentId: docId
    });

    const res = await fetchWithAuth(getApiUrl(`/api/ai-goal-keywords?${params.toString()}`));
    if (!res.ok) {
      const data = await res.json().catch(() => ({}));
      throw new Error(data.message || `Lỗi tải danh sách từ khóa (HTTP ${res.status})`);
    }
    const data = await res.json();
    items.value = Array.isArray(data.items) ? data.items : [];
    total.value = data.total || 0;
    if (Array.isArray(data.availableGoals)) {
      availableGoals.value = data.availableGoals;
    }
  } catch (err) {
    error.value = err.message || 'Không thể tải danh sách từ khóa AI.';
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  formError.value = '';
  selectedPresetGoal.value = '';
  form.value = {
    id: null,
    documentId: activeDocumentId.value || '12660000-0000-0000-0000-000000001266',
    goalCode: '',
    goalTitle: '',
    keywordsVi: '',
    keywordsEn: '',
    expectedUnit: '%',
    extractionPrompt: ''
  };
  modalOpen.value = true;
}

function openEdit(item) {
  formError.value = '';
  selectedPresetGoal.value = '';
  form.value = {
    id: item.id,
    documentId: item.documentId || activeDocumentId.value || '12660000-0000-0000-0000-000000001266',
    goalCode: item.goalCode || '',
    goalTitle: item.goalTitle || '',
    keywordsVi: item.keywordsVi || '',
    keywordsEn: item.keywordsEn || '',
    expectedUnit: item.expectedUnit || '%',
    extractionPrompt: item.extractionPrompt || ''
  };
  modalOpen.value = true;
}

function closeModal() {
  if (!busy.value) {
    modalOpen.value = false;
    formError.value = '';
  }
}

function applyPresetGoal() {
  if (!selectedPresetGoal.value) return;
  const docId = activeDocumentId.value;
  const found = availableGoals.value.find(g => (!docId || g.documentId === docId) && g.code === selectedPresetGoal.value);
  if (found) {
    form.value.goalCode = found.code;
    form.value.goalTitle = found.title;
    if (found.unit) {
      form.value.expectedUnit = found.unit === 'PERCENT' ? '%' : found.unit;
    }
    if (!form.value.keywordsVi) {
      form.value.keywordsVi = found.title.toLowerCase();
    }
    if (!form.value.extractionPrompt) {
      form.value.extractionPrompt = `Tìm số liệu thực tế đo đúng mục tiêu: ${found.title}. Đơn vị: ${form.value.expectedUnit || '%'}. Không lấy ngưỡng mục tiêu kế hoạch. Chỉ lấy số liệu thực tế đã công bố kèm năm tại Việt Nam.`;
    }
  }
}

async function saveForm() {
  formError.value = '';
  busy.value = true;
  message.value = '';
  try {
    const payload = {
      documentId: form.value.documentId || activeDocumentId.value,
      goalCode: form.value.goalCode,
      goalTitle: form.value.goalTitle,
      keywordsVi: form.value.keywordsVi,
      keywordsEn: form.value.keywordsEn || '',
      expectedUnit: form.value.expectedUnit,
      extractionPrompt: form.value.extractionPrompt
    };

    const isEdit = Boolean(form.value.id);
    const url = isEdit
      ? getApiUrl(`/api/ai-goal-keywords/${form.value.id}`)
      : getApiUrl('/api/ai-goal-keywords');

    const res = await fetchWithAuth(url, {
      method: isEdit ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const data = await res.json().catch(() => ({}));
      throw new Error(data.message || `Lỗi lưu bộ từ khóa (HTTP ${res.status})`);
    }

    message.value = isEdit
      ? `Đã cập nhật bộ từ khóa cho mục tiêu ${payload.goalCode} thành công.`
      : `Đã thêm mới bộ từ khóa cho mục tiêu ${payload.goalCode} thành công.`;
    modalOpen.value = false;
    await load();
  } catch (err) {
    formError.value = err.message || 'Không thể lưu bộ từ khóa.';
  } finally {
    busy.value = false;
  }
}

async function handleDelete(item) {
  const confirmed = await confirmModal({
    title: 'Xóa bộ từ khóa mục tiêu',
    message: `Bạn có chắc chắn muốn xóa bộ từ khóa của mục tiêu "${item.goalCode} · ${item.goalTitle}"? Sau khi xóa, AI sẽ sử dụng tiêu đề mặc định để nhận diện thay vì bộ từ khóa này.`,
    confirmText: 'Xóa bộ từ khóa',
    cancelText: 'Hủy'
  });

  if (!confirmed) return;

  busy.value = true;
  message.value = '';
  error.value = '';
  try {
    const res = await fetchWithAuth(getApiUrl(`/api/ai-goal-keywords/${item.id}`), {
      method: 'DELETE'
    });
    if (!res.ok) {
      const data = await res.json().catch(() => ({}));
      throw new Error(data.message || 'Không thể xóa bộ từ khóa.');
    }
    message.value = `Đã xóa bộ từ khóa của mục tiêu ${item.goalCode}.`;
    await load();
  } catch (err) {
    error.value = err.message || 'Lỗi khi xóa bộ từ khóa.';
  } finally {
    busy.value = false;
  }
}

onMounted(() => {
  load();
});
</script>
