<template>
  <div class="bg-white rounded-2xl p-3.5 sm:p-4 shadow-sm border border-slate-200/80 space-y-3.5 font-sans">
    
    <!-- Top Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-3 border-b border-slate-100 pb-3">
      <div>
        <h2 class="text-lg sm:text-xl font-extrabold text-slate-800 flex items-center gap-2">
          <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5m3 0h14"/></svg>
          Danh Mục Cơ Quan & Đơn Vị Trực Thuộc
        </h2>
        <p class="text-xs text-slate-500 mt-0.5">Cơ cấu cây phân cấp các Bộ, Ngành, Địa phương và Cục, Sở, Đơn vị trực thuộc</p>
      </div>

      <div class="flex items-center gap-2">
        <button @click="expandAll" class="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition">
          ▼ Mở tất cả
        </button>
        <button @click="collapseAll" class="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition">
          ► Thu gọn
        </button>
        <button @click="openCreateModal()" class="bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs px-3.5 py-2 rounded-xl shadow-sm transition flex items-center gap-1.5 shrink-0">
          + Thêm Cơ Quan / Đơn Vị Mới
        </button>
      </div>
    </div>

    <!-- Unified Container Block for Search, Tree Table -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden w-full flex flex-col">
      
      <!-- Search Bar -->
      <div class="flex items-center justify-between gap-4 p-3 border-b border-slate-200 bg-slate-50/60 w-full">
        <div class="flex items-center gap-2 flex-1 max-w-md">
          <div class="relative w-full">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              v-model="searchDraft" 
              @keyup.enter="execSearch"
              placeholder="Tìm kiếm theo mã cơ quan, tên cơ quan..." 
              class="w-full text-xs font-semibold pl-9 pr-4 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>
          <button @click="execSearch" class="px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-2xs transition shrink-0 cursor-pointer">Tìm Kiếm</button>
          <button @click="resetSearch" class="px-3 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition shrink-0 cursor-pointer">↺</button>
        </div>
        <span class="text-xs text-slate-500 font-bold shrink-0">
          Tổng số {{ totalCount }} cơ quan / đơn vị
        </span>
      </div>

      <!-- Tree Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-340px)] custom-scrollbar flex-1 bg-white">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh mục cơ quan..." />

        <table v-else class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-extrabold border-b border-slate-200 sticky top-0 z-10">
            <tr>
              <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100">Cấu Trúc Cây Cơ Quan (Mã & Tên Đầy Đủ)</th>
              <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100">Cơ Quan Cấp Trên</th>
              <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 text-center">Phân Loại</th>
              <th class="px-3 py-2.5 text-center bg-slate-100 whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr 
              v-for="row in visibleTreeRows" 
              :key="row.id" 
              :class="[
                'hover:bg-blue-50/40 transition',
                row.level === 0 ? 'bg-white font-bold' : 'bg-slate-50/50'
              ]"
            >
              <!-- Tree Node Column with Indentation and Chevron Expand Toggle -->
              <td class="px-3 py-2 border-r border-slate-200">
                <div class="flex items-center gap-2" :style="{ paddingLeft: `${row.level * 24}px` }">
                  
                  <!-- Expand/Collapse Chevron Button -->
                  <button 
                    v-if="row.hasChildren"
                    @click="toggleExpand(row.id)"
                    class="w-5 h-5 rounded hover:bg-slate-200 text-slate-600 flex items-center justify-center transition shrink-0"
                    :title="expandedNodes.has(row.id) ? 'Thu gọn' : 'Mở rộng'"
                  >
                    <svg class="w-3.5 h-3.5 transition-transform duration-200" :class="{ 'rotate-90': expandedNodes.has(row.id) }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/>
                    </svg>
                  </button>
                  <span v-else class="w-5 shrink-0 text-slate-300 text-center font-mono">
                    {{ row.level > 0 ? '└─' : '' }}
                  </span>

                  <!-- Code Badge & Name -->
                  <span class="px-2 py-0.5 rounded-md font-black text-xs text-blue-700 bg-blue-50 border border-blue-200 shrink-0">
                    {{ row.code }}
                  </span>

                  <span class="text-xs font-bold text-slate-900 truncate">
                    {{ row.name }}
                  </span>

                  <span v-if="row.hasChildren" class="text-[10px] font-extrabold text-blue-600 bg-blue-100 px-1.5 py-0.5 rounded-full shrink-0">
                    {{ row.children.length }} đơn vị con
                  </span>
                </div>
              </td>

              <!-- Parent Agency Name -->
              <td class="px-4 py-3 border-r border-slate-200 text-xs font-semibold text-slate-600">
                {{ row.parentName || '— (Cơ quan độc lập / Bộ / Tỉnh)' }}
              </td>

              <!-- Agency Type Badge -->
              <td class="px-4 py-3 border-r border-slate-200 text-center">
                <span :class="['px-2.5 py-1 rounded-full text-xs font-extrabold', getTypeBadgeClass(row.type)]">
                  {{ getTypeLabel(row.type) }}
                </span>
              </td>

              <!-- Actions -->
              <td class="px-4 py-3 text-center space-x-1 whitespace-nowrap">
                <button @click="openCreateModal(row.id)" class="px-2.5 py-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 font-extrabold text-xs rounded-lg transition border border-emerald-200" title="Thêm Đơn vị Trực thuộc">
                  + Trực thuộc
                </button>
                <button @click="openEditModal(row)" class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition inline-flex items-center" title="Sửa">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                </button>
                <button @click="deleteAgency(row)" class="p-1.5 text-rose-600 hover:text-rose-800 hover:bg-rose-50 rounded-lg transition inline-flex items-center" title="Xóa">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </td>
            </tr>

            <tr v-if="visibleTreeRows.length === 0">
              <td colspan="4" class="p-8 text-center text-slate-400 font-semibold italic">
                Không tìm thấy cơ quan nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Server Pagination Controls -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
        <div class="flex flex-wrap items-center gap-3">
          <span>Hiển thị <span class="font-extrabold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalCount }}</span> cơ quan / đơn vị</span>
          
          <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3">
            <span>Số bản ghi/trang:</span>
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

        <div class="flex items-center gap-2">
          <button 
            @click="changePage(pageNumber - 1)" 
            :disabled="pageNumber <= 1"
            class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
          >
            ‹ Trang trước
          </button>
          
          <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-black">
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

    <!-- Create/Edit Modal with Parent Selector -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-2xl w-full p-6 sm:p-7 space-y-4">
        <h3 class="text-lg font-bold text-slate-800 border-b border-slate-100 pb-2">
          {{ isEditing ? 'Chỉnh Sửa Cơ Quan' : 'Thêm Cơ Quan / Đơn Vị Mới' }}
        </h3>

        <form @submit.prevent="saveAgency" class="space-y-3">
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Mã Cơ Quan / Đơn Vị</label>
              <input 
                v-model="form.code" 
                :disabled="isEditing && editingUsedCount > 0"
                required 
                class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1 disabled:opacity-60 disabled:bg-slate-100 disabled:cursor-not-allowed" 
              />
            </div>

            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Tên Đầy Đủ</label>
              <input v-model="form.name" required class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="form.parentId" 
                :options="parentAgencyOptions" 
                :isMulti="false" 
                label="Cơ Quan Cấp Trên (Cơ Quan Cha)" 
                placeholder="-- Không có (Cơ quan độc lập / Bộ / Tỉnh) --"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="form.type" 
                :options="agencyTypeOptions" 
                :isMulti="false" 
                label="Phân Loại" 
              />
            </div>
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
import SearchableSelect from './SearchableSelect.vue';
import { toast } from 'vue3-toastify';
import LoadingSpinner from './LoadingSpinner.vue';
import { getApiUrl } from '../config/api';

