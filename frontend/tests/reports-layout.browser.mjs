import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{for(const width of [1440,390]){
 const page=await browser.newPage({viewport:{width,height:950}});page.setDefaultTimeout(12000);const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 const agencies=Array.from({length:21},(_,i)=>({id:'agency-'+i,agencyId:'agency-'+i,name:'Cơ quan '+i,type:'Ministry',totalItems:5,totalGoals:5,totalTasks:0,notStarted:5,goalNotStarted:5,onTimeReports:2}));
 await page.route('**/api/**',route=>{const path=new URL(route.request().url()).pathname;let data=[];
 if(path==='/api/agencies')data=agencies;
 if(path==='/api/dashboard/metrics')data={ministriesPerformance:agencies,provincesPerformance:[],totalGoals:105,totalTasks:0};
 if(path.endsWith('/grid'))data={items:agencies.map((a,i)=>({id:'item-'+i,title:'Mục tiêu chuyển đổi số '+i,code:'MT-'+i,itemType:'Goal',leadAgencyId:a.id,leadAgencyName:a.name,deadlineStatus:'Overdue',calculatedStatus:'NotStarted',section:'Nhóm '+i,scope:'specific',isGeneralTask:false}))};
 return route.fulfill({json:data});});
 await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5189')+'/#reports');await page.getByRole('heading',{name:'Trung tâm báo cáo và xuất dữ liệu Excel',exact:true}).waitFor();
 await page.getByText('Trang 1 / 3',{exact:true}).waitFor();await page.getByRole('button',{name:'Trang sau ›',exact:true}).click();await page.getByText('Trang 2 / 3',{exact:true}).waitFor();
 await page.getByRole('combobox',{name:'Số bản ghi/trang',exact:true}).click();await page.getByRole('option',{name:'25',exact:true}).click();await page.getByText('Trang 1 / 1',{exact:true}).waitFor();
 let headerColor;
 for(let index=1;index<=4;index++){
  await page.getByRole('button',{name:new RegExp('Báo cáo '+index)}).click();await page.locator('table:visible').waitFor();
  assert.equal(await page.getByRole('button',{name:new RegExp('Báo cáo '+index)}).getAttribute('aria-pressed'),'true');
  const color=await page.locator('thead:visible').evaluate(el=>getComputedStyle(el).backgroundColor);if(headerColor)assert.equal(color,headerColor);headerColor=color;
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1),false);
  await page.screenshot({animations:'disabled',path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/reports-layout-${width}-${index}.png`});
 }
 await page.getByRole('button',{name:/Lọc nâng cao/}).click();await page.getByText('Bộ lọc báo cáo nâng cao',{exact:true}).waitFor();await page.getByRole('button',{name:'Đóng bảng tùy chọn',exact:true}).click();
 const downloadPromise=page.waitForEvent('download');await page.getByRole('button',{name:'Xuất file Excel',exact:true}).click();const download=await downloadPromise;assert.match(download.suggestedFilename(),/\.xlsx$/);assert.equal(await download.failure(),null);
 assert.deepEqual(errors,[]);console.log('Reports '+width+'px: four types, pagination, filters and Excel download passed');await page.close();
}}finally{await browser.close();}
