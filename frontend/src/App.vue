<template>
  <div class="h-dvh w-full bg-slate-100 text-slate-900 font-sans flex flex-col antialiased overflow-hidden">
    <!-- Global Confirmation Modal -->
    <ConfirmModal />

    <!-- LOGIN VIEW (UNAUTHENTICATED SCREEN) -->
    <ResetPasswordView v-if="isResetRoute" />
    <LoginView v-else-if="!authState.isLoggedIn.value" @loggedIn="onLoggedIn" />

    <!-- MAIN SYSTEM INTERFACE (AUTHENTICATED SCREEN WITH LEFT SIDEBAR) -->
    <div v-else class="flex flex-1 h-full w-full overflow-hidden">
      <button v-if="mobileMenuOpen" class="fixed inset-0 top-16 z-40 bg-slate-900/40 md:hidden" aria-label="Đóng menu" @click="mobileMenuOpen = false"></button>
      
      <!-- Collapsible Left Navigation Sidebar -->
      <AppSidebar 
        :activeTab="currentTab" 
        @navigate="switchTab" 
        :mobile-open="mobileMenuOpen" @close="mobileMenuOpen = false" class="shrink-0 h-full z-50"
      />

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col h-full overflow-hidden bg-slate-100 min-w-0">
        
        <!-- Fixed Top Header -->
        <AppHeader 
          :user="authState.user.value" 
          @logout="handleLogout" @toggle-menu="mobileMenuOpen = !mobileMenuOpen" :menu-open="mobileMenuOpen"
          class="shrink-0 sticky top-0 z-40"
        />

        <!-- Scrollable Dynamic View Content -->
        <ApiFeedback />
        <main v-accessible-data id="main-content" tabindex="-1" class="flex-1 overflow-y-auto px-4 sm:px-6 pb-6 custom-scrollbar" style="padding-top: 16px;">
          
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

          <!-- Văn bản Quy Phạm Pháp Luật (VB QPPL) -->
          <LegalDocumentsView v-else-if="currentTab === 'legal-documents'" />

          <!-- Thông tin Kế hoạch & Đầu mối của các đơn vị -->
          <AgencyPlansView v-else-if="currentTab === 'agency-plans'" />

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
import { fetchWithAuth } from './services/auth';

import { ref, onMounted, onUnmounted, watch, defineAsyncComponent } from 'vue';
import LoginView from './views/LoginView.vue';
import ResetPasswordView from './views/ResetPasswordView.vue';
import ApiFeedback from './components/ApiFeedback.vue';
import AppSidebar from './components/AppSidebar.vue';
import AppHeader from './components/AppHeader.vue';
const ExecutiveDashboard = defineAsyncComponent(() => import('./views/ExecutiveDashboard.vue'));
const DocumentDetailView = defineAsyncComponent(() => import('./views/DocumentDetailView.vue'));
const ExecutiveReportsView = defineAsyncComponent(() => import('./views/ExecutiveReportsView.vue'));
const LegalDocumentsView = defineAsyncComponent(() => import('./views/LegalDocumentsView.vue'));
const AgencyPlansView = defineAsyncComponent(() => import('./views/AgencyPlansView.vue'));
const MasterDataView = defineAsyncComponent(() => import('./views/MasterDataView.vue'));
const AgencyManagement = defineAsyncComponent(() => import('./components/AgencyManagement.vue'));
const UnitManagement = defineAsyncComponent(() => import('./components/UnitManagement.vue'));
const UserManagementView = defineAsyncComponent(() => import('./views/UserManagementView.vue'));
const ImportHistoryAudit = defineAsyncComponent(() => import('./components/ImportHistoryAudit.vue'));
import ConfirmModal from './components/ConfirmModal.vue';
import { authState, logout } from './services/auth';
import { getApiUrl } from './config/api';

const mobileMenuOpen = ref(false);
const isResetRoute = ref(window.location.hash.startsWith('#reset-password'));
const currentTab = ref('dashboard');

function handleLogout() {
  logout();
  currentTab.value = 'dashboard';
}

function onLoggedIn() {
  currentTab.value = 'dashboard';
}

