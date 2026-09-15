import { createApp } from 'vue';
import { createPinia } from 'pinia';
import FloatingVue from 'floating-vue';
import 'floating-vue/dist/style.css';
import App from './App.vue';
import './style.css';

const app = createApp(App);
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
