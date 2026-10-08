import { styleWorksheet } from '../../utils/worksheetStyle';
import { loadAgencyReportData } from './reportData';

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

export async function appendAgencyWorksheet({ XLSX, wb, ag, usedSheetNames, filters, filterOutSpecialAgencies, isGoalsOnly, isTasksOnly, filterNameLabel, timeStr, agencies, formatDate, formatItemProgressDisplay }) {
  let rawSheetName = (ag.name || ag.code || 'Don_Vi').replace(/[\/\\?*:[\]]/g, '').trim();
  if (rawSheetName.length > 28) rawSheetName = rawSheetName.substring(0, 28);
  let sheetName = rawSheetName;
  let counter = 1;
  while (usedSheetNames.has(sheetName)) {
    sheetName = `${rawSheetName.substring(0, 25)}_${counter++}`;
  }
  usedSheetNames.add(sheetName);

  const { subAgencies, agencyItems } = await loadAgencyReportData({ agencyId: ag.agencyId, filters, filterOutSpecialAgencies });

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

  const sheetAgencyHeaderTitle = isGoalsOnly
    ? `BÁO CÁO CHI TIẾT THỰC HIỆN MỤC TIÊU CHIẾN LƯỢC - ${ag.name.toUpperCase()}`
    : (isTasksOnly
        ? `BÁO CÁO CHI TIẾT THỰC HIỆN NHIỆM VỤ CHIẾN LƯỢC - ${ag.name.toUpperCase()}`
        : `BÁO CÁO CHI TIẾT THỰC HIỆN MỤC TIÊU & NHIỆM VỤ CHIẾN LƯỢC - ${ag.name.toUpperCase()}`);

  const sheetRows = [
    [sheetAgencyHeaderTitle],
    [`Thời gian xuất: ${timeStr} | Loại hình: ${ag.type === 'Ministry' ? 'Bộ / Ngành' : 'Địa phương'}`],
    [],
    ["1. THÔNG TIN CHUNG VÀ TỔNG HỢP TIẾN ĐỘ THỰC HIỆN CỦA ĐƠN VỊ"],
    ["Tên đơn vị:", ag.name],
    ["Cán bộ đầu mối chính:", mainContactStr],
    ["Đơn vị trực thuộc:", subAgencies.length > 0 ? `${subAgencies.length} đơn vị trực thuộc` : "Không có đơn vị trực thuộc"],
    []
  ];

  // Table 1: Agency Progress Summary
  if (isGoalsOnly) {
    sheetRows.push(["BẢNG TỔNG HỢP TRẠNG THÁI TIẾN ĐỘ MỤC TIÊU CỦA ĐƠN VỊ"]);
    sheetRows.push([
      "Số lượng Mục tiêu", "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
      "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
    ]);
    sheetRows.push([
      ag.totalGoals ?? ag.totalItems ?? 0,
      ag.goalInProgressOverdue ?? ag.inProgressOverdue ?? 0,
      ag.goalInProgressOnTime ?? ag.inProgressOnTime ?? 0,
      ag.goalExpiringSoon ?? ag.expiringSoon ?? 0,
      ag.goalCompletedOverdue ?? ag.completedOverdue ?? 0,
      ag.goalCompletedOnTime ?? ag.completedOnTime ?? 0,
      ag.goalNotStarted ?? ag.notStarted ?? 0
    ]);
  } else if (isTasksOnly) {
    sheetRows.push(["BẢNG TỔNG HỢP TRẠNG THÁI TIẾN ĐỘ NHIỆM VỤ CỦA ĐƠN VỊ"]);
    sheetRows.push([
      "Số lượng Nhiệm vụ", "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
      "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
    ]);
    sheetRows.push([
      ag.totalTasks ?? ag.totalItems ?? 0,
      ag.taskInProgressOverdue ?? ag.inProgressOverdue ?? 0,
      ag.taskInProgressOnTime ?? ag.inProgressOnTime ?? 0,
      ag.taskExpiringSoon ?? ag.expiringSoon ?? 0,
      ag.taskCompletedOverdue ?? ag.completedOverdue ?? 0,
      ag.taskCompletedOnTime ?? ag.completedOnTime ?? 0,
      ag.taskNotStarted ?? ag.notStarted ?? 0
    ]);
  } else {
    sheetRows.push(["BẢNG TỔNG HỢP TRẠNG THÁI TIẾN ĐỘ CỦA ĐƠN VỊ"]);
    sheetRows.push([
      "Tổng số hạng mục", "Mục tiêu", "Nhiệm vụ",
      "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
      "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
    ]);
    sheetRows.push([
      ag.totalItems || 0, ag.totalGoals || 0, ag.totalTasks || 0,
      ag.inProgressOverdue || 0, ag.inProgressOnTime || 0, ag.expiringSoon || 0,
      ag.completedOverdue || 0, ag.completedOnTime || 0, ag.notStarted || 0
    ]);
  }
  sheetRows.push([]);

  const maxColsAgency = 11;
  const merges = [
    { s: { r: 0, c: 0 }, e: { r: 0, c: maxColsAgency - 1 } },
    { s: { r: 1, c: 0 }, e: { r: 1, c: maxColsAgency - 1 } },
    { s: { r: 3, c: 0 }, e: { r: 3, c: maxColsAgency - 1 } },
    { s: { r: 8, c: 0 }, e: { r: 8, c: maxColsAgency - 1 } }
  ];

  let secCounter = 2;

  // Table 2: Sub-agencies progress summary
  if (subAgencies.length > 0) {
    const rowIdx = sheetRows.length;
    merges.push({ s: { r: rowIdx, c: 0 }, e: { r: rowIdx, c: maxColsAgency - 1 } });

    if (isGoalsOnly) {
      sheetRows.push([`${secCounter}. TỔNG SỐ MỤC TIÊU THEO TRẠNG THÁI CỦA TỪNG ĐƠN VỊ TRỰC THUỘC`]);
      sheetRows.push([
        "STT", "Tên Đơn Vị Trực Thuộc", "Số Lượng Mục Tiêu",
        "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
        "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
      ]);
      subAgencies.forEach((sub, sIdx) => {
        sheetRows.push([
          sIdx + 1, sub.name, sub.totalGoals ?? sub.totalItems ?? 0,
          sub.goalInProgressOverdue ?? sub.inProgressOverdue ?? 0,
          sub.goalInProgressOnTime ?? sub.inProgressOnTime ?? 0,
          sub.goalExpiringSoon ?? sub.expiringSoon ?? 0,
          sub.goalCompletedOverdue ?? sub.completedOverdue ?? 0,
          sub.goalCompletedOnTime ?? sub.completedOnTime ?? 0,
          sub.goalNotStarted ?? sub.notStarted ?? 0
        ]);
      });
    } else if (isTasksOnly) {
      sheetRows.push([`${secCounter}. TỔNG SỐ NHIỆM VỤ THEO TRẠNG THÁI CỦA TỪNG ĐƠN VỊ TRỰC THUỘC`]);
      sheetRows.push([
        "STT", "Tên Đơn Vị Trực Thuộc", "Số Lượng Nhiệm Vụ",
        "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
        "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
      ]);
      subAgencies.forEach((sub, sIdx) => {
        sheetRows.push([
          sIdx + 1, sub.name, sub.totalTasks ?? sub.totalItems ?? 0,
          sub.taskInProgressOverdue ?? sub.inProgressOverdue ?? 0,
          sub.taskInProgressOnTime ?? sub.inProgressOnTime ?? 0,
          sub.taskExpiringSoon ?? sub.expiringSoon ?? 0,
          sub.taskCompletedOverdue ?? sub.completedOverdue ?? 0,
          sub.taskCompletedOnTime ?? sub.completedOnTime ?? 0,
          sub.taskNotStarted ?? sub.notStarted ?? 0
        ]);
      });
    } else {
      sheetRows.push([`${secCounter}. TỔNG SỐ MỤC TIÊU, NHIỆM VỤ THEO TRẠNG THÁI CỦA TỪNG ĐƠN VỊ TRỰC THUỘC`]);
      sheetRows.push([
        "STT", "Tên Đơn Vị Trực Thuộc", "Tổng Số", "Mục Tiêu", "Nhiệm Vụ",
        "Đang T/H quá hạn", "Đang T/H trong hạn", "Sắp tới hạn",
        "Đã H/T quá hạn", "Đã H/T trong hạn", "Chưa thực hiện"
      ]);
      subAgencies.forEach((sub, sIdx) => {
        sheetRows.push([
          sIdx + 1, sub.name, sub.totalItems || 0, sub.totalGoals || 0, sub.totalTasks || 0,
          sub.inProgressOverdue || 0, sub.inProgressOnTime || 0, sub.expiringSoon || 0,
          sub.completedOverdue || 0, sub.completedOnTime || 0, sub.notStarted || 0
        ]);
      });
    }
    secCounter++;
    sheetRows.push([]);
  }

  // Goals List Table (Include if isGoalsOnly or isAll)
  if (isGoalsOnly || !isTasksOnly) {
    const goalRowIdx = sheetRows.length;
    merges.push({ s: { r: goalRowIdx, c: 0 }, e: { r: goalRowIdx, c: maxColsAgency - 1 } });
    sheetRows.push([`${secCounter}. DANH SÁCH MỤC TIÊU CỦA ĐƠN VỊ (${goalsList.length} mục tiêu)`]);
    if (goalsList.length > 0) {
      sheetRows.push([
        "STT", "Mã Mục Tiêu", "Tên Mục Tiêu", "Cơ Quan Chủ Trì", "Giao Đơn Vị Trực Thuộc", "Phạm Vi", "Lĩnh Vực / Nhóm",
        "Thời Gian / Hạn Chót", "Tiến Độ Hiện Tại", "Trạng Thái Thực Hiện"
      ]);
      goalsList.forEach((g, gIdx) => {
        let dateStr = g.isOngoing ? 'Hằng năm' : (g.dueDate ? formatDate(g.dueDate) : '—');
        let progStr = formatItemProgressDisplay(g);

        sheetRows.push([
          gIdx + 1, g.code || '—', g.title, g.leadAgencyName, g.assignedAgencyName || '—',
          g.isGeneralTask ? 'Phạm vi chung' : 'Phạm vi riêng',
          [g.section, g.group].filter(Boolean).join(' - ') || '—',
          dateStr, progStr, getStatusLabelClean(g.status)
        ]);
      });
    } else {
      sheetRows.push(["Không có mục tiêu nào trong bộ lọc hiện tại."]);
    }
    secCounter++;
    sheetRows.push([]);
  }

  // Tasks List Table (Include if isTasksOnly or isAll)
  if (isTasksOnly || !isGoalsOnly) {
    const taskRowIdx = sheetRows.length;
    merges.push({ s: { r: taskRowIdx, c: 0 }, e: { r: taskRowIdx, c: maxColsAgency - 1 } });
    sheetRows.push([`${secCounter}. DANH SÁCH NHIỆM VỤ CỦA ĐƠN VỊ (${tasksList.length} nhiệm vụ)`]);
    if (tasksList.length > 0) {
      sheetRows.push([
        "STT", "Mã Nhiệm Vụ", "Tên Nhiệm Vụ", "Cơ Quan Chủ Trì", "Giao Đơn Vị Trực Thuộc", "Phạm Vi", "Lĩnh Vực / Nhóm",
        "Thời Gian / Hạn Chót", "Trạng Thái Thực Hiện"
      ]);
      tasksList.forEach((t, tIdx) => {
        let dateStr = t.isOngoing ? 'Hằng năm' : (t.dueDate ? formatDate(t.dueDate) : '—');

        sheetRows.push([
          tIdx + 1, t.code || '—', t.title, t.leadAgencyName, t.assignedAgencyName || '—',
          t.isGeneralTask ? 'Phạm vi chung' : 'Phạm vi riêng',
          [t.section, t.group].filter(Boolean).join(' - ') || '—',
          dateStr, getStatusLabelClean(t.status)
        ]);
      });
    } else {
      sheetRows.push(["Không có nhiệm vụ nào trong bộ lọc hiện tại."]);
    }
    secCounter++;
    sheetRows.push([]);
  }

  // Contact Persons Table
  const contactRowIdx = sheetRows.length;
  merges.push({ s: { r: contactRowIdx, c: 0 }, e: { r: contactRowIdx, c: maxColsAgency - 1 } });
  sheetRows.push([`${secCounter}. DANH SÁCH CÁN BỘ ĐẦU MỐI LIÊN HỆ (${contactList.length} cán bộ)`]);
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
    { wch: 15 }, // Mã / Họ tên
    { wch: 45 }, // Tên Hạng Mục / Tên Đơn Vị
    { wch: 25 }, // Cơ Quan Chủ Trì / Chức Danh
    { wch: 25 }, // Giao Đơn Vị Trực Thuộc / Phòng ban
    { wch: 18 }, // Phạm Vi / Điện thoại
    { wch: 25 }, // Lĩnh Vực - Nhóm / Email
    { wch: 20 }, // Thời Gian / Thuộc đơn vị
    { wch: 18 }, // Tiến Độ
    { wch: 18 }  // Trạng Thái
  ];

  styleWorksheet(ws, { numCols: maxColsAgency, headerRowIndex: 9, titleRowIndex: 0 }, XLSX);
  XLSX.utils.book_append_sheet(wb, ws, sheetName);
}
