import { formatPercent, formatNumber, formatMeasuredValue } from '../../shared/formatters';
import { getStatusLabel } from '../../shared/statusPresentation';
export { formatDate } from '../../shared/formatters';
export { getStatusLabel, getStatusBadgeClass } from '../../shared/statusPresentation';
export { isGeneralTaskItem } from '../../shared/agencyPresentation';
export function formatItemProgressDisplay(item) {
  if (!item) return '—';
  if (item.itemType === 'Task' || item.itemType === 2 || item.itemType === '2') {
    return getStatusLabel(item.calculatedStatus || item.latestProgressStatus || 'NotStarted');
  }

  const unit = item.unitName || item.unit?.name;
  const isPercentUnit = !unit || unit === '%' || unit === 'PERCENT';

  if (item.targetValue != null) {
    const targetStr = isPercentUnit ? `${formatNumber(item.targetValue)}%` : `${formatNumber(item.targetValue)}${unit ? ' ' + unit : ''}`;
    let actualStr = null;
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      actualStr = isPercentUnit ? `${formatNumber(item.latestProgressValue)}%` : formatNumber(item.latestProgressValue);
    } else if (item.latestProgressPercent !== null && item.latestProgressPercent !== undefined) {
      actualStr = formatPercent(item.latestProgressPercent);
    }

    if (actualStr != null) {
      return `Đạt ${actualStr} / KH ${targetStr}`;
    }
    return `Chưa báo cáo / KH ${targetStr}`;
  }

  if (item.latestProgressPercent != null) return formatPercent(item.latestProgressPercent);

  if (unit === 'Số lượng') {
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      return formatNumber(item.latestProgressValue);
    }
  }

  if (unit && unit !== '%' && unit !== 'Số lượng') {
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      return formatMeasuredValue(item.latestProgressValue, unit);
    }
  }

  if ((!unit || unit === '%') && item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    return formatPercent(item.latestProgressValue);
  }

  if (item.latestProgressPercent !== null && item.latestProgressPercent !== undefined) {
    return formatPercent(item.latestProgressPercent);
  }

  return '—';
}


