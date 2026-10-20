<template>
  <header class="ai-page-header">
    <div class="ai-heading-row">
      <div class="flex items-center gap-3 min-w-0">
        <span class="ai-page-icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M9 3h6v4H9zM5 9h14v12H5zM9 13h.01M15 13h.01M9 17h6M1 12h4m14 0h4" stroke-linecap="round" stroke-linejoin="round" /></svg></span>
        <h2 class="ui-page-title">{{ title }}</h2>
      </div>
      <div v-if="$slots.actions" class="ai-page-actions"><slot name="actions" /></div>
    </div>
    <div v-if="$slots.tabs" class="ai-header-tabs"><slot name="tabs" /></div>
  </header>
</template>
<script setup>
import '../features/ai/ai.css';
defineProps({title:{type:String,required:true}});
</script>
