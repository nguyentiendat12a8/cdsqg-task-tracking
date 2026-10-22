<template>
  <div 
    class="relative rounded-full flex items-center justify-center shrink-0 shadow-sm transition-transform duration-200 hover:scale-105 cursor-pointer group select-none"
    :style="{ width: `${size}px`, height: `${size}px`, background: conicGradient }"
    :title="tooltipText"
  >
    <!-- Center Cutout Hole -->
    <div 
      class="bg-white rounded-full flex flex-col items-center justify-center font-bold text-slate-800 shadow-inner"
      :style="{ width: `${innerSize}px`, height: `${innerSize}px` }"
    >
      <slot name="center"><span class="font-bold text-slate-900 leading-none" :style="{ fontSize: `${fontSize}px` }">{{ formatNumber(total) }}</span></slot>
    </div>
  </div>
</template>

<script setup>
import { executionStatusSeries } from '../shared/statusPresentation';
import { themeColor } from "../shared/themeColors";
import { formatNumber } from "../shared/formatters";
import { computed } from 'vue';

const props = defineProps({
  stats: {
    type: Object,
    default: () => ({})
  },
  size: {
    type: Number,
    default: 88
  },
  innerSize: {
    type: Number,
    default: 58
  },
  fontSize: {
    type: Number,
    default: 20
  }
});

const total = computed(() => props.stats.totalItems || props.stats.total || 0);

const conicGradient = computed(() => {
  const tot = total.value;
  if (!tot || tot <= 0) {
    return 'conic-gradient(#e2e8f0 0% 100%)';
  }

  const segments = executionStatusSeries.map(status => ({ count: props.stats[status.key] || 0, color: themeColor(status.color) }));

  let accumulatedPct = 0;
  const stops = [];

  segments.forEach(seg => {
    if (seg.count > 0) {
      const pct = (seg.count / tot) * 100;
      const start = accumulatedPct.toFixed(1);
      accumulatedPct += pct;
      const end = accumulatedPct.toFixed(1);
      stops.push(`${seg.color} ${start}% ${end}%`);
    }
  });

  if (stops.length === 0) return 'conic-gradient(#e2e8f0 0% 100%)';
  return `conic-gradient(${stops.join(', ')})`;
});

const tooltipText = computed(() => {
  return `Tổng: ${total.value} hạng mục\n` + executionStatusSeries.map(status => `• ${status.label}: ${props.stats[status.key] || 0}`).join('\n');
});
</script>
