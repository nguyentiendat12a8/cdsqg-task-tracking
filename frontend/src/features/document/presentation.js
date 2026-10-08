export function formatDate(dateInput) {
  if (!dateInput) return '—';
  try {
    let str = String(dateInput).trim();
    if (!str) return '—';
    if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
      const [y, m, d] = str.slice(0, 10).split('-');
      return `${d}/${m}/${y}`;
    }
    const dateObj = new Date(str);
    if (isNaN(dateObj.getTime())) return str;
    const day = String(dateObj.getUTCDate()).padStart(2, '0');
    const month = String(dateObj.getUTCMonth() + 1).padStart(2, '0');
    const year = dateObj.getUTCFullYear();
    return `${day}/${month}/${year}`;
  } catch {
    return String(dateInput);
  }
}

export function formatDateTime(dateInput) {
  if (!dateInput) return '—';
  try {
    let str = String(dateInput).trim();
    if (!str) return '—';
    if (str.includes('T') && !str.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(str)) {
      str += 'Z';
    }
    const d = new Date(str);
    if (isNaN(d.getTime())) return String(dateInput);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes} ${day}/${month}/${year}`;
  } catch {
    return String(dateInput);
  }
}

export function formatDateRange(sDate, dDate) {
  if (!sDate && !dDate) return '—';
  const s = sDate ? formatDate(sDate) : '...';
  const d = dDate ? formatDate(dDate) : '...';
  return `${s} ➔ ${d}`;
}

export function formatFileName(fullPath) {
  if (!fullPath) return 'Tệp đính kèm';
  const cleanPath = fullPath.split('?')[0];
  const parts = cleanPath.split(/[/\\]/);
  const name = parts[parts.length - 1];
  const guidMatch = name.match(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}_(.+)$/);
  if (guidMatch && guidMatch[1]) {
    return guidMatch[1];
  }
  return name || 'Tệp đính kèm';
}

export function getStatusLabel(st) {
  if (!st) return 'Chưa thực hiện';
  const map = {
    'NotStarted': 'Chưa thực hiện',
    'InProgress': 'Đang thực hiện',
    'InProgressOnTime': 'Đang thực hiện (trong hạn)',
    'InProgressOverdue': 'Đang thực hiện (quá hạn)',
    'Completed': 'Hoàn thành',
    'CompletedOnTime': 'Hoàn thành (đúng hạn)',
    'CompletedOverdue': 'Hoàn thành (quá hạn)',
    'ExpiringSoon': 'Sắp hết hạn',
    'Drafting': 'Đang xây dựng / soạn thảo',
    'Reviewing': 'Đang thẩm định / xin ý kiến',
    'PendingApproval': 'Chờ phê duyệt',
    'Approved': 'Đã phê duyệt',
    'Rejected': 'Bị từ chối'
  };
  return map[st] || st;
}

export function getStatusBadgeClass(st) {
  const map = {
    'NotStarted': 'bg-slate-100 text-slate-700 border border-slate-200',
    'InProgressOnTime': 'bg-blue-50 text-blue-800 border border-blue-200',
    'InProgressOverdue': 'bg-rose-50 text-rose-800 border border-rose-200',
    'CompletedOnTime': 'bg-emerald-50 text-emerald-800 border border-emerald-200',
    'CompletedOverdue': 'bg-teal-50 text-teal-800 border border-teal-200',
    'ExpiringSoon': 'bg-amber-50 text-amber-800 border border-amber-200'
  };
  return map[st] || 'bg-slate-100 text-slate-700';
}

export function formatProgressDisplay(item) {
  if (!item) return '—';

  if (item.completionPercentage !== null && item.completionPercentage !== undefined) {
    return `${Number(item.completionPercentage).toLocaleString('vi-VN', { maximumFractionDigits: 2 })}%`;
  }

  if (item.latestProgressValue !== null && item.latestProgressValue !== undefined) {
    const unit = item.unitName || item.unit?.name || '%';
    if (unit === 'Số lượng') {
      return `${item.latestProgressValue}`;
    }
    return `${item.latestProgressValue} ${unit}`;
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

export function isGeneralTaskOrAllAgencies(item) {
  if (!item) return false;
  if (item.isGeneralTask) return true;
  const code = (item.leadAgencyCode || '').toUpperCase();
  if (code === 'ALL_AGENCIES' || code === 'ALL_MINISTRIES' || code === 'ALL_PROVINCES' || code === 'ALL_PROVINCES_UBND' || code === 'ALL_MINISTRIES_DIRECT') return true;
  if (item.leadAgencyId && ['00000000-0000-0000-0000-000000009999', '00000000-0000-0000-0000-000000009998', '00000000-0000-0000-0000-000000009997', '00000000-0000-0000-0000-000000009996', '00000000-0000-0000-0000-000000009995'].includes(String(item.leadAgencyId).toLowerCase())) return true;
  if (item.leadAgencyName && (item.leadAgencyName.toLowerCase().includes('các bộ, ngành') || item.leadAgencyName.toLowerCase().includes('các địa phương') || item.leadAgencyName.toLowerCase().includes('ubnd tỉnh, thành phố'))) return true;
  return false;
}

