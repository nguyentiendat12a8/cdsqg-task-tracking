<template>
  <div class="bg-white rounded-2xl p-6 shadow-sm border border-slate-200/80 space-y-6 font-sans">
    
    <!-- Top Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-slate-100 pb-4">
      <div>
        <h2 class="text-xl font-extrabold text-slate-800 flex items-center gap-2">
          <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"/></svg>
          Danh Mục Đơn Vị Tính
        </h2>
        <p class="text-xs text-slate-500 mt-1">Cấu hình tên đơn vị (%, Lượt, Văn bản...) và Kiểu dữ liệu hỗ trợ</p>
      </div>

      <button @click="openCreateModal" class="bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs px-4 py-2.5 rounded-xl shadow-sm transition flex items-center gap-1.5 shrink-0">
        + Thêm Đơn Vị Tính Mới
      </button>
    </div>

    <!-- Unified Container Block for Search, Table & Server Pagination -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden w-full flex flex-col">
      
      <!-- 1. Top Header: Search & Filter Bar -->
      <div class="flex items-center justify-between gap-4 p-4 border-b border-slate-200 bg-white w-full">
        <div class="flex items-center gap-2 flex-1 max-w-md">
          <div class="relative w-full">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              :value="searchDraft" 
              @input="searchDraft = $event.target.value"
              placeholder="Tìm kiếm theo mã, tên đơn vị tính, kiểu dữ liệu..." 
              class="w-full text-xs font-semibold pl-9 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>
          <button @click="resetSearch" class="px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer">↺</button>
        </div>
        <span class="text-xs text-slate-500 font-semibold shrink-0">
          Hiển thị {{ units.length }} / {{ totalCount }} đơn vị
        </span>
      </div>

      <!-- 2. Middle Body: Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-380px)] custom-scrollbar flex-1 bg-white">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh mục đơn vị tính..." />

        <table v-else class="w-full text-left text-sm text-slate-700">
          <thead class="bg-slate-100 text-xs text-slate-500 uppercase font-bold border-b border-slate-200 sticky top-0 z-10">
            <tr>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100">Mã Đơn Vị</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100">Tên Hiển Thị</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100">Kiểu Dữ Liệu</th>
              <th class="px-4 py-3 text-center bg-slate-100 whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-for="unit in units" :key="unit.id" class="hover:bg-slate-50 transition">
              <td class="px-4 py-3 border-r border-slate-200 font-extrabold text-blue-700">{{ unit.code }}</td>
              <td class="px-4 py-3 border-r border-slate-200 font-bold text-slate-800">{{ unit.name }}</td>
              <td class="px-4 py-3 border-r border-slate-200">
                <span class="px-2.5 py-1 bg-emerald-50 text-emerald-800 border border-emerald-100 rounded-full text-xs font-bold">
                  {{ formatDataType(unit.dataType) }}
                </span>
              </td>
              <td class="px-4 py-3 text-center space-x-2 whitespace-nowrap">
                <button @click="openEditModal(unit)" class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition inline-flex items-center" title="Sửa">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                </button>
                <button @click="deleteUnit(unit.id)" class="p-1.5 text-rose-600 hover:text-rose-800 hover:bg-rose-50 rounded-lg transition inline-flex items-center" title="Xóa">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </td>
            </tr>

            <tr v-if="units.length === 0">
              <td colspan="4" class="p-8 text-center text-slate-400 font-semibold italic">
                Không tìm thấy đơn vị tính nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 3. Bottom Footer: Server Pagination Controls -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div class="flex items-center gap-3 whitespace-nowrap flex-wrap sm:flex-nowrap">
          <span class="whitespace-nowrap">Hiển thị <span class="font-extrabold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalCount }}</span> đơn vị tính</span>
          
          <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3 whitespace-nowrap">
            <span class="whitespace-nowrap">Số bản ghi/trang:</span>
            <SearchableSelect 
              v-model="pageSize" 
              :options="pageSizeOptions" 
              :isMulti="false" 
              :clearable="false" 
              @change="execSearch" 
              class="w-20"
            />
          </div>
        </div>

        <div class="flex items-center gap-2 shrink-0 whitespace-nowrap">
          <button 
            @click="changePage(pageNumber - 1)" 
            :disabled="pageNumber <= 1"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-xs cursor-pointer"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-black">
            Trang {{ pageNumber }} / {{ Math.max(1, totalPages) }}
          </span>

          <button 
            @click="changePage(pageNumber + 1)" 
            :disabled="pageNumber >= totalPages"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-xs cursor-pointer"
          >
            Trang sau ›
          </button>
        </div>
      </div>

    </div>

    <!-- Create/Edit Modal -->
    <div v-if="isModalOpen" @click.self="isModalOpen = false" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-xl w-full p-6 sm:p-7 space-y-4">
        <h3 class="text-lg font-bold text-slate-800 border-b border-slate-100 pb-2">
          {{ isEditing ? 'Sửa Đơn Vị Tính' : 'Thêm Đơn Vị Tính Mới' }}
        </h3>

        <form @submit.prevent="saveUnit" class="space-y-3">
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Mã Đơn Vị (e.g. PERCENT) <span class="text-rose-500">*</span></label>
              <input v-model="form.code" required class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>

            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Tên Hiển Thị (e.g. %) <span class="text-rose-500">*</span></label>
              <input v-model="form.name" required class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>
          </div>

          <div>
            <SearchableSelect 
              v-model="form.dataType" 
              :options="dataTypeOptions" 
              :isMulti="false" 
              label="Kiểu Dữ Liệu" 
            />
            <p v-if="isEditing && editingUsedCount > 0" class="text-[11px] text-amber-700 bg-amber-50 p-2 rounded-lg border border-amber-200 mt-1.5 font-semibold flex items-center gap-1.5">
              <span>⚠️</span>
              <span>Đơn vị tính này đã được sử dụng bởi <strong>{{ editingUsedCount }}</strong> mục tiêu/nhiệm vụ. Không thể thay đổi kiểu dữ liệu.</span>
            </p>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
            <button type="button" @click="isModalOpen = false" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition">Hủy</button>
            <button type="submit" class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl transition shadow-sm">Lưu</button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import SearchableSelect from './SearchableSelect.vue';
