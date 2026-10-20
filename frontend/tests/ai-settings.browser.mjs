import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const page=await browser.newPage({viewport:{width:1280,height:900}});const errors=[];let tested=false,saved=false;
 let data={enabled:false,baseUrl:'https://provider.test/v1',model:'old-model',hasApiKey:true};
 page.on('pageerror',e=>errors.push(e.message));
 await page.route('**/api/**',async route=>{
  const req=route.request(),url=new URL(req.url());let body=[];
  if(url.pathname==='/api/ai-provider'){
   if(req.method()==='PUT'){const value=req.postDataJSON();assert.equal(value.apiKey,'ui-test-key-not-real');saved=true;data={...data,...value,apiKey:undefined,hasApiKey:true};body={message:'Đã lưu cấu hình.'};}
   else body=data;
  }
  if(url.pathname==='/api/ai-provider/models')body={models:['old-model','new-model']};
  if(url.pathname==='/api/ai-provider/test'){assert.equal(req.postDataJSON().apiKey,'ui-test-key-not-real');tested=true;body={message:'Kết nối thành công; chưa lưu.'};}
  await route.fulfill({json:body});
 });
 await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5201');
 await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const {default:View}=await import('/src/views/AiSettingsView.vue');f.authState.user.value={role:1,username:'admin'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const node=document.createElement('main');node.id='settings-test';node.style.padding='16px';document.body.append(node);f.createApp(View).mount(node);});
 const root=page.locator('#settings-test');await root.getByRole('button',{name:'Thay khóa',exact:true}).waitFor();
 assert.equal(await root.locator('#ai-api-key').count(),0);
 await root.getByRole('button',{name:'Thay khóa',exact:true}).click();
 assert.equal(await root.locator('#ai-api-key').getAttribute('type'),'password');
 await root.locator('#ai-api-key').fill('ui-test-key-not-real');
 await root.getByRole('button',{name:'Tải danh sách phiên bản AI',exact:true}).click();
 await root.getByRole('combobox',{name:'Phiên bản AI đọc tài liệu',exact:true}).click();
 await page.getByRole('option',{name:'new-model',exact:true}).click(); await root.getByRole('button',{name:'Kiểm tra kết nối',exact:true}).click();
 await root.getByText('Kết nối thành công; chưa lưu.',{exact:true}).waitFor();assert.equal(tested,true);assert.equal(saved,false);
 await root.getByRole('button',{name:'Lưu cấu hình AI',exact:true}).click();
 await root.getByText('Đã lưu cấu hình.',{exact:true}).waitFor();
 await root.getByRole('button',{name:'Thay khóa',exact:true}).waitFor();assert.equal(await root.locator('#ai-api-key').count(),0);assert.equal(saved,true);
 assert.equal(await page.evaluate(()=>JSON.stringify(localStorage).includes('ui-test-key-not-real')),false);
 await page.setViewportSize({width:1920,height:1080});
 const bounds=await root.locator('header').boundingBox();assert.ok(bounds.width>1800,'Settings must fill wide screens');
 await page.waitForFunction(()=>!document.querySelector('#settings-test button[type=submit]').disabled);
 await page.screenshot({path:'../scratch/ai-build/settings-desktop.png'});
 await page.setViewportSize({width:390,height:844});assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'../scratch/ai-build/settings-mobile.png'});
 assert.deepEqual(errors,[]);console.log('AI settings browser passed: masked/blank key, separate test/save, no key in browser storage, desktop/mobile.');
} finally {await browser.close();}



