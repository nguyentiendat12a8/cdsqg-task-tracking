import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));
  await page.route('**/api/**', route => route.fulfill({ json: new URL(route.request().url()).pathname === '/api/agencies' ? [] : { items: [], totalCount: 0 } }));
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://localhost:5173');
  await page.evaluate(async () => {
    const f = await import('/tests/refactor-fixture.js');
    f.authState.user.value = { role: 1 }; f.authState.token.value = 'fixture';
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'dropdown-test'; document.body.append(root);
    const app = f.createApp(f.Reports);
    app.use(f.FloatingVue); app.directive('accessible-dialog', f.accessibleDialog); app.directive('accessible-data', f.accessibleData); app.mount(root);
  });
  const type = page.getByRole('combobox', { name: 'Loại đối tượng', exact: true });
  const search = page.getByPlaceholder('Tìm theo mã, tên mục tiêu / nhiệm vụ...');
  const filter = page.getByRole('button', { name: /Lọc Nâng Cao/ });
  async function assertHeight(expected) {
    for (const control of [type, search, filter]) assert.equal(Math.round((await control.boundingBox()).height), expected);
  }
  await assertHeight(36);
  await type.focus(); await page.keyboard.press('Enter');
  await page.getByRole('option', { name: 'Chỉ Mục tiêu', exact: true }).click();
  assert.ok((await type.innerText()).includes('Chỉ Mục tiêu'));
  await type.click();
  await page.screenshot({ path: 'tests/report-dropdown-desktop.png' });
  await page.keyboard.press('Escape');
  assert.equal(await type.getAttribute('aria-expanded'), 'false');
  await page.setViewportSize({ width: 390, height: 844 });
  await assertHeight(36);
  await type.click();
  await page.getByRole('option', { name: 'Chỉ Nhiệm vụ', exact: true }).click();
  assert.ok((await type.innerText()).includes('Chỉ Nhiệm vụ'));
  assert.equal(await page.locator('#dropdown-test select').count(), 0);
  assert.deepEqual(errors, []);
  console.log('Passed: reports use shared dropdowns, keyboard open/Escape, type selection and mobile selection.');
} finally { await browser.close(); }

