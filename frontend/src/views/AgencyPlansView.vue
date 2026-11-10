<template>
  <div class="w-full min-w-0 font-sans rounded-2xl border border-slate-200/80 bg-white shadow-sm overflow-hidden">
    
    <!-- Top Header Bar -->
    <div class="ui-screen-heading bg-[#f1f5fc] p-4 sm:p-5 rounded-t-2xl border-b border-slate-200/80 space-y-3.5 w-full">
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div class="flex items-center gap-3 min-w-0">
          <div class="ui-screen-icon w-10 h-10 rounded-2xl flex items-center justify-center shrink-0" aria-hidden="true">
            <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <h2 class="ui-page-title tracking-tight text-slate-800 font-bold text-lg">
              Thông tin chuyển đổi số
            </h2>
          </div>
        </div>

        <div class="flex items-center gap-2 flex-wrap shrink-0">
          <ActionButton kind="export" 
            @click="exportAgencyPlansToExcel"
            title="Xuất file Excel thông tin chuyển đổi số"
          >
            <span>Xuất excel</span>
          </ActionButton>
        </div>
      </div>

      <!-- Navigation Tabs (Matching AiDataView / Dữ liệu từ internet style) -->
      <nav role="tablist" aria-label="Phân loại cơ quan" class="flex items-center gap-7 border-t border-slate-200/70 pt-2.5 -mb-2 overflow-x-auto custom-scrollbar">
        <button 
          type="button"
          role="tab"
          :aria-selected="currentTab === 'ministry'"
          @click="changeTab('ministry')"
          class="pb-2 text-xs transition cursor-pointer border-b-2 whitespace-nowrap"
          :class="currentTab === 'ministry' 
            ? 'border-blue-600 text-blue-700 font-bold' 
            : 'border-transparent text-slate-600 hover:text-slate-900 font-semibold'"
        >
          Các bộ, ngành
        </button>

        <button 
          type="button"
          role="tab"
          :aria-selected="currentTab === 'province'"
          @click="changeTab('province')"
          class="pb-2 text-xs transition cursor-pointer border-b-2 whitespace-nowrap"
          :class="currentTab === 'province' 
            ? 'border-blue-600 text-blue-700 font-bold' 
            : 'border-transparent text-slate-600 hover:text-slate-900 font-semibold'"
        >
          Địa phương
        </button>

        <button 
          type="button"
          role="tab"
          :aria-selected="currentTab === 'internal'"
          @click="changeTab('internal')"
          class="pb-2 text-xs transition cursor-pointer border-b-2 whitespace-nowrap"
          :class="currentTab === 'internal' 
            ? 'border-blue-600 text-blue-700 font-bold' 
            : 'border-transparent text-slate-600 hover:text-slate-900 font-semibold'"
        >
          Danh sách đầu mối trực thuộc bộ
        </button>
      </nav>
    </div>

    <!-- Main Card Container: Filter + Table + Pagination -->
    <div class="p-3.5 sm:p-4 w-full">
      <div class="w-full border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col">
      
      <!-- Filter & Search Bar -->
      <div class="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3 p-3 bg-slate-50/60 w-full border-b border-slate-200/80">
        <div class="flex flex-wrap sm:flex-nowrap items-center gap-2 flex-1 max-w-xl min-w-0">
          <!-- Search Input -->
          <div class="relative flex-1 min-w-0 basis-full sm:basis-0">
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
            <input 
              v-model="searchQuery" 
              @keyup.enter="execSearch"
              placeholder="Tìm theo tên cơ quan, số hiệu VB phúc đáp, quyết định, cán bộ đầu mối, SĐT, ghi chú..." 
              class="ui-single-line w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none"
            />
          </div>

          <!-- OverlayPanel Advanced Filter Popover -->
          <OverlayPanel 
            title="Bộ lọc tìm kiếm nâng cao"
            buttonText="Lọc nâng cao"
            :activeCount="activeFilterCount"
            widthClass="w-[340px] sm:w-[480px]"
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

              <template v-if="currentTab !== 'internal'">
                <div>
                  <SearchableSelect 
                    v-model="fiveYearPlanFilter" 
                    :options="fiveYearPlanFilterOptions" 
                    :isMulti="false" 
                    label="Tình trạng Kế hoạch CĐS 5 năm" 
                    placeholder="Tất cả trạng thái"
                  />
                </div>

                <div>
                  <SearchableSelect 
                    v-model="responseDocFilter" 
                    :options="responseDocFilterOptions" 
                    :isMulti="false" 
                    label="Văn bản phúc đáp" 
                    placeholder="Tất cả trạng thái"
                  />
                </div>

                <div>
                  <SearchableSelect 
                    v-model="updated1266Filter" 
                    :options="updated1266FilterOptions" 
                    :isMulti="false" 
                    label="Đã cập nhật theo QĐ 1266/QĐ-TTg" 
                    placeholder="Tất cả trạng thái"
                  />
                </div>
              </template>
            </div>
          </OverlayPanel>
        </div>
      </div>

      <!-- ==================================================================== -->
      <!-- DATA TABLE: TAB 1 (BỘ NGÀNH), TAB 2 (ĐỊA PHƯƠNG)                    -->
      <!-- ==================================================================== -->
      <div v-if="currentTab !== 'internal'" class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
        <LoadingSpinner v-if="loading" text="Đang tải danh sách cơ quan & thông tin kế hoạch CĐS..." />

        <table v-else class="w-full min-w-[1360px] text-left text-xs text-slate-700 border-collapse table-fixed">
          <colgroup>
            <col class="w-12" />
            <col class="w-48" />
            <col class="w-56" />
            <col class="w-28" />
            <col class="w-72" />
            <col class="w-28" />
            <col class="w-28" />
            <col class="w-48" />
            <col class="w-20" />
          </colgroup>
          <thead class="bg-slate-100 text-xs text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-10 shadow-2xs">
            <tr class="h-10">
              <th class="px-3 py-2 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap">STT</th>
              <th class="px-3 py-2 border-r border-slate-200 bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Tên Cơ quan'">Tên Cơ quan</span></th>
              <th class="px-3 py-2 border-r border-slate-200 bg-slate-100 whitespace-nowrap">
                <span v-if="currentTab === 'ministry'" class="block truncate" v-tip="'Phúc đáp VB 5799'">Phúc đáp VB 5799</span>
                <span v-else-if="currentTab === 'province'" class="block truncate" v-tip="'Phúc đáp VB 5799 & 8841'">Phúc đáp VB 5799 &amp; 8841</span>
                <span v-else class="block truncate">Phúc đáp</span>
              </th>
              <th class="px-2 py-2 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Có KH 05 năm'">Có KH 05 năm</span></th>
              <th class="px-3 py-2 border-r border-slate-200 bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Tên số hiệu QĐ, KH của Cơ quan'">Tên số hiệu QĐ, KH của Cơ quan</span></th>
              <th class="px-2 py-2 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Đã cập nhật 1266'">Đã cập nhật 1266</span></th>
              <th class="px-2 py-2 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Đầu mối liên hệ'">Đầu mối liên hệ</span></th>
              <th class="px-3 py-2 border-r border-slate-200 bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Ghi chú'">Ghi chú</span></th>
              <th class="px-2 py-2 text-center bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Thao tác'">Thao tác</span></th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-xs">
            <tr v-if="agencies.length === 0">
              <td colspan="9" class="py-12 text-center text-slate-400">
                <div class="flex flex-col items-center gap-2">
                  <svg class="w-10 h-10 text-slate-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                  </svg>
                  <span class="font-bold text-slate-600">Không tìm thấy cơ quan phù hợp với bộ lọc</span>
                </div>
              </td>
            </tr>

            <tr 
              v-for="(item, index) in agencies" 
              :key="item.id"
              class="h-10 hover:bg-blue-50/40 transition-colors"
            >
              <!-- STT -->
              <td class="px-3 py-2 border-r border-slate-200 text-center font-bold text-slate-500 whitespace-nowrap">
                {{ (pageNumber - 1) * pageSize + index + 1 }}
              </td>

              <!-- Agency Name -->
              <td class="px-3 py-2 border-r border-slate-200 font-bold text-slate-900 whitespace-nowrap overflow-hidden">
                <div class="flex items-center gap-1.5 w-full">
                  <span class="truncate block flex-1" v-tip="item.name">{{ item.name }}</span>
                  <span v-if="currentTab === 'all' && isMinistryAgency(item)" class="inline-flex items-center px-1.5 py-0.2 rounded text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200 shrink-0">Bộ</span>
                  <span v-else-if="currentTab === 'all' && isProvinceAgency(item)" class="inline-flex items-center px-1.5 py-0.2 rounded text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 shrink-0">Tỉnh</span>
                </div>
              </td>

              <!-- Văn bản phúc đáp -->
              <td class="px-3 py-2 border-r border-slate-200 whitespace-nowrap overflow-hidden">
                <span 
                  v-if="item.fiveYearPlan?.responseDocument" 
                  class="truncate block font-semibold text-slate-800" 
                  v-tip="item.fiveYearPlan.responseDocument"
                >
                  {{ item.fiveYearPlan.responseDocument }}
                </span>
                <span v-else class="text-slate-300 font-normal select-none block text-center">—</span>
              </td>

              <!-- Có KH 05 năm (Chỉ hiển thị dấu tick xanh) -->
              <td class="px-2 py-2 border-r border-slate-200 text-center whitespace-nowrap">
                <span 
                  v-if="hasFiveYearPlan(item)" 
                  class="inline-flex items-center justify-center text-emerald-600 font-bold cursor-default" 
                  v-tip="'Đã có Kế hoạch Chuyển đổi số 05 năm'"
                >
                  <svg class="w-4 h-4 text-emerald-600 stroke-[2.5]" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" /></svg>
                </span>
                <span v-else class="text-slate-300 font-normal select-none">—</span>
              </td>

              <!-- Tên số hiệu QĐ, KH của Cơ quan -->
              <td class="px-3 py-2 border-r border-slate-200 whitespace-nowrap overflow-hidden">
                <div v-if="item.fiveYearPlan?.documentNumber" class="flex items-center gap-1.5 w-full">
                  <span 
                    class="truncate block font-medium text-slate-800 flex-1" 
                    v-tip="item.fiveYearPlan.documentNumber"
                  >
                    {{ item.fiveYearPlan.documentNumber }}
                  </span>
                  <button 
                    v-if="getFiveYearPlanFiles(item).length > 0"
                    @click="openViewFiveYearPlan(item)"
                    class="shrink-0 text-blue-600 hover:text-blue-800 cursor-pointer p-0.5 rounded hover:bg-blue-50 text-xs"
                    v-tip="'Có ' + getFiveYearPlanFiles(item).length + ' tệp đính kèm. Bấm để xem/tải.'"
                  >
                    📎
                  </button>
                </div>
                <div v-else-if="getFiveYearPlanFiles(item).length > 0" class="flex items-center">
                  <button 
                    @click="openViewFiveYearPlan(item)"
                    class="text-xs text-blue-600 hover:underline cursor-pointer flex items-center gap-1 font-bold"
                  >
                    <span>📎</span>
                    <span>{{ getFiveYearPlanFiles(item).length }} tệp</span>
                  </button>
                </div>
                <span v-else class="text-slate-300 font-normal select-none block text-center">—</span>
              </td>

              <!-- Đã cập nhật 1266 (Chỉ hiển thị dấu tick xanh) -->
              <td class="px-2 py-2 border-r border-slate-200 text-center whitespace-nowrap">
                <span 
                  v-if="item.fiveYearPlan?.isUpdated1266" 
                  class="inline-flex items-center justify-center text-emerald-600 font-bold cursor-default" 
                  v-tip="'Đã cập nhật theo Chiến lược quốc gia QĐ 1266/QĐ-TTg'"
                >
                  <svg class="w-4 h-4 text-emerald-600 stroke-[2.5]" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" /></svg>
                </span>
                <span v-else class="text-slate-300 font-normal select-none">—</span>
              </td>

              <!-- Đầu mối liên hệ: Dấu tick và mở ngoặc số: click vô số thì hiện -->
              <td class="px-2 py-2 border-r border-slate-200 text-center whitespace-nowrap">
                <button 
                  v-if="hasContacts(item)" 
                  @click="openViewContacts(item)"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-lg text-xs font-bold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 hover:text-emerald-900 border border-emerald-200 transition cursor-pointer"
                  v-tip="'Có ' + item.contactPersons.length + ' cán bộ đầu mối liên hệ. Nhấn để xem danh sách.'"
                >
                  <svg class="w-3.5 h-3.5 text-emerald-600 stroke-[2.5] shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" /></svg>
                  <span>({{ item.contactPersons.length }})</span>
                </button>
                <span v-else class="text-slate-300 font-normal select-none">—</span>
              </td>

              <!-- Ghi chú (Dài quá thì ... di vào thì tooltip) -->
              <td class="px-3 py-2 border-r border-slate-200 whitespace-nowrap overflow-hidden">
                <span 
                  v-if="item.fiveYearPlan?.notes" 
                  class="truncate block text-slate-700" 
                  v-tip="item.fiveYearPlan.notes"
                >
                  {{ item.fiveYearPlan.notes }}
                </span>
                <span v-else class="text-slate-300 font-normal select-none block text-center">—</span>
              </td>

              <!-- Thao tác -->
              <td class="px-2 py-2 text-center whitespace-nowrap">
                <button 
                  v-if="canEditAgency(item)"
                  @click="openEditModal(item)"
                  class="ui-action ui-action-primary p-1.5 bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-blue-700 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer transition mx-auto group"
                  v-tip="'Cập nhật Kế hoạch CĐS & Đầu mối liên hệ'"
                  aria-label="Cập nhật Kế hoạch CĐS & Đầu mối liên hệ">
                  <svg class="w-4 h-4 text-slate-600 group-hover:text-blue-700 transition" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <span v-else class="p-1.5 bg-slate-50 text-slate-300 rounded-lg border border-slate-100 inline-flex items-center justify-center mx-auto" v-tip="'Bạn không có quyền chỉnh sửa đơn vị này'">
                  <svg class="w-4 h-4 text-slate-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ==================================================================== -->
      <!-- DATA TABLE: TAB 3 (DANH SÁCH ĐẦU MỐI LIÊN HỆ - SHEET 3)             -->
      <!-- ==================================================================== -->
      <div v-else class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
        <LoadingSpinner v-if="loading" text="Đang tải danh sách đầu mối trực thuộc bộ..." />

        <table v-else class="w-full min-w-[1040px] text-left text-xs text-slate-700 border-collapse table-fixed">
          <colgroup>
            <col class="w-12" />
            <col class="w-64" />
            <col class="w-32" />
            <col class="w-[520px]" />
            <col class="w-20" />
          </colgroup>
          <thead class="bg-slate-100 text-xs text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-10 shadow-2xs">
            <tr class="h-10">
              <th class="px-3 py-2 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap">STT</th>
              <th class="px-3 py-2 border-r border-slate-200 bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Tên Đơn vị trực thuộc'">Tên Đơn vị trực thuộc</span></th>
              <th class="px-2 py-2 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Đã gửi đầu mối?'">Đã gửi đầu mối?</span></th>
              <th class="px-3 py-2 border-r border-slate-200 bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Thông tin cán bộ đầu mối (di chuột xem đầy đủ)'">Thông tin cán bộ đầu mối (di chuột xem đầy đủ)</span></th>
              <th class="px-2 py-2 text-center bg-slate-100 whitespace-nowrap"><span class="block truncate" v-tip="'Thao tác'">Thao tác</span></th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 text-xs">
            <tr v-if="agencies.length === 0">
              <td colspan="5" class="py-12 text-center text-slate-400">
                <span class="font-bold text-slate-600">Không tìm thấy đơn vị trực thuộc phù hợp</span>
              </td>
            </tr>

            <tr 
              v-for="(item, index) in agencies" 
              :key="item.id"
              class="h-10 hover:bg-emerald-50/30 transition-colors"
            >
              <!-- STT -->
              <td class="px-3 py-2 border-r border-slate-200 text-center font-bold text-slate-500 whitespace-nowrap">
                {{ (pageNumber - 1) * pageSize + index + 1 }}
              </td>

              <!-- Subordinate Unit Name -->
              <td class="px-3 py-2 border-r border-slate-200 font-bold text-slate-900 whitespace-nowrap overflow-hidden">
                <div class="flex items-center gap-1.5 w-full">
                  <span class="truncate block flex-1" v-tip="item.name">{{ item.name }}</span>
                </div>
              </td>

              <!-- Đã gửi đầu mối? (Chỉ hiển thị dấu tick xanh) -->
              <td class="px-2 py-2 border-r border-slate-200 text-center whitespace-nowrap">
                <span 
                  v-if="hasContacts(item)" 
                  class="inline-flex items-center justify-center text-emerald-600 font-bold cursor-default" 
                  v-tip="'Đã cung cấp thông tin cán bộ đầu mối'"
                >
                  <svg class="w-4 h-4 text-emerald-600 stroke-[2.5]" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" /></svg>
                </span>
                <span v-else class="text-slate-300 font-normal select-none">—</span>
              </td>

              <!-- Thông tin đầu mối chi tiết (Cố định 1 dòng, dài quá thì ... di vào thì tooltip) -->
              <td class="px-3 py-2 border-r border-slate-200 whitespace-nowrap overflow-hidden">
                <div 
                  v-if="hasContacts(item)" 
                  @click="openViewContacts(item)"
                  class="truncate font-medium text-slate-800 cursor-pointer hover:text-blue-700 flex items-center gap-2"
                  v-tip="formatContactsTooltip(item.contactPersons)"
                >
                  <span class="px-1.5 py-0.2 bg-emerald-100 text-emerald-800 rounded font-bold text-xs shrink-0">({{ item.contactPersons.length }})</span>
                  <span class="truncate">{{ formatContactsSingleLine(item.contactPersons) }}</span>
                </div>
                <span v-else class="text-slate-300 font-normal select-none block text-center">—</span>
              </td>

              <!-- Thao tác -->
              <td class="px-2 py-2 text-center whitespace-nowrap">
                <button 
                  v-if="canEditAgency(item)"
                  @click="openEditModal(item, 'contacts')"
                  class="ui-action ui-action-primary p-1.5 bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-blue-700 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer transition mx-auto group"
                  v-tip="'Cập nhật cán bộ đầu mối liên hệ'"
                  aria-label="Cập nhật cán bộ đầu mối liên hệ">
                  <svg class="w-4 h-4 text-slate-600 group-hover:text-blue-700 transition" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <span v-else class="p-1.5 bg-slate-50 text-slate-300 rounded-lg border border-slate-100 inline-flex items-center justify-center mx-auto" v-tip="'Bạn không có quyền chỉnh sửa đơn vị này'">
                  <svg class="w-4 h-4 text-slate-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <DocumentPagination 
        v-model:current-page="pageNumber" 
        v-model:page-size="pageSize" 
        :total-count="totalCount" 
        :total-pages="Math.max(1, totalPages)" 
        :page-size-options="[10, 20, 35, 55].map(value => ({ value, label: String(value) }))" 
        :change-page="changePage" 
        :disabled="loading" 
        item-label="đơn vị" 
        @update:page-size="execSearch" 
      />
      </div>
    </div>

    <!-- ==================================================================== -->
    <!-- DETAIL POPUP 1: XEM CHI TIẾT CÁN BỘ ĐẦU MỐI                          -->
    <!-- ==================================================================== -->
    <div 
      v-if="viewingContactsAgency" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
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
            :key="cp.id || idx"
            class="p-4 bg-slate-50 rounded-xl border border-slate-200 hover:border-emerald-300 transition space-y-2"
          >
            <div class="flex items-center justify-between">
              <span class="font-bold text-slate-900 text-sm flex items-center gap-2">
                <span class="w-5 h-5 rounded-full bg-emerald-600 text-white text-xs flex items-center justify-center font-bold">{{ idx + 1 }}</span>
                <span>{{ cp.name }}</span>
              </span>
              <span v-if="cp.position" class="px-2 py-0.5 bg-emerald-100 text-emerald-800 rounded text-xs font-semibold">
                {{ cp.position }}
              </span>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs pt-1 border-t border-slate-200/60">
              <div v-if="cp.department" class="text-slate-600">
                <span class="font-semibold text-slate-500">Đơn vị:</span> {{ cp.department }}
              </div>
              <div v-if="cp.phone" class="text-slate-600">
                <span class="font-semibold text-slate-500">Điện thoại:</span> 
                <a :href="'tel:' + cp.phone" class="text-blue-600 font-bold hover:underline font-mono ml-1">{{ cp.phone }}</a>
              </div>
              <div v-if="cp.email" class="text-slate-600 sm:col-span-2">
                <span class="font-semibold text-slate-500">Email:</span> 
                <a :href="'mailto:' + cp.email" class="text-blue-600 hover:underline font-mono ml-1">{{ cp.email }}</a>
              </div>
              <div v-if="cp.notes" class="text-slate-600 sm:col-span-2 italic">
                <span class="font-semibold text-slate-500">Ghi chú:</span> {{ cp.notes }}
              </div>
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
    <!-- DETAIL POPUP 2: XEM CHI TIẾT KẾ HOẠCH 5 NĂM & TỆP ĐÍNH KÈM          -->
    <!-- ==================================================================== -->
    <div 
      v-if="viewingFiveYearPlanAgency" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-2xl overflow-hidden flex flex-col max-h-[85vh] my-auto">
        <div class="px-6 py-4 bg-slate-50 border-b border-slate-200 flex items-center justify-between shrink-0">
          <div class="flex items-center gap-3">
            <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm text-sm font-bold">📑</span>
            <div>
              <h3 class="text-base font-bold text-slate-800 leading-snug">Chi tiết Kế hoạch CĐS 5 năm (2026-2030)</h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">{{ viewingFiveYearPlanAgency.name }}</p>
            </div>
          </div>
          <button @click="viewingFiveYearPlanAgency = null" class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm">✕</button>
        </div>

        <div class="p-6 overflow-y-auto space-y-4 flex-1 custom-scrollbar text-xs">
          <!-- Metadata block -->
          <div class="bg-blue-50/60 p-4 rounded-xl border border-blue-200 space-y-2">
            <div class="flex items-center justify-between flex-wrap gap-2">
              <span class="font-bold text-slate-700">Tình trạng Kế hoạch:</span>
              <span 
                v-if="hasFiveYearPlan(viewingFiveYearPlanAgency)"
                class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-blue-100 text-blue-800 border border-blue-300"
              >
                ✓ Đã có Kế hoạch 05 năm
              </span>
              <span v-else class="text-slate-400">Chưa có</span>
            </div>

            <div v-if="viewingFiveYearPlanAgency.fiveYearPlan?.responseDocument" class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-blue-200/60">
              <span class="font-semibold text-slate-600">Văn bản phúc đáp:</span>
              <span class="font-bold text-blue-900">{{ viewingFiveYearPlanAgency.fiveYearPlan.responseDocument }}</span>
            </div>

            <div v-if="viewingFiveYearPlanAgency.fiveYearPlan?.documentNumber" class="pt-1 border-t border-blue-200/60">
              <span class="font-semibold text-slate-600">Tên số hiệu QĐ/KH của cơ quan:</span>
              <p class="font-bold text-slate-900 mt-0.5">{{ viewingFiveYearPlanAgency.fiveYearPlan.documentNumber }}</p>
            </div>

            <div class="flex items-center justify-between flex-wrap gap-2 pt-1 border-t border-blue-200/60">
              <span class="font-semibold text-slate-600">Đã cập nhật theo QĐ 1266/QĐ-TTg:</span>
              <span 
                v-if="viewingFiveYearPlanAgency.fiveYearPlan?.isUpdated1266"
                class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-300"
              >
                ✓ Đã cập nhật
              </span>
              <span v-else class="text-slate-500 font-medium">Chưa cập nhật</span>
            </div>

            <div v-if="viewingFiveYearPlanAgency.fiveYearPlan?.notes" class="pt-1 border-t border-blue-200/60">
              <span class="font-semibold text-slate-600">Ghi chú:</span>
              <p class="text-slate-800 mt-0.5">{{ viewingFiveYearPlanAgency.fiveYearPlan.notes }}</p>
            </div>
          </div>

          <!-- Files list -->
          <div class="space-y-2">
            <h4 class="font-bold text-slate-800 flex items-center gap-1.5">
              <span>📎</span>
              <span>Tệp đính kèm văn bản Kế hoạch 5 năm</span>
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
                  <span class="font-bold text-slate-800 text-xs truncate block" v-tip="file.fileName">{{ file.fileName }}</span>
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
    <!-- EDIT MODAL: CẬP NHẬT KẾ HOẠCH CĐS 5 NĂM & ĐẦU MỐI LIÊN HỆ           -->
    <!-- ==================================================================== -->
    <div 
      v-if="showModal" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-150 font-sans"
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
                Cập nhật thông tin Chuyển đổi số
              </h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">
                {{ editingAgency?.name }} ({{ isMinistryAgency(editingAgency) ? 'Bộ, ngành' : (isProvinceAgency(editingAgency) ? 'Địa phương' : 'Đơn vị trực thuộc') }})
              </p>
            </div>
          </div>

          <button 
            @click="closeModal" 
            class="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-500 hover:text-slate-800 flex items-center justify-center transition cursor-pointer font-bold text-sm"
            v-tip="'Đóng cửa sổ'"
          >
            ✕
          </button>
        </div>

        <!-- Navigation Tabs in Modal -->
        <div class="flex items-center border-b border-slate-200 px-6 bg-slate-50/70 gap-2 shrink-0 overflow-x-auto custom-scrollbar">
          <button 
            v-if="!isInternalAgency(editingAgency)"
            @click="activeEditTab = 'fiveYear'"
            :class="activeEditTab === 'fiveYear' ? 'border-b-2 border-blue-600 text-blue-700 font-bold bg-white' : 'text-slate-600 hover:text-slate-900 font-medium'"
            class="px-4 py-2.5 text-xs transition cursor-pointer whitespace-nowrap flex items-center gap-1.5"
          >
            <span>📑</span>
            <span>1. Kế hoạch CĐS 5 năm & Phúc đáp</span>
            <span v-if="fiveYearPlanForm.hasReviewedPlan || fiveYearPlanForm.isIssued" class="w-2 h-2 rounded-full bg-blue-500"></span>
          </button>

          <button 
            @click="activeEditTab = 'contacts'"
            :class="activeEditTab === 'contacts' ? 'border-b-2 border-blue-600 text-blue-700 font-bold bg-white' : 'text-slate-600 hover:text-slate-900 font-medium'"
            class="px-4 py-2.5 text-xs transition cursor-pointer whitespace-nowrap flex items-center gap-1.5"
          >
            <span>👥</span>
            <span>{{ !isInternalAgency(editingAgency) ? '2. ' : '' }}Cán bộ đầu mối liên hệ</span>
            <span v-if="contactPersonsForm.length" class="ml-1 px-1.5 py-0.2 rounded-full text-xs bg-emerald-100 text-emerald-700">{{ contactPersonsForm.length }}</span>
          </button>
        </div>

        <!-- Modal Body (Active Tab Content) -->
        <div class="p-6 overflow-y-auto space-y-5 flex-1 custom-scrollbar text-xs">
          
          <!-- TAB 1: KẾ HOẠCH CĐS 5 NĂM & PHÚC ĐÁP -->
          <div v-show="activeEditTab === 'fiveYear'" class="space-y-4">
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <!-- Văn bản phúc đáp -->
              <div class="sm:col-span-2">
                <label class="block font-bold text-slate-700 mb-1">
                  Văn bản phúc đáp
                  <span class="text-slate-400 font-normal">
                    ({{ isMinistryAgency(editingAgency) ? 'VB 5799/BKHCN-CĐSQG' : 'VB 5799/BKHCN-CĐSQG và VB 8841/BKHCN-CĐSQG (lần 2)' }})
                  </span>
                </label>
                <input 
                  v-model="fiveYearPlanForm.responseDocument" 
                  type="text" 
                  class="ui-single-line w-full text-xs font-semibold px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                />
              </div>

              <!-- Checkbox: Đã rà soát, có KH 5 năm -->
              <div class="flex items-center gap-3 p-3 bg-blue-50/70 rounded-xl border border-blue-200">
                <input 
                  id="check-has-plan" 
                  type="checkbox" 
                  v-model="fiveYearPlanForm.hasReviewedPlan"
                  @change="onHasPlanChange"
                  class="w-4 h-4 text-blue-600 rounded border-slate-300 cursor-pointer"
                />
                <label for="check-has-plan" class="font-bold text-slate-800 text-xs cursor-pointer select-none">
                  Đã rà soát, có Kế hoạch 05 năm
                </label>
              </div>

              <!-- Checkbox: Đã cập nhật 1266 -->
              <div class="flex items-center gap-3 p-3 bg-emerald-50/70 rounded-xl border border-emerald-200">
                <input 
                  id="check-updated-1266" 
                  type="checkbox" 
                  v-model="fiveYearPlanForm.isUpdated1266"
                  class="w-4 h-4 text-emerald-600 rounded border-slate-300 cursor-pointer"
                />
                <label for="check-updated-1266" class="font-bold text-slate-800 text-xs cursor-pointer select-none">
                  Đã cập nhật Chiến lược quốc gia QĐ 1266/QĐ-TTg
                </label>
              </div>

              <!-- Tên, số hiệu QĐ/KH của Cơ quan -->
              <div class="sm:col-span-2">
                <label class="block font-bold text-slate-700 mb-1">
                  Tên số hiệu Quyết định, Kế hoạch của Cơ quan
                </label>
                <textarea 
                  v-model="fiveYearPlanForm.documentNumber" 
                  rows="3"
                  class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                ></textarea>
              </div>

              <!-- Ghi chú tiến độ, tình trạng -->
              <div class="sm:col-span-2">
                <label class="block font-bold text-slate-700 mb-1">Ghi chú tình hình rà soát, ban hành</label>
                <textarea 
                  v-model="fiveYearPlanForm.notes" 
                  rows="3"
                  class="w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
                ></textarea>
              </div>
            </div>

            <!-- Upload files for 5-Year Plan -->
            <div class="space-y-2 pt-2 border-t border-slate-200">
              <label class="block font-bold text-slate-800">Tệp đính kèm văn bản Kế hoạch 5 năm / Phúc đáp</label>
              
              <div v-if="fiveYearPlanForm.files && fiveYearPlanForm.files.length > 0" class="space-y-2 max-h-[140px] overflow-y-auto custom-scrollbar">
                <div 
                  v-for="(file, fIdx) in fiveYearPlanForm.files" 
                  :key="file.id || fIdx"
                  class="flex items-center justify-between gap-3 p-2.5 bg-slate-50 rounded-xl border border-slate-200"
                >
                  <div class="flex items-center gap-2 overflow-hidden">
                    <span>📄</span>
                    <span class="font-bold text-slate-800 truncate" v-tip="file.fileName">{{ file.fileName }}</span>
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
                    {{ uploadingFile ? 'Đang tải tệp lên...' : 'Nhấn vào đây để tải lên tệp văn bản Kế hoạch (.pdf, .doc, .docx)' }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 2: CÁN BỘ ĐẦU MỐI (GIỮ NGUYÊN) -->
          <div v-show="activeEditTab === 'contacts'" class="space-y-3">
            <div class="flex items-center justify-between border-b border-slate-200 pb-2">
              <div>
                <h4 class="text-xs font-bold text-slate-800">Danh sách cán bộ đầu mối chuyển đổi số</h4>
                <p class="text-xs text-slate-500">Khai báo thông tin các cán bộ chịu trách nhiệm đầu mối kết nối, tổng hợp báo cáo CĐS.</p>
              </div>

              <button 
                @click="addContactPerson" 
                class="ui-single-line px-3 py-1.5 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 text-xs font-bold rounded-xl transition border border-emerald-200 flex items-center gap-1 cursor-pointer shadow-2xs"
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
                class="p-3.5 bg-slate-50/70 rounded-xl border border-slate-200 space-y-3 relative hover:border-emerald-300 transition"
              >
                <div class="flex items-center justify-between">
                  <span class="text-xs font-bold text-slate-800 flex items-center gap-1.5">
                    <span class="w-5 h-5 rounded-full bg-emerald-600 text-white text-xs flex items-center justify-center font-bold">{{ idx + 1 }}</span>
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
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Chức vụ</label>
                    <input 
                      v-model="cp.position" 
                      type="text" 
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Đơn vị / Phòng ban</label>
                    <input 
                      v-model="cp.department" 
                      type="text" 
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:outline-none transition"
                    />
                  </div>

                  <div>
                    <label class="block font-bold text-slate-700 mb-1">Số điện thoại</label>
                    <input 
                      v-model="cp.phone" 
                      type="text" 
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:outline-none transition font-mono"
                    />
                  </div>

                  <div class="sm:col-span-2">
                    <label class="block font-bold text-slate-700 mb-1">Email</label>
                    <input 
                      v-model="cp.email" 
                      type="email" 
                      class="ui-single-line w-full text-xs font-medium px-3 py-2 bg-white border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:outline-none transition"
                    />
                  </div>
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
import { fetchWithAuth, authState } from '../services/auth';
import { MODULE, ACTION } from '../shared/permissions';
import { ref, computed, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { loadExcelWriter } from '../utils/excelRuntime';
import { getApiUrl } from '../config/api';
import { confirmModal } from '../services/confirm';
import OverlayPanel from '../components/OverlayPanel.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';

// Navigation tab: 'ministry' (Bộ ngành), 'province' (Địa phương), 'contacts' (Đầu mối liên hệ), 'all' (Tất cả)
const currentTab = ref('ministry');

const agencies = ref([]);
const allAgenciesRaw = ref([]);
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
const responseDocFilter = ref('ALL');
const updated1266Filter = ref('ALL');

// Detail Popups
const viewingContactsAgency = ref(null);
const viewingFiveYearPlanAgency = ref(null);

// Edit Modal
const showModal = ref(false);
const editingAgency = ref(null);
const activeEditTab = ref('fiveYear');
const contactPersonsForm = ref([]);
const fiveYearPlanForm = ref({
  isIssued: false,
  hasReviewedPlan: false,
  responseDocument: '',
  documentNumber: '',
  isUpdated1266: false,
  issueDate: '',
  period: '2026-2030',
  notes: '',
  files: []
});
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
  // Admin và Văn thư: xem và cập nhật kế hoạch CĐS của mọi cơ quan (bảng quyền phần Tiện ích).
  if (authState.can(MODULE.utilities, ACTION.edit)) return true;
  if (currentUserAgencyId.value) {
    const userAgency = currentUserAgency.value || authState.user.value?.agency;
    if (userAgency && isBKHCN(userAgency)) return true;

    const code = (currentUserAgencyCode.value || '').toLowerCase();
    if (code === 'bkhcn') return true;

    const userAgName = (authState.user.value?.agencyName || authState.user.value?.agency?.name || '').toLowerCase();
    if (userAgName.includes('khoa học và công nghệ') || userAgName.includes('khoa học & công nghệ') || userAgName.includes('khoa học công nghệ')) return true;

    return false;
  }
  const code = (currentUserAgencyCode.value || '').toLowerCase();
  return code === 'bkhcn';
});

