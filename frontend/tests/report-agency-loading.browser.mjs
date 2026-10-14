import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel:'msedge', headless:true });
try {
  const page = await browser.newPage();
  const agencies = [
    { id:'11111111-1111-1111-1111-111111111111', name:'Đơn vị A', code:'A', type:'Ministry' },
    { id:'22222222-2222-2222-2222-222222222222', name:'Đơn vị B', code:'B', type:'Province' },
    { id:'33333333-3333-3333-3333-333333333333', name:'Đơn vị C', code:'C', type:'Province' },
    { id:'44444444-4444-4444-4444-444444444444', name:'Các bộ, ngành, địa phương', code:'ALL_AGENCIES', type:'Special' },
    { id:'55555555-5555-5555-5555-555555555555', name:'Các doanh nghiệp viễn thông', code:'TELECOM', type:'Special' }
  ];
  let releaseB;
  const gate = new Promise(resolve => { releaseB=resolve; });
  await page.route('**/api/**', async route => {
    const url = new URL(route.request().url());
    let data = [];
    if (url.pathname === '/api/agencies') data=agencies;
    if (url.pathname.endsWith('/progress')) {
      const id = url.searchParams.get('agencyId');
      if (id === agencies[1].id) await gate;
      data = id === agencies[0].id ? { summaryNotes:'Ghi chú của A', deliverables:[{id:'p1',title:'Sản phẩm',currentStatus:'Completed'}] }
        : id === agencies[1].id ? {summaryNotes:'Ghi chú của B'} : null;
    }
    await route.fulfill({ json:data });
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  await page.evaluate(async agencies => {
    const f = await import('/tests/refactor-fixture.js');
    f.authState.user.value={role:1}; f.authState.token.value='';
    document.querySelector('#app').style.display='none';
    const root=document.createElement('div');root.id='agency-report-test';document.body.append(root);
    const app=f.createApp({render:()=>f.h(f.AnnualProgress,{isOpen:true,taskId:'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      taskCode:'NV_TEST',taskTitle:'Nhiệm vụ chung',evaluationType:'Qualitative',isGeneralTask:true,
      leadAgencyId:agencies[0].id,agencies,deliverables:[{id:'p1',title:'Sản phẩm',currentStatus:'NotStarted'}]})});
    app.directive('accessible-dialog',f.accessibleDialog);app.mount(root);
  }, agencies);
  const root=page.locator('#agency-report-test');
  const notes=root.getByPlaceholder('Tóm tắt kết quả triển khai hoặc khó khăn vướng mắc...');
  await page.waitForFunction(()=>document.querySelector('#agency-report-test textarea')?.value==='Ghi chú của A');
  const selector=root.getByRole('combobox').first();
  await selector.click();
  assert.equal(await page.getByRole('option',{name:'Các bộ, ngành, địa phương',exact:true}).count(),1);
  assert.equal(await page.getByRole('option',{name:'Các doanh nghiệp viễn thông',exact:true}).count(),0);
  await page.getByRole('option',{name:'Đơn vị B',exact:true}).click();
  await root.getByText('Đang tải báo cáo của đơn vị đã chọn...', {exact:true}).waitFor();
  assert.equal(await notes.isVisible(),false);
  assert.equal(await root.getByRole('button',{name:'Gửi báo cáo tiến độ',exact:true}).isDisabled(),true);
  await selector.click(); await page.getByRole('option',{name:'Đơn vị C',exact:true}).click();
  await notes.waitFor({state:'visible'});
  assert.equal(await notes.inputValue(),'');
  releaseB(); await page.waitForTimeout(150);
  assert.equal(await notes.inputValue(),'');
  assert.equal(await root.getByRole('combobox').nth(2).innerText(),'Chưa thực hiện');
  console.log('Passed agency change loading, disabled submit, blank agency resets products/notes, stale response ignored, known groups enabled and enterprise group hidden.');
} finally { await browser.close(); }
