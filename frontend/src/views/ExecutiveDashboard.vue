<template>
  <div class="w-full space-y-6 font-sans">
    
    <!-- Top Header -->
    <header class="flex flex-col lg:flex-row lg:items-center justify-between gap-4 bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div>
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
          </span>
          <div>
            <h1 class="text-xl font-extrabold tracking-tight text-slate-800">Dashboard Lãnh Đạo - Chỉ Đạo Chuyển Đổi Số Quốc Gia</h1>
            <p class="text-xs text-slate-500 mt-0.5">Tổng hợp chỉ số theo dõi tiến độ và đôn đốc thực hiện nhiệm vụ các Bộ, Ngành, Địa phương</p>
          </div>
        </div>
      </div>

      <!-- Filters & Action Bar -->
      <div class="flex flex-wrap items-center gap-3">
        <!-- DOCUMENT SELECTOR DROPDOWN -->
        <div class="flex items-center bg-blue-50 border border-blue-200 rounded-xl px-3 py-1.5">
          <span class="text-xs font-extrabold text-blue-900 mr-2 shrink-0">Văn bản:</span>
          <select 
            v-model="selectedDocumentId" 
            @change="loadDashboardMetrics" 
            class="bg-transparent text-xs font-bold text-blue-950 focus:outline-none max-w-[320px] truncate cursor-pointer"
          >
            <option value="">-- Tất Cả Văn Bản Chỉ Đạo --</option>
            <option v-for="doc in documents" :key="doc.id" :value="doc.id">
              {{ doc.documentNumber || doc.code }} - {{ doc.name || doc.title }}
            </option>
          </select>
        </div>
      </div>
    </header>

    <!-- 3 Core Summary KPI Cards (Executive Navy Theme) -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 w-full">
      
      <!-- Card 1: Completed Goals -->
      <div class="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/90 flex flex-col justify-between space-y-3">
        <div class="flex justify-between items-center text-slate-500">
          <span class="text-xs font-extrabold uppercase tracking-wider text-slate-700">🎯 Tổng Số Mục Tiêu Hoàn Thành</span>
          <span class="p-2 bg-blue-50 text-blue-900 rounded-xl font-bold">📊</span>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-4xl font-black text-slate-900">{{ completedGoalsDisplay }}</span>
          <span class="text-lg font-extrabold text-slate-400">/ {{ totalGoalsDisplay }}</span>
          <span class="ml-auto text-xs font-black bg-blue-900 px-3 py-1 rounded-xl text-white shadow-2xs">
            {{ goalCompletionPct }}%
          </span>
        </div>
        <div class="w-full bg-slate-100 rounded-full h-2 overflow-hidden border border-slate-200/60">
          <div class="bg-blue-900 h-full rounded-full transition-all duration-500" :style="{ width: `${goalCompletionPct}%` }"></div>
        </div>
        <p class="text-[11px] text-slate-500 font-medium">Mục tiêu chiến lược đã đạt mốc kế hoạch</p>
      </div>

      <!-- Card 2: Completed Tasks -->
      <div class="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/90 flex flex-col justify-between space-y-3">
        <div class="flex justify-between items-center text-slate-500">
          <span class="text-xs font-extrabold uppercase tracking-wider text-slate-700">📋 Tổng Số Nhiệm Vụ Hoàn Thành</span>
          <span class="p-2 bg-emerald-50 text-emerald-700 rounded-xl font-bold">✅</span>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-4xl font-black text-slate-900">{{ completedTasksDisplay }}</span>
          <span class="text-lg font-extrabold text-slate-400">/ {{ totalTasksDisplay }}</span>
          <span class="ml-auto text-xs font-black bg-emerald-600 px-3 py-1 rounded-xl text-white shadow-2xs">
            {{ taskCompletionPct }}%
          </span>
        </div>
        <div class="w-full bg-slate-100 rounded-full h-2 overflow-hidden border border-slate-200/60">
          <div class="bg-emerald-600 h-full rounded-full transition-all duration-500" :style="{ width: `${taskCompletionPct}%` }"></div>
        </div>
        <p class="text-[11px] text-slate-500 font-medium">Nhiệm vụ cụ thể giao Bộ/Ngành hoàn thành</p>
      </div>

      <!-- Card 3: Overdue / Lagging / At Risk Summary Card -->
      <div class="bg-white p-6 rounded-2xl shadow-xs border border-slate-200/90 flex flex-col justify-between space-y-3">
        <div class="flex justify-between items-center text-slate-700">
          <span class="text-xs font-extrabold uppercase tracking-wider text-rose-800 flex items-center gap-1.5">
            <span class="w-2.5 h-2.5 rounded-full bg-rose-600 animate-ping"></span>
            ⚠️ Mục Tiêu & Nhiệm Vụ Cần Đôn Đốc
          </span>
          <span class="text-xs font-extrabold text-rose-700 bg-rose-50 border border-rose-200 px-2.5 py-1 rounded-lg">
            {{ (metrics.overdueCount ?? 0) + (metrics.laggingCount ?? 0) + (metrics.atRiskCount ?? 0) }} Mục
          </span>
        </div>
        
        <!-- 3 Category Counter Badges Grid -->
        <div class="grid grid-cols-3 gap-2 py-1">
          <!-- Overdue Counter -->
          <div 
            @click="activeViewTab = 'Overdue'"
            class="bg-rose-50 hover:bg-rose-100 border border-rose-200 rounded-xl p-2.5 flex flex-col items-center justify-center cursor-pointer transition shadow-2xs"
            title="Bấm để lọc danh sách Quá hạn"
          >
            <span class="text-[11px] font-bold text-rose-800">⛔ Quá Hạn</span>
            <span class="text-2xl font-black text-rose-900 mt-0.5">{{ metrics.overdueCount ?? 0 }}</span>
          </div>

          <!-- Lagging Counter -->
          <div 
            @click="activeViewTab = 'Lagging'"
            class="bg-red-50 hover:bg-red-100 border border-red-200 rounded-xl p-2.5 flex flex-col items-center justify-center cursor-pointer transition shadow-2xs"
            title="Bấm để lọc danh sách Chậm tiến độ"
          >
            <span class="text-[11px] font-bold text-red-800">🚨 Chậm Tiến Độ</span>
            <span class="text-2xl font-black text-red-900 mt-0.5">{{ metrics.laggingCount ?? 0 }}</span>
          </div>

          <!-- At Risk Counter -->
          <div 
            @click="activeViewTab = 'AtRisk'"
            class="bg-amber-50 hover:bg-amber-100 border border-amber-200 rounded-xl p-2.5 flex flex-col items-center justify-center cursor-pointer transition shadow-2xs"
            title="Bấm để lọc danh sách Nguy cơ chậm"
          >
            <span class="text-[11px] font-bold text-amber-800">⚠️ Nguy Cơ</span>
            <span class="text-2xl font-black text-amber-900 mt-0.5">{{ metrics.atRiskCount ?? 0 }}</span>
          </div>
        </div>

        <p class="text-[11px] text-slate-500 font-medium">Bấm vào từng ô để lọc danh sách đôn đốc tương ứng</p>
      </div>

    </div>

    <!-- Chart.js Visualization Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 w-full">
      
      <!-- Chart 1: Doughnut Chart (Overall Completion) -->
      <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 flex flex-col justify-between">
        <div>
          <h3 class="text-base font-extrabold text-slate-800">1. Tỉ Lệ Hoàn Thành Chỉ Tiêu Tổng Thể</h3>
          <p class="text-xs text-slate-400 mt-0.5">% Thực tế đạt được so với chỉ tiêu kế hoạch</p>
        </div>

        <div class="h-60 relative my-2 flex items-center justify-center">
          <Doughnut :data="doughnutChartData" :options="doughnutOptions" :plugins="[centerTextPlugin]" />
        </div>
      </div>

      <!-- Chart 2: Stacked Bar Chart (Tasks by Agency) -->
      <div class="lg:col-span-2 bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 flex flex-col justify-between">
        <div>
          <h3 class="text-base font-extrabold text-slate-800">2. Thống Kê Nhiệm Vụ Theo Bộ, Ngành, Địa Phương</h3>
          <p class="text-xs text-slate-400 mt-0.5">Thống kê phân loại theo 3 mức độ: Hoàn thành, Nguy cơ chậm tiến độ và Chậm tiến độ / Quá hạn</p>
        </div>

        <div class="h-64 mt-4">
          <Bar :data="barChartData" :options="barChartOptions" :plugins="[barDataLabelsPlugin]" />
        </div>
      </div>
    </div>

    <!-- Lagging / Stale Tasks Action Table -->
    <div class="bg-white rounded-2xl p-6 shadow-sm border border-slate-200/80 space-y-4 w-full">
      <div class="flex flex-col lg:flex-row lg:items-center justify-between border-b border-slate-100 pb-4 gap-4">
        <div>
          <h3 class="text-base font-extrabold text-rose-800 flex items-center gap-2">
            ⚠️ Danh Sách Mục Tiêu & Nhiệm Vụ Báo Động
          </h3>
          <p class="text-xs text-slate-400 mt-0.5">Theo dõi quá hạn, chậm tiến độ và nguy cơ chậm tiến độ để chỉ đạo đôn đốc</p>
        </div>

        <div class="flex flex-wrap items-center gap-3">
          <button 
            @click="exportLaggingTasksExcel"
            class="px-3.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl transition flex items-center gap-1.5 shadow-sm cursor-pointer"
            title="Xuất danh sách đang xem ra file Excel (.xlsx)"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
            Xuất Excel
          </button>
        </div>
      </div>

      <!-- 4 View Mode Tabs (User Requirement) -->
      <div class="flex flex-wrap items-center gap-2 p-1 bg-slate-100/80 rounded-xl border border-slate-200/80">
        <button 
          @click="changeViewTab('all')"
          :class="[
            'px-4 py-2 rounded-lg text-xs font-black transition cursor-pointer flex items-center gap-1.5',
            activeViewTab === 'all' 
              ? 'bg-white text-slate-900 shadow-sm border border-slate-200' 
              : 'text-slate-600 hover:text-slate-900 hover:bg-slate-200/60'
          ]"
        >
          📋 Tất Cả
          <span class="px-2 py-0.5 text-[11px] rounded-full bg-slate-200 text-slate-800 font-extrabold">{{ metrics.staleTasks?.length ?? 0 }}</span>
        </button>

        <button 
          @click="changeViewTab('Overdue')"
          :class="[
            'px-4 py-2 rounded-lg text-xs font-black transition cursor-pointer flex items-center gap-1.5',
            activeViewTab === 'Overdue' 
              ? 'bg-rose-800 text-white shadow-sm' 
              : 'text-rose-700 hover:bg-rose-100/80'
          ]"
        >
          ⛔ Quá Hạn Hoàn Thành
          <span class="px-2 py-0.5 text-[11px] rounded-full bg-rose-950/40 text-rose-100 font-extrabold">{{ metrics.overdueCount ?? 0 }}</span>
        </button>

        <button 
          @click="changeViewTab('Lagging')"
          :class="[
            'px-4 py-2 rounded-lg text-xs font-black transition cursor-pointer flex items-center gap-1.5',
            activeViewTab === 'Lagging' 
              ? 'bg-red-600 text-white shadow-sm' 
              : 'text-red-700 hover:bg-red-100/80'
          ]"
        >
          🚨 Chậm Tiến Độ
          <span class="px-2 py-0.5 text-[11px] rounded-full bg-red-950/40 text-red-100 font-extrabold">{{ metrics.laggingCount ?? 0 }}</span>
        </button>

        <button 
          @click="changeViewTab('AtRisk')"
          :class="[
            'px-4 py-2 rounded-lg text-xs font-black transition cursor-pointer flex items-center gap-1.5',
            activeViewTab === 'AtRisk' 
              ? 'bg-amber-500 text-slate-950 shadow-sm' 
              : 'text-amber-800 hover:bg-amber-100/80'
          ]"
        >
          ⚠️ Nguy Cơ Chậm Tiến Độ
          <span class="px-2 py-0.5 text-[11px] rounded-full bg-amber-950/20 text-amber-950 font-extrabold">{{ metrics.atRiskCount ?? 0 }}</span>
        </button>
      </div>

      <div v-if="laggingTasksList.length === 0" class="p-8 text-center text-slate-400 text-xs font-semibold italic">
        🎉 Không có mục tiêu/nhiệm vụ nào trong danh mục này!
      </div>

      <div v-else class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col w-full">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm text-slate-700 border-collapse">
            <thead class="bg-slate-100 text-xs text-slate-500 uppercase font-bold border-b border-slate-200">
              <tr>
                <th class="px-3 py-3 border-r border-slate-200 whitespace-nowrap text-center min-w-[130px]">Trạng Thái</th>
                <th class="px-3 py-3 border-r border-slate-200 whitespace-nowrap min-w-[85px] w-[85px]">Mã</th>
                <th class="px-4 py-3 border-r border-slate-200 min-w-[220px]">Mục Tiêu / Nhiệm Vụ</th>
                <th class="px-3 py-3 border-r border-slate-200 whitespace-nowrap min-w-[120px]">Cơ Quan Chủ Trì</th>
                <th class="px-3 py-3 border-r border-slate-200 whitespace-nowrap text-center min-w-[95px]">Thực Tế</th>
                <th class="px-3 py-3 border-r border-slate-200 whitespace-nowrap text-center min-w-[110px]">Mốc Kế Hoạch</th>
                <th class="px-3 py-3 border-r border-slate-200 whitespace-nowrap text-center min-w-[95px]">Chênh Lệch</th>
                <th class="px-3 py-3 text-center whitespace-nowrap min-w-[140px]">Hành Động Đôn Đốc</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <tr v-for="t in paginatedLaggingTasks" :key="t.id" class="hover:bg-slate-50 transition">
                <!-- Status & Category Badge -->
                <td class="px-3 py-3 text-center border-r border-slate-200 whitespace-nowrap min-w-[130px]">
                  <span v-if="t.staleCategory === 'Overdue'" class="inline-block px-2.5 py-1 bg-rose-900 text-white font-extrabold text-[11px] rounded-lg shadow-2xs">
                    ⛔ Quá hạn
                  </span>
                  <span v-else-if="t.staleCategory === 'Lagging'" class="inline-block px-2.5 py-1 bg-red-600 text-white font-extrabold text-[11px] rounded-lg shadow-2xs">
                    🚨 Chậm tiến độ
                  </span>
                  <span v-else-if="t.staleCategory === 'AtRisk'" class="inline-block px-2.5 py-1 bg-amber-500 text-slate-950 font-extrabold text-[11px] rounded-lg shadow-2xs">
                    ⚠️ Nguy cơ chậm
                  </span>
                  <span v-else class="inline-block px-2 py-0.5 bg-slate-200 text-slate-700 font-bold text-xs rounded">
                    {{ t.staleCategoryName || 'Cảnh báo' }}
                  </span>
                </td>

                <td class="px-3 py-3 font-extrabold text-blue-900 border-r border-slate-200 whitespace-nowrap min-w-[85px] w-[85px]">
                  <a :href="`#documents?id=${t.documentId}`" class="hover:underline text-blue-900 hover:text-blue-600 transition cursor-pointer" title="Xem chi tiết văn bản">
                    {{ t.code }}
                  </a>
                </td>

                <td class="px-4 py-3 font-semibold text-slate-800 border-r border-slate-200 min-w-[220px] leading-relaxed">
                  <div class="flex items-center gap-1.5 mb-0.5">
                    <span class="text-[10px] uppercase font-black px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">
                      {{ t.itemType || 'Nhiệm vụ' }}
                    </span>
                  </div>
                  <a :href="`#documents?id=${t.documentId}`" class="hover:underline text-slate-800 hover:text-blue-600 transition cursor-pointer" title="Xem chi tiết văn bản">
                    {{ t.title }}
                  </a>
                </td>

                <td class="px-3 py-3 font-bold text-slate-700 border-r border-slate-200 whitespace-nowrap min-w-[120px]">{{ t.leadAgency }}</td>

                <td class="px-3 py-3 text-center border-r border-slate-200 whitespace-nowrap min-w-[95px]">
                  <span v-if="t.hasReport === false" class="inline-block px-2 py-0.5 text-xs font-bold text-amber-700 bg-amber-50 rounded border border-amber-200/80 shadow-2xs">
                    Chưa báo cáo
                  </span>
                  <span v-else class="font-black text-rose-600">
                    {{ t.actualProgressPct }}%
                  </span>
                </td>

                <td class="px-3 py-3 text-center font-bold text-slate-600 border-r border-slate-200 whitespace-nowrap min-w-[110px]">
                  <div>{{ t.expectedTargetPct }}%</div>
                  <div v-if="t.expectedLinearProgress !== undefined && t.expectedLinearProgress !== null && t.expectedLinearProgress > 0" class="text-[10px] text-slate-400 font-medium mt-0.5">
                    (Mốc TT: {{ t.expectedLinearProgress }}%)
                  </div>
                </td>

                <td class="px-3 py-3 text-center font-black text-rose-700 border-r border-slate-200 whitespace-nowrap min-w-[95px]">
                  <span v-if="t.hasReport === false" class="inline-block px-2 py-0.5 text-xs font-bold text-amber-700 bg-amber-50 rounded border border-amber-200/80 shadow-2xs">
                    Chưa báo cáo
                  </span>
                  <span v-else :class="['px-2 py-0.5 rounded text-xs', t.staleCategory === 'AtRisk' ? 'bg-amber-100 text-amber-900' : 'bg-rose-100 text-rose-800']">
                    {{ t.laggingDeltaPct }}%
                  </span>
                </td>

                <td class="px-3 py-3 text-center whitespace-nowrap min-w-[140px]">
                  <button 
                    @click="openUrgeModal(t)"
                    class="px-3 py-1.5 bg-rose-600 hover:bg-rose-700 text-white font-bold text-xs rounded-lg transition shadow-sm inline-flex items-center gap-1 cursor-pointer"
                  >
                    ⚡ Chỉ Đạo Đôn Đốc
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination Controls for Lagging Tasks Table (SEAMLESSLY ATTACHED) -->
        <div class="flex flex-col sm:flex-row items-center justify-between bg-slate-50/70 p-4 border-t border-slate-200/80 w-full gap-3 text-xs text-slate-600 font-semibold">
          <span>
            Hiển thị <span class="font-extrabold text-slate-900">{{ laggingTasksList.length > 0 ? (currentLaggingPage - 1) * laggingPageSize + 1 : 0 }} - {{ Math.min(currentLaggingPage * laggingPageSize, laggingTasksList.length) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ laggingTasksList.length }}</span> mục
          </span>

          <div class="flex items-center gap-2">
            <button 
              @click="currentLaggingPage--" 
              :disabled="currentLaggingPage <= 1"
              class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
            >
              ‹ Trang trước
            </button>

            <span class="px-3 py-1.5 bg-rose-50 text-rose-800 border border-rose-200 rounded-xl font-black">
              Trang {{ currentLaggingPage }} / {{ totalLaggingPages }}
            </span>

            <button 
              @click="currentLaggingPage++" 
              :disabled="currentLaggingPage >= totalLaggingPages"
              class="px-3 py-1.5 bg-white hover:bg-slate-100 border border-slate-200 rounded-xl disabled:opacity-40 font-bold transition shadow-2xs cursor-pointer"
            >
              Trang sau ›
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <UrgeTaskModal 
      v-if="selectedTaskForUrge"
      :isOpen="isUrgeModalOpen"
      :taskId="selectedTaskForUrge.id"
      :taskCode="selectedTaskForUrge.code"
      :taskTitle="selectedTaskForUrge.title"
      :leadAgencyName="selectedTaskForUrge.leadAgency"
      :actualProgressPct="selectedTaskForUrge.actualProgressPct"
      :expectedTargetPct="selectedTaskForUrge.expectedTargetPct"
      :laggingDeltaPct="selectedTaskForUrge.laggingDeltaPct"
      :hasReport="selectedTaskForUrge.hasReport !== false"
      @close="isUrgeModalOpen = false"
      @submitted="onTaskUrged"
    />

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { Doughnut, Bar } from 'vue-chartjs';
import { 
  Chart as ChartJS, Title, Tooltip, Legend, 
  ArcElement, BarElement, CategoryScale, LinearScale 
} from 'chart.js';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import * as XLSX from 'xlsx';
import UrgeTaskModal from '../components/UrgeTaskModal.vue';
import { getApiUrl } from '../config/api';

