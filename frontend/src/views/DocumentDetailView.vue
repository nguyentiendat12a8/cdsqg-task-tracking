<template>
  <div class="w-full space-y-4 font-sans">

    <!-- UNIFIED MAIN CONTENT CARD BLOCK -->
    <div class="bg-white rounded-2xl shadow-sm border border-slate-200/80 overflow-hidden w-full">

      <!-- Integrated Top Header Bar -->
      <div class="p-4 sm:p-5 border-b border-slate-200/80 bg-white flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div>
          <h2 class="text-sm sm:text-base font-bold text-slate-800">
            {{ filterItemType === 'Goal' ? '🎯 Danh Sách Mục Tiêu' : '📋 Danh Sách Nhiệm Vụ' }}
          </h2>
        </div>

        <div class="flex items-center gap-2 flex-wrap">
          <!-- General List Export (All Users including Admin) -->
          <button
            @click="exportDocumentItemsToExcel" :disabled="isExportingDocument" :aria-busy="isExportingDocument"
            class="ui-single-line px-3.5 py-2 text-slate-700 hover:text-slate-900 font-bold text-xs rounded-xl bg-slate-100 hover:bg-slate-200 border border-slate-200/80 transition shadow-2xs flex items-center gap-1.5 cursor-pointer"
            title="Xuất danh sách mục tiêu/nhiệm vụ ra file Excel"
          >
            <svg class="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
            <span>Xuất Excel Danh Sách</span>
          </button>

          <button
            v-if="authState.isAdmin.value"
            @click="openPendingApprovalsModal"
            class="ui-single-line px-3.5 py-2 text-amber-950 font-bold text-xs rounded-xl bg-amber-100 hover:bg-amber-200 border border-amber-300 transition shadow-2xs flex items-center gap-1.5 cursor-pointer relative"
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
            class="ui-single-line px-3.5 py-2 text-white font-bold text-xs rounded-xl bg-blue-600 hover:bg-blue-700 transition shadow-sm flex items-center gap-1.5 cursor-pointer"
          >
            + Thêm {{ filterItemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} Mới
          </button>
        </div>
      </div>

      <!-- Main Body Container -->
      <div class="p-3.5 sm:p-4 space-y-3.5 w-full">

      <!-- DYNAMIC PLANNING GRID SUB-TAB -->
      <div v-show="activeSubTab === 'grid'" class="w-full">
        <DynamicPlanningGrid ref="planningGridRef" :documentId="REPORTING_DOCUMENT_ID" :filterItemType="filterItemType || 'Task'" />
      </div>

      <!-- LIST TABLE SUB-TAB WITH STICKY HEADERS & EVIDENCE FILES AUDIT LOG -->
      <div v-show="activeSubTab === 'list'" class="w-full border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col">

        <!-- ADVANCED SEARCH & SCOPE FILTER BAR -->
        <DocumentFilters
      :activeFilterCount="activeFilterCount"
      :execFilterSearch="execFilterSearch"
      :resetFilterSearch="resetFilterSearch"
      :sectionFilterOptions="sectionFilterOptions"
      :groupFilterOptions="groupFilterOptions"
      :leadAgencyOptions="leadAgencyOptions"
      :subAgencyOptions="subAgencyOptions"
      :yearOptions="yearOptions"
      :scopeOptions="scopeOptions"
      :statusOptions="statusOptions"
      :filterItemType="filterItemType"
      v-model:filterDraft="filterDraft"
    />

        <!-- MAIN DATA TABLE WITH STICKY HEADER & FROZEN FIRST 3 COLUMNS -->
        <div v-if="loadError" role="alert" class="p-3 text-rose-700 bg-rose-50">{{ loadError }} <button type="button" class="underline" @click="loadData">Thử lại</button></div>
        <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu danh sách từ máy chủ..." />
        <DocumentItemsTable
      :authState="authState"
      :formatDateRange="formatDateRange"
      :getStatusLabel="getStatusLabel"
      :getStatusBadgeClass="getStatusBadgeClass"
      :formatProgressDisplay="formatProgressDisplay"
      :isGeneralTaskOrAllAgencies="isGeneralTaskOrAllAgencies"
      :sortBy="sortBy"
      :sortOrder="sortOrder"
      :handleSort="handleSort"
      :paginatedPrimaryList="paginatedPrimaryList"
      :canUpdateProgress="canUpdateProgress"
      :isCoordinatingOnly="isCoordinatingOnly"
      :openProgressModal="openProgressModal"
      :isLeadAgencyFilteredByBKHCN="isLeadAgencyFilteredByBKHCN"
      :canAssignTask="canAssignTask"
      :openAssignModalFromList="openAssignModalFromList"
      :openNotificationModal="openNotificationModal"
      :openItemDetailModal="openItemDetailModal"
      :hasProgress="hasProgress"
      :handleDeleteItem="handleDeleteItem"
      :openEditModal="openEditModal"
      :filterItemType="filterItemType"
      v-else
    />

        <!-- Attached Pagination Controls Bar -->
        <DocumentPagination
      :changePage="changePage"
      :pageSizeOptions="pageSizeOptions"
      :totalCount="totalCount"
      :totalPages="totalPages"
      :filterItemType="filterItemType"
      v-model:pageSize="pageSize"
      v-model:currentPage="currentPage"
    />
      </div>

    </div>
    </div>

    <!-- Create Goal/Task/Sub-task Modal with Date Validation -->
    <CreateItemDialog
      :formatDateRange="formatDateRange"
      :leadAgencyOptions="leadAgencyOptions"
      :yearOptions="yearOptions"
      :createItemType="createItemType"
      :addDeliverable="addDeliverable"
      :removeDeliverable="removeDeliverable"
      :currentFormSections="currentFormSections"
      :currentFormGroups="currentFormGroups"
      :goalUnitOptions="goalUnitOptions"
      :createAssignedAgencyOptions="createAssignedAgencyOptions"
      :coordinatingAgencyOptions="coordinatingAgencyOptions"
      :submitCreateItem="submitCreateItem"
      v-model:createForm="createForm"
      v-model:isCreateModalOpen="isCreateModalOpen"
      v-model:createErrorMessage="createErrorMessage"
      v-if="isCreateModalOpen"
    />

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
      :is-general-task="isGeneralTaskOrAllAgencies(selectedTaskForProgress)"
      :lead-agency-id="selectedTaskForProgress.leadAgencyId || ''"
      :lead-agency-name="selectedTaskForProgress.leadAgencyName || ''"
      :coordinating-agency-ids="selectedTaskForProgress.coordinatingAgencyIds || []"
      :agencies="agencies || []"
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
    <EditItemDialog
      :sectionFilterOptions="sectionFilterOptions"
      :groupFilterOptions="groupFilterOptions"
      :leadAgencyOptions="leadAgencyOptions"
      :yearOptions="yearOptions"
      :editingItem="editingItem"
      :goalUnitOptions="goalUnitOptions"
      :editAssignedAgencyOptions="editAssignedAgencyOptions"
      :coordinatingAgencyOptions="coordinatingAgencyOptions"
      :addEditDeliverable="addEditDeliverable"
      :removeEditDeliverable="removeEditDeliverable"
      :submitEditItem="submitEditItem"
      v-model:editForm="editForm"
      v-model:isEditModalOpen="isEditModalOpen"
      v-model:editErrorMessage="editErrorMessage"
      v-if="isEditModalOpen"
    />

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
          <button @click="isAssignModalOpen = false" class="ui-single-line px-3 py-1.5 text-xs text-slate-600 hover:bg-slate-100 rounded-xl font-bold cursor-pointer">Hủy</button>
          <button @click="submitAssignTaskFromList" class="ui-single-line px-4 py-1.5 text-xs bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl shadow-2xs cursor-pointer">Lưu Giao Đơn Vị Trực Thuộc</button>
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
                    <span v-if="isGoalItem(log)" class="px-2 py-0.5 rounded text-[10px] font-bold bg-purple-100 text-purple-800 border border-purple-200">
                      🎯 Mục tiêu
                    </span>
                    <span v-else-if="log.isGeneralTask" class="px-2 py-0.5 rounded text-[10px] font-bold bg-purple-100 text-purple-800 border border-purple-200">
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
                  <div v-if="!isGoalItem(log) && log.actualValue !== null && log.actualValue !== undefined" class="text-slate-600 text-[11px]">Giá trị thực tế: <strong>{{ log.actualValue }}</strong></div>
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
                  class="ui-single-line px-4 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-2xs transition cursor-pointer flex items-center gap-1"
                >
                  ✓ Phê Duyệt
                </button>
                <button
                  @click="openRejectModalFromGlobalList(log.id)"
                  class="ui-single-line px-4 py-1.5 bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs rounded-xl shadow-2xs transition cursor-pointer flex items-center gap-1"
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
            class="ui-single-line px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer"
          >
            Hủy bỏ
          </button>
          <button
            @click="confirmRejectFromGlobalList"
            class="ui-single-line px-4 py-2 bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs rounded-xl shadow-2xs transition cursor-pointer flex items-center gap-1"
          >
            ✕ Xác Nhận Từ Chối
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { REPORTING_DOCUMENT_ID } from '../config/reporting';
import { useCreateItem } from '../features/document/useCreateItem';
import { useEditItem } from '../features/document/useEditItem';
import { usePendingApprovals } from '../features/document/usePendingApprovals';

import DocumentFilters from '../features/document/DocumentFilters.vue';
import DocumentItemsTable from '../features/document/DocumentItemsTable.vue';
import DocumentPagination from '../features/document/DocumentPagination.vue';
const CreateItemDialog = defineAsyncComponent(() => import('../features/document/CreateItemDialog.vue'));
const EditItemDialog = defineAsyncComponent(() => import('../features/document/EditItemDialog.vue'));
import { fetchWithAuth } from '../services/auth';

import { defineAsyncComponent, ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { toast } from 'vue3-toastify';
import { confirmModal } from '../services/confirm';
import { authState } from '../services/auth';
import DynamicPlanningGrid from '../components/DynamicPlanningGrid.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
const ProgressUpdateModal = defineAsyncComponent(() => import('../components/ProgressUpdateModal.vue'));
const ProgressImportModal = defineAsyncComponent(() => import('../components/ProgressImportModal.vue'));
const SendNotificationModal = defineAsyncComponent(() => import('../components/SendNotificationModal.vue'));
const ItemDetailModal = defineAsyncComponent(() => import('../components/ItemDetailModal.vue'));

import { getApiUrl } from '../config/api';

const props = defineProps({
  filterItemType: { type: String, default: 'Task' },
  subTab: { type: String, default: 'list' }
});

import { useDocumentFilters } from '../features/document/useDocumentFilters';
import { useDocumentData } from '../features/document/useDocumentData';
import { formatDateTime, formatDateRange, formatFileName, getStatusLabel, getStatusBadgeClass, formatProgressDisplay, isGeneralTaskOrAllAgencies } from '../features/document/presentation';

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
  if (authState.isAdmin.value) return 'Admin';
  if (r === 'Level2' || r === 2 || r === '2') return 'Level2';
  if (r === 'Level3' || r === 3 || r === '3') return 'Level3';
  return String(r || 'Level2');
});
const currentUserAgencyObj = computed(() => authState.user.value?.agency || null);

