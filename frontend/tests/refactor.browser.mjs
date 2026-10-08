import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
const base = process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5201';
const agency = { id: '11111111-1111-1111-1111-111111111111', name: 'Cơ quan kiểm thử tiếng Việt', code: 'TEST', type: 'Ministry', parentId: null };
const goal = { id: '22222222-2222-2222-2222-222222222222', taskId: '22222222-2222-2222-2222-222222222222', itemType: 'Goal', code: 'MT001', section: 'Mục A', title: 'Mục tiêu kiểm thử tiếng Việt', leadAgencyId: agency.id, leadAgencyName: agency.name, calculatedStatus: 'NotStarted', status: 'NotStarted', startDate: '2026-01-01', dueDate: '2030-12-31', latestProgressValue: null, unitName: '%', subItems: [] };
const task = { ...goal, id: '33333333-3333-3333-3333-333333333333', taskId: '33333333-3333-3333-3333-333333333333', itemType: 'Task', code: 'NV001', title: 'Nhiệm vụ kiểm thử tiếng Việt' };
const status = { notStarted: 1, totalGoals: 1, totalTasks: 1 };
const metrics = { totalGoals: 1, totalTasks: 1, createdMetrics: { totalGoals: 1, totalTasks: 1, goalsSpecific: 1, tasksSpecific: 1, goalStatusSummary: status, taskStatusSummary: status }, goalStatusSummary: status, taskStatusSummary: status, ministriesPerformance: [{ ...agency, agencyId: agency.id, totalItems: 1, totalGoals: 1, totalTasks: 1, notStarted: 1 }], provincesPerformance: [], othersPerformance: [] };
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 }, acceptDownloads: true });
  page.setDefaultTimeout(10000);
  const errors = [], warnings = [], requests = [];
  page.on('pageerror', e => { errors.push(e.message); console.error('Browser error:', e.message); });
  page.on('console', m => { if (m.type() === 'warning' && m.text().includes('[Vue warn]')) warnings.push(m.text()); });
  page.on('request', r => requests.push(r.url()));
  let failItems = false, lastEdit, lastCreate;
  await page.route('**/api/**', async route => {
    const url = new URL(route.request().url());
    let data = [];
    if (route.request().method() === 'POST' && url.pathname === '/api/planning/items') {
      lastCreate = route.request().postDataJSON();
      return route.fulfill({ contentType: 'application/json', body: '{}' });
    }
    if (route.request().method() === 'PUT' && url.pathname.startsWith('/api/planning/items/')) {
      lastEdit = route.request().postDataJSON();
      return route.fulfill({ contentType: 'application/json', body: '{}' });
    }
    if (url.pathname === '/api/agencies') data = [agency];
    else if (url.pathname === '/api/dashboard/metrics') data = url.searchParams.has('parentAgencyId') ? { ministriesPerformance: [] } : metrics;
    else if (url.pathname === '/api/dashboard/agency-items') data = [goal, task];
    else if (url.pathname.match(/\/api\/documents\/.*\/items/)) {
      if (failItems) return route.fulfill({ status: 500, contentType: 'application/json', body: '{}' });
      data = { items: url.searchParams.get('itemType') === 'Goal' ? [goal] : [task], totalCount: 1, totalPages: 1 };
      const search = url.searchParams.get('search');
      if (search === 'old' || search === 'new') {
        data.items = [{ ...goal, title: search + ' response' }];
        await new Promise(resolve => setTimeout(resolve, search === 'old' ? 700 : 10));
      }
    } else if (url.pathname.includes('grid')) data = { items: [], agencies: [], baselines: [] };
    else if (url.pathname.includes('dashboard-stats')) data = {};
    await route.fulfill({ contentType: 'application/json', body: JSON.stringify(data) });
  });
  await page.goto(base);
  await page.evaluate(async () => {
    const fixture = await import('/tests/refactor-fixture.js');
    fixture.authState.user.value = { role: 1, username: 'review', fullName: 'Kiểm thử' };
    fixture.authState.token.value = 'fixture-token';
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'review-root'; document.body.appendChild(root);
    window.reviewMode = fixture.ref('dashboard');
    window.reviewType = fixture.ref('Goal');
    const app = fixture.createApp({ render: () => fixture.h(window.reviewMode.value === 'dashboard' ? fixture.Dashboard : fixture.Document, { filterItemType: window.reviewType.value }) });
    app.use(fixture.FloatingVue);
    app.directive('accessible-dialog', fixture.accessibleDialog);
    app.directive('accessible-data', fixture.accessibleData);
    app.mount(root);
  });
  await page.getByRole('button', { name: /Xuất Báo Cáo Excel/ }).waitFor();
  await page.locator('#review-root').getByText('Khối Các Bộ / Ngành', { exact: false }).click();
  await page.getByRole('button', { name: 'Xem chi tiết ' + agency.name }).waitFor();
  assert.equal(requests.some(u => /xlsx[._-](?:js|bundle|min|js-style)/i.test(u)), false, 'Opening dashboard must not download Excel');
  await page.getByRole('button', { name: 'Xem chi tiết ' + agency.name }).click();
  await page.getByText(goal.title, { exact: true }).waitFor();
  await page.locator('#review-root').getByRole('button', { name: '✕', exact: true }).click();
  const downloadPromise = page.waitForEvent('download');
  await page.getByRole('button', { name: /Xuất Báo Cáo Excel/ }).click();
  const download = await downloadPromise;
  const wb = XLSX.read(await fs.readFile(await download.path()), { type: 'buffer', cellStyles: true });
  assert.equal(wb.SheetNames.length, 2);
  assert.equal(wb.SheetNames[0], 'TỔNG HỢP CHUNG');
  assert.ok(XLSX.utils.sheet_to_json(wb.Sheets[wb.SheetNames[1]], { header: 1 }).flat().includes(goal.title), 'Vietnamese text must survive XLSX export');
  assert.ok(wb.Sheets[wb.SheetNames[0]]['!merges']?.length, 'Report keeps merged title cells');
  assert.ok(wb.Sheets[wb.SheetNames[0]].A1.s?.fgColor, 'Styled export keeps the title fill');
  const modern = await page.evaluate(async bytes => {
    const { loadExcelReader } = await import('/tests/refactor-fixture.js');
    const reader = await loadExcelReader('report.xlsx');
    const wb = reader.read(new Uint8Array(bytes), { type: 'array' });
    return wb.SheetNames.length;
  }, Array.from(await fs.readFile(await download.path())));
  assert.equal(modern, 2);
  assert.equal(requests.some(u => /xlsx-js-style(?:_|\/|%2F)*(?:dist_)?xlsx_min|xlsx\.min/i.test(u)), false, 'Export must not load the full legacy reader');

  await page.evaluate(() => { window.reviewMode.value = 'document'; });
  await page.locator('#review-root table').getByText(goal.title, { exact: true }).waitFor();
  await page.mouse.move(0, 0);
  for (const close of await page.locator('.Toastify__close-button').all()) await close.click();
  await page.locator('#review-root').getByRole('button', { name: /Thêm Mục Tiêu Mới/ }).click();
  await page.getByRole('heading', { name: /Thêm mới Mục Tiêu/ }).waitFor();
  await page.getByRole('dialog').locator('textarea').fill('Tên mục tiêu mới');
  await page.getByRole('dialog').getByRole('button', { name: 'Lưu', exact: true }).click();
  await page.getByRole('dialog').waitFor({ state: 'hidden' });
  assert.equal(lastCreate.title, 'Tên mục tiêu mới');
  assert.equal(lastCreate.leadAgencyId, agency.id);
  await page.locator('#review-root table').getByText(goal.title, { exact: true }).waitFor();
  await page.locator('#review-root table button').last().click();
  await page.getByRole('button', { name: 'Chỉnh sửa', exact: true }).click();
  await page.getByRole('dialog').locator('textarea').fill('Mục tiêu đã chỉnh sửa');
  await page.getByRole('button', { name: 'Lưu Thay Đổi', exact: true }).click();
  await page.getByRole('dialog').waitFor({ state: 'hidden' });
  assert.equal(lastEdit.title, 'Mục tiêu đã chỉnh sửa');
  const pageSizeRequest = page.waitForRequest(r => new URL(r.url()).searchParams.get('pageSize') === '25');
  await page.locator('#review-root').getByRole('combobox').last().click();
  await page.getByRole('option', { name: '25', exact: true }).click();
  assert.equal(new URL((await pageSizeRequest).url()).searchParams.get('pageNumber'), '1');
  await page.locator('#review-root table').getByText(goal.title, { exact: true }).waitFor();
  const docDownloadPromise = page.waitForEvent('download');
  await page.locator('#review-root').getByRole('button', { name: /Xuất Excel/ }).click();
  const docDownload = await docDownloadPromise;
  const docWb = XLSX.read(await fs.readFile(await docDownload.path()), { type: 'buffer' });
  assert.ok(XLSX.utils.sheet_to_json(docWb.Sheets[docWb.SheetNames[0]], { header: 1 }).flat().includes(goal.title));
  await page.evaluate(() => { window.reviewType.value = 'Task'; });
  await page.locator('#review-root table').getByText(task.title, { exact: true }).waitFor();
  failItems = true;
  await page.evaluate(() => { window.reviewType.value = 'Goal'; });
  await page.getByRole('alert').filter({ hasText: 'Không tải được danh sách' }).waitFor();
  failItems = false;
  await page.getByRole('button', { name: 'Thử lại', exact: true }).click();
  await page.locator('#review-root table').getByText(goal.title, { exact: true }).waitFor();
  const oldRequest = page.waitForRequest(r => new URL(r.url()).searchParams.get('search') === 'old');
  const search = page.locator('#review-root input').filter({ visible: true }).first();
  await search.fill('old');
  await oldRequest;
  const oldResponse = page.waitForResponse(r => new URL(r.url()).searchParams.get('search') === 'old');
  await search.fill('new');
  await page.locator('#review-root table').getByText('new response', { exact: true }).waitFor();
  await oldResponse;
  assert.equal(await page.locator('#review-root table').getByText('new response', { exact: true }).count(), 1, 'Older request must not replace the latest search');
  const legacy = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(legacy, XLSX.utils.aoa_to_sheet([['Café', 'Tiếng Việt']]), 'Legacy');
  const buffer = XLSX.write(legacy, { bookType: 'biff8', type: 'buffer' });
  const text = await page.evaluate(async bytes => {
    const { loadExcelReader } = await import('/tests/refactor-fixture.js');
    const reader = await loadExcelReader();
    const wb = reader.read(new Uint8Array(bytes), { type: 'array' });
    return reader.utils.sheet_to_json(wb.Sheets[wb.SheetNames[0]], { header: 1 });
  }, Array.from(buffer));
  assert.deepEqual(text, [['Café', 'Tiếng Việt']]);
  assert.deepEqual(errors, []);
  assert.deepEqual(warnings, []);
  console.log('Passed: dashboard/detail, drilldown, create modal, XLSX downloads with Vietnamese/merges, lazy Excel, legacy XLS, API failure/retry, no Vue warnings.');
} finally { await browser.close(); }
