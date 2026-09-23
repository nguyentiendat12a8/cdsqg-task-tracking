<template>
  <div class="w-full bg-white p-4 rounded-2xl border border-slate-200/90 shadow-2xs space-y-3">
    <div v-if="title" class="flex items-center justify-between border-b border-slate-100 pb-2.5">
      <h4 class="text-xs sm:text-sm font-bold text-slate-800 flex items-center gap-2">
        <span>📊</span>
        <span>{{ title }}</span>
      </h4>
    </div>

    <div v-if="hasData" class="relative w-full min-h-[280px] max-h-[360px] flex items-center justify-center">
      <Bar :data="chartData" :options="chartOptions" />
    </div>

    <div v-else class="p-8 text-center text-xs text-slate-400 font-semibold italic">
      Chưa có dữ liệu văn bản quy phạm pháp luật để hiển thị biểu đồ.
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { Bar } from 'vue-chartjs';
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale
} from 'chart.js';

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale);

const props = defineProps({
  statsData: {
    type: Array,
    default: () => []
  },
  title: {
    type: String,
    default: ''
  }
});

const TYPE_COLORS = {
  'Luật': '#dc2626',       // Red
  'Nghị định': '#8b5cf6',  // Purple
  'Thông tư': '#10b981',   // Emerald
  'Quyết định': '#3b82f6', // Blue
  'Chỉ thị': '#f59e0b',    // Amber
  'Nghị quyết': '#06b6d4', // Cyan
  'Quy chế': '#ec4899',    // Pink
  'Quy định': '#d946ef',   // Fuchsia
  'Kế hoạch': '#14b8a6',   // Teal
  'Khác': '#64748b'        // Slate
};

const FALLBACK_COLORS = ['#6366f1', '#a855f7', '#0284c7', '#059669', '#d97706', '#e11d48', '#475569'];

const hasData = computed(() => {
  return Array.isArray(props.statsData) && props.statsData.some(ag => (ag.totalCount || 0) > 0 || Object.keys(ag.typeCounts || {}).length > 0);
});

const chartData = computed(() => {
  if (!props.statsData || props.statsData.length === 0) {
    return { labels: [], datasets: [] };
  }

  // 1. Labels: Short agency names
  const labels = props.statsData.map(ag => {
    let name = ag.agencyName || 'Cơ quan';
    if (name.length > 28) {
      name = name.replace('Bộ Khoa học và Công nghệ', 'Bộ KH&CN')
                 .replace('Bộ Thông tin và Truyền thông', 'Bộ TT&TT')
                 .replace('Bộ Kế hoạch và Đầu tư', 'Bộ KH&ĐT')
                 .replace('UBND TP. Hồ Chí Minh', 'TP.HCM')
                 .replace('UBND Thành phố', 'UBND TP');
    }
    return name;
  });

  // 2. Extract unique Document Types across all agencies
  const docTypeSet = new Set();
  props.statsData.forEach(ag => {
    if (ag.typeCounts) {
      Object.keys(ag.typeCounts).forEach(type => docTypeSet.add(type));
    }
  });

  const docTypes = Array.from(docTypeSet);
  if (docTypes.length === 0) {
    docTypes.push('Quyết định', 'Nghị định', 'Thông tư');
  }

  // 3. Build Stacked Datasets
  const datasets = docTypes.map((docType, idx) => {
    const color = TYPE_COLORS[docType] || FALLBACK_COLORS[idx % FALLBACK_COLORS.length];
    const dataValues = props.statsData.map(ag => ag.typeCounts?.[docType] || 0);

    return {
      label: docType,
      data: dataValues,
      backgroundColor: color,
      borderRadius: 4,
      borderSkipped: false,
      maxBarThickness: 48
    };
  });

  return {
    labels,
    datasets
  };
});

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
      align: 'end',
      labels: {
        usePointStyle: true,
        pointStyle: 'circle',
        boxWidth: 8,
        boxHeight: 8,
        padding: 14,
        font: {
          family: 'system-ui, -apple-system, sans-serif',
          size: 11,
          weight: 'bold'
        },
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
      displayColors: true,
      boxWidth: 8,
      boxHeight: 8,
      callbacks: {
        footer: (tooltipItems) => {
          let sum = 0;
          tooltipItems.forEach(item => {
            sum += item.parsed.y || 0;
          });
          return 'Tổng văn bản: ' + sum;
        }
      }
    }
  },
  scales: {
    x: {
      stacked: true,
      grid: { display: false },
      ticks: {
        font: {
          size: 11,
          weight: 'bold'
        },
        color: '#475569',
        maxRotation: 20,
        minRotation: 0
      }
    },
    y: {
      stacked: true,
      beginAtZero: true,
      ticks: {
        stepSize: 1,
        precision: 0,
        font: {
          size: 11,
          weight: '600'
        },
        color: '#64748b'
      },
      grid: {
        color: '#f1f5f9'
      }
    }
  }
}));
</script>
