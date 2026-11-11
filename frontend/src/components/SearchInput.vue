<template>
  <div :class="['relative min-w-0', wrapperClass]">
    <label v-if="label" :for="inputId" :class="labelClass || 'sr-only'">
      {{ label }}
    </label>

    <!-- Magnifying glass icon on left -->
    <span
      class="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400 flex items-center justify-center"
      aria-hidden="true"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
      </svg>
    </span>

    <!-- Text input -->
    <input
      :id="inputId"
      ref="inputRef"
      type="text"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :aria-label="ariaLabel || label || placeholder"
      :class="[
        'ui-single-line w-full text-xs font-semibold pl-9 pr-8 bg-white border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500 focus:outline-none transition shadow-2xs',
        disabled ? 'bg-slate-100 text-slate-400 border-slate-200 cursor-not-allowed' : '',
        inputClass
      ]"
      @input="handleInput"
      @keydown.enter.prevent="handleEnter"
      @keydown.esc.prevent="handleClear"
    />

    <!-- Clear button on right -->
    <button
      v-if="clearable && hasValue && !disabled"
      type="button"
      class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 focus:text-slate-700 p-1 rounded-md transition cursor-pointer flex items-center justify-center leading-none"
      :aria-label="clearAriaLabel || 'Xóa tìm kiếm'"
      v-tip="clearAriaLabel || 'Xóa tìm kiếm'"
      @click="handleClear"
    >
      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, computed, useId, onUnmounted } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Tìm kiếm...'
  },
  ariaLabel: {
    type: String,
    default: ''
  },
  label: {
    type: String,
    default: ''
  },
  labelClass: {
    type: String,
    default: ''
  },
  id: {
    type: String,
    default: null
  },
  debounceMs: {
    type: Number,
    default: 500
  },
  disabled: {
    type: Boolean,
    default: false
  },
  clearable: {
    type: Boolean,
    default: true
  },
  clearAriaLabel: {
    type: String,
    default: 'Xóa tìm kiếm'
  },
  wrapperClass: {
    type: String,
    default: 'flex-1'
  },
  inputClass: {
    type: String,
    default: ''
  }
});

const emit = defineEmits(['update:modelValue', 'search', 'clear']);

const generatedId = useId();
const inputId = computed(() => props.id || `search-input-${generatedId}`);
const inputRef = ref(null);

let debounceTimer = null;

const hasValue = computed(() => {
  return props.modelValue !== null && props.modelValue !== undefined && String(props.modelValue).length > 0;
});

function handleInput(event) {
  const value = event.target.value;
  emit('update:modelValue', value);

  if (debounceTimer) {
    clearTimeout(debounceTimer);
  }

  if (props.debounceMs > 0) {
    debounceTimer = setTimeout(() => {
      emit('search', value.trim());
    }, props.debounceMs);
  } else {
    emit('search', value.trim());
  }
}

function handleEnter() {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
    debounceTimer = null;
  }
  emit('search', (props.modelValue || '').trim());
}

function handleClear() {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
    debounceTimer = null;
  }
  emit('update:modelValue', '');
  emit('search', '');
  emit('clear');
  inputRef.value?.focus();
}

onUnmounted(() => {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
  }
});

defineExpose({
  focus: () => inputRef.value?.focus()
});
</script>
