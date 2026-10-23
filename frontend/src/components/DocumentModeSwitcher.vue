<template>
  <OverlayPanel title="Chọn chiến lược" placement="bottom-end" width-class="w-[440px] max-w-[calc(100vw-2rem)]" :show-actions="false" panel-class="strategy-options" @show="focusOptions">
    <template #trigger="{isOpen,setTriggerRef,close}">
      <button :ref="setTriggerRef" type="button" @keydown.esc.stop.prevent="close" class="strategy-trigger ui-control inline-flex items-center gap-2 rounded-lg border px-3 text-left transition focus-visible:outline-2 focus-visible:outline-blue-600" :class="isOpen?'border-blue-400 bg-blue-50 text-blue-800':'border-blue-300 bg-white text-blue-900 hover:border-blue-500 hover:bg-blue-50'" aria-label="Chọn chiến lược" aria-haspopup="true" :aria-expanded="isOpen">
        <svg class="h-4 w-4 shrink-0 text-blue-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M4 4h7v7H4zM14 4h6v6h-6zM4 14h6v6H4zM14 14h6v6h-6z" stroke-linejoin="round" /></svg>
        <span class="hidden sm:inline text-xs text-slate-700 font-medium">{{ isDashboard && dashboardDocumentScope === 'all' ? 'Trang chủ' : activeDocument.shortTitle }}</span><span class="hidden sm:block h-4 w-px bg-slate-200" aria-hidden="true"></span>
        <span class="text-xs font-semibold whitespace-nowrap">{{ isDashboard && dashboardDocumentScope === 'all' ? 'Tổng' : activeDocument.documentNumber }}</span>
        <svg class="h-4 w-4 shrink-0 text-slate-500 transition-transform" :class="{'rotate-180':isOpen}" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="m7 10 5 5 5-5" stroke-linecap="round" stroke-linejoin="round" /></svg>
      </button>
    </template>
    <template #heading><div><h3 class="text-sm font-semibold text-slate-900">Chiến lược &amp; chương trình</h3><p class="mt-1 text-xs text-slate-700">Chọn phạm vi dữ liệu của hệ thống</p></div></template>
    <template #default="{close}">
      <div v-if="countsError" role="alert" class="mb-3 flex flex-wrap items-center justify-between gap-2 rounded-lg border border-amber-200 bg-amber-50 p-2 text-xs text-amber-900"><span>Chưa tải được số lượng mục tiêu, nhiệm vụ.</span><button type="button" class="ui-btn ui-btn-secondary" @click.stop="loadCounts">Thử lại</button></div>
      <div ref="optionsRef" class="space-y-2" role="group" aria-label="Danh sách chiến lược" @keydown="handleKeys">
        <button v-if="isDashboard" type="button" :aria-pressed="dashboardDocumentScope==='all'" @click="handleSelect('all',close)" class="strategy-option group w-full rounded-xl border p-4 text-left transition focus-visible:outline-2 focus-visible:outline-blue-600" :class="dashboardDocumentScope==='all'?'border-blue-700 bg-blue-700 text-white':'border-slate-300 bg-white text-slate-900 hover:border-blue-500 hover:bg-blue-50'">
          <span class="block text-sm font-bold">Tổng quan hai văn bản</span>
          <span class="mt-1 block text-xs">Chiến lược 1266/QĐ-TTg và Chương trình 2629/QĐ-TTg</span>
          <span class="strategy-meta mt-3 flex flex-wrap gap-3 rounded-lg bg-slate-100 px-3 py-2 text-xs font-medium text-slate-800"><span>{{ combinedCountText('totalGoals') }} mục tiêu</span><span>{{ combinedCountText('totalTasks') }} nhiệm vụ</span></span>
          <span v-if="dashboardDocumentScope==='all'" class="mt-2 inline-flex rounded-full bg-blue-100 px-2 py-1 text-xs font-semibold text-blue-900">Đang chọn</span>
        </button>
        <button v-for="doc in SUPPORTED_DOCUMENTS" :key="doc.code" type="button" :aria-pressed="(!isDashboard || dashboardDocumentScope!=='all') && doc.code===activeDocument.code" @click="handleSelect(doc.code,close)" class="strategy-option group w-full rounded-xl border p-4 text-left transition focus-visible:outline-2 focus-visible:outline-blue-600" :class="(!isDashboard || dashboardDocumentScope!=='all') && doc.code===activeDocument.code?'border-blue-700 bg-blue-700 text-white':'border-slate-300 bg-white text-slate-900 hover:border-blue-500 hover:bg-blue-50'">
          <div class="flex items-center gap-3"><span class="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-xs font-bold" :class="(!isDashboard || dashboardDocumentScope!=='all') && doc.code===activeDocument.code?'bg-blue-600 text-white':'bg-slate-100 text-slate-600'"><svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M14 3H6a1 1 0 0 0-1 1v16a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V8l-5-5Z" stroke-linejoin="round"/><path d="M14 3v5h5M8 12h8M8 16h6" stroke-linecap="round" stroke-linejoin="round"/></svg></span><div class="min-w-0 flex-1"><span class="block text-xs font-semibold text-slate-900">{{ doc.documentNumber }}</span><span class="mt-1 block text-xs text-slate-700">{{ doc.shortTitle }}</span></div><span v-if="(!isDashboard || dashboardDocumentScope!=='all') && doc.code===activeDocument.code" class="inline-flex shrink-0 items-center justify-center gap-1 rounded-full bg-blue-100 px-2 py-1 text-blue-900 text-xs font-semibold" aria-label="Đang chọn"><svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="m6 12 4 4 8-8" stroke-linecap="round" stroke-linejoin="round" /></svg>Đang chọn</span></div>
          <p class="mt-3 text-xs leading-relaxed font-medium text-slate-800">{{ doc.title }}</p>
          <div class="strategy-meta mt-3 flex flex-wrap items-center gap-x-4 gap-y-2 rounded-lg bg-slate-100 px-3 py-2 text-xs font-medium text-slate-800"><span>{{ doc.startYear }} – {{ doc.endYear }}</span><span class="inline-flex items-center gap-1.5"><svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><circle cx="12" cy="12" r="8"/><circle cx="12" cy="12" r="4"/></svg>{{ countText(doc, 'totalGoals') }} mục tiêu</span><span>{{ countText(doc, 'totalTasks') }} nhiệm vụ</span></div>
        </button>
      </div>
    </template>
  </OverlayPanel>
