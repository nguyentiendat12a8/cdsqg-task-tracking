<template>
 <section class="min-w-0 rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
  <header>
   <div class="flex flex-wrap items-center justify-between gap-3 p-4 sm:p-5 bg-[#f1f5fc] border-b border-slate-200">
    <h2 class="ui-page-title">Thông tin chuyển đổi số</h2>
    <div class="flex flex-wrap gap-2"><button type="button" class="ui-btn ui-btn-secondary" :disabled="loading||busy" @click="refresh">Tải lại</button><template v-if="isAdmin"><button type="button" class="ui-btn ui-btn-secondary" :disabled="busy||settings?.busy" @click="openSettings">Cấu hình đồng bộ</button><button type="button" class="ui-btn ui-btn-primary" :disabled="busy||settings?.busy" @click="syncNow">{{ busy||settings?.busy?'Đang đồng bộ…':'Đồng bộ ngay' }}</button></template></div>
   </div>

  </header>
  <p v-if="error" role="alert" class="m-4 rounded-xl bg-rose-50 border border-rose-200 p-3 text-rose-800">{{ error }}</p>
  <p v-if="message" role="status" class="m-4 rounded-xl bg-blue-50 border border-blue-200 p-3 text-blue-900">{{ message }}</p>
  <div>
   <form class="flex flex-wrap gap-3 items-end p-4 bg-slate-50 border-b border-slate-200" @submit.prevent="applyFilters">
    <div class="flex-1 min-w-0 basis-64"><label for="news-search" class="block font-semibold mb-2">Tìm bài viết</label><input id="news-search" v-model="searchDraft" type="search" maxlength="250" class="news-field" placeholder="Tìm theo tên bài viết, bộ hoặc tỉnh…" /></div>
    <div class="w-full sm:w-64"><SearchableSelect v-model="agencyDraft" :options="[{value:'',label:'Tất cả bộ/tỉnh'},...agencies.map(a=>({value:a,label:a}))]" label="Bộ / tỉnh" :is-multi="false" :clearable="false" /></div>
    <button class="ui-btn ui-btn-primary" type="submit" :disabled="loading">Tìm kiếm</button><button class="ui-btn ui-btn-secondary" type="button" :disabled="loading" @click="resetFilters">Đặt lại</button>
    <div class="w-full md:hidden"><SearchableSelect :model-value="sortBy+':'+sortOrder" :options="sortOptions" label="Sắp xếp" :is-multi="false" :clearable="false" :disabled="loading" @update:model-value="setSort($event)"/></div>
   </form>
   <div class="p-3 sm:p-4"><div class="overflow-hidden rounded-xl border border-slate-200">
   <LoadingSpinner v-if="loading" text="Đang tải thông tin chuyển đổi số…" />
   <template v-else>
    <div v-if="!items.length" class="p-8 text-center text-slate-600"><p class="font-semibold">{{ search||agency?'Không có bài viết phù hợp.':'Chưa có thông tin chuyển đổi số.' }}</p><p v-if="!search&&!agency" class="mt-2">Dữ liệu sẽ hiển thị sau lần đồng bộ Google Sheets đầu tiên.</p></div>
    <template v-else>
     <div class="hidden md:block overflow-x-auto"><table class="w-full text-left border-collapse"><thead class="bg-[#f1f5fc]"><tr><th class="news-cell w-16 text-center" scope="col">STT</th><th class="news-cell w-56" scope="col" :aria-sort="sortBy==='agencyName'?(sortOrder==='asc'?'ascending':'descending'):'none'"><button type="button" class="flex items-center justify-between gap-2 w-full text-left hover:text-blue-700" :disabled="loading" @click="toggleSort('agencyName')" title="Bấm để sắp xếp theo Bộ / tỉnh"><span>Bộ / tỉnh</span><span aria-hidden="true" :class="sortBy==='agencyName'?'text-blue-600':'text-slate-300'">{{ sortBy==='agencyName'?(sortOrder==='asc'?'▲':'▼'):'↕' }}</span></button></th><th class="news-cell " scope="col" :aria-sort="sortBy==='title'?(sortOrder==='asc'?'ascending':'descending'):'none'"><button type="button" class="flex items-center justify-between gap-2 w-full text-left hover:text-blue-700" :disabled="loading" @click="toggleSort('title')" title="Bấm để sắp xếp theo Tên bài viết"><span>Tên bài viết</span><span aria-hidden="true" :class="sortBy==='title'?'text-blue-600':'text-slate-300'">{{ sortBy==='title'?(sortOrder==='asc'?'▲':'▼'):'↕' }}</span></button></th><th class="news-cell w-40" scope="col" :aria-sort="sortBy==='importedAt'?(sortOrder==='asc'?'ascending':'descending'):'none'"><button type="button" class="flex items-center justify-between gap-2 w-full text-left hover:text-blue-700" :disabled="loading" @click="toggleSort('importedAt')" title="Bấm để sắp xếp theo Thời gian nạp"><span>Thời gian nạp</span><span aria-hidden="true" :class="sortBy==='importedAt'?'text-blue-600':'text-slate-300'">{{ sortBy==='importedAt'?(sortOrder==='asc'?'▲':'▼'):'↕' }}</span></button></th><th class="news-cell w-32 text-center" scope="col">Thao tác</th></tr></thead><tbody><tr v-for="(item,index) in items" :key="item.id" class="hover:bg-blue-50/40"><td class="news-cell text-center">{{ (page-1)*pageSize+index+1 }}</td><td class="news-cell">{{ item.agencyName }}</td><td class="news-cell"><a :href="item.url" target="_blank" rel="noopener noreferrer" class="font-semibold text-blue-800 hover:underline break-words">{{ item.title }}</a><span class="block mt-1 text-slate-600">{{ sourceHost(item.url) }}</span></td><td class="news-cell">{{ formatDateTime(item.importedAt) }}</td><td class="news-cell text-center"><a :href="item.url" target="_blank" rel="noopener noreferrer" class="ui-btn ui-btn-secondary whitespace-nowrap">Xem nguồn ↗</a></td></tr></tbody></table></div>
     <div class="md:hidden divide-y divide-slate-200"><article v-for="(item,index) in items" :key="item.id" class="p-4 space-y-3"><p class="text-slate-600">{{ (page-1)*pageSize+index+1 }} · {{ item.agencyName }}</p><h3 class="ui-section-title"><a :href="item.url" target="_blank" rel="noopener noreferrer" class="text-blue-800 hover:underline break-words">{{ item.title }}</a></h3><p class="text-slate-600 break-words">{{ sourceHost(item.url) }} · {{ formatDateTime(item.importedAt) }}</p><a :href="item.url" target="_blank" rel="noopener noreferrer" class="ui-btn ui-btn-secondary">Xem nguồn ↗</a></article></div>
    </template>
   </template>
   <DocumentPagination v-model:current-page="page" v-model:page-size="pageSize" :total-count="total" :total-pages="Math.max(1,Math.ceil(total/pageSize))" :page-size-options="pageSizes" :change-page="value=>page=value" item-label="bài viết" :disabled="loading" />
   </div></div>
  </div>
  <div v-if="configOpen" v-accessible-dialog="closeSettings" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3" @click.self="closeSettings">
   <form class="bg-white rounded-2xl w-full max-w-2xl max-h-[90dvh] flex flex-col overflow-hidden shadow-xl" @submit.prevent="saveSettings">
    <header class="p-4 sm:p-6 border-b border-slate-200 bg-[#f1f5fc]"><h3 class="ui-title">Đồng bộ thông tin chuyển đổi số</h3></header>
    <div class="p-4 sm:p-6 overflow-y-auto space-y-4"><p v-if="configError" role="alert" class="p-3 rounded-xl bg-rose-50 text-rose-800">{{ configError }}</p>
     <div><label for="news-sheet" class="block font-semibold mb-2">Link Google Sheets</label><input id="news-sheet" v-model.trim="form.sheetUrl" type="url" required maxlength="1000" class="news-field" aria-describedby="news-sheet-help"/><p id="news-sheet-help" class="text-slate-600 mt-2">Giữ gid trong link để chọn đúng trang tính. Link không có gid sẽ đọc trang tính đầu tiên. Sheet cần cho phép bất kỳ ai có link được xem.</p></div>
     <div class="grid sm:grid-cols-2 gap-4 items-end"><div><label for="news-interval" class="block font-semibold mb-2">Chu kỳ đồng bộ (giờ)</label><input id="news-interval" v-model.number="form.intervalHours" type="number" min="1" max="168" step="1" required class="news-field" /></div><label class="flex items-center gap-2 min-h-9"><input v-model="form.enabled" type="checkbox"/>Bật đồng bộ tự động</label></div>
     <p class="text-slate-600">Đọc các cột Tên bộ/tỉnh, Tên bài viết và Link nguồn bài viết. Link đã có sẽ không được thêm trùng; bài đã nhập vẫn được giữ nếu xóa dòng trên sheet.</p>
     <p class="text-slate-600">Lịch chạy khi backend hoạt động. Khi khởi động lại, hệ thống đồng bộ bù nếu đã đến hạn.</p>
    </div>
    <footer class="p-4 border-t border-slate-200 bg-slate-50 flex justify-end gap-2"><button type="button" class="ui-btn ui-btn-secondary" :disabled="saving" @click="closeSettings">Đóng</button><button type="submit" class="ui-btn ui-btn-primary" :disabled="saving">{{ saving?'Đang lưu…':'Lưu lịch đồng bộ' }}</button></footer>
   </form>
  </div>
 </section>
