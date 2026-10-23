import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite';
import { excelWriterPlugin, excelWriterDependencyPlugin } from './build/excelWriter';

export default defineConfig({
  cacheDir: process.env.CDSQG_VITE_CACHE_DIR || 'node_modules/.vite',
  plugins: [excelWriterPlugin(), vue(), tailwindcss()],
  optimizeDeps: { esbuildOptions: { plugins: [excelWriterDependencyPlugin()] } },
  server: {
    port: 5173,
    host: true,
    // Windows sync folders may not emit native file events; poll to avoid serving stale modules.
    watch: {
      usePolling: process.env.CDSQG_VITE_USE_POLLING !== 'false' && process.platform === 'win32',
      interval: 500
    }
  }
});
