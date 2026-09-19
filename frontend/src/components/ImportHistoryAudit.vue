<template>
  <div class="w-full bg-white rounded-2xl p-6 shadow-sm border border-slate-200/80 space-y-6 font-sans">
    
    <!-- Header -->
    <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between border-b border-slate-100 pb-4 gap-4">
      <div>
        <h2 class="text-lg font-extrabold text-slate-800 flex items-center gap-2">
          📥 Thống Kê Lịch Sử Nạp Dữ Liệu File
        </h2>
        <p class="text-xs text-slate-500 mt-0.5">Theo dõi chi tiết các file dữ liệu (JSON, PDF, Word, Excel) đã nạp vào hệ thống</p>
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <button 
          @click="exportExcel" 
          class="px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer shadow-xs"
          title="Xuất bảng này ra file Excel"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          Xuất Excel
        </button>

        <button 
          @click="loadImportHistory" 
          class="px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition flex items-center gap-1.5 shrink-0 cursor-pointer"
        >
          <svg class="w-4 h-4 text-slate-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>
          Làm Mới
        </button>
      </div>
    </div>

    <!-- Unified Card Container Block -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden w-full flex flex-col">
      
      <!-- 1. Top Header: Search & Filter Bar -->
      <div class="p-3 border-b border-slate-200/80 bg-slate-50/60 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 w-full">
        <div class="flex items-center gap-2 flex-1 max-w-xl">
          <!-- Keyword Search Input -->
          <div class="relative flex-1">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              :value="searchQueryDraft" 
              @input="searchQueryDraft = $event.target.value"
              placeholder="Tìm tên file, loại nạp, người thực hiện, ghi chú..." 
              class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none h-[34px]"
            />
          </div>

          <!-- OverlayPanel Advanced Filter Popover -->
          <OverlayPanel
            title="Lọc Nhật Ký Nạp File"
            buttonText="Lọc Nâng Cao"
            :activeCount="activeFilterCount"
            widthClass="w-[320px] sm:w-[420px]"
            @apply="execSearch"
            @reset="resetSearch"
          >
            <div class="space-y-3">
              <div>
                <SearchableSelect 
                  v-model="selectedCategoryDraft" 
                  :options="importCategoryOptions" 
                  :isMulti="false" 
                  label="Phân Loại Dữ Liệu"
                  placeholder="Tất Cả Phân Loại Dữ Liệu"
                />
              </div>
            </div>
          </OverlayPanel>
        </div>

        <span class="text-xs font-bold text-slate-500 shrink-0">
          Tổng số {{ totalCount }} nhật ký nạp file
        </span>
      </div>

      <!-- 2. Middle Body: Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-380px)] custom-scrollbar flex-1 bg-white">
        <LoadingSpinner v-if="isLoading" text="Đang tải nhật ký nạp dữ liệu..." />

        <div v-else-if="logs.length === 0" class="p-12 text-center text-slate-400 text-xs font-semibold italic">
          Chưa phát sinh nhật ký nạp file dữ liệu nào phù hợp với bộ lọc.
        </div>

        <table v-else class="w-full text-left text-sm text-slate-700 border-collapse table-auto">
          <thead class="bg-slate-100 text-xs text-slate-500 uppercase font-bold border-b border-slate-200 sticky top-0 z-10">
            <tr>
              <th class="px-3 py-3 border-r border-slate-200 text-center w-12 bg-slate-100 shrink-0">STT</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[260px] bg-slate-100">Tên File Dữ Liệu Thực Tế</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[170px] bg-slate-100 whitespace-nowrap">Phân Loại Dữ Liệu</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[120px] bg-slate-100 whitespace-nowrap">Định Dạng</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[140px] bg-slate-100 whitespace-nowrap">Người Thực Hiện</th>
              <th class="px-4 py-3 border-r border-slate-200 min-w-[140px] bg-slate-100 whitespace-nowrap">Thời Gian Nạp</th>
              <th class="px-4 py-3 border-r border-slate-200 text-center min-w-[90px] bg-slate-100 whitespace-nowrap">Mục Tiêu</th>
              <th class="px-4 py-3 border-r border-slate-200 text-center min-w-[90px] bg-slate-100 whitespace-nowrap">Nhiệm Vụ</th>
              <th class="px-4 py-3 border-r border-slate-200 text-center min-w-[110px] bg-slate-100 whitespace-nowrap">Trạng Thái</th>
              <th class="px-4 py-3 min-w-[200px] bg-slate-100">Ghi Chú Chi Tiết</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="(log, idx) in logs" :key="log.id" class="hover:bg-slate-50 transition">
              <td class="px-3 py-3 border-r border-slate-200 text-center text-xs font-extrabold text-slate-500">
                {{ (currentPage - 1) * pageSize + idx + 1 }}
              </td>

              <!-- Clean File Names -->
              <td class="px-4 py-3 border-r border-slate-200">
                <div class="flex flex-col space-y-2">
                  <div 
                    v-for="(fItem, fIdx) in parseCleanFileList(log)" 
                    :key="fIdx"
                    class="flex items-center text-xs font-bold leading-snug"
                  >
                    <a 
                      :href="fItem.url"
                      @click.prevent="downloadFile(fItem.url, fItem.cleanName)"
                      class="inline-flex items-center gap-1.5 text-blue-600 hover:text-blue-800 hover:underline cursor-pointer group transition-colors break-all"
                      :title="`Click để tải về file: ${fItem.cleanName}`"
                    >
                      <span class="text-blue-500 shrink-0">📄</span>
                      <span class="group-hover:underline">{{ fItem.cleanName }}</span>
                      <svg class="w-3.5 h-3.5 text-slate-400 group-hover:text-blue-600 shrink-0 transition-opacity opacity-70 group-hover:opacity-100 ml-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
                      </svg>
                    </a>
                  </div>
                  <span v-if="!log.fileName || parseCleanFileList(log).length === 0" class="text-xs text-slate-400 italic">
                    Không có file đính kèm
                  </span>
                </div>
              </td>

              <!-- Data Category -->
              <td class="px-4 py-3 border-r border-slate-200 text-xs font-bold whitespace-nowrap">
                <span :class="['px-2.5 py-1 rounded-full border text-xs font-extrabold inline-block', getCategoryBadgeClass(log.category || log.fileType)]">
                  {{ getCategoryLabel(log.category || log.fileType) }}
                </span>
              </td>

              <!-- File Format / Type -->
              <td class="px-4 py-3 border-r border-slate-200 text-xs font-bold text-purple-700 whitespace-nowrap">
                <span class="px-2 py-0.5 bg-purple-50 border border-purple-100 rounded text-xs">
                  {{ log.fileType || 'JSON' }}
                </span>
              </td>

              <!-- Imported By -->
              <td class="px-4 py-3 border-r border-slate-200 text-xs font-bold text-slate-700 whitespace-nowrap">
                {{ log.importedBy || 'Chuyên viên' }}
              </td>

              <!-- Imported Date -->
              <td class="px-4 py-3 border-r border-slate-200 text-xs font-medium text-slate-500 whitespace-nowrap">
                {{ formatDate(log.importedAt) }}
              </td>

              <!-- Created Goals -->
              <td class="px-4 py-3 border-r border-slate-200 text-center font-extrabold text-purple-900 text-xs">
                {{ log.totalGoalsCreated }}
              </td>

              <!-- Created Tasks -->
              <td class="px-4 py-3 border-r border-slate-200 text-center font-extrabold text-blue-900 text-xs">
                {{ log.totalTasksCreated }}
              </td>

              <!-- Status -->
              <td class="px-4 py-3 border-r border-slate-200 text-center text-xs whitespace-nowrap">
                <span :class="['px-2.5 py-0.5 rounded-full font-bold', log.status === 'Thành công' ? 'bg-emerald-100 text-emerald-800 border border-emerald-200' : 'bg-rose-100 text-rose-800 border border-rose-200']">
                  {{ log.status }}
                </span>
              </td>

              <!-- Notes -->
              <td class="px-4 py-3 text-xs text-slate-600 leading-relaxed">
                {{ log.summaryNotes || 'Khởi tạo thành công' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 3. Bottom Footer: Server Pagination Controls -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div>
          Hiển thị <span class="font-extrabold text-slate-900">{{ logs.length > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, totalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalCount }}</span> nhật ký nạp dữ liệu
        </div>

        <div class="flex items-center gap-2">
          <button 
            @click="changePage(currentPage - 1)" 
            :disabled="currentPage <= 1"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-xs"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-black">
            Trang {{ currentPage }} / {{ totalPages }}
          </span>

          <button 
            @click="changePage(currentPage + 1)" 
            :disabled="currentPage >= totalPages"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-xs"
          >
            Trang sau ›
          </button>
        </div>
      </div>

    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import SearchableSelect from './SearchableSelect.vue';
import { exportToExcel } from '../utils/excelExport';
import LoadingSpinner from './LoadingSpinner.vue';
import OverlayPanel from './OverlayPanel.vue';
import { getApiUrl } from '../config/api';

const importCategoryOptions = ref([
  { value: 'Minh chứng quyết định', label: 'Minh chứng quyết định' },
  { value: 'Minh chứng báo cáo tiến độ', label: 'Minh chứng báo cáo tiến độ' },
  { value: 'Nhập liệu AI LLM JSON', label: 'Nhập liệu AI LLM JSON' },
  { value: 'Khác', label: 'Khác' }
]);

const STORAGE_KEY = 'cdsqg_import_history_query';

const logs = ref([]);
const isLoading = ref(true);

const searchQueryDraft = ref('');
const appliedSearchQuery = ref('');

const selectedCategoryDraft = ref('');
const appliedCategory = ref('');

const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedCategoryDraft.value) count++;
  return count;
});

