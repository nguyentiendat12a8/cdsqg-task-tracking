import { ref, computed } from 'vue';
import { getApiUrl } from '../config/api';
import { isAdminRole } from '../shared/roles';

const TOKEN_KEY = 'cdsqg_auth_token';
const USER_KEY = 'cdsqg_auth_user';

const token = ref(localStorage.getItem(TOKEN_KEY) || '');
let storedUser = null;
try { storedUser = JSON.parse(localStorage.getItem(USER_KEY) || 'null'); }
catch { localStorage.removeItem(USER_KEY); localStorage.removeItem(TOKEN_KEY); token.value = ''; }
const user = ref(storedUser);

export const authState = {
  token,
  user,
  isLoggedIn: computed(() => !!token.value && !!user.value),
  isAdmin: computed(() => isAdminRole(user.value?.role))
};

export async function login(username, password) {
  const res = await fetch(getApiUrl('/api/auth/login'), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ username, password })
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.message || 'Đăng nhập không thành công.');
  }

  const data = await res.json();
  token.value = data.token;
  user.value = data.user;

  localStorage.setItem(TOKEN_KEY, data.token);
  localStorage.setItem(USER_KEY, JSON.stringify(data.user));

  return data.user;
}

export function logout() {
  token.value = '';
  user.value = null;
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function getAuthHeaders() {
  const headers = {};
  if (token.value) {
    headers['Authorization'] = `Bearer ${token.value}`;
  }
  return headers;
}

export async function fetchWithAuth(url, options = {}) {
  const target = new URL(url instanceof Request ? url.url : url, window.location.href);
  const api = new URL(getApiUrl());
  const isApi = target.origin === api.origin && target.pathname.startsWith('/api/');
  const headers = new Headers(options.headers || (url instanceof Request ? url.headers : undefined));
  if (isApi && token.value) headers.set('Authorization', `Bearer ${token.value}`);

  let response;
  try { response = await fetch(url, { ...options, headers }); }
  catch (error) {
    if (isApi && error.name !== 'AbortError') window.dispatchEvent(new CustomEvent('api-feedback', {detail:'Không thể kết nối máy chủ. Kiểm tra kết nối rồi thử lại; chưa thể xác nhận thao tác thành công.'}));
    throw error;
  }
  if (isApi && !target.pathname.startsWith('/api/auth/') && [403,429,500,502,503,504].includes(response.status)) {
    const message = response.status === 403 ? 'Bạn không có quyền thực hiện thao tác này. Vui lòng liên hệ quản trị viên nếu cần được cấp quyền.'
      : response.status === 429 ? 'Bạn thao tác quá nhiều lần. Vui lòng chờ rồi thử lại.'
      : 'Máy chủ đang gặp lỗi. Vui lòng thử lại sau; chưa thể xác nhận thao tác thành công.';
    window.dispatchEvent(new CustomEvent('api-feedback', {detail:message}));
  }

  if (isApi && response.status === 401) {
    logout();
  }

  return response;
}
