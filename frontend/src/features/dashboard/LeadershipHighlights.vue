<template>
  <section aria-label="Các chỉ số điều hành" class="space-y-3">
    <dl class="grid grid-cols-2 gap-3">
      <div v-for="card in cards" :key="card.status" :data-status="card.status" class="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm border-t-4" :class="toneBorders[card.tone]">
        <dt class="text-sm font-semibold text-slate-600 min-h-10">{{ card.label }}</dt>
        <dd class="text-[1.75rem] leading-9 font-bold text-slate-900 mt-2 tabular-nums">{{ formatNumber(card.value) }}</dd>
      </div>
    </dl>
  </section>
</template>
<script setup>
import { computed } from 'vue';
import { formatNumber } from '../../shared/formatters';
const props = defineProps({ summary: { type: Object, default: () => ({}) }, total: { type: Number, default: 0 } });
const toneBorders = { slate: 'border-t-slate-500', emerald: 'border-t-emerald-600', rose: 'border-t-rose-600', amber: 'border-t-amber-600', blue: 'border-t-blue-600', teal: 'border-t-teal-600', orange: 'border-t-teal-600' };
const cards = computed(() => [
  { status:'total', label:'Tổng', tone:'blue', value:props.total },
  { status:'notStarted', label:'Chưa thực hiện', tone:'slate', value:props.summary.notStarted ?? 0 },
  { status:'inProgress', label:'Đang thực hiện', tone:'amber', value:props.summary.inProgress ?? 0 },
  { status:'completed', label:'Đã hoàn thành', tone:'emerald', value:props.summary.completed ?? 0 }
]);
</script>
