<template>
  <div class="login-wrapper min-h-screen bg-slate-950 flex items-center justify-center p-4 font-sans relative overflow-hidden">
    
    <!-- ================= MODERN ANIMATED TECH BACKGROUND ================= -->
    <!-- 1. Ambient Moving Gradient Glow Orbs -->
    <div class="animated-orb orb-1 pointer-events-none" aria-hidden="true"></div>
    <div class="animated-orb orb-2 pointer-events-none" aria-hidden="true"></div>
    <div class="animated-orb orb-3 pointer-events-none" aria-hidden="true"></div>
    <div class="animated-orb orb-4 pointer-events-none" aria-hidden="true"></div>

    <!-- 2. Cyber Digital Grid with Moving Scan/Wave Effect -->
    <div class="cyber-grid pointer-events-none" aria-hidden="true"></div>
    <div class="cyber-glow-beam pointer-events-none" aria-hidden="true"></div>

    <!-- 3. Tech Constellation Nodes & Connecting Lines (SVG Animation) -->
    <svg class="absolute inset-0 w-full h-full pointer-events-none opacity-25" xmlns="http://www.w3.org/2005/Atom" aria-hidden="true">
      <defs>
        <linearGradient id="lineGrad" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#38bdf8" stop-opacity="0.6"/>
          <stop offset="100%" stop-color="#3b82f6" stop-opacity="0"/>
        </linearGradient>
      </defs>
      <!-- Floating decorative lines -->
      <line x1="10%" y1="20%" x2="35%" y2="40%" stroke="url(#lineGrad)" stroke-width="1.5" stroke-dasharray="6 4" class="anim-dash" />
      <line x1="85%" y1="15%" x2="65%" y2="35%" stroke="url(#lineGrad)" stroke-width="1.5" stroke-dasharray="6 4" class="anim-dash-rev" />
      <line x1="20%" y1="80%" x2="45%" y2="60%" stroke="url(#lineGrad)" stroke-width="1.5" stroke-dasharray="6 4" class="anim-dash" />
      <line x1="90%" y1="85%" x2="70%" y2="65%" stroke="url(#lineGrad)" stroke-width="1.5" stroke-dasharray="6 4" class="anim-dash-rev" />

      <!-- Glowing constellation circles -->
      <circle cx="10%" cy="20%" r="4" fill="#38bdf8" class="anim-node node-1" />
      <circle cx="35%" cy="40%" r="3" fill="#60a5fa" class="anim-node node-2" />
      <circle cx="85%" cy="15%" r="4" fill="#38bdf8" class="anim-node node-3" />
      <circle cx="65%" cy="35%" r="3" fill="#818cf8" class="anim-node node-1" />
      <circle cx="20%" cy="80%" r="4" fill="#60a5fa" class="anim-node node-2" />
      <circle cx="45%" cy="60%" r="3" fill="#38bdf8" class="anim-node node-3" />
      <circle cx="90%" cy="85%" r="4" fill="#38bdf8" class="anim-node node-1" />
      <circle cx="70%" cy="65%" r="3" fill="#60a5fa" class="anim-node node-2" />
    </svg>

    <!-- ================= MAIN CENTERED LOGIN CARD ================= -->
    <div class="w-full max-w-md bg-white/95 backdrop-blur-xl rounded-3xl shadow-2xl border border-slate-200/90 p-7 sm:p-9 relative z-10 space-y-6">
      
      <!-- Logo & System Branding -->
      <div class="text-center space-y-3">
        <div class="relative inline-block">
          <div class="absolute -inset-1 bg-gradient-to-r from-blue-500 to-indigo-500 rounded-2xl blur-xs opacity-40 animate-pulse"></div>
          <img 
            src="/logo-cds.jpg" 
            alt="Logo Cục Chuyển đổi số quốc gia" 
            class="relative w-16 h-16 rounded-2xl mx-auto shadow-md object-cover border border-slate-200 bg-white p-0.5" 
          />
        </div>
        <div class="space-y-1">
          <h1 class="text-sm sm:text-base font-bold text-slate-800 leading-snug tracking-tight">
            HỆ THỐNG THEO DÕI NHIỆM VỤ CHUYỂN ĐỔI SỐ QUỐC GIA
          </h1>
          <p class="text-xs font-semibold text-slate-500">Cục Chuyển đổi số quốc gia</p>
        </div>
      </div>

      <!-- Error Alert Message -->
      <div 
        v-if="errorMessage" 
        role="alert" 
        class="p-3 bg-rose-50 border border-rose-200 rounded-xl text-xs font-semibold text-rose-800 flex items-start gap-2 animate-in fade-in"
      >
        <svg class="w-4 h-4 text-rose-600 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
        </svg>
        <div class="min-w-0 flex-1">
          <p class="font-bold text-rose-900">Đăng nhập không thành công</p>
          <p class="mt-0.5 text-rose-700 font-normal leading-relaxed">{{ errorMessage }}</p>
        </div>
      </div>

      <!-- Login Form -->
      <form @submit.prevent="handleLogin" class="space-y-4">
        <div>
          <label for="login-username" class="block text-xs font-semibold text-slate-700 mb-1.5">
            Tên đăng nhập <span class="text-rose-500" aria-hidden="true">*</span>
          </label>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
              </svg>
            </span>
            <input 
              id="login-username"
              v-model="username"
              type="text" 
              autocomplete="username"
              required
              placeholder="Nhập tên đăng nhập hoặc mã đơn vị..."
              class="ui-single-line block w-full pl-10 pr-3.5 py-2 text-base sm:text-xs font-semibold text-slate-900 bg-slate-50 border border-slate-200 rounded-xl placeholder:text-slate-400 placeholder:font-normal focus:bg-white focus:border-blue-600 focus:ring-2 focus:ring-blue-500/20 focus:outline-none transition"
            />
          </div>
        </div>

        <div>
          <label for="login-password" class="block text-xs font-semibold text-slate-700 mb-1.5">
            Mật khẩu <span class="text-rose-500" aria-hidden="true">*</span>
          </label>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
              </svg>
            </span>
            <input 
              id="login-password"
              v-model="password"
              :type="showLoginPassword ? 'text' : 'password'" 
              autocomplete="current-password"
              required
              placeholder="Nhập mật khẩu..."
              class="ui-single-line block w-full pl-10 pr-10 py-2 text-base sm:text-xs font-semibold text-slate-900 bg-slate-50 border border-slate-200 rounded-xl placeholder:text-slate-400 placeholder:font-normal focus:bg-white focus:border-blue-600 focus:ring-2 focus:ring-blue-500/20 focus:outline-none transition"
            />
            <button 
              type="button" 
              @click="showLoginPassword = !showLoginPassword" 
              class="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-600 focus:outline-none cursor-pointer"
              :title="showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'"
              :aria-label="showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'"
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
          <label for="login-remember" class="flex items-center gap-2 cursor-pointer text-slate-600 font-semibold select-none">
            <input 
              id="login-remember"
              type="checkbox" 
              v-model="rememberMe" 
              class="rounded border-slate-300 text-blue-600 focus:ring-blue-500 w-4 h-4 cursor-pointer" 
            />
            <span>Ghi nhớ đăng nhập</span>
          </label>

          <button 
            type="button" 
            @click="openForgotPasswordModal" 
            class="text-xs font-bold text-blue-600 hover:text-blue-800 hover:underline transition cursor-pointer"
          >
            Quên mật khẩu?
          </button>
        </div>

        <button 
          type="submit" 
          :disabled="isSubmitting"
          class="w-full py-3 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-lg shadow-blue-600/25 transition-all flex items-center justify-center gap-2 cursor-pointer disabled:cursor-not-allowed"
        >
          <svg v-if="isSubmitting" class="w-4 h-4 animate-spin text-white" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span>{{ isSubmitting ? 'Đang Đăng Nhập...' : 'ĐĂNG NHẬP HỆ THỐNG' }}</span>
        </button>
      </form>

      <!-- Technical Support Footnote -->
      <div class="text-center pt-2">
        <p class="text-xs text-slate-400">
          Cần cấp tài khoản? Liên hệ Cục Chuyển đổi số quốc gia
        </p>
      </div>

    </div>

    <!-- ================= FORGOT PASSWORD MODAL ================= -->
    <div 
      v-if="isForgotPasswordOpen" 
      v-accessible-dialog="() => isForgotPasswordOpen = false" 
      class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4"
    >
      <div class="bg-white rounded-3xl shadow-2xl border border-slate-200 max-w-md w-full p-6 space-y-4 font-sans relative animate-in fade-in">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2 text-slate-900">
            <span class="p-2 bg-blue-50 text-blue-700 rounded-xl font-bold">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"/>
              </svg>
            </span>
            <h3 class="text-base font-bold text-slate-800">Khôi phục mật khẩu tài khoản</h3>
          </div>
          <button 
            type="button"
            @click="isForgotPasswordOpen = false" 
            class="text-slate-400 hover:text-slate-600 font-bold p-1 rounded-lg hover:bg-slate-100 transition"
            aria-label="Đóng"
          >
            ✕
          </button>
        </div>

        <div class="space-y-2 text-xs text-slate-600 leading-relaxed">
          <p>
            Để đảm bảo an toàn thông tin theo quy định bảo mật, tính năng tự đặt lại mật khẩu trực tuyến đang được tạm khóa.
          </p>
          <p>
            Vui lòng liên hệ cán bộ quản trị hệ thống tại cơ quan/đơn vị của bạn hoặc gửi yêu cầu tới Cục Chuyển đổi số quốc gia để được cấp lại mật khẩu.
          </p>
        </div>

        <div class="flex justify-end pt-2 border-t border-slate-100">
          <button 
            type="button" 
            @click="isForgotPasswordOpen = false" 
            class="ui-btn ui-btn-primary"
          >
            Đã hiểu
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue';
import { login } from '../services/auth';

