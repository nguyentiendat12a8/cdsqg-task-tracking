<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Top Header Bar -->
    <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3 w-full">
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm font-bold text-sm">
            📜
          </span>
          <div>
            <h2 class="text-sm sm:text-base font-bold text-slate-800">
              Hệ thống văn bản chuyển đổi số
            </h2>
            <p class="text-xs text-slate-500">Lưu trữ, đính kèm tệp và theo dõi hiệu lực văn bản pháp luật</p>
          </div>
        </div>

        <div class="flex items-center gap-2 flex-wrap">
          <button 
            @click="exportExcel"
            class="px-3.5 py-2 text-slate-700 hover:text-slate-900 font-bold text-xs rounded-xl bg-slate-100 hover:bg-slate-200 border border-slate-200/80 transition shadow-2xs flex items-center gap-1.5 cursor-pointer"
            title="Xuất danh sách văn bản QPPL ra Excel"
          >
            <svg class="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
            <span>Xuất Báo Cáo Excel</span>
          </button>

          <button 
            v-if="authState.isAdmin.value"
            @click="triggerImportExcel"
            :disabled="isImporting"
            class="px-3.5 py-2 text-slate-700 hover:text-slate-900 font-bold text-xs rounded-xl bg-slate-100 hover:bg-slate-200 border border-slate-200/80 transition shadow-2xs flex items-center gap-1.5 cursor-pointer disabled:opacity-50"
            title="Nhập danh sách văn bản QPPL từ file Excel"
          >
            <svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/></svg>
            <span>{{ isImporting ? 'Đang Import...' : 'Import Excel' }}</span>
          </button>
          <input 
            type="file" 
            ref="importFileInputRef" 
            @change="handleImportExcelFile" 
            accept=".xlsx, .xls" 
            class="hidden" 
          />

          <button 
            v-if="authState.isAdmin.value"
            @click="openCreateModal"
            class="px-3.5 py-2 text-white font-bold text-xs rounded-xl bg-blue-600 hover:bg-blue-700 transition shadow-sm flex items-center gap-1.5 cursor-pointer"
          >
            + Thêm Văn Bản QPPL Mới
          </button>
        </div>
      </div>
    </div>

    <!-- Main Card Container: Filter + Table + Pagination -->
    <div class="bg-white rounded-2xl shadow-sm border border-slate-200/80 overflow-hidden flex flex-col w-full">
      
      <!-- Filter & Search Bar -->
      <div class="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3 p-3 bg-slate-50/60 border-b border-slate-200/80 w-full">
        <div class="flex items-center gap-2 flex-1 max-w-xl min-w-0">
          <!-- Search Input -->
          <div class="relative flex-1 min-w-[200px]">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              v-model="filterDraft.searchQuery" 
              @keyup.enter="execSearch"
              placeholder="Tìm theo số ký hiệu, trích yếu, người ký, cơ quan..." 
              class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none h-[34px]"
            />
          </div>

          <!-- OverlayPanel Advanced Filter -->
          <OverlayPanel 
            title="Bộ Lọc Tìm Kiếm Nâng Cao"
            buttonText="Lọc Nâng Cao"
            :activeCount="activeFilterCount"
            widthClass="w-[340px] sm:w-[500px]"
            @apply="execSearch"
            @reset="resetSearch"
          >
            <div class="space-y-3">
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <SearchableSelect 
                    v-model="filterDraft.documentType" 
                    :options="documentTypeFilterOptions" 
                    :isMulti="false" 
                    label="Loại Văn Bản" 
                    placeholder="Tất cả loại"
                  />
                </div>

                <div>
                  <SearchableSelect 
                    v-model="filterDraft.effectStatus" 
                    :options="effectStatusFilterOptions" 
                    :isMulti="false" 
                    label="Trạng Thái Hiệu Lực" 
                    placeholder="Tất cả trạng thái"
                  />
                </div>
              </div>

              <div>
                <SearchableSelect 
                  v-model="filterDraft.field" 
                  :options="fieldFilterOptions" 
                  :isMulti="false" 
                  label="Lĩnh Vực / Nhóm CĐS" 
                  placeholder="Tất cả lĩnh vực"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="filterDraft.issuingAgencyId" 
                  :options="agencyFilterOptions" 
                  :isMulti="false" 
                  label="Cơ Quan Ban Hành" 
                  placeholder="Tất cả cơ quan ban hành"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="filterDraft.draftingAgencyId" 
                  :options="agencyFilterOptions" 
                  :isMulti="false" 
                  label="Cơ Quan Dự Thảo" 
                  placeholder="Tất cả cơ quan dự thảo"
                />
              </div>

              <div>
                <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wider block mb-1">Thời Gian Ban Hành (Từ ngày ➔ Đến ngày)</label>
                <div class="flex items-center gap-2">
                  <input type="date" v-model="filterDraft.fromDate" class="w-full text-xs font-semibold p-2 bg-white border border-slate-200 rounded-xl" />
                  <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                  <input type="date" v-model="filterDraft.toDate" class="w-full text-xs font-semibold p-2 bg-white border border-slate-200 rounded-xl" />
                </div>
              </div>
            </div>
          </OverlayPanel>
        </div>

        <span class="text-xs text-slate-500 font-bold shrink-0">
          Tổng số {{ totalCount }} văn bản QPPL
        </span>
      </div>

      <!-- Data Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-330px)] custom-scrollbar w-full">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh sách văn bản quy phạm pháp luật..." />

        <table v-else class="w-full min-w-[1100px] text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-10 shadow-2xs">
            <tr>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[130px] bg-slate-100">Số Ký Hiệu</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[280px] bg-slate-100">Trích Yếu Nội Dung</th>
              <th class="px-3.5 py-3 border-r border-slate-200 text-center min-w-[110px] bg-slate-100 whitespace-nowrap">Loại VB</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[180px] bg-slate-100">Cơ Quan Ban Hành</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[180px] bg-slate-100">Cơ Quan Dự Thảo</th>
              <th class="px-3.5 py-3 border-r border-slate-200 text-center min-w-[110px] bg-slate-100 whitespace-nowrap">Ngày Ban Hành</th>
              <th class="px-3.5 py-3 border-r border-slate-200 text-center min-w-[145px] bg-slate-100 whitespace-nowrap">Trạng Thái</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[200px] bg-slate-100">File Đính Kèm</th>
              <th class="px-3.5 py-3 text-center min-w-[100px] bg-slate-100 whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-xs">
            <tr v-for="item in documents" :key="item.id" class="hover:bg-blue-50/40 transition">
              <!-- Số ký hiệu -->
              <td class="px-3.5 py-3 border-r border-slate-200 font-normal text-blue-900 whitespace-nowrap">
                <span class="px-2 py-1 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-medium">
                  {{ item.code }}
                </span>
              </td>

              <!-- Trích yếu -->
              <td class="px-3.5 py-3 border-r border-slate-200 font-normal text-slate-800 leading-relaxed">
                <div class="line-clamp-2" :title="item.title">
                  {{ item.title }}
                </div>
                <div v-if="item.signerName" class="text-[11px] text-slate-500 mt-0.5 truncate">
                  ✍️ {{ item.signerTitle ? `${item.signerTitle}: ` : '' }}{{ item.signerName }}
                </div>
              </td>

              <!-- Loại VB -->
              <td class="px-3.5 py-3 border-r border-slate-200 text-center whitespace-nowrap">
                <span class="px-2.5 py-0.5 rounded-full text-[11px] font-medium bg-slate-100 text-slate-700 border border-slate-200">
                  {{ item.documentType }}
                </span>
              </td>

              <!-- Cơ quan ban hành -->
              <td class="px-3.5 py-3 border-r border-slate-200 font-normal text-slate-700">
                {{ item.issuingAgencyName || '—' }}
              </td>

              <!-- Cơ quan dự thảo -->
              <td class="px-3.5 py-3 border-r border-slate-200 font-normal text-slate-700">
                {{ item.draftingAgencyName || '—' }}
              </td>

              <!-- Ngày ban hành -->
              <td class="px-3.5 py-3 border-r border-slate-200 text-center font-normal text-slate-600 whitespace-nowrap">
                {{ formatDate(item.issuedDate) }}
              </td>

              <!-- Trạng thái -->
              <td class="px-3.5 py-3 border-r border-slate-200 text-center whitespace-nowrap">
                <span :class="['px-2.5 py-0.5 rounded-full text-[11px] font-medium inline-block', getStatusBadgeClass(item.effectStatus)]">
                  {{ item.effectStatus }}
                </span>
              </td>

              <!-- File đính kèm -->
              <td class="px-3.5 py-3 border-r border-slate-200">
                <div v-if="item.attachments && item.attachments.length > 0" class="space-y-1">
                  <div 
                    v-for="(att, fIdx) in item.attachments" 
                    :key="fIdx"
                    class="flex items-center gap-1.5"
                  >
                    <button 
                      @click="openFileViewer(att)"
                      class="inline-flex items-center gap-1.5 text-blue-600 hover:text-blue-800 hover:underline cursor-pointer group text-xs text-left"
                      :title="`Click để xem trực tiếp file: ${att.cleanName || att.fileName}`"
                    >
                      <span class="text-blue-500">📄</span>
                      <span class="truncate max-w-[150px] font-normal">{{ att.cleanName || att.fileName }}</span>
                    </button>
                    <span class="text-[10px] text-slate-400">({{ att.fileType }})</span>
                  </div>
                </div>
                <span v-else class="text-slate-400 italic text-[11px]">Không có file</span>
              </td>

              <!-- Thao tác -->
              <td class="px-3.5 py-3 text-center whitespace-nowrap">
                <div class="inline-flex items-center justify-center gap-1">
                  <button 
                    @click="openViewModal(item)" 
                    class="p-1.5 text-slate-600 hover:text-blue-600 hover:bg-slate-100 rounded-lg transition inline-flex items-center cursor-pointer" 
                    title="Xem chi tiết văn bản"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                  </button>
                  <button 
                    v-if="authState.isAdmin.value"
                    @click="openEditModal(item)" 
                    class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition inline-flex items-center cursor-pointer" 
                    title="Chỉnh sửa văn bản"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                  </button>
                  <button 
                    v-if="authState.isAdmin.value"
                    @click="handleDelete(item)" 
                    class="p-1.5 text-rose-600 hover:text-rose-800 hover:bg-rose-50 rounded-lg transition inline-flex items-center cursor-pointer" 
                    title="Xóa văn bản"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="documents.length === 0">
              <td colspan="9" class="p-8 text-center text-slate-400 font-semibold italic">
                Chưa có văn bản quy phạm pháp luật nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Bar -->
      <div class="flex flex-col md:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold w-full">
        <div class="flex items-center gap-3 whitespace-nowrap flex-wrap justify-center sm:justify-start">
          <span class="whitespace-nowrap">Hiển thị <span class="font-bold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ totalCount }}</span> văn bản QPPL</span>
          
          <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3 whitespace-nowrap">
            <span class="whitespace-nowrap">Số bản ghi/trang:</span>
            <SearchableSelect 
              v-model="pageSize" 
              :options="[10, 25, 50, 100].map(n => ({ value: n, label: String(n) }))" 
              :isMulti="false" 
              :clearable="false" 
              @change="execSearch" 
              class="w-20"
            />
          </div>
        </div>

        <div class="flex items-center gap-2 shrink-0 whitespace-nowrap flex-wrap justify-center">
          <button 
            @click="changePage(pageNumber - 1)" 
            :disabled="pageNumber <= 1"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-bold">
            Trang {{ pageNumber }} / {{ Math.max(1, totalPages) }}
          </span>

          <button 
            @click="changePage(pageNumber + 1)" 
            :disabled="pageNumber >= totalPages"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
          >
            Trang sau ›
          </button>
        </div>
      </div>

    </div>

    <!-- Modals -->
    <LegalDocumentModal 
      :isOpen="isModalOpen" 
      :editingDocument="selectedDocument" 
      :agencies="agencies"
      :readOnly="isReadOnlyModal"
      @close="isModalOpen = false" 
      @saved="fetchDocuments" 
    />

    <LegalFileViewerModal 
      :isOpen="isFileViewerOpen" 
      :fileItem="selectedFileForViewing" 
      @close="isFileViewerOpen = false" 
    />

  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import XLSX from 'xlsx-js-style';
