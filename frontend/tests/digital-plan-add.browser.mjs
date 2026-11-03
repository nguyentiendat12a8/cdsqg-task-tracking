import assert from 'node:assert/strict';
import { createRequire } from 'node:module';

const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  for (const width of [1440, 390]) {
    const page = await browser.newPage({ viewport: { width, height: 850 } });
    page.setDefaultTimeout(15000);
    let savedBatch = null;
    await page.addInitScript(() => {
      localStorage.setItem('cdsqg_auth_token', 'fixture');
      localStorage.setItem('cdsqg_auth_user', JSON.stringify({ role: 1, username: 'fixture' }));
    });
    await page.route('**/api/**', route => {
      const path = new URL(route.request().url()).pathname;
      let data = [];
      if (path === '/api/digital-news') data = { items: [], total: 0, page: 1, pageSize: 10 };
      if (path === '/api/digital-news/settings' && route.request().method() === 'PUT') {
        savedBatch = route.request().postDataJSON().batchSize;
        data = { message: 'Đã lưu lịch quét tin.' };
      } else if (path === '/api/digital-news/settings') data = {
        enabled: true, intervalMinutes: 30, batchSize: 12,
        sources: [
          { id: 'source-1', name: 'Nguồn 1', agencyName: 'Bộ 1', domain: 'one.gov.vn', requiredTerms: '', enabled: true, sortOrder: 1 },
          { id: 'source-2', name: 'Nguồn 2', agencyName: 'Bộ 2', domain: 'two.gov.vn', requiredTerms: '', enabled: true, sortOrder: 2 },
        ],
        keywords: [
          { id: 'keyword-1', name: 'Nhóm 1', phrase: 'cụm 1', conditions: '', enabled: true, sortOrder: 1 },
          { id: 'keyword-2', name: 'Nhóm 2', phrase: 'cụm 2', conditions: '', enabled: true, sortOrder: 2 },
        ],
      };
      if (path === '/api/digital-news/agencies') data = [];
      if (path === '/api/notification') data = { items: [], unreadCount: 0 };
      return route.fulfill({ json: data });
    });
    await page.goto((process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173') + '/#digital-news');
    await page.getByRole('button', { name: 'Cấu hình theo dõi', exact: true }).click();
    const modal = page.getByRole('dialog');
    await modal.getByLabel('Số truy vấn mỗi lượt', { exact: false }).fill('20');
    await modal.getByRole('button', { name: 'Lưu lịch quét', exact: true }).click();
    await modal.waitFor({ state: 'hidden' });
    assert.equal(savedBatch, 20);
    await page.getByRole('button', { name: 'Cấu hình theo dõi', exact: true }).click();
    await modal.getByRole('button', { name: 'Nguồn theo dõi', exact: true }).click();
    await modal.getByRole('button', { name: '+ Thêm nguồn', exact: true }).click();
    const sourceInput = modal.locator('[data-new-entry]');
    assert.equal(await modal.locator('article').first().locator('[data-new-entry]').count(), 1);
    assert.equal(await sourceInput.evaluate(el => el === document.activeElement), true);
    await modal.getByRole('button', { name: 'Từ khóa thu thập', exact: true }).click();
    await modal.getByRole('button', { name: '+ Thêm từ khóa', exact: true }).click();
    const keywordInput = modal.locator('[data-new-entry]');
    assert.equal(await modal.locator('article').first().locator('[data-new-entry]').count(), 1);
    assert.equal(await keywordInput.evaluate(el => el === document.activeElement), true);
    console.log(`${width}px passed`);
    await page.close();
  }
} finally {
  await browser.close();
}