</template>
<script setup>
import OverlayPanel from './OverlayPanel.vue';
import { nextTick, ref, onUnmounted } from 'vue';
import { loadAllExportPages } from '../services/exportData';
import { fetchWithAuth } from '../services/auth';
import { getApiUrl } from '../config/api';
import { formatNumber } from '../shared/formatters';
import { SUPPORTED_DOCUMENTS, activeDocument, dashboardDocumentScope, switchDocument } from '../services/documentContext';
defineProps({isDashboard:{type:Boolean,default:false}});
const optionsRef = ref(null);
const counts = ref({}), countsLoading = ref(false), countsError = ref(false);
let countsVersion = 0, countsController;
onUnmounted(() => { countsVersion++; countsController?.abort(); });
function countText(doc, key) {
  if (countsLoading.value) return '…';
  const value = counts.value[doc.id]?.[key];
  return Number.isInteger(value) && value >= 0 ? formatNumber(value) : '—';
}
function combinedCountText(key) {
  if (countsLoading.value) return '…';
  const values = SUPPORTED_DOCUMENTS.map(doc => counts.value[doc.id]?.[key]);
  return values.every(value => Number.isInteger(value) && value >= 0) ? formatNumber(values.reduce((sum, value) => sum + value, 0)) : '—';
}
async function loadCounts() {
  const version = ++countsVersion;
  countsController?.abort(); const controller = countsController = new AbortController();
  counts.value = {}; countsLoading.value = true; countsError.value = false;
  try {
    const documents = await loadAllExportPages(getApiUrl('/api/documents'), {fetchPage: url => fetchWithAuth(url, {signal:controller.signal})});
    if (version === countsVersion) counts.value = Object.fromEntries(documents.map(doc => [doc.id, {totalGoals:doc.totalGoals,totalTasks:doc.totalTasks}]));
  } catch {
    if (version === countsVersion) countsError.value = true;
  } finally {
    if (version === countsVersion) countsLoading.value = false;
  }
}
async function focusOptions(){loadCounts();await nextTick(); optionsRef.value?.querySelector('[aria-pressed="true"]')?.focus();}
function handleKeys(event){
 if(!['ArrowDown','ArrowUp','Home','End'].includes(event.key))return;
 event.preventDefault();
 const buttons=[...optionsRef.value.querySelectorAll('button')];
 const index=buttons.indexOf(document.activeElement);
 const next=event.key==='Home'?0:event.key==='End'?buttons.length-1:(index+(event.key==='ArrowDown'?1:-1)+buttons.length)%buttons.length;
 buttons[next]?.focus();
}
function handleSelect(code,close){if(code==='all')dashboardDocumentScope.value='all';else switchDocument(code);close();}
</script>
<style>
.strategy-options{background:white;border:1px solid var(--color-blue-200);box-shadow:0 16px 48px rgb(15 23 42 / .2);}
.strategy-option[aria-pressed="true"] p,.strategy-option[aria-pressed="true"] .min-w-0 > span{color:white;}
.strategy-option[aria-pressed="true"] .strategy-meta{background:rgb(255 255 255 / .14);color:white;}
.strategy-option[aria-pressed="true"]:focus-visible{outline-color:var(--color-blue-900);outline-offset:3px;}
</style>
