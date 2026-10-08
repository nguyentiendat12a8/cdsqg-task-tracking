import { createApp } from 'vue';
import { createPinia } from 'pinia';
import FloatingVue from 'floating-vue';
import 'floating-vue/dist/style.css';
import App from './App.vue';
import './style.css';
import { accessibleDialog } from './directives/accessibleDialog';
import { accessibleData } from './directives/accessibleData';

const app = createApp(App);
app.directive('accessible-dialog', accessibleDialog);
app.directive('accessible-data', accessibleData);
app.use(createPinia());
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
