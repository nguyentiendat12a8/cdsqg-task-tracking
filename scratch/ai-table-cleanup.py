from pathlib import Path
p=Path('frontend/src/components/AiInboxPanel.vue');s=p.read_text(encoding='utf-8-sig')
s=s.replace('lg:grid-cols-[minmax(240px,1fr)_minmax(0,2fr)]','lg:grid-cols-[minmax(220px,1fr)_minmax(0,1.6fr)_auto]')
s=s.replace('    </div>\n   </div>\n   <div class="flex justify-end border-t border-slate-100 pt-4"><ActionButton','    </div>\n    <div class="lg:pt-6"><ActionButton').replace('</ActionButton></div>\n  </form>','</ActionButton></div>\n   </div>\n  </form>')
a=s.index('   <div class="overflow-auto max-h-[60vh]">');b=s.index('   <footer ',a)
s=s[:a]+'''   <div class="overflow-auto max-h-[60vh]"><table class="inbox-table w-full text-left text-xs"><colgroup><col class="document-col" /><col class="metric-col" /><col class="status-col" /><col class="action-col" /></colgroup><thead><tr><th>Tài liệu</th><th>Chỉ số</th><th>Trạng thái</th><th class="text-right">Thao tác</th></tr></thead><tbody>
    <tr v-for="item in rows" :key="item.id" class="hover:bg-slate-50/60">
     <td><p class="font-semibold break-words">{{ item.title }}</p><p class="text-slate-600 mt-1">{{ formatDateTime(item.receivedAt) }}</p></td>
     <td>{{ metricNames(item.metricKeysJson) || 'Chưa chọn' }}</td>
     <td><span class="inline-flex rounded-full px-2 py-1 text-xs font-semibold" :class="getStatusBadgeClass(item.processing?'InProgressOnTime':item.status==='Error'?'Rejected':item.status==='Read'?'Pending':'NotStarted')">{{ item.processing?'Đang xử lý':statusLabel(item.status) }}</span></td>
     <td class="text-right"><button class="ui-btn ui-btn-secondary" @click="openDetails(item)">Chi tiết</button></td>
    </tr></tbody></table><p v-if="!rows.length" class="p-6 text-center">Chưa có thông báo hoặc tài liệu.</p></div>
'''+s[b:]
a=s.index('\n </section>\n</template>')
s=s[:a]+'''
  <div v-if="selectedItem" v-accessible-dialog="closeDetails" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3" @click.self="closeDetails">
   <section class="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90dvh] flex flex-col overflow-hidden">
    <header class="flex justify-between gap-4 p-4 sm:p-6 border-b border-slate-200"><div class="min-w-0"><h3 class="ui-title">Chi tiết tài liệu</h3><p class="mt-2 text-slate-600 break-words">{{ selectedItem.title }}</p></div><button class="ui-btn ui-btn-secondary shrink-0" :disabled="busy" @click="closeDetails">Đóng</button></header>
    <div class="overflow-y-auto min-h-0 p-4 sm:p-6 space-y-4">
     <p v-if="error" role="alert" class="rounded-xl bg-rose-50 p-3 text-rose-800">{{ error }}</p>
     <dl class="grid sm:grid-cols-2 gap-4"><div><dt class="text-slate-600">Trạng thái</dt><dd class="font-semibold mt-1">{{ selectedItem.processing?'Đang xử lý':statusLabel(selectedItem.status) }}</dd></div><div><dt class="text-slate-600">Thời gian nạp</dt><dd class="mt-1">{{ formatDateTime(selectedItem.receivedAt) }}</dd></div><div class="sm:col-span-2"><dt class="text-slate-600">Chỉ số</dt><dd class="mt-1">{{ metricNames(selectedItem.metricKeysJson) || 'Chưa chọn' }}</dd></div></dl>
     <p v-if="selectedItem.error" class="text-rose-800 bg-rose-50 p-3 rounded-xl">{{ selectedItem.error }}</p>
     <p v-if="selectedItem.reason">{{ selectedItem.reason }}</p>
     <p v-if="selectedItem.origin" class="text-slate-600">{{ selectedItem.origin }}</p>
     <p v-if="selectedItem.model" class="text-slate-600 break-words">AI: {{ selectedItem.model }}</p>
     <button v-if="selectedItem.isUpload" class="ui-btn ui-btn-secondary" :disabled="busy" @click="download(selectedItem)">Tải file gốc</button><a v-else-if="selectedItem.url" :href="selectedItem.url" target="_blank" rel="noopener noreferrer" class="text-blue-700 underline break-all">{{ selectedItem.url }}</a>
     <div v-if="reread" class="border-t border-slate-200 pt-4"><SearchableSelect v-model="choices[selectedItem.id]" :options="metricOptions" label="Chỉ số đọc lại" :is-multi="false" :clearable="false" :disabled="busy||selectedItem.processing" /></div>
    </div>
    <footer class="p-4 sm:px-6 bg-slate-50 border-t border-slate-200 flex flex-wrap justify-end gap-2">
     <button v-if="!reread" class="ui-btn ui-btn-secondary" :disabled="busy||selectedItem.processing" @click="reread=true">Đọc lại</button>
     <button v-if="reread" class="ui-btn ui-btn-primary" :disabled="busy||selectedItem.processing||!choices[selectedItem.id]" @click="queue(selectedItem,false)">Bắt đầu đọc lại</button>
     <button v-else-if="selectedItem.status==='Read'" class="ui-btn ui-btn-primary" @click="viewSuggestions">Xem đề xuất kiểm chứng</button>
    </footer>
   </section>
  </div>'''+s[a:]
