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
</template>

<script setup>
import SearchableSelect from '../../components/SearchableSelect.vue';
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
</script>


