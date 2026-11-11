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
              {{ unitsOnly ? 'Báo cáo tiến độ trong Bộ' : 'Báo cáo tiến độ mục tiêu, nhiệm vụ' }}
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

    <!-- Agency and report type controls -->
    <div class="px-4 py-3 border-b border-slate-200/80 bg-slate-50/60 flex flex-col md:flex-row md:items-center justify-between gap-3">
      <div class="flex min-w-0 items-center gap-2.5">
        <template v-if="unitsOnly || activeTab === 'units'">
          <span class="text-xs font-bold text-slate-600 whitespace-nowrap hidden sm:inline">Đơn vị báo cáo:</span>
          <div class="min-w-0 flex-1 sm:flex-none sm:w-80 md:w-96">
            <SearchableSelect
              v-model="selectedUnitId"
              placeholder="Chọn đơn vị thuộc Bộ KH&CN..."
              :options="unitOptions"
              :isMulti="false"
              :clearable="false"
              :disabled="isSubmitting || isImportModalOpen"
            />
          </div>
        </template>

        <span v-else-if="isAgencyUser" class="text-xs text-amber-700 bg-amber-50 px-3 py-1.5 rounded-xl border border-amber-200 font-medium">
          🔒 Đơn vị: <strong>{{ selectedAgencyName }}</strong>
        </span>

        <template v-else>
          <span class="text-xs font-bold text-slate-600 whitespace-nowrap hidden sm:inline">Cơ quan báo cáo:</span>
          <div class="min-w-0 flex-1 sm:flex-none sm:w-80 md:w-96">
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
    <!-- Tabs (Mục tiêu / Nhiệm vụ) - Dùng chung giao diện segmented control đồng nhất cho cả 2 màn hình -->
    <div class="report-type-switch flex shrink-0 flex-wrap items-center gap-2 self-end md:self-auto">
      <div class="inline-flex rounded-xl border border-slate-200 bg-slate-50 p-1" role="tablist" aria-label="Loại dữ liệu">
        <button
          v-for="opt in [{ key: 'goals', label: '🎯 Mục tiêu' }, { key: 'tasks', label: '📋 Nhiệm vụ' }]"
          :key="opt.key"
          type="button"
          role="tab"
          :aria-selected="effectiveTab === opt.key"
          @click="selectTab(opt.key)"
          :class="[
            'px-3 py-1.5 text-xs font-bold rounded-lg transition flex items-center gap-1.5 cursor-pointer',
            effectiveTab === opt.key ? 'bg-white text-blue-700 shadow-xs border border-slate-200' : 'text-slate-600 hover:text-slate-900'
          ]"
        >
          <span>{{ opt.label }}</span>
          <span class="px-1.5 rounded-full bg-slate-100 text-slate-700 font-semibold">
            {{ opt.key === 'goals' ? displayGoalsCount : displayTasksCount }}
          </span>
        </button>
      </div>
      <span v-if="(unitsOnly || activeTab === 'units') && !unitOptions.length" class="text-xs text-slate-500">Chưa có đơn vị thuộc Bộ KH&amp;CN trong danh mục.</span>
    </div>

    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="p-12 flex flex-col items-center justify-center gap-3 text-slate-500">
      <span class="w-8 h-8 border-3 border-blue-600 border-t-transparent rounded-full animate-spin"></span>
      <span class="text-xs font-semibold">Đang tải dữ liệu báo cáo tiến độ...</span>
    </div>

    <!-- Tab 1: MỤC TIÊU -->
    <div v-else-if="effectiveTab === 'goals'" class="p-4 space-y-4">
      <div class="overflow-x-auto border border-slate-200 rounded-xl shadow-2xs">
        <table class="w-full text-left text-xs border-collapse" aria-label="Báo cáo tiến độ mục tiêu">
          <thead>
            <tr class="bg-slate-100 text-slate-800 font-bold text-center border-b border-slate-200">
              <th rowspan="2" class="p-2.5 border-r border-slate-200 w-12 font-bold text-slate-700">STT</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-200 w-20 font-bold text-slate-700">Mã MT</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-200 min-w-[280px] text-left font-bold text-slate-700">Tên mục tiêu / Chỉ tiêu</th>
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
              <td colspan="12" class="p-6">Không có mục tiêu nào được giao cho đơn vị.</td>
            </tr>

            <!-- SECTION I: 2629 -->
            <template v-if="goals2629.length > 0">
              <tr class="bg-blue-900 text-white font-bold border-b border-blue-800">
                <td colspan="3" class="p-2.5 px-3">I. {{ getDocumentReportHeading('2629') }}</td>
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
                <td colspan="3" class="p-2.5 px-3">II. {{ getDocumentReportHeading('1266') }}</td>
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
    <div v-else-if="effectiveTab === 'tasks'" class="p-4 space-y-4">
      <div class="overflow-x-auto border border-slate-200 rounded-xl shadow-2xs">
        <table class="w-full text-left text-xs border-collapse" aria-label="Báo cáo tiến độ nhiệm vụ">
          <colgroup><col style="width: 3%"><col style="width: 5%"><col style="width: 36%"><col style="width: 8%"><col style="width: 22%"><col style="width: 10%"><col style="width: 16%"></colgroup>
          <thead>
            <tr class="bg-slate-100 text-slate-800 font-bold text-center border-b border-slate-200">
              <th class="p-2.5 border-r border-slate-200 w-12 font-bold text-slate-700">STT</th>
              <th class="p-2.5 border-r border-slate-200 w-20 font-bold text-slate-700">Mã NV</th>
              <th class="p-2.5 border-r border-slate-200 min-w-[280px] text-left font-bold text-slate-700">Tên nhiệm vụ</th>
              <th class="p-2.5 border-r border-slate-200 w-24 font-bold text-slate-700 whitespace-nowrap">Hạn hoàn thành</th>
              <th class="p-2.5 border-r border-slate-200 min-w-[200px] text-left font-bold text-slate-700">Sản phẩm bàn giao</th>
              <th class="p-2.5 border-r border-slate-200 w-36 font-bold text-slate-700 whitespace-nowrap">Trạng thái thực hiện</th>
              <th class="p-2.5 min-w-[260px] text-left font-bold text-slate-700">Ghi chú</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-if="tasksList.length === 0" class="text-slate-500 italic text-center">
              <td colspan="7" class="p-6">Không có nhiệm vụ nào do đơn vị chủ trì.</td>
            </tr>

            <!-- SECTION I: 2629 Tasks -->
            <template v-if="tasks2629.length > 0">
              <tr class="bg-blue-900 text-white font-bold border-b border-blue-800">
                <td colspan="7" class="p-2.5 px-3">
                  I. {{ getDocumentReportHeading('2629') }}
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
                <td colspan="7" class="p-2.5 px-3">
                  II. {{ getDocumentReportHeading('1266') }}
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
      <div class="report-import-dialog bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-7xl w-full min-w-0 p-3 sm:p-5 gap-3 max-h-[calc(100dvh-2rem)] flex flex-col overflow-hidden" role="dialog" aria-modal="true" aria-label="Nhập báo cáo tiến độ">
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

        <p class="shrink-0 text-sm text-slate-600 break-words">Đơn vị nhập báo cáo: <strong class="font-semibold text-slate-800">{{ importAgencyName }}</strong></p>

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
        <div v-else-if="importStep === 2" class="space-y-3 flex-1 min-h-0 min-w-0 overflow-y-auto overflow-x-hidden">
          <div v-if="parsedData.warnings?.length" class="p-3 rounded-xl bg-amber-50 text-amber-900 text-sm" role="alert">
            <p v-for="(warning, index) in parsedData.warnings" :key="index">{{ warning }}</p>
          </div>
          <p class="text-sm text-slate-600">Chỉ cập nhật dữ liệu thay đổi. Ô tô vàng hiển thị giá trị cũ → giá trị trong file.</p>
          <div v-if="!parsedData.preview?.goals.length && !parsedData.preview?.tasks.length" class="p-6 text-center text-slate-500">Không có thay đổi so với dữ liệu hiện tại.</div>
          <div v-for="kind in ['goals', 'tasks']" :key="kind" class="import-preview-scroll min-w-0 w-full overflow-x-auto border border-slate-200 rounded-xl" v-show="parsedData.preview?.[kind].length">
            <table class="import-preview-table w-full text-sm border-separate border-spacing-0" :class="kind === 'goals' ? 'preview-goals' : 'preview-tasks'">
              <caption class="text-left p-3 font-bold bg-slate-50 border-b border-slate-200">{{ kind === 'goals' ? 'Mục tiêu' : 'Nhiệm vụ' }}</caption>
              <thead class="bg-blue-50">
                <tr v-if="kind === 'goals'">
                  <th rowspan="2">STT</th><th rowspan="2" class="preview-code">Mã MT</th><th rowspan="2" class="min-w-64">Tên mục tiêu / Chỉ tiêu</th>
                  <th colspan="2">Kết quả theo QĐ</th><th colspan="2">Kết quả thực hiện</th><th colspan="4">Mục tiêu theo từng năm (đến 2030)</th><th rowspan="2" class="min-w-48">Ghi chú / Khó khăn & kiến nghị</th>
                </tr>
                <tr><template v-if="kind === 'tasks'"><th>STT</th><th class="preview-code">Mã NV</th><th class="min-w-64">Tên nhiệm vụ</th></template>
                  <th v-for="col in previewColumns(kind).filter(c => kind !== 'goals' || c.key !== 'notes')" :key="col.key" class="min-w-28">{{ col.label }}</th>
                </tr>
              </thead>
              <tbody>
                <template v-for="section in importPreviewSections(kind)" :key="section.code">
                  <tr class="bg-blue-900 text-white font-semibold">
                    <td :colspan="kind === 'goals' ? 12 : 7">{{ section.roman }}. {{ getDocumentReportHeading(section.code) }}</td>
                  </tr>
                  <tr v-for="(row, index) in section.rows" :key="row.id">
                    <td class="text-center">{{ row.sequenceNumber || index + 1 }}</td><td class="preview-code whitespace-pre-line font-semibold">{{ row.code.replaceAll(' / ', '\n') }}</td><td>{{ row.title }}</td>
                    <td v-for="col in previewColumns(kind)" :key="col.key" :class="row.cells[col.key]?.changed ? 'bg-amber-100 text-amber-950 font-semibold' : 'text-slate-600'">
                      <template v-if="row.cells[col.key]?.changed"><span class="block text-xs font-normal line-through text-slate-500">{{ row.cells[col.key].previous ?? 'Trống' }}</span><span>→ {{ row.cells[col.key].value || row.cells[col.key].value === 0 ? row.cells[col.key].value : 'Trống' }}</span></template>
                      <template v-else>{{ row.cells[col.key]?.value }}</template>
                    </td>
                  </tr>
                </template>
              </tbody>
            </table>
          </div>

        </div>
        <div v-if="importError" role="alert" class="shrink-0 max-h-32 overflow-y-auto whitespace-pre-line break-words rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          <strong class="block mb-1">Chưa thể cập nhật báo cáo</strong>{{ importError }}
        </div>
        <div v-if="importStep === 2" class="import-preview-footer shrink-0 flex flex-wrap items-center justify-end gap-2 pt-2 border-t border-slate-100">
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

        <!-- Step 3: Success -->
        <div v-if="importStep === 3" class="space-y-4 py-6 flex flex-col items-center justify-center text-center">
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
import { toTextStatusApiValue } from '../shared/statusPresentation.js';
import { parseApiError } from '../utils/errorUtils.js';
import { getDocumentReportHeading } from '../services/documentContext.js';
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
import { findBKHCN, isUnitGroup } from '../shared/subAgencies.js';
import { sortAgenciesByStandardOrder } from '../shared/agencyPresentation.js';