const currentPage = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

function parseCleanFileList(log) {
  const raw = log?.fileName;
  if (!raw) return [];
  
  const parts = raw.split(/[,;]/).map(p => p.trim()).filter(p => p.length > 0);
  const result = [];
  const seenCleanNames = new Set();

  for (const p of parts) {
    let rawFileName = p.split(/[/\\]/).pop() || p;
    let cleanName = rawFileName.replace(/^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}_/i, '');

    // Deduplicate duplicate clean file names within the same import record
    if (seenCleanNames.has(cleanName)) {
      continue;
    }
    seenCleanNames.add(cleanName);

    let url = '';
    if (p.startsWith('http://') || p.startsWith('https://')) {
      url = p;
    } else if (p.startsWith('/uploads/') || p.startsWith('uploads/')) {
      const cleanPath = p.startsWith('/') ? p : '/' + p;
      url = getApiUrl(cleanPath);
    } else {
      const cat = log?.category || log?.fileType || '';
      const folder = (cat.includes('tiến độ') || cat === 'File Minh chứng tiến độ') ? 'evidence' : 'documents';
      url = getApiUrl(`/uploads/${folder}/${rawFileName}`);
    }

    result.push({
      cleanName,
      rawFileName,
      url
    });
  }

  return result;
}

async function downloadFile(url, fileName) {
  if (!url || url === '#') return;
  try {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    const blob = await response.blob();
    const blobUrl = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = blobUrl;
    a.download = fileName || 'download';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(blobUrl);
  } catch (err) {
    console.warn('Direct blob download failed, opening in new tab:', err);
    window.open(url, '_blank');
  }
}

