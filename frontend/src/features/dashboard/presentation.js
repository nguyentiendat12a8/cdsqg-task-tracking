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
  if (item.latestProgressPercent != null) return formatPercent(item.latestProgressPercent);

  const unit = item.unitName || item.unit?.name;

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


