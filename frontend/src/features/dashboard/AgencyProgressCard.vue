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
      <div class="flex-1 min-w-0 space-y-1 text-[10px] font-bold">
        <div v-for="status in statuses" v-show="status.key !== 'notStarted' || item.notStarted > 0" :key="status.key" class="flex items-center justify-between gap-1.5">
          <span class="flex items-center gap-1.5 min-w-0">
            <span :class="['w-2.5 h-2.5 rounded-full shrink-0', status.color]"></span>
            <span class="text-slate-600 truncate">{{ status.label }}</span>
          </span>
          <span class="font-bold text-slate-900">{{ item[status.key] || 0 }}</span>
        </div>
      </div>
    </div>
    <div class="pt-2 border-t border-slate-100 flex items-center justify-between text-[10px] text-slate-500 font-bold w-full">
      <span v-if="dashboardFilter === 'goals'" class="text-purple-800 bg-purple-50 px-1.5 py-0.5 rounded border border-purple-200/60">🎯 {{ item.totalGoals || 0 }} Mục tiêu</span>
      <span v-else class="text-blue-800 bg-blue-50 px-1.5 py-0.5 rounded border border-blue-200/60">📋 {{ item.totalTasks || 0 }} Nhiệm vụ</span>
      <span class="text-blue-600 group-hover:underline">Chi tiết →</span>
    </div>
  </button>
</template>

<script setup>
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
const statuses = [
  { key: 'inProgressOverdue', label: 'Đang t/h quá hạn', color: 'bg-rose-500' },
  { key: 'inProgressOnTime', label: 'Đang t/h trong hạn', color: 'bg-blue-500' },
  { key: 'expiringSoon', label: 'Sắp tới hạn', color: 'bg-amber-500' },
  { key: 'completedOverdue', label: 'Đã h/t quá hạn', color: 'bg-teal-500' },
  { key: 'completedOnTime', label: 'Đã h/t trong hạn', color: 'bg-emerald-500' },
  { key: 'notStarted', label: 'Chưa thực hiện', color: 'bg-slate-400' }
];
</script>
