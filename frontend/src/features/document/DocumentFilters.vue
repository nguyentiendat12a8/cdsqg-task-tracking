<template>
<div class="flex flex-col md:flex-row items-stretch md:items-center justify-between gap-3 bg-slate-50/60 p-3 w-full">
          <div class="flex items-center gap-2 flex-1 max-w-xl min-w-0">
            <!-- Quick Search Input -->
            <div class="relative flex-1 min-w-[200px]">
              <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
              <input
                :value="filterDraft.searchQuery"
                @input="filterDraft.searchQuery = $event.target.value"
                :placeholder="filterItemType === 'Goal' ? 'Tìm theo mã, tên mục tiêu...' : 'Tìm theo mã, tên nhiệm vụ...'"
                class="ui-single-line w-full text-xs font-semibold pl-9 pr-3 py-1.5 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none "
              />
            </div>

            <!-- OverlayPanel Advanced Filter Popover -->
            <OverlayPanel
              title="Bộ lọc tìm kiếm nâng cao"
              buttonText="Lọc nâng cao"
              :activeCount="activeFilterCount"
              widthClass="w-[320px] sm:w-[480px] max-w-[90vw]"
              heightClass="h-[480px] max-h-[calc(100dvh-3rem)]"
              :applyDisabled="!!filterError"
              @apply="execFilterSearch"
              @reset="resetFilterSearch"
            >
              <div class="space-y-3">
                <div>
                  <SearchableSelect
                    v-model="filterDraft.selectedAgencyIds"
                    :options="leadAgencyOptions"
                    :isMulti="true"
                    label="Cơ quan chủ trì"
                    placeholder="Tất cả cơ quan chủ trì"
                  />
                </div>

                <div>
                  <SearchableSelect
                    v-model="filterDraft.selectedSubAgencyIds"
                    :options="subAgencyOptions"
                    :isMulti="true"
                    label="Đơn vị trực thuộc"
                    placeholder="Tất cả đơn vị trực thuộc"
                  />
                </div>

                <!-- Combined 2-Column Row: Phạm vi & Trạng thái hạng mục -->
                <div class="grid grid-cols-2 gap-3">
                  <div>
                    <SearchableSelect
                      v-model="filterDraft.selectedScopes"
                      :options="scopeOptions"
                      :isMulti="true"
                      label="Phạm vi"
                      placeholder="Tất cả phạm vi"
                    />
                  </div>

                  <div>
                    <SearchableSelect
                      v-model="filterDraft.selectedStatuses"
                      :options="statusOptions"
                      :isMulti="true"
                      label="Trạng thái hạng mục"
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
                    label="Nhóm trọng tâm"
                    placeholder="Tất cả nhóm"
                  />
                </div>

                <div>
                  <label class="text-xs font-semibold text-slate-700 block mb-1">Thời gian báo cáo (từ năm ➔ đến năm)</label>
                  <div class="flex items-center gap-2">
                    <SearchableSelect
                      v-model="filterDraft.reportFromYear"
                      :options="yearOptions"
                      :isMulti="false"
                      label="Từ năm"
                      label-class="sr-only"
                      placeholder="Từ năm"
                      class="w-full"
                    />
                    <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                    <SearchableSelect
                      v-model="filterDraft.reportToYear"
                      :options="yearOptions"
                      :isMulti="false"
                      label="Đến năm"
                      label-class="sr-only"
                      placeholder="Đến năm"
                      class="w-full"
                    />
                  </div>
                </div>

                <div>
                  <label class="text-xs font-semibold text-slate-700 block mb-1">Thời gian hoàn thành (từ ngày ➔ đến ngày)</label>
                  <div class="flex items-center gap-2">
                    <div class="w-full">
                      <label :for="`${filterId}-from`" class="sr-only">Từ ngày</label>
                      <DatePicker v-model="filterDraft.executionFrom" :input-id="`${filterId}-from`" placeholder="Từ ngày" />
                    </div>
                    <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                    <div class="w-full">
                      <label :for="`${filterId}-to`" class="sr-only">Đến ngày</label>
                      <DatePicker v-model="filterDraft.executionTo" :input-id="`${filterId}-to`" placeholder="Đến ngày" />
                    </div>
                  </div>
                </div>

                <label class="flex cursor-pointer items-center gap-2 text-xs font-semibold text-slate-700 select-none">
                  <input v-model="filterDraft.onlyOngoing" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500" />
                  <span>{{ filterItemType === 'Goal' ? 'Mục tiêu thường xuyên' : 'Nhiệm vụ thường xuyên' }}</span>
                </label>

                <p v-if="filterError" role="alert" class="text-xs text-rose-700">{{ filterError }}</p>
              </div>
            </OverlayPanel>
          </div>
        </div>
</template>

<script setup>
import { computed, useId } from 'vue';
import SearchableSelect from '../../components/SearchableSelect.vue';
import DatePicker from '../../components/DatePicker.vue';
import OverlayPanel from '../../components/OverlayPanel.vue';

defineProps([
  'activeFilterCount',
  'execFilterSearch',
  'resetFilterSearch',
  'sectionFilterOptions',
  'groupFilterOptions',
  'leadAgencyOptions',
  'subAgencyOptions',
  'yearOptions',
  'scopeOptions',
  'statusOptions',
  'filterItemType'
]);
const filterDraft = defineModel('filterDraft', { required: true });
const filterId = `doc-filter-${useId()}`;

const filterError = computed(() => {
  const draft = filterDraft.value;
  if (draft.reportFromYear && draft.reportToYear && Number(draft.reportFromYear) > Number(draft.reportToYear)) return 'Năm báo cáo từ không được lớn hơn năm đến.';
  if (draft.executionFrom && draft.executionTo && draft.executionFrom > draft.executionTo) return 'Ngày hoàn thành từ không được lớn hơn ngày đến.';
  return '';
});
</script>


