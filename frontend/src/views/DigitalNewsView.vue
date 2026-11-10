<template>
 <section class="min-w-0 w-full rounded-2xl border border-slate-200/80 bg-white shadow-sm overflow-hidden">
  <header>
   <div class="ui-screen-heading flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 p-4 sm:p-5 border-b border-slate-200/80">
    <div class="flex items-center gap-3 min-w-0"><div class="ui-screen-icon w-10 h-10 rounded-2xl flex items-center justify-center shrink-0" aria-hidden="true"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-width="1.8" stroke-linejoin="round" stroke-linecap="round" d="M5 4h14v16H5zM8 8h8M8 12h3M8 16h8M14 11h2v3h-2z"/></svg></div><h2 class="ui-page-title tracking-tight">Thông tin liên quan</h2></div>
    <div class="flex flex-wrap gap-2"><button type="button" class="ui-btn ui-btn-secondary" :disabled="loading||busy" @click="refresh"><svg aria-hidden="true" class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M20 7v5h-5M4 17v-5h5M6 7a7 7 0 0112-1l2 3M4 15l2 3a7 7 0 0012-1"/></svg>Tải lại</button><template v-if="isAdmin"><button type="button" class="ui-btn ui-btn-secondary" :disabled="busy||settings?.busy" @click="openSettings"><svg aria-hidden="true" class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" d="M4 7h16M4 17h16M9 4v6M15 14v6"/></svg>Cấu hình theo dõi</button><button type="button" class="ui-btn ui-btn-primary" :disabled="busy||settings?.busy" @click="syncNow"><svg aria-hidden="true" class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M8 3H4v4M16 3h4v4M4 17v4h4M20 17v4h-4M7 12h10"/></svg>{{ busy||settings?.busy?'Đang quét…':'Quét tin ngay' }}</button></template></div>
   </div>

  </header>
  <p v-if="error" role="alert" class="m-4 rounded-xl bg-rose-50 border border-rose-200 p-3 text-rose-800">{{ error }}</p>
  <p v-if="message" role="status" class="m-4 rounded-xl bg-blue-50 border border-blue-200 p-3 text-blue-900">{{ message }}</p>
  <div class="p-3.5 sm:p-4 w-full">
   <div class="w-full rounded-2xl border border-slate-200/80 bg-white overflow-hidden shadow-sm flex flex-col">
   <form class="flex flex-wrap gap-2 items-center p-3 bg-slate-50/60" @submit.prevent="applyFilters">
    <div class="relative flex-1 min-w-0 basis-64 max-w-xl"><label for="news-search" class="sr-only">Tìm bài viết</label><svg aria-hidden="true" class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" d="m21 21-6-6"/><circle cx="10" cy="10" r="7"/></svg><input id="news-search" v-model="searchDraft" type="search" maxlength="250" class="news-field news-search font-semibold" placeholder="Tìm theo tên bài viết, bộ hoặc tỉnh…" /></div>
    <div class="w-full sm:w-56"><SearchableSelect v-model="agencyDraft" :options="[{value:'',label:'Tất cả bộ/tỉnh'},...agencies.map(a=>({value:a,label:a}))]" label="Bộ / tỉnh" label-class="sr-only" :is-multi="false" :clearable="false" /></div>
    <div class="w-full sm:w-48"><SearchableSelect v-model="statusDraft" :options="statusOptions" label="Trạng thái" label-class="sr-only" :is-multi="false" :clearable="false" /></div>
    <button class="ui-btn ui-btn-primary" type="submit" :disabled="loading">Tìm kiếm</button><button class="ui-btn ui-btn-secondary" type="button" :disabled="loading" @click="resetFilters">Đặt lại</button>
    <div class="w-full md:hidden"><SearchableSelect :model-value="sortBy+':'+sortOrder" :options="sortOptions" label="Sắp xếp" :is-multi="false" :clearable="false" :disabled="loading" @update:model-value="setSort($event)"/></div>
   </form>

   <LoadingSpinner v-if="loading" text="Đang tải thông tin liên quan…" />
   <template v-else>
    <div v-if="!items.length" class="p-8 text-center text-slate-600"><p class="font-semibold">{{ search||agency?'Không có bài viết phù hợp.':'Chưa có thông tin liên quan.' }}</p><p v-if="!search&&!agency" class="mt-2">Bật nguồn theo dõi và quét tin để thu thập bài viết. Chỉ đánh giá tiêu đề khi bạn bấm Đọc.</p></div>
    <template v-else>
     <div class="hidden md:block overflow-auto max-h-[calc(100dvh-320px)] custom-scrollbar w-full"><table class="w-full min-w-[1100px] text-left text-sm text-slate-700 border-collapse"><thead class="bg-slate-100 text-xs text-slate-700 font-semibold border-b border-slate-200 sticky top-0 z-10 shadow-xs select-none"><tr><th class="news-cell w-14 text-center" scope="col">STT</th><th class="news-cell w-36" scope="col" :aria-sort="sortBy==='agencyName'?(sortOrder==='asc'?'ascending':'descending'):'none'"><button type="button" class="flex items-center justify-between gap-2 w-full text-left hover:text-blue-700" :disabled="loading" @click="toggleSort('agencyName')" v-tip="'Bấm để sắp xếp theo Bộ / tỉnh'"><span>Bộ / tỉnh</span><span aria-hidden="true" :class="sortBy==='agencyName'?'text-blue-600':'text-slate-300'">{{ sortBy==='agencyName'?(sortOrder==='asc'?'▲':'▼'):'↕' }}</span></button></th><th class="news-cell min-w-[200px]" scope="col" :aria-sort="sortBy==='title'?(sortOrder==='asc'?'ascending':'descending'):'none'"><button type="button" class="flex items-center justify-between gap-2 w-full text-left hover:text-blue-700" :disabled="loading" @click="toggleSort('title')" v-tip="'Bấm để sắp xếp theo Tên bài viết'"><span>Tên bài viết</span><span aria-hidden="true" :class="sortBy==='title'?'text-blue-600':'text-slate-300'">{{ sortBy==='title'?(sortOrder==='asc'?'▲':'▼'):'↕' }}</span></button></th><th class="news-cell w-36 text-center" scope="col" :aria-sort="sortBy==='importedAt'?(sortOrder==='asc'?'ascending':'descending'):'none'"><button type="button" class="flex items-center justify-between gap-2 w-full text-left hover:text-blue-700" :disabled="loading" @click="toggleSort('importedAt')" v-tip="'Bấm để sắp xếp theo Thời gian nạp'"><span>Thời gian nạp</span><span aria-hidden="true" :class="sortBy==='importedAt'?'text-blue-600':'text-slate-300'">{{ sortBy==='importedAt'?(sortOrder==='asc'?'▲':'▼'):'↕' }}</span></button></th><th class="news-cell w-44 text-center whitespace-nowrap" scope="col">Trạng thái</th><th class="news-cell w-40 text-center whitespace-nowrap" scope="col">Thao tác</th></tr></thead><tbody><tr v-for="(item,index) in items" :key="item.id" class="hover:bg-blue-50/40"><td class="news-cell text-center">{{ (page-1)*pageSize+index+1 }}</td><td class="news-cell">{{ item.agencyName }}</td><td class="news-cell"><a :href="item.url" target="_blank" rel="noopener noreferrer" class="font-medium text-blue-800 hover:underline break-words line-clamp-2" v-tip="item.title">{{ item.title }}</a><span class="block mt-1 text-slate-600">{{ sourceHost(item.url) }}</span></td><td class="news-cell text-center whitespace-nowrap">{{ formatDateTime(item.importedAt) }}</td><td class="news-cell text-center whitespace-nowrap"><span class="ai-badge whitespace-nowrap text-xs" :class="badge(item)" v-tip="item.aiError">{{ item.processing ? 'Đang xử lý' : statusLabel(item.aiStatus) }}</span></td><td class="news-cell text-center whitespace-nowrap"><div class="flex items-center justify-center gap-2"><button v-if="isAdmin" type="button" class="ui-btn whitespace-nowrap" :class="item.assessment?'ui-btn-secondary':'ui-btn-primary'" :disabled="readingId===item.id||isQueued(item)" @click="readWithAi(item)">{{ readingId===item.id?'Đang gửi…':isQueued(item)?statusLabel(item.aiStatus,item.assessment?.planKind):item.aiStatus==='Error'?'Thử lại':item.assessment?'Xem kết quả':'Đọc' }}</button><a :href="item.url" target="_blank" rel="noopener noreferrer" class="ui-btn ui-btn-secondary whitespace-nowrap">Xem nguồn ↗</a></div></td></tr></tbody></table></div>
     <div class="md:hidden divide-y divide-slate-200"><article v-for="(item,index) in items" :key="item.id" class="p-4 space-y-3"><p class="text-slate-600">{{ (page-1)*pageSize+index+1 }} · {{ item.agencyName }}</p><h3 class="ui-section-title"><a :href="item.url" target="_blank" rel="noopener noreferrer" class="text-blue-800 hover:underline break-words">{{ item.title }}</a></h3><p class="text-slate-600 break-words">{{ sourceHost(item.url) }} · {{ formatDateTime(item.importedAt) }}</p><div class="flex flex-wrap items-center justify-between gap-2 mt-2"><span class="ai-badge whitespace-nowrap text-xs" :class="badge(item)" v-tip="item.aiError">{{ item.processing ? 'Đang xử lý' : statusLabel(item.aiStatus) }}</span><div class="flex items-center gap-2"><button v-if="isAdmin" type="button" class="ui-btn" :class="item.assessment?'ui-btn-secondary':'ui-btn-primary'" :disabled="readingId===item.id||isQueued(item)" @click="readWithAi(item)">{{ readingId===item.id?'Đang gửi…':isQueued(item)?statusLabel(item.aiStatus,item.assessment?.planKind):item.aiStatus==='Error'?'Thử lại':item.assessment?'Xem kết quả':'Đọc' }}</button><a :href="item.url" target="_blank" rel="noopener noreferrer" class="ui-btn ui-btn-secondary">Xem nguồn ↗</a></div></div></article></div>
    </template>
   </template>
   <DocumentPagination v-model:current-page="page" v-model:page-size="pageSize" :total-count="total" :total-pages="Math.max(1,Math.ceil(total/pageSize))" :page-size-options="pageSizes" :change-page="value=>page=value" item-label="bài viết" :disabled="loading" />
   </div>
  </div>
  <DigitalPlanSettings v-if="configOpen" :settings="settings" @close="configOpen=false" @saved="message=$event;configOpen=false;refresh()" />
  <div v-if="selected" v-accessible-dialog="closeSelected" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3">
   <section class="bg-white rounded-2xl w-full max-w-2xl max-h-[90dvh] flex flex-col overflow-hidden shadow-xl">
    <header class="p-4 sm:p-6 border-b border-slate-200 bg-[#f1f5fc]"><h3 class="ui-title">Kết quả đánh giá tiêu đề</h3></header>
    <div class="p-4 sm:p-6 overflow-y-auto space-y-4">
     <h4 class="font-semibold">{{ selected.title }}</h4><p class="text-slate-600">{{ selected.agencyName }}</p>
     <p role="status" class="rounded-xl border p-4" :class="selected.assessment?.status==='Related'?'bg-emerald-50 border-emerald-200 text-emerald-900':selected.assessment?.status==='NeedsReview'?'bg-amber-50 border-amber-200 text-amber-900':selected.assessment?.status==='NotRelevant'?'bg-rose-50 border-rose-200 text-rose-900':'bg-slate-50 border-slate-200'"><span class="font-bold text-sm">{{ statusLabel(selected.assessment?.status) }}</span><span v-if="selected.assessment?.planKind" class="block mt-2 font-semibold">Phân loại: {{ kindLabel(selected.assessment.planKind) }}<template v-if="selected.assessment.startYear!=null"> · Giai đoạn/Năm: {{ selected.assessment.startYear }}{{ selected.assessment.endYear!==selected.assessment.startYear?' – '+selected.assessment.endYear:'' }}</template></span></p>
     <p v-if="isQueued({aiStatus:selected.assessment?.status})">Đang chờ AI đánh giá tiêu đề. Kết quả sẽ tự cập nhật tại đây.</p>
     <p v-if="selected.assessment?.reason" class="whitespace-pre-wrap">{{ selected.assessment.reason }}</p>
     <blockquote v-if="selected.assessment?.evidence" class="border-l-4 border-blue-300 pl-3 text-slate-700">{{ selected.assessment.evidence }}</blockquote>
     <p class="text-slate-500">Kết quả dựa trên tiêu đề. Hãy xem nguồn để xác minh nội dung văn bản.</p>
    </div>
    <footer class="p-4 border-t border-slate-200 bg-slate-50 flex justify-end gap-2"><a :href="selected.url" target="_blank" rel="noopener noreferrer" class="ui-btn ui-btn-secondary">Xem nguồn ↗</a><button type="button" class="ui-btn ui-btn-primary" @click="closeSelected">Đóng</button></footer>
   </section>
  </div>
 </section>
