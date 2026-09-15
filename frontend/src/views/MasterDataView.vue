<template>
  <div class="w-full flex flex-col md:flex-row gap-6 font-sans">
    
    <!-- Vertical Left Sidebar Menu -->
    <div class="w-full md:w-64 bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-3 shrink-0 h-fit">
      <div class="px-3 py-2 border-b border-slate-100 mb-1">
        <h3 class="text-xs font-black text-slate-400 uppercase tracking-wider">Cài Đặt Chung</h3>
        <p class="text-[11px] text-slate-500 font-medium">Quản lý từ điển & cấu hình hệ thống</p>
      </div>

      <nav class="space-y-1.5">
        <button 
          @click="activeSubTab = 'agencies'" 
          :class="['w-full text-left px-4 py-3 rounded-xl text-xs font-extrabold transition flex items-center gap-3 cursor-pointer', activeSubTab === 'agencies' ? 'bg-blue-900 text-white shadow-sm' : 'text-slate-700 hover:bg-slate-100']"
        >
          <span class="text-base">🏢</span>
          <span>Danh Mục Cơ Quan, Bộ Ngành</span>
        </button>

        <button 
          @click="activeSubTab = 'units'" 
          :class="['w-full text-left px-4 py-3 rounded-xl text-xs font-extrabold transition flex items-center gap-3 cursor-pointer', activeSubTab === 'units' ? 'bg-blue-900 text-white shadow-sm' : 'text-slate-700 hover:bg-slate-100']"
        >
          <span class="text-base">📏</span>
          <span>Danh Mục Đơn Vị Tính</span>
        </button>

        <button 
          @click="activeSubTab = 'imports'" 
          :class="['w-full text-left px-4 py-3 rounded-xl text-xs font-extrabold transition flex items-center gap-3 cursor-pointer', activeSubTab === 'imports' ? 'bg-blue-900 text-white shadow-sm' : 'text-slate-700 hover:bg-slate-100']"
        >
          <span class="text-base">📥</span>
          <span>Lịch Sử Nạp Dữ Liệu</span>
        </button>

        <!-- SUB-TAB 4: USER ACCOUNT MANAGEMENT -->
        <button 
          @click="activeSubTab = 'users'" 
          :class="['w-full text-left px-4 py-3 rounded-xl text-xs font-extrabold transition flex items-center gap-3 cursor-pointer', activeSubTab === 'users' ? 'bg-blue-900 text-white shadow-sm' : 'text-slate-700 hover:bg-slate-100']"
        >
          <span class="text-base">👥</span>
          <span>Quản Lý Tài Khoản Người Dùng</span>
        </button>
      </nav>
    </div>

    <!-- Main Content Area -->
    <div class="flex-1 min-w-0">
      <AgencyManagement v-if="activeSubTab === 'agencies'" />
      <UnitManagement v-else-if="activeSubTab === 'units'" />
      <ImportHistoryAudit v-else-if="activeSubTab === 'imports'" />
      <UserManagementView v-else-if="activeSubTab === 'users'" />
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue';
import AgencyManagement from '../components/AgencyManagement.vue';
import UnitManagement from '../components/UnitManagement.vue';
import ImportHistoryAudit from '../components/ImportHistoryAudit.vue';
import UserManagementView from './UserManagementView.vue';

const activeSubTab = ref('agencies');
</script>
