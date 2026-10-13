<template>
  <section aria-label="Các chỉ số điều hành" class="space-y-3">
    <dl class="grid grid-cols-2 md:grid-cols-3 2xl:grid-cols-6 gap-3">
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
import { executionStatuses } from '../../shared/statusPresentation';
const props = defineProps({ summary: { type: Object, default: () => ({}) } });
const toneBorders = { slate: 'border-t-slate-500', emerald: 'border-t-emerald-600', rose: 'border-t-rose-600', amber: 'border-t-amber-600', blue: 'border-t-blue-600', teal: 'border-t-teal-600', orange: 'border-t-teal-600' };
const cards = computed(() => Object.entries(executionStatuses).map(([status, presentation]) => ({
  status,
  ...presentation,
  value: props.summary[status.charAt(0).toLowerCase() + status.slice(1)] ?? 0
})));
</script>
