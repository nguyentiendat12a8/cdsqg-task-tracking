<template>
  <div class="w-full space-y-6 font-sans">
    
    <!-- Top Header Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div class="flex items-center gap-3">
        <span class="p-2.5 bg-blue-600 text-white rounded-xl shadow-sm font-bold">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"/></svg>
        </span>
        <div>
          <h1 class="text-xl font-bold text-slate-800">Quản Trị Tài Khoản Người Dùng</h1>
          <p class="text-xs text-slate-500 mt-0.5">Quản lý danh sách tài khoản, phân quyền truy cập và gán cơ quan chủ trì</p>
        </div>
      </div>

      <button 
        @click="openAddModal" 
        class="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm transition flex items-center gap-2 shrink-0 cursor-pointer"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"/></svg>
        Thêm Tài Khoản Mới
      </button>
    </div>

    <!-- Unified Card Container: Search + Users Table + Attached Pagination -->
    <div class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col w-full">
      
      <!-- Filter & Search Bar -->
      <div class="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3 p-3 bg-slate-50/60 border-b border-slate-200/80 w-full">
        <div class="flex items-center gap-2 flex-1 max-w-xl min-w-0">
          <!-- Search Input -->
          <div class="relative flex-1 min-w-[200px]">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              :value="searchInput" 
              @input="searchInput = $event.target.value"
              placeholder="Tìm theo Username, họ tên, email..." 
              class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none h-[34px]"
            />
          </div>

          <!-- OverlayPanel Advanced Filter -->
          <OverlayPanel
            title="Lọc Tài Khoản Nâng Cao"
            buttonText="Lọc Nâng Cao"
            :activeCount="activeFilterCount"
            widthClass="w-[320px] sm:w-[420px]"
            @apply="execSearch"
            @reset="resetSearch"
          >
            <div class="space-y-3">
              <div>
                <SearchableSelect 
                  v-model="selectedRoleFilters" 
                  :options="roleOptions" 
                  :isMulti="true" 
                  label="Phân Loại Vai Trò" 
                  placeholder="Tất cả vai trò" 
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="selectedAgencyFilters" 
                  :options="leadAgencyOptions" 
                  :isMulti="true" 
                  label="Cơ Quan Chủ Trì" 
                  placeholder="Tất cả cơ quan chủ trì" 
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="selectedSubAgencyFilters" 
                  :options="subAgencyOptions" 
                  :isMulti="true" 
                  label="Đơn Vị Trực Thuộc" 
                  placeholder="Tất cả đơn vị trực thuộc" 
                />
              </div>
            </div>
          </OverlayPanel>
        </div>

        <span class="text-xs text-slate-500 font-bold shrink-0">
          Tổng số {{ totalCount }} tài khoản
        </span>
      </div>

      <!-- Users Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar flex-1 bg-white">
        <LoadingSpinner v-if="isLoading" text="Đang tải danh sách tài khoản..." />

        <table v-else class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-10">
            <tr>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 min-w-[120px]">Tên Đăng Nhập</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 min-w-[180px]">Họ và Tên</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 min-w-[160px]">Email</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 text-center min-w-[120px]">Vai Trò</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 min-w-[180px]">Cơ Quan Chủ Trì Gán</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 text-center min-w-[110px]">Trạng Thái</th>
              <th class="px-4 py-3 text-center bg-slate-100 whitespace-nowrap min-w-[130px]">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr 
              v-for="u in users" 
              :key="u.id" 
              class="hover:bg-blue-50/40 transition"
            >
              <td class="px-4 py-3 border-r border-slate-200 font-normal text-slate-800 text-xs whitespace-nowrap">
                {{ u.username }}
              </td>

              <td class="px-4 py-3 border-r border-slate-200 font-normal text-xs text-slate-800">
                {{ u.fullName }}
              </td>

              <td class="px-4 py-3 border-r border-slate-200 text-xs text-slate-600">
                {{ u.email || '—' }}
              </td>

              <td class="px-4 py-3 border-r border-slate-200 text-center whitespace-nowrap">
                <span :class="['px-2.5 py-1 rounded-full text-xs font-medium', getRoleBadgeClass(u.role)]">
                  {{ getRoleLabel(u.role) }}
                </span>
              </td>

              <td class="px-4 py-3 border-r border-slate-200 text-xs font-normal text-slate-700">
                <span v-if="u.agencyName" class="px-2 py-0.5 bg-blue-50 text-blue-700 rounded border border-blue-200 font-normal">
                  {{ u.agencyName }}
                </span>
                <span v-else class="text-slate-400 italic">Tất cả (Admin)</span>
              </td>

              <td class="px-4 py-3 border-r border-slate-200 text-center whitespace-nowrap">
                <span :class="['px-2.5 py-0.5 rounded-full text-[11px] font-medium', u.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800']">
                  {{ u.isActive ? 'Hoạt động' : 'Đã khóa' }}
                </span>
              </td>

              <td class="px-4 py-3 text-center whitespace-nowrap">
                <div class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap">
                  <button @click="openResetModal(u)" class="px-2.5 py-1 bg-amber-50 hover:bg-amber-100 text-amber-700 font-bold text-xs rounded-lg transition border border-amber-200 cursor-pointer" title="Đặt lại mật khẩu">
                    🔑 Mật khẩu
                  </button>
                  <button @click="openEditModal(u)" class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition inline-flex items-center cursor-pointer" title="Sửa thông tin">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                  </button>
                  <button v-if="u.username !== 'admin'" @click="deleteUser(u)" class="p-1.5 text-rose-600 hover:text-rose-800 hover:bg-rose-50 rounded-lg transition inline-flex items-center cursor-pointer" title="Xóa tài khoản">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="users.length === 0">
              <td colspan="7" class="p-8 text-center text-slate-400 font-semibold italic">
                Không tìm thấy tài khoản người dùng nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Attached Pagination Bar -->
      <div class="flex flex-col md:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold w-full">
        <div class="flex items-center gap-3 whitespace-nowrap flex-wrap justify-center sm:justify-start">
          <span class="whitespace-nowrap">Hiển thị <span class="font-bold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ totalCount }}</span> tài khoản</span>
          
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

    <!-- Add Account Modal -->
    <div v-if="isAddModalOpen" @click.self="isAddModalOpen = false" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-xl w-full p-6 space-y-4 font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-bold text-slate-800">Thêm Tài Khoản Mới</h3>
          <button @click="isAddModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg cursor-pointer">✕</button>
        </div>

        <form @submit.prevent="saveNewUser" class="space-y-3">
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Tên Đăng Nhập <span class="text-rose-500">*</span></label>
              <input v-model="newUserForm.username" type="text" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Mật Khẩu <span class="text-rose-500">*</span></label>
              <div class="relative">
                <input 
                  v-model="newUserForm.password" 
                  :type="showAddPassword ? 'text' : 'password'" 
                  required 
                  class="w-full text-xs font-semibold p-2.5 pr-9 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" 
                />
                <button 
                  type="button" 
                  @click="showAddPassword = !showAddPassword" 
                  class="absolute right-2.5 top-2.5 text-slate-400 hover:text-slate-600 focus:outline-none cursor-pointer"
                  :title="showAddPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'"
                >
                  <svg v-if="!showAddPassword" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                  </svg>
                  <svg v-else class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858-5.908a10.046 10.046 0 013.122-.463c4.478 0 8.268 2.943 9.542 7a9.97 9.97 0 01-2.163 3.652M3 3l18 18"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Họ và Tên <span class="text-rose-500">*</span></label>
              <input v-model="newUserForm.fullName" type="text" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Email</label>
              <input v-model="newUserForm.email" type="email" class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="newUserForm.role" 
                :options="roleModalOptions" 
                :isMulti="false" 
                label="Vai Trò" 
                placeholder="Chọn vai trò"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="newUserForm.agencyId" 
                :options="agencyOptions" 
                :isMulti="false" 
                :disabled="newUserForm.role === 'Admin'"
                label="Cơ Quan Gán" 
                :placeholder="newUserForm.role === 'Admin' ? 'Không gán (Toàn quyền Admin)' : '-- Chọn cơ quan gán --'"
              />
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="isAddModalOpen = false" class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl cursor-pointer">Hủy</button>
            <button type="submit" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm cursor-pointer">Tạo Tài Khoản</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Account Modal -->
    <div v-if="isEditModalOpen" @click.self="isEditModalOpen = false" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-xl w-full p-6 space-y-4 font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-bold text-slate-800">Chỉnh Sửa Tài Khoản: {{ editUserForm.username }}</h3>
          <button @click="isEditModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg cursor-pointer">✕</button>
        </div>

        <form @submit.prevent="saveEditUser" class="space-y-3">
          <div v-if="editUserForm.hasDataOperations" class="p-3 bg-amber-50 border border-amber-200 text-amber-800 rounded-xl text-xs font-semibold flex items-start gap-2">
            <svg class="w-4 h-4 text-amber-600 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/></svg>
            <span>Tài khoản này đã phát sinh thao tác dữ liệu trong hệ thống, không được phép thay đổi <strong>Vai trò</strong> và <strong>Cơ quan gắn</strong>.</span>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Họ và Tên <span class="text-rose-500">*</span></label>
              <input v-model="editUserForm.fullName" type="text" required class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Email</label>
              <input v-model="editUserForm.email" type="email" class="w-full text-xs font-semibold p-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="editUserForm.role" 
                :options="roleModalOptions" 
                :isMulti="false" 
                :disabled="editUserForm.hasDataOperations"
                label="Vai Trò" 
                placeholder="Chọn vai trò"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="editUserForm.agencyId" 
                :options="agencyOptions" 
                :isMulti="false" 
                :disabled="editUserForm.role === 'Admin' || editUserForm.hasDataOperations"
                label="Cơ Quan Gán" 
                :placeholder="editUserForm.role === 'Admin' ? 'Không gán (Toàn quyền Admin)' : '-- Chọn cơ quan gán --'"
              />
            </div>
          </div>

          <div class="flex items-center gap-2 pt-1">
            <input type="checkbox" id="userActiveChk" v-model="editUserForm.isActive" class="rounded border-slate-300 text-blue-600 w-4 h-4 cursor-pointer" />
            <label for="userActiveChk" class="text-xs font-bold text-slate-700 cursor-pointer">Tài khoản hoạt động (Active)</label>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="isEditModalOpen = false" class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl cursor-pointer">Hủy</button>
            <button type="submit" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm cursor-pointer">Lưu Thay Đổi</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Reset Password Modal -->
    <div v-if="isResetModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-sm w-full p-6 space-y-4 font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-bold text-slate-800">🔑 Đặt Mật Khẩu Mới</h3>
          <button @click="isResetModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg cursor-pointer">✕</button>
        </div>

        <p class="text-xs text-slate-600 font-semibold">Đặt lại mật khẩu mới cho tài khoản: <strong class="text-blue-900 font-bold">{{ selectedUserForReset?.username }}</strong></p>

        <form @submit.prevent="saveResetPassword" class="space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Mật khẩu mới <span class="text-rose-500">*</span></label>
            <div class="relative">
              <input 
                v-model="newPasswordInput" 
                :type="showResetPassword ? 'text' : 'password'" 
                required 
                class="w-full text-xs font-semibold p-2.5 pr-9 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none" 
              />
              <button 
                type="button" 
                @click="showResetPassword = !showResetPassword" 
                class="absolute right-2.5 top-2.5 text-slate-400 hover:text-slate-600 focus:outline-none cursor-pointer"
                :title="showResetPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'"
              >
                <svg v-if="!showResetPassword" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <svg v-else class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858-5.908a10.046 10.046 0 013.122-.463c4.478 0 8.268 2.943 9.542 7a9.97 9.97 0 01-2.163 3.652M3 3l18 18"/>
                </svg>
              </button>
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
            <button type="button" @click="isResetModalOpen = false" class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl cursor-pointer">Hủy</button>
            <button type="submit" class="px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow-sm cursor-pointer">Cập Nhật Mật Khẩu</button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import OverlayPanel from '../components/OverlayPanel.vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { getApiUrl } from '../config/api';
