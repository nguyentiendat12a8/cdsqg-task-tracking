<template>
  <div>
    <!-- LOGIN VIEW (UNAUTHENTICATED SCREEN) -->
    <LoginView v-if="!authState.isLoggedIn.value" @loggedIn="onLoggedIn" />

    <!-- MAIN SYSTEM INTERFACE (AUTHENTICATED) -->
    <div v-else class="min-h-screen bg-slate-100/70 text-slate-900 font-sans w-full">
      
      <!-- Top Navigation Navbar (Executive Navy Blue Theme) -->
      <nav class="bg-slate-900 text-white border-b border-slate-800 sticky top-0 z-40 shadow-md w-full">
        <div class="w-full px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between gap-4">
          
          <!-- System Logo & Title -->
          <div class="flex items-center gap-3 cursor-pointer shrink-0" @click="switchTab('dashboard')">
            <div class="w-9 h-9 bg-blue-600 rounded-xl flex items-center justify-center text-white shadow-sm font-black text-lg border border-blue-400/30">
              CĐS
            </div>
            <div class="hidden sm:block">
              <h1 class="text-base font-extrabold tracking-tight text-white leading-snug">Hệ Thống Theo Dõi Nhiệm Vụ CĐS Quốc Gia</h1>
              <p class="text-[11px] text-slate-300 font-medium">National Digital Transformation Task Tracking System</p>
            </div>
          </div>

          <!-- Right Controls: Navigation Tabs + Logged-in User Profile Badge -->
          <div class="flex items-center gap-3 overflow-x-auto custom-scrollbar py-1">
            <div class="flex items-center bg-slate-800/90 p-1 rounded-xl border border-slate-700/80 shrink-0">
              <button 
                @click="switchTab('dashboard')" 
                :class="['px-3.5 py-1.5 rounded-lg text-xs font-extrabold transition flex items-center gap-1.5 whitespace-nowrap cursor-pointer', currentTab === 'dashboard' ? 'bg-blue-600 text-white shadow-sm' : 'text-slate-300 hover:text-white hover:bg-slate-700/60']"
              >
                📊 Dashboard
              </button>

              <button 
                @click="openDocumentsTab" 
                :class="['px-3.5 py-1.5 rounded-lg text-xs font-extrabold transition flex items-center gap-1.5 whitespace-nowrap cursor-pointer', currentTab === 'documents' ? 'bg-blue-600 text-white shadow-sm' : 'text-slate-300 hover:text-white hover:bg-slate-700/60']"
              >
                📜 Văn Bản Nhiệm Vụ
              </button>

              <button 
                @click="switchTab('urge-history')" 
                :class="['px-3.5 py-1.5 rounded-lg text-xs font-extrabold transition flex items-center gap-1.5 whitespace-nowrap cursor-pointer', currentTab === 'urge-history' ? 'bg-blue-600 text-white shadow-sm' : 'text-slate-300 hover:text-white hover:bg-slate-700/60']"
              >
                ⚡ Lịch Sử Đôn Đốc
              </button>

              <button 
                @click="switchTab('masterdata')" 
                :class="['px-3.5 py-1.5 rounded-lg text-xs font-extrabold transition flex items-center gap-1.5 whitespace-nowrap cursor-pointer', currentTab === 'masterdata' ? 'bg-blue-600 text-white shadow-sm' : 'text-slate-300 hover:text-white hover:bg-slate-700/60']"
              >
                ⚙️ Cài Đặt Chung
              </button>
            </div>

            <!-- User Profile Avatar Badge & Logout -->
            <div class="flex items-center gap-2 bg-slate-800 border border-slate-700 p-1.5 pl-3 rounded-xl shrink-0">
              <div class="flex flex-col text-right pr-1">
                <span class="text-xs font-extrabold text-white leading-tight max-w-[140px] truncate">
                  {{ authState.user.value?.fullName || authState.user.value?.username }}
                </span>
                <span :class="['text-[10px] font-extrabold uppercase tracking-wider', authState.isAdmin.value ? 'text-amber-400' : 'text-blue-300']">
                  {{ authState.isAdmin.value ? '👑 Admin' : '👤 Cán bộ' }}
                </span>
              </div>

              <button 
                @click="handleLogout"
                class="p-2 text-slate-400 hover:text-rose-400 hover:bg-slate-700 rounded-lg transition cursor-pointer"
                title="Đăng xuất khỏi hệ thống"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/></svg>
              </button>
            </div>

          </div>
        </div>
      </nav>

      <!-- Dynamic View Container (Full Width Responsive) -->
      <main class="w-full px-4 sm:px-6 lg:px-8 py-6">
        <ExecutiveDashboard 
          v-if="currentTab === 'dashboard'" 
          @openProgressModal="openProgressModalForTask" 
        />

        <div v-else-if="currentTab === 'documents'" class="w-full">
          <DocumentListView 
            v-if="!selectedDocumentId" 
            @selectDocument="onSelectDocument" 
            @openLlmImport="isLlmImportOpen = true"
          />
          <DocumentDetailView 
            v-else 
            :documentId="selectedDocumentId" 
            @back="selectedDocumentId = null" 
          />
        </div>

        <UrgeHistoryView v-else-if="currentTab === 'urge-history'" />
        <MasterDataView v-else-if="currentTab === 'masterdata'" />
        <UserManagementView v-else-if="currentTab === 'users'" />
      </main>

      <!-- Global Modals -->
      <ProgressUpdateModal 
        v-if="selectedTask"
        :isOpen="isProgressModalOpen"
        :taskId="selectedTask.id || selectedTask.taskId || '33333333-3333-3333-3333-333333333333'"
        :taskCode="selectedTask.code"
        :taskTitle="selectedTask.title"
        :evaluationType="selectedTask.evaluationType || 'Quantitative'"
        @close="isProgressModalOpen = false"
        @submitted="handleProgressSubmitted"
      />

      <LLMImportModal 
        :isOpen="isLlmImportOpen"
        @close="isLlmImportOpen = false"
        @imported="onLlmImported"
      />

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue';
import LoginView from './views/LoginView.vue';
import ExecutiveDashboard from './views/ExecutiveDashboard.vue';
import DocumentListView from './views/DocumentListView.vue';
import DocumentDetailView from './views/DocumentDetailView.vue';
import UrgeHistoryView from './views/UrgeHistoryView.vue';
import MasterDataView from './views/MasterDataView.vue';
import UserManagementView from './views/UserManagementView.vue';
import ProgressUpdateModal from './components/ProgressUpdateModal.vue';
import LLMImportModal from './components/LLMImportModal.vue';
import { authState, logout } from './services/auth';

