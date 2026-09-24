<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Top Header Bar -->
    <div class="bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3 w-full">
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div>
          <h2 class="text-sm sm:text-base font-bold text-slate-800">
            {{ filterItemType === 'Goal' ? '🎯 Danh Sách Mục Tiêu' : '📋 Danh Sách Nhiệm Vụ' }}
          </h2>
        </div>

        <div class="flex items-center gap-2 flex-wrap">
          <!-- General List Export (All Users including Admin) -->
          <button 
            @click="exportDocumentItemsToExcel"
            class="px-3.5 py-2 text-slate-700 hover:text-slate-900 font-bold text-xs rounded-xl bg-slate-100 hover:bg-slate-200 border border-slate-200/80 transition shadow-2xs flex items-center gap-1.5 cursor-pointer"
            title="Xuất danh sách mục tiêu/nhiệm vụ ra file Excel"
          >
            <svg class="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
            <span>Xuất Excel Danh Sách</span>
          </button>



          <button 
            v-if="authState.isAdmin.value"
            @click="openPendingApprovalsModal"
            class="px-3.5 py-2 text-amber-950 font-bold text-xs rounded-xl bg-amber-100 hover:bg-amber-200 border border-amber-300 transition shadow-2xs flex items-center gap-1.5 cursor-pointer relative"
            title="Xem danh sách báo cáo tiến độ chờ phê duyệt"
          >
            <span class="text-amber-700">⏳</span>
            <span>Duyệt Báo Cáo Tiến Độ</span>
            <span v-if="globalPendingLogs?.length > 0" class="ml-1 px-2 py-0.5 bg-amber-600 text-white rounded-full text-[10px] font-bold animate-pulse">
              {{ globalPendingLogs.length }}
            </span>
          </button>

          <button 
            v-if="authState.isAdmin.value"
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
        <div class="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3 bg-slate-50/60 p-3 w-full">
          <div class="flex items-center gap-2 flex-1 max-w-xl min-w-0">
            <!-- Quick Search Input -->
            <div class="relative flex-1 min-w-[200px]">
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
              widthClass="w-[320px] sm:w-[480px] max-w-[90vw]"
              @apply="execFilterSearch"
              @reset="resetFilterSearch"
            >
              <div class="space-y-3">
                <div>
                  <SearchableSelect 
                    v-model="filterDraft.selectedAgencyIds" 
                    :options="leadAgencyOptions" 
                    :isMulti="true" 
                    label="Cơ Quan Chủ Trì" 
                    placeholder="Tất cả cơ quan chủ trì"
                  />
                </div>

                <div>
                  <SearchableSelect 
                    v-model="filterDraft.selectedSubAgencyIds" 
                    :options="subAgencyOptions" 
                    :isMulti="true" 
                    label="Đơn Vị Trực Thuộc" 
                    placeholder="Tất cả đơn vị trực thuộc"
                  />
                </div>

                <!-- Combined 2-Column Row: Phạm Vi & Trạng Thái Hạng Mục -->
                <div class="grid grid-cols-2 gap-3">
                  <div>
                    <SearchableSelect 
                      v-model="filterDraft.selectedScopes" 
                      :options="scopeOptions" 
                      :isMulti="true" 
                      label="Phạm Vi" 
                      placeholder="Tất cả phạm vi"
                    />
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
                    <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wider block">Giai Đoạn (Từ năm ➔ Đến năm)</label>
                    <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-bold text-blue-700 select-none">
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
              </div>
            </OverlayPanel>
          </div>
        </div>

        <!-- MAIN DATA TABLE WITH STICKY HEADER & FROZEN FIRST 3 COLUMNS -->
        <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu danh sách từ máy chủ..." />
        <div v-else class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
          <table class="w-full min-w-[1280px] text-left text-sm text-slate-700 border-collapse">
            <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-30 shadow-xs">
              <tr>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-30">Mã</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-30 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                  {{ filterItemType === 'Goal' ? 'Tên Mục Tiêu' : (filterItemType === 'Task' ? 'Tên Nhiệm Vụ' : 'Tên Mục Tiêu / Nhiệm Vụ') }}
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px]">Cơ Quan Chủ Trì</th>
                <th v-if="isLeadAgencyFilteredByBKHCN" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px]">Giao Đơn Vị Trực Thuộc</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px]">Cơ Quan Phối Hợp</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[160px] w-[160px] max-w-[160px]">Thời Gian thực hiện</th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 min-w-[125px] w-[125px] max-w-[125px]">Tiến Độ</th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 min-w-[195px] w-[195px] max-w-[195px]">Trạng Thái</th>
                <th class="px-3 py-2.5 text-center bg-slate-100 min-w-[90px] w-[90px] max-w-[90px]">Thao Tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <template v-for="item in paginatedPrimaryList" :key="item.taskId">
                <!-- Parent Row -->
                <tr @click="openItemDetailModal(item)" class="group hover:bg-blue-100/90 cursor-pointer">
                  <td class="px-3 py-2.5 font-normal text-slate-700 border-r border-slate-200 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-white group-hover:bg-blue-100">
                    {{ item.code }}
                  </td>
                  <td class="px-3 py-2.5 font-normal text-slate-800 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-20 bg-white group-hover:bg-blue-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                    <VTooltip 
                      theme="custom-dark"
                      placement="top"
                      :delay="{ show: 1500, hide: 0 }"
                    >
                      <div class="line-clamp-2 font-normal text-slate-800 text-xs leading-relaxed cursor-help">
                        {{ item.title }}
                      </div>

                      <template #popper>
                        <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                          <span class="font-bold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">
                            {{ item.itemType === 'Goal' ? '🎯 Chi Tiết Mục Tiêu' : '📋 Chi Tiết Nhiệm Vụ' }}
                          </span>
                          {{ item.title }}
                        </div>
                      </template>
                    </VTooltip>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    {{ item.leadAgencyName }}
                  </td>
                  <td v-if="isLeadAgencyFilteredByBKHCN" class="px-3 py-2.5 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <span v-if="item.assignedAgencyName" class="px-2 py-0.5 rounded-md bg-blue-50 text-blue-700 font-semibold text-[11px] border border-blue-100 inline-block">
                      {{ item.assignedAgencyName }}
                    </span>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <template v-if="item.coordinatingAgencyNames && item.coordinatingAgencyNames.length > 0">
                      {{ item.coordinatingAgencyNames.join(', ') }}
                    </template>
                    <template v-else-if="item.coordinatingAgencyCodes && item.coordinatingAgencyCodes.length > 0">
                      {{ item.coordinatingAgencyCodes.join(', ') }}
                    </template>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-xs font-normal text-slate-600 whitespace-nowrap min-w-[160px] w-[160px] max-w-[160px]">
                    <span v-if="item.isOngoing" class="px-2 py-0.5 rounded-full font-medium text-[11px] bg-slate-100 text-slate-700 border border-slate-200">
                      Thường xuyên
                    </span>
                    <span v-else>
                      {{ formatDateRange(item.startDate, item.dueDate) }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-center text-xs min-w-[125px] w-[125px] max-w-[125px] overflow-hidden">
                    <span v-if="authState.isAdmin.value && isGeneralTaskOrAllAgencies(item)" class="text-slate-400 font-normal">—</span>
                    <span v-else class="font-normal text-xs text-slate-700 line-clamp-2 break-words leading-tight block" :title="formatProgressDisplay(item)">
                      {{ formatProgressDisplay(item) }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-center min-w-[195px] w-[195px] max-w-[195px]">
                    <div class="flex flex-col items-center gap-1">
                      <span v-if="authState.isAdmin.value && isGeneralTaskOrAllAgencies(item)" class="px-2.5 py-0.5 rounded-full text-xs font-normal bg-slate-100 text-slate-500 italic border border-slate-200">
                        —
                      </span>
                      <span v-else :class="['px-2.5 py-0.5 rounded-full text-xs font-medium shadow-2xs inline-block whitespace-nowrap', getStatusBadgeClass(item.calculatedStatus)]">
                        {{ getStatusLabel(item.calculatedStatus) }}
                      </span>
                      <span v-if="item.hasPendingApproval" class="px-2 py-0.5 rounded-md text-[10px] font-medium bg-amber-100 text-amber-900 border border-amber-300 animate-pulse cursor-pointer" @click.stop="openItemDetailModal(item, 'history')" title="Có báo cáo tiến độ mới chờ Cấp 2 phê duyệt. Bấm để xem chi tiết.">
                        ⏳ Báo cáo chờ duyệt
                      </span>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-center whitespace-nowrap" @click.stop>
                    <div class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap">
                      <button 
                        v-if="canCreateSubTask(item)"
                        @click.stop="openCreateSubTaskModal(item)" 
                        class="p-1.5 bg-slate-100 text-slate-700 hover:bg-slate-200 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                        title="Thêm Nhiệm Vụ Con"
                      >
                        <svg class="w-4 h-4 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
                      </button>

                      <button 
                        v-if="canUpdateProgress(item)"
                        @click.stop="openProgressModal(item)" 
                        class="p-1.5 bg-slate-100 text-slate-700 hover:bg-slate-200 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                        title="Cập Nhật Tiến Độ & Minh Chứng"
                      >
                        <svg class="w-4 h-4 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                      </button>

                      <span 
                        v-else-if="isCoordinatingOnly(item)"
                        class="px-2 py-0.5 rounded-md text-[10px] font-medium bg-amber-50 text-amber-700 border border-amber-200/80"
                        title="Đơn vị phối hợp - Chỉ xem thông tin"
                      >
                        Đơn vị phối hợp (Chỉ xem)
                      </span>

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
                          <div class="py-1.5 w-44 bg-white rounded-xl shadow-xl border border-slate-200 text-xs font-normal space-y-0.5" @click.stop>
                            <button 
                              v-if="canAssignTask(item)"
                              @click="openAssignModalFromList(item); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Giao cho đơn vị trực thuộc"
                            >
                              <span>Giao đơn vị trực thuộc</span>
                            </button>

                            <button 
                              v-if="authState.isAdmin.value && !hasProgress(item)"
                              @click="openEditModal(item); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Chỉnh Sửa"
                            >
                              <span>Chỉnh sửa</span>
                            </button>

                            <button 
                              @click="openNotificationModal(item); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Gửi thông báo đến đơn vị chủ trì & phối hợp"
                            >
                              <span>Gửi thông báo</span>
                            </button>

                            <div v-if="authState.isAdmin.value && !hasProgress(item)" class="border-t border-slate-100 my-1"></div>

                            <button 
                              v-if="authState.isAdmin.value && !hasProgress(item)"
                              @click="handleDeleteItem(item); hide()" 
                              class="w-full text-left px-3 py-2 text-rose-600 hover:bg-rose-50 transition cursor-pointer"
                              title="Xóa"
                            >
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
                  <td class="px-3 py-2 font-normal text-slate-600 border-r border-slate-200 pl-3 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-slate-50 group-hover:bg-blue-100">
                    └─ {{ sub.code }}
                  </td>
                  <td class="px-3 py-2 font-normal text-slate-800 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-20 bg-slate-50 group-hover:bg-blue-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                    <VTooltip 
                      theme="custom-dark"
                      placement="top"
                      :delay="{ show: 1500, hide: 0 }"
                    >
                      <div class="line-clamp-2 font-normal text-slate-800 text-xs leading-relaxed cursor-help">
                        {{ sub.title }}
                      </div>

                      <template #popper>
                        <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                          <span class="font-bold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">
                            📌 Chi Tiết Sub-Task
                          </span>
                          {{ sub.title }}
                        </div>
                      </template>
                    </VTooltip>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-normal text-slate-700 min-w-[180px] w-[180px] max-w-[180px]">
                    {{ sub.leadAgencyName }}
                  </td>
                  <td v-if="isLeadAgencyFilteredByBKHCN" class="px-3 py-2 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <span v-if="sub.assignedAgencyName" class="px-2 py-0.5 rounded-md bg-blue-50 text-blue-700 font-semibold text-[11px] border border-blue-100 inline-block">
                      {{ sub.assignedAgencyName }}
                    </span>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-normal text-slate-600 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <template v-if="sub.coordinatingAgencyNames && sub.coordinatingAgencyNames.length > 0">
                      {{ sub.coordinatingAgencyNames.join(', ') }}
                    </template>
                    <template v-else-if="sub.coordinatingAgencyCodes && sub.coordinatingAgencyCodes.length > 0">
                      {{ sub.coordinatingAgencyCodes.join(', ') }}
                    </template>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-normal text-slate-600 whitespace-nowrap min-w-[160px] w-[160px] max-w-[160px]">
                    <span v-if="sub.isOngoing" class="px-2 py-0.5 rounded-full font-medium text-[11px] bg-slate-100 text-slate-700 border border-slate-200">
                      Thường xuyên
                    </span>
                    <span v-else>
                      {{ formatDateRange(sub.startDate, sub.dueDate) }}
                    </span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 text-center font-normal text-xs min-w-[125px] w-[125px] max-w-[125px] overflow-hidden">
                    <span v-if="authState.isAdmin.value && isGeneralTaskOrAllAgencies(sub)" class="text-slate-400 font-normal">—</span>
                    <span v-else class="font-normal text-xs text-slate-700 line-clamp-2 break-words leading-tight block" :title="formatProgressDisplay(sub)">
                      {{ formatProgressDisplay(sub) }}
                    </span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 text-center min-w-[195px] w-[195px] max-w-[195px]">
                    <span v-if="authState.isAdmin.value && isGeneralTaskOrAllAgencies(sub)" class="px-2.5 py-0.5 rounded-full text-[11px] font-normal bg-slate-100 text-slate-500 italic border border-slate-200">
                      —
                    </span>
                    <span v-else :class="['px-2.5 py-0.5 rounded-full text-[11px] font-medium inline-block whitespace-nowrap', getStatusBadgeClass(sub.calculatedStatus)]">
                      {{ getStatusLabel(sub.calculatedStatus) }}
                    </span>
                  </td>
                  <td class="px-3 py-2 text-center whitespace-nowrap" @click.stop>
                    <div class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap">
                      <button 
                        v-if="canUpdateProgress(sub)"
                        @click.stop="openProgressModal(sub)" 
                        class="p-1.5 bg-slate-100 text-slate-700 hover:bg-slate-200 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                        title="Cập Nhật Tiến Độ & Minh Chứng"
                      >
                        <svg class="w-4 h-4 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                      </button>

                      <span 
                        v-else-if="isCoordinatingOnly(sub)"
                        class="px-2 py-0.5 rounded-md text-[10px] font-bold bg-amber-50 text-amber-700 border border-amber-200/80"
                        title="Đơn vị phối hợp - Chỉ xem thông tin"
                      >
                        Đơn vị phối hợp (Chỉ xem)
                      </span>

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
                              v-if="authState.isAdmin.value && !hasProgress(sub)"
                              @click="openEditModal(sub); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Chỉnh Sửa Sub-Task"
                            >
                              <span>Chỉnh sửa</span>
                            </button>

                            <button 
                              @click="openNotificationModal(sub); hide()" 
                              class="w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Gửi thông báo đến đơn vị chủ trì & phối hợp"
                            >
                              <span>Gửi thông báo</span>
                            </button>

                            <div v-if="authState.isAdmin.value && !hasProgress(sub)" class="border-t border-slate-100 my-1"></div>

                            <button 
                              v-if="authState.isAdmin.value && !hasProgress(sub)"
                              @click="handleDeleteItem(sub); hide()" 
                              class="w-full text-left px-3 py-2 text-rose-600 hover:bg-rose-50 transition cursor-pointer"
                              title="Xóa Sub-Task"
                            >
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
                <td :colspan="isLeadAgencyFilteredByBKHCN ? 9 : 8" class="p-8 text-center text-slate-400 font-semibold italic">
                  Không tìm thấy dữ liệu phù hợp.
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Attached Pagination Controls Bar -->
        <div class="flex flex-col md:flex-row items-center justify-between gap-3 bg-slate-50/70 p-3.5 border-t border-slate-200/80 text-xs text-slate-600 font-semibold w-full">
          <div class="flex items-center gap-3 whitespace-nowrap flex-wrap justify-center sm:justify-start">
            <span class="whitespace-nowrap">Hiển thị <span class="font-bold text-slate-900">{{ totalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, totalCount) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ totalCount }}</span> {{ filterItemType === 'Goal' ? 'mục tiêu' : 'nhiệm vụ' }}</span>
            
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

          <div class="flex items-center gap-2 shrink-0 whitespace-nowrap flex-wrap justify-center">
            <button 
              @click="changePage(currentPage - 1)" 
              :disabled="currentPage <= 1"
              class="px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
            >
              ‹ Trang trước
            </button>
            
            <span class="px-3 py-1.5 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-bold">
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
    <div v-if="isCreateModalOpen" @click.self="isCreateModalOpen = false" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-6 sm:p-7 max-h-[92vh] flex flex-col font-sans">
        <h3 class="text-base font-bold text-slate-800 border-b border-slate-100 pb-2 shrink-0">
          {{ parentTaskForSubTask ? `Thêm Nhiệm Vụ Con Cho: ${parentTaskForSubTask.code}` : `Thêm mới ${createItemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ'}` }}
        </h3>

        <form @submit.prevent="submitCreateItem" class="flex-1 flex flex-col min-h-0 pt-3">
          <div class="flex-1 overflow-y-auto custom-scrollbar space-y-3 pr-1">
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
                :required="true"
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

          <!-- Unit Selection for Goals -->
          <div v-if="createItemType === 'Goal'">
            <SearchableSelect 
              v-model="createForm.unitName" 
              :options="goalUnitOptions" 
              :isMulti="false" 
              :required="true"
              label="Đơn Vị Tính" 
              placeholder="-- Chọn Đơn vị tính --"
            />
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
              <span class="text-[11px] font-normal text-slate-500">(Tự động áp dụng từ 01/01/2026 đến 31/12/2030)</span>
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

          <!-- Lead Agency, Assigned Sub-Agency & Coordinating Agencies -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div>
              <SearchableSelect 
                v-model="createForm.leadAgencyId" 
                :options="leadAgencyOptions" 
                :isMulti="false" 
                :required="true"
                label="Đơn Vị Chủ Trì (Cấp 2)" 
                placeholder="-- Chọn đơn vị chủ trì --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="createForm.assignedAgencyId" 
                :options="createAssignedAgencyOptions" 
                :isMulti="false" 
                label="Giao Đơn Vị Trực Thuộc" 
                placeholder="-- Chọn đơn vị trực thuộc --"
                :disabled="!createForm.leadAgencyId || createAssignedAgencyOptions.length === 0"
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
              <label class="text-xs font-bold text-slate-800 uppercase flex items-center gap-1.5">
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
                  <span class="text-[11px] font-bold text-blue-800">Sản phẩm đầu ra #{{ idx + 1 }}</span>
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
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3 shrink-0">
            <button type="button" @click="isCreateModalOpen = false" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl cursor-pointer">Hủy</button>
            <button type="submit" class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm cursor-pointer">Lưu</button>
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
      :unit-name="selectedTaskForProgress.unitName || selectedTaskForProgress.unit?.name || '%'"
      :custom-baseline="selectedTaskForProgress.customBaseline || {}"
      :deliverables="selectedTaskForProgress.deliverables || []"
      :has-pending-approval="!!selectedTaskForProgress.hasPendingApproval"
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
    <div v-if="isEditModalOpen" @click.self="isEditModalOpen = false" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-6 sm:p-7 space-y-4 max-h-[90vh] flex flex-col">
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
                :required="true"
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

          <!-- Unit Selection for Goals -->
          <div v-if="editingItem?.itemType === 'Goal' || editingItem?.itemType === 1 || editingItem?.itemType === '1'">
            <SearchableSelect 
              v-model="editForm.unitName" 
              :options="goalUnitOptions" 
              :isMulti="false" 
              :required="true"
              label="Đơn Vị Tính" 
              placeholder="-- Chọn Đơn vị tính --"
            />
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
              <span class="text-[11px] font-normal text-slate-500">(Tự động áp dụng từ 01/01/2026 đến 31/12/2030)</span>
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

          <!-- Lead Agency, Assigned Sub-Agency & Coordinating Agencies -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div>
              <SearchableSelect 
                v-model="editForm.leadAgencyId" 
                :options="leadAgencyOptions" 
                :isMulti="false" 
                :required="true"
                label="Đơn Vị Chủ Trì (Cấp 2)" 
                placeholder="-- Chọn đơn vị chủ trì --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="editForm.assignedAgencyId" 
                :options="editAssignedAgencyOptions" 
                :isMulti="false" 
                label="Giao Đơn Vị Trực Thuộc" 
                placeholder="-- Chọn đơn vị trực thuộc --"
                :disabled="!editForm.leadAgencyId || editAssignedAgencyOptions.length === 0"
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
              <label class="text-xs font-bold text-slate-800 uppercase flex items-center gap-1.5">
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
                  <span class="text-[11px] font-bold text-blue-800">Sản phẩm đầu ra #{{ idx + 1 }}</span>
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
      :initial-tab="itemDetailModalInitialTab"
      @close="isItemDetailModalOpen = false"
      @edit="openEditModal"
    />

    <!-- Modal Giao Nhiệm Vụ cho Đơn Vị Trực Thuộc từ Danh Sách -->
    <div v-if="isAssignModalOpen" @click.self="isAssignModalOpen = false" class="fixed inset-0 z-60 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-md w-full p-5 space-y-4 font-sans">
        <div class="flex justify-between items-center border-b border-slate-100 pb-2">
          <h4 class="font-bold text-slate-800 text-sm">⚡ Giao Đơn Vị Trực Thuộc</h4>
          <button @click="isAssignModalOpen = false" class="text-slate-400 hover:text-slate-600 font-bold">✕</button>
        </div>
        <div class="space-y-3">
          <p class="text-xs text-slate-600">Chọn đơn vị trực thuộc để giao cho <strong>{{ assignItemTarget?.code }}: {{ assignItemTarget?.title }}</strong>:</p>
          <div v-if="assignSubAgencyOptions.length === 0" class="p-3 bg-amber-50 text-amber-800 text-xs rounded-xl border border-amber-200 font-semibold">
            Cơ quan chủ trì <strong>{{ assignItemTarget?.leadAgencyName || 'này' }}</strong> hiện chưa có đơn vị trực thuộc nào trong hệ thống.
          </div>
          <div v-else class="space-y-1">
            <SearchableSelect
              v-model="selectedSubAgencyId"
              :options="[
                { value: '', label: '— Bỏ giao (Chưa giao đơn vị trực thuộc) —' },
                ...assignSubAgencyOptions.map(sub => ({ value: sub.id, label: sub.name }))
              ]"
              :isMulti="false"
              :clearable="false"
              placeholder="— Bỏ giao (Chưa giao đơn vị trực thuộc) —"
            />
          </div>
        </div>
        <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
          <button @click="isAssignModalOpen = false" class="px-3 py-1.5 text-xs text-slate-600 hover:bg-slate-100 rounded-xl font-bold cursor-pointer">Hủy</button>
          <button @click="submitAssignTaskFromList" class="px-4 py-1.5 text-xs bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl shadow-2xs cursor-pointer">Lưu Giao Đơn Vị Trực Thuộc</button>
        </div>
      </div>
    </div>

    <!-- Progress Import Modal -->
    <ProgressImportModal
      :isOpen="isProgressImportModalOpen"
      :items="rawItemsList"
      :agencyId="userAgencyId"
      :userRole="userRoleStr"
      :currentAgency="currentUserAgencyObj"
      @close="isProgressImportModalOpen = false"
      @imported="handleProgressImported"
    />
    <!-- Pending Approvals System Modal for Admin (Cấp 1) -->
    <div v-if="isPendingApprovalsModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 font-sans">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-5xl w-full p-5 sm:p-6 space-y-4 max-h-[90vh] flex flex-col">
        
        <!-- Modal Header -->
        <div class="flex items-start justify-between border-b border-slate-100 pb-3 shrink-0">
          <div>
            <h3 class="text-base sm:text-lg font-bold text-slate-800 flex items-center gap-2">
              <span class="p-1.5 bg-amber-100 text-amber-800 rounded-xl text-sm">⏳</span>
              Danh Sách Báo Cáo Tiến Độ Chờ Phê Duyệt
            </h3>
          </div>
          <button @click="isPendingApprovalsModalOpen = false" class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-xl transition cursor-pointer">✕</button>
        </div>

        <!-- Modal Body -->
        <div class="flex-1 overflow-y-auto custom-scrollbar space-y-3.5 pr-1">
          <LoadingSpinner v-if="isLoadingPendingLogs" text="Đang tải danh sách báo cáo chờ duyệt..." padding="py-8" />

          <div v-else-if="!globalPendingLogs || globalPendingLogs.length === 0" class="p-10 text-center bg-slate-50 rounded-2xl border border-slate-200 text-slate-400 text-xs font-semibold italic">
            🎉 Không có báo cáo tiến độ nào đang chờ phê duyệt!
          </div>

          <div v-else class="space-y-3">
            <div 
              v-for="log in globalPendingLogs" 
              :key="log.id" 
              class="bg-white rounded-2xl border border-amber-200/90 p-4 shadow-2xs hover:shadow-md transition-all space-y-3"
            >
              <!-- Card Top Row: Agency Flow & Task Code -->
              <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-100 pb-2.5">
                <div class="min-w-0">
                  <div class="flex items-center gap-2 flex-wrap">
                    <span class="text-xs font-bold text-slate-900 bg-slate-100 px-2.5 py-1 rounded-lg border border-slate-200">
                      🏛️ {{ log.agencyName || 'Cơ quan gửi báo cáo' }}
                    </span>
                    <span v-if="log.parentAgencyName" class="text-xs text-slate-700 font-bold flex items-center gap-1">
                      ➔ Thuộc {{ log.parentAgencyName }}
                    </span>
                    <span v-if="log.isGeneralTask" class="px-2 py-0.5 rounded text-[10px] font-bold bg-purple-100 text-purple-800 border border-purple-200">
                      🌐 Nhiệm vụ chung
                    </span>
                  </div>
                  <h4 class="text-xs sm:text-sm font-normal text-slate-800 mt-1.5 leading-relaxed">
                    [{{ log.taskCode }}] {{ log.taskTitle }}
                  </h4>
                </div>

                <span class="text-[11px] font-semibold text-slate-400 shrink-0">
                  🕒 {{ formatDateTime(log.logDate) }}
                </span>
              </div>

              <!-- Card Body: Reported Progress, Notes, Files -->
              <div class="grid grid-cols-1 md:grid-cols-12 gap-4 text-xs font-semibold text-slate-700">
                <div class="md:col-span-4 bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-1">
                  <span class="text-[10px] font-bold text-slate-400 uppercase block">Tiến độ báo cáo</span>
                  <div class="text-sm font-bold text-blue-700">{{ log.completionPercentage || 0 }}% hoàn thành</div>
                  <div v-if="log.actualValue !== null && log.actualValue !== undefined" class="text-slate-600 text-[11px]">Giá trị thực tế: <strong>{{ log.actualValue }}</strong></div>
                  <div v-if="log.status" class="text-slate-600 text-[11px]">Trạng thái: <strong>{{ getStatusLabel(log.status) }}</strong></div>
                </div>

                <div class="md:col-span-8 bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-1.5">
                  <span class="text-[10px] font-bold text-slate-400 uppercase block">Diễn giải / File minh chứng</span>
                  <p v-if="log.summaryNotes" class="text-slate-800 text-xs font-normal leading-relaxed">{{ log.summaryNotes }}</p>
                  <span v-else class="text-slate-400 italic font-normal block text-[11px]">Không có ghi chú diễn giải.</span>

                  <div v-if="log.attachmentFileUrls?.length" class="pt-1 flex flex-wrap items-center gap-2">
                    <a v-for="(fileUrl, fIdx) in log.attachmentFileUrls" :key="fIdx" :href="getApiUrl(fileUrl)" target="_blank" class="px-2.5 py-1 bg-white hover:bg-blue-50 text-blue-700 font-bold text-[11px] rounded-lg border border-blue-200 shadow-2xs transition inline-flex items-center gap-1">
                      📎 {{ formatFileName(fileUrl) }}
                    </a>
                  </div>
                </div>
              </div>

              <!-- Card Actions: Approve / Reject buttons -->
              <div class="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
                <button 
                  @click="handleApproveFromGlobalList(log.id)" 
                  class="px-4 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-2xs transition cursor-pointer flex items-center gap-1"
                >
                  ✓ Phê Duyệt
                </button>
                <button 
                  @click="openRejectModalFromGlobalList(log.id)" 
                  class="px-4 py-1.5 bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs rounded-xl shadow-2xs transition cursor-pointer flex items-center gap-1"
                >
                  ✕ Từ Chối Phê Duyệt
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Rejection Reason Custom Modal Popup -->
    <div 
      v-if="isRejectModalOpen" 
      class="fixed inset-0 z-60 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 font-sans"
      @click.self="isRejectModalOpen = false"
    >
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-md w-full p-5 sm:p-6 space-y-4">
        <!-- Modal Header -->
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-bold text-rose-700 flex items-center gap-2">
            <span class="p-1.5 bg-rose-100 text-rose-700 rounded-xl text-sm">❌</span>
            Từ Chối Phê Duyệt Báo Cáo
          </h3>
          <button 
            @click="isRejectModalOpen = false" 
            class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-xl transition cursor-pointer"
          >
            ✕
          </button>
        </div>

        <!-- Modal Body -->
        <div class="space-y-2">
          <label class="block text-xs font-bold text-slate-700">
            Nhập lý do từ chối phê duyệt báo cáo tiến độ này: <span class="text-rose-500">*</span>
          </label>
          <textarea 
            v-model="rejectionReason" 
            rows="3" 
            placeholder="Nhập lý do cụ thể..." 
            class="w-full text-xs font-medium p-3 bg-slate-50 border border-slate-300 rounded-xl focus:ring-2 focus:ring-rose-500 focus:outline-none transition leading-relaxed"
          ></textarea>
        </div>

        <!-- Modal Actions -->
        <div class="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
          <button 
            @click="isRejectModalOpen = false" 
            class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer"
          >
            Hủy bỏ
          </button>
          <button 
            @click="confirmRejectFromGlobalList" 
            class="px-4 py-2 bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs rounded-xl shadow-2xs transition cursor-pointer flex items-center gap-1"
          >
            ✕ Xác Nhận Từ Chối
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { toast } from 'vue3-toastify';
import { confirmModal } from '../services/confirm';
import { authState } from '../services/auth';
import DynamicPlanningGrid from '../components/DynamicPlanningGrid.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import ProgressUpdateModal from '../components/ProgressUpdateModal.vue';
import ProgressImportModal from '../components/ProgressImportModal.vue';
import SendNotificationModal from '../components/SendNotificationModal.vue';
import ItemDetailModal from '../components/ItemDetailModal.vue';
import DatePicker from '../components/DatePicker.vue';
import OverlayPanel from '../components/OverlayPanel.vue';
import { getApiUrl } from '../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';
import { exportToExcel, exportProgressReportTemplate } from '../utils/excelExport';
import { parseApiError } from '../utils/errorUtils';

