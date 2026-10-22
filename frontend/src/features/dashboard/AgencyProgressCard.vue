<template>
  <button
    type="button"
    :aria-label="`Xem chi tiết ${item.name}`"
    class="bg-white text-left rounded-2xl border border-slate-200/90 p-3.5 shadow-2xs hover:shadow-md transition-all duration-200 cursor-pointer space-y-3 group flex flex-col justify-between"
    @click="$emit('select', item)"
  >
    <div class="flex items-start justify-between gap-2 border-b border-slate-100 pb-2.5 w-full">
      <h4 :title="item.name" class="min-w-0 flex-1 text-xs sm:text-sm font-bold text-slate-800 leading-snug truncate">
        {{ item.name }}
      </h4>
      <span :class="['w-7 h-7 text-white font-bold text-xs rounded-xl flex items-center justify-center shrink-0 shadow-2xs', badgeColor]">
        {{ index + 1 }}
      </span>
    </div>
    <div class="flex items-center gap-3 py-0.5 w-full">
      <MiniStatusDonut :stats="item" :size="84" :innerSize="54" :fontSize="18" />
      <ExecutionStatusBreakdown class="flex-1 min-w-0" :summary="item" />
    </div>
    <div class="pt-2 border-t border-slate-100 flex items-center justify-between text-xs text-slate-500 font-bold w-full">
      <span v-if="dashboardFilter === 'goals'" class="text-purple-800 bg-purple-50 px-1.5 py-0.5 rounded border border-purple-200/60">🎯 {{ formatNumber(item.totalGoals || 0) }} mục tiêu</span>
      <span v-else class="text-blue-800 bg-blue-50 px-1.5 py-0.5 rounded border border-blue-200/60">📋 {{ formatNumber(item.totalTasks || 0) }} nhiệm vụ</span>
      <span class="text-blue-600 group-hover:underline">Chi tiết →</span>
    </div>
  </button>
</template>

<script setup>
import ExecutionStatusBreakdown from '../../components/ExecutionStatusBreakdown.vue';
import { formatNumber } from "../../shared/formatters";
import { computed } from 'vue';
import MiniStatusDonut from '../../components/MiniStatusDonut.vue';

const props = defineProps({
  item: { type: Object, required: true },
  index: { type: Number, required: true },
  dashboardFilter: { type: String, required: true },
  tone: { type: String, default: 'blue' }
});
defineEmits(['select']);
const badgeColor = computed(() => ({ blue: 'bg-blue-600', purple: 'bg-purple-600', indigo: 'bg-indigo-600' })[props.tone] || 'bg-blue-600');
</script>
