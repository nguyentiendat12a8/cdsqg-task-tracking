import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.route('**/api/**', route => route.fulfill({ contentType: 'application/json', body: JSON.stringify(route.request().url().includes('/legaldocuments?') ? {
    items: [{ id: 'copy-test', code: '148/2025/QH15', documentType: 'Luật', issuedDate: '2025-12-11', issuingAgencyName: 'Quốc hội', title: 'Luật Chuyển đổi số', attachments: [] }], totalCount: 1, totalPages: 1
  } : []) }));
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://localhost:5173');
  await page.evaluate(async () => {
    const fixture = await import('/tests/refactor-fixture.js');
    const { default: Legal } = await import('/src/views/LegalDocumentsView.vue');
    fixture.authState.user.value = { role: 1, username: 'review' };
    fixture.authState.token.value = 'fixture-token';
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'copy-root'; document.body.appendChild(root);
    Object.defineProperty(navigator, 'clipboard', { configurable: true, value: { writeText: async text => { window.copiedText = text; } } });
    fixture.createApp(Legal).use(fixture.FloatingVue).mount(root);
    const { formatLegalDocumentReference: format } = await import('/src/shared/legalDocumentText.js');
    if (format({ documentType: 'Quyết định', title: 'Ban hành Kế hoạch' }) !== 'Quyết định Ban hành Kế hoạch') throw Error('Missing fields or duplicate ban hành');
  });
  const button = page.locator('#copy-root').getByRole('button', { name: 'Sao chép thông tin văn bản 148/2025/QH15' });
  await button.click();
  assert.equal(await page.evaluate(() => window.copiedText), 'Luật số 148/2025/QH15 ngày 11/12/2025 của Quốc hội ban hành Luật Chuyển đổi số');
  await page.setViewportSize({ width: 390, height: 844 });
  await button.scrollIntoViewIfNeeded();
  assert.equal(await button.isVisible(), true);
  assert.deepEqual(errors, []);
  console.log('Passed legal reference copy, missing fields, desktop/mobile. API and clipboard mocked.');
} finally { await browser.close(); }
