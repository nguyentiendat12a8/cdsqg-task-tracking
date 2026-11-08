import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{for(const [width,zoom] of [[1440,1],[390,1],[1440,2]]){
 const page=await browser.newPage({viewport:{width,height:1000}});
 await page.route('**/api/**',r=>r.fulfill({json:r.request().url().includes('/items')?{items:[{id:'sample',itemType:'Goal',code:'MT-01',title:'Tỷ lệ hồ sơ và kết quả giải quyết thủ tục hành chính được số hóa — kiểm tra dấu tiếng Việt',leadAgencyName:'Bộ Khoa học và Công nghệ',calculatedStatus:'NotStarted',subItems:[]}],totalCount:1,totalPages:1}:[]}));
 await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5189');
 await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:1,username:'fixture'};const root=document.createElement('div');document.body.replaceChildren(root);const app=f.createApp({render:()=>f.h('main',{class:'ui-density-compact p-4'},[f.h(f.Document,{filterItemType:'Goal'})])});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);});
 await page.getByRole('button',{name:/Xem chi tiết MT-01/}).waitFor();
 await page.evaluate(async zoom=>{document.documentElement.style.zoom=String(zoom);await document.fonts.ready;},zoom);
 const result=await page.evaluate(()=>({font:document.fonts.check('400 14px "Be Vietnam Pro"'),body:getComputedStyle(document.body).fontSize,title:getComputedStyle(document.querySelector('h2')).fontSize,overflow:document.documentElement.scrollWidth>innerWidth+1}));
 assert(result.font);assert.equal(result.title,'20px');assert.equal(result.body,width===390?'16px':'14px');assert.equal(result.overflow,false);
 await page.screenshot({animations:'disabled',path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/typography-${width}-${zoom}.png`});
 console.log('PASS typography',width,zoom,result);await page.close();
}}finally{await browser.close();}
