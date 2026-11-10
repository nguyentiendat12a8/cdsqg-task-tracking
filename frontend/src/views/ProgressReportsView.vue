<template>
  <div class="w-full min-w-0 font-sans rounded-2xl border border-slate-200/80 bg-white shadow-sm overflow-hidden flex flex-col">
    <!-- Header Screen Heading -->
    <div class="ui-screen-heading p-4 sm:p-5 border-b border-slate-200/80 flex flex-col lg:flex-row lg:items-center justify-between gap-3 w-full">
      <div class="flex items-center gap-3 min-w-0">
        <span class="ui-screen-icon w-10 h-10 flex items-center justify-center shrink-0 rounded-2xl bg-blue-100 text-blue-700">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
          </svg>
        </span>
        <div>
          <div class="flex flex-wrap items-center gap-2">
            <h2 class="ui-page-title tracking-tight text-slate-900 font-bold text-base sm:text-lg">
              Báo cáo tiến độ mục tiêu, nhiệm vụ
            </h2>
            <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-bold border bg-blue-50 text-blue-800 border-blue-200">
              QĐ 2629/QĐ-TTg & QĐ 1266/QĐ-TTg
            </span>
          </div>
          <p class="text-xs text-slate-500 font-medium mt-0.5">
            Báo cáo và kế hoạch mục tiêu, tiến độ nhiệm vụ do từng đơn vị chủ trì thực hiện
          </p>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex flex-wrap items-center gap-2.5">
        <button
          type="button"
          @click="handleExportExcel"
          :disabled="isLoading || isExporting || !reportData"
          :aria-busy="isExporting"
          class="ui-btn ui-btn-secondary px-3.5 py-2 text-xs font-bold rounded-xl flex items-center gap-2 cursor-pointer shadow-xs disabled:opacity-50"
        >
          <svg class="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
          </svg>
          <span>{{ isExporting ? 'Đang xuất Excel…' : 'Xuất excel' }}</span>
        </button>

        <button
          type="button"
          @click="openImportModal"
          :disabled="isLoading"
          class="ui-btn ui-btn-primary px-3.5 py-2 text-xs font-bold rounded-xl flex items-center gap-2 cursor-pointer shadow-xs disabled:opacity-50"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/>
          </svg>
          <span>Nhập báo cáo</span>
        </button>
      </div>
    </div>

    <!-- Tabs & Agency Control Row -->
    <div class="px-4 py-3 border-b border-slate-200/80 bg-slate-50/60 flex flex-col md:flex-row md:items-center justify-between gap-3">
      <!-- Tabs (Mục tiêu / Nhiệm vụ) -->
      <div class="flex items-center gap-2">
        <button
          type="button"
          @click="activeTab = 'goals'"
          :class="[
            'px-4 py-2 text-xs font-bold rounded-xl transition-all flex items-center gap-2 cursor-pointer shadow-2xs',
            activeTab === 'goals'
              ? 'bg-blue-600 text-white shadow-xs'
              : 'bg-white text-slate-700 border border-slate-200 hover:bg-slate-100'
          ]"
        >
          <span>🎯 Mục tiêu</span>
          <span
            class="px-2 py-0.5 rounded-full text-xs font-semibold"
            :class="activeTab === 'goals' ? 'bg-white/20 text-white' : 'bg-slate-100 text-slate-700'"
          >
            {{ totalGoals }}
          </span>
        </button>

        <button
          type="button"
          @click="activeTab = 'tasks'"
          :class="[
            'px-4 py-2 text-xs font-bold rounded-xl transition-all flex items-center gap-2 cursor-pointer shadow-2xs',
            activeTab === 'tasks'
              ? 'bg-blue-600 text-white shadow-xs'
              : 'bg-white text-slate-700 border border-slate-200 hover:bg-slate-100'
          ]"
        >
          <span>📋 Nhiệm vụ</span>
          <span
            class="px-2 py-0.5 rounded-full text-xs font-semibold"
            :class="activeTab === 'tasks' ? 'bg-white/20 text-white' : 'bg-slate-100 text-slate-700'"
          >
            {{ totalTasks }}
          </span>
        </button>
      </div>

      <!-- Agency Dropdown & Actions -->
      <div class="flex items-center gap-2.5">
        <span v-if="isAgencyUser" class="text-xs text-amber-700 bg-amber-50 px-3 py-1.5 rounded-xl border border-amber-200 font-medium">
          🔒 Đơn vị: <strong>{{ selectedAgencyName }}</strong>
        </span>

        <template v-else>
          <span class="text-xs font-bold text-slate-600 whitespace-nowrap hidden sm:inline">Cơ quan báo cáo:</span>
          <div class="w-72 sm:w-80 md:w-96">
            <SearchableSelect
              v-model="selectedAgencyId"
              placeholder="Chọn cơ quan báo cáo..."
              :options="agencyOptions"
              :isMulti="false"
              :clearable="false"
              :disabled="isSubmitting || isImportModalOpen"
            />
          </div>
        </template>

        <button
          type="button"
          @click="fetchReport"
          :disabled="isLoading"
          class="p-2 text-slate-600 hover:text-slate-900 bg-white hover:bg-slate-100 rounded-xl border border-slate-200 transition cursor-pointer shrink-0 shadow-2xs"
          title="Tải lại dữ liệu"
        >
          <svg class="w-4 h-4" :class="{ 'animate-spin': isLoading }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="p-12 flex flex-col items-center justify-center gap-3 text-slate-500">
      <span class="w-8 h-8 border-3 border-blue-600 border-t-transparent rounded-full animate-spin"></span>
      <span class="text-xs font-semibold">Đang tải dữ liệu báo cáo tiến độ...</span>
    </div>

    <!-- Tab 1: MỤC TIÊU -->
    <div v-else-if="activeTab === 'goals'" class="p-4 space-y-4">
      <div class="overflow-x-auto border border-slate-200 rounded-xl shadow-2xs">
        <table class="w-full text-left text-xs border-collapse" aria-label="Báo cáo tiến độ mục tiêu">
          <thead>
            <tr class="bg-slate-100 text-slate-800 font-bold text-center border-b border-slate-200">
              <th rowspan="2" class="p-2.5 border-r border-slate-200 w-12 font-bold text-slate-700">STT</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-200 w-20 font-bold text-slate-700">Mã MT</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-200 min-w-[280px] text-left font-bold text-slate-700">Tên mục tiêu / Chỉ tiêu</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-200 w-16 font-bold text-slate-700">ĐVT</th>
              <th colspan="2" class="p-2 border-r border-slate-200 font-bold text-slate-700">Kết quả theo QĐ</th>
              <th colspan="2" class="p-2 border-r border-slate-200 bg-amber-50/70 text-amber-900 font-bold">Kết quả thực hiện</th>
              <th colspan="4" class="p-2 border-r border-slate-200 bg-blue-50/60 text-blue-900 font-bold">Mục tiêu theo từng năm (đến 2030)</th>
              <th rowspan="2" class="p-2.5 min-w-[220px] text-left font-bold text-slate-700">Ghi chú / Khó khăn & kiến nghị</th>
            </tr>
            <tr class="bg-slate-50 text-slate-700 font-bold text-center border-b border-slate-200">
              <th class="p-2 border-r border-slate-200 w-28 text-center font-bold text-slate-700">Giai đoạn 1</th>
              <th class="p-2 border-r border-slate-200 w-28 text-center font-bold text-slate-700">Giai đoạn 2</th>
              <th class="p-2 border-r border-slate-200 bg-amber-50/50 text-amber-900 w-24 font-bold">Kết quả 2025</th>
              <th class="p-2 border-r border-slate-200 bg-amber-50/50 text-amber-900 w-24 font-bold">Ước tính 2026</th>
              <th class="p-2 border-r border-slate-200 bg-blue-50/40 text-blue-900 w-24 font-bold">2027</th>
              <th class="p-2 border-r border-slate-200 bg-blue-50/40 text-blue-900 w-24 font-bold">2028</th>
              <th class="p-2 border-r border-slate-200 bg-blue-50/40 text-blue-900 w-24 font-bold">2029</th>
              <th class="p-2 border-r border-slate-200 bg-blue-50/40 text-blue-900 w-24 font-bold">2030</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-if="goalsList.length === 0" class="text-slate-500 italic text-center">
              <td colspan="13" class="p-6">Không có mục tiêu nào được giao cho đơn vị.</td>
            </tr>

            <!-- SECTION I: 2629 -->
            <template v-if="goals2629.length > 0">
              <tr class="bg-blue-900 text-white font-bold border-b border-blue-800">
                <td colspan="4" class="p-2.5 px-3">I. QUYẾT ĐỊNH SỐ 2629/QĐ-TTg NGÀY 10/12/2024</td>
                <td class="p-2.5 text-center whitespace-nowrap" aria-label="Giai đoạn 1: 2025 - 2027">2025 - 2027</td>
                <td class="p-2.5 text-center whitespace-nowrap" aria-label="Giai đoạn 2: 2028 - 2030">2028 - 2030</td>
                <td colspan="7" class="p-2.5"></td>
              </tr>
              <tr
                v-for="(g, idx) in goals2629"
                :key="g.id"
                class="hover:bg-slate-50/80 transition font-normal"
              >
                <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ g.sequenceNumber || (idx + 1) }}</td>
                <td class="p-2.5 text-center border-r border-slate-200">
                  <div v-for="c in String(g.code).split('\n')" :key="c" class="font-bold text-blue-900 leading-tight">
                    {{ c }}
                  </div>
                </td>
                <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ g.title }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 text-slate-600">{{ g.unitName }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 bg-slate-50 font-semibold text-slate-700" aria-label="Kết quả theo quyết định, dữ liệu cố định">{{ formatTarget(g.targetPhase1) }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 bg-slate-50 font-semibold text-slate-700" aria-label="Kết quả theo quyết định, dữ liệu cố định">{{ formatTarget(g.targetPhase2) }}</td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2025)" :data-report-year="2025" :aria-label="reportYearCellLabel(g, 2025)">
                  <span v-if="isProgressReportYearApplicable(g, 2025)">{{ formatCellVal(progressReportYearValue(g, 2025), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2026)" :data-report-year="2026" :aria-label="reportYearCellLabel(g, 2026)">
                  <span v-if="isProgressReportYearApplicable(g, 2026)">{{ formatCellVal(progressReportYearValue(g, 2026), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2027)" :data-report-year="2027" :aria-label="reportYearCellLabel(g, 2027)">
                  <span v-if="isProgressReportYearApplicable(g, 2027)">{{ formatCellVal(progressReportYearValue(g, 2027), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2028)" :data-report-year="2028" :aria-label="reportYearCellLabel(g, 2028)">
                  <span v-if="isProgressReportYearApplicable(g, 2028)">{{ formatCellVal(progressReportYearValue(g, 2028), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2029)" :data-report-year="2029" :aria-label="reportYearCellLabel(g, 2029)">
                  <span v-if="isProgressReportYearApplicable(g, 2029)">{{ formatCellVal(progressReportYearValue(g, 2029), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2030)" :data-report-year="2030" :aria-label="reportYearCellLabel(g, 2030)">
                  <span v-if="isProgressReportYearApplicable(g, 2030)">{{ formatCellVal(progressReportYearValue(g, 2030), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ g.notes || '' }}</td>
              </tr>
            </template>

            <!-- SECTION II: 1266 -->
            <template v-if="goals1266.length > 0">
              <tr class="bg-blue-900 text-white font-bold border-b border-blue-800">
                <td colspan="4" class="p-2.5 px-3">II. QUYẾT ĐỊNH SỐ 1266/QĐ-TTg NGÀY 24/10/2024</td>
                <td class="p-2.5 text-center whitespace-nowrap" aria-label="Giai đoạn 1: 2026 - 2030">2026 - 2030</td>
                <td class="p-2.5 text-center whitespace-nowrap" aria-label="Giai đoạn 2: 2031 - 2045">2031 - 2045</td>
                <td colspan="7" class="p-2.5"></td>
              </tr>
              <tr
                v-for="(g, idx) in goals1266"
                :key="g.id"
                class="hover:bg-slate-50/80 transition font-normal"
              >
                <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ g.sequenceNumber || (idx + 1) }}</td>
                <td class="p-2.5 text-center border-r border-slate-200">
                  <div v-for="c in String(g.code).split('\n')" :key="c" class="font-bold text-blue-900 leading-tight">
                    {{ c }}
                  </div>
                </td>
                <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ g.title }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 text-slate-600">{{ g.unitName }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 bg-slate-50 font-semibold text-slate-700" aria-label="Kết quả theo quyết định, dữ liệu cố định">{{ formatTarget(g.targetPhase1) }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 bg-slate-50 font-semibold text-slate-700" aria-label="Kết quả theo quyết định, dữ liệu cố định">{{ formatTarget(g.targetPhase2) }}</td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2025)" :data-report-year="2025" :aria-label="reportYearCellLabel(g, 2025)">
                  <span v-if="isProgressReportYearApplicable(g, 2025)">{{ formatCellVal(progressReportYearValue(g, 2025), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2026)" :data-report-year="2026" :aria-label="reportYearCellLabel(g, 2026)">
                  <span v-if="isProgressReportYearApplicable(g, 2026)">{{ formatCellVal(progressReportYearValue(g, 2026), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2027)" :data-report-year="2027" :aria-label="reportYearCellLabel(g, 2027)">
                  <span v-if="isProgressReportYearApplicable(g, 2027)">{{ formatCellVal(progressReportYearValue(g, 2027), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2028)" :data-report-year="2028" :aria-label="reportYearCellLabel(g, 2028)">
                  <span v-if="isProgressReportYearApplicable(g, 2028)">{{ formatCellVal(progressReportYearValue(g, 2028), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2029)" :data-report-year="2029" :aria-label="reportYearCellLabel(g, 2029)">
                  <span v-if="isProgressReportYearApplicable(g, 2029)">{{ formatCellVal(progressReportYearValue(g, 2029), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-center border-r border-slate-200" :class="reportYearCellClass(g, 2030)" :data-report-year="2030" :aria-label="reportYearCellLabel(g, 2030)">
                  <span v-if="isProgressReportYearApplicable(g, 2030)">{{ formatCellVal(progressReportYearValue(g, 2030), g.unitName) }}</span>
                </td>
                <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ g.notes || '' }}</td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

      <!-- Pagination for Goals -->
      <DocumentPagination
        v-model:currentPage="currentPage"
        v-model:pageSize="pageSize"
        :totalCount="totalCount"
        :totalPages="totalPages"
        filterItemType="Goal"
        :pageSizeOptions="pageSizeOptions"
        :disabled="isLoading"
        :changePage="handlePageChange"
      />
    </div>

    <!-- Tab 2: NHIỆM VỤ -->
    <div v-else-if="activeTab === 'tasks'" class="p-4 space-y-4">
      <div class="overflow-x-auto border border-slate-200 rounded-xl shadow-2xs">
        <table class="w-full text-left text-xs border-collapse" aria-label="Báo cáo tiến độ nhiệm vụ">
          <thead>
            <tr class="bg-slate-100 text-slate-800 font-bold text-center border-b border-slate-200">
              <th class="p-2.5 border-r border-slate-200 w-12 font-bold text-slate-700">STT</th>
              <th class="p-2.5 border-r border-slate-200 w-20 font-bold text-slate-700">Mã NV</th>
              <th class="p-2.5 border-r border-slate-200 min-w-[280px] text-left font-bold text-slate-700">Tên nhiệm vụ</th>
              <th class="p-2.5 border-r border-slate-200 w-36 min-w-[130px] text-left font-bold text-slate-700">Cơ quan phối hợp</th>
              <th class="p-2.5 border-r border-slate-200 w-24 font-bold text-slate-700 whitespace-nowrap">Hạn hoàn thành</th>
              <th class="p-2.5 border-r border-slate-200 min-w-[200px] text-left font-bold text-slate-700">Sản phẩm bàn giao</th>
              <th class="p-2.5 border-r border-slate-200 w-36 font-bold text-slate-700 whitespace-nowrap">Trạng thái thực hiện</th>
              <th class="p-2.5 min-w-[200px] text-left font-bold text-slate-700">Ghi chú</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-if="tasksList.length === 0" class="text-slate-500 italic text-center">
              <td colspan="8" class="p-6">Không có nhiệm vụ nào do đơn vị chủ trì.</td>
            </tr>

            <!-- SECTION I: 2629 Tasks -->
            <template v-if="tasks2629.length > 0">
              <tr class="bg-blue-900 text-white font-bold border-b border-blue-800">
                <td colspan="8" class="p-2.5 px-3">
                  I. QUYẾT ĐỊNH SỐ 2629/QĐ-TTg NGÀY 10/12/2024
                </td>
              </tr>
              <tr
                v-for="(t, idx) in tasks2629"
                :key="t.id"
                class="hover:bg-slate-50/80 transition font-normal"
              >
                <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ t.sequenceNumber || (idx + 1) }}</td>
                <td class="p-2.5 text-center font-bold text-blue-900 border-r border-slate-200">{{ t.code }}</td>
                <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ t.title }}</td>
                <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.coordinatingAgencies || '—' }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 text-slate-600 whitespace-nowrap">{{ t.dueDate || '—' }}</td>
                <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.deliverables || '—' }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 whitespace-nowrap">
                  <span :class="statusBadgeClass(t.status)">
                    {{ t.statusLabel }}
                  </span>
                </td>
                <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ t.notes || '' }}</td>
              </tr>
            </template>

            <!-- SECTION II: 1266 Tasks -->
            <template v-if="tasks1266.length > 0">
              <tr class="bg-blue-900 text-white font-bold border-b border-blue-800">
                <td colspan="8" class="p-2.5 px-3">
                  II. QUYẾT ĐỊNH SỐ 1266/QĐ-TTg NGÀY 24/10/2024
                </td>
              </tr>
              <tr
                v-for="(t, idx) in tasks1266"
                :key="t.id"
                class="hover:bg-slate-50/80 transition font-normal"
              >
                <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ t.sequenceNumber || (idx + 1) }}</td>
                <td class="p-2.5 text-center font-bold text-blue-900 border-r border-slate-200">{{ t.code }}</td>
                <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ t.title }}</td>
                <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.coordinatingAgencies || '—' }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 text-slate-600 whitespace-nowrap">{{ t.dueDate || '—' }}</td>
                <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.deliverables || '—' }}</td>
                <td class="p-2.5 text-center border-r border-slate-200 whitespace-nowrap">
                  <span :class="statusBadgeClass(t.status)">
                    {{ t.statusLabel }}
                  </span>
                </td>
                <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ t.notes || '' }}</td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

      <!-- Pagination for Tasks -->
      <DocumentPagination
        v-model:currentPage="currentPage"
        v-model:pageSize="pageSize"
        :totalCount="totalCount"
        :totalPages="totalPages"
        filterItemType="Task"
        :pageSizeOptions="pageSizeOptions"
        :disabled="isLoading"
        :changePage="handlePageChange"
      />
    </div>

    <!-- UNIFIED IMPORT MODAL -->
    <div v-if="isImportModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-7xl w-full p-5 space-y-4 max-h-[90vh] flex flex-col">
        <!-- Modal Header -->
        <div class="flex items-center justify-between border-b border-slate-100 pb-3 shrink-0">
          <div class="flex items-center gap-2.5">
            <span class="p-2 bg-emerald-100 text-emerald-700 rounded-xl">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/></svg>
            </span>
            <div>
              <h3 class="text-sm font-bold text-slate-800">
                Nhập báo cáo tiến độ mục tiêu, nhiệm vụ
              </h3>
              <p class="text-xs text-slate-500 font-medium mt-0.5">
                Nhập dữ liệu từ file Excel (Sheet 1: Mục tiêu, Sheet 2: Nhiệm vụ)
              </p>
            </div>
          </div>
          <button type="button" @click="closeImportModal" :disabled="isSubmitting" class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-xl transition cursor-pointer">✕</button>
        </div>

        <!-- Step 1: Select File -->
        <div v-if="importStep === 1" class="space-y-4 flex-1 flex flex-col justify-center items-center py-8 border-2 border-dashed border-slate-300 rounded-2xl bg-slate-50/50 hover:bg-slate-50 transition text-center px-4">
          <div class="w-16 h-16 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center text-2xl shadow-2xs mb-2">
            📄
          </div>
          <div>
            <h4 class="text-sm font-bold text-slate-800">Chọn file báo cáo tiến độ (.xlsx, .xls)</h4>
            <p class="text-xs text-slate-500 mt-1">Sử dụng file Excel đã xuất từ hệ thống để cập nhật kết quả và kế hoạch</p>
          </div>

          <input
            type="file"
            ref="fileInputRef"
            @change="handleFileUpload"
            accept=".xlsx, .xls"
            class="hidden"
          />

          <button
            type="button"
            @click="$refs.fileInputRef.click()"
            :disabled="isParsing"
            class="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center gap-2 cursor-pointer disabled:opacity-50"
          >
            <span>📁 Chọn file báo cáo Excel</span>
          </button>

          <div v-if="isParsing" class="flex items-center gap-2 text-xs font-bold text-blue-700 pt-2">
            <span class="w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin"></span>
            <span>Đang đọc và đối soát dữ liệu 2 sheet...</span>
          </div>
        </div>

        <!-- Step 2: Preview Parsed Items -->
        <div v-else-if="importStep === 2" class="space-y-3 flex-1 overflow-y-auto">
          <div v-if="parsedData.warnings?.length" class="p-3 rounded-xl bg-amber-50 text-amber-900 text-sm" role="alert">
            <p v-for="(warning, index) in parsedData.warnings" :key="index">{{ warning }}</p>
          </div>
          <!-- Summary Cards -->
          <div class="grid grid-cols-3 gap-3 text-xs font-bold shrink-0">
            <div class="bg-blue-50 p-3 rounded-xl border border-blue-200">
              <span class="text-blue-700 block text-xs">Kết quả mục tiêu thay đổi</span>
              <span class="text-blue-900 text-lg font-bold">{{ parsedData.goals.length }}</span>
            </div>
            <div class="bg-amber-50 p-3 rounded-xl border border-amber-200">
              <span class="text-amber-700 block text-xs">Ô kế hoạch mục tiêu (2027-2030)</span>
              <span class="text-amber-900 text-lg font-bold">{{ parsedData.plans.length }}</span>
            </div>
            <div class="bg-emerald-50 p-3 rounded-xl border border-emerald-200">
              <span class="text-emerald-700 block text-xs">Nhiệm vụ thay đổi</span>
              <span class="text-emerald-900 text-lg font-bold">{{ parsedData.tasks.length }}</span>
            </div>
          </div>

          <p class="text-sm text-slate-600">Chỉ cập nhật dữ liệu thay đổi. Ô tô vàng hiển thị giá trị cũ → giá trị trong file.</p>
          <div v-if="!parsedData.preview?.goals.length && !parsedData.preview?.tasks.length" class="p-6 text-center text-slate-500">Không có thay đổi so với dữ liệu hiện tại.</div>
          <div v-for="kind in ['goals', 'tasks']" :key="kind" class="overflow-auto border border-slate-200 rounded-xl" v-show="parsedData.preview?.[kind].length">
            <table class="w-full text-sm border-collapse">
              <caption class="text-left p-3 font-bold bg-slate-50">{{ kind === 'goals' ? 'Mục tiêu' : 'Nhiệm vụ' }}</caption>
              <thead class="bg-blue-50"><tr><th class="p-3 border">Mã</th><th class="p-3 border min-w-64">Tên / Chỉ tiêu</th><th v-for="col in (kind === 'goals' ? [2025,2026,2027,2028,2029,2030,'notes'] : ['status','notes'])" :key="col" class="p-3 border min-w-28">{{ col === 'notes' ? 'Ghi chú' : col === 'status' ? 'Trạng thái' : col }}</th></tr></thead>
              <tbody><tr v-for="row in parsedData.preview?.[kind]" :key="row.id">
                <td class="p-3 border whitespace-pre-line">{{ row.documentNumber }}<br>{{ row.code }}</td><td class="p-3 border">{{ row.title }}</td>
                <td v-for="col in (kind === 'goals' ? [2025,2026,2027,2028,2029,2030,'notes'] : ['status','notes'])" :key="col" class="p-3 border" :class="row.cells[col]?.changed ? 'bg-amber-100 text-amber-950 font-semibold' : 'text-slate-500'">
                  <template v-if="row.cells[col]?.changed"><span class="block text-xs font-normal line-through text-slate-500">{{ row.cells[col].previous ?? 'Trống' }}</span><span>→ {{ row.cells[col].value || row.cells[col].value === 0 ? row.cells[col].value : 'Trống' }}</span></template>
                  <template v-else>{{ row.cells[col]?.value }}</template>
                </td>
              </tr></tbody>
            </table>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
            <button
              type="button"
              @click="importStep = 1"
              :disabled="isSubmitting"
              class="px-4 py-2 text-xs font-semibold rounded-xl text-slate-600 hover:bg-slate-100 cursor-pointer"
            >
              Chọn lại file
            </button>
            <button
              type="button"
              @click="submitImportData"
              :disabled="isSubmitting || (parsedData.goals.length === 0 && parsedData.plans.length === 0 && parsedData.tasks.length === 0)"
              class="px-4 py-2 text-xs font-bold rounded-xl bg-blue-600 hover:bg-blue-700 text-white shadow-sm flex items-center gap-2 cursor-pointer disabled:opacity-50"
            >
              <span v-if="isSubmitting" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <span>Xác nhận cập nhật</span>
            </button>
          </div>
        </div>

        <!-- Step 3: Success -->
        <div v-else-if="importStep === 3" class="space-y-4 py-6 flex flex-col items-center justify-center text-center">
          <div class="w-14 h-14 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center text-2xl">
            ✓
          </div>
          <div>
            <h4 class="text-sm font-bold text-slate-800">Cập nhật dữ liệu thành công!</h4>
            <p class="text-xs text-slate-500 mt-1">Đã cập nhật tiến độ mục tiêu, kế hoạch và nhiệm vụ thành công vào hệ thống.</p>
          </div>
          <button
            type="button"
            @click="closeImportModal"
            class="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-xs cursor-pointer"
          >
            Hoàn tất và xem báo cáo
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reconcileProgressReportImport } from '../utils/progressReportImportDiff.js';
import { ref, computed, onMounted, watch } from 'vue';
import { fetchWithAuth, authState } from '../services/auth';
import { isProgressReportYearApplicable, progressReportYearValue } from '../shared/progressReportCells.js';
import { getApiUrl } from '../config/api';
import { toast } from 'vue3-toastify';
import { formatNumber } from '../shared/formatters';
import SearchableSelect from '../components/SearchableSelect.vue';
import DocumentPagination from '../features/document/DocumentPagination.vue';
import { exportUnifiedProgressReport, parseUnifiedProgressReportFile } from '../utils/unifiedProgressReportExcel.js';

const activeTab = ref('goals'); // 'goals' | 'tasks'
const isLoading = ref(false);
const isExporting = ref(false);
const agencies = ref([]);
const selectedAgencyId = ref('');
const reportData = ref(null);

// Pagination State
const currentPage = ref(1);
const pageSize = ref(15);
const totalCount = ref(0);
const totalPages = ref(1);
const totalGoals = ref(0);
const totalTasks = ref(0);

const pageSizeOptions = [
  { value: 10, label: '10' },
  { value: 15, label: '15' },
  { value: 20, label: '20' },
  { value: 50, label: '50' }
];

// Import Modal State
const isImportModalOpen = ref(false);
const importStep = ref(1);
const isParsing = ref(false);
const isSubmitting = ref(false);
const fileInputRef = ref(null);
const parsedData = ref({ goals: [], plans: [], tasks: [], warnings: [] });

const isAgencyUser = computed(() => {
  return !authState.isAdmin.value && Boolean(authState.user.value?.agencyId);
});

const agencyOptions = computed(() => {
  return (agencies.value || []).map(a => ({
    value: a.id,
    label: a.name
  }));
});

const selectedAgencyName = computed(() => {
  if (!selectedAgencyId.value) return '';
  const found = agencies.value.find(a => a.id === selectedAgencyId.value);
  return found ? found.name : (reportData.value?.agencyName || '');
});

const goalsList = computed(() => reportData.value?.goals || []);
const tasksList = computed(() => reportData.value?.tasks || []);

const goals2629 = computed(() => goalsList.value.filter(g => String(g.documentNumber).includes('2629')));
const goals1266 = computed(() => goalsList.value.filter(g => String(g.documentNumber).includes('1266')));

const tasks2629 = computed(() => tasksList.value.filter(t => String(t.documentNumber).includes('2629')));
const tasks1266 = computed(() => tasksList.value.filter(t => String(t.documentNumber).includes('1266')));

function reportYearCellClass(goal, year) {
  if (!isProgressReportYearApplicable(goal, year)) return 'bg-slate-50 text-slate-400 font-medium';
  const value = progressReportYearValue(goal, year);
  return value !== '' ? 'text-emerald-800 font-bold' : 'text-slate-400';
}
function reportYearCellLabel(goal, year) {
  if (!isProgressReportYearApplicable(goal, year)) return `${year}: Không áp dụng, không điền`;
  return `${year}: ${year === 2025 ? 'Dữ liệu kết quả năm 2025' : 'Ô nhập trong biểu mẫu Excel'}`;
}

function formatCellVal(val, unitName) {
  if (val == null || val === '') return '—';
  if (val === '—') return '—';

  const isPercentUnit = !unitName || unitName.trim() === '%';
  let str = String(val).trim();
  if (str === '—') return '—';

  const hasPercentSign = str.endsWith('%');
  if (hasPercentSign) {
    str = str.slice(0, -1).trim();
  }

  if (/^\d{1,3}(\.\d{3})+$/.test(str)) {
    return (hasPercentSign || isPercentUnit) ? `${str}%` : str;
  }

  const num = Number(str.replace(/,/g, '.'));
  if (!isNaN(num) && Number.isFinite(num)) {
    let formatted = '';
    if (Number.isInteger(num)) {
      formatted = formatNumber(num);
    } else {
      formatted = String(num);
    }
    return (hasPercentSign || isPercentUnit) ? `${formatted}%` : formatted;
  }

  return (hasPercentSign || isPercentUnit) && !str.endsWith('%') ? `${str}%` : str;
}

function formatTarget(val) {
  if (val == null || val === '' || val === '—') return '';
  const str = String(val).trim();
  if (str === '—') return '';
  let cleanStr = str;
  if (cleanStr.endsWith('.0')) cleanStr = cleanStr.slice(0, -2);
  else if (cleanStr.endsWith('.00')) cleanStr = cleanStr.slice(0, -3);

  if (/^\d{1,3}(\.\d{3})+$/.test(cleanStr)) return cleanStr;

  const num = Number(cleanStr);
  if (!isNaN(num) && Number.isFinite(num)) {
    return formatNumber(num);
  }
  return str;
}

function statusBadgeClass(status) {
  if (status === 'Completed') return 'inline-flex px-2.5 py-0.5 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-200 whitespace-nowrap';
  if (status === 'InProgress' || status === 'Drafting') return 'inline-flex px-2.5 py-0.5 rounded-full text-xs font-bold bg-blue-100 text-blue-800 border border-blue-200 whitespace-nowrap';
  return 'inline-flex px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-100 text-slate-700 border border-slate-200 whitespace-nowrap';
}

async function loadAgencies() {
  try {
    const res = await fetchWithAuth(getApiUrl('/api/agencies?excludeSpecial=true'));
    if (!res.ok) throw new Error('Không thể tải danh sách cơ quan');
    const data = await res.json();
    agencies.value = data || [];

    // Default agency selection logic:
    if (isAgencyUser.value) {
      selectedAgencyId.value = authState.user.value.agencyId;
    } else if (!selectedAgencyId.value && agencies.value.length > 0) {
      // Find Bộ Công an or take first agency
      const bca = agencies.value.find(a => a.name.toLowerCase().includes('bộ công an'));
      selectedAgencyId.value = bca ? bca.id : agencies.value[0].id;
    }
  } catch (err) {
    console.error('Error loading agencies:', err);
    toast.error('Lỗi khi tải danh sách cơ quan.');
  }
}

let reportRequestVersion = 0;
async function fetchReport() {
  if (!selectedAgencyId.value) return;
  const requestVersion = ++reportRequestVersion;
  isLoading.value = true;
  reportData.value = null;
  try {
    const url = `/api/planning/agency-progress-report?agencyId=${selectedAgencyId.value}&reportYear=2026&tab=${activeTab.value}&page=${currentPage.value}&pageSize=${pageSize.value}&userRole=${authState.user.value?.role || 'Admin'}&userAgencyId=${authState.user.value?.agencyId || ''}`;
    const res = await fetchWithAuth(getApiUrl(url));
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Lỗi khi tải báo cáo tiến độ cơ quan');
    }
    const data = await res.json();
    if (requestVersion !== reportRequestVersion) return;
    reportData.value = data;
    totalGoals.value = data.totalGoals ?? 0;
    totalTasks.value = data.totalTasks ?? 0;
    totalCount.value = data.totalCount ?? 0;
    totalPages.value = data.totalPages ?? 1;
    currentPage.value = data.page ?? 1;
  } catch (err) {
    if (requestVersion !== reportRequestVersion) return;
    console.error('Error fetching report:', err);
    toast.error(err.message || 'Lỗi khi tải dữ liệu báo cáo');
  } finally {
    if (requestVersion === reportRequestVersion) isLoading.value = false;
  }
}

