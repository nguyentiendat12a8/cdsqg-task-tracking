<template>
 <DocumentPagination v-model:current-page="page" v-model:page-size="pageSize" :total-count="total" :total-pages="Math.ceil(total/pageSize)" :page-size-options="sizes" :change-page="value=>page=value" :item-label="itemLabel" :disabled="disabled" :hide-page-size="fixed"/>
</template>
<script setup>
import DocumentPagination from '../features/document/DocumentPagination.vue';
defineProps({total:{type:Number,default:0},itemLabel:{type:String,default:'bản ghi'},disabled:Boolean,fixed:Boolean});
const page=defineModel('page',{required:true});
const pageSize=defineModel('pageSize',{required:true});
const sizes=[5,10,20,50].map(value=>({value,label:String(value)}));
</script>
