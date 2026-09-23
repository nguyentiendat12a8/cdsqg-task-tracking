<template>
  <div class="min-h-screen bg-slate-900 flex items-center justify-center p-4 font-sans relative overflow-hidden">
    <!-- Decorative background glow gradients (Executive Navy Theme) -->
    <div class="absolute -top-40 -left-40 w-96 h-96 bg-blue-600/20 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-blue-900/30 rounded-full blur-3xl pointer-events-none"></div>

    <div class="w-full max-w-md bg-white rounded-3xl shadow-2xl border border-slate-200 p-8 relative z-10 space-y-6">
      
      <!-- Logo & System Branding -->
      <div class="text-center space-y-3">
        <img src="/logo-cds.jpg" alt="Logo Cục Chuyển đổi số" class="w-16 h-16 rounded-2xl mx-auto shadow-lg object-cover border border-slate-200 bg-white" />
        <div>
          <h1 class="text-xl font-bold text-slate-800">Hệ Thống Theo Dõi Nhiệm Vụ CĐS Quốc Gia</h1>
          <p class="text-xs text-slate-500 mt-1 font-semibold">Vui lòng đăng nhập để truy cập hệ thống quản hành</p>
        </div>
      </div>

      <!-- Error Alert Message -->
      <div v-if="errorMessage" class="p-3.5 bg-rose-50 border border-rose-200 rounded-xl text-xs font-bold text-rose-700 flex items-center gap-2 animate-in fade-in">
        <svg class="w-4 h-4 text-rose-600 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
        <span>{{ errorMessage }}</span>
      </div>

      <!-- Login Form -->
      <form @submit.prevent="handleLogin" class="space-y-4">
        <div>
          <label class="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">Tên Đăng Nhập <span class="text-rose-500">*</span></label>
          <div class="relative">
            <svg class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>
            <input 
              v-model="username"
              type="text" 
              required
              placeholder="Nhập tên đăng nhập..."
              class="w-full text-xs font-bold pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">Mật Khẩu <span class="text-rose-500">*</span></label>
          <div class="relative">
            <svg class="w-4 h-4 text-slate-400 absolute left-3.5 top-3 pointer-events-none" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/></svg>
            <input 
              v-model="password"
              :type="showLoginPassword ? 'text' : 'password'" 
              required
              placeholder="Nhập mật khẩu..."
              class="w-full text-xs font-bold pl-10 pr-10 py-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
            />
            <button 
              type="button" 
              @click="showLoginPassword = !showLoginPassword" 
              class="absolute right-3 top-3 text-slate-400 hover:text-slate-600 focus:outline-none cursor-pointer"
              :title="showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'"
            >
              <svg v-if="!showLoginPassword" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
              </svg>
              <svg v-else class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858-5.908a10.046 10.046 0 013.122-.463c4.478 0 8.268 2.943 9.542 7a9.97 9.97 0 01-2.163 3.652M3 3l18 18"/>
              </svg>
            </button>
          </div>
        </div>

        <div class="flex items-center justify-between text-xs pt-1">
          <label class="flex items-center gap-2 cursor-pointer text-slate-600 font-semibold select-none">
            <input type="checkbox" v-model="rememberMe" class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-4 h-4" />
            <span>Ghi nhớ đăng nhập</span>
          </label>

          <button 
            type="button" 
            @click="openForgotPasswordModal" 
            class="text-xs font-bold text-blue-600 hover:text-blue-800 hover:underline transition"
          >
            Quên mật khẩu?
          </button>
        </div>

        <button 
          type="submit" 
          :disabled="isSubmitting"
          class="w-full py-3 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-lg shadow-blue-500/25 transition flex items-center justify-center gap-2"
        >
          <svg v-if="isSubmitting" class="w-4 h-4 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
          <span>{{ isSubmitting ? 'Đang Đăng Nhập...' : 'ĐĂNG NHẬP HỆ THỐNG' }}</span>
        </button>
      </form>

    </div>

    <!-- FORGOT PASSWORD MODAL -->
    <div v-if="isForgotPasswordOpen" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl shadow-2xl border border-slate-200 max-w-md w-full p-6 space-y-4 font-sans relative animate-in fade-in">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2 text-slate-900">
            <span class="p-2 bg-blue-100 text-blue-700 rounded-xl font-bold">🔑</span>
            <h3 class="text-base font-bold">Khôi Phục Mật Khẩu Tài Khoản</h3>
          </div>
          <button @click="isForgotPasswordOpen = false" class="text-slate-400 hover:text-slate-600 font-bold text-lg">✕</button>
        </div>

        <p class="text-xs text-slate-500 font-semibold leading-relaxed">
          Nhập Email (hoặc Username) đã liên kết với tài khoản. Hệ thống sẽ tạo và gửi lại mật khẩu mới về Email của bạn.
        </p>

        <!-- Forgot Password Messages -->
        <div v-if="forgotErrorMessage" class="p-3 bg-rose-50 border border-rose-200 rounded-xl text-xs font-bold text-rose-700 flex items-center gap-2">
          <span>{{ forgotErrorMessage }}</span>
        </div>

        <div v-if="forgotSuccessMessage" class="p-3 bg-emerald-50 border border-emerald-200 rounded-xl text-xs font-semibold text-emerald-800 space-y-1">
          <div class="font-bold text-emerald-900">✓ {{ forgotSuccessMessage }}</div>
          <div v-if="generatedTempPassword" class="text-[11px] bg-white p-2 rounded-lg border border-emerald-200 mt-1">
            Mật khẩu tạm thời mới: <strong class="text-blue-900 text-xs font-bold">{{ generatedTempPassword }}</strong>
          </div>
        </div>

        <form @submit.prevent="handleForgotPassword" class="space-y-4">
          <div>
            <label class="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">Email hoặc Username <span class="text-rose-500">*</span></label>
            <div class="relative">
              <svg class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/></svg>
              <input 
                v-model="forgotInput"
                type="text" 
                required
                placeholder="Nhập email hoặc username..."
                class="w-full text-xs font-bold pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none transition"
              />
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2 border-t border-slate-100">
            <button 
              type="button" 
              @click="isForgotPasswordOpen = false" 
              class="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-600 font-bold text-xs rounded-xl transition"
            >
              Hủy
            </button>
            <button 
              type="submit" 
              :disabled="isForgotSubmitting"
              class="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-sm transition flex items-center gap-1.5"
            >
              <svg v-if="isForgotSubmitting" class="w-4 h-4 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
              <span>Gửi Yêu Cầu Mật Khẩu</span>
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue';
import { login } from '../services/auth';
import { getApiUrl } from '../config/api';