const props = defineProps({
  filterItemType: { type: String, default: 'Task' },
  subTab: { type: String, default: 'list' }
});

const isLoading = ref(false);
const planningGridRef = ref(null);

const activeSubTab = ref(props.subTab || 'list');

watch(() => [props.subTab, props.filterItemType], ([newSubTab, itemType]) => {
  if (itemType === 'Task') {
    activeSubTab.value = 'list';
  } else if (newSubTab) {
    activeSubTab.value = newSubTab;
  }
  if (activeSubTab.value === 'grid') {
    planningGridRef.value?.loadGridData();
  }
}, { immediate: true });

const userAgencyId = computed(() => authState.user.value?.agencyId || authState.user.value?.agency?.id || null);
const userRoleStr = computed(() => {
  const r = authState.user.value?.role;
  if (r === 'Admin' || r === 1 || r === '1' || authState.isAdmin.value) return 'Admin';
  if (r === 'Level2' || r === 2 || r === '2') return 'Level2';
  if (r === 'Level3' || r === 3 || r === '3') return 'Level3';
  return String(r || 'Level2');
});
const currentUserAgencyObj = computed(() => authState.user.value?.agency || null);

const isProgressModalOpen = ref(false);
const selectedTaskForProgress = ref(null);
const isProgressImportModalOpen = ref(false);

