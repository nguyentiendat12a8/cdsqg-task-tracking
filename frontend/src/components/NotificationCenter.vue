<template>
  <div class="relative" ref="containerRef">
    <!-- Notification Bell Button -->
    <button 
      @click="isOpen = !isOpen"
      class="relative p-2 text-slate-500 hover:text-slate-800 hover:bg-slate-100 rounded-xl transition"
      title="Thông báo hệ thống"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
      </svg>
      <span 
        v-if="unreadCount > 0" 
        class="absolute -top-1 -right-1 px-1.5 py-0.5 bg-rose-600 text-white text-[10px] font-bold rounded-full shadow-sm animate-pulse"
      >
        {{ unreadCount }}
      </span>
    </button>

    <!-- Notification Dropdown Drawer -->
    <div 
      v-if="isOpen" 
      class="absolute right-0 mt-3 w-80 sm:w-96 bg-white rounded-2xl shadow-2xl border border-slate-200 z-50 overflow-hidden animate-in fade-in duration-200"
    >
      <div class="p-4 bg-slate-900 text-white flex items-center justify-between">
        <div class="flex items-center gap-2">
          <span class="font-bold text-sm">🔔 Thông báo Nhắc Nhở</span>
          <span v-if="unreadCount > 0" class="px-2 py-0.5 bg-blue-500 text-white text-[10px] font-bold rounded-full">
            {{ unreadCount }} mới
          </span>
        </div>
        <button 
          @click="markAllRead" 
          class="text-xs text-blue-300 hover:text-white font-bold transition"
        >
          Đã đọc tất cả
        </button>
      </div>

      <div class="max-h-96 overflow-y-auto divide-y divide-slate-100">
        <div v-if="notifications.length === 0" class="p-8 text-center text-slate-400 text-xs font-semibold">
          Không có thông báo mới nào.
        </div>

        <div 
          v-for="notif in notifications" 
          :key="notif.id"
          :class="[
            'p-4 hover:bg-slate-50 transition cursor-pointer flex items-start gap-3',
            !notif.isRead ? 'bg-blue-50/50 font-medium' : ''
          ]"
          @click="clickNotification(notif)"
        >
          <div class="w-8 h-8 rounded-xl shrink-0 flex items-center justify-center font-bold text-xs shadow-xs"
            :class="notif.type === 'OverdueAlert' ? 'bg-rose-100 text-rose-700' : 'bg-amber-100 text-amber-800'"
          >
            {{ notif.type === 'OverdueAlert' ? '⚠️' : '⏰' }}
          </div>
          <div class="flex-1 space-y-1">
            <div class="text-xs font-bold text-slate-900 leading-snug">{{ notif.title }}</div>
            <div class="text-[10px] text-slate-400 font-semibold pt-0.5">{{ formatDate(notif.createdAt) }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';

const containerRef = ref(null);
const isOpen = ref(false);
const unreadCount = ref(0);
const notifications = ref([]);

function handleClickOutside(e) {
  if (containerRef.value && !containerRef.value.contains(e.target)) {
    isOpen.value = false;
  }
}

function formatDate(dStr) {
  if (!dStr) return '';
  try {
    let str = String(dStr).trim();
    if (str.includes('T') && !str.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(str)) {
      str += 'Z';
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return dStr;
    return d.toLocaleString('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh' });
  } catch {
    return dStr;
  }
}

async function fetchNotifications() {
  try {
    const user = authState.user.value;
    const isAdmin = authState.isAdmin.value;

    let url = getApiUrl('/api/notification');
    const params = new URLSearchParams();

    if (user?.id) params.append('userId', user.id);
    if (user?.agencyId) params.append('agencyId', user.agencyId);
    if (isAdmin) params.append('isAdmin', 'true');

    if (params.toString()) {
      url += '?' + params.toString();
    }

    const res = await fetch(url);
    if (res.ok) {
      const data = await res.json();
      const rawItems = data.items || [];
      const uniqueItems = [];
      const seenKeys = new Set();
      for (const item of rawItems) {
        const key = `${item.title}_${item.message}_${item.createdAt?.substring(0, 19)}`;
        if (!seenKeys.has(key)) {
          seenKeys.add(key);
          uniqueItems.push(item);
        }
      }
      notifications.value = uniqueItems;
      unreadCount.value = uniqueItems.filter(i => !i.isRead).length;
    }
  } catch (e) {
    // Backend offline fallback - fail silently
  }
}

async function markAllRead() {
  try {
    const user = authState.user.value;
    const isAdmin = authState.isAdmin.value;

    let url = getApiUrl('/api/notification/read-all');
    const params = new URLSearchParams();

    if (user?.id) params.append('userId', user.id);
    if (user?.agencyId) params.append('agencyId', user.agencyId);
    if (isAdmin) params.append('isAdmin', 'true');

    if (params.toString()) {
      url += '?' + params.toString();
    }

    await fetch(url, { method: 'PUT' });
    unreadCount.value = 0;
    notifications.value.forEach(n => n.isRead = true);
  } catch (e) {}
}

async function clickNotification(notif) {
  notif.isRead = true;
  try {
    await fetch(getApiUrl(`/api/notification/${notif.id}/read`), { method: 'PUT' });
  } catch (e) {}
  isOpen.value = false;

  window.dispatchEvent(new CustomEvent('open-notification-detail', {
    detail: notif
  }));
}

let pollTimer = null;

function handleNotificationSent() {
  fetchNotifications();
}

function handleVisibilityChange() {
  if (!document.hidden) {
    fetchNotifications();
  }
}

watch(isOpen, (newVal) => {
  if (newVal) {
    fetchNotifications();
  }
});

watch(() => authState.user.value?.id, () => {
  fetchNotifications();
});

onMounted(() => {
  fetchNotifications();
  document.addEventListener('click', handleClickOutside);
  window.addEventListener('notification-sent', handleNotificationSent);
  document.addEventListener('visibilitychange', handleVisibilityChange);

  // Real-time polling every 5 seconds
  pollTimer = setInterval(fetchNotifications, 5000);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
  window.removeEventListener('notification-sent', handleNotificationSent);
  document.removeEventListener('visibilitychange', handleVisibilityChange);

  if (pollTimer) {
    clearInterval(pollTimer);
    pollTimer = null;
  }
});
</script>
