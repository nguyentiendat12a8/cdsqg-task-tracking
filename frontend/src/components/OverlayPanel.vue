<template>
  <VDropdown 
    :triggers="['click']" 
    :auto-hide="false" 
    placement="bottom-start"
    :distance="8"
    v-model:shown="isOpen"
    popper-class="v-popper-clean-overlay"
  >
    <!-- Trigger Button -->
    <button 
      type="button" 
      class="px-3.5 py-1.5 bg-slate-100 hover:bg-slate-200 active:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl border border-slate-200/80 transition flex items-center gap-1.5 shrink-0 cursor-pointer shadow-2xs select-none whitespace-nowrap"
      :class="{ '!bg-blue-50 !text-blue-700 !border-blue-300': isOpen || activeCount > 0 }"
      title="Mở bộ lọc nâng cao"
    >
      <svg class="w-4 h-4 text-slate-500" :class="{ '!text-blue-600': activeCount > 0 || isOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/>
      </svg>
      <span>{{ buttonText }}</span>
      <span 
        v-if="activeCount > 0" 
        class="ml-0.5 px-1.5 py-0.5 bg-blue-600 text-white font-extrabold text-[10px] rounded-full min-w-[18px] text-center leading-none shadow-xs"
      >
        {{ activeCount }}
      </span>
      <svg class="w-3.5 h-3.5 opacity-60 transition-transform duration-200" :class="{ 'rotate-180': isOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <!-- Overlay Panel Popper Content -->
    <template #popper>
      <div 
        class="bg-white rounded-2xl shadow-2xl border border-slate-200/90 font-sans p-4 space-y-3.5 z-50 text-slate-800"
        :class="panelWidthClass"
      >
        <!-- Panel Header -->
        <div class="flex items-center justify-between border-b border-slate-100 pb-2.5">
          <div class="flex items-center gap-2">
            <div class="w-7 h-7 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center font-bold">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"/>
              </svg>
            </div>
            <h4 class="text-xs sm:text-sm font-extrabold text-slate-800 uppercase tracking-wide">
              {{ title }}
            </h4>
          </div>

          <button 
            type="button" 
            @click="closePanel" 
            class="w-6 h-6 rounded-lg hover:bg-slate-100 text-slate-400 hover:text-slate-600 flex items-center justify-center text-xs font-bold transition cursor-pointer"
            title="Đóng panel"
          >
            ✕
          </button>
        </div>

        <!-- Filter Form Fields Slot -->
        <div class="space-y-3 max-h-[70vh] overflow-y-auto custom-scrollbar px-0.5">
          <slot></slot>
        </div>

        <!-- Panel Footer / Actions -->
        <div class="flex items-center justify-between gap-2 border-t border-slate-100 pt-3">
          <button 
            type="button" 
            @click="handleReset" 
            class="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl transition cursor-pointer flex items-center gap-1"
          >
            <span>↺</span>
            <span>Đặt Lại</span>
          </button>

          <div class="flex items-center gap-2">
            <button 
              type="button" 
              @click="closePanel" 
              class="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl transition cursor-pointer"
            >
              Đóng
            </button>
            <button 
              type="button" 
              @click="handleApply" 
              class="px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-xs transition cursor-pointer flex items-center gap-1.5"
            >
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
              <span>Áp Dụng Lọc</span>
            </button>
          </div>
        </div>
      </div>
    </template>
  </VDropdown>
</template>

<script setup>
import { ref, computed } from 'vue';

const props = defineProps({
  title: {
    type: String,
    default: 'Bộ Lọc Tìm Kiếm Nâng Cao'
  },
  buttonText: {
    type: String,
    default: 'Tìm Kiếm Nâng Cao'
  },
  activeCount: {
    type: Number,
    default: 0
  },
  widthClass: {
    type: String,
    default: 'w-[320px] sm:w-[460px]'
  }
});

const emit = defineEmits(['apply', 'reset']);

const isOpen = ref(false);

const panelWidthClass = computed(() => props.widthClass);

function closePanel() {
  isOpen.value = false;
}

function handleApply() {
  isOpen.value = false;
  emit('apply');
}

function handleReset() {
  emit('reset');
}
</script>

<style>
.v-popper-clean-overlay .v-popper__inner {
  border-radius: 0 !important;
  border: none !important;
  background: transparent !important;
  box-shadow: none !important;
  padding: 0 !important;
  outline: none !important;
}
.v-popper-clean-overlay .v-popper__arrow-container,
.v-popper-clean-overlay .v-popper__arrow-outer,
.v-popper-clean-overlay .v-popper__arrow-inner {
  display: none !important;
  visibility: hidden !important;
  opacity: 0 !important;
}
</style>
