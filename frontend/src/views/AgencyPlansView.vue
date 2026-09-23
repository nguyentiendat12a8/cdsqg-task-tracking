<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Top Header Bar -->
    <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3 w-full">
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm font-bold text-sm">
            🏢
          </span>
          <div>
            <h2 class="text-sm sm:text-base font-bold text-slate-800">
              Đầu mối liên hệ và KH CĐS
            </h2>
            <p class="text-xs text-slate-500">
              Tổng hợp thông tin cán bộ đầu mối liên hệ và tệp Kế hoạch chuyển đổi số giai đoạn 2026-2030 của các Bộ, Ngành, Địa phương và Đơn vị trực thuộc.
            </p>
          </div>
        </div>

        <div class="flex items-center gap-2 flex-wrap shrink-0">
          <button 
            @click="exportAgencyPlansToExcel"
            class="px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl transition shadow-2xs flex items-center gap-1.5 cursor-pointer"
            title="Xuất Báo cáo Excel danh sách kế hoạch & đầu mối"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
            </svg>
            <span>Xuất Excel Danh Sách</span>
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
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
            <input 
              v-model="searchQuery" 
              @keyup.enter="execSearch"
              placeholder="Tìm theo tên cơ quan, tên cán bộ đầu mối, email, số điện thoại..." 
              class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none h-[34px]"
            />
          </div>

          <!-- OverlayPanel Advanced Filter Popover -->
          <OverlayPanel 
            title="Bộ Lọc Tìm Kiếm Nâng Cao"
            buttonText="Lọc Nâng Cao"
            :activeCount="activeFilterCount"
            widthClass="w-[340px] sm:w-[500px]"
            @apply="applyFilters"
            @reset="resetFilters"
          >
            <div class="space-y-3">
              <div>
                <SearchableSelect 
                  v-model="selectedAgencyId" 
                  :options="agencyOptions" 
                  :isMulti="false" 
                  label="Đơn vị, cơ quan" 
                  placeholder="Tất cả đơn vị, cơ quan"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="contactFilter" 
                  :options="contactFilterOptions" 
                  :isMulti="false" 
                  label="Cung cấp cán bộ đầu mối" 
                  placeholder="Tất cả trạng thái"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="planFileFilter" 
                  :options="planFileFilterOptions" 
                  :isMulti="false" 
                  label="Thông tin Kế hoạch CĐS" 
                  placeholder="Tất cả trạng thái"
                />
              </div>
            </div>
          </OverlayPanel>
        </div>
      </div>

      <!-- Data Table Section -->
      <div class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
        <LoadingSpinner v-if="loading" text="Đang tải danh sách cơ quan & thông tin đầu mối..." />

        <table v-else class="w-full min-w-[1000px] text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-10 shadow-2xs">
            <tr>
              <th class="px-3.5 py-3 border-r border-slate-200 text-center min-w-[60px] bg-slate-100 whitespace-nowrap">STT</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[240px] bg-slate-100">Cơ Quan, Đơn Vị</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[320px] bg-slate-100">Danh Sách Cán Bộ Đầu Mối</th>
              <th class="px-3.5 py-3 border-r border-slate-200 min-w-[280px] bg-slate-100">Tệp Đính Kèm Kế Hoạch CĐS(2026-2030)</th>
              <th class="px-3.5 py-3 text-center w-[90px] min-w-[90px] bg-slate-100 whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-xs">
            <tr v-if="agencies.length === 0">
              <td colspan="5" class="py-12 text-center text-slate-400">
                <div class="flex flex-col items-center gap-2">
                  <svg class="w-10 h-10 text-slate-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                  </svg>
                  <span class="font-bold text-slate-600">Không tìm thấy cơ quan phù hợp</span>
                </div>
              </td>
            </tr>

            <tr 
              v-for="(item, index) in agencies" 
              :key="item.id"
              class="hover:bg-blue-50/40 transition-colors"
            >
              <!-- STT -->
              <td class="px-3.5 py-3 border-r border-slate-200 text-center font-bold text-slate-500 align-top">
                {{ (pageNumber - 1) * pageSize + index + 1 }}
              </td>

              <!-- Agency Name -->
              <td class="px-3.5 py-3 border-r border-slate-200 font-bold text-slate-900 leading-snug align-top">
                {{ item.name }}
              </td>

              <!-- Contact Persons List -->
              <td class="px-3.5 py-3 border-r border-slate-200 align-top">
                <div v-if="item.contactPersons && item.contactPersons.length > 0" class="space-y-2.5 max-h-[140px] overflow-y-auto custom-scrollbar pr-1">
                  <div 
                    v-for="(cp, idx) in item.contactPersons" 
                    :key="idx"
                    class="bg-white p-2.5 rounded-xl border border-slate-200/90 shadow-2xs hover:shadow-xs hover:border-blue-300 transition-all duration-150 space-y-1.5"
                  >
                    <!-- Header: Index + Name + Position + Department -->
                    <div class="flex items-center gap-2">
                      <div class="w-5 h-5 rounded-lg bg-gradient-to-br from-blue-600 to-indigo-700 text-white font-bold text-[10px] flex items-center justify-center shrink-0 shadow-2xs">
                        {{ idx + 1 }}
                      </div>
                      <div class="min-w-0 flex-1 flex items-center gap-1.5 flex-wrap">
                        <span class="font-bold text-slate-900 text-xs tracking-tight">{{ cp.name || 'Chưa rõ họ tên' }}</span>
                        <span v-if="cp.position" class="text-[11px] font-semibold text-slate-500">
                          ({{ cp.position }})
                        </span>
                        <span v-if="cp.department" class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-slate-100 text-slate-700 font-semibold text-[10px] border border-slate-200/60">
                          <svg class="w-3 h-3 text-slate-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                          </svg>
                          <span>{{ cp.department }}</span>
                        </span>
                      </div>
                    </div>

                    <!-- Contact Details: Phone & Email -->
                    <div v-if="cp.phone || cp.email" class="pl-7 flex flex-wrap items-center gap-x-3 gap-y-1 text-[11px] pt-1.5 border-t border-slate-100">
                      <span v-if="cp.phone" class="inline-flex items-center gap-1 text-slate-700 hover:text-emerald-700 transition">
                        <span class="w-4 h-4 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center shrink-0">
                          <svg class="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                          </svg>
                        </span>
                        <a :href="'tel:' + cp.phone" class="font-bold hover:underline font-mono text-[11px]">{{ cp.phone }}</a>
                      </span>
                      <span v-if="cp.email" class="inline-flex items-center gap-1 text-slate-700 hover:text-blue-700 transition">
                        <span class="w-4 h-4 rounded-full bg-blue-50 text-blue-600 flex items-center justify-center shrink-0">
                          <svg class="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                          </svg>
                        </span>
                        <a :href="'mailto:' + cp.email" class="font-semibold hover:underline text-[11px] truncate max-w-[180px]">{{ cp.email }}</a>
                      </span>
                    </div>
                  </div>
                </div>
                <div v-else class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-slate-100 text-slate-400 text-xs italic border border-slate-200/60">
                  <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span>Chưa cập nhật cán bộ đầu mối</span>
                </div>
              </td>

              <!-- Plan Files List -->
              <td class="px-3.5 py-3 border-r border-slate-200 align-top">
                <div v-if="item.planFiles && item.planFiles.length > 0" class="space-y-2 max-h-[140px] overflow-y-auto custom-scrollbar pr-1">
                  <div 
                    v-for="file in item.planFiles" 
                    :key="file.id"
                    class="flex items-center justify-between gap-2 p-2 bg-blue-50/50 rounded-xl border border-blue-100 hover:bg-blue-50 transition"
                  >
                    <div class="flex items-center gap-2 overflow-hidden min-w-0">
                      <div class="w-8 h-8 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center shrink-0">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                        </svg>
                      </div>
                      <div class="min-w-0 overflow-hidden">
                        <a 
                          :href="getFileFullUrl(file.fileUrl)" 
                          target="_blank" 
                          download
                          class="font-bold text-slate-800 hover:text-blue-700 text-xs truncate block"
                          :title="file.fileName"
                        >
                          {{ file.fileName }}
                        </a>
                        <span class="text-[10px] text-slate-400 font-semibold">
                          {{ formatFileSize(file.fileSize) }}
                        </span>
                      </div>
                    </div>

                    <a 
                      :href="getFileFullUrl(file.fileUrl)" 
                      target="_blank" 
                      download
                      class="px-2.5 py-1 bg-white hover:bg-blue-600 hover:text-white text-blue-700 text-[11px] font-bold rounded-lg border border-blue-200 transition shrink-0 flex items-center gap-1 shadow-xs"
                      title="Tải xuống tệp kế hoạch"
                    >
                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                      </svg>
                      <span>Tải về</span>
                    </a>
                  </div>
                </div>
                <div v-else class="text-slate-400 text-xs italic py-1">
                  Chưa tải lên file Kế hoạch 2026-2030
                </div>
              </td>

              <!-- Action Button -->
              <td class="px-3.5 py-3 text-center align-top">
                <button 
                  v-if="canEditAgency(item)"
                  @click="openEditModal(item)"
                  class="w-9 h-9 bg-blue-50 hover:bg-blue-600 text-blue-600 hover:text-white rounded-xl border border-blue-200/80 transition-all duration-150 flex items-center justify-center mx-auto cursor-pointer shadow-2xs hover:shadow-xs group"
                  title="Cập nhật Kế hoạch CĐS & Đầu mối liên hệ"
                >
                  <svg class="w-4 h-4 transition-transform group-hover:scale-110" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <span v-else class="inline-flex w-9 h-9 items-center justify-center rounded-xl bg-slate-100 text-slate-400 mx-auto" title="Bạn không có quyền chỉnh sửa đơn vị này">
                  <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Bar -->
      <div class="flex flex-col md:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold w-full">
        <div class="flex items-center gap-3 whitespace-nowrap flex-wrap justify-center sm:justify-start">
          <span class="whitespace-nowrap">Hiển thị <span class="font-bold text-slate-900">{{ totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0 }} - {{ Math.min(pageNumber * pageSize, totalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ totalCount }}</span> đơn vị</span>
          
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

    <!-- Edit Agency Contacts & Plans Modal -->
    <div 
      v-if="showModal" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
      @click.self="closeModal"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl overflow-hidden flex flex-col max-h-[92vh] my-auto">
        <!-- Modal Header -->
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm text-sm font-bold">
              📝
            </span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">
                Cập nhật Kế hoạch CĐS & Đầu mối liên hệ
              </h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">
                {{ editingAgency?.name }}
              </p>
            </div>
          </div>

          <button 
            @click="closeModal" 
            class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm"
            title="Đóng cửa sổ"
          >
            ✕
          </button>
        </div>

        <!-- Modal Body -->
        <div class="p-6 overflow-y-auto space-y-5 flex-1 custom-scrollbar text-xs">
          
          <!-- SECTION 1: CONTACT PERSONS -->
          <div class="space-y-2.5">
            <div class="flex items-center justify-between border-b border-slate-200 pb-2">
              <h4 class="text-xs uppercase font-bold text-slate-800 tracking-wider flex items-center gap-2">
                <span class="text-blue-600">👥</span>
                <span>1. Danh sách Cán bộ Đầu mối liên hệ</span>
              </h4>

              <button 
                @click="addContactPerson" 
                class="px-3 py-1.5 bg-blue-50 hover:bg-blue-100 text-blue-700 text-xs font-bold rounded-xl transition border border-blue-200 flex items-center gap-1 cursor-pointer shadow-2xs"
              >
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
                </svg>
                <span>Thêm Cán bộ</span>
              </button>
            </div>

            <!-- Scrollable container for Contact Persons list -->
            <div class="max-h-[220px] sm:max-h-[250px] overflow-y-auto custom-scrollbar space-y-3 pr-1">
              <div v-if="contactPersonsForm.length === 0" class="p-4 bg-slate-50 rounded-xl text-center text-xs text-slate-400 italic border border-dashed border-slate-200">
                Chưa có cán bộ đầu mối nào. Nhấn "Thêm Cán bộ" ở trên để bổ sung.
              </div>

              <div 
                v-for="(cp, idx) in contactPersonsForm" 
                :key="idx"
                class="p-3.5 bg-slate-50/70 rounded-xl border border-slate-200 space-y-3 relative group hover:border-slate-300 transition"
              >
                <div class="flex items-center justify-between">
                  <span class="text-xs font-bold text-slate-800 flex items-center gap-1.5">
                    <span class="w-5 h-5 rounded-full bg-blue-600 text-white text-[10px] flex items-center justify-center font-bold">{{ idx + 1 }}</span>
                    <span>Cán bộ đầu mối #{{ idx + 1 }}</span>
                  </span>
                  <button 
                    @click="removeContactPerson(idx)" 
                    class="text-rose-600 hover:text-rose-800 text-xs font-bold flex items-center gap-1 cursor-pointer px-2 py-1 hover:bg-rose-50 rounded-lg transition"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                    <span>Xóa</span>
                  </button>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Họ và tên <span class="text-rose-500">*</span></label>
                    <input 
                      v-model="cp.name" 
                      type="text" 
                      placeholder="Ví dụ: Nguyễn Tiến Đạt"
                      class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Chức vụ</label>
                    <input 
                      v-model="cp.position" 
                      type="text" 
                      placeholder="Ví dụ: Chuyên viên"
                      class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Phòng / Đơn vị trực thuộc</label>
                    <input 
                      v-model="cp.department" 
                      type="text" 
                      placeholder="Ví dụ: Cục CĐSQG"
                      class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Số điện thoại liên hệ</label>
                    <input 
                      v-model="cp.phone" 
                      type="text" 
                      placeholder="Ví dụ: 0379836255"
                      class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div class="sm:col-span-2">
                    <label class="block font-bold text-slate-700 mb-1">Địa chỉ Email</label>
                    <input 
                      v-model="cp.email" 
                      type="email" 
                      placeholder="Ví dụ: dat@gmail.com"
                      class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- SECTION 2: PLAN FILES -->
          <div class="space-y-2.5 pt-1">
            <div class="border-b border-slate-200 pb-2">
              <h4 class="text-xs uppercase font-bold text-slate-800 tracking-wider flex items-center gap-2">
                <span class="text-indigo-600">📄</span>
                <span>2. Tệp đính kèm Kế hoạch giai đoạn 2026-2030</span>
              </h4>
            </div>

            <!-- Existing Plan Files List -->
            <div v-if="planFilesForm.length > 0" class="space-y-2 max-h-[140px] overflow-y-auto custom-scrollbar pr-1">
              <div 
                v-for="file in planFilesForm" 
                :key="file.id"
                class="flex items-center justify-between gap-3 p-2.5 bg-slate-50/70 rounded-xl border border-slate-200"
              >
                <div class="flex items-center gap-2.5 overflow-hidden">
                  <div class="w-8 h-8 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center shrink-0">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <div class="overflow-hidden">
                    <span class="font-bold text-slate-800 text-xs truncate block" :title="file.fileName">
                      {{ file.fileName }}
                    </span>
                    <span class="text-[10px] text-slate-400 font-semibold">
                      {{ formatFileSize(file.fileSize) }}
                    </span>
                  </div>
                </div>

                <div class="flex items-center gap-2 shrink-0">
                  <a 
                    :href="getFileFullUrl(file.fileUrl)" 
                    target="_blank" 
                    download 
                    class="px-2.5 py-1 bg-white hover:bg-slate-100 text-slate-700 text-xs font-bold rounded-lg border border-slate-300 transition shadow-2xs"
                  >
                    Xem / Tải
                  </a>
                  <button 
                    @click="deletePlanFile(file.id)"
                    class="px-2.5 py-1 bg-rose-50 hover:bg-rose-600 hover:text-white text-rose-600 text-xs font-bold rounded-lg border border-rose-200 transition cursor-pointer"
                  >
                    Xóa
                  </button>
                </div>
              </div>
            </div>

            <!-- Upload new plan file box -->
            <div class="border-2 border-dashed border-slate-300 hover:border-blue-500 rounded-xl p-3.5 text-center bg-slate-50/60 hover:bg-blue-50/30 transition cursor-pointer relative">
              <input 
                type="file" 
                multiple
                @change="handleFileUpload" 
                accept=".pdf,.doc,.docx,.xls,.xlsx"
                class="absolute inset-0 w-full h-full opacity-0 cursor-pointer" 
                :disabled="uploadingFile"
              />
              <div class="flex flex-col items-center justify-center gap-1.5">
                <svg v-if="!uploadingFile" class="w-7 h-7 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                <svg v-else class="w-7 h-7 text-blue-600 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>

                <span v-if="!uploadingFile" class="text-xs font-bold text-slate-700">
                  Kéo thả hoặc nhấn vào đây để tải lên tệp Kế hoạch / Phụ lục mới
                </span>
                <span v-else class="text-xs font-bold text-blue-600 animate-pulse">
                  Đang tải lên tệp...
                </span>
                <span class="text-[10px] text-slate-400 font-semibold">
                  Hỗ trợ định dạng: .pdf, .doc, .docx, .xls, .xlsx (Tối đa 10MB)
                </span>
              </div>
            </div>

          </div>

        </div>

        <!-- Modal Footer -->
        <div class="px-6 py-3.5 bg-slate-50 border-t border-slate-200 flex items-center justify-end gap-3 shrink-0">
          <button 
            @click="closeModal" 
            class="px-4 py-2 bg-white hover:bg-slate-100 text-slate-700 text-xs font-bold rounded-xl border border-slate-300 transition shadow-2xs cursor-pointer"
            :disabled="saving"
          >
            Hủy bỏ
          </button>
          
          <button 
            @click="saveAgencyDetails" 
            class="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-xl shadow-md transition flex items-center gap-1.5 cursor-pointer"
            :disabled="saving"
          >
            <svg v-if="saving" class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <span>{{ saving ? 'Đang lưu...' : 'Lưu Thay Đổi' }}</span>
          </button>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import XLSX from 'xlsx-js-style';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';
import { confirmModal } from '../services/confirm';
import OverlayPanel from '../components/OverlayPanel.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';

const agencies = ref([]);
const allAgenciesOptions = ref([]);
const loading = ref(false);
const saving = ref(false);
const uploadingFile = ref(false);

const pageNumber = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const searchQuery = ref('');
const selectedAgencyId = ref('ALL');
const contactFilter = ref('ALL');
const planFileFilter = ref('ALL');

const showModal = ref(false);
const editingAgency = ref(null);
const contactPersonsForm = ref([]);
const planFilesForm = ref([]);

const currentUserAgencyId = computed(() => authState.user.value?.agencyId || null);
const currentUserAgencyCode = computed(() => authState.user.value?.agencyCode || '');

const currentUserAgency = computed(() => {
  if (!currentUserAgencyId.value || !allAgenciesOptions.value.length) return null;
  return allAgenciesOptions.value.find(a => a.id === currentUserAgencyId.value) || null;
});

function isBKHCN(agency) {
  if (!agency) return false;
  const code = (agency.code || '').toLowerCase();
  const name = (agency.name || '').toLowerCase();
  return code === 'bkhcn' || name.includes('khoa học và công nghệ') || name.includes('khoa học & công nghệ') || name.includes('khoa học công nghệ');
}

const isUserLevel1 = computed(() => {
  // If user is assigned to a specific agency, check if that agency is BKHCN
  if (currentUserAgencyId.value) {
    const userAgency = currentUserAgency.value || authState.user.value?.agency;
    if (userAgency && isBKHCN(userAgency)) return true;

    const code = (currentUserAgencyCode.value || '').toLowerCase();
    if (code === 'bkhcn') return true;

    const userAgName = (authState.user.value?.agencyName || authState.user.value?.agency?.name || '').toLowerCase();
    if (userAgName.includes('khoa học và công nghệ') || userAgName.includes('khoa học & công nghệ') || userAgName.includes('khoa học công nghệ')) return true;

    return false;
  }

  // System Admin with no specific agency assigned -> Level 1 System Admin
  if (authState.isAdmin.value) return true;
  const code = (currentUserAgencyCode.value || '').toLowerCase();
  return code === 'bkhcn';
});

function isBKHCNChild(agency) {
  if (!agency) return false;
  if (agency.parentId) {
    const parent = allAgenciesOptions.value.find(a => a.id === agency.parentId);
    if (parent && isBKHCN(parent)) return true;
  }
  if (agency.parentName) {
    const pName = agency.parentName.toLowerCase();
    if (pName.includes('khoa học và công nghệ') || pName.includes('khoa học & công nghệ') || pName.includes('khoa học công nghệ')) return true;
  }
  return false;
}

function isAllowedAgencyForUser(agency) {
  if (isUserLevel1.value) return true;
  if (!agency) return false;

  const userAgId = currentUserAgencyId.value;
  const userAgCode = currentUserAgencyCode.value;
  const userAgency = currentUserAgency.value || authState.user.value?.agency;

  // 1. Own agency or child unit of own agency
  if (userAgId && (agency.id === userAgId || agency.parentId === userAgId)) return true;
  if (userAgency && userAgency.parentId && agency.id === userAgency.parentId) return true;
  if (userAgCode && agency.code && agency.code.toLowerCase() === userAgCode.toLowerCase()) return true;

  // 2. BKHCN agency
  if (isBKHCN(agency)) return true;

  // 3. BKHCN child agency/department
  if (isBKHCNChild(agency)) return true;

  return false;
}

onMounted(() => {
  fetchAllAgenciesOptions();
  fetchAgencies();
});

async function fetchAllAgenciesOptions() {
  try {
    const params = new URLSearchParams();
    params.append('excludeSpecial', 'true');
    if (currentUserAgencyId.value) {
      params.append('userAgencyId', currentUserAgencyId.value);
    }
    if (!isUserLevel1.value) {
      params.append('restrictForUser', 'true');
    }
    const res = await fetch(getApiUrl(`/api/agencies?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      allAgenciesOptions.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (err) {
    console.error('Error loading agency select options:', err);
  }
}

async function fetchAgencies() {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    params.append('pageNumber', pageNumber.value);
    params.append('pageSize', pageSize.value);
    params.append('excludeSpecial', 'true');

    if (currentUserAgencyId.value) {
      params.append('userAgencyId', currentUserAgencyId.value);
    }
    if (!isUserLevel1.value) {
      params.append('restrictForUser', 'true');
    }

    if (searchQuery.value.trim()) {
      params.append('search', searchQuery.value.trim());
    }

    if (selectedAgencyId.value && selectedAgencyId.value !== 'ALL') {
      params.append('agencyId', selectedAgencyId.value);
    }

    if (contactFilter.value && contactFilter.value !== 'ALL') {
      params.append('contactFilter', contactFilter.value);
    }

    if (planFileFilter.value && planFileFilter.value !== 'ALL') {
      params.append('planFileFilter', planFileFilter.value);
    }

    const res = await fetch(getApiUrl(`/api/agencies?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      let rawItems = [];
      let total = 0;
      let totalP = 1;

      if (data && data.items) {
        rawItems = data.items;
        total = data.totalCount || 0;
        totalP = data.totalPages || 1;
      } else if (Array.isArray(data)) {
        rawItems = data;
        total = data.length;
        totalP = 1;
      }

      if (!isUserLevel1.value) {
        agencies.value = rawItems.filter(a => isAllowedAgencyForUser(a));
        totalCount.value = agencies.value.length;
        totalPages.value = 1;
      } else {
        agencies.value = rawItems;
        totalCount.value = total;
        totalPages.value = totalP;
      }
    }
  } catch (err) {
    console.error('Error loading agencies:', err);
  } finally {
    loading.value = false;
  }
}

function execSearch() {
  pageNumber.value = 1;
  fetchAgencies();
}

const agencyOptions = computed(() => {
  const opts = [{ value: 'ALL', label: 'Tất cả đơn vị, cơ quan' }];
  const list = allAgenciesOptions.value.filter(a => 
    a.code !== 'ALL_AGENCIES' && 
    a.id !== 'ALL_AGENCIES' && 
    a.name !== 'Các bộ, ngành, địa phương' &&
    isAllowedAgencyForUser(a)
  );
  list.forEach(a => {
    opts.push({ value: a.id, label: a.name });
  });
  return opts;
});

const contactFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'PROVIDED', label: 'Đã cung cấp cán bộ đầu mối' },
  { value: 'NOT_PROVIDED', label: 'Chưa cung cấp cán bộ đầu mối' }
];

const planFileFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'SENT', label: 'Đã gửi thông tin kế hoạch CĐS' },
  { value: 'NOT_SENT', label: 'Chưa gửi thông tin kế hoạch CĐS' }
];

const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedAgencyId.value && selectedAgencyId.value !== 'ALL') count++;
  if (contactFilter.value && contactFilter.value !== 'ALL') count++;
  if (planFileFilter.value && planFileFilter.value !== 'ALL') count++;
  return count;
});

function applyFilters() {
  pageNumber.value = 1;
  fetchAgencies();
}

function resetFilters() {
  selectedAgencyId.value = 'ALL';
  contactFilter.value = 'ALL';
  planFileFilter.value = 'ALL';
  searchQuery.value = '';
  pageNumber.value = 1;
  fetchAgencies();
}

function changePage(p) {
  if (p < 1 || p > totalPages.value) return;
  pageNumber.value = p;
  fetchAgencies();
}

function getFileFullUrl(path) {
  if (!path) return '#';
  return getApiUrl(path);
}

function formatFileSize(bytes) {
  if (!bytes) return '0 KB';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function canEditAgency(agency) {
  if (isUserLevel1.value) return true;
  if (!currentUserAgencyId.value && !currentUserAgencyCode.value) return false;
  
  const userAgId = currentUserAgencyId.value;
  const userAgCode = currentUserAgencyCode.value;
  
  if (userAgId && (agency.id === userAgId || agency.parentId === userAgId)) return true;
  if (userAgCode && agency.code && userAgCode.toLowerCase() === agency.code.toLowerCase()) return true;

  return false;
}

function openEditModal(agency) {
  editingAgency.value = agency;
  contactPersonsForm.value = JSON.parse(JSON.stringify(agency.contactPersons || []));
  planFilesForm.value = JSON.parse(JSON.stringify(agency.planFiles || []));
  showModal.value = true;
}

function closeModal() {
  showModal.value = false;
  editingAgency.value = null;
  contactPersonsForm.value = [];
  planFilesForm.value = [];
}

function addContactPerson() {
  contactPersonsForm.value.push({
    name: '',
    position: '',
    department: '',
    phone: '',
    email: ''
  });
}

async function removeContactPerson(index) {
  const confirmed = await confirmModal({
    title: 'Xóa Cán bộ đầu mối',
    message: 'Bạn có chắc chắn muốn xóa thông tin cán bộ đầu mối này không?',
    confirmText: 'Xóa cán bộ',
    cancelText: 'Hủy',
    type: 'warning'
  });

  if (confirmed) {
    contactPersonsForm.value.splice(index, 1);
  }
}

async function handleFileUpload(event) {
  const files = Array.from(event.target.files || []);
  if (files.length === 0 || !editingAgency.value) return;

  uploadingFile.value = true;
  let successCount = 0;
  let failCount = 0;

  try {
    for (const file of files) {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('agencyId', editingAgency.value.id);

      const res = await fetch(getApiUrl('/api/agencies/upload-plan-file'), {
        method: 'POST',
        body: formData
      });

      if (res.ok) {
        const data = await res.json();
        planFilesForm.value.push(data.file || data);
        successCount++;
      } else {
        failCount++;
      }
    }

    if (successCount > 0 && failCount === 0) {
      toast.success(successCount === 1 ? 'Tải lên 1 tệp kế hoạch thành công!' : `Đã tải lên ${successCount} tệp kế hoạch thành công!`);
    } else if (successCount > 0 && failCount > 0) {
      toast.warning(`Đã tải lên thành công ${successCount} tệp, ${failCount} tệp thất bại.`);
    } else if (failCount > 0) {
      toast.error('Lỗi khi tải lên các tệp kế hoạch đã chọn.');
    }
  } catch (err) {
    console.error('Error uploading files:', err);
    toast.error('Không thể kết nối đến máy chủ để tải tệp lên.');
  } finally {
    uploadingFile.value = false;
    event.target.value = '';
  }
}

async function deletePlanFile(fileId) {
  const confirmed = await confirmModal({
    title: 'Xóa File Kế hoạch',
    message: 'Bạn có chắc chắn muốn xóa file kế hoạch này không?',
    confirmText: 'Xóa tệp',
    cancelText: 'Hủy',
    type: 'warning'
  });

  if (!confirmed) return;

  try {
    const res = await fetch(getApiUrl(`/api/agencies/plan-files/${fileId}`), {
      method: 'DELETE'
    });

    if (res.ok) {
      planFilesForm.value = planFilesForm.value.filter(f => f.id !== fileId);
      toast.success('Đã xóa tệp Kế hoạch thành công!');
    } else {
      toast.error('Lỗi khi xóa file Kế hoạch');
    }
  } catch (err) {
    console.error('Error deleting plan file:', err);
    toast.error('Không thể kết nối máy chủ để xóa tệp.');
  }
}

async function saveAgencyDetails() {
  if (!editingAgency.value) return;

  saving.value = true;
  try {
    const payload = {
      contactPersons: contactPersonsForm.value.filter(cp => cp.name && cp.name.trim() !== ''),
      planFiles: planFilesForm.value,
      planFileIds: planFilesForm.value.map(f => f.id)
    };

    const res = await fetch(getApiUrl(`/api/agencies/${editingAgency.value.id}/plans-and-contacts?userAgencyId=${currentUserAgencyId.value || ''}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      const updatedAgency = await res.json();
      const idx = agencies.value.findIndex(a => a.id === editingAgency.value.id);
      if (idx !== -1) {
        agencies.value[idx] = updatedAgency;
      }
      toast.success('Lưu thông tin thành công!');
      closeModal();
      fetchAgencies();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.message || err.error || 'Lỗi khi lưu thông tin đơn vị');
    }
  } catch (err) {
    console.error('Error saving agency details:', err);
    toast.error('Không thể kết nối máy chủ để lưu thông tin.');
  } finally {
    saving.value = false;
  }
}

async function exportAgencyPlansToExcel() {
  try {
    toast.info('Đang khởi tạo báo cáo Excel...', { autoClose: 1500 });

    const params = new URLSearchParams();
    params.append('excludeSpecial', 'true');

    if (currentUserAgencyId.value) {
      params.append('userAgencyId', currentUserAgencyId.value);
    }
    if (!isUserLevel1.value) {
      params.append('restrictForUser', 'true');
    }

    if (searchQuery.value && searchQuery.value.trim()) {
      params.append('search', searchQuery.value.trim());
    }

    if (selectedAgencyId.value && selectedAgencyId.value !== 'ALL') {
      params.append('agencyId', selectedAgencyId.value);
    }

    if (contactFilter.value && contactFilter.value !== 'ALL') {
      params.append('contactFilter', contactFilter.value);
    }

    if (planFileFilter.value && planFileFilter.value !== 'ALL') {
      params.append('planFileFilter', planFileFilter.value);
    }

    const res = await fetch(getApiUrl(`/api/agencies?${params.toString()}`));
    if (!res.ok) throw new Error('Không thể lấy dữ liệu danh sách đơn vị từ máy chủ');
    
    const data = await res.json();
    let agencyList = Array.isArray(data) ? data : (data.items || []);
    if (!isUserLevel1.value) {
      agencyList = agencyList.filter(a => isAllowedAgencyForUser(a));
    }

    if (agencyList.length === 0) {
      toast.warning('Không có dữ liệu đơn vị phù hợp để xuất Excel!');
      return;
    }

    const excelRows = agencyList.map((agency, index) => {
      let contactStr = '';
      if (agency.contactPersons && agency.contactPersons.length > 0) {
        contactStr = agency.contactPersons.map((cp, idx) => {
          const parts = [cp.name || 'Chưa rõ họ tên'];
          if (cp.position) parts.push(cp.position);
          if (cp.department) parts.push(cp.department);
          if (cp.phone) parts.push(cp.phone);
          if (cp.email) parts.push(cp.email);
          return `${idx + 1}. ${parts.join(' - ')}`;
        }).join('\n');
      }

      let fileStr = '';
      if (agency.planFiles && agency.planFiles.length > 0) {
        fileStr = agency.planFiles.map(f => f.fileName || f.name || 'Tệp đính kèm').join('\n');
      }

      return {
        'stt': index + 1,
        'Đơn vị': agency.name || '',
        'Thông tin liên hệ': contactStr,
        'Tài liệu KH CĐS': fileStr
      };
    });

    const ws = XLSX.utils.json_to_sheet(excelRows);

    ws['!cols'] = [
      { wch: 8 },  // stt
      { wch: 38 }, // Đơn vị
      { wch: 85 }, // Thông tin liên hệ
      { wch: 35 }  // Tài liệu KH CĐS
    ];

    const range = XLSX.utils.decode_range(ws['!ref']);
    const headerStyle = {
      font: { name: 'Times New Roman', sz: 11, bold: true, color: { rgb: '000000' } },
      fill: { fgColor: { rgb: 'F2F2F2' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: {
        top: { style: 'thin', color: { rgb: '000000' } },
        bottom: { style: 'thin', color: { rgb: '000000' } },
        left: { style: 'thin', color: { rgb: '000000' } },
        right: { style: 'thin', color: { rgb: '000000' } }
      }
    };

    const bodyStyleLeft = {
      font: { name: 'Times New Roman', sz: 11, color: { rgb: '000000' } },
      alignment: { horizontal: 'left', vertical: 'top', wrapText: true },
      border: {
        top: { style: 'thin', color: { rgb: '000000' } },
        bottom: { style: 'thin', color: { rgb: '000000' } },
        left: { style: 'thin', color: { rgb: '000000' } },
        right: { style: 'thin', color: { rgb: '000000' } }
      }
    };

    const bodyStyleCenter = {
      font: { name: 'Times New Roman', sz: 11, color: { rgb: '000000' } },
      alignment: { horizontal: 'center', vertical: 'top', wrapText: true },
      border: {
        top: { style: 'thin', color: { rgb: '000000' } },
        bottom: { style: 'thin', color: { rgb: '000000' } },
        left: { style: 'thin', color: { rgb: '000000' } },
        right: { style: 'thin', color: { rgb: '000000' } }
      }
    };

    for (let r = range.s.r; r <= range.e.r; r++) {
      for (let c = range.s.c; c <= range.e.c; c++) {
        const cellRef = XLSX.utils.encode_cell({ r, c });
        if (!ws[cellRef]) continue;
        if (r === 0) {
          ws[cellRef].s = headerStyle;
        } else {
          ws[cellRef].s = c === 0 ? bodyStyleCenter : bodyStyleLeft;
        }
      }
    }

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'KeHoach_DauMoi');

    const dateStr = new Date().toISOString().slice(0, 10);
    XLSX.writeFile(wb, `Danh_Sach_Ke_Hoach_Dau_Moi_${dateStr}.xlsx`);

    toast.success(`Đã xuất Excel thành công ${agencyList.length} đơn vị!`);
  } catch (err) {
    console.error('Export excel error:', err);
    toast.error('Lỗi khi xuất file Excel: ' + (err.message || err));
  }
}
</script>