const props = defineProps({ unitsOnly: { type: Boolean, default: false } });
const activeTab = ref(props.unitsOnly ? 'units' : 'goals');
const unitView = ref('goals'); // Màn báo cáo trong Bộ: xem mục tiêu hay nhiệm vụ

function selectTab(tabKey) {
  if (props.unitsOnly || activeTab.value === 'units') {
    unitView.value = tabKey;
  } else {
    activeTab.value = tabKey;
  }
}
const selectedUnitId = ref('');
// Số lượng trên 2 tab Mục tiêu/Nhiệm vụ luôn là của cơ quan đang chọn, không bị ghi đè khi xem đơn vị thuộc Bộ.
const ministryCounts = ref({ goals: 0, tasks: 0 });
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

const displayGoalsCount = computed(() => {
  if (props.unitsOnly || activeTab.value === 'units') return totalGoals.value;
  return ministryCounts.value.goals;
});

const displayTasksCount = computed(() => {
  if (props.unitsOnly || activeTab.value === 'units') return totalTasks.value;
  return ministryCounts.value.tasks;
});

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
const importAgencyId = ref('');
const importError = ref('');
const importAgencyName = ref('');
const parsedData = ref({ goals: [], plans: [], tasks: [], warnings: [] });

const isAgencyUser = computed(() => {
  return !authState.isAdmin.value && Boolean(authState.user.value?.agencyId);
});

