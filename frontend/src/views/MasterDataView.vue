<template>
  <div class="w-full font-sans">
    <AgencyManagement v-if="currentSubTab === 'agencies'" />
    <UnitManagement v-else-if="currentSubTab === 'units'" />
    <ImportHistoryAudit v-else-if="currentSubTab === 'imports' || currentSubTab === 'import-history'" />
    <UserManagementView v-else-if="currentSubTab === 'users'" />
    <AgencyManagement v-else />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import AgencyManagement from '../components/AgencyManagement.vue';
import UnitManagement from '../components/UnitManagement.vue';
import ImportHistoryAudit from '../components/ImportHistoryAudit.vue';
import UserManagementView from './UserManagementView.vue';

const props = defineProps({
  subTab: { type: String, default: 'agencies' }
});

const currentSubTab = ref(props.subTab || 'agencies');

watch(() => props.subTab, (newVal) => {
  if (newVal) {
    currentSubTab.value = newVal;
  }
}, { immediate: true });
</script>
