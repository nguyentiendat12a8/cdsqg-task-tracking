import { fetchWithAuth } from './auth';
import { getApiUrl } from '../config/api';

export async function requestJson(path, { method = 'GET', body, params, signal } = {}) {
  const url = new URL(getApiUrl(path));
  for (const [key, value] of Object.entries(params || {})) if (value != null) url.searchParams.set(key, String(value));
  const multipart = body instanceof FormData;
  const response = await fetchWithAuth(url.href, {
    method, signal,
    headers: body != null && !multipart ? { 'Content-Type': 'application/json' } : undefined,
    body: body == null ? undefined : multipart ? body : JSON.stringify(body)
  });
  const data = response.status === 204 ? null : await response.json().catch(() => null);
  if (!response.ok) {
    const error = new Error(data?.message || data?.error || `Không thể xử lý yêu cầu (${response.status}).`);
    error.status = response.status;
    throw error;
  }
  return data;
}
