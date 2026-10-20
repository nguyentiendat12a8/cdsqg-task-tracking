import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 const page=await browser.newPage({viewport:{width:1440,height:960}});const errors=[];let feeds=[],scanStatus='',scanCalls=0,saveCalls=0,classifyCalls=0;
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'admin'}));});
 await page.route('**/api/**',async route=>{
  const req=route.request(),url=new URL(req.url());let body=[];
  if(url.pathname==='/api/ai-data/catalog')body={configured:true,metrics:[{key:'EGDI_RANK',name:'Thứ hạng EGDI toàn cầu của Việt Nam',unit:'rank',goalCodes:['MT-12','MT-25']}],goals:[]};
  if(url.pathname==='/api/ai-alerts'){
   if(req.method()==='POST'){const v=req.postDataJSON();assert.equal(v.scanTime,'09:30');assert.equal(v.enabled,true);feeds=[{...v,id:'source',nextScanAt:'2026-10-20T02:30:00Z'}];saveCalls++;body={message:'Đã lưu nguồn RSS.'};}
   else body={configured:true,allowedHosts:['desapublications.un.org','www.nso.gov.vn','moit.gov.vn'],feeds:feeds.map(f=>({...f,lastScan:scanStatus?{status:scanStatus,queuedAt:'2026-10-19T02:30:00Z',added:1,duplicates:2,blocked:1}:null}))};
  }
  if(url.pathname==='/api/ai-data/documents')body={total:0,items:[]};
  if(url.pathname==='/api/ai-alerts/source/scan'){scanCalls++;scanStatus='Queued';body={message:'Đã xếp lượt quét.'};}
  if(url.pathname==='/api/ai-alerts/scans')body={total:1,items:[{id:'scan',feedName:'Nguồn báo cáo UN',status:scanStatus,trigger:'Manual',requestedBy:'Admin',queuedAt:'2026-10-19T02:30:00Z',received:4,added:1,duplicates:2,blocked:1}]};
  if(url.pathname==='/api/ai-inbox'){assert.equal(url.searchParams.get('uploadsOnly'),'false');assert.equal(url.searchParams.get('alertsOnly'),'false');body={total:1,items:[{id:'title',title:'Báo cáo mới về chính phủ số Việt Nam',origin:'Google Alerts · UN',url:'https://desapublications.un.org/report',receivedAt:'2026-10-19T02:30:00Z',status:'NeedsReview',metricKeysJson:'[]',reason:'Tiêu đề chưa đủ rõ',isUpload:false}]};}
  if(url.pathname==='/api/ai-inbox/title/queue'){assert.equal(req.postDataJSON().titleOnly,true);classifyCalls++;body={message:'Đã xếp hàng đánh giá.'};}
  await route.fulfill({json:body});
 });
 await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5201')+'/#ai-alerts');
 await page.getByRole('heading',{name:'Nguồn dữ liệu',exact:true}).waitFor();
 await page.getByRole('button',{name:'Thêm nguồn RSS',exact:true}).first().click();const dialog=page.getByRole('dialog');
 await dialog.getByLabel('Tên nguồn',{exact:true}).fill('Nguồn báo cáo UN');await dialog.getByLabel('URL RSS').fill('https://www.google.com/alerts/feeds/123/456');
 await dialog.getByRole('checkbox',{name:'Bật lịch quét hằng ngày'}).check();await dialog.getByLabel('Giờ quét (Việt Nam)').fill('09:30');
 await dialog.getByRole('button',{name:'Lưu nguồn',exact:true}).click();await page.getByText('Đã lưu nguồn RSS.',{exact:true}).waitFor();assert.equal(saveCalls,1);
 await page.getByRole('button',{name:'Quét ngay',exact:true}).click();await page.getByRole('button',{name:'Đang quét',exact:true}).waitFor();assert.equal(scanCalls,1);assert.equal(await page.getByRole('button',{name:'Đang quét',exact:true}).isDisabled(),true);
 scanStatus='Completed';await page.getByRole('button',{name:'Tải lại',exact:true}).click();await page.locator('.ai-wide-list').getByText('Hoàn tất',{exact:true}).waitFor();await page.screenshot({path:'../scratch/ai-build/alerts-desktop.png'});
 await page.getByRole('tab',{name:'Dữ liệu đầu vào',exact:true}).click();await page.getByRole('button',{name:'Chi tiết',exact:true}).click();await page.getByRole('button',{name:'Đánh giá lại tiêu đề',exact:true}).click();assert.equal(classifyCalls,1);await page.getByRole('button',{name:'Đóng',exact:true}).click();
 await page.getByRole('tab',{name:'Lịch sử xử lý',exact:true}).click();await page.locator('.ai-wide-list').getByText('4 nhận · 1 mới',{exact:true}).waitFor();
 await page.setViewportSize({width:390,height:844});assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.getByRole('tab',{name:'Nguồn Google Alerts',exact:true}).click();await page.getByRole('button',{name:'Sửa',exact:true}).waitFor();await page.screenshot({path:'../scratch/ai-build/alerts-mobile.png'});
 await page.getByRole('button',{name:'Sửa',exact:true}).click();assert.equal(await dialog.getByLabel('Giờ quét (Việt Nam)').inputValue(),'09:30');assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);await page.keyboard.press('Escape');assert.equal(await dialog.count(),0);
 assert.deepEqual(errors,[]);console.log('Alerts UI passed: RSS add, Vietnam schedule, scan lock, title review, history, desktop/mobile.');
}finally{await browser.close();}
