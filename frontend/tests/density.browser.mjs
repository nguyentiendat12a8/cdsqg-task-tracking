import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
const base = process.env.FRONTEND_TEST_URL || 'http://localhost:5173';
const views = ['views/ExecutiveDashboard.vue', 'views/DocumentDetailView.vue', 'views/ExecutiveReportsView.vue', 'views/AgencyPlansView.vue', 'components/AgencyManagement.vue', 'views/UserManagementView.vue', 'views/LegalDocumentsView.vue', 'views/UrgeHistoryView.vue', 'views/MasterDataView.vue'];
try {
  for (const width of [1440, 390]) {
    const page = await browser.newPage({ viewport: { width, height: 900 } });
    const errors = [];
    page.on('pageerror', e => errors.push(e.message));
    await page.route('**/api/**', route => {
      const path = new URL(route.request().url()).pathname;
      const data = path.endsWith('/items') ? { items: [], totalCount: 0, totalPages: 1 } : path.includes('dashboard') ? {} : [];
      return route.fulfill({ json: data });
    });
    await page.goto(base);
    await page.evaluate(async () => {
      const f = await import('/tests/refactor-fixture.js');
      f.authState.user.value = { role: 1 }; f.authState.token.value = 'fixture';
      document.querySelector('#app').style.display = 'none';
      const root = document.createElement('div'); root.id = 'density-review'; root.className = 'ui-density-compact'; document.body.append(root);
      window.mountDensity = async path => {
        window.densityApp?.unmount();
        const component = (await import('/src/' + path)).default;
        window.densityApp = f.createApp({ render: () => f.h(component, { filterItemType: 'Goal' }) });
        window.densityApp.use(f.FloatingVue); window.densityApp.directive('accessible-dialog', f.accessibleDialog); window.densityApp.directive('accessible-data', f.accessibleData); window.densityApp.mount(root);
        await f.nextTick();
      };
    });
    for (const view of views) {
      await page.evaluate(path => window.mountDensity(path), view);
      await page.locator('#density-review').waitFor();
      await page.waitForLoadState('networkidle');
      assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false, `${view} at ${width}px must not overflow page`);
      const metrics = await page.locator('#density-review').evaluate(root => [...root.querySelectorAll('.ui-button,.ui-single-line,.ui-toolbar-control')].filter(e => e.getClientRects().length).map(e => getComputedStyle(e).height));
      assert.ok(metrics.every(height => height === '36px'), `${view}: controls keep height 36px`);
      const fontSizes = await page.locator('#density-review').evaluate(root => [...root.querySelectorAll('button.ui-single-line,.ui-button,[role=combobox],label')].filter(e => e.getClientRects().length).map(e => getComputedStyle(e).fontSize));
      assert.ok(fontSizes.every(size => size === '12px'), `${view}: controls and labels use 12px`);
      if (view === views[0] || view === views[1]) await page.screenshot({ path: `tests/density-${view.includes('Dashboard') ? 'dashboard' : 'list'}-${width}.png` });
    }
    const density = await page.evaluate(() => {
      const root = document.querySelector('#density-review'); const node = document.createElement('div'); node.className = 'p-5 sm:p-6 gap-4'; root.append(node);
      const css = getComputedStyle(node); return { padding: css.paddingTop, gap: css.gap };
    });
    assert.equal(density.padding, width >= 640 ? '20px' : '16px'); assert.equal(density.gap, '12px');
    assert.deepEqual(errors, [], `Vue runtime errors at ${width}px`);
    await page.close();
  }
  console.log('Passed: 9 system views desktop/mobile, no horizontal page overflow, 36px controls, shared density tokens. Empty API fixtures.');
} finally { await browser.close(); }
