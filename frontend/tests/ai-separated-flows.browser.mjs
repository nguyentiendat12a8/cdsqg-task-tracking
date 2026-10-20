import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
 const page=await browser.newPage({viewport:{width:1440,height:960}});const errors=[],queries=[];
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'admin'}));});
 const metric={key:'GOAL_00000000-0000-0000-0000-000000000099',name:'MT-99 · Số nền tảng hoạt động',unit:'số lượng',goalCodes:['MT-99']};
 const suggestion={id:'result',metricKey:metric.key,value:250,unit:'số lượng',dataYear:2026,status:'Pending',quote:'Năm 2026 Việt Nam có 250 nền tảng hoạt động.',page:1,inputType:'upload',inputSequence:1,inputReceivedAt:'2026-10-20T02:00:00Z',sourceTitle:'report.txt',sourceUrl:'/api/ai-inbox/file/file'};
 await page.route('**/api/**',async route=>{
  const u=new URL(route.request().url());let data={};
  if(u.pathname.endsWith('/catalog'))data={configured:true,metrics:[{key:'EGDI_RANK',name:'EGDI',unit:'rank',goalCodes:['MT-12']}],uploadMetrics:[metric],goals:[{id:'goal',code:'MT-12',title:'EGDI'}]};
  if(u.pathname==='/api/ai-inbox')data={total:1,pageSize:5,items:[{id:u.searchParams.get('uploadsOnly')==='true'?'file':'rss',title:'report.txt',isUpload:u.searchParams.get('uploadsOnly')==='true',receivedAt:'2026-10-20T02:00:00Z',sequenceNumber:1,status:'Read',metricKeysJson:JSON.stringify([metric.key])}]};
  if(u.pathname.endsWith('/suggestions')){queries.push(Object.fromEntries(u.searchParams));data={total:1,items:[u.searchParams.has('inboxId')?suggestion:{...suggestion,id:'rss-result',metricKey:'EGDI_RANK',value:71,unit:'rank',inputType:'rss',sourceUrl:'https://www.nso.gov.vn/report'}]};}
  if(u.pathname.endsWith('/goals'))data=[{id:'goal',code:'MT-99'}];
  if(u.pathname==='/api/ai-alerts')data={configured:true,feeds:[],allowedHosts:[]};
  if(u.pathname.endsWith('/documents')){queries.push(Object.fromEntries(u.searchParams));data={total:0,items:[],pageSize:5};}
  await route.fulfill({json:data});
 });
 const base=process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5202';
 await page.goto(base+'/#ai-data');await page.getByRole('heading',{name:'Nạp file trực tiếp',exact:true}).waitFor();
 assert.equal(await page.getByRole('tab',{name:'Google Alerts',exact:true}).count(),0);
 await page.getByRole('button',{name:'Nạp tài liệu',exact:true}).click();const upload=page.getByRole('dialog');await upload.getByRole('combobox',{name:'Mục tiêu cần bóc tách'}).click();await page.getByRole('option',{name:metric.name,exact:true}).waitFor();await page.keyboard.press('Escape');await upload.getByRole('button',{name:'Đóng',exact:true}).click();
 await page.locator('.ai-wide-list').getByRole('button',{name:'Chi tiết',exact:true}).click();const dialog=page.getByRole('dialog');await dialog.getByText('250 số lượng',{exact:false}).waitFor();
 assert.equal(await dialog.getByRole('button',{name:'Xem kết quả và dẫn chứng',exact:true}).count(),0);assert.equal(queries.at(-1).inboxId,'file');
 await dialog.getByRole('button',{name:'Xác nhận số liệu',exact:true}).click();await page.getByRole('dialog').getByRole('heading',{name:'Kết quả bóc tách',exact:true}).waitFor();assert.equal(new URL(page.url()).hash,'#ai-data');await page.screenshot({path:'../scratch/ai-build/direct-result-desktop.png'});await page.getByRole('dialog').getByRole('checkbox').check();assert.equal(await page.getByRole('button',{name:'Xác nhận và ghi số liệu'}).isEnabled(),true);await page.keyboard.press('Escape');
 await page.goto(base+'/#ai-alerts');await page.getByRole('heading',{name:'Dữ liệu từ internet',exact:true}).waitFor();await page.getByRole('tab',{name:'Kiểm chứng số liệu',exact:true}).click();await page.locator('.ai-wide-list').getByText('71 hạng',{exact:false}).waitFor();assert.equal(queries.at(-1).internetOnly,'true');
 
 await page.setViewportSize({width:390,height:844});assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'../scratch/ai-build/separated-internet-mobile.png'});
 assert.deepEqual(errors,[]);console.log('Passed: direct goals beyond five, file results in place, guarded save, internet-only review/history, desktop/mobile.');
}finally{await browser.close();}
