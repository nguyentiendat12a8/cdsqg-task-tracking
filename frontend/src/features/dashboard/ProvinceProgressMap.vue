<template>
  <section class="province-map-container rounded-2xl border border-slate-200 overflow-hidden bg-white" :class="{ 'fit-height': fitHeight }">
    <div class="p-3 border-b border-slate-200 flex flex-wrap items-center justify-between gap-2">
      <div>
        <h4 class="text-sm font-bold text-slate-900">Bản đồ hoàn thành {{ noun }}</h4>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <div class="flex items-center gap-1" role="group" aria-label="Điều khiển bản đồ">
          <button type="button" class="ui-single-line w-9 rounded-lg border border-slate-200 font-bold disabled:opacity-40" aria-label="Thu nhỏ bản đồ" :disabled="zoom <= 1" @click="changeZoom(-1)">−</button>
          <span class="text-xs tabular-nums w-10 text-center" aria-live="polite">{{ Math.round(zoom * 100) }}%</span>
          <button type="button" class="ui-single-line w-9 rounded-lg border border-slate-200 font-bold disabled:opacity-40" aria-label="Phóng to bản đồ" :disabled="zoom >= 4" @click="changeZoom(1)">+</button>
          <button type="button" class="ui-single-line px-2 rounded-lg border border-slate-200 text-xs" aria-label="Đặt lại bản đồ" @click="resetMap">Đặt lại</button>
        </div>
        <slot name="type-switch" />
      </div>
    </div>

    <div class="province-map-body">
      <!-- Cột trái: Bản đồ SVG -->
      <div class="province-map-canvas min-w-0 bg-slate-50 p-3 border-b border-slate-200" data-testid="province-map-canvas">
        <div class="map-legend flex flex-wrap gap-x-3 gap-y-1.5 text-xs text-slate-800 mb-2 rounded-lg border border-sky-200 bg-white px-2 py-2">
          <span v-for="band in bands" :key="band.label" class="flex items-center gap-2">
            <i class="w-3 h-3 rounded-sm border border-slate-300" :style="{ backgroundColor: band.color }"></i>
            {{ band.label }}
          </span>
        </div>

        <svg :viewBox="mapViewBox" class="w-full touch-none select-none cursor-grab active:cursor-grabbing" :class="compact ? 'h-[320px] sm:h-[360px]' : 'h-[360px] sm:h-[420px] xl:h-[560px]'" tabindex="0" aria-label="Bản đồ 34 tỉnh thành Việt Nam" aria-describedby="province-map-help" @pointerdown="startPan" @pointermove="movePan" @pointerup="endPan" @pointercancel="endPan" @click.capture="ignoreDragClick" @keydown="mapKeyboard">
          <!-- Các tỉnh thành phố trên bản đồ -->
          <path
            v-for="province in provinces"
            :key="province.code"
            :d="province.path"
            fill-rule="evenodd"
            :fill="color(province.rate)"
            :stroke="selectedCode === province.code ? '#0f172a' : '#fff'"
            :stroke-width="selectedCode === province.code ? 2 : 0.6"
            vector-effect="non-scaling-stroke"
            tabindex="0"
            role="button"
            :aria-label="`${province.name}: ${rateText(province)}`"
            :aria-pressed="selectedCode === province.code"
            class="province-shape"
            @click="selectedCode = province.code"
            @keydown.enter.prevent="selectedCode = province.code"
            @keydown.space.prevent="selectedCode = province.code"
          >
            <title>{{ province.name }}: {{ rateText(province) }}</title>
          </path>

          <!-- Tên Biển Đông (đặt ở vùng biển thoáng ngoài khơi giữa Hoàng Sa và Trường Sa, hoàn toàn không chạm đất liền) -->
          <text
            x="450"
            y="435"
            text-anchor="middle"
            class="select-none font-semibold text-xs fill-slate-400 tracking-[0.3em] uppercase pointer-events-none"
          >
            BIỂN ĐÔNG
          </text>

          <!-- Quần đảo Hoàng Sa (thuộc Thành phố Đà Nẵng) -->
          <g
            class="cursor-pointer group"
            tabindex="0"
            role="button"
            :aria-pressed="selectedCode === '21'"
            aria-label="Quần đảo Hoàng Sa (Thành phố Đà Nẵng)"
            @click="selectedCode = '21'"
            @keydown.enter.prevent="selectedCode = '21'"
            @keydown.space.prevent="selectedCode = '21'"
          >
            <!-- Khung chú thích địa phận Hoàng Sa -->
            <rect
              x="372"
              y="272"
              width="86"
              height="80"
              rx="6"
              :fill="selectedCode === '21' ? '#3b82f6' : '#2563eb'"
              :fill-opacity="selectedCode === '21' ? 0.12 : 0.04"
              :stroke="selectedCode === '21' ? '#1d4ed8' : '#94a3b8'"
              :stroke-width="selectedCode === '21' ? 1.8 : 1"
              stroke-dasharray="4,3"
              class="transition-all"
            />
            <!-- Tên Quần đảo Hoàng Sa -->
            <text
              x="415"
              y="266"
              text-anchor="middle"
              :class="['select-none font-bold text-xs tracking-wide transition-colors', selectedCode === '21' ? 'fill-blue-800' : 'fill-slate-800']"
            >
              QUẦN ĐẢO HOÀNG SA
            </text>
            <text
              x="415"
              y="364"
              text-anchor="middle"
              :class="['select-none text-xs font-semibold transition-colors', selectedCode === '21' ? 'fill-blue-700' : 'fill-slate-500']"
            >
              (TP. Đà Nẵng)
            </text>
            <!-- Các điểm đảo, rạn san hô Hoàng Sa rõ nét -->
            <circle
              v-for="(island, idx) in hoangSaIslands"
              :key="'hs-' + idx"
              :cx="island.x"
              :cy="island.y"
              :r="island.r"
              :fill="color(daNangProvince?.rate)"
              :stroke="selectedCode === '21' ? '#0f172a' : '#334155'"
              :stroke-width="selectedCode === '21' ? 1.5 : 0.8"
              class="transition-all"
            >
              <title>{{ island.name }} (Quần đảo Hoàng Sa, TP. Đà Nẵng)</title>
            </circle>
            <title>Quần đảo Hoàng Sa (TP. Đà Nẵng) - {{ rateText(daNangProvince) }}</title>
          </g>

          <!-- Quần đảo Trường Sa (thuộc Tỉnh Khánh Hòa) -->
          <g
            class="cursor-pointer group"
            tabindex="0"
            role="button"
            :aria-pressed="selectedCode === '23'"
            aria-label="Quần đảo Trường Sa (Tỉnh Khánh Hòa)"
            @click="selectedCode = '23'"
            @keydown.enter.prevent="selectedCode = '23'"
            @keydown.space.prevent="selectedCode = '23'"
          >
            <!-- Khung chú thích địa phận Trường Sa -->
            <rect
              x="350"
              y="515"
              width="295"
              height="185"
              rx="8"
              :fill="selectedCode === '23' ? '#3b82f6' : '#2563eb'"
              :fill-opacity="selectedCode === '23' ? 0.12 : 0.04"
              :stroke="selectedCode === '23' ? '#1d4ed8' : '#94a3b8'"
              :stroke-width="selectedCode === '23' ? 1.8 : 1"
              stroke-dasharray="4,3"
              class="transition-all"
            />
            <!-- Tên Quần đảo Trường Sa -->
            <text
              x="500"
              y="510"
              text-anchor="middle"
              :class="['select-none font-bold text-xs tracking-wide transition-colors', selectedCode === '23' ? 'fill-blue-800' : 'fill-slate-800']"
            >
              QUẦN ĐẢO TRƯỜNG SA
            </text>
            <text
              x="500"
              y="712"
              text-anchor="middle"
              :class="['select-none text-xs font-semibold transition-colors', selectedCode === '23' ? 'fill-blue-700' : 'fill-slate-500']"
            >
              (Tỉnh Khánh Hòa)
            </text>
            <!-- Các điểm đảo, rạn san hô Trường Sa rõ nét -->
            <circle
              v-for="(island, idx) in truongSaIslands"
              :key="'ts-' + idx"
              :cx="island.x"
              :cy="island.y"
              :r="island.r"
              :fill="color(khanhHoaProvince?.rate)"
              :stroke="selectedCode === '23' ? '#0f172a' : '#334155'"
              :stroke-width="selectedCode === '23' ? 1.5 : 0.8"
              class="transition-all"
            >
              <title>{{ island.name }} (Quần đảo Trường Sa, Tỉnh Khánh Hòa)</title>
            </circle>
            <title>Quần đảo Trường Sa (Tỉnh Khánh Hòa) - {{ rateText(khanhHoaProvince) }}</title>
          </g>
        </svg>
        <p id="province-map-help" class="sr-only">Kéo để di chuyển · Dùng +/− để phóng to, thu nhỏ.</p>
      </div>

      <!-- Cột phải: Tìm kiếm & Danh sách địa phương -->
      <div class="p-3 flex flex-col min-w-0" data-testid="province-map-directory">
          <div class="flex flex-wrap items-center justify-between gap-2">
            <h5 class="text-sm font-bold text-slate-800">Danh sách địa phương</h5>
          </div>
        <div id="province-map-list" class="mt-3">
        <div class="space-y-3 mb-4">
          <div class="relative">
            <input
              id="province-map-search"
              v-model="search"
              type="search"
              placeholder="Tìm kiếm tỉnh, thành phố..."
              class="ui-control w-full pl-9 pr-3 rounded-xl border border-slate-300 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
            />
            <svg class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <p class="text-xs text-slate-500 italic">
            Tỷ lệ = số {{ noun }} hoàn thành / số {{ noun }} được giao (gồm đúng hạn &amp; quá hạn).
          </p>
        </div>

        <div class="province-map-directory-scroll flex-1 max-h-[240px] overflow-y-auto overscroll-contain border border-slate-200 rounded-xl divide-y divide-slate-100 bg-white" aria-label="Danh sách 34 tỉnh thành">
          <div
            v-for="province in visibleProvinces"
            :key="province.code"
            :class="['w-full flex items-center justify-between gap-3 px-3 py-2 text-xs text-left transition cursor-pointer', selectedCode === province.code ? 'bg-blue-50/80 text-blue-900 font-semibold' : 'text-slate-700 hover:bg-slate-50']"
            @click="selectedCode = province.code"
          >
            <div class="flex items-center gap-2.5 min-w-0">
              <span class="w-2.5 h-2.5 rounded-full shrink-0 border border-slate-300" :style="{ backgroundColor: color(province.rate) }"></span>
              <span class="truncate">{{ province.name }}</span>
            </div>
            <div class="flex items-center gap-2 shrink-0">
              <span :class="['text-xs font-semibold px-2 py-0.5 rounded-md', province.rate != null ? 'bg-slate-100 text-slate-800' : 'text-slate-400']">
                {{ rateText(province) }}
              </span>
              <button
                v-if="province.agency"
                type="button"
                @click.stop="$emit('select-agency', province.agency)"
                class="p-1 text-slate-400 hover:text-blue-600 rounded transition"
                title="Xem danh sách chi tiết"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </div>
          </div>
          <p v-if="!visibleProvinces.length" class="p-4 text-sm text-slate-600">Không tìm thấy địa phương phù hợp.</p>
        </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { computed, ref } from 'vue';
