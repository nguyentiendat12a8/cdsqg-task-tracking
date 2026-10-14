import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
import { installReportingClock } from './reporting-clock.mjs';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel:'msedge', headless:true });
try {
  const page = await browser.newPage();
  await installReportingClock(page, 2026);
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  const years = await page.evaluate(async () => {
    const { createApp, h } = await import('/tests/refactor-fixture.js');
    const { default: Select } = await import('/src/components/ReportYearSelect.vue');
    const { availableReportingYears, availableViewingYears, currentReportingYear, isValidReportingYear } = await import('/src/config/reporting.js');
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); document.body.appendChild(root);
    createApp({ render:()=>h(Select) }).mount(root);
    return { current:currentReportingYear(), options:availableReportingYears(), viewing:availableViewingYears(), valid:isValidReportingYear(2026), invalid:isValidReportingYear(2027) };
  });
  assert.deepEqual(years, { current:2026, options:[2026], viewing:[2026,2027,2028,2029,2030], valid:true, invalid:false });
  await page.getByRole('combobox', { name:'Năm báo cáo' }).click();
  assert.equal(await page.getByRole('option', { name:'Năm 2027', exact:true }).count(), 1);
  assert.equal(await page.getByRole('option', { name:'Năm 2026', exact:true }).count(), 1);
  await page.getByRole('option', { name:'Năm 2027', exact:true }).click();
  assert.equal(await page.evaluate(async () => (await import('/src/shared/reportYear.js')).reportYear.value), 2027);
  console.log('Passed future year viewing while report submission remains limited to current/past years. Clock fixed to 2026.');
} finally { await browser.close(); }
