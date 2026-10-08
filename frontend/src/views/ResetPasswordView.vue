<template>
  <main class="min-h-dvh flex items-center justify-center bg-slate-100 p-4">
    <section class="w-full max-w-md bg-white rounded-2xl p-6 space-y-4 shadow">
      <h1 class="text-xl font-bold">Khôi phục mật khẩu</h1>
      <p role="status">Tính năng đang phát triển</p>
      <a href="#dashboard" class="block text-blue-700 underline">Quay lại đăng nhập</a>
    </section>
  </main>
</template>
<script setup>
window.history.replaceState(null, '', '#reset-password');
</script>