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
function metrics(type,empty=false){const perf=empty?[]:agencies.map((a,i)=>{const g=performance(a,i,'Goal'),t=performance(a,i+3,'Task');return {...g,totalTasks:t.totalTasks,taskNotStarted:t.taskNotStarted,taskInProgress:t.taskInProgress,taskCompleted:t.taskCompleted,taskCompletionRate:t.taskCompletionRate};});const unique=empty?{notStarted:0,inProgress:0,completed:0}:type==='Goal'?{notStarted:11,inProgress:9,completed:7}:{notStarted:42,inProgress:25,completed:15};const prefix=type==='Goal'?'goal':'task';return {groupStatistics:empty?[]:[{itemType:'Goal',group:'Phát triển nhân lực',completionRate:25.93,total:27,notStarted:11,inProgress:9,completed:7},{itemType:'Task',group:'Phát triển hạ tầng',completionRate:18.29,total:82,notStarted:42,inProgress:25,completed:15}],totalGoals:empty?0:27,totalTasks:empty?0:82,overviewMetrics:{goalStatusSummary:unique,taskStatusSummary:{notStarted:42,inProgress:25,completed:15}},createdMetrics:{totalGoals:empty?0:27,goalsSpecific:empty?0:20,goalsGeneral:empty?0:7,totalTasks:empty?0:82,tasksSpecific:empty?0:60,tasksGeneral:empty?0:22},statusSummary:sum(perf),goalStatusSummary:sum(perf),taskStatusSummary:sum(perf.map(a=>({notStarted:a.taskNotStarted,inProgress:a.taskInProgress,completed:a.taskCompleted}))),ministriesPerformance:perf.filter(a=>a.type==='Ministry'),provincesPerformance:perf.filter(a=>a.type==='Province'),othersPerformance:perf.filter(a=>a.type==='Other')};}
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 for(const width of (process.env.TEST_WIDTH?[Number(process.env.TEST_WIDTH)]:[1440,390])){
  const page=await browser.newPage({viewport:{width,height:1000},acceptDownloads:true});page.setDefaultTimeout(12000);const errors=[],warnings=[],requests=[];let fail=false,empty=false,documentCountsFail=false;
  page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.text().includes('[Vue warn]'))warnings.push(m.text())});
  await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));localStorage.setItem('cdsqg_active_doc_code','1266')});
  await page.route('**/api/**',async route=>{const url=new URL(route.request().url());requests.push(url);const type=url.searchParams.get('itemType')||'Goal';let data=[];
   if(url.pathname==='/api/agencies')data=agencies;
   else if(url.pathname==='/api/documents'){
    if(documentCountsFail){await route.fulfill({status:500,json:{message:'test failure'}});return;}
    data={items:url.searchParams.get('pageNumber')==='2'?[{id:'12660000-0000-0000-0000-000000001266',totalGoals:3,totalTasks:5}]:[{id:'26290000-0000-0000-0000-000000002629',totalGoals:0,totalTasks:75}],totalCount:2};
   }

   else if(url.pathname==='/api/dashboard/metrics'){
    if(fail){await route.fulfill({status:500,json:{message:'test failure'}});return;}
    if(url.searchParams.has('detailMode'))data={items:[{id:'22222222-2222-2222-2222-222222222222',code:'MT-01',title:'Chi tiết mục tiêu đã chọn',leadAgencyName:'Bộ Công an',itemType:type,calculatedStatus:url.searchParams.get('detailStatus')||'Completed',latestProgressPercent:100}],totalCount:1,totalPages:1};
    else data=url.searchParams.has('parentAgencyId')?{...metrics(type,true)}:metrics(type,empty);
   }else if(url.pathname==='/api/dashboard/agency-items')data=[
    {id:'overdue-goal',code:'MT-QH',title:'Mục tiêu quá hạn của đơn vị',itemType:'Goal',status:'InProgress',deadlineStatus:'Overdue'},
    {id:'on-time-goal',code:'MT-TH',title:'Mục tiêu trong hạn của đơn vị',itemType:'Goal',status:'NotStarted',deadlineStatus:'OnTime'},
    {id:'overdue-task',code:'NV-QH',title:'Nhiệm vụ quá hạn của đơn vị',itemType:'Task',status:'NotStarted',deadlineStatus:'Overdue'},
    {id:'done-task',code:'NV-HT',title:'Nhiệm vụ đã hoàn thành muộn',itemType:'Task',status:'Completed',deadlineStatus:'CompletedOverdue'}
   ];
   await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5203',{waitUntil:'networkidle'});
  await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const {default:Header}=await import('/src/components/AppHeader.vue');f.authState.user.value={role:1,username:'fixture'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';document.documentElement.style.overflow='auto';document.body.style.overflow='auto';document.body.style.height='auto';const root=document.createElement('div');root.id='leadership-review';root.className='ui-density-compact';document.body.append(root);const app=f.createApp({render:()=>f.h('div',{},[f.h(Header,{user:{username:'fixture'}}),f.h('div',{style:'padding:20px;max-width:1680px;margin:auto;'},[f.h(f.Dashboard)])])});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);});
  const root=page.locator('#leadership-review');await root.locator('.executive-kpi').first().waitFor();

  assert.equal(await root.locator('.executive-kpi').count(),8);
  const grouped=root.getByRole('region',{name:'Thống kê theo nhóm',exact:true});
  assert.equal(await grouped.locator('canvas').count(),0);
  await grouped.getByRole('button',{name:'Mục tiêu',exact:true}).click();
  await grouped.getByRole('listitem').filter({hasText:'Phát triển nhân lực'}).waitFor();
  assert((await grouped.getByRole('listitem').filter({hasText:'Phát triển nhân lực'}).innerText()).includes('27'));
  await grouped.getByRole('button',{name:'Nhiệm vụ',exact:true}).click();
  await grouped.getByRole('listitem').filter({hasText:'Phát triển hạ tầng'}).waitFor();
  if(width>=1024){const map=await root.locator('.executive-map').boundingBox();const comparison=await root.getByRole('region',{name:'So sánh kết quả các đơn vị',exact:true}).boundingBox();assert(Math.abs(map.width-comparison.width)<2);assert(Math.abs(comparison.height-(await grouped.boundingBox()).height)<2);}

  const overdueList=root.getByRole('region',{name:'Danh sách đơn vị quá hạn',exact:true});
  assert.equal(Math.round((await overdueList.boundingBox()).height),96);
  assert.equal(await overdueList.getByRole('button').count(),agencies.filter((_,i)=>i%4>0).length);
  assert(await overdueList.evaluate(el=>el.scrollHeight>el.clientHeight));
  await overdueList.getByRole('button').first().click();
  await page.getByText('Mục tiêu quá hạn của đơn vị',{exact:true}).waitFor();
  assert.equal(await page.getByText('Mục tiêu trong hạn của đơn vị',{exact:true}).count(),0);
  await page.getByRole('button',{name:/Danh sách nhiệm vụ/}).click();
  await page.getByText('Nhiệm vụ quá hạn của đơn vị',{exact:true}).waitFor();
  assert.equal(await page.getByText('Nhiệm vụ đã hoàn thành muộn',{exact:true}).count(),0);
  await page.getByRole('button',{name:'Xem tất cả công việc',exact:true}).click();
  await page.getByText('Nhiệm vụ đã hoàn thành muộn',{exact:true}).waitFor();
  assert(requests.some(u=>u.pathname==='/api/dashboard/agency-items'&&u.searchParams.get('documentId')==='12660000-0000-0000-0000-000000001266'));
  await page.getByRole('button',{name:'Đóng chi tiết đơn vị',exact:true}).click();


  await root.getByRole('button',{name:'Chọn chiến lược',exact:true}).click();
  const picker=page.getByRole('group',{name:'Danh sách chiến lược',exact:true});await picker.waitFor();
  await picker.getByText('3 mục tiêu',{exact:true}).waitFor();await picker.getByText('5 nhiệm vụ',{exact:true}).waitFor();await picker.getByText('0 mục tiêu',{exact:true}).waitFor();assert.equal(await picker.getByText('27 mục tiêu',{exact:true}).count(),0);
  const chosen=picker.locator('[aria-pressed="true"]');assert((await chosen.innerText()).includes('Đang chọn'));
  const palette=await chosen.evaluate(el=>({background:getComputedStyle(el).backgroundColor,text:getComputedStyle(el.querySelector('p')).color}));assert.equal(palette.text,'rgb(255, 255, 255)');assert.notEqual(palette.background,'rgb(255, 255, 255)');assert.equal(await chosen.locator('.strategy-meta').evaluate(el=>getComputedStyle(el).color),'rgb(255, 255, 255)');
  await page.screenshot({path:`${process.env.TEMP}/strategy-contrast-${width}.png`,fullPage:true});
  await chosen.focus();await page.keyboard.press('ArrowDown');await page.keyboard.press('Enter');
  await page.waitForFunction(()=>localStorage.getItem('cdsqg_active_doc_code')==='2629');
  await root.getByRole('button',{name:'Chọn chiến lược',exact:true}).click();await picker.getByRole('button').first().click();
  await page.waitForFunction(()=>localStorage.getItem('cdsqg_active_doc_code')==='1266');
  documentCountsFail=true;
  await root.getByRole('button',{name:'Chọn chiến lược',exact:true}).click();
  await page.getByText('Chưa tải được số lượng mục tiêu, nhiệm vụ.',{exact:true}).waitFor();
  assert.equal(await picker.getByText('3 mục tiêu',{exact:true}).count(),0);
  documentCountsFail=false;await page.getByRole('button',{name:'Thử lại',exact:true}).click();
  await picker.getByText('3 mục tiêu',{exact:true}).waitFor();
  await picker.getByRole('button').first().click();


  if(width>=1280){const positions=await root.locator('.executive-kpi').evaluateAll(cards=>cards.map(card=>Math.round(card.getBoundingClientRect().top)));assert.equal(new Set(positions).size,1,'All eight KPI cards must share a row on desktop');}

  await root.locator('.province-shape').first().waitFor();
  assert.equal(await root.locator('.province-shape').count(),34);
  if(width>=1024){const map=await root.locator('.executive-map').boundingBox();const side=await root.locator('aside').boundingBox();assert(map.height>=528);assert(Math.abs(map.height-side.height)<2,'Map and right-hand panels must have matching heights');}

  await root.getByRole('region',{name:'Tổng quan nhiệm vụ',exact:true}).getByRole('button',{name:'Xem Đã hoàn thành — nhiệm vụ',exact:true}).click();
  await root.getByText('Chi tiết mục tiêu đã chọn',{exact:true}).waitFor();
  assert(requests.some(u=>u.searchParams.get('detailStatus')==='Completed'&&u.searchParams.get('itemType')==='Task'));
  await root.getByRole('button',{name:'Đóng danh sách thống kê'}).click();
  const before=requests.filter(u=>u.pathname==='/api/dashboard/metrics').length;
  await root.getByRole('group',{name:'Loại dữ liệu bản đồ',exact:true}).getByRole('button',{name:'Nhiệm vụ',exact:true}).click();
  await root.getByRole('heading',{name:'Bản đồ hoàn thành nhiệm vụ',exact:true}).waitFor();
  const board=root.getByRole('region',{name:'So sánh kết quả các đơn vị',exact:true});
  await board.getByRole('group',{name:'Loại dữ liệu so sánh',exact:true}).getByRole('button',{name:'Nhiệm vụ',exact:true}).click();
  assert.equal(await root.getByRole('group',{name:'Loại dữ liệu bản đồ',exact:true}).getByRole('button',{name:'Nhiệm vụ',exact:true}).getAttribute('aria-pressed'),'true');
  assert.equal(requests.filter(u=>u.pathname==='/api/dashboard/metrics').length,before);
  assert.equal(await board.getByRole('listitem').count(),agencies.length);
  await board.getByRole('listitem').first().click();
  await page.getByText('Nhiệm vụ đã hoàn thành muộn',{exact:true}).waitFor();
  assert.equal(await page.getByRole('button',{name:'Xem tất cả công việc',exact:true}).count(),0);
  await page.getByRole('button',{name:'Đóng chi tiết đơn vị',exact:true}).click();

  assert.equal(await board.getByRole('button',{name:'Trang đơn vị sau'}).count(),0);
  const scroll=board.getByRole('region',{name:'Cuộn ngang để xem tất cả đơn vị'});const scrollBox=await scroll.boundingBox(),boardBox=await board.boundingBox();assert(scrollBox.x-boardBox.x>=12);assert(boardBox.x+boardBox.width-scrollBox.x-scrollBox.width>=12);if(width<640)assert(await scroll.evaluate(el=>el.scrollWidth>el.clientWidth));
  assert(!(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1)),`Overflow ${width}`);
  await page.screenshot({path:`${process.env.TEMP}/dashboard-combined-${width}.png`,fullPage:true});
  assert.deepEqual(errors,[]);assert.deepEqual(warnings,[]);
  await page.close();
 }
 console.log('PASS combined dashboard desktop/mobile: 8 KPIs, 34 provinces, independent switches, chart, no overflow or Vue errors');
}finally{await browser.close();}
