import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const web=process.env.AI_REVIEW_WEB==='1';
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 const page=await browser.newPage({viewport:{width:1440,height:1000}});const errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 await page.route('**/api/**',async route=>{
  const path=new URL(route.request().url()).pathname;let data=[];
  if(path.endsWith('/catalog'))data={configured:true,metrics:[{key:'EGDI_RANK',name:'Thứ hạng EGDI toàn cầu của Việt Nam',unit:'rank',goalCodes:['MT-12']}],goals:[{id:'goal',code:'MT-12',title:'Chính phủ điện tử'}]};
  if(path.endsWith('/suggestions'))data={total:1,items:[{id:'sample',documentId:'doc',metricKey:'EGDI_RANK',dataYear:2024,periodLabel:'2024',value:71,unit:'rank',scope:'Việt Nam',page:204,status:'Pending',inputSequence:3,inputReceivedAt:'2026-10-20T02:18:00Z',inputType:web?'rss':'upload',readAt:'2026-10-20T02:20:00Z',sourceTitle:web?'Thông cáo dài về kết quả biên soạn chỉ tiêu kinh tế số':'UN-E-Government-Survey-2024.pdf',sourceUrl:web?'https://www.nso.gov.vn/report/2024':'/api/ai-inbox/sample/file',quote:'Viet Nam Asia South-Eastern Asia Very High EGDI V1 71 53 0.7709',methodology:'UN E-Government Survey',warnings:'Số liệu trước giai đoạn theo dõi; chỉ dùng làm tham chiếu lịch sử.',model:'Gemini',contentHash:'hash'}]};
  if(path.endsWith('/goals'))data=[{id:'goal',code:'MT-12'}];
  if(path.endsWith('/page/204'))data={text:'Viet Nam Asia South-Eastern Asia Very High EGDI V1 71 53 0.7709'};
  await route.fulfill({json:data});
 });
 await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5201');
 await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const {default:View}=await import('/src/views/AiDataView.vue');f.authState.user.value={role:1};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const node=document.createElement('main');node.id='review-test';node.style.padding='16px';document.body.append(node);const app=f.createApp({render:()=>f.h(View,{section:'suggestions'})});app.directive('accessible-dialog',f.accessibleDialog);app.mount(node);});
 await page.getByRole('button',{name:'Kiểm chứng',exact:true}).waitFor();
 await page.locator('.ai-wide-list').getByText(web?'www.nso.gov.vn':'UN-E-Government-Survey-2024.pdf',{exact:true}).waitFor();assert.equal(await page.locator('.ai-wide-list').getByText(web?'RSS-20102026-003':'TL-20102026-003',{exact:true}).count(),1);
 await page.screenshot({path:'../scratch/ai-build/review-table-desktop.png'});
 await page.getByRole('button',{name:'Kiểm chứng',exact:true}).click();
 const dialog=page.getByRole('dialog');await dialog.getByRole('heading',{name:web?'Kiểm chứng số liệu':'Kết quả bóc tách',exact:true}).waitFor();
 await dialog.getByRole('heading',{name:'Nguồn đối soát',exact:true}).waitFor();await dialog.getByText(web?'RSS-20102026-003':'TL-20102026-003',{exact:true}).waitFor();assert.equal(await dialog.getByText('Nạp trực tiếp',{exact:true}).count(),0);
 assert.equal(await dialog.getByRole('button',{name:web?'Duyệt dữ liệu kiểm chứng':'Xác nhận và ghi số liệu'}).isDisabled(),true);
 await dialog.getByRole('checkbox').check();
 assert.equal(await dialog.locator('#ai-review-note').inputValue(),'');
 assert.equal(await dialog.getByRole('button',{name:web?'Duyệt dữ liệu kiểm chứng':'Xác nhận và ghi số liệu'}).isEnabled(),true);
 assert.equal(await dialog.getByRole('button',{name:web?'Từ chối':'Bỏ qua',exact:true}).isEnabled(),true);
 await dialog.getByRole('checkbox').uncheck();
 assert.equal(await dialog.getByRole('button',{name:web?'Duyệt dữ liệu kiểm chứng':'Xác nhận và ghi số liệu'}).isDisabled(),true);
 assert.equal(await dialog.getByRole('button',{name:'Xem bản chụp văn bản trang nguồn'}).count(),0);assert.equal(await dialog.getByText('Số liệu trước giai đoạn theo dõi; chỉ dùng làm tham chiếu lịch sử.',{exact:true}).count(),0);
 await page.screenshot({path:'../scratch/ai-build/review-desktop.png'});
 await page.setViewportSize({width:390,height:844});
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 const rect=await dialog.getByRole('button',{name:web?'Duyệt dữ liệu kiểm chứng':'Xác nhận và ghi số liệu'}).boundingBox();assert.ok(rect.y+rect.height<=844,'Approval footer must remain on screen');
 await page.screenshot({path:'../scratch/ai-build/review-mobile.png'});
 await page.keyboard.press('Escape');assert.equal(await page.getByRole('dialog').count(),0);await page.locator('.ai-compact-list').getByText(web?'RSS-20102026-003':'TL-20102026-003',{exact:true}).waitFor();
 await page.getByRole('button',{name:'Kiểm chứng',exact:true}).click();await page.getByRole('dialog').waitFor();await page.keyboard.press('Escape');
 assert.deepEqual(errors,[]);console.log('Review UI passed: evidence, guarded approval, fixed footer, Escape, desktop/mobile.');
}finally{await browser.close();}