const emit = defineEmits(['loggedIn']);

const username = ref('');
const password = ref('');
const showLoginPassword = ref(false);
const rememberMe = ref(true);
const errorMessage = ref('');
const isSubmitting = ref(false);

const isForgotPasswordOpen = ref(false);

function openForgotPasswordModal() {
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
</script>

<style scoped>
/* ================= CSS ANIMATIONS FOR MODERN TECH BACKGROUND ================= */

/* Animated Floating Orbs */
.animated-orb {
  position: absolute;
  border-radius: 9999px;
  filter: blur(80px);
  will-change: transform, opacity;
}

.orb-1 {
  width: 28rem;
  height: 28rem;
  top: -10%;
  left: -8%;
  background: radial-gradient(circle, rgba(37, 99, 235, 0.35) 0%, rgba(30, 58, 138, 0.1) 70%);
  animation: floatOrb1 16s ease-in-out infinite alternate;
}

.orb-2 {
  width: 32rem;
  height: 32rem;
  bottom: -15%;
  right: -10%;
  background: radial-gradient(circle, rgba(79, 70, 229, 0.3) 0%, rgba(30, 27, 75, 0.08) 70%);
  animation: floatOrb2 20s ease-in-out infinite alternate;
}

.orb-3 {
  width: 22rem;
  height: 22rem;
  top: 35%;
  right: 12%;
  background: radial-gradient(circle, rgba(14, 165, 233, 0.22) 0%, rgba(3, 105, 161, 0.05) 70%);
  animation: floatOrb3 14s ease-in-out infinite alternate;
}

.orb-4 {
  width: 20rem;
  height: 20rem;
  bottom: 25%;
  left: 10%;
  background: radial-gradient(circle, rgba(16, 185, 129, 0.18) 0%, rgba(6, 78, 59, 0.04) 70%);
  animation: floatOrb4 18s ease-in-out infinite alternate;
}

@keyframes floatOrb1 {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(60px, 40px) scale(1.12); }
  100% { transform: translate(-30px, 80px) scale(0.95); }
}

