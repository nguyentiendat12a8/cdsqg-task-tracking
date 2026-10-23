<template>
 <section class="space-y-3">
  <p v-if="error" role="alert" class="p-3 bg-rose-50 text-rose-800 rounded-xl">{{ error }}</p>
  <div class="ai-panel">
   <div class="ai-toolbar ai-inbox-toolbar"><div class="ai-list-filters flex flex-wrap gap-4 items-end flex-1"><div v-if="mode==='all'" class="w-full sm:w-52"><SearchableSelect v-model="sourceFilter" label="Loại nguồn" :options="sourceOptions" :is-multi="false" :clearable="false" :disabled="busy"/></div><div class="w-full sm:w-60"><SearchableSelect v-model="statusFilter" label="Trạng thái xử lý" :is-multi="false" :clearable="false" :disabled="busy" :options="visibleStatusOptions" /></div></div><div class="ai-list-actions flex items-center gap-3"><button type="button" class="ui-btn ui-btn-secondary" :disabled="loading||busy" @click="load">Tải lại hàng đợi</button><ActionButton v-if="mode==='uploads'" kind="create" :disabled="busy" @click="openUpload">Nạp tài liệu</ActionButton></div></div>
   <LoadingSpinner v-if="loading" text="Đang tải hộp tiếp nhận..." />
   <div v-else class="ai-records-layout">
    <div class="ai-wide-list ai-table-scroll"><table class="ai-table"><colgroup><col class="ai-stt-column"/><col style="width:31%"/><col style="width:22%"/><col style="width:21%"/><col style="width:14%"/><col style="width:12%"/></colgroup><thead><tr><th scope="col" class="ai-stt">STT</th><th>Tài liệu / Bài viết</th><th>Nguồn</th><th>{{ mode==='uploads'?'Mục tiêu':'Chỉ số' }}</th><th class="text-center">Trạng thái</th><th class="text-center">Thao tác</th></tr></thead><tbody><tr v-for="(item,index) in rows" :key="item.id"><td class="ai-stt">{{ (page-1)*pageSize+index+1 }}</td><td><p class="ai-input-code">{{ inputCode(item) }}</p><p class="font-semibold">{{ item.title }}</p><p class="ai-muted mt-1">{{ formatDateTime(item.receivedAt) }}</p></td><td><p class="font-semibold break-words">{{ sourceName(item) }}</p><p class="ai-muted mt-1">{{ sourceType(item) }}</p></td><td>{{ metricNames(item.metricKeysJson) || 'Chưa xác định' }}</td><td class="text-center"><span class="ai-badge" :class="badge(item)">{{ item.processing?'Đang xử lý':statusLabel(item.status) }}</span></td><td class="text-center"><button type="button" class="ui-btn ui-btn-secondary" @click="openDetails(item)">Chi tiết</button></td></tr></tbody></table></div>
    <div class="ai-compact-list"><article v-for="(item,index) in rows" :key="item.id" class="ai-record"><p class="ai-muted mb-2">STT {{ (page-1)*pageSize+index+1 }}</p><p class="ai-input-code">{{ inputCode(item) }}</p><p class="ai-record-title">{{ item.title }}</p><p class="ai-muted mt-1">{{ formatDateTime(item.receivedAt) }}</p><p class="mt-2"><strong>Nguồn: {{ sourceName(item) }}</strong><span class="ai-muted"> · {{ sourceType(item) }}</span></p><p class="mt-3">{{ metricNames(item.metricKeysJson) || 'Chưa xác định chỉ số' }}</p><div class="flex justify-between items-center gap-3 mt-4"><span class="ai-badge" :class="badge(item)">{{ item.processing?'Đang xử lý':statusLabel(item.status) }}</span><button type="button" class="ui-btn ui-btn-secondary" @click="openDetails(item)">Chi tiết</button></div></article></div>
    <div v-if="!rows.length" class="ai-empty"><strong>{{ statusFilter?'Không có kết quả phù hợp':mode==='uploads'?'Chưa có tài liệu':'Chưa có bài viết' }}</strong><p>Tải tài liệu hoặc thêm nguồn RSS để AI đề xuất số liệu.</p><button v-if="statusFilter" type="button" class="ui-btn ui-btn-secondary mt-2" @click="statusFilter=''">Xóa bộ lọc</button><ActionButton v-else-if="mode!=='alerts'" kind="create" class="mt-2" @click="openUpload">Nạp tài liệu</ActionButton></div>
   </div>
   <AiListPagination v-model:page="page" v-model:page-size="pageSize" :total="total" :disabled="busy||loading"/>
  </div>
  <div v-if="uploadOpen" v-accessible-dialog="closeUpload" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3" @click.self="closeUpload"><form class="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90dvh] flex flex-col overflow-hidden" @submit.prevent="upload"><header class="flex justify-between items-center gap-4 p-4 sm:p-6 border-b border-slate-200"><h3 class="ui-title">Nạp tài liệu</h3></header><div class="p-4 sm:p-6 overflow-y-auto min-h-0 space-y-5"><p v-if="error" role="alert" class="rounded-xl bg-rose-50 text-rose-800 p-3">{{ error }}</p><SearchableSelect v-model="metric" :options="metricOptions" label="Mục tiêu cần bóc tách" :is-multi="false" :clearable="false" :disabled="busy"/><div class="ai-upload-file"><label for="ai-document" class="block font-semibold mb-2">Tài liệu</label><input id="ai-document" class="document-input block w-full max-w-full rounded-lg border border-slate-300 file:mr-3 file:rounded-md file:border-0 file:bg-blue-50 file:px-3 file:text-blue-800" aria-describedby="ai-upload-help" type="file" accept=".pdf,.docx,.txt" :disabled="busy" required @change="documentFile=$event.target.files[0]"/><p id="ai-upload-help" class="ai-muted mt-2">PDF có chữ, DOCX hoặc TXT UTF-8 · Tối đa 12 MB</p></div><p v-if="!configured" class="text-amber-900 rounded-xl bg-amber-50 p-3">Tài liệu sẽ chờ xử lý đến khi cấu hình AI sẵn sàng.</p></div><footer class="p-4 sm:px-6 bg-slate-50 border-t border-slate-200 flex justify-end gap-2"><button type="button" class="ui-btn ui-btn-secondary" :disabled="busy" @click="closeUpload">Đóng</button><button type="submit" class="ui-btn ui-btn-primary" :disabled="busy||!documentFile">{{ busy?'Đang nạp…':'Nạp tài liệu để AI đọc' }}</button></footer></form></div>
  <div v-if="selectedItem" v-accessible-dialog="closeDetails" class="fixed inset-0 z-50 bg-slate-900/60 flex items-center justify-center p-3 sm:p-4 lg:p-6" @click.self="closeDetails">
    <section class="bg-white rounded-2xl shadow-2xl w-full max-w-4xl lg:max-w-5xl max-h-[92dvh] flex flex-col overflow-hidden border border-slate-200">
      <!-- Modal Header -->
      <header class="flex justify-between items-start gap-4 p-5 sm:p-6 border-b border-slate-200 bg-slate-50/50">
        <div class="min-w-0 flex-1">
          <div class="flex items-center gap-2 flex-wrap mb-1.5">
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-md text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200 font-mono">
              {{ inputCode(selectedItem) }}
            </span>
            <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-600">
              {{ selectedItem.isUpload ? 'Nạp tĩnh · Tài liệu' : 'Nạp động · ' + sourceType(selectedItem) }}
            </span>
          </div>
          <h3 class="ui-title text-xl sm:text-2xl font-bold text-slate-900 truncate">
            {{ selectedItem.isUpload ? 'Chi tiết tài liệu' : 'Chi tiết bài viết' }}
          </h3>
          <p class="mt-1 text-sm text-slate-600 font-medium break-words flex items-center gap-1.5">
            <svg class="w-4 h-4 text-slate-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"/>
            </svg>
            {{ selectedItem.title }}
          </p>
        </div>
        <button type="button" class="text-slate-400 hover:text-slate-700 p-2 rounded-lg hover:bg-slate-200/60 transition shrink-0" aria-label="Đóng" @click="closeDetails">
          <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
        </button>
      </header>

      <!-- Modal Body -->
      <div class="overflow-y-auto min-h-0 p-5 sm:p-6 space-y-6">
        <p v-if="message" role="status" class="rounded-xl bg-blue-50 text-blue-900 p-4 border border-blue-200 flex items-center gap-2">
          <svg class="w-5 h-5 text-blue-600 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
          {{ message }}
        </p>

        <p v-if="selectedItem.status==='ReadQueued'" class="rounded-xl bg-amber-50 text-amber-900 p-4 border border-amber-200 flex items-center gap-2 ai-muted">
          <svg class="w-5 h-5 text-amber-600 animate-spin shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>
          Tài liệu đang chờ AI đọc. Trạng thái và kết quả được cập nhật tự động tại đây.
        </p>

        <p v-if="error" role="alert" class="rounded-xl bg-rose-50 p-4 text-rose-800 border border-rose-200 flex items-center gap-2">
          <svg class="w-5 h-5 text-rose-600 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/></svg>
          {{ error }}
        </p>

        <!-- General Info Grid -->
        <div class="rounded-xl bg-slate-50 border border-slate-200 p-4 sm:p-5">
          <dl class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div>
              <dt class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Trạng thái</dt>
              <dd class="mt-1.5 flex items-center">
                <span class="ai-badge text-sm" :class="badge(selectedItem)">
                  {{ selectedItem.processing ? 'Đang xử lý' : statusLabel(selectedItem.status) }}
                </span>
              </dd>
            </div>
            <div>
              <dt class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Thời gian nạp</dt>
              <dd class="mt-1.5 text-sm font-medium text-slate-800">
                {{ formatDateTime(selectedItem.receivedAt) }}
              </dd>
            </div>
            <div class="sm:col-span-2">
              <dt class="text-xs font-semibold text-slate-500 uppercase tracking-wider">{{ selectedItem.isUpload ? 'Mục tiêu' : 'Chỉ số' }}</dt>
              <dd class="mt-1.5 text-sm font-semibold text-blue-900 leading-snug">
                {{ metricNames(selectedItem.metricKeysJson) || 'Chưa xác định' }}
              </dd>
            </div>
          </dl>
        </div>

        <!-- Read Explanation Alert -->
        <div v-if="readExplanation(selectedItem)" class="p-4 rounded-xl space-y-1.5 border" :class="selectedItem.status==='ReadNoData' ? 'text-amber-900 bg-amber-50 border-amber-200' : 'text-rose-900 bg-rose-50 border-rose-200'">
          <p class="font-semibold flex items-center gap-2">
            <svg class="w-4 h-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
            {{ selectedItem.status==='ReadNoData' ? 'Nguyên nhân chưa có số liệu' : 'Lỗi xử lý' }}
          </p>
          <p class="text-sm leading-relaxed pl-6">{{ readExplanation(selectedItem) }}</p>
        </div>

        <p v-if="selectedItem.reason" class="bg-slate-50 border border-slate-200 rounded-xl p-4 text-sm text-slate-700">
          <span class="font-semibold text-slate-900">Đánh giá sơ bộ: </span>{{ selectedItem.reason }}
        </p>

        <!-- Extraction Results Section -->
        <section v-if="selectedItem.isUpload" class="border-t border-slate-200 pt-5 space-y-4">
          <div class="flex items-center justify-between flex-wrap gap-2">
            <h4 class="ui-section-title text-base sm:text-lg font-bold text-slate-900 flex items-center gap-2">
              <svg class="w-5 h-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01"/></svg>
              Kết quả bóc tách
            </h4>
            <span v-if="results.length" class="text-xs font-semibold px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 border border-emerald-200">
              {{ results.length }} số liệu trích xuất
            </span>
          </div>

          <LoadingSpinner v-if="resultsLoading" text="Đang tải kết quả..."/>
          <p v-else-if="resultsError" role="alert" class="p-4 bg-rose-50 text-rose-800 rounded-xl border border-rose-200">{{ resultsError }}</p>
          
          <template v-else>
            <div v-if="results.length" class="space-y-4">
              <article v-for="result in results" :key="result.id" class="rounded-2xl border-2 border-blue-100 bg-white shadow-sm overflow-hidden transition hover:border-blue-300">
                <!-- Result Card Header -->
                <div class="p-4 sm:p-5 bg-gradient-to-r from-blue-50/70 to-slate-50 border-b border-blue-100 flex items-center justify-between flex-wrap gap-3">
                  <div class="min-w-0 flex-1">
                    <p class="font-bold text-slate-900 sm:text-base leading-snug">
                      {{ metricNames(JSON.stringify([result.metricKey])) }}
                    </p>
                  </div>
                  <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold"
                    :class="result.status==='Approved' ? 'bg-emerald-100 text-emerald-800 border border-emerald-200' : result.status==='Rejected' ? 'bg-rose-100 text-rose-800 border border-rose-200' : 'bg-amber-100 text-amber-800 border border-amber-200'">
                    {{ result.status==='Approved' ? 'Đã ghi số liệu' : result.status==='Rejected' ? 'Đã bỏ qua' : 'Chưa ghi số liệu' }}
                  </span>
                </div>

                <!-- Result Card Body -->
                <div class="p-4 sm:p-5 space-y-4">
                  <!-- Metrics highlight grid -->
                  <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
                    <div class="p-3 rounded-xl bg-slate-50 border border-slate-100">
                      <span class="block text-xs font-semibold text-slate-500 uppercase">Kỳ báo cáo</span>
                      <span class="block mt-1 text-base font-bold text-slate-800">Năm {{ result.dataYear }}</span>
                    </div>
                    <div class="p-3 rounded-xl bg-blue-50/60 border border-blue-100 sm:col-span-1">
                      <span class="block text-xs font-semibold text-blue-700 uppercase">Số liệu thực tế</span>
                      <div class="block mt-1 text-xl sm:text-2xl font-extrabold text-blue-700">
                        {{ formatNumber(result.acceptedValue ?? result.value) }}
                        <span class="text-xs sm:text-sm font-semibold text-blue-900">{{ result.unit==='rank' ? 'hạng' : result.unit }}</span>
                      </div>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 border border-slate-100">
                      <span class="block text-xs font-semibold text-slate-500 uppercase">Phạm vi</span>
                      <span class="block mt-1 text-sm font-medium text-slate-800">{{ result.scope || 'Toàn quốc' }}</span>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 border border-slate-100">
                      <span class="block text-xs font-semibold text-slate-500 uppercase">Vị trí tài liệu</span>
                      <span class="block mt-1 text-sm font-bold text-slate-800">Trang / đoạn {{ result.page }}</span>
                    </div>
                  </div>

                  <!-- Quote Evidence -->
                  <div>
                    <label class="block text-xs font-bold text-slate-600 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
                      <svg class="w-4 h-4 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"/></svg>
                      Đoạn trích dẫn chứng nguyên văn (Trang {{ result.page }}):
                    </label>
                    <blockquote class="bg-slate-50/90 border border-slate-200 rounded-xl p-3.5 text-xs sm:text-sm font-mono text-slate-800 leading-relaxed break-words max-h-48 overflow-y-auto select-all">
                      {{ result.quote }}
                    </blockquote>
                  </div>

                  <!-- Action CTA -->
                  <div class="pt-2 flex justify-end">
                    <button type="button" class="ui-btn ui-btn-primary flex items-center gap-2" @click="confirmResult(result)">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                      {{ result.status==='Pending' ? 'Xác nhận số liệu' : 'Xem chi tiết kết quả' }}
                    </button>
                  </div>
                </div>
              </article>
            </div>

            <p v-else class="ai-muted p-6 text-center bg-slate-50 rounded-xl border border-dashed border-slate-200">
              {{ isQueued(selectedItem) ? 'Kết quả sẽ hiển thị tại đây sau khi AI đọc xong.' : selectedItem.status==='Error' ? 'Chưa có kết quả vì lượt đọc bị lỗi. Xem nguyên nhân ở trên.' : 'Chưa tìm thấy số liệu phù hợp trong tài liệu.' }}
            </p>
          </template>
        </section>

        <!-- Document source attachment / details -->
        <div class="border-t border-slate-200 pt-4 flex items-center justify-between flex-wrap gap-3">
          <div class="flex items-center gap-2 min-w-0">
            <span v-if="selectedItem.origin" class="ai-muted text-xs bg-slate-100 px-2.5 py-1 rounded-md">{{ selectedItem.origin }}</span>
            <details v-if="selectedItem.model" class="ai-muted text-xs inline-block">
              <summary class="cursor-pointer hover:text-slate-900">Thông tin xử lý AI</summary>
              <p class="mt-1 font-mono text-xs text-slate-700">{{ selectedItem.model }}</p>
            </details>
          </div>
          <div>
            <button v-if="selectedItem.isUpload" type="button" class="ui-btn ui-btn-secondary flex items-center gap-1.5 text-xs sm:text-sm" :disabled="busy" @click="download(selectedItem)">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
              Tải file gốc
            </button>
            <a v-else-if="selectedItem.url" :href="selectedItem.url" target="_blank" rel="noopener noreferrer" class="text-blue-700 underline break-all text-sm flex items-center gap-1">
              {{ selectedItem.url }}
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/></svg>
            </a>
          </div>
        </div>

        <div v-if="reread" class="border-t border-slate-200 pt-4">
          <SearchableSelect v-model="choices[selectedItem.id]" :options="metricOptions" label="Mục tiêu đọc lại" :is-multi="false" :clearable="false" :disabled="busy||isQueued(selectedItem)"/>
        </div>
      </div>

      <!-- Modal Footer -->
      <footer class="p-4 sm:px-6 bg-slate-50 border-t border-slate-200 flex flex-wrap justify-end gap-2">
        <button type="button" class="ui-btn ui-btn-secondary shrink-0" :disabled="busy" @click="closeDetails">Đóng</button>
        <button v-if="!selectedItem.isUpload" type="button" class="ui-btn ui-btn-secondary" :disabled="busy||isQueued(selectedItem)" @click="queue(selectedItem,true)">Đánh giá lại tiêu đề</button>
        <button v-if="!reread" type="button" class="ui-btn ui-btn-secondary" :disabled="busy||isQueued(selectedItem)" @click="reread=true">Đọc lại</button>
        <button v-if="reread" type="button" class="ui-btn ui-btn-primary" :disabled="busy||isQueued(selectedItem)||!choices[selectedItem.id]" @click="queue(selectedItem,false)">Bắt đầu đọc lại</button>
        <button v-else-if="selectedItem.status==='Read'&&!selectedItem.isUpload" type="button" class="ui-btn ui-btn-primary" @click="viewSuggestions">Xem kết quả và dẫn chứng</button>
      </footer>
    </section>
  </div>
 </section>
