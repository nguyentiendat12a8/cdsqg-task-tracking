<template>
  <section class="agency-work-queue flex min-h-0 min-w-0 flex-col overflow-hidden rounded-2xl border border-blue-200 bg-white" aria-label="Công việc của đơn vị">
    <header class="flex shrink-0 flex-wrap items-center justify-between gap-3 border-b border-blue-100 bg-blue-50 p-4">
      <div class="min-w-0">
        <h3 class="ui-section-title text-blue-900">Công việc của đơn vị</h3>
        <p class="mt-1 text-xs text-slate-700">{{ agencyName || 'Đơn vị của bạn' }}</p>
      </div>
      <DashboardTypeSwitch v-model="filterType" label="Loại công việc của đơn vị" />
    </header>
    <div class="flex shrink-0 flex-wrap gap-2 border-b border-slate-100 p-3">
      <input v-model="search" type="search" aria-label="Tìm công việc của đơn vị" placeholder="Tìm mã hoặc tên công việc..." class="ui-control min-w-0 flex-1 rounded-lg border border-slate-200 px-3" />
      <SearchableSelect v-model="status" :options="[{value:'all',label:'Tất cả trạng thái'},...executionStatusOptions]" :clearable="false" :is-multi="false" label="Trạng thái công việc" label-class="sr-only" class="w-full sm:w-48" />
    </div>
    <div v-if="loading" role="status" class="min-h-0 flex-1 overflow-auto p-8"><LoadingSpinner message="Đang tải công việc của đơn vị..." /></div>
    <div v-else-if="error" role="alert" class="min-h-0 flex-1 overflow-auto flex flex-wrap items-center justify-between gap-2 p-4 text-sm text-rose-800">
      <span>{{ error }}</span>
      <button type="button" class="ui-btn ui-btn-secondary" @click="load">Thử lại</button>
    </div>
    <div v-else class="agency-work-items min-h-0 max-h-[520px] flex-1 overflow-y-auto overscroll-contain">
      <ul class="divide-y divide-slate-100">
        <li v-for="item in items" :key="item.id">
          <button type="button" class="flex w-full flex-wrap items-start justify-between gap-3 p-4 text-left hover:bg-blue-50 focus-visible:outline-2 focus-visible:outline-blue-700" @click="$emit('select-item',item)">
            <div class="min-w-0 flex-1">
              <span class="text-xs font-semibold text-blue-800">{{ item.code }}</span>
              <h4 class="mt-1 text-sm font-semibold text-slate-900">{{ item.title }}</h4>
              <div class="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-slate-600">
                <span>Hạn: {{ formatDate(item.dueDate) || 'Chưa xác định' }}</span>
                <span v-if="item.deadlineStatus==='Overdue'" class="font-semibold text-rose-700">Quá hạn</span>
                <span v-else-if="item.deadlineStatus==='ExpiringSoon'" class="font-semibold text-amber-700">Sắp hết hạn</span>
              </div>
            </div>
            <span class="rounded-full border px-2 py-1 text-xs font-semibold" :class="getStatusBadgeClass(item.status || item.calculatedStatus)">{{ getStatusLabel(item.status || item.calculatedStatus) }}</span>
          </button>
        </li>
      </ul>
      <p v-if="!items.length" class="p-8 text-center text-sm text-slate-600">{{ search.trim() || status!=='all' ? 'Không có công việc phù hợp với bộ lọc.' : 'Chưa có công việc được giao trong phạm vi này.' }}</p>
    </div>
    <!-- Server-side Pagination Footer -->
    <footer class="flex flex-col sm:flex-row items-center justify-between gap-3 border-t border-slate-100 bg-slate-50/60 px-4 py-3 text-xs text-slate-600 font-semibold shrink-0">
      <div class="flex items-center gap-3 whitespace-nowrap flex-wrap justify-center sm:justify-start">
        <span>Hiển thị <span class="font-bold text-slate-900">{{ formatNumber(totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0) }} - {{ formatNumber(Math.min(pageNumber * pageSize, totalCount)) }}</span> trên tổng số <span class="font-bold text-slate-900">{{ formatNumber(totalCount) }}</span> {{ filterType==='goals'?'mục tiêu':'nhiệm vụ' }}</span>
        <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3">
          <span>Số bản ghi:</span>
          <SearchableSelect
            v-model="pageSize"
            :options="pageSizeOptions"
            :is-multi="false"
            :clearable="false"
            label="Số bản ghi mỗi trang"
            label-class="sr-only"
            class="w-20"
          />
        </div>
      </div>
      <div class="flex items-center gap-2 shrink-0 whitespace-nowrap">
        <button
          type="button"
          @click="changePage(pageNumber - 1)"
          :disabled="loading || pageNumber <= 1"
          class="ui-btn ui-btn-secondary px-3 py-1.5 rounded-lg disabled:opacity-40 font-semibold shadow-2xs"
        >
          ‹ Trang trước
        </button>
        <span class="px-2.5 py-1 bg-blue-50 text-blue-800 border border-blue-200 rounded-lg font-bold">
          Trang {{ formatNumber(pageNumber) }} / {{ formatNumber(Math.max(1, totalPages)) }}
        </span>
        <button
          type="button"
          @click="changePage(pageNumber + 1)"
          :disabled="loading || pageNumber >= totalPages"
          class="ui-btn ui-btn-secondary px-3 py-1.5 rounded-lg disabled:opacity-40 font-semibold shadow-2xs"
        >
          Trang sau ›
        </button>
      </div>
    </footer>
  </section>