function parseHashRoute() {
  isResetRoute.value = window.location.hash.startsWith('#reset-password');
  if (isResetRoute.value) return;
  const hash = window.location.hash || '#dashboard';
  const rawPath = hash.replace(/^#\/?/, '');
  const [route] = rawPath.split('?');

  const adminOnlyTabs = ['reports', 'agencies', 'units', 'users', 'goals-grid', 'master-data', 'settings'];
  if (!authState.isAdmin.value && adminOnlyTabs.includes(route)) {
    if (route === 'goals-grid') currentTab.value = 'goals-list';
    else if (['agencies', 'units', 'users', 'settings', 'master-data'].includes(route)) currentTab.value = 'import-history';
    else currentTab.value = 'dashboard';
    return;
  }

  if (['goals', 'goals-list', 'goals-grid', 'tasks', 'tasks-list', 'tasks-grid', 'reports', 'legal-documents', 'agency-plans', 'agencies', 'units', 'users', 'import-history', 'settings', 'master-data'].includes(route)) {
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
  mobileMenuOpen.value = false;
  const adminOnlyTabs = ['reports', 'agencies', 'units', 'users', 'goals-grid', 'master-data', 'settings'];
  if (!authState.isAdmin.value && adminOnlyTabs.includes(tabName)) {
    if (tabName === 'goals-grid') currentTab.value = 'goals-list';
    else if (['agencies', 'units', 'users', 'settings', 'master-data'].includes(tabName)) currentTab.value = 'import-history';
    else currentTab.value = 'dashboard';
    return;
  }
  currentTab.value = tabName;
}

watch([currentTab], () => {
  syncHashRoute();
});

async function handleOpenNotificationDetail(event) {
  const notif = event.detail;
  if (!notif) return;

  const linkUrl = notif.linkUrl || notif.LinkUrl || '';
  const notifType = notif.type || notif.Type || '';
  const notifTitle = notif.title || notif.Title || '';
  const notifMsg = notif.message || notif.Message || '';

  const isApprovalType = notifType === 'PROGRESS_APPROVAL' ||
                         notifTitle.toLowerCase().includes('chờ duyệt') ||
                         notifTitle.toLowerCase().includes('phê duyệt') ||
                         notifMsg.toLowerCase().includes('phê duyệt');

  if (isApprovalType && authState.isAdmin.value) {
    const isGoal = notifMsg.includes('MT-') || notifTitle.includes('MT-');
    currentTab.value = isGoal ? 'goals' : 'tasks';

    setTimeout(() => {
      window.dispatchEvent(new CustomEvent('open-pending-approvals-modal'));
    }, 150);
    return;
  }

  const guidMatch = linkUrl.match(/(?:taskId|goalId|itemId)=([a-f0-9-]+)/i) || linkUrl.match(/([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})/i);
  const targetId = guidMatch ? guidMatch[1] : null;

  const titleAndMsg = notifTitle + ' ' + notifMsg;
  const codeMatch = titleAndMsg.match(/(NV-?\d+|MT-?\d+|SUB-?\d+)/i);
  const targetCode = codeMatch ? codeMatch[1].toUpperCase() : null;

  const tabParamMatch = linkUrl.match(/[?&](?:tab|initialTab)=([^&]+)/i);
  let targetTab = tabParamMatch ? tabParamMatch[1] : null;

  if (!targetTab) {
    if (notifType.toUpperCase().includes('PROGRESS')) {
      targetTab = 'reports';
    } else {
      targetTab = 'notifications';
    }
  }

  try {
    const docId = '12660000-0000-0000-0000-000000001266';
    const currentAgencyId = authState.user.value?.agencyId || '';
    const agencyParam = currentAgencyId ? `?agencyId=${currentAgencyId}` : '';
    const res = await fetchWithAuth(getApiUrl(`/api/planning/documents/${docId}/grid${agencyParam}`));
    if (!res.ok) return;

    const data = await res.json();
    const allItems = data.items || [];

    let targetItem = null;
    if (targetId) {
      for (const item of allItems) {
        if (item.id === targetId || item.taskId === targetId) {
          targetItem = item;
          break;
        }
      }
    }

    if (!targetItem && targetCode) {
      for (const item of allItems) {
        if (item.code && item.code.toUpperCase() === targetCode) {
          targetItem = item;
          break;
        }
      }
    }

    if (targetItem) {
      const isGoal = targetItem.itemType === 'Goal' || targetItem.itemType === 1 || targetItem.itemType === '1';
      currentTab.value = isGoal ? 'goals' : 'tasks';

      setTimeout(() => {
        window.dispatchEvent(new CustomEvent('open-target-item-detail', {
          detail: { item: targetItem, initialTab: targetTab }
        }));
      }, 150);
    }
  } catch (e) {
    console.error('Lỗi khi mở chi tiết thông báo:', e);
  }
}

onMounted(() => {
  parseHashRoute();
  window.addEventListener('hashchange', parseHashRoute);
  window.addEventListener('open-notification-detail', handleOpenNotificationDetail);
});

onUnmounted(() => {
  window.removeEventListener('hashchange', parseHashRoute);
  window.removeEventListener('open-notification-detail', handleOpenNotificationDetail);
});

function openProgressModalForTask(task) {
  console.log('Opening progress modal for task:', task);
}
</script>
