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

        <button 
          @click="openCreateModal(filterItemType || 'Task')"
          class="px-3.5 py-2 text-white font-bold text-xs rounded-xl bg-blue-600 hover:bg-blue-700 transition shadow-sm flex items-center gap-1.5"
        >
          + Thêm {{ filterItemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} Mới
        </button>
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
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 xl:grid-cols-7 gap-2 bg-slate-50/60 p-3 border-b border-slate-200/80 items-end w-full">
          <div>
            <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Từ Khóa</label>
            <input 
              v-model="filterDraft.searchQuery" 
              @keyup.enter="execFilterSearch"
              :placeholder="filterItemType === 'Goal' ? 'Mã, tên mục tiêu...' : 'Mã, tên nhiệm vụ...'" 
              class="w-full text-xs font-semibold px-2.5 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none min-h-[34px]"
            />
          </div>

          <div>
            <SearchableSelect 
              v-model="filterDraft.selectedAgencyIds" 
              :options="agencyOptions" 
              :isMulti="true" 
              label="Cơ Quan Chủ Trì" 
              placeholder="Tất cả cơ quan"
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

          <!-- Từ năm ➔ Đến năm -->
          <div>
            <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Giai Đoạn (Từ năm ➔ Đến)</label>
            <div class="flex items-center gap-1">
              <SearchableSelect 
                v-model="filterDraft.fromYear" 
                :options="yearOptions" 
                :isMulti="false" 
                placeholder="Từ năm" 
                class="w-full"
              />
              <span class="text-xs font-bold text-slate-400">➔</span>
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
              label="Trạng Thái" 
              placeholder="Tất cả trạng thái"
            />
          </div>

          <div class="flex items-center gap-1.5 col-span-1 xl:col-start-7 ml-auto w-full justify-end">
            <button 
              type="button" 
              @click="execFilterSearch" 
              class="w-full sm:w-auto px-5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm transition min-h-[34px] cursor-pointer whitespace-nowrap"
            >
              Tìm Kiếm
            </button>
            <button 
              type="button" 
              @click="resetFilterSearch" 
              class="px-3 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition min-h-[34px] cursor-pointer shrink-0"
              title="Đặt lại bộ lọc"
            >
              ↺
            </button>
          </div>
        </div>

        <!-- MAIN DATA TABLE WITH STICKY HEADER & FROZEN FIRST 3 COLUMNS -->
        <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu danh sách từ máy chủ..." />
        <div v-else class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
          <table class="w-full min-w-[1050px] text-left text-sm text-slate-700 border-collapse">
            <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-30 shadow-xs">
              <tr>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-30">Mã</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-30">
                  {{ filterItemType === 'Goal' ? 'Tên Mục Tiêu' : (filterItemType === 'Task' ? 'Tên Nhiệm Vụ' : 'Tên Mục Tiêu / Nhiệm Vụ') }}
                </th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[150px] w-[150px] max-w-[150px] sticky left-[355px] z-30 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">Cơ Quan Chủ Trì</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[160px] w-[160px] max-w-[160px]">Cơ Quan Phối Hợp</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[110px]">Phạm Vi</th>
                <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[150px]">Thời Gian thực hiện</th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[120px]">Tiến Độ</th>
                <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[180px]">Trạng Thái</th>
                <th class="px-3 py-2.5 text-center bg-slate-100 whitespace-nowrap min-w-[100px]">Thao Tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <template v-for="item in paginatedPrimaryList" :key="item.taskId">
                <!-- Parent Row -->
                <tr @click="openItemDetailModal(item)" class="group hover:bg-blue-50/50 cursor-pointer transition">
                  <td class="px-3 py-2.5 font-extrabold text-blue-700 border-r border-slate-200 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-white group-hover:bg-blue-50/50">
                    {{ item.code }}
                  </td>
                  <td class="px-3 py-2.5 font-semibold text-slate-900 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-20 bg-white group-hover:bg-blue-50/50">
                    {{ item.title }}
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-bold text-slate-800 text-xs leading-relaxed min-w-[150px] w-[150px] max-w-[150px] sticky left-[355px] z-20 bg-white group-hover:bg-blue-50/50 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                    {{ item.leadAgencyName }} ({{ item.leadAgencyCode }})
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
                  <td class="px-3 py-2.5 border-r border-slate-200 text-xs whitespace-nowrap">
                    <span :class="['px-2 py-0.5 rounded-full font-extrabold inline-block whitespace-nowrap text-[11px]', item.isGeneralTask ? 'bg-purple-100 text-purple-800' : 'bg-slate-100 text-slate-700']">
                      {{ item.isGeneralTask ? 'Nhiệm vụ chung' : 'Nhiệm vụ riêng' }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-xs font-semibold text-slate-600 whitespace-nowrap">
                    {{ formatDateRange(item.startDate, item.dueDate) }}
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
                  <td class="px-3 py-2.5 text-center space-x-1.5 whitespace-nowrap" @click.stop>
                    <button 
                      @click.stop="openCreateSubTaskModal(item)" 
                      class="p-1.5 bg-blue-50 text-blue-700 hover:bg-blue-100 rounded-lg transition border border-blue-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                      title="Thêm Nhiệm Vụ Con"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
                    </button>

                    <button 
                      @click.stop="openProgressModal(item)" 
                      class="p-1.5 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-lg transition border border-emerald-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                      title="Cập Nhật Tiến Độ & Minh Chứng"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                    </button>

                    <button 
                      @click.stop="openNotificationModal(item)" 
                      class="p-1.5 bg-amber-50 text-amber-700 hover:bg-amber-100 rounded-lg transition border border-amber-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                      title="Gửi Thông Báo Đến Đơn Vị Chủ Trì & Phối Hợp"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/></svg>
                    </button>
                  </td>
                </tr>

                <!-- Child Sub-task Rows -->
                <tr v-for="sub in item.subItems" :key="sub.taskId" @click="openItemDetailModal(sub)" class="group bg-slate-50/70 hover:bg-blue-50/50 cursor-pointer transition text-xs">
                  <td class="px-3 py-2 font-bold text-slate-600 border-r border-slate-200 pl-3 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-20 bg-slate-50 group-hover:bg-blue-50/50">
                    └─ {{ sub.code }}
                  </td>
                  <td class="px-3 py-2 font-medium text-slate-800 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-20 bg-slate-50 group-hover:bg-blue-50/50">
                    {{ sub.title }}
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-semibold text-slate-700 min-w-[150px] w-[150px] max-w-[150px] sticky left-[355px] z-20 bg-slate-50 group-hover:bg-blue-50/50 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
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
                  <td class="px-3 py-2 border-r border-slate-200 whitespace-nowrap">
                    <span class="text-[10px] font-bold text-slate-500 bg-slate-200/60 px-2 py-0.5 rounded">Sub-task</span>
                  </td>
                  <td class="px-3 py-2 border-r border-slate-200 font-semibold text-slate-600 whitespace-nowrap">
                    {{ formatDateRange(sub.startDate, sub.dueDate) }}
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
                  <td class="px-3 py-2 text-center space-x-1.5 whitespace-nowrap" @click.stop>
                    <button 
                      @click.stop="openProgressModal(sub)" 
                      class="p-1.5 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded-lg transition border border-emerald-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                      title="Cập Nhật Tiến Độ & Minh Chứng"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                    </button>

                    <button 
                      @click.stop="openNotificationModal(sub)" 
                      class="p-1.5 bg-amber-50 text-amber-700 hover:bg-amber-100 rounded-lg transition border border-amber-200 inline-flex items-center justify-center shadow-2xs cursor-pointer" 
                      title="Gửi Thông Báo Đến Đơn Vị Chủ Trì & Phối Hợp"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/></svg>
                    </button>
                  </td>
                </tr>
              </template>

              <tr v-if="paginatedPrimaryList.length === 0">
                <td colspan="9" class="p-8 text-center text-slate-400 font-semibold italic">
                  Không tìm thấy dữ liệu phù hợp.
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Attached Pagination Controls Bar -->
        <div class="flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/70 p-4 border-t border-slate-200/80 text-xs text-slate-600 font-semibold">
          <div class="flex flex-wrap items-center gap-3">
            <span>Hiển thị <span class="font-extrabold text-slate-900">{{ totalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} - {{ Math.min(currentPage * pageSize, totalCount) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ totalCount }}</span> {{ filterItemType === 'Goal' ? 'mục tiêu' : 'nhiệm vụ' }}</span>
            
            <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3">
              <span>Số bản ghi/trang:</span>
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

          <div class="flex items-center gap-2">
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

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Mã Số</label>
              <div class="w-full text-xs font-extrabold text-blue-800 bg-blue-50/60 border border-blue-200 rounded-xl px-2.5 py-1.5 flex items-center min-h-[34px] shadow-2xs">
                ⚡ Tự động sinh mã
              </div>
            </div>
            <div>
              <SearchableSelect 
                v-model="createForm.isGeneralTask" 
                :options="[
                  { value: false, label: 'Nhiệm vụ riêng (Đơn vị)' },
                  { value: true, label: 'Nhiệm vụ chung' }
                ]" 
                :isMulti="false" 
                label="Phạm Vi" 
                placeholder="-- Chọn phạm vi --"
              />
            </div>
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

          <!-- Date Range Inputs with Parent Constraint Info -->
          <div v-if="!parentTaskForSubTask" class="grid grid-cols-2 gap-3">
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
              <input v-model="createForm.startDate" type="date" class="w-full text-xs font-bold bg-white border border-blue-200 rounded-xl px-2.5 py-1.5 min-h-[34px]" />
            </div>
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase block mb-1">Ngày Hoàn Thành</label>
              <input v-model="createForm.dueDate" type="date" class="w-full text-xs font-bold bg-white border border-blue-200 rounded-xl px-2.5 py-1.5 min-h-[34px]" />
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
                :options="agencyOptions" 
                :isMulti="false" 
                label="Đơn Vị Chủ Trì" 
                placeholder="-- Chọn đơn vị chủ trì --"
              />
            </div>
            <div>
              <SearchableSelect 
                v-model="createForm.coordinatingAgencyIds" 
                :options="agencyOptions" 
                :isMulti="true" 
                label="Cơ Quan Phối Hợp" 
                placeholder="-- Chọn cơ quan phối hợp --"
              />
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

    <!-- Item Detail Modal -->
    <ItemDetailModal
      v-if="selectedItemForDetail"
      :is-open="isItemDetailModalOpen"
      :item="selectedItemForDetail"
      @close="isItemDetailModalOpen = false"
    />

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import DynamicPlanningGrid from '../components/DynamicPlanningGrid.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import ProgressUpdateModal from '../components/ProgressUpdateModal.vue';
import SendNotificationModal from '../components/SendNotificationModal.vue';
import ItemDetailModal from '../components/ItemDetailModal.vue';
import { getApiUrl } from '../config/api';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';

const props = defineProps({
  filterItemType: { type: String, default: 'Task' },
  subTab: { type: String, default: 'list' }
});

const isLoading = ref(false);

const activeSubTab = ref(props.subTab || 'list');

watch(() => props.subTab, (newVal) => {
  if (newVal) {
    activeSubTab.value = newVal;
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
  toYear: null
});

const appliedFilters = ref({
  searchQuery: '',
  selectedAgencyIds: [],
  selectedScopes: [],
  selectedStatuses: [],
  selectedSections: [],
  selectedGroups: [],
  fromYear: null,
  toYear: null
});

const currentPage = ref(1);
const pageSize = ref(10);

function execFilterSearch() {
  appliedFilters.value = JSON.parse(JSON.stringify(filterDraft.value));
  currentPage.value = 1;
}

function resetFilterSearch() {
  filterDraft.value = {
    searchQuery: '',
    selectedAgencyIds: [],
    selectedScopes: [],
    selectedStatuses: [],
    selectedSections: [],
    selectedGroups: [],
    fromYear: null,
    toYear: null
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
  coordinatingAgencyIds: []
});

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
  return agencies.value.map(ag => ({ value: ag.id, label: `${ag.code} - ${ag.name}` }));
});

const yearOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));
const pageSizeOptions = ref([10, 25, 50, 100].map(n => ({ value: n, label: String(n) })));

