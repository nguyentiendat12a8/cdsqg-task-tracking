<template>
  <div 
    :class="[
      'relative h-full shrink-0 z-50 transition-all duration-300 ease-in-out',
      isManualCollapsed ? 'w-20' : 'w-64'
    ]"
    @mouseenter="onMouseEnter"
    @mouseleave="onMouseLeave"
  >
    <aside 
      :class="[
        'bg-slate-900 text-slate-300 transition-all duration-300 ease-in-out flex flex-col border-r border-slate-800 h-full overflow-hidden select-none',
        isManualCollapsed ? 'absolute left-0 top-0 z-50 shadow-2xl' : 'relative z-50 shadow-xl',
        isCollapsed ? 'w-20' : 'w-64'
      ]"
    >
      <!-- Brand / System Header -->
      <div class="h-16 flex items-center border-b border-slate-800 shrink-0 px-3 relative overflow-hidden">
        <!-- WHEN EXPANDED (or Hover-Expanded) -->
        <div v-if="!isCollapsed" class="flex items-center justify-between w-full min-w-[230px]">
          <div class="flex items-center gap-3 overflow-hidden">
            <!-- Official Cục Chuyển đổi số Logo Image -->
            <img src="/logo-cds.jpg" alt="Logo Cục Chuyển đổi số" class="w-10 h-10 rounded-xl object-cover shrink-0 shadow-lg border border-slate-700/50 bg-white" />
            <div class="flex flex-col whitespace-nowrap overflow-hidden">
              <span class="font-bold text-white text-xs tracking-wider">CĐS QUỐC GIA</span>
              <span class="text-[10px] text-blue-400 font-bold">Quyết định 1266/QĐ-TTg</span>
            </div>
          </div>

          <button 
            @click.stop="toggleCollapse"
            class="w-7 h-7 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white flex items-center justify-center transition shrink-0 cursor-pointer"
            title="Thu gọn Menu"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
            </svg>
          </button>
        </div>

        <!-- WHEN COLLAPSED -->
        <div v-else class="flex items-center justify-center w-full">
          <button 
            @click.stop="toggleCollapse"
            class="w-10 h-10 rounded-xl overflow-hidden shrink-0 shadow-md border border-slate-700/50 hover:scale-105 transition transform cursor-pointer p-0.5 bg-white"
            title="Mở rộng Menu"
          >
            <img src="/logo-cds.jpg" alt="Logo Cục Chuyển đổi số" class="w-full h-full object-cover rounded-lg" />
          </button>
        </div>
      </div>

      <!-- Main Navigation Items -->
      <nav class="flex-1 py-4 px-2.5 space-y-1.5 overflow-y-auto custom-scrollbar">
        
        <!-- Trang chủ (Dashboard) -->
        <button 
          @click="selectTab('dashboard')"
          :class="[
            'w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            activeTab === 'dashboard' ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
          ]"
          :title="isCollapsed ? 'Trang Chủ' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/></svg>
          <span v-if="!isCollapsed" class="truncate">Trang Chủ</span>
        </button>

        <!-- Submenu: Mục tiêu (Goals) -->
        <div>
          <button 
            @click="handleGoalsClick"
            :class="[
              'w-full flex items-center justify-between py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
              isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
              activeTab.startsWith('goals') ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
            ]"
            :title="isCollapsed ? 'Mục Tiêu' : ''"
          >
            <div class="flex items-center gap-3 truncate">
              <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
              <span v-if="!isCollapsed" class="truncate">Mục Tiêu</span>
            </div>
            <svg v-if="!isCollapsed" class="w-4 h-4 transition-transform shrink-0" :class="{ 'rotate-180': isGoalsOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/></svg>
          </button>

          <div v-if="isGoalsOpen && !isCollapsed" class="pl-9 pr-2 py-1.5 space-y-1">
            <button 
              @click="selectTab('goals-list')"
              :class="['w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', ['goals', 'goals-list'].includes(activeTab) ? 'text-blue-400 bg-slate-800' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50']"
            >
              Danh sách mục tiêu
            </button>
            <button 
              v-if="authState.isAdmin.value"
              @click="selectTab('goals-grid')"
              :class="['w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'goals-grid' ? 'text-blue-400 bg-slate-800' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50']"
            >
              Thiết lập kế hoạch mục tiêu
            </button>
          </div>
        </div>

        <!-- Nhiệm vụ (Tasks) -->
        <button 
          @click="selectTab('tasks-list')"
          :class="[
            'w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            ['tasks', 'tasks-list', 'tasks-grid'].includes(activeTab) ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
          ]"
          :title="isCollapsed ? 'Nhiệm Vụ' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg>
          <span v-if="!isCollapsed" class="truncate">Nhiệm Vụ</span>
        </button>

        <!-- Báo cáo (Reports) - Admin only -->
        <button 
          v-if="authState.isAdmin.value"
          @click="selectTab('reports')"
          :class="[
            'w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            activeTab === 'reports' ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
          ]"
          :title="isCollapsed ? 'Báo Cáo' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          <span v-if="!isCollapsed" class="truncate">Báo Cáo</span>
        </button>

        <!-- Văn bản QPPL -->
        <button 
          @click="selectTab('legal-documents')"
          :class="[
            'w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            activeTab === 'legal-documents' ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
          ]"
          :title="isCollapsed ? 'Văn bản QPPL' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
          </svg>
          <span v-if="!isCollapsed" class="truncate">Văn bản QPPL</span>
        </button>

        <!-- Đầu mối liên hệ và KH CĐS -->
        <button 
          @click="selectTab('agency-plans')"
          :class="[
            'w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            activeTab === 'agency-plans' ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
          ]"
          :title="isCollapsed ? 'Đầu mối liên hệ và KH CĐS' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5m0 0h4m-4 0a2 2 0 01-2-2V7a2 2 0 012-2h4a2 2 0 012 2v12a2 2 0 01-2 2" />
          </svg>
          <span v-if="!isCollapsed" class="truncate">Đầu mối liên hệ và KH CĐS</span>
        </button>

        <!-- Submenu: Thiết lập chung (Settings) -->
        <div class="pt-2">
          <button 
            @click="handleSettingsClick"
            :class="[
              'w-full flex items-center justify-between py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
              isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
              ['agencies', 'units', 'users', 'import-history', 'settings', 'master-data'].includes(activeTab) ? 'bg-slate-800 text-blue-400' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
            ]"
            :title="isCollapsed ? 'Thiết Lập Chung' : ''"
          >
            <div class="flex items-center gap-3 truncate">
              <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>
              <span v-if="!isCollapsed" class="truncate">Thiết Lập Chung</span>
            </div>
            <svg v-if="!isCollapsed" class="w-4 h-4 transition-transform shrink-0" :class="{ 'rotate-180': isSettingsOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/></svg>
          </button>

          <!-- Submenu options -->
          <div v-if="isSettingsOpen && !isCollapsed" class="pl-9 pr-2 py-1.5 space-y-1">
            <button 
              v-if="authState.isAdmin.value"
              @click="selectTab('agencies')"
              :class="['w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'agencies' ? 'text-blue-400 bg-slate-800' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50']"
            >
              Danh mục Cơ quan
            </button>
            <button 
              v-if="false"
              @click="selectTab('units')"
              :class="['w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'units' ? 'text-blue-400 bg-slate-800' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50']"
            >
              Danh mục Đơn vị tính
            </button>
            <button 
              v-if="authState.isAdmin.value"
              @click="selectTab('users')"
              :class="['w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'users' ? 'text-blue-400 bg-slate-800' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50']"
            >
              Quản lý Tài khoản
            </button>
            <button 
              @click="selectTab('import-history')"
              :class="['w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'import-history' ? 'text-blue-400 bg-slate-800' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50']"
            >
              Lịch sử Nạp Dữ liệu File
            </button>
          </div>
        </div>

      </nav>

      <!-- Footer user role badge -->
      <div v-if="!isCollapsed" class="p-4 border-t border-slate-800 text-xs text-slate-500 font-medium whitespace-nowrap overflow-hidden">
        <div>Đơn vị vận hành</div>
        <div class="text-slate-300 font-bold truncate">Phòng Chính Sách Số - Cục CĐSQG</div>
      </div>
    </aside>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { authState } from '../services/auth';

