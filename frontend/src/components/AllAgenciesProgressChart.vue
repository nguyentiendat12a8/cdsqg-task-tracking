<template>
  <div class="bg-white rounded-2xl shadow-sm border border-slate-200/80 overflow-hidden transition-all duration-300">
    <!-- Header Card -->
    <div class="p-4 flex items-center justify-between border-b border-slate-100 bg-slate-50/50">
      <div class="flex items-center gap-3 min-w-0">
        <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-600 to-indigo-700 text-white flex items-center justify-center font-bold text-lg shadow-sm shrink-0">
          📊
        </div>
        <div class="min-w-0">
          <h3 class="text-base font-bold text-slate-800 leading-snug flex items-center gap-2 flex-wrap">
            <span>Biểu đồ tổng quan {{ filterNoun }}</span>
            <span class="text-[11px] font-semibold text-slate-500 bg-slate-100 px-2 py-0.5 rounded-lg">
              (Bộ, Ngành, Địa phương, Cơ quan, Doanh nghiệp)
            </span>
          </h3>
          <p class="text-xs text-slate-500 truncate mt-0.5">
            Tự động sắp xếp đơn vị có nhiều {{ filterNoun }} lên trước • Phân rã theo trạng thái thực hiện
          </p>
        </div>
      </div>

      <div class="flex items-center gap-3 shrink-0">
        <span class="text-xs font-bold text-indigo-700 bg-indigo-50 px-3 py-1.5 rounded-xl border border-indigo-200/60 hidden sm:inline-block">
          {{ sortedAgencies.length }} Đơn vị được xếp hạng
        </span>
      </div>
    </div>

    <!-- Always Visible Body -->
    <div class="p-4 sm:p-5 space-y-4 bg-slate-50/20">
      <!-- Toolbar: View Mode Switch & Summary Badges -->
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 bg-white p-3 rounded-xl border border-slate-200/70 shadow-2xs">
        <!-- Summary Stats -->
        <div class="flex items-center gap-3 flex-wrap text-xs font-bold">
          <span class="text-slate-600 bg-slate-100 px-2.5 py-1 rounded-lg">
            🏢 Tổng {{ sortedAgencies.length }} đơn vị
          </span>
          <span class="text-blue-800 bg-blue-50 px-2.5 py-1 rounded-lg border border-blue-200/60">
            📌 {{ grandTotalItems }} {{ filterNoun }}
          </span>
          <span class="text-emerald-800 bg-emerald-50 px-2.5 py-1 rounded-lg border border-emerald-200/60">
            ✅ {{ grandTotalCompleted }} Đã hoàn thành
          </span>
          <span class="text-rose-800 bg-rose-50 px-2.5 py-1 rounded-lg border border-rose-200/60" v-if="grandTotalOverdue > 0">
            ⚠️ {{ grandTotalOverdue }} Quá hạn
          </span>
        </div>

        <!-- Chart Type Switcher -->
        <div class="flex items-center gap-1 bg-slate-100 p-1 rounded-xl shrink-0 text-xs font-bold">
          <button 
            @click.stop="chartType = 'bar'" 
            :class="['px-3 py-1 rounded-lg transition cursor-pointer flex items-center gap-1.5', chartType === 'bar' ? 'bg-white text-indigo-700 shadow-2xs' : 'text-slate-600 hover:text-slate-900']"
          >
            <span>📊 Cột Chồng</span>
          </button>
          <button 
            @click.stop="chartType = 'line'" 
            :class="['px-3 py-1 rounded-lg transition cursor-pointer flex items-center gap-1.5', chartType === 'line' ? 'bg-white text-indigo-700 shadow-2xs' : 'text-slate-600 hover:text-slate-900']"
          >
            <span>📈 Biểu đồ Đường</span>
          </button>
        </div>
      </div>

      <!-- Chart Container -->
      <div v-if="sortedAgencies.length > 0" class="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
        <div class="relative w-full h-[380px] sm:h-[440px]">
          <Bar v-if="chartType === 'bar'" :data="chartData" :options="chartOptions" />
          <Line v-else :data="lineChartData" :options="lineChartOptions" />
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="p-8 text-center text-xs text-slate-400 font-semibold italic bg-white rounded-xl border border-slate-200">
        Không có dữ liệu đơn vị để hiển thị biểu đồ.
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { Bar, Line } from 'vue-chartjs';
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale
} from 'chart.js';