import SearchableSelect from '../components/SearchableSelect.vue';
import OverlayPanel from '../components/OverlayPanel.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import LegalDocumentModal from '../components/LegalDocumentModal.vue';
import LegalFileViewerModal from '../components/LegalFileViewerModal.vue';
import { exportFormattedReportExcel, styleWorksheet } from '../utils/excelExport';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';
import { openFileInNewWindow } from '../utils/fileViewer';
import { confirmModal } from '../services/confirm';

const documents = ref([]);
const agencies = ref([]);
const isLoading = ref(true);

const importFileInputRef = ref(null);
const isImporting = ref(false);

function triggerImportExcel() {
  if (importFileInputRef.value) {
    importFileInputRef.value.value = '';
    importFileInputRef.value.click();
  }
}

function parseExcelDateStr(val) {
  if (!val) return null;
  const valStr = String(val).trim();
  if (!valStr || valStr === '—') return null;

  const ddmmyyyy = valStr.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
  if (ddmmyyyy) {
    const day = parseInt(ddmmyyyy[1], 10);
    const month = parseInt(ddmmyyyy[2], 10) - 1;
    const year = parseInt(ddmmyyyy[3], 10);
    const d = new Date(Date.UTC(year, month, day));
    if (!isNaN(d.getTime())) return d.toISOString();
  }

  const yyyymmdd = valStr.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/);
  if (yyyymmdd) {
    const year = parseInt(yyyymmdd[1], 10);
    const month = parseInt(yyyymmdd[2], 10) - 1;
    const day = parseInt(yyyymmdd[3], 10);
    const d = new Date(Date.UTC(year, month, day));
    if (!isNaN(d.getTime())) return d.toISOString();
  }

  const d = new Date(valStr);
  if (!isNaN(d.getTime())) return d.toISOString();
  return null;
}