import geography from './map/provinces.json';
import { formatPercent } from '../../shared/formatters';

const props = defineProps({
  fitHeight: { type: Boolean, default: false },
  compact: { type: Boolean, default: false },
  agencies: { type: Array, default: () => [] },
  filterType: { type: String, default: 'goals' }
});

defineEmits(['select-agency']);

const selectedCode = ref(null);
const search = ref('');
const initialBounds = geography.viewBox.split(/[ ,]+/).map(Number);
const zoom = ref(1);
const mapCenter = ref([initialBounds[0] + initialBounds[2] / 2, initialBounds[1] + initialBounds[3] / 2]);
const mapViewBox = computed(() => {
  const width = initialBounds[2] / zoom.value, height = initialBounds[3] / zoom.value;
  return `${mapCenter.value[0] - width / 2} ${mapCenter.value[1] - height / 2} ${width} ${height}`;
});
function changeZoom(direction) { zoom.value = Math.max(1, Math.min(4, zoom.value * (direction > 0 ? 1.25 : .8))); }
function resetMap() {
  zoom.value = 1;
  mapCenter.value = [initialBounds[0] + initialBounds[2] / 2, initialBounds[1] + initialBounds[3] / 2];
}
let pan = null, dragged = false;
function startPan(event) {
  if (!event.isPrimary || event.button !== 0) return;
  dragged = false;
  pan = { id: event.pointerId, x: event.clientX, y: event.clientY, center: [...mapCenter.value], scale: event.currentTarget.getScreenCTM()?.a || 1 };
}
function movePan(event) {
  if (!pan || event.pointerId !== pan.id) return;
  const dx = event.clientX - pan.x, dy = event.clientY - pan.y;
  if (!dragged && Math.hypot(dx, dy) < 5) return;
  if (!dragged) event.currentTarget.setPointerCapture(event.pointerId);
  dragged = true;
  mapCenter.value = [pan.center[0] - dx / pan.scale, pan.center[1] - dy / pan.scale];
}
function endPan(event) {
  if (pan?.id !== event.pointerId) return;
  if (event.currentTarget.hasPointerCapture(event.pointerId)) event.currentTarget.releasePointerCapture(event.pointerId);
  pan = null;
}
function ignoreDragClick(event) {
  if (dragged) { event.preventDefault(); event.stopPropagation(); dragged = false; }
}
function mapKeyboard(event) {
  const directions = { ArrowLeft: [-1, 0], ArrowRight: [1, 0], ArrowUp: [0, -1], ArrowDown: [0, 1] };
  if (directions[event.key]) {
    event.preventDefault();
    const [x, y] = directions[event.key];
    mapCenter.value = [mapCenter.value[0] + x * initialBounds[2] / zoom.value / 10, mapCenter.value[1] + y * initialBounds[3] / zoom.value / 10];
  } else if (['+', '=', '-', 'Home'].includes(event.key)) {
    event.preventDefault();
    if (event.key === 'Home') resetMap(); else changeZoom(event.key === '-' ? -1 : 1);
  }
}

