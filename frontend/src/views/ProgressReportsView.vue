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
          :disabled="isLoading || !reportData"
          class="ui-btn ui-btn-secondary px-3.5 py-2 text-xs font-bold rounded-xl flex items-center gap-2 cursor-pointer shadow-xs disabled:opacity-50"
        >
          <svg class="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
          </svg>
          <span>Xuất excel</span>
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

    <!-- Agency Filter & Control Bar -->
    <div class="p-4 bg-slate-50/80 border-b border-slate-200/80 flex flex-col md:flex-row md:items-center justify-between gap-3">
      <div class="w-full md:w-96">
        <SearchableSelect
          v-model="selectedAgencyId"
          label="Cơ quan báo cáo"
          :options="agencyOptions"
          :isMulti="false"
          :clearable="false"
          :disabled="isAgencyUser || isLoading"
        />
      </div>

      <div class="flex items-center gap-3">
        <span v-if="isAgencyUser" class="text-xs text-amber-700 bg-amber-50 px-3 py-1.5 rounded-lg border border-amber-200 font-medium">
          🔒 Đang hiển thị dữ liệu của đơn vị: <strong>{{ selectedAgencyName }}</strong>
        </span>

        <button
          type="button"
          @click="fetchReport"
          :disabled="isLoading"
          class="p-2 text-slate-600 hover:text-slate-900 bg-white hover:bg-slate-100 rounded-xl border border-slate-200 transition cursor-pointer"
          title="Tải lại dữ liệu"
        >
          <svg class="w-4 h-4" :class="{ 'animate-spin': isLoading }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Tabs Navigation (Mục tiêu / Nhiệm vụ) -->
    <div class="px-4 pt-3 border-b border-slate-200 flex items-center gap-2 bg-white">
      <button
        type="button"
        @click="activeTab = 'goals'"
        :class="[
          'px-4 py-2.5 text-xs font-bold rounded-t-xl transition-all border-b-2 flex items-center gap-2 cursor-pointer',
          activeTab === 'goals'
            ? 'border-blue-600 text-blue-700 bg-blue-50/40'
            : 'border-transparent text-slate-600 hover:text-slate-900 hover:bg-slate-50'
        ]"
      >
        <span>🎯 Mục tiêu</span>
        <span class="px-2 py-0.5 rounded-full text-xs font-semibold" :class="activeTab === 'goals' ? 'bg-blue-600 text-white' : 'bg-slate-200 text-slate-700'">
          {{ goalsList.length }}
        </span>
      </button>

      <button
        type="button"
        @click="activeTab = 'tasks'"
        :class="[
          'px-4 py-2.5 text-xs font-bold rounded-t-xl transition-all border-b-2 flex items-center gap-2 cursor-pointer',
          activeTab === 'tasks'
            ? 'border-blue-600 text-blue-700 bg-blue-50/40'
            : 'border-transparent text-slate-600 hover:text-slate-900 hover:bg-slate-50'
        ]"
      >
        <span>📋 Nhiệm vụ</span>
        <span class="px-2 py-0.5 rounded-full text-xs font-semibold" :class="activeTab === 'tasks' ? 'bg-blue-600 text-white' : 'bg-slate-200 text-slate-700'">
          {{ tasksList.length }}
        </span>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="p-12 flex flex-col items-center justify-center gap-3 text-slate-500">
      <span class="w-8 h-8 border-3 border-blue-600 border-t-transparent rounded-full animate-spin"></span>
      <span class="text-xs font-semibold">Đang tải dữ liệu báo cáo tiến độ...</span>
    </div>

    <!-- Tab 1: MỤC TIÊU -->
    <div v-else-if="activeTab === 'goals'" class="p-4 space-y-4">
      <!-- Guidance Banner -->
      <div class="p-3 bg-amber-50/80 border border-amber-200 rounded-xl text-xs text-amber-900 space-y-1">
        <div class="font-bold">📌 CĂN CỨ VÀ HƯỚNG DẪN BÁO CÁO MỤC TIÊU:</div>
        <p>
          Kế hoạch mục tiêu được xây dựng căn cứ theo Kế hoạch chuyển đổi số 5 năm của cơ quan, đơn vị đã được cấp có thẩm quyền ban hành.
          Các ô màu vàng thể hiện kết quả ước tính và kế hoạch mục tiêu từng năm (chỉ hiển thị đến năm 2030).
          Các ô gạch ngang (<span class="font-bold text-slate-500">—</span>) là các năm không thuộc giai đoạn thực hiện của mục tiêu.
        </p>
      </div>

      <div class="overflow-x-auto border border-slate-200 rounded-xl shadow-2xs">
        <table class="w-full text-left text-xs border-collapse">
          <thead>
            <tr class="bg-slate-800 text-white font-bold text-center border-b border-slate-700">
              <th rowspan="2" class="p-2.5 border-r border-slate-700 w-12">STT</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-700 w-28">Căn cứ QĐ</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-700 w-20">Mã MT</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-700 min-w-[280px] text-left">Tên mục tiêu / Chỉ tiêu</th>
              <th rowspan="2" class="p-2.5 border-r border-slate-700 w-16">ĐVT</th>
              <th colspan="2" class="p-2 border-r border-slate-700">Kết quả theo QĐ</th>
              <th colspan="2" class="p-2 border-r border-slate-700 bg-amber-600/90 text-white">Kết quả thực hiện</th>
              <th colspan="4" class="p-2 border-r border-slate-700 bg-amber-500/90 text-white">Mục tiêu theo từng năm (đến 2030)</th>
              <th rowspan="2" class="p-2.5 min-w-[220px] text-left">Ghi chú / Khó khăn & kiến nghị</th>
            </tr>
            <tr class="bg-slate-700 text-white font-bold text-center border-b border-slate-600">
              <th class="p-2 border-r border-slate-600 w-24">2025-2027</th>
              <th class="p-2 border-r border-slate-600 w-24">2028-2030</th>
              <th class="p-2 border-r border-slate-600 bg-amber-700/80 text-amber-100 w-24">Kết quả 2025</th>
              <th class="p-2 border-r border-slate-600 bg-amber-700/80 text-amber-100 w-24">Ước tính 2026</th>
              <th class="p-2 border-r border-slate-600 bg-amber-600/80 text-amber-100 w-24">Kế hoạch 2027</th>
              <th class="p-2 border-r border-slate-600 bg-amber-600/80 text-amber-100 w-24">Kế hoạch 2028</th>
              <th class="p-2 border-r border-slate-600 bg-amber-600/80 text-amber-100 w-24">Kế hoạch 2029</th>
              <th class="p-2 border-r border-slate-600 bg-amber-600/80 text-amber-100 w-24">Kế hoạch 2030</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <!-- SECTION I: 2629 -->
            <tr class="bg-blue-900 text-white font-bold">
              <td colspan="13" class="p-2.5 px-3">
                I. QUYẾT ĐỊNH SỐ 2629/QĐ-TTg NGÀY 10/12/2024
              </td>
            </tr>
            <tr v-if="goals2629.length === 0" class="text-slate-500 italic text-center">
              <td colspan="13" class="p-4">Không có mục tiêu nào được giao cho đơn vị thuộc QĐ 2629/QĐ-TTg</td>
            </tr>
            <tr
              v-for="(g, idx) in goals2629"
              :key="g.id"
              class="hover:bg-slate-50/80 transition font-normal"
            >
              <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ idx + 1 }}</td>
              <td class="p-2.5 text-center font-medium text-blue-700 border-r border-slate-200">{{ g.documentNumber }}</td>
              <td class="p-2.5 text-center font-bold text-blue-900 border-r border-slate-200">{{ g.code }}</td>
              <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ g.title }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 text-slate-600">{{ g.unitName }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 font-semibold text-slate-700">{{ g.targetPhase1 || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 font-semibold text-slate-700">{{ g.targetPhase2 || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.actual2025)">{{ formatCellVal(g.actual2025) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.estimate2026)">{{ formatCellVal(g.estimate2026) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2027)">{{ formatCellVal(g.plan2027) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2028)">{{ formatCellVal(g.plan2028) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2029)">{{ formatCellVal(g.plan2029) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2030)">{{ formatCellVal(g.plan2030) }}</td>
              <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ g.notes || '' }}</td>
            </tr>

            <!-- SECTION II: 1266 -->
            <tr class="bg-blue-900 text-white font-bold">
              <td colspan="13" class="p-2.5 px-3">
                II. QUYẾT ĐỊNH SỐ 1266/QĐ-TTg NGÀY 24/10/2024
              </td>
            </tr>
            <tr v-if="goals1266.length === 0" class="text-slate-500 italic text-center">
              <td colspan="13" class="p-4">Không có mục tiêu nào được giao cho đơn vị thuộc QĐ 1266/QĐ-TTg</td>
            </tr>
            <tr
              v-for="(g, idx) in goals1266"
              :key="g.id"
              class="hover:bg-slate-50/80 transition font-normal"
            >
              <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ idx + 1 }}</td>
              <td class="p-2.5 text-center font-medium text-blue-700 border-r border-slate-200">{{ g.documentNumber }}</td>
              <td class="p-2.5 text-center font-bold text-blue-900 border-r border-slate-200">{{ g.code }}</td>
              <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ g.title }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 text-slate-600">{{ g.unitName }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 font-semibold text-slate-700">{{ g.targetPhase1 || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 font-semibold text-slate-700">{{ g.targetPhase2 || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.actual2025)">{{ formatCellVal(g.actual2025) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.estimate2026)">{{ formatCellVal(g.estimate2026) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2027)">{{ formatCellVal(g.plan2027) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2028)">{{ formatCellVal(g.plan2028) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2029)">{{ formatCellVal(g.plan2029) }}</td>
              <td class="p-2.5 text-center border-r border-slate-200" :class="cellBgClass(g.plan2030)">{{ formatCellVal(g.plan2030) }}</td>
              <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ g.notes || '' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Tab 2: NHIỆM VỤ -->
    <div v-else-if="activeTab === 'tasks'" class="p-4 space-y-4">
      <div class="p-3 bg-blue-50/80 border border-blue-200 rounded-xl text-xs text-blue-900">
        <span class="font-bold">📌 THEO DÕI TIẾN ĐỘ NHIỆM VỤ:</span>
        Danh sách nhiệm vụ do đơn vị chủ trì thực hiện thuộc các Quyết định chiến lược quốc gia.
      </div>

      <div class="overflow-x-auto border border-slate-200 rounded-xl shadow-2xs">
        <table class="w-full text-left text-xs border-collapse">
          <thead>
            <tr class="bg-slate-800 text-white font-bold text-center border-b border-slate-700">
              <th class="p-2.5 border-r border-slate-700 w-12">STT</th>
              <th class="p-2.5 border-r border-slate-700 w-28">Căn cứ QĐ</th>
              <th class="p-2.5 border-r border-slate-700 w-20">Mã NV</th>
              <th class="p-2.5 border-r border-slate-700 min-w-[280px] text-left">Tên nhiệm vụ</th>
              <th class="p-2.5 border-r border-slate-700 min-w-[180px] text-left">Cơ quan phối hợp</th>
              <th class="p-2.5 border-r border-slate-700 w-24">Hạn hoàn thành</th>
              <th class="p-2.5 border-r border-slate-700 w-32">Trạng thái thực hiện</th>
              <th class="p-2.5 border-r border-slate-700 w-20">Tiến độ %</th>
              <th class="p-2.5 border-r border-slate-700 min-w-[200px] text-left">Sản phẩm bàn giao</th>
              <th class="p-2.5 min-w-[200px] text-left">Ghi chú</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <!-- SECTION I: 2629 Tasks -->
            <tr class="bg-blue-900 text-white font-bold">
              <td colspan="10" class="p-2.5 px-3">
                I. QUYẾT ĐỊNH SỐ 2629/QĐ-TTg NGÀY 10/12/2024
              </td>
            </tr>
            <tr v-if="tasks2629.length === 0" class="text-slate-500 italic text-center">
              <td colspan="10" class="p-4">Không có nhiệm vụ nào do đơn vị chủ trì thuộc QĐ 2629/QĐ-TTg</td>
            </tr>
            <tr
              v-for="(t, idx) in tasks2629"
              :key="t.id"
              class="hover:bg-slate-50/80 transition font-normal"
            >
              <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ idx + 1 }}</td>
              <td class="p-2.5 text-center font-medium text-blue-700 border-r border-slate-200">{{ t.documentNumber }}</td>
              <td class="p-2.5 text-center font-bold text-blue-900 border-r border-slate-200">{{ t.code }}</td>
              <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ t.title }}</td>
              <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.coordinatingAgencies || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 text-slate-600">{{ t.dueDate || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200">
                <span :class="statusBadgeClass(t.status)">
                  {{ t.statusLabel }}
                </span>
              </td>
              <td class="p-2.5 text-center border-r border-slate-200 font-bold" :class="t.completionPercentage != null ? 'text-emerald-700' : 'text-slate-400'">
                {{ t.completionPercentage != null ? `${t.completionPercentage}%` : '—' }}
              </td>
              <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.deliverables || '—' }}</td>
              <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ t.notes || '' }}</td>
            </tr>

            <!-- SECTION II: 1266 Tasks -->
            <tr class="bg-blue-900 text-white font-bold">
              <td colspan="10" class="p-2.5 px-3">
                II. QUYẾT ĐỊNH SỐ 1266/QĐ-TTg NGÀY 24/10/2024
              </td>
            </tr>
            <tr v-if="tasks1266.length === 0" class="text-slate-500 italic text-center">
              <td colspan="10" class="p-4">Không có nhiệm vụ nào do đơn vị chủ trì thuộc QĐ 1266/QĐ-TTg</td>
            </tr>
            <tr
              v-for="(t, idx) in tasks1266"
              :key="t.id"
              class="hover:bg-slate-50/80 transition font-normal"
            >
              <td class="p-2.5 text-center font-medium text-slate-500 border-r border-slate-200">{{ idx + 1 }}</td>
              <td class="p-2.5 text-center font-medium text-blue-700 border-r border-slate-200">{{ t.documentNumber }}</td>
              <td class="p-2.5 text-center font-bold text-blue-900 border-r border-slate-200">{{ t.code }}</td>
              <td class="p-2.5 border-r border-slate-200 text-slate-800 leading-relaxed">{{ t.title }}</td>
              <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.coordinatingAgencies || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200 text-slate-600">{{ t.dueDate || '—' }}</td>
              <td class="p-2.5 text-center border-r border-slate-200">
                <span :class="statusBadgeClass(t.status)">
                  {{ t.statusLabel }}
                </span>
              </td>
              <td class="p-2.5 text-center border-r border-slate-200 font-bold" :class="t.completionPercentage != null ? 'text-emerald-700' : 'text-slate-400'">
                {{ t.completionPercentage != null ? `${t.completionPercentage}%` : '—' }}
              </td>
              <td class="p-2.5 border-r border-slate-200 text-slate-600 text-xs">{{ t.deliverables || '—' }}</td>
              <td class="p-2.5 text-slate-600 text-xs leading-relaxed">{{ t.notes || '' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- UNIFIED IMPORT MODAL -->
    <div v-if="isImportModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl w-full p-5 space-y-4 max-h-[90vh] flex flex-col">
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
          <!-- Summary Cards -->
          <div class="grid grid-cols-3 gap-3 text-xs font-bold shrink-0">
            <div class="bg-blue-50 p-3 rounded-xl border border-blue-200">
              <span class="text-blue-700 block text-xs">Mục tiêu có kết quả mới</span>
              <span class="text-blue-900 text-lg font-bold">{{ parsedData.goals.length }}</span>
            </div>
            <div class="bg-amber-50 p-3 rounded-xl border border-amber-200">
              <span class="text-amber-700 block text-xs">Ô kế hoạch mục tiêu (2027-2030)</span>
              <span class="text-amber-900 text-lg font-bold">{{ parsedData.plans.length }}</span>
            </div>
            <div class="bg-emerald-50 p-3 rounded-xl border border-emerald-200">
              <span class="text-emerald-700 block text-xs">Nhiệm vụ có tiến độ mới</span>
              <span class="text-emerald-900 text-lg font-bold">{{ parsedData.tasks.length }}</span>
            </div>
          </div>

          <!-- Preview Tabs/List -->
          <div class="space-y-2 border border-slate-200 rounded-xl p-3 max-h-64 overflow-y-auto">
            <div v-if="parsedData.goals.length > 0" class="space-y-1">
              <div class="font-bold text-xs text-blue-800">🎯 Tiến độ Mục tiêu:</div>
              <div v-for="g in parsedData.goals" :key="g.code" class="text-xs text-slate-700 flex justify-between py-0.5 border-b border-slate-100">
                <span class="font-semibold">{{ g.code }} - {{ g.title }}</span>
                <span class="font-bold text-emerald-700">{{ g.value }}%</span>
              </div>
            </div>

            <div v-if="parsedData.plans.length > 0" class="space-y-1 pt-2">
              <div class="font-bold text-xs text-amber-800">📅 Kế hoạch mục tiêu từng năm:</div>
              <div v-for="(p, pIdx) in parsedData.plans" :key="pIdx" class="text-xs text-slate-700 flex justify-between py-0.5 border-b border-slate-100">
                <span>{{ p.code }} (Năm {{ p.year }})</span>
                <span class="font-bold text-amber-700">{{ p.targetQuantity }}%</span>
              </div>
            </div>

            <div v-if="parsedData.tasks.length > 0" class="space-y-1 pt-2">
              <div class="font-bold text-xs text-emerald-800">📋 Tiến độ Nhiệm vụ:</div>
              <div v-for="t in parsedData.tasks" :key="t.code" class="text-xs text-slate-700 flex justify-between py-0.5 border-b border-slate-100">
                <span class="font-semibold">{{ t.code }} - {{ t.title }}</span>
                <span class="font-bold text-emerald-700">{{ t.status }} ({{ t.value != null ? `${t.value}%` : '' }})</span>
              </div>
            </div>
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
import { ref, computed, onMounted, watch } from 'vue';
import { fetchWithAuth, authState } from '../services/auth';
import { getApiUrl } from '../config/api';
import { toast } from 'vue3-toastify';
import SearchableSelect from '../components/SearchableSelect.vue';
import { exportUnifiedProgressReport, parseUnifiedProgressReportFile } from '../utils/unifiedProgressReportExcel.js';

const activeTab = ref('goals'); // 'goals' | 'tasks'
const isLoading = ref(false);
const agencies = ref([]);
const selectedAgencyId = ref('');
const reportData = ref(null);

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
    label: `${a.name} (${a.type === 'Ministry' ? 'Bộ/Ngành' : 'Địa phương'})`
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

function cellBgClass(val) {
  if (val === '—') return 'bg-slate-100 text-slate-400 font-medium';
  if (val != null && val !== '') return 'bg-amber-50/70 text-emerald-800 font-bold';
  return 'bg-amber-50/30 text-slate-400';
}

function formatCellVal(val) {
  if (val == null || val === '') return '—';
  if (val === '—') return '—';
  if (typeof val === 'number') return `${val}%`;
  return String(val);
}

function statusBadgeClass(status) {
  if (status === 'Completed') return 'inline-flex px-2 py-0.5 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-200';
  if (status === 'InProgress' || status === 'Drafting') return 'inline-flex px-2 py-0.5 rounded-full text-xs font-bold bg-blue-100 text-blue-800 border border-blue-200';
  return 'inline-flex px-2 py-0.5 rounded-full text-xs font-semibold bg-slate-100 text-slate-700 border border-slate-200';
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

async function fetchReport() {
  if (!selectedAgencyId.value) return;
  isLoading.value = true;
  try {
    const res = await fetchWithAuth(getApiUrl(`/api/planning/agency-progress-report?agencyId=${selectedAgencyId.value}&reportYear=2026&userRole=${authState.user.value?.role || 'Admin'}&userAgencyId=${authState.user.value?.agencyId || ''}`));
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Lỗi khi tải báo cáo tiến độ cơ quan');
    }
    const data = await res.json();
    reportData.value = data;
  } catch (err) {
    console.error('Error fetching report:', err);
    toast.error(err.message || 'Lỗi khi tải dữ liệu báo cáo');
  } finally {
    isLoading.value = false;
  }
}

watch(selectedAgencyId, (newVal) => {
  if (newVal) fetchReport();
});

async function handleExportExcel() {
  if (!reportData.value) return;
  await exportUnifiedProgressReport({
    reportData: reportData.value,
    agencyName: selectedAgencyName.value,
    reportYear: 2026
  });
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
    const res = await parseUnifiedProgressReportFile(file, {
      existingGoals: goalsList.value,
      existingTasks: tasksList.value,
      reportYear: 2026
    });

    parsedData.value = res;
    if (res.goals.length === 0 && res.plans.length === 0 && res.tasks.length === 0) {
      toast.warning('Không tìm thấy dữ liệu tiến độ hoặc kế hoạch mới nào trong file.');
    } else {
      importStep.value = 2;
    }
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
        periodYear: 2026
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
        await fetchWithAuth(getApiUrl(`/api/planning/documents/${docId}/yearly-targets/import`), {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ confirmOverwrite: true, rows })
        });
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
