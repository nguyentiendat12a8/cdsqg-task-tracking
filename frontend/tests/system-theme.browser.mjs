import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE || import.meta.url)('playwright');
const base=process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5173';
const agency={id:'11111111-1111-1111-1111-111111111111',agencyId:'11111111-1111-1111-1111-111111111111',code:'TEST',name:'Bộ Khoa học và Công nghệ',type:'Ministry',totalItems:2,totalGoals:1,totalTasks:1,goalNotStarted:1,taskInProgress:1,notStarted:1,inProgress:1,completed:0,completionRateOverall:0,contactPersons:[]};
const states={notStarted:1,inProgress:0,completed:0};
const metrics={totalGoals:1,totalTasks:1,overviewMetrics:{totalGoals:1,totalTasks:1,goalStatusSummary:states,taskStatusSummary:{notStarted:0,inProgress:1,completed:0}},goalStatusSummary:states,taskStatusSummary:{notStarted:0,inProgress:1,completed:0},ministriesPerformance:[agency],provincesPerformance:[],othersPerformance:[],groupStatistics:[]};
function contrast(a,b){const luminance=x=>{const v=x.match(/[\d.]+/g).slice(0,3).map(Number).map(n=>{n/=255;return n<=.04045?n/12.92:((n+.055)/1.055)**2.4});return v[0]*.2126+v[1]*.7152+v[2]*.0722};const x=luminance(a),y=luminance(b);return(Math.max(x,y)+.05)/(Math.min(x,y)+.05);}
const browser=await chromium.launch({channel:'msedge',headless:true});
try{
for(const width of [1440,390]){
 const page=await browser.newPage({viewport:{width,height:1000}});page.setDefaultTimeout(12000);const errors=[];
 page.on('pageerror',e=>{errors.push(e.message);console.log('Theme fixture error',page.url(),e.stack)});
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'theme-test',fullName:'Kiểm thử giao diện'}));});
 await page.route('**/api/**',route=>{
  const u=new URL(route.request().url()),p=u.pathname;let data=[];
  if(p==='/api/agencies'||p==='/api/agency')data=[agency];
  else if(p==='/api/dashboard/metrics')data=metrics;
  else if(p.includes('/items')||p==='/api/user'||p==='/api/legaldocuments'||p.includes('import-history'))data={items:[],totalCount:0,total:0,pageNumber:1,pageSize:10,totalPages:1};
  else if(p==='/api/ai-data/catalog')data={configured:true,metrics:[],goals:[]};
  else if(p==='/api/ai-provider')data={enabled:false,baseUrl:'',model:'',hasApiKey:false};
  else if(p==='/api/ai-alerts')data={feeds:[],configured:true,allowedHosts:[]};
  else if(p.startsWith('/api/ai-inbox'))data={items:[],total:0};
  else if(p.startsWith('/api/ai-data/')||p.startsWith('/api/ai-alerts/'))data={items:[],total:0};
  return route.fulfill({json:data});
 });
 await page.goto(base);await page.locator('.executive-dashboard').waitFor();
 const token=await page.evaluate(()=>getComputedStyle(document.documentElement).getPropertyValue('--ui-header-background').trim());assert.equal(token,'#f1f5fc');
 if(width<768)await page.getByRole('button',{name:'Mở hoặc đóng menu'}).click();
 await page.locator('.app-sidebar').hover();await page.getByTitle('Bấm để ghim cố định Menu',{exact:true}).click();await page.waitForFunction(()=>document.querySelector('.app-sidebar').getAnimations({subtree:true}).every(a=>a.playState!=='running'));
 const active=page.locator('.nav-link.is-active').first();await active.waitFor();await active.focus();await page.keyboard.press("Tab");await page.keyboard.press("Shift+Tab");
 const navColors=await active.evaluate(el=>{const s=getComputedStyle(el),n=getComputedStyle(el.closest('aside'));return {fg:s.color,bg:s.backgroundColor,nav:n.backgroundColor,outline:s.outlineStyle};});
 assert.equal(navColors.bg,'rgb(59, 95, 168)');assert.equal(navColors.nav,'rgb(241, 245, 252)');assert.ok(contrast(navColors.fg,navColors.bg)>=4.5);assert.notEqual(navColors.outline,'none');
 const idle=page.locator('.nav-link:not(.is-active)').first();const idleColor=await idle.evaluate(el=>getComputedStyle(el).color);assert.ok(contrast(idleColor,navColors.nav)>=4.5);
 if(process.env.THEME_QA_DIR)await page.screenshot({path:`${process.env.THEME_QA_DIR}/theme-${width}-menu.png`});
 if(width<768)await page.getByRole('button',{name:'Đóng menu',exact:true}).click({position:{x:width-10,y:100},force:true});
 for(const [route,selector]of [['goals-list','.ui-screen-heading'],['tasks-list','.ui-screen-heading'],['goals-grid','.ui-screen-heading'],['reports','.ui-screen-heading'],['legal-documents','.ui-screen-heading'],['agency-plans','.ui-screen-heading'],['agencies','.ui-section-heading'],['units','.ui-section-heading'],['users','.ui-screen-heading'],['import-history','.ui-section-heading'],['ai-data','.ai-heading-row'],['ai-alerts','.ai-heading-row'],['ai-settings','.ai-heading-row']]){
  await page.evaluate(route=>{location.hash=route;},route);
  const heading=page.locator('#main-content '+selector).first();await heading.waitFor();await page.waitForLoadState("networkidle");await page.evaluate(()=>new Promise(resolve=>requestAnimationFrame(()=>requestAnimationFrame(resolve))));await page.waitForFunction(()=>document.querySelector(".app-sidebar").getAnimations({subtree:true}).every(a=>a.playState!=="running"));assert.equal(await page.locator(".nav-link.is-active").count(),1,route);
  assert.equal(await heading.evaluate(el=>getComputedStyle(el).backgroundColor),'rgb(241, 245, 252)',`${width} ${route}`);
  const title=heading.locator('h1,h2,h3').first();if(await title.count()){const colors=await title.evaluate(el=>({fg:getComputedStyle(el).color,bg:getComputedStyle(el.closest('.ui-screen-heading,.ui-section-heading,.ai-heading-row')).backgroundColor}));assert.ok(contrast(colors.fg,colors.bg)>=4.5,`${route} heading contrast`);}
  const th=page.locator('#main-content thead th').first();if(await th.isVisible())assert.equal(await th.evaluate(el=>getComputedStyle(el).backgroundColor),'rgb(241, 245, 252)',`${route} table header`);
  assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),`${route} document overflow`);
  if(process.env.THEME_QA_DIR&&['tasks-list','users','ai-data'].includes(route))await page.screenshot({path:`${process.env.THEME_QA_DIR}/theme-${width}-${route}.png`});
 }
 assert.deepEqual(errors,[],`${width}px runtime errors`);console.log(`${width}px: menu active/idle/focus contrast, 13 screens, header/table palette and viewport passed.`);await page.close();
}
}finally{await browser.close();}
