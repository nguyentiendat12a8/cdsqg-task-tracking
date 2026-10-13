import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
const base = process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173';
try {
  for (const width of [1440, 390]) for (const [component, endpoint, markerField] of [
    ['views/LegalDocumentsView.vue', '/api/legaldocuments', 'title'],
    ['views/UrgeHistoryView.vue', '/api/execution/urge-logs', 'taskTitle'],
    ['components/ImportHistoryAudit.vue', '/api/documents/import-history', 'fileName']
  ]) {
    const page = await browser.newPage({ viewport: { width, height: 950 }, acceptDownloads: true });
    const errors = [], requests = []; let failSecond = false;
    page.on('pageerror', error => errors.push(error.message));
    await page.route('**/api/**', async route => {
      const url = new URL(route.request().url()); let data = [];
      if (url.pathname === endpoint) {
        requests.push(url);
        const size = Number(url.searchParams.get('pageSize')), number = Number(url.searchParams.get('pageNumber'));
        if (failSecond && size === 100 && number === 2) return route.fulfill({ status: 500, json: {} });
        // 101 rows forces two export requests even when the server caps the page size.
        const actualSize = size === 100 ? 60 : size;
        const start = (number - 1) * actualSize;
        data = { items: Array.from({ length: Math.max(0, Math.min(actualSize, 101 - start)) }, (_, i) => ({ id: `row-${start+i}`, [markerField]: `Bản ghi tiếng Việt ${start+i}`, code: `VB-${start+i}`, taskCode: `NV-${start+i}`, importedAt: '2026-10-13', createdAt: '2026-10-13', issuedDate: '2026-10-13', category: 'Khác', fileType: 'Khác', status: 'Thành công', attachments: [] })), totalCount: 101, totalPages: Math.ceil(101 / actualSize), pageNumber: number, pageSize: actualSize };
      }
      return route.fulfill({ json: data });
    });
    await page.addInitScript(() => { localStorage.setItem('cdsqg_auth_token', 'fixture'); localStorage.setItem('cdsqg_auth_user', JSON.stringify({role:1,username:'fixture'})); });
    await page.goto(base, {waitUntil:'networkidle'});
    await page.evaluate(async component => {
      const f = await import('/tests/refactor-fixture.js');
      f.authState.user.value = { role: 1, username: 'fixture' }; f.authState.token.value = 'fixture';
      document.querySelector('#app').style.display = 'none';
      const root = document.createElement('div'); root.id = 'export-test'; document.body.appendChild(root);
      const { default: View } = await import('/src/' + component);
      const app = f.createApp(View); app.use(f.FloatingVue); app.directive('accessible-dialog', f.accessibleDialog); app.directive('accessible-data', f.accessibleData); app.mount(root);
    }, component);
    const root = page.locator('#export-test');
    const button = root.getByRole('button', { name: /Xuất.*Excel/ });
    await root.getByText('Bản ghi tiếng Việt 0', { exact: true }).first().waitFor();
    await page.waitForFunction(() => !document.querySelector('#export-test button[aria-busy]')?.disabled);
    const search = root.locator('input[placeholder^="Tìm"]').first();
    await search.fill('Bản ghi tiếng Việt'); await search.press('Enter');
    await page.waitForFunction(() => !document.querySelector('#export-test button[aria-busy]')?.disabled);
    await page.waitForTimeout(500);
    const original = requests.filter(u => u.pathname === endpoint).at(-1);
    assert.equal(original.searchParams.get(component.includes('Legal') ? 'searchQuery' : 'search'), 'Bản ghi tiếng Việt');
    const pending = page.waitForEvent('download'); await button.click(); const download = await pending;
    const workbook = XLSX.read(await fs.readFile(await download.path()), { type: 'buffer' });
    const cells = XLSX.utils.sheet_to_json(workbook.Sheets[workbook.SheetNames[0]], { header: 1 }).flat();
    const markers = cells.filter(c => typeof c === 'string' && c.startsWith('Bản ghi tiếng Việt '));
    assert.equal(markers.length, 101); assert.equal(new Set(markers).size, 101); assert(markers.includes('Bản ghi tiếng Việt 100'));
    const exportRequests = requests.filter(u => u.searchParams.get('pageSize') === '100');
    assert.equal(exportRequests.length, 2);
    for (const url of exportRequests) for (const [key,value] of original.searchParams) if (!['pageNumber','pageSize'].includes(key)) assert.equal(url.searchParams.get(key),value);
    // A failed later page must not create a partial workbook.
    failSecond = true; let partial = false; page.on('download', () => partial = true);
    await page.waitForFunction(() => document.querySelector('#export-test button[aria-busy]')?.getAttribute('aria-busy') === 'false');
    await button.dispatchEvent('click'); await page.getByText(/Không tải được dữ liệu xuất Excel/).first().waitFor({timeout:7000}).catch(async e=>{console.log('DEBUG',component,errors,requests.slice(-5).map(u=>u.toString()),await page.locator('.Toastify').allTextContents(),await button.getAttribute('aria-busy'));throw e;}); assert.equal(partial, false);
    assert.deepEqual(errors, []); console.log(`PASS ${component}: 101 rows, no partial download, ${width}px`); await page.close();
  }
} finally { await browser.close(); }
