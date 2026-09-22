<template>
  <header class="h-16 bg-white border-b border-slate-200 px-4 sm:px-6 flex items-center justify-between shrink-0 shadow-xs z-20">
    <!-- System Official Title Branding -->
    <div class="flex items-center gap-3">
      <div>
        <h1 class="text-sm sm:text-base md:text-lg font-bold text-slate-800 leading-snug">
          Hệ thống theo dõi nhiệm vụ được giao tại Quyết định số 1266/QĐ-TTg ngày 14/07/2026
        </h1>
      </div>
    </div>

    <!-- Right Side Actions: Bell & User Character Avatar Popover -->
    <div class="flex items-center gap-2">
      <!-- 1. Notification Bell Button (Right next to user icon) -->
      <NotificationCenter />

      <!-- 2. User Profile Avatar Trigger with Click Popover Menu -->
      <div class="relative" ref="userMenuContainer">
        <button 
          @click="isUserMenuOpen = !isUserMenuOpen"
          :class="[
            'w-9 h-9 rounded-xl border flex items-center justify-center transition shadow-2xs group relative',
            isUserMenuOpen ? 'bg-blue-50 border-blue-400 text-blue-600 ring-2 ring-blue-500/20' : 'bg-slate-100 hover:bg-slate-200/80 border-slate-200 text-slate-600'
          ]"
          title="Thông tin tài khoản cá nhân"
        >
          <!-- User Person Character Icon -->
          <svg class="w-5 h-5 transition-transform group-hover:scale-110" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
        </button>

        <!-- User Profile Dropdown Popover Panel -->
        <transition name="fade">
          <div 
            v-if="isUserMenuOpen" 
            class="absolute right-0 mt-2 w-60 bg-white rounded-2xl shadow-xl border border-slate-200 z-50 p-3 space-y-3 animate-in fade-in zoom-in-95 duration-100"
          >
            <!-- User Info Summary Header -->
            <div class="flex items-center gap-3 pb-2.5 border-b border-slate-100">
              <div class="w-10 h-10 rounded-xl bg-blue-50 border border-blue-200 text-blue-600 flex items-center justify-center shrink-0 shadow-2xs">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
              <div class="overflow-hidden min-w-0 flex-1">
                <div class="text-xs font-extrabold text-slate-900 truncate leading-snug">
                  {{ user?.fullName || user?.username || 'Quản trị viên Hệ thống' }}
                </div>
                <div class="text-[11px] font-bold text-blue-600 mt-0.5 truncate" :title="userDisplayAgency">
                  {{ userDisplayAgency }}
                </div>
              </div>
            </div>

            <!-- Action Items Menu -->
            <div class="space-y-1">
              <button 
                @click="handleLogout"
                class="w-full text-left px-3 py-2 text-xs font-bold text-rose-600 hover:bg-rose-50 rounded-xl transition flex items-center gap-2 cursor-pointer"
              >
                <svg class="w-4 h-4 text-rose-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                <span>Đăng xuất khỏi hệ thống</span>
              </button>
            </div>
          </div>
        </transition>
      </div>
    </div>
  </header>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import NotificationCenter from './NotificationCenter.vue';
import { authState } from '../services/auth';

const props = defineProps({
  user: { type: Object, default: null }
});

const emit = defineEmits(['logout']);

const isUserMenuOpen = ref(false);
const userMenuContainer = ref(null);

const userDisplayAgency = computed(() => {
  if (props.user?.agencyName) return props.user.agencyName;
  if (props.user?.agency?.name) return props.user.agency.name;
  if (authState.isAdmin.value) return 'Quản trị viên Hệ thống';
  return 'Cơ quan / Đơn vị';
});

function handleLogout() {
  isUserMenuOpen.value = false;
  emit('logout');
}

function handleClickOutside(e) {
  if (userMenuContainer.value && !userMenuContainer.value.contains(e.target)) {
    isUserMenuOpen.value = false;
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>
