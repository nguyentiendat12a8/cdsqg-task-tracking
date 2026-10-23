<template>
  <div v-if="isOpen" @click.self="close" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 font-sans" v-accessible-dialog="close">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-5 sm:p-6 space-y-4 max-h-[92vh] flex flex-col animate-in fade-in duration-150">
      
      <!-- Modal Header -->
      <div class="flex items-center justify-between border-b border-slate-100 pb-3.5 shrink-0">
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-emerald-100 text-emerald-700 rounded-xl">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/></svg>
          </span>
          <div>
            <h3 class="text-sm sm:text-base font-bold text-slate-800">
              Nhập báo cáo tiến độ từ file Excel
            </h3>
            <p class="text-xs text-slate-500 font-semibold mt-0.5">
              Số liệu định lượng là tổng đã đạt đến năm báo cáo, không phải phần tăng thêm trong năm. Hệ thống đối soát mã, quyền và dữ liệu trước khi nhập.
            </p>
          </div>
        </div>
        <button type="button" aria-label="Đóng import báo cáo" @click="close" :disabled="isSubmitting || isParsing" class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-xl transition cursor-pointer">✕</button>
      </div>

      <SearchableSelect v-model="reportYear" label="Năm báo cáo" :isMulti="false" :clearable="false" :options="yearOptions" :disabled="isSubmitting || step !== 1" />
      <SearchableSelect v-if="needsReportingAgency" v-model="reportingAgencyId" label="Cơ quan báo cáo cho mục tiêu / nhiệm vụ chung" :isMulti="false" :clearable="false" :options="reportingAgencyOptions" :disabled="isSubmitting || step === 3" />
      <!-- STEP 1: File Picker & Drag-and-Drop Area -->
      <div v-if="step === 1" class="space-y-4 flex-1 flex flex-col justify-center items-center py-8 border-2 border-dashed border-slate-300 rounded-2xl bg-slate-50/50 hover:bg-slate-50 transition text-center px-4">
        <div class="w-16 h-16 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center text-2xl shadow-2xs mb-2">
          📄
        </div>
        <div>
          <h4 class="text-sm font-bold text-slate-800">Chọn file báo cáo Excel (.xlsx, .xls)</h4>
          <p class="text-xs text-slate-500 mt-1">Hoặc sử dụng file Excel đã xuất từ hệ thống để nhập tiến độ mới</p>
        </div>

        <input 
          type="file" 
          ref="fileInputRef" 
          @change="handleFileUpload" 
          accept=".xlsx, .xls" 
          class="hidden" 
        />

        <div class="flex items-center gap-3 pt-2">
          <button 
            type="button" 
            @click="$refs.fileInputRef.click()" 
            class="ui-single-line px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center gap-2 cursor-pointer"
          >
            <span>📁 Chọn file báo cáo Excel</span>
          </button>
        </div>

        <div v-if="isParsing" class="flex items-center gap-2 text-xs font-bold text-blue-700 pt-2">
          <span class="w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin"></span>
          <span>Đang đọc và đối soát mã dữ liệu...</span>
        </div>
      </div>

      <!-- STEP 2: Preview & Validation Table -->
      <div v-else-if="step === 2" class="flex-1 flex flex-col min-h-0 space-y-3">
        <!-- Summary Stats Banner -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-2.5 text-xs font-bold shrink-0">
          <div class="bg-slate-100 p-2.5 rounded-xl border border-slate-200">
            <span class="text-slate-500 block text-xs">Tổng mục có tiến độ mới</span>
            <span class="text-slate-800 text-base font-bold">{{ formatNumber(parsedItems.length) }}</span>
          </div>

          <div class="bg-emerald-50 p-2.5 rounded-xl border border-emerald-200">
            <span class="text-emerald-700 block text-xs">Hợp lệ (duyệt ngay)</span>
            <span class="text-emerald-800 text-base font-bold">{{ formatNumber(validApprovedCount) }}</span>
          </div>

          <div class="bg-amber-50 p-2.5 rounded-xl border border-amber-200">
            <span class="text-amber-700 block text-xs">Hợp lệ (chờ phê duyệt)</span>
            <span class="text-amber-800 text-base font-bold">{{ formatNumber(validPendingCount) }}</span>
          </div>

          <div class="bg-rose-50 p-2.5 rounded-xl border border-rose-200">
            <span class="text-rose-700 block text-xs">Lỗi / không có quyền</span>
            <span class="text-rose-800 text-base font-bold">{{ formatNumber(invalidCount) }}</span>
          </div>
        </div>

        <!-- Preview Table -->
        <div class="flex-1 overflow-y-auto border border-slate-200 rounded-xl custom-scrollbar">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-100 sticky top-0 font-bold text-slate-700 border-b border-slate-200">
              <tr>
                <th class="p-2.5 text-center w-10">STT</th>
                <th class="p-2.5 w-24">Mã</th>
                <th class="p-2.5">Mục tiêu / Nhiệm vụ</th>
                <th class="p-2.5 w-28">Đơn vị tính</th>
                <th class="p-2.5 w-32">Tiến độ mới</th>
                <th class="p-2.5">Ghi chú / Trích yếu</th>
                <th class="p-2.5 w-44 text-center">Kiểm tra quyền & trạng thái</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <tr 
                v-for="(row, idx) in parsedItems" 
                :key="idx" 
                :class="[
                  'hover:bg-slate-50 transition',
                  !row.isValid ? 'bg-rose-50/30' : (row.isPending ? 'bg-amber-50/20' : 'bg-white')
                ]"
              >
                <td class="p-2.5 text-center font-bold text-slate-500">{{ idx + 1 }}</td>
                <td class="p-2.5 font-bold text-blue-700">{{ row.code }}</td>
                <td class="p-2.5 font-semibold text-slate-800 leading-snug">
                  <span v-if="row.taskTitle">{{ row.taskTitle }}</span>
                  <span v-else class="text-rose-500 italic">Không tìm thấy mã này</span>
                </td>
                <td class="p-2.5 font-bold text-slate-600">{{ row.unitName || '—' }}</td>
                <td class="p-2.5 font-bold text-emerald-700">
                  <span v-if="row.newValueDisplay">{{ row.newValueDisplay }}</span>
                  <span v-else class="text-slate-400 italic">Chưa nhập</span>
                </td>
                <td class="p-2.5 text-slate-600 truncate max-w-xs" :title="row.summaryNotes">
                  {{ row.summaryNotes || '—' }}
                </td>
                <td class="p-2.5 text-center">
                  <span :class="['px-2 py-0.5 rounded-full text-xs font-bold border inline-block whitespace-nowrap', row.statusBadgeClass]">
                    {{ row.statusText }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Footer Actions for Step 2 -->
        <div class="flex items-center justify-between pt-2 border-t border-slate-100 shrink-0">
          <button 
            type="button" 
            @click="step = 1" 
            class="ui-single-line px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer"
          >
            ← Chọn lại file
          </button>

          <button 
            type="button" 
            @click="submitImport" 
            :disabled="isSubmitting || validTotalCount === 0"
            class="ui-single-line px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center gap-2 cursor-pointer"
          >
            <span v-if="isSubmitting" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>{{ isSubmitting ? 'Đang cập nhật tiến độ...' : `Xác nhận nhập (${formatNumber(validTotalCount)} mục hợp lệ)` }}</span>
          </button>
        </div>
      </div>

      <!-- STEP 3: Final Import Summary Results -->
      <div v-else-if="step === 3" class="flex-1 flex flex-col min-h-0 space-y-3">
        <div class="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl flex items-center justify-between">
          <div class="flex items-center gap-3">
            <span class="text-2xl">🎉</span>
            <div>
              <h4 class="text-sm font-bold text-emerald-900">Nhập tiến độ hàng loạt hoàn tất!</h4>
              <p class="text-xs text-emerald-700 font-medium">
                Đã xử lý thành công {{ formatNumber(importResults.successCount) }} mục, {{ formatNumber(importResults.pendingCount) }} mục đã gửi chờ phê duyệt, {{ formatNumber(importResults.failureCount) }} mục lỗi/bỏ qua.
              </p>
            </div>
          </div>
        </div>

        <!-- Results Table -->
        <div class="flex-1 overflow-y-auto border border-slate-200 rounded-xl custom-scrollbar">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-100 sticky top-0 font-bold text-slate-700 border-b border-slate-200">
              <tr>
                <th class="p-2.5 text-center w-10 shrink-0">STT</th>
                <th class="p-2.5 w-24 shrink-0">Mã</th>
                <th class="p-2.5 min-w-[200px]">Mục tiêu / Nhiệm vụ</th>
                <th class="p-2.5 text-center w-32 shrink-0">Trạng thái xử lý</th>
                <th class="p-2.5 min-w-[240px] w-2/5">Thông báo hệ thống</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <tr v-for="(res, idx) in importResults.results" :key="idx" class="hover:bg-slate-50 transition">
                <td class="p-2.5 text-center font-bold text-slate-500">{{ idx + 1 }}</td>
                <td class="p-2.5 font-bold text-blue-700 whitespace-nowrap">{{ res.code }}</td>
                <td class="p-2.5 font-semibold text-slate-800 leading-snug">{{ res.title }}</td>
                <td class="p-2.5 text-center whitespace-nowrap">
                  <span v-if="res.success && res.approvalStatus === 'Pending'" class="px-2 py-0.5 rounded-full text-xs font-bold bg-amber-50 text-amber-800 border border-amber-200 whitespace-nowrap">
                    ⏳ Chờ duyệt
                  </span>
                  <span v-else-if="res.success" class="px-2 py-0.5 rounded-full text-xs font-bold bg-emerald-50 text-emerald-800 border border-emerald-200 whitespace-nowrap">
                    ✅ Đã cập nhật
                  </span>
                  <span v-else class="px-2 py-0.5 rounded-full text-xs font-bold bg-rose-50 text-rose-800 border border-rose-200 whitespace-nowrap">
                    ❌ Thất bại
                  </span>
                </td>
                <td class="p-2.5 text-slate-700 font-medium">{{ res.message }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="flex justify-end pt-2 border-t border-slate-100 shrink-0">
          <button 
            type="button" 
            @click="finishImport" 
            class="ui-single-line px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-md transition cursor-pointer"
          >
            Hoàn tất và đóng
          </button>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { formatNumber } from "../shared/formatters";
import { isAdminRole } from '../shared/roles';
import SearchableSelect from './SearchableSelect.vue';
import { availableReportingYears, currentReportingYear, isValidReportingYear } from '../config/reporting';
import { formatMeasuredValue } from '../shared/formatters';
import { parseQuantityInput } from '../shared/numericInput';
import { readProductProgress } from '../utils/progressProducts';
import { parseReportStatus, reportStatusLabel } from '../shared/reportStatuses';
import { fetchWithAuth } from '../services/auth';

import { ref, computed } from 'vue';
import { loadExcelReader } from '../utils/excelRuntime';
import { toast } from 'vue3-toastify';
import { getApiUrl } from '../config/api';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  items: { type: Array, default: () => [] },
  agencies: { type: Array, default: () => [] },
  agencyId: { type: String, default: null },
  userRole: { type: String, default: '' },
  currentAgency: { type: Object, default: () => null }
});

