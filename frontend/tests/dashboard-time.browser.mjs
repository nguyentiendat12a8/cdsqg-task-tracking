import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const runtime=process.env.PLAYWRIGHT_PACKAGE || import.meta.url;
const {chromium}=createRequire(runtime)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
const base=process.env.FRONTEND_TEST_URL || 'http://localhost:5173';
async function chooseYear(page,label,year){await page.getByRole('combobox',{name:label,exact:true}).click();await page.getByRole('option',{name:String(year),exact:true}).click();}
async function chooseDate(page,label,value){await page.getByLabel(label,{exact:true}).click();const calendar=page.locator('.v-popper__popper--shown').getByRole('dialog',{name:'Chọn ngày',exact:true});await calendar.getByLabel('Năm',{exact:true}).selectOption(value.slice(0,4));await calendar.getByLabel('Tháng',{exact:true}).selectOption(String(Number(value.slice(5,7))-1));await calendar.getByRole('button',{name:value,exact:true}).click();}
try{
 for(const width of [1440,390]){
  const page=await browser.newPage({viewport:{width,height:950}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
  await page.addInitScript(()=>localStorage.setItem('cdsqg_active_doc_code','2629'));
  await page.goto(base);
  await page.evaluate(async()=>{
   const {createApp,h}=await import('/tests/refactor-fixture.js');
   const {default:Header}=await import('/src/features/dashboard/DashboardHeader.vue');
   const {useDashboardFilters}=await import('/src/features/dashboard/useDashboardFilters.js');
   const {buildDashboardQuery}=await import('/src/features/dashboard/queries.js');
   const {ref}=await import('/tests/refactor-fixture.js');
   const {switchDocument,activeDocumentId}=await import('/src/services/documentContext.js');switchDocument('2629');
   const {FloatingVue}=await import('/tests/refactor-fixture.js');
   document.querySelector('#app').style.display='none';const root=document.createElement('div');root.id='dashboard-filter-review';root.style.padding='16px';document.body.append(root);window.filterCalls=[];
   createApp({setup(){let f;const load=()=>{const value=Object.fromEntries(Object.entries(f).filter(([,v])=>v&&typeof v==='object'&&'value' in v).map(([k,v])=>[k,v.value]));window.filterCalls.push(buildDashboardQuery({...value,documentId:activeDocumentId.value}).toString());};f=useDashboardFilters({agencies:ref([]),loadDashboardMetrics:load});window.filterState=f;
   return()=>h(Header,{yearOptions:f.yearOptions.value,activeDashboardFilterCount:f.activeDashboardFilterCount.value,resetDashboardFilters:f.resetDashboardFilters,loadDashboardMetrics:load,exportDashboardExcelReport:()=>{},isLoading:false,isExportingExcel:false,...Object.fromEntries(['reportFromYear','reportToYear','executionFrom','executionTo','isOngoingOnly'].flatMap(key=>[[key,f[key].value],[`onUpdate:${key}`,v=>f[key].value=v]]))});}}).use(FloatingVue).mount(root);
  });
  const open=()=>page.getByRole('button',{name:/Bộ lọc nâng cao/}).click();
  await open();const fields=page.getByTestId('dashboard-time-filters');await fields.waitFor();
  const panel=fields.locator('..').locator('..');const box=await panel.boundingBox();assert.ok(box.x>=15 && box.x+box.width<=width-15,`Popup inset at ${width}: ${JSON.stringify(box)}`);
  for(const old of ['Phạm vi','Vai trò của cơ quan','Cơ quan chủ trì','Đơn vị trực thuộc','Mục (Phụ lục)','Nhóm trọng tâm'])assert.equal(await fields.getByText(old,{exact:true}).count(),0);
  await chooseYear(page,'Từ năm',2025);assert.equal(await page.evaluate(()=>window.filterCalls.length),0);
  await page.getByRole('button',{name:'Đóng',exact:true}).click();await open();assert.equal(await page.evaluate(()=>window.filterState.reportFromYear.value),null);
  await chooseYear(page,'Từ năm',2028);await chooseYear(page,'Đến năm',2026);assert.ok(await page.getByRole('alert').filter({hasText:'Năm báo cáo từ'}).isVisible());assert.ok(await page.getByRole('button',{name:'Áp dụng lọc',exact:true}).isDisabled());
  await page.getByRole('button',{name:'Đặt lại',exact:true}).click();assert.equal(await page.evaluate(()=>window.filterCalls.length),1);
  await chooseYear(page,'Từ năm',2025);await chooseYear(page,'Đến năm',2026);await chooseDate(page,'Từ ngày','2026-01-01');await chooseDate(page,'Đến ngày','2026-12-31');await fields.getByRole('checkbox',{name:'Nhiệm vụ thường xuyên',exact:true}).check();
  await page.getByRole('button',{name:'Áp dụng lọc',exact:true}).click();await fields.waitFor({state:'hidden'});
  const query=new URLSearchParams(await page.evaluate(()=>window.filterCalls.at(-1)));for(const [key,value]of Object.entries({reportFromYear:'2025',reportToYear:'2026',executionFrom:'2026-01-01',executionTo:'2026-12-31',isOngoing:'true'}))assert.equal(query.get(key),value);
  assert.equal(await page.evaluate(()=>window.filterState.activeDashboardFilterCount.value),3);
  await open();await chooseDate(page,'Từ ngày','2027-01-01');assert.ok(await page.getByRole('alert').filter({hasText:'Ngày hoàn thành từ'}).isVisible());assert.ok(await page.getByRole('button',{name:'Áp dụng lọc',exact:true}).isDisabled());
  await page.getByRole('button',{name:'Đặt lại',exact:true}).click();assert.equal(await page.evaluate(()=>window.filterState.activeDashboardFilterCount.value),0);
  const empty=new URLSearchParams(await page.evaluate(()=>window.filterCalls.at(-1)));for(const key of ['reportFromYear','reportToYear','executionFrom','executionTo','isOngoing','scope','section','group','fromYear','toYear'])assert.equal(empty.has(key),false,key);
  await chooseDate(page,'Đến ngày','2026-12-31');await page.getByRole('button',{name:'Áp dụng lọc',exact:true}).click();const oneSide=new URLSearchParams(await page.evaluate(()=>window.filterCalls.at(-1)));assert.equal(oneSide.get('executionTo'),'2026-12-31');assert.equal(oneSide.has('executionFrom'),false);
  await open();await page.getByRole('button',{name:'Đóng bảng tùy chọn',exact:true}).focus();await page.keyboard.press('Escape');await fields.waitFor({state:'hidden'});assert.ok(await page.evaluate(()=>document.activeElement.textContent.includes('Bộ lọc nâng cao')));await open();await page.getByRole('heading',{name:/Toàn cảnh thực hiện/}).click();await fields.waitFor({state:'hidden'});assert.deepEqual(errors,[]);
  console.log(`${width}px: inset, new fields only, draft/cancel, year/date validation, calendar entry, apply, reset, open bounds and query serialization passed.`);
  await page.close();
 }

 const propagation=await browser.newPage();const requests=[];
 await propagation.route('**/api/**',route=>{requests.push(route.request().url());return route.fulfill({json:route.request().url().includes('/agency-items')?[]:{ministriesPerformance:[],provincesPerformance:[],othersPerformance:[]}});});
 await propagation.addInitScript(()=>localStorage.setItem('cdsqg_active_doc_code','2629')); await propagation.goto(base); await propagation.evaluate(()=>document.querySelector('#app').__vue_app__?.unmount()); requests.length=0;
 await propagation.evaluate(async()=>{
  const {createApp,h,ref,authState}=await import('/tests/refactor-fixture.js');authState.user.value={role:'Admin'};authState.token.value='fixture';
  const {useDashboardMetrics}=await import('/src/features/dashboard/useDashboardMetrics.js');
  const {useAgencyDrilldown}=await import('/src/features/dashboard/useAgencyDrilldown.js');
  const {loadAgencyReportData}=await import('/src/features/dashboard/reportData.js');
  const {switchDocument,activeDocumentId}=await import('/src/services/documentContext.js');switchDocument('2629');
  const values={assignmentRole:'Lead',dashboardFilter:'all',selectedAgencyIds:[],selectedSubAgencyIds:[],selectedScopes:[],selectedSections:[],selectedGroups:[],fromYear:null,toYear:null,isOngoingOnly:true,reportFromYear:2025,reportToYear:2026,executionFrom:'2026-01-01',executionTo:'2026-12-31',agencies:[]};
  const refs=Object.fromEntries(Object.entries(values).map(([key,value])=>[key,ref(value)]));const root=document.createElement('div');document.body.append(root);
  let metrics,drilldown;const app=createApp({setup(){metrics=useDashboardMetrics({...refs,isSpecialAgencyCode:()=>false});drilldown=useAgencyDrilldown({...refs,metrics:metrics.metrics});return()=>h('div');}});app.mount(root);
  await metrics.loadDashboardMetrics();await drilldown.drilldownAgency({agencyId:'00000000-0000-0000-0000-000000000001'});
  await loadAgencyReportData({agencyId:'00000000-0000-0000-0000-000000000001',filters:{...values,documentId:activeDocumentId.value},filterOutSpecialAgencies:x=>x});app.unmount();
 });
 const queries=requests.filter(url=>url.includes('/dashboard/')).map(url=>new URL(url).searchParams);assert.ok(queries.length>=5);
 for(const query of queries)for(const [key,value]of Object.entries({documentId:'26290000-0000-0000-0000-000000002629',reportFromYear:'2025',reportToYear:'2026',executionFrom:'2026-01-01',executionTo:'2026-12-31',isOngoing:'true'}))assert.equal(query.get(key),value,`${key}: ${query}`);
 console.log('Metrics, agency details, subordinate metrics and Excel data requests preserve identical time/document filters.');await propagation.close();
}finally{await browser.close();}