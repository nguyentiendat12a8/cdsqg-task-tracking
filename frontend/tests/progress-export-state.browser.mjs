import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const b=await chromium.launch({channel:'msedge',headless:true});
try{
 const p=await b.newPage({viewport:{width:1366,height:700},acceptDownloads:true});let release,exportRequests=0,fail=false;
 await p.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 const data={agencyId:'agency',agencyName:'Cục Chuyển đổi số quốc gia',goals:[],tasks:[{id:'task',documentId:'doc',documentNumber:'2629/QĐ-TTg',code:'NV-01',title:'Nhiệm vụ giữ nguyên khi xuất',status:'InProgress',statusLabel:'Đang thực hiện'}],totalGoals:0,totalTasks:1,totalCount:1,totalPages:1,page:1};
 await p.route('**/api/**',async r=>{const u=new URL(r.request().url());if(u.pathname==='/api/agencies')return r.fulfill({json:[{id:'agency',name:'Cục Chuyển đổi số quốc gia'}]});if(u.pathname==='/api/planning/agency-progress-report'){if(u.searchParams.has('fetchAll')){exportRequests++;await new Promise(resolve=>release=resolve);if(fail)return r.fulfill({status:500,json:{}});}return r.fulfill({json:data});}return r.fulfill({json:[]});});
 await p.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5189')+'/#reports-progress');
 await p.locator('#main-content').getByRole('button',{name:/Nhiệm vụ/}).first().click();await p.getByText(data.tasks[0].title,{exact:true}).waitFor();
 await p.evaluate(()=>{window.__exportTable=document.querySelector('#main-content table');window.__documentMarker='same-document';});let navigations=0;p.on('framenavigated',f=>{if(f===p.mainFrame())navigations++;});
 const originalUrl=p.url();const button=p.getByRole('button',{name:'Xuất excel',exact:true});
 for(const shouldFail of [false,true]){
  fail=shouldFail;const request=p.waitForRequest(r=>r.url().includes('fetchAll=true'));const download=shouldFail?null:p.waitForEvent('download');await button.click();await request;
  const busy=p.getByRole('button',{name:'Đang xuất Excel…',exact:true});assert(await busy.isDisabled());
  assert(await p.getByText(data.tasks[0].title,{exact:true}).isVisible());assert.equal(await p.getByText('Đang tải dữ liệu báo cáo tiến độ...',{exact:true}).count(),0);
  assert(await p.evaluate(()=>window.__exportTable===document.querySelector('#main-content table')&&window.__exportTable.isConnected));
  const before=exportRequests;await busy.dispatchEvent('click');assert.equal(exportRequests,before);release();
  if(download){const file=await download;assert.equal(await file.failure(),null);assert.match(file.suggestedFilename(),/^Bao_Cao_Tien_Do_Cục_Chuyển_đổi_số_quốc_gia_\d{4}-\d{2}-\d{2}\.xlsx$/);}
  await button.waitFor();await p.waitForFunction(()=>document.querySelector('button[aria-busy="false"]')!==null);
  assert.equal(p.url(),originalUrl);assert.equal(navigations,0);assert(await p.evaluate(()=>window.__documentMarker==='same-document'&&window.__exportTable===document.querySelector('#main-content table')));
 }
 console.log('PASS export download/error keeps table mounted, prevents duplicate export and never navigates');
}finally{await b.close();}
