import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
  await page.route('**/api/**', route => route.fulfill({ json: route.request().url().includes('agencies-execution') ? {
    agencyExecutions: [{ agencyId: 'a', agencyName: 'Bộ kiểm thử', completionPercentage: 25, calculatedStatus: 'InProgressOnTime' }]
  } : [] }));
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  const displays = await page.evaluate(async () => {
    const f = await import('/tests/refactor-fixture.js');
    const { default: Modal } = await import('/src/components/ItemDetailModal.vue');
    const { formatProgressDisplay } = await import('/src/features/document/presentation.js');
    const { formatItemProgressDisplay } = await import('/src/features/dashboard/presentation.js');
    f.authState.user.value = { role: 1 }; f.authState.token.value = 'fixture';
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'task-status-test'; document.body.append(root);
    window.statusItem = f.ref({ id: 'test', itemType: 'Task', isGeneralTask: true, title: 'Nhiệm vụ chung', completionPercentage: 25, latestProgressPercent: 25, calculatedStatus: 'InProgressOnTime' });
    const app = f.createApp({ render: () => f.h(Modal, { isOpen: true, item: window.statusItem.value, initialTab: 'agencies-matrix' }) });
    app.directive('accessible-dialog', f.accessibleDialog); app.mount(root);
    return [formatProgressDisplay(window.statusItem.value), formatItemProgressDisplay(window.statusItem.value), formatProgressDisplay({ itemType: 'Goal', completionPercentage: 25 })];
  });
  assert.deepEqual(displays, ['Đang thực hiện (trong hạn)', 'Đang thực hiện (trong hạn)', '25%']);
  const root = page.locator('#task-status-test');
  await root.getByText('Bộ kiểm thử', { exact: true }).waitFor();
  assert.equal(await root.getByRole('columnheader', { name: 'Tiến độ (%)', exact: true }).count(), 0);
  assert.equal(await root.getByText('25%', { exact: true }).count(), 0);
  await root.getByText('Đang thực hiện (trong hạn)', { exact: true }).waitFor();
  await page.setViewportSize({ width: 390, height: 844 });
  assert.equal(await root.getByText('25%', { exact: true }).count(), 0);
  await page.evaluate(() => { window.statusItem.value = { ...window.statusItem.value, itemType: 'Goal' }; });
  await root.getByRole('columnheader', { name: 'Tiến độ (%)', exact: true }).waitFor();
  await root.getByText('25%', { exact: true }).waitFor();
  console.log('Passed task matrix status only on desktop/mobile, shared formatters, and goal percentage preserved.');
} finally { await browser.close(); }
