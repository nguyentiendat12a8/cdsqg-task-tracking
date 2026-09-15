<template>
  <div :class="['w-full flex flex-col items-center justify-center gap-3', pyClass]">
    <div class="relative flex items-center justify-center">
      <!-- Outer Pulsing Glow -->
      <div class="absolute w-12 h-12 rounded-full bg-blue-400/20 animate-ping"></div>
      
      <!-- Spinning Gradient SVG Ring -->
      <svg class="w-10 h-10 animate-spin text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-20" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3.5"></circle>
        <path class="opacity-90" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
      
      <!-- Inner Core Dot -->
      <div class="absolute w-3 h-3 rounded-full bg-blue-600"></div>
    </div>
    
    <!-- Optional Loading Text -->
    <p v-if="text" class="text-xs font-extrabold text-slate-500 tracking-wide animate-pulse">
      {{ text }}
    </p>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  text: {
    type: String,
    default: 'Đang tải dữ liệu từ máy chủ...'
  },
  padding: {
    type: String,
    default: 'py-12'
  }
});

const pyClass = computed(() => props.padding || 'py-12');
</script>