</template>
<script setup>
import {ref,computed,onMounted,onUnmounted,watch} from 'vue';
import {authState,fetchWithAuth} from '../services/auth';
import {getApiUrl} from '../config/api';
import {formatDateTime} from '../shared/formatters';
import SearchableSelect from '../components/SearchableSelect.vue';
import DocumentPagination from '../features/document/DocumentPagination.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
const isAdmin=computed(()=>authState.isAdmin.value);
const items=ref([]),agencies=ref([]),total=ref(0),page=ref(1),pageSize=ref(10),searchDraft=ref(''),agencyDraft=ref(''),search=ref(''),agency=ref('');
const settings=ref(null),loading=ref(true),busy=ref(false),saving=ref(false),error=ref(''),message=ref(''),configOpen=ref(false),configError=ref(''),form=ref({});
const sortBy=ref('importedAt'),sortOrder=ref('desc');
const sortOptions=[{value:'importedAt:desc',label:'Bài mới nhất'},{value:'importedAt:asc',label:'Bài cũ nhất'},{value:'agencyName:asc',label:'Bộ / tỉnh: A → Z'},{value:'agencyName:desc',label:'Bộ / tỉnh: Z → A'},{value:'title:asc',label:'Tên bài viết: A → Z'},{value:'title:desc',label:'Tên bài viết: Z → A'}];
function setSort(value){[sortBy.value,sortOrder.value]=value.split(':');if(page.value!==1)page.value=1;else load();}
function toggleSort(key){setSort(key+':'+(sortBy.value===key&&sortOrder.value==='asc'?'desc':'asc'));}
const pageSizes=[10,20,50].map(value=>({value,label:String(value)}));let version=0,settingsVersion=0,timer;
async function api(path='',options){const res=await fetchWithAuth(getApiUrl('/api/digital-news'+path),options);const body=await res.json().catch(()=>({}));if(!res.ok)throw new Error(body.message||'Không tải được thông tin chuyển đổi số.');return body;}
async function load(silent=false){const request=++version;if(!silent)loading.value=true;try{const q=new URLSearchParams({page:String(page.value),pageSize:String(pageSize.value),search:search.value,agency:agency.value||'',sortBy:sortBy.value,sortOrder:sortOrder.value});const data=await api('?'+q);if(request!==version)return;items.value=data.items;total.value=data.total;if(data.page!==page.value)page.value=data.page;}catch(e){if(request===version)error.value=e.message;}finally{if(request===version)loading.value=false;}}
async function loadSettings(){if(!isAdmin.value)return;const request=++settingsVersion;try{const data=await api('/settings');if(request===settingsVersion)settings.value=data;}catch(e){if(request===settingsVersion)error.value=e.message;}}
async function loadAgencies(){try{agencies.value=await api('/agencies');}catch(e){error.value=e.message;}}
async function refresh(){error.value='';await Promise.all([load(),loadSettings(),loadAgencies()]);}
function applyFilters(){search.value=searchDraft.value.trim();agency.value=agencyDraft.value||'';if(page.value!==1)page.value=1;else load();}
function resetFilters(){searchDraft.value='';agencyDraft.value='';applyFilters();}
function sourceHost(url){try{return new URL(url).hostname;}catch{return '';}}
function openSettings(){if(!settings.value)return;form.value={sheetUrl:settings.value.sheetUrl,enabled:settings.value.enabled,intervalHours:settings.value.intervalHours};configError.value='';configOpen.value=true;}
function closeSettings(){if(!saving.value)configOpen.value=false;}
async function saveSettings(){saving.value=true;configError.value='';try{const data=await api('/settings',{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify(form.value)});message.value=data.message;configOpen.value=false;await loadSettings();}catch(e){configError.value=e.message;}finally{saving.value=false;}}
async function syncNow(){if(busy.value)return;busy.value=true;message.value='';error.value='';try{const data=await api('/sync',{method:'POST'});message.value=data.message;await Promise.all([load(),loadAgencies()]);}catch(e){error.value=e.message;}finally{busy.value=false;await loadSettings();}}
watch(page,()=>load());watch(pageSize,()=>{if(page.value!==1)page.value=1;else load();});
onMounted(()=>{refresh();timer=setInterval(()=>{if(document.visibilityState==='visible'&&!busy.value&&!configOpen.value){load(true);loadSettings();}},30000);});onUnmounted(()=>{version++;settingsVersion++;clearInterval(timer);});
</script>
<style scoped>
.news-field{display:block;width:100%;height:var(--ui-control-height);border:1px solid var(--color-slate-300);border-radius:8px;padding:8px 12px;background:white;min-width:0}
.news-cell{padding:12px;border-bottom:1px solid var(--color-slate-200);vertical-align:middle}
</style>
