import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const require = createRequire(process.env.PLAYWRIGHT_PACKAGE);
const { chromium } = require('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.route('**/api/**', route => route.fulfill({contentType:'application/json', body: JSON.stringify(
    route.request().url().includes('metrics') ? {totalGoals:0,totalTasks:0,ministriesPerformance:[],provincesPerformance:[],othersPerformance:[]} : {items:[],totalCount:0}) }));
  await page.goto('http://127.0.0.1:5201');
  await page.evaluate(() => {
    localStorage.setItem('cdsqg_auth_token', 'review-token');
    localStorage.setItem('cdsqg_auth_user', JSON.stringify({username:'review',fullName:'Kiểm tra giao diện',role:1}));
  });
  await page.reload();
  await page.waitForSelector('header');
  await page.setViewportSize({width:390,height:844});
  assert.equal(await page.locator('.app-sidebar').isVisible(), false);
  await page.getByRole('button', {name:'Mở hoặc đóng menu'}).click();
  assert.equal(await page.locator('.app-sidebar').isVisible(), true);
  await page.getByRole('button', {name:'Đóng menu',exact:true}).click({position:{x:370,y:20}});
  assert.equal(await page.locator('.app-sidebar').isVisible(), false);
  await page.screenshot({path:'../scratch/ui-mobile-review.png',fullPage:true});
  await page.setViewportSize({width:1440,height:900});
  assert.equal(await page.locator('.app-sidebar').isVisible(), true);
  await page.screenshot({path:'../scratch/ui-desktop-review.png',fullPage:true});
  await page.evaluate(async () => { const {confirmModal} = await import('/src/services/confirm.js'); confirmModal({title:'Kiểm tra xác nhận',message:'Thao tác bàn phím'}); });
  await page.getByRole('alertdialog').waitFor();
  await page.keyboard.press('Shift+Tab');
  assert.equal(await page.evaluate(() => document.activeElement.closest('[role="alertdialog"]') !== null),true);
  await page.keyboard.press('Escape');
  await page.getByRole('alertdialog').waitFor({state:'hidden'});
  assert.deepEqual(errors,[]);
  console.log('Passed: mobile menu, desktop sidebar, confirmation keyboard, no runtime errors with empty API fixtures.');
} finally {await browser.close();}
