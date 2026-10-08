import { fetchWithAuth } from '../../services/auth';
import { getApiUrl } from '../../config/api';
import { buildDashboardQuery } from './queries';

export async function loadAgencyReportData({ agencyId, filters, filterOutSpecialAgencies }) {
  const subQuery = buildDashboardQuery(filters, { parentAgencyId: agencyId });
  const itemsQuery = buildDashboardQuery(filters, { agencyIds: [agencyId], includeItemType: false });
  async function read(path) {
    const response = await fetchWithAuth(getApiUrl(path));
    if (!response.ok) throw new Error('Không tải đủ dữ liệu đơn vị để xuất báo cáo. Vui lòng thử lại.');
    return response.json();
  }
  const [subData, agencyItems] = await Promise.all([
    read(`/api/dashboard/metrics?${subQuery}`),
    read(`/api/dashboard/agency-items?${itemsQuery}`)
  ]);
  return {
    subAgencies: filterOutSpecialAgencies([
      ...(subData.ministriesPerformance || []),
      ...(subData.provincesPerformance || []),
      ...(subData.othersPerformance || [])
    ]),
    agencyItems
  };
}