import { toast } from 'vue3-toastify';
import LoadingSpinner from './LoadingSpinner.vue';
import { getApiUrl } from '../config/api';
import { confirmModal } from '../services/confirm';

const units = ref([]);
const isLoading = ref(true);

const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

const dataTypeOptions = ref([
  { value: 1, label: 'Số thập phân (%)' },
  { value: 2, label: 'Số nguyên' },
  { value: 3, label: 'Boolean (Đúng/Sai)' },
  { value: 4, label: 'Trạng thái văn bản' }
]);

const searchDraft = ref('');
const appliedSearch = ref('');

const pageNumber = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const isModalOpen = ref(false);
const isEditing = ref(false);
const editingId = ref(null);
const editingUsedCount = ref(0);
const form = ref({ code: '', name: '', dataType: 1 });

let unitFetchRequestId = 0;
let unitSearchDebounceTimer = null;

function execSearch() {
  if (unitSearchDebounceTimer) clearTimeout(unitSearchDebounceTimer);
  unitFetchRequestId++;
  appliedSearch.value = searchDraft.value;
  pageNumber.value = 1;
  fetchUnits();
}

watch(searchDraft, () => {
  if (unitSearchDebounceTimer) clearTimeout(unitSearchDebounceTimer);
  unitSearchDebounceTimer = setTimeout(() => {
    execSearch();
  }, 300);
});

function resetSearch() {
  if (unitSearchDebounceTimer) clearTimeout(unitSearchDebounceTimer);
  unitFetchRequestId++;
  searchDraft.value = '';
  appliedSearch.value = '';
  pageNumber.value = 1;
  fetchUnits();
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  pageNumber.value = newPage;
  fetchUnits();
}

