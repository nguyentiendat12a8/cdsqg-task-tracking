<template>
  <section class="relative min-w-0 rounded-2xl border border-blue-100 bg-white overflow-hidden" :aria-label="title">
    <header class="flex shrink-0 flex-wrap items-center justify-between gap-2 border-b border-blue-100 bg-[#f1f5fc] p-3"><h3 class="ui-section-title text-blue-900">{{ title }}</h3><DashboardTypeSwitch v-model="filterType" label="Loại dữ liệu thống kê" /></header>
    <div class="flex flex-col lg:flex-row lg:flex-wrap gap-3 lg:items-center lg:justify-between px-4 pt-3"><div class="flex flex-wrap gap-1 rounded-xl bg-blue-100/60 p-1 self-start" role="group" aria-label="Khối đơn vị"><button v-for="group in groups" :key="group.value" type="button" :aria-pressed="agencyType===group.value" @click="agencyType=group.value" class="ui-single-line rounded-lg px-3 font-semibold text-xs transition" :class="agencyType===group.value?'bg-white shadow-sm text-blue-700':'text-slate-600 hover:bg-white/60'">{{ group.label }}</button></div><div class="flex flex-col sm:flex-row gap-2 min-w-0"><input v-model="search" type="search" aria-label="Tìm đơn vị trong bảng thống kê" placeholder="Tìm tên đơn vị..." class="ui-control rounded-lg border border-slate-200 px-3 w-full sm:w-56" /><SearchableSelect v-model="sort" :options="sortOptions" :clearable="false" :is-multi="false" label="Sắp xếp đơn vị" label-class="sr-only" class="w-full sm:w-56" /></div></div>
    <div class="px-4 pt-3 flex flex-wrap gap-4 text-xs text-slate-700" aria-label="Chú giải trạng thái"><span v-for="state in executionStatusSeries" :key="state.key" class="flex items-center gap-2"><i class="h-3 w-3 rounded-sm" :style="{background:themeColor(state.color)}"></i>{{ state.label }}</span><span class="ml-auto">Chiều cao cột: số lượng được giao · Bên dưới: tỷ lệ hoàn thành</span></div>
    <div class="relative overflow-x-auto mx-4 my-3 px-2 pt-4 pb-3" tabindex="0" role="region" aria-label="Cuộn ngang để xem tất cả đơn vị">
      <div v-if="filtered.length" :style="{gridTemplateColumns:`36px repeat(${filtered.length}, minmax(72px,1fr))`}" class="comparison-chart" role="list" aria-label="Biểu đồ kết quả các đơn vị">
        <div class="comparison-axis" aria-hidden="true"><span>{{ formatNumber(maxTotal) }}</span><span>{{ formatNumber(maxTotal/2) }}</span><span>0</span></div>
        <button v-for="agency in filtered" :key="agency.id || agency.agencyId" type="button" role="listitem" class="comparison-column rounded-lg text-center hover:bg-blue-50 focus-visible:outline-2 focus-visible:outline-blue-700" @click="$emit('select-agency',agency,filterType)" :aria-label="`${agency.name}: ${executionStatusSeries.map(s=>s.label+' '+formatNumber(agency[s.key])).join(', ')}. Xem chi tiết`">
          <span class="comparison-plot"><span class="comparison-total font-semibold text-slate-700">{{ formatNumber(agency.totalItems) }}</span><span class="comparison-stack" :style="{height:visualShare(agency.totalItems,maxTotal)+'%'}"><span v-for="state in executionStatusSeries" :key="state.key" class="flex items-center justify-center text-xs font-semibold text-white overflow-hidden" :style="{height:visualShare(agency[state.key],agency.totalItems)+'%',background:themeColor(state.color)}"><span v-if="visualShare(agency[state.key],maxTotal)>8">{{ formatNumber(agency[state.key]) }}</span></span></span></span>
          <span class="comparison-name mt-2 font-medium text-xs text-slate-800 text-center" :title="agency.name">{{ agency.name }}</span>
          <strong class="block mt-1 text-xs text-emerald-700 tabular-nums">{{ agency.completionRate == null ? '—' : formatPercent(agency.completionRate) }} <span aria-hidden="true" class="text-xs">↗</span></strong>
        </button>
      </div>
      <p v-else class="py-8 text-center text-slate-600">{{ search ? 'Không tìm thấy đơn vị phù hợp.' : 'Chưa có dữ liệu phân công trong phạm vi này.' }}</p>
    </div>
    <footer class="flex flex-wrap items-center justify-between gap-2 border-t border-slate-100 px-4 py-3 text-xs text-slate-600"><span>{{ formatNumber(filtered.length) }} đơn vị · Toàn bộ kết quả theo bộ lọc</span><span>← Cuộn ngang để xem thêm đơn vị →</span></footer>
  </section>