function handleExportProgressReport() {
  exportProgressReportTemplate({
    items: filteredList.value || [],
    currentAgency: currentUserAgencyObj.value,
    userRole: userRoleStr.value,
    fileName: `Bao_Cao_Tien_Do_${props.filterItemType === 'Goal' ? 'Muc_Tieu' : 'Nhiem_Vu'}`
  });
}

function handleProgressImported() {
  fetchDocumentData();
}

const isNotificationModalOpen = ref(false);
const selectedItemForNotification = ref(null);

const isItemDetailModalOpen = ref(false);
const selectedItemForDetail = ref(null);
const itemDetailModalInitialTab = ref('info');

const agencies = ref([]);
const rawItemsList = ref([]);

const filterDraft = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedSubAgencyIds: [],
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
  if (filterDraft.value.selectedSubAgencyIds?.length) count++;
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
  selectedSubAgencyIds: [],
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
    selectedSubAgencyIds: [],
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
  assignedAgencyId: '',
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
  assignedAgencyId: '',
  coordinatingAgencyIds: [],
  deliverables: []
});

watch(() => createForm.value.isOngoing, (val) => {
  if (val) {
    createForm.value.startYear = 2026;
    createForm.value.dueYear = 2030;
    createForm.value.startDate = '2026-01-01';
    createForm.value.dueDate = '2030-12-31';
  }
});