const bkhcn = computed(() => findBKHCN(agencies.value || []));
const isBkhcnUnit = a => bkhcn.value && a.parentId && String(a.parentId).toLowerCase() === String(bkhcn.value.id).toLowerCase();

// Đơn vị thuộc Bộ KH&CN báo cáo riêng ở tab thứ 3; bỏ các danh mục nhóm (giao cho tất cả đơn vị) vì không có báo cáo riêng.
const unitOptions = computed(() => (agencies.value || [])
  .filter(a => isBkhcnUnit(a) && !isUnitGroup(a) && a.isActive !== false)
  .sort((a, b) => String(a.name).localeCompare(String(b.name), 'vi'))
  .map(a => ({ value: a.id, label: a.name })));

const agencyOptions = computed(() => {
  return sortAgenciesByStandardOrder((agencies.value || []).filter(a => !isBkhcnUnit(a)))
    .map(a => ({
      value: a.id,
      label: a.name,
      code: a.code,
      type: a.type
    }));
});

// Admin hoặc tài khoản Bộ KH&CN mới xem được báo cáo các đơn vị thuộc Bộ.
const canViewUnits = computed(() => {
  if (!bkhcn.value) return false;
  if (!isAgencyUser.value) return true;
  return String(authState.user.value?.agencyId || '').toLowerCase() === String(bkhcn.value.id).toLowerCase();
});

