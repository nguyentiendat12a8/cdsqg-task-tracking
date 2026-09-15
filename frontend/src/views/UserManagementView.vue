<template>
  <div class="w-full space-y-6 font-sans">
    
    <!-- Top Header Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div class="flex items-center gap-3">
        <span class="p-2.5 bg-blue-600 text-white rounded-xl shadow-sm font-black">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"/></svg>
        </span>
        <div>
          <h1 class="text-xl font-extrabold text-slate-800">Quản Trị Tài Khoản Người Dùng</h1>
          <p class="text-xs text-slate-500 mt-0.5">Quản lý danh sách tài khoản, phân quyền truy cập và gán cơ quan chủ trì</p>
        </div>
      </div>

      <button 
        @click="openAddModal" 
        class="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-extrabold text-xs rounded-xl shadow-sm transition flex items-center gap-2 shrink-0"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"/></svg>
        + Thêm Tài Khoản Mới
      </button>
    </div>

    <!-- Unified Card Container: Search + Users Table + Attached Pagination -->
    <div class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col w-full">
      
      <!-- Filter & Search Bar -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 p-4 bg-slate-50/60 border-b border-slate-200/80 w-full items-end">
        <!-- Search Input -->
        <div>
          <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Tìm Kiếm</label>
          <div class="relative">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              v-model="searchInput" 
              @keyup.enter="execSearch"
              placeholder="Username, họ tên, email..." 
              class="w-full text-xs font-semibold pl-9 pr-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
            />
          </div>
        </div>

        <!-- Role Filter -->
        <div>
          <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Vai Trò</label>
          <select 
            v-model="selectedRoleFilter" 
            class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
          >
            <option value="all">Tất cả vai trò</option>
            <option value="Admin">Quản trị viên (Admin)</option>
            <option value="AgencyUser">Cán bộ Cơ quan/Bộ ngành</option>
          </select>
        </div>

        <!-- Agency Filter -->
        <div>
          <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Cơ Quan Gán</label>
          <select 
            v-model="selectedAgencyFilter" 
            class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
          >
            <option value="all">Tất cả cơ quan</option>
            <option v-for="ag in agencies" :key="ag.id" :value="ag.id">
              {{ ag.code }} - {{ ag.name }}
            </option>
          </select>
        </div>

        <!-- Action Buttons -->
        <div class="flex items-center gap-2">
          <button 
            @click="execSearch" 
            class="w-full py-2 px-3 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm transition flex items-center justify-center gap-1.5"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <span>Tìm Kiếm</span>
          </button>
          <button 
            @click="resetSearch" 
            class="px-3 py-2 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition shrink-0"
            title="Đặt lại bộ lọc"
          >
            ↺
          </button>
        </div>
      </div>

      <!-- Users Table Container -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-340px)] custom-scrollbar w-full">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh sách tài khoản..." />

        <table v-else class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200">
            <tr>
              <th class="px-4 py-3 min-w-[120px]">Username</th>
              <th class="px-4 py-3 min-w-[180px]">Họ Và Tên</th>
              <th class="px-4 py-3 min-w-[160px]">Email</th>
              <th class="px-4 py-3 text-center min-w-[120px]">Vai Trò</th>
              <th class="px-4 py-3 min-w-[180px]">Cơ Quan Gán</th>
              <th class="px-4 py-3 text-center min-w-[110px]">Trạng Thái</th>
              <th class="px-4 py-3 text-center min-w-[140px]">Đăng Nhập Cuối</th>
              <th class="px-4 py-3 text-center min-w-[130px]">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="user in users" :key="user.id" class="hover:bg-slate-50 transition">
              <td class="px-4 py-3 font-extrabold text-blue-900 whitespace-nowrap">
                {{ user.username }}
              </td>

              <td class="px-4 py-3 font-bold text-slate-800">
                {{ user.fullName }}
              </td>

              <td class="px-4 py-3 text-xs text-slate-600">
                {{ user.email || '—' }}
              </td>

              <td class="px-4 py-3 text-center whitespace-nowrap">
                <span :class="['px-2.5 py-1 rounded-lg text-xs font-extrabold shadow-2xs', user.role === 'Admin' || user.role === 1 ? 'bg-purple-100 text-purple-900 border border-purple-200' : 'bg-blue-100 text-blue-900 border border-blue-200']">
                  {{ user.role === 'Admin' || user.role === 1 ? '👑 Admin' : '👤 Cán Bộ' }}
                </span>
              </td>

              <td class="px-4 py-3 text-xs font-semibold text-slate-700">
                <span v-if="user.agencyCode" class="px-2 py-0.5 bg-slate-100 text-slate-800 rounded border border-slate-200 font-bold mr-1">
                  {{ user.agencyCode }}
                </span>
                <span>{{ user.agencyName || (user.role === 'Admin' || user.role === 1 ? 'Toàn quyền Hệ thống' : 'Chưa gán') }}</span>
              </td>

              <td class="px-4 py-3 text-center whitespace-nowrap">
                <span :class="['px-2 py-0.5 rounded text-[11px] font-extrabold', user.isActive ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-rose-50 text-rose-700 border border-rose-200']">
                  {{ user.isActive ? 'Hoạt động' : 'Tạm khóa' }}
                </span>
              </td>

              <td class="px-4 py-3 text-center text-xs text-slate-500 whitespace-nowrap">
                {{ formatDate(user.lastLoginAt) }}
              </td>

              <!-- Action Buttons -->
              <td class="px-4 py-3 text-center whitespace-nowrap">
                <div class="flex items-center justify-center gap-1.5">
                  <!-- Edit Button -->
                  <button 
                    @click="openEditModal(user)"
                    class="p-1.5 text-slate-500 hover:text-blue-600 bg-slate-100 hover:bg-blue-50 rounded-lg transition"
                    title="Chỉnh sửa tài khoản"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                  </button>

                  <!-- Reset Password Button -->
                  <button 
                    @click="openResetPasswordModal(user)"
                    class="p-1.5 text-slate-500 hover:text-amber-600 bg-slate-100 hover:bg-amber-50 rounded-lg transition"
                    title="Đặt lại mật khẩu"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"/></svg>
                  </button>

                  <!-- Delete Button -->
                  <button 
                    v-if="user.username !== 'admin'"
                    @click="deleteAccount(user)"
                    class="p-1.5 text-slate-500 hover:text-rose-600 bg-slate-100 hover:bg-rose-50 rounded-lg transition"
                    title="Xóa tài khoản"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="users.length === 0">
              <td colspan="8" class="p-8 text-center text-slate-400 font-semibold italic">
                Không tìm thấy tài khoản nào phù hợp bộ lọc.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Attached Pagination Bar -->
      <div v-if="totalPages > 1" class="flex flex-col sm:flex-row items-center justify-between bg-slate-50/70 p-4 border-t border-slate-200/80 w-full gap-3 text-xs text-slate-600 font-semibold">
        <span>
          Hiển thị {{ (pageNumber - 1) * pageSize + 1 }} - {{ Math.min(pageNumber * pageSize, totalCount) }} / {{ totalCount }} tài khoản
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

    <!-- Add Account Modal -->
    <div v-if="isAddModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-md w-full p-6 space-y-4 font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-800">+ Thêm Tài Khoản Mới</h3>
          <button @click="isAddModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg">✕</button>
        </div>

        <form @submit.prevent="saveNewUser" class="space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Username *</label>
            <input v-model="newUserForm.username" type="text" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Mật khẩu *</label>
            <input v-model="newUserForm.password" type="password" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Họ và Tên *</label>
            <input v-model="newUserForm.fullName" type="text" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Email</label>
            <input v-model="newUserForm.email" type="email" class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Vai Trò</label>
            <select v-model="newUserForm.role" class="w-full text-xs font-bold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none">
              <option value="Admin">Quản trị viên (Admin)</option>
              <option value="AgencyUser">Cán bộ Cơ quan/Bộ ngành</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Cơ Quan Chủ Trì Gán</label>
            <select v-model="newUserForm.agencyId" class="w-full text-xs font-bold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none">
              <option :value="null">Không chọn (Hoặc Admin)</option>
              <option v-for="ag in agencies" :key="ag.id" :value="ag.id">{{ ag.code }} - {{ ag.name }}</option>
            </select>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="isAddModalOpen = false" class="px-4 py-2 bg-slate-100 text-slate-600 font-bold text-xs rounded-xl">Hủy</button>
            <button type="submit" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm">Tạo Tài Khoản</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Account Modal -->
    <div v-if="isEditModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-md w-full p-6 space-y-4 font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-800">Chỉnh Sửa Tài Khoản: {{ editUserForm.username }}</h3>
          <button @click="isEditModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg">✕</button>
        </div>

        <form @submit.prevent="saveEditUser" class="space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Họ và Tên</label>
            <input v-model="editUserForm.fullName" type="text" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Email</label>
            <input v-model="editUserForm.email" type="email" class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Vai Trò</label>
            <select v-model="editUserForm.role" class="w-full text-xs font-bold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none">
              <option value="Admin">Quản trị viên (Admin)</option>
              <option value="AgencyUser">Cán bộ Cơ quan/Bộ ngành</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Cơ Quan Gán</label>
            <select v-model="editUserForm.agencyId" class="w-full text-xs font-bold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none">
              <option :value="null">Không gán (Toàn quyền)</option>
              <option v-for="ag in agencies" :key="ag.id" :value="ag.id">{{ ag.code }} - {{ ag.name }}</option>
            </select>
          </div>

          <div class="flex items-center gap-2 pt-1">
            <input type="checkbox" id="userActiveChk" v-model="editUserForm.isActive" class="rounded border-slate-300 text-blue-600 w-4 h-4" />
            <label for="userActiveChk" class="text-xs font-bold text-slate-700 cursor-pointer">Tài khoản hoạt động (Active)</label>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="isEditModalOpen = false" class="px-4 py-2 bg-slate-100 text-slate-600 font-bold text-xs rounded-xl">Hủy</button>
            <button type="submit" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm">Lưu Thay Đổi</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Reset Password Modal -->
    <div v-if="isResetModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-sm w-full p-6 space-y-4 font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-800">🔑 Đặt Mật Khẩu Mới</h3>
          <button @click="isResetModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg">✕</button>
        </div>

        <p class="text-xs text-slate-600 font-semibold">Đặt lại mật khẩu mới cho tài khoản: <strong class="text-blue-900 font-extrabold">{{ selectedUserForReset?.username }}</strong></p>

        <form @submit.prevent="saveResetPassword" class="space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Mật khẩu mới *</label>
            <input v-model="newPasswordInput" type="password" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="isResetModalOpen = false" class="px-4 py-2 bg-slate-100 text-slate-600 font-bold text-xs rounded-xl">Hủy</button>
            <button type="submit" class="px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow-sm">Cập Nhật Mật Khẩu</button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { getApiUrl } from '../config/api';