const rawAgenciesList = ref([]);
const allParentOptions = ref([]);
const isLoading = ref(true);

const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

const agencyTypeOptions = ref([
  { value: 1, label: 'Bộ / Ngành' },
  { value: 2, label: 'Tỉnh / Thành phố' },
  { value: 3, label: 'Đơn vị nội bộ / Trực thuộc (Cục, Sở, Trung tâm)' },
  { value: 4, label: 'Khác' }
]);

const parentAgencyOptions = computed(() => {
  return allParentOptions.value.map(p => ({ value: p.id, label: `${p.name} (${p.code})` }));
});

const searchDraft = ref('');
const searchQuery = ref('');
const expandedNodes = ref(new Set());

const pageNumber = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const isModalOpen = ref(false);
const isEditing = ref(false);
const editingId = ref(null);
const editingUsedCount = ref(0);

const form = ref({ code: '', name: '', parentId: null, type: 1 });

function execSearch() {
  searchQuery.value = searchDraft.value;
  pageNumber.value = 1;
  fetchAgencies();
}

function resetSearch() {
  searchDraft.value = '';
  searchQuery.value = '';
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
    3: 'Trực thuộc / Nội bộ', 'Internal': 'Trực thuộc / Nội bộ', '3': 'Trực thuộc / Nội bộ',
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

function toggleExpand(agencyId) {
  if (expandedNodes.value.has(agencyId)) {
    expandedNodes.value.delete(agencyId);
  } else {
    expandedNodes.value.add(agencyId);
  }
}

function expandAll() {
  const set = new Set();
  rawAgenciesList.value.forEach(a => set.add(a.id));
  expandedNodes.value = set;
}

function collapseAll() {
  expandedNodes.value = new Set();
}

// Build Tree Hierarchy Structure & Flattened Visible Rows
const visibleTreeRows = computed(() => {
  const list = rawAgenciesList.value || [];
  let filtered = list;

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase();
    filtered = list.filter(a => a.code.toLowerCase().includes(q) || a.name.toLowerCase().includes(q));
    // When searching, display flat matching list
    return filtered.map(a => ({
      ...a,
      level: 0,
      hasChildren: false,
      children: []
    }));
  }

  // Group by ParentId
  const agencyMap = new Map();
  list.forEach(a => agencyMap.set(a.id, { ...a, children: [] }));

  const roots = [];
  agencyMap.forEach(agency => {
    if (agency.parentId && agencyMap.has(agency.parentId)) {
      agencyMap.get(agency.parentId).children.push(agency);
    } else {
      roots.push(agency);
    }
  });

  const result = [];
  function traverse(nodes, level = 0) {
    nodes.forEach(node => {
      const hasChildren = node.children && node.children.length > 0;
      result.push({
        ...node,
        level,
        hasChildren
      });

      if (hasChildren && expandedNodes.value.has(node.id)) {
        traverse(node.children, level + 1);
      }
    });
  }

  traverse(roots);
  return result;
});

