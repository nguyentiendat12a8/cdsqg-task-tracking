import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX=createRequire(import.meta.url)('xlsx-js-style');
const names=['Bộ Công an','Bộ Tài chính','Bộ Khoa học và Công nghệ','Bộ Công Thương','Bộ Giáo dục và Đào tạo','Bộ Nội vụ','Bộ Tư pháp','Bộ Nông nghiệp và Môi trường','Hà Nội','Hải Phòng','Đà Nẵng','Cần Thơ','Hồ Chí Minh','Khánh Hòa','Đơn vị triển khai khác'];
const agencies=names.map((name,index)=>({id:`11111111-1111-1111-1111-${String(index+1).padStart(12,'0')}`,agencyId:`11111111-1111-1111-1111-${String(index+1).padStart(12,'0')}`,name,code:`TEST${index}`,type:index<8?'Ministry':index<14?'Province':'Other'}));
function performance(a,index,type='Goal'){
 const total=12+index,completed=Math.round(total*(index%5)*.2),inProgress=Math.floor((total-completed)*.6),notStarted=total-completed-inProgress;
 const prefix=type==='Goal'?'goal':'task';
 return {...a,totalItems:total,totalGoals:type==='Goal'?total:0,totalTasks:type==='Task'?total:0,notStarted,inProgress,completed,[prefix+'NotStarted']:notStarted,[prefix+'InProgress']:inProgress,[prefix+'Completed']:completed,[prefix+'CompletionRate']:Number((completed/total*100).toFixed(2)),completionRateOverall:Number((completed/total*100).toFixed(2)),overdue:index%4,expiringSoon:index%3,reportedCount:inProgress+completed,onTimeReports:completed,warningCount:index%4+index%3};
}
const sum=rows=>rows.reduce((s,a)=>({notStarted:s.notStarted+a.notStarted,inProgress:s.inProgress+a.inProgress,completed:s.completed+a.completed}),{notStarted:0,inProgress:0,completed:0});
function metrics(type,empty=false){const perf=empty?[]:agencies.map((a,i)=>performance(a,i,type));const unique=empty?{notStarted:0,inProgress:0,completed:0}:type==='Goal'?{notStarted:11,inProgress:9,completed:7}:{notStarted:42,inProgress:25,completed:15};const prefix=type==='Goal'?'goal':'task';return {totalGoals:empty?0:27,totalTasks:empty?0:82,overviewMetrics:{[prefix+'StatusSummary']:unique},createdMetrics:{totalGoals:empty?0:27,goalsSpecific:empty?0:20,goalsGeneral:empty?0:7,totalTasks:empty?0:82,tasksSpecific:empty?0:60,tasksGeneral:empty?0:22},statusSummary:sum(perf),[prefix+'StatusSummary']:sum(perf),ministriesPerformance:perf.filter(a=>a.type==='Ministry'),provincesPerformance:perf.filter(a=>a.type==='Province'),othersPerformance:perf.filter(a=>a.type==='Other')};}
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 for(const width of (process.env.TEST_WIDTH?[Number(process.env.TEST_WIDTH)]:[1440,390])){
  const page=await browser.newPage({viewport:{width,height:1000},acceptDownloads:true});page.setDefaultTimeout(12000);const errors=[],warnings=[],requests=[];let fail=false,empty=false;
  page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.text().includes('[Vue warn]'))warnings.push(m.text())});
  await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));localStorage.setItem('cdsqg_active_doc_code','1266')});
  await page.route('**/api/**',async route=>{const url=new URL(route.request().url());requests.push(url);const type=url.searchParams.get('itemType')||'Goal';let data=[];
   if(url.pathname==='/api/agencies')data=agencies;
   else if(url.pathname==='/api/dashboard/metrics'){
    if(fail){await route.fulfill({status:500,json:{message:'test failure'}});return;}
    if(url.searchParams.has('detailMode'))data={items:[{id:'22222222-2222-2222-2222-222222222222',code:'MT-01',title:'Chi tiết mục tiêu đã chọn',leadAgencyName:'Bộ Công an',itemType:type,calculatedStatus:url.searchParams.get('detailStatus')||'Completed',latestProgressPercent:100}],totalCount:1,totalPages:1};
    else data=url.searchParams.has('parentAgencyId')?{...metrics(type,true)}:metrics(type,empty);
   }else if(url.pathname==='/api/dashboard/agency-items')data=[{id:'22222222-2222-2222-2222-222222222222',code:'MT-01',title:'Mục tiêu của đơn vị',leadAgencyName:'Bộ Công an',itemType:'Goal',calculatedStatus:'InProgress',status:'InProgress',latestProgressPercent:60}];
   await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5203',{waitUntil:'networkidle'});
  await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const {default:Header}=await import('/src/components/AppHeader.vue');f.authState.user.value={role:1,username:'fixture'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';document.documentElement.style.overflow='auto';document.body.style.overflow='auto';document.body.style.height='auto';const root=document.createElement('div');root.id='leadership-review';root.className='ui-density-compact';document.body.append(root);const app=f.createApp({render:()=>f.h('div',{},[f.h(Header,{user:{username:'fixture'}}),f.h('div',{style:'padding:20px;max-width:1680px;margin:auto;'},[f.h(f.Dashboard)])])});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);});
  const root=page.locator('#leadership-review');await root.locator('.executive-kpi').first().waitFor();
  assert.equal(await root.locator('.executive-kpi').count(),4);
  assert((await root.locator('.executive-kpi').first().innerText()).includes('27'));
  await root.locator('.province-shape').first().waitFor();assert.equal(await root.locator('.province-shape').count(),34);
  assert.equal(await root.getByText('Văn bản theo dõi',{exact:true}).count(),0);
  const overflow=await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1);if(overflow){await page.screenshot({path:`${process.env.TEMP}/leadership-overflow-${width}.png`,fullPage:true});console.log(await page.evaluate(()=>[...document.querySelectorAll('#leadership-review *')].filter(e=>e.getClientRects().length&&e.getBoundingClientRect().right>innerWidth+1).slice(0,12).map(e=>({tag:e.tagName,cls:e.className,width:e.getBoundingClientRect().width,right:e.getBoundingClientRect().right}))));}assert(!overflow,`page overflow at ${width}`);
  await page.screenshot({path:`${process.env.TEMP}/leadership-home-${width}.png`,fullPage:true});
  if(width===1440){const boardBox=await root.getByRole('region',{name:'So sánh kết quả các đơn vị',exact:true}).boundingBox();assert(boardBox.y<850,'Comparison board should be close to the first screen');}
  const selected=root.getByRole('button',{name:'Xem Đã hoàn thành — danh sách',exact:true}).first();await selected.click();await root.getByText('Chi tiết mục tiêu đã chọn',{exact:true}).waitFor();assert(requests.some(u=>u.searchParams.get('detailStatus')==='Completed'&&u.searchParams.get('detailScope')==='all'));await root.getByRole('button',{name:'Đóng danh sách thống kê'}).click();
  const board=root.getByRole('region',{name:'So sánh kết quả các đơn vị',exact:true});
  const search=board.getByRole('searchbox');await search.fill('Bộ Công an');await board.getByRole('button',{name:'Xem chi tiết Bộ Công an',exact:true}).waitFor();assert.equal(await board.locator('tbody tr').count(),1);await search.fill('');
  await board.getByRole('button',{name:'Bộ, ngành',exact:true}).click();assert.equal(await board.locator('tbody tr').count(),6);await board.getByRole('button',{name:'Trang đơn vị sau'}).click();assert.equal(await board.locator('tbody tr').count(),2);
  await board.getByRole('button',{name:'Tất cả',exact:true}).click();
  const advanced=root.getByRole('button',{name:/Bộ lọc nâng cao/});
  await advanced.click();await page.getByRole('combobox',{name:'Loại đối tượng',exact:true}).click();await page.getByRole('option',{name:/Nhiệm vụ/}).click();await page.getByRole('button',{name:'Đóng',exact:true}).click();assert((await root.locator('.executive-kpi').first().innerText()).includes('27'),'Closing a draft must preserve applied data/type');
  await advanced.click();await page.getByRole('combobox',{name:'Vai trò của cơ quan',exact:true}).click();await page.getByRole('option',{name:'Phối hợp',exact:true}).click();await Promise.all([page.waitForResponse(r=>new URL(r.url()).pathname==='/api/dashboard/metrics'&&new URL(r.url()).searchParams.get('assignmentRole')==='Coordinating'),page.getByRole('button',{name:'Áp dụng lọc',exact:true}).click()]);await page.getByRole('combobox',{name:'Vai trò của cơ quan',exact:true}).waitFor({state:'hidden'});
  await page.waitForFunction(()=>!document.querySelector('#leadership-review .executive-dashboard')?.getAttribute('aria-busy')||document.querySelector('#leadership-review .executive-dashboard').getAttribute('aria-busy')==='false');await advanced.click();await page.getByRole('combobox',{name:'Vai trò của cơ quan',exact:true}).waitFor();await page.getByRole('button',{name:'Đặt lại',exact:true}).click();await page.getByRole('button',{name:'Đóng',exact:true}).click();
  const switcher=root.getByRole('button',{name:'Chọn chiến lược',exact:true});await switcher.focus();await switcher.press('Enter');const choices=page.getByRole('group',{name:'Danh sách chiến lược',exact:true});await choices.waitFor();
  const panel=page.locator('.strategy-options');const bounds=await panel.boundingBox();assert(bounds.x>=0&&bounds.x+bounds.width<=width+1);await page.waitForTimeout(250);assert.equal(await panel.evaluate(el=>getComputedStyle(el).backgroundColor),'rgb(255, 255, 255)');await page.screenshot({path:`${process.env.TEMP}/strategy-picker-${width}.png`,animations:'disabled'});
  const other=choices.getByRole('button').filter({hasText:'2629/QĐ-TTg'});await other.focus();await other.press('Enter');await choices.waitFor({state:'hidden'});assert.equal(await page.evaluate(()=>localStorage.getItem('cdsqg_active_doc_code')),'2629');await root.getByRole('heading',{name:'Toàn cảnh thực hiện Chính phủ số',exact:true}).waitFor();assert(requests.some(u=>u.pathname==='/api/dashboard/metrics'&&u.searchParams.get('documentId')==='26290000-0000-0000-0000-000000002629'));
  await switcher.press('Enter');await choices.waitFor();await choices.getByRole('button').first().focus();await page.keyboard.press('Escape');await choices.waitFor({state:'hidden'});assert(await switcher.evaluate(el=>el===document.activeElement));
  const tasks=root.getByRole('button',{name:'Nhiệm vụ',exact:true});await tasks.click();await page.waitForFunction(()=>document.querySelector('#leadership-review .executive-kpi')?.textContent.includes('82'));assert((await root.locator('.executive-kpi').first().innerText()).includes('82'));assert(requests.some(u=>u.searchParams.get('itemType')==='Task'));
  await root.getByRole('button',{name:'Mục tiêu',exact:true}).click();await root.locator('.executive-kpi').first().waitFor();
  const [download]=await Promise.all([page.waitForEvent('download'),root.getByRole('button',{name:'Xuất báo cáo Excel',exact:true}).click()]);const wb=XLSX.read(await fs.readFile(await download.path()),{type:'buffer'});assert(Object.keys(wb.Sheets).length>1);const flat=XLSX.utils.sheet_to_json(wb.Sheets[wb.SheetNames[0]],{header:1}).flat();for(const text of ['Chưa thực hiện','Đang thực hiện','Đã hoàn thành'])assert(flat.includes(text));
  fail=true;await root.getByRole('button',{name:'Nhiệm vụ',exact:true}).click();await root.getByRole('alert').filter({hasText:'Không tải được thống kê'}).waitFor();fail=false;await root.getByRole('button',{name:'Thử lại',exact:true}).click();await root.getByRole('alert').filter({hasText:'Không tải được thống kê'}).waitFor({state:'hidden'});
  empty=true;await root.getByRole('button',{name:'Mục tiêu',exact:true}).click();await root.getByText('Chưa có dữ liệu phân công trong phạm vi này.',{exact:true}).waitFor();assert.equal(await root.locator('.executive-kpi').first().getAttribute('disabled'),'');
  assert.deepEqual(errors,[]);assert.deepEqual(warnings,[]);console.log(`PASS redesigned dashboard ${width}px: map, counts, drilldown, board, strategy, keyboard, Excel, error/retry/empty`);await page.close();
 }
 {
  const page=await browser.newPage({viewport:{width:1280,height:900}});const requests=[];
  const own=agencies[0];
  await page.addInitScript(agencyId=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:2,username:'unit',agencyId}));},own.agencyId);
  await page.route('**/api/**',async route=>{const url=new URL(route.request().url());requests.push(url);let data=[];
    if(url.pathname==='/api/agencies')data=[own];
    if(url.pathname==='/api/dashboard/metrics')data=url.searchParams.has('parentAgencyId')?metrics('Goal',true):{...metrics('Goal'),ministriesPerformance:[performance(own,0)],provincesPerformance:[],othersPerformance:[]};
    if(url.pathname==='/api/dashboard/agency-items')data=[{id:'22222222-2222-2222-2222-222222222222',code:'MT-OWN',title:'Mục tiêu của đơn vị',itemType:'Goal',status:'InProgress',calculatedStatus:'InProgress',leadAgencyName:own.name,latestProgressPercent:50}];
    await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5203',{waitUntil:'networkidle'});
  await page.evaluate(async own=>{const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:2,username:'unit',agencyId:own.agencyId};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const root=document.createElement('div');root.id='unit-review';document.body.append(root);const app=f.createApp({render:()=>f.h(f.Dashboard)});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);},own);
  const root=page.locator('#unit-review');await root.locator('.executive-kpi').first().waitFor();assert((await root.locator('.executive-kpi').first().innerText()).includes('12'));
  assert.equal(await root.locator('[data-testid="province-map-panel"]').count(),0);
  assert.equal(await root.getByRole('button',{name:/Giao chung/}).count(),0);
  await root.getByRole('button',{name:'Xem toàn bộ danh sách',exact:true}).click();await root.getByText('Mục tiêu của đơn vị',{exact:true}).waitFor();
  assert(!requests.some(u=>u.searchParams.has('detailMode')));assert(requests.some(u=>u.pathname==='/api/dashboard/agency-items'&&u.searchParams.get('agencyId')===own.agencyId));
  console.log('PASS agency user: own progress and own drilldown, no admin-only statistics');await page.close();
 }
}finally{await browser.close();}
