<template>
  <div class="grid grid-cols-1 gap-2 text-xs">
    <component :is="interactive ? 'button' : 'div'" v-for="status in executionStatusSeries" :key="status.value"
      :type="interactive ? 'button' : undefined" :aria-label="interactive ? `Xem ${status.label} — ${context}` : undefined"
      class="flex items-center justify-between gap-3 rounded-lg border p-2 text-left"
      :class="[getStatusBadgeClass(status.value), interactive && 'cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600']"
      @click="interactive && $emit('select', status.value)">
      <span class="flex items-center gap-2"><span class="h-2.5 w-2.5 shrink-0 rounded-full" :style="{ backgroundColor: themeColor(status.color) }"></span>{{ status.label }}</span>
      <span class="font-bold tabular-nums">{{ formatNumber(summary[status.key] ?? 0) }}</span>
    </component>
  </div>
</template>
<script setup>
import { executionStatusSeries, getStatusBadgeClass } from '../shared/statusPresentation';
import { formatNumber } from '../shared/formatters';
import { themeColor } from '../shared/themeColors';
defineProps({ summary: { type: Object, default: () => ({}) }, interactive: Boolean, context: { type: String, default: 'danh sách' } });
defineEmits(['select']);
</script>
