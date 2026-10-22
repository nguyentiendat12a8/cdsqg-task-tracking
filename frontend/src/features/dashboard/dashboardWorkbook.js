import { styleWorksheet } from '../../utils/worksheetStyle';
import { getStatusLabel } from '../../shared/statusPresentation';

const percent = value => value == null ? null : value / 100;
const blockName = type => type === 'Ministry' ? 'Bộ, ngành' : type === 'Province' ? 'Địa phương' : 'Đơn vị khác';
const dateValue = value => {
  const parts = value && String(value).match(/^(\d{4})-(\d{2})-(\d{2})/);
  return parts ? new Date(Number(parts[1]), Number(parts[2]) - 1, Number(parts[3])) : null;
};
const assignmentTotal = summary => summary && ['notStarted', 'inProgress', 'completed'].every(k => Number.isFinite(summary[k]))
  ? summary.notStarted + summary.inProgress + summary.completed : null;

// Presentation only. Statuses, progress and completion rates are calculated by the API.
export function buildDashboardWorkbook({ XLSX, document, filters, metrics, agencies, items, now }) {
  const wb = XLSX.utils.book_new();
  const time = now.toLocaleString('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh' });
  const metadata = `${document.documentNumber} | ${document.title} | Xuất: ${time} (UTC+7)`;
  const bounds = (from, to) => `${from || 'Không giới hạn'} → ${to || 'Không giới hạn'}`;
  const filterText = `Báo cáo: ${bounds(filters.reportFromYear, filters.reportToYear)}; Thực hiện: ${bounds(filters.executionFrom, filters.executionTo)}; Thường xuyên: ${filters.isOngoingOnly ? 'Có' : 'Tất cả'}`;
  const hasFilters = ['reportFromYear', 'reportToYear', 'executionFrom', 'executionTo', 'fromYear', 'toYear'].some(key => filters[key] != null && filters[key] !== '') || filters.isOngoingOnly || [filters.selectedScopes, filters.selectedSections, filters.selectedGroups].some(value => value?.length) || (filters.dashboardFilter && filters.dashboardFilter !== 'all');
  function sheet(name, headers, rows, widths, formats = {}, note = '') {
    const opening = [[name.toLocaleUpperCase('vi-VN')], [metadata], ...(hasFilters ? [[filterText]] : []), [note]];
    const headerRow = opening.length;
    const data = [...opening, headers, ...rows];
    const ws = XLSX.utils.aoa_to_sheet(data, { dateNF: 'dd/mm/yyyy' });
    ws['!cols'] = widths.map(wch => ({ wch }));
    ws['!merges'] = opening.map((_, r) => ({ s: { r, c: 0 }, e: { r, c: headers.length - 1 } }));
    styleWorksheet(ws, { numCols: headers.length, headerRowIndex: headerRow, titleRowIndex: 0 }, XLSX);
    ws['!autofilter'] = { ref: XLSX.utils.encode_range({ s: { r: headerRow, c: 0 }, e: { r: Math.max(headerRow, data.length - 1), c: headers.length - 1 } }) };
    for (let r = headerRow + 1; r < data.length; r++) for (let c = 0; c < headers.length; c++) {
      const cell = ws[XLSX.utils.encode_cell({ r, c })];
      if (cell?.t === 'n') cell.z = formats[c] || '#,##0';
    }
    ws['!rows'] ||= [];
    ws['!rows'][headerRow] = { hpt: 40 };
    for (let r = headerRow + 1; r < data.length; r++) {
      const lines = Math.max(1, ...data[r].map((value, c) => typeof value === 'string'
        ? value.split('\n').reduce((sum, line) => sum + Math.max(1, Math.ceil(line.length / Math.max(1, widths[c] * .85))), 0) : 1));
      ws['!rows'][r] = { hpt: Math.max(26, lines * 15 + 8) };
    }
    ws['!rows'][1] = { hpt: 36 };
    for (let r = 2; r < headerRow; r++) ws['!rows'][r] = { hpt: 30 };
    XLSX.utils.book_append_sheet(wb, ws, name);
  }
  const overview = metrics.overviewMetrics || {};
  const states = (label, total, summary) => [label, total ?? null, summary?.notStarted ?? null, summary?.inProgress ?? null, summary?.completed ?? null];
  sheet('Tổng hợp', ['Chỉ tiêu', 'Tổng số', 'Chưa thực hiện', 'Đang thực hiện', 'Đã hoàn thành'], [
    states('Mục tiêu (không lặp)', overview.totalGoals, overview.goalStatusSummary),
    states('Nhiệm vụ (không lặp)', overview.totalTasks, overview.taskStatusSummary),
    states('Lượt giao mục tiêu', assignmentTotal(metrics.goalStatusSummary), metrics.goalStatusSummary),
    states('Lượt giao nhiệm vụ', assignmentTotal(metrics.taskStatusSummary), metrics.taskStatusSummary),
    ['Đơn vị trong báo cáo', agencies.length],
    ...(metrics.groupStatistics || []).map(g => states(`${g.itemType === 'Goal' ? 'Mục tiêu' : 'Nhiệm vụ'} / ${g.group}`, g.total, g))
  ], [54, 20, 22, 22, 22], {}, 'Số mục tiêu/nhiệm vụ không lặp khác số lượt giao. Chi tiết: mỗi dòng là một mục tiêu/nhiệm vụ giao cho một đơn vị.');
  sheet('So sánh đơn vị', ['STT', 'Tên đơn vị', 'Lượt giao', 'Mục tiêu', 'MT chưa thực hiện', 'MT đang thực hiện', 'MT hoàn thành', 'Nhiệm vụ', 'NV chưa thực hiện', 'NV đang thực hiện', 'NV hoàn thành', 'Tỷ lệ hoàn thành', 'Quá hạn', 'Sắp đến hạn'], agencies.map((a, i) => [
    i + 1, a.name, a.totalItems, a.totalGoals, a.goalNotStarted, a.goalInProgress, a.goalCompleted,
    a.totalTasks, a.taskNotStarted, a.taskInProgress, a.taskCompleted, percent(a.completionRateOverall), a.overdue, a.expiringSoon
  ]), [7, 42, 14, 14, 18, 18, 18, 14, 18, 18, 18, 20, 14, 16], { 11: '0.0%' }, 'Tỷ lệ hoàn thành và các chỉ số lấy từ Dashboard; đơn vị không có lượt giao không có tỷ lệ hoàn thành.');
  const headers = ['STT', 'Mã', 'Tên', 'Đơn vị nhận giao', 'Khối', 'Phạm vi', 'Cơ quan chủ trì', 'Cơ quan phối hợp', 'Mục', 'Nhóm', 'Bắt đầu', 'Kết thúc', 'Thường xuyên', 'Giá trị báo cáo', 'Đơn vị tính', 'Tiến độ', 'Trạng thái', 'Thời hạn', 'Sản phẩm'];
  for (const [type, name] of [['Goal', 'Chi tiết mục tiêu'], ['Task', 'Chi tiết nhiệm vụ']]) {
    const selected = items.filter(item => item.itemType === type);
    sheet(name, headers, selected.map((item, index) => [
      index + 1, item.code, item.title, item.reportAgency.name, blockName(item.reportAgency.type), item.isGeneralTask ? 'Giao chung' : 'Giao riêng',
      item.leadAgencyName, (item.coordinatingAgencyNames || []).join('; '), item.section, item.group,
      dateValue(item.startDate), dateValue(item.dueDate), item.isOngoing ? 'Có' : 'Không', item.latestProgressValue ?? null, item.unitName,
      percent(item.latestProgressPercent), getStatusLabel(item.calculatedStatus || item.status),
      ({ Overdue: 'Quá hạn', ExpiringSoon: 'Sắp đến hạn', OnTime: 'Trong hạn', CompletedOnTime: 'Hoàn thành đúng hạn', CompletedOverdue: 'Hoàn thành quá hạn' })[item.deadlineStatus] || item.deadlineStatus || null,
      (item.deliverables || []).map(d => `${d.name || d.title || ''}${d.currentStatus ? ' (' + getStatusLabel(d.currentStatus) + ')' : ''}`).filter(Boolean).join('\n')
    ]), [7, 16, 65, 40, 18, 16, 40, 40, 24, 34, 16, 16, 18, 20, 18, 16, 22, 20, 55], { 10: 'dd/mm/yyyy', 11: 'dd/mm/yyyy', 13: '#,##0.00', 15: '0.0%' },
    'Mỗi dòng là một lượt giao cho đơn vị; một mã có thể xuất hiện ở nhiều đơn vị. Giá trị trống là chưa có dữ liệu, khác số 0.');
  }
  return wb;
}
