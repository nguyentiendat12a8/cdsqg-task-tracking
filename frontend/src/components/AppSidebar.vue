<template>
  <div 
    class="app-sidebar"
    :class="[
      { 'mobile-open': mobileOpen },
      'relative h-full shrink-0 z-50 transition-all duration-300 ease-in-out',
      isManualCollapsed ? 'w-20' : 'w-64'
    ]"
    @mouseenter="onMouseEnter"
    @mouseleave="onMouseLeave"
  >
    <aside 
      :class="[
        'app-navigation text-slate-300 transition-all duration-300 ease-in-out flex flex-col border-r border-slate-800 h-full overflow-hidden select-none',
        isManualCollapsed ? 'absolute left-0 top-0 z-50 shadow-2xl' : 'relative z-50 shadow-xl',
        isCollapsed ? 'w-20' : 'w-64'
      ]"
    >
      <!-- Brand / System Header -->
      <div class="nav-brand h-16 flex items-center border-b border-slate-800 shrink-0 px-3 relative overflow-hidden">
        <!-- WHEN EXPANDED (or Hover-Expanded) -->
        <div v-if="!isCollapsed" class="flex items-center justify-between w-full min-w-[230px]">
          <div class="flex items-center gap-3 overflow-hidden">
            <!-- Official Cục Chuyển đổi số Logo Image -->
            <img src="/logo-cds.jpg" alt="Logo Cục Chuyển đổi số" class="w-10 h-10 rounded-xl object-cover shrink-0 shadow-lg border border-slate-700/50 bg-white" />
            <div class="flex flex-col whitespace-nowrap overflow-hidden">
              <span class="font-bold text-white text-xs tracking-wider">CĐS QUỐC GIA</span>
              <span class="text-xs font-bold truncate" :class="activeTab === 'dashboard' && dashboardDocumentScope === 'all' || activeDocument.code === '1266' ? 'text-blue-400' : 'text-emerald-400'" :title="activeTab === 'dashboard' && dashboardDocumentScope === 'all' ? 'Chiến lược 1266 và Chương trình 2629' : activeDocument.title">
                {{ activeTab === 'dashboard' && dashboardDocumentScope === 'all' ? 'Tổng quan 1266 + 2629' : activeDocument.documentNumber }}
              </span>
            </div>
          </div>

          <button 
            @click.stop="toggleCollapse"
            :class="[
              'w-7 h-7 rounded-lg flex items-center justify-center transition-all duration-300 shrink-0 cursor-pointer border',
              !isManualCollapsed 
                ? 'bg-blue-900/40 border-blue-500/40 text-blue-400 hover:bg-blue-900/60 hover:text-blue-300 shadow-sm' 
                : 'bg-slate-800 border-slate-700/50 text-slate-400 hover:bg-slate-700 hover:text-white'
            ]"
            :title="!isManualCollapsed ? 'Đang ghim cố định Menu (Bấm để bỏ ghim)' : 'Bấm để ghim cố định Menu'"
          >
            <svg 
              class="w-4 h-4 transition-transform duration-300"
              :class="!isManualCollapsed ? '-rotate-90' : ''"
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
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
            'nav-link w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            activeTab === 'dashboard' ? 'is-active' : 'nav-idle'
          ]"
          :title="isCollapsed ? 'Trang chủ' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/></svg>
          <span v-if="!isCollapsed" class="truncate">Trang chủ</span>
        </button>

        <!-- Submenu: Mục tiêu (Goals) - Dropdown for Admin, Direct Button for Level 2/3 -->
        <div v-if="authState.isAdmin.value">
          <button 
            @click="handleGoalsClick"
            :class="[
              'nav-link w-full flex items-center justify-between py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
              isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
              activeTab.startsWith('goals') ? 'is-active' : 'nav-idle'
            ]"
            :title="isCollapsed ? 'Mục tiêu' : ''"
          >
            <div class="flex items-center gap-3 truncate">
              <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
              <span v-if="!isCollapsed" class="truncate">Mục tiêu</span>
            </div>
            <svg v-if="!isCollapsed" class="w-4 h-4 transition-transform shrink-0" :class="{ 'rotate-180': isGoalsOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/></svg>
          </button>

          <div v-if="isGoalsOpen && !isCollapsed" class="pl-9 pr-2 py-1.5 space-y-1">
            <button 
              @click="selectTab('goals-list')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', ['goals', 'goals-list'].includes(activeTab) ? 'is-active' : 'nav-idle']"
            >
              Danh sách mục tiêu
            </button>
            <button 
              @click="selectTab('goals-grid')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'goals-grid' ? 'is-active' : 'nav-idle']"
            >
              Thiết lập kế hoạch mục tiêu
            </button>
          </div>
        </div>

        <button 
          v-else
          @click="selectTab('goals-list')"
          :class="[
            'nav-link w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            ['goals', 'goals-list', 'goals-grid'].includes(activeTab) ? 'is-active' : 'nav-idle'
          ]"
          :title="isCollapsed ? 'Mục tiêu' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
          <span v-if="!isCollapsed" class="truncate">Mục tiêu</span>
        </button>

        <!-- Nhiệm vụ (Tasks) -->
        <button 
          @click="selectTab('tasks-list')"
          :class="[
            'nav-link w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            ['tasks', 'tasks-list', 'tasks-grid'].includes(activeTab) ? 'is-active' : 'nav-idle'
          ]"
          :title="isCollapsed ? 'Nhiệm vụ' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg>
          <span v-if="!isCollapsed" class="truncate">Nhiệm vụ</span>
        </button>

        <!-- Báo cáo (Reports) - Admin only -->
        <button 
          v-if="authState.isAdmin.value"
          @click="selectTab('reports')"
          :class="[
            'nav-link w-full flex items-center gap-3 py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
            isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
            activeTab === 'reports' ? 'is-active' : 'nav-idle'
          ]"
          :title="isCollapsed ? 'Báo cáo' : ''"
        >
          <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          <span v-if="!isCollapsed" class="truncate">Báo cáo</span>
        </button>

        <div v-if="authState.isAdmin.value">
          <button @click="handleAiClick" aria-label="Quản trị dữ liệu AI" :aria-expanded="isAiOpen && !isCollapsed" aria-controls="ai-submenu" :title="isCollapsed ? 'Quản trị dữ liệu AI' : ''" :class="['nav-link w-full flex items-center justify-between py-3 rounded-xl font-bold text-xs transition whitespace-nowrap cursor-pointer', isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3', isAiRoute(activeTab) ? 'is-active' : 'nav-idle']">
            <div class="flex items-center gap-3 min-w-0"><svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3h6v4H9zM5 9h14v12H5zM9 13h.01M15 13h.01M9 17h6M3 12H1m22 0h-2"/></svg><span v-if="!isCollapsed" class="truncate">Quản trị dữ liệu AI</span></div>
            <svg v-if="!isCollapsed" class="w-4 h-4 shrink-0 transition-transform" :class="{'rotate-180':isAiOpen}" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true"><path d="M19 9l-7 7-7-7" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" /></svg>
          </button>
          <div v-if="isAiOpen && !isCollapsed" id="ai-submenu" class="pl-9 pr-2 py-1.5 space-y-1">
            <button v-for="item in AI_MENU" :key="item.route" @click="selectTab(item.route)" :aria-current="aiMenuRoute(activeTab)===item.route?'page':undefined" :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer',aiMenuRoute(activeTab)===item.route?'is-active':'nav-idle']">{{ item.label }}</button>
          </div>
        </div>

        <!-- Menu to: Tiện ích (Chứa Văn bản QPPL và Đầu mối liên hệ và KH CĐS) -->
        <div>
          <button
            @click="handleUtilitiesClick"
            :class="[
              'nav-link w-full flex items-center justify-between py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
              isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
              ['legal-documents', 'agency-plans'].includes(activeTab) ? 'is-active' : 'nav-idle'
            ]"
            :title="isCollapsed ? 'Tiện ích' : ''"
          >
            <div class="flex items-center gap-3 truncate">
              <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
              </svg>
              <span v-if="!isCollapsed" class="truncate">Tiện ích</span>
            </div>
            <svg v-if="!isCollapsed" class="w-4 h-4 transition-transform shrink-0" :class="{ 'rotate-180': isUtilitiesOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/></svg>
          </button>

          <!-- Submenu Tiện ích -->
          <div v-if="isUtilitiesOpen && !isCollapsed" class="pl-9 pr-2 py-1.5 space-y-1">
            <button
              @click="selectTab('legal-documents')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'legal-documents' ? 'is-active' : 'nav-idle']"
            >
              Văn bản QPPL
            </button>
            <button
              @click="selectTab('agency-plans')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'agency-plans' ? 'is-active' : 'nav-idle']"
            >
              Đầu mối liên hệ và KH CĐS
            </button>
          </div>
        </div>

        <!-- Submenu: Thiết lập chung (Settings) -->
        <div class="pt-2">
          <button 
            @click="handleSettingsClick"
            :class="[
              'nav-link w-full flex items-center justify-between py-3 rounded-xl font-bold text-xs transition-all duration-200 whitespace-nowrap overflow-hidden cursor-pointer',
              isCollapsed ? 'justify-center px-0 w-12 h-12 mx-auto' : 'px-3',
              ['agencies', 'units', 'users', 'import-history', 'settings', 'master-data'].includes(activeTab) ? 'is-active' : 'nav-idle'
            ]"
            :title="isCollapsed ? 'Thiết lập chung' : ''"
          >
            <div class="flex items-center gap-3 truncate">
              <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>
              <span v-if="!isCollapsed" class="truncate">Thiết lập chung</span>
            </div>
            <svg v-if="!isCollapsed" class="w-4 h-4 transition-transform shrink-0" :class="{ 'rotate-180': isSettingsOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/></svg>
          </button>

          <!-- Submenu options -->
          <div v-if="isSettingsOpen && !isCollapsed" class="pl-9 pr-2 py-1.5 space-y-1">
            <button 
              v-if="authState.isAdmin.value"
              @click="selectTab('agencies')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'agencies' ? 'is-active' : 'nav-idle']"
            >
              Danh mục cơ quan
            </button>
            <button 
              v-if="false"
              @click="selectTab('units')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'units' ? 'is-active' : 'nav-idle']"
            >
              Danh mục đơn vị tính
            </button>
            <button 
              v-if="authState.isAdmin.value"
              @click="selectTab('users')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'users' ? 'is-active' : 'nav-idle']"
            >
              Quản lý tài khoản
            </button>
            <button 
              @click="selectTab('import-history')"
              :class="['nav-sublink w-full text-left py-2 px-3 rounded-lg text-xs font-semibold transition whitespace-nowrap cursor-pointer', activeTab === 'import-history' ? 'is-active' : 'nav-idle']"
            >
              Lịch sử nạp dữ liệu file
            </button>
          </div>
        </div>

      </nav>

      <!-- Footer user role badge -->
      <div v-if="!isCollapsed" class="nav-footer p-4 border-t border-slate-800 text-xs text-slate-500 font-medium whitespace-nowrap overflow-hidden">
        <div>Đơn vị vận hành</div>
        <div class="text-slate-300 font-bold truncate" title="Phòng Chính sách và Chiến lược">Phòng Chính sách và Chiến lược</div>
      </div>
    </aside>
  </div>
