<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Top Header Bar -->
    <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3 w-full">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-sm sm:text-base font-extrabold text-slate-800">
            {{ filterItemType === 'Goal' ? '🎯 Theo Dõi Mục Tiêu Quyết Định 1266/QĐ-TTg' : '📋 Theo Dõi Nhiệm Vụ Quyết Định 1266/QĐ-TTg' }}
          </h2>
        </div>

        <div class="flex items-center gap-2">
          <button 
            @click="exportDocumentItemsToExcel"
            class="px-3.5 py-2 text-slate-700 hover:text-slate-900 font-bold text-xs rounded-xl bg-slate-100 hover:bg-slate-200 border border-slate-200/80 transition shadow-2xs flex items-center gap-1.5 cursor-pointer"
            title="Xuất danh sách hiện tại ra file Excel"
          >
            <svg class="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
            <span>Xuất Excel</span>
          </button>

          <button 
            @click="openCreateModal(filterItemType || 'Task')"
            class="px-3.5 py-2 text-white font-bold text-xs rounded-xl bg-blue-600 hover:bg-blue-700 transition shadow-sm flex items-center gap-1.5 cursor-pointer"
          >
            + Thêm {{ filterItemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} Mới
          </button>
        </div>
      </div>
    </div>

    <!-- MAIN CONTENT CARD BLOCK -->
    <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3.5 w-full">

      <!-- DYNAMIC PLANNING GRID SUB-TAB -->
      <div v-show="activeSubTab === 'grid'" class="w-full">
        <DynamicPlanningGrid ref="planningGridRef" documentId="12660000-0000-0000-0000-000000001266" :filterItemType="filterItemType || 'Task'" />
      </div>

      <!-- LIST TABLE SUB-TAB WITH STICKY HEADERS & EVIDENCE FILES AUDIT LOG -->
      <div v-show="activeSubTab === 'list'" class="w-full border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col">
        
        <!-- ADVANCED SEARCH & SCOPE FILTER BAR -->
        <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-slate-50/60 p-3 w-full">
          <div class="flex items-center gap-2 flex-1 max-w-xl">
            <!-- Quick Search Input -->
            <div class="relative flex-1">
              <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
              <input 
                :value="filterDraft.searchQuery" 
                @input="filterDraft.searchQuery = $event.target.value"
                :placeholder="filterItemType === 'Goal' ? 'Tìm theo mã, tên mục tiêu...' : 'Tìm theo mã, tên nhiệm vụ...'" 
                class="w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none h-[34px]"
              />
            </div>

            <!-- OverlayPanel Advanced Filter Popover -->
            <OverlayPanel 
              title="Bộ Lọc Tìm Kiếm Nâng Cao"
              buttonText="Lọc Nâng Cao"
              :activeCount="activeFilterCount"
              widthClass="w-[340px] sm:w-[500px]"
              @apply="execFilterSearch"
              @reset="resetFilterSearch"
            >
              <div class="space-y-3">
                <div>
                  <SearchableSelect 
                    v-model="filterDraft.selectedAgencyIds" 
                    :options="agencyOptions" 
                    :isMulti="true" 
                    label="Cơ Quan Chủ Trì" 
                    placeholder="Tất cả cơ quan"
                  />
                </div>

                <div>
                  <SearchableSelect 
                    v-model="filterDraft.selectedScopes" 
                    :options="scopeOptions" 
                    :isMulti="true" 
                    label="Phạm Vi (Chung - Riêng)" 
                    placeholder="Tất cả phạm vi"
                  />
                </div>

                <div v-if="filterItemType === 'Goal'">
                  <SearchableSelect 
                    v-model="filterDraft.selectedSections" 
                    :options="sectionFilterOptions" 
                    :isMulti="true" 
                    label="Mục (Phụ lục)" 
                    placeholder="Tất cả mục"
                  />
                </div>

                <div>
                  <SearchableSelect 
                    v-model="filterDraft.selectedGroups" 
                    :options="groupFilterOptions" 
                    :isMulti="true" 
                    label="Nhóm Trọng Tâm" 
                    placeholder="Tất cả nhóm"
                  />
                </div>

                <div>
                  <div class="flex items-center justify-between mb-1">
                    <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block">Giai Đoạn (Từ năm ➔ Đến năm)</label>
                    <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-extrabold text-blue-700 select-none">
                      <input type="checkbox" v-model="filterDraft.onlyOngoing" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3.5 h-3.5">
                      <span>Thường xuyên</span>
                    </label>
                  </div>
                  <div class="flex items-center gap-2">
                    <SearchableSelect 
                      v-model="filterDraft.fromYear" 
                      :options="yearOptions" 
                      :isMulti="false" 
                      placeholder="Từ năm" 
                      class="w-full"
                    />
                    <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                    <SearchableSelect 
                      v-model="filterDraft.toYear" 
                      :options="yearOptions" 
                      :isMulti="false" 
                      placeholder="Đến năm" 
                      class="w-full"
                    />
                  </div>
                </div>

                <div>
                  <SearchableSelect 
                    v-model="filterDraft.selectedStatuses" 
                    :options="statusOptions" 
                    :isMulti="true" 
                    label="Trạng Thái Hạng Mục" 
                    placeholder="Tất cả trạng thái"
                  />
                </div>
              </div>
            </OverlayPanel>
          </div>
        </div>

        <!-- MAIN DATA TABLE WITH STICKY HEADER & FROZEN FIRST 3 COLUMNS -->
        <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu danh sách từ máy chủ..." />
        <div v-else class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
          <table class="w-full min-w-[1050px] text-left text-sm text-slate-700 border-collapse">
            <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-30 shadow-xs">
              <tr>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-30">Mã</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-30 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                  {{ filterItemType === 'Goal' ? 'Tên Mục Tiêu' : (filterItemType === 'Task' ? 'Tên Nhiệm Vụ' : 'Tên Mục Tiêu / Nhiệm Vụ') }}
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[150px] w-[150px] max-w-[150px]">Cơ Quan Chủ Trì</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[160px] w-[160px] max-w-[160px]">Cơ Quan Phối Hợp</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[150px]">Thời Gian thực hiện</th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[120px]">Tiến Độ</th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[180px]">Trạng Thái</th>
                <th class="px-3 py-2.5 text-center bg-slate-100 whitespace-nowrap min-w-[100px]">Thao Tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <template v-for="item in paginatedPrimaryList" :key="item.taskId">
                <!-- Parent Row -->
                <tr @click="openItemDetailModal(item)" class="group hover:bg-blue-100/90 cursor-pointer">
                  <td class="px-3 py-2.5 font-extrabold text-blue-700 border-r border-slate-200 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-white group-hover:bg-blue-100">
                    {{ item.code }}
                  </td>
                  <td class="px-3 py-2.5 font-semibold text-slate-900 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-20 bg-white group-hover:bg-blue-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                    <VTooltip 
                      theme="custom-dark"
                      placement="top"
                      :delay="{ show: 1500, hide: 0 }"
                    >
                      <div class="line-clamp-5 font-semibold text-slate-900 text-xs leading-relaxed cursor-help">
                        {{ item.title }}
                      </div>

                      <template #popper>
                        <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                          <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">
                            {{ item.itemType === 'Goal' ? '🎯 Chi Tiết Mục Tiêu' : '📋 Chi Tiết Nhiệm Vụ' }}
                          </span>
                          {{ item.title }}
                        </div>
                      </template>
                    </VTooltip>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-bold text-slate-800 text-xs leading-relaxed min-w-[150px] w-[150px] max-w-[150px]">
                    {{ item.leadAgencyName }}
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-medium text-slate-700 text-xs leading-relaxed min-w-[160px]">
                    <template v-if="item.coordinatingAgencyNames && item.coordinatingAgencyNames.length > 0">
                      {{ item.coordinatingAgencyNames.join(', ') }}
                    </template>
                    <template v-else-if="item.coordinatingAgencyCodes && item.coordinatingAgencyCodes.length > 0">
                      {{ item.coordinatingAgencyCodes.join(', ') }}
                    </template>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-xs font-semibold text-slate-600 whitespace-nowrap">
                    <span v-if="item.isOngoing" class="px-2 py-0.5 rounded-full font-extrabold text-[11px] bg-blue-100 text-blue-800">
                      Thường xuyên
                    </span>
                    <span v-else>
                      {{ formatDateRange(item.startDate, item.dueDate) }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-center text-xs whitespace-nowrap">
                    <span :class="['font-extrabold px-2 py-0.5 rounded-lg text-xs', item.latestProgressValue !== null && item.latestProgressValue !== undefined ? 'bg-blue-50 text-blue-900 border border-blue-200' : (item.latestProgressStatus ? 'bg-slate-100 text-slate-800' : 'text-slate-400 italic')]">
                      {{ formatProgressDisplay(item) }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-center whitespace-nowrap">
                    <span :class="['px-2.5 py-0.5 rounded-full text-xs font-bold shadow-2xs inline-block whitespace-nowrap', getStatusBadgeClass(item.calculatedStatus)]">
                      {{ getStatusLabel(item.calculatedStatus) }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 text-center whitespace-nowrap" @click.stop>
                    <div class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap">
                      <button 
                        @click.stop="openCreateSubTaskModal(item)" 
                        class="p-1.5 bg-blue-50 text-blue-700 hover:bg-blue-100 rounded-lg border border-blue-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                        title="Thêm Nhiệm Vụ Con"
                      >
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
                      </button>

                      <button 
                        v-if="!authState.isAdmin.value"
                        @click.stop="openProgressModal(item)" 
                        class="p-1.5 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-lg border border-emerald-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                        title="Cập Nhật Tiến Độ & Minh Chứng"
                      >
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                      </button>

                      <!-- 3-Dots Dropdown Menu -->
                      <VDropdown placement="bottom-end" :distance="6">
                        <button 
                          type="button"
                          @click.stop
                          class="p-1.5 bg-slate-50 text-slate-600 hover:bg-slate-200 hover:text-slate-900 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer transition"
                          title="Thao tác khác"
                        >
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
                          </svg>
                        </button>

                        <template #popper="{ hide }">
                          <div class="py-1.5 w-44 bg-white rounded-xl shadow-xl border border-slate-200 text-xs font-semibold space-y-0.5" @click.stop>
                            <button 
                              @click="openEditModal(item); hide()" 
                              :disabled="hasProgress(item)"
                              :class="[
                                'w-full text-left px-3 py-2 flex items-center gap-2 transition',
                                hasProgress(item) ? 'text-slate-300 cursor-not-allowed bg-slate-50' : 'text-slate-700 hover:bg-blue-50 hover:text-blue-700 cursor-pointer'
                              ]"
                              :title="hasProgress(item) ? 'Chỉ được phép chỉnh sửa khi ở trạng thái Chưa bắt đầu' : 'Chỉnh Sửa'"
                            >
                              <svg class="w-4 h-4 text-blue-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/></svg>
                              <span>Chỉnh sửa</span>
                            </button>

                            <button 
                              @click="openNotificationModal(item); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-amber-50 hover:text-amber-700 flex items-center gap-2 transition cursor-pointer"
                              title="Gửi thông báo đến đơn vị chủ trì & phối hợp"
                            >
                              <svg class="w-4 h-4 text-amber-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/></svg>
                              <span>Gửi thông báo</span>
                            </button>

                            <div class="border-t border-slate-100 my-1"></div>

                            <button 
                              @click="handleDeleteItem(item); hide()" 
                              :disabled="hasProgress(item)"
                              :class="[
                                'w-full text-left px-3 py-2 flex items-center gap-2 transition',
                                hasProgress(item) ? 'text-slate-300 cursor-not-allowed bg-slate-50' : 'text-rose-600 hover:bg-rose-50 cursor-pointer'
                              ]"
                              :title="hasProgress(item) ? 'Chỉ được phép xóa khi ở trạng thái Chưa bắt đầu' : 'Xóa'"
                            >
                              <svg class="w-4 h-4 text-rose-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                              <span>Xóa</span>
                            </button>
                          </div>
                        </template>
                      </VDropdown>
                    </div>
                  </td>
                </tr>

                <!-- Child Sub-task Rows -->
                <tr v-for="sub in item.subItems" :key="sub.taskId" @click="openItemDetailModal(sub)" class="group bg-slate-50 hover:bg-blue-100/90 cursor-pointer text-xs">
                  <td class="px-3 py-2 font-bold text-slate-600 border-r border-slate-200 pl-3 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-slate-50 group-hover:bg-blue-100">
                    └─ {{ sub.code }}
                  </td>
                  <td class="px-3 py-2 font-medium text-slate-800 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-20 bg-slate-50 group-hover:bg-blue-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                    <VTooltip 
                      theme="custom-dark"
                      placement="top"
                      :delay="{ show: 1500, hide: 0 }"
                    >
                      <div class="line-clamp-5 font-medium text-slate-800 text-xs leading-relaxed cursor-help">
                        {{ sub.title }}
                      </div>

                      <template #popper>
                        <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                          <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">
                            📌 Chi Tiết Sub-Task
                          </span>
                          {{ sub.title }}
                        </div>
                      </template>
                    </VTooltip>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-semibold text-slate-700 min-w-[150px] w-[150px] max-w-[150px]">
                    {{ sub.leadAgencyName }}
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-medium text-slate-600 text-xs leading-relaxed min-w-[160px]">
                    <template v-if="sub.coordinatingAgencyNames && sub.coordinatingAgencyNames.length > 0">
                      {{ sub.coordinatingAgencyNames.join(', ') }}
                    </template>
                    <template v-else-if="sub.coordinatingAgencyCodes && sub.coordinatingAgencyCodes.length > 0">
                      {{ sub.coordinatingAgencyCodes.join(', ') }}
                    </template>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-semibold text-slate-600 whitespace-nowrap">
                    <span v-if="sub.isOngoing" class="px-2 py-0.5 rounded-full font-extrabold text-[11px] bg-blue-100 text-blue-800">
                      Thường xuyên
                    </span>
                    <span v-else>
                      {{ formatDateRange(sub.startDate, sub.dueDate) }}
                    </span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 text-center font-semibold whitespace-nowrap">
                    <span :class="['font-bold text-xs px-2 py-0.5 rounded-lg', sub.latestProgressValue !== null && sub.latestProgressValue !== undefined ? 'bg-blue-50 text-blue-900 border border-blue-200' : 'text-slate-500']">
                      {{ formatProgressDisplay(sub) }}
                    </span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 text-center whitespace-nowrap">
                    <span :class="['px-2 py-0.5 rounded-full text-[11px] font-bold inline-block whitespace-nowrap', getStatusBadgeClass(sub.calculatedStatus)]">
                      {{ getStatusLabel(sub.calculatedStatus) }}
                    </span>
                  </td>
                  <td class="px-3 py-2 text-center whitespace-nowrap" @click.stop>
                    <div class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap">
                      <button 
                        v-if="!authState.isAdmin.value"
                        @click.stop="openProgressModal(sub)" 
                        class="p-1.5 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-lg border border-emerald-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                        title="Cập Nhật Tiến Độ & Minh Chứng"
                      >
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                      </button>

                      <!-- 3-Dots Dropdown Menu -->
                      <VDropdown placement="bottom-end" :distance="6">
                        <button 
                          type="button"
                          @click.stop
                          class="p-1.5 bg-slate-50 text-slate-600 hover:bg-slate-200 hover:text-slate-900 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer transition"
                          title="Thao tác khác"
                        >
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
                          </svg>
                        </button>

                        <template #popper="{ hide }">
                          <div class="py-1.5 w-44 bg-white rounded-xl shadow-xl border border-slate-200 text-xs font-semibold space-y-0.5" @click.stop>
                            <button 
                              @click="openEditModal(sub); hide()" 
                              :disabled="hasProgress(sub)"
                              :class="[
                                'w-full text-left px-3 py-2 flex items-center gap-2 transition',
                                hasProgress(sub) ? 'text-slate-300 cursor-not-allowed bg-slate-50' : 'text-slate-700 hover:bg-blue-50 hover:text-blue-700 cursor-pointer'
                              ]"
                              :title="hasProgress(sub) ? 'Chỉ được phép chỉnh sửa khi ở trạng thái Chưa bắt đầu' : 'Chỉnh Sửa Sub-Task'"
                            >
                              <svg class="w-4 h-4 text-blue-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/></svg>
                              <span>Chỉnh sửa</span>
                            </button>

                            <button 
                              @click="openNotificationModal(sub); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-amber-50 hover:text-amber-700 flex items-center gap-2 transition cursor-pointer"
                              title="Gửi thông báo đến đơn vị chủ trì & phối hợp"
                            >
                              <svg class="w-4 h-4 text-amber-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/></svg>
                              <span>Gửi thông báo</span>
                            </button>

                            <div class="border-t border-slate-100 my-1"></div>

                            <button 
                              @click="handleDeleteItem(sub); hide()" 
                              :disabled="hasProgress(sub)"
                              :class="[
                                'w-full text-left px-3 py-2 flex items-center gap-2 transition',
                                hasProgress(sub) ? 'text-slate-300 cursor-not-allowed bg-slate-50' : 'text-rose-600 hover:bg-rose-50 cursor-pointer'
                              ]"
                              :title="hasProgress(sub) ? 'Chỉ được phép xóa khi ở trạng thái Chưa bắt đầu' : 'Xóa Sub-Task'"
                            >
                              <svg class="w-4 h-4 text-rose-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                              <span>Xóa</span>
                            </button>
                          </div>
                        </template>
                      </VDropdown>
                    </div>
                  </td>
                </tr>
              </template>

              <tr v-if="paginatedPrimaryList.length === 0">
                <td colspan="8" class="p-8 text-center text-slate-400 font-semibold italic">
                  Không tìm thấy dữ liệu phù hợp.
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Attached Pagination Controls Bar -->
        <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
          <div class="flex items-center gap-3 whitespace-nowrap flex-wrap sm:flex-nowrap">
            <span class="whitespace-nowrap">Hiển thị <span class="font-extrabold text-slate-900">{{ totalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, totalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalCount }}</span> {{ filterItemType === 'Goal' ? 'mục tiêu' : 'nhiệm vụ' }}</span>
            
            <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3 whitespace-nowrap">
              <span class="whitespace-nowrap">Số bản ghi/trang:</span>
              <SearchableSelect 
                v-model="pageSize" 
                :options="pageSizeOptions" 
                :isMulti="false" 
                :clearable="false"
                @change="currentPage = 1" 
                class="w-20"
              />
            </div>
          </div>

          <div class="flex items-center gap-2 shrink-0 whitespace-nowrap">
            <button 
              @click="changePage(currentPage - 1)" 
              :disabled="currentPage <= 1"
              class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
            >
              ‹ Trang trước
            </button>
            
            <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-black">
              Trang {{ currentPage }} / {{ Math.max(1, totalPages) }}
            </span>

            <button 
              @click="changePage(currentPage + 1)" 
              :disabled="currentPage >= totalPages"
              class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
            >
              Trang sau ›
            </button>
          </div>
        </div>
      </div>

    </div>

    <!-- Create Goal/Task/Sub-task Modal with Date Validation -->
    <div v-if="isCreateModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 sm:p-7 space-y-4">
        <h3 class="text-base font-bold text-slate-800 border-b border-slate-100 pb-2">
          {{ parentTaskForSubTask ? `Thêm Nhiệm Vụ Con Cho: ${parentTaskForSubTask.code}` : `Thêm mới ${createItemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ'}` }}
        </h3>

        <form @submit.prevent="submitCreateItem" class="space-y-3">
          <div v-if="createErrorMessage" class="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs font-bold flex items-center justify-between gap-2 shadow-2xs">
            <div class="flex items-center gap-2">
              <span class="text-base">⚠️</span>
              <span>{{ createErrorMessage }}</span>
            </div>
            <button type="button" @click="createErrorMessage = ''" class="text-rose-400 hover:text-rose-600 font-bold text-sm cursor-pointer p-1">✕</button>
          </div>
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">
              {{ createItemType === 'Goal' ? 'Tên Mục Tiêu' : 'Tên Nhiệm Vụ' }} <span class="text-rose-500">*</span>
            </label>
            <textarea 
              v-model="createForm.title" 
              required 
              rows="2" 
              :placeholder="createItemType === 'Goal' ? 'Nhập tên chi tiết mục tiêu...' : 'Nhập tên chi tiết nhiệm vụ...'"
              class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl p-2.5 mt-1 focus:bg-white focus:ring-2 focus:ring-blue-500"
            ></textarea>
          </div>

          <!-- Section & Group Selects according to Phụ lục I & II -->
          <div :class="createItemType === 'Goal' ? 'grid grid-cols-2 gap-3' : 'block'">
            <div v-if="createItemType === 'Goal'">
              <SearchableSelect 
                v-model="createForm.section" 
                :options="currentFormSections" 
                :isMulti="false" 
                label="Mục" 
                placeholder="-- Chọn Mục --"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="createForm.group" 
                :options="currentFormGroups" 
                :isMulti="false" 
                label="Nhóm Trọng Tâm" 
                placeholder="-- Chọn Nhóm --"
              />
            </div>
          </div>

          <!-- Is Ongoing / Thường Xuyên Toggle -->
          <div class="flex items-center gap-2.5 p-3 bg-blue-50/70 border border-blue-200 rounded-xl">
            <input 
              type="checkbox" 
              id="docIsOngoingToggle" 
              v-model="createForm.isOngoing" 
              class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer"
            />
            <label for="docIsOngoingToggle" class="text-xs font-bold text-blue-950 cursor-pointer select-none flex items-center gap-1.5">
              <span>Thời hạn thực hiện: Thường xuyên</span>
              <span class="text-[11px] font-normal text-slate-500">(Không giới hạn năm cố định, báo cáo theo kỳ)</span>
            </label>
          </div>

          <!-- Date Range Inputs with Parent Constraint Info -->
          <div v-if="!parentTaskForSubTask && !createForm.isOngoing" class="grid grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="createForm.startYear" 
                :options="yearOptions" 
                :isMulti="false" 
                label="Năm Bắt Đầu" 
                placeholder="-- Chọn năm bắt đầu --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="createForm.dueYear" 
                :options="yearOptions" 
                :isMulti="false" 
                label="Năm Hoàn Thành" 
                placeholder="-- Chọn năm hoàn thành --"
              />
            </div>
          </div>
          <div v-else class="grid grid-cols-2 gap-3 bg-blue-50/50 p-3 rounded-xl border border-blue-200/60">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Ngày Bắt Đầu</label>
              <DatePicker v-model="createForm.startDate" placeholder="dd/mm/yyyy" />
            </div>
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Ngày Hoàn Thành</label>
              <DatePicker v-model="createForm.dueDate" placeholder="dd/mm/yyyy" />
            </div>
            <p v-if="parentTaskForSubTask" class="col-span-2 text-[10px] text-blue-800 font-semibold italic">
              ℹ Hạn nhiệm vụ cha: {{ formatDateRange(parentTaskForSubTask.startDate, parentTaskForSubTask.dueDate) }}
            </p>
          </div>

          <!-- Lead Agency & Coordinating Agencies -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="createForm.leadAgencyId" 
                :options="leadAgencyOptions" 
                :isMulti="false" 
                label="Đơn Vị Chủ Trì" 
                placeholder="-- Chọn đơn vị chủ trì --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="createForm.coordinatingAgencyIds" 
                :options="coordinatingAgencyOptions" 
                :isMulti="true" 
                label="Cơ Quan Phối Hợp" 
                placeholder="-- Chọn cơ quan phối hợp --"
              />
            </div>
          </div>

          <!-- Multi-Deliverables Section for Tasks (Phương án 1: Phụ lục II) -->
          <div v-if="createItemType === 'Task'" class="border border-slate-200 rounded-xl p-3.5 bg-slate-50/50 space-y-3">
            <div class="flex items-center justify-between">
              <label class="text-xs font-extrabold text-slate-800 uppercase flex items-center gap-1.5">
                <span>📋 Danh Mục Sản Phẩm Đầu Ra Dự Kiến (Phụ Lục II - Phương Án 1)</span>
              </label>
              <button 
                type="button" 
                @click="addDeliverable" 
                class="px-2.5 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-xs rounded-lg transition border border-blue-200 flex items-center gap-1 cursor-pointer"
              >
                + Thêm sản phẩm đầu ra
              </button>
            </div>

            <div v-if="!createForm.deliverables || createForm.deliverables.length === 0" class="text-xs text-slate-400 italic text-center py-2">
              Chưa khai báo sản phẩm đầu ra. Nhấn nút trên để thêm sản phẩm cụ thể.
            </div>

            <div v-else class="space-y-2.5 max-h-48 overflow-y-auto pr-1">
              <div 
                v-for="(del, idx) in createForm.deliverables" 
                :key="idx" 
                class="bg-white p-2.5 rounded-xl border border-slate-200 shadow-2xs space-y-2 relative"
              >
                <div class="flex items-center justify-between border-b border-slate-100 pb-1">
                  <span class="text-[11px] font-extrabold text-blue-800">Sản phẩm đầu ra #{{ idx + 1 }}</span>
                  <button 
                    type="button" 
                    @click="removeDeliverable(idx)" 
                    class="text-rose-500 hover:text-rose-700 text-xs font-bold hover:bg-rose-50 px-2 py-0.5 rounded transition cursor-pointer"
                  >
                    ✕ Xóa
                  </button>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
                  <div class="sm:col-span-2">
                    <label class="text-[10px] font-bold text-slate-600">Tên sản phẩm / Tên văn bản <span class="text-rose-500">*</span></label>
                    <input 
                      v-model="del.title" 
                      required 
                      placeholder="Ví dụ: Nghị định quy định về Dữ liệu số / Nền tảng chia sẻ..." 
                      class="w-full text-xs font-semibold bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                  <div>
                    <label class="text-[10px] font-bold text-slate-600">Hạn chót sản phẩm</label>
                    <DatePicker v-model="del.dueDate" placeholder="dd/mm/yyyy" />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
            <button type="button" @click="isCreateModalOpen = false" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl">Hủy</button>
            <button type="submit" class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm">Lưu</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Progress Update Modal -->
    <ProgressUpdateModal
      v-if="selectedTaskForProgress"
      :is-open="isProgressModalOpen"
      :task-id="selectedTaskForProgress.taskId || selectedTaskForProgress.id || ''"
      :task-code="selectedTaskForProgress.code || ''"
      :task-title="selectedTaskForProgress.title || ''"
      :evaluation-type="selectedTaskForProgress.evaluationType || 'Quantitative'"
      :unit-name="selectedTaskForProgress.unitName || ''"
      :custom-baseline="selectedTaskForProgress.customBaseline || {}"
      :deliverables="selectedTaskForProgress.deliverables || []"
      @close="isProgressModalOpen = false"
      @submitted="loadData"
    />

    <!-- Send Notification Modal -->
    <SendNotificationModal
      v-if="selectedItemForNotification"
      :is-open="isNotificationModalOpen"
      :item-id="selectedItemForNotification.taskId || selectedItemForNotification.id || ''"
      :item-code="selectedItemForNotification.code || ''"
      :item-title="selectedItemForNotification.title || ''"
      :lead-agency-id="selectedItemForNotification.leadAgencyId || ''"
      :lead-agency-name="selectedItemForNotification.leadAgencyName || ''"
      :coordinating-agency-ids="selectedItemForNotification.coordinatingAgencyIds || []"
      :coordinating-names="selectedItemForNotification.coordinatingAgencyNames || ''"
      :agencies="agencies"
      @close="isNotificationModalOpen = false"
      @sent="loadData"
    />

    <!-- Edit Goal/Task/Sub-task Modal -->
    <div v-if="isEditModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-3xl w-full p-6 sm:p-7 space-y-4 max-h-[90vh] flex flex-col">
        <h3 class="text-base font-bold text-slate-800 border-b border-slate-100 pb-2 shrink-0">
          Chỉnh Sửa {{ editingItem?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} ({{ editingItem?.code }})
        </h3>

        <form @submit.prevent="submitEditItem" class="space-y-3 overflow-y-auto pr-1 custom-scrollbar">
          <div v-if="editErrorMessage" class="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs font-bold flex items-center justify-between gap-2 shadow-2xs shrink-0">
            <div class="flex items-center gap-2">
              <span class="text-base">⚠️</span>
              <span>{{ editErrorMessage }}</span>
            </div>
            <button type="button" @click="editErrorMessage = ''" class="text-rose-400 hover:text-rose-600 font-bold text-sm cursor-pointer p-1">✕</button>
          </div>
          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">
              Tên {{ editingItem?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} <span class="text-rose-500">*</span>
            </label>
            <textarea 
              v-model="editForm.title" 
              required 
              rows="2" 
              placeholder="Nhập tên chi tiết..."
              class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl p-2.5 mt-1 focus:bg-white focus:ring-2 focus:ring-blue-500"
            ></textarea>
          </div>

          <!-- Section & Group Selects -->
          <div :class="editingItem?.itemType === 'Goal' ? 'grid grid-cols-2 gap-3' : 'block'">
            <div v-if="editingItem?.itemType === 'Goal'">
              <SearchableSelect 
                v-model="editForm.section" 
                :options="sectionFilterOptions" 
                :isMulti="false" 
                label="Mục" 
                placeholder="-- Chọn Mục --"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="editForm.group" 
                :options="groupFilterOptions" 
                :isMulti="false" 
                label="Nhóm Trọng Tâm" 
                placeholder="-- Chọn Nhóm --"
              />
            </div>
          </div>

          <!-- Is Ongoing Toggle -->
          <div class="flex items-center gap-2.5 p-3 bg-blue-50/70 border border-blue-200 rounded-xl">
            <input 
              type="checkbox" 
              id="editIsOngoingToggle" 
              v-model="editForm.isOngoing" 
              class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer"
            />
            <label for="editIsOngoingToggle" class="text-xs font-bold text-blue-950 cursor-pointer select-none flex items-center gap-1.5">
              <span>Thời hạn thực hiện: Thường xuyên</span>
              <span class="text-[11px] font-normal text-slate-500">(Không giới hạn năm cố định, báo cáo theo kỳ)</span>
            </label>
          </div>

          <!-- Date Range Inputs -->
          <div v-if="!editingItem?.parentId && !editingItem?.parentItemId && !editForm.isOngoing" class="grid grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="editForm.startYear" 
                :options="yearOptions" 
                :isMulti="false" 
                label="Năm Bắt Đầu" 
                placeholder="-- Chọn năm bắt đầu --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="editForm.dueYear" 
                :options="yearOptions" 
                :isMulti="false" 
                label="Năm Hoàn Thành" 
                placeholder="-- Chọn năm hoàn thành --"
              />
            </div>
          </div>
          <div v-else class="grid grid-cols-2 gap-3 bg-blue-50/50 p-3 rounded-xl border border-blue-200/60">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Ngày Bắt Đầu</label>
              <DatePicker v-model="editForm.startDate" placeholder="dd/mm/yyyy" />
            </div>
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Ngày Hoàn Thành</label>
              <DatePicker v-model="editForm.dueDate" placeholder="dd/mm/yyyy" />
            </div>
          </div>

          <!-- Lead Agency & Coordinating Agencies -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div>
              <SearchableSelect 
                v-model="editForm.leadAgencyId" 
                :options="leadAgencyOptions" 
                :isMulti="false" 
                label="Đơn Vị Chủ Trì" 
                placeholder="-- Chọn đơn vị chủ trì --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="editForm.coordinatingAgencyIds" 
                :options="coordinatingAgencyOptions" 
                :isMulti="true" 
                label="Cơ Quan Phối Hợp" 
                placeholder="-- Chọn cơ quan phối hợp --"
              />
            </div>
          </div>

          <!-- Multi-Deliverables Section for Tasks -->
          <div v-if="editingItem?.itemType !== 'Goal'" class="border border-slate-200 rounded-xl p-3.5 bg-slate-50/50 space-y-3">
            <div class="flex items-center justify-between">
              <label class="text-xs font-extrabold text-slate-800 uppercase flex items-center gap-1.5">
                <span>📋 Danh Mục Sản Phẩm Đầu Ra Dự Kiến (Phụ Lục II - Phương Án 1)</span>
              </label>
              <button 
                type="button" 
                @click="addEditDeliverable" 
                class="px-2.5 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-xs rounded-lg transition border border-blue-200 flex items-center gap-1 cursor-pointer"
              >
                + Thêm sản phẩm đầu ra
              </button>
            </div>

            <div v-if="!editForm.deliverables || editForm.deliverables.length === 0" class="text-xs text-slate-400 italic text-center py-2">
              Chưa khai báo sản phẩm đầu ra. Nhấn nút trên để thêm sản phẩm cụ thể.
            </div>

            <div v-else class="space-y-2.5 max-h-48 overflow-y-auto pr-1">
              <div 
                v-for="(del, idx) in editForm.deliverables" 
                :key="idx" 
                class="bg-white p-2.5 rounded-xl border border-slate-200 shadow-2xs space-y-2 relative"
              >
                <div class="flex items-center justify-between border-b border-slate-100 pb-1">
                  <span class="text-[11px] font-extrabold text-blue-800">Sản phẩm đầu ra #{{ idx + 1 }}</span>
                  <button 
                    type="button" 
                    @click="removeEditDeliverable(idx)" 
                    class="text-rose-500 hover:text-rose-700 text-xs font-bold hover:bg-rose-50 px-2 py-0.5 rounded transition cursor-pointer"
                  >
                    ✕ Xóa
                  </button>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
                  <div class="sm:col-span-2">
                    <label class="text-[10px] font-bold text-slate-600">Tên sản phẩm / Tên văn bản <span class="text-rose-500">*</span></label>
                    <input 
                      v-model="del.title" 
                      required 
                      placeholder="Ví dụ: Nghị định quy định về Dữ liệu số / Nền tảng chia sẻ..." 
                      class="w-full text-xs font-semibold bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500" 
                    />
                  </div>
                  <div>
                    <label class="text-[10px] font-bold text-slate-600">Hạn chót sản phẩm</label>
                    <DatePicker v-model="del.dueDate" placeholder="dd/mm/yyyy" />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
            <button type="button" @click="isEditModalOpen = false" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl cursor-pointer">Hủy</button>
            <button type="submit" class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm cursor-pointer">Lưu Thay Đổi</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Item Detail Modal -->
    <ItemDetailModal
      v-if="selectedItemForDetail"
      :is-open="isItemDetailModalOpen"
      :item="selectedItemForDetail"
      @close="isItemDetailModalOpen = false"
      @edit="openEditModal"
    />

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import { confirmModal } from '../services/confirm';
import { authState } from '../services/auth';
import DynamicPlanningGrid from '../components/DynamicPlanningGrid.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import ProgressUpdateModal from '../components/ProgressUpdateModal.vue';
import SendNotificationModal from '../components/SendNotificationModal.vue';
import ItemDetailModal from '../components/ItemDetailModal.vue';
import DatePicker from '../components/DatePicker.vue';
import OverlayPanel from '../components/OverlayPanel.vue';
import { getApiUrl } from '../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';
import { exportToExcel } from '../utils/excelExport';
import { parseApiError } from '../utils/errorUtils';

const props = defineProps({
  filterItemType: { type: String, default: 'Task' },
  subTab: { type: String, default: 'list' }
});

const isLoading = ref(false);

const activeSubTab = ref(props.subTab || 'list');

watch(() => [props.subTab, props.filterItemType], ([newSubTab, itemType]) => {
  if (itemType === 'Task') {
    activeSubTab.value = 'list';
  } else if (newSubTab) {
    activeSubTab.value = newSubTab;
  }
}, { immediate: true });

const isProgressModalOpen = ref(false);
const selectedTaskForProgress = ref(null);

const isNotificationModalOpen = ref(false);
const selectedItemForNotification = ref(null);

const isItemDetailModalOpen = ref(false);
const selectedItemForDetail = ref(null);

const agencies = ref([]);
const rawItemsList = ref([]);

const filterDraft = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedScopes: [],
  selectedStatuses: [],
  selectedSections: [],
  selectedGroups: [],
  fromYear: null,
  toYear: null,
  onlyOngoing: false
});

