<template>
  <div class="bg-white rounded-2xl p-3.5 sm:p-4 shadow-sm border border-slate-200/80 space-y-3.5 font-sans">
    
    <!-- Top Bar -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-3 border-b border-slate-100 pb-3">
      <div>
        <h2 class="text-lg sm:text-xl font-bold text-slate-800 flex items-center gap-2">
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
      <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 p-3 border-b border-slate-200 bg-slate-50/60 w-full">
        <div class="flex items-center gap-2 flex-1 min-w-0 max-w-md">
          <div class="relative w-full min-w-[180px]">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
            <input 
              :value="searchDraft" 
              @input="searchDraft = $event.target.value"
              placeholder="Tìm kiếm theo tên cơ quan..." 
              class="w-full text-xs font-semibold pl-9 pr-4 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>
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
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-10">
            <tr>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 min-w-[320px] w-auto">Cấu Trúc Cây Cơ Quan & Đơn Vị Trực Thuộc</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 text-center min-w-[180px] w-48 whitespace-nowrap">Phân Loại</th>
              <th class="px-4 py-3 border-r border-slate-200 bg-slate-100 text-center min-w-[280px] w-72 whitespace-nowrap">Thông Tin Đầu Mối Liên Hệ</th>
              <th class="px-4 py-3 text-center bg-slate-100 whitespace-nowrap min-w-[110px] w-28">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr 
              v-for="row in visibleTreeRows" 
              :key="row.id"
              class="hover:bg-blue-50/40 transition text-xs"
            >
              <!-- Hierarchy Tree Node Column -->
              <td class="px-4 py-3 border-r border-slate-200 min-w-[320px]">
                <div class="flex items-center gap-2" :style="{ paddingLeft: `${row.level * 24}px` }">
                  <button 
                    v-if="row.hasChildren"
                    @click="toggleNode(row.id)"
                    class="w-5 h-5 rounded hover:bg-slate-200 text-slate-600 flex items-center justify-center transition shrink-0 cursor-pointer"
                    :title="expandedNodes.has(row.id) ? 'Thu gọn' : 'Mở rộng'"
                  >
                    <svg class="w-3.5 h-3.5 transition-transform duration-200" :class="{ 'rotate-90': expandedNodes.has(row.id) }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/>
                    </svg>
                  </button>
                  <span v-else class="w-5 shrink-0 text-slate-300 text-center font-mono">
                    {{ row.level > 0 ? '└─' : '' }}
                  </span>

                  <!-- Name -->
                  <span class="text-xs font-normal text-slate-800 truncate">
                    {{ row.name }}
                  </span>

                  <span v-if="row.hasChildren" class="text-[10px] font-medium text-blue-600 bg-blue-100 px-1.5 py-0.5 rounded-full shrink-0">
                    {{ row.children.length }} đơn vị con
                  </span>
                </div>
              </td>

              <!-- Agency Type Badge -->
              <td class="px-4 py-3 border-r border-slate-200 text-center min-w-[180px] w-48">
                <span :class="['px-2.5 py-1 rounded-full text-xs font-medium', getTypeBadgeClass(row)]">
                  {{ getTypeLabel(row) }}
                </span>
              </td>

              <!-- Contact Persons Column (Clickable Badge Button to View Details) -->
              <td class="px-4 py-3 border-r border-slate-200 text-center min-w-[280px] w-72">
                <button 
                  v-if="row.contactPersons && row.contactPersons.length > 0"
                  @click="openContactPersonsModal(row)"
                  class="px-2.5 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-medium text-xs rounded-xl transition border border-blue-200 inline-flex items-center gap-1.5 shadow-2xs cursor-pointer"
                  title="Click để xem chi tiết cán bộ đầu mối liên hệ"
                >
                  <svg class="w-3.5 h-3.5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/></svg>
                  <span>{{ row.contactPersons.length }} cán bộ đầu mối</span>
                </button>
                <span v-else class="text-slate-400 italic text-xs">—</span>
              </td>

              <!-- Actions -->
              <td class="px-4 py-3 text-center space-x-1 whitespace-nowrap min-w-[110px] w-28">
                <template v-if="isBKHCN(row)">
                  <span class="inline-flex items-center gap-1 text-[11px] font-bold text-amber-700 bg-amber-50 px-2.5 py-1 rounded-lg border border-amber-200 shadow-2xs" title="Bộ Khoa học và Công nghệ là cơ quan hệ thống cố định (Không thể sửa/xóa)">
                    🔒 Cố định
                  </span>
                </template>
                <template v-else>
                  <button @click="openEditModal(row)" class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition inline-flex items-center cursor-pointer" title="Sửa">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                  </button>
                  <button @click="deleteAgency(row)" class="p-1.5 text-rose-600 hover:text-rose-800 hover:bg-rose-50 rounded-lg transition inline-flex items-center cursor-pointer" title="Xóa">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                  </button>
                </template>
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
      <div class="flex flex-col md:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold w-full">
        <div class="flex items-center gap-3 whitespace-nowrap flex-wrap justify-center sm:justify-start">
          <span class="whitespace-nowrap">Hiển thị <span class="font-bold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ totalCount }}</span> cơ quan / đơn vị</span>
          
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

    <!-- Create/Edit Modal with Parent Selector & Contact Persons -->
    <div v-if="isModalOpen" @click.self="closeModal" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 sm:p-7 space-y-4 max-h-[90vh] flex flex-col">
        <h3 class="text-lg font-bold text-slate-800 border-b border-slate-100 pb-2 shrink-0">
          {{ isEditing ? 'Chỉnh Sửa Cơ Quan' : 'Thêm Cơ Quan / Đơn Vị Mới' }}
        </h3>

        <form @submit.prevent="saveAgency" class="space-y-4 flex-1 overflow-y-auto pr-1">
          <div class="grid grid-cols-3 gap-3">
            <div class="col-span-2">
              <label class="text-xs font-bold text-slate-700 uppercase">Tên Đầy Đủ Cơ Quan / Đơn Vị <span class="text-rose-500">*</span></label>
              <input v-model="form.name" @input="onNameInput" required placeholder="Nhập tên cơ quan / đơn vị..." class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1 focus:bg-white focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Mã Cơ Quan / Viết Tắt <span class="text-rose-500">*</span></label>
              <input v-model="form.code" required placeholder="VD: TTCDS (Tự động)..." class="w-full text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1 focus:bg-white focus:ring-2 focus:ring-blue-500" />
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
              <template v-if="form.parentId">
                <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Phân Loại</label>
                <div class="h-[34px] min-h-[34px] max-h-[34px] px-2.5 bg-slate-100 text-slate-600 font-bold text-xs rounded-xl border border-slate-200 flex items-center gap-1.5 cursor-not-allowed">
                  <span class="w-2 h-2 rounded-full bg-emerald-500 shrink-0"></span>
                  <span class="truncate">Đơn vị trực thuộc (Tự động theo cơ quan cấp trên)</span>
                </div>
              </template>
              <template v-else>
                <SearchableSelect 
                  v-model="form.type" 
                  :options="agencyTypeOptions" 
                  :isMulti="false" 
                  label="Phân Loại" 
                />
              </template>
            </div>
          </div>

          <!-- Contact Persons Section -->
          <div class="border border-slate-200 rounded-xl p-3.5 bg-slate-50/50 space-y-3">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold text-slate-800 uppercase flex items-center gap-1.5">
                <svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/></svg>
                Thông Tin Cán Bộ Đầu Mối (Liên Hệ)
              </label>
              <button 
                type="button" 
                @click="addContactPerson" 
                class="px-2.5 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-xs rounded-lg transition border border-blue-200 flex items-center gap-1"
              >
                + Thêm cán bộ đầu mối
              </button>
            </div>

            <div v-if="!form.contactPersons || form.contactPersons.length === 0" class="text-xs text-slate-400 italic text-center py-2">
              Chưa có cán bộ đầu mối. Nhấn nút trên để thêm.
            </div>

            <div v-else class="space-y-3 max-h-60 overflow-y-auto pr-1">
              <div 
                v-for="(person, idx) in form.contactPersons" 
                :key="idx" 
                class="bg-white p-3 rounded-xl border border-slate-200 shadow-2xs space-y-2 relative"
              >
                <div class="flex items-center justify-between border-b border-slate-100 pb-1.5">
                  <span class="text-xs font-bold text-blue-700">Cán bộ đầu mối #{{ idx + 1 }}</span>
                  <button 
                    type="button" 
                    @click="removeContactPerson(idx)" 
                    class="text-rose-500 hover:text-rose-700 text-xs font-bold hover:bg-rose-50 px-2 py-0.5 rounded transition"
                  >
                    ✕ Xóa
                  </button>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
                  <div>
                    <label class="text-[11px] font-bold text-slate-600">Họ và tên</label>
                    <input 
                      v-model="person.name" 
                      placeholder="Phạm Quang Cường" 
                      class="w-full text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                  <div>
                    <label class="text-[11px] font-bold text-slate-600">Chức vụ</label>
                    <input 
                      v-model="person.position" 
                      placeholder="Phó giám đốc" 
                      class="w-full text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                  <div>
                    <label class="text-[11px] font-bold text-slate-600">Phòng ban / Đơn vị</label>
                    <input 
                      v-model="person.department" 
                      placeholder="Sở KHCN" 
                      class="w-full text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <div>
                    <label class="text-[11px] font-bold text-slate-600">Số điện thoại</label>
                    <input 
                      v-model="person.phone" 
                      placeholder="0976 819 323" 
                      class="w-full text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                  <div>
                    <label class="text-[11px] font-bold text-slate-600">Email</label>
                    <input 
                      v-model="person.email" 
                      placeholder="cuongpq.sokhcn@laichau.gov.vn" 
                      class="w-full text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
            <button type="button" @click="isModalOpen = false" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition">Hủy</button>
            <button type="submit" class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl transition shadow-sm">Lưu</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Contact Persons Detail View Modal -->
    <div v-if="isContactModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-2xl w-full p-6 space-y-4 font-sans max-h-[85vh] flex flex-col">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3 shrink-0">
          <div class="flex items-center gap-2 text-slate-800">
            <span class="p-2 bg-blue-50 text-blue-600 rounded-xl font-bold">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/></svg>
            </span>
            <div>
              <h3 class="text-base font-bold text-slate-800">Danh Sách Cán Bộ Đầu Mối</h3>
              <p class="text-xs text-blue-700 font-bold mt-0.5">{{ selectedAgencyForContacts?.name }}</p>
            </div>
          </div>
          <button @click="isContactModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg cursor-pointer">✕</button>
        </div>

        <div class="space-y-3 overflow-y-auto pr-1 flex-1">
          <div 
            v-for="(cp, idx) in (selectedAgencyForContacts?.contactPersons || [])" 
            :key="idx"
            class="bg-slate-50/80 p-3.5 rounded-xl border border-slate-200/80 space-y-2"
          >
            <div class="flex items-center justify-between border-b border-slate-200/60 pb-1.5">
              <div class="flex items-center gap-2">
                <span class="w-5 h-5 rounded-full bg-blue-600 text-white font-bold text-[11px] flex items-center justify-center shrink-0">{{ idx + 1 }}</span>
                <span class="text-xs font-bold text-slate-900">{{ cp.name }}</span>
              </div>
              <span v-if="cp.position" class="px-2 py-0.5 bg-blue-100 text-blue-800 text-[10px] font-bold rounded-full">{{ cp.position }}</span>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs font-semibold text-slate-700">
              <div v-if="cp.department" class="flex items-center gap-1.5">
                <span class="text-slate-400">🏢 Phòng ban:</span>
                <span class="font-bold text-slate-800">{{ cp.department }}</span>
              </div>
              <div v-if="cp.phone" class="flex items-center gap-1.5">
                <span class="text-slate-400">📞 Điện thoại:</span>
                <span class="font-bold text-blue-700">{{ cp.phone }}</span>
              </div>
              <div v-if="cp.email" class="flex items-center gap-1.5 col-span-1 sm:col-span-2">
                <span class="text-slate-400">✉️ Email:</span>
                <span class="font-bold text-slate-800">{{ cp.email }}</span>
              </div>
            </div>
          </div>

          <div v-if="!selectedAgencyForContacts?.contactPersons || selectedAgencyForContacts.contactPersons.length === 0" class="text-center py-8 text-slate-400 font-semibold italic">
            Chưa có thông tin cán bộ đầu mối liên hệ cho cơ quan này.
          </div>
        </div>

        <div class="flex items-center justify-between border-t border-slate-100 pt-3 shrink-0">
          <button @click="openEditModal(selectedAgencyForContacts); isContactModalOpen = false;" class="px-3.5 py-1.5 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-xs rounded-xl transition border border-blue-200 cursor-pointer">
            ✏️ Chỉnh sửa cán bộ đầu mối
          </button>
          <button @click="isContactModalOpen = false" class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl cursor-pointer">
            Đóng
          </button>
        </div>
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

