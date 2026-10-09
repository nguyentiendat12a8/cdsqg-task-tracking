<template>
<div  class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
          <table class="w-full min-w-[1280px] text-left text-sm text-slate-700 border-collapse">
            <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-30 shadow-xs select-none">
              <tr>
                <th @click="handleSort('code')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[75px] w-[75px] max-w-[75px] sticky left-0 z-30 cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Mã">
                  <div class="flex items-center justify-between gap-1">
                    <span>Mã</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'code' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'code' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th @click="handleSort('title')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[280px] w-[280px] max-w-[280px] sticky left-[75px] z-30 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Tên">
                  <div class="flex items-center justify-between gap-1">
                    <span>{{ filterItemType === 'Goal' ? 'Tên Mục Tiêu' : (filterItemType === 'Task' ? 'Tên Nhiệm Vụ' : 'Tên Mục Tiêu / Nhiệm Vụ') }}</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'title' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'title' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th @click="handleSort('leadAgency')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Cơ quan chủ trì">
                  <div class="flex items-center justify-between gap-1">
                    <span>Cơ Quan Chủ Trì</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'leadAgency' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'leadAgency' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th v-if="isLeadAgencyFilteredByBKHCN" @click="handleSort('assignedAgency')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Đơn vị trực thuộc">
                  <div class="flex items-center justify-between gap-1">
                    <span>Giao Đơn Vị Trực Thuộc</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'assignedAgency' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'assignedAgency' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th @click="handleSort('coordinatingAgency')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Cơ quan phối hợp">
                  <div class="flex items-center justify-between gap-1">
                    <span>Cơ Quan Phối Hợp</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'coordinatingAgency' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'coordinatingAgency' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th @click="handleSort('period')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[160px] w-[160px] max-w-[160px] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Thời gian thực hiện">
                  <div class="flex items-center justify-between gap-1">
                    <span>Thời Gian thực hiện</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'period' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'period' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th v-if="filterItemType === 'Goal'" @click="handleSort('progress')" class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 min-w-[125px] w-[125px] max-w-[125px] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Tiến độ">
                  <div class="flex items-center justify-center gap-1">
                    <span>Tiến Độ</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'progress' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'progress' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th @click="handleSort('status')" class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 min-w-[195px] w-[195px] max-w-[195px] cursor-pointer hover:bg-slate-200 transition" title="Bấm để sắp xếp theo Trạng thái">
                  <div class="flex items-center justify-center gap-1">
                    <span>Trạng Thái</span>
                    <span class="text-[10px] font-bold text-slate-400">
                      <span v-if="sortBy === 'status' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'status' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
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
                  <td v-if="filterItemType === 'Goal'" class="px-3 py-2.5 border-r border-slate-200 text-center text-xs min-w-[125px] w-[125px] max-w-[125px] overflow-hidden">
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
                              class="ui-single-line w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Giao cho đơn vị trực thuộc"
                            >
                              <span>Giao đơn vị trực thuộc</span>
                            </button>

                            <button
                              v-if="authState.isAdmin.value && !hasProgress(item)"
                              @click="openEditModal(item); hide()"
                              class="ui-single-line w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Chỉnh Sửa"
                            >
                              <span>Chỉnh sửa</span>
                            </button>

                            <button
                              @click="openNotificationModal(item); hide()"
                              class="ui-single-line w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              title="Gửi thông báo đến đơn vị chủ trì & phối hợp"
                            >
                              <span>Gửi thông báo</span>
                            </button>

                            <div v-if="authState.isAdmin.value && !hasProgress(item)" class="border-t border-slate-100 my-1"></div>

                            <button
                              v-if="authState.isAdmin.value && !hasProgress(item)"
                              @click="handleDeleteItem(item); hide()"
                              class="ui-single-line w-full text-left px-3 py-2 text-rose-600 hover:bg-rose-50 transition cursor-pointer"
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
              </template>

              <tr v-if="paginatedPrimaryList.length === 0">
                <td :colspan="isLeadAgencyFilteredByBKHCN ? 9 : 8" class="p-8 text-center text-slate-400 font-semibold italic">
                  Không tìm thấy dữ liệu phù hợp.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
</template>

<script setup>

defineProps([
  'authState',
  'formatDateRange',
  'getStatusLabel',
  'getStatusBadgeClass',
  'formatProgressDisplay',
  'isGeneralTaskOrAllAgencies',
  'sortBy',
  'sortOrder',
  'handleSort',
  'paginatedPrimaryList',
  'canUpdateProgress',
  'isCoordinatingOnly',
  'openProgressModal',
  'isLeadAgencyFilteredByBKHCN',
  'canAssignTask',
  'openAssignModalFromList',
  'openNotificationModal',
  'openItemDetailModal',
  'hasProgress',
  'handleDeleteItem',
  'openEditModal',
  'filterItemType'
]);

</script>

