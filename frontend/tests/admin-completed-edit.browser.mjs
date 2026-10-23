import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 for(const width of [1440,390])for(const itemType of ['Goal','Task']){
  const page=await browser.newPage({viewport:{width,height:900}});let sent=false;const queries=[];
  await page.route('**/api/**',async route=>{
   const url=new URL(route.request().url());let data=[];
   if(url.pathname==='/api/agencies')data=[{id:'11111111-1111-1111-1111-111111111111',name:'Bộ Công an',code:'BCA'}];
   if(url.pathname.endsWith('/items'))data={items:[{taskId:'22222222-2222-2222-2222-222222222222',code:'NV-2629-18',title:'Sửa báo cáo chờ duyệt',itemType,evaluationType:'Quantitative',unitName:'%',leadAgencyId:'11111111-1111-1111-1111-111111111111',leadAgencyName:'Bộ Công an',leadAssignmentAgencyIds:['11111111-1111-1111-1111-111111111111'],hasPendingApproval:false,calculatedStatus:'Completed'}],totalCount:1,totalPages:1};
   if(url.pathname.includes('/progress')){
    if(route.request().method()==='POST'){sent=true;assert.match(route.request().postData(),/ReplaceExistingFiles/);data={approvalStatus:'Approved'};}
    else {queries.push(url.searchParams.get('preferPending'));data={actualValue:100,summaryNotes:'Bản đã hoàn thành',approvalStatus:'Approved',attachmentFileUrls:[]};}
   }
   await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5211');
  await page.evaluate(async(itemType)=>{
   const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:1,agencyId:'11111111-1111-1111-1111-111111111111'};f.authState.token.value='fixture';
   document.querySelector('#app').style.display='none';const root=document.createElement('div');root.id='pending-test';document.body.append(root);
   const app=f.createApp({render:()=>f.h(f.Document,{filterItemType:itemType,subTab:'list'})});
   app.directive('accessible-dialog',f.accessibleDialog);app.mount(root);
  },itemType);
  const root=page.locator('#pending-test');await root.getByRole('button',{name:'Cập nhật tiến độ và minh chứng',exact:true}).click();
  const submit=root.locator('button[type="submit"]');await submit.waitFor();await page.waitForFunction(()=>document.querySelector('#pending-test textarea')?.value==='Bản đã hoàn thành');
  assert.equal(await submit.isEnabled(),true);assert.ok(queries.includes('true'));await root.locator('textarea').fill('Admin sửa thông tin đã hoàn thành');await submit.click();await page.waitForTimeout(300);assert.equal(sent,true);
  await page.close();
 }
 console.log('PASS: admin opens and updates completed goals/tasks from lists on desktop/mobile.');
}finally{await browser.close();}
