<template>
<div v-accessible-dialog="() => isEditModalOpen = false"  @click.self="isEditModalOpen = false" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 max-w-4xl sm:max-w-5xl w-full p-6 sm:p-7 space-y-4 max-h-[90vh] flex flex-col">
        <h3 class="text-base font-bold text-slate-800 border-b border-slate-100 pb-2 shrink-0">
          Chỉnh Sửa {{ editingItem?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} ({{ editingItem?.code }})
        </h3>

        <form @submit.prevent="submitEditItem" class="space-y-3 overflow-y-auto pr-1 custom-scrollbar">
          <div v-if="editErrorMessage" class="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs font-bold flex items-center justify-between gap-2 shadow-2xs shrink-0">
            <div class="flex items-center gap-2">
              <span class="text-base">⚠️</span>
              <span>{{ editErrorMessage }}</span>
            </div>
            <button type="button" @click="editErrorMessage = ''" class="text-rose-400 hover:text-rose-600 font-bold text-sm cursor-pointer p-1">✕</button>
          </div>
          <!-- Row 1: Code, Section & Group -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div>
              <label class="text-sm font-semibold text-slate-700 uppercase block mb-1">
                Mã {{ editingItem?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} <span class="text-rose-500">*</span>
              </label>
              <input
                v-model="editForm.code"
                type="text"
                required
                :placeholder="editingItem?.itemType === 'Goal' ? 'e.g. MT-01' : 'e.g. NV-01'"
                class="ui-single-line ui-control w-full text-base sm:text-sm font-bold bg-slate-50 border border-slate-300 rounded-xl px-3 py-2 focus:bg-white focus:ring-2 focus:ring-blue-500 uppercase  "
              />
            </div>

            <div v-if="editingItem?.itemType === 'Goal' || editingItem?.itemType === 1 || editingItem?.itemType === '1'">
              <SearchableSelect
                v-model="editForm.section"
                :options="sectionFilterOptions"
                :isMulti="false"
                :required="true"
                label="Mục"
                placeholder="-- Chọn Mục --"
              />
            </div>

            <div :class="(editingItem?.itemType === 'Goal' || editingItem?.itemType === 1 || editingItem?.itemType === '1') ? '' : 'sm:col-span-2'">
              <SearchableSelect
                v-model="editForm.group"
                :options="groupFilterOptions"
                :isMulti="false"
                label="Nhóm Trọng Tâm"
                placeholder="-- Chọn Nhóm --"
              />
            </div>
          </div>

          <!-- Row 2: Title (Full Width) -->
          <div>
            <label class="text-sm font-semibold text-slate-700 uppercase block mb-1">
              Tên {{ editingItem?.itemType === 'Goal' ? 'Mục Tiêu' : 'Nhiệm Vụ' }} <span class="text-rose-500">*</span>
            </label>
            <textarea
              v-model="editForm.title"
              required
              rows="2"
              placeholder="Nhập tên chi tiết..."
              class="ui-control w-full text-base sm:text-sm font-semibold bg-slate-50 border border-slate-300 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-blue-500"
            ></textarea>
          </div>

          <!-- Unit Selection for Goals -->
          <div v-if="editingItem?.itemType === 'Goal' || editingItem?.itemType === 1 || editingItem?.itemType === '1'">
            <SearchableSelect
              v-model="editForm.unitName"
              :options="goalUnitOptions"
              :isMulti="false"
              :required="true"
              label="Đơn Vị Tính"
              placeholder="-- Chọn Đơn vị tính --"
            />
          </div>

          <!-- Is Ongoing Toggle -->
          <div class="flex items-center gap-2.5 p-3 bg-blue-50/70 border border-blue-200 rounded-xl">
            <input
              type="checkbox"
              id="editIsOngoingToggle"
              v-model="editForm.isOngoing"
              class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer"
            />
            <label for="editIsOngoingToggle" class="text-sm font-semibold text-blue-950 cursor-pointer select-none flex items-center gap-1.5">
              <span>Thời hạn thực hiện: Thường xuyên</span>
              <span class="text-[11px] font-normal text-slate-500">(Tự động áp dụng từ 01/01/2026 đến 31/12/2030)</span>
            </label>
          </div>

          <!-- Date Range Inputs -->
          <div v-if="!editForm.isOngoing" class="grid grid-cols-2 gap-3">
            <div>
              <SearchableSelect
                v-model="editForm.startYear"
                :options="yearOptions"
                :isMulti="false"
                label="Năm Bắt Đầu"
                placeholder="-- Chọn năm bắt đầu --"
              />
            </div>
            <div>
              <SearchableSelect
                v-model="editForm.dueYear"
                :options="yearOptions"
                :isMulti="false"
                label="Năm Hoàn Thành"
                placeholder="-- Chọn năm hoàn thành --"
              />
            </div>
          </div>
          <div v-else class="grid grid-cols-2 gap-3 bg-blue-50/50 p-3 rounded-xl border border-blue-200/60">
            <div>
              <label class="text-sm font-semibold text-slate-700 uppercase block mb-1">Ngày Bắt Đầu</label>
              <DatePicker v-model="editForm.startDate" placeholder="dd/mm/yyyy" />
            </div>
            <div>
              <label class="text-sm font-semibold text-slate-700 uppercase block mb-1">Ngày Hoàn Thành</label>
              <DatePicker v-model="editForm.dueDate" placeholder="dd/mm/yyyy" />
            </div>
          </div>

          <!-- Lead Agency, Assigned Sub-Agency & Coordinating Agencies -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div>
              <SearchableSelect
                v-model="editForm.leadAgencyId"
                :options="leadAgencyOptions"
                :isMulti="false"
                :required="true"
                label="Đơn Vị Chủ Trì"
                placeholder="-- Chọn đơn vị chủ trì --"
              />
            </div>
            <div>
              <SearchableSelect
                v-model="editForm.assignedAgencyId"
                :options="editAssignedAgencyOptions"
                :isMulti="false"
                label="Giao Đơn Vị Trực Thuộc"
                placeholder="-- Chọn đơn vị trực thuộc --"
                :disabled="!editForm.leadAgencyId || editAssignedAgencyOptions.length === 0"
              />
            </div>
            <div>
              <SearchableSelect
                v-model="editForm.coordinatingAgencyIds"
                :options="coordinatingAgencyOptions"
                :isMulti="true"
                label="Cơ Quan Phối Hợp"
                placeholder="-- Chọn cơ quan phối hợp --"
              />
            </div>
          </div>

          <!-- Multi-Deliverables Section for Tasks -->
          <div v-if="editingItem?.itemType !== 'Goal'" class="border border-slate-200 rounded-xl p-3.5 bg-slate-50/50 space-y-3">
            <div class="flex items-center justify-between">
              <label class="text-sm font-semibold text-slate-800 uppercase flex items-center gap-1.5">
                <span>📋 DANH MỤC SẢN PHẨM ĐẦU RA DỰ KIẾN</span>
              </label>
              <button
                type="button"
                @click="addEditDeliverable"
                class="ui-single-line px-2.5 py-1 bg-blue-50 hover:bg-blue-100 text-blue-700 font-bold text-xs rounded-lg transition border border-blue-200 flex items-center gap-1 cursor-pointer"
              >
                + Thêm sản phẩm đầu ra
              </button>
            </div>

            <div v-if="!editForm.deliverables || editForm.deliverables.length === 0" class="text-xs text-slate-400 italic text-center py-2">
              Chưa khai báo sản phẩm đầu ra. Nhấn nút trên để thêm sản phẩm cụ thể.
            </div>

            <div v-else class="space-y-2.5 max-h-48 overflow-y-auto pr-1">
              <div
                v-for="(del, idx) in editForm.deliverables"
                :key="idx"
                class="bg-white p-2.5 rounded-xl border border-slate-200 shadow-2xs space-y-2 relative"
              >
                <div class="flex items-center justify-between border-b border-slate-100 pb-1">
                  <span class="text-[11px] font-bold text-blue-800">Sản phẩm đầu ra #{{ idx + 1 }}</span>
                  <button
                    type="button"
                    @click="removeEditDeliverable(idx)"
                    class="text-rose-500 hover:text-rose-700 text-xs font-bold hover:bg-rose-50 px-2 py-0.5 rounded transition cursor-pointer"
                  >
                    ✕ Xóa
                  </button>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
                  <div class="sm:col-span-2">
                    <label class="text-[10px] font-semibold text-slate-600">Tên sản phẩm / Tên văn bản <span class="text-rose-500">*</span></label>
                    <input
                      v-model="del.title"
                      required
                      placeholder="Ví dụ: Nghị định quy định về Dữ liệu số / Nền tảng chia sẻ..."
                      class="ui-single-line ui-control w-full text-base sm:text-sm font-semibold bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-500  "
                    />
                  </div>
                  <div>
                    <label class="text-[10px] font-semibold text-slate-600">Hạn chót sản phẩm</label>
                    <DatePicker v-model="del.dueDate" placeholder="dd/mm/yyyy" />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end gap-2 border-t border-slate-100 pt-3">
            <button type="button" @click="isEditModalOpen = false" class="ui-single-line px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl cursor-pointer">Hủy</button>
            <button type="submit" class="ui-single-line px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm cursor-pointer">Lưu Thay Đổi</button>
          </div>
        </form>
      </div>
    </div>
</template>

<script setup>
import SearchableSelect from '../../components/SearchableSelect.vue';
import DatePicker from '../../components/DatePicker.vue';

defineProps([
  'sectionFilterOptions',
  'groupFilterOptions',
  'leadAgencyOptions',
  'yearOptions',
  'editingItem',
  'goalUnitOptions',
  'editAssignedAgencyOptions',
  'coordinatingAgencyOptions',
  'addEditDeliverable',
  'removeEditDeliverable',
  'submitEditItem'
]);
const editForm = defineModel('editForm', { required: true });
const isEditModalOpen = defineModel('isEditModalOpen', { required: true });
const editErrorMessage = defineModel('editErrorMessage', { required: true });
</script>



