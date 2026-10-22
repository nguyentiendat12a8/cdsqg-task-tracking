<template>
  <VDropdown 
    :triggers="['click']" 
    :auto-hide="false" 
    :placement="placement"
    :distance="8"
    :overflow-padding="overflowPadding"
    v-model:shown="isOpen"
    @show="emit('show')"
    popper-class="v-popper-clean-overlay"
  >
    <slot name="trigger" :is-open="isOpen" :set-trigger-ref="setTriggerRef" :close="closePanel">
    <!-- Trigger Button -->
    <button 
      ref="triggerBtnRef"
      type="button" 
      class="ui-single-line ui-toolbar-control px-3.5 py-2 bg-slate-100 hover:bg-slate-200 active:bg-slate-300 text-slate-700 font-semibold border border-slate-200/80 transition flex items-center gap-1.5 shrink-0 cursor-pointer shadow-2xs select-none whitespace-nowrap"
      :class="{ '!bg-blue-50 !text-blue-700 !border-blue-300': isOpen || activeCount > 0 }"
      title="Mở bộ lọc nâng cao"
    >
      <svg class="w-4 h-4 text-slate-500" :class="{ '!text-blue-600': activeCount > 0 || isOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/>
      </svg>
      <span>{{ buttonText }}</span>
      <span 
        v-if="activeCount > 0" 
        class="ml-0.5 px-1.5 py-0.5 bg-blue-600 text-white font-bold text-xs rounded-full min-w-[18px] text-center leading-none shadow-xs"
      >
        {{ formatNumber(activeCount) }}
      </span>
      <svg class="w-3.5 h-3.5 opacity-60 transition-transform duration-200" :class="{ 'rotate-180': isOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    </slot>

    <!-- Overlay Panel Popper Content -->
    <template #popper>
      <div 
        ref="panelRef"
        @keydown.esc.stop.prevent="closePanel"
        class="rounded-2xl font-sans p-4 space-y-3.5 z-50 text-slate-800 flex flex-col max-w-full max-h-[calc(100dvh-2rem)]"
        :class="[panelWidthClass, heightClass, panelClass || 'bg-white shadow-xl shadow-blue-900/10 border border-blue-200']"
      >
        <!-- Panel Header -->
        <div class="flex shrink-0 items-center justify-between border-b border-slate-200 pb-2.5">
          <slot name="heading">
          <div class="flex items-center gap-2">
            <div class="w-7 h-7 rounded-lg bg-blue-100 text-blue-700 flex items-center justify-center font-bold">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"/>
              </svg>
            </div>
            <h4 class="text-xs sm:text-sm font-semibold text-slate-900">
              {{ title }}
            </h4>
          </div>

          </slot>
          <button 
            type="button" 
            aria-label="Đóng bảng tùy chọn"
            @click="closePanel" 
            class="w-6 h-6 rounded-lg bg-slate-200/80 hover:bg-slate-300 text-slate-500 hover:text-slate-800 flex items-center justify-center text-xs font-bold transition cursor-pointer"
            title="Đóng panel"
          >
            ✕
          </button>
        </div>

        <!-- Filter Form Fields Slot -->
        <div class="space-y-3 min-h-0 flex-1 overflow-y-auto custom-scrollbar px-0.5">
          <slot :close="closePanel"></slot>
        </div>

        <!-- Panel Footer / Actions -->
        <div v-if="showActions" class="flex shrink-0 items-center justify-between gap-2 border-t border-slate-200 pt-3">
          <button 
            type="button" 
            @click="handleReset" 
            class="ui-single-line px-3 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer flex items-center gap-1"
          >
            <span aria-hidden="true">↺</span>
            <span>Đặt lại</span>
          </button>

          <div class="flex items-center gap-2">
            <button 
              type="button" 
              @click="closePanel" 
              class="ui-single-line px-3 py-1.5 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition cursor-pointer"
            >
              Đóng
            </button>
            <button 
              type="button" 
              @click="handleApply"
              :disabled="applyDisabled"
              class="ui-single-line px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-xs transition cursor-pointer flex items-center gap-1.5"
            >
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
              <span>Áp dụng lọc</span>
            </button>
          </div>
        </div>
      </div>
    </template>
  </VDropdown>
</template>

<script setup>
import { formatNumber } from "../shared/formatters";
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue';

const props = defineProps({
  applyDisabled: { type: Boolean, default: false },
  overflowPadding: { type: Number, default: 0 },
  placement: { type: String, default: 'bottom-start' },
  showActions: { type: Boolean, default: true },
  panelClass: { type: String, default: '' },
  heightClass: { type: String, default: '' },
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

const emit = defineEmits(['apply', 'reset', 'show']);

const isOpen = ref(false);
const panelRef = ref(null);
const triggerBtnRef = ref(null);

const panelWidthClass = computed(() => props.widthClass);

function setTriggerRef(element) { triggerBtnRef.value = element; }
function closePanel() {
  isOpen.value = false;
  nextTick(() => triggerBtnRef.value?.focus());
}

function handleApply() {
  if (props.applyDisabled) return;
  isOpen.value = false;
  emit('apply');
}

function handleReset() {
  emit('reset');
}

function handleClickOutside(e) {
  if (!isOpen.value) return;
  const target = e.target;
  if (panelRef.value && panelRef.value.contains(target)) return;
  if (triggerBtnRef.value && triggerBtnRef.value.contains(target)) return;
  if (target.closest && (target.closest('.fixed.bg-white') || target.closest('.z-\\[9999999\\]') || target.closest('.v-popper__popper'))) return;

  isOpen.value = false;
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>

<style>
.v-popper-clean-overlay,
.v-popper-clean-overlay .v-popper__popper,
.v-popper-clean-overlay .v-popper__wrapper,
.v-popper-clean-overlay .v-popper__inner {
  border-radius: 1rem !important;
  border: none !important;
  background: transparent !important;
  box-shadow: none !important;
  padding: 0 !important;
  outline: none !important;
  overflow: hidden !important;
}
.v-popper-clean-overlay .v-popper__arrow-container,
.v-popper-clean-overlay .v-popper__arrow-outer,
.v-popper-clean-overlay .v-popper__arrow-inner,
.v-popper-clean-overlay .v-popper__arrow {
  display: none !important;
  visibility: hidden !important;
  opacity: 0 !important;
}
@media (max-width: 639px) {
  .v-popper-clean-overlay.v-popper__popper {
    position: fixed !important;
    top: 1rem !important;
    bottom: auto !important;
    left: 50% !important;
    transform: translateX(-50%) !important;
    max-width: calc(100vw - 2rem);
    max-height: calc(100dvh - 2rem);
  }
}
</style>