watch(activeTab, () => {
  currentPage.value = 1;
  fetchReport();
});

watch(pageSize, () => {
  currentPage.value = 1;
  fetchReport();
});

function handlePageChange(p) {
  if (p < 1 || (totalPages.value > 0 && p > totalPages.value) || p === currentPage.value) return;
  currentPage.value = p;
  fetchReport();
}

watch(selectedAgencyId, (newVal) => {
  if (newVal) {
    currentPage.value = 1;
    fetchReport();
  }
});

async function handleExportExcel() {
  if (!selectedAgencyId.value || isExporting.value || isLoading.value) return;
  const agencyId = selectedAgencyId.value;
  const agencyName = selectedAgencyName.value;
  isExporting.value = true;
  try {
    const fullRes = await fetchWithAuth(getApiUrl(`/api/planning/agency-progress-report?agencyId=${agencyId}&reportYear=2026&fetchAll=true&userRole=${authState.user.value?.role || 'Admin'}&userAgencyId=${authState.user.value?.agencyId || ''}`));
    if (!fullRes.ok) throw new Error('Không thể tải dữ liệu để xuất Excel');
    const fullData = await fullRes.json();
    await exportUnifiedProgressReport({
      reportData: fullData,
      agencyName,
      reportYear: 2026
    });
  } catch (err) {
    console.error('Lỗi khi xuất excel:', err);
    toast.error(err.message || 'Lỗi khi xuất file Excel');
  } finally {
    isExporting.value = false;
  }
}

