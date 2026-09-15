<template>
  <div class="bg-white rounded-2xl p-6 shadow-sm border border-slate-200/80 space-y-6 font-sans">
    
    <!-- Top Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-slate-100 pb-4">
      <div>
        <h2 class="text-xl font-extrabold text-slate-800 flex items-center gap-2">
          <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5m3 0h14"/></svg>
          Danh Mục Cơ Quan, Bộ Ngành
        </h2>
        <p class="text-xs text-slate-500 mt-1">Danh mục Bộ, Ngành, Địa phương phục vụ gán Đơn vị chủ trì & Đơn vị phối hợp</p>
      </div>

      <button @click="openCreateModal" class="bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs px-4 py-2.5 rounded-xl shadow-sm transition flex items-center gap-1.5 shrink-0">
        + Thêm Cơ Quan Mới
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
              v-model="searchDraft" 
              @keyup.enter="execSearch"
              placeholder="Tìm kiếm theo mã cơ quan, tên cơ quan..." 
              class="w-full text-xs font-semibold pl-9 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>
          <button @click="execSearch" class="px-3.5 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-xs transition cursor-pointer">Tìm</button>
          <button @click="resetSearch" class="px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer">↺</button>
        </div>
        <span class="text-xs text-slate-500 font-semibold shrink-0">
          Hiển thị {{ agencies.length }} / {{ totalCount }} cơ quan
        </span>
      </div>

      <!-- 2. Middle Body: Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-380px)] custom-scrollbar flex-1 bg-white">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh mục cơ quan..." />

        <table v-else class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-500 uppercase font-bold border-b border-slate-200 sticky top-0 z-10">
            <tr>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100">Mã Cơ Quan</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100">Tên Đầy Đủ</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100">Phân Loại</th>
              <th class="px-4 py-3 text-center bg-slate-100 whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-for="agency in agencies" :key="agency.id" class="hover:bg-slate-50 transition">
              <td class="px-4 py-3 border-r border-slate-200 font-extrabold text-blue-700">{{ agency.code }}</td>
              <td class="px-4 py-3 border-r border-slate-200 font-bold text-slate-800">{{ agency.name }}</td>
              <td class="px-4 py-3 border-r border-slate-200">
                <span :class="['px-2.5 py-1 rounded-full text-xs font-bold', getTypeBadgeClass(agency.type)]">
                  {{ getTypeLabel(agency.type) }}
                </span>
              </td>
              <td class="px-4 py-3 text-center space-x-2 whitespace-nowrap">
                <button @click="openEditModal(agency)" class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition inline-flex items-center" title="Sửa">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                </button>
                <button @click="deleteAgency(agency)" class="p-1.5 text-rose-600 hover:text-rose-800 hover:bg-rose-50 rounded-lg transition inline-flex items-center" title="Xóa">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </td>
            </tr>

            <tr v-if="agencies.length === 0">
              <td colspan="4" class="p-8 text-center text-slate-400 font-semibold italic">
                Không tìm thấy cơ quan nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 3. Bottom Footer: Server Pagination Controls -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div class="flex flex-wrap items-center gap-3">
          <span>Hiển thị <span class="font-extrabold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalCount }}</span> cơ quan</span>
          
          <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3">
            <span>Số bản ghi/trang:</span>
            <select v-model="pageSize" @change="execSearch" class="bg-white border border-slate-300 rounded-lg px-2 py-1 font-bold text-xs focus:outline-none">
              <option :value="10">10</option>
              <option :value="25">25</option>
              <option :value="50">50</option>
              <option :value="100">100</option>
            </select>
          </div>
        </div>

        <div class="flex items-center gap-2">
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

    <!-- Create/Edit Modal (No IsActive checkbox) -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-md w-full p-6 space-y-4">
        <h3 class="text-lg font-bold text-slate-800 border-b border-slate-100 pb-2">
          {{ isEditing ? 'Chỉnh Sửa Cơ Quan' : 'Thêm Cơ Quan Mới' }}
        </h3>

        <form @submit.prevent="saveAgency" class="space-y-3">
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Mã Cơ Quan</label>
            <input 
              v-model="form.code" 
              :disabled="isEditing && editingUsedCount > 0"
              required 
              class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1 disabled:opacity-60 disabled:bg-slate-100 disabled:cursor-not-allowed" 
            />
            <p v-if="isEditing && editingUsedCount > 0" class="text-[11px] text-amber-700 bg-amber-50 p-2 rounded-lg border border-amber-200 mt-1.5 font-semibold flex items-center gap-1.5">
              <span>⚠️</span>
              <span>Cơ quan này đã được sử dụng bởi <strong>{{ editingUsedCount }}</strong> mục tiêu/nhiệm vụ/đôn đốc. Không thể thay đổi mã cơ quan.</span>
            </p>
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Tên Đầy Đủ</label>
            <input v-model="form.name" required class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Phân Loại</label>
            <select v-model="form.type" required class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1">
              <option :value="1">Bộ / Ngành</option>
              <option :value="2">Tỉnh / Thành phố</option>
              <option :value="3">Đơn vị nội bộ</option>
              <option :value="4">Khác</option>
            </select>
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
import { ref, computed, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import LoadingSpinner from './LoadingSpinner.vue';

const agencies = ref([]);
const isLoading = ref(true);

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

const form = ref({ code: '', name: '', type: 1 });

function execSearch() {
  appliedSearch.value = searchDraft.value;
  pageNumber.value = 1;
  fetchAgencies();
}

function resetSearch() {
  searchDraft.value = '';
  appliedSearch.value = '';
  pageNumber.value = 1;
  fetchAgencies();
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  pageNumber.value = newPage;
  fetchAgencies();
}

function getTypeLabel(type) {
  const map = { 
    1: 'Bộ / Ngành', 'Ministry': 'Bộ / Ngành', '1': 'Bộ / Ngành',
    2: 'Tỉnh / TP', 'Province': 'Tỉnh / TP', '2': 'Tỉnh / TP',
    3: 'Nội bộ', 'Internal': 'Nội bộ', '3': 'Nội bộ',
    4: 'Khác', 'Other': 'Khác', '4': 'Khác'
  };
  return map[type] || type || 'Khác';
}

function getTypeBadgeClass(type) {
  if (type === 1 || type === '1' || type === 'Ministry') return 'bg-purple-50 text-purple-700 border border-purple-100';
  if (type === 2 || type === '2' || type === 'Province') return 'bg-blue-50 text-blue-700 border border-blue-100';
  if (type === 3 || type === '3' || type === 'Internal') return 'bg-emerald-50 text-emerald-700 border border-emerald-100';
  return 'bg-slate-100 text-slate-700 border border-slate-200';
}

function openCreateModal() {
  isEditing.value = false;
  editingId.value = null;
  editingUsedCount.value = 0;
  form.value = { code: '', name: '', type: 1 };
  isModalOpen.value = true;
}

function openEditModal(agency) {
  isEditing.value = true;
  editingId.value = agency.id;
  editingUsedCount.value = agency.usedCount || 0;
  
  let mappedType = agency.type;
  if (agency.type === 'Ministry') mappedType = 1;
  else if (agency.type === 'Province') mappedType = 2;
  else if (agency.type === 'Internal') mappedType = 3;
  else if (agency.type === 'Other') mappedType = 4;

  form.value = { code: agency.code, name: agency.name, type: mappedType };
  isModalOpen.value = true;
}

async function fetchAgencies() {
  isLoading.value = true;
  try {
    const url = new URL('http://localhost:5000/api/agencies');
    url.searchParams.append('pageNumber', pageNumber.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (appliedSearch.value.trim()) {
      url.searchParams.append('search', appliedSearch.value.trim());
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (data.items) {
        agencies.value = data.items;
        totalCount.value = data.totalCount || data.items.length;
        pageNumber.value = data.pageNumber || 1;
        pageSize.value = data.pageSize || 10;
        totalPages.value = data.totalPages || 1;
      } else {
        agencies.value = data;
        totalCount.value = data.length;
        totalPages.value = 1;
      }
    }
  } catch (e) {
    console.error('Error fetching agencies:', e);
  } finally {
    isLoading.value = false;
  }
}

async function saveAgency() {
  try {
    const payload = {
      code: form.value.code,
      name: form.value.name,
      type: Number(form.value.type),
      isActive: true
    };

    let res;
    if (isEditing.value && editingId.value) {
      res = await fetch(`http://localhost:5000/api/agencies/${editingId.value}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    } else {
      res = await fetch('http://localhost:5000/api/agencies', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    }

    if (res.ok) {
      toast.success(isEditing.value ? 'Cập nhật cơ quan thành công!' : 'Thêm mới cơ quan thành công!');
      isModalOpen.value = false;
      await fetchAgencies();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.error || err.message || 'Lỗi khi lưu cơ quan.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

async function deleteAgency(agency) {
  if (agency.usedCount > 0) {
    toast.warning(`Không thể xóa vì cơ quan "${agency.name}" đang được sử dụng bởi ${agency.usedCount} mục tiêu/nhiệm vụ/đôn đốc.`);
    return;
  }

  if (confirm(`Bạn có chắc chắn muốn xóa cơ quan "${agency.name}"?`)) {
    try {
      const res = await fetch(`http://localhost:5000/api/agencies/${agency.id}`, { method: 'DELETE' });
      if (res.ok) {
        toast.success('Đã xóa cơ quan thành công!');
        await fetchAgencies();
      } else {
        const err = await res.json().catch(() => ({}));
        toast.error(err.error || err.message || 'Lỗi khi xóa cơ quan.');
      }
    } catch (e) {
      toast.error('Không thể kết nối máy chủ.');
    }
  }
}

onMounted(() => {
  fetchAgencies();
});
</script>
