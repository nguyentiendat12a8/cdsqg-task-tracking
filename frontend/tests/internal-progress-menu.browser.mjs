import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try { for(const viewport of [{width:1366,height:768},{width:390,height:844}]) {
const p=await browser.newPage({viewport});const requests=[];
await p.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
await p.route('**/api/**',r=>{const u=new URL(r.request().url());if(u.pathname==='/api/agencies')return r.fulfill({json:[{id:'ministry',code:'BKHCN',name:'Bộ Khoa học và Công nghệ'},{id:'unit',parentId:'ministry',name:'Cục Chuyển đổi số quốc gia'},{id:'police',name:'Bộ Công an'}]});if(u.pathname==='/api/planning/agency-progress-report'){requests.push(u.searchParams.get('agencyId'));return r.fulfill({json:{goals:[],tasks:[],totalGoals:0,totalTasks:0,totalCount:0,totalPages:1,page:1}});}return r.fulfill({json:[]});});
await p.goto('http://127.0.0.1:5189/#reports-progress-internal');await p.getByRole('heading',{name:'Báo cáo tiến độ trong Bộ',exact:true}).waitFor();await p.getByRole('combobox',{name:'Chọn đơn vị thuộc Bộ KH&CN...'}).waitFor();await p.waitForFunction(()=>document.querySelector('#main-content')?.textContent.includes('Không có mục tiêu'));
if(viewport.width>1000){
 await p.mouse.move(20,300);
 const sizes=await p.evaluate(async()=>{const samples=[];for(let i=0;i<18;i++){await new Promise(r=>requestAnimationFrame(r));const link=document.querySelector('[title="Báo cáo tiến độ trong Bộ"]');if(link)samples.push({height:link.getBoundingClientRect().height,wrap:getComputedStyle(link).whiteSpace,nav:document.querySelector('nav[aria-label="Điều hướng chính"]').getBoundingClientRect().width});}return samples;});
 assert(sizes.length>0);assert(sizes.every(s=>s.wrap==='nowrap'&&s.nav===256));assert.equal(new Set(sizes.map(s=>s.height)).size,1);
 await p.mouse.move(800,300);
}
assert(requests.includes('unit'));assert.equal(await p.getByRole('button',{name:/Đơn vị thuộc Bộ KH/}).count(),0);
await p.reload();await p.getByRole('heading',{name:'Báo cáo tiến độ trong Bộ',exact:true}).waitFor();
await p.evaluate(()=>location.hash='reports-progress');await p.getByRole('heading',{name:'Báo cáo tiến độ mục tiêu, nhiệm vụ',exact:true}).waitFor();await p.getByRole('combobox',{name:'Chọn cơ quan báo cáo...'}).waitFor();assert.equal(await p.getByRole('tab').count(),2);
const selector=await p.getByRole('combobox',{name:'Chọn cơ quan báo cáo...'}).boundingBox();const tabs=await p.getByRole('tablist',{name:'Loại dữ liệu'}).boundingBox();if(viewport.width>1000){assert(tabs.x>selector.x+selector.width);assert(Math.abs((tabs.y+tabs.height/2)-(selector.y+selector.height/2))<8);}assert.equal(await p.locator('body').evaluate(e=>e.scrollWidth>innerWidth),false);
await p.close();}console.log('PASS separate internal report route, correct agency selectors, no old third tab, reload and desktop/mobile navigation');}finally{await browser.close();}