function isMinistryAgency(agency) {
  if (!agency) return false;
  const t = typeof agency === 'object' ? agency.type : agency;
  return t === 1 || t === '1' || t === 'Ministry' || (typeof t === 'string' && t.toLowerCase() === 'ministry');
}

function isProvinceAgency(agency) {
  if (!agency) return false;
  const t = typeof agency === 'object' ? agency.type : agency;
  return t === 2 || t === '2' || t === 'Province' || (typeof t === 'string' && t.toLowerCase() === 'province');
}

function isInternalAgency(agency) {
  if (!agency) return false;
  const t = typeof agency === 'object' ? agency.type : agency;
  return t === 3 || t === '3' || t === 'Internal' || (typeof t === 'string' && t.toLowerCase() === 'internal');
}

function isAllowedAgencyForUser(agency) {
  if (isUserLevel1.value) return true;
  if (!agency) return false;
  if (agencies.value && agencies.value.some(x => x.id === agency.id)) return true;

  const userAgId = currentUserAgencyId.value;
  const userAgCode = currentUserAgencyCode.value;
  const userAgency = currentUserAgency.value || authState.user.value?.agency;

  if (userAgId && (agency.id === userAgId || agency.parentId === userAgId)) return true;
  if (userAgency && userAgency.parentId && agency.id === userAgency.parentId) return true;
  if (userAgCode && agency.code && agency.code.toLowerCase() === userAgCode.toLowerCase()) return true;
  if (isBKHCN(agency)) return true;

  return false;
}