const emit = defineEmits(['loggedIn']);

const username = ref('admin');
const password = ref('adminpassword');
const showLoginPassword = ref(false);
const rememberMe = ref(true);
const errorMessage = ref('');
const isSubmitting = ref(false);

const isForgotPasswordOpen = ref(false);
const forgotInput = ref('');
const forgotErrorMessage = ref('');
const forgotSuccessMessage = ref('');
const generatedTempPassword = ref('');
const isForgotSubmitting = ref(false);

function openForgotPasswordModal() {
  forgotInput.value = username.value || '';
  forgotErrorMessage.value = '';
  forgotSuccessMessage.value = '';
  generatedTempPassword.value = '';
  isForgotPasswordOpen.value = true;
}

async function handleLogin() {
  errorMessage.value = '';
  isSubmitting.value = true;
  try {
    const user = await login(username.value, password.value);
    emit('loggedIn', user);
  } catch (err) {
    errorMessage.value = err.message || 'Đăng nhập không thành công.';
  } finally {
    isSubmitting.value = false;
  }
}

async function handleForgotPassword() {
  forgotErrorMessage.value = '';
  forgotSuccessMessage.value = '';
  generatedTempPassword.value = '';

  if (!forgotInput.value.trim()) {
    forgotErrorMessage.value = 'Vui lòng nhập Email hoặc Username.';
    return;
  }

  isForgotSubmitting.value = true;
  try {
    const res = await fetch(getApiUrl('/api/auth/forgot-password'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOrUsername: forgotInput.value.trim() })
    });

    if (res.ok) {
      const data = await res.json();
      forgotSuccessMessage.value = data.message || `Đã gửi mật khẩu mới về email ${data.email}`;
      if (data.tempPassword) {
        generatedTempPassword.value = data.tempPassword;
      }
    } else {
      const errData = await res.json().catch(() => ({}));
      forgotErrorMessage.value = errData.message || 'Không tìm thấy tài khoản tương ứng.';
    }
  } catch (e) {
    forgotErrorMessage.value = 'Không thể kết nối máy chủ.';
  } finally {
    isForgotSubmitting.value = false;
  }
}
</script>
