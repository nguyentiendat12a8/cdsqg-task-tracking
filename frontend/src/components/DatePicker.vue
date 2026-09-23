<template>
  <VDropdown 
    :triggers="['click']" 
    :auto-hide="true" 
    placement="bottom-start"
    :distance="6"
    v-model:shown="isOpen"
    popper-class="v-popper-clean-overlay"
  >
    <!-- Trigger Input Box -->
    <div 
      class="relative flex items-center w-full cursor-pointer select-none group"
      :class="{ 'opacity-60 pointer-events-none': disabled }"
    >
      <input
        type="text"
        readonly
        :value="formattedDisplay"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        class="w-full text-xs font-semibold bg-slate-50 group-hover:bg-white border border-slate-200 rounded-xl pl-3 pr-8 py-2 text-slate-800 transition shadow-2xs focus:bg-white focus:outline-none focus:ring-2 focus:ring-blue-500/80 focus:border-blue-500 cursor-pointer"
        :class="inputClass"
      />
      
      <!-- Actions / Icons on the right -->
      <div class="absolute right-2.5 flex items-center gap-1">
        <button 
          v-if="modelValue && !disabled && !readonly" 
          type="button" 
          @click.stop="clearDate" 
          class="w-4 h-4 rounded-full hover:bg-slate-200 text-slate-400 hover:text-slate-600 flex items-center justify-center text-[10px] font-bold transition cursor-pointer"
          title="Xóa ngày"
        >
          ✕
        </button>
        <svg class="w-4 h-4 text-slate-400 group-hover:text-blue-600 transition-colors pointer-events-none" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
        </svg>
      </div>
    </div>

    <!-- Calendar Popper Content -->
    <template #popper>
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 p-3.5 w-[280px] font-sans text-slate-800 z-50 select-none">
        
        <!-- Header Controls: Prev, Month/Year Selects, Next -->
        <div class="flex items-center justify-between gap-1 pb-3 mb-2 border-b border-slate-100">
          <button 
            type="button" 
            @click="prevMonth" 
            class="w-7 h-7 rounded-lg hover:bg-slate-100 text-slate-500 hover:text-blue-600 flex items-center justify-center font-bold text-sm transition cursor-pointer shrink-0"
            title="Tháng trước"
          >
            ‹
          </button>

          <div class="flex items-center gap-1">
            <!-- Month Select -->
            <select 
              v-model="selectedMonth" 
              class="text-xs font-bold text-slate-800 bg-slate-100 hover:bg-slate-200 border-none rounded-lg px-2 py-1 cursor-pointer focus:ring-2 focus:ring-blue-500 outline-none"
            >
              <option v-for="(mName, idx) in monthNames" :key="idx" :value="idx">
                {{ mName }}
              </option>
            </select>

            <!-- Year Select -->
            <select 
              v-model="selectedYear" 
              class="text-xs font-bold text-slate-800 bg-slate-100 hover:bg-slate-200 border-none rounded-lg px-2 py-1 cursor-pointer focus:ring-2 focus:ring-blue-500 outline-none"
            >
              <option v-for="y in yearOptions" :key="y" :value="y">
                {{ y }}
              </option>
            </select>
          </div>

          <button 
            type="button" 
            @click="nextMonth" 
            class="w-7 h-7 rounded-lg hover:bg-slate-100 text-slate-500 hover:text-blue-600 flex items-center justify-center font-bold text-sm transition cursor-pointer shrink-0"
            title="Tháng sau"
          >
            ›
          </button>
        </div>

        <!-- Week Day Labels (T2, T3, T4, T5, T6, T7, CN) -->
        <div class="grid grid-cols-7 gap-1 text-center mb-1.5">
          <span 
            v-for="(wDay, idx) in weekDays" 
            :key="idx" 
            class="text-[10px] font-bold uppercase tracking-wider"
            :class="idx === 6 ? 'text-rose-500' : 'text-slate-400'"
          >
            {{ wDay }}
          </span>
        </div>

        <!-- Days Grid -->
        <div class="grid grid-cols-7 gap-1">
          <button
            v-for="(item, idx) in calendarDays"
            :key="idx"
            type="button"
            @click="selectDay(item)"
            class="h-7 w-7 rounded-lg text-xs font-bold flex items-center justify-center transition-all cursor-pointer relative"
            :class="[
              item.isSelected 
                ? 'bg-blue-600 text-white font-bold shadow-xs scale-105 z-10' 
                : item.isCurrentMonth 
                  ? 'text-slate-700 hover:bg-blue-50 hover:text-blue-600' 
                  : 'text-slate-300 hover:bg-slate-50',
              item.isToday && !item.isSelected ? 'border border-blue-500 text-blue-600 bg-blue-50/50' : ''
            ]"
          >
            {{ item.day }}
          </button>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-between border-t border-slate-100 pt-2.5 mt-2.5 text-[11px]">
          <button 
            type="button" 
            @click="clearDate" 
            class="text-rose-500 hover:text-rose-700 font-bold px-2 py-1 rounded-md hover:bg-rose-50 transition cursor-pointer"
          >
            Xóa
          </button>
          
          <button 
            type="button" 
            @click="selectToday" 
            class="text-blue-600 hover:text-blue-800 font-bold px-2.5 py-1 rounded-md hover:bg-blue-50 transition cursor-pointer flex items-center gap-1"
          >
            <span>Hôm nay</span>
          </button>
        </div>

      </div>
    </template>
  </VDropdown>
