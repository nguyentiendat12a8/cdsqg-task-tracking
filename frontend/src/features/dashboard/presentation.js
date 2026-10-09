import { formatPercent } from '../../shared/formatters';
export { formatDate } from '../../shared/formatters';
export { getStatusLabel, getStatusBadgeClass } from '../../shared/statusPresentation';
export { isGeneralTaskItem } from '../../shared/agencyPresentation';
export function formatItemProgressDisplay(item) {
  if (!item) return '—';
  if (item.latestProgressPercent != null) return formatPercent(item.latestProgressPercent);

  const unit = item.unitName || item.unit?.name;

  if (unit === 'Số lượng') {
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      return `${item.latestProgressValue}`;
    }
  }

  if (unit && unit !== '%' && unit !== 'Số lượng') {
    if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
      return `${item.latestProgressValue} ${unit}`;
    }
  }

  if ((!unit || unit === '%') && item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    return `${item.latestProgressValue}%`;
  }

  if (item.latestProgressPercent !== null && item.latestProgressPercent !== undefined) {
    return `${item.latestProgressPercent}%`;
  }

  return '—';
}