const props = defineProps({
  activeTab: { type: String, default: 'dashboard' }
});

const emit = defineEmits(['navigate']);

const isManualCollapsed = ref(true);
const isHovered = ref(false);
let hoverTimer = null;

const isGoalsOpen = ref(props.activeTab.startsWith('goals'));
const isSettingsOpen = ref(['agencies', 'units', 'users', 'import-history'].includes(props.activeTab));

const isCollapsed = computed(() => {
  return isManualCollapsed.value && !isHovered.value;
});

watch(() => props.activeTab, (newTab) => {
  if (newTab.startsWith('goals')) isGoalsOpen.value = true;
  if (['agencies', 'units', 'users', 'import-history'].includes(newTab)) isSettingsOpen.value = true;
}, { immediate: true });

function toggleCollapse() {
  if (hoverTimer) clearTimeout(hoverTimer);
  isManualCollapsed.value = !isManualCollapsed.value;
  isHovered.value = false;
}

function onMouseEnter() {
  if (!isManualCollapsed.value) return;
  if (hoverTimer) clearTimeout(hoverTimer);
  isHovered.value = true;
}

function onMouseLeave() {
  if (!isManualCollapsed.value) return;
  if (hoverTimer) clearTimeout(hoverTimer);
  hoverTimer = setTimeout(() => {
    isHovered.value = false;
  }, 150);
}

function handleGoalsClick() {
  isGoalsOpen.value = !isGoalsOpen.value;
}

function handleSettingsClick() {
  isSettingsOpen.value = !isSettingsOpen.value;
}

function selectTab(tab) {
  if (hoverTimer) clearTimeout(hoverTimer);
  emit('navigate', tab);
  if (isManualCollapsed.value) {
    isHovered.value = false;
  }
}
</script>
