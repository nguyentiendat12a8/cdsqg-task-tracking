<template>
  <div class="w-full space-y-6 font-sans">
    
    <!-- Header Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div>
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 01-2-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          </span>
          <div>
            <h1 class="text-xl font-extrabold text-slate-800">Danh Mục Văn Bản & Quyết Định Chỉ Đạo</h1>
            <p class="text-xs text-slate-500 mt-0.5">Quản lý các chương trình, chiến lược và văn bản chỉ đạo CĐS Quốc gia</p>
          </div>
        </div>
      </div>

      <div class="flex items-center gap-3">
        <button 
          @click="$emit('openLlmImport')"
          class="px-4 py-2.5 bg-purple-50 hover:bg-purple-100 text-purple-700 font-extrabold text-xs rounded-xl border border-purple-200/80 transition flex items-center gap-2 shadow-2xs"
        >
          <svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>
          AI LLM Import JSON
        </button>

        <button 
          @click="isDecisionModalOpen = true" 
          class="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl transition flex items-center gap-2 shadow-sm"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
          Thêm Quyết Định
        </button>
      </div>
    </div>

    <!-- Document Level Summary Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 w-full">
      <div class="bg-white p-5 rounded-2xl shadow-sm border border-slate-200/80">
        <span class="text-xs font-bold text-slate-400 uppercase">Tổng Số Văn Bản Chỉ Đạo</span>
        <div class="text-3xl font-black text-slate-800 mt-1.5">{{ documents.length }}</div>
        <span class="text-xs text-slate-500 mt-1 block">Quyết định / Nghị quyết đang lưu vết</span>
      </div>

      <div class="bg-white p-5 rounded-2xl shadow-sm border border-slate-200/80">
        <span class="text-xs font-bold text-purple-600 uppercase">Tổng Mục Tiêu Chiến Lược</span>
        <div class="text-3xl font-black text-purple-900 mt-1.5">{{ totalGoalsSum }}</div>
        <span class="text-xs text-slate-500 mt-1 block">Mục tiêu tổng thể thuộc các văn bản</span>
      </div>

      <div class="bg-white p-5 rounded-2xl shadow-sm border border-slate-200/80">
        <span class="text-xs font-bold text-blue-600 uppercase">Tổng Nhiệm Vụ Thực Thi</span>
        <div class="text-3xl font-black text-blue-900 mt-1.5">{{ totalTasksSum }}</div>
        <span class="text-xs text-slate-500 mt-1 block">Nhiệm vụ cụ thể giao các Bộ/Ngành</span>
      </div>

      <div class="bg-emerald-50/80 p-5 rounded-2xl border border-emerald-200 shadow-sm">
        <span class="text-xs font-bold text-emerald-800 uppercase">Tiến Độ Trung Bình Hợp Nhất</span>
        <div class="text-3xl font-black text-emerald-700 mt-1.5">{{ avgCompletionRate }}%</div>
        <span class="text-xs font-semibold text-emerald-600 mt-1 block">Tỉ lệ hoàn thành các chỉ tiêu</span>
      </div>
    </div>

    <!-- Unified Card Block: Search + Document Cards Grid + Attached Pagination -->
    <div class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col w-full">
      <!-- Search Bar -->
      <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 p-4 bg-slate-50/60 border-b border-slate-200/80 w-full">
        <div class="flex items-center gap-2 w-full max-w-lg">
          <div class="relative w-full">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              :value="searchInput" 
              @input="searchInput = $event.target.value"
              placeholder="Tìm kiếm theo số hiệu văn bản, trích yếu quyết định..." 
              class="w-full text-xs font-semibold pl-9 pr-4 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
            />
          </div>

          <button 
            @click="resetSearch" 
            class="px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl transition shrink-0"
            title="Đặt lại từ khóa"
          >
            ↺
          </button>
        </div>

        <span class="text-xs text-slate-500 font-bold shrink-0">Tổng cộng {{ totalCount }} văn bản (Trang {{ pageNumber }}/{{ totalPages }})</span>
      </div>

      <!-- Document Cards Grid Content Area -->
      <div class="p-6">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh sách văn bản chỉ đạo..." />

        <div v-else-if="filteredDocuments.length === 0" class="p-12 text-center text-slate-500 font-semibold w-full">
          Chưa có văn bản nào trong hệ thống. Hãy nhấp nút "Thêm Quyết Định" để tạo mới.
        </div>

        <div v-else class="max-h-[calc(100vh-340px)] overflow-y-auto custom-scrollbar p-1">
          <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6 w-full">
            <div 
              v-for="doc in filteredDocuments" 
              :key="doc.id"
              @click="$emit('selectDocument', doc)"
              class="bg-white rounded-2xl p-6 border border-slate-200/80 shadow-sm hover:shadow-lg hover:border-blue-300 cursor-pointer transition-all space-y-4 flex flex-col justify-between group"
            >
              <div class="space-y-3">
                <div class="flex items-start justify-between gap-3">
                  <div class="flex items-center gap-2">
                    <span class="px-2.5 py-1 bg-blue-100 text-blue-900 font-extrabold text-xs rounded-lg group-hover:bg-blue-600 group-hover:text-white transition">
                      {{ doc.documentNumber }}
                    </span>
                    <span class="px-2 py-0.5 bg-slate-100 text-slate-600 font-semibold text-[11px] rounded">
                      Giai đoạn {{ doc.startYear }}-{{ doc.endYear }}
                    </span>
                  </div>

                  <div class="flex items-center gap-1">
                    <button 
                      @click.stop="openEditDocModal(doc)"
                      class="text-slate-400 hover:text-blue-600 transition p-1 rounded-md hover:bg-slate-100"
                      title="Chỉnh sửa văn bản"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                    </button>

                    <button 
                      @click.stop="deleteDoc(doc)"
                      class="text-slate-400 hover:text-rose-600 transition p-1 rounded-md hover:bg-slate-100"
                      title="Xóa văn bản"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                    </button>
                  </div>
                </div>

                <h3 class="text-base font-extrabold text-slate-800 line-clamp-2 leading-snug group-hover:text-blue-600 transition">
                  {{ doc.name }}
                </h3>

                <div class="text-xs text-slate-400 font-medium">
                  Ngày ban hành: <span class="text-slate-700 font-bold">{{ formatDate(doc.issueDate) }}</span>
                </div>

                <!-- Goals & Tasks Badges -->
                <div class="flex items-center gap-3 pt-1">
                  <div class="flex items-center gap-1.5 text-xs font-extrabold text-purple-800 bg-purple-50 border border-purple-100 px-3 py-1.5 rounded-xl">
                    <span>🎯 {{ doc.totalGoals }} Mục tiêu</span>
                  </div>
                  <div class="flex items-center gap-1.5 text-xs font-extrabold text-blue-800 bg-blue-50 border border-blue-100 px-3 py-1.5 rounded-xl">
                    <span>📋 {{ doc.totalTasks }} Nhiệm vụ</span>
                  </div>
                </div>
              </div>

              <!-- Progress Bar & Actions -->
              <div class="border-t border-slate-100 pt-4 space-y-3">
                <div>
                  <div class="flex justify-between text-xs font-bold text-slate-700 mb-1">
                    <span>Tiến độ thực hiện</span>
                    <span class="text-blue-600 font-black">{{ doc.overallCompletionRate }}%</span>
                  </div>
                  <div class="w-full h-2 bg-slate-100 rounded-full overflow-hidden">
                    <div 
                      class="h-full bg-blue-600 rounded-full transition-all duration-500" 
                      :style="{ width: `${doc.overallCompletionRate}%` }"
                    ></div>
                  </div>
                </div>

                <div class="flex items-center justify-between gap-3 pt-1">
                  <a 
                    v-if="doc.attachmentPath" 
                    :href="getApiUrl(doc.attachmentPath)" 
                    target="_blank"
                    @click.stop
                    class="text-xs font-bold text-purple-700 hover:text-purple-900 bg-purple-50 hover:bg-purple-100 border border-purple-200 px-3 py-1.5 rounded-xl transition flex items-center gap-1"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
                    File Đính Kèm
                  </a>
                  <span v-else class="text-xs text-slate-400 italic">Không có file đính kèm</span>

                  <span class="text-xs font-extrabold text-blue-600 group-hover:translate-x-1 transition flex items-center gap-1">
                    Chi tiết & Kế hoạch →
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Attached Server Pagination Controls -->
      <div v-if="totalPages > 1" class="flex flex-col sm:flex-row items-center justify-between bg-slate-50/70 p-4 border-t border-slate-200/80 w-full gap-3 text-xs text-slate-600 font-semibold">
        <span>
          Hiển thị {{ (pageNumber - 1) * pageSize + 1 }} - {{ Math.min(pageNumber * pageSize, totalCount) }} / {{ totalCount }} văn bản
        </span>

        <div class="flex items-center gap-2">
          <button 
            @click="changePage(pageNumber - 1)" 
            :disabled="pageNumber <= 1"
            class="px-3.5 py-1.5 rounded-lg text-xs font-bold bg-white text-slate-700 hover:bg-slate-100 disabled:opacity-40 transition border border-slate-200 shadow-2xs"
          >
            ← Trang Trước
          </button>

          <span class="px-3 py-1.5 rounded-lg text-xs font-extrabold bg-blue-50 text-blue-800 border border-blue-200 shadow-2xs">
            Trang {{ pageNumber }} / {{ totalPages }}
          </span>

          <button 
            @click="changePage(pageNumber + 1)" 
            :disabled="pageNumber >= totalPages"
            class="px-3.5 py-1.5 rounded-lg text-xs font-bold bg-white text-slate-700 hover:bg-slate-100 disabled:opacity-40 transition border border-slate-200 shadow-2xs"
          >
            Trang Sau →
          </button>
        </div>
      </div>
    </div>

    <!-- Decision Form Modal -->
    <DecisionFormModal 
      :isOpen="isDecisionModalOpen"
      :documentToEdit="selectedDocToEdit"
      @close="isDecisionModalOpen = false; selectedDocToEdit = null;"
      @created="onDocumentCreated"
      @updated="onDocumentCreated"
    />

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import DecisionFormModal from '../components/DecisionFormModal.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { getApiUrl } from '../config/api';
import { confirmModal } from '../services/confirm';