const emit = defineEmits(['close', 'imported']);

const step = ref(1);
const reportingAgencyId = ref(null);
const needsReportingAgency = computed(() => isAdminRole(props.userRole) && props.items.some(item => item.isGeneralTask));
const reportingAgencyOptions = computed(() => props.agencies.filter(agency => !agency.code?.startsWith('ALL_')).map(agency => ({ value: agency.id, label: agency.name })));
const reportYear = ref(currentReportingYear());
const yearOptions = availableReportingYears().map(value => ({ value, label: String(value) }));
const fileInputRef = ref(null);
const isParsing = ref(false);
const isSubmitting = ref(false);
const parsedItems = ref([]);
const importResults = ref({
  totalProcessed: 0,
  successCount: 0,
  pendingCount: 0,
  failureCount: 0,
  results: []
});

const validApprovedCount = computed(() => parsedItems.value.filter(i => i.isValid && !i.isPending).length);
const validPendingCount = computed(() => parsedItems.value.filter(i => i.isValid && i.isPending).length);
const validTotalCount = computed(() => parsedItems.value.filter(i => i.isValid).length);
const invalidCount = computed(() => parsedItems.value.filter(i => !i.isValid).length);

function close() {
  if (isSubmitting.value || isParsing.value) return;
  step.value = 1;
  parsedItems.value = [];
  emit('close');
}