const effectiveTab = computed(() => (activeTab.value === 'units' ? unitView.value : activeTab.value));
const reportAgencyId = computed(() => (activeTab.value === 'units' ? (canViewUnits.value ? selectedUnitId.value : '') : selectedAgencyId.value));

const selectedAgencyName = computed(() => {
  if (!selectedAgencyId.value) return '';
  const found = agencies.value.find(a => a.id === selectedAgencyId.value);
  return found ? found.name : (reportData.value?.agencyName || '');
});

const reportAgencyName = computed(() => {
  if (activeTab.value !== 'units') return selectedAgencyName.value;
  return agencies.value.find(a => a.id === selectedUnitId.value)?.name || reportData.value?.agencyName || '';
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
    agencies.value = sortAgenciesByStandardOrder(data || []);

    // Default agency selection logic:
    if (isAgencyUser.value) {
      selectedAgencyId.value = authState.user.value.agencyId;
    } else if (!selectedAgencyId.value && agencies.value.length > 0) {
      // Find Bộ Công an or take first agency
      const bca = agencyOptions.value.find(o => String(o.label).toLowerCase().includes('bộ công an'));
      selectedAgencyId.value = (bca || agencyOptions.value[0])?.value || '';
    }
    if (!selectedUnitId.value && unitOptions.value.length > 0) {
      const cds = unitOptions.value.find(o => o.label.toLowerCase().includes('chuyển đổi số quốc gia'));
      selectedUnitId.value = (cds || unitOptions.value[0]).value;
    }
  } catch (err) {
    console.error('Error loading agencies:', err);
    toast.error('Lỗi khi tải danh sách cơ quan.');
  }
}

