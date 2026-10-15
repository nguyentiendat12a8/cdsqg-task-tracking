<template>
  <div class="min-w-0">
    <!-- Chart body -->
    <div class="space-y-4">
      <!-- Chart Container -->
      <div v-if="sortedAgencies.length > 0" class="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs space-y-3">
        <!-- Static Fixed Legend Header Bar -->
        <div class="flex flex-wrap items-center justify-end gap-2.5 bg-slate-50/90 p-3 rounded-xl border border-slate-200/70 text-xs font-semibold">
          <!-- Fixed Visible Status Legend Badges -->
          <div class="flex flex-wrap items-center gap-x-3 gap-y-1.5 text-xs font-bold">

            <div class="flex items-center gap-1.5 text-slate-700">
              <span class="w-2.5 h-2.5 rounded-full bg-rose-500 inline-block"></span>
              <span>Đang t/h quá hạn</span>
            </div>
            <div class="flex items-center gap-1.5 text-slate-700">
              <span class="w-2.5 h-2.5 rounded-full bg-blue-500 inline-block"></span>
              <span>Đang t/h trong hạn</span>
            </div>
            <div class="flex items-center gap-1.5 text-slate-700">
              <span class="w-2.5 h-2.5 rounded-full bg-amber-500 inline-block"></span>
              <span>Sắp tới hạn</span>
            </div>
            <div class="flex items-center gap-1.5 text-slate-700">
              <span class="w-2.5 h-2.5 rounded-full bg-teal-500 inline-block"></span>
              <span>Đã h/t quá hạn</span>
            </div>
            <div class="flex items-center gap-1.5 text-slate-700">
              <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 inline-block"></span>
              <span>Đã h/t trong hạn</span>
            </div>
            <div class="flex items-center gap-1.5 text-slate-700">
              <span class="w-2.5 h-2.5 rounded-full bg-slate-400 inline-block"></span>
              <span>Chưa thực hiện</span>
            </div>
          </div>
        </div>

        <!-- Scrollable Chart Area -->
        <div class="overflow-x-auto custom-scrollbar pb-3 pt-1">
          <div :style="{ width: dynamicChartWidth, height: '490px' }" class="relative min-w-full">
            <Bar :data="chartData" :options="chartOptions" />

          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="p-8 text-center text-xs text-slate-600 font-semibold italic bg-white rounded-xl border border-slate-200">
        Không có dữ liệu đơn vị để hiển thị biểu đồ.
      </div>
    </div>
  </div>
</template>

<script setup>
import { themeColor } from "../shared/themeColors";
import { formatNumber } from "../shared/formatters";
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
  ministriesData: { type: Array, default: () => [] },
  provincesData: { type: Array, default: () => [] },
  othersData: { type: Array, default: () => [] },
  filterType: { type: String, default: 'goals' }
});

const emit = defineEmits(['select-agency']);



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

// Dynamic Chart Width based on number of agencies for horizontal scroll view
const dynamicChartWidth = computed(() => {
  const count = sortedAgencies.value.length;
  if (count <= 10) return '100%';
  const minWidthPerAgency = 84; // Compact columns; labels wrap within each category.
  return `${Math.max(840, count * minWidthPerAgency)}px`;
});

// Helper to format short labels for X axis
function shortenName(name) {
  if (!name) return '';
  let str = name.trim();
  str = str.replace('Bộ Khoa học và Công nghệ', 'Bộ KH&CN')
           .replace('Bộ Thông tin và Truyền thông', 'Bộ TT&TT')
           .replace('Bộ Kế hoạch và Đầu tư', 'Bộ KH&ĐT')
           .replace('Bộ Văn hóa, Thể thao và Du lịch', 'Bộ VHTTDL')
           .replace('Bộ Giáo dục và Đào tạo', 'Bộ GD&ĐT')
           .replace('Bộ Nông nghiệp và Môi trường', 'Bộ NN&MT')
           .replace('Bộ Dân tộc và Tôn giáo', 'Bộ DT & TG')
           .replace('Ngân hàng Nhà nước Việt Nam', 'NHNN VN')
           .replace('Thành phố Hồ Chí Minh', 'TP.HCM')
           .replace('Thành phố Hà Nội', 'TP. Hà Nội')
           .replace('Thành phố Đà Nẵng', 'TP. Đà Nẵng')
           .replace('Thành phố Hải Phòng', 'TP. Hải Phòng')
           .replace('Thành phố Cần Thơ', 'TP. Cần Thơ')
           .replace('Thành phố Huế', 'TP. Huế')
           .replace('Thành phố Bắc Ninh', 'TP. Bắc Ninh')
           .replace('Thành phố Quảng Ninh', 'TP. Quảng Ninh')
           .replace('Thành phố Đồng Nai', 'TP. Đồng Nai')
           .replace('Thành phố', 'TP.')
           .replace('Văn phòng Chính phủ', 'VP Chính phủ')
           .replace('Thanh tra Chính phủ', 'Thanh tra CP')
           .replace('Công ty TNHH một thành viên', 'Cty TNHH MTV')
           .replace('Công ty TNHH MTV', 'Cty TNHH MTV')
           .replace('Công ty TNHH', 'Cty TNHH')
           .replace('Công ty Cổ phần', 'Cty CP')
           .replace('Tổng Công ty', 'Tổng Cty')
           .replace('Nhà xuất bản', 'NXB')
           .replace('Ủy ban', 'UB')
           .replace('UBND ', '');
  return str;
}