const activeFilterCount = computed(() => {
  let count = 0;
  if (filterDraft.value.selectedAgencyIds?.length) count++;
  if (filterDraft.value.selectedScopes?.length) count++;
  if (filterDraft.value.selectedSections?.length) count++;
  if (filterDraft.value.selectedGroups?.length) count++;
  if (filterDraft.value.fromYear || filterDraft.value.toYear || filterDraft.value.onlyOngoing) count++;
  if (filterDraft.value.selectedStatuses?.length) count++;
  return count;
});

const appliedFilters = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedScopes: [],
  selectedStatuses: [],
  selectedSections: [],
  selectedGroups: [],
  fromYear: null,
  toYear: null,
  onlyOngoing: false
});

const currentPage = ref(1);
const pageSize = ref(10);

let docDetailSearchTimer = null;
let docDetailSearchRequestId = 0;

function execFilterSearch() {
  if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer);
  docDetailSearchRequestId++;
  appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
  currentPage.value = 1;
}

watch(() => filterDraft.value.searchQuery, (newVal) => {
  if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer);
  const currentId = ++docDetailSearchRequestId;
  docDetailSearchTimer = setTimeout(() => {
    if (currentId !== docDetailSearchRequestId) return;
    appliedFilters.value.searchQuery = newVal || '';
    currentPage.value = 1;
  }, 300);
});

