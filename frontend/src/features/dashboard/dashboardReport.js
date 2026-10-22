import { activeDocument } from '../../services/documentContext';
import { toast } from 'vue3-toastify';
import { loadExcelWriter } from '../../utils/excelRuntime';
import { fetchWithAuth } from '../../services/auth';
import { getApiUrl } from '../../config/api';
import { buildDashboardQuery } from './queries';
import { buildDashboardWorkbook } from './dashboardWorkbook';

export async function exportDashboardReport(options) {

  options.isExportingExcel.value = true;
  // Capture filters before asynchronous work, so changing the UI cannot mix decisions.
  const document = { ...activeDocument.value };
  const filters = { documentId: document.id };
  for (const key of ['assignmentRole', 'dashboardFilter', 'selectedScopes', 'selectedSections', 'selectedGroups', 'fromYear', 'toYear', 'isOngoingOnly', 'reportFromYear', 'reportToYear', 'executionFrom', 'executionTo']) {
    const value = options[key]?.value;
    filters[key] = Array.isArray(value) ? [...value] : value;
  }
  const metricsQuery = options.metricsQuery?.value || buildDashboardQuery(filters).toString();
  const now = new Date();
  try {
    toast.info('Đang tải đầy đủ dữ liệu để xuất báo cáo Excel...', { autoClose: 2000 });
    async function read(query, endpoint = 'metrics') {
      const response = await fetchWithAuth(getApiUrl(`/api/dashboard/${endpoint}?${query}`));
      if (!response.ok) throw new Error('Không tải đủ dữ liệu báo cáo. Vui lòng thử lại.');
      return response.json();
    }
    const XLSX = await loadExcelWriter();
    const metrics = await read(metricsQuery);
    const agencies = options.filterOutSpecialAgencies([
      ...(metrics.ministriesPerformance || []), ...(metrics.provincesPerformance || []), ...(metrics.othersPerformance || [])
    ]).sort((a, b) => (b.totalItems || 0) - (a.totalItems || 0) || a.name.localeCompare(b.name, 'vi'));
    const details = new Array(agencies.length);
    let next = 0;
    // Four concurrent requests, without the old subordinate-metrics request per unit.
    await Promise.all(Array.from({ length: Math.min(4, agencies.length) }, async () => {
      while (next < agencies.length) {
        const index = next++;
        const agency = agencies[index];
        const items = await read(buildDashboardQuery(filters, { agencyIds: [agency.agencyId] }), 'agency-items');
        if (!Array.isArray(items)) throw new Error('Dữ liệu chi tiết không hợp lệ.');
        details[index] = items.map(item => ({ ...item, reportAgency: agency }));
      }
    }));
    const workbook = buildDashboardWorkbook({ XLSX, document, filters, metrics, agencies, items: details.flat(), now });
    const date = new Intl.DateTimeFormat('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).format(now);
    XLSX.writeFile(workbook, `Bao_cao_trang_chu_${document.code}_${date}.xlsx`);
    toast.success('Đã xuất báo cáo Excel gồm 4 sheet.');
  } catch (error) {
    console.error('Lỗi xuất báo cáo trang chủ:', error);
    toast.error(`Không thể xuất Excel: ${error.message || error}`);
  } finally {
    options.isExportingExcel.value = false;
  }
}