const currentTab = ref('dashboard');
const selectedDocumentId = ref(null);
const isProgressModalOpen = ref(false);
const isLlmImportOpen = ref(false);
const selectedTask = ref(null);

function handleLogout() {
  logout();
  currentTab.value = 'dashboard';
  selectedDocumentId.value = null;
}

function onLoggedIn() {
  currentTab.value = 'dashboard';
}

function parseHashRoute() {
  const hash = window.location.hash || '#dashboard';
  const rawPath = hash.replace(/^#\/?/, '');
  const [route, queryStr] = rawPath.split('?');
  
  if (route === 'documents') {
    currentTab.value = 'documents';
    if (queryStr) {
      const params = new URLSearchParams(queryStr);
      selectedDocumentId.value = params.get('id') || null;
    }
  } else if (route === 'urge-history') {
    currentTab.value = 'urge-history';
    selectedDocumentId.value = null;
  } else if (route === 'masterdata' || route === 'master-data') {
    currentTab.value = 'masterdata';
    selectedDocumentId.value = null;
  } else if (route === 'users' || route === 'user-management') {
    currentTab.value = 'users';
    selectedDocumentId.value = null;
  } else {
    currentTab.value = 'dashboard';
    selectedDocumentId.value = null;
  }
}

function syncHashRoute() {
  if (!authState.isLoggedIn.value) return;
  let targetHash = `#${currentTab.value}`;
  if (currentTab.value === 'documents' && selectedDocumentId.value) {
    targetHash += `?id=${selectedDocumentId.value}`;
  }
  if (window.location.hash !== targetHash) {
    window.location.hash = targetHash;
  }
}

function switchTab(tabName) {
  currentTab.value = tabName;
  if (tabName !== 'documents') {
    selectedDocumentId.value = null;
  }
}

function openDocumentsTab() {
  currentTab.value = 'documents';
  selectedDocumentId.value = null;
}

function onSelectDocument(doc) {
  selectedDocumentId.value = doc.id;
  currentTab.value = 'documents';
}

watch([currentTab, selectedDocumentId], () => {
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
  selectedTask.value = task;
  isProgressModalOpen.value = true;
}

function handleProgressSubmitted(data) {
  console.log('Progress log submitted:', data);
}

function onLlmImported(result) {
  console.log('LLM Import result:', result);
}
</script>
