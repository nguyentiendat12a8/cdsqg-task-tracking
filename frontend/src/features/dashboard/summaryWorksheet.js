import { styleWorksheet } from '../../utils/worksheetStyle';
import { activeDocument } from '../../services/documentContext';

export function appendSummaryWorksheet({ XLSX, wb, isGoalsOnly, isTasksOnly, filterNameLabel, timeStr, allFilteredAgencies, metrics }) {
  const doc = activeDocument.value;
  const docType = (doc.shortTitle || 'Chiến Lược').toUpperCase();
  const docName = (doc.name || 'Quyết định số 1266/QĐ-TTg').toUpperCase();
  const reportTitle = isGoalsOnly
    ? `BÁO CÁO TỔNG HỢP TIẾN ĐỘ THỰC HIỆN MỤC TIÊU ${docType} - ${docName}`
    : (isTasksOnly
        ? `BÁO CÁO TỔNG HỢP TIẾN ĐỘ THỰC HIỆN NHIỆM VỤ ${docType} - ${docName}`
        : `BÁO CÁO TỔNG HỢP TIẾN ĐỘ THỰC HIỆN MỤC TIÊU & NHIỆM VỤ ${docType} - ${docName}`);

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
      "Chưa thực hiện", "Đang thực hiện", "Đã hoàn thành",
      "Cán bộ đầu mối chính"
    ];
  } else if (isTasksOnly) {
    headerRowSheet1 = [
      "STT", "Loại hình", "Tên Bộ / Ngành / Địa phương", "Số lượng Nhiệm vụ",
      "Chưa thực hiện", "Đang thực hiện", "Đã hoàn thành",
      "Cán bộ đầu mối chính"
    ];
  } else {
    headerRowSheet1 = [
      "STT", "Loại hình", "Tên Bộ / Ngành / Địa phương", "Tổng số", "Mục tiêu", "Nhiệm vụ",
      "Chưa thực hiện", "Đang thực hiện", "Đã hoàn thành",
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
        ag.goalNotStarted ?? ag.notStarted ?? 0,
        ag.goalInProgress ?? ag.inProgress ?? 0,
        ag.goalCompleted ?? ag.completed ?? 0,
        contactStr
      ]);
    } else if (isTasksOnly) {
      summaryData.push([
        idx + 1, agencyTypeLabel, ag.name,
        ag.totalTasks ?? ag.totalItems ?? 0,
        ag.taskNotStarted ?? ag.notStarted ?? 0,
        ag.taskInProgress ?? ag.inProgress ?? 0,
        ag.taskCompleted ?? ag.completed ?? 0,
        contactStr
      ]);
    } else {
      summaryData.push([
        idx + 1, agencyTypeLabel, ag.name,
        ag.totalItems || 0, ag.totalGoals || 0, ag.totalTasks || 0,
        ag.notStarted || 0, ag.inProgress || 0, ag.completed || 0,
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

  wsSummary['!cols'] = headerRowSheet1.map((_, index) => ({ wch: index === 0 ? 6 : index === 2 ? 40 : index === headerRowSheet1.length - 1 ? 35 : 20 }));

  styleWorksheet(wsSummary, { numCols: numColsSheet1, headerRowIndex: headerRowIdxSheet1, titleRowIndex: 0 }, XLSX);
  XLSX.utils.book_append_sheet(wb, wsSummary, "TỔNG HỢP CHUNG");

}