const rawAgenciesList = ref([]);
const allParentOptions = ref([]);
const isLoading = ref(true);

const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

const agencyTypeOptions = ref([
  { value: 1, label: 'Bộ / Ngành' },
  { value: 2, label: 'Tỉnh / Thành phố' },
  { value: 5, label: 'Đặc biệt' },
  { value: 4, label: 'Khác (Danh mục riêng)' }
]);

const parentAgencyOptions = computed(() => {
  // Only "Bộ Khoa học và Công nghệ" can be selected as parent agency
  return allParentOptions.value
    .filter(p => {
      if (p.parentId || p.id === editingId.value || p.code === 'ALL_AGENCIES' || p.code === 'ALL_MINISTRIES' || p.code === 'ALL_PROVINCES' || p.code === 'ALL_PROVINCES_UBND' || p.code === 'ALL_MINISTRIES_DIRECT') return false;
      const lowerName = (p.name || '').toLowerCase();
      const lowerCode = (p.code || '').toLowerCase();
      return lowerName.includes('khoa học') || lowerCode === 'bkhcn';
    })
    .map(p => ({ value: p.id, label: p.name }));
});

function isFixedAgency(agency) {
  if (!agency) return false;
  const name = (agency.name || '').toLowerCase().trim();
  const code = (agency.code || '').toLowerCase().trim();
  if (code === 'all_agencies' || code === 'all_ministries' || code === 'all_provinces' || code === 'all_provinces_ubnd' || code === 'all_ministries_direct') return true;
  if (name.includes('các bộ, ngành') || name.includes('các địa phương') || name.includes('ubnd tỉnh, thành phố')) return true;
  if (!agency.parentId && (code === 'bkhcn' || (name.startsWith('bộ') && (name.includes('khoa học và công nghệ') || name.includes('khoa học & công nghệ'))))) {
    return true;
  }
  return false;
}
const isBKHCN = isFixedAgency;

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

