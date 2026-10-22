<template>
  <section :aria-label="`Tổng quan ${noun}`" class="min-w-0 space-y-2">
    <div class="grid grid-cols-2 md:grid-cols-4 gap-3 items-stretch">
      <button v-for="card in cards" :key="card.key" :data-tone="card.key" type="button" :disabled="!card.value" @click="select(card.status)"
        :aria-label="card.status ? `Xem ${card.label} — ${noun}` : `Xem toàn bộ ${noun}`"
        class="executive-kpi group text-left rounded-2xl border border-slate-200 bg-white p-2.5 transition hover:border-blue-300 hover:shadow-sm disabled:cursor-default">
        <div class="flex items-center justify-between gap-2"><span class="text-xs font-semibold text-slate-600">{{ card.label }}</span><span class="flex h-7 w-7 shrink-0 items-center justify-center rounded-xl" :class="card.iconClass"><svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path :d="card.path" stroke-linecap="round" stroke-linejoin="round" /></svg></span></div>
        <div class="mt-1 flex items-end gap-2"><strong class="kpi-value text-[1.75rem] leading-9 font-bold tabular-nums">{{ formatNumber(card.value) }}</strong><span class="mb-1 text-xs text-slate-500">{{ noun }}</span></div>
        <div class="mt-2 h-1 w-full rounded-full bg-slate-100 overflow-hidden"><div class="h-full rounded-full transition-all duration-300" :class="card.barClass" :style="{width: `${visualShare(card.value,total)}%`}"></div></div>
      </button>
    </div>
    <div class="rounded-xl border border-slate-200 bg-white p-3 space-y-2" aria-label="Phân công và lượt giao">
      <p class="text-xs text-slate-600">Một mục tiêu hoặc nhiệm vụ giao cho một đơn vị được tính là một lượt giao.</p>
      <div class="flex flex-wrap items-center justify-between gap-2">
        <button type="button" @click="$emit('select-statistic',{mode:'assignments',scope:'all',status:null})" class="flex flex-wrap items-center gap-x-2 gap-y-1 rounded-md text-slate-800 hover:text-blue-700 text-xs" aria-label="Xem toàn bộ lượt giao theo cơ quan">
          <span class="font-bold text-slate-900">{{ noun === 'mục tiêu' ? 'Mục tiêu' : 'Nhiệm vụ' }}:</span>
          <strong class="text-blue-800 tabular-nums">{{ formatNumber(assignmentTotal) }} lượt giao</strong>
          <span class="text-slate-600">/ {{ formatNumber(unitCount) }} đơn vị</span>
          <span aria-hidden="true">↗</span>
        </button>
        <div v-if="scopeCounts" class="flex flex-wrap gap-2 text-xs" aria-label="Phạm vi giao"><button type="button" @click="$emit('select-statistic',{mode:'items',scope:'specific',status:null})" class="rounded-md bg-blue-50 px-2 py-1 font-medium text-blue-900 hover:bg-blue-100">Giao riêng <strong class="ml-1 tabular-nums">{{ formatNumber(scopeCounts.specific) }}</strong></button><button type="button" @click="$emit('select-statistic',{mode:'items',scope:'general',status:null})" class="rounded-md bg-slate-100 px-2 py-1 font-medium text-slate-800 hover:bg-slate-200">Giao chung <strong class="ml-1 tabular-nums">{{ formatNumber(scopeCounts.general) }}</strong></button></div>
      </div>
      <div class="grid grid-cols-3 gap-2"><button v-for="state in executionStatusSeries" :key="state.key" type="button" @click="$emit('select-statistic',{mode:'assignments',scope:'all',status:state.value})" class="flex flex-wrap items-center justify-between gap-1 rounded-lg border px-2 py-2 text-left text-xs hover:shadow-sm" :class="state.value==='Completed'?'border-emerald-200 bg-emerald-50 text-emerald-900':state.value==='InProgress'?'border-blue-200 bg-blue-50 text-blue-900':'border-slate-200 bg-slate-100 text-slate-800'" :aria-label="`Xem ${state.label} — lượt giao theo cơ quan`"><span class="font-medium">{{ state.label }}</span><strong class="tabular-nums ui-section-title">{{ formatNumber(assignmentSummary[state.key] ?? 0) }}</strong></button></div>
    </div>
  </section>
</template>
<script setup>
import { computed } from 'vue';
import { executionStatusSeries } from '../../shared/statusPresentation';
import { formatNumber } from '../../shared/formatters';
import { visualShare } from './leadershipPresentation';
const props=defineProps({summary:{type:Object,default:()=>({})},assignmentSummary:{type:Object,default:()=>({})},total:{type:Number,default:0},unitCount:{type:Number,default:0},noun:{type:String,default:'mục tiêu'},scopeCounts:{type:Object,default:null}});
const emit=defineEmits(['select-statistic']);
const assignmentTotal=computed(()=>executionStatusSeries.reduce((sum,s)=>sum+(props.assignmentSummary[s.key]||0),0));
const cards=computed(()=>[
 {key:'total',status:null,label:'Tổng '+props.noun,value:props.total,iconClass:'bg-sky-100 text-sky-700',barClass:'bg-sky-500',path:'M4 4h6v6H4zM14 4h6v6h-6zM4 14h6v6H4zM14 14h6v6h-6z'},
 ...executionStatusSeries.map(s=>({key:s.key,status:s.value,label:s.label,value:props.summary[s.key]??0,iconClass:({slate:'bg-slate-100 text-slate-600',blue:'bg-blue-50 text-blue-700',emerald:'bg-emerald-50 text-emerald-700'})[s.tone],barClass:({slate:'bg-slate-400',blue:'bg-blue-500',emerald:'bg-emerald-500'})[s.tone],path:s.value==='Completed'?'m6 12 4 4 8-8':s.value==='InProgress'?'M12 3a9 9 0 1 1-9 9M12 7v5l3 2':'M12 8v4m0 4h.01M4 5h16v14H4z'}))
]);
function select(status){emit('select-statistic',{mode:'items',scope:'all',status});}
</script>

<style scoped>
.executive-kpi{--kpi-accent:var(--color-slate-600);--kpi-surface:var(--color-slate-50);background:var(--kpi-accent);color:white;border-color:color-mix(in srgb,var(--kpi-accent) 22%,white);border-top:3px solid var(--kpi-accent);box-shadow:0 2px 6px rgb(15 23 42 / .025);}
.executive-kpi[data-tone="total"]{--kpi-accent:var(--color-sky-700);--kpi-surface:var(--color-sky-50);}
.executive-kpi[data-tone="inProgress"]{--kpi-accent:var(--color-blue-700);--kpi-surface:var(--color-blue-50);}
.executive-kpi[data-tone="completed"]{--kpi-accent:var(--color-emerald-700);--kpi-surface:var(--color-emerald-50);}
.executive-kpi .kpi-value,.executive-kpi span{color:white;}
.executive-kpi .bg-slate-100{background:rgb(255 255 255 / .22);}
.executive-kpi .mt-2 > div{background:white;}
.executive-kpi span.flex{background:rgb(255 255 255 / .18);}
.executive-kpi:hover:not(:disabled){border-color:var(--kpi-accent);}
</style>
