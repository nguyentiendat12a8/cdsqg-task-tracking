import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ acceptDownloads: true });
  const errors = [], requests = [];
  let mode = 'normal';
  page.on('pageerror', e => errors.push(e.message));
  page.on('request', r => requests.push(r.url()));
  const states = {notStarted:1,inProgress:0,completed:0};
  const agency = {agencyId:'11111111-1111-1111-1111-111111111111',name:'Cơ quan tiếng Việt',code:'TEST',type:'Ministry',totalGoals:1,totalTasks:1,totalItems:2,goalNotStarted:1,goalInProgress:0,goalCompleted:0,taskNotStarted:0,taskInProgress:1,taskCompleted:0,completionRateOverall:0,overdue:1,expiringSoon:0};
  const metrics = {totalGoals:1,totalTasks:1,overviewMetrics:{totalGoals:1,totalTasks:1,goalStatusSummary:states,taskStatusSummary:{notStarted:0,inProgress:1,completed:0}},goalStatusSummary:{notStarted:2,inProgress:0,completed:0},taskStatusSummary:{notStarted:0,inProgress:2,completed:0},ministriesPerformance:[agency,{...agency,agencyId:'22222222-2222-2222-2222-222222222222',name:'Đơn vị thứ hai'}],groupStatistics:[]};
  await page.addInitScript(() => {
    localStorage.setItem('cdsqg_active_doc_code','2629');
    localStorage.setItem('cdsqg_auth_token', 'fixture');
    localStorage.setItem('cdsqg_auth_user', JSON.stringify({ role: 1, username: 'review', fullName: 'Kiểm thử Excel' }));
  });
  await page.route('**/api/**', route => {
    const url = new URL(route.request().url());
    if (mode === 'failure' && url.pathname.endsWith('/agency-items')) return route.fulfill({status:500,json:{}});
    let data = [];
    if (url.pathname === '/api/agencies') data = [{...agency,id:agency.agencyId}];
    if (url.pathname === '/api/dashboard/metrics') data = mode === 'empty' ? {overviewMetrics:{totalGoals:0,totalTasks:0,goalStatusSummary:{notStarted:0,inProgress:0,completed:0},taskStatusSummary:{notStarted:0,inProgress:0,completed:0}},goalStatusSummary:{notStarted:0,inProgress:0,completed:0},taskStatusSummary:{notStarted:0,inProgress:0,completed:0},ministriesPerformance:[]} : metrics;
    if (url.pathname === '/api/dashboard/agency-items') data = [
      {id:'goal1',itemType:'Goal',title:'Mục tiêu tiếng Việt',code:'MT-01',startDate:'2025-12-01T00:00:00',dueDate:'2027-12-31',latestProgressValue:0,latestProgressPercent:0,calculatedStatus:'NotStarted',deadlineStatus:'OnTime'},
      {id:'task1',itemType:'Task',title:'Nhiệm vụ tiếng Việt',code:'NV-01',latestProgressValue:null,latestProgressPercent:50.5,calculatedStatus:'InProgress',deadlineStatus:'Overdue',deliverables:[{title:'Báo cáo triển khai',currentStatus:'Completed'}]}
    ];
    return route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  const button=page.getByRole('button',{name:/Xuất báo cáo Excel/i});
  await button.waitFor(); await page.waitForFunction(()=>!Array.from(document.querySelectorAll('button')).find(b=>/Xuất báo cáo Excel/i.test(b.textContent))?.disabled);
  assert.equal(requests.some(u => /xlsx\.(?:bundle|min)/.test(u)), false);
  const [download] = await Promise.all([page.waitForEvent('download'),button.click()]);
  assert.match(download.suggestedFilename(),/^Bao_cao_trang_chu_2629_\d{4}-\d{2}-\d{2}\.xlsx$/);
  const bytes=await fs.readFile(await download.path());
  if(process.env.EXCEL_QA_PATH) await fs.writeFile(process.env.EXCEL_QA_PATH,bytes);
  const wb=XLSX.read(bytes,{type:'buffer',cellStyles:true,cellNF:true});
  assert.deepEqual(wb.SheetNames,['Tổng hợp','So sánh đơn vị','Chi tiết mục tiêu','Chi tiết nhiệm vụ']);
  assert.match(wb.Sheets['Tổng hợp'].A2.v,/2629\/QĐ-TTg/);
  assert.equal(wb.Sheets['Tổng hợp'].B5.v,1);
  assert.equal(wb.Sheets['Tổng hợp'].B7.v,2);
  const goals=wb.Sheets['Chi tiết mục tiêu'],tasks=wb.Sheets['Chi tiết nhiệm vụ'];
  assert.equal(goals.C5.v,'Mục tiêu tiếng Việt');assert.equal(goals.B5.v,goals.B6.v);assert.notEqual(goals.D5.v,goals.D6.v);
  assert.equal(goals.N5.v,0);assert.equal(goals.N5.t,'n');
  assert.equal(goals.P5.v,0);assert.equal(goals.P5.z,'0.0%');
  assert.equal(goals.K5.t,'n');assert.equal(XLSX.SSF.format(goals.K5.z,goals.K5.v),'01/12/2025');
  assert.ok(!tasks.N5 || tasks.N5.v==='');assert.equal(tasks.P5.v,0.505);
  assert.equal(tasks.Q5.v,'Đang thực hiện');assert.match(tasks.S5.v,/Báo cáo triển khai.*Đã hoàn thành/);
  assert.ok(goals['!autofilter']);assert.ok(goals.A1.s?.fgColor);
  assert.equal(wb.Sheets['So sánh đơn vị'].L5.v,0);
  for (const sheet of Object.values(wb.Sheets)) assert.ok(!XLSX.utils.sheet_to_json(sheet,{header:1}).flat().some(value=>String(value).startsWith('Báo cáo:')));
  assert.deepEqual(XLSX.utils.sheet_to_json(wb.Sheets['So sánh đơn vị'],{header:1})[3].slice(0,2),['STT','Tên đơn vị']);
  assert.ok(!XLSX.utils.sheet_to_json(wb.Sheets['So sánh đơn vị'],{header:1})[3].includes('Khối'));
  assert.ok(!XLSX.utils.sheet_to_json(wb.Sheets['So sánh đơn vị'],{header:1})[3].includes('Mã đơn vị'));
  assert.equal(requests.some(u => /xlsx\.min/.test(u)), false);
  // Exercise captured date filters directly with the same application export module.
  await page.goto(process.env.FRONTEND_FIXTURE_URL || 'http://127.0.0.1:5173');await page.evaluate(()=>document.querySelector('#app').__vue_app__?.unmount());requests.length=0;
  async function directExport(){await page.evaluate(async()=>{
    const {ref}=await import('/tests/refactor-fixture.js');
    const {exportDashboardReport}=await import('/src/features/dashboard/dashboardReport.js');
    const values={assignmentRole:'Lead',dashboardFilter:'all',selectedScopes:[],selectedSections:[],selectedGroups:[],reportFromYear:2025,reportToYear:2026,executionFrom:'2025-12-01',executionTo:'2026-12-31',isOngoingOnly:true,isExportingExcel:false};
    const refs=Object.fromEntries(Object.entries(values).map(([k,v])=>[k,ref(v)]));window.exportState=refs;
    await exportDashboardReport({...refs,filterOutSpecialAgencies:x=>x});
  });}
  const [filtered]=await Promise.all([page.waitForEvent('download'),directExport()]);
  const filteredWb=XLSX.read(await fs.readFile(await filtered.path()),{type:'buffer'});
  assert.match(filteredWb.Sheets['Tổng hợp'].A3.v,/2025.*2026.*2025-12-01.*2026-12-31.*Có/);
  for(const url of requests.filter(u=>u.includes('/api/dashboard/'))){const q=new URL(url).searchParams;for(const [k,v]of Object.entries({documentId:'26290000-0000-0000-0000-000000002629',reportFromYear:'2025',reportToYear:'2026',executionFrom:'2025-12-01',executionTo:'2026-12-31',isOngoing:'true'}))assert.equal(q.get(k),v);}
  let downloads=0;page.on('download',()=>downloads++);mode='failure';await directExport();assert.equal(downloads,0);assert.equal(await page.evaluate(()=>window.exportState.isExportingExcel.value),false);
  mode='empty';const [empty]=await Promise.all([page.waitForEvent('download'),directExport()]);
  const emptyWb=XLSX.read(await fs.readFile(await empty.path()),{type:'buffer'});assert.equal(emptyWb.SheetNames.length,4);assert.equal(emptyWb.Sheets['Tổng hợp'].B6.v,0);assert.equal(emptyWb.Sheets['Chi tiết mục tiêu']['!autofilter'].ref,'A5:S5');
  assert.deepEqual(errors,[]);
  console.log('Passed: real homepage download, four styled Unicode sheets, 2629 metadata, numeric dates/percent/zero, missing values, products, matching filters, failed fetch produces no file, empty export, lazy writer.');
} finally { await browser.close(); }
