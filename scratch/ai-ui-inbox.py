from pathlib import Path
p=Path('frontend/src/components/AiInboxPanel.vue');s=p.read_text(encoding='utf-8-sig');a=s.index('  <p class="p-3 rounded-xl');b=s.index('  <p v-if="error"',a);s=s[:a]+s[b:]
a=s.index('  <form v-if=');b=s.index('  <div v-else',a)
s=s[:a]+'''  <form v-if="mode==='uploads'" @submit.prevent="upload" class="bg-white border border-slate-200 rounded-2xl p-4 sm:p-6 space-y-4">
   <h3 class="ui-section-title">Nạp tài liệu</h3>
   <div class="grid lg:grid-cols-[minmax(240px,1fr)_minmax(0,2fr)] gap-4 items-start">
    <SearchableSelect v-model="metric" :options="metricOptions" label="Chỉ số cần bóc tách" :is-multi="false" :clearable="false" :disabled="busy" />
    <div class="min-w-0"><label for="ai-document" class="block font-semibold mb-2">Tài liệu</label>
     <input id="ai-document" class="block w-full max-w-full rounded-lg border border-slate-300 p-2 text-xs file:mr-3 file:rounded-md file:border-0 file:bg-blue-50 file:px-3 file:py-2 file:text-blue-800" aria-describedby="ai-upload-help" type="file" accept=".pdf,.docx,.txt" :disabled="busy" required @change="documentFile=$event.target.files[0]" />
     <p id="ai-upload-help" class="text-slate-600 mt-2">PDF có chữ, DOCX hoặc TXT UTF-8 · Tối đa 12 MB</p>
    </div>
   </div>
   <div class="flex justify-end border-t border-slate-100 pt-4"><ActionButton kind="import" type="submit" :disabled="busy || !documentFile" aria-describedby="ai-upload-help">Nạp tài liệu để AI đọc</ActionButton></div>
  </form>
'''+s[b:]
s=s.replace('<h3 class="font-bold">Tài liệu đã nạp</h3>','<h3 class="ui-section-title">Tài liệu đã nạp</h3>')
s=s.replace('rounded-xl overflow-hidden','rounded-2xl overflow-hidden')
s=s.replace('<th>AI đánh giá</th>','<th>Chỉ số</th>')
s=s.replace('<td>{{ statusLabel(item.status) }}','<td><span class="inline-flex rounded-full px-2 py-1 text-xs font-semibold" :class="getStatusBadgeClass(item.status===\'Error\'?\'Rejected\':item.status===\'Read\'?\'Approved\':\'Pending\')">{{ statusLabel(item.status) }}</span>')
s=s.replace('<p>{{ item.origin }} · {{ formatDateTime(item.receivedAt) }}</p>','<p class="text-slate-600 mt-2">{{ formatDateTime(item.receivedAt) }}</p>')
s=s.replace('<p>{{ item.reason }}</p><p v-if="item.model" class="text-slate-500">Model: {{ item.model }}</p>','<details v-if="item.reason || item.model || item.origin" class="mt-2 text-slate-600"><summary class="cursor-pointer">Chi tiết</summary><p v-if="item.reason" class="mt-2">{{ item.reason }}</p><p>{{ item.origin }}</p><p v-if="item.model" class="break-words">{{ item.model }}</p></details>')
s=s.replace("import {loadExcelReader}","import {getStatusBadgeClass} from '../shared/statusPresentation';\nimport {loadExcelReader}")
s=s.replace('th{background:var(--color-slate-50);font-weight:600;}','th{background:var(--color-slate-50);font-weight:600;position:sticky;top:0;z-index:1;}')
p.write_text(s,encoding='utf-8')
p=Path('frontend/tests/ai-inbox.browser.mjs');s=p.read_text(encoding='utf-8-sig').replace("'Chọn tệp trên máy trước; nút Nạp tài liệu sẽ bật sau khi chọn file.'","'PDF có chữ, DOCX hoặc TXT UTF-8 · Tối đa 12 MB'");p.write_text(s,encoding='utf-8')
