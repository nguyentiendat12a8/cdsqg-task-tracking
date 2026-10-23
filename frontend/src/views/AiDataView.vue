<template>
 <div class="ai-workspace ai-admin-shell">
  <AiPageHeader :title="sectionTitle"><template #tabs v-if="fileTabs.length>0"><nav role="tablist" aria-label="Nguồn dữ liệu AI" class="ai-tabs"><button v-for="item in fileTabs" :id="'ai-file-tab-'+item.key" :key="item.key" role="tab" type="button" :aria-selected="fileTab===item.key" :aria-controls="'ai-file-panel-'+item.key" :tabindex="fileTab===item.key?0:-1" @click="fileTab=item.key" @keydown="handleFileTabKey($event,item.key)">{{ item.label }}</button></nav></template></AiPageHeader>
  <div class="ai-admin-body space-y-4">
    <details v-if="tab==='internet' && catalog" class="ai-panel p-4 sm:p-5"><summary class="cursor-pointer font-semibold text-slate-700">Chỉ số và mục tiêu áp dụng ({{ catalogGoals.length }})</summary><div class="grid xl:grid-cols-3 gap-4 mt-4"><article v-for="metric in activeMetrics" :key="metric.key" class="ai-goal-card"><h3 class="ui-section-title">{{ metric.name }}</h3><p class="ai-muted mb-4">{{ metric.definition }}</p><ul><li v-for="goal in catalogGoals.filter(g=>g.metric.key===metric.key)" :key="goal.id"><span class="ai-goal-code">{{ goal.code }}</span><span>{{ goal.title }}</span></li></ul></article></div></details>
  <p v-if="catalog && !catalog.configured" role="status" class="rounded-xl p-3 border border-amber-200 bg-amber-50 text-amber-900">AI chưa sẵn sàng. Dữ liệu vẫn được tiếp nhận. <a href="#ai-settings" class="font-semibold underline">Cấu hình AI</a></p>
  <p v-if="error" role="alert" class="rounded-xl p-3 bg-rose-50 text-rose-800">{{ error }}</p>
  <LoadingSpinner v-if="loading" text="Đang tải dữ liệu AI..."/>
  <section v-else-if="!showingSuggestions">
   <div v-show="fileTab==='files'||tab==='uploads'" id="ai-file-panel-files" role="tabpanel" aria-labelledby="ai-file-tab-files"><AiInboxPanel ref="inboxPanel" mode="uploads" :metrics="uploadMetrics" :configured="catalog.configured" @view-suggestions="viewInputResults" @view-result="openReview"/></div>
   <div v-if="fileTab==='sources'" id="ai-file-panel-sources" role="tabpanel" aria-labelledby="ai-file-tab-sources" class="space-y-4"><details class="ai-panel p-4"><summary class="cursor-pointer font-semibold">Nguồn RSS và lịch quét</summary><div class="mt-4"><AiAlertsView ref="rssPanel" embedded @navigate="$emit('navigate',$event)"/></div></details><AiInboxPanel ref="alertsInbox" mode="alerts" :metrics="catalog.metrics" :configured="catalog.configured" @view-suggestions="viewInputResults"/></div>
   <div v-if="fileTab==='history'&&tab==='internet'" id="ai-file-panel-history" role="tabpanel" aria-labelledby="ai-file-tab-history" class="space-y-4">
      <AiAlertsView ref="rssHistory" embedded initial-tab="history"/>
    </div>
  
  </section>
    <section v-else id="ai-file-panel-review" role="tabpanel" aria-labelledby="ai-file-tab-review" class="ai-panel">
      <div v-if="resultInput" class="ai-toolbar flex justify-between items-center gap-3"><p>Kết quả của: <strong>{{ resultInput.title }}</strong></p><button type="button" class="ui-btn ui-btn-secondary" @click="resultInput=null;page=1;refresh()">Xem tất cả tài liệu</button></div>
      <div v-if="showingSuggestions" class="ai-toolbar ai-review-filters">
        <div class="min-w-0"><SearchableSelect v-model="status" label="Trạng thái" :is-multi="false" :clearable="false" :options="[{value:'Pending',label:'Chờ kiểm chứng'},{value:'Approved',label:'Đã duyệt'},{value:'Rejected',label:'Đã từ chối'},{value:'',label:'Tất cả'}]" /></div>
        <div class="min-w-0"><SearchableSelect v-model="metricFilter" label="Chỉ số" :is-multi="false" :clearable="false" :options="[{value:'',label:'Tất cả chỉ số'},...catalog.metrics.map(m=>({value:m.key,label:m.name}))]" /></div>
        <label class="font-semibold">Năm số liệu<input v-model="year" type="number" min="2000" :max="currentYear" class="field" placeholder="Tất cả" @change="page=1;refresh()" /></label>
      </div>
      <div class="ai-records-layout"><div class="ai-wide-list ai-table-scroll">
        <table class="ai-table"><colgroup><col class="ai-stt-column"/><col style="width:30%" /><col style="width:7%" /><col style="width:10%" /><col style="width:25%" /><col style="width:16%" /><col style="width:12%" /></colgroup><thead><tr><th scope="col" class="ai-stt">STT</th><th>Chỉ số</th><th>Năm</th><th class="text-right">Giá trị</th><th>Nguồn</th><th class="text-center">Trạng thái</th><th class="text-center">Thao tác</th></tr></thead><tbody>
          <tr v-for="(s,index) in rows" :key="s.id" class="hover:bg-slate-50/60"><td class="ai-stt">{{ (page-1)*pageSize+index+1 }}</td>
            <td>{{ metricName(s.metricKey) }}</td><td class="tabular-nums">{{ s.dataYear }}</td>
            <td class="text-right tabular-nums whitespace-nowrap"><strong>{{ formatNumber(s.status==='Approved'?(s.acceptedValue??s.value):s.value) }}</strong> {{ unitLabel(s.unit) }}</td>
            <td><span class="break-words">{{ shortSource(s) }}</span><p v-if="sourceCode(s)" class="ai-input-code mt-1">{{ sourceCode(s) }}</p></td>
            <td class="text-center"><span class="ai-badge" :class="getStatusBadgeClass(s.status)">{{ reviewStatus(s.status) }}</span></td>
            <td class="text-center"><button class="ui-btn ui-btn-secondary whitespace-nowrap" @click="openReview(s)">{{ s.status==='Pending'?'Kiểm chứng':'Xem chi tiết' }}</button></td>
          </tr>
        </tbody></table>
      </div><div class="ai-compact-list"><article v-for="(s,index) in rows" :key="s.id" class="ai-record"><p class="ai-muted mb-2">STT {{ (page-1)*pageSize+index+1 }}</p><p class="ai-record-title">{{ metricName(s.metricKey) }}</p><div class="flex justify-between items-center gap-3 mt-3"><span class="ai-badge" :class="getStatusBadgeClass(s.status)">{{ reviewStatus(s.status) }}</span><strong>{{ formatNumber(s.status==='Approved'?(s.acceptedValue??s.value):s.value) }} {{ unitLabel(s.unit) }}</strong></div><dl class="ai-record-meta"><div><dt>Năm số liệu</dt><dd>{{ s.dataYear }}</dd></div><div><dt>Nguồn</dt><dd><span>{{ shortSource(s) }}</span><p v-if="sourceCode(s)" class="ai-input-code mt-1">{{ sourceCode(s) }}</p></dd></div></dl><div class="ai-record-actions"><button type="button" class="ui-btn ui-btn-secondary" @click="openReview(s)">{{ s.status==='Pending'?'Kiểm chứng':'Xem chi tiết' }}</button></div></article></div><div v-if="!rows.length" class="ai-empty"><strong>Không có số liệu phù hợp</strong><p>Đề xuất từ tài liệu và nguồn RSS đều được kiểm chứng tại đây.</p></div></div>
      <AiListPagination v-model:page="page" v-model:page-size="pageSize" :total="total" :disabled="loading||busy"/>
    </section>
  </div>
    <div v-if="selected" v-accessible-dialog="closeReview" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3" @click.self="closeReview">
      <section class="bg-white rounded-2xl w-full max-w-5xl max-h-[90dvh] flex flex-col overflow-hidden shadow-xl">
        <header class="flex justify-between items-start gap-4 p-4 sm:p-6 border-b border-slate-200 shrink-0">
          <div><h3 class="ui-title">{{ selected.inputType==='upload'?'Kết quả bóc tách':'Kiểm chứng số liệu' }}</h3><p class="text-slate-600 mt-2">{{ metricName(selected.metricKey) }}</p></div>
        </header>
        <div class="p-4 sm:p-6 space-y-4 overflow-y-auto min-h-0">
          <div v-if="reviewError" role="alert" class="text-rose-800 bg-rose-50 p-3 rounded-xl">{{ reviewError }}</div>
          <dl class="grid grid-cols-2 sm:grid-cols-4 gap-4 rounded-xl bg-slate-50 p-4">
            <div><dt class="text-slate-600">Năm số liệu</dt><dd class="mt-1 font-semibold">{{ selected.dataYear }}</dd></div>
            <div><dt class="text-slate-600">Phạm vi</dt><dd class="mt-1 font-semibold">{{ selected.scope }}</dd></div>
            <div><dt class="text-slate-600">Trang / đoạn</dt><dd class="mt-1 font-semibold">{{ selected.page }}</dd></div>
            <div><dt class="text-slate-600">Mục tiêu liên kết</dt><dd class="mt-1 font-semibold">{{ linkedGoals.map(g=>g.code).join(', ') || 'Chưa có' }}</dd></div>
          </dl>
          <section class="space-y-3">
            <div class="bg-slate-50 rounded-xl p-4"><h4 class="ui-section-title">Nguồn đối soát</h4><p class="font-semibold mt-2 break-words">{{ selected.sourceTitle }}</p><AiSourceIdentity :source="selected" detailed/><a v-if="!selected.sourceUrl.startsWith('/api/ai-inbox/')" :href="selected.sourceUrl" target="_blank" rel="noopener noreferrer" class="text-blue-700 underline break-all block mt-2">{{ selected.sourceUrl }}</a><button v-else type="button" class="ui-btn ui-btn-secondary mt-2" :disabled="busy" @click="downloadPrivate(selected.sourceUrl,selected.sourceTitle)">Tải tài liệu gốc</button></div>
            <h4 class="ui-section-title">Dẫn chứng</h4>
            <blockquote class="p-4 rounded-r-xl bg-slate-50 border-l-4 border-blue-600 whitespace-pre-wrap break-words">{{ selected.quote }}</blockquote>
            <p class="text-slate-600">Phương pháp: {{ selected.methodology || 'Chưa xác định' }}</p>
            <details class="text-slate-600"><summary class="cursor-pointer font-semibold">Thông tin đối chiếu</summary><dl class="mt-3 space-y-2 break-all"><div><dt class="font-semibold">Tài liệu</dt><dd>{{ selected.sourceTitle }}</dd></div><div><dt class="font-semibold">Kỳ số liệu</dt><dd>{{ selected.periodLabel }}</dd></div><div><dt class="font-semibold">Phiên bản AI</dt><dd>{{ selected.model }}</dd></div><div><dt class="font-semibold">SHA-256</dt><dd>{{ selected.contentHash }}</dd></div></dl></details>
          </section>
          <section v-if="selected.status==='Pending'" class="border-t border-slate-200 pt-4 space-y-4">
            <label class="block font-semibold">{{ selected.inputType==='upload'?'Giá trị ghi vào mục tiêu':'Giá trị đã kiểm chứng' }}<QuantityInput v-model="acceptedValue" class="field" /></label>
            <div><label for="ai-review-note" class="block font-semibold">{{ selected.inputType==='upload'?'Ghi chú (không bắt buộc)':'Ghi chú kiểm chứng hoặc lý do từ chối (không bắt buộc)' }}</label><textarea id="ai-review-note" v-model="reviewNote" maxlength="2000" class="field font-normal" rows="3"></textarea></div>
            <label class="flex items-start gap-3 rounded-xl border border-slate-200 p-3"><input v-model="checked" type="checkbox" class="mt-1 shrink-0" /><span>Xác nhận số liệu</span></label>
          </section>
          <p v-else>{{ reviewStatus(selected.status) }} · {{ selected.reviewedBy }} · {{ selected.reviewNote }}</p>
        </div>
        <footer class="p-4 sm:px-6 border-t border-slate-200 bg-slate-50 flex justify-end gap-2 flex-wrap shrink-0">
          <button class="ui-btn ui-btn-secondary" :disabled="busy" @click="closeReview">Đóng</button>
          <button v-if="selected.status==='Pending'" class="ui-btn ui-btn-secondary" :disabled="busy" @click="review(false)">{{ selected.inputType==='upload'?'Bỏ qua':'Từ chối' }}</button><button v-if="selected.status==='Pending'" class="ui-btn ui-btn-primary" :disabled="busy || !linkedGoals.length || !checked || acceptedValue===null || acceptedValue===''" @click="review(true)">{{ selected.inputType==='upload'?'Xác nhận và ghi số liệu':'Duyệt dữ liệu kiểm chứng' }}</button>
        </footer>
      </section>
    </div>
  </div>