</template>
<script setup>
import { computed, ref } from 'vue';
import DashboardTypeSwitch from './DashboardTypeSwitch.vue';
import { agencyForDashboard, visualShare } from './leadershipPresentation';
import { themeColor } from '../../shared/themeColors';
import SearchableSelect from '../../components/SearchableSelect.vue';
import { executionStatusSeries } from '../../shared/statusPresentation';
import { formatNumber, formatPercent } from '../../shared/formatters';
const props=defineProps({agencies:{type:Array,default:()=>[]},title:{type:String,default:'Thống kê kết quả các đơn vị'}});
defineEmits(['select-agency']);
const filterType=ref('tasks');
const typedAgencies=computed(()=>props.agencies.map(a=>agencyForDashboard(a,filterType.value)));
const search=ref(''),sort=ref('completion'),agencyType=ref('all');
const groups=[{value:'all',label:'Tất cả'},{value:'Ministry',label:'Bộ, ngành'},{value:'Province',label:'Địa phương'},{value:'Other',label:'Đơn vị khác'}];
const sortOptions=[{value:'completion',label:'Hoàn thành cao nhất'},{value:'assigned',label:'Khối lượng được giao'}];
const filtered=computed(()=>typedAgencies.value.filter(a=>(agencyType.value==='all'||(agencyType.value==='Other'?!['Ministry','Province'].includes(a.type):a.type===agencyType.value))&&a.name.toLocaleLowerCase('vi').includes(search.value.trim().toLocaleLowerCase('vi'))).sort((a,b)=>{
 const difference=sort.value==='completion'?(b.completionRate??-1)-(a.completionRate??-1):sort.value==='overdue'?(b.overdue||0)-(a.overdue||0):b.totalItems-a.totalItems;
 return difference||b.totalItems-a.totalItems||a.name.localeCompare(b.name,'vi');
}));
const maxTotal=computed(()=>Math.max(1,...filtered.value.map(a=>a.totalItems)));
</script>

<style scoped>
.comparison-chart{display:grid;gap:8px;min-width:100%;align-items:start;}
.comparison-axis{height:228px;display:flex;flex-direction:column;justify-content:space-between;text-align:right;padding:0 6px 0 0;color:var(--color-slate-600);font-size:var(--ui-font-body);}
.comparison-column{min-width:0;padding:0 2px 6px;}
.comparison-name{display:-webkit-box;-webkit-box-orient:vertical;-webkit-line-clamp:2;line-clamp:2;overflow:hidden;overflow-wrap:anywhere;line-height:1rem;height:2rem;}
.comparison-plot{height:228px;padding-top:28px;display:flex;flex-direction:column;justify-content:flex-end;align-items:center;position:relative;border-bottom:1px solid var(--color-slate-300);background:repeating-linear-gradient(to top,transparent 0,transparent 99px,var(--color-slate-100) 99px,var(--color-slate-100) 100px);}
.comparison-stack{display:flex;flex-direction:column-reverse;width:28px;max-width:32px;flex-shrink:0;overflow:hidden;border-radius:4px 4px 0 0;}
.comparison-total{font-size:var(--ui-font-body);line-height:18px;}
</style>