function getCategoryLabel(cat) {
  if (!cat) return 'Minh chứng quyết định';
  if (cat.includes('quyết định') || cat === 'Văn bản Quyết định' || cat === 'Document_PDF') return 'Minh chứng quyết định';
  if (cat.includes('tiến độ') || cat === 'File Minh chứng tiến độ') return 'Minh chứng báo cáo tiến độ';
  if (cat.includes('LLM') || cat.includes('JSON')) return 'Nhập liệu AI LLM JSON';
  return cat;
}

function getCategoryBadgeClass(cat) {
  const label = getCategoryLabel(cat);
  if (label === 'Minh chứng quyết định') return 'bg-blue-50 text-blue-800 border-blue-200';
  if (label === 'Minh chứng báo cáo tiến độ') return 'bg-amber-50 text-amber-800 border-amber-200';
  if (label === 'Nhập liệu AI LLM JSON') return 'bg-purple-50 text-purple-800 border-purple-200';
  return 'bg-slate-100 text-slate-700 border-slate-200';
}

function loadSavedQuery() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      const parsed = JSON.parse(saved);
      if (parsed.search) {
        searchQueryDraft.value = parsed.search;
        appliedSearchQuery.value = parsed.search;
      }
      if (parsed.category) {
        selectedCategoryDraft.value = parsed.category;
        appliedCategory.value = parsed.category;
      }
    }
  } catch (e) {
    console.error('Error reading localStorage:', e);
  }
}

