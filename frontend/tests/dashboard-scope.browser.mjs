import { createRequire } from 'node:module';
import assert from 'node:assert/strict';

const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const first = '12660000-0000-0000-0000-000000001266';
const second = '26290000-0000-0000-0000-000000002629';
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  for (const width of [1440, 390]) {
    const page = await browser.newPage({ viewport: { width, height: 900 } });
    const requests = [];
    page.on('pageerror', error => { throw error; });
    await page.addInitScript(() => {
      localStorage.setItem('cdsqg_auth_token', 'fixture');
      localStorage.setItem('cdsqg_auth_user', JSON.stringify({ role: 1, username: 'fixture' }));
      localStorage.setItem('cdsqg_active_doc_code', '1266');
    });
    await page.route('**/api/**', route => {
      const url = new URL(route.request().url());
      requests.push(url);
      if (url.pathname === '/api/agencies') return route.fulfill({ json: [] });
      if (url.pathname === '/api/documents') return route.fulfill({json:{items:[{id:first,totalGoals:3,totalTasks:1},{id:second,totalGoals:22,totalTasks:75}],totalCount:2}});
      if (url.pathname === '/api/dashboard/metrics') {
        const combined=url.searchParams.getAll('documentIds').length===2;
        const goals=combined?25:3, tasks=combined?76:1;
        return route.fulfill({ json: {
        overviewMetrics: { goalStatusSummary: { notStarted: goals, inProgress: 0, completed: 0 }, taskStatusSummary: { notStarted: tasks, inProgress: 0, completed: 0 } },
        createdMetrics: { goalsSpecific: goals, goalsGeneral: 0, tasksSpecific: tasks, tasksGeneral: 0 },
        goalStatusSummary: { notStarted: goals, inProgress: 0, completed: 0 }, taskStatusSummary: { notStarted: tasks, inProgress: 0, completed: 0 },
        ministriesPerformance: [], provincesPerformance: [], othersPerformance: [], groupStatistics: []
      } });
      }
      return route.fulfill({ json: [] });
    });
    await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173', { waitUntil: 'networkidle' });
    await page.evaluate(async () => {
      const fixture = await import('/tests/refactor-fixture.js');
      fixture.authState.user.value = { role: 1, username: 'fixture' };
      fixture.authState.token.value = 'fixture';
      document.querySelector('#app').style.display = 'none';
      const root = document.createElement('div');
      root.id = 'scope-test';
      document.body.append(root);
      const {default:Header}=await import('/src/components/AppHeader.vue');
      const app = fixture.createApp({ render: () => fixture.h('div',{},[fixture.h(Header,{activeTab:'dashboard',user:{username:'fixture'}}),fixture.h(fixture.Dashboard)]) });
      app.use(fixture.FloatingVue);
      app.directive('accessible-dialog', fixture.accessibleDialog);
      app.directive('accessible-data', fixture.accessibleData);
      app.mount(root);
    });
    const root = page.locator('#scope-test');
    await root.getByRole('heading', { name: 'Toàn cảnh thực hiện Chiến lược' }).waitFor();
    await page.waitForLoadState('networkidle');
    assert(requests.some(url => url.pathname === '/api/dashboard/metrics' && url.searchParams.getAll('documentIds').join() === first));
    assert.equal(await root.getByRole('region',{name:'Tổng quan mục tiêu'}).locator('[data-tone="total"] .kpi-value').innerText(),'3');
    await root.getByRole('button',{name:'Chọn chiến lược'}).click();
    await page.getByRole('group',{name:'Danh sách chiến lược'}).getByRole('button',{name:/Tổng quan hai văn bản/}).click();
    await root.getByRole('heading', { name: 'Toàn cảnh thực hiện Chiến lược & Chương trình' }).waitFor();
    await page.waitForLoadState('networkidle');
    assert(requests.some(url => url.pathname === '/api/dashboard/metrics' && url.searchParams.getAll('documentIds').join() === `${first},${second}`));
    assert.equal(await root.getByRole('region',{name:'Tổng quan mục tiêu'}).locator('[data-tone="total"] .kpi-value').innerText(),'25');
    assert.equal(await root.getByRole('region',{name:'Tổng quan nhiệm vụ'}).locator('[data-tone="total"] .kpi-value').innerText(),'76');
    assert((await root.getByRole('button',{name:'Chọn chiến lược'}).innerText()).includes('Tổng'));
    await root.getByRole('button',{name:'Chọn chiến lược'}).click();
    await page.getByRole('group',{name:'Danh sách chiến lược'}).getByRole('button',{name:/^1266\/QĐ-TTg/}).click();
    await page.waitForLoadState('networkidle');
    assert.equal(await root.getByRole('region',{name:'Tổng quan mục tiêu'}).locator('[data-tone="total"] .kpi-value').innerText(),'3');
    assert.equal(await root.getByRole('region',{name:'Tổng quan nhiệm vụ'}).locator('[data-tone="total"] .kpi-value').innerText(),'1');
    assert(!(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth + 1)), `Horizontal overflow at ${width}px`);
    await page.close();
  }
  console.log('PASS dashboard scope desktop/mobile');
} finally {
  await browser.close();
}
