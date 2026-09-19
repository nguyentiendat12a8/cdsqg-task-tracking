<template>
  <div class="h-screen w-screen bg-slate-100 text-slate-900 font-sans flex flex-col antialiased overflow-hidden">
    <!-- Global Confirmation Modal -->
    <ConfirmModal />

    <!-- LOGIN VIEW (UNAUTHENTICATED SCREEN) -->
    <LoginView v-if="!authState.isLoggedIn.value" @loggedIn="onLoggedIn" />

    <!-- MAIN SYSTEM INTERFACE (AUTHENTICATED SCREEN WITH LEFT SIDEBAR) -->
    <div v-else class="flex flex-1 h-full w-full overflow-hidden">
      
      <!-- Collapsible Left Navigation Sidebar -->
      <AppSidebar 
        :activeTab="currentTab" 
        @navigate="switchTab" 
        class="shrink-0 h-full sticky top-0 z-40"
      />

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col h-full overflow-hidden bg-slate-100 min-w-0">
        
        <!-- Fixed Top Header -->
        <AppHeader 
          :user="authState.user.value" 
          @logout="handleLogout" 
          class="shrink-0 sticky top-0 z-30"
        />

        <!-- Scrollable Dynamic View Content -->
        <main class="flex-1 overflow-y-auto p-3 sm:p-4 custom-scrollbar">
          
          <!-- Trang chủ (Executive / Agency User Dashboard) -->
          <ExecutiveDashboard 
            v-if="currentTab === 'dashboard'" 
            @openProgressModal="openProgressModalForTask" 
          />

          <!-- Mục tiêu QĐ 1266 (Goals View) -->
          <DocumentDetailView 
            v-else-if="['goals', 'goals-list', 'goals-grid'].includes(currentTab)" 
            filterItemType="Goal"
            :subTab="currentTab === 'goals-grid' ? 'grid' : 'list'"
          />

          <!-- Nhiệm vụ QĐ 1266 (Tasks View) -->
          <DocumentDetailView 
            v-else-if="['tasks', 'tasks-list', 'tasks-grid'].includes(currentTab)" 
            filterItemType="Task"
            subTab="list"
          />

          <!-- Báo cáo - Xuất Excel (Executive Reports Center) -->
          <ExecutiveReportsView v-else-if="currentTab === 'reports'" />

          <!-- Thiết lập chung (Submenus) -->
          <MasterDataView v-else-if="['settings', 'master-data'].includes(currentTab)" />
          <AgencyManagement v-else-if="currentTab === 'agencies'" />
          <UnitManagement v-else-if="currentTab === 'units'" />
          <UserManagementView v-else-if="currentTab === 'users'" />
          <ImportHistoryAudit v-else-if="currentTab === 'import-history'" />

        </main>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue';
import LoginView from './views/LoginView.vue';
import AppSidebar from './components/AppSidebar.vue';
import AppHeader from './components/AppHeader.vue';
import ExecutiveDashboard from './views/ExecutiveDashboard.vue';
import DocumentDetailView from './views/DocumentDetailView.vue';
import ExecutiveReportsView from './views/ExecutiveReportsView.vue';
import MasterDataView from './views/MasterDataView.vue';
import AgencyManagement from './components/AgencyManagement.vue';
import UnitManagement from './components/UnitManagement.vue';
import UserManagementView from './views/UserManagementView.vue';
import ImportHistoryAudit from './components/ImportHistoryAudit.vue';
import ConfirmModal from './components/ConfirmModal.vue';
import { authState, logout } from './services/auth';

const currentTab = ref('dashboard');

function handleLogout() {
  logout();
  currentTab.value = 'dashboard';
}

function onLoggedIn() {
  currentTab.value = 'dashboard';
}

function parseHashRoute() {
  const hash = window.location.hash || '#dashboard';
  const rawPath = hash.replace(/^#\/?/, '');
  const [route] = rawPath.split('?');

  if (['goals', 'goals-list', 'goals-grid', 'tasks', 'tasks-list', 'tasks-grid', 'reports', 'agencies', 'units', 'users', 'import-history', 'settings', 'master-data'].includes(route)) {
    currentTab.value = route;
  } else {
    currentTab.value = 'dashboard';
  }
}

function syncHashRoute() {
  if (!authState.isLoggedIn.value) return;
  const targetHash = `#${currentTab.value}`;
  if (window.location.hash !== targetHash) {
    window.location.hash = targetHash;
  }
}

function switchTab(tabName) {
  currentTab.value = tabName;
}

watch([currentTab], () => {
  syncHashRoute();
});

onMounted(() => {
  parseHashRoute();
  window.addEventListener('hashchange', parseHashRoute);
});

onUnmounted(() => {
  window.removeEventListener('hashchange', parseHashRoute);
});

function openProgressModalForTask(task) {
  console.log('Opening progress modal for task:', task);
}
</script>