watch(() => editForm.value.isOngoing, (val) => {
  if (val) {
    editForm.value.startYear = 2026;
    editForm.value.dueYear = 2030;
    editForm.value.startDate = '2026-01-01';
    editForm.value.dueDate = '2030-12-31';
  }
});

function addDeliverable() {
  if (!createForm.value.deliverables) {
    createForm.value.deliverables = [];
  }
  const defaultDueDate = createForm.value.dueDate || '2030-12-31';
  createForm.value.deliverables.push({
    title: '',
    dueDate: defaultDueDate,
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

const goalUnitOptions = ref([
  { value: '%', label: '% (Phần trăm)' },
  { value: 'Số lượng', label: 'Số lượng (Số nguyên / Thập phân)' }
]);

const sectionFilterOptions = computed(() => {
  return props.filterItemType === 'Goal' ? GOAL_SECTIONS : TASK_SECTIONS;
});

const groupFilterOptions = computed(() => {
  return props.filterItemType === 'Goal' ? GOAL_GROUPS : TASK_GROUPS;
});

const agencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (isSpecialAgencyCode(ag.code)) {
      return { value: ag.id, label: `🌐 ${ag.name}` };
    }
    if (ag.parentId) {
      const parentAg = agencies.value.find(p => p.id === ag.parentId);
      return { value: ag.id, label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name };
    }
    return { value: ag.id, label: ag.name };
  });
});

