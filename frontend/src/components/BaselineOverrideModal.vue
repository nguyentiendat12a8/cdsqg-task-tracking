<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-xl w-full p-6 space-y-5 animate-in fade-in duration-150 max-h-[90vh] overflow-y-auto font-sans">
      
      <!-- Modal Header -->
      <div class="flex justify-between items-start border-b border-slate-100 pb-3">
        <div>
          <span class="text-xs font-bold text-amber-600 uppercase tracking-wider block">Thiết Lập Mốc Tiến Độ Chi Tiết Theo Quý / Tháng</span>
          <h3 class="text-lg font-bold text-slate-800 mt-0.5">{{ taskCode }} - {{ taskTitle }}</h3>
        </div>
        <button @click="close" class="text-slate-400 hover:text-slate-600 text-xl font-bold p-1">✕</button>
      </div>

      <!-- Year Selector & Annual Target Display -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-amber-50/80 p-3.5 rounded-xl border border-amber-200/80">
        <div class="flex items-center gap-2">
          <label class="text-xs font-bold text-amber-950 uppercase shrink-0">Năm Cấu Hình:</label>
          <select v-model="selectedYear" class="bg-white border border-amber-300 text-amber-900 font-bold text-xs rounded-lg px-3 py-1.5 focus:outline-none shadow-sm">
            <option v-for="y in dynamicYears" :key="y" :value="y">Năm {{ y }}</option>
          </select>
        </div>

        <div class="text-xs font-bold text-amber-900 bg-amber-100/90 px-3 py-1 rounded-lg border border-amber-200 shrink-0">
          Chỉ tiêu cả năm {{ selectedYear }}: 
          <span class="text-purple-900 font-black">{{ yearlyTargets[selectedYear] ?? 'Chưa đặt' }}</span>
          <span v-if="isQuant" class="text-[10px] text-slate-500 font-bold ml-1">({{ unitName || '%' }})</span>
        </div>
      </div>

      <!-- Frequency Selection Checkboxes -->
      <div class="space-y-2 bg-slate-50 p-3.5 rounded-xl border border-slate-200">
        <label class="text-xs font-extrabold text-slate-700 uppercase block">Tick chọn tần suất thiết lập mốc chỉ tiêu bổ sung:</label>
        <div class="flex flex-wrap gap-4 pt-1">
          <label class="flex items-center gap-2 cursor-pointer text-xs font-extrabold text-slate-800 hover:text-amber-700">
            <input type="checkbox" v-model="hasQuarter" class="rounded text-amber-600 focus:ring-amber-500 w-4 h-4 cursor-pointer" />
            <span>📅 Thiết Lập Chỉ Tiêu Theo Quý (4 Quý)</span>
          </label>

          <label class="flex items-center gap-2 cursor-pointer text-xs font-extrabold text-slate-800 hover:text-amber-700">
            <input type="checkbox" v-model="hasMonth" class="rounded text-amber-600 focus:ring-amber-500 w-4 h-4 cursor-pointer" />
            <span>📆 Thiết Lập Chỉ Tiêu Theo Tháng (12 Tháng)</span>
          </label>
        </div>
      </div>

      <!-- Sub-tabs for Quarters vs Months -->
      <div v-if="hasQuarter && hasMonth" class="flex items-center gap-2 bg-slate-100 p-1 rounded-xl border border-slate-200">
        <button 
          @click="activeViewTab = 'quarterly'" 
          :class="['flex-1 py-1.5 rounded-lg text-xs font-extrabold transition text-center', activeViewTab === 'quarterly' ? 'bg-white text-amber-700 shadow-sm' : 'text-slate-600 hover:text-slate-900']"
        >
          📅 Mốc Quý (Q1-Q4)
        </button>

        <button 
          @click="activeViewTab = 'monthly'" 
          :class="['flex-1 py-1.5 rounded-lg text-xs font-extrabold transition text-center', activeViewTab === 'monthly' ? 'bg-white text-amber-700 shadow-sm' : 'text-slate-600 hover:text-slate-900']"
        >
          📆 Mốc Tháng (T1-T12)
        </button>
      </div>

      <!-- QUARTERLY MILESTONE INPUT GRID -->
      <div v-if="hasQuarter && (!hasMonth || activeViewTab === 'quarterly')" class="space-y-2">
        <div class="text-xs font-extrabold text-amber-900 flex items-center justify-between">
          <span>📌 Chỉ Tiêu 4 Quý Năm {{ selectedYear }}</span>
          <span class="text-[11px] font-normal text-slate-500">Loại: {{ isQuant ? `Định lượng (${unitName || '%'})` : 'Định tính văn bản' }}</span>
        </div>

        <div class="grid grid-cols-2 gap-3 bg-slate-50 p-4 rounded-xl border border-slate-200">
          <div v-for="q in [1, 2, 3, 4]" :key="q" class="space-y-1">
            <label class="text-xs font-bold text-slate-700">Quý {{ q }} / {{ selectedYear }} ({{ getQuarterKey(q) }})</label>
            
            <!-- Quantitative Number Input -->
            <div v-if="isQuant" class="relative">
              <input 
                type="number" 
                step="0.1" 
                :value="getMilestoneVal(getQuarterKey(q))" 
                @input="setMilestoneVal(getQuarterKey(q), $event.target.value)"
                placeholder="—"
                class="w-full text-xs font-bold text-slate-800 bg-white border border-slate-300 rounded-lg px-3 py-2 pr-8 focus:ring-2 focus:ring-amber-500 focus:outline-none"
              />
              <span class="absolute right-2.5 top-2 text-xs font-bold text-slate-400">{{ unitName || '%' }}</span>
            </div>

            <!-- Qualitative Status Select -->
            <div v-else>
              <select 
                :value="getMilestoneVal(getQuarterKey(q)) || 'NotStarted'" 
                @change="setMilestoneVal(getQuarterKey(q), $event.target.value)"
                class="w-full text-xs font-bold bg-white border border-slate-300 rounded-lg px-2.5 py-2 focus:ring-2 focus:ring-amber-500"
              >
                <option value="NotStarted">Chưa thực hiện</option>
                <option value="Drafting">Đang soạn thảo</option>
                <option value="Reviewing">Đang xin ý kiến</option>
                <option value="Completed">Hoàn thành</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <!-- MONTHLY MILESTONE INPUT GRID (12 Months) -->
      <div v-if="hasMonth && (!hasQuarter || activeViewTab === 'monthly')" class="space-y-2">
        <div class="text-xs font-extrabold text-amber-900 flex items-center justify-between">
          <span>📌 Chỉ Tiêu 12 Tháng Năm {{ selectedYear }}</span>
          <span class="text-[11px] font-normal text-slate-500">Loại: {{ isQuant ? `Định lượng (${unitName || '%'})` : 'Định tính văn bản' }}</span>
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-3 gap-3 bg-slate-50 p-4 rounded-xl border border-slate-200 max-h-64 overflow-y-auto">
          <div v-for="m in 12" :key="m" class="space-y-1">
            <label class="text-[11px] font-bold text-slate-700">Tháng {{ m }} (T{{ m }})</label>

            <!-- Quantitative Number Input -->
            <div v-if="isQuant" class="relative">
              <input 
                type="number" 
                step="0.1" 
                :value="getMilestoneVal(getMonthKey(m))" 
                @input="setMilestoneVal(getMonthKey(m), $event.target.value)"
                placeholder="—"
                class="w-full text-xs font-bold text-slate-800 bg-white border border-slate-300 rounded-lg px-2 py-1.5 pr-7 focus:ring-2 focus:ring-amber-500 focus:outline-none"
              />
              <span class="absolute right-1.5 top-1.5 text-[10px] font-bold text-slate-400">{{ unitName || '%' }}</span>
            </div>

            <!-- Qualitative Status Select -->
            <div v-else>
              <select 
                :value="getMilestoneVal(getMonthKey(m)) || 'NotStarted'" 
                @change="setMilestoneVal(getMonthKey(m), $event.target.value)"
                class="w-full text-[11px] font-bold bg-white border border-slate-300 rounded-lg px-1.5 py-1 focus:ring-2 focus:ring-amber-500"
              >
                <option value="NotStarted">Chưa làm</option>
                <option value="Drafting">Soạn thảo</option>
                <option value="Reviewing">Xin ý kiến</option>
                <option value="Completed">Hoàn thành</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!hasQuarter && !hasMonth" class="p-6 text-center text-xs text-slate-400 font-semibold bg-slate-50 rounded-xl border border-slate-200 italic">
        Vui lòng tick chọn ít nhất 1 tần suất (Theo Quý hoặc Theo Tháng) ở trên để thiết lập mốc chỉ tiêu. Mặc định hệ thống sẽ áp dụng chỉ tiêu Báo cáo Theo Năm.
      </div>

      <!-- Footer Actions -->
      <div class="flex justify-end gap-3 border-t border-slate-100 pt-3">
        <button @click="close" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition">Hủy bỏ</button>
        <button 
          @click="saveCustomBaseline" 
          :disabled="isSaving"
          class="px-5 py-2 text-xs font-bold text-white bg-amber-600 hover:bg-amber-700 disabled:opacity-50 rounded-xl shadow-sm transition flex items-center gap-1.5"
        >
          <span v-if="isSaving" class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          {{ isSaving ? 'Đang lưu...' : 'Lưu Custom Baseline' }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { toast } from 'vue3-toastify';

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  taskId: { type: String, required: true },
  taskCode: { type: String, default: '' },
  taskTitle: { type: String, default: '' },
  evaluationType: { type: String, default: 'Quantitative' },
  unitName: { type: String, default: '%' },
  dynamicYears: { type: Array, default: () => [2026, 2027, 2028, 2029, 2030] },
  existingCustomBaseline: { type: Object, default: () => ({}) },
  yearlyTargets: { type: Object, default: () => ({}) }
});

