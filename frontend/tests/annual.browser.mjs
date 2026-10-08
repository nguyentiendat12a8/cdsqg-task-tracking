import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage();
  const errors = [], warnings = [], queries = [];
  let baseline, progress;
  page.on('pageerror', e => errors.push(e.message));
  page.on('console', m => { if (m.text().includes('[Vue warn]')) warnings.push(m.text()); });
  await page.route('**/api/**', async route => {
    const req = route.request(), url = new URL(req.url());
    let data = [];
    if (url.pathname.endsWith('/custom-baseline') && req.method() === 'POST') { baseline = req.postDataJSON(); data = { success: true }; }
    if (url.pathname.endsWith('/progress')) {
      if (req.method() === 'POST') { progress = req.postData(); data = { approvalStatus: 'Approved' }; }
      else { queries.push(url); data = null; }
    }
    await route.fulfill({ json: data });
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5201');
  await page.evaluate(async () => {
    const f = await import('/tests/refactor-fixture.js');
    f.authState.user.value = { role: 1, username: 'review' }; f.authState.token.value = 'fixture';
    document.querySelector('#app').style.display = 'none';
    const node = document.createElement('div'); node.id = 'annual-test'; document.body.append(node);
    window.annualMode = f.ref('baseline');
    const agency = { id: '11111111-1111-1111-1111-111111111111', name: 'Cơ quan kiểm thử', code: 'TEST', type: 'Ministry' };
    const app = f.createApp({ render: () => f.h(window.annualMode.value === 'baseline' ? f.AnnualBaseline : f.AnnualProgress, {
      isOpen: true, taskId: '22222222-2222-2222-2222-222222222222', taskCode: 'MT01', evaluationType: 'Quantitative',
      existingCustomBaseline: { '2026': '50', Q1_2026: '10', M1_2026: '2', hasQuarter: 'true' },
      customBaseline: { hasQuarter: 'true', Q1_2026: '10', M1_2026: '2' },
      agencies: [agency], leadAgencyId: agency.id, dynamicYears: [2026, 2027], yearlyTargets: { '2026': 100 }
    }) });
    app.directive('accessible-dialog', f.accessibleDialog); app.directive('accessible-data', f.accessibleData); app.mount(node);
  });
  const root = page.locator('#annual-test');
  await root.getByText('Mốc tổng cần đạt đến từng năm').waitFor();
  await page.screenshot({ path: 'tests/annual-baseline-desktop.png' });
  await page.setViewportSize({ width: 390, height: 844 });
  assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > innerWidth), false);
  await page.screenshot({ path: 'tests/annual-baseline-mobile.png' });
  await page.setViewportSize({ width: 1280, height: 720 });
  await root.getByLabel('Năm 2026 (%)').fill('40');
  await root.getByRole('button', { name: 'Lưu chỉ tiêu năm' }).click();
  await page.waitForFunction(() => !document.querySelector('#annual-test button[disabled]'));
  assert.deepEqual(baseline, { milestones: { '2026': '40' } });
  await page.evaluate(() => { window.annualMode.value = 'progress'; });
  await root.getByPlaceholder('Nhập con số thực tế...').waitFor();
  await page.waitForTimeout(200);
  assert.equal(await root.getByText(/Theo Quý|Theo Tháng|Chọn Quý|Chọn Tháng/).count(), 0);
  assert.ok(queries.length > 0);
  assert.ok(queries.every(u => u.searchParams.has('year') && !u.searchParams.has('period') && !u.searchParams.has('quarter')));
  await root.getByPlaceholder('Nhập con số thực tế...').fill('40');
  await root.getByRole('button', { name: /Lưu|Gửi|Cập nhật/ }).last().click();
  await page.waitForTimeout(300);
  assert.ok(progress?.includes('name="PeriodYear"'));
  assert.ok(!/name="Period(?:Quarter|Month|Type)"/.test(progress));
  assert.deepEqual(errors, []); assert.deepEqual(warnings, []);
  console.log('Passed: annual baseline payload, annual report query and multipart fields, no quarter/month controls, no Vue errors.');
} finally { await browser.close(); }
