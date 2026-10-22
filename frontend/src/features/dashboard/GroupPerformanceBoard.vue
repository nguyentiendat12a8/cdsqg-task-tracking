<template>
  <section class="group-board flex min-h-0 min-w-0 flex-col overflow-hidden rounded-2xl border border-blue-200 bg-white" aria-label="Thống kê theo nhóm">
    <header class="flex shrink-0 flex-wrap items-center justify-between gap-2 border-b border-blue-100 bg-[#f1f5fc] p-3"><h3 class="ui-section-title text-blue-900">Thống kê theo nhóm</h3><DashboardTypeSwitch v-model="filterType" label="Loại dữ liệu thống kê nhóm" /></header>
    <div class="group-results min-h-0 flex-1 overflow-y-auto overscroll-contain px-4" tabindex="0" role="region" aria-label="Kết quả các nhóm">
      <ul v-if="rows.length" class="divide-y divide-slate-200">
        <li v-for="row in rows" :key="row.group" class="py-4">
          <div class="mb-3 flex items-start justify-between gap-3"><h4 class="ui-section-title text-slate-900">{{ row.group }}</h4><span class="shrink-0 rounded-lg bg-blue-50 px-2 py-1 text-xs font-semibold text-blue-900">Tổng {{ formatNumber(row.total) }}</span></div>
          <div class="flex items-center gap-4">
            <div class="flex shrink-0 flex-col items-center gap-2"><MiniStatusDonut :stats="row" :size="72" :inner-size="56" :font-size="14"><template #center><strong class="text-sm font-bold tabular-nums text-slate-900">{{ row.completionRate == null ? '—' : formatPercent(row.completionRate) }}</strong></template></MiniStatusDonut><span class="text-xs font-medium text-slate-600">Hoàn thành</span></div>
            <dl class="min-w-0 flex-1 space-y-2"><div v-for="state in executionStatusSeries" :key="state.key" class="flex items-center justify-between gap-2 text-xs"><dt class="flex min-w-0 items-center gap-2 text-slate-700"><i class="h-2 w-2 shrink-0 rounded-full" :style="{background:themeColor(state.color)}" aria-hidden="true"></i>{{ state.label }}</dt><dd class="font-semibold tabular-nums" :class="state.value==='Completed'?'text-emerald-800':state.value==='InProgress'?'text-blue-800':'text-slate-900'">{{ formatNumber(row[state.key]) }}</dd></div></dl>
          </div>
        </li>
      </ul>
      <p v-else class="py-8 text-center text-xs text-slate-600">Chưa có dữ liệu nhóm theo bộ lọc hiện tại.</p>
    </div>
    <footer class="flex shrink-0 flex-wrap justify-between gap-2 border-t border-slate-100 px-4 py-3 text-xs text-slate-600"><span>{{ formatNumber(rows.length) }} nhóm</span><span>Mỗi mục được tính một lần</span></footer>
  </section>
</template>
<script setup>
import {computed,ref} from 'vue';
import DashboardTypeSwitch from './DashboardTypeSwitch.vue';
import MiniStatusDonut from '../../components/MiniStatusDonut.vue';
import {executionStatusSeries} from '../../shared/statusPresentation';
import {formatNumber,formatPercent} from '../../shared/formatters';
import {themeColor} from '../../shared/themeColors';
const props=defineProps({statistics:{type:Array,default:()=>[]}});
const filterType=ref('tasks');
const rows=computed(()=>props.statistics.filter(row=>row.itemType===(filterType.value==='goals'?'Goal':'Task')));
</script>
<style scoped>
.group-results{max-height:400px;}
.group-results :deep(.cursor-pointer){cursor:default;box-shadow:none;}
.group-results :deep(.hover\:scale-105:hover){transform:none;}
@media(min-width:1024px){.group-results{max-height:none;}}
</style>
