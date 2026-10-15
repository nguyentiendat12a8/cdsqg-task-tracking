<template>
<div class="w-full space-y-3.5">

        <!-- Biểu đồ 1: Thống kê mục tiêu / nhiệm vụ -->
        <div class="bg-white rounded-2xl border border-slate-200 p-4 sm:p-5 shadow-2xs w-full flex flex-col gap-4 ">
          <!-- Section Header -->
          <div class="space-y-1.5 border-b border-slate-100 pb-3">
            <h3 class="text-sm sm:text-base font-bold text-slate-800 flex items-center gap-2">
              <span class="p-1.5 bg-purple-100 text-purple-700 rounded-lg text-xs shrink-0">🎯</span>
              <span>Thống kê {{ dashboardItemNoun }}</span>
            </h3>
            <p class="text-xs text-slate-500 font-normal">
              (Trong đó các {{ dashboardItemNoun }} giao chung không hiển thị ở biểu đồ trạng thái)
            </p>

            <!-- Summary Badges: Tổng, Mục tiêu riêng & Mục tiêu chung -->
            <div class="flex flex-wrap items-center gap-2 text-xs font-bold pt-1">
              <button type="button" @click="openStatistic('items', null, 'all')" aria-label="Xem toàn bộ danh sách" class="px-2.5 py-1 bg-slate-50 border border-slate-200 text-slate-700 rounded-lg flex items-center gap-1.5 hover:bg-blue-50 cursor-pointer">
                <span>Tổng: <strong class="text-purple-700 font-bold text-xs">{{ formatNumber(activeCreatedTotals.total) }}</strong></span>
              </button>

              <button type="button" @click="openStatistic('items', null, 'specific')" aria-label="Xem danh sách giao riêng" class="px-2.5 py-1 bg-slate-50 border border-slate-200 text-slate-700 rounded-lg flex items-center gap-1.5 hover:bg-blue-50 cursor-pointer">
                <span>{{ dashboardFilter === 'goals' ? 'Mục tiêu riêng' : 'Nhiệm vụ riêng' }}: <strong class="text-blue-700 font-bold text-xs">{{ formatNumber(activeCreatedTotals.specific) }}</strong></span>
              </button>

              <button type="button" @click="openStatistic('items', null, 'general')" aria-label="Xem danh sách giao chung" class="px-2.5 py-1 bg-slate-50 border border-slate-200 text-slate-700 rounded-lg flex items-center gap-1.5 hover:bg-blue-50 cursor-pointer">
                <span>{{ dashboardFilter === 'goals' ? 'Mục tiêu chung' : 'Nhiệm vụ chung' }}: <strong class="text-emerald-700 font-bold text-xs">{{ formatNumber(activeCreatedTotals.general) }}</strong></span>
              </button>
            </div>
          </div>

          <!-- Chart & Status Breakdown -->
          <div class="grid grid-cols-1 sm:grid-cols-12 gap-4 items-center">
            <!-- Donut Circle Chart on Left -->
            <div class="sm:col-span-5 flex flex-col items-center justify-center p-3 bg-slate-50/60 rounded-xl border border-slate-100 space-y-2">
              <button type="button" aria-label="Xem danh sách trong biểu đồ thống kê" @click="openStatistic('items')" class="rounded-full focus-visible:outline-2 focus-visible:outline-blue-600"><MiniStatusDonut :stats="createdDonutStats" :size="130" :innerSize="85" :fontSize="24" /></button>
              <div class="text-center pt-1">
                <div class="text-xs font-bold text-slate-700">
                  {{ dashboardFilter === 'goals' ? 'Mục tiêu riêng' : 'Nhiệm vụ riêng' }}: {{ formatNumber(activeCreatedTotals.specific) }}
                </div>
              </div>
            </div>

            <!-- 6 Status Legend Breakdown Grid on Right -->
            <div class="sm:col-span-7 grid grid-cols-1 gap-2 text-xs font-bold">
              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-slate-50 border border-slate-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('items', 'NotStarted')" @click="openStatistic('items', 'NotStarted')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-slate-400 shrink-0"></span>
                  <span class="text-slate-700 font-medium">Chưa thực hiện</span>
                </div>
                <span class="font-bold text-slate-900 px-1">{{ formatNumber(activeCreatedStatusSummary.notStarted ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-blue-50/60 border border-blue-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('items', 'InProgressOnTime')" @click="openStatistic('items', 'InProgressOnTime')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-blue-500 shrink-0"></span>
                  <span class="text-blue-800 font-medium">Đang thực hiện (trong hạn)</span>
                </div>
                <span class="font-bold text-blue-900 px-1">{{ formatNumber(activeCreatedStatusSummary.inProgressOnTime ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-rose-50/60 border border-rose-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('items', 'InProgressOverdue')" @click="openStatistic('items', 'InProgressOverdue')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-rose-500 shrink-0"></span>
                  <span class="text-rose-800 font-medium">Đang thực hiện (quá hạn)</span>
                </div>
                <span class="font-bold text-rose-900 px-1">{{ formatNumber(activeCreatedStatusSummary.inProgressOverdue ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-emerald-50/60 border border-emerald-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('items', 'CompletedOnTime')" @click="openStatistic('items', 'CompletedOnTime')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-emerald-500 shrink-0"></span>
                  <span class="text-emerald-800 font-medium">Hoàn thành (đúng hạn)</span>
                </div>
                <span class="font-bold text-emerald-900 px-1">{{ formatNumber(activeCreatedStatusSummary.completedOnTime ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-teal-50/60 border border-teal-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('items', 'CompletedOverdue')" @click="openStatistic('items', 'CompletedOverdue')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-teal-500 shrink-0"></span>
                  <span class="text-teal-800 font-medium">Hoàn thành (quá hạn)</span>
                </div>
                <span class="font-bold text-teal-900 px-1">{{ formatNumber(activeCreatedStatusSummary.completedOverdue ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-amber-50/60 border border-amber-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('items', 'ExpiringSoon')" @click="openStatistic('items', 'ExpiringSoon')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-amber-500 shrink-0"></span>
                  <span class="text-amber-800 font-medium">Sắp hết hạn</span>
                </div>
                <span class="font-bold text-amber-900 px-1">{{ formatNumber(activeCreatedStatusSummary.expiringSoon ?? 0) }}</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Biểu đồ 2: Thống kê mục tiêu / nhiệm vụ giao cho các Bộ, ngành, địa phương và đơn vị khác -->
        <div class="bg-white rounded-2xl border border-slate-200 p-4 sm:p-5 shadow-2xs w-full flex flex-col gap-4 ">
          <!-- Section Header -->
          <div class="border-b border-slate-100 pb-3">
            <div class="flex items-center justify-between gap-2">
              <h3 class="text-sm sm:text-base font-bold text-slate-800 flex items-center gap-2">
                <span class="p-1.5 bg-blue-100 text-blue-700 rounded-lg text-xs shrink-0">🏛️</span>
                <span>Thống kê {{ dashboardItemNoun }} giao cho các Bộ, ngành, địa phương và đơn vị khác</span>
              </h3>
            </div>
          </div>

          <!-- Chart & Status Breakdown -->
          <div class="grid grid-cols-1 sm:grid-cols-12 gap-4 items-center">
            <!-- Donut Circle Chart on Left -->
            <div class="sm:col-span-5 flex flex-col items-center justify-center p-3 bg-slate-50/60 rounded-xl border border-slate-100 space-y-2">
              <button type="button" aria-label="Xem toàn bộ lượt giao theo cơ quan" @click="openStatistic('assignments')" class="rounded-full focus-visible:outline-2 focus-visible:outline-blue-600"><MiniStatusDonut :stats="overallDonutStats" :size="130" :innerSize="85" :fontSize="24" /></button>
              <div class="text-center pt-1">
                <div class="text-xs font-bold text-slate-700">
                  Tổng: {{ formatNumber(activeStatusTotal) }}
                </div>
              </div>
            </div>

            <!-- 6 Status Legend Breakdown Grid on Right -->
            <div class="sm:col-span-7 grid grid-cols-1 gap-2 text-xs font-bold">
              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-slate-50 border border-slate-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('assignments', 'NotStarted')" @click="openStatistic('assignments', 'NotStarted')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-slate-400 shrink-0"></span>
                  <span class="text-slate-700 font-medium">Chưa thực hiện</span>
                </div>
                <span class="font-bold text-slate-900 px-1">{{ formatNumber(activeStatusSummary.notStarted ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-blue-50/60 border border-blue-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('assignments', 'InProgressOnTime')" @click="openStatistic('assignments', 'InProgressOnTime')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-blue-500 shrink-0"></span>
                  <span class="text-blue-800 font-medium">Đang thực hiện (trong hạn)</span>
                </div>
                <span class="font-bold text-blue-900 px-1">{{ formatNumber(activeStatusSummary.inProgressOnTime ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-rose-50/60 border border-rose-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('assignments', 'InProgressOverdue')" @click="openStatistic('assignments', 'InProgressOverdue')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-rose-500 shrink-0"></span>
                  <span class="text-rose-800 font-medium">Đang thực hiện (quá hạn)</span>
                </div>
                <span class="font-bold text-rose-900 px-1">{{ formatNumber(activeStatusSummary.inProgressOverdue ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-emerald-50/60 border border-emerald-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('assignments', 'CompletedOnTime')" @click="openStatistic('assignments', 'CompletedOnTime')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-emerald-500 shrink-0"></span>
                  <span class="text-emerald-800 font-medium">Hoàn thành (đúng hạn)</span>
                </div>
                <span class="font-bold text-emerald-900 px-1">{{ formatNumber(activeStatusSummary.completedOnTime ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-teal-50/60 border border-teal-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('assignments', 'CompletedOverdue')" @click="openStatistic('assignments', 'CompletedOverdue')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-teal-500 shrink-0"></span>
                  <span class="text-teal-800 font-medium">Hoàn thành (quá hạn)</span>
                </div>
                <span class="font-bold text-teal-900 px-1">{{ formatNumber(activeStatusSummary.completedOverdue ?? 0) }}</span>
              </button>

              <button type="button" class="flex items-center justify-between p-2 rounded-lg bg-amber-50/60 border border-amber-200/80 w-full text-left cursor-pointer hover:brightness-95 focus-visible:outline-2 focus-visible:outline-blue-600" :aria-label="statisticLabel('assignments', 'ExpiringSoon')" @click="openStatistic('assignments', 'ExpiringSoon')">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-amber-500 shrink-0"></span>
                  <span class="text-amber-800 font-medium">Sắp hết hạn</span>
                </div>
                <span class="font-bold text-amber-900 px-1">{{ formatNumber(activeStatusSummary.expiringSoon ?? 0) }}</span>
              </button>
            </div>
          </div>
        </div>

      </div>
</template>

<script setup>
import { formatNumber } from "../../shared/formatters";
import MiniStatusDonut from '../../components/MiniStatusDonut.vue';
import { getStatusLabel } from '../../shared/statusPresentation';
const emit = defineEmits(['select-statistic']);
function statisticLabel(mode, status) {
  return `Xem ${getStatusLabel(status)} — ${mode === 'items' ? 'danh sách' : 'lượt giao theo cơ quan'}`;
}
function openStatistic(mode, status = null, scope = 'specific') {
  emit('select-statistic', { mode, status, scope });
}

defineProps([
  'dashboardFilter',
  'dashboardItemNoun',
  'activeStatusSummary',
  'activeStatusTotal',
  'overallDonutStats',
  'activeCreatedStatusSummary',
  'activeCreatedTotals',
  'createdDonutStats'
]);

</script>
