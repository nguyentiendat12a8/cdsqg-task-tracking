import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{for(const width of [1440,390]){
 const page=await browser.newPage({viewport:{width,height:950}});page.setDefaultTimeout(12000);const errors=[];page.on('pageerror',e=>errors.push(e.message));
 let state='Unreadable',saves=0,models=0;
 const message='Không giải mã được API key đã lưu. Nhập lại API key và lưu.';
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 await page.route('**/api/**',route=>{
  const url=new URL(route.request().url());let data=[];
  if(url.pathname==='/api/ai-provider'){
   if(route.request().method()==='PUT'){
    const body=route.request().postDataJSON();saves++;
    if(body.removeKey){assert.equal(body.enabled,false);state='Missing';}
    else{assert.equal(body.apiKey,'fixture-replacement');state='Available';}
    return route.fulfill({json:{message:'Đã lưu cấu hình.'}});
   }
   data={enabled:state!=='Missing',baseUrl:'https://example.com/v1',model:'fixture-model',hasApiKey:state==='Available',hasStoredApiKey:state!=='Missing',keyStatus:state,keyError:state==='Unreadable'?message:null,configured:state==='Available'};
  }else if(url.pathname==='/api/ai-provider/models'){models++;data={models:['fixture-model']};}
  return route.fulfill({json:data});
 });
 const base=process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5188';
 await page.goto(base+'/#ai-settings');
 await page.getByRole('alert').filter({hasText:message}).waitFor();
 assert.ok(await page.getByText('Không giải mã được',{exact:true}).isVisible());
 assert.equal(models,0);assert.ok(await page.getByRole('button',{name:'Kiểm tra kết nối',exact:true}).isDisabled());
 await page.getByLabel('API key',{exact:true}).fill('fixture-replacement');
 await page.getByRole('button',{name:'Lưu cấu hình AI',exact:true}).click();
 await page.getByText('Đã lưu khóa API',{exact:true}).waitFor();
 assert.equal(saves,1);assert.equal(await page.getByRole('alert').filter({hasText:message}).count(),0);
 assert.ok(!((await page.locator('body').innerText()).includes('fixture-replacement')));
 await page.reload();await page.getByText('Đã lưu khóa API',{exact:true}).waitFor();
 assert.equal(await page.getByLabel('API key',{exact:true}).count(),0);
 // A lost key can also be removed explicitly; no silent overwrite on a normal save.
 state='Unreadable';await page.reload();await page.getByRole('alert').filter({hasText:message}).waitFor();
 await page.getByText('Tùy chọn nâng cao',{exact:true}).click();
 await page.getByLabel('Xóa khóa đang lưu khi bấm Lưu (cần tắt xử lý AI)',{exact:true}).check();
 await page.getByLabel('Cho phép AI xử lý dữ liệu',{exact:false}).uncheck();
 await page.getByRole('button',{name:'Lưu cấu hình AI',exact:true}).click();await page.getByText('Chưa có',{exact:true}).waitFor();assert.equal(saves,2);
 assert.deepEqual(errors,[]);console.log(`${width}px: unreadable diagnosis, replacement, reload, no plaintext display, disabled test button and explicit removal passed.`);await page.close();
}}finally{await browser.close();}