// Format X axis labels: wrap long names into at most 3 short lines, trailing with '...' if truncated
function formatXAxisLabel(name, maxLines = 3, maxCharsPerLine = 10) {
  if (!name) return '';
  const str = shortenName(name);
  const words = str.split(/\s+/).filter(Boolean);
  if (!words.length) return '';

  const lines = [];
  let currentLine = '';

  for (let i = 0; i < words.length; i++) {
    const word = words[i];

    // If on the last allowed line, determine whether remaining words fit or require truncation
    if (lines.length === maxLines - 1) {
      const remainingWords = words.slice(i);
      const fullRemaining = (currentLine ? currentLine + ' ' : '') + remainingWords.join(' ');
      if (fullRemaining.length <= maxCharsPerLine) {
        lines.push(fullRemaining);
        currentLine = '';
        break;
      }

      // Needs truncation: reserve 3 characters for '...'
      const targetMax = Math.max(3, maxCharsPerLine - 3);
      let lastLine = currentLine;
      for (let j = i; j < words.length; j++) {
        const test = lastLine ? lastLine + ' ' + words[j] : words[j];
        if (test.length <= targetMax) {
          lastLine = test;
        } else {
          if (!lastLine) {
            lastLine = words[j].slice(0, targetMax);
          }
          break;
        }
      }
      lines.push((lastLine ? lastLine.trim() : '') + '...');
      currentLine = '';
      break;
    }

    // For lines before the last line
    if (!currentLine) {
      currentLine = word.length <= maxCharsPerLine ? word : word.slice(0, maxCharsPerLine);
    } else {
      if ((currentLine + ' ' + word).length <= maxCharsPerLine) {
        currentLine += ' ' + word;
      } else {
        lines.push(currentLine);
        currentLine = word.length <= maxCharsPerLine ? word : word.slice(0, maxCharsPerLine);
      }
    }
  }

  if (currentLine && lines.length < maxLines) {
    lines.push(currentLine);
  }

  return lines;
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
  const labels = sortedAgencies.value.map(ag => formatXAxisLabel(ag.name));

  const datasetConfigs = [
    { label: 'Đang t/h quá hạn', keyG: 'goalInProgressOverdue', keyT: 'taskInProgressOverdue', keyA: 'inProgressOverdue', color: themeColor('rose-500') },
    { label: 'Đang t/h trong hạn', keyG: 'goalInProgressOnTime', keyT: 'taskInProgressOnTime', keyA: 'inProgressOnTime', color: themeColor('blue-500') },
    { label: 'Sắp tới hạn', keyG: 'goalExpiringSoon', keyT: 'taskExpiringSoon', keyA: 'expiringSoon', color: themeColor('amber-500') },
    { label: 'Đã h/t quá hạn', keyG: 'goalCompletedOverdue', keyT: 'taskCompletedOverdue', keyA: 'completedOverdue', color: themeColor('teal-500') },
    { label: 'Đã h/t trong hạn', keyG: 'goalCompletedOnTime', keyT: 'taskCompletedOnTime', keyA: 'completedOnTime', color: themeColor('emerald-500') },
    { label: 'Chưa thực hiện', keyG: 'goalNotStarted', keyT: 'taskNotStarted', keyA: 'notStarted', color: themeColor('slate-400') }
  ];

  const datasets = datasetConfigs.map(cfg => ({
    label: cfg.label,
    data: getStatValues(cfg.keyG, cfg.keyT, cfg.keyA),
    backgroundColor: cfg.color,
    borderRadius: 4,
    maxBarThickness: 38,
    barPercentage: 0.85,
    categoryPercentage: 0.9
  }));

  return { labels, datasets };
});

const sharedOptions = computed(() => ({
  locale: 'vi-VN',
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
      display: false
    },
    tooltip: {
      backgroundColor: '#0f172a',
      titleColor: '#f8fafc',
      bodyColor: '#e2e8f0',
      titleFont: { size: 12, weight: 'bold' },
      bodyFont: { size: 12 },
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
          return `Tổng số ${filterNoun.value}: ${formatNumber(sum)}`;
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
        font: { size: 12, weight: '600', lineHeight: 1.25 },
        color: '#334155',
        maxRotation: 0,
        minRotation: 0,
        autoSkip: false,
        padding: 6
      }
    },
    y: {
      stacked: true,
      beginAtZero: true,
      ticks: {
        precision: 0,
        font: { size: 12, weight: '600' },
        color: '#64748b'
      },
      grid: { color: '#f1f5f9' }
    }
  }
}));

</script>