function resetFilterSearch() {
  if (docDetailSearchTimer) clearTimeout(docDetailSearchTimer);
  docDetailSearchRequestId++;
  filterDraft.value = {
    searchQuery: '',
    selectedAgencyIds: [],
    selectedScopes: [],
    selectedStatuses: [],
    selectedSections: [],
    selectedGroups: [],
    fromYear: null,
    toYear: null,
    onlyOngoing: false
  };
  appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
  currentPage.value = 1;
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return;
  currentPage.value = newPage;
}

const isCreateModalOpen = ref(false);
const createItemType = ref('Task');
const parentTaskForSubTask = ref(null);

const createForm = ref({
  code: '',
  title: '',
  section: '',
  group: '',
  isOngoing: false,
  isGeneralTask: false,
  startYear: 2026,
  dueYear: 2030,
  startDate: '',
  dueDate: '',
  leadAgencyId: '',
  coordinatingAgencyIds: [],
  deliverables: []
});

const createErrorMessage = ref('');
const isEditModalOpen = ref(false);
const editErrorMessage = ref('');
const editingItem = ref(null);
const editForm = ref({
  title: '',
  section: '',
  group: '',
  isOngoing: false,
  isGeneralTask: false,
  startYear: 2026,
  dueYear: 2030,
  startDate: '',
  dueDate: '',
  leadAgencyId: '',
  coordinatingAgencyIds: [],
  deliverables: []
});

