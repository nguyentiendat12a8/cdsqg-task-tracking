<template>
  <div class="space-y-1.5">
    <label :for="inputId" class="block text-xs font-semibold text-slate-700">Kết quả cần đạt</label>
    <div class="relative">
      <QuantityInput :id="inputId" v-model="model" min="0.000001" max="1000000000000" step="any" placeholder="Nhập kết quả cuối kỳ" class="ui-control w-full rounded-xl border border-slate-300 bg-white pl-3 pr-20" />
      <span class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-xs text-slate-500">{{ unitName || '%' }}</span>
    </div>
  </div>
</template>
<script setup>
import { useId } from 'vue';
import QuantityInput from '../../components/QuantityInput.vue';
const model = defineModel();
defineProps({ unitName: { type: String, default: '%' } });
const inputId = `goal-result-${useId()}`;
</script>