s=s.replace("defineEmits(['view-suggestions']);","const emit=defineEmits(['view-suggestions']);\nconst selected=ref(null),reread=ref(false);\nconst selectedItem=computed(()=>selected.value?(rows.value.find(item=>item.id===selected.value.id)||selected.value):null);\nfunction openDetails(item){selected.value=item;reread.value=false;try{choices.value[item.id]=JSON.parse(item.metricKeysJson)[0]||props.metrics[0]?.key;}catch{choices.value[item.id]=props.metrics[0]?.key;}}\nfunction closeDetails(){if(!busy.value)selected.value=null;}\nfunction viewSuggestions(){selected.value=null;emit('view-suggestions');}")
s=s.replace("TitleQueued:'Chờ AI lọc tiêu đề',ReadQueued:'Có khả năng liên quan — chờ đọc',NeedsReview:'Chưa rõ — cần admin xem',NotRelevant:'AI đánh giá chưa liên quan',Read:'Đã có đề xuất — chờ kiểm chứng',ReadNoData:'Đã xử lý — chưa có số liệu hợp lệ'", "TitleQueued:'Chờ lọc',ReadQueued:'Chờ đọc',NeedsReview:'Cần xem lại',NotRelevant:'Không liên quan',Read:'Chờ kiểm chứng',ReadNoData:'Chưa có số liệu'")
s=s.replace('th,td{padding:12px;border-bottom:1px solid var(--color-slate-200);vertical-align:top;min-width:180px;}','th,td{padding:12px 16px;border-bottom:1px solid var(--color-slate-200);vertical-align:middle;}')
s=s.replace('td:first-child{min-width:250px;max-width:480px;}','table{table-layout:fixed;min-width:760px;}.document-col{width:38%;}.metric-col{width:32%;}.status-col{width:18%;}.action-col{width:12%;}')
p.write_text(s,encoding='utf-8')
p=Path('frontend/src/views/AiDataView.vue');s=p.read_text(encoding='utf-8-sig')
s=s.replace('sm:grid-cols-2 xl:grid-cols-[220px_minmax(240px,1fr)_140px]','sm:grid-cols-2 xl:grid-cols-[200px_minmax(240px,420px)_120px]')
a=s.index('        <table class="w-full text-left"><thead><tr><th>Chỉ số / Kỳ</th>');b=s.index('\n        <p v-if="!rows.length"',a)
s=s[:a]+'''        <table class="review-table w-full text-left"><colgroup><col style="width:30%" /><col style="width:7%" /><col style="width:10%" /><col style="width:27%" /><col style="width:16%" /><col style="width:10%" /></colgroup><thead><tr><th>Chỉ số</th><th>Năm</th><th class="text-right">Giá trị</th><th>Nguồn</th><th>Trạng thái</th><th class="text-right">Thao tác</th></tr></thead><tbody>
          <tr v-for="s in rows" :key="s.id" class="hover:bg-slate-50/60">
            <td>{{ metricName(s.metricKey) }}</td><td class="tabular-nums">{{ s.dataYear }}</td>
            <td class="text-right tabular-nums whitespace-nowrap"><strong>{{ formatNumber(s.status==='Approved'?(s.acceptedValue??s.value):s.value) }}</strong> {{ unitLabel(s.unit) }}</td>
            <td><button v-if="s.sourceUrl.startsWith('/api/ai-inbox/')" class="text-blue-700 text-left hover:underline break-words" @click="downloadPrivate(s.sourceUrl,s.sourceTitle)">{{ s.sourceTitle }}</button><a v-else :href="s.sourceUrl" target="_blank" rel="noopener noreferrer" class="text-blue-700 hover:underline break-words">{{ s.sourceTitle }}</a></td>
            <td><span class="inline-flex rounded-full px-2 py-1 text-xs font-semibold" :class="getStatusBadgeClass(s.status)">{{ reviewStatus(s.status) }}</span></td>
            <td class="text-right"><button class="ui-btn ui-btn-secondary whitespace-nowrap" @click="openReview(s)">{{ s.status==='Pending'?'Kiểm chứng':'Xem chi tiết' }}</button></td>
          </tr>
        </tbody></table>'''+s[b:]
s=s.replace('padding:12px;vertical-align:top;', 'padding:12px 16px;vertical-align:middle;').replace('td{min-width:130px;}','td{min-width:130px;}.review-table{table-layout:fixed;min-width:1000px;}.review-table td{min-width:0;}')
p.write_text(s,encoding='utf-8')