function addDeliverable() {
  if (!createForm.value.deliverables) {
    createForm.value.deliverables = [];
  }
  createForm.value.deliverables.push({
    title: '',
    dueDate: '',
    notes: ''
  });
}

function removeDeliverable(idx) {
  if (createForm.value.deliverables) {
    createForm.value.deliverables.splice(idx, 1);
  }
}

const currentFormSections = computed(() => {
  return createItemType.value === 'Goal' ? GOAL_SECTIONS : TASK_SECTIONS;
});

const currentFormGroups = computed(() => {
  if (createItemType.value === 'Task') {
    return TASK_GROUPS;
  }
  const groups = GOAL_GROUPS;
  if (!createForm.value.section) return groups;
  return groups.filter(g => g.section === createForm.value.section);
});

const sectionFilterOptions = computed(() => {
  return props.filterItemType === 'Goal' ? GOAL_SECTIONS : TASK_SECTIONS;
});

const groupFilterOptions = computed(() => {
  return props.filterItemType === 'Goal' ? GOAL_GROUPS : TASK_GROUPS;
});

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: ag.name }));
});

const leadAgencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (ag.code === 'ALL_AGENCIES') {
      return { value: ag.id, label: `🌐 ${ag.name} (Tất cả đơn vị)` };
    }
    return { value: ag.id, label: ag.name };
  });
});

const coordinatingAgencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (ag.code === 'ALL_AGENCIES') {
      return { value: ag.id, label: `🌐 ${ag.name} (Tất cả đơn vị)` };
    }
    return { value: ag.id, label: ag.name };
  });
});

watch(() => createForm.value.leadAgencyId, (newId) => {
  const selected = agencies.value.find(a => a.id === newId);
  if (selected && selected.code === 'ALL_AGENCIES') {
    createForm.value.isGeneralTask = true;
  } else {
    createForm.value.isGeneralTask = false;
  }
}, { immediate: true });

const yearOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));
const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

const scopeOptions = computed(() => [
  { value: 'general', label: props.filterItemType === 'Goal' ? 'Mục tiêu chung (Tất cả đơn vị)' : 'Nhiệm vụ chung (Tất cả đơn vị)' },
  { value: 'specific', label: props.filterItemType === 'Goal' ? 'Mục tiêu riêng (Đơn vị cụ thể)' : 'Nhiệm vụ riêng (Đơn vị cụ thể)' }
]);

const statusOptions = ref([
  { value: 'NotStarted', label: '1. Chưa thực hiện' },
  { value: 'InProgressOnTime', label: '2. Đang thực hiện (trong hạn)' },
  { value: 'InProgressOverdue', label: '3. Đang thực hiện (quá hạn)' },
  { value: 'CompletedOnTime', label: '4. Hoàn thành (đúng hạn)' },
  { value: 'CompletedOverdue', label: '5. Hoàn thành (quá hạn)' },
  { value: 'ExpiringSoon', label: '6. Sắp hết hạn' }
]);