</template>

<script setup>
import { ref, computed, watch } from 'vue';

const props = defineProps({
  modelValue: { type: String, default: '' }, // YYYY-MM-DD
  placeholder: { type: String, default: 'dd/mm/yyyy' },
  disabled: { type: Boolean, default: false },
  readonly: { type: Boolean, default: false },
  required: { type: Boolean, default: false },
  inputClass: { type: String, default: '' }
});

const emit = defineEmits(['update:modelValue', 'change']);

const isOpen = ref(false);
const viewDate = ref(new Date());

const monthNames = ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'];
const weekDays = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

const yearOptions = computed(() => {
  const currentY = new Date().getFullYear();
  const years = [];
  for (let y = currentY - 10; y <= currentY + 15; y++) {
    years.push(y);
  }
  return years;
});

const selectedMonth = computed({
  get: () => viewDate.value.getMonth(),
  set: (val) => {
    viewDate.value = new Date(viewDate.value.getFullYear(), val, 1);
  }
});

const selectedYear = computed({
  get: () => viewDate.value.getFullYear(),
  set: (val) => {
    viewDate.value = new Date(val, viewDate.value.getMonth(), 1);
  }
});

watch(() => props.modelValue, (val) => {
  if (val) {
    const parts = val.split('T')[0].split('-');
    if (parts.length === 3) {
      const y = parseInt(parts[0]);
      const m = parseInt(parts[1]) - 1;
      const d = parseInt(parts[2]);
      if (!isNaN(y) && !isNaN(m) && !isNaN(d)) {
        viewDate.value = new Date(y, m, d);
      }
    }
  }
}, { immediate: true });

const formattedDisplay = computed(() => {
  if (!props.modelValue) return '';
  const parts = props.modelValue.split('T')[0].split('-');
  if (parts.length === 3) {
    const [y, m, d] = parts;
    return `${d.padStart(2, '0')}/${m.padStart(2, '0')}/${y}`;
  }
  return props.modelValue;
});

function prevMonth() {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() - 1, 1);
}

function nextMonth() {
  viewDate.value = new Date(viewDate.value.getFullYear(), viewDate.value.getMonth() + 1, 1);
}

function selectToday() {
  const today = new Date();
  const y = today.getFullYear();
  const m = String(today.getMonth() + 1).padStart(2, '0');
  const d = String(today.getDate()).padStart(2, '0');
  const val = `${y}-${m}-${d}`;
  emit('update:modelValue', val);
  emit('change', val);
  isOpen.value = false;
}

function clearDate() {
  emit('update:modelValue', '');
  emit('change', '');
  isOpen.value = false;
}

function selectDay(dayItem) {
  emit('update:modelValue', dayItem.dateStr);
  emit('change', dayItem.dateStr);
  isOpen.value = false;
}

const calendarDays = computed(() => {
  const year = viewDate.value.getFullYear();
  const month = viewDate.value.getMonth();

  const firstDayOfMonth = new Date(year, month, 1);
  let startingDay = firstDayOfMonth.getDay() - 1;
  if (startingDay < 0) startingDay = 6; // Sunday becomes 6

  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const daysInPrevMonth = new Date(year, month, 0).getDate();

  const days = [];

  // Previous month padding
  for (let i = startingDay - 1; i >= 0; i--) {
    const dayNum = daysInPrevMonth - i;
    const prevDate = new Date(year, month - 1, dayNum);
    days.push({
      day: dayNum,
      dateStr: formatDateIso(prevDate),
      isCurrentMonth: false,
      isToday: isSameDay(prevDate, new Date()),
      isSelected: isSameDayStr(props.modelValue, formatDateIso(prevDate))
    });
  }

  // Current month
  for (let d = 1; d <= daysInMonth; d++) {
    const curDate = new Date(year, month, d);
    days.push({
      day: d,
      dateStr: formatDateIso(curDate),
      isCurrentMonth: true,
      isToday: isSameDay(curDate, new Date()),
      isSelected: isSameDayStr(props.modelValue, formatDateIso(curDate))
    });
  }

  // Next month padding to fill grid
  const totalCells = Math.ceil(days.length / 7) * 7;
  const paddingCount = (totalCells < 35 ? 35 : totalCells) - days.length;
  for (let d = 1; d <= paddingCount; d++) {
    const nextDate = new Date(year, month + 1, d);
    days.push({
      day: d,
      dateStr: formatDateIso(nextDate),
      isCurrentMonth: false,
      isToday: isSameDay(nextDate, new Date()),
      isSelected: isSameDayStr(props.modelValue, formatDateIso(nextDate))
    });
  }

  return days;
});

function formatDateIso(date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function isSameDay(d1, d2) {
  return d1.getFullYear() === d2.getFullYear() &&
         d1.getMonth() === d2.getMonth() &&
         d1.getDate() === d2.getDate();
}

function isSameDayStr(val1, val2) {
  if (!val1 || !val2) return false;
  return val1.split('T')[0] === val2.split('T')[0];
}
</script>
