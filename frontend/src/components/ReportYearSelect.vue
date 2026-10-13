<template>
  <div class="w-36 shrink-0">
    <SearchableSelect v-model="reportYear" :options="reportYearOptions" :isMulti="false" :clearable="false" placeholder="Năm báo cáo" aria-label="Năm báo cáo" />
  </div>
</template>
<script setup>
import SearchableSelect from './SearchableSelect.vue';
import { reportYear, reportYearOptions } from '../shared/reportYear';
</script>