function finishImport() {
  emit('imported');
  close();
}

function mapQualitativeStatus(str) {
  return parseReportStatus(str);
}

function mapQualitativeStatusDisplay(statusKey) {
  return reportStatusLabel(statusKey);
}

function checkUserPermission(task) {
  if (!task) return false;
  const role = props.userRole !== null && props.userRole !== undefined ? String(props.userRole).toLowerCase() : '';
  const isAdmin = isAdminRole(role);
  const currAgId = props.agencyId ? String(props.agencyId).toLowerCase() : null;

  if (isAdmin) return true;
  if (!currAgId) return false;

  const leadAgId = task.leadAgencyId ? String(task.leadAgencyId).toLowerCase() : null;
  const assignedAgId = task.assignedAgencyId ? String(task.assignedAgencyId).toLowerCase() : null;
  const isGeneral = task.isGeneralTask || task.leadAgencyCode === 'ALL_AGENCIES';

  const isLevel2 = props.currentAgency && !props.currentAgency.parentId;
  const isLevel3 = props.currentAgency && props.currentAgency.parentId;

  if (isLevel2) {
    if (leadAgId === currAgId || isGeneral) return true;
    if (task.leadAgency?.parentId && String(task.leadAgency.parentId).toLowerCase() === currAgId) return true;
    if (assignedAgId && props.currentAgency?.children?.some(c => String(c.id).toLowerCase() === assignedAgId)) return true;
    return false;
  }

  if (isLevel3) {
    if (assignedAgId === currAgId || leadAgId === currAgId) return true;
    return false;
  }

  return leadAgId === currAgId || assignedAgId === currAgId;
}