// Helpers for Table Display
function hasContacts(item) {
  return item.hasContacts ?? (item.contactPersons && item.contactPersons.length > 0);
}

function hasFiveYearPlan(item) {
  if (item.hasFiveYearPlan !== undefined) return item.hasFiveYearPlan;
  if (!item.fiveYearPlan) return false;
  return Boolean(
    item.fiveYearPlan.hasReviewedPlan || 
    item.fiveYearPlan.isIssued || 
    item.fiveYearPlan.documentNumber ||
    (item.fiveYearPlan.files && item.fiveYearPlan.files.length > 0)
  );
}

function getFiveYearPlanFiles(agency) {
  if (!agency) return [];
  if (agency.fiveYearPlan?.files && agency.fiveYearPlan.files.length > 0) {
    return agency.fiveYearPlan.files;
  }
  return agency.planFiles || [];
}

function formatContactsSingleLine(persons) {
  if (!persons || persons.length === 0) return '';
  return persons.map((cp, idx) => {
    const parts = [cp.name];
    if (cp.position) parts.push(cp.position);
    if (cp.phone) parts.push(cp.phone);
    if (cp.email) parts.push(cp.email);
    return `${idx + 1}. ${parts.join(' - ')}`;
  }).join('; ');
}

function formatContactsTooltip(persons) {
  if (!persons || persons.length === 0) return '';
  return persons.map((cp, idx) => {
    const parts = [cp.name];
    if (cp.position) parts.push(cp.position);
    if (cp.department) parts.push(cp.department);
    if (cp.phone) parts.push('SĐT: ' + cp.phone);
    if (cp.email) parts.push('Email: ' + cp.email);
    return `${idx + 1}. ${parts.join(' - ')}`;
  }).join('\n');
}

