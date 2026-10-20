import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const page=await browser.newPage({viewport:{width:1440,height:960}}),errors=[],queries=[];
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'admin'}));});
 const items=[{id:'file',sequenceNumber:1,title:'UN-E-Government-Survey-2024.pdf',isUpload:true,status:'Read',metricKeysJson:'["EGDI_RANK"]',receivedAt:'2026-10-19T01:00:00Z'},{id:'rss',sequenceNumber:2,title:'Thông cáo tỷ trọng kinh tế số trong GDP',isUpload:false,alertFeedId:'feed',url:'https://www.nso.gov.vn/report',origin:'Google Alerts',status:'Read',metricKeysJson:'["DIGITAL_GDP_SHARE"]',receivedAt:'2026-10-19T02:00:00Z'}];
 await page.route('**/api/**',async route=>{
  const u=new URL(route.request().url());let body=[];
  if(u.pathname.endsWith('/catalog'))body={configured:true,metrics:[{key:'EGDI_RANK',name:'Thứ hạng EGDI',unit:'rank',definition:'UN Survey',goalCodes:['MT-12','MT-25']},{key:'DIGITAL_GDP_SHARE',name:'Tỷ trọng kinh tế số trong GDP',unit:'%',definition:'GDP toàn quốc',goalCodes:['MT-13','MT-26']}],goals:[{id:'12',code:'MT-12',title:'Nhóm 50 quốc gia'},{id:'25',code:'MT-25',title:'Nhóm 30 quốc gia'},{id:'13',code:'MT-13',title:'Kinh tế số 30% GDP'},{id:'26',code:'MT-26',title:'Kinh tế số 50% GDP'}]};
  if(u.pathname==='/api/ai-inbox'){queries.push(Object.fromEntries(u.searchParams));let result=items;if(u.searchParams.get('uploadsOnly')==='true')result=result.filter(i=>i.isUpload);if(u.searchParams.get('alertsOnly')==='true')result=result.filter(i=>!i.isUpload);body={total:result.length,items:result};}
  if(u.pathname.endsWith('/suggestions')){queries.push(Object.fromEntries(u.searchParams));body={total:0,items:[]};}
  if(u.pathname==='/api/ai-alerts')body={configured:true,feeds:[],allowedHosts:['www.nso.gov.vn']};
  await route.fulfill({json:body});
 });
 const base=process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5202';await page.goto(base+'/#ai-data');
 await page.getByRole('heading',{name:'Nạp file trực tiếp',exact:true}).waitFor();
 const table=page.locator('.ai-wide-list').first();await table.getByText('TL-19102026-001',{exact:true}).waitFor();
 assert.equal(queries.at(-1).uploadsOnly,'true');assert.equal(await table.getByText(items[1].title,{exact:true}).count(),0);
 await page.goto(base+'/#ai-alerts');await page.getByRole('tab',{name:'Google Alerts',exact:true}).waitFor();await page.locator('.ai-wide-list').getByText('RSS-19102026-002',{exact:true}).waitFor();assert.equal(queries.at(-1).alertsOnly,'true');
 await page.getByRole('button',{name:'Chi tiết',exact:true}).last().click();await page.getByRole('dialog').getByText(/RSS-19102026-002/).waitFor();await page.keyboard.press('Escape');
 await page.getByRole('button',{name:'Chi tiết',exact:true}).last().click();await Promise.all([page.waitForResponse(r=>r.url().includes('/suggestions?')&&r.url().includes('inboxId=rss')),page.getByRole('dialog').getByRole('button',{name:'Xem kết quả và dẫn chứng',exact:true}).click()]);await page.getByRole('tab',{name:'Kiểm chứng số liệu',exact:true}).waitFor();assert.equal(queries.at(-1).inboxId,'rss');assert.equal(queries.at(-1).status,undefined);await page.goto(base+'/#ai-data');
 if(await page.getByRole('tab',{name:'Tài liệu đã nạp',exact:true}).count())await page.getByRole('tab',{name:'Tài liệu đã nạp',exact:true}).click();await page.getByRole('button',{name:'Nạp tài liệu',exact:true}).click();await page.getByRole('dialog').waitFor();await page.keyboard.press('Escape');
 assert.equal(await page.getByRole('heading',{name:'Tài liệu nạp trực tiếp',exact:true}).count(),0);const buttonBox=await page.getByRole('button',{name:'Nạp tài liệu',exact:true}).boundingBox(),tableBox=await table.boundingBox();assert.ok(buttonBox.y<tableBox.y);
 await page.screenshot({path:'../scratch/ai-build/workspace-separated-desktop.png'});
 await page.setViewportSize({width:390,height:844});await page.locator('.ai-compact-list').first().getByText('TL-19102026-001',{exact:true}).waitFor();assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'../scratch/ai-build/workspace-separated-mobile.png'});
 assert.deepEqual(errors,[]);console.log('Passed: separated server-filtered sources, date codes, upload action, details and responsive layout.');
} finally {await browser.close();}
