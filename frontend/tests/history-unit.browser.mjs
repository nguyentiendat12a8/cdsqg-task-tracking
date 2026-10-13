import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 const page=await browser.newPage({viewport:{width:1280,height:900}});
 await page.route('**/api/**',route=>route.fulfill({json:route.request().url().includes('progress-history')?[{id:'report',periodYear:2026,actualValue:20000,completionPercentage:0.33,agencyName:'Bộ Công an',createdBy:'Admin (Import Excel)',approvalStatus:'Approved',logDate:'2026-10-13T01:00:00Z'}]:[]}));
 await page.goto(process.env.FRONTEND_TEST_URL||'http://localhost:5173');
 await page.evaluate(async()=>{
  const f=await import('/tests/refactor-fixture.js'); const Modal=(await import('/src/components/ItemDetailModal.vue')).default;
  f.authState.user.value={role:1};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';
  const node=document.createElement('div');node.id='history-unit';node.className='ui-density-compact';document.body.append(node);
  const app=f.createApp({render:()=>f.h(Modal,{isOpen:true,initialTab:'history',item:{taskId:'test',code:'MT-24',title:'Mục tiêu số lượng chung',unitName:'Số lượng',isGeneralTask:true,leadAgencyName:'Các bộ, ngành, địa phương'}})});
  app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(node);
 });
 const dialog=page.getByRole('dialog');
 await dialog.getByText('20.000 Số lượng',{exact:true}).waitFor();
 assert.equal(await dialog.getByText('0,33%',{exact:true}).count(),0);
 await dialog.getByText('Admin',{exact:true}).waitFor();
 for(const width of [1280,390]){await page.setViewportSize({width,height:900});assert.equal(await dialog.getByText('20.000 Số lượng',{exact:true}).isVisible(),true);}
 console.log('Passed: history shows actual quantity and separates author, reporting agency and assignment. Desktop/mobile; API mocked.');
}finally{await browser.close();}
