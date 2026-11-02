<template>
  <div class="w-36 shrink-0">
    <SearchableSelect v-model="reportYear" :options="displayOptions" :isMulti="false" :clearable="false" placeholder="Năm báo cáo" aria-label="Năm báo cáo" />
  </div>
</template>
<script setup>
import { computed } from 'vue';
import SearchableSelect from './SearchableSelect.vue';
import { reportYear, getReportYearOptions } from '../shared/reportYear';
import { activeDocument } from '../services/documentContext';

const props = defineProps(['options']);
const displayOptions = computed(() => {
  if (props.options && props.options.length) return props.options;
  const minYear = (activeDocument.value?.code === '2629' || activeDocument.value?.startYear === 2025) ? 2025 : 2026;
  return getReportYearOptions(minYear);
});
</script>
