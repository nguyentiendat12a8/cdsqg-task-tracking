import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
  const errors=[]; page.on('pageerror',error=>errors.push(error.message));
  const items=Array.from({length:27},(_,i)=>({taskId:`goal-${i}`,code:`MT-${String(i+1).padStart(2,'0')}`,title:`Mục tiêu ${i+1}`,itemType:'Goal',evaluationType:'Quantitative',yearlyTargets:{2026:i+1},leadAgencyName:'Cơ quan kiểm thử',startDate:'2026-01-01',dueDate:'2030-12-31'}));
  await page.route('**/api/**',route=>route.fulfill({contentType:'application/json',body:JSON.stringify(route.request().url().includes('/grid')?{items,dynamicYears:[2026]}:[])}));
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5201');
  await page.evaluate(async()=>{
    const f=await import('/tests/refactor-fixture.js'); const {default:Grid}=await import('/src/components/DynamicPlanningGrid.vue');
    f.authState.user.value={role:1}; f.authState.token.value='fixture-token'; document.querySelector('#app').style.display='none';
    const root=document.createElement('div');root.id='grid-test';document.body.appendChild(root);
    const app=f.createApp({render:()=>f.h(Grid,{documentId:'test',filterItemType:'Goal'})});app.use(f.FloatingVue);app.mount(root);
  });
  const root=page.locator('#grid-test'); await root.getByText('Trang 1 / 3',{exact:true}).waitFor();
  assert.equal(await root.locator('tbody tr').count(),10);
  await root.getByRole('button',{name:'Trang sau'}).click();await root.getByText('Trang 2 / 3',{exact:true}).waitFor();assert.match(await root.locator('tbody tr').first().innerText(),/MT-11/);
  await root.getByRole('button',{name:'Trang sau'}).click();await root.getByText('Trang 3 / 3',{exact:true}).waitFor();assert.equal(await root.locator('tbody tr').count(),7);
  await root.getByPlaceholder('Tìm theo mã, tên chỉ tiêu kế hoạch...').fill('MT-27');await root.getByText('Trang 1 / 1',{exact:true}).waitFor();assert.equal(await root.locator('tbody tr').count(),1);
  await root.getByPlaceholder('Tìm theo mã, tên chỉ tiêu kế hoạch...').fill('');await root.getByText('Trang 1 / 3',{exact:true}).waitFor();
  await root.getByRole('combobox').last().click();
  await page.getByRole('option',{name:'25',exact:true}).click();
  await root.getByText('Trang 1 / 2',{exact:true}).waitFor();assert.equal(await root.locator('tbody tr').count(),25);
  await page.setViewportSize({width:390,height:844});assert(await root.getByText('Số bản ghi/trang:',{exact:true}).isVisible());
  await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const root=document.createElement('div');root.id='title-test';document.body.appendChild(root);const app=f.createApp({render:()=>f.h(f.Document,{filterItemType:'Goal',subTab:'grid'})});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);});
  await page.locator('#title-test h2').filter({hasText:'Danh sách thiết lập kế hoạch mục tiêu'}).waitFor();
  assert.deepEqual(errors,[]);console.log('PASS grid pagination: 10 rows, next/last page, filter reset, mobile controls, grid title');
} finally { await browser.close(); }
