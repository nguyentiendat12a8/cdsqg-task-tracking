<template>
  <button type="button" :class="['ui-btn shrink-0', kind === 'create' ? 'ui-btn-primary' : 'ui-btn-secondary']">
    <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
      <path v-if="kind === 'create'" d="M12 5v14M5 12h14" stroke-width="2" stroke-linecap="round" />
      <template v-else>
        <path d="M14 3H5a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2V10zM14 3v7h7" stroke-width="1.8" stroke-linejoin="round" />
        <path :d="kind === 'import' ? 'M12 18v-6m-3 3l3-3 3 3' : 'M12 12v6m-3-3l3 3 3-3'" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
      </template>
    </svg>
    <span><slot /></span>
  </button>
</template>

<script setup>
defineProps({ kind: { type: String, default: 'export', validator: value => ['export', 'import', 'create'].includes(value) } });
</script>
