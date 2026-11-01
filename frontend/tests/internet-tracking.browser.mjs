import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
const goal='GOAL_22222222-2222-2222-2222-222222222222',agency='11111111-1111-1111-1111-111111111111';
try{for(const width of [1440,390]){
 const page=await browser.newPage({viewport:{width,height:950}});page.setDefaultTimeout(12000);const errors=[];page.on('pageerror',e=>errors.push(e.message));let feeds=[],payload;
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 await page.route('**/api/**',route=>{
  const p=new URL(route.request().url()).pathname;let data=[];
  if(p==='/api/ai-data/catalog')data={configured:true,metrics:[{key:'EGDI_RANK',name:'Thứ hạng EGDI toàn cầu',goalCodes:[]}],goals:[],uploadMetrics:[],internetMetrics:[{key:goal,name:'2629/QĐ-TTg · MT-01 · Tỷ lệ hồ sơ số hóa',goalCodes:['MT-01']}],agencies:[{id:agency,name:'Hà Nội'}]};
  else if(p==='/api/ai-alerts'){
   if(route.request().method()==='POST'||route.request().method()==='PUT'){
    payload=route.request().postDataJSON();feeds=[{...payload,id:'feed1',metricKeysJson:JSON.stringify(payload.metricKeys),agencyIdsJson:JSON.stringify(payload.agencyIds),lastScan:null}];return route.fulfill({json:{id:'feed1',message:'Đã lưu nguồn RSS.'}});
   }data={page:1,total:feeds.length,configured:true,feeds,allowedHosts:['dangcongsan.vn','nhandan.vn']};
  }else if(p==='/api/ai-inbox'||p==='/api/ai-data/suggestions')data={items:[],total:0};
  return route.fulfill({json:data});
 });
 await page.goto((process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5188')+'/#ai-alerts');
 await page.getByText('Nguồn RSS và lịch quét',{exact:true}).click();
 await page.getByRole('button',{name:'Thêm nguồn RSS',exact:true}).first().click();
 const dialog=page.getByRole('dialog');
 assert.equal(await dialog.getByText(/Google News/).count(),0);
 await dialog.getByLabel('URL RSS từ Google Alerts',{exact:true}).fill('https://dangcongsan.vn/rss');
 await dialog.getByLabel('Tên nguồn theo dõi',{exact:true}).fill('Nguồn số hóa Hà Nội');
 assert.equal(await dialog.getByText('Phạm vi theo dõi',{exact:true}).count(),0);
 assert.equal(await dialog.getByRole('combobox').count(),0);
 if(process.env.INTERNET_QA_DIR)await page.screenshot({path:`${process.env.INTERNET_QA_DIR}/internet-tracking-${width}.png`});
 await dialog.getByRole('button',{name:'Lưu nguồn',exact:true}).click();await dialog.waitFor({state:'hidden'});
 assert.equal(payload.metricKeys,undefined);assert.equal(payload.agencyIds,undefined);assert.equal(payload.enabled,false);assert.match(payload.feedUrl,/dangcongsan/);
 const edit=page.getByRole('button',{name:'Sửa',exact:true});await edit.click();
 await page.getByRole('dialog').waitFor();assert.equal(await page.getByRole('dialog').getByLabel('Tên nguồn theo dõi',{exact:true}).inputValue(),'Nguồn số hóa Hà Nội');
 await page.getByRole('dialog').getByRole('button',{name:'Hủy',exact:true}).click();assert.deepEqual(errors,[]);console.log(`${width}px: direct RSS input, no Google News, no goal/agency restrictions, saved RSS and edit restoration passed.`);await page.close();
}}finally{await browser.close();}