function saveState() {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({
      search: appliedSearchQuery.value,
      category: appliedCategory.value
    }));
  } catch (e) {
    console.error('Error saving localStorage:', e);
  }
}

let importFetchRequestId = 0;
let importSearchDebounceTimer = null;

function execSearch() {
  if (importSearchDebounceTimer) clearTimeout(importSearchDebounceTimer);
  importFetchRequestId++;
  appliedSearchQuery.value = searchQueryDraft.value;
  appliedCategory.value = selectedCategoryDraft.value;
  currentPage.value = 1;
  saveState();
  loadImportHistory();
}

watch(searchQueryDraft, () => {
  if (importSearchDebounceTimer) clearTimeout(importSearchDebounceTimer);
  importSearchDebounceTimer = setTimeout(() => {
    execSearch();
  }, 300);
});

function resetSearch() {
  if (importSearchDebounceTimer) clearTimeout(importSearchDebounceTimer);
  importFetchRequestId++;
  searchQueryDraft.value = '';
  appliedSearchQuery.value = '';
  selectedCategoryDraft.value = '';
  appliedCategory.value = '';
  currentPage.value = 1;
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch (e) {
    console.error('Error removing localStorage:', e);
  }
  loadImportHistory();
}

function exportExcel() {
  const headers = ["STT", "Tên File Dữ Liệu Thực Tế", "Phân Loại Dữ Liệu", "Định Dạng", "Người Thực Hiện", "Thời Gian Nạp", "Mục Tiêu Tạo", "Nhiệm Vụ Tạo", "Trạng Thái", "Ghi Chú Chi Tiết"];
  const minColWidths = { 0: 8, 1: 45, 2: 25, 3: 12, 4: 20, 5: 20, 6: 15, 7: 15, 8: 18, 9: 50 };

  const rows = logs.value.map((log, idx) => {
    const cleanFiles = parseCleanFileList(log).map(f => f.cleanName).join("; ");
    return [
      idx + 1,
      cleanFiles || log.fileName || '',
      getCategoryLabel(log.category || log.fileType),
      log.fileType || 'JSON',
      log.importedBy || 'Chuyên viên',
      formatDate(log.importedAt),
      log.totalGoalsCreated || 0,
      log.totalTasksCreated || 0,
      log.status || '',
      log.summaryNotes || ''
    ];
  });

  exportToExcel({
    title: "THỐNG KÊ LỊCH SỬ NẠP DỮ LIỆU FILE HỆ THỐNG",
    headers,
    rows,
    fileName: "Thong_Ke_Lich_Su_Nap_Du_Lieu",
    sheetName: "Lịch sử nạp dữ liệu",
    minColWidths
  });
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  currentPage.value = newPage;
  loadImportHistory();
}

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    const d = new Date(dateStr);
    return `${d.toLocaleDateString('vi-VN')} ${d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
  } catch {
    return dateStr;
  }
}

async function loadImportHistory() {
  const currentRequestId = ++importFetchRequestId;
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/documents/import-history'));
    url.searchParams.append('pageNumber', currentPage.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (appliedSearchQuery.value.trim()) {
      url.searchParams.append('search', appliedSearchQuery.value.trim());
    }
    if (appliedCategory.value) {
      url.searchParams.append('category', appliedCategory.value);
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (currentRequestId !== importFetchRequestId) return;
      if (data.items) {
        logs.value = data.items;
        totalCount.value = data.totalCount || data.items.length;
        currentPage.value = data.pageNumber || 1;
        pageSize.value = data.pageSize || 10;
        totalPages.value = data.totalPages || 1;
      } else {
        logs.value = data;
        totalCount.value = data.length;
        totalPages.value = 1;
      }
    }
  } catch (e) {
    if (currentRequestId !== importFetchRequestId) return;
    console.error('Failed to load import history:', e);
  } finally {
    if (currentRequestId === importFetchRequestId) {
      isLoading.value = false;
    }
  }
}

onMounted(() => {
  loadSavedQuery();
  loadImportHistory();
});
</script>
