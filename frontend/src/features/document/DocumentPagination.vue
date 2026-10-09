<template>
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
              class="ui-single-line px-3.5 py-1.5 bg-white hover:bg-slate-100 border border-slate-300 rounded-lg disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
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
</template>

<script setup>
import SearchableSelect from '../../components/SearchableSelect.vue';

defineProps([
  'changePage',
  'pageSizeOptions',
  'totalCount',
  'totalPages',
  'filterItemType'
]);
const pageSize = defineModel('pageSize', { required: true });
const currentPage = defineModel('currentPage', { required: true });
</script>