ChartJS.register(Title, Tooltip, Legend, ArcElement, BarElement, CategoryScale, LinearScale);

const selectedYear = ref(2026);
const selectedDocumentId = ref('');
const documents = ref([]);
const activeViewTab = ref('all'); // 'all', 'Overdue', 'Lagging', 'AtRisk'

const isUrgeModalOpen = ref(false);
const selectedTaskForUrge = ref(null);

const metrics = ref({
  documentId: null,
  totalGoals: 0,
  completedGoals: 0,
  totalTasks: 0,
  completedTasks: 0,
  overallQuantitativeCompletionPct: 0,
  trafficLights: { greenCount: 0, yellowCount: 0, redCount: 0 },
  overdueCount: 0,
  laggingCount: 0,
  atRiskCount: 0,
  staleTasks: [],
  agencyPerformance: []
});

function changeViewTab(tab) {
  activeViewTab.value = tab;
  currentLaggingPage.value = 1;
}

const completedGoalsDisplay = computed(() => metrics.value?.completedGoals ?? 0);
const totalGoalsDisplay = computed(() => metrics.value?.totalGoals ?? 0);
const goalCompletionPct = computed(() => totalGoalsDisplay.value > 0 ? Math.min(100, (completedGoalsDisplay.value / totalGoalsDisplay.value) * 100).toFixed(1) : '0.0');