function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? new Date(sDate).toLocaleDateString('vi-VN') : '...';
  const d = dDate ? new Date(dDate).toLocaleDateString('vi-VN') : '...';
  return `${s} ➔ ${d}`;
}

function getStatusLabel(st) {
  const map = {
    'NotStarted': '1. Chưa thực hiện',
    'InProgressOnTime': '2. Đang thực hiện (trong hạn)',
    'InProgressOverdue': '3. Đang thực hiện (quá hạn)',
    'CompletedOnTime': '4. Hoàn thành (đúng hạn)',
    'CompletedOverdue': '5. Hoàn thành (quá hạn)',
    'ExpiringSoon': '6. Sắp hết hạn'
  };
  return map[st] || st || 'Chưa thực hiện';
}

function getStatusBadgeClass(st) {
  const map = {
    'NotStarted': 'bg-slate-100 text-slate-700 border border-slate-200',
    'InProgressOnTime': 'bg-blue-50 text-blue-800 border border-blue-200',
    'InProgressOverdue': 'bg-rose-50 text-rose-800 border border-rose-200',
    'CompletedOnTime': 'bg-emerald-50 text-emerald-800 border border-emerald-200',
    'CompletedOverdue': 'bg-teal-50 text-teal-800 border border-teal-200',
    'ExpiringSoon': 'bg-amber-50 text-amber-800 border border-amber-200'
  };
  return map[st] || 'bg-slate-100 text-slate-700';
}

function formatProgressDisplay(item) {
  if (!item) return '—';
  
  if (item.evaluationType === 'Qualitative' || item.evaluationType === 2 || item.evaluationType === '2') {
    if (item.latestProgressStatus) {
      const statusMap = {
        'Completed': 'Đã hoàn thành',
        'Reviewing': 'Đang trình/xét duyệt',
        'Drafting': 'Đang soạn thảo',
        'NotStarted': 'Chưa thực hiện'
      };
      return statusMap[item.latestProgressStatus] || item.latestProgressStatus;
    }
    return 'Chưa cập nhật';
  }

  if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    const unit = item.unitName || '%';
    return `${item.latestProgressValue} ${unit}`;
  }

  if (item.latestProgressStatus) {
    return item.latestProgressStatus;
  }

  return 'Chưa cập nhật';
}

const filteredList = computed(() => {
  let list = rawItemsList.value;

  // Search Query
  const searchQ = (appliedFilters.value.searchQuery || '').trim().toLowerCase();
  if (searchQ) {
    list = list.filter(i => {
      const matchCode = i.code?.toLowerCase().includes(searchQ);
      const matchTitle = i.title?.toLowerCase().includes(searchQ);
      const matchLeadAgency = i.leadAgencyName?.toLowerCase().includes(searchQ);
      const matchCoopAgencies = i.coordinatingAgencyNames?.some(c => c.toLowerCase().includes(searchQ)) || (typeof i.coordinatingAgencies === 'string' && i.coordinatingAgencies.toLowerCase().includes(searchQ));
      const matchSubTasks = i.subItems?.some(sub => sub.code?.toLowerCase().includes(searchQ) || sub.title?.toLowerCase().includes(searchQ));
      return matchCode || matchTitle || matchLeadAgency || matchCoopAgencies || matchSubTasks;
    });
  }

  // Agency Filter (Multi-select)
  if (appliedFilters.value.selectedAgencyIds && appliedFilters.value.selectedAgencyIds.length > 0) {
    list = list.filter(i => appliedFilters.value.selectedAgencyIds.includes(i.leadAgencyId));
  }

  // Section Filter (Multi-select)
  if (appliedFilters.value.selectedSections && appliedFilters.value.selectedSections.length > 0) {
    list = list.filter(i => appliedFilters.value.selectedSections.includes(i.section));
  }

  // Group Filter (Multi-select)
  if (appliedFilters.value.selectedGroups && appliedFilters.value.selectedGroups.length > 0) {
    list = list.filter(i => appliedFilters.value.selectedGroups.includes(i.group));
  }

  // Ongoing Tasks Filter (Thường xuyên)
  if (appliedFilters.value.onlyOngoing) {
    list = list.filter(i => i.isOngoing);
  }

  // Year Range Filter (From Year -> To Year)
  if (appliedFilters.value.fromYear || appliedFilters.value.toYear) {
    const fYr = appliedFilters.value.fromYear ? Number(appliedFilters.value.fromYear) : 2026;
    const tYr = appliedFilters.value.toYear ? Number(appliedFilters.value.toYear) : 2030;
    list = list.filter(i => {
      if (i.isOngoing) return true;
      const startY = i.startDate ? new Date(i.startDate).getFullYear() : 2026;
      const dueY = i.dueDate ? new Date(i.dueDate).getFullYear() : startY;
      return (startY <= tYr && dueY >= fYr);
    });
  }

  // Scope Filter (Multi-select)
  if (appliedFilters.value.selectedScopes && appliedFilters.value.selectedScopes.length > 0) {
    list = list.filter(i => {
      if (appliedFilters.value.selectedScopes.includes('general') && i.isGeneralTask) return true;
      if (appliedFilters.value.selectedScopes.includes('specific') && !i.isGeneralTask) return true;
      return false;
    });
  }

  // Status Filter (Multi-select)
  if (appliedFilters.value.selectedStatuses && appliedFilters.value.selectedStatuses.length > 0) {
    list = list.filter(i => appliedFilters.value.selectedStatuses.includes(i.calculatedStatus));
  }

  return list;
});