const isProgressModalOpen = ref(false);
const selectedTaskForProgress = ref(null);
const isProgressImportModalOpen = ref(false);

function handleProgressImported() {
  loadData();
}

const isNotificationModalOpen = ref(false);
const selectedItemForNotification = ref(null);

const isItemDetailModalOpen = ref(false);
const selectedItemForDetail = ref(null);
const itemDetailModalInitialTab = ref('info');

const { isLoading, loadError, agencies, rawItemsList, serverTotalCount, serverTotalPages, loadData, loadExportItems } = useDocumentData({ props, getQueryState: () => ({ userRoleStr, appliedFilters, currentPage, pageSize, sortBy, sortOrder }) });
const {
  filterDraft,
  activeFilterCount,
  appliedFilters,
  currentPage,
  pageSize,
  sortBy,
  sortOrder,
  handleSort,
  execFilterSearch,
  resetFilterSearch,
  changePage,
  sectionFilterOptions,
  groupFilterOptions,
  agencyOptions,
  isSpecialAgencyCode,
  leadAgencyOptions,
  subAgencyOptions,
  yearOptions,
  pageSizeOptions,
  scopeOptions,
  statusOptions,
  filteredList,
  totalCount,
  totalPages,
  paginatedPrimaryList
} = useDocumentFilters({ props, userAgencyId, agencies, rawItemsList, serverTotalCount, serverTotalPages, isGeneralTaskOrAllAgencies, loadData });

