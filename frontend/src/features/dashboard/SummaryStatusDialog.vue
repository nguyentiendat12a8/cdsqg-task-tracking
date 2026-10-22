<template>
  <div v-accessible-dialog="() => emit('close')" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-3 sm:p-4" @click.self="emit('close')">
    <section class="bg-white rounded-2xl border border-slate-200 shadow-xl w-full max-w-7xl max-h-[90vh] flex flex-col p-4 sm:p-5 gap-3 min-w-0">
      <header class="flex items-start justify-between gap-3 border-b border-slate-100 pb-3">
        <div><h3 class="text-sm font-semibold text-slate-900">{{ title }}</h3><p class="text-xs text-slate-500 mt-1">{{ selection.mode === 'assignments' ? 'Mỗi dòng là một lượt giao cho một cơ quan; cùng mã có thể xuất hiện nhiều lần.' : 'Mỗi mục tiêu hoặc nhiệm vụ hiển thị một lần.' }}</p></div>
        <button type="button" aria-label="Đóng danh sách thống kê" class="ui-btn ui-btn-secondary shrink-0" @click="emit('close')">Đóng</button>
      </header>
      <div class="flex flex-wrap items-center gap-3">
        <input v-model="search" class="ui-single-line border border-slate-200 rounded-lg px-3 w-full sm:w-96" aria-label="Tìm trong danh sách thống kê" placeholder="Tìm mã, tên, cơ quan..." />
        <span v-if="!loading && !error" class="text-xs text-slate-600">{{ formatNumber(total) }} bản ghi</span>
      </div>
      <div v-if="error" role="alert" class="text-xs text-rose-700">{{ error }} <button class="underline" type="button" @click="load">Thử lại</button></div>
      <LoadingSpinner v-else-if="loading" text="Đang tải danh sách chi tiết..." padding="py-8" />
      <div v-else class="overflow-auto min-h-0 border border-slate-200 rounded-xl">
        <table class="w-full text-xs text-left border-collapse">
          <thead class="sticky top-0 bg-slate-100 text-slate-700"><tr><th class="p-3">STT</th><th class="p-3">Mã</th><th class="p-3 min-w-64">{{ itemType === 'Goal' ? 'Tên mục tiêu' : 'Tên nhiệm vụ' }}</th><th class="p-3 min-w-44">Cơ quan chủ trì</th><th v-if="itemType === 'Goal'" class="p-3 whitespace-nowrap">Tiến độ</th><th class="p-3 min-w-44">Trạng thái</th><th class="p-3">Chi tiết</th></tr></thead>
          <tbody class="divide-y divide-slate-200"><tr v-for="(item, index) in rows" :key="`${item.id}-${item.progressAgencyId || 'item'}`" class="hover:bg-slate-50">
            <td class="p-3">{{ formatNumber((page-1)*pageSize+index+1) }}</td><td class="p-3 whitespace-nowrap text-blue-700">{{ item.code }}</td><td class="p-3">{{ item.title }}</td><td class="p-3">{{ item.leadAgencyName }}</td>
            <td v-if="itemType === 'Goal'" class="p-3 whitespace-nowrap">{{ formatItemProgressDisplay(item) }}</td>
            <td class="p-3"><span :class="['inline-block rounded-full px-2 py-0.5', getStatusBadgeClass(item.calculatedStatus)]">{{ getStatusLabel(item.calculatedStatus) }}</span></td>
            <td class="p-3"><button type="button" class="ui-btn ui-btn-secondary whitespace-nowrap" @click="emit('select-item',item)">Chi tiết</button></td>
          </tr><tr v-if="!rows.length"><td :colspan="itemType === 'Goal' ? 7 : 6" class="p-6 text-center text-slate-500">Không có bản ghi phù hợp.</td></tr></tbody>
        </table>
      </div>
      <footer v-if="!error && !loading" class="flex items-center justify-end gap-2 text-xs border-t border-slate-100 pt-3">
        <button type="button" class="ui-btn ui-btn-secondary" :disabled="loading || page<=1" @click="page--">Trang trước</button>
        <span>Trang {{ formatNumber(page) }} / {{ formatNumber(Math.max(1,totalPages)) }}</span>
        <button type="button" class="ui-btn ui-btn-secondary" :disabled="loading || page>=totalPages" @click="page++">Trang sau</button>
      </footer>
    </section>
  </div>
