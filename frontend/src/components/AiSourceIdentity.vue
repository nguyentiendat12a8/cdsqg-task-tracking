<template>
 <div class="space-y-1 mt-2">
  <p class="ai-input-code" v-if="code">{{ code }}</p>
  <p v-if="!isUpload" class="ai-muted">{{ source.inputType==='rss'?'Google Alerts':'Nguồn web' }}<span v-if="host"> · {{ host }}</span></p>
  <p v-if="source.feedName" class="ai-muted">Nguồn Alert: {{ source.feedName }}</p>
  <p v-if="detailed && source.readAt" class="ai-muted">Lần đọc: {{ formatDateTime(source.readAt) }}</p>
 </div>
</template>
<script setup>
import {computed} from 'vue';
import {aiInputCode} from '../shared/aiInputCode';
import {formatDateTime} from '../shared/formatters';
const props=defineProps({source:{type:Object,required:true},detailed:Boolean});
const isUpload=computed(()=>props.source.inputType==='upload'||props.source.sourceUrl?.startsWith('/api/ai-inbox/'));
const code=computed(()=>aiInputCode(props.source.inputSequence,props.source.inputReceivedAt,props.source.inputType==='upload'));
const host=computed(()=>{try{return new URL(props.source.sourceUrl).hostname;}catch{return '';}});
</script>
