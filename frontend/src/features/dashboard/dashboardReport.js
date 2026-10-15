import { toast } from 'vue3-toastify';
import { loadExcelWriter } from '../../utils/excelRuntime';
import { appendSummaryWorksheet } from './summaryWorksheet';
import { appendAgencyWorksheet } from './agencyWorksheet';

export async function exportDashboardReport({ dashboardFilter, selectedScopes, selectedSections, selectedGroups, fromYear, toYear, isOngoingOnly, agencies, metrics, filterOutSpecialAgencies, filteredMinistriesPerformance, filteredProvincesPerformance, filteredOthersPerformance, formatDate, formatItemProgressDisplay, isExportingExcel }) {
  const XLSX = await loadExcelWriter();
  isExportingExcel.value = true;
  const filterType = dashboardFilter.value; // 'goals', 'tasks', or 'all'
  const isGoalsOnly = filterType === 'goals';
  const isTasksOnly = filterType === 'tasks';

  const filterNameLabel = isGoalsOnly ? 'Mục tiêu' : (isTasksOnly ? 'Nhiệm vụ' : 'Mục tiêu & Nhiệm vụ');
  toast.info(`Đang khởi tạo báo cáo Excel (${filterNameLabel}) theo bộ lọc...`, { autoClose: 2000 });

  try {
    const allFilteredAgencies = [
      ...(filteredMinistriesPerformance.value || []),
      ...(filteredProvincesPerformance.value || []),
      ...(filteredOthersPerformance.value || [])
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
    appendSummaryWorksheet({ XLSX, wb, isGoalsOnly, isTasksOnly, filterNameLabel, timeStr, allFilteredAgencies, metrics });

    const filters = {
      dashboardFilter: filterType,
      selectedScopes: [...selectedScopes.value],
      selectedSections: [...selectedSections.value],
      selectedGroups: [...selectedGroups.value],
      fromYear: fromYear.value,
      toYear: toYear.value,
      isOngoingOnly: isOngoingOnly.value
    };
    const usedSheetNames = new Set(["TỔNG HỢP CHUNG"]);

    // ==========================================
    // SHEETS FOR EACH AGENCY
    // ==========================================
    for (const ag of allFilteredAgencies) {
      await appendAgencyWorksheet({ XLSX, wb, ag, usedSheetNames, filters, filterOutSpecialAgencies, isGoalsOnly, isTasksOnly, filterNameLabel, timeStr, agencies, formatDate, formatItemProgressDisplay });
    }

    const fileSuffix = isGoalsOnly ? "Muc_Tieu" : (isTasksOnly ? "Nhiem_Vu" : "Chien_Luoc");
    const dateFileStr = now.toISOString().slice(0, 10);
    XLSX.writeFile(wb, `Bao_Cao_Theo_Doi_${fileSuffix}_${dateFileStr}.xlsx`);
    toast.success(`Đã xuất thành công file Báo cáo Excel (${filterNameLabel}) gồm ${allFilteredAgencies.length + 1} Sheet!`);
  } catch (err) {
    console.error("Lỗi khi xuất báo cáo Excel:", err);
    toast.error("Lỗi khi xuất file Excel: " + (err.message || err));
  } finally {
    isExportingExcel.value = false;
  }
}
