import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const browser = await chromium.launch({channel:'msedge',headless:true});
const bca={id:'11111111-1111-1111-1111-111111111111',agencyId:'11111111-1111-1111-1111-111111111111',name:'Bộ Công an',code:'BCA',type:'Ministry'};
const parent={id:'99999999-9999-9999-9999-999999999999',name:'Các bộ, ngành, địa phương',code:'ALL_AGENCIES',type:'Special'};
const item=(code,role)=>({id:code,taskId:code,code,title:role==='Coordinating'?'Mục phối hợp qua nhóm':'Mục được giao qua nhóm',itemType:'Goal',isGeneralTask:true,leadAgencyId:parent.id,leadAgencyName:parent.name,leadAgencyCode:parent.code,coordinatingAgencyIds:[parent.id],coordinatingAgencyNames:[parent.name],leadAssignmentAgencyIds:[bca.id],coordinatingAssignmentAgencyIds:[bca.id],progressAgencyId:bca.id,progressAgencyName:bca.name,assignmentKind:'Group',yearlyTargets:{},customBaseline:{},subItems:[],unitName:'%',calculatedStatus:role==='Coordinating'?'CompletedOnTime':'InProgressOnTime',status:role==='Coordinating'?'CompletedOnTime':'InProgressOnTime',latestProgressPercent:role==='Coordinating'?100:66.67,completionPercentage:role==='Coordinating'?100:66.67,startDate:'2026-01-01',dueDate:'2030-12-31'});
try {
 for (const width of [1440,390]) for (const mode of ['document','dashboard','reports']) {
  const page=await browser.newPage({viewport:{width,height:950},acceptDownloads:true});const errors=[],requests=[];
  page.on('pageerror',e=>errors.push(e.message));
  await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
  await page.route('**/api/**',async route=>{
   const url=new URL(route.request().url());requests.push(url);const role=url.searchParams.get('assignmentRole')||'Lead';const row=item(role==='Coordinating'?'MT-COORD':'MT-LEAD',role);let data=[];
   if(url.pathname==='/api/agencies')data=[bca,parent];
   else if(url.pathname==='/api/dashboard/metrics')data={totalGoals:1,goalStatusSummary:{completedOnTime:role==='Coordinating'?1:0,inProgressOnTime:role==='Lead'?1:0},ministriesPerformance:url.searchParams.has('parentAgencyId')?[]:[{...bca,totalGoals:1,totalItems:1,goalCompletedOnTime:role==='Coordinating'?1:0,goalInProgressOnTime:role==='Lead'?1:0,goalCompletionRate:role==='Coordinating'?100:0,completionRate:role==='Coordinating'?100:0}],provincesPerformance:[],othersPerformance:[]};
   else if(url.pathname==='/api/dashboard/agency-items')data=[row];
   else if(url.pathname.endsWith('/grid'))data={items:[row]};
   else if(url.pathname.endsWith('/items'))data={items:[row],totalCount:1,totalPages:1};
   await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5173',{waitUntil:'networkidle'});
  await page.evaluate(async mode=>{const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:1,username:'fixture'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const root=document.createElement('div');root.id='assignment-review';document.body.append(root);const View=mode==='document'?f.Document:mode==='dashboard'?f.Dashboard:f.Reports;const app=f.createApp({render:()=>f.h(View,{filterItemType:'Goal'})});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);},mode);
  const root=page.locator('#assignment-review'); await root.getByRole('button',{name:/Lọc nâng cao|Bộ lọc nâng cao/i}).click();
  const rolePicker=page.getByRole('combobox',{name:'Vai trò của cơ quan',exact:true});
  await rolePicker.waitFor(); await page.waitForTimeout(100);
  const rect=await rolePicker.boundingBox();assert(rect.y>=0 && rect.y+rect.height<=950);
  if(width===390 && mode==='reports')await page.screenshot({path:(process.env.TEMP||'.')+'/cdsqg-assignment-filter-mobile.png'});
  await rolePicker.click();await page.getByRole('option',{name:'Phối hợp',exact:true}).click();
  const agency=page.getByRole('combobox',{name:'Cơ quan phối hợp',exact:true});await agency.click();await page.getByRole('option',{name:'Bộ Công an',exact:true}).click();await agency.press('Escape');
  await page.getByRole('button',{name:'Áp dụng lọc',exact:true}).click();await page.waitForTimeout(500);
  const relevant=requests.filter(u=>u.searchParams.get('assignmentRole')==='Coordinating');assert(relevant.length>0);
  if(mode==='document'){assert(relevant.some(u=>u.searchParams.get('selectedAgencyIds')===bca.id));await root.getByText('Mục phối hợp qua nhóm',{exact:true}).first().waitFor();}
  if(mode==='dashboard'){
   assert(relevant.some(u=>u.pathname==='/api/dashboard/metrics'&&u.searchParams.getAll('agencyId').includes(bca.id)));
   await root.getByRole('tab',{name:'Bộ / Ngành'}).click();
   await root.getByRole('button',{name:'Biểu đồ tròn',exact:true}).filter({visible:true}).first().click();await root.getByRole('button',{name:'Xem chi tiết Bộ Công an',exact:true}).click();await root.getByText('Mục phối hợp qua nhóm',{exact:true}).first().waitFor();await root.getByRole('button',{name:'✕',exact:true}).click();
   assert(requests.some(u=>u.pathname==='/api/dashboard/agency-items'&&u.searchParams.get('assignmentRole')==='Coordinating'));
  }
  if(mode==='reports')assert(relevant.some(u=>u.pathname.endsWith('/grid')&&u.searchParams.get('selectedAgencyIds')===bca.id));
  const name=mode==='document'?/^Xuất Excel danh sách$/i:mode==='reports'?/^Xuất file Excel$/i:/^Xuất báo cáo Excel$/i;
  const [download]=await Promise.all([page.waitForEvent('download',{timeout:30000}),root.getByRole('button',{name}).click()]);
  const workbook=XLSX.read(await fs.readFile(await download.path()),{type:'buffer'});const text=JSON.stringify(workbook.Sheets);
  assert(text.includes(mode==='reports'?'Bộ Công an':'Mục phối hợp qua nhóm'));assert.deepEqual(errors,[]);
  console.log(`PASS ${mode} ${width}px: coordination group filter and XLSX`);await page.close();
 }
}finally{await browser.close();}
