import { fetchWithAuth } from './auth';

// The caller supplies the same applied query used by the list; only paging changes.
export async function loadAllExportPages(sourceUrl, { fetchPage = fetchWithAuth, pageSize = 100 } = {}) {
  const url = new URL(sourceUrl);
  url.searchParams.set('pageSize', String(pageSize));
  const rows = [], ids = new Set();
  let expectedCount = null;
  for (let page = 1; ; page++) {
    url.searchParams.set('pageNumber', String(page));
    const response = await fetchPage(new URL(url));
    if (!response.ok) throw new Error('Không tải được dữ liệu xuất Excel. Vui lòng thử lại.');
    const data = await response.json();
    if (Array.isArray(data) && page === 1) return data;
    if (!Array.isArray(data.items) || !Number.isInteger(data.totalCount) || data.totalCount < 0) {
      throw new Error('Dữ liệu phân trang xuất Excel không hợp lệ.');
    }
    if (expectedCount !== null && expectedCount !== data.totalCount) throw new Error('Danh sách đã thay đổi khi xuất Excel. Vui lòng thử lại.');
    expectedCount = data.totalCount;
    for (const row of data.items) {
      if (row.id != null) {
        if (ids.has(row.id)) throw new Error('Dữ liệu xuất Excel bị trùng giữa các trang. Vui lòng thử lại.');
        ids.add(row.id);
      }
      rows.push(row);
    }
    if (rows.length === expectedCount) return rows;
    if (!data.items.length || rows.length > expectedCount) throw new Error('Danh sách đã thay đổi khi xuất Excel. Vui lòng thử lại.');
  }
}