const isContactModalOpen = ref(false);
const selectedAgencyForContacts = ref(null);

function openContactPersonsModal(agency) {
  selectedAgencyForContacts.value = agency;
  isContactModalOpen.value = true;
}

const form = ref({ code: '', name: '', parentId: null, type: 1, contactPersons: [] });

function generateAgencyCode(name) {
  if (!name || typeof name !== 'string') return '';
  const cleanStr = name
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/Đ/g, 'D')
    .replace(/đ/g, 'd');
    
  const words = cleanStr
    .replace(/[^a-zA-Z0-9\s]/g, ' ')
    .trim()
    .split(/\s+/)
    .filter(Boolean);
    
  if (words.length === 0) return '';
  return words.map(w => w[0].toUpperCase()).join('');
}

function onNameInput() {
  if (form.value.name) {
    form.value.code = generateAgencyCode(form.value.name);
  } else {
    form.value.code = '';
  }
}

function formatContactPerson(cp) {
  if (!cp) return '';
  const parts = [cp.name, cp.position, cp.department, cp.phone, cp.email].filter(p => p && String(p).trim().length > 0);
  return parts.join(' - ');
}

function addContactPerson() {
  if (!form.value.contactPersons) form.value.contactPersons = [];
  form.value.contactPersons.push({ name: '', position: '', department: '', phone: '', email: '' });
}