</template>
<script setup>
import AiListPagination from '../components/AiListPagination.vue';

import {ref,computed,watch,onMounted,onUnmounted,nextTick} from 'vue';
import {fetchWithAuth} from '../services/auth';
import {getApiUrl} from '../config/api';
import {formatNumber,formatDateTime} from '../shared/formatters';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import AiPageHeader from '../components/AiPageHeader.vue';
import ActionButton from '../components/ActionButton.vue';
import AiAlertsView from './AiAlertsView.vue';
import AiInboxPanel from '../components/AiInboxPanel.vue';
import SearchableSelect from '../components/SearchableSelect.vue';
import {getStatusBadgeClass} from '../shared/statusPresentation';
import QuantityInput from '../components/QuantityInput.vue';
import AiSourceIdentity from '../components/AiSourceIdentity.vue';
import {aiInputCode} from '../shared/aiInputCode';
import {AI_NAVIGATION} from '../shared/aiNavigation';
const props=defineProps({section:{type:String,default:'uploads'},initialTab:{type:String,default:'files'}});
const emit=defineEmits(['navigate']);
const tab=computed(()=>props.section);
const sectionTitle=computed(()=>AI_NAVIGATION.find(item=>item.key===tab.value)?.label||'Tài liệu AI');
const resultInput=ref(null);
function viewInputResults(item){resultInput.value=item;status.value='';metricFilter.value='';year.value='';page.value=1;fileTab.value='review';refresh();}
const catalog=ref(null),rows=ref([]),total=ref(0),page=ref(1),status=ref('Pending'),metricFilter=ref(''),year=ref('');
const loading=ref(true),busy=ref(false),error=ref(''),message=ref(''),selected=ref(null),reviewNote=ref(''),acceptedValue=ref(null),checked=ref(false),reviewError=ref('');
const linkedGoals=ref([]);
const fileTab=ref(props.initialTab),inboxPanel=ref(null),rssPanel=ref(null),rssHistory=ref(null),alertsInbox=ref(null);
const activeMetrics=computed(()=>catalog.value?.metrics.filter(m=>catalogGoals.value.some(g=>g.metric.key===m.key))||[]);
async function openUpload(){fileTab.value='files';await nextTick();inboxPanel.value?.openUpload();}
async function refreshWorkspace(){await refresh(true);inboxPanel.value?.refresh();rssPanel.value?.refresh();alertsInbox.value?.refresh();if(fileTab.value==='history'&&tab.value==='internet')rssHistory.value?.refresh();}