function openCreateModal(parentAgencyId = null) {
  isEditing.value = false;
  editingId.value = null;
  editingUsedCount.value = 0;
  form.value = { code: '', name: '', parentId: parentAgencyId, type: parentAgencyId ? 3 : 1 };
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

  form.value = { 
    code: agency.code, 
    name: agency.name, 
    parentId: agency.parentId || null,
    type: mappedType 
  };
  isModalOpen.value = true;
}

async function fetchAgencies() {
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/agencies'));
    url.searchParams.append('pageNumber', pageNumber.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (searchQuery.value.trim()) {
      url.searchParams.append('search', searchQuery.value.trim());
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (data.items) {
        rawAgenciesList.value = data.items;
        totalCount.value = data.totalCount || 0;
        pageNumber.value = data.pageNumber || 1;
        pageSize.value = data.pageSize || 10;
        totalPages.value = data.totalPages || 1;
      } else {
        rawAgenciesList.value = Array.isArray(data) ? data : [];
        totalCount.value = rawAgenciesList.value.length;
        totalPages.value = 1;
      }

      // Expand all root nodes by default
      const defaultExpanded = new Set();
      rawAgenciesList.value.forEach(a => {
        if (!a.parentId) defaultExpanded.add(a.id);
      });
      expandedNodes.value = defaultExpanded;
    }
  } catch (e) {
    console.error('Error fetching agencies:', e);
  } finally {
    isLoading.value = false;
  }
}

async function fetchAllParentOptions() {
  try {
    const res = await fetch(getApiUrl('/api/agencies'));
    if (res.ok) {
      const data = await res.json();
      allParentOptions.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (e) {
    console.error('Error fetching parent agency options:', e);
  }
}

async function saveAgency() {
  try {
    const payload = {
      code: form.value.code,
      name: form.value.name,
      parentId: form.value.parentId || null,
      type: Number(form.value.type),
      isActive: true
    };

    let res;
    if (isEditing.value && editingId.value) {
      res = await fetch(getApiUrl(`/api/agencies/${editingId.value}`), {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    } else {
      res = await fetch(getApiUrl('/api/agencies'), {
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
    toast.warning(`Không thể xóa vì cơ quan "${agency.name}" đang được sử dụng bởi ${agency.usedCount} mục tiêu/nhiệm vụ.`);
    return;
  }

  if (confirm(`Bạn có chắc chắn muốn xóa cơ quan "${agency.name}"?`)) {
    try {
      const res = await fetch(getApiUrl(`/api/agencies/${agency.id}`), { method: 'DELETE' });
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
  fetchAllParentOptions();
});
</script>