const totalCount = computed(() => filteredList.value.length);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value) || 1);

const paginatedPrimaryList = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredList.value.slice(start, start + pageSize.value);
});

function exportDocumentItemsToExcel() {
  const isGoal = (props.filterItemType === 'Goal');
  const itemTypeLabel = isGoal ? 'Mục Tiêu' : 'Nhiệm Vụ';
  const title = `DANH SÁCH ${itemTypeLabel.toUpperCase()} THEO DÕI CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg`;
  const fileName = `Danh_Sach_${isGoal ? 'Muc_Tieu' : 'Nhiem_Vu'}_1266`;
  const sheetName = isGoal ? 'Mục tiêu' : 'Nhiệm vụ';

  const headers = [
    "STT",
    "Mã",
    isGoal ? "Tên Mục Tiêu" : "Tên Nhiệm Vụ",
    "Cơ Quan Chủ Trì",
    "Cơ Quan Phối Hợp",
    "Thời Gian Thực Hiện",
    "Tiến Độ Hiện Tại (%)",
    "Trạng Thái"
  ];

  const minColWidths = {
    0: 8,
    1: 15,
    2: 50,
    3: 30,
    4: 25,
    5: 22,
    6: 18,
    7: 22
  };

  const rows = filteredList.value.map((item, idx) => {
    let dateStr = 'Thường xuyên';
    if (!item.isOngoing) {
      dateStr = formatDateRange(item.startDate, item.dueDate);
    }
    const progStr = item.latestProgressValue !== null && item.latestProgressValue !== undefined 
      ? `${item.latestProgressValue}%` 
      : 'Chưa cập nhật';

    return [
      idx + 1,
      item.code || '',
      item.title || '',
      item.leadAgencyName || '',
      item.cooperatingAgencies || '',
      dateStr,
      progStr,
      getStatusLabel(item.calculatedStatus)
    ];
  });

  exportToExcel({
    title,
    headers,
    rows,
    fileName,
    sheetName,
    minColWidths
  });
}

async function loadData() {
  isLoading.value = true;
  try {
    const docId = '12660000-0000-0000-0000-000000001266';
    const res = await fetch(getApiUrl(`/api/planning/documents/${docId}/grid`));
    if (res.ok) {
      const data = await res.json();
      const all = data.items || [];
      const targetType = props.filterItemType || 'Task';
      rawItemsList.value = all.filter(i => {
        if (targetType === 'Goal') {
          return i.itemType === 'Goal' || i.itemType === 1 || i.itemType === '1';
        } else {
          return i.itemType === 'Task' || i.itemType === 2 || i.itemType === '2';
        }
      });
    }

    const agRes = await fetch(getApiUrl('/api/agencies'));
    if (agRes.ok) {
      const agData = await agRes.json();
      agencies.value = Array.isArray(agData) ? agData : (agData.items || []);
    }
  } catch (e) {
    console.error('Lỗi tải dữ liệu:', e);
  } finally {
    isLoading.value = false;
  }
}

watch(() => props.filterItemType, () => {
  currentPage.value = 1;
  loadData();
});

function openCreateModal(type) {
  createItemType.value = type;
  parentTaskForSubTask.value = null;
  createForm.value = {
    code: '',
    title: '',
    section: type === 'Goal' ? 'Mục A' : '',
    group: '',
    isOngoing: false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: '',
    dueDate: '',
    leadAgencyId: agencies.value[0]?.id || '',
    coordinatingAgencyIds: [],
    deliverables: []
  };
  isCreateModalOpen.value = true;
}

function openCreateSubTaskModal(parentItem) {
  createItemType.value = 'Task';
  parentTaskForSubTask.value = parentItem;
  createForm.value = {
    code: '',
    title: '',
    section: '',
    group: parentItem.group || '',
    isOngoing: parentItem.isOngoing || false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: parentItem.startDate ? new Date(parentItem.startDate).toISOString().split('T')[0] : '',
    dueDate: parentItem.dueDate ? new Date(parentItem.dueDate).toISOString().split('T')[0] : '',
    leadAgencyId: parentItem.leadAgencyId || agencies.value[0]?.id || '',
    coordinatingAgencyIds: [],
    deliverables: []
  };
  isCreateModalOpen.value = true;
}

