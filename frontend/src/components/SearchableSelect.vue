<template>
  <div class="relative w-full text-left font-sans" ref="container">
    <label v-if="label" :class="labelClass || 'text-xs font-bold text-slate-700 uppercase block mb-1'">
      {{ label }} <span v-if="required" class="text-rose-500 ml-0.5">*</span>
    </label>
    
    <!-- Select Box Trigger -->
    <div 
      ref="triggerRef"
      @click="toggleDropdown"
      :class="[
        'w-full border rounded-xl px-2.5 py-1 text-xs font-semibold flex items-center justify-between transition shadow-2xs h-[34px] min-h-[34px] max-h-[34px] overflow-hidden',
        disabled ? 'bg-slate-100 border-slate-200 text-slate-500 cursor-not-allowed opacity-75' : 'bg-white text-slate-800 cursor-pointer',
        isOpen ? 'border-blue-500 ring-2 ring-blue-500/20' : (disabled ? '' : 'border-slate-200 hover:border-slate-300')
      ]"
    >
      <div class="flex flex-nowrap gap-1 items-center overflow-hidden flex-1 min-w-0 max-w-full">
        <!-- Empty / All state -->
        <span v-if="selectedList.length === 0" class="text-slate-500 font-semibold truncate">
          {{ placeholder || 'Tất cả' }}
        </span>

        <!-- Selected Tags -->
        <template v-else>
          <!-- Single Select text display -->
          <span 
            v-if="!isMulti && displaySelectedItems.length > 0" 
            class="text-xs font-bold text-blue-900 truncate max-w-full block"
            :title="displaySelectedItems[0].label"
          >
            {{ displaySelectedItems[0].label }}
          </span>

          <!-- Multi Select Tags display -->
          <template v-else>
            <span 
              v-for="item in displaySelectedItems" 
              :key="item.value"
              class="bg-blue-50 text-blue-700 border border-blue-200/80 text-[11px] font-bold px-1.5 py-0.5 rounded-md flex items-center gap-1 shrink min-w-0 max-w-[140px] truncate"
              :title="item.label"
            >
              <span class="truncate">{{ item.label }}</span>
              <button 
                v-if="clearable"
                type="button"
                @click.stop="removeItem(item)"
                class="hover:text-blue-950 text-slate-400 hover:text-rose-600 font-bold text-xs leading-none shrink-0"
                title="Xóa chọn"
              >
                ×
              </button>
            </span>
            <span v-if="selectedList.length > maxDisplayTags" class="text-[11px] font-bold text-slate-600 bg-slate-100 border border-slate-200 px-1.5 py-0.5 rounded-md shrink-0">
              +{{ selectedList.length - maxDisplayTags }}
            </span>
          </template>
        </template>
      </div>

      <div class="flex items-center gap-1 shrink-0 ml-1">
        <button 
          v-if="clearable && selectedList.length > 0"
          type="button"
          @click.stop="clearAll"
          class="text-slate-400 hover:text-slate-600 p-0.5 text-xs font-bold"
          title="Xóa tất cả chọn"
        >
          ✕
        </button>
        <svg 
          :class="['w-4 h-4 text-slate-400 transition-transform duration-200', { 'rotate-180 text-blue-600': isOpen }]" 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
        </svg>
      </div>
    </div>

    <!-- Dropdown Floating Popover Teleported to Body -->
    <Teleport to="body">
      <div 
        v-if="isOpen" 
        ref="dropdownPanelRef"
        :style="dropdownStyle"
        class="fixed bg-white border border-slate-200 rounded-2xl shadow-2xl z-[9999999] p-2 space-y-2 max-w-full animate-in fade-in zoom-in-95 duration-100 font-sans text-left"
      >
        <!-- Quick Search input inside control -->
        <div class="relative">
          <input 
            ref="searchInputRef"
            :value="searchQuery"
            @input="searchQuery = $event.target.value"
            type="text" 
            placeholder="Tìm nhanh..."
            class="w-full bg-slate-100 border-0 rounded-xl pl-8 pr-7 py-1.5 text-xs font-semibold text-slate-800 focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
            @click.stop
          />
          <svg class="w-3.5 h-3.5 text-slate-400 absolute left-2.5 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          <button 
            v-if="searchQuery" 
            @click.stop="searchQuery = ''" 
            class="absolute right-2 top-2 text-slate-400 hover:text-slate-600 text-xs font-bold"
          >
            ✕
          </button>
        </div>

        <!-- Action helper buttons for Multi-select -->
        <div v-if="isMulti" class="flex items-center justify-between text-[11px] px-1 font-bold border-b border-slate-100 pb-1.5">
          <button type="button" @click.stop="selectAll" class="text-blue-600 hover:text-blue-800 hover:underline flex items-center gap-1">
            <span>✓ Chọn tất cả</span>
          </button>
          <button type="button" @click.stop="clearAll" class="text-slate-400 hover:text-slate-600">
            <span>✕ Bỏ chọn</span>
          </button>
        </div>

        <!-- Options List with Checkboxes -->
        <div class="max-h-52 overflow-y-auto space-y-0.5 custom-scrollbar pr-0.5">
          <div 
            v-for="opt in filteredOptions" 
            :key="opt.value"
            @click.stop="toggleOption(opt)"
            :class="[
              'px-2.5 py-1.5 rounded-xl text-xs font-medium cursor-pointer transition flex items-center gap-2 select-none',
              isSelected(opt) ? 'bg-blue-50 text-blue-800 font-bold' : 'hover:bg-slate-100 text-slate-700'
            ]"
          >
            <!-- Checkbox UI icon for multi-select -->
            <div 
              v-if="isMulti" 
              :class="[
                'w-4 h-4 rounded border flex items-center justify-center transition shrink-0',
                isSelected(opt) ? 'bg-blue-600 border-blue-600 text-white' : 'border-slate-300 bg-white'
              ]"
            >
              <svg v-if="isSelected(opt)" class="w-3 h-3 stroke-current" fill="none" viewBox="0 0 24 24" stroke-width="3">
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            </div>

            <span class="truncate flex-1">{{ opt.label }}</span>

            <!-- Single select check mark -->
            <svg v-if="!isMulti && isSelected(opt)" class="w-4 h-4 text-blue-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7"/>
            </svg>
          </div>

          <div v-if="filteredOptions.length === 0" class="p-3 text-center text-xs text-slate-400 font-semibold italic">
            Không tìm thấy kết quả.
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';

