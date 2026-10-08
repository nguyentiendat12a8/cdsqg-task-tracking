import { authState } from '../../services/auth';
import { exportToExcel, exportProgressReportTemplate } from '../../utils/excelExport';

export function createDocumentReportActions({ props, userRoleStr, currentUserAgencyObj, formatDateRange, getStatusLabel, formatProgressDisplay, isGeneralTaskOrAllAgencies, filteredList }) {
  async function handleExportProgressReport() {
    await exportProgressReportTemplate({
      items: filteredList.value || [],
      currentAgency: currentUserAgencyObj.value,
      userRole: userRoleStr.value,
      fileName: `Bao_Cao_Tien_Do_${props.filterItemType === 'Goal' ? 'Muc_Tieu' : 'Nhiem_Vu'}`
    });
  }

  async function exportDocumentItemsToExcel() {
    const isGoal = (props.filterItemType === 'Goal');
    const itemTypeLabel = isGoal ? 'Mục Tiêu' : 'Nhiệm Vụ';
    const title = `DANH SÁCH ${itemTypeLabel.toUpperCase()} THEO DÕI CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg`;
    const fileName = `Danh_Sach_${isGoal ? 'Muc_Tieu' : 'Nhiem_Vu'}_1266`;
    const sheetName = isGoal ? 'Mục tiêu' : 'Nhiệm vụ';

    const headers = [
      "STT",
      "Mã",
      isGoal ? "Tên Mục Tiêu" : "Tên Nhiệm Vụ",
      "Cơ Quan Chủ Trì",
      "Giao Đơn Vị Trực Thuộc",
      "Cơ Quan Phối Hợp",
      "Thời Gian Thực Hiện",
      ...(isGoal ? ["Tiến Độ Hiện Tại"] : []),
      "Trạng Thái"
    ];

    const minColWidths = isGoal ? {
      0: 8, 1: 15, 2: 50, 3: 30, 4: 28, 5: 25, 6: 22, 7: 18, 8: 22
    } : {
      0: 8, 1: 15, 2: 50, 3: 30, 4: 28, 5: 25, 6: 22, 7: 22
    };

    const rows = filteredList.value.map((item, idx) => {
      let dateStr = 'Thường xuyên';
      if (!item.isOngoing) {
        dateStr = formatDateRange(item.startDate, item.dueDate);
      }
      const isGeneral = authState.isAdmin.value && isGeneralTaskOrAllAgencies(item);
      const progStr = isGeneral ? '—' : formatProgressDisplay(item);
      const statusStr = isGeneral ? '—' : getStatusLabel(item.calculatedStatus);

      const row = [
        idx + 1,
        item.code || '',
        item.title || '',
        item.leadAgencyName || '',
        item.assignedAgencyName || '—',
        item.cooperatingAgencies || '',
        dateStr
      ];
      if (isGoal) {
        row.push(progStr);
      }
      row.push(statusStr);
      return row;
    });

    await exportToExcel({
      title,
      headers,
      rows,
      fileName,
      sheetName,
      minColWidths
    });
  }

  return { handleExportProgressReport, exportDocumentItemsToExcel };
}
