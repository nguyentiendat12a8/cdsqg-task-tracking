from pathlib import Path
root=Path('frontend/src')
p=root/'views/AiDataView.vue';s=p.read_text(encoding='utf-8-sig')
s=s.replace('class="p-3 flex gap-3 flex-wrap border-b border-slate-200"','class="p-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-[220px_minmax(240px,1fr)_140px] border-b border-slate-200 bg-slate-50/50"')
s=s.replace('<label><SearchableSelect','<div class="min-w-0"><SearchableSelect').replace('</SearchableSelect></label>','</SearchableSelect></div>')
s=s.replace('}]\" /></label>','}]\" /></div>').replace('}]" /></label>','}]" /></div>').replace('...catalog.metrics.map(m=>({value:m.key,label:m.name}))]" /></label>','...catalog.metrics.map(m=>({value:m.key,label:m.name}))]" /></div>')
s=s.replace('<label>Năm số liệu<input','<label class="font-semibold">Năm số liệu<input').replace('class="field w-32"','class="field"')
s=s.replace('<th>Giá trị số liệu</th>','<th class="text-right">Giá trị số liệu</th>').replace('<td class="font-bold">{{ formatNumber','<td class="font-semibold text-right tabular-nums">{{ formatNumber')
s=s.replace('<p>{{ s.dataYear }} · {{ s.periodLabel }}</p>','<p class="mt-2 text-slate-600">{{ s.dataYear }}<span v-if="s.periodLabel && String(s.periodLabel)!==String(s.dataYear)"> · {{ s.periodLabel }}</span></p><span class="inline-flex mt-2 rounded-full px-2 py-1 text-xs font-semibold" :class="getStatusBadgeClass(s.status)">{{ reviewStatus(s.status) }}</span>')
s=s.replace('<a :href="s.sourceUrl" target="_blank" rel="noopener noreferrer" class="text-blue-700 underline">{{ s.sourceTitle }}</a><p class="text-amber-800 mt-1 max-w-lg">{{ s.warnings }}</p>','<button v-if="s.sourceUrl.startsWith(\'/api/ai-inbox/\')" class="text-blue-700 text-left font-semibold hover:underline break-words" @click="downloadPrivate(s.sourceUrl,s.sourceTitle)">{{ s.sourceTitle }}</button><a v-else :href="s.sourceUrl" target="_blank" rel="noopener noreferrer" class="text-blue-700 underline">{{ s.sourceTitle }}</a><p v-if="s.warnings" class="text-amber-800 mt-2">Có lưu ý cần kiểm chứng</p>')
s=s.replace('import QuantityInput from',"import {getStatusBadgeClass} from '../shared/statusPresentation';\nimport QuantityInput from")
a=s.index('    <div v-if="selected"');b=s.index('\n  </div>\n</template>',a)
s=s[:a]+'''    <div v-if="selected" v-accessible-dialog="closeReview" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3" @click.self="closeReview">
      <section class="bg-white rounded-2xl w-full max-w-4xl max-h-[90dvh] flex flex-col overflow-hidden shadow-xl">
        <header class="flex justify-between items-start gap-4 p-4 sm:p-6 border-b border-slate-200 shrink-0">
          <div><h3 class="ui-title">Kiểm chứng số liệu</h3><p class="text-slate-600 mt-2">{{ metricName(selected.metricKey) }}</p></div>
          <button class="ui-btn ui-btn-secondary" :disabled="busy" @click="closeReview">Đóng</button>
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
            <div class="flex justify-between items-center gap-3 flex-wrap"><h4 class="ui-section-title">Dẫn chứng</h4><button v-if="selected.sourceUrl.startsWith('/api/ai-inbox/')" class="ui-btn ui-btn-secondary" :disabled="busy" @click="downloadPrivate(selected.sourceUrl,selected.sourceTitle)">Tải tài liệu gốc</button><a v-else :href="selected.sourceUrl" target="_blank" rel="noopener noreferrer" class="text-blue-700 underline break-all">{{ selected.sourceTitle }}</a></div>
            <blockquote class="p-4 rounded-r-xl bg-slate-50 border-l-4 border-blue-600 whitespace-pre-wrap break-words">{{ selected.quote }}</blockquote>
            <p class="text-slate-600">Phương pháp: {{ selected.methodology || 'Chưa xác định' }}</p>
            <p v-if="selected.warnings" class="text-amber-900 bg-amber-50 rounded-xl p-3">{{ selected.warnings }}</p>
            <button class="ui-btn ui-btn-secondary" :disabled="busy" @click="loadSnapshot">Xem bản chụp văn bản trang nguồn</button>
            <pre v-if="snapshot" class="whitespace-pre-wrap break-words max-h-64 overflow-auto bg-slate-50 p-3 text-xs">{{ snapshot }}</pre>
            <details class="text-slate-600"><summary class="cursor-pointer font-semibold">Thông tin đối chiếu</summary><dl class="mt-3 space-y-2 break-all"><div><dt class="font-semibold">Tài liệu</dt><dd>{{ selected.sourceTitle }}</dd></div><div><dt class="font-semibold">Kỳ số liệu</dt><dd>{{ selected.periodLabel }}</dd></div><div><dt class="font-semibold">Phiên bản AI</dt><dd>{{ selected.model }}</dd></div><div><dt class="font-semibold">SHA-256</dt><dd>{{ selected.contentHash }}</dd></div></dl></details>
          </section>
          <section v-if="selected.status==='Pending'" class="border-t border-slate-200 pt-4 space-y-4">
            <label class="block font-semibold">Giá trị đã kiểm chứng<QuantityInput v-model="acceptedValue" class="field" /></label>
            <label class="block font-semibold">Ghi chú kiểm chứng hoặc lý do từ chối<textarea v-model="reviewNote" minlength="5" maxlength="2000" class="field font-normal" rows="3"></textarea></label>
            <label class="flex items-start gap-3 rounded-xl border border-slate-200 p-3"><input v-model="checked" type="checkbox" class="mt-1 shrink-0" /><span>Tôi xác nhận số liệu, kỳ, phạm vi và phương pháp đã đúng; đồng ý tạo báo cáo được duyệt cho các mục tiêu liên kết.</span></label>
          </section>
          <p v-else>{{ reviewStatus(selected.status) }} · {{ selected.reviewedBy }} · {{ selected.reviewNote }}</p>
        </div>
        <footer v-if="selected.status==='Pending'" class="p-4 sm:px-6 border-t border-slate-200 bg-slate-50 flex justify-end gap-2 flex-wrap shrink-0">
          <button class="ui-btn ui-btn-secondary" :disabled="busy || reviewNote.trim().length<5" @click="review(false)">Từ chối</button><button class="ui-btn ui-btn-primary" :disabled="busy || !linkedGoals.length || !checked || reviewNote.trim().length<5 || acceptedValue===null || acceptedValue===''" @click="review(true)">Duyệt dữ liệu kiểm chứng</button>
        </footer>
      </section>
    </div>'''+s[b:]