const emit = defineEmits(['close', 'saved']);

const selectedYear = ref(2026);
const hasQuarter = ref(false);
const hasMonth = ref(false);
const activeViewTab = ref('quarterly'); // 'quarterly' | 'monthly'

const milestones = ref({});
const isSaving = ref(false);

const isQuant = computed(() => {
  return props.evaluationType === 'Quantitative' || props.evaluationType === 1 || props.evaluationType === '1';
});

watch(() => props.existingCustomBaseline, (newVal) => {
  const base = { ...(newVal || {}) };
  milestones.value = base;
  
  // Detect flags or keys
  hasQuarter.value = base.hasQuarter === 'true' || base.hasQuarter === true || Object.keys(base).some(k => k.startsWith('Q'));
  hasMonth.value = base.hasMonth === 'true' || base.hasMonth === true || Object.keys(base).some(k => k.startsWith('M'));
}, { immediate: true });

function getQuarterKey(quarter) {
  return `Q${quarter}_${selectedYear.value}`;
}

function getMonthKey(month) {
  return `M${month}_${selectedYear.value}`;
}

function getMilestoneVal(key) {
  return milestones.value[key] !== undefined ? milestones.value[key] : '';
}

function setMilestoneVal(key, val) {
  if (val !== '' && val !== null && val !== undefined) {
    milestones.value[key] = String(val);
  } else {
    delete milestones.value[key];
  }
}

function close() {
  emit('close');
}

async function saveCustomBaseline() {
  isSaving.value = true;
  try {
    const payload = { ...milestones.value };
    payload.hasQuarter = hasQuarter.value ? 'true' : 'false';
    payload.hasMonth = hasMonth.value ? 'true' : 'false';

    const response = await fetch(`http://localhost:5000/api/planning/tasks/${props.taskId}/custom-baseline`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ milestones: payload })
    });

    if (response.ok) {
      toast.success(`Đã lưu cấu hình Custom Baseline cho ${props.taskCode} thành công!`);
      emit('saved', payload);
      close();
    } else {
      const err = await response.json();
      toast.error('Lỗi khi lưu Custom Baseline: ' + (err.error || err.message || 'Thất bại'));
    }
  } catch (error) {
    toast.error('Không thể kết nối Server API: ' + error.message);
  } finally {
    isSaving.value = false;
  }
}
</script>
