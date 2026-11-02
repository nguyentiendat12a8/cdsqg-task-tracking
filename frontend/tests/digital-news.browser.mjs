import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {for(const width of [1440,390])for(const admin of [true,false]){
 const page=await browser.newPage({viewport:{width,height:950}});page.setDefaultTimeout(12000);const errors=[];let saved,monitorSaved,criteriaSaved,readCalls=0,assessment=null;
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(({admin})=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:admin?1:2,username:'fixture'}));},{admin});
 const item=()=>({id:'10000000-0000-0000-0000-000000000001',agencyName:'Bộ Tài chính',title:'Bộ Tài chính ban hành Khung kiến trúc chính phủ số phiên bản 3.0',url:'https://mof.gov.vn/a',importedAt:'2026-11-02T08:00:00Z',aiStatus:assessment?.status||'Unread',assessment});
 await page.route('**/api/**',route=>{const req=route.request(),url=new URL(req.url()),path=url.pathname;let data=[];
  if(path==='/api/digital-news')data={items:url.searchParams.get('search')?[]:[item()],total:url.searchParams.get('search')?0:1,page:1,pageSize:10};
  if(path==='/api/notification')data={items:[{id:'20000000-0000-0000-0000-000000000002',title:'Phát hiện tin ban hành khung kiến trúc số',message:'Đã phê duyệt khung kiến trúc',type:'System',linkUrl:'#digital-news?article=10000000-0000-0000-0000-000000000001',createdAt:'2026-11-02T08:00:00Z',isRead:false}],unreadCount:1};
  if(path==='/api/legaldocuments')data={items:[],totalCount:0,totalPages:1};
  if(path==='/api/digital-news/agencies')data=['Bộ Tài chính'];
  if(path==='/api/digital-news/settings'){if(req.method()==='PUT'){saved=req.postDataJSON();data={message:'Đã lưu lịch quét tin.'};}else data={enabled:true,intervalMinutes:30,lookbackDays:365,batchSize:12,busy:false,sources:[{id:'source',name:'Cổng Bộ Tài chính',agencyName:'Bộ Tài chính',domain:'mof.gov.vn',requiredTerms:'',enabled:true,sortOrder:1}],keywords:[{id:'keyword',name:'Kế hoạch',phrase:'kế hoạch chuyển đổi số',conditions:'',enabled:true,sortOrder:1}]};}
  if(path==='/api/digital-news/criteria'){criteriaSaved=req.postDataJSON();data={message:'Đã lưu tiêu chí.'};}
  if(path==='/api/digital-news/monitoring'){monitorSaved=req.postDataJSON();data={message:'Đã lưu nguồn và từ khóa theo dõi.'};}
  if(path==='/api/digital-news/sync')data={message:'Quét xong: 1 bài mới.'};
  if(path==='/api/digital-news/10000000-0000-0000-0000-000000000001/read'){readCalls++;assessment={status:'Related',planKind:'Architecture',startYear:null,endYear:null,reason:'Tiêu đề nêu việc ban hành khung kiến trúc số.',evidence:'ban hành Khung kiến trúc chính phủ số'};data={assessment};}
  if(path==='/api/digital-news/10000000-0000-0000-0000-000000000001/assessment')data={item:item(),assessment};
  return route.fulfill({json:data});
 });
 await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5189')+'/#digital-news');await page.getByRole('heading',{name:'Thông tin chuyển đổi số',exact:true}).waitFor();
 await page.getByRole('link',{name:'Xem nguồn ↗',exact:true}).filter({visible:true}).waitFor();
 if(width>=768){const pending=page.waitForRequest(req=>req.url().includes('/api/digital-news?')&&new URL(req.url()).searchParams.get('sortBy')==='title');await page.getByRole('button',{name:'Tên bài viết',exact:false}).filter({visible:true}).click();await pending;}
 await page.screenshot({path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/digital-news-task-style-${width}-${admin}.png`});
 if(admin){
  await page.getByRole('button',{name:'Cấu hình theo dõi',exact:true}).click();let modal=page.getByRole('dialog');await modal.waitFor();await modal.getByLabel('Chu kỳ quét (phút)').fill('60');await modal.getByRole('button',{name:'Lưu lịch quét',exact:true}).click();await modal.waitFor({state:'hidden'});assert.equal(saved.intervalMinutes,60);assert.match(saved.publishedFrom,/^\d{4}-\d{2}-01$/);assert.equal(saved.publishedTo,null);
  await page.getByRole('button',{name:'Cấu hình theo dõi',exact:true}).click();modal=page.getByRole('dialog');await modal.getByRole('button',{name:'Nguồn theo dõi',exact:true}).click();await modal.getByLabel('Tên miền',{exact:true}).fill('www.mof.gov.vn');await modal.getByRole('button',{name:'Lưu nguồn và từ khóa',exact:true}).click();await modal.waitFor({state:'hidden'});assert.equal(monitorSaved.sources[0].domain,'www.mof.gov.vn');
  await page.getByRole('button',{name:'Cấu hình theo dõi',exact:true}).click();modal=page.getByRole('dialog');await modal.getByRole('button',{name:'Tiêu chí đánh giá',exact:true}).click();assert.ok((await modal.getByLabel('Từ khóa liên quan',{exact:true}).inputValue()).includes('khung kiến trúc chính phủ số'));assert.equal(await modal.getByRole('checkbox').count(),0);assert.equal(await modal.getByRole('textbox').count(),3);await modal.getByLabel('Từ khóa liên quan',{exact:true}).fill('chuyển đổi số\nchính quyền số\nkhung kiến trúc số');await modal.getByRole('button',{name:'Lưu tiêu chí đánh giá',exact:true}).click();await modal.waitFor({state:'hidden'});assert.deepEqual(Object.keys(criteriaSaved).sort(),['actionKeywords','excludedKeywords','relatedKeywords']);assert.ok(criteriaSaved.relatedKeywords.includes('chính quyền số'));assert.equal(readCalls,0);
  await page.getByRole('button',{name:'Quét tin ngay',exact:true}).click();await page.getByRole('status').filter({hasText:'Quét xong:'}).waitFor();assert.equal(readCalls,0);
  await page.getByRole('button',{name:'Đọc',exact:true}).filter({visible:true}).click();await page.getByRole('button',{name:'Xem kết quả',exact:true}).filter({visible:true}).waitFor();assert.equal(await page.getByRole('dialog').count(),0);await page.getByRole('button',{name:'Xem kết quả',exact:true}).filter({visible:true}).click();modal=page.getByRole('dialog');await modal.getByText('Tiêu đề nêu việc ban hành khung kiến trúc số.',{exact:true}).waitFor();assert.equal(readCalls,1);
  await page.screenshot({path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/native-plan-result-${width}.png`});await modal.getByRole('button',{name:'Đóng',exact:true}).click();await page.getByRole('button',{name:'Xem kết quả',exact:true}).filter({visible:true}).click();assert.equal(readCalls,1);await page.getByRole('dialog').getByRole('button',{name:'Đóng',exact:true}).click();
  for(const fromOtherScreen of [false,false,true]){
   if(fromOtherScreen){await page.evaluate(()=>location.hash='#legal-documents');await page.getByRole('heading',{name:'Thông tin chuyển đổi số',exact:true}).waitFor({state:'hidden'});}
   await page.getByTitle('Thông báo hệ thống',{exact:true}).click();await page.getByText('Phát hiện tin ban hành khung kiến trúc số',{exact:true}).click();
   modal=page.getByRole('dialog');await modal.getByText('Tiêu đề nêu việc ban hành khung kiến trúc số.',{exact:true}).waitFor();assert.ok(page.url().includes('#digital-news?article='));assert.equal(readCalls,1);await modal.getByRole('button',{name:'Đóng',exact:true}).click();
  }
 }else{assert.equal(await page.getByRole('button',{name:'Cấu hình theo dõi',exact:true}).count(),0);assert.equal(await page.getByRole('button',{name:'Đọc',exact:true}).count(),0);}
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1),false);assert.deepEqual(errors,[]);
 await page.getByLabel('Tìm bài viết',{exact:true}).fill('không có');await page.getByRole('button',{name:'Tìm kiếm',exact:true}).click();await page.getByText('Không có bài viết phù hợp.',{exact:true}).waitFor();
 console.log(`${width}px ${admin?'admin':'reader'} passed`);await page.close();
}}finally{await browser.close();}