// Detail Popups
function openViewContacts(item) {
  viewingContactsAgency.value = item;
}

function openViewFiveYearPlan(item) {
  viewingFiveYearPlanAgency.value = item;
}

function changeTab(tabName) {
  currentTab.value = tabName;
  pageNumber.value = 1;
  selectedAgencyId.value = 'ALL';
  fetchAgencies();
}

onMounted(() => {
  fetchAllAgenciesOptions();
  fetchAgencies();
});

async function fetchAllAgenciesOptions() {
  try {
    const params = new URLSearchParams();
    params.append('excludeSpecial', 'true');
    const res = await fetchWithAuth(getApiUrl(`/api/agencies?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      const list = Array.isArray(data) ? data : (data.items || []);
      allAgenciesRaw.value = list;
      allAgenciesOptions.value = list;
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

    // Type filter based on Tab
    if (currentTab.value === 'ministry') {
      params.append('typeFilter', 'Ministry');
    } else if (currentTab.value === 'province') {
      params.append('typeFilter', 'Province');
    } else if (currentTab.value === 'internal') {
      params.append('typeFilter', 'Internal');
    }

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

    if (responseDocFilter.value && responseDocFilter.value !== 'ALL') {
      params.append('responseDocFilter', responseDocFilter.value);
    }

    if (updated1266Filter.value && updated1266Filter.value !== 'ALL') {
      params.append('updated1266Filter', updated1266Filter.value);
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

      agencies.value = rawItems;
      totalCount.value = total;
      totalPages.value = totalP;
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
  const source = (allAgenciesOptions.value && allAgenciesOptions.value.length > 0)
    ? allAgenciesOptions.value
    : (agencies.value || []);

  let list = source.filter(a => 
    a &&
    a.code !== 'ALL_AGENCIES' && 
    a.id !== 'ALL_AGENCIES' && 
    a.name !== 'Các bộ, ngành, địa phương' &&
    isAllowedAgencyForUser(a)
  );

  if (currentTab.value === 'ministry') {
    list = list.filter(a => isMinistryAgency(a));
  } else if (currentTab.value === 'province') {
    list = list.filter(a => isProvinceAgency(a));
  } else if (currentTab.value === 'internal') {
    list = list.filter(a => isInternalAgency(a));
  }

  const seenIds = new Set();
  list.forEach(a => {
    if (a.id && !seenIds.has(a.id)) {
      seenIds.add(a.id);
      opts.push({ value: a.id, label: a.name });
    }
  });
  return opts;
});

const contactFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'PROVIDED', label: 'Đã có cán bộ đầu mối' },
  { value: 'NOT_PROVIDED', label: 'Chưa có cán bộ đầu mối' }
];

const fiveYearPlanFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'SENT', label: 'Đã có Kế hoạch 05 năm' },
  { value: 'NOT_SENT', label: 'Chưa có Kế hoạch 05 năm' }
];

const responseDocFilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'PROVIDED', label: 'Đã có Văn bản phúc đáp' },
  { value: 'NOT_PROVIDED', label: 'Chưa có Văn bản phúc đáp' }
];

const updated1266FilterOptions = [
  { value: 'ALL', label: 'Tất cả trạng thái' },
  { value: 'UPDATED', label: 'Đã cập nhật theo QĐ 1266' },
  { value: 'NOT_UPDATED', label: 'Chưa cập nhật theo QĐ 1266' }
];

const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedAgencyId.value && selectedAgencyId.value !== 'ALL') count++;
  if (contactFilter.value && contactFilter.value !== 'ALL') count++;
  if (fiveYearPlanFilter.value && fiveYearPlanFilter.value !== 'ALL') count++;
  if (responseDocFilter.value && responseDocFilter.value !== 'ALL') count++;
  if (updated1266Filter.value && updated1266Filter.value !== 'ALL') count++;
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
  responseDocFilter.value = 'ALL';
  updated1266Filter.value = 'ALL';
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
function openEditModal(agency, initialTab = 'fiveYear') {
  editingAgency.value = agency;
  activeEditTab.value = isInternalAgency(agency) ? 'contacts' : initialTab;
  contactPersonsForm.value = JSON.parse(JSON.stringify(agency.contactPersons || []));

  // 5-Year Plan
  if (agency.fiveYearPlan) {
    fiveYearPlanForm.value = JSON.parse(JSON.stringify(agency.fiveYearPlan));
    if (agency.fiveYearPlan.hasReviewedPlan === undefined) {
      fiveYearPlanForm.value.hasReviewedPlan = Boolean(agency.fiveYearPlan.isIssued || agency.fiveYearPlan.documentNumber);
    }
    if (!fiveYearPlanForm.value.files || fiveYearPlanForm.value.files.length === 0) {
      fiveYearPlanForm.value.files = JSON.parse(JSON.stringify(agency.planFiles || []));
    }
  } else {
    fiveYearPlanForm.value = {
      isIssued: false,
      hasReviewedPlan: false,
      responseDocument: '',
      documentNumber: '',
      isUpdated1266: false,
      issueDate: '',
      period: '2026-2030',
      notes: '',
      files: JSON.parse(JSON.stringify(agency.planFiles || []))
    };
  }
  planFilesForm.value = JSON.parse(JSON.stringify(fiveYearPlanForm.value.files || []));

  showModal.value = true;
}

function onHasPlanChange() {
  fiveYearPlanForm.value.isIssued = fiveYearPlanForm.value.hasReviewedPlan;
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
      fiveYearPlanForm.value.hasReviewedPlan = true;
      fiveYearPlanForm.value.isIssued = true;
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

async function saveAgencyDetails() {
  if (!editingAgency.value) return;

  saving.value = true;
  try {
    planFilesForm.value = fiveYearPlanForm.value.files || [];
    fiveYearPlanForm.value.isIssued = fiveYearPlanForm.value.hasReviewedPlan;

    const payload = {
      contactPersons: contactPersonsForm.value.filter(cp => cp.name && cp.name.trim() !== ''),
      planFiles: planFilesForm.value,
      planFileIds: planFilesForm.value.map(f => f.id),
      fiveYearPlan: fiveYearPlanForm.value
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
      fetchAllAgenciesOptions();
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

// ====================================================================
// EXPORT EXCEL 3 TRANG (MATCHING GOOGLE SHEET 1:1)
// ====================================================================
async function exportAgencyPlansToExcel() {
  try {
    const XLSX = await loadExcelWriter();

    // Fetch all ministries, provinces, and internal units
    const mRes = await fetchWithAuth(getApiUrl('/api/agencies?typeFilter=Ministry&excludeSpecial=true'));
    const pRes = await fetchWithAuth(getApiUrl('/api/agencies?typeFilter=Province&excludeSpecial=true'));
    const iRes = await fetchWithAuth(getApiUrl('/api/agencies?typeFilter=Internal&excludeSpecial=true'));
    
    if (!mRes.ok || !pRes.ok) throw new Error('Không thể lấy dữ liệu từ máy chủ');
    
    const mData = await mRes.json();
    const pData = await pRes.json();
    const iData = iRes.ok ? await iRes.json() : [];

    const ministries = Array.isArray(mData) ? mData : (mData.items || []);
    const provinces = Array.isArray(pData) ? pData : (pData.items || []);
    const internals = Array.isArray(iData) ? iData : (iData.items || []);

    const wb = XLSX.utils.book_new();

    // Common styling
    const headerStyle = {
      font: { name: 'Times New Roman', sz: 11, bold: true, color: { rgb: '000000' } },
      fill: { fgColor: { rgb: 'E2EFDA' } },
      alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
      border: {
        top: { style: 'thin', color: { rgb: '000000' } },
        bottom: { style: 'thin', color: { rgb: '000000' } },
        left: { style: 'thin', color: { rgb: '000000' } },
        right: { style: 'thin', color: { rgb: '000000' } }
      }
    };

    const cellLeft = {
      font: { name: 'Times New Roman', sz: 11, color: { rgb: '000000' } },
      alignment: { horizontal: 'left', vertical: 'top', wrapText: true },
      border: {
        top: { style: 'thin', color: { rgb: '000000' } },
        bottom: { style: 'thin', color: { rgb: '000000' } },
        left: { style: 'thin', color: { rgb: '000000' } },
        right: { style: 'thin', color: { rgb: '000000' } }
      }
    };

    const cellCenter = {
      font: { name: 'Times New Roman', sz: 11, color: { rgb: '000000' } },
      alignment: { horizontal: 'center', vertical: 'top', wrapText: true },
      border: {
        top: { style: 'thin', color: { rgb: '000000' } },
        bottom: { style: 'thin', color: { rgb: '000000' } },
        left: { style: 'thin', color: { rgb: '000000' } },
        right: { style: 'thin', color: { rgb: '000000' } }
      }
    };

    function applyTableStyles(ws, centerCols = []) {
      const range = XLSX.utils.decode_range(ws['!ref']);
      for (let r = range.s.r; r <= range.e.r; r++) {
        for (let c = range.s.c; c <= range.e.c; c++) {
          const cellRef = XLSX.utils.encode_cell({ r, c });
          if (!ws[cellRef]) continue;
          if (r === 0) {
            ws[cellRef].s = headerStyle;
          } else {
            ws[cellRef].s = centerCols.includes(c) ? cellCenter : cellLeft;
          }
        }
      }
    }

    // ----------------------------------------------------
    // SHEET 1: CÁC BỘ, NGÀNH
    // ----------------------------------------------------
    const mRows = ministries.map((a, idx) => ({
      'STT': idx + 1,
      'Tên Cơ quan': a.name || '',
      'Văn bản phúc đáp VB 5799/BKHCN-CĐSQG': a.fiveYearPlan?.responseDocument || '',
      'Đã rà soát, có KH 05 năm (đánh dấu x)': hasFiveYearPlan(a) ? 'x' : '',
      'Tên số hiệu QĐ, KH của Cơ quan': a.fiveYearPlan?.documentNumber || '',
      'Đã cập nhật 1266': a.fiveYearPlan?.isUpdated1266 ? 'x' : '',
      'Ghi chú': a.fiveYearPlan?.notes || ''
    }));

    const ws1 = XLSX.utils.json_to_sheet(mRows);
    ws1['!cols'] = [
      { wch: 6 },  // STT
      { wch: 32 }, // Tên Cơ quan
      { wch: 36 }, // Phúc đáp
      { wch: 22 }, // Có KH 5 năm
      { wch: 45 }, // Tên số hiệu QĐ
      { wch: 18 }, // Đã cập nhật 1266
      { wch: 35 }  // Ghi chú
    ];
    applyTableStyles(ws1, [0, 3, 5]);
    XLSX.utils.book_append_sheet(wb, ws1, 'Cac_Bo_Nganh');

    // ----------------------------------------------------
    // SHEET 2: ĐỊA PHƯƠNG (34 TỈNH/THÀNH)
    // ----------------------------------------------------
    const pRows = provinces.map((a, idx) => ({
      'STT': idx + 1,
      'Tên Cơ quan': a.name || '',
      'Văn bản phúc đáp VB 5799/BKHCN-CĐSQG và VB 8841/BKHCN-CĐSQG (lần 2)': a.fiveYearPlan?.responseDocument || '',
      'Đã rà soát, có KH 05 năm (đánh dấu x)': hasFiveYearPlan(a) ? 'x' : '',
      'Tên số hiệu QĐ, KH của Cơ quan': a.fiveYearPlan?.documentNumber || '',
      'Đã cập nhật 1266': a.fiveYearPlan?.isUpdated1266 ? 'x' : '',
      'Ghi chú': a.fiveYearPlan?.notes || ''
    }));

    const ws2 = XLSX.utils.json_to_sheet(pRows);
    ws2['!cols'] = [
      { wch: 6 },  // STT
      { wch: 28 }, // Tên Cơ quan
      { wch: 40 }, // Phúc đáp
      { wch: 22 }, // Có KH 5 năm
      { wch: 50 }, // Tên số hiệu QĐ
      { wch: 18 }, // Đã cập nhật 1266
      { wch: 45 }  // Ghi chú
    ];
    applyTableStyles(ws2, [0, 3, 5]);
    XLSX.utils.book_append_sheet(wb, ws2, 'Dia_Phuong');

    // ----------------------------------------------------
    // SHEET 3: DANH SÁCH ĐẦU MỐI TRỰC THUỘC BỘ
    // ----------------------------------------------------
    const iRows = internals.map((a, idx) => {
      let contactStr = '';
      if (a.contactPersons && a.contactPersons.length > 0) {
        contactStr = a.contactPersons.map((cp, cIdx) => {
          const parts = [cp.name || ''];
          if (cp.position) parts.push(cp.position);
          if (cp.department) parts.push(cp.department);
          if (cp.phone) parts.push(cp.phone);
          if (cp.email) parts.push(cp.email);
          return `${cIdx + 1}. ${parts.filter(Boolean).join(' - ')}`;
        }).join(';\n');
      }
      return {
        'STT': idx + 1,
        'Tên Đơn vị trực thuộc': a.name || '',
        'Đã gửi đầu mối? (đánh dấu x)': hasContacts(a) ? 'x' : '',
        'Thông tin đầu mối': contactStr
      };
    });

    const ws3 = XLSX.utils.json_to_sheet(iRows);
    ws3['!cols'] = [
      { wch: 6 },  // STT
      { wch: 38 }, // Tên Đơn vị trực thuộc
      { wch: 26 }, // Đã gửi đầu mối
      { wch: 75 }  // Thông tin đầu mối
    ];
    applyTableStyles(ws3, [0, 2]);
    XLSX.utils.book_append_sheet(wb, ws3, 'Dau_Moi_Truc_Thuoc_Bo');

    const dateStr = new Date().toISOString().slice(0, 10);
    XLSX.writeFile(wb, `Thong_Tin_Chuyen_Doi_So_3_Trang_${dateStr}.xlsx`);

    toast.success('Đã xuất file Excel thành công!');
  } catch (err) {
    console.error('Export excel error:', err);
    toast.error('Lỗi khi xuất file Excel: ' + (err.message || err));
  }
}
</script>
