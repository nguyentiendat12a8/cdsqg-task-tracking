import { createApp } from 'vue';
import { createPinia } from 'pinia';
import Vue3Toastify from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import FloatingVue from 'floating-vue';
import 'floating-vue/dist/style.css';
import App from './App.vue';
import './style.css';
import { accessibleDialog } from './directives/accessibleDialog';
import { accessibleData } from './directives/accessibleData';
import { appTooltip } from './directives/appTooltip';

const app = createApp(App);
app.directive('accessible-dialog', accessibleDialog);
app.directive('accessible-data', accessibleData);
app.directive('tip', appTooltip);
app.use(createPinia());
app.use(Vue3Toastify, { autoClose: 6000, position: 'top-right', theme: 'light' });
app.use(FloatingVue, {
  themes: {
    'custom-dark': {
      $extend: 'tooltip',
      delay: { show: 1000, hide: 0 },
      placement: 'top',
    }
  }
});
app.mount('#app');
