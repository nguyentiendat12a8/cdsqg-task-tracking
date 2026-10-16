import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 for(const kind of ['Task','Goal']) {
  const page=await browser.newPage({viewport:{width:1440,height:900}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
  const code=kind==='Goal'?'MT-01':'NV-01';
  const report={id:'pending-1',itemType:kind,taskCode:code,taskTitle:'Báo cáo kiểm thử',agencyName:'Cơ quan kiểm thử',logDate:'2026-10-16T08:00:00Z',status:'InProgress',calculatedStatus:'InProgressOnTime',completionPercentage:30,attachmentFileUrls:[]};
  await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,fullName:'Admin test'}));localStorage.setItem('cdsqg_auth_token','fixture-token');});
  await page.route('**/api/**',async route=>{const path=new URL(route.request().url()).pathname;let data=[];
   if(path.endsWith('/pending-approvals'))data=[report];
   else if(path==='/api/notification')data={items:[{id:'notice-1',type:'PROGRESS_APPROVAL',title:'Báo cáo chờ phê duyệt '+code,message:'Phê duyệt '+code,isRead:false,createdAt:report.logDate}]};
   else if(path.endsWith('/items'))data={items:[],totalCount:0,totalPages:1};
   else if(path.endsWith('/grid'))data={items:[],dynamicYears:[2026]};
   await route.fulfill({contentType:'application/json',body:JSON.stringify(data)});
  });
  await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5201')+'/#goals-list');
  await page.getByRole('button',{name:/Duyệt báo cáo tiến độ/}).waitFor();await page.getByRole('button',{name:/Duyệt báo cáo tiến độ/}).getByText('1',{exact:true}).waitFor();
  await page.getByTitle('Thông báo hệ thống').click();await page.getByText('Báo cáo chờ phê duyệt '+code,{exact:true}).click();
  await page.locator('h3').filter({hasText:'Danh sách báo cáo tiến độ chờ phê duyệt'}).waitFor();await page.getByText('['+code+'] Báo cáo kiểm thử',{exact:true}).waitFor();
  assert(await page.getByRole('button',{name:/Phê duyệt/}).isVisible());assert.deepEqual(errors,[]);console.log('PASS admin approval notification '+kind+': pending badge, navigation, modal, no render error');await page.close();
 }
}finally{await browser.close();}
