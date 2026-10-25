import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
const ids={1266:'12660000-0000-0000-0000-000000001266',2629:'26290000-0000-0000-0000-000000002629'};
try{for(const width of [1440,390]){
 const page=await browser.newPage({viewport:{width,height:950}});const errors=[],requests=[];page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));localStorage.setItem('cdsqg_active_doc_code','1266');});
 await page.route('**/api/**',route=>{
  const url=new URL(route.request().url()),path=url.pathname,doc=url.searchParams.get('documentId');requests.push({path,doc});let data=[];
  if(path==='/api/ai-data/catalog')data={configured:true,metrics:[],goals:[],uploadMetrics:[{key:'GOAL_'+doc,name:'Mục tiêu '+(doc===ids[1266]?'1266':'2629')}],internetMetrics:[],agencies:[]};
  else if(path==='/api/ai-inbox')data={items:[{id:doc,title:'File '+(doc===ids[1266]?'1266':'2629'),sequenceNumber:1,isUpload:true,status:'Read',metricKeysJson:'[]',receivedAt:'2026-10-25T00:00:00Z'}],total:1};
  else if(path==='/api/ai-alerts')data={page:1,total:0,feeds:[],configured:true,allowedHosts:[]};
  else if(path==='/api/ai-alerts/scans'||path==='/api/ai-data/suggestions')data={items:[],total:0};
  return route.fulfill({json:data});
 });
 await page.goto('http://127.0.0.1:5188/#ai-data');await page.getByText('File 1266',{exact:true}).filter({visible:true}).waitFor();
 await page.getByRole('button',{name:'Chọn chiến lược',exact:true}).click();await page.getByRole('group',{name:'Danh sách chiến lược'}).getByRole('button').filter({hasText:'2629/QĐ-TTg'}).click();
 await page.getByText('File 2629',{exact:true}).filter({visible:true}).waitFor();assert.equal(await page.getByText('File 1266',{exact:true}).count(),0);
 await page.evaluate(async()=>{const ctx=await import('/src/services/documentContext.js');ctx.dashboardDocumentScope.value='all';});
 await page.getByRole('button',{name:'Chọn chiến lược',exact:true}).click();
 const choices=page.getByRole('group',{name:'Danh sách chiến lược'});
 assert.equal(await choices.getByRole('button',{name:/Tổng quan hai văn bản/}).count(),0);
 assert.equal(await choices.getByRole('button').filter({hasText:'2629/QĐ-TTg'}).getAttribute('aria-pressed'),'true');
 await choices.getByRole('button').filter({hasText:'2629/QĐ-TTg'}).click();
 await choices.waitFor({state:'hidden'});
 await page.goto('http://127.0.0.1:5188/#ai-alerts');await page.getByText('Nguồn RSS và lịch quét',{exact:true}).click();
 await page.getByRole('button',{name:'Thêm nguồn RSS',exact:true}).first().click();await page.getByRole('dialog').waitFor();
 for(const path of ['/api/ai-data/catalog','/api/ai-inbox','/api/ai-alerts'])assert(requests.some(r=>r.path===path&&r.doc===ids[2629]),path+' missing document filter');
 assert.deepEqual(errors,[]);console.log(width+'px: document switch refreshes uploads, catalog and RSS without stale rows');await page.close();
}}finally{await browser.close();}
