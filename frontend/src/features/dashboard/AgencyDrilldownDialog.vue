<template>
<div v-accessible-dialog="() => selectedDrilldownAgency = null"  class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 font-sans">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-6xl sm:max-w-7xl w-full p-5 sm:p-6 space-y-4">

        <!-- Modal Header -->
        <div class="flex items-start justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base sm:text-lg font-bold text-slate-800 flex items-center gap-2">
              <span class="p-1.5 bg-blue-100 text-blue-700 rounded-xl text-sm">🏛️</span>
              {{ selectedDrilldownAgency.name }}
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">Chi tiết tiến độ đơn vị trực thuộc, danh sách mục tiêu & nhiệm vụ được gán và thông tin cán bộ đầu mối liên hệ</p>
          </div>
          <button @click="selectedDrilldownAgency = null" class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-xl transition cursor-pointer">✕</button>
        </div>

        <!-- Navigation Tabs inside Modal -->
        <div class="flex items-center gap-2 border-b border-slate-200/80 pb-2 overflow-x-auto custom-scrollbar">
          <!-- TAB 1: Goal Items Assigned to Agency -->
          <button
            @click="switchDrilldownTab('goals')"
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'goals' ? 'bg-purple-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>🎯 Danh Sách Mục Tiêu ({{ isAgencyItemsLoading ? '...' : modalGoalsList.length }})</span>
          </button>

          <!-- TAB 2: Task Items Assigned to Agency -->
          <button
            @click="switchDrilldownTab('tasks')"
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'tasks' ? 'bg-blue-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>📋 Danh Sách Nhiệm Vụ ({{ isAgencyItemsLoading ? '...' : modalTasksList.length }})</span>
          </button>

          <!-- TAB 3: Sub-Agencies & Progress (Chỉ hiển thị với Bộ Khoa học và Công nghệ) -->
          <button
            v-if="isBKHCNItem(selectedDrilldownAgency)"
            @click="switchDrilldownTab('sub-agencies')"
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'sub-agencies' ? 'bg-blue-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>🏛️ Đơn Vị Trực Thuộc ({{ isSubAgenciesLoading ? '...' : subAgenciesList.length }})</span>
          </button>

          <!-- TAB 4: Contact Persons -->
          <button
            @click="switchDrilldownTab('contacts')"
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'contacts' ? 'bg-blue-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>📞 Cán Bộ Đầu Mối Liên Hệ ({{ isSubAgenciesLoading ? '...' : allDrilldownContacts.length }})</span>
          </button>
        </div>

        <!-- TAB 1 CONTENT: Sub-Agencies & Progress -->
        <div v-if="drilldownTab === 'sub-agencies' && isBKHCNItem(selectedDrilldownAgency)" class="space-y-3.5 max-h-[60vh] overflow-y-auto custom-scrollbar pr-1">
          <!-- Loading State -->
          <div v-if="isSubAgenciesLoading" class="p-8 text-center">
            <LoadingSpinner size="md" message="Đang tải danh sách đơn vị trực thuộc..." />
          </div>

          <div v-else-if="subAgenciesList.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
            <div
              v-for="(child, index) in subAgenciesList"
              :key="child.agencyId"
              class="bg-white rounded-2xl border border-slate-200/90 p-3.5 shadow-2xs hover:shadow-md transition-all duration-200 space-y-3 flex flex-col justify-between"
            >
              <!-- Card Header: Child Agency Name & Blue Index Badge -->
              <div class="flex items-start justify-between gap-2 border-b border-slate-100 pb-2.5">
                <div class="min-w-0 flex-1">
                  <h4 class="text-xs sm:text-sm font-bold text-slate-800 leading-snug truncate" :title="child.name">
                    {{ child.name }}
                  </h4>
                  <p class="text-[11px] text-slate-500 font-medium mt-0.5">
                    <span v-if="dashboardFilter === 'goals'">🎯 {{ child.totalGoals || 0 }} Mục tiêu</span>
                    <span v-else>📋 {{ child.totalTasks || 0 }} Nhiệm vụ</span>
                  </p>
                </div>

                <div class="w-7 h-7 bg-blue-600 text-white font-bold text-xs rounded-xl flex items-center justify-center shrink-0 shadow-2xs">
                  {{ index + 1 }}
                </div>
              </div>

              <!-- Card Body: Donut Chart on Left, Legend Breakdown List on Right -->
              <div class="flex items-center gap-3 py-0.5">
                <!-- Donut Chart -->
                <div class="shrink-0 flex items-center justify-center">
                  <MiniStatusDonut :stats="child" :size="84" :innerSize="54" :fontSize="18" />
                </div>

                <!-- 6 Status Legend List -->
                <div class="flex-1 min-w-0 space-y-1 text-[10px] font-bold">
                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-rose-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h quá hạn</span>
                    </div>
                    <span class="font-bold text-slate-900">{{ child.inProgressOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h trong hạn</span>
                    </div>
                    <span class="font-bold text-slate-900">{{ child.inProgressOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-amber-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Sắp tới hạn</span>
                    </div>
                    <span class="font-bold text-slate-900">{{ child.expiringSoon || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-teal-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t quá hạn</span>
                    </div>
                    <span class="font-bold text-slate-900">{{ child.completedOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t trong hạn</span>
                    </div>
                    <span class="font-bold text-slate-900">{{ child.completedOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-slate-400 shrink-0"></span>
                      <span class="text-slate-600 truncate">Chưa thực hiện</span>
                    </div>
                    <span class="font-bold text-slate-900">{{ child.notStarted || 0 }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-if="subAgenciesList.length === 0" class="p-8 text-center text-xs text-slate-400 font-semibold italic">
            Không có đơn vị trực thuộc nào.
          </div>
        </div>

        <!-- TAB CONTENT: Goal/Task Items Assigned to Agency -->
        <div v-if="drilldownTab === 'goals' || drilldownTab === 'tasks'" class="space-y-3 max-h-[60vh] overflow-y-auto custom-scrollbar pr-1">
          <!-- Local Filters Toolbar -->
          <div class="flex flex-wrap items-center justify-between gap-2 bg-slate-50 p-2.5 rounded-xl border border-slate-200/80">
            <div class="flex items-center gap-2 flex-1 min-w-[200px]">
              <div class="relative w-full">
                <span class="absolute inset-y-0 left-0 flex items-center pl-2.5 text-slate-400 text-xs">🔍</span>
                <input
                  v-model="modalSearchKeyword"
                  type="text"
                  :placeholder="drilldownTab === 'goals' ? 'Tìm mã, tên mục tiêu, lĩnh vực...' : 'Tìm mã, tên nhiệm vụ, lĩnh vực...'"
                  class="ui-single-line w-full pl-8 pr-3 py-1.5 bg-white text-xs border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 text-slate-800"
                />
              </div>
            </div>

            <div class="flex items-center gap-2 shrink-0">
              <!-- Status Filter -->
              <div class="w-60 max-w-full">
                <SearchableSelect v-model="modalStatusFilter" :options="[{ value: 'all', label: 'Tất cả trạng thái' }, ...executionStatusOptions]" :isMulti="false" :clearable="false" label="Trạng thái thực hiện" label-class="sr-only" />
              </div>
            </div>
          </div>

          <!-- Loading State -->
          <div v-if="isAgencyItemsLoading" class="p-8 text-center">
            <LoadingSpinner size="md" :text="drilldownTab === 'goals' ? 'Đang tải danh sách mục tiêu...' : 'Đang tải danh sách nhiệm vụ...'" />
          </div>

          <!-- Task/Goal Items List (BẢNG) -->
          <div v-else-if="filteredAgencyItems.length > 0" class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-xs">
            <div class="overflow-x-auto overflow-y-auto max-h-[50vh] custom-scrollbar w-full">
              <table class="w-full min-w-[850px] text-left text-xs text-slate-700 border-collapse">
                <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-30 shadow-2xs">
                  <tr>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[70px] w-[70px] max-w-[70px] sticky left-0 z-30">
                      {{ drilldownTab === 'goals' ? 'Mã MT' : 'Mã NV' }}
                    </th>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[260px]">
                      {{ drilldownTab === 'goals' ? 'Tên Mục Tiêu' : 'Tên Nhiệm Vụ' }}
                    </th>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[140px] w-[140px]">Cơ Quan Chủ Trì</th>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[130px] w-[130px]">Thời Gian</th>
                    <th v-if="drilldownTab === 'goals'" class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[100px] w-[100px]">Tiến Độ</th>
                    <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[150px] w-[150px]">Trạng Thái</th>
                    <th class="px-3 py-2.5 text-center bg-slate-100 whitespace-nowrap min-w-[90px] w-[90px]">Chi Tiết</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-200">
                  <tr
                    v-for="item in filteredAgencyItems"
                    :key="item.id"
                    @click="selectedDetailItem = item"
                    class="group hover:bg-blue-50/80 transition cursor-pointer"
                  >
                    <!-- Mã -->
                    <td class="px-3 py-2.5 font-bold text-blue-700 border-r border-slate-200 whitespace-nowrap sticky left-0 z-20 bg-white group-hover:bg-blue-50">
                      {{ item.code || (drilldownTab === 'goals' ? 'MT' : 'NV') }}
                    </td>

                    <!-- Tên & Phân loại -->
                    <td class="px-3 py-2.5 border-r border-slate-200 leading-relaxed">
                      <div class="flex items-center gap-1.5 flex-wrap mb-1">
                        <span :class="['px-2 py-0.5 text-[10px] font-bold rounded-md border', item.itemType === 'Goal' ? 'bg-purple-50 text-purple-700 border-purple-200' : 'bg-slate-100 text-slate-700 border-slate-200']">
                          {{ item.itemType === 'Goal' ? '🎯 Mục tiêu' : '📋 Nhiệm vụ' }}
                        </span>
                        <span :class="['px-2 py-0.5 text-[10px] font-bold rounded-md border', item.isGeneralTask ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-50 text-slate-600 border-slate-200']">
                          {{ item.isGeneralTask ? 'Phạm vi chung' : 'Phạm vi riêng' }}
                        </span>
                        <span v-if="item.section || item.category" class="text-[10px] font-medium text-slate-400">
                          • {{ item.section || item.category }}
                        </span>
                      </div>
                      <div class="font-semibold text-slate-900 leading-snug line-clamp-2" :title="item.title">
                        {{ item.title }}
                      </div>
                    </td>

                    <!-- Cơ quan chủ trì -->
                    <td class="px-3 py-2.5 border-r border-slate-200 font-bold text-slate-800 text-xs leading-relaxed truncate" :title="item.leadAgencyName">
                      {{ item.leadAgencyName }}
                    </td>

                    <!-- Thời gian -->
                    <td class="px-3 py-2.5 border-r border-slate-200 text-xs font-semibold text-slate-600 whitespace-nowrap">
                      <span v-if="item.isOngoing" class="px-2 py-0.5 rounded-full font-bold text-[11px] bg-blue-100 text-blue-800">
                        Thường xuyên
                      </span>
                      <span v-else-if="item.dueDate">
                        📅 {{ formatDate(item.dueDate) }}
                      </span>
                      <span v-else class="text-slate-400 italic">—</span>
                    </td>

                    <!-- Tiến độ -->
                    <td v-if="drilldownTab === 'goals'" class="px-3 py-2.5 border-r border-slate-200 text-center text-xs whitespace-nowrap">
                      <span v-if="formatItemProgressDisplay(item) !== '—'" class="font-bold px-2 py-0.5 rounded-lg text-xs bg-blue-50 text-blue-900 border border-blue-200">
                        {{ formatItemProgressDisplay(item) }}
                      </span>
                      <span v-else class="text-slate-400 italic">—</span>
                    </td>

                    <!-- Trạng thái -->
                    <td class="px-3 py-2.5 border-r border-slate-200 text-center whitespace-nowrap">
                      <span :class="['px-2.5 py-0.5 rounded-full text-[11px] font-bold shadow-2xs inline-block whitespace-nowrap border', getStatusBadgeClass(item.status)]">
                        {{ getStatusLabel(item.status) }}
                      </span>
                    </td>

                    <!-- Nút thao tác Xem chi tiết -->
                    <td class="px-3 py-2.5 text-center whitespace-nowrap" @click.stop>
                      <button
                        @click.stop="selectedDetailItem = item"
                        class="ui-single-line px-2 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-[11px] rounded-lg border border-blue-200 transition shadow-2xs inline-flex items-center gap-1 cursor-pointer"
                        title="Xem chi tiết"
                      >
                        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                        <span>Chi tiết</span>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Empty State -->
          <div v-else class="p-8 text-center text-xs text-slate-400 font-semibold italic">
            Không tìm thấy {{ drilldownTab === 'goals' ? 'mục tiêu' : 'nhiệm vụ' }} nào phù hợp với bộ lọc.
          </div>
        </div>

        <!-- TAB 3: Contact Persons -->
        <div v-if="drilldownTab === 'contacts'" class="space-y-3 max-h-[60vh] overflow-y-auto custom-scrollbar pr-1">
          <!-- Loading State -->
          <div v-if="isSubAgenciesLoading" class="p-8 text-center">
            <LoadingSpinner size="md" message="Đang tải danh sách cán bộ đầu mối..." />
          </div>

          <div v-else-if="allDrilldownContacts.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div
              v-for="(contact, index) in allDrilldownContacts"
              :key="index"
              class="bg-slate-50 p-3.5 rounded-xl border border-slate-200/80 space-y-2 hover:bg-blue-50/40 transition"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex items-center gap-2.5">
                  <div class="w-8 h-8 rounded-full bg-blue-600 text-white font-bold text-xs flex items-center justify-center shrink-0 shadow-xs uppercase">
                    {{ (contact.name || 'CB').charAt(0) }}
                  </div>
                  <div>
                    <h5 class="text-xs font-bold text-slate-900 leading-snug">{{ contact.name || 'Cán bộ đầu mối' }}</h5>
                    <p class="text-[11px] font-semibold text-blue-700 mt-0.5">{{ contact.position || 'Cán bộ liên hệ' }}</p>
                  </div>
                </div>

                <span :class="['text-[10px] font-bold px-2 py-0.5 rounded-md border shrink-0', contact.isParent ? 'bg-purple-50 text-purple-700 border-purple-200' : 'bg-slate-200/60 text-slate-700 border-slate-300']">
                  {{ contact.unitCode || 'Đơn vị' }}
                </span>
              </div>

              <div class="text-[11px] text-slate-600 space-y-1 pt-1.5 border-t border-slate-200/60">
                <div v-if="contact.department" class="flex items-center gap-1.5">
                  <span class="text-slate-400 font-bold">🏢 Phòng ban:</span>
                  <span class="font-semibold text-slate-800">{{ contact.department }}</span>
                </div>

                <div v-if="contact.phone" class="flex items-center gap-1.5">
                  <span class="text-slate-400 font-bold">📞 Điện thoại:</span>
                  <a :href="`tel:${contact.phone}`" class="font-bold text-blue-600 hover:underline">{{ contact.phone }}</a>
                </div>

                <div v-if="contact.email" class="flex items-center gap-1.5">
                  <span class="text-slate-400 font-bold">✉️ Email:</span>
                  <a :href="`mailto:${contact.email}`" class="font-bold text-blue-600 hover:underline truncate">{{ contact.email }}</a>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="p-8 text-center text-xs text-slate-400 font-semibold italic">
            Chưa có thông tin cán bộ đầu mối liên hệ cho cơ quan này.
          </div>
        </div>

      </div>
    </div>
</template>

<script setup>
import SearchableSelect from '../../components/SearchableSelect.vue';
import { executionStatusOptions } from '../../shared/statusPresentation';
import LoadingSpinner from '../../components/LoadingSpinner.vue';
import MiniStatusDonut from '../../components/MiniStatusDonut.vue';

defineProps([
  'getStatusBadgeClass',
  'getStatusLabel',
  'formatDate',
  'formatItemProgressDisplay',
  'agencies',
  'dashboardFilter',
  'isBKHCNItem',
  'subAgenciesList',
  'isSubAgenciesLoading',
  'drilldownTab',
  'isAgencyItemsLoading',
  'modalGoalsList',
  'modalTasksList',
  'filteredAgencyItems',
  'allDrilldownContacts',
  'switchDrilldownTab'
]);
const modalSearchKeyword = defineModel('modalSearchKeyword', { required: true });
const modalStatusFilter = defineModel('modalStatusFilter', { required: true });
const selectedDrilldownAgency = defineModel('selectedDrilldownAgency', { required: true });
const selectedDetailItem = defineModel('selectedDetailItem', { required: true });
</script>