const completedTasksDisplay = computed(() => metrics.value?.completedTasks ?? 0);
const totalTasksDisplay = computed(() => metrics.value?.totalTasks ?? 0);
const taskCompletionPct = computed(() => totalTasksDisplay.value > 0 ? Math.min(100, (completedTasksDisplay.value / totalTasksDisplay.value) * 100).toFixed(1) : '0.0');

const currentLaggingPage = ref(1);
const laggingPageSize = ref(10);

const filteredStaleTasks = computed(() => {
  const list = metrics.value?.staleTasks || [];
  if (activeViewTab.value === 'all') return list;
  return list.filter(t => t.staleCategory === activeViewTab.value);
});

const laggingTasksList = computed(() => filteredStaleTasks.value);

const totalLaggingPages = computed(() => Math.ceil(laggingTasksList.value.length / laggingPageSize.value) || 1);

const paginatedLaggingTasks = computed(() => {
  const start = (currentLaggingPage.value - 1) * laggingPageSize.value;
  return laggingTasksList.value.slice(start, start + laggingPageSize.value);
});

async function loadDocuments() {
  try {
    const res = await fetch(getApiUrl('/api/documents?pageSize=100'));
    if (res.ok) {
      const data = await res.json();
      documents.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (e) {
    console.error('Lỗi tải danh sách văn bản:', e);
  }
}

async function loadDashboardMetrics() {
  try {
    let url = getApiUrl(`/api/dashboard/metrics?year=${selectedYear.value}`);
    if (selectedDocumentId.value) url += `&documentId=${selectedDocumentId.value}`;
    const res = await fetch(url);
    if (res.ok) metrics.value = await res.json();
  } catch (e) {}
}

function openUrgeModal(task) {
  selectedTaskForUrge.value = task;
  isUrgeModalOpen.value = true;
}

function onTaskUrged(result) {
  toast.success("Đã hoàn tất lưu nhật ký đôn đốc!");
  loadDashboardMetrics();
}

function exportLaggingTasksExcel() {
  if (!laggingTasksList.value || laggingTasksList.value.length === 0) {
    toast.warning("Không có dữ liệu mục tiêu/nhiệm vụ trong danh mục này để xuất!");
    return;
  }

  const now = new Date();
  const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}`;

  const categoryTitleMap = {
    'all': 'DANH SÁCH MỤC TIÊU & NHIỆM VỤ BÁO ĐỘNG (TẤT CẢ)',
    'Overdue': 'DANH SÁCH MỤC TIÊU & NHIỆM VỤ QUÁ HẠN HOÀN THÀNH',
    'Lagging': 'DANH SÁCH MỤC TIÊU & NHIỆM VỤ CHẬM TIẾN ĐỘ',
    'AtRisk': 'DANH SÁCH MỤC TIÊU & NHIỆM VỤ NGUY CƠ CHẬM TIẾN ĐỘ'
  };

  const titleText = categoryTitleMap[activeViewTab.value] || 'DANH SÁCH MỤC TIÊU & NHIỆM VỤ BÁO ĐỘNG';

  // Header rows with Report Title & Export Time
  const data = [
    [titleText],
    [`Thời gian xuất báo cáo: ${timeStr}`],
    [], // Blank separator line
    ["STT", "Trạng Thái", "Loại", "Mã", "Mục Tiêu / Nhiệm Vụ", "Cơ Quan Chủ Trì", "Thực Tế (%)", "Mốc Kế Hoạch (%)", "Chênh Lệch (%)"]
  ];

  laggingTasksList.value.forEach((t, index) => {
    data.push([
      index + 1,
      t.staleCategoryName || 'Cảnh báo',
      t.itemType || 'Nhiệm vụ',
      t.code || '',
      t.title || '',
      t.leadAgency || '',
      t.hasReport === false ? 'Chưa báo cáo' : (t.actualProgressPct !== undefined && t.actualProgressPct !== null ? `${t.actualProgressPct}%` : '0%'),
      t.expectedTargetPct !== undefined && t.expectedTargetPct !== null ? `${t.expectedTargetPct}%` : '0%',
      t.hasReport === false ? 'Chưa báo cáo' : (t.laggingDeltaPct !== undefined && t.laggingDeltaPct !== null ? `${t.laggingDeltaPct}%` : '0%')
    ]);
  });

  // Create worksheet
  const ws = XLSX.utils.aoa_to_sheet(data);

  // Auto-fit column widths dynamically based on content length
  const colWidths = data[3].map((hdr, colIdx) => {
    let maxLen = hdr ? hdr.toString().length : 10;
    for (let r = 4; r < data.length; r++) {
      const cellVal = data[r][colIdx] !== undefined && data[r][colIdx] !== null ? data[r][colIdx].toString() : '';
      if (cellVal.length > maxLen) {
        maxLen = cellVal.length;
      }
    }
    return Math.max(maxLen + 4, 12);
  });

  // Ample width for mission/goal titles
  if (colWidths[4] < 50) colWidths[4] = 50;

  ws['!cols'] = colWidths.map(w => ({ wch: w }));

  // Create workbook and download .xlsx
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, "Nhiệm vụ báo động");

  const dateFileStr = now.toISOString().slice(0, 10);
  XLSX.writeFile(wb, `Danh_sach_nhiem_vu_bao_dong_${dateFileStr}.xlsx`);

  toast.success("Đã xuất file Excel (.xlsx) thành công!");
}

// Chart 1: Doughnut Chart Data & Center Text Plugin
const centerTextPlugin = {
  id: 'centerText',
  beforeDraw(chart) {
    const { ctx } = chart;
    const meta = chart.getDatasetMeta(0);
    if (!meta || !meta.data || !meta.data[0]) return;
    const { x, y } = meta.data[0];

    const val = chart.data?.datasets?.[0]?.data?.[0] ?? 0;

    ctx.save();
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';

    // Label
    ctx.font = 'bold 11px system-ui, -apple-system, sans-serif';
    ctx.fillStyle = '#64748b';
    ctx.fillText('MỨC ĐỘ ĐẠT', x, y - 11);

    // Percentage
    ctx.font = '900 24px system-ui, -apple-system, sans-serif';
    ctx.fillStyle = '#2563eb';
    ctx.fillText(`${val}%`, x, y + 13);

    ctx.restore();
  }
};

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '72%',
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        font: { size: 11, weight: 'bold' },
        usePointStyle: true,
        padding: 14
      }
    }
  }
};

const doughnutChartData = computed(() => {
  const pct = metrics.value?.overallQuantitativeCompletionPct ?? 0;
  return {
    labels: ['Hoàn thành (%)', 'Còn lại (%)'],
    datasets: [
      {
        backgroundColor: ['#2563eb', '#e2e8f0'],
        data: [pct, Math.max(0, 100 - pct)]
      }
    ]
  };
});

// Chart 2: Stacked Bar Chart Data
// Chart 2: Stacked Bar Chart Data & Data Labels Plugin
const barDataLabelsPlugin = {
  id: 'barDataLabels',
  afterDatasetsDraw(chart) {
    const { ctx } = chart;
    chart.data.datasets.forEach((dataset, datasetIndex) => {
      const meta = chart.getDatasetMeta(datasetIndex);
      if (!meta || meta.hidden) return;

      meta.data.forEach((element, index) => {
        const val = dataset.data[index];
        if (val === null || val === undefined || val <= 0) return;

        const { x, y, base } = element;
        const centerY = (y + base) / 2;
        const barHeight = Math.abs(base - y);

        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.font = '900 12px system-ui, -apple-system, sans-serif';

        if (barHeight >= 16) {
          ctx.fillStyle = '#ffffff';
          ctx.fillText(val, x, centerY);
        } else {
          ctx.fillStyle = dataset.backgroundColor || '#1e293b';
          ctx.fillText(val, x, y - 8);
        }
        ctx.restore();
      });
    });
  }
};

const barChartData = computed(() => {
  const agencies = metrics.value?.agencyPerformance || [];
  return {
    labels: agencies.map(a => a.code),
    datasets: [
      {
        label: 'Hoàn thành (Đạt)',
        backgroundColor: '#10b981',
        data: agencies.map(a => a.completed || 0)
      },
      {
        label: 'Nguy cơ chậm tiến độ',
        backgroundColor: '#f59e0b',
        data: agencies.map(a => a.atRisk || 0)
      },
      {
        label: 'Chậm tiến độ & Quá hạn',
        backgroundColor: '#ef4444',
        data: agencies.map(a => a.overdue || 0)
      }
    ]
  };
});

const barChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    x: { 
      stacked: true,
      ticks: {
        font: { size: 11, weight: 'bold' }
      },
      grid: { display: false }
    },
    y: { 
      stacked: true, 
      beginAtZero: true,
      ticks: { 
        precision: 0,
        stepSize: 1,
        font: { size: 11, weight: 'bold' }
      }
    }
  },
  plugins: {
    legend: {
      position: 'top',
      labels: {
        font: { size: 11, weight: 'bold' },
        usePointStyle: true,
        padding: 12
      }
    }
  }
};

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false
};

onMounted(() => {
  loadDocuments();
  loadDashboardMetrics();
});
</script>
