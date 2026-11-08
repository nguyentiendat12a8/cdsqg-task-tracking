<template>
  <div class="w-full min-w-0 font-sans rounded-2xl border border-slate-200/80 bg-white shadow-sm overflow-hidden">
    
    <!-- Top Header Bar -->
    <div class="ui-screen-heading bg-[#f1f5fc] p-4 sm:p-5 rounded-t-2xl border-b border-slate-200/80 space-y-3.5 w-full">
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div class="flex items-center gap-3 min-w-0">
          <div class="ui-screen-icon w-10 h-10 rounded-2xl flex items-center justify-center shrink-0" aria-hidden="true"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M3 21h18M5 21V5l7-3 7 3v16M9 21v-5h6v5M8 7h1M15 7h1M8 11h1M15 11h1" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg></div>
          <div>
            <h2 class="ui-page-title tracking-tight">
              Đầu mối liên hệ và KH CĐS
            </h2>
            <p class="text-xs text-slate-500">
              Tổng hợp thông tin cán bộ đầu mối liên hệ, Kế hoạch chuyển đổi số (5 năm & hàng năm) và Khung kiến trúc số của các Bộ, Ngành, Địa phương.
            </p>
          </div>
        </div>

        <div class="flex items-center gap-2 flex-wrap shrink-0">
          <ActionButton kind="export" 
            @click="exportAgencyPlansToExcel"
            title="Xuất Báo cáo Excel danh sách kế hoạch & đầu mối"
          ><span>Xuất Excel danh sách</span></ActionButton>
        </div>
      </div>
    </div>

    <!-- Main Card Container: Filter + Table + Pagination -->
    <div class="p-3.5 sm:p-4 w-full">
      <div class="w-full border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col">
      
      <!-- Filter & Search Bar -->
      <div class="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3 p-3 bg-slate-50/60 w-full">
        <div class="flex flex-wrap sm:flex-nowrap items-center gap-2 flex-1 max-w-xl min-w-0">
          <!-- Search Input -->
          <div class="relative flex-1 min-w-0 basis-full sm:basis-0">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
            <input 
              v-model="searchQuery" 
              @keyup.enter="execSearch"
              placeholder="Tìm theo tên cơ quan, tên cán bộ đầu mối, email, số điện thoại..." 
              class="ui-single-line w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none "
            />
          </div>

          <!-- OverlayPanel Advanced Filter Popover -->
          <OverlayPanel 
            title="Bộ lọc tìm kiếm nâng cao"
            buttonText="Lọc nâng cao"
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
                  v-model="fiveYearPlanFilter" 
                  :options="fiveYearPlanFilterOptions" 
                  :isMulti="false" 
                  label="Ban hành Kế hoạch CĐS 5 năm" 
                  placeholder="Tất cả trạng thái"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="annualPlanFilter" 
                  :options="annualPlanFilterOptions" 
                  :isMulti="false" 
                  label="Ban hành Kế hoạch CĐS hàng năm" 
                  placeholder="Tất cả trạng thái"
                />
              </div>

              <div>
                <SearchableSelect 
                  v-model="architectureFilter" 
                  :options="architectureFilterOptions" 
                  :isMulti="false" 
                  label="Ban hành Khung kiến trúc số" 
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

        <table v-else class="w-full min-w-[1050px] text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100 text-xs text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-10 shadow-2xs">
            <tr>
              <th class="px-3 py-2.5 border-r border-slate-200 text-center w-12 min-w-[48px] bg-slate-100 whitespace-nowrap">STT</th>
              <th class="px-3.5 py-2.5 border-r border-slate-200 min-w-[220px] bg-slate-100">Cơ quan, đơn vị</th>
              <th class="px-3 py-2.5 border-r border-slate-200 text-center min-w-[140px] bg-slate-100 whitespace-nowrap">Có đầu mối liên hệ</th>
              <th class="px-3 py-2.5 border-r border-slate-200 text-center min-w-[150px] bg-slate-100 whitespace-nowrap">Ban hành KH 5 năm</th>
              <th class="px-3 py-2.5 border-r border-slate-200 text-center min-w-[160px] bg-slate-100 whitespace-nowrap">Ban hành KH hàng năm</th>
              <th class="px-3 py-2.5 border-r border-slate-200 text-center min-w-[160px] bg-slate-100 whitespace-nowrap">Khung kiến trúc số</th>
              <th class="px-3 py-2.5 text-center w-16 min-w-[64px] bg-slate-100 whitespace-nowrap">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-xs">
            <tr v-if="agencies.length === 0">
              <td colspan="7" class="py-12 text-center text-slate-400">
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
              <td class="px-3 py-3 border-r border-slate-200 text-center font-bold text-slate-500 align-middle">
                {{ (pageNumber - 1) * pageSize + index + 1 }}
              </td>

              <!-- Agency Name -->
              <td class="px-3.5 py-3 border-r border-slate-200 font-bold text-slate-900 leading-snug align-middle">
                <div class="flex items-center gap-2">
                  <span>{{ item.name }}</span>
                  <span v-if="item.isFixedProvince" class="inline-flex items-center px-1.5 py-0.2 rounded text-xs font-semibold bg-slate-100 text-slate-600 border border-slate-200">34 tỉnh/thành</span>
                </div>
              </td>

              <!-- 1. Có đầu mối liên hệ -->
              <td class="px-3 py-3 border-r border-slate-200 text-center align-middle">
                <div v-if="hasContacts(item)" class="flex flex-col items-center gap-1.5">
                  <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                    <svg class="w-3 h-3 text-emerald-600" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/></svg>
                    Có ({{ item.contactPersons.length }})
                  </span>
                  <button 
                    @click="openViewContacts(item)"
                    class="text-xs text-blue-600 hover:text-blue-800 font-bold inline-flex items-center gap-1 hover:underline cursor-pointer"
                    title="Xem chi tiết danh sách cán bộ đầu mối"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                    <span>Xem chi tiết</span>
                  </button>
                </div>
                <div v-else>
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-100 text-slate-400 border border-slate-200">
                    Chưa có
                  </span>
                </div>
              </td>

              <!-- 2. Ban hành KH 5 năm -->
              <td class="px-3 py-3 border-r border-slate-200 text-center align-middle">
                <div v-if="hasFiveYearPlan(item)" class="flex flex-col items-center gap-1.5">
                  <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-blue-50 text-blue-700 border border-blue-200">
                    <svg class="w-3 h-3 text-blue-600" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/></svg>
                    Đã ban hành
                  </span>
                  <button 
                    @click="openViewFiveYearPlan(item)"
                    class="text-xs text-blue-600 hover:text-blue-800 font-bold inline-flex items-center gap-1 hover:underline cursor-pointer"
                    title="Xem chi tiết Kế hoạch 5 năm & Tệp đính kèm"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                    <span>Chi tiết & Tệp</span>
                  </button>
                </div>
                <div v-else>
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-100 text-slate-400 border border-slate-200">
                    Chưa có
                  </span>
                </div>
              </td>

              <!-- 3. Ban hành KH hàng năm -->
              <td class="px-3 py-3 border-r border-slate-200 text-center align-middle">
                <div v-if="hasAnnualPlan(item)" class="flex flex-col items-center gap-1.5">
                  <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-indigo-50 text-indigo-700 border border-indigo-200">
                    <svg class="w-3 h-3 text-indigo-600" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/></svg>
                    Có ({{ annualPlansIssuedCount(item) }} năm)
                  </span>
                  <button 
                    @click="openViewAnnualPlans(item)"
                    class="text-xs text-indigo-600 hover:text-indigo-800 font-bold inline-flex items-center gap-1 hover:underline cursor-pointer"
                    title="Xem bảng theo dõi Kế hoạch từng năm & Tệp đính kèm"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                    <span>Chi tiết & Tệp</span>
                  </button>
                </div>
                <div v-else>
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-100 text-slate-400 border border-slate-200">
                    Chưa có
                  </span>
                </div>
              </td>

              <!-- 4. Ban hành khung kiến trúc số -->
              <td class="px-3 py-3 border-r border-slate-200 text-center align-middle">
                <div v-if="hasDigitalArchitecture(item)" class="flex flex-col items-center gap-1.5">
                  <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-amber-50 text-amber-800 border border-amber-200">
                    <svg class="w-3 h-3 text-amber-600" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/></svg>
                    Đã ban hành
                  </span>
                  <button 
                    @click="openViewArchitecture(item)"
                    class="text-xs text-amber-700 hover:text-amber-900 font-bold inline-flex items-center gap-1 hover:underline cursor-pointer"
                    title="Xem chi tiết Khung kiến trúc số & Tệp đính kèm"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                    <span>Chi tiết & Tệp</span>
                  </button>
                </div>
                <div v-else>
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-100 text-slate-400 border border-slate-200">
                    Chưa có
                  </span>
                </div>
              </td>

              <!-- Action Button -->
              <td class="px-3 py-3 text-center align-middle">
                <button 
                  v-if="canEditAgency(item)"
                  @click="openEditModal(item)"
                  class="ui-action ui-action-primary w-8 h-8 bg-blue-50 hover:bg-blue-600 text-blue-600 hover:text-white rounded-xl border border-blue-200/80 transition-all duration-150 flex items-center justify-center mx-auto cursor-pointer shadow-2xs hover:shadow-xs group"
                  title="Cập nhật Kế hoạch CĐS & Đầu mối liên hệ"
                  aria-label="Cập nhật Kế hoạch CĐS & Đầu mối liên hệ">
                  <svg class="w-4 h-4 transition-transform group-hover:scale-110" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <span v-else class="inline-flex w-8 h-8 items-center justify-center rounded-xl bg-slate-100 text-slate-400 mx-auto" title="Bạn không có quyền chỉnh sửa đơn vị này">
                  <svg class="w-3.5 h-3.5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <DocumentPagination v-model:current-page="pageNumber" v-model:page-size="pageSize" :total-count="totalCount" :total-pages="Math.max(1,totalPages)" :page-size-options="[10,25,50,100].map(value=>({value,label:String(value)}))" :change-page="changePage" :disabled="loading" item-label="đơn vị" @update:page-size="execSearch" />
      </div>
    </div>

    <!-- ==================================================================== -->
    <!-- DETAIL POPUP 1: XEM CHI TIẾT CÁN BỘ ĐẦU MỐI                          -->
    <!-- ==================================================================== -->
    <div 
      v-if="viewingContactsAgency" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
      @click.self="viewingContactsAgency = null"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-2xl overflow-hidden flex flex-col max-h-[85vh] my-auto">
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-emerald-600 text-white rounded-xl shadow-sm text-sm font-bold">👥</span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">Danh sách cán bộ đầu mối liên hệ</h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">{{ viewingContactsAgency.name }}</p>
            </div>
          </div>
          <button @click="viewingContactsAgency = null" class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm">✕</button>
        </div>

        <div class="p-6 overflow-y-auto space-y-3 flex-1 custom-scrollbar text-xs">
          <div v-if="!viewingContactsAgency.contactPersons || viewingContactsAgency.contactPersons.length === 0" class="p-6 text-center text-slate-400 italic">
            Chưa có thông tin cán bộ đầu mối.
          </div>
          <div 
            v-for="(cp, idx) in viewingContactsAgency.contactPersons" 
            :key="idx"
            class="bg-white p-3.5 rounded-xl border border-slate-200 shadow-2xs hover:border-emerald-300 transition space-y-2"
          >
            <div class="flex items-center gap-2.5">
              <div class="w-6 h-6 rounded-lg bg-emerald-600 text-white font-bold text-xs flex items-center justify-center shrink-0">
                {{ idx + 1 }}
              </div>
              <div class="min-w-0 flex-1 flex items-center gap-2 flex-wrap">
                <span class="font-bold text-slate-900 text-sm">{{ cp.name || 'Chưa rõ họ tên' }}</span>
                <span v-if="cp.position" class="text-xs font-semibold text-slate-600">({{ cp.position }})</span>
                <span v-if="cp.department" class="px-2 py-0.5 rounded-md bg-slate-100 text-slate-700 font-medium text-xs border border-slate-200">
                  {{ cp.department }}
                </span>
              </div>
            </div>

            <div v-if="cp.phone || cp.email" class="pl-8.5 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs pt-2 border-t border-slate-100">
              <span v-if="cp.phone" class="inline-flex items-center gap-1.5 text-slate-700">
                <span class="w-4 h-4 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center shrink-0">📞</span>
                <a :href="'tel:' + cp.phone" class="font-bold hover:underline font-mono text-emerald-700">{{ cp.phone }}</a>
              </span>
              <span v-if="cp.email" class="inline-flex items-center gap-1.5 text-slate-700">
                <span class="w-4 h-4 rounded-full bg-blue-50 text-blue-600 flex items-center justify-center shrink-0">✉️</span>
                <a :href="'mailto:' + cp.email" class="font-medium hover:underline text-blue-700">{{ cp.email }}</a>
              </span>
            </div>
          </div>
        </div>

        <div class="px-6 py-3 bg-slate-50 border-t border-slate-200 flex items-center justify-end">
          <button @click="viewingContactsAgency = null" class="px-4 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-800 text-xs font-bold rounded-xl transition cursor-pointer">
            Đóng
          </button>
        </div>
      </div>
    </div>

    <!-- ==================================================================== -->
    <!-- DETAIL POPUP 2: XEM CHI TIẾT KẾ HOẠCH CĐS 5 NĂM (2026-2030)         -->
    <!-- ==================================================================== -->
    <div 
      v-if="viewingFiveYearPlanAgency" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
      @click.self="viewingFiveYearPlanAgency = null"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-2xl overflow-hidden flex flex-col max-h-[85vh] my-auto">
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm text-sm font-bold">📑</span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">Kế hoạch CĐS giai đoạn 2026-2030</h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">{{ viewingFiveYearPlanAgency.name }}</p>
            </div>
          </div>
          <button @click="viewingFiveYearPlanAgency = null" class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm">✕</button>
        </div>

        <div class="p-6 overflow-y-auto space-y-4 flex-1 custom-scrollbar text-xs">
          <!-- Metadata block -->
          <div class="bg-blue-50/60 p-4 rounded-xl border border-blue-100 space-y-2">
            <div class="flex items-center justify-between flex-wrap gap-2">
              <span class="font-bold text-slate-700">Trạng thái ban hành:</span>
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-blue-100 text-blue-800">
                ✓ Đã ban hành
              </span>
            </div>
            <div v-if="viewingFiveYearPlanAgency.fiveYearPlan?.documentNumber" class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-blue-100">
              <span class="font-semibold text-slate-600">Số quyết định / văn bản:</span>
              <span class="font-bold text-slate-900">{{ viewingFiveYearPlanAgency.fiveYearPlan.documentNumber }}</span>
            </div>
            <div v-if="viewingFiveYearPlanAgency.fiveYearPlan?.issueDate" class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-blue-100">
              <span class="font-semibold text-slate-600">Ngày ban hành:</span>
              <span class="font-medium text-slate-900">{{ formatDate(viewingFiveYearPlanAgency.fiveYearPlan.issueDate) }}</span>
            </div>
            <div v-if="viewingFiveYearPlanAgency.fiveYearPlan?.notes" class="pt-1 border-t border-blue-100">
              <span class="font-semibold text-slate-600">Ghi chú:</span>
              <p class="text-slate-800 mt-0.5">{{ viewingFiveYearPlanAgency.fiveYearPlan.notes }}</p>
            </div>
          </div>

          <!-- Files list -->
          <div class="space-y-2">
            <h4 class="font-bold text-slate-800 flex items-center gap-1.5">
              <span>📎</span>
              <span>Tệp đính kèm văn bản kế hoạch</span>
            </h4>
            
            <div v-if="getFiveYearPlanFiles(viewingFiveYearPlanAgency).length === 0" class="p-4 bg-slate-50 rounded-xl text-center text-slate-400 italic border border-slate-200">
              Chưa có tệp đính kèm nào được tải lên.
            </div>

            <div 
              v-for="file in getFiveYearPlanFiles(viewingFiveYearPlanAgency)" 
              :key="file.id"
              class="flex items-center justify-between gap-3 p-3 bg-slate-50 rounded-xl border border-slate-200 hover:border-blue-300 transition"
            >
              <div class="flex items-center gap-2.5 overflow-hidden">
                <div class="w-8 h-8 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center shrink-0">
                  📄
                </div>
                <div class="overflow-hidden">
                  <span class="font-bold text-slate-800 text-xs truncate block" :title="file.fileName">{{ file.fileName }}</span>
                  <span class="text-xs text-slate-400 font-semibold">{{ formatFileSize(file.fileSize) }}</span>
                </div>
              </div>
              <a 
                :href="getFileFullUrl(file.fileUrl)" 
                target="_blank" 
                download
                class="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-lg transition shrink-0 flex items-center gap-1 shadow-xs"
              >
                <span>Tải về</span>
              </a>
            </div>
          </div>
        </div>

        <div class="px-6 py-3 bg-slate-50 border-t border-slate-200 flex items-center justify-end">
          <button @click="viewingFiveYearPlanAgency = null" class="px-4 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-800 text-xs font-bold rounded-xl transition cursor-pointer">
            Đóng
          </button>
        </div>
      </div>
    </div>

    <!-- ==================================================================== -->
    <!-- DETAIL POPUP 3: XEM BẢNG KẾ HOẠCH CĐS HÀNG NĂM                       -->
    <!-- ==================================================================== -->
    <div 
      v-if="viewingAnnualPlansAgency" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
      @click.self="viewingAnnualPlansAgency = null"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl overflow-hidden flex flex-col max-h-[85vh] my-auto">
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-indigo-600 text-white rounded-xl shadow-sm text-sm font-bold">📅</span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">Kế hoạch chuyển đổi số hàng năm</h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">{{ viewingAnnualPlansAgency.name }}</p>
            </div>
          </div>
          <button @click="viewingAnnualPlansAgency = null" class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm">✕</button>
        </div>

        <div class="p-6 overflow-y-auto space-y-4 flex-1 custom-scrollbar text-xs">
          <p class="text-slate-600">Bảng tổng hợp tình hình ban hành kế hoạch chuyển đổi số theo từng năm của đơn vị:</p>

          <div class="border border-slate-200 rounded-xl overflow-hidden">
            <table class="w-full text-left text-xs border-collapse">
              <thead class="bg-slate-100 text-slate-700 font-semibold border-b border-slate-200">
                <tr>
                  <th class="p-2.5 text-center w-16">Năm</th>
                  <th class="p-2.5 text-center w-28">Trạng thái</th>
                  <th class="p-2.5 min-w-[160px]">Số QĐ / Văn bản ban hành</th>
                  <th class="p-2.5 w-28">Ngày ban hành</th>
                  <th class="p-2.5 min-w-[140px]">Tệp đính kèm</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr 
                  v-for="plan in getDisplayAnnualPlans(viewingAnnualPlansAgency)" 
                  :key="plan.year"
                  :class="plan.isIssued ? 'bg-indigo-50/20' : 'bg-white'"
                >
                  <td class="p-2.5 font-bold text-slate-800 text-center">Năm {{ plan.year }}</td>
                  <td class="p-2.5 text-center">
                    <span 
                      v-if="plan.isIssued || (plan.files && plan.files.length > 0)" 
                      class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-indigo-100 text-indigo-800 border border-indigo-200"
                    >
                      ✓ Đã ban hành
                    </span>
                    <span v-else class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-slate-100 text-slate-400">
                      Chưa ban hành
                    </span>
                  </td>
                  <td class="p-2.5 font-medium text-slate-900">
                    {{ plan.documentNumber || (plan.isIssued ? '—' : 'Chưa có') }}
                  </td>
                  <td class="p-2.5 text-slate-600">
                    {{ plan.issueDate ? formatDate(plan.issueDate) : '—' }}
                  </td>
                  <td class="p-2.5">
                    <div v-if="plan.files && plan.files.length > 0" class="space-y-1">
                      <div v-for="f in plan.files" :key="f.id" class="flex items-center gap-1.5">
                        <a 
                          :href="getFileFullUrl(f.fileUrl)" 
                          target="_blank" 
                          download
                          class="font-bold text-indigo-700 hover:underline truncate max-w-[150px] block"
                          :title="f.fileName"
                        >
                          📎 {{ f.fileName }}
                        </a>
                      </div>
                    </div>
                    <span v-else class="text-slate-400 italic">Không có tệp</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="px-6 py-3 bg-slate-50 border-t border-slate-200 flex items-center justify-end">
          <button @click="viewingAnnualPlansAgency = null" class="px-4 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-800 text-xs font-bold rounded-xl transition cursor-pointer">
            Đóng
          </button>
        </div>
      </div>
    </div>

    <!-- ==================================================================== -->
    <!-- DETAIL POPUP 4: XEM CHI TIẾT KHUNG KIẾN TRÚC SỐ                      -->
    <!-- ==================================================================== -->
    <div 
      v-if="viewingArchitectureAgency" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
      @click.self="viewingArchitectureAgency = null"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-2xl overflow-hidden flex flex-col max-h-[85vh] my-auto">
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-amber-600 text-white rounded-xl shadow-sm text-sm font-bold">🏛️</span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">Khung kiến trúc số</h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">{{ viewingArchitectureAgency.name }}</p>
            </div>
          </div>
          <button @click="viewingArchitectureAgency = null" class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm">✕</button>
        </div>

        <div class="p-6 overflow-y-auto space-y-4 flex-1 custom-scrollbar text-xs">
          <!-- Metadata block -->
          <div class="bg-amber-50/60 p-4 rounded-xl border border-amber-200 space-y-2">
            <div class="flex items-center justify-between flex-wrap gap-2">
              <span class="font-bold text-slate-700">Trạng thái:</span>
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-amber-100 text-amber-900 border border-amber-300">
                ✓ Đã ban hành Khung kiến trúc số
              </span>
            </div>
            <div v-if="viewingArchitectureAgency.digitalArchitecture?.version" class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-amber-200/60">
              <span class="font-semibold text-slate-600">Phiên bản kiến trúc:</span>
              <span class="font-bold text-amber-900">{{ viewingArchitectureAgency.digitalArchitecture.version }}</span>
            </div>
            <div v-if="viewingArchitectureAgency.digitalArchitecture?.documentNumber" class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-amber-200/60">
              <span class="font-semibold text-slate-600">Số quyết định / văn bản ban hành:</span>
              <span class="font-bold text-slate-900">{{ viewingArchitectureAgency.digitalArchitecture.documentNumber }}</span>
            </div>
            <div v-if="viewingArchitectureAgency.digitalArchitecture?.issueDate" class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-amber-200/60">
              <span class="font-semibold text-slate-600">Ngày ban hành:</span>
              <span class="font-medium text-slate-900">{{ formatDate(viewingArchitectureAgency.digitalArchitecture.issueDate) }}</span>
            </div>
            <div v-if="viewingArchitectureAgency.digitalArchitecture?.notes" class="pt-1 border-t border-amber-200/60">
              <span class="font-semibold text-slate-600">Ghi chú:</span>
              <p class="text-slate-800 mt-0.5">{{ viewingArchitectureAgency.digitalArchitecture.notes }}</p>
            </div>
          </div>

          <!-- Files list -->
          <div class="space-y-2">
            <h4 class="font-bold text-slate-800 flex items-center gap-1.5">
              <span>📎</span>
              <span>Tệp đính kèm khung kiến trúc</span>
            </h4>
            
            <div v-if="!viewingArchitectureAgency.digitalArchitecture?.files || viewingArchitectureAgency.digitalArchitecture.files.length === 0" class="p-4 bg-slate-50 rounded-xl text-center text-slate-400 italic border border-slate-200">
              Chưa có tệp đính kèm nào được tải lên.
            </div>

            <div 
              v-for="file in (viewingArchitectureAgency.digitalArchitecture?.files || [])" 
              :key="file.id"
              class="flex items-center justify-between gap-3 p-3 bg-slate-50 rounded-xl border border-slate-200 hover:border-amber-300 transition"
            >
              <div class="flex items-center gap-2.5 overflow-hidden">
                <div class="w-8 h-8 rounded-lg bg-amber-100 text-amber-800 flex items-center justify-center shrink-0">
                  🏛️
                </div>
                <div class="overflow-hidden">
                  <span class="font-bold text-slate-800 text-xs truncate block" :title="file.fileName">{{ file.fileName }}</span>
                  <span class="text-xs text-slate-400 font-semibold">{{ formatFileSize(file.fileSize) }}</span>
                </div>
              </div>
              <a 
                :href="getFileFullUrl(file.fileUrl)" 
                target="_blank" 
                download
                class="px-3 py-1 bg-amber-600 hover:bg-amber-700 text-white text-xs font-bold rounded-lg transition shrink-0 flex items-center gap-1 shadow-xs"
              >
                <span>Tải về</span>
              </a>
            </div>
          </div>
        </div>

        <div class="px-6 py-3 bg-slate-50 border-t border-slate-200 flex items-center justify-end">
          <button @click="viewingArchitectureAgency = null" class="px-4 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-800 text-xs font-bold rounded-xl transition cursor-pointer">
            Đóng
          </button>
        </div>
      </div>
    </div>

    <!-- ==================================================================== -->
    <!-- EDIT MODAL: CẬP NHẬT ĐẦU MỐI, KH 5 NĂM, KH HÀNG NĂM & KIẾN TRÚC      -->
    <!-- ==================================================================== -->
    <div 
      v-if="showModal" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
      @click.self="closeModal"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-4xl overflow-hidden flex flex-col max-h-[92vh] my-auto">
        <!-- Modal Header -->
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm text-sm font-bold">
              📝
            </span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">
                Cập nhật thông tin Kế hoạch CĐS & Đầu mối liên hệ
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

        <!-- Navigation Tabs -->
        <div class="flex items-center border-b border-slate-200 px-6 bg-slate-50/70 gap-2 shrink-0 overflow-x-auto custom-scrollbar">
          <button 
            @click="activeEditTab = 'contacts'"
            :class="activeEditTab === 'contacts' ? 'border-b-2 border-blue-600 text-blue-700 font-bold bg-white' : 'text-slate-600 hover:text-slate-900 font-medium'"
            class="px-4 py-2.5 text-xs transition cursor-pointer whitespace-nowrap flex items-center gap-1.5"
          >
            <span>👥</span>
            <span>1. Cán bộ đầu mối</span>
            <span v-if="contactPersonsForm.length" class="ml-1 px-1.5 py-0.2 rounded-full text-xs bg-blue-100 text-blue-700">{{ contactPersonsForm.length }}</span>
          </button>

          <button 
            @click="activeEditTab = 'fiveYear'"
            :class="activeEditTab === 'fiveYear' ? 'border-b-2 border-blue-600 text-blue-700 font-bold bg-white' : 'text-slate-600 hover:text-slate-900 font-medium'"
            class="px-4 py-2.5 text-xs transition cursor-pointer whitespace-nowrap flex items-center gap-1.5"
          >
            <span>📑</span>
            <span>2. KH CĐS 5 năm (2026-2030)</span>
            <span v-if="fiveYearPlanForm.isIssued" class="w-2 h-2 rounded-full bg-emerald-500"></span>
          </button>

          <button 
            @click="activeEditTab = 'annual'"
            :class="activeEditTab === 'annual' ? 'border-b-2 border-blue-600 text-blue-700 font-bold bg-white' : 'text-slate-600 hover:text-slate-900 font-medium'"
            class="px-4 py-2.5 text-xs transition cursor-pointer whitespace-nowrap flex items-center gap-1.5"
          >
            <span>📅</span>
            <span>3. KH CĐS hàng năm</span>
            <span v-if="annualPlansForm.some(p => p.isIssued)" class="w-2 h-2 rounded-full bg-indigo-500"></span>
          </button>

          <button 
            @click="activeEditTab = 'architecture'"
            :class="activeEditTab === 'architecture' ? 'border-b-2 border-blue-600 text-blue-700 font-bold bg-white' : 'text-slate-600 hover:text-slate-900 font-medium'"
            class="px-4 py-2.5 text-xs transition cursor-pointer whitespace-nowrap flex items-center gap-1.5"
          >
            <span>🏛️</span>
            <span>4. Khung kiến trúc số</span>
            <span v-if="digitalArchitectureForm.isIssued" class="w-2 h-2 rounded-full bg-amber-500"></span>
          </button>
        </div>

        <!-- Modal Body (Active Tab Content) -->
        <div class="p-6 overflow-y-auto space-y-5 flex-1 custom-scrollbar text-xs">
          
          <!-- TAB 1: CÁN BỘ ĐẦU MỐI -->
          <div v-show="activeEditTab === 'contacts'" class="space-y-3">
            <div class="flex items-center justify-between border-b border-slate-200 pb-2">
              <div>
                <h4 class="text-xs font-bold text-slate-800">Danh sách cán bộ đầu mối chuyển đổi số</h4>
                <p class="text-xs text-slate-500">Khai báo thông tin các cán bộ chịu trách nhiệm đầu mối kết nối, tổng hợp báo cáo CĐS.</p>
              </div>

              <button 
                @click="addContactPerson" 
                class="ui-single-line px-3 py-1.5 bg-blue-50 hover:bg-blue-100 text-blue-700 text-xs font-bold rounded-xl transition border border-blue-200 flex items-center gap-1 cursor-pointer shadow-2xs"
              >
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
                <span>Thêm cán bộ</span>
              </button>
            </div>

            <div class="max-h-[360px] overflow-y-auto custom-scrollbar space-y-3 pr-1">
              <div v-if="contactPersonsForm.length === 0" class="p-6 bg-slate-50 rounded-xl text-center text-xs text-slate-400 italic border border-dashed border-slate-200">
                Chưa có cán bộ đầu mối nào. Nhấn "Thêm cán bộ" ở trên để bổ sung.
              </div>

              <div 
                v-for="(cp, idx) in contactPersonsForm" 
                :key="idx"
                class="p-3.5 bg-slate-50/70 rounded-xl border border-slate-200 space-y-3 relative hover:border-blue-300 transition"
              >
                <div class="flex items-center justify-between">
                  <span class="text-xs font-bold text-slate-800 flex items-center gap-1.5">
                    <span class="w-5 h-5 rounded-full bg-blue-600 text-white text-xs flex items-center justify-center font-bold">{{ idx + 1 }}</span>
                    <span>Cán bộ đầu mối #{{ idx + 1 }}</span>
                  </span>
                  <button 
                    @click="removeContactPerson(idx)" 
                    class="ui-single-line text-rose-600 hover:text-rose-800 text-xs font-bold flex items-center gap-1 cursor-pointer px-2 py-1 hover:bg-rose-50 rounded-lg transition"
                  >
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
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
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Chức vụ</label>
                    <input 
                      v-model="cp.position" 
                      type="text" 
                      placeholder="Ví dụ: Trưởng phòng / Phó Cục trưởng"
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Đơn vị / Phòng ban</label>
                    <input 
                      v-model="cp.department" 
                      type="text" 
                      placeholder="Ví dụ: Phòng Chuyển đổi số"
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Số điện thoại</label>
                    <input 
                      v-model="cp.phone" 
                      type="text" 
                      placeholder="Ví dụ: 0912 345 678"
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition font-mono"
                    />
                  </div>

                  <div class="sm:col-span-2">
                    <label class="block font-bold text-slate-700 mb-1">Email</label>
                    <input 
                      v-model="cp.email" 
                      type="email" 
                      placeholder="Ví dụ: datnt@bca.gov.vn"
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 2: KẾ HOẠCH CĐS 5 NĂM (2026-2030) -->
          <div v-show="activeEditTab === 'fiveYear'" class="space-y-4">
            <div class="flex items-center gap-3 p-3 bg-blue-50/70 rounded-xl border border-blue-200">
              <input 
                id="check-five-year" 
                type="checkbox" 
                v-model="fiveYearPlanForm.isIssued"
                class="w-4 h-4 text-blue-600 rounded border-slate-300 cursor-pointer"
              />
              <label for="check-five-year" class="font-bold text-slate-800 text-xs cursor-pointer select-none">
                Đơn vị đã ban hành Kế hoạch chuyển đổi số 5 năm giai đoạn 2026-2030
              </label>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block font-bold text-slate-700 mb-1">Số quyết định / Số hiệu văn bản</label>
                <input 
                  v-model="fiveYearPlanForm.documentNumber" 
                  type="text" 
                  placeholder="Ví dụ: 1266/QĐ-UBND"
                  class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                />
              </div>

              <div>
                <label class="block font-bold text-slate-700 mb-1">Ngày ban hành</label>
                <input 
                  v-model="fiveYearPlanForm.issueDate" 
                  type="date" 
                  class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                />
              </div>

              <div class="sm:col-span-2">
                <label class="block font-bold text-slate-700 mb-1">Ghi chú thêm</label>
                <textarea 
                  v-model="fiveYearPlanForm.notes" 
                  rows="2"
                  placeholder="Ghi chú về quyết định phê duyệt kế hoạch 5 năm..."
                  class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                ></textarea>
              </div>
            </div>

            <!-- Upload files for 5-Year Plan -->
            <div class="space-y-2 pt-2 border-t border-slate-200">
              <label class="block font-bold text-slate-800">Tệp đính kèm văn bản Kế hoạch 5 năm</label>
              
              <div v-if="fiveYearPlanForm.files && fiveYearPlanForm.files.length > 0" class="space-y-2 max-h-[140px] overflow-y-auto custom-scrollbar">
                <div 
                  v-for="(file, fIdx) in fiveYearPlanForm.files" 
                  :key="file.id || fIdx"
                  class="flex items-center justify-between gap-3 p-2.5 bg-slate-50 rounded-xl border border-slate-200"
                >
                  <div class="flex items-center gap-2 overflow-hidden">
                    <span>📄</span>
                    <span class="font-bold text-slate-800 truncate" :title="file.fileName">{{ file.fileName }}</span>
                    <span class="text-xs text-slate-400">({{ formatFileSize(file.fileSize) }})</span>
                  </div>
                  <button 
                    @click="removeFiveYearFile(fIdx)" 
                    class="text-rose-600 hover:text-rose-800 font-bold px-2 py-1 text-xs hover:bg-rose-50 rounded-lg cursor-pointer"
                  >
                    Xóa tệp
                  </button>
                </div>
              </div>

              <!-- Upload Dropzone -->
              <div class="border-2 border-dashed border-slate-300 hover:border-blue-500 rounded-xl p-3.5 text-center bg-slate-50/60 hover:bg-blue-50/30 transition cursor-pointer relative">
                <input 
                  type="file" 
                  multiple
                  @change="handleFiveYearFileUpload" 
                  accept=".pdf,.doc,.docx,.xls,.xlsx"
                  class="absolute inset-0 w-full h-full opacity-0 cursor-pointer" 
                  :disabled="uploadingFile"
                />
                <div class="flex flex-col items-center justify-center gap-1">
                  <span class="text-xs font-bold text-blue-600">
                    {{ uploadingFile ? 'Đang tải tệp lên...' : 'Nhấn vào đây để tải lên tệp Kế hoạch 5 năm (.pdf, .doc, .docx)' }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 3: KẾ HOẠCH CĐS HÀNG NĂM -->
          <div v-show="activeEditTab === 'annual'" class="space-y-4">
            <div class="flex items-center justify-between border-b border-slate-200 pb-2">
              <div>
                <h4 class="text-xs font-bold text-slate-800">Theo dõi ban hành Kế hoạch CĐS hàng năm</h4>
                <p class="text-xs text-slate-500">Tích chọn các năm đơn vị đã ban hành kế hoạch và đính kèm tệp văn bản phê duyệt.</p>
              </div>
              <button 
                @click="addAnnualPlanYear" 
                class="px-2.5 py-1 bg-indigo-50 hover:bg-indigo-100 text-indigo-700 text-xs font-bold rounded-lg border border-indigo-200 cursor-pointer"
              >
                + Thêm năm
              </button>
            </div>

            <!-- Table of Annual Plans -->
            <div class="border border-slate-200 rounded-xl overflow-hidden max-h-[360px] overflow-y-auto custom-scrollbar">
              <table class="w-full text-left text-xs border-collapse">
                <thead class="bg-slate-100 text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-10">
                  <tr>
                    <th class="p-2.5 text-center w-24">Năm</th>
                    <th class="p-2.5 text-center w-28">Đã ban hành</th>
                    <th class="p-2.5 min-w-[160px]">Số quyết định / Văn bản</th>
                    <th class="p-2.5 w-32">Ngày ban hành</th>
                    <th class="p-2.5 min-w-[200px]">Tệp đính kèm</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100">
                  <tr v-for="plan in annualPlansForm" :key="plan.year" :class="plan.isIssued ? 'bg-indigo-50/20' : 'bg-white'">
                    <td class="p-2.5 font-bold text-slate-800 text-center">Năm {{ plan.year }}</td>
                    <td class="p-2.5 text-center">
                      <input 
                        type="checkbox" 
                        v-model="plan.isIssued"
                        class="w-4 h-4 text-indigo-600 rounded border-slate-300 cursor-pointer"
                      />
                    </td>
                    <td class="p-2.5">
                      <input 
                        v-model="plan.documentNumber" 
                        type="text" 
                        placeholder="Số QĐ..." 
                        class="w-full text-xs px-2 py-1 bg-white border border-slate-300 rounded-lg focus:ring-1 focus:ring-indigo-500"
                      />
                    </td>
                    <td class="p-2.5">
                      <input 
                        v-model="plan.issueDate" 
                        type="date" 
                        class="w-full text-xs px-1.5 py-1 bg-white border border-slate-300 rounded-lg focus:ring-1 focus:ring-indigo-500"
                      />
                    </td>
                    <td class="p-2.5">
                      <div class="space-y-1">
                        <div v-for="(f, fIdx) in (plan.files || [])" :key="f.id || fIdx" class="flex items-center justify-between gap-1 text-xs bg-slate-100 px-2 py-0.5 rounded">
                          <span class="truncate max-w-[120px]" :title="f.fileName">📎 {{ f.fileName }}</span>
                          <button @click="plan.files.splice(fIdx, 1)" class="text-rose-600 font-bold hover:underline cursor-pointer">✕</button>
                        </div>
                        <label class="inline-flex items-center gap-1 text-xs text-indigo-600 hover:text-indigo-800 font-bold cursor-pointer hover:underline">
                          <span>+ Tải tệp</span>
                          <input type="file" @change="handleAnnualPlanFileUpload($event, plan)" class="hidden" accept=".pdf,.doc,.docx" />
                        </label>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- TAB 4: KHUNG KIẾN TRÚC SỐ -->
          <div v-show="activeEditTab === 'architecture'" class="space-y-4">
            <div class="flex items-center gap-3 p-3 bg-amber-50/70 rounded-xl border border-amber-200">
              <input 
                id="check-architecture" 
                type="checkbox" 
                v-model="digitalArchitectureForm.isIssued"
                class="w-4 h-4 text-amber-600 rounded border-slate-300 cursor-pointer"
              />
              <label for="check-architecture" class="font-bold text-slate-800 text-xs cursor-pointer select-none">
                Đơn vị đã ban hành Khung kiến trúc Chính quyền điện tử / Chính quyền số / Khung kiến trúc số
              </label>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block font-bold text-slate-700 mb-1">Phiên bản kiến trúc</label>
                <input 
                  v-model="digitalArchitectureForm.version" 
                  type="text" 
                  placeholder="Ví dụ: Phiên bản 2.0 / 3.0"
                  class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-amber-500 focus:outline-none transition"
                />
              </div>

              <div>
                <label class="block font-bold text-slate-700 mb-1">Số quyết định / Số hiệu văn bản</label>
                <input 
                  v-model="digitalArchitectureForm.documentNumber" 
                  type="text" 
                  placeholder="Ví dụ: 890/QĐ-UBND"
                  class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-amber-500 focus:outline-none transition"
                />
              </div>

              <div>
                <label class="block font-bold text-slate-700 mb-1">Ngày ban hành</label>
                <input 
                  v-model="digitalArchitectureForm.issueDate" 
                  type="date" 
                  class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-amber-500 focus:outline-none transition"
                />
              </div>

              <div>
                <label class="block font-bold text-slate-700 mb-1">Ghi chú thêm</label>
                <input 
                  v-model="digitalArchitectureForm.notes" 
                  type="text" 
                  placeholder="Ghi chú về Khung kiến trúc số..."
                  class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-amber-500 focus:outline-none transition"
                />
              </div>
            </div>

            <!-- Upload files for Architecture -->
            <div class="space-y-2 pt-2 border-t border-slate-200">
              <label class="block font-bold text-slate-800">Tệp đính kèm văn bản Khung kiến trúc số</label>
              
              <div v-if="digitalArchitectureForm.files && digitalArchitectureForm.files.length > 0" class="space-y-2 max-h-[140px] overflow-y-auto custom-scrollbar">
                <div 
                  v-for="(file, fIdx) in digitalArchitectureForm.files" 
                  :key="file.id || fIdx"
                  class="flex items-center justify-between gap-3 p-2.5 bg-slate-50 rounded-xl border border-slate-200"
                >
                  <div class="flex items-center gap-2 overflow-hidden">
                    <span>🏛️</span>
                    <span class="font-bold text-slate-800 truncate" :title="file.fileName">{{ file.fileName }}</span>
                    <span class="text-xs text-slate-400">({{ formatFileSize(file.fileSize) }})</span>
                  </div>
                  <button 
                    @click="digitalArchitectureForm.files.splice(fIdx, 1)" 
                    class="text-rose-600 hover:text-rose-800 font-bold px-2 py-1 text-xs hover:bg-rose-50 rounded-lg cursor-pointer"
                  >
                    Xóa tệp
                  </button>
                </div>
              </div>

              <!-- Upload Dropzone -->
              <div class="border-2 border-dashed border-slate-300 hover:border-amber-500 rounded-xl p-3.5 text-center bg-slate-50/60 hover:bg-amber-50/30 transition cursor-pointer relative">
                <input 
                  type="file" 
                  multiple
                  @change="handleArchitectureFileUpload" 
                  accept=".pdf,.doc,.docx,.xls,.xlsx"
                  class="absolute inset-0 w-full h-full opacity-0 cursor-pointer" 
                  :disabled="uploadingFile"
                />
                <div class="flex flex-col items-center justify-center gap-1">
                  <span class="text-xs font-bold text-amber-700">
                    {{ uploadingFile ? 'Đang tải tệp lên...' : 'Nhấn vào đây để tải lên tệp Khung kiến trúc số (.pdf, .doc, .docx)' }}
                  </span>
                </div>
              </div>
            </div>
          </div>

        </div>

        <!-- Modal Footer -->
        <div class="px-6 py-3.5 bg-slate-50 border-t border-slate-200 flex items-center justify-end gap-3 shrink-0">
          <button 
            @click="closeModal" 
            class="ui-single-line px-4 py-2 bg-white hover:bg-slate-100 text-slate-700 text-xs font-bold rounded-xl border border-slate-300 transition shadow-2xs cursor-pointer"
            :disabled="saving"
          >
            Hủy bỏ
          </button>
          
          <button 
            @click="saveAgencyDetails" 
            class="ui-single-line px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-xl shadow-md transition flex items-center gap-1.5 cursor-pointer"
            :disabled="saving"
          >
            <svg v-if="saving" class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
            <span>{{ saving ? 'Đang lưu...' : 'Lưu thay đổi' }}</span>
          </button>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import ActionButton from "../components/ActionButton.vue";
import DocumentPagination from "../features/document/DocumentPagination.vue";
import { formatNumber, formatDate } from "../shared/formatters";
import { fetchWithAuth } from '../services/auth';

import { ref, computed, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { loadExcelWriter } from '../utils/excelRuntime';
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
const fiveYearPlanFilter = ref('ALL');
const annualPlanFilter = ref('ALL');
const architectureFilter = ref('ALL');

// Detail Popups
const viewingContactsAgency = ref(null);
const viewingFiveYearPlanAgency = ref(null);
const viewingAnnualPlansAgency = ref(null);
const viewingArchitectureAgency = ref(null);

// Edit Modal
const showModal = ref(false);
const editingAgency = ref(null);
const activeEditTab = ref('contacts');
const contactPersonsForm = ref([]);
const fiveYearPlanForm = ref({ isIssued: false, documentNumber: '', issueDate: '', period: '2026-2030', notes: '', files: [] });
const planFilesForm = ref([]);
const annualPlansForm = ref([]);
const digitalArchitectureForm = ref({ isIssued: false, documentNumber: '', version: '', issueDate: '', notes: '', files: [] });

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
  if (currentUserAgencyId.value) {
    const userAgency = currentUserAgency.value || authState.user.value?.agency;
    if (userAgency && isBKHCN(userAgency)) return true;

    const code = (currentUserAgencyCode.value || '').toLowerCase();
    if (code === 'bkhcn') return true;

    const userAgName = (authState.user.value?.agencyName || authState.user.value?.agency?.name || '').toLowerCase();
    if (userAgName.includes('khoa học và công nghệ') || userAgName.includes('khoa học & công nghệ') || userAgName.includes('khoa học công nghệ')) return true;

    return false;
  }
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

  if (userAgId && (agency.id === userAgId || agency.parentId === userAgId)) return true;
  if (userAgency && userAgency.parentId && agency.id === userAgency.parentId) return true;
  if (userAgCode && agency.code && agency.code.toLowerCase() === userAgCode.toLowerCase()) return true;
  if (isBKHCN(agency)) return true;
  if (isBKHCNChild(agency)) return true;

  return false;
}

// Helpers for Table Display
function hasContacts(item) {
  return item.hasContacts ?? (item.contactPersons && item.contactPersons.length > 0);
}

function hasFiveYearPlan(item) {
  return item.hasFiveYearPlan ?? (
    (item.fiveYearPlan && (item.fiveYearPlan.isIssued || (item.fiveYearPlan.files && item.fiveYearPlan.files.length > 0))) ||
    (item.planFiles && item.planFiles.length > 0)
  );
}

function getFiveYearPlanFiles(agency) {
  if (!agency) return [];
  if (agency.fiveYearPlan?.files && agency.fiveYearPlan.files.length > 0) {
    return agency.fiveYearPlan.files;
  }
  return agency.planFiles || [];
}

function hasAnnualPlan(item) {
  return item.hasAnnualPlan ?? (
    item.annualPlans && item.annualPlans.some(p => p.isIssued || (p.files && p.files.length > 0))
  );
}

function annualPlansIssuedCount(item) {
  if (!item.annualPlans) return 0;
  return item.annualPlans.filter(p => p.isIssued || (p.files && p.files.length > 0)).length;
}

function getDisplayAnnualPlans(agency) {
  if (!agency) return [];
  const standardYears = [2025, 2026, 2027, 2028, 2029, 2030];
  const existing = agency.annualPlans || [];
  return standardYears.map(year => {
    const found = existing.find(p => p.year === year);
    return found ? { ...found } : { year, isIssued: false, documentNumber: '', issueDate: null, files: [] };
  });
}

function hasDigitalArchitecture(item) {
  return item.hasDigitalArchitecture ?? (
    item.digitalArchitecture && (
      item.digitalArchitecture.isIssued || 
      (item.digitalArchitecture.files && item.digitalArchitecture.files.length > 0) || 
      Boolean(item.digitalArchitecture.documentNumber)
    )
  );
}

// Open Detail Popups
function openViewContacts(item) {
  viewingContactsAgency.value = item;
}

function openViewFiveYearPlan(item) {
  viewingFiveYearPlanAgency.value = item;
}

function openViewAnnualPlans(item) {
  viewingAnnualPlansAgency.value = item;
}

function openViewArchitecture(item) {
  viewingArchitectureAgency.value = item;
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
    const res = await fetchWithAuth(getApiUrl(`/api/agencies?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      allAgenciesOptions.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (err) {
    console.error('Error loading agency select options:', err);
  }
}

function sortWithContactsFirst(list) {
  return [...list].sort((a, b) => {
    const aHasContacts = (a.contactPersons && a.contactPersons.length > 0) ? 1 : 0;
    const bHasContacts = (b.contactPersons && b.contactPersons.length > 0) ? 1 : 0;
    if (bHasContacts !== aHasContacts) {
      return bHasContacts - aHasContacts;
    }
    return (a.name || '').localeCompare(b.name || '', 'vi');
  });
}

async function fetchAgencies() {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    params.append('pageNumber', pageNumber.value);
    params.append('pageSize', pageSize.value);
    params.append('excludeSpecial', 'true');
    params.append('prioritizeContacts', 'true');

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

    if (fiveYearPlanFilter.value && fiveYearPlanFilter.value !== 'ALL') {
      params.append('fiveYearPlanFilter', fiveYearPlanFilter.value);
    }

    if (annualPlanFilter.value && annualPlanFilter.value !== 'ALL') {
      params.append('annualPlanFilter', annualPlanFilter.value);
    }

    if (architectureFilter.value && architectureFilter.value !== 'ALL') {
      params.append('architectureFilter', architectureFilter.value);
    }

    const res = await fetchWithAuth(getApiUrl(`/api/agencies?${params.toString()}`));
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
        agencies.value = sortWithContactsFirst(rawItems.filter(a => isAllowedAgencyForUser(a)));
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

const fiveYearPlanFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'SENT', label: 'Đã ban hành Kế hoạch 5 năm' },
  { value: 'NOT_SENT', label: 'Chưa ban hành Kế hoạch 5 năm' }
];

const annualPlanFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'PROVIDED', label: 'Đã ban hành Kế hoạch hàng năm' },
  { value: 'NOT_PROVIDED', label: 'Chưa ban hành Kế hoạch hàng năm' }
];

const architectureFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'PROVIDED', label: 'Đã ban hành Khung kiến trúc số' },
  { value: 'NOT_PROVIDED', label: 'Chưa ban hành Khung kiến trúc số' }
];

const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedAgencyId.value && selectedAgencyId.value !== 'ALL') count++;
  if (contactFilter.value && contactFilter.value !== 'ALL') count++;
  if (fiveYearPlanFilter.value && fiveYearPlanFilter.value !== 'ALL') count++;
  if (annualPlanFilter.value && annualPlanFilter.value !== 'ALL') count++;
  if (architectureFilter.value && architectureFilter.value !== 'ALL') count++;
  return count;
});

function applyFilters() {
  pageNumber.value = 1;
  fetchAgencies();
}

function resetFilters() {
  selectedAgencyId.value = 'ALL';
  contactFilter.value = 'ALL';
  fiveYearPlanFilter.value = 'ALL';
  annualPlanFilter.value = 'ALL';
  architectureFilter.value = 'ALL';
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

// Edit Modal Functions
function openEditModal(agency) {
  editingAgency.value = agency;
  activeEditTab.value = 'contacts';
  contactPersonsForm.value = JSON.parse(JSON.stringify(agency.contactPersons || []));

  // 5-Year Plan
  if (agency.fiveYearPlan) {
    fiveYearPlanForm.value = JSON.parse(JSON.stringify(agency.fiveYearPlan));
    if (!fiveYearPlanForm.value.files || fiveYearPlanForm.value.files.length === 0) {
      fiveYearPlanForm.value.files = JSON.parse(JSON.stringify(agency.planFiles || []));
    }
  } else {
    fiveYearPlanForm.value = {
      isIssued: (agency.planFiles && agency.planFiles.length > 0),
      documentNumber: '',
      issueDate: '',
      period: '2026-2030',
      notes: '',
      files: JSON.parse(JSON.stringify(agency.planFiles || []))
    };
  }
  planFilesForm.value = JSON.parse(JSON.stringify(fiveYearPlanForm.value.files || []));

  // Annual Plans (2025 - 2030)
  const standardYears = [2025, 2026, 2027, 2028, 2029, 2030];
  const existingAnnual = agency.annualPlans || [];
  annualPlansForm.value = standardYears.map(year => {
    const found = existingAnnual.find(p => p.year === year);
    if (found) {
      return JSON.parse(JSON.stringify(found));
    }
    return {
      year,
      isIssued: false,
      documentNumber: '',
      issueDate: '',
      notes: '',
      files: []
    };
  });

  // Digital Architecture
  if (agency.digitalArchitecture) {
    digitalArchitectureForm.value = JSON.parse(JSON.stringify(agency.digitalArchitecture));
  } else {
    digitalArchitectureForm.value = {
      isIssued: false,
      documentNumber: '',
      version: 'Phiên bản 2.0',
      issueDate: '',
      notes: '',
      files: []
    };
  }

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

function addAnnualPlanYear() {
  const maxYear = annualPlansForm.value.reduce((max, p) => Math.max(max, p.year), 2030);
  const nextYear = maxYear + 1;
  annualPlansForm.value.push({
    year: nextYear,
    isIssued: false,
    documentNumber: '',
    issueDate: '',
    notes: '',
    files: []
  });
}

// Upload file helper
async function uploadSingleFile(file) {
  const formData = new FormData();
  formData.append('file', file);
  const res = await fetchWithAuth(getApiUrl('/api/agencies/upload-file'), {
    method: 'POST',
    body: formData
  });
  if (!res.ok) throw new Error('Lỗi khi tải tệp');
  return await res.json();
}

async function handleFiveYearFileUpload(event) {
  const files = Array.from(event.target.files || []);
  if (files.length === 0) return;

  uploadingFile.value = true;
  try {
    for (const file of files) {
      const uploaded = await uploadSingleFile(file);
      fiveYearPlanForm.value.files.push(uploaded);
      planFilesForm.value.push(uploaded);
    }
    toast.success('Tải lên tệp kế hoạch 5 năm thành công!');
  } catch (err) {
    console.error(err);
    toast.error('Lỗi khi tải tệp kế hoạch');
  } finally {
    uploadingFile.value = false;
    event.target.value = '';
  }
}

function removeFiveYearFile(index) {
  fiveYearPlanForm.value.files.splice(index, 1);
  planFilesForm.value.splice(index, 1);
}

async function handleAnnualPlanFileUpload(event, plan) {
  const files = Array.from(event.target.files || []);
  if (files.length === 0) return;

  uploadingFile.value = true;
  try {
    for (const file of files) {
      const uploaded = await uploadSingleFile(file);
      if (!plan.files) plan.files = [];
      plan.files.push(uploaded);
      plan.isIssued = true; // Auto check issued if file uploaded
    }
    toast.success(`Đã đính kèm tệp cho năm ${plan.year}!`);
  } catch (err) {
    console.error(err);
    toast.error('Lỗi khi tải tệp');
  } finally {
    uploadingFile.value = false;
    event.target.value = '';
  }
}

async function handleArchitectureFileUpload(event) {
  const files = Array.from(event.target.files || []);
  if (files.length === 0) return;

  uploadingFile.value = true;
  try {
    for (const file of files) {
      const uploaded = await uploadSingleFile(file);
      if (!digitalArchitectureForm.value.files) digitalArchitectureForm.value.files = [];
      digitalArchitectureForm.value.files.push(uploaded);
      digitalArchitectureForm.value.isIssued = true;
    }
    toast.success('Tải lên tệp khung kiến trúc thành công!');
  } catch (err) {
    console.error(err);
    toast.error('Lỗi khi tải tệp');
  } finally {
    uploadingFile.value = false;
    event.target.value = '';
  }
}

async function saveAgencyDetails() {
  if (!editingAgency.value) return;

  saving.value = true;
  try {
    // Sync planFiles with fiveYearPlan files
    planFilesForm.value = fiveYearPlanForm.value.files || [];

    const payload = {
      contactPersons: contactPersonsForm.value.filter(cp => cp.name && cp.name.trim() !== ''),
      planFiles: planFilesForm.value,
      planFileIds: planFilesForm.value.map(f => f.id),
      fiveYearPlan: fiveYearPlanForm.value,
      annualPlans: annualPlansForm.value,
      digitalArchitecture: digitalArchitectureForm.value
    };

    const res = await fetchWithAuth(getApiUrl(`/api/agencies/${editingAgency.value.id}/plans-and-contacts?userAgencyId=${currentUserAgencyId.value || ''}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
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

// Export Excel
async function exportAgencyPlansToExcel() {
  try {
    const XLSX = await loadExcelWriter();

    const params = new URLSearchParams();
    params.append('excludeSpecial', 'true');
    params.append('prioritizeContacts', 'true');

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

    if (fiveYearPlanFilter.value && fiveYearPlanFilter.value !== 'ALL') {
      params.append('fiveYearPlanFilter', fiveYearPlanFilter.value);
    }

    if (annualPlanFilter.value && annualPlanFilter.value !== 'ALL') {
      params.append('annualPlanFilter', annualPlanFilter.value);
    }

    if (architectureFilter.value && architectureFilter.value !== 'ALL') {
      params.append('architectureFilter', architectureFilter.value);
    }

    const res = await fetchWithAuth(getApiUrl(`/api/agencies?${params.toString()}`));
    if (!res.ok) throw new Error('Không thể lấy dữ liệu danh sách đơn vị từ máy chủ');
    
    const data = await res.json();
    let agencyList = Array.isArray(data) ? data : (data.items || []);
    if (!isUserLevel1.value) {
      agencyList = sortWithContactsFirst(agencyList.filter(a => isAllowedAgencyForUser(a)));
    }

    if (agencyList.length === 0) {
      toast.warning('Không có dữ liệu đơn vị phù hợp để xuất Excel!');
      return;
    }

    const excelRows = agencyList.map((agency, index) => {
      let contactStr = 'Chưa có';
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

      // Five-year plan string
      let fiveYearStr = 'Chưa ban hành';
      if (hasFiveYearPlan(agency)) {
        const files = getFiveYearPlanFiles(agency).map(f => f.fileName).join(', ');
        fiveYearStr = `Đã ban hành${agency.fiveYearPlan?.documentNumber ? ' (' + agency.fiveYearPlan.documentNumber + ')' : ''}${files ? ' - Tệp: ' + files : ''}`;
      }

      // Annual plan string
      let annualStr = 'Chưa ban hành';
      if (hasAnnualPlan(agency)) {
        const issued = (agency.annualPlans || []).filter(p => p.isIssued || (p.files && p.files.length > 0));
        annualStr = issued.map(p => `Năm ${p.year}: ${p.documentNumber || 'Đã ban hành'}`).join('\n');
      }

      // Architecture string
      let archStr = 'Chưa ban hành';
      if (hasDigitalArchitecture(agency)) {
        archStr = `Đã ban hành${agency.digitalArchitecture?.version ? ' (' + agency.digitalArchitecture.version + ')' : ''}${agency.digitalArchitecture?.documentNumber ? ' - QĐ: ' + agency.digitalArchitecture.documentNumber : ''}`;
      }

      return {
        'STT': index + 1,
        'Cơ quan, đơn vị': agency.name || '',
        'Có đầu mối liên hệ': hasContacts(agency) ? 'Có' : 'Chưa có',
        'Chi tiết đầu mối': contactStr,
        'KH CĐS 5 năm': hasFiveYearPlan(agency) ? 'Đã ban hành' : 'Chưa ban hành',
        'Chi tiết KH 5 năm': fiveYearStr,
        'KH CĐS hàng năm': hasAnnualPlan(agency) ? 'Đã ban hành' : 'Chưa ban hành',
        'Chi tiết KH hàng năm': annualStr,
        'Khung kiến trúc số': hasDigitalArchitecture(agency) ? 'Đã ban hành' : 'Chưa ban hành',
        'Chi tiết kiến trúc số': archStr
      };
    });

    const ws = XLSX.utils.json_to_sheet(excelRows);

    ws['!cols'] = [
      { wch: 8 },  // STT
      { wch: 35 }, // Cơ quan, đơn vị
      { wch: 18 }, // Có đầu mối liên hệ
      { wch: 50 }, // Chi tiết đầu mối
      { wch: 18 }, // KH CĐS 5 năm
      { wch: 40 }, // Chi tiết KH 5 năm
      { wch: 20 }, // KH CĐS hàng năm
      { wch: 40 }, // Chi tiết KH hàng năm
      { wch: 20 }, // Khung kiến trúc số
      { wch: 40 }  // Chi tiết kiến trúc số
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
          ws[cellRef].s = (c === 0 || c === 2 || c === 4 || c === 6 || c === 8) ? bodyStyleCenter : bodyStyleLeft;
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