function removeContactPerson(index) {
  if (form.value.contactPersons && index >= 0 && index < form.value.contactPersons.length) {
    form.value.contactPersons.splice(index, 1);
  }
}

let agencyFetchRequestId = 0;
let agencySearchDebounceTimer = null;

function execSearch() {
  if (agencySearchDebounceTimer) clearTimeout(agencySearchDebounceTimer);
  agencyFetchRequestId++;
  searchQuery.value = searchDraft.value;
  pageNumber.value = 1;
  fetchAgencies();
}

watch(searchDraft, () => {
  if (agencySearchDebounceTimer) clearTimeout(agencySearchDebounceTimer);
  agencySearchDebounceTimer = setTimeout(() => {
    execSearch();
  }, 300);
});

function resetSearch() {
  if (agencySearchDebounceTimer) clearTimeout(agencySearchDebounceTimer);
  agencyFetchRequestId++;
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

function closeModal() {
  isModalOpen.value = false;
}

function getTypeLabel(row) {
  const type = (typeof row === 'object' && row !== null && row.parentId) ? 3 : (typeof row === 'object' ? row.type : row);
  if (type === 1 || type === '1' || type === 'Ministry') return 'Bộ / Ngành';
  if (type === 2 || type === '2' || type === 'Province') return 'Tỉnh / TP';
  if (type === 3 || type === '3' || type === 'Internal') return 'Đơn vị trực thuộc';
  if (type === 5 || type === '5' || type === 'Special') return 'Đặc biệt';
  if (type === 4 || type === '4' || type === 'Other') return 'Khác (Danh mục riêng)';
  return 'Bộ / Ngành';
}

function getTypeBadgeClass(row) {
  const type = (typeof row === 'object' && row !== null && row.parentId) ? 3 : (typeof row === 'object' ? row.type : row);
  if (type === 1 || type === '1' || type === 'Ministry') return 'bg-purple-50 text-purple-700 border border-purple-100';
  if (type === 2 || type === '2' || type === 'Province') return 'bg-blue-50 text-blue-700 border border-blue-100';
  if (type === 3 || type === '3' || type === 'Internal') return 'bg-emerald-50 text-emerald-700 border border-emerald-100';
  if (type === 5 || type === '5' || type === 'Special') return 'bg-amber-50 text-amber-800 border border-amber-200 font-bold';
  if (type === 4 || type === '4' || type === 'Other') return 'bg-slate-100 text-slate-700 border border-slate-300 font-bold';
  return 'bg-slate-100 text-slate-700 border border-slate-200';
}

function toggleExpand(agencyId) {
  if (expandedNodes.value.has(agencyId)) {
    expandedNodes.value.delete(agencyId);
  } else {
    expandedNodes.value.add(agencyId);
  }
}
const toggleNode = toggleExpand;

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
  const list = (rawAgenciesList.value || []).filter(a => a.code !== 'ALL_AGENCIES');
  
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

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase();
    const matchingRoots = roots.filter(r => {
      const matchParent = r.name.toLowerCase().includes(q) || (r.code && r.code.toLowerCase().includes(q));
      const matchChild = r.children.some(c => c.name.toLowerCase().includes(q) || (c.code && c.code.toLowerCase().includes(q)));
      return matchParent || matchChild;
    });

    totalCount.value = matchingRoots.length;
    totalPages.value = Math.max(1, Math.ceil(matchingRoots.length / pageSize.value));
    const pagedRoots = matchingRoots.slice((pageNumber.value - 1) * pageSize.value, pageNumber.value * pageSize.value);

    const result = [];
    pagedRoots.forEach(r => {
      const matchParent = r.name.toLowerCase().includes(q) || (r.code && r.code.toLowerCase().includes(q));
      const childrenToShow = matchParent 
        ? r.children 
        : r.children.filter(c => c.name.toLowerCase().includes(q) || (c.code && c.code.toLowerCase().includes(q)));

      const hasChildren = r.children && r.children.length > 0;
      result.push({ ...r, level: 0, hasChildren });
      if (hasChildren && (expandedNodes.value.has(r.id) || matchParent || childrenToShow.length > 0)) {
        childrenToShow.forEach(c => {
          result.push({ ...c, level: 1, hasChildren: false });
        });
      }
    });
    return result;
  }

  totalCount.value = roots.length;
  totalPages.value = Math.max(1, Math.ceil(roots.length / pageSize.value));
  const pagedRoots = roots.slice((pageNumber.value - 1) * pageSize.value, pageNumber.value * pageSize.value);

  const result = [];
  function traverse(nodes, level = 0) {
    nodes.forEach(node => {
      const hasChildren = level === 0 && node.children && node.children.length > 0;
      result.push({
        ...node,
        level,
        hasChildren
      });

      if (hasChildren && level === 0 && expandedNodes.value.has(node.id)) {
        traverse(node.children, level + 1);
      }
    });
  }

  traverse(pagedRoots);
  return result;
});

