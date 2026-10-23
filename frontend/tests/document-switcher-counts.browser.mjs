import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 for(const width of [1440,390]){
  const page=await browser.newPage({viewport:{width,height:900}});page.setDefaultTimeout(12000);
  let fail=false,goals=3,tasks=5;const errors=[],requests=[];
  page.on('pageerror',error=>errors.push(error.message));
  await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));localStorage.setItem('cdsqg_active_doc_code','1266');});
  await page.route('**/api/**',async route=>{
   const url=new URL(route.request().url());
   if(url.pathname!=='/api/documents'){await route.fulfill({json:[]});return;}
   requests.push({query:url.search,fail,goals,tasks});
   if(fail){await route.fulfill({status:500,json:{message:'fixture failure'}});return;}
   await route.fulfill({json:{items:url.searchParams.get('pageNumber')==='2'?[{id:'12660000-0000-0000-0000-000000001266',totalGoals:goals,totalTasks:tasks}]:[{id:'26290000-0000-0000-0000-000000002629',totalGoals:0,totalTasks:75}],totalCount:2}});
  });
  await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5207',{waitUntil:'networkidle'});
  await page.evaluate(async()=>{
   const f=await import('/tests/refactor-fixture.js');const {default:Switcher}=await import('/src/components/DocumentModeSwitcher.vue');
   f.authState.user.value={role:1,username:'fixture'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';
   const root=document.createElement('div');root.id='switcher-review';root.style='padding:20px;display:flex;justify-content:flex-end';document.body.append(root);
   const app=f.createApp({render:()=>f.h(Switcher)});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.mount(root);
  });
  const trigger=page.locator('#switcher-review').getByRole('button',{name:'Chọn chiến lược',exact:true});
  await trigger.click();const picker=page.getByRole('group',{name:'Danh sách chiến lược',exact:true});
  await picker.getByText('3 mục tiêu',{exact:true}).waitFor();await picker.getByText('5 nhiệm vụ',{exact:true}).waitFor();await picker.getByText('0 mục tiêu',{exact:true}).waitFor();
  assert(requests.some(request=>request.query.includes('pageNumber=2')));
  assert.equal(await picker.getByText('27 mục tiêu',{exact:true}).count(),0);assert.equal(await picker.getByText('82 nhiệm vụ',{exact:true}).count(),0);
  await picker.getByRole('button').first().click();
  fail=true;await trigger.click();await page.getByText('Chưa tải được số lượng mục tiêu, nhiệm vụ.',{exact:true}).waitFor();
  assert.equal(await picker.getByText('3 mục tiêu',{exact:true}).count(),0);assert.equal(await picker.getByText('— mục tiêu',{exact:true}).count(),2);
  fail=false;goals=4;tasks=6;await page.getByRole('button',{name:'Thử lại',exact:true}).click();
  await picker.getByText('4 mục tiêu',{exact:true}).waitFor();await picker.getByText('6 nhiệm vụ',{exact:true}).waitFor();
  await page.screenshot({path:`${process.env.TEMP}/document-counts-${width}.png`,fullPage:true});
  assert.deepEqual(errors,[]);await page.close();
 }
 console.log('PASS document switcher counts: API totals across pages, zero, refresh, error/retry, desktop/mobile');
} finally {await browser.close();}
