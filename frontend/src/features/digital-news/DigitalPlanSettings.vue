<template>
 <div v-accessible-dialog="close" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3">
  <section class="bg-white rounded-2xl w-full max-w-5xl max-h-[90dvh] flex flex-col overflow-hidden shadow-xl">
   <header class="p-4 sm:p-6 bg-[#f1f5fc] border-b border-slate-200"><h3 class="ui-title">Cấu hình theo dõi thông tin liên quan</h3></header>
   <nav class="flex flex-wrap gap-2 px-4 pt-4" aria-label="Cấu hình"><button v-for="item in tabs" :key="item.key" type="button" class="ui-btn" :class="tab===item.key?'ui-btn-primary':'ui-btn-secondary'" @click="tab=item.key">{{ item.label }}</button></nav>
   <form ref="formElement" class="overflow-y-auto p-4 sm:p-6 space-y-4" id="plan-monitor-form" @submit.prevent="save">
    <p v-if="error" role="alert" class="bg-rose-50 text-rose-800 rounded-lg p-3">{{ error }}</p>
    <div v-if="tab==='schedule'" class="space-y-4">
     <div class="grid sm:grid-cols-2 gap-4"><label>Chu kỳ quét (phút)<input v-model.number="form.intervalMinutes" required type="number" min="15" max="10080" class="field mt-2" /></label><label>Số truy vấn mỗi lượt<input v-model.number="form.batchSize" required type="number" min="1" max="20" class="field mt-2" /><span class="block mt-1 text-slate-600">Tối đa 20 truy vấn để một lượt quét</span></label></div>
     <fieldset class="border border-slate-200 rounded-xl p-3"><legend class="px-1 font-semibold">Thời gian đăng bài</legend><div class="grid sm:grid-cols-2 gap-4"><div><label for="news-published-from" class="block mb-2">Từ ngày</label><DatePicker v-model="form.publishedFrom" input-id="news-published-from" :required="true" placeholder="dd/mm/yyyy" /></div><div><label for="news-published-to" class="block mb-2">Đến ngày</label><DatePicker v-model="form.publishedTo" input-id="news-published-to" placeholder="Để trống: đến hiện tại" /></div></div><p class="mt-2 text-slate-500">Mặc định từ ngày 1 của tháng hiện tại, theo giờ Việt Nam. Chỉ lấy bài có ngày đăng trong khoảng đã chọn; bài đã lưu được giữ nguyên.</p></fieldset>
     <label class="flex items-center gap-2"><input v-model="form.enabled" type="checkbox"/>Bật quét tự động</label>
     <p v-if="settings.lastError" class="text-amber-800">{{ settings.lastError }}</p>
    </div>
    <div v-else-if="tab==='criteria'" class="space-y-4">
     <p class="rounded-xl border border-blue-200 bg-blue-50 p-3 text-blue-900">Chỉ đọc và đánh giá tiêu đề; không tải nội dung bài viết. Các từ khóa dưới đây dùng để đánh giá khi bạn bấm Đọc, độc lập với từ khóa thu thập tin.</p>
     <div class="grid sm:grid-cols-2 gap-4"><label>Từ khóa liên quan<textarea aria-label="Từ khóa liên quan" v-model="form.evaluationCriteria.relatedKeywords" required maxlength="6200" rows="6" class="criteria-field mt-2"/></label><label>Từ khóa hành động ban hành<textarea aria-label="Từ khóa hành động ban hành" v-model="form.evaluationCriteria.actionKeywords" required maxlength="3000" rows="6" class="criteria-field mt-2"/></label></div>
     <label class="block">Từ khóa loại trừ<textarea aria-label="Từ khóa loại trừ" v-model="form.evaluationCriteria.excludedKeywords" maxlength="3000" rows="6" class="criteria-field mt-2"/></label>

     <p class="text-slate-600">Kết quả phù hợp cần có từ khóa liên quan, hành động và không có từ khóa loại trừ. AI tự nhận diện kế hoạch, đề án, chương trình, nghị quyết, chiến lược chuyển đổi số hoặc kiến trúc số/điện tử từ tiêu đề. Năm chỉ hiển thị khi tiêu đề nêu rõ. Đổi tiêu chí không tự gọi AI; bấm Đọc để đánh giá lại theo cấu hình mới, kết quả cũ được giữ trong dữ liệu.</p>
    </div>
    <template v-else>
     <div class="flex flex-wrap items-end gap-3">
     <SearchInput v-model="search" placeholder="Tên nguồn, bộ/tỉnh hoặc từ khóa…" aria-label="Tìm cấu hình" wrapper-class="flex-1 min-w-0 basis-64" />
     <button type="button" class="ui-btn ui-btn-secondary" @click="add">+ {{ tab==='sources'?'Thêm nguồn':'Thêm từ khóa' }}</button>
     </div>
     <div v-if="tab==='sources'" class="space-y-3"><article v-for="source in visibleSources" :key="source.id" class="border border-slate-200 rounded-xl p-3 space-y-3"><div class="flex justify-between gap-3"><label class="flex gap-2 items-center font-semibold"><input v-model="source.enabled" type="checkbox"/>{{ source.name||'Nguồn mới' }}</label><button type="button" class="ui-btn ui-btn-secondary" @click="remove('sources',source)">Xóa</button></div><div class="grid sm:grid-cols-2 gap-3"><label>Tên nguồn<input v-model.trim="source.name" :data-new-entry="source.id===newEntryId ? '' : null" required maxlength="300" class="field mt-1"/></label><label>Bộ / tỉnh<input v-model.trim="source.agencyName" required maxlength="300" class="field mt-1"/></label><label>Tên miền<input v-model.trim="source.domain" required maxlength="250" class="field mt-1" placeholder="hanoi.gov.vn"/></label><label>Từ khóa bổ sung<input v-model.trim="source.requiredTerms" maxlength="300" class="field mt-1"/></label></div></article></div>
     <div v-else class="space-y-3"><article v-for="keyword in visibleKeywords" :key="keyword.id" class="border border-slate-200 rounded-xl p-3 space-y-3"><div class="flex justify-between gap-3"><label class="flex gap-2 items-center font-semibold"><input v-model="keyword.enabled" type="checkbox"/>{{ keyword.phrase||'Từ khóa mới' }}</label><button type="button" class="ui-btn ui-btn-secondary" @click="remove('keywords',keyword)">Xóa</button></div><div class="grid sm:grid-cols-3 gap-3"><label>Nhóm<input v-model.trim="keyword.name" maxlength="300" class="field mt-1"/></label><label>Cụm tìm kiếm<input v-model.trim="keyword.phrase" :data-new-entry="keyword.id===newEntryId ? '' : null" required maxlength="300" class="field mt-1"/></label><label>Điều kiện đi kèm<input v-model.trim="keyword.conditions" maxlength="500" class="field mt-1"/></label></div></article></div>
    </template>
   </form>
   <footer class="p-4 border-t border-slate-200 bg-slate-50 flex justify-end gap-2"><button type="button" class="ui-btn ui-btn-secondary" :disabled="saving" @click="close">Đóng</button><button type="submit" form="plan-monitor-form" class="ui-btn ui-btn-primary" :disabled="saving">{{ saving?'Đang lưu…':tab==='schedule'?'Lưu lịch quét':tab==='criteria'?'Lưu tiêu chí đánh giá':'Lưu nguồn và từ khóa' }}</button></footer>
  </section>
 </div>
