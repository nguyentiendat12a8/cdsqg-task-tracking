<template>
  <div 
    class="relative rounded-full flex items-center justify-center shrink-0 shadow-sm transition-transform duration-200 hover:scale-105 cursor-pointer group select-none"
    :style="{ width: `${size}px`, height: `${size}px`, background: conicGradient }"
    :title="tooltipText"
  >
    <!-- Center Cutout Hole -->
    <div 
      class="bg-white rounded-full flex flex-col items-center justify-center font-black text-slate-800 shadow-inner"
      :style="{ width: `${innerSize}px`, height: `${innerSize}px` }"
    >
      <span class="font-black text-slate-900 leading-none" :style="{ fontSize: `${fontSize}px` }">{{ total }}</span>
    </div>
  </div>
</template>

<script setup>
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

  // Exact colors matching application standard status legend:
  // 1. Đang t/h quá hạn (Red: #ef4444)
  // 2. Đang t/h trong hạn (Blue: #3b82f6)
  // 3. Sắp tới hạn (Amber: #f59e0b)
  // 4. Đã h/t quá hạn (Teal: #14b8a6)
  // 5. Đã h/t trong hạn (Emerald Green: #10b981)
  // 6. Chưa thực hiện (Slate Gray: #94a3b8)
  const segments = [
    { count: props.stats.inProgressOverdue || 0, color: '#ef4444' }, 
    { count: props.stats.inProgressOnTime || 0, color: '#3b82f6' },  
    { count: props.stats.expiringSoon || 0, color: '#f59e0b' },      
    { count: props.stats.completedOverdue || 0, color: '#14b8a6' },  
    { count: props.stats.completedOnTime || 0, color: '#10b981' },   
    { count: props.stats.notStarted || 0, color: '#94a3b8' }         
  ];

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
  const s = props.stats;
  return `Tổng: ${total.value} hạng mục\n• Đang t/h quá hạn: ${s.inProgressOverdue || 0}\n• Đang t/h trong hạn: ${s.inProgressOnTime || 0}\n• Sắp tới hạn: ${s.expiringSoon || 0}\n• Đã h/t quá hạn: ${s.completedOverdue || 0}\n• Đã h/t trong hạn: ${s.completedOnTime || 0}\n• Chưa thực hiện: ${s.notStarted || 0}`;
});
</script>
