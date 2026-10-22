import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const agency = { id:'11111111-1111-1111-1111-111111111111', agencyId:'11111111-1111-1111-1111-111111111111', name:'Bộ Công an', code:'BCA', type:'Ministry' };
const states = ['NotStarted','InProgress','Completed'];
const labels = ['Chưa thực hiện','Đang thực hiện','Đã hoàn thành'];
const rows = states.map((state,index)=>({ id:`22222222-2222-2222-2222-22222222222${index}`, taskId:`22222222-2222-2222-2222-22222222222${index}`, code:`MT-0${index+1}`, title:`Mục tiêu trạng thái ${index+1}`, itemType:'Goal', leadAgencyId:agency.id, leadAgencyName:agency.name, leadAssignmentAgencyIds:[agency.id], coordinatingAssignmentAgencyIds:[], calculatedStatus:state, status:state, deadlineStatus:index===0?'Overdue':index===1?'ExpiringSoon':'CompletedOverdue', section:'Mục A', unitName:'%', completionPercentage:index*50, latestProgressPercent:index*50, yearlyTargets:{}, customBaseline:{}, coordinatingAgencyIds:[], coordinatingAgencyNames:[], deliverables:[], dueDate:'2026-10-22' }));
const counts={notStarted:1,inProgress:1,completed:1};
const summary={...agency,...counts,totalItems:3,totalGoals:3,totalTasks:0,goalNotStarted:1,goalInProgress:1,goalCompleted:1,goalCompletionRate:33.33,completionRateOverall:33.33,expiringSoon:1,overdue:1,reportedCount:2,onTimeReports:0,warningCount:2};
const metrics={overviewMetrics:{goalStatusSummary:counts},totalGoals:3,totalTasks:0,createdMetrics:{totalGoals:3,goalsSpecific:3,goalsGeneral:0,goalStatusSummary:counts,statusSummary:counts},goalStatusSummary:counts,statusSummary:counts,ministriesPerformance:[summary],provincesPerformance:[],othersPerformance:[]};
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 for(const width of [1440,390]) for(const mode of ['dashboard','document','reports']) {
  const page=await browser.newPage({viewport:{width,height:960},acceptDownloads:true});page.setDefaultTimeout(12000);
  const errors=[],requests=[];page.on('pageerror',e=>errors.push(e.message));
  await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
  await page.route('**/api/**',async route=>{
   const url=new URL(route.request().url());requests.push(url);let data=[];
   if(url.pathname==='/api/agencies') data=[agency];
   else if(url.pathname==='/api/dashboard/metrics') {
    if(url.searchParams.has('detailMode')) { const selected=rows.filter(r=>!url.searchParams.get('detailStatus')||r.status===url.searchParams.get('detailStatus'));data={items:selected,totalCount:selected.length,totalPages:1}; }
    else data=url.searchParams.has('parentAgencyId')?{...metrics,ministriesPerformance:[]}:metrics;
   }
   else if(url.pathname==='/api/dashboard/agency-items')data=rows;
   else if(url.pathname.endsWith('/grid'))data={items:rows};
   else if(url.pathname.endsWith('/items')) {const selected=rows.filter(r=>!url.searchParams.get('selectedStatuses')||url.searchParams.get('selectedStatuses').split(',').includes(r.status));data={items:selected,totalCount:selected.length,totalPages:1};}
   await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5210',{waitUntil:'networkidle'});
  await page.evaluate(async mode=>{const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:1,username:'fixture'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const root=document.createElement('div');root.id='status-review';root.style.padding='16px';document.body.append(root);const View=mode==='dashboard'?f.Dashboard:mode==='document'?f.Document:f.Reports;const app=f.createApp({render:()=>f.h(View,{filterItemType:'Goal'})});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);},mode);
  const root=page.locator('#status-review');
  if(mode==='dashboard') {
   for(const label of labels)await root.getByRole('button',{name:`Xem ${label} — lượt giao theo cơ quan`,exact:true}).waitFor();
   assert.equal(await root.getByRole('button',{name:/Xem .*— lượt giao theo cơ quan/}).count(),3);
   await root.getByRole('button',{name:'Xem Đang thực hiện — lượt giao theo cơ quan',exact:true}).click();
   await root.getByText(rows[1].title,{exact:true}).filter({visible:true}).first().waitFor();
   assert(requests.some(u=>u.searchParams.get('detailStatus')==='InProgress'));
   await root.getByRole('button',{name:'Đóng danh sách thống kê'}).click();
   const board=root.getByRole('region',{name:'So sánh kết quả các đơn vị',exact:true});
   for(const label of labels)assert((await board.locator('thead').innerText()).includes(label));
  } else if(mode==='document') {
   await root.getByText(rows[0].title,{exact:true}).filter({visible:true}).first().waitFor();
   for(const label of labels)assert((await root.getByRole('table').innerText()).includes(label));
   await root.getByRole('button',{name:/Lọc nâng cao/i}).click();
   await page.getByRole('combobox',{name:'Trạng thái hạng mục',exact:true}).click();
   assert.deepEqual(await page.getByRole('option').allTextContents(),labels);
   await page.getByRole('option',{name:'Đang thực hiện',exact:true}).click();
   await page.keyboard.press('Escape');await Promise.all([page.waitForResponse(r=>new URL(r.url()).searchParams.get('selectedStatuses')==='InProgress'), page.getByRole('button',{name:'Áp dụng lọc',exact:true}).click()]);
   await root.getByText(rows[1].title,{exact:true}).filter({visible:true}).first().waitFor();
   assert.equal(await root.getByText(rows[0].title,{exact:true}).filter({visible:true}).count(),0);
  } else {
   await root.locator('tbody tr').first().waitFor();
   for(const label of labels)assert((await root.locator('thead').innerText()).includes(label));
   assert(!(await root.locator('thead').innerText()).includes('Sắp hết hạn'));
   const [download]=await Promise.all([page.waitForEvent('download'),root.getByRole('button',{name:'Xuất file Excel',exact:true}).click()]);
   const book=XLSX.read(await fs.readFile(await download.path()),{type:'buffer'});const cells=XLSX.utils.sheet_to_json(book.Sheets[book.SheetNames[0]],{header:1}).flat();
   for(const label of labels)assert(cells.includes(label));
   assert(!cells.includes('Hoàn thành (quá hạn)'));assert(!cells.includes('Sắp hết hạn'));
   await root.getByRole('button',{name:/Báo cáo 2/}).click();
   await root.getByText('🚨 Quá hạn thực hiện',{exact:true}).waitFor();
   await root.getByText('⏰ Sắp hết hạn',{exact:true}).waitFor();
   assert.equal(await root.getByText(rows[2].title,{exact:true}).count(),0);
   await root.getByRole('button',{name:/Báo cáo 4/}).click();
   await root.locator('thead').getByText('Đã hoàn thành',{exact:true}).waitFor();
  }
  assert.deepEqual(errors,[]);
  await page.screenshot({path:`${process.env.TEMP}/cdsqg-three-status-${mode}-${width}.png`,fullPage:true});
  console.log(`PASS ${mode} ${width}px: three states, filters/drilldown, warnings/Excel`);await page.close();
 }
}finally{await browser.close();}
