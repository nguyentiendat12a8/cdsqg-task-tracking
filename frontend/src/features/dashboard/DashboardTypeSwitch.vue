<template>
  <div class="inline-flex shrink-0 gap-1 rounded-lg border border-blue-200 bg-white p-1" role="group" :aria-label="label">
    <button v-for="option in options" :key="option.value" type="button" :aria-pressed="model===option.value" @click="model=option.value" class="rounded-md px-3 py-1.5 text-xs font-semibold" :class="model===option.value?'bg-blue-700 text-white shadow-sm':'text-slate-700 hover:bg-blue-50'">{{ option.label }}</button>
  </div>
</template>
<script setup>
const model=defineModel({default:'tasks'});
defineProps({label:{type:String,required:true}});
const options=[{value:'tasks',label:'Nhiệm vụ'},{value:'goals',label:'Mục tiêu'}];
</script>