const {
  isCreateModalOpen,
  createItemType,
  createForm,
  createErrorMessage,
  addDeliverable,
  removeDeliverable,
  currentFormSections,
  currentFormGroups,
  createAssignedAgencyOptions,
  openCreateModal,
  submitCreateItem
} = useCreateItem({ planningGridRef, agencies, rawItemsList, loadData, isSpecialAgencyCode });

const {
  isEditModalOpen,
  editErrorMessage,
  editingItem,
  editForm,
  editAssignedAgencyOptions,
  openEditModal,
  addEditDeliverable,
  removeEditDeliverable,
  submitEditItem
} = useEditItem({ planningGridRef, agencies, loadData, hasProgress });

const goalUnitOptions = ref([
  { value: '%', label: '% (Phần trăm)' },
  { value: 'Số lượng', label: 'Số lượng (Số nguyên / Thập phân)' }
]);

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


function canUpdateProgress(item) {
  if (!item) return false;
  // Tài khoản Quản trị viên (Admin) cho phép thực hiện cập nhật tiến độ
  if (authState.isAdmin.value) return true;
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

function openProgressModal(item) {
  if (item && item.hasPendingApproval && !authState.isAdmin.value) {
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
    const res = await fetchWithAuth(getApiUrl(`/api/planning/items/${targetId}/assign`), {
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
        await loadData();
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

  if (item.calculatedStatus && item.calculatedStatus !== 'NotStarted' && item.calculatedStatus !== 'Chưa thực hiện' && item.calculatedStatus !== '1. Chưa thực hiện') {
    return true;
  }

  const selfHasLogs = (item.progressLogs && item.progressLogs.length > 0) ||
                      (item.latestProgressValue !== null && item.latestProgressValue !== undefined && item.latestProgressValue > 0) ||
                      (item.latestProgressStatus !== null && item.latestProgressStatus !== undefined && item.latestProgressStatus !== '' && item.latestProgressStatus !== 'NotStarted');
  if (selfHasLogs) return true;


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

  const itemTypeLabel = props.filterItemType === 'Goal' ? 'mục tiêu' : 'nhiệm vụ';
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
    const res = await fetchWithAuth(getApiUrl(`/api/planning/items/${targetId}`), {
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

const {
  globalPendingLogs,
  isPendingApprovalsModalOpen,
  isLoadingPendingLogs,
  isGoalItem,
  isRejectModalOpen,
  rejectLogId,
  rejectionReason,
  loadGlobalPendingLogs,
  openPendingApprovalsModal,
  handleApproveFromGlobalList,
  openRejectModalFromGlobalList,
  confirmRejectFromGlobalList
} = usePendingApprovals({ loadData });

let globalPendingPollTimer = null;

function handlePendingLogsUpdate() {
  if (authState.isAdmin.value) {
    loadGlobalPendingLogs(true);
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
    loadGlobalPendingLogs(false);
    isPendingApprovalsModalOpen.value = true;
  }
}

onMounted(() => {
  loadData();
  if (authState.isAdmin.value) {
    loadGlobalPendingLogs(false);
    globalPendingPollTimer = setInterval(() => loadGlobalPendingLogs(true), 5000);
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

const isExportingDocument = ref(false);
async function runDocumentExport(action) {
  if (isExportingDocument.value) return;
  isExportingDocument.value = true;
  try {
    const { createDocumentReportActions } = await import('../features/document/documentReport');
    const actions = createDocumentReportActions({ props, userRoleStr, currentUserAgencyObj, formatDateRange, getStatusLabel, formatProgressDisplay, isGeneralTaskOrAllAgencies, filteredList, loadExportItems });
    await actions[action]();
  } catch (error) {
    toast.error('Không thể xuất Excel. Vui lòng thử lại.');
    console.error('Excel export failed:', error);
  } finally { isExportingDocument.value = false; }
}
function exportDocumentItemsToExcel() { return runDocumentExport('exportDocumentItemsToExcel'); }
function handleExportProgressReport() { return runDocumentExport('handleExportProgressReport'); }
</script>