async function handleImportExcelFile(event) {
  const file = event.target.files?.[0];
  if (!file) return;

  isImporting.value = true;
  try {
    const arrayBuffer = await file.arrayBuffer();
    const workbook = XLSX.read(arrayBuffer, { type: 'array' });
    const sheetName = workbook.SheetNames[0];
    const worksheet = workbook.Sheets[sheetName];
    if (!worksheet) {
      toast.error('File Excel không chứa dữ liệu.');
      return;
    }

    const rawRows = XLSX.utils.sheet_to_json(worksheet, { header: 1, raw: false, defval: '' });
    if (!rawRows || rawRows.length === 0) {
      toast.error('File Excel rỗng.');
      return;
    }

    let headerRowIdx = -1;
    let colMap = {
      code: -1,
      title: -1,
      type: -1,
      issuing: -1,
      drafting: -1,
      signer: -1,
      issuedDate: -1,
      effectiveDate: -1,
      status: -1,
      field: -1
    };

    for (let r = 0; r < Math.min(rawRows.length, 20); r++) {
      const row = rawRows[r] || [];
      const rowStr = row.map(c => String(c || '')).join(' ').toLowerCase();
      if (rowStr.includes('số ký hiệu') || rowStr.includes('trích yếu')) {
        headerRowIdx = r;
        row.forEach((cellVal, cIdx) => {
          const cStr = String(cellVal || '').trim().toLowerCase();
          if (cStr.includes('số ký hiệu') || cStr === 'mã') colMap.code = cIdx;
          else if (cStr.includes('trích yếu') || cStr.includes('tên văn bản')) colMap.title = cIdx;
          else if (cStr.includes('loại vb') || cStr.includes('loại văn bản')) colMap.type = cIdx;
          else if (cStr.includes('cơ quan ban hành')) colMap.issuing = cIdx;
          else if (cStr.includes('cơ quan dự thảo')) colMap.drafting = cIdx;
          else if (cStr.includes('người ký')) colMap.signer = cIdx;
          else if (cStr.includes('ngày ban hành')) colMap.issuedDate = cIdx;
          else if (cStr.includes('ngày hiệu lực')) colMap.effectiveDate = cIdx;
          else if (cStr.includes('trạng thái')) colMap.status = cIdx;
          else if (cStr.includes('lĩnh vực')) colMap.field = cIdx;
        });
        break;
      }
    }

    if (headerRowIdx === -1) {
      headerRowIdx = 0;
      colMap = {
        code: 1,
        title: 2,
        type: 3,
        issuing: 4,
        drafting: 5,
        signer: 6,
        issuedDate: 7,
        effectiveDate: 8,
        status: 9,
        field: 10
      };
    }

    const items = [];
    for (let r = headerRowIdx + 1; r < rawRows.length; r++) {
      const row = rawRows[r] || [];
      const codeVal = colMap.code >= 0 ? String(row[colMap.code] || '').trim() : '';
      const titleVal = colMap.title >= 0 ? String(row[colMap.title] || '').trim() : '';
      const typeVal = colMap.type >= 0 ? String(row[colMap.type] || '').trim() : 'Quyết định';
      const issuingVal = colMap.issuing >= 0 ? String(row[colMap.issuing] || '').trim() : '';
      const draftingVal = colMap.drafting >= 0 ? String(row[colMap.drafting] || '').trim() : '';
      const signerVal = colMap.signer >= 0 ? String(row[colMap.signer] || '').trim() : '';
      const issuedDateVal = colMap.issuedDate >= 0 ? row[colMap.issuedDate] : null;
      const effectiveDateVal = colMap.effectiveDate >= 0 ? row[colMap.effectiveDate] : null;
      const statusVal = colMap.status >= 0 ? String(row[colMap.status] || '').trim() : 'Còn hiệu lực';
      const fieldVal = colMap.field >= 0 ? String(row[colMap.field] || '').trim() : 'Thể chế số';

      if (!codeVal && !titleVal) continue;

      let signerName = signerVal;
      let signerTitle = '';
      if (signerVal && signerVal.includes('(') && signerVal.endsWith(')')) {
        const match = signerVal.match(/^(.*?)\s*\((.*?)\)$/);
        if (match) {
          signerName = match[1].trim();
          signerTitle = match[2].trim();
        }
      }

      items.push({
        rowIndex: r,
        code: codeVal,
        title: titleVal,
        documentType: typeVal && typeVal !== '—' ? typeVal : 'Quyết định',
        issuingAgencyName: issuingVal !== '—' ? issuingVal : '',
        draftingAgencyName: draftingVal !== '—' ? draftingVal : '',
        signerName: signerName !== '—' ? signerName : '',
        signerTitle: signerTitle !== '—' ? signerTitle : '',
        issuedDate: parseExcelDateStr(issuedDateVal),
        effectiveDate: parseExcelDateStr(effectiveDateVal),
        effectStatus: statusVal && statusVal !== '—' ? statusVal : 'Còn hiệu lực',
        field: fieldVal && fieldVal !== '—' ? fieldVal : 'Thể chế số',
        scope: 'Toàn quốc',
        notes: ''
      });
    }

    if (items.length === 0) {
      toast.warning('Không tìm thấy dòng dữ liệu văn bản nào trong file Excel.');
      return;
    }

    const userRoleStr = authState.user.value?.role || (authState.isAdmin.value ? 'Admin' : 'Level2');
    const res = await fetch(getApiUrl(`/api/legaldocuments/bulk-import?userRole=${encodeURIComponent(userRoleStr)}`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ items })
    });

    const resData = await res.json().catch(() => ({}));

    if (res.ok && resData.success) {
      toast.success(resData.message || `Đã nhập thành công ${resData.importedCount || items.length} văn bản QPPL!`);
      await fetchDocuments();
    } else if (resData.errors && resData.errors.length > 0) {
      const errorMap = {};
      resData.errors.forEach(e => {
        errorMap[e.rowIndex] = e.errorDetail;
      });

      const range = XLSX.utils.decode_range(worksheet['!ref'] || 'A1');
      let errColIdx = -1;

      for (let c = range.s.c; c <= range.e.c; c++) {
        const cell = worksheet[XLSX.utils.encode_cell({ r: headerRowIdx, c })];
        const valStr = cell?.v ? String(cell.v).toLowerCase() : '';
        if (valStr.includes('chi tiết lỗi') || valStr.includes('lỗi import')) {
          errColIdx = c;
          break;
        }
      }

      if (errColIdx === -1) {
        errColIdx = range.e.c + 1;
        range.e.c = errColIdx;
        worksheet['!ref'] = XLSX.utils.encode_range(range);
      }

      // 1. Format the entire worksheet with standard table theme (navy headers, zebra striping, thin borders)
      styleWorksheet(worksheet, {
        numCols: range.e.c + 1,
        headerRowIndex: headerRowIdx,
        titleRowIndex: 0
      });

      // 2. Dark Red Header style for "CHI TIẾT LỖI IMPORT" column
      const headerCellRef = XLSX.utils.encode_cell({ r: headerRowIdx, c: errColIdx });
      worksheet[headerCellRef] = {
        t: 's',
        v: 'CHI TIẾT LỖI IMPORT',
        s: {
          font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: 'FFFFFF' } },
          fill: { fgColor: { rgb: '991B1B' } }, // Dark Red
          alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
          border: {
            top: { style: 'medium', color: { rgb: '7F1D1D' } },
            bottom: { style: 'medium', color: { rgb: '7F1D1D' } },
            left: { style: 'thin', color: { rgb: '991B1B' } },
            right: { style: 'thin', color: { rgb: '991B1B' } }
          }
        }
      };

      // 3. Format error detail cells and valid row cells
      for (let r = headerRowIdx + 1; r <= range.e.r; r++) {
        const errCellRef = XLSX.utils.encode_cell({ r, c: errColIdx });
        if (errorMap[r]) {
          worksheet[errCellRef] = {
            t: 's',
            v: errorMap[r],
            s: {
              font: { name: 'Segoe UI', sz: 10, bold: true, color: { rgb: 'B91C1C' } },
              fill: { fgColor: { rgb: 'FEE2E2' } }, // Soft Red
              alignment: { horizontal: 'left', vertical: 'center', wrapText: true },
              border: {
                top: { style: 'thin', color: { rgb: 'FCA5A5' } },
                bottom: { style: 'thin', color: { rgb: 'FCA5A5' } },
                left: { style: 'thin', color: { rgb: 'FCA5A5' } },
                right: { style: 'thin', color: { rgb: 'FCA5A5' } }
              }
            }
          };
        } else {
          const hasData = rawRows[r] && (rawRows[r][colMap.code] || rawRows[r][colMap.title]);
          if (hasData) {
            worksheet[errCellRef] = {
              t: 's',
              v: 'Hợp lệ',
              s: {
                font: { name: 'Segoe UI', sz: 10, color: { rgb: '15803D' } },
                fill: { fgColor: { rgb: 'F0FDF4' } }, // Soft Green
                alignment: { horizontal: 'center', vertical: 'center' },
                border: {
                  top: { style: 'thin', color: { rgb: 'CBD5E1' } },
                  bottom: { style: 'thin', color: { rgb: 'CBD5E1' } },
                  left: { style: 'thin', color: { rgb: 'CBD5E1' } },
                  right: { style: 'thin', color: { rgb: 'CBD5E1' } }
                }
              }
            };
          }
        }
      }

      // 4. Calculate dynamic column widths for all columns
      const colWidths = [];
      for (let c = range.s.c; c <= range.e.c; c++) {
        let maxLen = 10;
        for (let r = headerRowIdx; r <= range.e.r; r++) {
          const cellVal = worksheet[XLSX.utils.encode_cell({ r, c })]?.v;
          if (cellVal !== undefined && cellVal !== null) {
            const len = String(cellVal).length;
            if (len > maxLen) maxLen = len;
          }
        }
        let width = Math.min(Math.max(maxLen + 4, 12), 70);
        if (c === errColIdx) width = Math.max(width, 45);
        colWidths.push({ wch: width });
      }
      worksheet['!cols'] = colWidths;

      const dateStr = new Date().toISOString().slice(0, 10);
      XLSX.writeFile(workbook, `File_Import_VBQPPL_Loi_${dateStr}.xlsx`);

      toast.error(`Import thất bại! Phát hiện ${resData.errors.length} dòng bị lỗi. File chi tiết lỗi đã được tự động tải xuống.`);
    } else {
      toast.error(resData.message || 'Lỗi khi import file Excel.');
    }

  } catch (err) {
    console.error('Error importing Excel:', err);
    toast.error(err.message || 'Đã xảy ra lỗi khi xử lý file Excel.');
  } finally {
    isImporting.value = false;
    if (importFileInputRef.value) importFileInputRef.value.value = '';
  }
}