ChartJS.register(Title, Tooltip, Legend, BarElement, LineElement, PointElement, CategoryScale, LinearScale);

const props = defineProps({
  ministriesData: { type: Array, default: () => [] },
  provincesData: { type: Array, default: () => [] },
  othersData: { type: Array, default: () => [] },
  filterType: { type: String, default: 'goals' }
});

const emit = defineEmits(['select-agency']);

// Always open by default as requested
const chartType = ref('bar'); // Default to column/bar chart as requested ('bar' or 'line')

const filterNoun = computed(() => {
  if (props.filterType === 'goals') return 'mục tiêu';
  if (props.filterType === 'tasks') return 'nhiệm vụ';
  return 'mục tiêu/nhiệm vụ';
});

const isSpecialAgencyCode = (code) => ['ALL_AGENCIES', 'ALL_MINISTRIES', 'ALL_PROVINCES', 'ALL_PROVINCES_UBND', 'ALL_MINISTRIES_DIRECT'].includes(code);

// Combine all agencies & sort descending by total count
const sortedAgencies = computed(() => {
  const combined = [
    ...(props.ministriesData || []),
    ...(props.provincesData || []),
    ...(props.othersData || [])
  ];

  // Exclude special pseudo agencies
  const filtered = combined.filter(a => !isSpecialAgencyCode(a.code) && !(a.name && (a.name.startsWith('Các bộ, ngành') || a.name.startsWith('Các địa phương'))));

  // Deduplicate by AgencyId
  const uniqueMap = new Map();
  filtered.forEach(item => {
    if (item.agencyId && !uniqueMap.has(item.agencyId)) {
      uniqueMap.set(item.agencyId, item);
    }
  });

  const list = Array.from(uniqueMap.values());

  // Helper to extract total count according to filterType
  const getItemCount = (ag) => {
    if (props.filterType === 'goals') return ag.totalGoals ?? ag.totalItems ?? 0;
    if (props.filterType === 'tasks') return ag.totalTasks ?? ag.totalItems ?? 0;
    return ag.totalItems ?? ((ag.totalGoals || 0) + (ag.totalTasks || 0));
  };

  // Sort descending by total count
  return list.sort((a, b) => {
    const cA = getItemCount(a);
    const cB = getItemCount(b);
    if (cB !== cA) return cB - cA;
    return (a.name || '').localeCompare(b.name || '', 'vi');
  });
});

const grandTotalItems = computed(() => {
  return sortedAgencies.value.reduce((acc, ag) => {
    if (props.filterType === 'goals') return acc + (ag.totalGoals ?? ag.totalItems ?? 0);
    if (props.filterType === 'tasks') return acc + (ag.totalTasks ?? ag.totalItems ?? 0);
    return acc + (ag.totalItems ?? 0);
  }, 0);
});

const grandTotalCompleted = computed(() => {
  return sortedAgencies.value.reduce((acc, ag) => {
    if (props.filterType === 'goals') return acc + (ag.goalCompletedOnTime || 0) + (ag.goalCompletedOverdue || 0);
    if (props.filterType === 'tasks') return acc + (ag.taskCompletedOnTime || 0) + (ag.taskCompletedOverdue || 0);
    return acc + (ag.completedOnTime || 0) + (ag.completedOverdue || 0);
  }, 0);
});

const grandTotalOverdue = computed(() => {
  return sortedAgencies.value.reduce((acc, ag) => {
    if (props.filterType === 'goals') return acc + (ag.goalInProgressOverdue || 0);
    if (props.filterType === 'tasks') return acc + (ag.taskInProgressOverdue || 0);
    return acc + (ag.inProgressOverdue || 0);
  }, 0);
});