const uploadMetrics=computed(()=>catalog.value?.uploadMetrics||catalog.value?.metrics||[]);
const showingSuggestions=computed(()=>tab.value==='suggestions'||(tab.value==='internet'&&fileTab.value==='review'));
const fileTabs=computed(()=>tab.value==='internet'?[{key:'sources',label:'Nguồn RSS'},{key:'review',label:'Kiểm chứng số liệu'},{key:'history',label:'Lịch sử xử lý'}]:[]);
const pageSize=ref(10);
watch(pageSize,()=>{page.value=1;refresh();});
const currentYear=new Date().getFullYear();let version=0;
const catalogGoals=computed(()=>catalog.value?.metrics.flatMap(m=>m.goalCodes.map(code=>catalog.value.goals.find(g=>g.code===code)).filter(Boolean).map(goal=>({...goal,metric:m})))||[]);
const sourceCode=source=>aiInputCode(source.inputSequence,source.inputReceivedAt,source.inputType==='upload'||source.sourceUrl?.startsWith('/api/ai-inbox/'));
function shortSource(source){
 if(source.inputType==='upload'||source.sourceUrl?.startsWith('/api/ai-inbox/'))return source.sourceTitle;
 try{return new URL(source.sourceUrl).hostname;}catch{return 'Nguồn web';}
}
const metricName=key=>[...(catalog.value?.metrics||[]),...uploadMetrics.value].find(m=>m.key===key)?.name||key;
const unitLabel=unit=>unit==='rank'?'hạng':unit;
const date=value=>value?formatDateTime(value):'Chưa có';
const reviewStatus=s=>({Pending:'Chờ kiểm chứng',Approved:'Đã duyệt',Rejected:'Đã từ chối'}[s]||s);
async function api(path,options){const res=await fetchWithAuth(getApiUrl('/api/ai-data/'+path),options);if(!res.ok){const data=await res.json().catch(()=>({}));throw new Error(data.message||`Không tải được dữ liệu (HTTP ${res.status}).`);}return res.status===204?null:await res.text().then(t=>t?JSON.parse(t):null);}
async function refresh(silent=false){const request=++version;if(!catalog.value)loading.value=true;error.value='';try{if(!catalog.value||!silent){const next=await api('catalog');if(request!==version)return;catalog.value=next;}if(showingSuggestions.value){const q=new URLSearchParams({page:String(page.value),pageSize:String(pageSize.value)});if(resultInput.value)q.set('inboxId',resultInput.value.id);if(showingSuggestions.value){if(status.value)q.set('status',status.value);if(metricFilter.value)q.set('metricKey',metricFilter.value);if(year.value){if(Number(year.value)>currentYear||Number(year.value)<2000)throw new Error('Năm số liệu không hợp lệ.');q.set('year',year.value);}}q.set('internetOnly','true');const data=await api('suggestions?'+q);if(request!==version)return;rows.value=Array.isArray(data)?data:data.items;total.value=data.total??rows.value.length;}}catch(e){if(request===version)error.value=e.message;}finally{if(request===version)loading.value=false;}}
watch(fileTab,value=>{if(value==='history'&&tab.value==='internet')rssHistory.value?.refresh();else if(value==='review')refresh();});
watch([tab,()=>props.initialTab],([value,initial])=>{fileTab.value=value==='internet'?(initial==='review'?'review':'sources'):'files';resultInput.value=null;refresh();});
function handleFileTabKey(event,key){
  const index=fileTabs.value.findIndex(item=>item.key===key);
  let next;
  if(event.key==='ArrowRight')next=(index+1)%fileTabs.value.length;
  else if(event.key==='ArrowLeft')next=(index+fileTabs.value.length-1)%fileTabs.value.length;
  else if(event.key==='Home')next=0;
  else if(event.key==='End')next=fileTabs.value.length-1;
  else return;
  event.preventDefault();fileTab.value=fileTabs.value[next].key;
  event.currentTarget.parentElement.querySelectorAll('[role="tab"]')[next]?.focus();
}
async function act(fn){busy.value=true;error.value='';message.value='';try{await fn();await refresh(true);}catch(e){error.value=e.message;}finally{busy.value=false;}}
async function openReview(s){linkedGoals.value=[];selected.value=s;reviewNote.value='';reviewError.value='';acceptedValue.value=s.acceptedValue??s.value;checked.value=false;busy.value=true;try{linkedGoals.value=await api(`suggestions/${s.id}/goals`);}catch(e){reviewError.value=e.message;}finally{busy.value=false;}}
function closeReview(){if(!busy.value)selected.value=null;}
async function downloadPrivate(path,name){await act(async()=>{const res=await fetchWithAuth(getApiUrl(path));if(!res.ok)throw new Error('Không tải được tài liệu gốc.');const url=URL.createObjectURL(await res.blob());const a=document.createElement('a');a.href=url;a.download=name;a.click();setTimeout(()=>URL.revokeObjectURL(url),10000);});}
async function review(approve){busy.value=true;reviewError.value='';try{await api(`suggestions/${selected.value.id}/review`,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({approve,acceptedValue:approve?Number(acceptedValue.value):null,note:reviewNote.value,verified:checked.value})});selected.value=null;message.value=approve?'Đã duyệt và tạo báo cáo mới cho các mục tiêu liên kết theo đúng năm số liệu.':'Đã từ chối đề xuất.';await refresh(true);}catch(e){reviewError.value=e.message;}finally{busy.value=false;}}
watch([tab,status,metricFilter],()=>{page.value=1;refresh();});watch(page,()=>refresh());
onMounted(()=>refresh());onUnmounted(()=>{version++;});
</script>
<style scoped>
.field{display:block;width:100%;min-height:var(--ui-control-height);margin-top:8px;border:1px solid var(--color-slate-300);border-radius:8px;padding:8px 12px;background:white}
</style>