const users = ref([]);
const agencies = ref([]);
const isLoading = ref(true);

const searchInput = ref('');
const searchQuery = ref('');
const selectedRoleFilter = ref('all');
const selectedAgencyFilter = ref('all');

const pageNumber = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const isAddModalOpen = ref(false);
const isEditModalOpen = ref(false);
const isResetModalOpen = ref(false);

const newUserForm = ref({
  username: '',
  password: '',
  fullName: '',
  email: '',
  role: 'AgencyUser',
  agencyId: null
});

const editUserForm = ref({
  id: '',
  username: '',
  fullName: '',
  email: '',
  role: 'AgencyUser',
  agencyId: null,
  isActive: true
});

const selectedUserForReset = ref(null);
const newPasswordInput = ref('');

function formatDate(dateStr) {
  if (!dateStr) return 'Chưa đăng nhập';
  try {
    return new Date(dateStr).toLocaleString('vi-VN');
  } catch {
    return dateStr;
  }
}

async function loadAgencies() {
  try {
    const res = await fetch(getApiUrl('/api/agency'));
    if (res.ok) {
      agencies.value = await res.json();
    }
  } catch (e) {
    console.error('Lỗi khi tải danh sách cơ quan:', e);
  }
}

async function fetchUsers() {
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/user'));
    url.searchParams.append('pageNumber', pageNumber.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (searchQuery.value.trim()) url.searchParams.append('search', searchQuery.value.trim());
    if (selectedRoleFilter.value !== 'all') url.searchParams.append('role', selectedRoleFilter.value);
    if (selectedAgencyFilter.value !== 'all') url.searchParams.append('agencyId', selectedAgencyFilter.value);

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      users.value = data.items || [];
      totalCount.value = data.totalCount || 0;
      totalPages.value = data.totalPages || 1;
    }
  } catch (e) {
    toast.error('Lỗi kết nối máy chủ khi lấy danh sách người dùng.');
  } finally {
    isLoading.value = false;
  }
}