function openCreateModal(parentAgencyId = null) {
  isEditing.value = false;
  editingId.value = null;
  editingUsedCount.value = 0;
  form.value = { 
    code: '', 
    name: '', 
    parentId: parentAgencyId, 
    type: parentAgencyId ? 3 : 1,
    contactPersons: [] 
  };
  isModalOpen.value = true;
}

function openEditModal(agency) {
  if (isBKHCN(agency)) {
    toast.warning('Bộ Khoa học và Công nghệ là cơ quan hệ thống cố định, không thể chỉnh sửa.');
    return;
  }

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
    type: mappedType,
    contactPersons: Array.isArray(agency.contactPersons) 
      ? JSON.parse(JSON.stringify(agency.contactPersons)) 
      : []
  };
  isModalOpen.value = true;
}

async function fetchAgencies() {
  const currentRequestId = ++agencyFetchRequestId;
  isLoading.value = true;
  try {
    const url = new URL(getApiUrl('/api/agencies'));
    if (searchQuery.value.trim()) {
      url.searchParams.append('search', searchQuery.value.trim());
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      if (currentRequestId !== agencyFetchRequestId) return;
      const allItems = Array.isArray(data) ? data : (data.items || []);
      rawAgenciesList.value = allItems;

      // Expand all root nodes by default
      const defaultExpanded = new Set();
      allItems.forEach(a => {
        if (!a.parentId) defaultExpanded.add(a.id);
      });
      expandedNodes.value = defaultExpanded;
    }
  } catch (e) {
    if (currentRequestId !== agencyFetchRequestId) return;
    console.error('Error fetching agencies:', e);
  } finally {
    if (currentRequestId === agencyFetchRequestId) {
      isLoading.value = false;
    }
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
    const cleanedContacts = (form.value.contactPersons || [])
      .filter(cp => cp.name || cp.position || cp.department || cp.phone || cp.email)
      .map(cp => ({
        name: cp.name || '',
        position: cp.position || '',
        department: cp.department || '',
        phone: cp.phone || '',
        email: cp.email || ''
      }));

    const isSubAgency = !!form.value.parentId;
    const finalType = isSubAgency ? 3 : Number(form.value.type || 1);

    const finalCode = (form.value.code && form.value.code.trim()) 
      ? form.value.code.trim() 
      : generateAgencyCode(form.value.name);

    const payload = {
      code: finalCode,
      name: form.value.name,
      parentId: form.value.parentId || null,
      type: finalType,
      contactPersons: cleanedContacts,
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
      toast.success(isEditing.value ? 'Lưu thành công!' : 'Lưu thành công!');
      isModalOpen.value = false;
      await fetchAgencies();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.error || err.message || 'Lưu thất bại.');
    }
  } catch (e) {
    toast.error('Lưu thất bại.');
  }
}

async function deleteAgency(agency) {
  if (isBKHCN(agency)) {
    toast.warning('Bộ Khoa học và Công nghệ là cơ quan hệ thống cố định, không thể xóa.');
    return;
  }

  if (agency.usedCount > 0) {
    toast.warning('Không thể xóa dữ liệu này.');
    return;
  }

  const confirmed = await confirmModal({
    title: 'Xóa dữ liệu',
    message: 'Chắc chắn xóa dữ liệu này?',
    confirmText: 'Xóa ngay',
    cancelText: 'Hủy',
    type: 'danger'
  });

  if (confirmed) {
    try {
      const res = await fetch(getApiUrl(`/api/agencies/${agency.id}`), { method: 'DELETE' });
      if (res.ok) {
        toast.success('Đã xóa dữ liệu thành công!');
        await fetchAgencies();
      } else {
        toast.error('Không thể xóa dữ liệu này.');
      }
    } catch (e) {
      toast.error('Không thể xóa dữ liệu này.');
    }
  }
}

onMounted(() => {
  fetchAgencies();
  fetchAllParentOptions();
});
</script>
