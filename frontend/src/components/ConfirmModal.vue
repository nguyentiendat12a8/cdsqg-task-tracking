<template>
  <Teleport to="body">
    <Transition name="fade">
      <div 
        v-if="confirmState.isVisible.value" 
        class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 backdrop-blur-xs p-4 overflow-y-auto font-sans"
        @click.self="handleConfirmResponse(false)"
      >
        <div 
          class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-md p-5 sm:p-6 transition-all transform animate-in fade-in zoom-in-95 duration-150"
        >
          <div class="flex items-start gap-4">
            <!-- Icon Badge -->
            <div 
              v-if="confirmState.type.value === 'danger'"
              class="w-11 h-11 rounded-2xl bg-rose-100 text-rose-600 flex items-center justify-center shrink-0 shadow-inner"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </div>
            <div 
              v-else-if="confirmState.type.value === 'warning'"
              class="w-11 h-11 rounded-2xl bg-amber-100 text-amber-600 flex items-center justify-center shrink-0 shadow-inner"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
            </div>
            <div 
              v-else
              class="w-11 h-11 rounded-2xl bg-blue-100 text-blue-600 flex items-center justify-center shrink-0 shadow-inner"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>

            <!-- Modal Body Text -->
            <div class="flex-1 min-w-0">
              <h3 class="text-base sm:text-lg font-bold text-slate-800 leading-snug">
                {{ confirmState.title.value }}
              </h3>
              <p class="text-xs sm:text-sm text-slate-600 mt-1.5 leading-relaxed break-words">
                {{ confirmState.message.value }}
              </p>
            </div>
          </div>

          <!-- Modal Actions -->
          <div class="mt-6 flex items-center justify-end gap-2.5 pt-3 border-t border-slate-100">
            <button
              @click="handleConfirmResponse(false)"
              class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold text-xs sm:text-sm rounded-xl transition cursor-pointer"
            >
              {{ confirmState.cancelText.value }}
            </button>
            <button
              @click="handleConfirmResponse(true)"
              :class="[
                'px-4 py-2 text-white font-bold text-xs sm:text-sm rounded-xl shadow-xs transition cursor-pointer',
                confirmState.type.value === 'danger' ? 'bg-rose-600 hover:bg-rose-700' : 
                confirmState.type.value === 'warning' ? 'bg-amber-600 hover:bg-amber-700' : 'bg-blue-600 hover:bg-blue-700'
              ]"
            >
              {{ confirmState.confirmText.value }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue';
import { confirmState, handleConfirmResponse } from '../services/confirm';

function handleKeydown(e) {
  if (confirmState.isVisible.value && e.key === 'Escape') {
    handleConfirmResponse(false);
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeydown);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown);
});
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