const emit = defineEmits(['selectDocument', 'openLlmImport']);

const documents = ref([]);
const isLoading = ref(true);

const searchInput = ref('');
const searchQuery = ref('');

const pageNumber = ref(1);
const pageSize = ref(6);
const totalCount = ref(0);
const totalPages = ref(1);

const isDecisionModalOpen = ref(false);
const selectedDocToEdit = ref(null);

const STORAGE_KEY = 'cdsqg_doc_list_filters';

function loadStoredState() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      const parsed = JSON.parse(saved);
      if (parsed.searchQuery) {
        searchQuery.value = parsed.searchQuery;
        searchInput.value = parsed.searchQuery;
      }
      if (parsed.pageNumber) pageNumber.value = parsed.pageNumber;
    }
  } catch (e) {
    console.error(e);
  }
}

function saveState() {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({
      searchQuery: searchQuery.value,
      pageNumber: pageNumber.value
    }));
  } catch (e) {
    console.error(e);
  }
}

let docFetchRequestId = 0;
let docSearchDebounceTimer = null;

function execSearch() {
  if (docSearchDebounceTimer) clearTimeout(docSearchDebounceTimer);
  docFetchRequestId++;
  searchQuery.value = searchInput.value;
  pageNumber.value = 1;
  saveState();
  fetchDocuments();
}