let reportRequestVersion = 0;
async function fetchReport() {
  if (!reportAgencyId.value) {
    reportData.value = null;
    totalGoals.value = 0;
    totalTasks.value = 0;
    totalCount.value = 0;
    totalPages.value = 1;
    return;
  }
  const requestVersion = ++reportRequestVersion;
  const isUnitReport = activeTab.value === 'units';
  isLoading.value = true;
  reportData.value = null;
  try {
    const url = `/api/planning/agency-progress-report?agencyId=${reportAgencyId.value}&reportYear=2026&tab=${effectiveTab.value}&page=${currentPage.value}&pageSize=${pageSize.value}&userRole=${authState.user.value?.role || 'Admin'}&userAgencyId=${authState.user.value?.agencyId || ''}`;
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
    if (!isUnitReport) ministryCounts.value = { goals: totalGoals.value, tasks: totalTasks.value };
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

watch([activeTab, unitView], () => {
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
  if (newVal && activeTab.value !== 'units') {
    currentPage.value = 1;
    fetchReport();
  }
});

watch(selectedUnitId, (newVal) => {
  if (newVal && activeTab.value === 'units') {
    currentPage.value = 1;
    fetchReport();
  }
});

async function handleExportExcel() {
  if (!reportAgencyId.value || isExporting.value || isLoading.value) return;
  const agencyId = reportAgencyId.value;
  const agencyName = reportAgencyName.value;
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

function previewColumns(kind) {
  return kind === 'goals'
    ? [{key:'targetPhase1',label:'Giai đoạn 1'},{key:'targetPhase2',label:'Giai đoạn 2'},
       ...[2025,2026,2027,2028,2029,2030].map(year=>({key:year,label:year===2025?'Kết quả 2025':year===2026?'Ước tính 2026':String(year)})),{key:'notes',label:'Ghi chú / Khó khăn & kiến nghị'}]
    : [{key:'dueDate',label:'Hạn hoàn thành'},{key:'deliverables',label:'Sản phẩm bàn giao'},{key:'status',label:'Trạng thái thực hiện'},{key:'notes',label:'Ghi chú'}];
}
function importPreviewSections(kind) {
  return ['2629','1266'].map((code,index)=>({code,roman:index===0?'I':'II',rows:(parsedData.value.preview?.[kind] || []).filter(row=>String(row.documentNumber).includes(code))})).filter(section=>section.rows.length);
}

function openImportModal() {
  importAgencyId.value = reportAgencyId.value;
  importAgencyName.value = reportAgencyName.value;
  importError.value = '';
  importStep.value = 1;
  parsedData.value = { goals: [], plans: [], tasks: [], warnings: [] };
  isImportModalOpen.value = true;
}

function closeImportModal() {
  isImportModalOpen.value = false;
  importStep.value = 1;
}

async function handleFileUpload(e) {
  importError.value = '';
  const file = e.target?.files?.[0];
  if (!file || !importAgencyId.value) return;

  isParsing.value = true;
  try {
    const fullRes = await fetchWithAuth(getApiUrl(`/api/planning/agency-progress-report?agencyId=${importAgencyId.value}&reportYear=2026&fetchAll=true&userRole=${authState.user.value?.role || 'Admin'}&userAgencyId=${authState.user.value?.agencyId || ''}`));
    if (!fullRes.ok) throw new Error('Không thể đối soát dữ liệu cơ quan');
    const fullData = await fullRes.json();

    const res = await parseUnifiedProgressReportFile(file, {
      existingGoals: fullData.goals || [],
      existingTasks: fullData.tasks || [],
      reportYear: 2026
    });

    parsedData.value = reconcileProgressReportImport(res, fullData.goals || [], fullData.tasks || []);
    if (activeTab.value === 'units' && parsedData.value.plans.length > 0) {
      // Kế hoạch 2027-2030 là của cả mục tiêu (dùng chung mọi cơ quan), không nhập từ báo cáo của đơn vị thuộc Bộ.
      parsedData.value.warnings = [...(parsedData.value.warnings || []),
        `Bỏ qua ${parsedData.value.plans.length} ô kế hoạch 2027-2030: kế hoạch mục tiêu được cập nhật ở báo cáo của cơ quan chủ trì, không theo từng đơn vị thuộc Bộ.`];
      parsedData.value.plans = [];
      const preview = parsedData.value.preview;
      if (preview?.goals) {
        preview.goals = preview.goals.filter(row => {
          for (const y of [2027, 2028, 2029, 2030]) if (row.cells[y]?.changed) row.cells[y] = { ...row.cells[y], value: row.cells[y].previous, changed: false };
          return Object.values(row.cells).some(c => c?.changed);
        });
      }
    }
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
  importError.value = '';
  isSubmitting.value = true;
  try {
    // 1. Submit goals and tasks progress to /api/execution/import-progress-bulk
    const progressItems = [
      ...parsedData.value.goals.map(g => ({
        id: g.id,
        code: g.code,
        value: g.value,
        status: null,
        deliverables: null,
        summaryNotes: g.summaryNotes,
        periodYear: g.periodYear ?? 2026
      })),
      ...parsedData.value.tasks.map(t => ({
        id: t.id,
        code: t.code,
        value: t.value,
        status: toTextStatusApiValue(t.status),
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
          reportingAgencyId: importAgencyId.value,
          items: progressItems
        })
      });

      if (!resProgress.ok) {
        const err = await resProgress.json().catch(() => ({}));
        throw new Error(parseApiError(err, 'Lỗi khi cập nhật tiến độ'));
      }
      // API trả 200 kể cả khi từng dòng lỗi: báo rõ các dòng không lưu được.
      const result = await resProgress.json().catch(() => null);
      const failed = (result?.results || []).filter(r => !r.success);
      if (failed.length) {
        const sample = failed.slice(0, 3).map(r => `${r.code}: ${r.message}`).join('\n');
        throw new Error(`${failed.length} dòng chưa được cập nhật:\n${sample}${failed.length > 3 ? '\n…' : ''}`);
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
          body: JSON.stringify({ confirmOverwrite: true, reportingAgencyId: importAgencyId.value, rows })
        });
        if (!planResponse.ok) {
          const error = await planResponse.json().catch(() => ({}));
          throw new Error(parseApiError(error, 'Không thể cập nhật kế hoạch mục tiêu. Vui lòng kiểm tra và thử lại.'));
        }
      }
    }

    importStep.value = 3;
    toast.success('Đã cập nhật tiến độ và kế hoạch thành công!');
    await fetchReport();
  } catch (err) {
    console.error('Lỗi khi lưu dữ liệu import:', err);
    importError.value = err.message || 'Lỗi khi lưu dữ liệu import.';
    toast.error(importError.value, { autoClose: 10000 });
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(async () => {
  await loadAgencies();
  if (reportAgencyId.value) {
    await fetchReport();
  }
});
</script>

<style scoped>
.preview-goals { min-width: 1550px; }
.preview-tasks { min-width: 1050px; }
.import-preview-table .preview-code { min-width: 112px; width: 112px; white-space: pre; }
.import-preview-table th,  .import-preview-table td { padding: 12px; border-right: 1px solid #dbe3ef; border-bottom: 1px solid #dbe3ef; }
.import-preview-table thead tr:first-child th:last-child, .import-preview-table td:last-child { border-right: 0; }
.import-preview-table tbody tr:last-child td { border-bottom: 0; }
@media (max-width: 640px) {
  .report-import-dialog { max-height: calc(100dvh - 16px); }
  .import-preview-table th, .import-preview-table td { padding: 10px; }
  .import-preview-footer > button { flex: 1; justify-content: center; }
}
</style>
