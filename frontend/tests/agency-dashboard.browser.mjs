import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
const ownId='11111111-1111-1111-1111-111111111111',childId='22222222-2222-2222-2222-222222222222';
const summary=(id,name,type)=>({agencyId:id,id,name,type,totalItems:35,totalGoals:4,totalTasks:31,goalNotStarted:1,goalInProgress:2,goalCompleted:1,taskNotStarted:25,taskInProgress:3,taskCompleted:3,notStarted:26,inProgress:5,completed:4,overdue:1,expiringSoon:0,completionRateOverall:11.43});
try {
 for(const width of [1440,390])for(const type of ['Ministry','Province']){
  const page=await browser.newPage({viewport:{width,height:1000}});page.setDefaultTimeout(15000);let fail=false;const errors=[],requests=[];
  const own=summary(ownId,type==='Ministry'?'Bộ Công an':'Hà Nội',type),child=summary(childId,'Đơn vị trực thuộc','Other');
  const empty={ministriesPerformance:[],provincesPerformance:[],othersPerformance:[]};
  const metrics={...empty,overviewMetrics:{},createdMetrics:{},groupStatistics:[{group:'Phát triển nhân lực',itemType:'Task',total:31,notStarted:25,inProgress:3,completed:3,completionRate:9.68}],[type==='Province'?'provincesPerformance':'ministriesPerformance']:[own]};
  page.on('pageerror',error=>errors.push(error.message));
  await page.addInitScript(({ownId,name})=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:2,username:'agency',agencyId:ownId,agencyName:name}));localStorage.setItem('cdsqg_active_doc_code','1266');},{ownId,name:own.name});
  await page.route('**/api/**',async route=>{const url=new URL(route.request().url());requests.push(url);let data=[];
   if(url.pathname==='/api/agencies')data=[{id:ownId,name:own.name,type,code:'OWN'},...(type==='Ministry'?[{id:childId,name:child.name,type:'Other',parentId:ownId}]:[])];
   if(url.pathname==='/api/dashboard/metrics')data=url.searchParams.has('parentAgencyId')?(type==='Ministry'?{...empty,othersPerformance:[child]}:empty):metrics;
   if(url.pathname==='/api/dashboard/agency-items'){
    if(fail){await route.fulfill({status:500,json:{message:'fixture failure'}});return;}
    data=[{id:'task-overdue',code:'NV-01',title:'Công việc quá hạn của chính đơn vị',itemType:'Task',status:'InProgress',deadlineStatus:'Overdue',dueDate:'2026-01-01'}, {id:'task-active',code:'NV-02',title:'Công việc trong hạn của chính đơn vị',itemType:'Task',status:'NotStarted',deadlineStatus:'OnTime',dueDate:'2030-12-31'},{id:'goal-done',code:'MT-01',title:'Mục tiêu đã hoàn thành của đơn vị',itemType:'Goal',status:'Completed',deadlineStatus:'CompletedOnTime'}];
   }
   await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5207',{waitUntil:'networkidle'});
  await page.evaluate(async({ownId,name})=>{const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:2,agencyId:ownId,agencyName:name,username:'agency'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';document.documentElement.style.overflow='auto';document.body.style.overflow='auto';document.body.style.height='auto';const root=document.createElement('div');root.id='agency-review';root.className='ui-density-compact';root.style='padding:16px';document.body.append(root);const app=f.createApp({render:()=>f.h(f.Dashboard)});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);},{ownId,name:own.name});
  const root=page.locator('#agency-review'),queue=root.getByRole('region',{name:'Công việc của đơn vị',exact:true});
  await queue.getByText('Công việc quá hạn của chính đơn vị',{exact:true}).waitFor();
  assert.equal(await root.locator('.executive-kpi').count(),8);
  if(width>=1024){const left=await queue.boundingBox(),right=await root.locator('.agency-dashboard-sidebar').boundingBox();assert(Math.abs(left.y-right.y)<2);assert(Math.abs(left.y+left.height-right.y-right.height)<2,'Agency columns must align at bottom');}

  assert.equal(await root.getByText('Phân bổ theo khối',{exact:true}).count(),0);assert.equal(await root.getByText('PHẠM VI ĐƯỢC GIAO',{exact:true}).count(),0);
  assert.equal(await root.getByTestId('dashboard-comparison-groups').count(),0);assert.equal(await root.getByTestId('province-map-panel').count(),0);
  assert.equal(await root.getByLabel('Phân công và lượt giao',{exact:true}).count(),0);
  assert.equal(await root.getByRole('region',{name:'Kết quả các đơn vị trực thuộc',exact:true}).count(),type==='Ministry'?1:0);
  assert(requests.some(url=>url.pathname==='/api/dashboard/agency-items'&&url.searchParams.get('agencyId')===ownId&&url.searchParams.has('documentId')));
  await queue.getByRole('searchbox',{name:'Tìm công việc của đơn vị'}).fill('trong hạn');assert.equal(await queue.getByRole('listitem').count(),1);
  await queue.getByRole('searchbox').fill('');await queue.getByRole('button',{name:'Mục tiêu',exact:true}).click();await queue.getByText('Mục tiêu đã hoàn thành của đơn vị',{exact:true}).waitFor();
  await queue.getByRole('button',{name:'Nhiệm vụ',exact:true}).click();
  await page.screenshot({path:`${process.env.TEMP}/agency-dashboard-${type}-${width}.png`,fullPage:true});
  assert(!(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1)),`overflow ${type} ${width}`);
  fail=true;await page.evaluate(async()=>{const {switchDocument}=await import('/src/services/documentContext.js');switchDocument('2629');});
  await queue.getByText('Không tải được công việc của đơn vị.',{exact:true}).waitFor();fail=false;await queue.getByRole('button',{name:'Thử lại',exact:true}).click();await queue.getByText('Công việc quá hạn của chính đơn vị',{exact:true}).waitFor();

  metrics[type==='Province'?'provincesPerformance':'ministriesPerformance']=[summary('99999999-9999-9999-9999-999999999999','Đơn vị khác không được dùng làm thay thế',type)];
  await page.evaluate(async()=>{const {switchDocument}=await import('/src/services/documentContext.js');switchDocument('1266');});
  await page.waitForFunction(()=>document.querySelector('#agency-review [aria-busy]')?.getAttribute('aria-busy')==='false');
  assert((await root.locator('.executive-kpi').first().innerText()).includes('0'));
  assert.equal(await root.getByText('Đơn vị khác không được dùng làm thay thế',{exact:true}).count(),0);
  if(width===1440){
   await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:1,username:'admin'};});
   await root.getByTestId('province-map-panel').waitFor();
   assert.equal(await root.getByTestId('dashboard-comparison-groups').count(),1);
   assert.equal(await root.getByText('Phân bổ theo khối',{exact:true}).count(),1);
   assert.equal(await root.getByRole('region',{name:'Công việc của đơn vị',exact:true}).count(),0);
  }
  assert.deepEqual(errors,[]);await page.close();
 }
 console.log('PASS agency dashboards: ministries/provinces, desktop/mobile, scoped work queue, search/type switch, children only when present, error/retry');
}finally{await browser.close();}