watch(searchInput, () => {
  if (docSearchDebounceTimer) clearTimeout(docSearchDebounceTimer);
  docSearchDebounceTimer = setTimeout(() => {
    execSearch();
  }, 300);
});

function resetSearch() {
  if (docSearchDebounceTimer) clearTimeout(docSearchDebounceTimer);
  docFetchRequestId++;
  searchInput.value = '';
  searchQuery.value = '';
  pageNumber.value = 1;
  saveState();
  fetchDocuments();
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  pageNumber.value = newPage;
  saveState();
  fetchDocuments();
}

function openEditDocModal(doc) {
  selectedDocToEdit.value = doc;
  isDecisionModalOpen.value = true;
}

const filteredDocuments = computed(() => documents.value);

const totalGoalsSum = computed(() => documents.value.reduce((acc, d) => acc + (d.totalGoals || 0), 0));
const totalTasksSum = computed(() => documents.value.reduce((acc, d) => acc + (d.totalTasks || 0), 0));
const avgCompletionRate = computed(() => {
  if (documents.value.length === 0) return 0;
  const sum = documents.value.reduce((acc, d) => acc + (d.overallCompletionRate || 0), 0);
  return Math.round(sum / documents.value.length);
});

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    return new Date(dateStr).toLocaleDateString('vi-VN');
  } catch {
    return dateStr;
  }
}

async function fetchDocuments() {
  const currentRequestId = ++docFetchRequestId;
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/documents'));
    url.searchParams.append('pageNumber', pageNumber.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (searchQuery.value.trim()) {
      url.searchParams.append('search', searchQuery.value.trim());
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (currentRequestId !== docFetchRequestId) return; // switchMap: ignore stale response
      if (data.items) {
        documents.value = data.items;
        totalCount.value = data.totalCount || data.items.length;
        pageNumber.value = data.pageNumber || 1;
        pageSize.value = data.pageSize || 6;
        totalPages.value = data.totalPages || 1;
      } else {
        documents.value = data;
        totalCount.value = data.length;
        totalPages.value = 1;
      }
    }
  } catch (e) {
    if (currentRequestId !== docFetchRequestId) return;
    console.error('Failed to load documents:', e);
  } finally {
    if (currentRequestId === docFetchRequestId) {
      isLoading.value = false;
    }
  }
}

function onDocumentCreated(newDoc) {
  fetchDocuments();
}

async function deleteDoc(doc) {
  const confirmed = await confirmModal({
    title: 'Xóa văn bản / Quyết định',
    message: `Bạn có chắc chắn muốn xóa văn bản ${doc.documentNumber}? Tất cả mục tiêu và nhiệm vụ liên quan thuộc văn bản này cũng sẽ bị xóa. Thao tác này không thể hoàn tác.`,
    confirmText: 'Xóa văn bản',
    cancelText: 'Hủy bỏ',
    type: 'danger'
  });

  if (!confirmed) return;

  try {
    const res = await fetch(getApiUrl(`/api/documents/${doc.id}`), { method: 'DELETE' });
    if (res.ok) {
      toast.success(`Đã xóa văn bản ${doc.documentNumber}`);
      fetchDocuments();
    } else {
      toast.error('Lỗi khi xóa văn bản.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ khi xóa văn bản.');
  }
}

onMounted(() => {
  loadStoredState();
  fetchDocuments();
});
</script>