async function submitCreateItem() {
  createErrorMessage.value = '';

  if (!createForm.value.title || !createForm.value.title.trim()) {
    createErrorMessage.value = 'Tên mục tiêu / nhiệm vụ không được để trống.';
    toast.error(createErrorMessage.value);
    return;
  }

  if (!createForm.value.leadAgencyId) {
    createErrorMessage.value = 'Vui lòng chọn đơn vị chủ trì.';
    toast.error(createErrorMessage.value);
    return;
  }

  if (createItemType.value !== 'Goal' && createForm.value.deliverables && createForm.value.deliverables.length > 0) {
    for (let idx = 0; idx < createForm.value.deliverables.length; idx++) {
      const del = createForm.value.deliverables[idx];
      if (!del.title || !del.title.trim()) {
        createErrorMessage.value = `Sản phẩm đầu ra #${idx + 1}: Tên sản phẩm / tên văn bản không được để trống.`;
        toast.error(createErrorMessage.value);
        return;
      }
    }
  }

  try {
    const docId = '12660000-0000-0000-0000-000000001266';
    let startDateIso = null;
    let dueDateIso = null;

    if (parentTaskForSubTask.value) {
      startDateIso = createForm.value.startDate ? new Date(createForm.value.startDate).toISOString() : null;
      dueDateIso = createForm.value.dueDate ? new Date(createForm.value.dueDate).toISOString() : null;
    } else {
      const sYear = createForm.value.startYear || 2026;
      const dYear = createForm.value.dueYear || 2030;
      startDateIso = new Date(`${sYear}-01-01T00:00:00.000Z`).toISOString();
      dueDateIso = new Date(`${dYear}-12-31T23:59:59.999Z`).toISOString();
    }

    const payload = {
      documentId: docId,
      parentId: parentTaskForSubTask.value ? parentTaskForSubTask.value.taskId : null,
      itemType: createItemType.value,
      code: '',
      title: createForm.value.title,
      section: createItemType.value === 'Goal' ? createForm.value.section : '',
      group: createForm.value.group,
      isOngoing: createForm.value.isOngoing,
      isGeneralTask: createForm.value.isGeneralTask,
      startDate: startDateIso,
      dueDate: dueDateIso,
      leadAgencyId: createForm.value.leadAgencyId,
      coordinatingAgencyIds: createForm.value.coordinatingAgencyIds || [],
      evaluationType: createItemType.value === 'Goal' ? 'Quantitative' : 'Qualitative',
      deliverables: createForm.value.deliverables || []
    };

    const res = await fetch(getApiUrl('/api/planning/items'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      toast.success("Thêm mới thành công!");
      isCreateModalOpen.value = false;
      await loadData();
    } else {
      const err = await res.json().catch(() => ({}));
      const msg = parseApiError(err, 'Lỗi khi thêm mới mục tiêu/nhiệm vụ.');
      createErrorMessage.value = msg;
      toast.error(msg);
    }
  } catch (e) {
    createErrorMessage.value = 'Không thể kết nối máy chủ.';
    toast.error('Không thể kết nối máy chủ.');
  }
}

function openProgressModal(item) {
  if (authState.isAdmin.value) {
    toast.warning('Tài khoản Quản trị viên (Admin) không thực hiện cập nhật tiến độ. Thao tác này dành cho tài khoản cán bộ đầu mối của các Cơ quan / Bộ / Ngành.');
    return;
  }
  selectedTaskForProgress.value = item;
  isProgressModalOpen.value = true;
}

function openNotificationModal(item) {
  selectedItemForNotification.value = item;
  isNotificationModalOpen.value = true;
}

function openItemDetailModal(item) {
  selectedItemForDetail.value = item;
  isItemDetailModalOpen.value = true;
}

function hasProgress(item) {
  if (!item) return false;

  if (item.calculatedStatus && item.calculatedStatus !== 'NotStarted' && item.calculatedStatus !== '1. Chưa thực hiện') {
    return true;
  }

  const selfHasLogs = (item.progressLogs && item.progressLogs.length > 0) ||
                      (item.latestProgressValue !== null && item.latestProgressValue !== undefined && item.latestProgressValue > 0) ||
                      (item.latestProgressStatus !== null && item.latestProgressStatus !== undefined && item.latestProgressStatus !== '' && item.latestProgressStatus !== 'NotStarted');
  if (selfHasLogs) return true;

  if (item.subItems && Array.isArray(item.subItems)) {
    return item.subItems.some(sub => hasProgress(sub));
  }

  return false;
}

async function handleDeleteItem(item) {
  if (hasProgress(item)) {
    toast.warning("Chỉ được phép xóa Mục tiêu / Nhiệm vụ khi ở trạng thái Chưa bắt đầu.");
    return;
  }

  const isSubTask = !!item.parentItemId || !!item.parentId;
  const itemTypeName = isSubTask ? 'Sub-Task' : (filterItemType.value === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ');

  const confirmed = await confirmModal({
    title: `Xóa ${itemTypeName}`,
    message: `Bạn có chắc chắn muốn xóa ${itemTypeName} "${item.code ? item.code + ': ' : ''}${item.title}"? Thao tác này không thể hoàn tác.`,
    confirmText: 'Xóa ngay',
    cancelText: 'Hủy',
    type: 'danger'
  });

  if (!confirmed) return;

  try {
    const targetId = item.taskId || item.id;
    const res = await fetch(getApiUrl(`/api/planning/items/${targetId}`), {
      method: 'DELETE'
    });

    if (res.ok) {
      const data = await res.json().catch(() => ({}));
      toast.success(data.message || `Đã xóa ${itemTypeName} thành công.`);
      await loadData();
    } else {
      const err = await res.json().catch(() => ({}));
      toast.error(err.error || err.message || `Không thể xóa ${itemTypeName}.`);
    }
  } catch (e) {
    toast.error('Có lỗi xảy ra khi kết nối máy chủ.');
  }
}

function openEditModal(item) {
  if (!item) return;

  if (hasProgress(item)) {
    toast.warning("Chỉ được phép chỉnh sửa Mục tiêu / Nhiệm vụ khi ở trạng thái Chưa bắt đầu.");
    return;
  }

  editErrorMessage.value = '';
  editingItem.value = item;

  let sDate = '';
  let dDate = '';
  let sYear = 2026;
  let dYear = 2030;

  if (item.startDate) {
    const dt = new Date(item.startDate);
    if (!isNaN(dt.getTime())) {
      sDate = dt.toISOString().split('T')[0];
      sYear = dt.getFullYear();
    }
  }
  if (item.dueDate) {
    const dt = new Date(item.dueDate);
    if (!isNaN(dt.getTime())) {
      dDate = dt.toISOString().split('T')[0];
      dYear = dt.getFullYear();
    }
  }

  editForm.value = {
    title: item.title || '',
    section: item.section || '',
    group: item.group || '',
    isOngoing: !!item.isOngoing,
    isGeneralTask: !!item.isGeneralTask,
    startYear: sYear,
    dueYear: dYear,
    startDate: sDate,
    dueDate: dDate,
    leadAgencyId: item.leadAgencyId || agencies.value[0]?.id || '',
    coordinatingAgencyIds: item.coordinatingAgencyIds ? [...item.coordinatingAgencyIds] : [],
    deliverables: item.deliverables ? JSON.parse(JSON.stringify(item.deliverables)) : []
  };

  isEditModalOpen.value = true;
}

function addEditDeliverable() {
  if (!editForm.value.deliverables) {
    editForm.value.deliverables = [];
  }
  editForm.value.deliverables.push({
    title: '',
    dueDate: '',
    currentStatus: 'NotStarted'
  });
}

function removeEditDeliverable(idx) {
  if (editForm.value.deliverables) {
    editForm.value.deliverables.splice(idx, 1);
  }
}

async function submitEditItem() {
  if (!editingItem.value) return;

  editErrorMessage.value = '';

  if (!editForm.value.title || !editForm.value.title.trim()) {
    editErrorMessage.value = 'Tên mục tiêu / nhiệm vụ không được để trống.';
    toast.error(editErrorMessage.value);
    return;
  }

  if (!editForm.value.leadAgencyId) {
    editErrorMessage.value = 'Vui lòng chọn đơn vị chủ trì.';
    toast.error(editErrorMessage.value);
    return;
  }

  if (editingItem.value.itemType !== 'Goal' && editForm.value.deliverables && editForm.value.deliverables.length > 0) {
    for (let idx = 0; idx < editForm.value.deliverables.length; idx++) {
      const del = editForm.value.deliverables[idx];
      if (!del.title || !del.title.trim()) {
        editErrorMessage.value = `Sản phẩm đầu ra #${idx + 1}: Tên sản phẩm / tên văn bản không được để trống.`;
        toast.error(editErrorMessage.value);
        return;
      }
    }
  }

  try {
    let startDateIso = null;
    let dueDateIso = null;

    if (editingItem.value.parentId || editingItem.value.parentItemId || editForm.value.startDate || editForm.value.dueDate) {
      startDateIso = editForm.value.startDate ? new Date(editForm.value.startDate).toISOString() : null;
      dueDateIso = editForm.value.dueDate ? new Date(editForm.value.dueDate).toISOString() : null;
    } else {
      const sYear = editForm.value.startYear || 2026;
      const dYear = editForm.value.dueYear || 2030;
      startDateIso = new Date(`${sYear}-01-01T00:00:00.000Z`).toISOString();
      dueDateIso = new Date(`${dYear}-12-31T23:59:59.999Z`).toISOString();
    }

    const payload = {
      title: editForm.value.title,
      section: editForm.value.section,
      group: editForm.value.group,
      isOngoing: editForm.value.isOngoing,
      isGeneralTask: editForm.value.isGeneralTask,
      startDate: startDateIso,
      dueDate: dueDateIso,
      leadAgencyId: editForm.value.leadAgencyId,
      coordinatingAgencyIds: editForm.value.coordinatingAgencyIds || [],
      deliverables: editForm.value.deliverables || []
    };

    const targetId = editingItem.value.taskId || editingItem.value.id;
    const res = await fetch(getApiUrl(`/api/planning/items/${targetId}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      toast.success("Cập nhật thông tin thành công!");
      isEditModalOpen.value = false;
      await loadData();
    } else {
      const err = await res.json().catch(() => ({}));
      const msg = parseApiError(err, 'Lỗi khi cập nhật mục tiêu/nhiệm vụ.');
      editErrorMessage.value = msg;
      toast.error(msg);
    }
  } catch (e) {
    editErrorMessage.value = 'Không thể kết nối máy chủ.';
    toast.error('Không thể kết nối máy chủ.');
  }
}

onMounted(() => {
  loadData();
});
</script>
