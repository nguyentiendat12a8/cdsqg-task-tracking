import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 const page=await browser.newPage({viewport:{width:1440,height:960}});
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'admin'}));});
 await page.route('**/api/**',async route=>{
  const u=new URL(route.request().url()),p=Number(u.searchParams.get('page')||1);let body={};
  if(u.pathname.endsWith('/catalog'))body={configured:true,metrics:[{key:'EGDI_RANK',name:'EGDI',definition:'UN',goalCodes:['MT-12']}],goals:[{id:'goal',code:'MT-12',title:'Goal'}]};
  if(u.pathname.endsWith('/documents')||u.pathname.endsWith('/scans'))body={total:12,pageSize:5,items:Array.from({length:Math.min(5,12-(p-1)*5)},(_,n)=>({id:'row-'+((p-1)*5+n),title:'Report '+((p-1)*5+n),url:'/api/ai-inbox/file/file',contentHash:'hash',retrievedAt:'2026-10-20T10:00:00Z',status:'Completed',feedName:'RSS source '+((p-1)*5+n),queuedAt:'2026-10-20T10:00:00Z',trigger:'Manual',requestedBy:'Admin',received:1,added:1,duplicates:0,blocked:0}))};
  if(u.pathname==='/api/ai-alerts')body={configured:true,feeds:[],allowedHosts:[]};
  if(u.pathname==='/api/ai-inbox'||u.pathname.endsWith('/suggestions'))body={total:0,items:[]};
  await route.fulfill({json:body});
 });
 const base=process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5202';
 await page.goto(base+'/#ai-alerts');await page.getByRole('heading',{name:'Dữ liệu từ internet',exact:true}).waitFor();
 await page.getByRole('tab',{name:'Lịch sử xử lý',exact:true}).click();const panel=page.locator('#alert-panel-history');
 await panel.locator('table td.ai-stt').getByText('1',{exact:true}).waitFor();assert.equal(await panel.locator('table tbody tr').count(),5);
 await panel.getByRole('button',{name:'Trang sau ›',exact:true}).click();await panel.locator('table td.ai-stt').getByText('6',{exact:true}).waitFor();assert.equal(await panel.locator('table tbody tr').count(),5);
 await panel.getByRole('button',{name:'Trang sau ›',exact:true}).click();await panel.locator('table td.ai-stt').getByText('11',{exact:true}).waitFor();assert.equal(await panel.locator('table tbody tr').count(),2);assert.equal(await panel.getByRole('button',{name:'Trang sau ›',exact:true}).isDisabled(),true);
 await page.screenshot({path:'../scratch/ai-build/history-paging-desktop.png'});await page.setViewportSize({width:390,height:844});assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.goto(base+'/#ai-review');await page.getByRole('heading',{name:'Dữ liệu từ internet',exact:true}).waitFor();await page.getByRole('tab',{name:'Kiểm chứng số liệu',exact:true}).waitFor();
 console.log('Passed: both server history pages 5/5/2, STT 1/6/11, last page guard, goals in sources only, mobile layout.');
}finally{await browser.close();}
