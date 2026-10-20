import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const page=await browser.newPage({viewport:{width:1280,height:800}});const errors=[];let uploaded=false,queued=false,blockedStatus='';
 page.on('pageerror',e=>errors.push(e.message));
 await page.route('**/api/**',async route=>{
  const url=new URL(route.request().url());let data=[];
  if(url.pathname.endsWith('/catalog'))data={configured:false,metrics:[{key:'EGDI_RANK',name:'EGDI',unit:'rank',definition:'UN Survey',goalCodes:['MT-12','MT-25','MT-27']}],goals:[{id:'one',code:'MT-12',title:'Chính phủ điện tử'}],allowedHosts:[]};
  if(url.pathname==='/api/ai-inbox')data={total:11,items:[{id:'one',title:'Báo cáo kinh tế số',origin:'Google Alerts',receivedAt:'2026-10-17T01:00:00Z',status:blockedStatus==='Processing'?'ReadQueued':blockedStatus||(uploaded?'ReadNoData':'NeedsReview'),processing:blockedStatus==='Processing',metricKeysJson:'["EGDI_RANK"]' ,reason:'Chưa đủ rõ',error:uploaded?'Chưa có số liệu vượt qua kiểm tra. Xem Lịch sử xử lý để biết nguyên nhân.':'',processingDetails:uploaded?'Trang 1: giá trị không xuất hiện trong dẫn chứng.':'',isUpload:true}]};
  if(url.pathname.endsWith('/queue')){queued=true;assert.equal(route.request().postDataJSON().metricKey,'EGDI_RANK');data={message:'Đã xếp hàng xử lý.'};}
  if(url.pathname.endsWith('/upload')){uploaded=true;data={id:'one',message:'Đã nhận tài liệu vào hàng đợi.'};}
  await route.fulfill({json:data});
 });
 await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5201');
 await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const {default:View}=await import('/src/views/AiDataView.vue');f.authState.user.value={role:1,username:'admin'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const node=document.createElement('main');node.id='ai-test';node.style.padding='16px';document.body.append(node);window.aiTestSection=f.ref('uploads');const app=f.createApp({render:()=>f.h(View,{section:window.aiTestSection.value})});app.directive('accessible-dialog',f.accessibleDialog);app.mount(node);});
 const root=page.locator('#ai-test');await root.getByRole('heading',{name:'Nạp file trực tiếp',exact:true}).waitFor();
 assert.equal(await root.getByText('MT-27',{exact:false}).count(),0);
 await page.evaluate(()=>window.aiTestSection.value='uploads');
 await root.locator('.ai-wide-list').getByText('Cần xem lại',{exact:true}).waitFor();
 await root.locator('.ai-wide-list td.ai-stt').getByText('1',{exact:true}).waitFor();await root.getByRole('button',{name:'Trang sau ›',exact:true}).click();await root.locator('.ai-wide-list td.ai-stt').getByText('6',{exact:true}).waitFor();await root.getByRole('button',{name:'‹ Trang trước',exact:true}).click();await root.locator('.ai-wide-list td.ai-stt').getByText('1',{exact:true}).waitFor();
 assert.equal(await root.getByRole('button',{name:'Thu thập ngay',exact:true}).count(),0);
 await page.evaluate(()=>window.aiTestSection.value='uploads');
 await root.getByRole('button',{name:'Nạp tài liệu',exact:true}).click();
 assert.equal(await root.getByRole('button',{name:'Nạp tài liệu để AI đọc',exact:true}).isDisabled(),true);
 await root.getByText('PDF có chữ, DOCX hoặc TXT UTF-8 · Tối đa 12 MB',{exact:false}).waitFor();
 await root.locator('#ai-document').setInputFiles({name:'bao-cao.txt',mimeType:'text/plain',buffer:Buffer.from('Báo cáo kinh tế số Việt Nam năm 2025 có dữ liệu để kiểm chứng.')});
 assert.equal(await root.getByRole('button',{name:'Nạp tài liệu để AI đọc',exact:true}).isEnabled(),true);
 await root.getByRole('button',{name:'Nạp tài liệu để AI đọc',exact:true}).click();
 await page.getByRole('dialog').getByText('Đã nhận tài liệu vào hàng đợi.',{exact:true}).waitFor();assert.equal(uploaded,true);await page.getByRole('dialog').getByRole('heading',{name:'Chi tiết tài liệu'}).waitFor();await page.getByRole('dialog').getByRole('button',{name:'Đóng',exact:true}).click();
 await root.locator('.ai-wide-list').getByText('Chưa có số liệu',{exact:true}).waitFor();
 assert.equal(await root.getByRole('button',{name:'Xem đề xuất kiểm chứng',exact:true}).count(),0);
 assert.equal(await root.getByRole('combobox',{name:'Mục tiêu đọc lại'}).count(),0);
 await root.getByRole('button',{name:'Chi tiết',exact:true}).click();
 const dialog=page.getByRole('dialog');await dialog.getByRole('heading',{name:'Chi tiết tài liệu'}).waitFor();await dialog.getByText('Nguyên nhân chưa có số liệu',{exact:true}).waitFor();await dialog.getByText('Trang 1: giá trị không xuất hiện trong dẫn chứng.',{exact:true}).waitFor();assert.equal(await dialog.getByText(/Xem Lịch sử xử lý/).count(),0);
 await dialog.getByRole('button',{name:'Đọc lại',exact:true}).click();
 await dialog.getByRole('combobox',{name:'Mục tiêu đọc lại',exact:true}).waitFor();
 await dialog.getByRole('button',{name:'Bắt đầu đọc lại'}).click();
 await dialog.waitFor({state:'hidden'});assert.equal(queued,true);
 for(const pending of ['TitleQueued','ReadQueued','Processing']){
  blockedStatus=pending;await root.getByRole('button',{name:'Tải lại hàng đợi',exact:true}).click();await root.getByRole('button',{name:'Chi tiết',exact:true}).click();await page.getByRole('dialog').waitFor();assert.equal(await page.getByRole('dialog').getByRole('button',{name:'Đọc lại',exact:true}).isDisabled(),true);await page.getByRole('dialog').getByRole('button',{name:'Đóng',exact:true}).click();
 }
 blockedStatus='Error';await root.getByRole('button',{name:'Tải lại hàng đợi',exact:true}).click();await root.getByRole('button',{name:'Chi tiết',exact:true}).click();await page.getByRole('dialog').getByText('Chưa có kết quả vì lượt đọc bị lỗi. Xem nguyên nhân ở trên.',{exact:true}).waitFor();assert.equal(await page.getByRole('dialog').getByText('Chưa tìm thấy số liệu phù hợp trong tài liệu.',{exact:true}).count(),0);await page.getByRole('dialog').getByRole('button',{name:'Đóng',exact:true}).click();
 blockedStatus='';await root.getByRole('button',{name:'Tải lại hàng đợi',exact:true}).click();
 await page.screenshot({path:'../scratch/ai-build/inbox-desktop.png'});
 await page.setViewportSize({width:390,height:844});
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'../scratch/ai-build/inbox-mobile.png'});
 await root.getByRole('button',{name:'Chi tiết',exact:true}).click();
 await page.getByRole('dialog').waitFor();
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.keyboard.press('Escape');assert.equal(await page.getByRole('dialog').count(),0);
 assert.deepEqual(errors,[]);console.log('AI inbox browser passed: stale MT-27 filtered, upload-first view, paused crawling, desktop/mobile.');
} finally {await browser.close();}


