import { authState } from '../../services/auth';
import { activeDocument } from '../../services/documentContext';
import { exportToExcel, exportProgressReportTemplate } from '../../utils/excelExport';

export function createDocumentReportActions({ props, userRoleStr, currentUserAgencyObj, formatDateRange, getStatusLabel, formatProgressDisplay, isGeneralTaskOrAllAgencies, loadExportItems }) {
  async function handleExportProgressReport() {
    const doc = activeDocument.value;
    await exportProgressReportTemplate({
      items: await loadExportItems(),
      itemType: props.filterItemType,
      currentAgency: currentUserAgencyObj.value,
      userRole: userRoleStr.value,
      fileName: `Bao_Cao_Tien_Do_${props.filterItemType === 'Goal' ? 'Muc_Tieu' : 'Nhiem_Vu'}_${doc.code || '1266'}`,
      documentTitle: doc.title,
      documentNumber: doc.documentNumber,
      documentShortTitle: doc.shortTitle
    });
  }

  async function exportDocumentItemsToExcel() {
    const doc = activeDocument.value;
    const isGoal = (props.filterItemType === 'Goal');
    const itemTypeLabel = isGoal ? 'Mục Tiêu' : 'Nhiệm Vụ';
    const docTypeLabel = (doc.shortTitle || 'Chiến Lược').toUpperCase();
    const docTitle = (doc.title || doc.name).toUpperCase();
    const title = `DANH SÁCH ${itemTypeLabel.toUpperCase()} THEO DÕI ${docTypeLabel} - ${docTitle} (${doc.documentNumber})`;
    const subtitle = `Văn bản: ${doc.name} | Giai đoạn: ${doc.startYear} – ${doc.endYear} | Thời gian xuất: ${new Date().toLocaleDateString('vi-VN')} ${new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
    const fileName = `Danh_Sach_${isGoal ? 'Muc_Tieu' : 'Nhiem_Vu'}_${doc.code || '1266'}`;
    const sheetName = `${isGoal ? 'Mục tiêu' : 'Nhiệm vụ'} ${doc.code || ''}`.trim();

    const headers = [
      "STT",
      "Mã",
      isGoal ? "Tên Mục Tiêu" : "Tên Nhiệm Vụ",
      "Cơ Quan Chủ Trì",
      "Giao Đơn Vị Trực Thuộc",
      "Cơ Quan Phối Hợp",
      "Đơn Vị Theo Dõi Tiến Độ",
      "Hình Thức Giao",
      "Thời Gian Thực Hiện",
      ...(isGoal ? ["Tiến Độ Hiện Tại"] : []),
      "Trạng Thái"
    ];

    const minColWidths = isGoal ? {
      0: 8, 1: 15, 2: 50, 3: 30, 4: 28, 5: 25, 6: 28, 7: 20, 8: 22, 9: 18, 10: 22
    } : {
      0: 8, 1: 15, 2: 50, 3: 30, 4: 28, 5: 25, 6: 28, 7: 20, 8: 22, 9: 22
    };

    const rows = (await loadExportItems()).map((item, idx) => {
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
        Array.isArray(item.coordinatingAgencyNames) ? item.coordinatingAgencyNames.join(', ') : item.cooperatingAgencies || '',
        item.progressAgencyName || '—',
        item.assignmentKind === 'Group' ? 'Giao qua nhóm' : item.assignmentKind === 'Direct' ? 'Giao trực tiếp' : '—',
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
      subtitle,
      headers,
      rows,
      fileName,
      sheetName,
      minColWidths
    });
  }

  return { handleExportProgressReport, exportDocumentItemsToExcel };
}

