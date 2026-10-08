import { styleWorksheet } from '../../utils/worksheetStyle';

export function appendSummaryWorksheet({ XLSX, wb, isGoalsOnly, isTasksOnly, filterNameLabel, timeStr, allFilteredAgencies, metrics }) {
  const reportTitle = isGoalsOnly
    ? "BÁO CÁO TỔNG HỢP TIẾN ĐỘ THỰC HIỆN MỤC TIÊU CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg"
    : (isTasksOnly
        ? "BÁO CÁO TỔNG HỢP TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg"
        : "BÁO CÁO TỔNG HỢP TIẾN ĐỘ THỰC HIỆN MỤC TIÊU & NHIỆM VỤ CHIẾN LƯỢC - QUYẾT ĐỊNH 1266/QĐ-TTg");

  const summaryData = [
    [reportTitle],
    [`Thời gian xuất: ${timeStr} | Lọc theo: ${filterNameLabel} | Tổng số đơn vị: ${allFilteredAgencies.length}`],
    [],
    ["CHỈ SỐ TỔNG QUAN TOÀN HỆ THỐNG"]
  ];

  if (isGoalsOnly) {
    summaryData.push(["Tổng số mục tiêu", metrics.value.goalStatusSummary?.totalGoals ?? metrics.value.totalGoals ?? 0]);
  } else if (isTasksOnly) {
    summaryData.push(["Tổng số nhiệm vụ", metrics.value.taskStatusSummary?.totalTasks ?? metrics.value.totalTasks ?? 0]);
  } else {
    summaryData.push(["Tổng số mục tiêu", metrics.value.totalGoals || 0]);
    summaryData.push(["Tổng số nhiệm vụ", metrics.value.totalTasks || 0]);
    summaryData.push(["Tổng số hạng mục", (metrics.value.totalGoals || 0) + (metrics.value.totalTasks || 0)]);
  }

  summaryData.push([]);
  summaryData.push(["DANH SÁCH BỘ, NGÀNH, ĐỊA PHƯƠNG VÀ TIẾN ĐỘ THỰC HIỆN"]);

  let headerRowSheet1 = [];
  if (isGoalsOnly) {
    headerRowSheet1 = [
      "STT", "Loại hình", "Tên Bộ / Ngành / Địa phương", "Số lượng Mục tiêu",
      "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
      "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện",
      "Cán bộ đầu mối chính"
    ];
  } else if (isTasksOnly) {
    headerRowSheet1 = [
      "STT", "Loại hình", "Tên Bộ / Ngành / Địa phương", "Số lượng Nhiệm vụ",
      "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
      "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện",
      "Cán bộ đầu mối chính"
    ];
  } else {
    headerRowSheet1 = [
      "STT", "Loại hình", "Tên Bộ / Ngành / Địa phương", "Tổng số", "Mục tiêu", "Nhiệm vụ",
      "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
      "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện",
      "Cán bộ đầu mối chính"
    ];
  }
  summaryData.push(headerRowSheet1);

  allFilteredAgencies.forEach((ag, idx) => {
    const mainContact = ag.contactPersons?.[0];
    const contactStr = mainContact ? `${mainContact.name || ''} (${mainContact.phone || mainContact.email || ''})` : '—';
    const agencyTypeLabel = ag.type === 'Ministry' ? 'Bộ / Ngành' : (ag.type === 'Province' ? 'Địa phương' : 'Đơn vị khác');

    if (isGoalsOnly) {
      summaryData.push([
        idx + 1, agencyTypeLabel, ag.name,
        ag.totalGoals ?? ag.totalItems ?? 0,
        ag.goalInProgressOverdue ?? ag.inProgressOverdue ?? 0,
        ag.goalInProgressOnTime ?? ag.inProgressOnTime ?? 0,
        ag.goalExpiringSoon ?? ag.expiringSoon ?? 0,
        ag.goalCompletedOverdue ?? ag.completedOverdue ?? 0,
        ag.goalCompletedOnTime ?? ag.completedOnTime ?? 0,
        ag.goalNotStarted ?? ag.notStarted ?? 0,
        contactStr
      ]);
    } else if (isTasksOnly) {
      summaryData.push([
        idx + 1, agencyTypeLabel, ag.name,
        ag.totalTasks ?? ag.totalItems ?? 0,
        ag.taskInProgressOverdue ?? ag.inProgressOverdue ?? 0,
        ag.taskInProgressOnTime ?? ag.inProgressOnTime ?? 0,
        ag.taskExpiringSoon ?? ag.expiringSoon ?? 0,
        ag.taskCompletedOverdue ?? ag.completedOverdue ?? 0,
        ag.taskCompletedOnTime ?? ag.completedOnTime ?? 0,
        ag.taskNotStarted ?? ag.notStarted ?? 0,
        contactStr
      ]);
    } else {
      summaryData.push([
        idx + 1, agencyTypeLabel, ag.name,
        ag.totalItems || 0, ag.totalGoals || 0, ag.totalTasks || 0,
        ag.inProgressOverdue || 0, ag.inProgressOnTime || 0, ag.expiringSoon || 0,
        ag.completedOverdue || 0, ag.completedOnTime || 0, ag.notStarted || 0,
        contactStr
      ]);
    }
  });

  const headerRowIdxSheet1 = isGoalsOnly || isTasksOnly ? 7 : 9;
  const wsSummary = XLSX.utils.aoa_to_sheet(summaryData);
  const numColsSheet1 = headerRowSheet1.length;

  wsSummary['!merges'] = [
    { s: { r: 0, c: 0 }, e: { r: 0, c: numColsSheet1 - 1 } },
    { s: { r: 1, c: 0 }, e: { r: 1, c: numColsSheet1 - 1 } },
    { s: { r: 3, c: 0 }, e: { r: 3, c: numColsSheet1 - 1 } },
    { s: { r: headerRowIdxSheet1 - 1, c: 0 }, e: { r: headerRowIdxSheet1 - 1, c: numColsSheet1 - 1 } }
  ];

  if (isGoalsOnly || isTasksOnly) {
    wsSummary['!cols'] = [
      { wch: 6 },  // STT
      { wch: 15 }, // Loại hình
      { wch: 40 }, // Tên Bộ / Ngành / Địa phương
      { wch: 18 }, // Số lượng Mục tiêu/Nhiệm vụ
      { wch: 18 }, // Đang T/H quá hạn
      { wch: 18 }, // Đang T/H trong hạn
      { wch: 16 }, // Sắp tới hạn
      { wch: 16 }, // Đã H/T quá hạn
      { wch: 16 }, // Đã H/T trong hạn
      { wch: 16 }, // Chưa thực hiện
      { wch: 35 }  // Cán bộ đầu mối chính
    ];
  } else {
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
  }

  styleWorksheet(wsSummary, { numCols: numColsSheet1, headerRowIndex: headerRowIdxSheet1, titleRowIndex: 0 }, XLSX);
  XLSX.utils.book_append_sheet(wb, wsSummary, "TỔNG HỢP CHUNG");

}