function checkIsLevel3Pending(task) {
  return Boolean(task) && !isAdminRole(props.userRole);
}

async function handleFileUpload(e) {
  if (isParsing.value) return;
  const file = e.target.files?.[0];
  if (!file) return;

  isParsing.value = true;
  parsedItems.value = [];

  try {
    const XLSX = await loadExcelReader(file.name);
    const data = await file.arrayBuffer();
    const workbook = XLSX.read(data, { type: 'array' });
    const sheetName = workbook.SheetNames[0];
    const worksheet = workbook.Sheets[sheetName];
    const jsonRows = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

    if (!jsonRows || jsonRows.length === 0) {
      toast.error('File Excel không có dữ liệu.');
      isParsing.value = false;
      return;
    }

    // Find header row containing STT or Mã
    let headerRowIdx = -1;
    for (let r = 0; r < Math.min(jsonRows.length, 10); r++) {
      const row = jsonRows[r];
      if (row && row.some(cell => String(cell).toLowerCase().includes('mã') || String(cell).toLowerCase().includes('stt'))) {
        headerRowIdx = r;
        break;
      }
    }

    if (headerRowIdx === -1) throw new Error('Missing headers');

    const headerRow = jsonRows[headerRowIdx] || [];
    let codeColIdx = -1;
    let newProgressColIdx = -1;
    let notesColIdx = -1;

    headerRow.forEach((cell, idx) => {
      const str = String(cell || '').toLowerCase().trim();
      if (str === 'mã' || str.includes('mã hạng mục') || str.includes('mã mục tiêu')) {
        codeColIdx = idx;
      }
      if (str.includes('tiến độ') && (str.includes('mới') || str.includes('cần điền'))) {
        newProgressColIdx = idx;
      }
      if (str.includes('ghi chú') || str.includes('trích yếu')) {
        notesColIdx = idx;
      }
    });

    if (codeColIdx === -1 || newProgressColIdx === -1) throw new Error('Missing report columns');

    const resultList = [];
    for (let r = headerRowIdx + 1; r < jsonRows.length; r++) {
      const row = jsonRows[r];
      if (!row || row.length === 0) continue;

      const rawCode = String(row[codeColIdx] || '').trim();
      if (!rawCode || rawCode.toLowerCase().startsWith('hướng dẫn') || rawCode.toLowerCase().startsWith('báo cáo')) continue;

      const rawVal = row[newProgressColIdx] !== undefined && row[newProgressColIdx] !== null ? String(row[newProgressColIdx]).trim() : '';
      const rawNotes = row[notesColIdx] !== undefined && row[notesColIdx] !== null ? String(row[notesColIdx]).trim() : '';

      // Match item by code in props.items
      const matchedTask = props.items.find(i => i.code && i.code.trim().toLowerCase() === rawCode.toLowerCase());
      const productInput = matchedTask && !['Quantitative', '1', 1].includes(matchedTask.evaluationType) && matchedTask.deliverables?.length
        ? readProductProgress(workbook, matchedTask, XLSX) : null;
      // A blank new value is an intentional omission, not a validation failure.
      // For product tasks inspect all merged rows, including non-first products.
      if (productInput?.skipped || (!productInput && rawVal === '')) continue;

      let hasPerm = false;
      let isPending = false;
      let isValid = false;
      let parsedValue = null;
      let parsedStatus = null;
      let parsedDeliverables = null;
      let newValueDisplay = '—';
      let statusText = 'Chưa rõ';
      let statusBadgeClass = 'bg-slate-100 text-slate-700 border-slate-300';

      if (!matchedTask) {
        statusText = 'Mã không tồn tại';
        statusBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
      } else {
        hasPerm = checkUserPermission(matchedTask);
        if (!hasPerm) {
          statusText = 'Không có quyền';
          statusBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
        } else {
          isPending = checkIsLevel3Pending(matchedTask);

          // Parse value based on evaluation type
          const isQuantitative = ['Quantitative', '1', 1].includes(matchedTask.evaluationType);
          
          if (isQuantitative) {
            if (rawVal !== '') {
              const num = typeof row[newProgressColIdx] === 'number' ? row[newProgressColIdx] : parseQuantityInput(rawVal);
              if (num != null && Number.isFinite(num) && num >= 0) {
                parsedValue = num;
                newValueDisplay = formatMeasuredValue(num, matchedTask.unitName || '');
                isValid = true;
              } else {
                statusText = 'Con số không hợp lệ';
                statusBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
              }
            } else {
              statusText = 'Chưa nhập giá trị';
              statusBadgeClass = 'bg-amber-50 text-amber-700 border-amber-200';
            }
          } else if (matchedTask.deliverables?.length) {
            const products = productInput;
            if (products.error) {
              statusText = products.error;
              statusBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
            } else {
              parsedDeliverables = products.deliverables;
              newValueDisplay = parsedDeliverables.map((product, index) => `${index + 1}. ${reportStatusLabel(product.currentStatus)}`).join('; ');
              isValid = true;
            }
          } else {
            // Direct status only when there are no output products.
            const mappedSt = mapQualitativeStatus(rawVal);
            if (mappedSt) {
              parsedStatus = mappedSt;
              newValueDisplay = mapQualitativeStatusDisplay(mappedSt);
              isValid = true;
            } else if (rawVal !== '') {
              statusText = 'Trạng thái không hợp lệ';
              statusBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
            } else {
              statusText = 'Chưa nhập trạng thái';
              statusBadgeClass = 'bg-amber-50 text-amber-700 border-amber-200';
            }
          }

          if (isValid) {
            if (isPending) {
              statusText = 'Hợp lệ (Chờ phê duyệt)';
              statusBadgeClass = 'bg-amber-50 text-amber-800 border-amber-200';
            } else {
              statusText = 'Hợp lệ (Duyệt ngay)';
              statusBadgeClass = 'bg-emerald-50 text-emerald-800 border-emerald-200';
            }
          }
        }
      }

      resultList.push({
        code: rawCode,
        taskTitle: matchedTask?.title || '',
        unitName: matchedTask?.unitName || '',
        evaluationType: matchedTask?.evaluationType || 'Quantitative',
        rawVal,
        value: parsedValue,
        status: parsedStatus,
        deliverables: parsedDeliverables,
        summaryNotes: rawNotes,
        newValueDisplay,
        isValid,
        hasPerm,
        isPending,
        statusText,
        statusBadgeClass
      });
    }

    const counts = new Map();
    resultList.forEach(row => counts.set(row.code.toLowerCase(), (counts.get(row.code.toLowerCase()) ?? 0) + 1));
    resultList.forEach(row => {
      if (counts.get(row.code.toLowerCase()) > 1) {
        row.isValid = false;
        row.statusText = 'Mã bị trùng trong file';
        row.statusBadgeClass = 'bg-rose-50 text-rose-700 border-rose-200';
      }
    });
    parsedItems.value = resultList;
    if (resultList.length === 0) {
      toast.info('Không có tiến độ mới để nhập. Các mục để trống đã được bỏ qua.');
    } else {
      step.value = 2;
    }
  } catch (err) {
    console.error('Lỗi khi đọc file Excel:', err);
    toast.error('Không thể đọc báo cáo. Dùng mẫu xuất báo cáo tiến độ có cột Mã và Tiến độ thực hiện mới.');
  } finally {
    isParsing.value = false;
  }
}

