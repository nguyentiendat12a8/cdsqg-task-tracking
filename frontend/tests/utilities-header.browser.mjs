import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{for(const width of [1440,390])for(const [route,title] of [['agency-plans','Đầu mối liên hệ và KH CĐS'],['legal-documents','Hệ thống văn bản chuyển đổi số']]){
 const page=await browser.newPage({viewport:{width,height:950}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 await page.route('**/api/**',route=>route.fulfill({json:route.request().url().includes('/api/legaldocuments')?{items:[],totalCount:0,totalPages:1}:[]}));
 await page.goto('http://127.0.0.1:5189/#'+route);const heading=page.getByRole('heading',{name:title,exact:true});await heading.waitFor();
 const gap=await heading.evaluate(node=>{const header=node.closest('.ui-screen-heading');return header.nextElementSibling.getBoundingClientRect().top-header.getBoundingClientRect().bottom;});assert(Math.abs(gap)<1,'Header gap: '+gap);assert.deepEqual(errors,[]);
 await page.screenshot({path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/${route}-header-${width}.png`});console.log(route+' '+width+'px joined header passed');await page.close();
}}finally{await browser.close();}