const pageNumber = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const isModalOpen = ref(false);
const isReadOnlyModal = ref(false);
const selectedDocument = ref(null);

const isFileViewerOpen = ref(false);
const selectedFileForViewing = ref(null);

const filterDraft = reactive({
  searchQuery: '',
  documentType: '',
  effectStatus: '',
  field: '',
  issuingAgencyId: null,
  draftingAgencyId: null,
  fromDate: '',
  toDate: ''
});

const activeFilterCount = computed(() => {
  let count = 0;
  if (filterDraft.documentType) count++;
  if (filterDraft.effectStatus) count++;
  if (filterDraft.field) count++;
  if (filterDraft.issuingAgencyId) count++;
  if (filterDraft.draftingAgencyId) count++;
  if (filterDraft.fromDate || filterDraft.toDate) count++;
  return count;
});

const documentTypeFilterOptions = [
  { value: 'Luật', label: 'Luật' },
  { value: 'Nghị định', label: 'Nghị định' },
  { value: 'Thông tư', label: 'Thông tư' },
  { value: 'Quyết định', label: 'Quyết định' },
  { value: 'Nghị quyết', label: 'Nghị quyết' },
  { value: 'Chỉ thị', label: 'Chỉ thị' },
  { value: 'Khác', label: 'Khác' }
];