</template>
<script setup>
import {ref,computed,onMounted,onUnmounted,watch} from 'vue';
import {authState,fetchWithAuth} from '../services/auth';
import { MODULE, ACTION } from '../shared/permissions';
import {getApiUrl} from '../config/api';
import {formatDateTime} from '../shared/formatters';
import SearchableSelect from '../components/SearchableSelect.vue';
import DocumentPagination from '../features/document/DocumentPagination.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import {getStatusBadgeClass} from '../shared/statusPresentation';
import '../features/ai/ai.css';
import DigitalPlanSettings from '../features/digital-news/DigitalPlanSettings.vue';
// Đồng bộ, đánh dấu đã đọc, thiết lập tiêu chí: Admin và Văn thư (bảng quyền phần Tiện ích).
const isAdmin=computed(()=>authState.can(MODULE.utilities,ACTION.edit));
const items=ref([]),agencies=ref([]),total=ref(0),page=ref(1),pageSize=ref(10),searchDraft=ref(''),agencyDraft=ref(''),statusDraft=ref(''),search=ref(''),agency=ref(''),status=ref('');
const settings=ref(null),loading=ref(true),busy=ref(false),saving=ref(false),error=ref(''),message=ref(''),configOpen=ref(false),configError=ref(''),form=ref({});
const readingId=ref(null),selected=ref(null);
const kindLabel=kind=>({Annual:'Kế hoạch hằng năm',FiveYear:'Kế hoạch 5 năm',Architecture:'Khung kiến trúc số / điện tử',DigitalDocument:'Văn bản chuyển đổi số'}[kind]||kind);
const statusOptions=[{value:'',label:'Tất cả trạng thái'},{value:'Unread',label:'Chưa đọc'},{value:'Related',label:'Có liên quan'},{value:'NeedsReview',label:'Cần xem lại'},{value:'NotRelevant',label:'Không liên quan'}];
const statusLabel=(s)=>({Queued:'Chờ đánh giá',Reading:'Đang đánh giá',Related:'Có liên quan',NeedsReview:'Cần xem lại',NotRelevant:'Không liên quan',Error:'Lỗi xử lý',Unread:'Chưa đọc'}[s]||'Chưa đọc');
const isQueued=item=>['Queued','Reading'].includes(item.aiStatus);
const badge=item=>{if(item.processing||isQueued(item))return getStatusBadgeClass('InProgress');if(item.aiStatus==='Related')return getStatusBadgeClass('Approved');if(item.aiStatus==='NeedsReview')return getStatusBadgeClass('Pending');if(item.aiStatus==='NotRelevant'||item.aiStatus==='Error')return getStatusBadgeClass('Rejected');return getStatusBadgeClass('NotStarted');};
async function readWithAi(item){
 if(readingId.value||isQueued(item))return;
 if(item.assessment&&item.aiStatus!=='Error'){selectedVersion++;selected.value=item;return;}
 readingId.value=item.id;error.value='';
 try{const data=await api('/'+item.id+'/read',{method:'POST'});message.value=data.message||'Đã gửi yêu cầu đánh giá tiêu đề.';await load(true);}
 catch(e){error.value=e.message;}finally{readingId.value=null;}
}
let selectedVersion=0;
function closeSelected(){selectedVersion++;selected.value=null;}
async function loadSelected(id,query=''){const request=++selectedVersion;try{const data=await api('/'+id+'/assessment'+query);if(request===selectedVersion)selected.value={...data.item,assessment:data.assessment};}catch(e){if(request===selectedVersion)error.value=e.message;}}
const sortBy=ref('importedAt'),sortOrder=ref('desc');
const sortOptions=[{value:'importedAt:desc',label:'Bài mới nhất'},{value:'importedAt:asc',label:'Bài cũ nhất'},{value:'agencyName:asc',label:'Bộ / tỉnh: A → Z'},{value:'agencyName:desc',label:'Bộ / tỉnh: Z → A'},{value:'title:asc',label:'Tên bài viết: A → Z'},{value:'title:desc',label:'Tên bài viết: Z → A'}];
function setSort(value){[sortBy.value,sortOrder.value]=value.split(':');if(page.value!==1)page.value=1;else load();}
function toggleSort(key){setSort(key+':'+(sortBy.value===key&&sortOrder.value==='asc'?'desc':'asc'));}
const pageSizes=[10,20,50].map(value=>({value,label:String(value)}));let version=0,settingsVersion=0,timer;
async function api(path='',options){const res=await fetchWithAuth(getApiUrl('/api/digital-news'+path),options);const body=await res.json().catch(()=>({}));if(!res.ok)throw new Error(body.message||'Không tải được thông tin liên quan.');return body;}
async function load(silent=false){const request=++version;if(!silent)loading.value=true;try{const q=new URLSearchParams({page:String(page.value),pageSize:String(pageSize.value),search:search.value,agency:agency.value||'',status:status.value||'',sortBy:sortBy.value,sortOrder:sortOrder.value});const data=await api('?'+q);if(request!==version)return;items.value=data.items;total.value=data.total;if(data.page!==page.value)page.value=data.page;}catch(e){if(request===version)error.value=e.message;}finally{if(request===version)loading.value=false;}}
async function loadSettings(){if(!isAdmin.value)return;const request=++settingsVersion;try{const data=await api('/settings');if(request===settingsVersion)settings.value=data;}catch(e){if(request===settingsVersion)error.value=e.message;}}
async function loadAgencies(){try{agencies.value=await api('/agencies');}catch(e){error.value=e.message;}}
async function refresh(){error.value='';await Promise.all([load(),loadSettings(),loadAgencies()]);}
function applyFilters(){search.value=searchDraft.value.trim();agency.value=agencyDraft.value||'';status.value=statusDraft.value||'';if(page.value!==1)page.value=1;else load();}
function resetFilters(){searchDraft.value='';agencyDraft.value='';statusDraft.value='';applyFilters();}
function sourceHost(url){try{return new URL(url).hostname;}catch{return '';}}
function openSettings(){if(settings.value)configOpen.value=true;}
async function syncNow(){if(busy.value)return;busy.value=true;message.value='';error.value='';try{const data=await api('/sync',{method:'POST'});message.value=data.message;await Promise.all([load(),loadAgencies()]);}catch(e){error.value=e.message;}finally{busy.value=false;await loadSettings();}}
watch(page,()=>load());watch(pageSize,()=>{if(page.value!==1)page.value=1;else load();});
function openHashArticle(){const params=new URLSearchParams(location.hash.split('?')[1]||'');const article=params.get('article');if(article){const query=new URLSearchParams();for(const key of ['result','notification'])if(params.get(key))query.set(key,params.get(key));loadSelected(article,query.size?'?'+query:'');}}
onMounted(()=>{refresh();openHashArticle();window.addEventListener('hashchange',openHashArticle);window.addEventListener('open-digital-news-result',openHashArticle);timer=setInterval(()=>{if(document.visibilityState==='visible'&&!busy.value&&!configOpen.value){load(true);if(selected.value&&['Queued','Reading'].includes(selected.value.assessment?.status))loadSelected(selected.value.id);if(!items.value.some(i=>isQueued(i)))loadSettings();}},4000);});onUnmounted(()=>{window.removeEventListener('hashchange',openHashArticle);window.removeEventListener('open-digital-news-result',openHashArticle);selectedVersion++;version++;settingsVersion++;clearInterval(timer);});
</script>
<style scoped>
.news-field{display:block;width:100%;height:var(--ui-control-height);border:1px solid var(--color-slate-300);border-radius:8px;padding:8px 12px;background:white;min-width:0}
.news-search{padding-left:36px;border-color:var(--color-slate-200);border-radius:12px}
.news-cell{padding:10px 12px;border-bottom:1px solid var(--color-slate-200);border-right:1px solid var(--color-slate-200);vertical-align:middle}
.news-cell:last-child{border-right:0}
thead .news-cell{background:var(--ui-heading-bg,#f1f5fc)}
tbody tr:last-child .news-cell{border-bottom:0}
</style>