const props = defineProps({
  modelValue: { type: [Array, String, Number], default: () => [] },
  options: { type: Array, default: () => [] }, // [{ value: '...', label: '...' }]
  label: { type: String, default: '' },
  labelClass: { type: String, default: '' },
  placeholder: { type: String, default: '' },
  isMulti: { type: Boolean, default: true },
  clearable: { type: Boolean, default: true },
  required: { type: Boolean, default: false },
  disabled: { type: Boolean, default: false },
  maxDisplayTags: { type: Number, default: 1 }
});

const emit = defineEmits(['update:modelValue', 'change']);

const container = ref(null);
const triggerRef = ref(null);
const dropdownPanelRef = ref(null);
const searchInputRef = ref(null);
const isOpen = ref(false);
const searchQuery = ref('');
const dropdownStyle = ref({});

const selectedList = computed(() => {
  if (props.isMulti) {
    return Array.isArray(props.modelValue) ? props.modelValue : [];
  } else {
    return props.modelValue !== null && props.modelValue !== undefined && props.modelValue !== '' ? [props.modelValue] : [];
  }
});

const filteredOptions = computed(() => {
  if (!searchQuery.value.trim()) return props.options;
  const q = searchQuery.value.trim().toLowerCase();
  return props.options.filter(o => o.label?.toLowerCase().includes(q) || String(o.value).toLowerCase().includes(q));
});

const displaySelectedItems = computed(() => {
  const result = [];
  for (const val of selectedList.value) {
    const found = props.options.find(o => o.value === val);
    if (found) result.push(found);
    else result.push({ value: val, label: String(val) });
  }
  return result.slice(0, props.maxDisplayTags);
});

function updateDropdownPosition() {
  if (!triggerRef.value || !isOpen.value) return;
  const rect = triggerRef.value.getBoundingClientRect();
  const minWidth = Math.max(rect.width, 320);
  
  let top = rect.bottom + 4;
  let left = rect.left;

  // Viewport flip logic if bottom overflow
  const viewportHeight = window.innerHeight;
  const estimatedHeight = 260;
  if (top + estimatedHeight > viewportHeight && rect.top > estimatedHeight) {
    top = rect.top - estimatedHeight - 4;
  }

  // Viewport right limit
  const viewportWidth = window.innerWidth;
  if (left + minWidth > viewportWidth - 12) {
    left = viewportWidth - minWidth - 12;
  }
  if (left < 12) left = 12;

  dropdownStyle.value = {
    top: `${top}px`,
    left: `${left}px`,
    width: `${minWidth}px`,
    zIndex: 9999999
  };
}

function removeScrollListeners() {
  window.removeEventListener('scroll', updateDropdownPosition, true);
  window.removeEventListener('resize', updateDropdownPosition);
}

function toggleDropdown() {
  if (props.disabled) return;
  isOpen.value = !isOpen.value;
  if (isOpen.value) {
    updateDropdownPosition();
    nextTick(() => {
      updateDropdownPosition();
      searchInputRef.value?.focus();
    });
    window.addEventListener('scroll', updateDropdownPosition, true);
    window.addEventListener('resize', updateDropdownPosition);
  } else {
    removeScrollListeners();
  }
}

function isSelected(opt) {
  return selectedList.value.includes(opt.value);
}

function toggleOption(opt) {
  if (props.isMulti) {
    const current = [...selectedList.value];
    const idx = current.indexOf(opt.value);
    if (idx >= 0) current.splice(idx, 1);
    else current.push(opt.value);
    emit('update:modelValue', current);
    emit('change', current);
  } else {
    emit('update:modelValue', opt.value);
    emit('change', opt.value);
    isOpen.value = false;
    removeScrollListeners();
  }
}

function removeItem(item) {
  if (props.isMulti) {
    const current = selectedList.value.filter(v => v !== item.value);
    emit('update:modelValue', current);
    emit('change', current);
  } else {
    emit('update:modelValue', null);
    emit('change', null);
  }
}

function selectAll() {
  const allValues = props.options.map(o => o.value);
  emit('update:modelValue', allValues);
  emit('change', allValues);
}

function clearAll() {
  emit('update:modelValue', props.isMulti ? [] : null);
  emit('change', props.isMulti ? [] : null);
}

function handleClickOutside(e) {
  if (
    container.value && !container.value.contains(e.target) &&
    dropdownPanelRef.value && !dropdownPanelRef.value.contains(e.target)
  ) {
    isOpen.value = false;
    removeScrollListeners();
  }
}

watch(isOpen, (newVal) => {
  if (!newVal) {
    removeScrollListeners();
  }
});

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
  removeScrollListeners();
});
</script>