</template>

<script setup>
import { ref, computed, watch, onUnmounted } from 'vue';
import { authState } from '../services/auth';
import { activeDocument, dashboardDocumentScope } from '../services/documentContext';
import { AI_MENU, isAiRoute, aiMenuRoute } from '../shared/aiNavigation';

const props = defineProps({
  activeTab: { type: String, default: 'dashboard' },
  mobileOpen: { type: Boolean, default: false }
});

const emit = defineEmits(['navigate', 'close']);

const isManualCollapsed = ref(true);
const isHovered = ref(false);
let hoverTimer = null;

const isAiOpen = ref(isAiRoute(props.activeTab));
const isGoalsOpen = ref(props.activeTab.startsWith('goals'));
const isUtilitiesOpen = ref(['legal-documents', 'agency-plans'].includes(props.activeTab));
const isSettingsOpen = ref(['agencies', 'units', 'users', 'import-history'].includes(props.activeTab));

const isCollapsed = computed(() => {
  return !props.mobileOpen && isManualCollapsed.value && !isHovered.value;
});

watch(() => props.activeTab, (newTab) => {
  if (isAiRoute(newTab)) isAiOpen.value = true;
  if (newTab.startsWith('goals')) isGoalsOpen.value = true;
  if (['legal-documents', 'agency-plans'].includes(newTab)) isUtilitiesOpen.value = true;
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

function handleAiClick() {
  isAiOpen.value = !isAiOpen.value;
}

function handleGoalsClick() {
  isGoalsOpen.value = !isGoalsOpen.value;
}

function handleUtilitiesClick() {
  isUtilitiesOpen.value = !isUtilitiesOpen.value;
}

function handleSettingsClick() {
  isSettingsOpen.value = !isSettingsOpen.value;
}

function selectTab(tab) {
  if (hoverTimer) clearTimeout(hoverTimer);
  emit('navigate', tab);
  emit('close');
  if (isManualCollapsed.value) {
    isHovered.value = false;
  }
}
onUnmounted(() => clearTimeout(hoverTimer));
</script>


<style scoped>
.app-navigation { background: var(--ui-nav-background); border-color: var(--ui-nav-line); box-shadow: 4px 0 18px rgb(33 54 88 / .07); }
.nav-brand, .nav-footer { border-color: var(--ui-nav-line); }
.nav-footer { color: var(--ui-nav-muted); }
.nav-link, .nav-sublink { position: relative; color: var(--ui-nav-text); border: 1px solid transparent; }
.nav-link:not(.is-active):hover, .nav-sublink:not(.is-active):hover { background: var(--ui-nav-hover); color: white; }
.nav-link.is-active { background: var(--ui-nav-active); color: white; border-color: rgb(255 255 255 / .12); box-shadow: 0 3px 10px rgb(10 25 51 / .12); }
.nav-link.is-active::before { content: ''; position: absolute; left: 0; top: 25%; bottom: 25%; width: 3px; border-radius: 0 3px 3px 0; background: #a9d6ff; }
.nav-sublink.is-active { background: rgb(255 255 255 / .1); color: #e1edff; border-color: rgb(255 255 255 / .08); }
.app-navigation :focus-visible { outline-color: #b6d7ff; outline-offset: 2px; }
.app-navigation .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.app-navigation .custom-scrollbar::-webkit-scrollbar-thumb { background: var(--ui-nav-line); }
</style>