</template>
<script setup>
import {ref,computed,nextTick} from 'vue';
import SearchInput from '../../components/SearchInput.vue';
import DatePicker from '../../components/DatePicker.vue';
import {fetchWithAuth} from '../../services/auth';
import {getApiUrl} from '../../config/api';
const props=defineProps({settings:{type:Object,required:true}}),emit=defineEmits(['close','saved']);
const form=ref(JSON.parse(JSON.stringify(props.settings))),tab=ref('schedule'),search=ref(''),error=ref(''),saving=ref(false),formElement=ref(null),newEntryId=ref(null);
const vnDate=new Intl.DateTimeFormat('en-CA',{timeZone:'Asia/Ho_Chi_Minh',year:'numeric',month:'2-digit',day:'2-digit'}).format(new Date());
form.value.publishedFrom??=vnDate.slice(0,7)+'-01';
form.value.publishedTo??=null;
const previous=form.value.evaluationCriteria||{};
form.value.evaluationCriteria={
 relatedKeywords:[...new Set(((previous.relatedKeywords??'chuyển đổi số\nkhung kiến trúc số\nkhung kiến trúc chính phủ số\nkhung kiến trúc chính quyền số')+'\n'+(previous.architectureKeywords||'')).split('\n').map(x=>x.trim()).filter(Boolean))].join('\n'),
 actionKeywords:previous.actionKeywords??'ban hành\nphê duyệt',excludedKeywords:previous.excludedKeywords??'dự thảo'
};
const tabs=[{key:'schedule',label:'Lịch quét'},{key:'sources',label:'Nguồn theo dõi'},{key:'keywords',label:'Từ khóa thu thập'},{key:'criteria',label:'Tiêu chí đánh giá'}];
const match=row=>Object.values(row).join(' ').toLocaleLowerCase('vi').includes(search.value.toLocaleLowerCase('vi'));
const sortByEnabled=(list)=>[...(list||[])].sort((a,b)=>{const aVal=a.enabled?1:0;const bVal=b.enabled?1:0;if(bVal!==aVal)return bVal-aVal;return (a.sortOrder||0)-(b.sortOrder||0);});
const visibleSources=computed(()=>sortByEnabled(form.value.sources.filter(match))),visibleKeywords=computed(()=>sortByEnabled(form.value.keywords.filter(match)));
function close(){if(!saving.value)emit('close');}
function remove(key,row){form.value[key]=form.value[key].filter(x=>x.id!==row.id);}
async function add(){
 search.value='';
 const key=tab.value,id=crypto.randomUUID();
 const firstOrder=Math.min(0,...form.value[key].map(item=>item.sortOrder??0));
 form.value[key].unshift({id,sortOrder:firstOrder-1,enabled:true,name:'',...(key==='sources'?{agencyName:'',domain:'',requiredTerms:''}:{phrase:'',conditions:''})});
 newEntryId.value=id;
 await nextTick();
 const input=formElement.value?.querySelector('[data-new-entry]');
 input?.scrollIntoView({block:'center',behavior:'smooth'});
 input?.focus({preventScroll:true});
}
async function save(){saving.value=true;error.value='';try{const path=tab.value==='schedule'?'/settings':tab.value==='criteria'?'/criteria':'/monitoring';const payload=tab.value==='criteria'?form.value.evaluationCriteria:tab.value==='schedule'?{...form.value,publishedFrom:form.value.publishedFrom||null,publishedTo:form.value.publishedTo||null}:{...form.value,sources:sortByEnabled(form.value.sources).map((s,idx)=>({...s,sortOrder:idx+1})),keywords:sortByEnabled(form.value.keywords).map((k,idx)=>({...k,sortOrder:idx+1}))};const res=await fetchWithAuth(getApiUrl('/api/digital-news'+path),{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify(payload)});const body=await res.json();if(!res.ok)throw new Error(body.message||'Không lưu được cấu hình.');emit('saved',body.message);}catch(e){error.value=e.message;}finally{saving.value=false;}}
</script>
<style scoped>.field{display:block;width:100%;height:36px;border:1px solid var(--color-slate-300);border-radius:8px;padding:8px 12px;background:white;min-width:0}.criteria-field{display:block;width:100%;border:1px solid var(--color-slate-300);border-radius:8px;padding:8px 12px;background:white;resize:vertical}</style>
