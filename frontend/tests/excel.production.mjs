import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ acceptDownloads: true });
  const errors = [], requests = [];
  page.on('pageerror', e => errors.push(e.message));
  page.on('request', r => requests.push(r.url()));
  await page.addInitScript(() => {
    localStorage.setItem('cdsqg_auth_token', 'fixture');
    localStorage.setItem('cdsqg_auth_user', JSON.stringify({ role: 1, username: 'review', fullName: 'Kiểm thử production' }));
  });
  await page.route('**/api/**', route => {
    const url = new URL(route.request().url());
    const agency = { agencyId: '11111111-1111-1111-1111-111111111111', name: 'Cơ quan tiếng Việt', code: 'TEST', totalGoals: 1, totalItems: 1, type: 'Ministry' };
    let data = [];
    if (url.pathname === '/api/agencies') data = [{ ...agency, id: agency.agencyId }];
    if (url.pathname === '/api/dashboard/metrics') data = { totalGoals: 1, ministriesPerformance: url.searchParams.has('parentAgencyId') ? [] : [agency] };
    if (url.pathname === '/api/dashboard/agency-items') data = [{ itemType: 'Goal', title: 'Mục tiêu tiếng Việt', code: 'MT001' }];
    return route.fulfill({ contentType: 'application/json', body: JSON.stringify(data) });
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5202');
  await page.getByText('🏢 Khối Các Bộ / Ngành Trung Ương', { exact: true }).waitFor();
  assert.equal(requests.some(u => /xlsx\.(?:bundle|min)/.test(u)), false);
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: /Xuất Báo Cáo Excel/ }).click();
  const download = await pending;
  const wb = XLSX.read(await fs.readFile(await download.path()), { type: 'buffer', cellStyles: true });
  assert.equal(wb.SheetNames.length, 2);
  assert.ok(wb.Sheets[wb.SheetNames[0]].A1.s?.fgColor);
  assert.ok(XLSX.utils.sheet_to_json(wb.Sheets[wb.SheetNames[1]], { header: 1 }).flat().includes('Mục tiêu tiếng Việt'));
  assert.equal(requests.some(u => /xlsx\.min/.test(u)), false);
  assert.deepEqual(errors, []);
  console.log('Passed: production dashboard downloads styled Unicode XLSX using only the smaller writer chunk.');
} finally { await browser.close(); }
