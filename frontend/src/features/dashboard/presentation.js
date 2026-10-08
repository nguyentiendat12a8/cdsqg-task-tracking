export function getStatusBadgeClass(status) {
  switch (status) {
    case 'InProgressOverdue':
      return 'bg-rose-50 text-rose-700 border-rose-200';
    case 'InProgressOnTime':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    case 'ExpiringSoon':
      return 'bg-purple-50 text-purple-700 border-purple-200';
    case 'CompletedOverdue':
      return 'bg-amber-50 text-amber-700 border-amber-200';
    case 'CompletedOnTime':
      return 'bg-blue-50 text-blue-700 border-blue-200';
    case 'NotStarted':
    default:
      return 'bg-slate-100 text-slate-700 border-slate-300';
  }
}

export function getStatusLabel(status) {
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

export function isGeneralTaskItem(item) {
  if (!item) return false;
  if (item.isGeneralTask) return true;
  const code = (item.leadAgencyCode || '').toUpperCase();
  if (code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND' || code === 'ALL_MINISTRIES_DIRECT') return true;
  if (item.leadAgencyId && ['00000000-0000-0000-0000-000000009999', '00000000-0000-0000-0000-000000009998', '00000000-0000-0000-0000-000000009997', '00000000-0000-0000-0000-000000009996', '00000000-0000-0000-0000-000000009995'].includes(String(item.leadAgencyId).toLowerCase())) return true;
  if (item.leadAgencyName && (item.leadAgencyName.toLowerCase().includes('các bộ, ngành') || item.leadAgencyName.toLowerCase().includes('các địa phương'))) return true;
  return false;
}

export function formatDate(dateStr) {
  if (!dateStr) return '—';
  try {
    let str = String(dateStr).trim();
    if (!str) return '—';
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
      const [y, m, d] = str.slice(0, 10).split('-');
      return `${d}/${m}/${y}`;
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return dateStr;
    const day = String(d.getUTCDate()).padStart(2, '0');
    const month = String(d.getUTCMonth() + 1).padStart(2, '0');
    const year = d.getUTCFullYear();
    return `${day}/${month}/${year}`;
  } catch (e) {
    return dateStr || '—';
  }
}

export function formatItemProgressDisplay(item) {
  if (!item) return '—';
  if (item.latestProgressPercent != null) return `${Number(item.latestProgressPercent).toLocaleString('vi-VN', { maximumFractionDigits: 2 })}%`;

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