// Helper to format short labels for X axis
function shortenName(name) {
  if (!name) return '';
  let str = name.trim();
  if (str.length > 22) {
    str = str.replace('Bộ Khoa học và Công nghệ', 'Bộ KH&CN')
             .replace('Bộ Thông tin và Truyền thông', 'Bộ TT&TT')
             .replace('Bộ Kế hoạch và Đầu tư', 'Bộ KH&ĐT')
             .replace('Bộ Văn hóa, Thể thao và Du lịch', 'Bộ VHTTDL')
             .replace('Bộ Giáo dục và Đào tạo', 'Bộ GD&ĐT')
             .replace('Bộ Nông nghiệp và Môi trường', 'Bộ NN&MT')
             .replace('Thành phố Hồ Chí Minh', 'TP.HCM')
             .replace('Thành phố Hà Nội', 'Hà Nội')
             .replace('Thành phố Đà Nẵng', 'Đà Nẵng')
             .replace('Thành phố Hải Phòng', 'Hải Phòng')
             .replace('Thành phố Cần Thơ', 'Cần Thơ')
             .replace('Thành phố', 'TP.')
             .replace('UBND ', '');
  }
  return str;
}

// Data extraction helpers per status
function getStatValues(statusKeyGoal, statusKeyTask, statusKeyAll) {
  return sortedAgencies.value.map(ag => {
    if (props.filterType === 'goals') return ag[statusKeyGoal] || 0;
    if (props.filterType === 'tasks') return ag[statusKeyTask] || 0;
    return ag[statusKeyAll] || 0;
  });
}

// 1. Stacked Bar Chart Data
const chartData = computed(() => {
  const labels = sortedAgencies.value.map(ag => shortenName(ag.name));

  const datasetConfigs = [
    { label: 'Đang t/h quá hạn', keyG: 'goalInProgressOverdue', keyT: 'taskInProgressOverdue', keyA: 'inProgressOverdue', color: '#f43f5e' },
    { label: 'Đang t/h trong hạn', keyG: 'goalInProgressOnTime', keyT: 'taskInProgressOnTime', keyA: 'inProgressOnTime', color: '#3b82f6' },
    { label: 'Sắp tới hạn', keyG: 'goalExpiringSoon', keyT: 'taskExpiringSoon', keyA: 'expiringSoon', color: '#f59e0b' },
    { label: 'Đã h/t quá hạn', keyG: 'goalCompletedOverdue', keyT: 'taskCompletedOverdue', keyA: 'completedOverdue', color: '#14b8a6' },
    { label: 'Đã h/t trong hạn', keyG: 'goalCompletedOnTime', keyT: 'taskCompletedOnTime', keyA: 'completedOnTime', color: '#10b981' },
    { label: 'Chưa thực hiện', keyG: 'goalNotStarted', keyT: 'taskNotStarted', keyA: 'notStarted', color: '#94a3b8' }
  ];

  const datasets = datasetConfigs.map(cfg => ({
    label: cfg.label,
    data: getStatValues(cfg.keyG, cfg.keyT, cfg.keyA),
    backgroundColor: cfg.color,
    borderRadius: 3,
    maxBarThickness: 36
  }));

  return { labels, datasets };
});

