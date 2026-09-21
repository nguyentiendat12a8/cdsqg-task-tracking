<template>
  <div class="w-full space-y-3.5 font-sans">
    
    <!-- Top Header -->
    <header class="flex flex-col lg:flex-row lg:items-center justify-between gap-3 bg-white p-3.5 sm:p-4 rounded-2xl shadow-sm border border-slate-200/80 w-full">
      <div>
        <div class="flex items-center gap-2.5">
          <span class="p-2 bg-blue-600 text-white rounded-xl shadow-sm">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
          </span>
          <div>
            <h2 class="text-sm sm:text-base font-extrabold tracking-tight text-slate-800">
              Trang chủ theo dõi tiến độ
            </h2>
          </div>
        </div>
      </div>

      <!-- Overview Goal vs Task Count Badges -->
      <div class="flex flex-wrap items-center gap-2 text-xs font-bold text-slate-500">
        <span class="px-3 py-1 bg-slate-100 rounded-lg border border-slate-200">
          Tổng số: <strong class="text-slate-800">{{ (metrics.totalGoals ?? 0) + (metrics.totalTasks ?? 0) }}</strong>
        </span>
        <span class="px-3 py-1 bg-purple-50 text-purple-700 rounded-lg border border-purple-200/60">
          🎯 Mục tiêu: <strong class="text-purple-800">{{ metrics.totalGoals ?? 0 }}</strong>
        </span>
        <span class="px-3 py-1 bg-blue-50 text-blue-700 rounded-lg border border-blue-200/60">
          📋 Nhiệm vụ: <strong class="text-blue-800">{{ metrics.totalTasks ?? 0 }}</strong>
        </span>
      </div>
    </header>

    <!-- UNIFIED ADVANCED FILTER BAR -->
    <div class="bg-white p-3 sm:p-3.5 rounded-2xl border border-slate-200/80 shadow-sm flex flex-wrap items-center justify-between gap-3 w-full">
      <!-- Left group: Filter Action Buttons -->
      <div class="flex flex-wrap items-center gap-2 min-w-0">
        <button 
          type="button" 
          @click="loadDashboardMetrics" 
          class="px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-2xs transition h-[34px] flex items-center gap-1.5 cursor-pointer whitespace-nowrap shrink-0"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>
          <span>Tải Dữ Liệu</span>
        </button>

        <button 
          type="button" 
          @click="exportDashboardExcelReport" 
          :disabled="isExportingExcel"
          class="px-3.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-2xs transition h-[34px] flex items-center gap-1.5 cursor-pointer whitespace-nowrap shrink-0"
          title="Xuất file báo cáo Excel theo bộ lọc (Mỗi Bộ/Ngành/Địa phương 1 Sheet)"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
          <span>{{ isExportingExcel ? 'Đang xuất...' : 'Xuất Báo Cáo Excel' }}</span>
        </button>

        <!-- OverlayPanel Filter Popover -->
        <OverlayPanel
          title="Lọc Dữ Liệu Bảng Điều Khiển"
          buttonText="Bộ Lọc Nâng Cao"
          :activeCount="activeDashboardFilterCount"
          widthClass="w-[340px] sm:w-[500px]"
          @apply="loadDashboardMetrics"
          @reset="resetDashboardFilters"
        >
          <div class="space-y-3">
            <div>
              <SearchableSelect 
                v-model="dashboardFilter" 
                :options="dashboardFilterOptions" 
                :isMulti="false" 
                label="Loại Đối Tượng (Mục tiêu / Nhiệm vụ)" 
                placeholder="Tất cả (Mục tiêu & Nhiệm vụ)"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="selectedAgencyIds" 
                :options="agencyOptions" 
                :isMulti="true" 
                label="Cơ Quan / Đơn Vị" 
                placeholder="Tất cả cơ quan / đơn vị"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="selectedScopes" 
                :options="scopeOptions" 
                :isMulti="true" 
                label="Phạm Vi (Chung - Riêng)" 
                placeholder="Tất cả phạm vi"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="selectedSections" 
                :options="sectionOptions" 
                :isMulti="true" 
                label="Mục (Phụ lục)" 
                placeholder="Tất cả mục"
              />
            </div>

            <div>
              <SearchableSelect 
                v-model="selectedGroups" 
                :options="groupOptions" 
                :isMulti="true" 
                label="Nhóm Trọng Tâm" 
                placeholder="Tất cả nhóm"
              />
            </div>

            <div>
              <div class="flex items-center justify-between mb-1">
                <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block">Giai Đoạn (Từ năm ➔ Đến năm)</label>
                <label class="inline-flex items-center gap-1 cursor-pointer text-[10px] font-extrabold text-blue-700">
                  <input type="checkbox" v-model="isOngoingOnly" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-3 h-3">
                  Thường xuyên
                </label>
              </div>
              <div class="flex items-center gap-2">
                <SearchableSelect 
                  v-model="fromYear" 
                  :options="yearOptions" 
                  :isMulti="false" 
                  placeholder="Từ năm" 
                  class="w-full"
                />
                <span class="text-xs font-bold text-slate-400 shrink-0">➔</span>
                <SearchableSelect 
                  v-model="toYear" 
                  :options="yearOptions" 
                  :isMulti="false" 
                  placeholder="Đến năm" 
                  class="w-full"
                />
              </div>
            </div>
          </div>
        </OverlayPanel>
      </div>

      <!-- Right group: Agency Count Badges -->
      <div v-if="!isSubAgencyUser" class="text-xs text-slate-500 font-bold flex items-center gap-2.5 shrink-0 whitespace-nowrap bg-slate-50 px-3 py-1.5 rounded-xl border border-slate-200/60">
        <span>Khối Bộ/Ngành: <strong class="text-blue-700">{{ metrics.ministriesPerformance?.length ?? 0 }}</strong></span>
        <span class="text-slate-300">•</span>
        <span>Khối Địa phương: <strong class="text-emerald-700">{{ metrics.provincesPerformance?.length ?? 0 }}</strong></span>
      </div>
      <div v-else class="text-xs text-slate-700 font-bold flex items-center gap-2 shrink-0 whitespace-nowrap bg-blue-50/80 px-3 py-1.5 rounded-xl border border-blue-200/80">
        <span class="text-blue-800">🏛️ {{ loggedUserAgency?.name || userAgencyName }}</span>
      </div>
    </div>

    <!-- Loading Spinner -->
    <LoadingSpinner v-if="isLoading" text="Đang tải dữ liệu tổng quan bảng điều khiển..." />

    <!-- 6 Execution Status Grid Cards (Filtered by Goal / Task / All) -->
    <div v-else class="space-y-3.5 w-full">
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-2.5 w-full items-stretch">
      <!-- 1. Chưa thực hiện -->
      <div class="bg-white p-3 rounded-2xl border border-slate-200 shadow-2xs flex flex-col justify-between h-full min-h-[76px]">
        <div class="text-[10px] font-bold text-slate-500 uppercase leading-snug">1. Chưa thực hiện</div>
        <div class="text-xl font-black text-slate-700 mt-1">{{ activeStatusSummary.notStarted ?? 0 }}</div>
      </div>

      <!-- 2. Đang thực hiện (trong hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-blue-200 bg-blue-50/40 shadow-2xs flex flex-col justify-between h-full min-h-[76px]">
        <div class="text-[10px] font-bold text-blue-700 uppercase leading-snug">2. Đang thực hiện (trong hạn)</div>
        <div class="text-xl font-black text-blue-800 mt-1">{{ activeStatusSummary.inProgressOnTime ?? 0 }}</div>
      </div>

      <!-- 3. Đang thực hiện (quá hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-rose-200 bg-rose-50/40 shadow-2xs flex flex-col justify-between h-full min-h-[76px]">
        <div class="text-[10px] font-bold text-rose-700 uppercase leading-snug">3. Đang thực hiện (quá hạn)</div>
        <div class="text-xl font-black text-rose-800 mt-1">{{ activeStatusSummary.inProgressOverdue ?? 0 }}</div>
      </div>

      <!-- 4. Hoàn thành (đúng hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-emerald-200 bg-emerald-50/40 shadow-2xs flex flex-col justify-between h-full min-h-[76px]">
        <div class="text-[10px] font-bold text-emerald-700 uppercase leading-snug">4. Hoàn thành (đúng hạn)</div>
        <div class="text-xl font-black text-emerald-800 mt-1">{{ activeStatusSummary.completedOnTime ?? 0 }}</div>
      </div>

      <!-- 5. Hoàn thành (quá hạn) -->
      <div class="bg-white p-3 rounded-2xl border border-teal-200 bg-teal-50/40 shadow-2xs flex flex-col justify-between h-full min-h-[76px]">
        <div class="text-[10px] font-bold text-teal-700 uppercase leading-snug">5. Hoàn thành (quá hạn)</div>
        <div class="text-xl font-black text-teal-800 mt-1">{{ activeStatusSummary.completedOverdue ?? 0 }}</div>
      </div>

      <!-- 6. Sắp hết hạn -->
      <div class="bg-white p-3 rounded-2xl border border-amber-200 bg-amber-50/40 shadow-2xs flex flex-col justify-between h-full min-h-[76px]">
        <div class="text-[10px] font-bold text-amber-700 uppercase leading-snug">6. Sắp hết hạn</div>
        <div class="text-xl font-black text-amber-800 mt-1">{{ activeStatusSummary.expiringSoon ?? 0 }}</div>
      </div>
    </div>

    <!-- Sub-Agency Dedicated Progress Dashboard Section (When logged in as Sub-Agency / Child Unit) -->
    <div v-if="isSubAgencyUser" class="space-y-4 w-full">
      <!-- 1. Progress of Level 2 Agency Itself -->
      <div v-if="singleSubAgencyPerformance" class="bg-white p-5 rounded-2xl shadow-sm border border-slate-200/80 space-y-4 w-full">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base font-extrabold text-slate-900 flex items-center gap-2">
              🏢 Bảng Tiến Độ Thực Hiện CỦA ĐƠN VỊ: <span class="text-blue-700 font-black">{{ singleSubAgencyPerformance.name }}</span>
            </h3>
            <span v-if="loggedUserAgency?.parentName" class="text-xs text-slate-500 font-semibold mt-0.5 block">
              Cơ quan quản lý trực tiếp: <strong>{{ loggedUserAgency.parentName }}</strong>
            </span>
          </div>
          
          <button 
            @click="drilldownAgency(singleSubAgencyPerformance)" 
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-extrabold text-xs rounded-xl shadow-2xs transition flex items-center gap-1.5 cursor-pointer shrink-0"
          >
            <span>📋 Xem Danh Sách Chi Tiết Nhiệm Vụ</span>
            <span>→</span>
          </button>
        </div>

        <!-- Big Circle Donut Chart & Status Breakdown -->
        <div class="bg-gradient-to-br from-slate-50 to-blue-50/40 rounded-2xl border border-slate-200 p-5 shadow-2xs space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-12 gap-6 items-center">
            
            <!-- Big Donut Circle Chart on Left -->
            <div class="md:col-span-5 flex flex-col items-center justify-center p-4 bg-white rounded-2xl border border-slate-200/80 shadow-2xs space-y-2">
              <MiniStatusDonut :stats="singleSubAgencyPerformance" :size="160" :innerSize="105" :fontSize="32" />
              <div class="text-center pt-1">
                <div class="text-xs font-black text-slate-800">Tổng số: {{ singleSubAgencyPerformance.totalItems || 0 }} hạng mục</div>
                <div class="flex items-center gap-2 justify-center text-[11px] font-bold text-slate-500 mt-1">
                  <span class="text-purple-800 bg-purple-50 px-2 py-0.5 rounded border border-purple-200/60">🎯 {{ singleSubAgencyPerformance.totalGoals || 0 }} Mục tiêu</span>
                  <span class="text-blue-800 bg-blue-50 px-2 py-0.5 rounded border border-blue-200/60">📋 {{ singleSubAgencyPerformance.totalTasks || 0 }} Nhiệm vụ</span>
                </div>
              </div>
            </div>

            <!-- 6 Status Legend Breakdown List on Right -->
            <div class="md:col-span-7 space-y-2 font-bold text-xs">
              <div class="flex items-center justify-between p-2.5 rounded-xl bg-white border border-slate-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-slate-400"></span>
                  <span class="text-slate-700">1. Chưa thực hiện</span>
                </div>
                <span class="font-black text-slate-900 text-sm">{{ singleSubAgencyPerformance.notStarted || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-blue-50/70 border border-blue-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-blue-500"></span>
                  <span class="text-blue-800">2. Đang thực hiện (trong hạn)</span>
                </div>
                <span class="font-black text-blue-900 text-sm">{{ singleSubAgencyPerformance.inProgressOnTime || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-rose-50/70 border border-rose-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-rose-500"></span>
                  <span class="text-rose-800">3. Đang thực hiện (quá hạn)</span>
                </div>
                <span class="font-black text-rose-900 text-sm">{{ singleSubAgencyPerformance.inProgressOverdue || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-emerald-50/70 border border-emerald-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-emerald-500"></span>
                  <span class="text-emerald-800">4. Hoàn thành (đúng hạn)</span>
                </div>
                <span class="font-black text-emerald-900 text-sm">{{ singleSubAgencyPerformance.completedOnTime || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-teal-50/70 border border-teal-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-teal-500"></span>
                  <span class="text-teal-800">5. Hoàn thành (quá hạn)</span>
                </div>
                <span class="font-black text-teal-900 text-sm">{{ singleSubAgencyPerformance.completedOverdue || 0 }}</span>
              </div>

              <div class="flex items-center justify-between p-2.5 rounded-xl bg-amber-50/70 border border-amber-200">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-amber-500"></span>
                  <span class="text-amber-800">6. Sắp hết hạn</span>
                </div>
                <span class="font-black text-amber-900 text-sm">{{ singleSubAgencyPerformance.expiringSoon || 0 }}</span>
              </div>
            </div>

          </div>
        </div>
      </div>

      <!-- 2. Subordinate Child Agencies Progress Block (Khối Các Đơn Vị Trực Thuộc) -->
      <div class="bg-white p-4.5 rounded-2xl shadow-sm border border-slate-200/80 space-y-4 w-full">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base font-extrabold text-slate-900 flex items-center gap-2">
              🏛️ Khối Các Đơn Vị Trực Thuộc
            </h3>
            <p class="text-xs text-slate-500 font-semibold mt-0.5">
              Thống kê tiến độ thực hiện nhiệm vụ các đơn vị trực thuộc (Đã sắp xếp theo tổng số nhiệm vụ giảm dần)
            </p>
          </div>
          <span class="text-xs font-extrabold text-blue-700 bg-blue-50 px-3 py-1 rounded-xl border border-blue-200/60">
            {{ userSubAgenciesPerformance?.length ?? 0 }} Đơn vị trực thuộc
          </span>
        </div>

        <div v-if="userSubAgenciesPerformance?.length" class="space-y-3">
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-4 gap-3.5">
            <div 
              v-for="(item, index) in visibleSubAgencies" 
              :key="item.agencyId"
              @click="drilldownAgency(item)"
              class="bg-white rounded-2xl border border-slate-200/90 p-3.5 shadow-2xs hover:shadow-md transition-all duration-200 cursor-pointer space-y-3 group flex flex-col justify-between"
            >
              <!-- Card Header: Agency Name & Blue Index Badge -->
              <div class="flex items-start justify-between gap-2 border-b border-slate-100 pb-2.5">
                <div class="min-w-0 flex-1">
                  <h4 class="text-xs sm:text-sm font-extrabold text-slate-800 group-hover:text-blue-600 transition leading-snug truncate" :title="item.name">
                    {{ item.name }}
                  </h4>
                </div>

                <div class="w-7 h-7 bg-blue-600 text-white font-black text-xs rounded-xl flex items-center justify-center shrink-0 shadow-2xs">
                  {{ index + 1 }}
                </div>
              </div>

              <!-- Card Body: Donut Chart on Left, Legend Breakdown List on Right -->
              <div class="flex items-center gap-3 py-0.5">
                <!-- Donut Chart -->
                <div class="shrink-0 flex items-center justify-center">
                  <MiniStatusDonut :stats="item" :size="84" :innerSize="54" :fontSize="18" />
                </div>

                <!-- 5-6 Status Legend List -->
                <div class="flex-1 min-w-0 space-y-1 text-[10px] font-bold">
                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-rose-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.inProgressOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.inProgressOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-amber-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Sắp tới hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.expiringSoon || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-teal-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.completedOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.completedOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5" v-if="item.notStarted > 0">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-slate-400 shrink-0"></span>
                      <span class="text-slate-600 truncate">Chưa thực hiện</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.notStarted || 0 }}</span>
                  </div>
                </div>
              </div>

              <!-- Card Footer: Sub-badges -->
              <div class="pt-2 border-t border-slate-100 flex items-center justify-between text-[10px] text-slate-500 font-bold">
                <div class="flex items-center gap-1.5">
                  <span class="text-purple-800 bg-purple-50 px-1.5 py-0.5 rounded border border-purple-200/60">🎯 {{ item.totalGoals || 0 }}</span>
                  <span class="text-blue-800 bg-blue-50 px-1.5 py-0.5 rounded border border-blue-200/60">📋 {{ item.totalTasks || 0 }}</span>
                </div>
                <span class="text-blue-600 group-hover:underline">Chi tiết →</span>
              </div>
            </div>
          </div>

          <!-- Expand / Collapse Button -->
          <div v-if="(userSubAgenciesPerformance?.length || 0) > 6" class="pt-2 text-center border-t border-slate-100">
            <button 
              @click="isSubAgenciesExpanded = !isSubAgenciesExpanded" 
              class="px-5 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
            >
              <span>{{ isSubAgenciesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${userSubAgenciesPerformance.length - 6} Đơn vị trực thuộc khác)` }}</span>
            </button>
          </div>
        </div>

        <div v-else class="p-8 text-center text-xs text-slate-400 italic font-semibold bg-slate-50/50 rounded-xl border border-slate-200/60">
          Chưa có đơn vị trực thuộc nào được giao nhiệm vụ.
        </div>
      </div>
    </div>

    <!-- 2 Main Sections: Khối Bộ / Ngành & Khối Địa Phương (Admin / Parent Agency View) -->
    <div v-else class="space-y-6 w-full">
      
      <!-- Section 1: Khối Bộ / Ngành -->
      <div class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-900 flex items-center gap-2">
            🏢 Khối Các Bộ / Ngành Trung Ương
          </h3>
          <span class="text-xs font-extrabold text-blue-600 bg-blue-50 px-3 py-1 rounded-xl border border-blue-200/60">
            {{ metrics.ministriesPerformance?.length ?? 0 }} Bộ/Ngành
          </span>
        </div>

        <div v-if="metrics.ministriesPerformance?.length" class="space-y-3">
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-4 gap-3.5">
            <div 
              v-for="(item, index) in visibleMinistries" 
              :key="item.agencyId"
              @click="drilldownAgency(item)"
              class="bg-white rounded-2xl border border-slate-200/90 p-3.5 shadow-2xs hover:shadow-md transition-all duration-200 cursor-pointer space-y-3 group flex flex-col justify-between"
            >
              <!-- Card Header: Agency Name & Blue Index Badge -->
              <div class="flex items-start justify-between gap-2 border-b border-slate-100 pb-2.5">
                <div class="min-w-0 flex-1">
                  <h4 class="text-xs sm:text-sm font-extrabold text-slate-800 group-hover:text-blue-600 transition leading-snug truncate">
                    {{ item.name }}
                  </h4>
                </div>

                <div class="w-7 h-7 bg-blue-600 text-white font-black text-xs rounded-xl flex items-center justify-center shrink-0 shadow-2xs">
                  {{ index + 1 }}
                </div>
              </div>

              <!-- Card Body: Donut Chart on Left, Legend Breakdown List on Right -->
              <div class="flex items-center gap-3 py-0.5">
                <!-- Donut Chart -->
                <div class="shrink-0 flex items-center justify-center">
                  <MiniStatusDonut :stats="item" :size="84" :innerSize="54" :fontSize="18" />
                </div>

                <!-- 5-6 Status Legend List -->
                <div class="flex-1 min-w-0 space-y-1 text-[10px] font-bold">
                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-rose-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.inProgressOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.inProgressOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-amber-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Sắp tới hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.expiringSoon || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-teal-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.completedOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.completedOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5" v-if="item.notStarted > 0">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-slate-400 shrink-0"></span>
                      <span class="text-slate-600 truncate">Chưa thực hiện</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.notStarted || 0 }}</span>
                  </div>
                </div>
              </div>

              <!-- Card Footer: Sub-badges -->
              <div class="pt-2 border-t border-slate-100 flex items-center justify-between text-[10px] text-slate-500 font-bold">
                <div class="flex items-center gap-1.5">
                  <span class="text-purple-800 bg-purple-50 px-1.5 py-0.5 rounded border border-purple-200/60">🎯 {{ item.totalGoals || 0 }}</span>
                  <span class="text-blue-800 bg-blue-50 px-1.5 py-0.5 rounded border border-blue-200/60">📋 {{ item.totalTasks || 0 }}</span>
                </div>
                <span class="text-blue-600 group-hover:underline">Chi tiết →</span>
              </div>
            </div>
          </div>

          <!-- Expand / Collapse Button -->
          <div v-if="(metrics.ministriesPerformance?.length || 0) > 6" class="pt-2 text-center border-t border-slate-100">
            <button 
              @click="isMinistriesExpanded = !isMinistriesExpanded" 
              class="px-5 py-2 text-xs font-bold text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
            >
              <span>{{ isMinistriesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${metrics.ministriesPerformance.length - 6} Bộ/Ngành khác)` }}</span>
            </button>
          </div>
        </div>

        <div v-if="!metrics.ministriesPerformance?.length" class="p-8 text-center text-xs text-slate-400 italic font-semibold">
          Không có dữ liệu Bộ/Ngành.
        </div>
      </div>

      <!-- Section 2: Khối Địa Phương -->
      <div class="bg-white p-4 rounded-2xl shadow-sm border border-slate-200/80 space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-base font-extrabold text-slate-900 flex items-center gap-2">
            🏛️ Khối Các Tỉnh / Thành Phố
          </h3>
          <span class="text-xs font-extrabold text-emerald-600 bg-emerald-50 px-3 py-1 rounded-xl border border-emerald-200/60">
            {{ metrics.provincesPerformance?.length ?? 0 }} Địa phương
          </span>
        </div>

        <div v-if="metrics.provincesPerformance?.length" class="space-y-3">
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-4 gap-3.5">
            <div 
              v-for="(item, index) in visibleProvinces" 
              :key="item.agencyId"
              @click="drilldownAgency(item)"
              class="bg-white rounded-2xl border border-slate-200/90 p-3.5 shadow-2xs hover:shadow-md transition-all duration-200 cursor-pointer space-y-3 group flex flex-col justify-between"
            >
              <!-- Card Header: Agency Name & Blue Index Badge -->
              <div class="flex items-start justify-between gap-2 border-b border-slate-100 pb-2.5">
                <div class="min-w-0 flex-1">
                  <h4 class="text-xs sm:text-sm font-extrabold text-slate-800 group-hover:text-emerald-700 transition leading-snug truncate">
                    {{ item.name }}
                  </h4>
                </div>

                <div class="w-7 h-7 bg-blue-600 text-white font-black text-xs rounded-xl flex items-center justify-center shrink-0 shadow-2xs">
                  {{ index + 1 }}
                </div>
              </div>

              <!-- Card Body: Donut Chart on Left, Legend Breakdown List on Right -->
              <div class="flex items-center gap-3 py-0.5">
                <!-- Donut Chart -->
                <div class="shrink-0 flex items-center justify-center">
                  <MiniStatusDonut :stats="item" :size="84" :innerSize="54" :fontSize="18" />
                </div>

                <!-- 5-6 Status Legend List -->
                <div class="flex-1 min-w-0 space-y-1 text-[10px] font-bold">
                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-rose-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.inProgressOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.inProgressOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-amber-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Sắp tới hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.expiringSoon || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-teal-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.completedOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.completedOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5" v-if="item.notStarted > 0">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-slate-400 shrink-0"></span>
                      <span class="text-slate-600 truncate">Chưa thực hiện</span>
                    </div>
                    <span class="font-black text-slate-900">{{ item.notStarted || 0 }}</span>
                  </div>
                </div>
              </div>

              <!-- Card Footer: Sub-badges -->
              <div class="pt-2 border-t border-slate-100 flex items-center justify-between text-[10px] text-slate-500 font-bold">
                <div class="flex items-center gap-1.5">
                  <span class="text-purple-800 bg-purple-50 px-1.5 py-0.5 rounded border border-purple-200/60">🎯 {{ item.totalGoals || 0 }}</span>
                  <span class="text-blue-800 bg-blue-50 px-1.5 py-0.5 rounded border border-blue-200/60">📋 {{ item.totalTasks || 0 }}</span>
                </div>
                <span class="text-emerald-700 group-hover:underline">Chi tiết →</span>
              </div>
            </div>
          </div>

          <!-- Expand / Collapse Button -->
          <div v-if="(metrics.provincesPerformance?.length || 0) > 6" class="pt-2 text-center border-t border-slate-100">
            <button 
              @click="isProvincesExpanded = !isProvincesExpanded" 
              class="px-5 py-2 text-xs font-bold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 rounded-xl transition shadow-2xs inline-flex items-center gap-2 cursor-pointer"
            >
              <span>{{ isProvincesExpanded ? '▲ Thu gọn danh sách' : `▼ Xem thêm (${metrics.provincesPerformance.length - 6} Địa phương khác)` }}</span>
            </button>
          </div>
        </div>

          <div v-if="!metrics.provincesPerformance?.length" class="col-span-full p-8 text-center text-xs text-slate-400 italic font-semibold">
            Không có dữ liệu Địa phương.
          </div>
        </div>
      </div>

    </div>

    <!-- Drilldown Sub-agencies, Tasks List & Contact Persons Modal -->
    <div v-if="selectedDrilldownAgency" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 font-sans">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-6xl sm:max-w-7xl w-full p-5 sm:p-6 space-y-4">
        
        <!-- Modal Header -->
        <div class="flex items-start justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 class="text-base sm:text-lg font-extrabold text-slate-900 flex items-center gap-2">
              <span class="p-1.5 bg-blue-100 text-blue-700 rounded-xl text-sm">🏛️</span>
              {{ selectedDrilldownAgency.name }}
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">Chi tiết tiến độ đơn vị trực thuộc, danh sách nhiệm vụ được gán và thông tin cán bộ đầu mối liên hệ</p>
          </div>
          <button @click="selectedDrilldownAgency = null" class="p-1.5 text-slate-400 hover:text-slate-700 bg-slate-100 rounded-xl transition cursor-pointer">✕</button>
        </div>

        <!-- Navigation Tabs inside Modal -->
        <div class="flex items-center gap-2 border-b border-slate-200/80 pb-2 overflow-x-auto custom-scrollbar">
          <!-- TAB 1: Goal/Task Items Assigned to Agency -->
          <button 
            @click="switchDrilldownTab('tasks')" 
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'tasks' ? 'bg-blue-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>📋 Danh Sách Nhiệm Vụ Được Gán ({{ isAgencyItemsLoading ? '...' : agencyItemsList.length }})</span>
          </button>

          <!-- TAB 2: Sub-Agencies & Progress -->
          <button 
            @click="switchDrilldownTab('sub-agencies')" 
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'sub-agencies' ? 'bg-blue-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>🏛️ Đơn Vị Trực Thuộc ({{ isSubAgenciesLoading ? '...' : subAgenciesList.length }})</span>
          </button>
          
          <!-- TAB 3: Contact Persons -->
          <button 
            @click="switchDrilldownTab('contacts')" 
            :class="[
              'px-3.5 py-1.5 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shrink-0',
              drilldownTab === 'contacts' ? 'bg-blue-600 text-white shadow-xs' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            ]"
          >
            <span>📞 Cán Bộ Đầu Mối Liên Hệ ({{ isSubAgenciesLoading ? '...' : allDrilldownContacts.length }})</span>
          </button>
        </div>

        <!-- TAB 1 CONTENT: Sub-Agencies & Progress -->
        <div v-if="drilldownTab === 'sub-agencies'" class="space-y-3.5 max-h-[60vh] overflow-y-auto custom-scrollbar pr-1">
          <!-- Loading State -->
          <div v-if="isSubAgenciesLoading" class="p-8 text-center">
            <LoadingSpinner size="md" message="Đang tải danh sách đơn vị trực thuộc..." />
          </div>

          <div v-else-if="subAgenciesList.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
            <div 
              v-for="(child, index) in subAgenciesList" 
              :key="child.agencyId" 
              class="bg-white rounded-2xl border border-slate-200/90 p-3.5 shadow-2xs hover:shadow-md transition-all duration-200 space-y-3 flex flex-col justify-between"
            >
              <!-- Card Header: Child Agency Name & Blue Index Badge -->
              <div class="flex items-start justify-between gap-2 border-b border-slate-100 pb-2.5">
                <div class="min-w-0 flex-1">
                  <h4 class="text-xs sm:text-sm font-extrabold text-slate-800 leading-snug truncate" :title="child.name">
                    {{ child.name }}
                  </h4>
                  <p class="text-[11px] text-slate-500 font-medium mt-0.5">
                    Tổng {{ child.totalItems || 0 }} hạng mục (🎯 {{ child.totalGoals || 0 }} Mục tiêu, 📋 {{ child.totalTasks || 0 }} Nhiệm vụ)
                  </p>
                </div>

                <div class="w-7 h-7 bg-blue-600 text-white font-black text-xs rounded-xl flex items-center justify-center shrink-0 shadow-2xs">
                  {{ index + 1 }}
                </div>
              </div>

              <!-- Card Body: Donut Chart on Left, Legend Breakdown List on Right -->
              <div class="flex items-center gap-3 py-0.5">
                <!-- Donut Chart -->
                <div class="shrink-0 flex items-center justify-center">
                  <MiniStatusDonut :stats="child" :size="84" :innerSize="54" :fontSize="18" />
                </div>

                <!-- 6 Status Legend List -->
                <div class="flex-1 min-w-0 space-y-1 text-[10px] font-bold">
                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-rose-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ child.inProgressOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-blue-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đang t/h trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ child.inProgressOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-amber-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Sắp tới hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ child.expiringSoon || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-teal-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t quá hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ child.completedOverdue || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-emerald-500 shrink-0"></span>
                      <span class="text-slate-600 truncate">Đã h/t trong hạn</span>
                    </div>
                    <span class="font-black text-slate-900">{{ child.completedOnTime || 0 }}</span>
                  </div>

                  <div class="flex items-center justify-between gap-1.5">
                    <div class="flex items-center gap-1.5 min-w-0">
                      <span class="w-2.5 h-2.5 rounded-full bg-slate-400 shrink-0"></span>
                      <span class="text-slate-600 truncate">Chưa thực hiện</span>
                    </div>
                    <span class="font-black text-slate-900">{{ child.notStarted || 0 }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-if="subAgenciesList.length === 0" class="p-8 text-center text-xs text-slate-400 font-semibold italic">
            Không có đơn vị trực thuộc nào.
          </div>
        </div>

        <!-- TAB 2: Goal/Task Items Assigned to Agency -->
        <div v-if="drilldownTab === 'tasks'" class="space-y-3 max-h-[60vh] overflow-y-auto custom-scrollbar pr-1">
          <!-- Local Filters Toolbar -->
          <div class="flex flex-wrap items-center justify-between gap-2 bg-slate-50 p-2.5 rounded-xl border border-slate-200/80">
            <div class="flex items-center gap-2 flex-1 min-w-[200px]">
              <div class="relative w-full">
                <span class="absolute inset-y-0 left-0 flex items-center pl-2.5 text-slate-400 text-xs">🔍</span>
                <input 
                  v-model="modalSearchKeyword" 
                  type="text" 
                  placeholder="Tìm mã, tên nhiệm vụ, lĩnh vực..." 
                  class="w-full pl-8 pr-3 py-1.5 bg-white text-xs border border-slate-200 rounded-lg focus:outline-none focus:border-blue-500 text-slate-800"
                />
              </div>
            </div>

            <div class="flex items-center gap-2 shrink-0">
              <!-- Item Type Filter -->
              <select 
                v-model="modalItemTypeFilter" 
                class="py-1.5 px-2.5 text-xs bg-white border border-slate-200 rounded-lg font-medium text-slate-700 focus:outline-none focus:border-blue-500 cursor-pointer"
              >
                <option value="all">Tất cả loại</option>
                <option value="Goal">🎯 Chỉ Mục tiêu</option>
                <option value="Task">📋 Chỉ Nhiệm vụ</option>
              </select>

              <!-- Status Filter -->
              <select 
                v-model="modalStatusFilter" 
                class="py-1.5 px-2.5 text-xs bg-white border border-slate-200 rounded-lg font-medium text-slate-700 focus:outline-none focus:border-blue-500 cursor-pointer"
              >
                <option value="all">Tất cả trạng thái</option>
                <option value="InProgressOverdue">🔴 Đang t/h quá hạn</option>
                <option value="InProgressOnTime">🟢 Đang t/h trong hạn</option>
                <option value="ExpiringSoon">🟣 Sắp tới hạn</option>
                <option value="CompletedOverdue">🟠 Đã h/t quá hạn</option>
                <option value="CompletedOnTime">🔵 Đã h/t trong hạn</option>
                <option value="NotStarted">⚪ Chưa thực hiện</option>
              </select>
            </div>
          </div>

          <!-- Loading State -->
          <div v-if="isAgencyItemsLoading" class="p-8 text-center">
            <LoadingSpinner size="md" message="Đang tải danh sách nhiệm vụ..." />
          </div>

          <!-- Task Items List (BẢNG) -->
          <div v-else-if="filteredAgencyItems.length > 0" class="border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-xs">
            <div class="overflow-x-auto overflow-y-auto max-h-[50vh] custom-scrollbar w-full">
              <table class="w-full min-w-[850px] text-left text-xs text-slate-700 border-collapse">
                <thead class="bg-slate-100 text-xs text-slate-600 uppercase font-bold border-b border-slate-200 sticky top-0 z-30 shadow-2xs">
                  <tr>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[70px] w-[70px] max-w-[70px] sticky left-0 z-30">Mã</th>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[260px]">
                      Tên Mục Tiêu / Nhiệm Vụ
                    </th>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 min-w-[140px] w-[140px]">Cơ Quan Chủ Trì</th>
                    <th class="px-3 py-2.5 border-r border-slate-200 bg-slate-100 whitespace-nowrap min-w-[130px] w-[130px]">Thời Gian</th>
                    <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[100px] w-[100px]">Tiến Độ</th>
                    <th class="px-3 py-2.5 border-r border-slate-200 text-center bg-slate-100 whitespace-nowrap min-w-[150px] w-[150px]">Trạng Thái</th>
                    <th class="px-3 py-2.5 text-center bg-slate-100 whitespace-nowrap min-w-[90px] w-[90px]">Chi Tiết</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-200">
                  <tr 
                    v-for="item in filteredAgencyItems" 
                    :key="item.id"
                    @click="selectedDetailItem = item"
                    class="group hover:bg-blue-50/80 transition cursor-pointer"
                  >
                    <!-- Mã -->
                    <td class="px-3 py-2.5 font-extrabold text-blue-700 border-r border-slate-200 whitespace-nowrap sticky left-0 z-20 bg-white group-hover:bg-blue-50">
                      {{ item.code || 'NV' }}
                    </td>

                    <!-- Tên & Phân loại -->
                    <td class="px-3 py-2.5 border-r border-slate-200 leading-relaxed">
                      <div class="flex items-center gap-1.5 flex-wrap mb-1">
                        <span :class="['px-2 py-0.5 text-[10px] font-bold rounded-md border', item.itemType === 'Goal' ? 'bg-purple-50 text-purple-700 border-purple-200' : 'bg-slate-100 text-slate-700 border-slate-200']">
                          {{ item.itemType === 'Goal' ? '🎯 Mục tiêu' : '📋 Nhiệm vụ' }}
                        </span>
                        <span :class="['px-2 py-0.5 text-[10px] font-bold rounded-md border', item.isGeneralTask ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-50 text-slate-600 border-slate-200']">
                          {{ item.isGeneralTask ? 'Phạm vi chung' : 'Phạm vi riêng' }}
                        </span>
                        <span v-if="item.section || item.category" class="text-[10px] font-medium text-slate-400">
                          • {{ item.section || item.category }}
                        </span>
                      </div>
                      <div class="font-semibold text-slate-900 leading-snug line-clamp-2" :title="item.title">
                        {{ item.title }}
                      </div>
                    </td>

                    <!-- Cơ quan chủ trì -->
                    <td class="px-3 py-2.5 border-r border-slate-200 font-bold text-slate-800 text-xs leading-relaxed truncate" :title="item.leadAgencyName">
                      {{ item.leadAgencyName }}
                    </td>

                    <!-- Thời gian -->
                    <td class="px-3 py-2.5 border-r border-slate-200 text-xs font-semibold text-slate-600 whitespace-nowrap">
                      <span v-if="item.isOngoing" class="px-2 py-0.5 rounded-full font-extrabold text-[11px] bg-blue-100 text-blue-800">
                        Thường xuyên
                      </span>
                      <span v-else-if="item.dueDate">
                        📅 {{ formatDate(item.dueDate) }}
                      </span>
                      <span v-else class="text-slate-400 italic">—</span>
                    </td>

                    <!-- Tiến độ -->
                    <td class="px-3 py-2.5 border-r border-slate-200 text-center text-xs whitespace-nowrap">
                      <span v-if="formatItemProgressDisplay(item) !== '—'" class="font-extrabold px-2 py-0.5 rounded-lg text-xs bg-blue-50 text-blue-900 border border-blue-200">
                        {{ formatItemProgressDisplay(item) }}
                      </span>
                      <span v-else class="text-slate-400 italic">—</span>
                    </td>

                    <!-- Trạng thái -->
                    <td class="px-3 py-2.5 border-r border-slate-200 text-center whitespace-nowrap">
                      <span :class="['px-2.5 py-0.5 rounded-full text-[11px] font-bold shadow-2xs inline-block whitespace-nowrap border', getStatusBadgeClass(item.status)]">
                        {{ getStatusLabel(item.status) }}
                      </span>
                    </td>

                    <!-- Nút thao tác Xem chi tiết -->
                    <td class="px-3 py-2.5 text-center whitespace-nowrap" @click.stop>
                      <button 
                        @click.stop="selectedDetailItem = item"
                        class="px-2 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-[11px] rounded-lg border border-blue-200 transition shadow-2xs inline-flex items-center gap-1 cursor-pointer"
                        title="Xem chi tiết"
                      >
                        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                        <span>Chi tiết</span>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Empty State -->
          <div v-else class="p-8 text-center text-xs text-slate-400 font-semibold italic">
            Không tìm thấy mục tiêu, nhiệm vụ nào phù hợp với bộ lọc.
          </div>
        </div>

        <!-- TAB 3: Contact Persons -->
        <div v-if="drilldownTab === 'contacts'" class="space-y-3 max-h-[60vh] overflow-y-auto custom-scrollbar pr-1">
          <!-- Loading State -->
          <div v-if="isSubAgenciesLoading" class="p-8 text-center">
            <LoadingSpinner size="md" message="Đang tải danh sách cán bộ đầu mối..." />
          </div>

          <div v-else-if="allDrilldownContacts.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div 
              v-for="(contact, index) in allDrilldownContacts" 
              :key="index"
              class="bg-slate-50 p-3.5 rounded-xl border border-slate-200/80 space-y-2 hover:bg-blue-50/40 transition"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex items-center gap-2.5">
                  <div class="w-8 h-8 rounded-full bg-blue-600 text-white font-black text-xs flex items-center justify-center shrink-0 shadow-xs uppercase">
                    {{ (contact.name || 'CB').charAt(0) }}
                  </div>
                  <div>
                    <h5 class="text-xs font-bold text-slate-900 leading-snug">{{ contact.name || 'Cán bộ đầu mối' }}</h5>
                    <p class="text-[11px] font-semibold text-blue-700 mt-0.5">{{ contact.position || 'Cán bộ liên hệ' }}</p>
                  </div>
                </div>

                <span :class="['text-[10px] font-bold px-2 py-0.5 rounded-md border shrink-0', contact.isParent ? 'bg-purple-50 text-purple-700 border-purple-200' : 'bg-slate-200/60 text-slate-700 border-slate-300']">
                  {{ contact.unitCode || 'Đơn vị' }}
                </span>
              </div>

              <div class="text-[11px] text-slate-600 space-y-1 pt-1.5 border-t border-slate-200/60">
                <div v-if="contact.department" class="flex items-center gap-1.5">
                  <span class="text-slate-400 font-bold">🏢 Phòng ban:</span>
                  <span class="font-semibold text-slate-800">{{ contact.department }}</span>
                </div>

                <div v-if="contact.phone" class="flex items-center gap-1.5">
                  <span class="text-slate-400 font-bold">📞 Điện thoại:</span>
                  <a :href="`tel:${contact.phone}`" class="font-bold text-blue-600 hover:underline">{{ contact.phone }}</a>
                </div>

                <div v-if="contact.email" class="flex items-center gap-1.5">
                  <span class="text-slate-400 font-bold">✉️ Email:</span>
                  <a :href="`mailto:${contact.email}`" class="font-bold text-blue-600 hover:underline truncate">{{ contact.email }}</a>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="p-8 text-center text-xs text-slate-400 font-semibold italic">
            Chưa có thông tin cán bộ đầu mối liên hệ cho cơ quan này.
          </div>
        </div>

      </div>
    </div>

    <!-- Item Detail Modal -->
    <ItemDetailModal 
      :isOpen="selectedDetailItem != null" 
      :item="selectedDetailItem" 
      @close="selectedDetailItem = null" 
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import XLSX from 'xlsx-js-style';
import { styleWorksheet } from '../utils/excelExport';
import SearchableSelect from '../components/SearchableSelect.vue';
import LoadingSpinner from '../components/LoadingSpinner.vue';
import MiniStatusDonut from '../components/MiniStatusDonut.vue';
import OverlayPanel from '../components/OverlayPanel.vue';
import ItemDetailModal from '../components/ItemDetailModal.vue';
import { getApiUrl } from '../config/api';
import { authState } from '../services/auth';
import { GOAL_SECTIONS, GOAL_GROUPS, TASK_SECTIONS, TASK_GROUPS } from '../config/planningStructureConfig';

const dashboardFilter = ref('all'); // 'all', 'goals', 'tasks'
const isLoading = ref(false);

const selectedAgencyIds = ref([]);
const selectedScopes = ref([]);
const selectedSections = ref([]);
const selectedGroups = ref([]);
const fromYear = ref(null);
const toYear = ref(null);
const isOngoingOnly = ref(false);
const agencies = ref([]);

const dashboardFilterOptions = ref([
  { value: 'all', label: 'Tất cả (Mục tiêu & Nhiệm vụ)' },
  { value: 'goals', label: '🎯 Chỉ Mục tiêu' },
  { value: 'tasks', label: '📋 Chỉ Nhiệm vụ' }
]);

const scopeOptions = ref([
  { value: 'general', label: 'Phạm vi Chung (Tất cả đơn vị)' },
  { value: 'specific', label: 'Phạm vi Riêng (Đơn vị cụ thể)' }
]);

const activeDashboardFilterCount = computed(() => {
  let count = 0;
  if (dashboardFilter.value && dashboardFilter.value !== 'all') count++;
  if (selectedAgencyIds.value?.length) count++;
  if (selectedScopes.value?.length) count++;
  if (selectedSections.value?.length) count++;
  if (selectedGroups.value?.length) count++;
  if (fromYear.value || toYear.value) count++;
  if (isOngoingOnly.value) count++;
  return count;
});

function resetDashboardFilters() {
  selectedAgencyIds.value = [];
  selectedScopes.value = [];
  selectedSections.value = [];
  selectedGroups.value = [];
  fromYear.value = null;
  toYear.value = null;
  isOngoingOnly.value = false;
  dashboardFilter.value = 'all';
  loadDashboardMetrics();
}

const agencyOptions = computed(() => {
  return agencies.value.map(ag => ({ value: ag.id, label: ag.name }));
});

const yearOptions = computed(() => [2026, 2027, 2028, 2029, 2030].map(y => ({ value: y, label: String(y) })));

const sectionOptions = computed(() => [...GOAL_SECTIONS, ...TASK_SECTIONS]);
const groupOptions = computed(() => [...GOAL_GROUPS, ...TASK_GROUPS]);

const metrics = ref({
  totalGoals: 0,
  completedGoals: 0,
  totalTasks: 0,
  completedTasks: 0,
  statusSummary: {},
  goalStatusSummary: {},
  taskStatusSummary: {},
  ministriesPerformance: [],
  provincesPerformance: []
});

const activeStatusSummary = computed(() => {
  if (dashboardFilter.value === 'goals') return metrics.value.goalStatusSummary || {};
  if (dashboardFilter.value === 'tasks') return metrics.value.taskStatusSummary || {};
  return metrics.value.statusSummary || {};
});

const userSubAgenciesPerformance = ref([]);
const isMinistriesExpanded = ref(false);
const isProvincesExpanded = ref(false);
const isSubAgenciesExpanded = ref(false);

const visibleMinistries = computed(() => {
  const list = metrics.value.ministriesPerformance || [];
  if (isMinistriesExpanded.value || list.length <= 6) return list;
  return list.slice(0, 6);
});

const visibleProvinces = computed(() => {
  const list = metrics.value.provincesPerformance || [];
  if (isProvincesExpanded.value || list.length <= 6) return list;
  return list.slice(0, 6);
});

const visibleSubAgencies = computed(() => {
  const list = userSubAgenciesPerformance.value || [];
  if (isSubAgenciesExpanded.value || list.length <= 6) return list;
  return list.slice(0, 6);
});
const selectedDrilldownAgency = ref(null);
const subAgenciesList = ref([]);
const isSubAgenciesLoading = ref(false);
const drilldownTab = ref('tasks'); // Default Tab 1: 'tasks' | 'sub-agencies' | 'contacts'

const selectedDetailItem = ref(null);
const agencyItemsList = ref([]);
const isAgencyItemsLoading = ref(false);
const modalSearchKeyword = ref('');
const modalItemTypeFilter = ref('all');
const modalStatusFilter = ref('all');

const filteredAgencyItems = computed(() => {
  let list = agencyItemsList.value || [];

  if (modalItemTypeFilter.value && modalItemTypeFilter.value !== 'all') {
    list = list.filter(i => i.itemType === modalItemTypeFilter.value);
  }

  if (modalStatusFilter.value && modalStatusFilter.value !== 'all') {
    list = list.filter(i => i.status === modalStatusFilter.value);
  }

  if (modalSearchKeyword.value && modalSearchKeyword.value.trim() !== '') {
    const q = modalSearchKeyword.value.trim().toLowerCase();
    list = list.filter(i => 
      (i.code && i.code.toLowerCase().includes(q)) ||
      (i.title && i.title.toLowerCase().includes(q)) ||
      (i.category && i.category.toLowerCase().includes(q)) ||
      (i.section && i.section.toLowerCase().includes(q)) ||
      (i.group && i.group.toLowerCase().includes(q)) ||
      (i.leadAgencyName && i.leadAgencyName.toLowerCase().includes(q))
    );
  }

  return list;
});

function getStatusBadgeClass(status) {
  switch (status) {
    case 'InProgressOverdue':
      return 'bg-rose-50 text-rose-700 border-rose-200';
    case 'InProgressOnTime':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    case 'ExpiringSoon':
      return 'bg-purple-50 text-purple-700 border-purple-200';
    case 'CompletedOverdue':
      return 'bg-amber-50 text-amber-700 border-amber-200';
    case 'CompletedOnTime':
      return 'bg-blue-50 text-blue-700 border-blue-200';
    case 'NotStarted':
    default:
      return 'bg-slate-100 text-slate-700 border-slate-300';
  }
}

function getStatusLabel(status) {
  switch (status) {
    case 'InProgressOverdue':
      return '🔴 Đang t/h quá hạn';
    case 'InProgressOnTime':
      return '🟢 Đang t/h trong hạn';
    case 'ExpiringSoon':
      return '🟣 Sắp tới hạn';
    case 'CompletedOverdue':
      return '🟠 Đã h/t quá hạn';
    case 'CompletedOnTime':
      return '🔵 Đã h/t trong hạn';
    case 'NotStarted':
    default:
      return '⚪ Chưa thực hiện';
  }
}

function isGeneralTaskItem(item) {
  if (!item) return false;
  if (item.isGeneralTask) return true;
  if (item.leadAgencyCode === 'ALL_AGENCIES') return true;
  if (item.leadAgencyId && String(item.leadAgencyId).toLowerCase() === '00000000-0000-0000-0000-000000009999') return true;
  if (item.leadAgencyName && item.leadAgencyName.toLowerCase().trim() === 'các bộ, ngành, địa phương') return true;
  return false;
}

function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    let str = String(dateStr).trim();
    if (!str) return '—';
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
      const [y, m, d] = str.slice(0, 10).split('-');
      return `${d}/${m}/${y}`;
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return dateStr;
    const day = String(d.getUTCDate()).padStart(2, '0');
    const month = String(d.getUTCMonth() + 1).padStart(2, '0');
    const year = d.getUTCFullYear();
    return `${day}/${month}/${year}`;
  } catch (e) {
    return dateStr || '—';
  }
}

function formatItemProgressDisplay(item) {
  if (!item) return '—';

  const unit = item.unitName || item.unit?.name;

  if (unit === 'Số lượng') {
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      return `${item.latestProgressValue}`;
    }
  }

  if (unit && unit !== '%' && unit !== 'Số lượng') {
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      return `${item.latestProgressValue} ${unit}`;
    }
  }

  if ((!unit || unit === '%') && item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    return `${item.latestProgressValue}%`;
  }

  if (item.latestProgressPercent !== null && item.latestProgressPercent !== undefined) {
    return `${item.latestProgressPercent}%`;
  }

  return '—';
}

const allDrilldownContacts = computed(() => {
  if (!selectedDrilldownAgency.value) return [];
  const list = [];

  // Contacts from root parent agency
  if (selectedDrilldownAgency.value.contactPersons?.length) {
    selectedDrilldownAgency.value.contactPersons.forEach(c => {
      list.push({
        ...c,
        unitName: selectedDrilldownAgency.value.name,
        unitCode: selectedDrilldownAgency.value.code,
        isParent: true
      });
    });
  }

  // Contacts from child agencies
  if (subAgenciesList.value?.length) {
    subAgenciesList.value.forEach(sub => {
      if (sub.contactPersons?.length) {
        sub.contactPersons.forEach(c => {
          list.push({
            ...c,
            unitName: sub.name,
            unitCode: sub.code,
            isParent: false
          });
        });
      }
    });
  }

  return list;
});

const userAgencyName = computed(() => {
  return authState.user.value?.agencyName || 'Cơ quan/Bộ/Địa phương';
});

const loggedUserAgencyId = computed(() => authState.user.value?.agencyId ? String(authState.user.value.agencyId).toLowerCase() : '');
const loggedUserAgency = computed(() => agencies.value.find(a => String(a.id).toLowerCase() === loggedUserAgencyId.value));
const isSubAgencyUser = computed(() => !authState.isAdmin.value && !!loggedUserAgencyId.value);

const singleSubAgencyPerformance = computed(() => {
  const allPerf = [
    ...(metrics.value.ministriesPerformance || []),
    ...(metrics.value.provincesPerformance || [])
  ];
  if (loggedUserAgencyId.value) {
    const found = allPerf.find(p => String(p.agencyId).toLowerCase() === loggedUserAgencyId.value);
    if (found) return found;
  }
  return allPerf[0] || null;
});

function getPct(val, total) {
  if (!total || total <= 0) return 0;
  return Math.round(((val || 0) / total) * 100);
}

async function loadAgencies() {
  try {
    const res = await fetch(getApiUrl('/api/agencies'));
    if (res.ok) {
      const data = await res.json();
      agencies.value = Array.isArray(data) ? data : (data.items || []);
    }
  } catch (e) {}
}

async function loadDashboardMetrics() {
  isLoading.value = true;
  try {
    const params = new URLSearchParams();
    if (dashboardFilter.value && dashboardFilter.value !== 'all') {
      const itemType = dashboardFilter.value === 'goals' ? 'Goal' : 'Task';
      params.append('itemType', itemType);
    }
    if (selectedAgencyIds.value && selectedAgencyIds.value.length > 0) {
      selectedAgencyIds.value.forEach(id => params.append('agencyId', id));
    } else if (!authState.isAdmin.value && authState.user.value?.agencyId) {
      params.append('agencyId', authState.user.value.agencyId);
    }
    if (selectedScopes.value && selectedScopes.value.length === 1) {
      params.append('scope', selectedScopes.value[0]);
    }
    if (selectedSections.value && selectedSections.value.length > 0) {
      selectedSections.value.forEach(sec => params.append('section', sec));
    }
    if (selectedGroups.value && selectedGroups.value.length > 0) {
      selectedGroups.value.forEach(grp => params.append('group', grp));
    }
    if (fromYear.value) params.append('fromYear', fromYear.value);
    if (toYear.value) params.append('toYear', toYear.value);
    if (isOngoingOnly.value) params.append('isOngoing', 'true');

    const queryString = params.toString();
    const url = getApiUrl(`/api/dashboard/metrics${queryString ? '?' + queryString : ''}`);
    
    // Concurrently fetch subordinate units performance if user has agencyId
    let fetchSubPromise = Promise.resolve(null);
    if (!authState.isAdmin.value && authState.user.value?.agencyId) {
      const subParams = new URLSearchParams();
      subParams.append('parentAgencyId', authState.user.value.agencyId);
      if (dashboardFilter.value && dashboardFilter.value !== 'all') {
        subParams.append('itemType', dashboardFilter.value === 'goals' ? 'Goal' : 'Task');
      }
      if (selectedScopes.value && selectedScopes.value.length === 1) {
        subParams.append('scope', selectedScopes.value[0]);
      }
      if (selectedSections.value && selectedSections.value.length > 0) {
        selectedSections.value.forEach(sec => subParams.append('section', sec));
      }
      if (selectedGroups.value && selectedGroups.value.length > 0) {
        selectedGroups.value.forEach(grp => subParams.append('group', grp));
      }
      if (fromYear.value) subParams.append('fromYear', fromYear.value);
      if (toYear.value) subParams.append('toYear', toYear.value);
      if (isOngoingOnly.value) subParams.append('isOngoing', 'true');

      fetchSubPromise = fetch(getApiUrl(`/api/dashboard/metrics?${subParams.toString()}`));
    }

    const [res, subRes] = await Promise.all([
      fetch(url),
      fetchSubPromise
    ]);

    if (res && res.ok) {
      metrics.value = await res.json();
    }

    if (subRes && subRes.ok) {
      const subData = await subRes.json();
      const rawSubList = [...(subData.ministriesPerformance || []), ...(subData.provincesPerformance || [])];
      // Sort by totalItems (goals + tasks) descending so units with most items are at the top
      userSubAgenciesPerformance.value = rawSubList.sort((a, b) => {
        const totalA = a.totalItems ?? ((a.totalGoals || 0) + (a.totalTasks || 0));
        const totalB = b.totalItems ?? ((b.totalGoals || 0) + (b.totalTasks || 0));
        if (totalB !== totalA) {
          return totalB - totalA;
        }
        return (a.name || '').localeCompare(b.name || '', 'vi');
      });
    }
  } catch (e) {
    // Silent catch
  } finally {
    isLoading.value = false;
  }
}

async function loadAgencyItems(agencyId) {
  isAgencyItemsLoading.value = true;
  try {
    const params = new URLSearchParams();
    params.append('agencyId', agencyId);
    if (dashboardFilter.value && dashboardFilter.value !== 'all') {
      const itemType = dashboardFilter.value === 'goals' ? 'Goal' : 'Task';
      params.append('itemType', itemType);
    }
    if (selectedScopes.value && selectedScopes.value.length === 1) {
      params.append('scope', selectedScopes.value[0]);
    }
    if (selectedSections.value && selectedSections.value.length > 0) {
      selectedSections.value.forEach(sec => params.append('section', sec));
    }
    if (selectedGroups.value && selectedGroups.value.length > 0) {
      selectedGroups.value.forEach(grp => params.append('group', grp));
    }
    if (fromYear.value) params.append('fromYear', fromYear.value);
    if (toYear.value) params.append('toYear', toYear.value);
    if (isOngoingOnly.value) params.append('isOngoing', 'true');

    const res = await fetch(getApiUrl(`/api/dashboard/agency-items?${params.toString()}`));
    if (res.ok) {
      agencyItemsList.value = await res.json();
    }
  } catch (e) {
    console.error('Lỗi khi tải danh sách nhiệm vụ của đơn vị:', e);
  } finally {
    isAgencyItemsLoading.value = false;
  }
}

async function loadSubAgencies(parentAgencyId) {
  isSubAgenciesLoading.value = true;
  try {
    const params = new URLSearchParams();
    params.append('parentAgencyId', parentAgencyId);
    if (dashboardFilter.value && dashboardFilter.value !== 'all') {
      const itemType = dashboardFilter.value === 'goals' ? 'Goal' : 'Task';
      params.append('itemType', itemType);
    }
    if (selectedScopes.value && selectedScopes.value.length === 1) {
      params.append('scope', selectedScopes.value[0]);
    }
    if (selectedSections.value && selectedSections.value.length > 0) {
      selectedSections.value.forEach(sec => params.append('section', sec));
    }
    if (selectedGroups.value && selectedGroups.value.length > 0) {
      selectedGroups.value.forEach(grp => params.append('group', grp));
    }
    if (fromYear.value) params.append('fromYear', fromYear.value);
    if (toYear.value) params.append('toYear', toYear.value);
    if (isOngoingOnly.value) params.append('isOngoing', 'true');

    const res = await fetch(getApiUrl(`/api/dashboard/metrics?${params.toString()}`));
    if (res.ok) {
      const data = await res.json();
      subAgenciesList.value = [...(data.ministriesPerformance || []), ...(data.provincesPerformance || [])];
    }
  } catch (e) {
    console.error('Lỗi khi tải đơn vị trực thuộc:', e);
  } finally {
    isSubAgenciesLoading.value = false;
  }
}

async function switchDrilldownTab(tab) {
  drilldownTab.value = tab;
  if (!selectedDrilldownAgency.value) return;

  if (tab === 'tasks') {
    if (agencyItemsList.value.length === 0 && !isAgencyItemsLoading.value) {
      await loadAgencyItems(selectedDrilldownAgency.value.agencyId);
    }
  } else if (tab === 'sub-agencies' || tab === 'contacts') {
    if (subAgenciesList.value.length === 0 && !isSubAgenciesLoading.value) {
      await loadSubAgencies(selectedDrilldownAgency.value.agencyId);
    }
  }
}

async function drilldownAgency(agency) {
  selectedDrilldownAgency.value = agency;
  subAgenciesList.value = [];
  agencyItemsList.value = [];
  modalSearchKeyword.value = '';
  modalItemTypeFilter.value = 'all';
  modalStatusFilter.value = 'all';
  drilldownTab.value = 'tasks'; // Default Tab 1: Assigned Tasks

  // Set loading states immediately so header tab badges display (...) loading indicator instead of (0)
  isAgencyItemsLoading.value = true;
  isSubAgenciesLoading.value = true;

  // Immediately load both assigned tasks list and sub-agencies metrics concurrently 
  // so all tab totals (Tasks, Sub-agencies, Contacts) are accurate right on popup open
  await Promise.all([
    loadAgencyItems(agency.agencyId),
    loadSubAgencies(agency.agencyId)
  ]);
}

const isExportingExcel = ref(false);

function getStatusLabelClean(status) {
  switch (status) {
    case 'InProgressOverdue':
      return 'Đang t/h quá hạn';
    case 'InProgressOnTime':
      return 'Đang t/h trong hạn';
    case 'ExpiringSoon':
      return 'Sắp tới hạn';
    case 'CompletedOverdue':
      return 'Đã h/t quá hạn';
    case 'CompletedOnTime':
      return 'Đã h/t trong hạn';
    case 'NotStarted':
    default:
      return 'Chưa thực hiện';
  }
}

async function exportDashboardExcelReport() {
  isExportingExcel.value = true;
  toast.info("Đang khởi tạo báo cáo Excel theo bộ lọc...", { autoClose: 2000 });

  try {
    const allFilteredAgencies = [
      ...(metrics.value.ministriesPerformance || []),
      ...(metrics.value.provincesPerformance || [])
    ];

    if (allFilteredAgencies.length === 0) {
      toast.warning("Không có dữ liệu Bộ/Ngành/Địa phương phù hợp với bộ lọc hiện tại!");
      isExportingExcel.value = false;
      return;
    }

    const wb = XLSX.utils.book_new();
    const now = new Date();
    const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;

    // ==========================================
    // SHEET 1: TỔNG HỢP CHUNG
    // ==========================================
    const summaryData = [
      ["BÁO CÁO TỔNG HỢP TIẾN ĐỘ THEO DÕI CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg"],
      [`Thời gian xuất: ${timeStr} | Tổng số đơn vị: ${allFilteredAgencies.length}`],
      [],
      ["CHỈ SỐ TỔNG QUAN TOÀN HỆ THỐNG"],
      ["Tổng số mục tiêu", metrics.value.totalGoals || 0],
      ["Tổng số nhiệm vụ", metrics.value.totalTasks || 0],
      ["Tổng số hạng mục", (metrics.value.totalGoals || 0) + (metrics.value.totalTasks || 0)],
      [],
      ["DANH SÁCH BỘ, NGÀNH, ĐỊA PHƯƠNG VÀ TIẾN ĐỘ THỰC HIỆN"],
      [
        "STT",
        "Loại hình",
        "Tên Bộ / Ngành / Địa phương",
        "Tổng số",
        "Mục tiêu",
        "Nhiệm vụ",
        "Đang T/H quá hạn",
        "Đang T/H trong hạn",
        "Sắp tới hạn",
        "Đã H/T quá hạn",
        "Đã H/T trong hạn",
        "Chưa thực hiện",
        "Cán bộ đầu mối chính"
      ]
    ];

    allFilteredAgencies.forEach((ag, idx) => {
      const mainContact = ag.contactPersons?.[0];
      const contactStr = mainContact ? `${mainContact.name || ''} (${mainContact.phone || mainContact.email || ''})` : '—';
      summaryData.push([
        idx + 1,
        ag.type === 'Ministry' ? 'Bộ / Ngành' : 'Địa phương',
        ag.name,
        ag.totalItems || 0,
        ag.totalGoals || 0,
        ag.totalTasks || 0,
        ag.inProgressOverdue || 0,
        ag.inProgressOnTime || 0,
        ag.expiringSoon || 0,
        ag.completedOverdue || 0,
        ag.completedOnTime || 0,
        ag.notStarted || 0,
        contactStr
      ]);
    });

    const wsSummary = XLSX.utils.aoa_to_sheet(summaryData);
    wsSummary['!merges'] = [
      { s: { r: 0, c: 0 }, e: { r: 0, c: 12 } },
      { s: { r: 1, c: 0 }, e: { r: 1, c: 12 } },
      { s: { r: 3, c: 0 }, e: { r: 3, c: 12 } },
      { s: { r: 8, c: 0 }, e: { r: 8, c: 12 } }
    ];
    wsSummary['!cols'] = [
      { wch: 6 },  // STT
      { wch: 15 }, // Loại hình
      { wch: 40 }, // Tên Bộ / Ngành / Địa phương
      { wch: 12 }, // Tổng số
      { wch: 12 }, // Mục tiêu
      { wch: 12 }, // Nhiệm vụ
      { wch: 18 }, // Đang T/H quá hạn
      { wch: 18 }, // Đang T/H trong hạn
      { wch: 16 }, // Sắp tới hạn
      { wch: 16 }, // Đã H/T quá hạn
      { wch: 16 }, // Đã H/T trong hạn
      { wch: 16 }, // Chưa thực hiện
      { wch: 35 }  // Cán bộ đầu mối chính
    ];

    styleWorksheet(wsSummary, { numCols: 13, headerRowIndex: 9, titleRowIndex: 0 });
    XLSX.utils.book_append_sheet(wb, wsSummary, "TỔNG HỢP CHUNG");

    const usedSheetNames = new Set(["TỔNG HỢP CHUNG"]);

    // ==========================================
    // SHEETS FOR EACH AGENCY
    // ==========================================
    for (const ag of allFilteredAgencies) {
      let rawSheetName = (ag.name || ag.code || 'Don_Vi').replace(/[\/\\?*:[\]]/g, '').trim();
      if (rawSheetName.length > 28) rawSheetName = rawSheetName.substring(0, 28);
      let sheetName = rawSheetName;
      let counter = 1;
      while (usedSheetNames.has(sheetName)) {
        sheetName = `${rawSheetName.substring(0, 25)}_${counter++}`;
      }
      usedSheetNames.add(sheetName);

      // 1. Fetch sub-agencies metrics
      let subAgencies = [];
      try {
        const subParams = new URLSearchParams();
        subParams.append('parentAgencyId', ag.agencyId);
        if (dashboardFilter.value && dashboardFilter.value !== 'all') {
          subParams.append('itemType', dashboardFilter.value === 'goals' ? 'Goal' : 'Task');
        }
        if (selectedScopes.value && selectedScopes.value.length === 1) subParams.append('scope', selectedScopes.value[0]);
        if (selectedSections.value && selectedSections.value.length > 0) selectedSections.value.forEach(s => subParams.append('section', s));
        if (selectedGroups.value && selectedGroups.value.length > 0) selectedGroups.value.forEach(g => subParams.append('group', g));
        if (fromYear.value) subParams.append('fromYear', fromYear.value);
        if (toYear.value) subParams.append('toYear', toYear.value);
        if (isOngoingOnly.value) subParams.append('isOngoing', 'true');

        const subRes = await fetch(getApiUrl(`/api/dashboard/metrics?${subParams.toString()}`));
        if (subRes.ok) {
          const subData = await subRes.json();
          subAgencies = [...(subData.ministriesPerformance || []), ...(subData.provincesPerformance || [])];
        }
      } catch (e) {}

      // 2. Fetch agency items list from backend API
      let agencyItems = [];
      try {
        const agParams = new URLSearchParams();
        agParams.append('agencyId', ag.agencyId);
        if (dashboardFilter.value && dashboardFilter.value !== 'all') agParams.append('itemType', dashboardFilter.value === 'goals' ? 'Goal' : 'Task');
        if (selectedScopes.value && selectedScopes.value.length === 1) agParams.append('scope', selectedScopes.value[0]);
        if (selectedSections.value && selectedSections.value.length > 0) selectedSections.value.forEach(s => agParams.append('section', s));
        if (selectedGroups.value && selectedGroups.value.length > 0) selectedGroups.value.forEach(g => agParams.append('group', g));
        if (fromYear.value) agParams.append('fromYear', fromYear.value);
        if (toYear.value) agParams.append('toYear', toYear.value);
        if (isOngoingOnly.value) agParams.append('isOngoing', 'true');

        const agItemsRes = await fetch(getApiUrl(`/api/dashboard/agency-items?${agParams.toString()}`));
        if (agItemsRes.ok) {
          agencyItems = await agItemsRes.json();
        }
      } catch (e) {}

      const goalsList = agencyItems.filter(i => i.itemType === 'Goal');
      const tasksList = agencyItems.filter(i => i.itemType === 'Task');

      // Contact persons list
      const contactList = [];
      if (ag.contactPersons?.length) {
        ag.contactPersons.forEach(c => contactList.push({ ...c, unitName: ag.name }));
      }
      if (subAgencies?.length) {
        subAgencies.forEach(sub => {
          if (sub.contactPersons?.length) {
            sub.contactPersons.forEach(c => contactList.push({ ...c, unitName: sub.name }));
          }
        });
      }

      // Main Contact Person
      const mainContact = ag.contactPersons?.[0];
      const mainContactStr = mainContact ? `${mainContact.name || ''} - ${mainContact.position || ''} (SĐT: ${mainContact.phone || '—'}, Email: ${mainContact.email || '—'})` : 'Chưa có thông tin';

      const sheetRows = [
        [`BÁO CÁO CHI TIẾT THỰC HIỆN NHIỆM VỤ CHIẾN LƯỢC - ${ag.name.toUpperCase()}`],
        [`Thời gian xuất: ${timeStr} | Loại hình: ${ag.type === 'Ministry' ? 'Bộ / Ngành' : 'Địa phương'}`],
        [],
        ["1. THÔNG TIN CHUNG VÀ TỔNG HỢP TIẾN ĐỘ THỰC HIỆN CỦA ĐƠN VỊ"],
        ["Tên đơn vị:", ag.name],
        ["Cán bộ đầu mối chính:", mainContactStr],
        ["Đơn vị trực thuộc:", subAgencies.length > 0 ? `${subAgencies.length} đơn vị trực thuộc` : "Không có đơn vị trực thuộc"],
        [],
        ["BẢNG TỔNG HỢP TRẠNG THÁI TIẾN ĐỘ CỦA ĐƠN VỊ"],
        [
          "Tổng số hạng mục", "Mục tiêu", "Nhiệm vụ",
          "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
          "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
        ],
        [
          ag.totalItems || 0, ag.totalGoals || 0, ag.totalTasks || 0,
          ag.inProgressOverdue || 0, ag.inProgressOnTime || 0, ag.expiringSoon || 0,
          ag.completedOverdue || 0, ag.completedOnTime || 0, ag.notStarted || 0
        ],
        []
      ];

      const merges = [
        { s: { r: 0, c: 0 }, e: { r: 0, c: 7 } },
        { s: { r: 1, c: 0 }, e: { r: 1, c: 7 } },
        { s: { r: 3, c: 0 }, e: { r: 3, c: 7 } },
        { s: { r: 8, c: 0 }, e: { r: 8, c: 7 } }
      ];

      // Table 2: Sub-agencies progress summary
      if (subAgencies.length > 0) {
        const rowIdx = sheetRows.length;
        merges.push({ s: { r: rowIdx, c: 0 }, e: { r: rowIdx, c: 7 } });
        sheetRows.push(["2. TỔNG SỐ MỤC TIÊU, NHIỆM VỤ THEO TRẠNG THÁI CỦA TỪNG ĐƠN VỊ TRỰC THUỘC"]);
        sheetRows.push([
          "STT", "Tên Đơn Vị Trực Thuộc", "Tổng Số", "Mục Tiêu", "Nhiệm Vụ",
          "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn"
        ]);
        subAgencies.forEach((sub, sIdx) => {
          sheetRows.push([
            sIdx + 1, sub.name, sub.totalItems || 0, sub.totalGoals || 0, sub.totalTasks || 0,
            sub.inProgressOverdue || 0, sub.inProgressOnTime || 0, sub.expiringSoon || 0
          ]);
        });
        sheetRows.push([]);
      }

      // Table 3: Goals List
      const secGoal = subAgencies.length > 0 ? "3" : "2";
      const goalRowIdx = sheetRows.length;
      merges.push({ s: { r: goalRowIdx, c: 0 }, e: { r: goalRowIdx, c: 7 } });
      sheetRows.push([`${secGoal}. DANH SÁCH MỤC TIÊU CỦA ĐƠN VỊ (${goalsList.length} mục tiêu)`]);
      if (goalsList.length > 0) {
        sheetRows.push([
          "STT", "Tên Mục Tiêu", "Cơ Quan Chủ Trì", "Phạm Vi", "Lĩnh Vực / Nhóm",
          "Thời Gian / Hạn Chót", "Tiến Độ Hiện Tại", "Trạng Thái Thực Hiện"
        ]);
        goalsList.forEach((g, gIdx) => {
          let dateStr = g.isOngoing ? 'Hằng năm' : (g.dueDate ? formatDate(g.dueDate) : '—');
          let progStr = formatItemProgressDisplay(g);

          sheetRows.push([
            gIdx + 1, g.title, g.leadAgencyName,
            g.isGeneralTask ? 'Phạm vi chung' : 'Phạm vi riêng',
            [g.section, g.group].filter(Boolean).join(' - ') || '—',
            dateStr, progStr, getStatusLabelClean(g.status)
          ]);
        });
      } else {
        sheetRows.push(["Không có mục tiêu nào trong bộ lọc hiện tại."]);
      }
      sheetRows.push([]);

      // Table 4: Tasks List
      const secTask = subAgencies.length > 0 ? "4" : "3";
      const taskRowIdx = sheetRows.length;
      merges.push({ s: { r: taskRowIdx, c: 0 }, e: { r: taskRowIdx, c: 7 } });
      sheetRows.push([`${secTask}. DANH SÁCH NHIỆM VỤ CỦA ĐƠN VỊ (${tasksList.length} nhiệm vụ)`]);
      if (tasksList.length > 0) {
        sheetRows.push([
          "STT", "Tên Nhiệm Vụ", "Cơ Quan Chủ Trì", "Phạm Vi", "Lĩnh Vực / Nhóm",
          "Thời Gian / Hạn Chót", "Tiến Độ Hiện Tại", "Trạng Thái Thực Hiện"
        ]);
        tasksList.forEach((t, tIdx) => {
          let dateStr = t.isOngoing ? 'Hằng năm' : (t.dueDate ? formatDate(t.dueDate) : '—');
          let progStr = formatItemProgressDisplay(t);

          sheetRows.push([
            tIdx + 1, t.title, t.leadAgencyName,
            t.isGeneralTask ? 'Phạm vi chung' : 'Phạm vi riêng',
            [t.section, t.group].filter(Boolean).join(' - ') || '—',
            dateStr, progStr, getStatusLabelClean(t.status)
          ]);
        });
      } else {
        sheetRows.push(["Không có nhiệm vụ nào trong bộ lọc hiện tại."]);
      }
      sheetRows.push([]);

      // Table 5: Contact Persons List
      const secContact = subAgencies.length > 0 ? "5" : "4";
      const contactRowIdx = sheetRows.length;
      merges.push({ s: { r: contactRowIdx, c: 0 }, e: { r: contactRowIdx, c: 7 } });
      sheetRows.push([`${secContact}. DANH SÁCH CÁN BỘ ĐẦU MỐI LIÊN HỆ (${contactList.length} cán bộ)`]);
      if (contactList.length > 0) {
        sheetRows.push(["STT", "Họ và Tên", "Chức Danh", "Phòng Ban", "Điện Thoại", "Email", "Thuộc Đơn Vị"]);
        contactList.forEach((c, cIdx) => {
          sheetRows.push([
            cIdx + 1, c.name || '—', c.position || '—', c.department || '—',
            c.phone || '—', c.email || '—', c.unitName || ag.name
          ]);
        });
      } else {
        sheetRows.push(["Chưa có thông tin cán bộ đầu mối liên hệ."]);
      }

      const ws = XLSX.utils.aoa_to_sheet(sheetRows);
      ws['!merges'] = merges;
      ws['!cols'] = [
        { wch: 6 },  // STT
        { wch: 55 }, // Tên Hạng Mục / Tên Đơn Vị / Tên Cán Bộ
        { wch: 28 }, // Cơ Quan Chủ Trì / Chức Danh
        { wch: 18 }, // Phạm Vi / Phòng Ban
        { wch: 25 }, // Lĩnh Vực - Nhóm / Điện Thoại
        { wch: 20 }, // Thời Gian / Email
        { wch: 18 }, // Tiến Độ / Thuộc Đơn Vị
        { wch: 25 }  // Trạng Thái
      ];

      styleWorksheet(ws, { numCols: 8, headerRowIndex: 9, titleRowIndex: 0 });
      XLSX.utils.book_append_sheet(wb, ws, sheetName);
    }

    const dateFileStr = now.toISOString().slice(0, 10);
    XLSX.writeFile(wb, `Bao_Cao_Theo_Doi_Chien_Luoc_${dateFileStr}.xlsx`);
    toast.success(`Đã xuất thành công file Báo cáo Excel gồm ${allFilteredAgencies.length + 1} Sheet!`);
  } catch (err) {
    console.error("Lỗi khi xuất báo cáo Excel:", err);
    toast.error("Lỗi khi xuất file Excel: " + (err.message || err));
  } finally {
    isExportingExcel.value = false;
  }
}

onMounted(() => {
  loadAgencies();
  loadDashboardMetrics();
});
</script>