function openImportModal() {
  importStep.value = 1;
  parsedData.value = { goals: [], plans: [], tasks: [], warnings: [] };
  isImportModalOpen.value = true;
}

function closeImportModal() {
  isImportModalOpen.value = false;
  importStep.value = 1;
}

async function handleFileUpload(e) {
  const file = e.target?.files?.[0];
  if (!file) return;

  isParsing.value = true;
  try {
    const fullRes = await fetchWithAuth(getApiUrl(`/api/planning/agency-progress-report?agencyId=${selectedAgencyId.value}&reportYear=2026&fetchAll=true&userRole=${authState.user.value?.role || 'Admin'}&userAgencyId=${authState.user.value?.agencyId || ''}`));
    if (!fullRes.ok) throw new Error('Không thể đối soát dữ liệu cơ quan');
    const fullData = await fullRes.json();

    const res = await parseUnifiedProgressReportFile(file, {
      existingGoals: fullData.goals || [],
      existingTasks: fullData.tasks || [],
      reportYear: 2026
    });

    parsedData.value = reconcileProgressReportImport(res, fullData.goals || [], fullData.tasks || []);
    if (parsedData.value.goals.length === 0 && parsedData.value.plans.length === 0 && parsedData.value.tasks.length === 0) {
      toast.warning('Không tìm thấy dữ liệu tiến độ hoặc kế hoạch mới nào trong file.');
    }
    importStep.value = 2;
  } catch (err) {
    console.error('Lỗi khi đọc file Excel:', err);
    toast.error(err.message || 'Lỗi khi đọc file Excel');
  } finally {
    isParsing.value = false;
  }
}