const effectStatusFilterOptions = [
  { value: 'Còn hiệu lực', label: 'Còn hiệu lực' },
  { value: 'Hết hiệu lực toàn bộ', label: 'Hết hiệu lực toàn bộ' },
  { value: 'Hết hiệu lực một phần', label: 'Hết hiệu lực một phần' },
  { value: 'Chưa có hiệu lực', label: 'Chưa có hiệu lực' }
];

const fieldFilterOptions = [
  { value: 'Thể chế số', label: 'Thể chế số' },
  { value: 'Chính phủ số', label: 'Chính phủ số' },
  { value: 'Kinh tế số', label: 'Kinh tế số' },
  { value: 'Xã hội số', label: 'Xã hội số' },
  { value: 'Hạ tầng số', label: 'Hạ tầng số' },
  { value: 'Dữ liệu số', label: 'Dữ liệu số' },
  { value: 'Khác', label: 'Khác' }
];

const agencyFilterOptions = computed(() => {
  return (agencies.value || [])
    .filter(a => a.code !== 'ALL_AGENCIES')
    .map(a => ({ value: a.id, label: a.name }));
});

const userAgencyId = computed(() => authState.user.value?.agencyId || authState.user.value?.agency?.id || null);
const userRoleStr = computed(() => authState.user.value?.role || (authState.isAdmin.value ? 'Admin' : 'Level2'));