@keyframes floatOrb2 {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(-70px, -50px) scale(1.15); }
  100% { transform: translate(40px, -90px) scale(0.9); }
}

@keyframes floatOrb3 {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(-50px, 60px) scale(1.08); }
  100% { transform: translate(40px, -40px) scale(0.92); }
}

@keyframes floatOrb4 {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(70px, -40px) scale(1.1); }
  100% { transform: translate(-40px, 50px) scale(0.96); }
}

/* Cyber Digital Grid Background */
.cyber-grid {
  position: absolute;
  inset: 0;
  background-image: 
    linear-gradient(to right, rgba(56, 189, 248, 0.07) 1px, transparent 1px),
    linear-gradient(to bottom, rgba(56, 189, 248, 0.07) 1px, transparent 1px);
  background-size: 40px 40px;
  mask-image: radial-gradient(ellipse 70% 60% at 50% 50%, #000 30%, transparent 80%);
  animation: gridDrift 30s linear infinite;
}

@keyframes gridDrift {
  0% { background-position: 0 0; }
  100% { background-position: 40px 40px; }
}

/* Moving Light Beam */
.cyber-glow-beam {
  position: absolute;
  top: -50%;
  left: 20%;
  width: 60%;
  height: 200%;
  background: radial-gradient(ellipse at center, rgba(59, 130, 246, 0.12) 0%, transparent 65%);
  transform: rotate(-30deg);
  animation: beamPulse 8s ease-in-out infinite alternate;
}

@keyframes beamPulse {
  0% { opacity: 0.3; transform: rotate(-30deg) scale(0.95); }
  100% { opacity: 0.7; transform: rotate(-30deg) scale(1.05); }
}

/* SVG Line Dash Movement */
.anim-dash {
  animation: dashMove 6s linear infinite;
}

.anim-dash-rev {
  animation: dashMoveRev 7s linear infinite;
}

@keyframes dashMove {
  to { stroke-dashoffset: -50; }
}

@keyframes dashMoveRev {
  to { stroke-dashoffset: 50; }
}

/* Constellation Nodes Glowing */
.anim-node {
  animation: nodeGlow 3s ease-in-out infinite alternate;
}

.node-1 { animation-delay: 0s; }
.node-2 { animation-delay: 1s; }
.node-3 { animation-delay: 2s; }

@keyframes nodeGlow {
  0% { opacity: 0.3; transform: scale(0.85); }
  100% { opacity: 1; transform: scale(1.3); }
}
</style>