async function submitImport() {
  if (needsReportingAgency.value && !reportingAgencyId.value) {
    toast.warning('Chọn cơ quan báo cáo cụ thể cho mục tiêu / nhiệm vụ chung.');
    return;
  }
  if (isSubmitting.value) return;
  if (!isValidReportingYear(reportYear.value)) {
    toast.error('Năm báo cáo không hợp lệ hoặc lớn hơn năm hiện tại.');
    return;
  }
  const validRows = parsedItems.value.filter(i => i.isValid);
  if (validRows.length === 0) {
    toast.warning('Không có hạng mục hợp lệ nào để nhập.');
    return;
  }

  isSubmitting.value = true;

  try {
    const payload = {
      userAgencyId: props.agencyId,
      reportingAgencyId: needsReportingAgency.value ? reportingAgencyId.value : null,
      userRole: props.userRole,
      items: validRows.map(r => ({
        code: r.code,
        value: r.value,
        status: r.status,
        deliverables: r.deliverables,
        summaryNotes: r.summaryNotes,
        periodYear: reportYear.value
      }))
    };

    const res = await fetchWithAuth(getApiUrl('/api/execution/import-progress-bulk'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errData = await res.json().catch(() => ({}));
      throw new Error(errData.message || 'Lỗi server khi import tiến độ.');
    }

    const data = await res.json();
    importResults.value = data;
    step.value = 3;
    toast.success(`Đã import thành công ${data.successCount + data.pendingCount} hạng mục!`);
  } catch (err) {
    console.error('Lỗi Submit Import:', err);
    toast.error(err.message || 'Lỗi hệ thống khi gửi dữ liệu import.');
  } finally {
    isSubmitting.value = false;
  }
}
</script>


