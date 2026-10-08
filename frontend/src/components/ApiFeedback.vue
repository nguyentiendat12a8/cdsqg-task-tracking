<template>
  <div v-if="message" role="alert" class="mx-4 mt-3 p-3 bg-amber-50 border border-amber-300 rounded-xl flex items-start gap-3 text-sm text-amber-950">
    <p class="flex-1">{{ message }}</p><button type="button" aria-label="Đóng thông báo lỗi" @click="message = ''">×</button>
  </div>
</template>
<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
const message = ref('');
function show(event) { message.value = event.detail; }
onMounted(() => window.addEventListener('api-feedback', show));
onUnmounted(() => window.removeEventListener('api-feedback', show));
</script>
