import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 const page=await browser.newPage({viewport:{width:1440,height:1000}});let deleted=false,deletes=0,restores=0;
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'admin'}));});
 await page.route('**/api/**',async route=>{
  const u=new URL(route.request().url());let body={};
  if(u.pathname.endsWith('/catalog'))body={configured:true,metrics:[],goals:[]};
  if(u.pathname==='/api/ai-alerts')body={configured:true,allowedHosts:[],feeds:(u.searchParams.get('deleted')==='true')===deleted?[{id:'demo',name:'Nguồn RSS thử',enabled:false,scanTime:'08:00',lastScan:{status:'Completed',added:6,duplicates:0,queuedAt:'2026-10-20T13:00:00Z'}}]:[]};
  if(u.pathname==='/api/ai-alerts/demo'&&route.request().method()==='DELETE'){deleted=true;deletes++;body={message:'Deleted'};}
  if(u.pathname==='/api/ai-alerts/demo/restore'){deleted=false;restores++;body={message:'Restored'};}
  if(u.pathname==='/api/ai-inbox')body={total:0,items:[]};
  await route.fulfill({json:body});
 });
 await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5202')+'/#ai-alerts');
 await page.getByText('Nguồn RSS và lịch quét',{exact:true}).click();
 await page.getByRole('button',{name:'Xóa',exact:true}).click();await page.getByRole('button',{name:'Hủy',exact:true}).click();assert.equal(deletes,0);
 await page.getByRole('button',{name:'Xóa',exact:true}).click();await page.getByRole('button',{name:'Xóa nguồn',exact:true}).click();await page.getByRole('button',{name:'Xóa',exact:true}).waitFor({state:'hidden'});assert.equal(deletes,1);
 await page.getByRole('checkbox',{name:'Nguồn đã xóa',exact:true}).check();await page.getByRole('button',{name:'Khôi phục',exact:true}).waitFor();
 await page.screenshot({path:'../scratch/ai-build/source-deleted-desktop.png'});
 await page.getByRole('button',{name:'Khôi phục',exact:true}).click();await page.getByRole('button',{name:'Khôi phục',exact:true}).waitFor({state:'hidden'});assert.equal(restores,1);
 await page.getByRole('checkbox',{name:'Nguồn đã xóa',exact:true}).uncheck();await page.getByRole('button',{name:'Xóa',exact:true}).waitFor();
 await page.setViewportSize({width:390,height:844});await page.getByRole('button',{name:'Xóa',exact:true}).waitFor();assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'../scratch/ai-build/source-delete-mobile.png'});
 console.log('Passed: cancel, delete confirmation, deleted list, restore, mobile source actions.');
}finally{await browser.close();}