</template>
<script setup>
import {ref,watch,onUnmounted} from 'vue';
import DashboardTypeSwitch from './DashboardTypeSwitch.vue';
import SearchableSelect from '../../components/SearchableSelect.vue';
import LoadingSpinner from '../../components/LoadingSpinner.vue';
import {executionStatusOptions} from '../../shared/statusPresentation';
import {getStatusBadgeClass,getStatusLabel,formatDate} from './presentation';
import {formatNumber} from '../../shared/formatters';
import {fetchDashboardAgencyItems} from './agencyItemsApi';

const props=defineProps({agencyId:{type:String,default:''},agencyName:{type:String,default:''},query:{type:String,default:''}});
defineEmits(['select-item']);

const filterType=ref('tasks');
const search=ref('');
const debouncedSearch=ref('');
const status=ref('all');
const items=ref([]);
const totalCount=ref(0);
const totalPages=ref(1);
const pageNumber=ref(1);
const pageSize=ref(10);
const pageSizeOptions=[10,25,50,100].map(n=>({value:n,label:String(n)}));
const loading=ref(false);
const error=ref('');

let version=0,controller,searchTimer=null;

onUnmounted(()=>{
  version++;
  controller?.abort();
  if(searchTimer)clearTimeout(searchTimer);
});

watch(search,(val)=>{
  if(searchTimer)clearTimeout(searchTimer);
  searchTimer=setTimeout(()=>{
    debouncedSearch.value=val.trim();
  },300);
});

function changePage(p){
  if(p<1 || (totalPages.value>0 && p>totalPages.value))return;
  pageNumber.value=p;
  load();
}

async function load(){
  const current=++version;
  controller?.abort();
  controller=new AbortController();
  error.value='';
  if(!props.agencyId){
    loading.value=false;
    items.value=[];
    totalCount.value=0;
    totalPages.value=1;
    error.value='Tài khoản chưa được gắn đơn vị.';
    return;
  }
  loading.value=true;
  try{
    const query=new URLSearchParams(props.query);
    query.set('agencyId',props.agencyId);
    query.delete('parentAgencyId');
    query.set('itemType',filterType.value==='goals'?'Goal':'Task');
    if(status.value && status.value!=='all'){
      query.set('status',status.value);
    }else{
      query.delete('status');
    }
    if(debouncedSearch.value){
      query.set('q',debouncedSearch.value);
    }else{
      query.delete('q');
    }
    query.set('pageNumber',String(pageNumber.value));
    query.set('pageSize',String(pageSize.value));

    const data=await fetchDashboardAgencyItems(query,{signal:controller.signal});
    if(current===version){
      if(Array.isArray(data)){
        items.value=data;
        totalCount.value=data.length;
        totalPages.value=Math.max(1,Math.ceil(data.length/pageSize.value));
      }else if(data && Array.isArray(data.items)){
        items.value=data.items;
        totalCount.value=data.totalCount ?? data.items.length;
        totalPages.value=data.totalPages ?? Math.max(1,Math.ceil((data.totalCount||1)/pageSize.value));
        pageNumber.value=data.pageNumber ?? pageNumber.value;
      }
    }
  }catch(cause){
    if(current===version){
      error.value=cause.message||'Không tải được công việc của đơn vị.';
      items.value=[];
      totalCount.value=0;
      totalPages.value=1;
    }
  }finally{
    if(current===version)loading.value=false;
  }
}

watch([()=>props.query,()=>props.agencyId,filterType,status,debouncedSearch,pageSize],()=>{
  pageNumber.value=1;
  load();
},{immediate:true});
</script>