</template>
<script setup>
import { computed, ref, watch, onUnmounted } from 'vue';
import LoadingSpinner from '../../components/LoadingSpinner.vue';
import { fetchWithAuth } from '../../services/auth';
import { getApiUrl } from '../../config/api';
import { formatNumber } from '../../shared/formatters';
import { getStatusLabel, getStatusBadgeClass, formatItemProgressDisplay } from './presentation';
const props = defineProps({ selection:{type:Object,required:true}, query:{type:String,default:''}, itemType:{type:String,default:'Goal'} });
const emit = defineEmits(['close','select-item']);
const title = computed(() => props.selection.mode==='assignments'?'Lượt giao theo cơ quan':'Danh sách '+(props.itemType==='Goal'?'mục tiêu':'nhiệm vụ')+(props.selection.scope==='general'?' giao chung':props.selection.scope==='specific'?' giao riêng':''));
const pageSize=10;
const loading=ref(true), error=ref(''), rows=ref([]), total=ref(0), totalPages=ref(0), page=ref(1), search=ref(''), searchQuery=ref('');
let controller, version=0, timer;
async function load() {
  const requestVersion=++version; controller?.abort(); controller=new AbortController(); loading.value=true; error.value='';
  const params=new URLSearchParams(props.query);
  params.set('itemType',props.itemType);
  params.set('detailMode',props.selection.mode); params.set('detailScope',props.selection.scope || 'specific');
  if(props.selection.status) params.set('detailStatus',props.selection.status);
  params.set('pageNumber',page.value); params.set('pageSize',String(pageSize)); if(searchQuery.value)params.set('search',searchQuery.value);
  let failureMessage='Không tải được danh sách chi tiết. Vui lòng thử lại.';
  try {
    const res=await fetchWithAuth(getApiUrl('/api/dashboard/metrics?'+params),{signal:controller.signal});
    if(!res.ok) {
      failureMessage=res.status===401?'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.':res.status===403?'Tài khoản không có quyền xem danh sách thống kê chi tiết.':`Không tải được danh sách chi tiết (HTTP ${res.status}). Vui lòng thử lại.`;
      throw new Error('Request failed');
    }
    const data=await res.json(); if(requestVersion!==version)return;
    if(!Array.isArray(data.items) || typeof data.totalCount!=='number') {
      failureMessage=data.createdMetrics || data.statusSummary || data.goalStatusSummary ? 'BE đang chạy chưa hỗ trợ danh sách thống kê chi tiết. Cần triển khai BE mới để đồng bộ với giao diện.' : 'Dữ liệu danh sách chi tiết không đúng định dạng. Vui lòng kiểm tra phiên bản BE.';
      throw new Error('Invalid detail response');
    }
    rows.value=data.items || []; total.value=data.totalCount || 0; totalPages.value=data.totalPages || 0;
  } catch(e) { if(requestVersion===version && e.name!=='AbortError')error.value=failureMessage; }
  finally { if(requestVersion===version)loading.value=false; }
}
watch(search,value=>{clearTimeout(timer);timer=setTimeout(()=>{page.value=1;searchQuery.value=value;},250);});
watch(()=>[props.query,props.selection],()=>{page.value=1;});
watch([()=>props.query,()=>props.selection,page,searchQuery],load,{immediate:true});
onUnmounted(()=>{version++;controller?.abort();clearTimeout(timer);});
</script>