</template>
<script setup>
import AiListPagination from './AiListPagination.vue';

import {ref,computed,watch,onMounted,onUnmounted} from 'vue';
import {fetchWithAuth} from '../services/auth';
import {getApiUrl} from '../config/api';
import {aiInputCode} from '../shared/aiInputCode';
import {formatNumber,formatDateTime} from '../shared/formatters';
import {getStatusBadgeClass} from '../shared/statusPresentation';
import {aiFeedType} from '../shared/aiFeedSource';
import '../features/ai/ai.css';
import SearchableSelect from './SearchableSelect.vue';
import ActionButton from './ActionButton.vue';
import LoadingSpinner from './LoadingSpinner.vue';
const emit=defineEmits(['view-suggestions','view-result']);
const selected=ref(null),reread=ref(false),uploadOpen=ref(false),results=ref([]),resultsLoading=ref(false),resultsError=ref('');
function openUpload(){if(!busy.value){documentFile.value=null;uploadOpen.value=true;error.value='';}}
function closeUpload(){if(!busy.value)uploadOpen.value=false;}
defineExpose({openUpload,refresh:load});
const selectedItem=computed(()=>selected.value?(rows.value.find(item=>item.id===selected.value.id)||selected.value):null);
function openDetails(item){results.value=[];resultsError.value='';if(item.isUpload)loadResults(item);selected.value=item;reread.value=false;try{choices.value[item.id]=JSON.parse(item.metricKeysJson)[0]||props.metrics[0]?.key;}catch{choices.value[item.id]=props.metrics[0]?.key;}}
async function loadResults(item){resultsLoading.value=true;try{const res=await fetchWithAuth(getApiUrl('/api/ai-data/suggestions?inboxId='+item.id+'&pageSize=100'));if(!res.ok)throw new Error('Không tải được kết quả bóc tách.');const data=await res.json();if(selected.value?.id===item.id)results.value=Array.isArray(data.items)?data.items:[];}catch(e){resultsError.value=e.message;}finally{resultsLoading.value=false;}}
function confirmResult(result){selected.value=null;emit('view-result',result);}
function closeDetails(){if(!busy.value)selected.value=null;}
function viewSuggestions(){const item=selectedItem.value;selected.value=null;emit('view-suggestions',item);}
const props=defineProps({metrics:{type:Array,default:()=>[]},configured:Boolean,mode:String});
const pageSize=ref(5);
const rows=ref([]),total=ref(0),page=ref(1),loading=ref(true),busy=ref(false),error=ref(''),message=ref(''),metric=ref(props.metrics[0]?.key||'EGDI_RANK'),documentFile=ref(null),choices=ref({});
const inputCode=item=>aiInputCode(item.sequenceNumber,item.receivedAt,item.isUpload)||'Chưa cấp mã';
const statusFilter=ref(''),sourceFilter=ref('');
const sourceOptions=[{value:'',label:'Tất cả nguồn'},{value:'uploads',label:'Nạp tĩnh — Tài liệu'},{value:'alerts',label:'Nạp động — Nguồn RSS'}];
function sourceType(item){
 return item.isUpload?'Tài liệu tải lên':aiFeedType(item.feedUrl,item.origin||'');
}
const sourceName=item=>item.isUpload?'Tài liệu tải lên':item.feedName||'Nguồn RSS không còn tên';
const alertStatusOptions=[{value:'',label:'Tất cả'},{value:'TitleQueued',label:'Chờ đánh giá'},{value:'NeedsReview',label:'Cần xem lại'},{value:'NotRelevant',label:'Không liên quan'},{value:'ReadQueued',label:'Chờ đọc'},{value:'Read',label:'Đã đọc'},{value:'ReadNoData',label:'Chưa có số liệu'},{value:'Error',label:'Lỗi xử lý'}];
watch(page,()=>load());watch(pageSize,()=>{page.value=1;load();});
watch([statusFilter,sourceFilter],()=>{page.value=1;load();});
const metricOptions=computed(()=>props.metrics.map(m=>({value:m.key,label:m.name})));
const visibleStatusOptions=computed(()=>props.mode==='uploads'?alertStatusOptions.filter(o=>!['TitleQueued','NeedsReview','NotRelevant'].includes(o.value)):alertStatusOptions);
function readExplanation(item){
 if(item.status==='ReadNoData')return item.processingDetails||(item.error&&!item.error.includes('Xem Lịch sử xử lý')?item.error:'AI không tìm thấy số liệu thực tế có đủ dẫn chứng phù hợp với mục tiêu đã chọn trong các phần tài liệu được đọc. Không có số liệu nào được ghi vào mục tiêu.');
 return item.error||'';
}
const isQueued=item=>item.processing||['TitleQueued','ReadQueued'].includes(item.status);
const badge=item=>getStatusBadgeClass(item.processing?'InProgressOnTime':(item.status==='Error'||item.status==='NotRelevant')?'Rejected':item.status==='Read'?'Approved':item.status==='NeedsReview'?'Pending':'NotStarted');
const statusLabel=s=>({TitleQueued:'Chờ đánh giá',ReadQueued:'Chờ đọc',NeedsReview:'Cần xem lại',NotRelevant:'Không liên quan',Read:'Đã đọc',ReadNoData:'Chưa có số liệu',Error:'Lỗi xử lý'}[s]||s);
function metricNames(json){try{return JSON.parse(json).map(key=>props.metrics.find(m=>m.key===key)?.name||key).join(', ');}catch{return '';}}
async function api(path='',options){const res=await fetchWithAuth(getApiUrl('/api/ai-inbox'+path),options);if(!res.ok){const d=await res.json().catch(()=>({}));throw new Error(d.message||`HTTP ${res.status}`);}return res.json();}
let version=0,timer;
async function load(){const v=++version;if(!rows.value.length)loading.value=true;try{const d=await api('?page='+page.value+'&pageSize='+pageSize.value+'&uploadsOnly='+(props.mode==='uploads'||sourceFilter.value==='uploads')+'&alertsOnly='+(props.mode==='alerts'||sourceFilter.value==='alerts')+'&status='+encodeURIComponent(statusFilter.value));if(v===version){rows.value=d.items;total.value=d.total;if(selectedItem.value?.isUpload)loadResults(selectedItem.value);}}catch(e){if(v===version)error.value=e.message;}finally{if(v===version)loading.value=false;}}
async function act(fn){if(busy.value)return;busy.value=true;error.value='';message.value='';try{await fn();await load();}catch(e){error.value=e.message;}finally{busy.value=false;}}
function upload(){act(async()=>{if(!documentFile.value||documentFile.value.size>12*1024*1024)throw new Error('Chọn tài liệu tối đa 12 MB.');const form=new FormData();form.append('file',documentFile.value);form.append('metricKey',metric.value);const d=await api('/upload',{method:'POST',body:form});message.value=d.message;uploadOpen.value=false;documentFile.value=null;page.value=1;sourceFilter.value='';statusFilter.value='';await load();const received=d.item||rows.value.find(item=>item.id===d.id);if(received)openDetails(received);});}
function queue(item,titleOnly){if(isQueued(item))return;act(async()=>{const d=await api('/'+item.id+'/queue',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({titleOnly,metricKey:choices.value[item.id]||''})});message.value=d.message;selected.value=null;reread.value=false;});}
async function download(item){await act(async()=>{const res=await fetchWithAuth(getApiUrl('/api/ai-inbox/'+item.id+'/file'));if(!res.ok)throw new Error('Không tải được file.');const url=URL.createObjectURL(await res.blob());const a=document.createElement('a');a.href=url;a.download=item.title;a.click();setTimeout(()=>URL.revokeObjectURL(url),10000);});}
onMounted(()=>{load();timer=setInterval(()=>{if(!busy.value&&!loading.value&&document.visibilityState==='visible')load();},30000);});onUnmounted(()=>{version++;clearInterval(timer);});
</script>
<style scoped>
.ai-upload-file{padding:16px;border:1px dashed var(--color-slate-300);border-radius:12px;background:var(--color-slate-50)}
.document-input{background:white;height:var(--ui-control-height);padding:0 8px}.document-input::file-selector-button{height:calc(var(--ui-control-height) - 2px);padding-top:0;padding-bottom:0}
</style>
