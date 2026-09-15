<template>
  <div class="w-full space-y-6 font-sans">
    
    <!-- Top Header Bar -->
    <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 space-y-4 w-full">
      <div class="flex items-center justify-between">
        <button 
          @click="$emit('back')" 
          class="text-xs font-extrabold text-slate-600 hover:text-blue-600 bg-slate-100 hover:bg-blue-50 px-3.5 py-2 rounded-xl transition flex items-center gap-1.5"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/></svg>
          Quay lại Danh Sách Văn Bản
        </button>

        <div class="flex items-center gap-3">
          <button 
            @click="openEditDocModal"
            class="text-xs font-bold text-slate-700 hover:text-blue-600 bg-slate-100 hover:bg-blue-50 border border-slate-200 px-3.5 py-2 rounded-xl transition flex items-center gap-1.5"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>
            Chỉnh Sửa Văn Bản
          </button>
        </div>
      </div>

      <!-- Document Metadata Header -->
      <div v-if="documentDetails" class="space-y-2 border-t border-slate-100 pt-4">
        <div class="flex items-center gap-2">
          <span class="px-3 py-1 bg-blue-600 text-white font-black text-xs rounded-lg shadow-sm">
            {{ documentDetails.documentNumber }}
          </span>
          <span class="text-xs font-bold text-slate-500 bg-slate-100 px-2.5 py-1 rounded-lg">
            Khung Thời Gian: {{ documentDetails.startYear || 2026 }} - {{ documentDetails.endYear || 2030 }}
          </span>
          <span v-if="documentDetails.signer" class="text-xs font-bold text-slate-600 bg-purple-50 text-purple-700 border border-purple-100 px-2.5 py-1 rounded-lg">
            ✍ Người ký: {{ documentDetails.signer }}
          </span>
        </div>

        <h2 class="text-xl font-extrabold text-slate-800 leading-snug">
          {{ documentDetails.name }}
        </h2>

        <p v-if="documentDetails.summary" class="text-xs text-slate-600 font-medium italic bg-slate-50 p-3 rounded-xl border border-slate-200/60 mt-1">
          📌 Trích yếu: {{ documentDetails.summary }}
        </p>

        <!-- Attached Files Section Right Below Content -->
        <div v-if="documentDetails?.attachmentPath" class="p-3 bg-blue-50/80 border border-blue-200/80 rounded-xl space-y-2 mt-2">
          <div class="flex items-center gap-2 text-xs font-extrabold text-blue-900">
            <svg class="w-4 h-4 text-blue-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"/></svg>
            <span>📄 File Minh Chứng Đính Kèm:</span>
          </div>
          <div class="flex flex-wrap gap-2">
            <a 
              v-for="(path, idx) in splitAttachmentPaths(documentDetails.attachmentPath)" 
              :key="idx"
              :href="getFileUrl(path)" 
              target="_blank"
              class="inline-flex items-center gap-2 bg-white hover:bg-blue-600 text-slate-800 hover:text-white font-extrabold text-xs px-3.5 py-2 rounded-xl border border-blue-200/80 shadow-sm transition group cursor-pointer"
              :title="`Click để tải về hoặc xem trực tiếp file: ${formatFileName(path)}`"
            >
              <svg class="w-4 h-4 text-rose-500 group-hover:text-white transition-colors flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 002 2v14a2 2 0 002 2z"/></svg>
              <span class="truncate max-w-sm">{{ formatFileName(path) }}</span>
              <svg class="w-3.5 h-3.5 opacity-60 group-hover:opacity-100 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/></svg>
            </a>
          </div>
        </div>
      </div>
    </div>

    <!-- UNIFIED MAIN CONTENT CARD BLOCK (Primary Tabs, Sub-Tabs, Data Grid/Table) -->
    <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200/80 space-y-5 w-full max-w-full min-w-0 overflow-hidden">

      <!-- PRIMARY TABS: MỤC TIÊU vs NHIỆM VỤ -->
      <div class="flex items-center justify-between bg-slate-50 p-2 rounded-xl border border-slate-200/80 w-full">
        <div class="flex items-center gap-2">
          <button 
            @click="activePrimaryTab = 'goals'" 
            :class="['px-5 py-2.5 rounded-xl text-xs font-extrabold transition flex items-center gap-2', activePrimaryTab === 'goals' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-600 hover:bg-slate-100']"
          >
            🎯 1. Danh Sách Mục Tiêu
            <span :class="['ml-1 px-2 py-0.5 text-[10px] rounded-full font-extrabold', activePrimaryTab === 'goals' ? 'bg-purple-800 text-white' : 'bg-purple-100 text-purple-800']">
              {{ goalsList.length }}
            </span>
          </button>

          <button 
            @click="activePrimaryTab = 'tasks'" 
            :class="['px-5 py-2.5 rounded-xl text-xs font-extrabold transition flex items-center gap-2', activePrimaryTab === 'tasks' ? 'bg-blue-600 text-white shadow-md' : 'text-slate-600 hover:bg-slate-100']"
          >
            📋 2. Danh Sách Nhiệm Vụ
            <span :class="['ml-1 px-2 py-0.5 text-[10px] rounded-full font-extrabold', activePrimaryTab === 'tasks' ? 'bg-blue-800 text-white' : 'bg-blue-100 text-blue-800']">
              {{ tasksList.length }}
            </span>
          </button>
        </div>

        <!-- Action Button to Add Goal/Task -->
        <button 
          @click="openCreateModal(activePrimaryTab === 'goals' ? 'Goal' : 'Task')"
          :class="['px-4 py-2 text-white font-bold text-xs rounded-xl transition shadow-sm flex items-center gap-1.5', activePrimaryTab === 'goals' ? 'bg-purple-600 hover:bg-purple-700' : 'bg-blue-600 hover:bg-blue-700']"
        >
          + Thêm {{ activePrimaryTab === 'goals' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} Mới
        </button>
      </div>

      <!-- INNER SUB-TABS: DANH SÁCH vs THIẾT LẬP CHỈ TIÊU -->
      <div class="flex items-center gap-2 bg-slate-100 p-1.5 rounded-xl border border-slate-200/80 w-fit">
        <button 
          @click="activeSubTab = 'list'" 
          :class="['px-4 py-2 rounded-lg text-xs font-extrabold transition flex items-center gap-1.5', activeSubTab === 'list' ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-600 hover:text-slate-900']"
        >
          📌 Danh Sách Chi Tiết
        </button>

        <button 
          @click="activeSubTab = 'grid'" 
          :class="['px-4 py-2 rounded-lg text-xs font-extrabold transition flex items-center gap-1.5', activeSubTab === 'grid' ? 'bg-white text-emerald-700 shadow-sm' : 'text-slate-600 hover:text-slate-900']"
        >
          ⚙ Thiết Lập Chỉ Tiêu Kế Hoạch
        </button>
      </div>

      <!-- DYNAMIC PLANNING GRID SUB-TAB -->
      <div v-show="activeSubTab === 'grid'" class="w-full max-w-full min-w-0 overflow-x-auto">
        <DynamicPlanningGrid ref="planningGridRef" :documentId="documentId" :filterItemType="activePrimaryTab === 'goals' ? 'Goal' : 'Task'" />
      </div>

      <!-- LIST TABLE SUB-TAB (WITH ADVANCED FILTER BAR & PROGRESS COMPARISON) -->
      <div v-show="activeSubTab === 'list'" class="w-full max-w-full min-w-0 border border-slate-200/80 rounded-2xl bg-white overflow-hidden shadow-sm flex flex-col">
        
        <!-- ADVANCED FILTER BAR WITH SEARCH BUTTON & LOCALSTORAGE PERSISTENCE -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 bg-slate-50/60 p-4 border-b border-slate-200/80 items-end w-full">
          <!-- Search Query -->
          <div>
            <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Tìm Kiếm Từ Khóa</label>
            <div class="relative">
              <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
              <input 
                v-model="filterDraft.searchQuery" 
                @keyup.enter="execFilterSearch"
                placeholder="Mã, tên, phân nhóm..." 
                class="w-full text-xs font-semibold pl-9 pr-3 py-2 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
              />
            </div>
          </div>

          <!-- Agency Filter -->
          <div>
            <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Cơ Quan Chủ Trì</label>
            <select 
              v-model="filterDraft.selectedAgencyId" 
              class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
            >
              <option value="all">Tất cả cơ quan / Bộ ngành</option>
              <option v-for="ag in agencies" :key="ag.id" :value="ag.id">
                {{ ag.code }} - {{ ag.name }}
              </option>
            </select>
          </div>

          <!-- Progress/Alert Status Filter -->
          <div>
            <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Trạng Thái Tiến Độ</label>
            <select 
              v-model="filterDraft.selectedStatusFilter" 
              class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
            >
              <option value="all">Tất cả trạng thái</option>
              <option value="Completed">Hoàn thành (≥100%)</option>
              <option value="OnTrack">Đạt kế hoạch (≥80%)</option>
              <option value="Lagging">Chậm tiến độ (<80%)</option>
              <option value="NoReport">Chưa có Báo cáo</option>
            </select>
          </div>

          <!-- Target Year Filter -->
          <div>
            <label class="text-[10px] font-extrabold text-slate-500 uppercase tracking-wider block mb-1">Năm Chỉ Tiêu</label>
            <select 
              v-model="filterDraft.selectedYearFilter" 
              class="w-full text-xs font-bold bg-white border border-slate-200 rounded-xl px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:outline-none shadow-2xs"
            >
              <option value="all">Tất cả các năm (2026-2030)</option>
              <option :value="2026">Năm 2026</option>
              <option :value="2027">Năm 2027</option>
              <option :value="2028">Năm 2028</option>
              <option :value="2029">Năm 2029</option>
              <option :value="2030">Năm 2030</option>
            </select>
          </div>

          <!-- Filter Action Buttons -->
          <div class="flex items-center gap-2">
            <button 
              type="button" 
              @click="execFilterSearch" 
              class="w-full py-2 px-3 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs rounded-xl shadow-sm transition flex items-center justify-center gap-1.5"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
              <span>Tìm Kiếm</span>
            </button>

            <button 
              type="button" 
              @click="resetFilterSearch" 
              class="px-3 py-2 bg-slate-200 hover:bg-slate-300 text-slate-700 font-bold text-xs rounded-xl transition shrink-0"
              title="Đặt lại bộ lọc"
            >
              ↺
            </button>
          </div>
        </div>

        <!-- GOALS TABLE (LEVEL 1A) -->
        <div v-if="activePrimaryTab === 'goals'" class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-360px)] custom-scrollbar w-full">
          <table class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100/90 text-xs text-slate-500 uppercase font-bold border-b border-slate-200">
            <tr>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[85px] w-[85px] max-w-[85px] whitespace-nowrap sticky left-0 z-30 bg-slate-100 shadow-[1px_0_0_0_#e2e8f0]">Mã</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[260px] w-[260px] max-w-[260px] sticky left-[85px] z-30 bg-slate-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">Mục Tiêu</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[140px] max-w-[180px]">Phân Nhóm</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[120px] whitespace-nowrap">Cơ Quan Chủ Trì</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[95px] whitespace-nowrap text-center">Loại Đánh Giá</th>
              <th class="px-3 py-3 border-r border-slate-200 text-center min-w-[75px] whitespace-nowrap">Đơn Vị</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[120px] whitespace-nowrap bg-purple-50/60 text-purple-900 font-extrabold text-center">Tiến Độ Hiện Tại</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[220px] whitespace-nowrap bg-rose-50/60 text-rose-950 font-extrabold">So Sánh Tiến Độ Với Kế Hoạch</th>
              <th class="px-3 py-3 text-center min-w-[90px] w-[90px] whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="(goal, goalIdx) in paginatedPrimaryList" :key="goal.id" class="hover:bg-slate-50 transition group">
              <!-- STICKY FROZEN CELL 1: MÃ -->
              <td class="px-3 py-3 border-r border-slate-200 font-extrabold text-purple-900 whitespace-nowrap min-w-[85px] w-[85px] max-w-[85px] sticky left-0 z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[1px_0_0_0_#e2e8f0]">
                {{ goal.code }}
              </td>
              
              <!-- STICKY FROZEN CELL 2: MỤC TIÊU -->
              <td class="px-3 py-3 border-r border-slate-200 sticky left-[85px] z-20 bg-white group-hover:bg-[#FAF5FF] shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)] min-w-[260px] w-[260px] max-w-[260px]">
                <VTooltip 
                  v-if="goal.title && goal.title.length > 40"
                  theme="custom-dark"
                  placement="top"
                  :delay="{ show: 1000, hide: 0 }"
                >
                  <div class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed cursor-help">
                    {{ goal.title }}
                  </div>
                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[400px]">
                      <span class="font-extrabold text-purple-300 block mb-1 text-[11px] uppercase tracking-wider">🎯 Chi Tiết Mục Tiêu</span>
                      {{ goal.title }}
                    </div>
                  </template>
                </VTooltip>
                <div v-else class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed">
                  {{ goal.title }}
                </div>
              </td>

              <!-- Category with max 4 lines clamp + Floating Dark Tooltip if length > 40 -->
              <td class="px-3 py-3 border-r border-slate-200 max-w-[180px]">
                <VTooltip 
                  v-if="goal.category && goal.category.length > 40"
                  theme="custom-dark"
                  placement="top"
                  :delay="{ show: 1000, hide: 0 }"
                >
                  <div class="line-clamp-4 text-xs text-slate-600 font-medium leading-relaxed cursor-help">
                    {{ goal.category }}
                  </div>
                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[240px] max-w-[360px]">
                      <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">📁 Phân Nhóm Chi Tiết</span>
                      {{ goal.category }}
                    </div>
                  </template>
                </VTooltip>
                <div v-else class="line-clamp-4 text-xs text-slate-600 font-medium leading-relaxed">
                  {{ goal.category || 'Chung' }}
                </div>
              </td>

              <td class="px-3 py-3 border-r border-slate-200 font-bold text-slate-700 whitespace-nowrap">
                {{ goal.leadAgency?.name || goal.leadAgency?.code || 'Chưa phân công' }}
              </td>

              <td class="px-3 py-3 border-r border-slate-200 text-xs whitespace-nowrap text-center">
                <span :class="['px-2 py-0.5 rounded font-semibold', isQuantitative(goal) ? 'bg-emerald-50 text-emerald-700' : 'bg-indigo-50 text-indigo-700']">
                  {{ isQuantitative(goal) ? 'Định lượng' : 'Định tính' }}
                </span>
              </td>

              <td class="px-3 py-3 border-r border-slate-200 text-center text-xs font-bold text-slate-600 whitespace-nowrap">
                {{ goal.unit?.name || '%' }}
              </td>

              <!-- LATEST PROGRESS COLUMN -->
              <td class="px-3 py-3 border-r border-slate-200 whitespace-nowrap text-center">
                <div v-if="goal.latestProgressValue !== null && goal.latestProgressValue !== undefined" class="font-black text-slate-800 text-xs">
                  {{ goal.latestProgressValue }} <span class="text-[10px] text-slate-500 font-normal">{{ goal.unit?.name || '%' }}</span>
                </div>
                <div v-else-if="goal.latestProgressStatus" class="font-bold text-slate-800 text-xs">
                  <span class="px-2 py-0.5 bg-slate-100 text-slate-700 rounded text-[11px] font-bold border border-slate-200">
                    {{ formatStatusText(goal.latestProgressStatus) }}
                  </span>
                </div>
                <div v-else class="text-xs text-slate-400 italic">
                  Chưa có Báo cáo
                </div>
                <div v-if="goal.lastUpdated" class="text-[10px] text-slate-400 mt-0.5 font-medium">
                  🕒 {{ formatDate(goal.lastUpdated) }}
                </div>
              </td>

              <!-- SO SÁNH TIẾN ĐỘ VỚI KẾ HOẠCH + ĐÔN ĐỐC BUTTON -->
              <td class="px-3 py-3 border-r border-slate-200 whitespace-nowrap">
                <div class="flex items-center justify-between gap-2">
                  <span :class="['px-2.5 py-1 rounded-lg text-xs font-extrabold shadow-2xs', getItemProgressComparison(goal).badgeClass]">
                    {{ getItemProgressComparison(goal).label }}
                  </span>

                  <!-- Đôn Đốc Icon Button with Floating Tooltip -->
                  <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                    <button 
                      @click="openUrgeModal(goal)"
                      class="w-7 h-7 rounded-lg bg-rose-50 hover:bg-rose-100 border border-rose-200 text-rose-700 flex items-center justify-center transition shadow-2xs shrink-0"
                    >
                      <svg class="w-3.5 h-3.5 text-rose-600 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
                      </svg>
                    </button>
                    <template #popper>
                      <div class="text-[11px] font-bold whitespace-nowrap">
                        ⚡ Gửi chỉ đạo đôn đốc tiến độ
                      </div>
                    </template>
                  </VTooltip>
                </div>
              </td>

              <!-- ACTION BUTTONS -->
              <td class="px-3 py-3 text-center whitespace-nowrap min-w-[90px] w-[90px]">
                <div class="flex items-center justify-center gap-1.5">
                  <!-- Cập Nhật Tiến Độ -->
                  <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                    <button 
                      @click="openProgressModal(goal)" 
                      class="w-7 h-7 rounded-lg bg-purple-50 hover:bg-purple-100 border border-purple-200 text-purple-700 flex items-center justify-center transition shadow-2xs"
                    >
                      <svg class="w-3.5 h-3.5 text-purple-600 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                      </svg>
                    </button>
                    <template #popper>
                      <div class="text-[11px] font-bold whitespace-nowrap">
                        📊 Cập nhật tiến độ thực hiện
                      </div>
                    </template>
                  </VTooltip>

                  <!-- Xóa -->
                  <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                    <button 
                      @click="deleteItem(goal)"
                      class="w-7 h-7 rounded-lg bg-rose-50 hover:bg-rose-100 border border-rose-200 text-rose-700 flex items-center justify-center transition shadow-2xs"
                    >
                      <svg class="w-3.5 h-3.5 text-rose-600 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                      </svg>
                    </button>
                    <template #popper>
                      <div class="text-[11px] font-bold whitespace-nowrap">
                        🗑️ Xóa mục tiêu
                      </div>
                    </template>
                  </VTooltip>
                </div>
              </td>
            </tr>

            <tr v-if="filteredPrimaryList.length === 0">
              <td colspan="9" class="p-8 text-center text-slate-400 font-semibold italic space-y-2">
                <div>Chưa có Mục tiêu nào phù hợp bộ lọc.</div>
                <button @click="resetFilterSearch" class="mt-2 px-3.5 py-1.5 bg-blue-50 text-blue-700 hover:bg-blue-100 border border-blue-200 rounded-xl text-xs font-bold transition not-italic">
                  🔄 Xóa bộ lọc tìm kiếm
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- TASKS TABLE -->
      <div v-else class="overflow-x-auto overflow-y-auto max-h-[calc(100vh-360px)] custom-scrollbar w-full">
        <table class="w-full text-left text-sm text-slate-700 border-collapse">
          <thead class="bg-slate-100/90 text-xs text-slate-500 uppercase font-bold border-b border-slate-200">
            <tr>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[85px] w-[85px] max-w-[85px] whitespace-nowrap sticky left-0 z-30 bg-slate-100 shadow-[1px_0_0_0_#e2e8f0]">Mã</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[260px] w-[260px] max-w-[260px] sticky left-[85px] z-30 bg-slate-100 shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)]">Nhiệm Vụ</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[120px] whitespace-nowrap">Cơ Quan Chủ Trì</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[120px] bg-purple-50/50 text-purple-900 font-extrabold">Cơ Quan Phối Hợp</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[95px] whitespace-nowrap text-center">Đánh Giá</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[120px] whitespace-nowrap bg-blue-50/60 text-blue-900 font-extrabold text-center">Tiến Độ Hiện Tại</th>
              <th class="px-3 py-3 border-r border-slate-200 min-w-[220px] whitespace-nowrap bg-rose-50/60 text-rose-950 font-extrabold">So Sánh Tiến Độ Với Kế Hoạch</th>
              <th class="px-3 py-3 text-center min-w-[90px] w-[90px] whitespace-nowrap">Thao Tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200">
            <tr v-for="(task, taskIdx) in paginatedPrimaryList" :key="task.id" class="hover:bg-slate-50 transition group">
              <!-- STICKY FROZEN CELL 1: MÃ -->
              <td class="px-3 py-3 border-r border-slate-200 font-extrabold text-blue-900 whitespace-nowrap min-w-[85px] w-[85px] max-w-[85px] sticky left-0 z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[1px_0_0_0_#e2e8f0]">
                {{ task.code }}
              </td>

              <!-- STICKY FROZEN CELL 2: NHIỆM VỤ -->
              <td class="px-3 py-3 border-r border-slate-200 sticky left-[85px] z-20 bg-white group-hover:bg-[#EFF6FF] shadow-[3px_0_6px_-1px_rgba(0,0,0,0.15)] min-w-[260px] w-[260px] max-w-[260px]">
                <VTooltip 
                  v-if="task.title && task.title.length > 40"
                  theme="custom-dark"
                  placement="top"
                  :delay="{ show: 1000, hide: 0 }"
                >
                  <div class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed cursor-help">
                    {{ task.title }}
                  </div>
                  <div class="text-[11px] text-slate-400 mt-1 line-clamp-2">Phân nhóm: {{ task.category || 'N/A' }}</div>

                  <template #popper>
                    <div class="whitespace-normal break-words text-left leading-relaxed min-w-[280px] max-w-[400px]">
                      <span class="font-extrabold text-blue-300 block mb-1 text-[11px] uppercase tracking-wider">📋 Chi Tiết Nhiệm Vụ</span>
                      {{ task.title }}
                      <div v-if="task.category" class="mt-1.5 pt-1.5 border-t border-slate-700/60 text-[11px] text-slate-300">
                        <strong class="text-blue-300">Phân nhóm:</strong> {{ task.category }}
                      </div>
                    </div>
                  </template>
                </VTooltip>
                <div v-else>
                  <div class="line-clamp-4 font-semibold text-slate-800 text-xs leading-relaxed">
                    {{ task.title }}
                  </div>
                  <div class="text-[11px] text-slate-400 mt-1 line-clamp-2">Phân nhóm: {{ task.category || 'N/A' }}</div>
                </div>
              </td>

              <td class="px-3 py-3 border-r border-slate-200 font-bold text-slate-700 whitespace-nowrap">
                {{ task.leadAgency?.code || task.leadAgency?.name || 'Chưa phân công' }}
              </td>
              
              <!-- Coordinating Agencies Tags -->
              <td class="px-3 py-3 border-r border-slate-200">
                <div class="flex flex-wrap gap-1">
                  <span v-for="coordId in task.coordinatingAgencyIds" :key="coordId" class="px-2 py-0.5 bg-purple-100 text-purple-800 rounded font-extrabold text-[10px]">
                    {{ getAgencyCode(coordId) }}
                  </span>
                  <span v-if="!task.coordinatingAgencyIds || task.coordinatingAgencyIds.length === 0" class="text-slate-400 italic text-xs">—</span>
                </div>
              </td>

              <td class="px-3 py-3 border-r border-slate-200 text-xs whitespace-nowrap text-center">
                <span :class="['px-2 py-0.5 rounded font-semibold', isQuantitative(task) ? 'bg-emerald-50 text-emerald-700' : 'bg-indigo-50 text-indigo-700']">
                  {{ isQuantitative(task) ? 'Định lượng' : 'Định tính' }}
                </span>
              </td>

              <!-- LATEST PROGRESS COLUMN -->
              <td class="px-3 py-3 border-r border-slate-200 whitespace-nowrap text-center">
                <div v-if="task.latestProgressValue !== null && task.latestProgressValue !== undefined" class="font-black text-slate-800 text-xs">
                  {{ task.latestProgressValue }} <span class="text-[10px] text-slate-500 font-normal">{{ task.unit?.name || '%' }}</span>
                </div>
                <div v-else-if="task.latestProgressStatus" class="font-bold text-slate-800 text-xs">
                  <span class="px-2 py-0.5 bg-slate-100 text-slate-700 rounded text-[11px] font-bold border border-slate-200">
                    {{ formatStatusText(task.latestProgressStatus) }}
                  </span>
                </div>
                <div v-else class="text-xs text-slate-400 italic">
                  Chưa có Báo cáo
                </div>
                <div v-if="task.lastUpdated" class="text-[10px] text-slate-400 mt-0.5 font-medium">
                  🕒 {{ formatDate(task.lastUpdated) }}
                </div>
              </td>

              <!-- SO SÁNH TIẾN ĐỘ VỚI KẾ HOẠCH + ĐÔN ĐỐC BUTTON -->
              <td class="px-3 py-3 border-r border-slate-200 whitespace-nowrap">
                <div class="flex items-center justify-between gap-2">
                  <span :class="['px-2.5 py-1 rounded-lg text-xs font-extrabold shadow-2xs', getItemProgressComparison(task).badgeClass]">
                    {{ getItemProgressComparison(task).label }}
                  </span>

                  <!-- Đôn Đốc Icon Button with Floating Tooltip -->
                  <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                    <button 
                      @click="openUrgeModal(task)"
                      class="w-7 h-7 rounded-lg bg-rose-50 hover:bg-rose-100 border border-rose-200 text-rose-700 flex items-center justify-center transition shadow-2xs shrink-0"
                    >
                      <svg class="w-3.5 h-3.5 text-rose-600 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
                      </svg>
                    </button>
                    <template #popper>
                      <div class="text-[11px] font-bold whitespace-nowrap">
                        ⚡ Gửi chỉ đạo đôn đốc tiến độ
                      </div>
                    </template>
                  </VTooltip>
                </div>
              </td>

              <!-- ACTION BUTTONS -->
              <td class="px-3 py-3 text-center whitespace-nowrap min-w-[90px] w-[90px]">
                <div class="flex items-center justify-center gap-1.5">
                  <!-- Cập Nhật Tiến Độ -->
                  <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                    <button 
                      @click="openProgressModal(task)" 
                      class="w-7 h-7 rounded-lg bg-blue-50 hover:bg-blue-100 border border-blue-200 text-blue-700 flex items-center justify-center transition shadow-2xs"
                    >
                      <svg class="w-3.5 h-3.5 text-blue-600 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                      </svg>
                    </button>
                    <template #popper>
                      <div class="text-[11px] font-bold whitespace-nowrap">
                        📊 Cập nhật tiến độ thực hiện
                      </div>
                    </template>
                  </VTooltip>

                  <!-- Xóa -->
                  <VTooltip theme="custom-dark" placement="top" :delay="{ show: 1000, hide: 0 }">
                    <button 
                      @click="deleteItem(task)"
                      class="w-7 h-7 rounded-lg bg-rose-50 hover:bg-rose-100 border border-rose-200 text-rose-700 flex items-center justify-center transition shadow-2xs"
                    >
                      <svg class="w-3.5 h-3.5 text-rose-600 hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                      </svg>
                    </button>
                    <template #popper>
                      <div class="text-[11px] font-bold whitespace-nowrap">
                        🗑️ Xóa nhiệm vụ
                      </div>
                    </template>
                  </VTooltip>
                </div>
              </td>
            </tr>

            <tr v-if="filteredPrimaryList.length === 0">
              <td colspan="8" class="p-8 text-center text-slate-400 font-semibold italic space-y-2">
                <div>Chưa có Nhiệm vụ nào phù hợp bộ lọc.</div>
                <button @click="resetFilterSearch" class="mt-2 px-3.5 py-1.5 bg-blue-50 text-blue-700 hover:bg-blue-100 border border-blue-200 rounded-xl text-xs font-bold transition not-italic">
                  🔄 Xóa bộ lọc tìm kiếm
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- PAGINATION CONTROLS FOR LIST VIEW (ATTACHED FOOTER) -->
      <div class="flex flex-col sm:flex-row items-center justify-between bg-slate-50/70 p-4 border-t border-slate-200/80 w-full text-xs text-slate-600 font-semibold gap-3">
        <span>
          Hiển thị <span class="font-extrabold text-slate-900">{{ filteredPrimaryList.length > 0 ? (currentPrimaryPage - 1) * primaryPageSize + 1 : 0 }} - {{ Math.min(currentPrimaryPage * primaryPageSize, filteredPrimaryList.length) }}</span> trên tổng số <span class="font-extrabold text-slate-900">{{ filteredPrimaryList.length }}</span> {{ activePrimaryTab === 'goals' ? 'mục tiêu' : 'nhiệm vụ' }}
        </span>

        <div class="flex items-center gap-2">
          <button 
            @click="currentPrimaryPage--" 
            :disabled="currentPrimaryPage <= 1"
            class="px-3.5 py-1.5 rounded-lg text-xs font-bold bg-white border border-slate-200 text-slate-700 hover:bg-slate-100 disabled:opacity-40 transition shadow-2xs"
          >
            ← Trang Trước
          </button>

          <span class="px-3 py-1.5 rounded-lg text-xs font-extrabold bg-blue-50 text-blue-800 border border-blue-200 shadow-2xs">
            Trang {{ currentPrimaryPage }} / {{ totalPrimaryPages }}
          </span>

          <button 
            @click="currentPrimaryPage++" 
            :disabled="currentPrimaryPage >= totalPrimaryPages"
            class="px-3.5 py-1.5 rounded-lg text-xs font-bold bg-white border border-slate-200 text-slate-700 hover:bg-slate-100 disabled:opacity-40 transition shadow-2xs"
          >
            Trang Sau →
          </button>
        </div>
      </div>

    </div>
  </div>

    <!-- Edit Document Modal -->
    <div v-if="isEditDocModalOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-lg w-full p-6 space-y-4 max-h-[90vh] overflow-y-auto font-sans">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <h3 class="text-lg font-extrabold text-slate-800 flex items-center gap-2">
            ✏ Chỉnh Sửa Thông Tin Văn Bản
          </h3>
          <button @click="isEditDocModalOpen = false" class="text-slate-400 hover:text-slate-600 text-lg font-bold">✕</button>
        </div>

        <form @submit.prevent="handleUpdateDocument" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Mã / Số Hiệu</label>
              <input v-model="editForm.documentNumber" required class="w-full text-xs font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>

            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Người Ký / Chức Vụ</label>
              <input v-model="editForm.signer" placeholder="VD: Thủ tướng Chính phủ" class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Tên Văn Bản / Quyết Định</label>
            <textarea v-model="editForm.name" required rows="2" class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1"></textarea>
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase">Trích Yếu Nội Dung</label>
            <textarea v-model="editForm.summary" rows="3" placeholder="Trích yếu tóm tắt nội dung chính..." class="w-full text-xs font-semibold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1"></textarea>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Năm Bắt Đầu</label>
              <input type="number" v-model.number="editForm.startYear" required class="w-full text-xs font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>
            <div>
              <label class="text-xs font-bold text-slate-700 uppercase">Năm Kết Thúc</label>
              <input type="number" v-model.number="editForm.endYear" required class="w-full text-xs font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 mt-1" />
            </div>
          </div>

          <div>
            <label class="text-xs font-bold text-slate-700 uppercase block mb-1">File PDF Đính Kèm Minh Chứng</label>
            
            <div v-if="documentDetails?.attachmentPath && !isReplacingEditFile" class="p-3 bg-purple-50 rounded-xl border border-purple-200 flex items-center justify-between">
              <a 
                :href="`http://localhost:5000${documentDetails.attachmentPath}`" 
                target="_blank" 
                class="text-xs font-extrabold text-purple-800 hover:text-purple-950 hover:underline flex items-center gap-1.5"
              >
                <svg class="w-4 h-4 text-purple-700" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
                📄 Xem File PDF Đính Kèm Hiện Tại
              </a>

              <button 
                type="button" 
                @click="isReplacingEditFile = true" 
                class="text-xs font-bold text-blue-600 hover:underline"
              >
                Thay Đổi File
              </button>
            </div>

            <div v-else class="space-y-1">
              <div class="flex items-center gap-2">
                <input type="file" ref="editFileInput" accept=".pdf,.doc,.docx" class="w-full text-xs bg-slate-50 border border-slate-300 rounded-xl px-3 py-2" />
                <button 
                  v-if="documentDetails?.attachmentPath && isReplacingEditFile" 
                  type="button" 
                  @click="isReplacingEditFile = false" 
                  class="text-xs font-bold text-slate-500 hover:text-slate-700 shrink-0 px-2.5 py-1.5 bg-slate-100 rounded-lg"
                >
                  Hủy
                </button>
              </div>
              <p class="text-[11px] text-slate-400">Đính kèm file PDF/DOCX mới (Tối đa 10MB)</p>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
            <button type="button" @click="isEditDocModalOpen = false" class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition">Hủy</button>
            <button type="submit" :disabled="isSubmittingEdit" class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl transition shadow-sm">
              {{ isSubmittingEdit ? 'Đang lưu...' : 'Cập Nhật' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modals -->
    <CreateItemModal 
      :isOpen="isCreateItemModalOpen"
      :documentId="documentId"
      :itemType="createItemType"
      @close="isCreateItemModalOpen = false"
      @created="onItemCreated"
    />

    <ProgressUpdateModal 
      v-if="selectedTaskForProgress"
      :isOpen="isProgressModalOpen"
      :taskId="selectedTaskForProgress.id || selectedTaskForProgress.taskId"
      :taskCode="selectedTaskForProgress.code"
      :taskTitle="selectedTaskForProgress.title"
      :evaluationType="isQuantitative(selectedTaskForProgress) ? 'Quantitative' : 'Qualitative'"
      :unitName="selectedTaskForProgress.unit?.name || '%'"
      :customBaseline="selectedTaskForProgress.customBaseline"
      @close="isProgressModalOpen = false"
      @submitted="onProgressSubmitted"
    />

    <UrgeTaskModal
      v-if="selectedTaskForUrge"
      :isOpen="isUrgeModalOpen"
      :taskId="selectedTaskForUrge.id"
      :taskCode="selectedTaskForUrge.code"
      :taskTitle="selectedTaskForUrge.title"
      :leadAgencyName="selectedTaskForUrge.leadAgency?.name || selectedTaskForUrge.leadAgency?.code || ''"
      :actualProgressPct="getItemProgressComparison(selectedTaskForUrge).actualPct"
      :expectedTargetPct="getItemProgressComparison(selectedTaskForUrge).targetPct"
      :laggingDeltaPct="getItemProgressComparison(selectedTaskForUrge).deltaPct"
      @close="isUrgeModalOpen = false"
      @submitted="onUrgeSubmitted"
    />

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import DynamicPlanningGrid from '../components/DynamicPlanningGrid.vue';
import CreateItemModal from '../components/CreateItemModal.vue';
import ProgressUpdateModal from '../components/ProgressUpdateModal.vue';
import UrgeTaskModal from '../components/UrgeTaskModal.vue';

const props = defineProps({
  documentId: { type: String, required: true }
});

const emit = defineEmits(['back']);

const activePrimaryTab = ref('goals'); // 'goals' or 'tasks'
const activeSubTab = ref('list'); // 'list' or 'grid'

const documentDetails = ref(null);
const agencies = ref([]);

// Advanced Filters State & Local Storage Persistence
const filterDraft = ref({
  searchQuery: '',
  selectedAgencyId: 'all',
  selectedStatusFilter: 'all',
  selectedYearFilter: 'all'
});

const appliedFilters = ref({
  searchQuery: '',
  selectedAgencyId: 'all',
  selectedStatusFilter: 'all',
  selectedYearFilter: 'all'
});

const currentPrimaryPage = ref(1);
const primaryPageSize = ref(10);

const STORAGE_KEY = computed(() => 'cdsqg_doc_detail_filters_' + props.documentId);

function loadDetailFilters() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY.value);
    if (saved) {
      const parsed = JSON.parse(saved);
      filterDraft.value = { ...parsed };
      appliedFilters.value = { ...parsed };
    }
  } catch (e) {
    console.error(e);
  }
}

function execFilterSearch() {
  appliedFilters.value = { ...filterDraft.value };
  currentPrimaryPage.value = 1;
  try {
    localStorage.setItem(STORAGE_KEY.value, JSON.stringify(appliedFilters.value));
  } catch (e) {
    console.error(e);
  }
}

function resetFilterSearch() {
  filterDraft.value = { searchQuery: '', selectedAgencyId: 'all', selectedStatusFilter: 'all', selectedYearFilter: 'all' };
  appliedFilters.value = { searchQuery: '', selectedAgencyId: 'all', selectedStatusFilter: 'all', selectedYearFilter: 'all' };
  currentPrimaryPage.value = 1;
  try {
    localStorage.removeItem(STORAGE_KEY.value);
  } catch (e) {
    console.error(e);
  }
}

const planningGridRef = ref(null);

function refreshAllData() {
  loadDocumentDetails();
  if (planningGridRef.value && planningGridRef.value.loadGridData) {
    planningGridRef.value.loadGridData();
  }
}

watch(activePrimaryTab, () => {
  currentPrimaryPage.value = 1;
  refreshAllData();
});

watch(activeSubTab, (newSubTab) => {
  if (newSubTab === 'grid') {
    if (planningGridRef.value && planningGridRef.value.loadGridData) {
      planningGridRef.value.loadGridData();
    }
  } else if (newSubTab === 'list') {
    loadDocumentDetails();
  }
});

const isCreateItemModalOpen = ref(false);
const createItemType = ref('Goal');

const isProgressModalOpen = ref(false);
const selectedTaskForProgress = ref(null);

const isUrgeModalOpen = ref(false);
const selectedTaskForUrge = ref(null);

const isEditDocModalOpen = ref(false);
const isSubmittingEdit = ref(false);
const isReplacingEditFile = ref(false);
const editFileInput = ref(null);
const editForm = ref({
  documentNumber: '',
  name: '',
  summary: '',
  signer: '',
  startYear: 2026,
  endYear: 2030
});

const documentItems = computed(() => {
  if (!documentDetails.value) return [];
  const list = documentDetails.value.items || documentDetails.value.Items || [];
  return Array.isArray(list) ? list : [];
});

const goalsList = computed(() => {
  return documentItems.value.filter(i => {
    const t = i.itemType ?? i.ItemType ?? i.itemTypeEnum;
    return t === 'Goal' || t === 1 || t === '1' || String(t).toLowerCase() === 'goal';
  });
});

const tasksList = computed(() => {
  return documentItems.value.filter(i => {
    const t = i.itemType ?? i.ItemType ?? i.itemTypeEnum;
    return t === 'Task' || t === 2 || t === '2' || String(t).toLowerCase() === 'task';
  });
});

const currentPrimaryList = computed(() => {
  return activePrimaryTab.value === 'goals' ? goalsList.value : tasksList.value;
});

const filteredPrimaryList = computed(() => {
  let list = currentPrimaryList.value;

  // 1. Text Search Filter
  if (appliedFilters.value.searchQuery.trim()) {
    const q = appliedFilters.value.searchQuery.toLowerCase().trim();
    list = list.filter(item => 
      (item.code && item.code.toLowerCase().includes(q)) ||
      (item.title && item.title.toLowerCase().includes(q)) ||
      (item.category && item.category.toLowerCase().includes(q)) ||
      (item.leadAgency && (item.leadAgency.name?.toLowerCase().includes(q) || item.leadAgency.code?.toLowerCase().includes(q)))
    );
  }

  // 2. Agency Filter
  if (appliedFilters.value.selectedAgencyId !== 'all') {
    list = list.filter(item => item.leadAgencyId === appliedFilters.value.selectedAgencyId || item.leadAgency?.id === appliedFilters.value.selectedAgencyId);
  }

  // 3. Status/Alert Filter
  if (appliedFilters.value.selectedStatusFilter !== 'all') {
    list = list.filter(item => {
      const comp = getItemProgressComparison(item);
      return comp.status === appliedFilters.value.selectedStatusFilter;
    });
  }

  return list;
});

const totalPrimaryPages = computed(() => Math.ceil(filteredPrimaryList.value.length / primaryPageSize.value) || 1);

const paginatedPrimaryList = computed(() => {
  const start = (currentPrimaryPage.value - 1) * primaryPageSize.value;
  return filteredPrimaryList.value.slice(start, start + primaryPageSize.value);
});

// BULLETPROOF DYNAMIC EVALUATION TYPE BINDING
function isQuantitative(item) {
  if (!item) return false;
  return item.evaluationType === 'Quantitative' || 
         item.evaluationType === 1 || 
         item.evaluationType === '1' || 
         (item.unit && (item.unit.dataType === 1 || item.unit.dataType === 2 || item.unit.dataType === 'Decimal' || item.unit.dataType === 'Integer'));
}

function getItemProgressComparison(item) {
  if (!item) return { status: 'NoPlan', label: 'Chưa thiết lập kế hoạch', badgeClass: 'bg-slate-100 text-slate-500 border border-slate-200 font-semibold', actualPct: 0, targetPct: 0, deltaPct: 0 };

  const targetYear = (appliedFilters.value?.selectedYearFilter && appliedFilters.value.selectedYearFilter !== 'all') ? Number(appliedFilters.value.selectedYearFilter) : 2026;

  if (isQuantitative(item)) {
    let target = null;
    let periodLabel = `Năm ${targetYear}`;

    // 1. Check customBaseline for year or quarter key
    if (item.customBaseline && typeof item.customBaseline === 'object' && Object.keys(item.customBaseline).length > 0) {
      const yKey = `${targetYear}`;
      const qKeys = [`Q1_${targetYear}`, `Q2_${targetYear}`, `Q3_${targetYear}`, `Q4_${targetYear}`, `${targetYear}_Q1`, `${targetYear}_Q2`, `${targetYear}_Q3`, `${targetYear}_Q4` ];
      
      if (item.customBaseline[yKey] !== undefined && !isNaN(Number(item.customBaseline[yKey])) && Number(item.customBaseline[yKey]) > 0) {
        target = Number(item.customBaseline[yKey]);
      } else {
        for (const k of qKeys) {
          if (item.customBaseline[k] !== undefined && !isNaN(Number(item.customBaseline[k])) && Number(item.customBaseline[k]) > 0) {
            target = Number(item.customBaseline[k]);
            periodLabel = `Quý ${k.split('_')[0].replace('Q', '')}`;
            break;
          }
        }
      }
    }

    // 2. Check baselines array (from item.baselines or item.targetBaselines)
    if (target === null) {
      const baselinesList = item.baselines || item.targetBaselines || [];
      if (Array.isArray(baselinesList) && baselinesList.length > 0) {
        const yearBaseline = baselinesList.find(b => b.year === targetYear && (b.quarter === 0 || b.targetQuantity > 0 || b.targetValue > 0));
        if (yearBaseline) {
          const val = yearBaseline.targetQuantity !== undefined ? yearBaseline.targetQuantity : yearBaseline.targetValue;
          if (val !== null && val !== undefined && !isNaN(Number(val)) && Number(val) > 0) {
            target = Number(val);
          }
        }
      }
    }

    // Requirement: If no plan has been setup for this year, do NOT show progress comparison info
    if (target === null || target === undefined || isNaN(target) || target <= 0) {
      return { 
        status: 'NoPlan', 
        label: 'Chưa thiết lập kế hoạch', 
        badgeClass: 'bg-slate-100 text-slate-500 border border-slate-200 font-semibold', 
        actualPct: 0, 
        targetPct: 0, 
        deltaPct: 0 
      };
    }

    if (item.latestProgressValue === null || item.latestProgressValue === undefined) {
      return { 
        status: 'NoReport', 
        label: `Chưa có Báo cáo (Kế hoạch: ${target}%)`, 
        badgeClass: 'bg-slate-100 text-slate-600 border border-slate-200 font-medium', 
        actualPct: 0, 
        targetPct: target, 
        deltaPct: -100 
      };
    }

    const val = Number(item.latestProgressValue);
    const unitStr = item.unitName || item.unit?.name || '%';
    const actualPct = target > 0 ? Math.round((val / target) * 100) : 0;
    const deltaPct = val - target;

    const displayRatio = unitStr === '%' ? `${val}% / ${target}% Kế hoạch` : `${val} / ${target} ${unitStr} (Kế hoạch)`;

    if (val >= target) {
      return { 
        status: 'Completed', 
        label: `Đạt kế hoạch (${displayRatio})`, 
        badgeClass: 'bg-emerald-100 text-emerald-800 border border-emerald-300 font-extrabold', 
        actualPct, 
        targetPct: target, 
        deltaPct 
      };
    } else {
      return { 
        status: 'Lagging', 
        label: `Chậm tiến độ (${displayRatio})`, 
        badgeClass: 'bg-rose-100 text-rose-800 border border-rose-300 font-black', 
        actualPct, 
        targetPct: target, 
        deltaPct 
      };
    }
  } else {
    // Qualitative Status: Check if target baseline exists for this year
    let hasPlan = false;
    if (item.customBaseline && typeof item.customBaseline === 'object' && Object.keys(item.customBaseline).length > 0) {
      const yKey = `${targetYear}`;
      const qKeys = [`Q1_${targetYear}`, `Q2_${targetYear}`, `Q3_${targetYear}`, `Q4_${targetYear}`, `${targetYear}_Q1`, `${targetYear}_Q2`, `${targetYear}_Q3`, `${targetYear}_Q4` ];
      if ((item.customBaseline[yKey] && item.customBaseline[yKey] !== '--') || qKeys.some(k => item.customBaseline[k] && item.customBaseline[k] !== '--')) {
        hasPlan = true;
      }
    }
    const baselinesList = item.baselines || item.targetBaselines || [];
    if (Array.isArray(baselinesList) && baselinesList.length > 0) {
      const b = baselinesList.find(b => b.year === targetYear && (b.quarter === 0 || b.targetQualitativeStatus));
      if (b && (b.targetQualitativeStatus || b.targetQuantity)) hasPlan = true;
    }

    if (!hasPlan) {
      return { status: 'NoPlan', label: 'Chưa thiết lập kế hoạch', badgeClass: 'bg-slate-100 text-slate-500 border border-slate-200 font-semibold', actualPct: 0, targetPct: 0, deltaPct: 0 };
    }

    const st = item.latestProgressStatus;
    if (!st || st === 'NotStarted') {
      return { status: 'Lagging', label: 'Chưa thực hiện (0%)', badgeClass: 'bg-rose-100 text-rose-800 border border-rose-300 font-bold', actualPct: 0, targetPct: 100, deltaPct: -100 };
    }
    if (st === 'Completed') {
      return { status: 'Completed', label: 'Đã hoàn thành (100%)', badgeClass: 'bg-emerald-100 text-emerald-800 border border-emerald-300 font-extrabold', actualPct: 100, targetPct: 100, deltaPct: 0 };
    }
    if (st === 'Reviewing') {
      return { status: 'OnTrack', label: 'Đang xin ý kiến (75%)', badgeClass: 'bg-sky-100 text-sky-800 border border-sky-300 font-bold', actualPct: 75, targetPct: 100, deltaPct: -25 };
    }
    if (st === 'Drafting') {
      return { status: 'OnTrack', label: 'Đang soạn thảo (40%)', badgeClass: 'bg-amber-100 text-amber-800 border border-amber-300 font-bold', actualPct: 40, targetPct: 100, deltaPct: -60 };
    }
    return { status: 'NoReport', label: st, badgeClass: 'bg-slate-100 text-slate-600 border border-slate-200', actualPct: 0, targetPct: 100, deltaPct: -100 };
  }
}

function getAgencyCode(agencyId) {
  const a = agencies.value.find(ag => ag.id === agencyId);
  return a ? a.code : (agencyId ? agencyId.substring(0, 5) : '—');
}

function formatStatusText(status) {
  if (!status) return 'Chưa có Báo cáo';
  switch (status) {
    case 'Completed': return 'Đã hoàn thành';
    case 'Reviewing': return 'Đang xin ý kiến';
    case 'Drafting': return 'Đang soạn thảo';
    case 'NotStarted': return 'Chưa thực hiện';
    default: return status;
  }
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return '';
  return d.toLocaleDateString('vi-VN');
}

async function loadDocumentDetails() {
  try {
    const res = await fetch(`http://localhost:5000/api/documents/${props.documentId}`);
    if (res.ok) {
      documentDetails.value = await res.json();
    }
  } catch (e) {
    console.error('Failed to load document detail:', e);
  }
}

async function loadAgencies() {
  try {
    const res = await fetch('http://localhost:5000/api/agencies');
    if (res.ok) agencies.value = await res.json();
  } catch (e) {}
}

function openEditDocModal() {
  if (!documentDetails.value) return;
  editForm.value = {
    documentNumber: documentDetails.value.documentNumber || '',
    name: documentDetails.value.name || '',
    summary: documentDetails.value.summary || '',
    signer: documentDetails.value.signer || '',
    startYear: documentDetails.value.startYear || 2026,
    endYear: documentDetails.value.endYear || 2030
  };
  isReplacingEditFile.value = false;
  isEditDocModalOpen.value = true;
}

async function handleUpdateDocument() {
  isSubmittingEdit.value = true;
  try {
    const formData = new FormData();
    formData.append('DocumentNumber', editForm.value.documentNumber);
    formData.append('Name', editForm.value.name);
    if (editForm.value.summary) formData.append('Summary', editForm.value.summary);
    if (editForm.value.signer) formData.append('Signer', editForm.value.signer);
    formData.append('StartYear', editForm.value.startYear);
    formData.append('EndYear', editForm.value.endYear);

    if (editFileInput.value && editFileInput.value.files && editFileInput.value.files[0]) {
      formData.append('AttachmentFile', editFileInput.value.files[0]);
    }

    const res = await fetch(`http://localhost:5000/api/documents/${props.documentId}`, {
      method: 'PUT',
      body: formData
    });

    if (res.ok) {
      isEditDocModalOpen.value = false;
      toast.success("Cập nhật thông tin văn bản thành công!");
      await loadDocumentDetails();
    } else {
      const err = await res.text();
      toast.error(`Lỗi cập nhật văn bản: ${err}`);
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ khi cập nhật văn bản.');
  } finally {
    isSubmittingEdit.value = false;
  }
}

function openCreateModal(type) {
  createItemType.value = type;
  isCreateItemModalOpen.value = true;
}

function onItemCreated() {
  toast.success("Thêm mới thành công!");
  refreshAllData();
}

function openProgressModal(item) {
  selectedTaskForProgress.value = item;
  isProgressModalOpen.value = true;
}

function onProgressSubmitted() {
  refreshAllData();
}

function openUrgeModal(item) {
  selectedTaskForUrge.value = item;
  isUrgeModalOpen.value = true;
}

function onUrgeSubmitted() {
  toast.success("Lưu văn bản đôn đốc thành công!");
  refreshAllData();
}

async function deleteItem(item) {
  if (!confirm(`Bạn có chắc muốn xóa ${item.code}: ${item.title}?`)) return;
  try {
    const res = await fetch(`http://localhost:5000/api/planning/items/${item.id}`, { method: 'DELETE' });
    if (res.ok) {
      toast.success(`Đã xóa thành công ${item.code}`);
      refreshAllData();
    } else {
      toast.error('Lỗi khi xóa mục.');
    }
  } catch (e) {
    toast.error('Không thể kết nối máy chủ.');
  }
}

function splitAttachmentPaths(pathString) {
  if (!pathString) return [];
  return pathString.split(',').map(s => s.trim()).filter(Boolean);
}

function getFileUrl(path) {
  if (!path) return '#';
  const clean = path.trim();
  if (clean.startsWith('http://') || clean.startsWith('https://')) return clean;
  return `http://localhost:5000${clean.startsWith('/') ? '' : '/'}${clean}`;
}

function formatFileName(fullPath) {
  if (!fullPath) return '';
  const fileName = fullPath.trim().split(/[/\\]/).pop();
  return fileName.replace(/^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}_/i, '');
}

watch(() => props.documentId, (newId) => {
  if (newId) {
    resetFilterSearch();
    loadDocumentDetails();
  }
}, { immediate: true });

onMounted(() => {
  loadAgencies();
});
</script>