const scopeOptions = ref([
  { value: 'general', label: 'Nhiệm vụ chung' },
  { value: 'specific', label: 'Nhiệm vụ riêng' }
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
  if (appliedFilters.value.searchQuery && appliedFilters.value.searchQuery.trim()) {
    const q = appliedFilters.value.searchQuery.trim().toLowerCase();
    list = list.filter(i => i.code?.toLowerCase().includes(q) || i.title?.toLowerCase().includes(q));
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
    group: type === 'Goal' ? 'Nhóm I' : 'Nhóm I',
    isOngoing: false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: '',
    dueDate: '',
    leadAgencyId: agencies.value[0]?.id || '',
    coordinatingAgencyIds: []
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
    group: parentItem.group || 'Nhóm I',
    isOngoing: parentItem.isOngoing || false,
    isGeneralTask: false,
    startYear: 2026,
    dueYear: 2030,
    startDate: parentItem.startDate ? new Date(parentItem.startDate).toISOString().split('T')[0] : '',
    dueDate: parentItem.dueDate ? new Date(parentItem.dueDate).toISOString().split('T')[0] : '',
    leadAgencyId: parentItem.leadAgencyId || agencies.value[0]?.id || '',
    coordinatingAgencyIds: []
  };
  isCreateModalOpen.value = true;
}

async function submitCreateItem() {
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
      coordinatingAgencyIds: createForm.value.coordinatingAgencyIds || []
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
      toast.error(err.message || 'Lỗi khi thêm mới.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

function openProgressModal(item) {
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

onMounted(() => {
  loadData();
});
</script>