s=s.replace('th{background:var(--color-slate-50);font-weight:600;}','th{background:var(--color-slate-50);font-weight:600;position:sticky;top:0;z-index:1;}')
p.write_text(s,encoding='utf-8')
p=root/'views/AiSettingsView.vue';s=p.read_text(encoding='utf-8-sig')
s=s.replace('<p class="text-slate-500 mb-1">Quản trị dữ liệu AI</p>','').replace('class="text-lg font-bold text-slate-900"','class="ui-page-title"').replace('<p class="mt-1 text-slate-600">Kết nối AI để đọc tài liệu và đề xuất số liệu cho admin kiểm chứng.</p>','')
s=s.replace('<span class="block mt-1">Khóa được giữ trên máy chủ, không hiển thị lại ở đây.</span>','')
s=s.replace('<p class="mt-1 text-slate-500">Chọn phiên bản AI sẽ đọc và bóc tách số liệu trong tài liệu. API key cấp quyền truy cập; phiên bản AI quyết định cách xử lý, tốc độ và chi phí.</p>','')
s=s.replace('<p v-else class="text-slate-500">Danh sách lấy từ tài khoản API. Kiểm tra kết nối để xác nhận AI đã chọn dùng được cho việc đọc tài liệu.</p>','')
s=s.replace('<p class="mt-1 text-slate-500">Bật và lưu để xử lý các file đang chờ trong hệ thống.</p>','')
s=s.replace('<span class="mt-1 block text-slate-600">AI chỉ đề xuất số liệu. Admin vẫn kiểm tra và duyệt trước khi ghi báo cáo.</span>','')
s=s.replace('Kiểm tra kết nối gửi một yêu cầu AI ngắn, có thể phát sinh phí; không lưu cấu hình hay đọc tài liệu.','Kiểm tra kết nối có thể phát sinh phí API.')
a=s.index('    <section class="rounded-2xl border border-blue-100');b=s.index('</section>',a)+len('</section>');s=s[:a]+s[b:]
s=s.replace("return host==='api.openai.com'?'OpenAI':host", "return host==='api.openai.com'?'OpenAI':host==='generativelanguage.googleapis.com'?'Google Gemini':host")
s=s.replace('lg:grid-cols-3','lg:grid-cols-[minmax(0,1fr)_300px]').replace('lg:col-span-2 ','')
s=s.replace('sm:p-5','sm:p-6')
p.write_text(s,encoding='utf-8')
