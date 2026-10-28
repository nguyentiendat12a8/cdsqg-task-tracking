import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
const sheet='https://docs.google.com/spreadsheets/d/1fOGT8aBm6X5ul2pk8p7zxzixEMklpqLVXEgsSCFrEDI/edit?gid=731708878';
try{for(const width of [1440,390])for(const admin of [true,false]){
 const page=await browser.newPage({viewport:{width,height:950}});page.setDefaultTimeout(12000);const errors=[],requests=[];let saved;page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(({admin})=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:admin?1:2,username:'fixture'}));},{admin});
 await page.route('**/api/**',route=>{
  const req=route.request(),url=new URL(req.url()),path=url.pathname;requests.push(path);let data=[];
  if(path==='/api/digital-news')data={items:url.searchParams.get('search')?[]:[{id:'one',agencyName:'Bộ Tài chính',title:'Bộ Tài chính triển khai Kế hoạch chuyển đổi số giai đoạn 2026 – 2030 trên ba trụ cột Chính phủ số, kinh tế số và xã hội số',url:'https://www.mof.gov.vn/chuyen-doi-so/a',importedAt:'2026-10-28T08:00:00Z'}],total:url.searchParams.get('search')?0:1,page:1,pageSize:10};
  if(path==='/api/digital-news/agencies')data=['Bộ Tài chính'];
  if(path==='/api/digital-news/settings'){if(req.method()==='PUT'){saved=req.postDataJSON();data={message:'Đã lưu lịch đồng bộ Google Sheets.'};}else data={sheetUrl:sheet,enabled:true,intervalHours:2,lastStatus:'Completed',added:1,updated:0,unchanged:0,skipped:0,busy:false};}
  if(path==='/api/digital-news/sync')data={message:'Đồng bộ xong: 1 mới.'};
  return route.fulfill({json:data});
 });
 await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5189')+'/#digital-news');await page.getByRole('heading',{name:'Thông tin chuyển đổi số',exact:true}).waitFor();
 await page.getByRole('link',{name:'Xem nguồn ↗',exact:true}).filter({visible:true}).waitFor();
 if(width>=768){
  const pending=page.waitForRequest(req=>req.url().includes('/api/digital-news?')&&new URL(req.url()).searchParams.get('sortBy')==='title'&&new URL(req.url()).searchParams.get('sortOrder')==='asc');
  await page.getByRole('button',{name:'Tên bài viết',exact:false}).filter({visible:true}).click();await pending;
  await page.locator('th[aria-sort="ascending"]').waitFor();
 }else{await page.getByRole('combobox',{name:'Sắp xếp',exact:true}).click();const pending=page.waitForRequest(req=>req.url().includes('/api/digital-news?')&&new URL(req.url()).searchParams.get('sortBy')==='title');await page.getByRole('option',{name:'Tên bài viết: A → Z',exact:true}).click();await pending;}
 if(admin){await page.getByRole('button',{name:'Cấu hình đồng bộ',exact:true}).click();const modal=page.getByRole('dialog');await modal.waitFor();assert.equal(await modal.getByLabel('Link Google Sheets',{exact:true}).inputValue(),sheet);await modal.getByLabel('Chu kỳ đồng bộ (giờ)',{exact:true}).fill('4');
 await page.screenshot({path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/digital-news-modal-${width}.png`});
 await modal.getByRole('button',{name:'Lưu lịch đồng bộ',exact:true}).click();await modal.waitFor({state:'hidden'});assert.equal(saved.intervalHours,4);
 await page.getByRole('button',{name:'Đồng bộ ngay',exact:true}).click();await page.getByRole('status').filter({hasText:'Đồng bộ xong:'}).waitFor();
 }else{assert.equal(await page.getByRole('button',{name:'Cấu hình đồng bộ',exact:true}).count(),0);assert.equal(requests.filter(p=>p.endsWith('/settings')).length,0);}
 await page.screenshot({path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/digital-news-list-${width}-${admin}.png`});
 await page.getByLabel('Tìm bài viết',{exact:true}).fill('không có');await page.getByRole('button',{name:'Tìm kiếm',exact:true}).click();await page.getByText('Không có bài viết phù hợp.',{exact:true}).waitFor();
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1),false);assert.deepEqual(errors,[]);console.log(`${width}px ${admin?'admin':'reader'} passed`);await page.close();
}}finally{await browser.close();}