const isSpecialAgencyCode = (code) => code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND';

const leadAgencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (isSpecialAgencyCode(ag.code)) {
      return { value: ag.id, label: `🌐 ${ag.name}` };
    }
    if (ag.parentId) {
      const parentAg = agencies.value.find(p => p.id === ag.parentId);
      return { value: ag.id, label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name };
    }
    return { value: ag.id, label: ag.name };
  });
});

const subAgencyOptions = computed(() => {
  return agencies.value
    .filter(ag => ag.parentId && ag.parentId !== '' && String(ag.parentId) !== '00000000-0000-0000-0000-000000000000')
    .map(ag => {
      const parentAg = agencies.value.find(p => p.id === ag.parentId);
      return {
        value: ag.id,
        label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name
      };
    });
});

const createAssignedAgencyOptions = computed(() => {
  if (!createForm.value.leadAgencyId) return [];
  return agencies.value
    .filter(ag => ag.parentId === createForm.value.leadAgencyId)
    .map(ag => ({ value: ag.id, label: ag.name }));
});

const editAssignedAgencyOptions = computed(() => {
  if (!editForm.value.leadAgencyId) return [];
  return agencies.value
    .filter(ag => ag.parentId === editForm.value.leadAgencyId)
    .map(ag => ({ value: ag.id, label: ag.name }));
});

const coordinatingAgencyOptions = computed(() => {
  return agencies.value.map(ag => {
    if (isSpecialAgencyCode(ag.code)) {
      return { value: ag.id, label: `🌐 ${ag.name}` };
    }
    if (ag.parentId) {
      const parentAg = agencies.value.find(p => p.id === ag.parentId);
      return { value: ag.id, label: parentAg ? `${ag.name} (Trực thuộc ${parentAg.name})` : ag.name };
    }
    return { value: ag.id, label: ag.name };
  });
});

watch(() => createForm.value.leadAgencyId, (newId) => {
  const selected = agencies.value.find(a => a.id === newId);
  if (selected && isSpecialAgencyCode(selected.code)) {
    createForm.value.isGeneralTask = true;
  } else {
    createForm.value.isGeneralTask = false;
  }
  if (createForm.value.assignedAgencyId) {
    const isChild = agencies.value.some(a => a.id === createForm.value.assignedAgencyId && a.parentId === newId);
    if (!isChild) createForm.value.assignedAgencyId = '';
  }
}, { immediate: true });

watch(() => editForm.value.leadAgencyId, (newId) => {
  if (editForm.value.assignedAgencyId) {
    const isChild = agencies.value.some(a => a.id === editForm.value.assignedAgencyId && a.parentId === newId);
    if (!isChild) editForm.value.assignedAgencyId = '';
  }
});

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

function formatDate(dateInput) {
  if (!dateInput) return '—';
  try {
    let str = String(dateInput).trim();
    if (!str) return '—';
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
      const [y, m, d] = str.slice(0, 10).split('-');
      return `${d}/${m}/${y}`;
    }
    const dateObj = new Date(str);
    if (isNaN(dateObj.getTime())) return str;
    const day = String(dateObj.getUTCDate()).padStart(2, '0');
    const month = String(dateObj.getUTCMonth() + 1).padStart(2, '0');
    const year = dateObj.getUTCFullYear();
    return `${day}/${month}/${year}`;
  } catch {
    return String(dateInput);
  }
}