async function fetchAgencies() {
  try {
    const res = await fetch(getApiUrl('/api/agencies'));
    if (res.ok) {
      agencies.value = await res.json();
    }
  } catch (err) {
    console.error('Error fetching agencies:', err);
  }
}

async function fetchDocuments() {
  isLoading.value = true;
  try {
    const params = new URLSearchParams({
      pageNumber: pageNumber.value.toString(),
      pageSize: pageSize.value.toString()
    });

    if (filterDraft.searchQuery) params.append('searchQuery', filterDraft.searchQuery);
    if (filterDraft.documentType) params.append('documentType', filterDraft.documentType);
    if (filterDraft.effectStatus) params.append('effectStatus', filterDraft.effectStatus);
    if (filterDraft.field) params.append('field', filterDraft.field);
    if (filterDraft.issuingAgencyId) params.append('issuingAgencyId', filterDraft.issuingAgencyId);
    if (filterDraft.draftingAgencyId) params.append('draftingAgencyId', filterDraft.draftingAgencyId);
    if (filterDraft.fromDate) params.append('fromDate', filterDraft.fromDate);
    if (filterDraft.toDate) params.append('toDate', filterDraft.toDate);

    if (userAgencyId.value) params.append('userAgencyId', userAgencyId.value);
    if (userRoleStr.value) params.append('userRole', String(userRoleStr.value));

    const res = await fetch(getApiUrl(`/api/legaldocuments?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      documents.value = data.items || [];
      totalCount.value = data.totalCount || 0;
      totalPages.value = data.totalPages || 1;
    }
  } catch (err) {
    console.error('Error fetching legal documents:', err);
  } finally {
    isLoading.value = false;
  }
}

function execSearch() {
  pageNumber.value = 1;
  fetchDocuments();
}

function resetSearch() {
  filterDraft.searchQuery = '';
  filterDraft.documentType = '';
  filterDraft.effectStatus = '';
  filterDraft.field = '';
  filterDraft.issuingAgencyId = null;
  filterDraft.draftingAgencyId = null;
  filterDraft.fromDate = '';
  filterDraft.toDate = '';
  pageNumber.value = 1;
  fetchDocuments();
}

function changePage(p) {
  if (p < 1 || p > totalPages.value) return;
  pageNumber.value = p;
  fetchDocuments();
}

function openCreateModal() {
  selectedDocument.value = null;
  isReadOnlyModal.value = false;
  isModalOpen.value = true;
}

function openEditModal(doc) {
  selectedDocument.value = doc;
  isReadOnlyModal.value = false;
  isModalOpen.value = true;
}

function openViewModal(doc) {
  selectedDocument.value = doc;
  isReadOnlyModal.value = true;
  isModalOpen.value = true;
}

function openFileViewer(fileItem) {
  openFileInNewWindow(fileItem);
}

async function handleClearAll() {
  const confirmed = await confirmModal({
    title: 'Xóa sạch dữ liệu văn bản demo',
    message: 'Bạn có chắc chắn muốn xóa TOÀN BỘ văn bản QPPL hiện tại để bắt đầu nhập dữ liệu thật không? Thao tác này không thể hoàn tác.',
    confirmText: 'Xóa toàn bộ',
    cancelText: 'Hủy',
    type: 'danger'
  });

  if (!confirmed) return;

  try {
    const userRoleStr = authState.user.value?.role || (authState.isAdmin.value ? 'Admin' : 'Level2');
    const res = await fetch(getApiUrl(`/api/legaldocuments/clear-all?userRole=${encodeURIComponent(userRoleStr)}`), {
      method: 'DELETE'
    });

    if (res.ok) {
      toast.success('Đã xóa sạch toàn bộ văn bản demo thành công!');
      fetchDocuments();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || 'Lỗi khi xóa dữ liệu.');
    }
  } catch (err) {
    toast.error('Lỗi khi gửi yêu cầu xóa.');
  }
}

async function handleDelete(doc) {
  const confirmed = await confirmModal({
    title: 'Xóa văn bản QPPL',
    message: `Bạn có chắc chắn muốn xóa văn bản QPPL "${doc.code} - ${doc.title}"?`,
    confirmText: 'Xóa văn bản',
    cancelText: 'Hủy',
    type: 'danger'
  });

  if (!confirmed) return;

  try {
    const res = await fetch(getApiUrl(`/api/legaldocuments/${doc.id}`), {
      method: 'DELETE'
    });

    if (res.ok) {
      toast.success('Đã xóa văn bản QPPL thành công!');
      fetchDocuments();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || 'Không thể xóa văn bản.');
    }
  } catch (err) {
    toast.error('Đã xảy ra lỗi khi xóa văn bản.');
  }
}

function formatDate(dStr) {
  if (!dStr) return '—';
  try {
    const d = new Date(dStr);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    return `${day}/${month}/${d.getFullYear()}`;
  } catch {
    return '—';
  }
}

function getStatusBadgeClass(status) {
  switch (status) {
    case 'Còn hiệu lực': return 'bg-emerald-100 text-emerald-800 border border-emerald-200';
    case 'Hết hiệu lực toàn bộ': return 'bg-rose-100 text-rose-800 border border-rose-200';
    case 'Hết hiệu lực một phần': return 'bg-amber-100 text-amber-800 border border-amber-200';
    case 'Chưa có hiệu lực': return 'bg-slate-100 text-slate-700 border border-slate-200';
    default: return 'bg-blue-100 text-blue-800 border border-blue-200';
  }
}

function exportExcel() {
  const docsList = documents.value || [];
  if (docsList.length === 0) {
    toast.warning("Không có dữ liệu để xuất file Excel!");
    return;
  }

  const headers = [
    "STT",
    "Số Ký Hiệu",
    "Trích Yếu Nội Dung",
    "Loại VB",
    "Cơ Quan Ban Hành",
    "Cơ Quan Dự Thảo",
    "Người Ký & Chức Danh",
    "Ngày Ban Hành",
    "Ngày Hiệu Lực",
    "Trạng Thái Hiệu Lực",
    "Lĩnh Vực",
    "Tệp Đính Kèm"
  ];

  const rows = docsList.map((d, i) => {
    const signer = d.signerName ? `${d.signerName}${d.signerTitle ? ' (' + d.signerTitle + ')' : ''}` : '—';
    const attachCount = d.attachments ? d.attachments.length : 0;
    const attachStr = attachCount > 0 ? `${attachCount} tệp đính kèm` : 'Không có';

    return [
      i + 1,
      d.code || '—',
      d.title || '—',
      d.documentType || '—',
      d.issuingAgencyName || '—',
      d.draftingAgencyName || '—',
      signer,
      formatDate(d.issuedDate),
      formatDate(d.effectiveDate),
      d.effectStatus || '—',
      d.field || '—',
      attachStr
    ];
  });

  const now = new Date();
  const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;

  exportFormattedReportExcel({
    title: "BÁO CÁO DANH SÁCH VĂN BẢN QUY PHẠM PHÁP LUẬT (VB QPPL)",
    subtitle: `Thời gian xuất: ${timeStr} | Tổng số văn bản: ${docsList.length}`,
    kpiTitle: "1. TỔNG QUAN HỆ THỐNG VĂN BẢN QPPL",
    kpiSection: [
      ["Tổng số văn bản quy phạm pháp luật trong danh sách", docsList.length]
    ],
    tableTitle: "2. DANH SÁCH CHI TIẾT VĂN BẢN QUY PHẠM PHÁP LUẬT",
    headers,
    rows,
    fileName: `Bao_Cao_Van_Ban_QPPL`,
    sheetName: 'Danh Sách VB QPPL'
  });
}

onMounted(async () => {
  await fetchAgencies();
  await fetchDocuments();
});
</script>