function formatDataType(dt) {
  if (dt === 1 || dt === '1' || dt === 'Decimal') return 'Số thập phân (%)';
  if (dt === 2 || dt === '2' || dt === 'Integer') return 'Số nguyên';
  if (dt === 3 || dt === '3' || dt === 'Boolean') return 'Boolean (Đúng/Sai)';
  if (dt === 4 || dt === '4' || dt === 'Text_Status') return 'Trạng thái văn bản';
  return dt || 'Số thập phân (%)';
}

function openCreateModal() {
  isEditing.value = false;
  editingId.value = null;
  editingUsedCount.value = 0;
  form.value = { code: '', name: '', dataType: 1 };
  isModalOpen.value = true;
}

function openEditModal(unit) {
  isEditing.value = true;
  editingId.value = unit.id;
  editingUsedCount.value = unit.usedCount || 0;

  let dt = unit.dataType;
  if (dt === 'Decimal') dt = 1;
  else if (dt === 'Integer') dt = 2;
  else if (dt === 'Boolean') dt = 3;
  else if (dt === 'Text_Status') dt = 4;

  form.value = { code: unit.code, name: unit.name, dataType: dt };
  isModalOpen.value = true;
}

async function fetchUnits() {
  const currentRequestId = ++unitFetchRequestId;
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/units'));
    url.searchParams.append('pageNumber', pageNumber.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (appliedSearch.value.trim()) {
      url.searchParams.append('search', appliedSearch.value.trim());
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (currentRequestId !== unitFetchRequestId) return;
      if (data.items) {
        units.value = data.items;
        totalCount.value = data.totalCount || data.items.length;
        pageNumber.value = data.pageNumber || 1;
        pageSize.value = data.pageSize || 10;
        totalPages.value = data.totalPages || 1;
      } else {
        units.value = data;
        totalCount.value = data.length;
        totalPages.value = 1;
      }
    }
  } catch (e) {
    if (currentRequestId !== unitFetchRequestId) return;
    console.error('Failed to fetch units:', e);
  } finally {
    if (currentRequestId === unitFetchRequestId) {
      isLoading.value = false;
    }
  }
}

async function saveUnit() {
  try {
    const payload = {
      code: form.value.code,
      name: form.value.name,
      dataType: Number(form.value.dataType)
    };

    let res;
    if (isEditing.value && editingId.value) {
      res = await fetch(getApiUrl(`/api/units/${editingId.value}`), {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    } else {
      res = await fetch(getApiUrl('/api/units'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    }

    if (res.ok) {
      toast.success(isEditing.value ? 'Cập nhật đơn vị tính thành công!' : 'Thêm đơn vị tính mới thành công!');
      isModalOpen.value = false;
      await fetchUnits();
    } else {
      const errData = await res.json().catch(() => ({}));
      toast.error(errData.error || errData.message || 'Lỗi khi lưu đơn vị tính.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

async function deleteUnit(unit) {
  if (unit.usedCount > 0) {
    toast.warning(`Không thể xóa vì đơn vị tính "${unit.name}" đang được sử dụng bởi ${unit.usedCount} mục tiêu/nhiệm vụ.`);
    return;
  }

  const confirmed = await confirmModal({
    title: 'Xóa đơn vị tính',
    message: `Bạn có chắc chắn muốn xóa đơn vị tính "${unit.name}"? Thao tác này không thể hoàn tác.`,
    confirmText: 'Xóa đơn vị tính',
    cancelText: 'Hủy bỏ',
    type: 'danger'
  });

  if (confirmed) {
    try {
      const res = await fetch(getApiUrl(`/api/units/${unit.id}`), { method: 'DELETE' });
      if (res.ok) {
        toast.success('Đã xóa đơn vị tính thành công!');
        await fetchUnits();
      } else {
        const errData = await res.json().catch(() => ({}));
        toast.error(errData.error || errData.message || 'Lỗi khi xóa đơn vị tính.');
      }
    } catch (e) {
      toast.error('Không thể kết nối máy chủ.');
    }
  }
}

onMounted(() => {
  fetchUnits();
});
</script>