function formatDateTime(dateInput) {
  if (!dateInput) return '—';
  try {
    let str = String(dateInput).trim();
    if (!str) return '—';
    if (str.includes('T') && !str.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(str)) {
      str += 'Z';
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return String(dateInput);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes} ${day}/${month}/${year}`;
  } catch {
    return String(dateInput);
  }
}

function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? formatDate(sDate) : '...';
  const d = dDate ? formatDate(dDate) : '...';
  return `${s} ➔ ${d}`;
}

function formatFileName(fullPath) {
  if (!fullPath) return 'Tệp đính kèm';
  const cleanPath = fullPath.split('?')[0];
  const parts = cleanPath.split(/[/\\]/);
  const name = parts[parts.length - 1];
  const guidMatch = name.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}_(.+)$/);
  if (guidMatch && guidMatch[1]) {
    return guidMatch[1];
  }
  return name || 'Tệp đính kèm';
}

function getStatusLabel(st) {
  if (!st) return 'Chưa thực hiện';
  const map = {
    'NotStarted': 'Chưa thực hiện',
    'InProgress': 'Đang thực hiện',
    'InProgressOnTime': 'Đang thực hiện (trong hạn)',
    'InProgressOverdue': 'Đang thực hiện (quá hạn)',
    'Completed': 'Hoàn thành',
    'CompletedOnTime': 'Hoàn thành (đúng hạn)',
    'CompletedOverdue': 'Hoàn thành (quá hạn)',
    'ExpiringSoon': 'Sắp hết hạn',
    'Drafting': 'Đang xây dựng / soạn thảo',
    'Reviewing': 'Đang thẩm định / xin ý kiến',
    'PendingApproval': 'Chờ phê duyệt',
    'Approved': 'Đã phê duyệt',
    'Rejected': 'Bị từ chối'
  };
  return map[st] || st;
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
  
  if (Array.isArray(item.deliverables) && item.deliverables.length > 0) {
    let totalProgress = 0;
    item.deliverables.forEach(d => {
      const st = d.currentStatus || d.CurrentStatus || 'NotStarted';
      if (st === 'Completed' || st === '4') {
        totalProgress += 100;
      } else if (st === 'Reviewing' || st === '3') {
        totalProgress += 60;
      } else if (st === 'Drafting' || st === '2') {
        totalProgress += 25;
      }
    });
    const avgPct = Math.round(totalProgress / item.deliverables.length);
    if (avgPct > 0) {
      return `${avgPct}%`;
    }
  }

  if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    const unit = item.unitName || item.unit?.name || '%';
    if (unit === 'Số lượng') {
      return `${item.latestProgressValue}`;
    }
    return `${item.latestProgressValue} ${unit}`;
  }

  if (item.evaluationType === 'Qualitative' || item.evaluationType === 2 || item.evaluationType === '2') {
    if (item.latestProgressStatus) {
      const statusMap = {
        'Completed': 'Đã hoàn thành / Ban hành',
        '4': 'Đã hoàn thành / Ban hành',
        'Reviewing': 'Đang xin ý kiến / Thẩm định',
        '3': 'Đang xin ý kiến / Thẩm định',
        'Submitted': 'Đang xin ý kiến / Thẩm định',
        'Drafting': 'Đang xây dựng / Soạn thảo',
        '2': 'Đang xây dựng / Soạn thảo',
        'NotStarted': 'Chưa thực hiện',
        '1': 'Chưa thực hiện'
      };
      return statusMap[item.latestProgressStatus] || item.latestProgressStatus;
    }
    return 'Chưa cập nhật';
  }

  if (item.latestProgressStatus) {
    return item.latestProgressStatus;
  }

  return 'Chưa cập nhật';
}

function isGeneralTaskOrAllAgencies(item) {
  if (!item) return false;
  if (item.isGeneralTask) return true;
  const code = (item.leadAgencyCode || '').toUpperCase();
  if (code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND') return true;
  if (item.leadAgencyId && ['00000000-0000-0000-0000-000000009999', '00000000-0000-0000-0000-000000009998', '00000000-0000-0000-0000-000000009997', '00000000-0000-0000-0000-000000009996'].includes(String(item.leadAgencyId).toLowerCase())) return true;
  if (item.leadAgencyName && (item.leadAgencyName.toLowerCase().includes('các bộ, ngành') || item.leadAgencyName.toLowerCase().includes('các địa phương') || item.leadAgencyName.toLowerCase().includes('ubnd tỉnh, thành phố'))) return true;
  return false;
}

function canCreateSubTask(item) {
  // Temporarily disabled per user request
  return false;
}

function canUpdateProgress(item) {
  if (!item) return false;
  // Tài khoản Quản trị viên (Admin) KHÔNG hiển thị nút Cập nhật tiến độ
  if (authState.isAdmin.value) return false;
  const userAgencyId = authState.user.value?.agencyId ? String(authState.user.value.agencyId).toLowerCase() : '';
  if (!userAgencyId) return false;

  const userAgency = agencies.value.find(a => String(a.id).toLowerCase() === userAgencyId);
  const isParentAgency = !userAgency || !userAgency.parentId;

  // Trường hợp đặc biệt: Cơ quan chủ trì là "Các bộ, ngành, địa phương" (Nhiệm vụ chung)
  // thì chỉ áp dụng cho đơn vị cha (ParentId == null)
  if (isParentAgency && isGeneralTaskOrAllAgencies(item)) return true;

  const isLead = item.leadAgencyId && String(item.leadAgencyId).toLowerCase() === userAgencyId;
  const isAssigned = item.assignedAgencyId && String(item.assignedAgencyId).toLowerCase() === userAgencyId;
  return isLead || isAssigned;
}

function isCoordinatingOnly(item) {
  if (!item) return false;
  if (authState.isAdmin.value) return false;
  const userAgencyId = authState.user.value?.agencyId ? String(authState.user.value.agencyId).toLowerCase() : '';
  if (!userAgencyId) return false;

  const userAgency = agencies.value.find(a => String(a.id).toLowerCase() === userAgencyId);
  const isParentAgency = !userAgency || !userAgency.parentId;

  // Nếu là nhiệm vụ chung và là đơn vị cha -> Có quyền cập nhật
  if (isParentAgency && isGeneralTaskOrAllAgencies(item)) return false;

  const isLead = item.leadAgencyId && String(item.leadAgencyId).toLowerCase() === userAgencyId;
  const isAssigned = item.assignedAgencyId && String(item.assignedAgencyId).toLowerCase() === userAgencyId;
  const isCoord = item.coordinatingAgencyIds && item.coordinatingAgencyIds.some(id => String(id).toLowerCase() === userAgencyId);
  return !isLead && !isAssigned && isCoord;
}

const filteredList = computed(() => {
  let list = rawItemsList.value;

  // Filter for Non-Admin Focal Point Accounts:
  // - Sub-Agency (ParentId != null): ONLY show items assigned to this Sub-Agency (cannot view parent agency data)
  // - Parent Agency (ParentId == null): Show items assigned to Parent Agency AND all of its Sub-Agencies (views all)
  if (!authState.isAdmin.value && authState.user.value?.agencyId) {
    const userAgencyId = String(authState.user.value.agencyId).toLowerCase();
    const userAgency = agencies.value.find(a => String(a.id).toLowerCase() === userAgencyId);
    const isParentAgency = !userAgency || !userAgency.parentId;

    const scopedAgencyIds = [userAgencyId];
    if (userAgency && !userAgency.parentId) {
      const childIds = agencies.value
        .filter(a => a.parentId && String(a.parentId).toLowerCase() === userAgencyId)
        .map(a => String(a.id).toLowerCase());
      scopedAgencyIds.push(...childIds);
    }

    list = list.filter(i => {
      const itemLeadId = i.leadAgencyId ? String(i.leadAgencyId).toLowerCase() : '';
      const itemAssignedId = i.assignedAgencyId ? String(i.assignedAgencyId).toLowerCase() : '';
      const itemCoordIds = (i.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());

      const isGeneral = isParentAgency && isGeneralTaskOrAllAgencies(i);
      const isLead = scopedAgencyIds.includes(itemLeadId);
      const isAssigned = itemAssignedId && scopedAgencyIds.includes(itemAssignedId);
      const isCoord = itemCoordIds.some(id => scopedAgencyIds.includes(id));
      const isSubMatch = i.subItems?.some(s => {
        const sLeadId = s.leadAgencyId ? String(s.leadAgencyId).toLowerCase() : '';
        const sAssignedId = s.assignedAgencyId ? String(s.assignedAgencyId).toLowerCase() : '';
        const sCoordIds = (s.coordinatingAgencyIds || []).map(id => String(id).toLowerCase());
        const sIsGeneral = isParentAgency && isGeneralTaskOrAllAgencies(s);
        return sIsGeneral || scopedAgencyIds.includes(sLeadId) || (sAssignedId && scopedAgencyIds.includes(sAssignedId)) || sCoordIds.some(id => scopedAgencyIds.includes(id));
      });

      return isGeneral || isLead || isAssigned || isCoord || isSubMatch;
    });
  }

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

  // Subordinate Agency Filter (Multi-select)
  if (appliedFilters.value.selectedSubAgencyIds && appliedFilters.value.selectedSubAgencyIds.length > 0) {
    list = list.filter(i => appliedFilters.value.selectedSubAgencyIds.includes(i.assignedAgencyId));
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

  // Sort by Most Recently Updated / Newly Created First
  return list.slice().sort((a, b) => {
    const getItemTime = (item) => {
      let t1 = item.lastUpdated ? new Date(item.lastUpdated).getTime() : 0;
      let t2 = item.createdAt ? new Date(item.createdAt).getTime() : 0;
      let maxT = Math.max(t1, t2);
      if (item.subItems && item.subItems.length > 0) {
        item.subItems.forEach(s => {
          let st1 = s.lastUpdated ? new Date(s.lastUpdated).getTime() : 0;
          let st2 = s.createdAt ? new Date(s.createdAt).getTime() : 0;
          maxT = Math.max(maxT, st1, st2);
        });
      }
      return maxT;
    };

    const timeA = getItemTime(a);
    const timeB = getItemTime(b);

    if (timeA !== timeB) {
      return timeB - timeA; // Descending (latest first)
    }

    return (a.code || '').localeCompare(b.code || '', undefined, { numeric: true, sensitivity: 'base' });
  });
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
    "Giao Đơn Vị Trực Thuộc",
    "Cơ Quan Phối Hợp",
    "Thời Gian Thực Hiện",
    "Tiến Độ Hiện Tại",
    "Trạng Thái"
  ];

  const minColWidths = {
    0: 8,
    1: 15,
    2: 50,
    3: 30,
    4: 28,
    5: 25,
    6: 22,
    7: 18,
    8: 22
  };

  const rows = filteredList.value.map((item, idx) => {
    let dateStr = 'Thường xuyên';
    if (!item.isOngoing) {
      dateStr = formatDateRange(item.startDate, item.dueDate);
    }
    const isGeneral = authState.isAdmin.value && isGeneralTaskOrAllAgencies(item);
    const progStr = isGeneral ? '—' : formatProgressDisplay(item);
    const statusStr = isGeneral ? '—' : getStatusLabel(item.calculatedStatus);

    return [
      idx + 1,
      item.code || '',
      item.title || '',
      item.leadAgencyName || '',
      item.assignedAgencyName || '—',
      item.cooperatingAgencies || '',
      dateStr,
      progStr,
      statusStr
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
    const currentAgencyId = authState.user.value?.agencyId || '';
    const agencyParam = currentAgencyId ? `?agencyId=${currentAgencyId}` : '';
    const [gridRes, agRes] = await Promise.all([
      fetch(getApiUrl(`/api/planning/documents/${docId}/grid${agencyParam}`)),
      fetch(getApiUrl('/api/agencies'))
    ]);

    if (agRes.ok) {
      const agData = await agRes.json();
      agencies.value = Array.isArray(agData) ? agData : (agData.items || []);
    }

    if (gridRes.ok) {
      const data = await gridRes.json();
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
    unitName: '%',
    isOngoing: false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: '',
    dueDate: '',
    leadAgencyId: agencies.value[0]?.id || '',
    assignedAgencyId: '',
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
    assignedAgencyId: '',
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

  if (createItemType.value === 'Goal' && !createForm.value.section) {
    createErrorMessage.value = 'Vui lòng chọn Mục (Phụ lục I).';
    toast.error(createErrorMessage.value);
    return;
  }

  if (createItemType.value === 'Goal' && !createForm.value.unitName) {
    createErrorMessage.value = 'Vui lòng chọn Đơn vị tính.';
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

    if (createForm.value.isOngoing) {
      startDateIso = new Date('2026-01-01T00:00:00.000Z').toISOString();
      dueDateIso = new Date('2030-12-31T23:59:59.000Z').toISOString();
    } else if (parentTaskForSubTask.value) {
      startDateIso = createForm.value.startDate ? new Date(createForm.value.startDate).toISOString() : null;
      dueDateIso = createForm.value.dueDate ? new Date(createForm.value.dueDate).toISOString() : null;
    } else {
      const sYear = createForm.value.startYear || 2026;
      const dYear = createForm.value.dueYear || 2030;
      startDateIso = new Date(`${sYear}-01-01T00:00:00.000Z`).toISOString();
      dueDateIso = new Date(`${dYear}-12-31T12:00:00.000Z`).toISOString();
    }

    const payload = {
      documentId: docId,
      parentId: parentTaskForSubTask.value ? parentTaskForSubTask.value.taskId : null,
      itemType: createItemType.value,
      code: '',
      title: createForm.value.title,
      section: createItemType.value === 'Goal' ? createForm.value.section : '',
      group: createForm.value.group,
      unitName: createForm.value.unitName || '%',
      isOngoing: createForm.value.isOngoing,
      isGeneralTask: createForm.value.isGeneralTask,
      startDate: startDateIso,
      dueDate: dueDateIso,
      leadAgencyId: createForm.value.leadAgencyId,
      assignedAgencyId: createForm.value.assignedAgencyId || null,
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
      planningGridRef.value?.loadGridData();
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
  if (item && item.hasPendingApproval) {
    toast.warning('Nhiệm vụ này đang ở trạng thái Chờ duyệt. Vui lòng chờ Cấp 2 phê duyệt hoặc từ chối trước khi gửi báo cáo mới.');
    return;
  }
  selectedTaskForProgress.value = item;
  isProgressModalOpen.value = true;
}

const isAssignModalOpen = ref(false);
const assignItemTarget = ref(null);
const selectedSubAgencyId = ref('');
const assignSubAgencyOptions = computed(() => {
  if (!assignItemTarget.value) return [];
  const targetLeadId = assignItemTarget.value.leadAgencyId;
  const userAgencyId = authState.user.value?.agencyId || authState.user.value?.agency?.id || null;
  const isGeneral = assignItemTarget.value.isGeneralTask || assignItemTarget.value.leadAgencyCode === 'ALL_AGENCIES' || targetLeadId === '00000000-0000-0000-0000-000000009999';

  if (isGeneral) {
    if (authState.isAdmin.value) {
      return agencies.value.filter(a => a.parentId != null && a.type !== 4 && a.type !== 'Other');
    }
    if (userAgencyId) {
      return agencies.value.filter(a => String(a.parentId).toLowerCase() === String(userAgencyId).toLowerCase() && a.type !== 4 && a.type !== 'Other');
    }
    return agencies.value.filter(a => a.parentId != null && a.type !== 4 && a.type !== 'Other');
  }

  if (targetLeadId) {
    const list = agencies.value.filter(a => a.parentId && String(a.parentId).toLowerCase() === String(targetLeadId).toLowerCase() && a.type !== 4 && a.type !== 'Other');
    if (list.length > 0) return list;
  }

  if (userAgencyId) {
    const list = agencies.value.filter(a => a.parentId && String(a.parentId).toLowerCase() === String(userAgencyId).toLowerCase() && a.type !== 4 && a.type !== 'Other');
    if (list.length > 0) return list;
  }

  return agencies.value.filter(a => a.parentId != null && a.type !== 4 && a.type !== 'Other');
});

const isBKHCNAgency = computed(() => {
  const userAgencyId = authState.user.value?.agencyId ? String(authState.user.value.agencyId).toLowerCase() : '';
  const userAgency = agencies.value.find(a => String(a.id).toLowerCase() === userAgencyId);
  const agName = (userAgency?.name || authState.user.value?.agencyName || '').toLowerCase();
  const agCode = (userAgency?.code || authState.user.value?.agencyCode || '').toLowerCase();
  return agCode === 'bkhcn' || agName.includes('khoa học và công nghệ') || agName.includes('khoa học & công nghệ') || agName.includes('khoa học công nghệ');
});

const isLeadAgencyFilteredByBKHCN = computed(() => {
  const selectedIds = appliedFilters.value.selectedAgencyIds || [];
  if (selectedIds.length === 0) return false;

  return selectedIds.some(id => {
    const ag = agencies.value.find(a => a.id === id);
    if (!ag) return false;
    const code = (ag.code || '').toLowerCase();
    const name = (ag.name || '').toLowerCase();
    return code === 'bkhcn' || name.includes('khoa học và công nghệ') || name.includes('khoa học & công nghệ') || name.includes('khoa học công nghệ');
  });
});

function isItemLeadByBKHCN(item) {
  if (!item) return false;
  const name = (item.leadAgencyName || '').toLowerCase();
  const code = (item.leadAgencyCode || '').toLowerCase();
  return code === 'bkhcn' || name.includes('khoa học và công nghệ') || name.includes('khoa học & công nghệ') || name.includes('khoa học công nghệ');
}

function canAssignTask(item) {
  if (!item) return false;

  // Nút giao đơn vị trực thuộc chỉ hiển thị khi cơ quan chủ trì là Bộ Khoa học và Công nghệ
  if (!isItemLeadByBKHCN(item)) return false;

  if (authState.isAdmin.value) return true;

  // Cấp 2: Nếu không phải Admin, user đăng nhập phải thuộc Bộ Khoa học và Công nghệ
  if (!isBKHCNAgency.value) return false;

  const userAgencyId = authState.user.value?.agencyId || authState.user.value?.agency?.id;
  if (userAgencyId && (String(userAgencyId).toLowerCase() === String(item.leadAgencyId).toLowerCase() || item.isGeneralTask || item.leadAgencyCode === 'ALL_AGENCIES')) {
    return true;
  }
  return false;
}

function openAssignModalFromList(item) {
  assignItemTarget.value = item;
  selectedSubAgencyId.value = item.assignedAgencyId || '';
  isAssignModalOpen.value = true;
}

async function submitAssignTaskFromList() {
  if (!assignItemTarget.value) return;
  try {
    const targetId = assignItemTarget.value.taskId || assignItemTarget.value.id;
    const res = await fetch(getApiUrl(`/api/planning/items/${targetId}/assign`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ assignedAgencyId: selectedSubAgencyId.value || null })
    });
    if (res.ok) {
      toast.success('Cập nhật giao đơn vị trực thuộc thành công!');
      const selectedSub = agencies.value.find(a => a.id === selectedSubAgencyId.value);
      assignItemTarget.value.assignedAgencyId = selectedSubAgencyId.value || null;
      assignItemTarget.value.assignedAgencyName = selectedSub ? selectedSub.name : null;
      isAssignModalOpen.value = false;
      
      try {
        await fetchDocumentData();
        if (activeSubTab.value === 'grid' && planningGridRef.value?.loadGridData) {
          await planningGridRef.value.loadGridData();
        }
      } catch (refreshErr) {
        console.warn('Không thể làm mới danh sách sau khi giao nhiệm vụ:', refreshErr);
      }
    } else {
      const errData = await res.json().catch(() => ({}));
      toast.error(errData.error || 'Lỗi khi cập nhật giao đơn vị trực thuộc.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

function openNotificationModal(item) {
  selectedItemForNotification.value = item;
  isNotificationModalOpen.value = true;
}

function openItemDetailModal(item, initialTab = 'info') {
  selectedItemForDetail.value = item;
  itemDetailModalInitialTab.value = initialTab;
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
  if (!authState.isAdmin.value) {
    toast.error("Chỉ có tài khoản Quản trị viên (Admin) mới có quyền xóa Mục tiêu / Nhiệm vụ.");
    return;
  }

  if (hasProgress(item)) {
    toast.warning("Chỉ được phép xóa Mục tiêu / Nhiệm vụ khi ở trạng thái Chưa bắt đầu.");
    return;
  }

  const isSubTask = !!item.parentItemId || !!item.parentId;
  const itemTypeLabel = isSubTask ? 'Sub-Task' : (props.filterItemType === 'Goal' ? 'mục tiêu' : 'nhiệm vụ');
  const codeStr = item.code ? ` mã ${item.code}` : '';

  const confirmed = await confirmModal({
    title: 'Xóa dữ liệu',
    message: 'Chắc chắn xóa dữ liệu này?',
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
      toast.success('Đã xóa dữ liệu thành công!');
      await loadData();
    } else {
      toast.error('Không thể xóa dữ liệu này.');
    }
  } catch (e) {
    toast.error('Không thể xóa dữ liệu này.');
  }
}

function openEditModal(item) {
  if (!item) return;

  if (!authState.isAdmin.value) {
    toast.error("Chỉ có tài khoản Quản trị viên (Admin) mới có quyền chỉnh sửa Mục tiêu / Nhiệm vụ.");
    return;
  }

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
    unitName: item.unitName || '%',
    isOngoing: !!item.isOngoing,
    isGeneralTask: !!item.isGeneralTask,
    startYear: sYear,
    dueYear: dYear,
    startDate: sDate,
    dueDate: dDate,
    leadAgencyId: item.leadAgencyId || agencies.value[0]?.id || '',
    assignedAgencyId: item.assignedAgencyId || '',
    coordinatingAgencyIds: item.coordinatingAgencyIds ? [...item.coordinatingAgencyIds] : [],
    deliverables: item.deliverables ? JSON.parse(JSON.stringify(item.deliverables)) : []
  };

  isEditModalOpen.value = true;
}

function addEditDeliverable() {
  if (!editForm.value.deliverables) {
    editForm.value.deliverables = [];
  }
  const defaultDueDate = editForm.value.dueDate || '2030-12-31';
  editForm.value.deliverables.push({
    title: '',
    dueDate: defaultDueDate,
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

  const isGoal = editingItem.value?.itemType === 'Goal' || editingItem.value?.itemType === 1 || editingItem.value?.itemType === '1';

  if (isGoal && !editForm.value.section) {
    editErrorMessage.value = 'Vui lòng chọn Mục (Phụ lục I).';
    toast.error(editErrorMessage.value);
    return;
  }

  if (isGoal && !editForm.value.unitName) {
    editErrorMessage.value = 'Vui lòng chọn Đơn vị tính.';
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

    if (editForm.value.isOngoing) {
      startDateIso = new Date('2026-01-01T00:00:00.000Z').toISOString();
      dueDateIso = new Date('2030-12-31T23:59:59.000Z').toISOString();
    } else if (editingItem.value.parentId || editingItem.value.parentItemId || editForm.value.startDate || editForm.value.dueDate) {
      startDateIso = editForm.value.startDate ? new Date(editForm.value.startDate).toISOString() : null;
      dueDateIso = editForm.value.dueDate ? new Date(editForm.value.dueDate).toISOString() : null;
    } else {
      const sYear = editForm.value.startYear || 2026;
      const dYear = editForm.value.dueYear || 2030;
      startDateIso = new Date(`${sYear}-01-01T00:00:00.000Z`).toISOString();
      dueDateIso = new Date(`${dYear}-12-31T12:00:00.000Z`).toISOString();
    }

    const payload = {
      title: editForm.value.title,
      section: editForm.value.section,
      group: editForm.value.group,
      unitName: editForm.value.unitName || '%',
      isOngoing: editForm.value.isOngoing,
      isGeneralTask: editForm.value.isGeneralTask,
      startDate: startDateIso,
      dueDate: dueDateIso,
      leadAgencyId: editForm.value.leadAgencyId,
      assignedAgencyId: editForm.value.assignedAgencyId || null,
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
      planningGridRef.value?.loadGridData();
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

const globalPendingLogs = ref([]);
const isPendingApprovalsModalOpen = ref(false);
const isLoadingPendingLogs = ref(false);

const isRejectModalOpen = ref(false);
const rejectLogId = ref(null);
const rejectionReason = ref('');

async function loadGlobalPendingLogs() {
  if (!authState.isAdmin.value) return;
  isLoadingPendingLogs.value = true;
  try {
    const res = await fetch(getApiUrl('/api/execution/pending-approvals'));
    if (res.ok) {
      globalPendingLogs.value = await res.json();
    } else {
      globalPendingLogs.value = [];
    }
  } catch {
    globalPendingLogs.value = [];
  } finally {
    isLoadingPendingLogs.value = false;
  }
}

function openPendingApprovalsModal() {
  isPendingApprovalsModalOpen.value = true;
  loadGlobalPendingLogs();
}

async function handleApproveFromGlobalList(logId) {
  if (!logId) return;
  try {
    const res = await fetch(getApiUrl(`/api/execution/approve/${logId}`), { method: 'POST' });
    if (res.ok) {
      toast.success('Đã phê duyệt báo cáo tiến độ thành công!');
      try {
        await loadGlobalPendingLogs();
        if (typeof loadData === 'function') await loadData();
      } catch (e) {
        console.error('Error refreshing after approve:', e);
      }
    } else {
      toast.error('Phê duyệt thất bại.');
    }
  } catch (err) {
    console.error('Approve error:', err);
    toast.error('Lỗi khi phê duyệt báo cáo.');
  }
}

function openRejectModalFromGlobalList(logId) {
  if (!logId) return;
  rejectLogId.value = logId;
  rejectionReason.value = 'Chưa đạt yêu cầu';
  isRejectModalOpen.value = true;
}

async function confirmRejectFromGlobalList() {
  if (!rejectLogId.value) return;
  try {
    const res = await fetch(getApiUrl(`/api/execution/reject/${rejectLogId.value}`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason: rejectionReason.value || 'Chưa đạt yêu cầu' })
    });
    if (res.ok) {
      toast.info('Đã từ chối báo cáo tiến độ.');
      isRejectModalOpen.value = false;
      rejectLogId.value = null;
      try {
        await loadGlobalPendingLogs();
        if (typeof loadData === 'function') await loadData();
      } catch (e) {
        console.error('Error refreshing after reject:', e);
      }
    } else {
      toast.error('Từ chối thất bại.');
    }
  } catch (err) {
    console.error('Reject error:', err);
    toast.error('Lỗi khi từ chối báo cáo.');
  }
}

let globalPendingPollTimer = null;

function handlePendingLogsUpdate() {
  if (authState.isAdmin.value) {
    loadGlobalPendingLogs();
  }
}

function handleTargetItemDetailEvent(e) {
  if (e && e.detail && e.detail.item) {
    const item = e.detail.item;
    const initialTab = e.detail.initialTab || 'notifications';
    openItemDetailModal(item, initialTab);
  }
}

function handleOpenPendingApprovalsModalEvent() {
  if (authState.isAdmin.value) {
    loadGlobalPendingLogs();
    isPendingApprovalsModalOpen.value = true;
  }
}

onMounted(() => {
  loadData();
  if (authState.isAdmin.value) {
    loadGlobalPendingLogs();
    globalPendingPollTimer = setInterval(loadGlobalPendingLogs, 5000);
  }
  window.addEventListener('open-target-item-detail', handleTargetItemDetailEvent);
  window.addEventListener('open-pending-approvals-modal', handleOpenPendingApprovalsModalEvent);
  window.addEventListener('notification-sent', handlePendingLogsUpdate);
  window.addEventListener('progress-report-submitted', handlePendingLogsUpdate);
});

onUnmounted(() => {
  if (globalPendingPollTimer) {
    clearInterval(globalPendingPollTimer);
    globalPendingPollTimer = null;
  }
  window.removeEventListener('open-target-item-detail', handleTargetItemDetailEvent);
  window.removeEventListener('open-pending-approvals-modal', handleOpenPendingApprovalsModalEvent);
  window.removeEventListener('notification-sent', handlePendingLogsUpdate);
  window.removeEventListener('progress-report-submitted', handlePendingLogsUpdate);
});
</script>