function execSearch() {
  searchQuery.value = searchInput.value;
  pageNumber.value = 1;
  fetchUsers();
}

function resetSearch() {
  searchInput.value = '';
  searchQuery.value = '';
  selectedRoleFilter.value = 'all';
  selectedAgencyFilter.value = 'all';
  pageNumber.value = 1;
  fetchUsers();
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  pageNumber.value = newPage;
  fetchUsers();
}

function openAddModal() {
  newUserForm.value = {
    username: '',
    password: '',
    fullName: '',
    email: '',
    role: 'AgencyUser',
    agencyId: null
  };
  isAddModalOpen.value = true;
}

async function saveNewUser() {
  try {
    const res = await fetch(getApiUrl('/api/user'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(newUserForm.value)
    });

    if (res.ok) {
      toast.success(`Đã tạo tài khoản '${newUserForm.value.username}' thành công.`);
      isAddModalOpen.value = false;
      fetchUsers();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || 'Lỗi khi tạo tài khoản.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ để tạo tài khoản.');
  }
}

function openEditModal(user) {
  editUserForm.value = {
    id: user.id,
    username: user.username,
    fullName: user.fullName,
    email: user.email,
    role: user.role,
    agencyId: user.agencyId,
    isActive: user.isActive
  };
  isEditModalOpen.value = true;
}

