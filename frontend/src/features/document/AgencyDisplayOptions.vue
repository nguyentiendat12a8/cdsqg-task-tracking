<template>
  <details class="group">
    <summary class="ui-control flex items-center justify-between gap-3 cursor-pointer list-none rounded-lg border border-slate-200 bg-white px-3 text-sm font-semibold text-slate-700 hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 [&::-webkit-details-marker]:hidden">
      <span>Tùy chỉnh tên hiển thị đơn vị chủ trì, phối hợp</span>
      <svg class="h-4 w-4 shrink-0 text-slate-500 transition-transform group-open:rotate-180" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="m6 9 6 6 6-6" /></svg>
    </summary>
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 pt-3">
      <label class="block text-xs font-semibold text-slate-700">
        Tên hiển thị đơn vị chủ trì
        <input v-model="leadText" type="text" placeholder="Nhập tên hiển thị đơn vị chủ trì" class="ui-control mt-1 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-normal focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </label>
      <label class="block text-xs font-semibold text-slate-700">
        Tên hiển thị cơ quan phối hợp
        <input v-model="coordinatingText" type="text" placeholder="Nhập tên hiển thị cơ quan phối hợp" class="ui-control mt-1 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm font-normal focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </label>
    </div>
  </details>
</template>

<script setup>
const leadText = defineModel('leadText');
const coordinatingText = defineModel('coordinatingText');
</script>