async function submitImportData() {
  isSubmitting.value = true;
  try {
    // 1. Submit goals and tasks progress to /api/execution/import-progress-bulk
    const progressItems = [
      ...parsedData.value.goals.map(g => ({
        code: g.code,
        value: g.value,
        status: null,
        deliverables: null,
        summaryNotes: g.summaryNotes,
        periodYear: g.periodYear ?? 2026
      })),
      ...parsedData.value.tasks.map(t => ({
        code: t.code,
        value: t.value,
        status: t.status,
        deliverables: null,
        summaryNotes: t.summaryNotes,
        periodYear: 2026
      }))
    ];

    if (progressItems.length > 0) {
      const resProgress = await fetchWithAuth(getApiUrl('/api/execution/import-progress-bulk'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          userAgencyId: selectedAgencyId.value,
          reportingAgencyId: selectedAgencyId.value,
          userRole: authState.user.value?.role || 'Admin',
          items: progressItems
        })
      });

      if (!resProgress.ok) {
        const err = await resProgress.json().catch(() => ({}));
        throw new Error(err.message || 'Lỗi khi cập nhật tiến độ');
      }
    }

    // 2. Submit plans if any
    if (parsedData.value.plans.length > 0) {
      // Group plans by documentId
      const plansByDoc = {};
      parsedData.value.plans.forEach(p => {
        const dId = p.documentId || '26290000-0000-0000-0000-000000002629';
        if (!plansByDoc[dId]) plansByDoc[dId] = [];
        plansByDoc[dId].push({ taskId: p.taskId, year: p.year, targetQuantity: p.targetQuantity });
      });

      for (const [docId, rows] of Object.entries(plansByDoc)) {
        const planResponse = await fetchWithAuth(getApiUrl(`/api/planning/documents/${docId}/yearly-targets/import`), {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ confirmOverwrite: true, rows })
        });
        if (!planResponse.ok) throw new Error('Không thể cập nhật kế hoạch mục tiêu. Vui lòng kiểm tra và thử lại.');
      }
    }

    importStep.value = 3;
    toast.success('Đã cập nhật tiến độ và kế hoạch thành công!');
    await fetchReport();
  } catch (err) {
    console.error('Lỗi khi lưu dữ liệu import:', err);
    toast.error(err.message || 'Lỗi khi lưu dữ liệu import.');
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(async () => {
  await loadAgencies();
  if (selectedAgencyId.value) {
    await fetchReport();
  }
});
</script>
