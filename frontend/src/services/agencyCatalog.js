import { fetchWithAuth, authState } from './auth';
import { getApiUrl } from '../config/api';

// Danh mục cơ quan (~40 KB) được nhiều màn hình/modal dùng chung. Giữ một bản trong bộ nhớ
// theo phiên đăng nhập để không tải lại mỗi lần mở modal hay đổi bộ lọc.
// Tự hết hạn sau 5 phút và bị xóa ngay khi có thao tác ghi vào /api/agencies hoặc /api/units.
const TTL_MS = 5 * 60 * 1000;
let cache = null; // { token, at, promise }

export const AGENCY_CATALOG_STALE_EVENT = 'agency-catalog-stale';

export function invalidateAgencyCatalog() { cache = null; }

if (typeof window !== 'undefined') window.addEventListener(AGENCY_CATALOG_STALE_EVENT, invalidateAgencyCatalog);

function normalize(data) { return Array.isArray(data) ? data : (data?.items || []); }

/** Trả về bản sao danh sách cơ quan (mảng). Ném lỗi nếu máy chủ không trả 2xx. */
export async function loadAgencyCatalog({ force = false } = {}) {
  const token = authState.token.value;
  const fresh = cache && cache.token === token && Date.now() - cache.at < TTL_MS;
  if (force || !fresh) {
    const promise = fetchWithAuth(getApiUrl('/api/agencies')).then(async res => {
      if (!res.ok) throw new Error(`Không tải được danh mục cơ quan (${res.status}).`);
      return normalize(await res.json());
    });
    cache = { token, at: Date.now(), promise };
    promise.catch(() => { if (cache?.promise === promise) cache = null; });
  }
  const list = await cache.promise;
  // Bản sao để component có thể chỉnh sửa mảng/đối tượng mà không ảnh hưởng màn hình khác.
  return structuredClone(list);
}
