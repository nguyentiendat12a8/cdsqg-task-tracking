import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel:'msedge', headless:true });
try {
  const page = await browser.newPage();
  let releaseAgencies;
  let mainRequested = false;
  const agencyGate = new Promise(resolve => { releaseAgencies = resolve; });
  await page.route('**/api/**', async route => {
    const url = new URL(route.request().url());
    if (url.pathname === '/api/dashboard/metrics' && !url.searchParams.has('parentAgencyId')) mainRequested = true;
    if (new URL(route.request().url()).pathname === '/api/agencies') await agencyGate;
    await route.fulfill({ json: new URL(route.request().url()).pathname === '/api/agencies' ? [] : {
      totalGoals: 0, totalTasks: 0, statusSummary: {}, goalStatusSummary: {}, taskStatusSummary: {},
      ministriesPerformance: [], provincesPerformance: [], othersPerformance: []
    } });
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  await page.evaluate(async () => {
    const f = await import('/tests/refactor-fixture.js');
    f.authState.user.value = { role: 1 }; f.authState.token.value = '';
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'loading-test'; document.body.append(root);
    const app = f.createApp({ render: () => f.h(f.Dashboard) });
    app.directive('accessible-dialog', f.accessibleDialog); app.mount(root);
  });
  const root = page.locator('#loading-test');
  await root.getByText('Đang tải dữ liệu tổng quan bảng điều khiển...', { exact:true }).waitFor();
  assert.equal(await root.getByText('Chưa thực hiện', { exact:true }).count(), 0);
  await page.waitForTimeout(100);
  assert.equal(mainRequested, true, 'Main metrics must start before the agency directory completes');
  releaseAgencies();
  await root.getByText('Chưa thực hiện', { exact:true }).first().waitFor();
  assert.equal(await root.getByText('Đang tải dữ liệu tổng quan bảng điều khiển...', { exact:true }).count(), 0);
  console.log('Passed initial loading during delayed agency fetch; no premature zero summary. API mocked.');
} finally { await browser.close(); }