async function saveEditUser() {
  try {
    const res = await fetch(getApiUrl(`/api/user/${editUserForm.value.id}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(editUserForm.value)
    });

    if (res.ok) {
      toast.success(`Đã cập nhật thông tin tài khoản '${editUserForm.value.username}'.`);
      isEditModalOpen.value = false;
      fetchUsers();
    } else {
      toast.error('Lỗi khi cập nhật tài khoản.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

function openResetPasswordModal(user) {
  selectedUserForReset.value = user;
  newPasswordInput.value = '';
  isResetModalOpen.value = true;
}

async function saveResetPassword() {
  if (!selectedUserForReset.value) return;
  try {
    const res = await fetch(getApiUrl(`/api/user/${selectedUserForReset.value.id}/reset-password`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ newPassword: newPasswordInput.value })
    });

    if (res.ok) {
      toast.success(`Đã đặt lại mật khẩu cho '${selectedUserForReset.value.username}'.`);
      isResetModalOpen.value = false;
    } else {
      toast.error('Lỗi khi đặt lại mật khẩu.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

async function deleteAccount(user) {
  if (!confirm(`Bạn có chắc chắn muốn xóa tài khoản '${user.username}'?`)) return;
  try {
    const res = await fetch(getApiUrl(`/api/user/${user.id}`), { method: 'DELETE' });
    if (res.ok) {
      toast.success(`Đã xóa tài khoản '${user.username}'.`);
      fetchUsers();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || 'Lỗi khi xóa tài khoản.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

onMounted(() => {
  loadAgencies();
  fetchUsers();
});
</script>
