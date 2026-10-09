<template>
  <div v-if="isOpen" v-accessible-dialog class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-3 sm:p-6" @click.self="close">
    <section role="dialog" aria-modal="true" aria-labelledby="annual-baseline-title" class="flex w-full max-w-2xl flex-col overflow-hidden rounded-2xl bg-white shadow-2xl max-h-[90vh]">
      <header class="flex items-start justify-between gap-4 border-b border-slate-100 px-5 py-5 sm:px-7">
        <div class="min-w-0">
          <span class="mb-2 inline-flex rounded-md bg-blue-50 px-2 py-1 text-xs font-semibold text-blue-700">Thiết lập · {{ taskCode }}</span>
          <h2 id="annual-baseline-title" class="text-xl font-bold tracking-tight text-slate-900">Chỉ tiêu theo năm</h2>
          <p v-if="taskTitle" class="mt-1.5 text-sm leading-5 text-slate-500 break-words">{{ taskTitle }}</p>
        </div>
        <button type="button" aria-label="Đóng" :disabled="isSaving" @click="close" class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-slate-400 transition hover:bg-slate-100 hover:text-slate-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 disabled:opacity-50">✕</button>
      </header>
      <form @submit.prevent="saveCustomBaseline" class="flex min-h-0 flex-1 flex-col">
        <div class="overflow-y-auto px-5 py-5 sm:px-7 space-y-5">
          <div class="rounded-xl border border-blue-100 bg-blue-50/60 px-4 py-3">
            <p class="text-sm font-semibold text-blue-950">Mốc tổng cần đạt đến từng năm</p>
            <p class="mt-1 text-xs leading-5 text-blue-800">Chỉ tiêu dùng để tính tỷ lệ và xác định hoàn thành. Để trống ô tùy chỉnh để dùng chỉ tiêu mặc định của năm.</p>
          </div>
          <div v-if="isQuant" class="overflow-hidden rounded-xl border border-slate-200">
            <div class="hidden sm:grid grid-cols-[100px_1fr_1fr] gap-4 bg-slate-50 px-4 py-3 text-xs font-semibold text-slate-500">
              <span>Năm báo cáo</span><span>Chỉ tiêu mặc định</span><span>Chỉ tiêu tùy chỉnh</span>
            </div>
            <div v-for="year in dynamicYears" :key="year" class="grid grid-cols-[1fr_1fr] sm:grid-cols-[100px_1fr_1fr] gap-x-4 gap-y-2 items-center border-t border-slate-100 first:border-t-0 px-4 py-3.5">
              <label :for="`annual-target-${year}`" class="text-sm font-semibold text-slate-800">Năm {{ year }} <span class="sr-only">({{ unitName }})</span></label>
              <div class="text-right sm:text-left text-sm text-slate-500">{{ yearlyTargets[year] == null ? 'Chưa thiết lập' : yearlyTargets[year] }} <span v-if="yearlyTargets[year] != null" class="text-xs">{{ unitName }}</span></div>
              <div class="relative col-span-2 sm:col-span-1">
                <input :id="`annual-target-${year}`" v-model="milestones[year]" type="number" min="0.000001" step="any" placeholder="Dùng mặc định" class="ui-single-line ui-control w-full rounded-lg border border-slate-200 bg-white py-2.5 pl-3 pr-14 text-sm font-medium text-slate-900 placeholder:font-normal placeholder:text-slate-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100 transition" />
                <span class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 max-w-10 truncate text-xs text-slate-400" :title="unitName">{{ unitName }}</span>
              </div>
            </div>
          </div>
          <p v-else class="text-sm leading-6 text-slate-600">Mục định tính hoàn thành khi tất cả sản phẩm đầu ra đã hoàn thành hoặc trạng thái báo cáo là Đã hoàn thành nếu không có sản phẩm.</p>
          <p v-if="error" role="alert" class="rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-700">{{ error }}</p>
        </div>
        <footer class="flex shrink-0 justify-end gap-3 border-t border-slate-100 bg-slate-50/70 px-5 py-4 sm:px-7">
          <button type="button" :disabled="isSaving" @click="close" class="ui-single-line ui-button rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-50 focus-visible:ring-2 focus-visible:ring-blue-500 disabled:opacity-50">Đóng</button>
          <button v-if="isQuant" type="submit" :disabled="isSaving" class="ui-single-line ui-button rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:opacity-50">{{ isSaving ? 'Đang lưu…' : 'Lưu chỉ tiêu năm' }}</button>
        </footer>
      </form>
    </section>
  </div>
</template>
<script setup>
import { REPORTING_YEARS } from '../config/reporting';
import { computed, ref, watch } from 'vue';
import { toast } from 'vue3-toastify';
import { fetchWithAuth } from '../services/auth';
import { getApiUrl } from '../config/api';
const props = defineProps({
  isOpen: Boolean, taskId: { type: String, required: true }, taskCode: { type: String, default: '' },
  taskTitle: { type: String, default: '' }, evaluationType: { type: String, default: 'Quantitative' },
  unitName: { type: String, default: '%' }, dynamicYears: { type: Array, default: () => [...REPORTING_YEARS] },
  existingCustomBaseline: { type: Object, default: () => ({}) }, yearlyTargets: { type: Object, default: () => ({}) }
});
const emit = defineEmits(['close', 'saved']);
const milestones = ref({});
const isSaving = ref(false);
const error = ref('');
const isQuant = computed(() => ['Quantitative', '1', 1].includes(props.evaluationType));
watch(() => [props.isOpen, props.existingCustomBaseline], () => {
  milestones.value = Object.fromEntries(Object.entries(props.existingCustomBaseline || {}).filter(([key]) => /^\d{4}$/.test(key)));
  error.value = '';
}, { immediate: true });
function close() { if (!isSaving.value) emit('close'); }
async function saveCustomBaseline() {
  const payload = Object.fromEntries(Object.entries(milestones.value).filter(([, value]) => value !== '' && value != null).map(([year, value]) => [year, String(value)]));
  if (Object.values(payload).some(value => !Number.isFinite(Number(value)) || Number(value) <= 0)) { error.value = 'Chỉ tiêu phải là số lớn hơn 0.'; return; }
  isSaving.value = true;
  error.value = '';
  try {
    const response = await fetchWithAuth(getApiUrl(`/api/planning/tasks/${props.taskId}/custom-baseline`), {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ milestones: payload })
    });
    if (!response.ok) { const data = await response.json(); throw new Error(data.error || data.message || 'Không thể lưu chỉ tiêu'); }
    toast.success('Đã lưu chỉ tiêu theo năm');
    emit('saved', payload);
    emit('close');
  } catch (e) { error.value = e.message; }
  finally { isSaving.value = false; }
}
</script>





