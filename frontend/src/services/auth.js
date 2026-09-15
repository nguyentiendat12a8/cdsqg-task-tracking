import { ref, computed } from 'vue';
import { getApiUrl } from '../config/api';

const TOKEN_KEY = 'cdsqg_auth_token';
const USER_KEY = 'cdsqg_auth_user';

const token = ref(localStorage.getItem(TOKEN_KEY) || '');
const user = ref(JSON.parse(localStorage.getItem(USER_KEY) || 'null'));

export const authState = {
  token,
  user,
  isLoggedIn: computed(() => !!token.value && !!user.value),
  isAdmin: computed(() => user.value?.role === 'Admin' || user.value?.role === 1)
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
  const headers = {
    ...options.headers,
    ...getAuthHeaders()
  };

  const response = await fetch(url, {
    ...options,
    headers
  });

  if (response.status === 401) {
    logout();
  }

  return response;
}