const noun = computed(() => (props.filterType === 'tasks' ? 'nhiệm vụ' : 'mục tiêu'));

const normalize = name =>
  String(name || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .toLowerCase()
    .replace(/^(ubnd\s*)?(tinh|thanh pho|tp\.?)?\s*/, '')
    .trim();

const provinces = computed(() =>
  geography.provinces.map(province => {
    const agency = props.agencies.find(a => normalize(a.name) === normalize(province.name));
    const raw = agency?.[props.filterType === 'tasks' ? 'taskCompletionRate' : 'goalCompletionRate'];
    const displayName = agency?.name || province.name;
    return {
      ...province,
      name: displayName,
      agency,
      rate: typeof raw === 'number' && Number.isFinite(raw) ? raw : null
    };
  })
);

const daNangProvince = computed(() => provinces.value.find(p => p.code === '21'));
const khanhHoaProvince = computed(() => provinces.value.find(p => p.code === '23'));


const visibleProvinces = computed(() =>
  provinces.value
    .filter(p => normalize(p.name).includes(normalize(search.value)))
    .sort((a, b) => a.name.localeCompare(b.name, 'vi'))
);

const bands = [
  { label: 'Chưa có tỷ lệ', color: '#cbd5e1' },
  { label: 'Dưới 25%', color: '#fef3c7' },
  { label: '25–<50%', color: '#fde68a' },
  { label: '50–<75%', color: '#93c5fd' },
  { label: '75–<100%', color: '#3b82f6' },
  { label: '100%', color: '#047857' }
];

const color = rate =>
  rate == null
    ? bands[0].color
    : rate < 25
    ? bands[1].color
    : rate < 50
    ? bands[2].color
    : rate < 75
    ? bands[3].color
    : rate < 100
    ? bands[4].color
    : bands[5].color;

const rateText = province => (province?.rate == null ? 'Chưa có tỷ lệ' : formatPercent(province.rate));

// Tọa độ các đảo/rạn san hô chính của Quần đảo Hoàng Sa (thuộc TP. Đà Nẵng)
const hoangSaIslands = [
  { x: 384.6, y: 339.3, r: 3.2, name: 'Đảo Tri Tôn' },
  { x: 437.6, y: 328.2, r: 3.5, name: 'Đảo Phú Lâm' },
  { x: 436.5, y: 328.7, r: 3.0, name: 'Đảo Linh Côn' },
  { x: 407.7, y: 327.7, r: 3.2, name: 'Đảo Hoàng Sa' },
  { x: 404.2, y: 320.2, r: 2.8, name: 'Đảo Quang Hòa' },
  { x: 404.1, y: 320.0, r: 3.5, name: 'Đảo Hữu Nhật' },
  { x: 417.0, y: 315.7, r: 3.2, name: 'Bãi Xà Cừ' },
  { x: 404.7, y: 310.9, r: 2.8, name: 'Đảo Bạch Quy' },
  { x: 397.6, y: 311.0, r: 3.0, name: 'Đảo Cây' },
  { x: 400.2, y: 310.7, r: 3.2, name: 'Đảo Bắc' },
  { x: 400.0, y: 308.5, r: 2.8, name: 'Đảo Trung' },
  { x: 406.5, y: 309.3, r: 3.0, name: 'Đảo Nam' },
  { x: 401.0, y: 307.6, r: 2.8, name: 'Cụm Lưỡi Liềm' },
  { x: 404.2, y: 306.7, r: 2.6, name: 'Đá Lồi' },
  { x: 402.9, y: 306.5, r: 3.0, name: 'Cụm An Vĩnh' },
  { x: 442.4, y: 305.9, r: 2.6, name: 'Bãi Thủy Tề' },
  { x: 405.3, y: 306.4, r: 3.0, name: 'Bãi Thám Hiểm' },
  { x: 446.3, y: 301.9, r: 3.0, name: 'Đảo Đông' },
  { x: 391.5, y: 299.5, r: 3.0, name: 'Đá Tháp' },
  { x: 430.2, y: 294.4, r: 2.8, name: 'Đá Bông Bay' },
  { x: 429.6, y: 289.7, r: 3.0, name: 'Đá Bắc' },
  { x: 426.3, y: 288.4, r: 3.2, name: 'Bãi Châu Viên' },
  { x: 396.5, y: 283.8, r: 3.2, name: 'Đá Trà Tây' },
  { x: 395.8, y: 283.9, r: 2.8, name: 'Cồn Cát Tây' }
];

// Tọa độ các đảo/rạn san hô chính của Quần đảo Trường Sa (thuộc Tỉnh Khánh Hòa)
const truongSaIslands = [
  { x: 480.0, y: 524.4, r: 3.5, name: 'Đảo Song Tử Tây' },
  { x: 511.3, y: 524.4, r: 3.2, name: 'Đảo Song Tử Đông' },
  { x: 521.8, y: 552.4, r: 3.5, name: 'Đảo Thị Tứ' },
  { x: 589.4, y: 552.0, r: 3.0, name: 'Đảo Bến Lạc' },
  { x: 598.2, y: 546.8, r: 3.0, name: 'Đảo Loại Ta' },
  { x: 631.0, y: 539.2, r: 3.2, name: 'Bãi Đinh Ba' },
  { x: 512.6, y: 577.5, r: 3.5, name: 'Đảo Nam Yết' },
  { x: 492.0, y: 582.1, r: 3.5, name: 'Đảo Sơn Ca' },
  { x: 519.4, y: 589.8, r: 3.5, name: 'Đảo Sinh Tồn' },
  { x: 509.2, y: 596.5, r: 3.2, name: 'Đảo Sinh Tồn Đông' },
  { x: 433.6, y: 606.0, r: 3.5, name: 'Đảo Đá Lớn' },
  { x: 596.1, y: 609.2, r: 3.2, name: 'Đá Lạc' },
  { x: 596.7, y: 595.0, r: 3.0, name: 'Đá Én Đất' },
  { x: 549.2, y: 655.2, r: 3.5, name: 'Đảo Phan Vinh' },
  { x: 494.7, y: 635.7, r: 3.5, name: 'Đảo Tốc Tan' },
  { x: 439.2, y: 635.4, r: 3.8, name: 'Đảo Trường Sa' },
  { x: 469.9, y: 662.5, r: 3.5, name: 'Đảo An Bang' },
  { x: 505.8, y: 680.9, r: 3.5, name: 'Bãi Thuyền Chài' },
  { x: 496.2, y: 685.7, r: 3.2, name: 'Đá Tiên Nữ' },
  { x: 398.2, y: 689.6, r: 3.2, name: 'Bãi Vũng Mây' },
  { x: 360.0, y: 668.4, r: 3.0, name: 'Bãi Quế Đường' }
];
</script>

<style scoped>
.province-map-container { container: province-map / inline-size; }
.province-map-body { display: flex; flex-direction: column; }
.province-map-canvas { background:white; }
.province-map-canvas > svg { box-sizing: border-box; padding: 12px; }
@media (min-width: 1024px) {
  .fit-height { display: flex; flex-direction: column; height: 100%; min-height: 0; }
  .fit-height > :first-child { flex-shrink: 0; }
  .fit-height .province-map-body { flex: 1; min-height: 0; overflow: hidden; }
  .fit-height .province-map-canvas { display: flex; flex-direction: column; min-height: 0; flex: 1; }
  .fit-height .province-map-canvas > svg { flex: 1; height: 0; min-height: 0; }
  .fit-height [data-testid="province-map-directory"] { min-height: 0; overflow: hidden; }
  .fit-height #province-map-list { display: flex; flex-direction: column; min-height: 0; flex: 1; }
  .fit-height #province-map-list > :first-child { flex-shrink: 0; }
  .fit-height .province-map-directory-scroll { max-height: none; min-height: 0; flex: 1; }
}
@container province-map (min-width: 560px) {
  .province-map-body { display: grid; grid-template-columns: minmax(0, 60fr) minmax(0, 40fr); align-items: stretch; }
  .province-map-canvas { border-bottom: 0; border-right: 1px solid var(--color-slate-200); }
  .province-map-directory-scroll { max-height: 460px; }
}
.province-shape {
  cursor: pointer;
  transition: fill 160ms ease;
}
.province-shape:hover,
.province-shape:focus-visible {
  stroke: #0f172a;
  stroke-width: 2;
  outline: none;
  filter: brightness(0.92);
}
</style>
