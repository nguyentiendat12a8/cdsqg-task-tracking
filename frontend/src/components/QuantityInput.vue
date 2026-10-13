<template>
  <input type="text" inputmode="decimal" :value="display" @focus="editing = true" @input="input" @blur="blur" />
</template>
<script setup>
import { ref, watch, useAttrs } from 'vue';
import { parseQuantityInput, formatQuantityInput } from '../shared/numericInput';
const props = defineProps({ modelValue: [Number, String], stringValue: Boolean });
const emit = defineEmits(['update:modelValue', 'change']);
const attrs = useAttrs();
const display = ref('');
let editing = false;
watch(() => props.modelValue, value => { if (!editing) display.value = formatQuantityInput(value); }, { immediate: true });
function input(event) {
  display.value = event.target.value;
  const value = parseQuantityInput(display.value);
  const invalid = Number.isNaN(value) || (value != null && ((attrs.min != null && value < Number(attrs.min)) || (attrs.max != null && value > Number(attrs.max))));
  event.target.setCustomValidity(invalid ? 'Nhập số hợp lệ: ví dụ 20.000 hoặc 20.000,5.' : '');
  event.target.setAttribute('aria-invalid', String(invalid));
  if (!invalid) emit('update:modelValue', value == null ? (props.stringValue ? '' : null) : (props.stringValue ? display.value.trim().replaceAll('.', '').replace(',', '.') : value));
}
function blur(event) {
  editing = false;
  const value = parseQuantityInput(display.value);
  if (!event.target.validity.valid || Number.isNaN(value)) { event.target.reportValidity(); return; }
  display.value = formatQuantityInput(props.stringValue ? props.modelValue : value);
  emit('change', value);
}
</script>