import { confirmModal } from '../services/confirm';

const users = ref([]);
const agencies = ref([]);
const isLoading = ref(true);

const searchInput = ref('');
const searchQuery = ref('');
const selectedRoleFilters = ref([]);
const selectedAgencyFilters = ref([]);
const selectedSubAgencyFilters = ref([]);

const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedRoleFilters.value?.length) count++;
  if (selectedAgencyFilters.value?.length) count++;
  if (selectedSubAgencyFilters.value?.length) count++;
  return count;
});

const roleOptions = ref([
  { value: 'Admin', label: 'Quản trị viên (Admin)' },
  { value: 'AgencyUser', label: 'Cán bộ Cơ quan/Bộ ngành' }
]);

const roleModalOptions = ref([
  { value: 'Admin', label: 'Quản trị viên (Admin)' },
  { value: 'AgencyUser', label: 'Cán bộ Cơ quan/Bộ ngành' }
]);

const isSpecialAgencyCode = (code) => code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND';

const formatAgencyLabel = (ag, list) => {
  if (isSpecialAgencyCode(ag.code)) {
    return `🌐 ${ag.name}`;
  }
  if (ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000') {
    const parentAg = list.find(p => p.id === ag.parentId);
    return parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name;
  }
  return ag.name;
};

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: formatAgencyLabel(ag, agencies.value) }));
});

const leadAgencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: formatAgencyLabel(ag, agencies.value) }));
});

const subAgencyOptions = computed(() => {
  return agencies.value
    .filter(ag => ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000')
    .map(ag => ({
      value: ag.id,
      label: formatAgencyLabel(ag, agencies.value)
    }));
});

const pageNumber = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const isAddModalOpen = ref(false);
const isEditModalOpen = ref(false);
const isResetModalOpen = ref(false);

const showAddPassword = ref(false);
const showResetPassword = ref(false);

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

watch(() => newUserForm.value.role, (newRole) => {
  if (newRole === 'Admin') {
    newUserForm.value.agencyId = null;
  }
});

watch(() => editUserForm.value.role, (newRole) => {
  if (newRole === 'Admin') {
    editUserForm.value.agencyId = null;
  }
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

let userFetchRequestId = 0;
let userSearchDebounceTimer = null;

async function fetchUsers() {
  const currentRequestId = ++userFetchRequestId;
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/user'));
    url.searchParams.append('pageNumber', pageNumber.value);
    url.searchParams.append('pageSize', pageSize.value);
    if (searchQuery.value.trim()) url.searchParams.append('search', searchQuery.value.trim());
    
    if (selectedRoleFilters.value && selectedRoleFilters.value.length > 0) {
      url.searchParams.append('role', selectedRoleFilters.value[0]);
    }
    if (selectedAgencyFilters.value && selectedAgencyFilters.value.length > 0) {
      url.searchParams.append('agencyId', selectedAgencyFilters.value[0]);
    }
    if (selectedSubAgencyFilters.value && selectedSubAgencyFilters.value.length > 0) {
      url.searchParams.append('subAgencyId', selectedSubAgencyFilters.value[0]);
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (currentRequestId !== userFetchRequestId) return;
      users.value = data.items || [];
      totalCount.value = data.totalCount || 0;
      totalPages.value = data.totalPages || 1;
    }
  } catch (e) {
    if (currentRequestId !== userFetchRequestId) return;
    toast.error('Lỗi kết nối máy chủ khi lấy danh sách người dùng.');
  } finally {
    if (currentRequestId === userFetchRequestId) {
      isLoading.value = false;
    }
  }
}

function execSearch() {
  if (userSearchDebounceTimer) clearTimeout(userSearchDebounceTimer);
  userFetchRequestId++;
  searchQuery.value = searchInput.value;
  pageNumber.value = 1;
  fetchUsers();
}

watch(searchInput, () => {
  if (userSearchDebounceTimer) clearTimeout(userSearchDebounceTimer);
  userSearchDebounceTimer = setTimeout(() => {
    execSearch();
  }, 300);
});

function resetSearch() {
  if (userSearchDebounceTimer) clearTimeout(userSearchDebounceTimer);
  userFetchRequestId++;
  searchInput.value = '';
  searchQuery.value = '';
  selectedRoleFilters.value = [];
  selectedAgencyFilters.value = [];
  selectedSubAgencyFilters.value = [];
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
    const payload = {
      username: newUserForm.value.username,
      password: newUserForm.value.password,
      fullName: newUserForm.value.fullName,
      email: newUserForm.value.email,
      roleString: newUserForm.value.role,
      agencyId: newUserForm.value.agencyId || null
    };

    const res = await fetch(getApiUrl('/api/user'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
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
  let mappedRole = 'AgencyUser';
  if (user.role === 'Admin' || user.roleName === 'Admin' || user.role === 1 || user.role === '1' || user.role === 0 || user.role === '0') {
    mappedRole = 'Admin';
  }

  editUserForm.value = {
    id: user.id,
    username: user.username,
    fullName: user.fullName,
    email: user.email,
    role: mappedRole,
    agencyId: mappedRole === 'Admin' ? null : user.agencyId,
    isActive: user.isActive,
    hasDataOperations: user.hasDataOperations || false
  };
  isEditModalOpen.value = true;
}

async function saveEditUser() {
  try {
    const payload = {
      fullName: editUserForm.value.fullName,
      email: editUserForm.value.email,
      roleString: editUserForm.value.role,
      agencyId: editUserForm.value.role === 'Admin' ? null : (editUserForm.value.agencyId || null),
      isActive: editUserForm.value.isActive
    };

    const res = await fetch(getApiUrl(`/api/user/${editUserForm.value.id}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      toast.success(`Đã cập nhật thông tin tài khoản '${editUserForm.value.username}'.`);
      isEditModalOpen.value = false;
      fetchUsers();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || 'Lỗi khi cập nhật tài khoản.');
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
  const confirmed = await confirmModal({
    title: 'Xóa tài khoản người dùng',
    message: `Bạn có chắc chắn muốn xóa tài khoản '${user.username}'? Thao tác này không thể hoàn tác.`,
    confirmText: 'Xóa tài khoản',
    cancelText: 'Hủy bỏ',
    type: 'danger'
  });

  if (!confirmed) return;

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

function getRoleLabel(role) {
  if (role === 'Admin' || role === 1 || role === '1' || role === 0 || role === '0') return 'Quản trị viên (Admin)';
  if (role === 'AgencyUser' || role === 2 || role === '2') return 'Cán bộ Cơ quan/Bộ ngành';
  return 'Cán bộ Cơ quan/Bộ ngành';
}

function getRoleBadgeClass(role) {
  if (role === 'Admin' || role === 1 || role === '1' || role === 0 || role === '0') return 'bg-purple-100 text-purple-800 border border-purple-200';
  return 'bg-blue-100 text-blue-800 border border-blue-200';
}

const openResetModal = openResetPasswordModal;
const deleteUser = deleteAccount;

onMounted(() => {
  loadAgencies();
  fetchUsers();
});
</script>
