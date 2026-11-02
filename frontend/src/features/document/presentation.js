import { formatPercent, formatNumber, formatMeasuredValue } from '../../shared/formatters';
import { getStatusLabel } from '../../shared/statusPresentation';
export { formatDate, formatDateTime, formatDateRange, formatFileName } from '../../shared/formatters';
export { getStatusLabel, getStatusBadgeClass } from '../../shared/statusPresentation';
export { isGeneralTaskItem as isGeneralTaskOrAllAgencies } from '../../shared/agencyPresentation';
export function formatProgressDisplay(item) {
  if (!item) return '—';
  if (item.itemType === 'Task' || item.itemType === 2 || item.itemType === '2') {
    return getStatusLabel(item.calculatedStatus || item.latestProgressStatus || 'NotStarted');
  }

  const target = item.expectedBaselineTarget ?? item.targetValue;
  const unit = item.unitName || item.unit?.name || '%';
  const isPercentUnit = !unit || unit === '%' || unit === 'PERCENT';

  if (target != null) {
    const targetStr = isPercentUnit ? `${formatNumber(target)}%` : `${formatNumber(target)}${unit ? ' ' + unit : ''}`;
    let actualStr = null;
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      actualStr = isPercentUnit ? `${formatNumber(item.latestProgressValue)}%` : formatNumber(item.latestProgressValue);
    } else if (item.completionPercentage !== null && item.completionPercentage !== undefined) {
      actualStr = formatPercent(item.completionPercentage);
    }
    if (actualStr != null) {
      return `Đạt ${actualStr} / KH ${targetStr}`;
    }
    return `Chưa báo cáo / KH ${targetStr}`;
  }

  if (item.completionPercentage !== null && item.completionPercentage !== undefined) {
    return formatPercent(item.completionPercentage);
  }

  if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    const unit = item.unitName || item.unit?.name || '%';
    if (unit === 'Số lượng') {
      return formatNumber(item.latestProgressValue);
    }
    return formatMeasuredValue(item.latestProgressValue, unit);
  }

  if (item.evaluationType === 'Qualitative' || item.evaluationType === 2 || item.evaluationType === '2') {
    if (item.latestProgressStatus) {
      const statusMap = {
        'Completed': 'Đã hoàn thành / Ban hành',
        '4': 'Đã hoàn thành / Ban hành',
        'Reviewing': 'Đang xin ý kiến / Thẩm định',
        '3': 'Đang xin ý kiến / Thẩm định',
        'Submitted': 'Đang xin ý kiến / Thẩm định',
        'Drafting': 'Đang xây dựng / Soạn thảo',
        '2': 'Đang xây dựng / Soạn thảo',
        'NotStarted': 'Chưa thực hiện',
        '1': 'Chưa thực hiện'
      };
      return statusMap[item.latestProgressStatus] || item.latestProgressStatus;
    }
    return 'Chưa cập nhật';
  }

  if (item.latestProgressStatus) {
    return item.latestProgressStatus;
  }

  return 'Chưa cập nhật';
}


