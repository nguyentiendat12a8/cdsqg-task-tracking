<template>
  <aside class="flex flex-col gap-3 min-w-0">
    <section class="rounded-2xl border border-amber-200 bg-white p-3" aria-labelledby="watchlist-title">
      <div class="flex items-center justify-between gap-2"><div><h3 id="watchlist-title" class="ui-section-title text-amber-700">Theo dõi thời hạn</h3></div><span class="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-50 text-amber-700"><svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M12 8v4m0 4h.01M10 4 2 18a1 1 0 0 0 1 2h18a1 1 0 0 0 1-2L14 4a2 2 0 0 0-4 0Z" stroke-linecap="round" stroke-linejoin="round" /></svg></span></div>
      <div class="mt-3 grid grid-cols-2 gap-2"><div class="flex items-center justify-between gap-2 rounded-xl border border-rose-100 bg-rose-50/60 p-2"><div class="text-xs text-rose-700 font-semibold">Quá hạn</div><strong class="block text-[1.75rem] leading-9 tabular-nums text-rose-800">{{ formatNumber(overdue) }}</strong></div><div class="flex items-center justify-between gap-2 rounded-xl border border-amber-100 bg-amber-50/60 p-2"><div class="text-xs text-amber-700 font-semibold">Sắp hết hạn</div><strong class="block text-[1.75rem] leading-9 tabular-nums text-amber-800">{{ formatNumber(expiring) }}</strong></div></div>
      <p class="mt-2 text-xs text-slate-500">Lượt giao chưa hoàn thành theo thời hạn.</p>
      <div class="mt-2 h-24 overflow-y-auto overscroll-contain border-t border-slate-100 pt-2" role="region" aria-label="Danh sách đơn vị quá hạn" tabindex="0">
      <div v-if="priority.length" class="space-y-1"><button v-for="agency in priority" :key="agency.agencyId" type="button" class="flex w-full items-center justify-between gap-3 rounded-lg py-1 text-left hover:bg-slate-50" @click="$emit('select-overdue-agency',agency)"><span class="text-xs font-semibold text-slate-700">{{ agency.name }}</span><span class="shrink-0 rounded-md bg-rose-50 px-2 py-1 text-xs font-semibold text-rose-700">{{ formatNumber(agency.overdue) }} quá hạn</span></button></div>
      <div v-else class="flex h-full items-center gap-2 text-xs text-emerald-800"><span aria-hidden="true">✓</span> Chưa ghi nhận lượt giao quá hạn.</div>
      </div>
    </section>
    <section v-if="showCoverage" class="rounded-2xl border border-blue-200 bg-white p-3 flex-1" aria-labelledby="coverage-title"><h3 id="coverage-title" class="ui-section-title text-blue-900">Phân bổ theo khối</h3><div class="mt-3 space-y-2"><div v-for="group in groups" :key="group.label" class="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-xs"><div class="flex items-center justify-between gap-3"><span class="font-semibold text-slate-800">{{ group.label }}</span><span class="text-slate-700">{{ formatNumber(group.units) }} đơn vị · <strong class="font-semibold text-slate-900">{{ formatNumber(group.total) }} lượt</strong></span></div><div class="mt-2 h-2 rounded-full bg-slate-200 overflow-hidden"><div class="h-full rounded-full" :class="group.color" :style="{width:`${visualShare(group.total,assignmentTotal)}%`}"></div></div></div></div>
      <div class="mt-3 border-t border-slate-100 pt-2" aria-label="Đơn vị nổi bật"><div class="text-xs font-semibold text-slate-700">Đơn vị nổi bật</div><div v-if="leaders.length" class="mt-1 space-y-1"><button v-for="agency in leaders" :key="agency.agencyId" type="button" class="flex w-full items-center justify-between gap-3 rounded-lg py-1 text-left text-xs hover:text-blue-700" @click="$emit('select-agency',agency)"><span class="min-w-0 text-slate-600">{{ agency.name }}</span><strong class="shrink-0 text-emerald-700 tabular-nums">{{ formatNumber(agency.totalItems) }} lượt giao</strong></button></div><p v-else class="mt-1 text-xs text-slate-500">Chưa có đơn vị được giao {{ noun }}.</p></div>
    </section>

  </aside>
</template>
<script setup>
import { computed } from 'vue';
import { formatNumber } from '../../shared/formatters';
import { visualShare } from './leadershipPresentation';
const props=defineProps({showCoverage:{type:Boolean,default:true},agencies:{type:Array,default:()=>[]},leaders:{type:Array,default:()=>[]},noun:{type:String,default:'mục tiêu'}});
defineEmits(['select-agency','select-overdue-agency']);
const overdue=computed(()=>props.agencies.reduce((sum,a)=>sum+(a.overdue||0),0));
const expiring=computed(()=>props.agencies.reduce((sum,a)=>sum+(a.expiringSoon||0),0));
const priority=computed(()=>props.agencies.filter(a=>a.overdue>0).sort((a,b)=>b.overdue-a.overdue||a.name.localeCompare(b.name,'vi')));
const assignmentTotal=computed(()=>props.agencies.reduce((sum,a)=>sum+a.totalItems,0));
const groups=computed(()=>[
 {type:'Ministry',label:'Bộ, ngành',color:'bg-blue-500'},
 {type:'Province',label:'Địa phương',color:'bg-emerald-500'},
 {type:'Other',label:'Đơn vị khác',color:'bg-violet-500'}
].map(g=>{const rows=props.agencies.filter(a=>g.type==='Other'?!['Ministry','Province'].includes(a.type):a.type===g.type);return {...g,units:rows.filter(a=>a.totalItems>0).length,total:rows.reduce((sum,a)=>sum+a.totalItems,0)};}));
</script>
