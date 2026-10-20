from pathlib import Path
p=Path('frontend/src/components/AiInboxPanel.vue');s=p.read_text(encoding='utf-8-sig');a=s.index('  <div v-else class="bg-white');b=s.index('  <div class="flex justify-between',a);s=s[:a]+'''  <div v-else class="bg-white border border-slate-200 rounded-2xl p-4"><div class="max-w-64"><SearchableSelect v-model="statusFilter" label="Trạng thái tiêu đề" :is-multi="false" :clearable="false" :disabled="busy" :options="alertStatusOptions" /></div></div>
'''+s[b:]
s=s.replace('<h3 class="ui-section-title">Tài liệu đã nạp</h3>','<h3 class="ui-section-title">{{ mode===\'uploads\'?\'Tài liệu đã nạp\':\'Tiêu đề đã nhận\' }}</h3>')
s=s.replace('<th>Tài liệu</th>',"<th>{{ mode==='uploads'?'Tài liệu':'Tiêu đề bài viết' }}</th>")
s=s.replace('Chưa có thông báo hoặc tài liệu.',"{{ mode==='uploads'?'Chưa có tài liệu.':'Chưa có tiêu đề phù hợp.' }}")
s=s.replace('<h3 class="ui-title">Chi tiết tài liệu</h3>',"<h3 class=\"ui-title\">{{ mode==='uploads'?'Chi tiết tài liệu':'Chi tiết tiêu đề' }}</h3>")
s=s.replace('     <button v-if="!reread"', '     <button v-if="!selectedItem.isUpload" type="button" class="ui-btn ui-btn-secondary" :disabled="busy||selectedItem.processing" @click="queue(selectedItem,true)">Đánh giá lại tiêu đề</button>\n     <button v-if="!reread"')
s=s.replace('import {ref,computed,onMounted,onUnmounted}', 'import {ref,computed,watch,onMounted,onUnmounted}')
s=s.replace('const metricOptions=', '''const statusFilter=ref('');
const alertStatusOptions=[{value:'',label:'Tất cả'},{value:'TitleQueued',label:'Chờ đánh giá'},{value:'NeedsReview',label:'Cần xem lại'},{value:'NotRelevant',label:'Không liên quan'},{value:'ReadQueued',label:'Chờ đọc'},{value:'Read',label:'Đã có số liệu'},{value:'ReadNoData',label:'Chưa có số liệu'},{value:'Error',label:'Lỗi xử lý'}];
watch(statusFilter,()=>{page.value=1;load();});
const metricOptions=''')
s=s.replace("(props.mode==='uploads'));", "(props.mode==='uploads')+'&alertsOnly='+(props.mode==='alerts')+'&status='+encodeURIComponent(statusFilter.value));")
s=s.replace('loading.value=true;try{const d=await api', 'if(!rows.value.length)loading.value=true;try{const d=await api')
p.write_text(s,encoding='utf-8')