// 2. Line Chart Data
const lineChartData = computed(() => {
  const labels = sortedAgencies.value.map(ag => shortenName(ag.name));

  const datasetConfigs = [
    { label: 'Đang t/h quá hạn', keyG: 'goalInProgressOverdue', keyT: 'taskInProgressOverdue', keyA: 'inProgressOverdue', color: '#f43f5e' },
    { label: 'Đang t/h trong hạn', keyG: 'goalInProgressOnTime', keyT: 'taskInProgressOnTime', keyA: 'inProgressOnTime', color: '#3b82f6' },
    { label: 'Sắp tới hạn', keyG: 'goalExpiringSoon', keyT: 'taskExpiringSoon', keyA: 'expiringSoon', color: '#f59e0b' },
    { label: 'Đã h/t quá hạn', keyG: 'goalCompletedOverdue', keyT: 'taskCompletedOverdue', keyA: 'completedOverdue', color: '#14b8a6' },
    { label: 'Đã h/t trong hạn', keyG: 'goalCompletedOnTime', keyT: 'taskCompletedOnTime', keyA: 'completedOnTime', color: '#10b981' },
    { label: 'Chưa thực hiện', keyG: 'goalNotStarted', keyT: 'taskNotStarted', keyA: 'notStarted', color: '#94a3b8' }
  ];

  const datasets = datasetConfigs.map(cfg => ({
    label: cfg.label,
    data: getStatValues(cfg.keyG, cfg.keyT, cfg.keyA),
    borderColor: cfg.color,
    backgroundColor: cfg.color,
    tension: 0.3,
    pointRadius: 4,
    pointHoverRadius: 7,
    borderWidth: 2.5
  }));

  // Add Total Items Line
  const totalValues = sortedAgencies.value.map(ag => {
    if (props.filterType === 'goals') return ag.totalGoals ?? ag.totalItems ?? 0;
    if (props.filterType === 'tasks') return ag.totalTasks ?? ag.totalItems ?? 0;
    return ag.totalItems ?? 0;
  });

  datasets.unshift({
    label: `Tổng số ${filterNoun.value}`,
    data: totalValues,
    borderColor: '#6366f1',
    backgroundColor: '#6366f1',
    borderDash: [5, 5],
    tension: 0.3,
    pointRadius: 5,
    pointHoverRadius: 8,
    borderWidth: 3
  });

  return { labels, datasets };
});

const sharedOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  onClick: (event, elements) => {
    if (elements && elements.length > 0) {
      const idx = elements[0].index;
      const agency = sortedAgencies.value[idx];
      if (agency) {
        emit('select-agency', agency);
      }
    }
  },
  plugins: {
    legend: {
      position: 'top',
      align: 'end',
      labels: {
        usePointStyle: true,
        pointStyle: 'circle',
        boxWidth: 8,
        boxHeight: 8,
        padding: 12,
        font: { family: 'system-ui, sans-serif', size: 11, weight: 'bold' },
        color: '#334155'
      }
    },
    tooltip: {
      backgroundColor: '#0f172a',
      titleColor: '#f8fafc',
      bodyColor: '#e2e8f0',
      titleFont: { size: 12, weight: 'bold' },
      bodyFont: { size: 11 },
      padding: 10,
      cornerRadius: 8,
      callbacks: {
        title: (items) => {
          if (!items || items.length === 0) return '';
          const idx = items[0].dataIndex;
          const ag = sortedAgencies.value[idx];
          return ag ? ag.name : items[0].label;
        },
        footer: (tooltipItems) => {
          let sum = 0;
          tooltipItems.forEach(item => {
            sum += item.parsed.y || 0;
          });
          return `Tổng số ${filterNoun.value}: ${sum}`;
        }
      }
    }
  }
}));

const chartOptions = computed(() => ({
  ...sharedOptions.value,
  scales: {
    x: {
      stacked: true,
      grid: { display: false },
      ticks: {
        font: { size: 10, weight: 'bold' },
        color: '#475569',
        maxRotation: 45,
        minRotation: 15
      }
    },
    y: {
      stacked: true,
      beginAtZero: true,
      ticks: {
        precision: 0,
        font: { size: 10, weight: '600' },
        color: '#64748b'
      },
      grid: { color: '#f1f5f9' }
    }
  }
}));

const lineChartOptions = computed(() => ({
  ...sharedOptions.value,
  scales: {
    x: {
      grid: { display: false },
      ticks: {
        font: { size: 10, weight: 'bold' },
        color: '#475569',
        maxRotation: 45,
        minRotation: 15
      }
    },
    y: {
      beginAtZero: true,
      ticks: {
        precision: 0,
        font: { size: 10, weight: '600' },
        color: '#64748b'
      },
      grid: { color: '#f1f5f9' }
    }
  }
}));
</script>
