import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const page=await browser.newPage({viewport:{width:1366,height:1000}}),errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'admin',fullName:'Admin kiểm thử'}));});
 await page.route('**/api/**',async route=>{
  const url=new URL(route.request().url());let data=[];
  if(url.pathname.endsWith('/catalog'))data={configured:false,metrics:[{key:'EGDI_RANK',name:'EGDI',unit:'rank',definition:'UN Survey',goalCodes:['MT-12','MT-25','MT-27']}],goals:[{id:'one',code:'MT-12',title:'Chính phủ điện tử'}],allowedHosts:[]};
  if(url.pathname==='/api/ai-provider')data={enabled:false,baseUrl:'https://api.openai.com/v1',model:'',hasApiKey:false};
  if(url.pathname==='/api/ai-inbox'||url.pathname.endsWith('/documents'))data={total:0,items:[]};
  if(url.pathname.endsWith('/suggestions')){const approved=url.searchParams.get('status')==='Approved';data={total:1,items:[{id:'row',metricKey:'EGDI_RANK',dataYear:2024,periodLabel:'UN 2024',value:71,acceptedValue:approved?40:null,unit:'rank',status:approved?'Approved':'Pending',sourceTitle:'UN report',sourceUrl:'https://desapublications.un.org/',reviewedBy:approved?'Admin':'',reviewNote:approved?'Đã đối chiếu':'',isCurrent:approved}]};}
  await route.fulfill({json:data});
 });
 const base=process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5201';
 await page.goto(base+'/#ai-data');
 await page.getByRole('heading',{name:'Nguồn dữ liệu',exact:true}).waitFor();
 assert.equal(await page.getByRole('tab').count(),3);
 await page.getByRole('button',{name:'Quản trị dữ liệu AI',exact:true}).click();
 const menu=page.locator('#ai-submenu');assert.equal(await menu.getByRole('button').count(),3);
 await menu.getByRole('button',{name:'Kiểm chứng số liệu',exact:true}).click();
 await page.getByRole('heading',{name:'Kiểm chứng số liệu',exact:true}).waitFor();
 assert.equal(new URL(page.url()).hash,'#ai-review');
 await page.getByRole('combobox',{name:'Trạng thái',exact:true}).click();await page.getByRole('option',{name:'Đã duyệt',exact:true}).click();
 await page.locator('.ai-wide-list').getByText('40 hạng',{exact:false}).waitFor();
 await page.getByText('Chỉ số và mục tiêu áp dụng',{exact:false}).click();assert.equal(await page.getByText('MT-27',{exact:false}).count(),0);
 await page.reload();await page.getByRole('heading',{name:'Kiểm chứng số liệu',exact:true}).waitFor();
 await page.getByRole('button',{name:'Quản trị dữ liệu AI',exact:true}).click();
 await page.locator('#ai-submenu').getByRole('button',{name:'Nguồn dữ liệu',exact:true}).click();
 await page.getByRole('heading',{name:'Nguồn dữ liệu',exact:true}).waitFor();
 assert.equal(await page.getByRole('tab',{name:'Mục tiêu áp dụng',exact:false}).count(),0);
 assert.equal(await page.getByText('MT-27',{exact:false}).count(),0);
 await page.getByRole('tab',{name:'Lịch sử xử lý',exact:true}).click();
 await page.getByText('Chưa có lịch sử xử lý.',{exact:true}).waitFor();
 await page.locator('#ai-submenu').getByRole('button',{name:'Cấu hình AI',exact:true}).click();
 await page.getByRole('heading',{name:'Cấu hình AI',exact:true}).waitFor();
 assert.equal(new URL(page.url()).hash,'#ai-settings');
 await page.reload();await page.getByRole('heading',{name:'Cấu hình AI',exact:true}).waitFor();
 await page.screenshot({path:'../scratch/ai-build/navigation-desktop.png'});
 await page.setViewportSize({width:390,height:844});
 await page.getByRole('button',{name:'Mở hoặc đóng menu',exact:true}).click();
 await page.locator('#ai-submenu').getByRole('button',{name:'Kiểm chứng số liệu',exact:true}).click();
 await page.getByRole('heading',{name:'Kiểm chứng số liệu',exact:true}).waitFor();
 assert.equal(await page.getByRole('button',{name:'Mở hoặc đóng menu'}).getAttribute('aria-expanded'),'false');
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'../scratch/ai-build/navigation-mobile.png'});
 assert.deepEqual(errors,[]);console.log('AI navigation passed: three grouped submenus, unified sources, reference goals in review, approved filter, direct route reload, document details, mobile menu.');
} finally {await browser.close();}


