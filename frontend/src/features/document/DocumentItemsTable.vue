<template>
<div  class="document-items-scroll overflow-x-auto overflow-y-auto max-h-[calc(100vh-320px)] custom-scrollbar w-full">
          <table class="w-full min-w-[1280px] text-left text-sm text-slate-700 border-collapse">
            <thead class="bg-slate-100 text-xs text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-30 shadow-xs select-none">
              <tr>
                <th tabindex="0" :aria-sort="sortBy === 'code' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('code')" @keydown.space.prevent="handleSort('code')" @click="handleSort('code')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[112px] w-[112px] max-w-[112px] sticky left-0 z-30 cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Mã'">
                  <div class="flex items-center justify-between gap-1">
                    <span>Mã</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'code' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'code' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th tabindex="0" :aria-sort="sortBy === 'title' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('title')" @keydown.space.prevent="handleSort('title')" @click="handleSort('title')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[280px] w-[280px] max-w-[280px] sticky left-[112px] z-30 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Tên'">
                  <div class="flex items-center justify-between gap-1">
                    <span>{{ filterItemType === 'Goal' ? 'Tên mục tiêu' : (filterItemType === 'Task' ? 'Tên nhiệm vụ' : 'Tên mục tiêu / nhiệm vụ') }}</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'title' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'title' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th tabindex="0" :aria-sort="sortBy === 'leadAgency' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('leadAgency')" @keydown.space.prevent="handleSort('leadAgency')" @click="handleSort('leadAgency')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Cơ quan chủ trì'">
                  <div class="flex items-center justify-between gap-1">
                    <span>Cơ quan chủ trì</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'leadAgency' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'leadAgency' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th v-if="isLeadAgencyFilteredByBKHCN" tabindex="0" :aria-sort="sortBy === 'assignedAgency' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('assignedAgency')" @keydown.space.prevent="handleSort('assignedAgency')" @click="handleSort('assignedAgency')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Đơn vị trực thuộc'">
                  <div class="flex items-center justify-between gap-1">
                    <span>Giao đơn vị trực thuộc</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'assignedAgency' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'assignedAgency' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th tabindex="0" :aria-sort="sortBy === 'coordinatingAgency' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('coordinatingAgency')" @keydown.space.prevent="handleSort('coordinatingAgency')" @click="handleSort('coordinatingAgency')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[180px] w-[180px] max-w-[180px] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Cơ quan phối hợp'">
                  <div class="flex items-center justify-between gap-1">
                    <span>Cơ quan phối hợp</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'coordinatingAgency' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'coordinatingAgency' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th tabindex="0" :aria-sort="sortBy === 'period' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('period')" @keydown.space.prevent="handleSort('period')" @click="handleSort('period')" class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[160px] w-[160px] max-w-[160px] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Thời gian thực hiện'">
                  <div class="flex items-center justify-between gap-1">
                    <span>Thời gian thực hiện</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'period' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'period' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th v-if="filterItemType === 'Goal'" tabindex="0" :aria-sort="sortBy === 'progress' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('progress')" @keydown.space.prevent="handleSort('progress')" @click="handleSort('progress')" class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 min-w-[125px] w-[125px] max-w-[125px] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Tiến độ'">
                  <div class="flex items-center justify-center gap-1">
                    <span>Hoàn thành chỉ tiêu</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'progress' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'progress' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th tabindex="0" :aria-sort="sortBy === 'status' ? (sortOrder === 'asc' ? 'ascending' : 'descending') : 'none'" @keydown.enter.prevent="handleSort('status')" @keydown.space.prevent="handleSort('status')" @click="handleSort('status')" class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 min-w-[195px] w-[195px] max-w-[195px] cursor-pointer hover:bg-slate-200 transition" v-tip="'Bấm để sắp xếp theo Trạng thái'">
                  <div class="flex items-center justify-center gap-1">
                    <span>Trạng thái</span>
                    <span class="text-xs font-bold text-slate-400">
                      <span v-if="sortBy === 'status' && sortOrder === 'asc'" class="text-blue-600">▲</span>
                      <span v-else-if="sortBy === 'status' && sortOrder === 'desc'" class="text-blue-600">▼</span>
                      <span v-else class="text-slate-300">↕</span>
                    </span>
                  </div>
                </th>
                <th class="px-3 py-2.5 text-center bg-slate-100 min-w-[90px] w-[90px] max-w-[90px]">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <template v-for="item in paginatedPrimaryList" :key="item.taskId">
                <!-- Parent Row -->
                <tr @click="openItemDetailModal(item)" class="group hover:bg-blue-100/90 cursor-pointer">
                  <td class="px-3 py-2.5 font-normal text-slate-700 border-r border-slate-200 whitespace-nowrap min-w-[112px] w-[112px] max-w-[112px] sticky left-0 z-20 bg-white group-hover:bg-blue-100">
                    <span v-tip="item.originalCode && item.originalCode !== item.code ? `STT theo quyết định: ${item.originalCode}` : ''">{{ item.code }}</span>
                  </td>
                  <td class="px-3 py-2.5 font-normal text-slate-800 border-r border-slate-200 leading-relaxed min-w-[280px] w-[280px] max-w-[280px] sticky left-[112px] z-20 bg-white group-hover:bg-blue-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.12)]">
                    <VTooltip
                      theme="custom-dark"
                      placement="top"
                      :delay="{ show: 1500, hide: 0 }"
                    >
                      <button type="button" class="document-title-button line-clamp-2 text-left font-normal text-slate-800 text-xs leading-relaxed hover:text-blue-700 hover:underline" @click.stop="openItemDetailModal(item)" :aria-label="`Xem chi tiết ${item.code}: ${item.title}`">
                        {{ item.title }}
                      </button>

                      <template #popper>
                        <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[450px] p-1">
                          <span class="app-tip-title">
                            {{ item.itemType === 'Goal' ? '🎯 Chi tiết mục tiêu' : '📋 Chi tiết nhiệm vụ' }}
                          </span>
                          {{ item.title }}
                        </div>
                      </template>
                    </VTooltip>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <VTooltip
                      v-if="item.leadAgencyDisplayText || item.leadAgencyName"
                      theme="custom-dark"
                      placement="top"
                      :delay="{ show: 1500, hide: 0 }"
                    >
                      <div class="line-clamp-2 font-normal text-slate-700 text-xs leading-relaxed cursor-help">
                        {{ item.leadAgencyDisplayText || item.leadAgencyName }}
                      </div>

                      <template #popper>
                        <div class="whitespace-normal break-words text-left leading-relaxed min-w-[200px] max-w-[420px] p-1">
                          <span class="app-tip-title">
                            🏛️ Cơ quan chủ trì
                          </span>
                          {{ item.leadAgencyDisplayText || item.leadAgencyName }}
                        </div>
                      </template>
                    </VTooltip>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td v-if="isLeadAgencyFilteredByBKHCN" class="px-3 py-2.5 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <div v-if="item.subAgencyNames?.length" class="flex flex-wrap gap-1">
                      <span v-for="name in item.subAgencyNames" :key="name" class="px-2 py-0.5 rounded-md bg-blue-50 text-blue-700 font-semibold text-xs border border-blue-100 inline-block">
                        {{ name }}
                      </span>
                    </div>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 font-normal text-slate-700 text-xs leading-relaxed min-w-[180px] w-[180px] max-w-[180px]">
                    <template v-if="item.coordinatingAgenciesDisplayText">
                      {{ item.coordinatingAgenciesDisplayText }}
                    </template>
                    <template v-else-if="item.coordinatingAgencyNames && item.coordinatingAgencyNames.length > 0">
                      {{ item.coordinatingAgencyNames.join(', ') }}
                    </template>
                    <template v-else-if="item.coordinatingAgencyCodes && item.coordinatingAgencyCodes.length > 0">
                      {{ item.coordinatingAgencyCodes.join(', ') }}
                    </template>
                    <span v-else class="text-slate-400 italic">—</span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-xs font-normal text-slate-600 whitespace-nowrap min-w-[160px] w-[160px] max-w-[160px]">
                    <span v-if="item.isOngoing" class="px-2 py-0.5 rounded-full font-medium text-xs bg-slate-100 text-slate-700 border border-slate-200">
                      Thường xuyên
                    </span>
                    <span v-else>
                      {{ formatDateRange(item.startDate, item.dueDate) }}
                    </span>
                  </td>
                  <td v-if="filterItemType === 'Goal'" class="px-3 py-2.5 border-r border-slate-200 text-center text-xs min-w-[125px] w-[125px] max-w-[125px] overflow-hidden">
                    <span v-if="authState.seesAll.value && isGeneralTaskOrAllAgencies(item) && !isAgencySumGoal(item)" class="text-slate-400 font-normal">—</span>
                    <span v-else class="font-normal text-xs text-slate-700 line-clamp-2 break-words leading-tight block" v-tip="formatProgressDisplay(item)">
                      {{ formatProgressDisplay(item) }}
                    </span>
                  </td>
                  <td class="px-3 py-2.5 border-r border-slate-200 text-center min-w-[195px] w-[195px] max-w-[195px]">
                    <div class="flex flex-col items-center gap-1">
                      <span :class="['px-2.5 py-0.5 rounded-full text-xs font-medium shadow-2xs inline-block whitespace-nowrap', getStatusBadgeClass(item.calculatedStatus)]">
                        {{ getStatusLabel(item.calculatedStatus) }}
                      </span>
                      <span v-if="item.deadlineStatus === 'Overdue'" class="text-xs font-semibold text-rose-700">
                        Quá hạn
                      </span>
                      <span v-if="item.hasPendingApproval" class="px-2 py-0.5 rounded-md text-xs font-medium bg-amber-100 text-amber-900 border border-amber-300 animate-pulse cursor-pointer" @click.stop="openItemDetailModal(item, 'history')" v-tip="'Có báo cáo tiến độ mới chờ Cấp 2 phê duyệt. Bấm để xem chi tiết.'">
                        ⏳ Báo cáo chờ duyệt
                      </span>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-center whitespace-nowrap" @click.stop>
                    <div class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap">

                      <button
                        v-if="canUpdateProgress(item)"
                        @click.stop="openProgressModal(item)"
                        class="ui-action ui-action-primary p-1.5 bg-slate-100 text-slate-700 hover:bg-slate-200 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer"
                        v-tip="'Cập nhật tiến độ và minh chứng'"
                       aria-label="Cập nhật tiến độ và minh chứng">
                        <svg class="w-4 h-4 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
                      </button>

                      <span
                        v-else-if="isCoordinatingOnly(item)"
                        class="px-2 py-0.5 rounded-md text-xs font-medium bg-amber-50 text-amber-700 border border-amber-200/80"
                        v-tip="'Đơn vị phối hợp - Chỉ xem thông tin'"
                      >
                        Đơn vị phối hợp (Chỉ xem)
                      </span>

                      <!-- 3-Dots Dropdown Menu (Văn thư chỉ xem ở phân hệ mục tiêu/nhiệm vụ nên không có thao tác) -->
                      <VDropdown v-if="!authState.isClerk.value" placement="bottom-end" :distance="6" popper-class="v-popper-clean-overlay">
                        <button
                          type="button"
                          @click.stop
                          class="ui-action ui-action-neutral p-1.5 bg-slate-50 text-slate-600 hover:bg-slate-200 hover:text-slate-900 rounded-lg border border-slate-200 inline-flex items-center justify-center shadow-2xs cursor-pointer transition"
                          v-tip="'Thao tác khác'"
                         aria-label="Thao tác khác">
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
                          </svg>
                        </button>

                        <template #popper="{ hide }">
                          <div class="py-1.5 w-48 overflow-hidden bg-white rounded-xl shadow-lg ring-1 ring-slate-200 text-xs font-normal space-y-0.5" @click.stop>
                            <button
                              v-if="canAssignTask(item)"
                              @click="openAssignModalFromList(item); hide()"
                              class="ui-action ui-action-primary ui-action-menu ui-single-line w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              v-tip="'Giao cho đơn vị trực thuộc'"
                             aria-label="Giao cho đơn vị trực thuộc">
                              <span>Giao đơn vị trực thuộc</span>
                            </button>

                            <button
                              v-if="authState.isAdmin.value && !hasProgress(item)"
                              @click="openEditModal(item); hide()"
                              class="ui-action ui-action-primary ui-action-menu ui-single-line w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              v-tip="'Chỉnh Sửa'"
                             aria-label="Chỉnh Sửa">
                              <span>Chỉnh sửa</span>
                            </button>

                            <button
                              @click="openNotificationModal(item); hide()"
                              class="ui-action ui-action-warning ui-action-menu ui-single-line w-full text-left px-3 py-2 text-slate-700 hover:bg-slate-100 hover:text-slate-900 transition cursor-pointer"
                              v-tip="'Gửi thông báo đến đơn vị chủ trì & phối hợp'"
                             aria-label="Gửi thông báo đến đơn vị chủ trì & phối hợp">
                              <span>Gửi thông báo</span>
                            </button>

                            <div v-if="canDeleteItems && authState.isAdmin.value && !hasProgress(item)" class="border-t border-slate-100 my-1"></div>

                            <button
                              v-if="canDeleteItems && authState.isAdmin.value && !hasProgress(item)"
                              @click="handleDeleteItem(item); hide()"
                              class="ui-action ui-action-danger ui-action-menu ui-single-line w-full text-left px-3 py-2 text-rose-600 hover:bg-rose-50 transition cursor-pointer"
                              v-tip="'Xóa'"
                             aria-label="Xóa">
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
// Mục chung tính bằng số lượng (MT-24) hiển thị tổng cộng dồn các cơ quan do API trả về.
import { isAgencySumGoal } from '../../shared/agencyPresentation';

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
  'canDeleteItems',
  'openEditModal',
  'filterItemType'
]);

</script>
<style scoped>
@media (max-width: 639px) {
  /* Pinned name columns must not cover actions when the table is scrolled. */
  .document-items-scroll :is(th, td).sticky { position: static; }
}
</style>

