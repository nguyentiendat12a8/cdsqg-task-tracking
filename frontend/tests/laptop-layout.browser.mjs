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
for(const [width,height] of (process.env.LAPTOP_PINNED ? [[1366,650]] : [[1920,1080],[1366,650],[1280,600],[1024,640],[390,740]])){
 const page=await browser.newPage({viewport:{width,height}});page.setDefaultTimeout(12000);const errors=[];
 page.on('pageerror',e=>{errors.push(e.message);console.log('Theme fixture error',page.url(),e.stack)});
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'theme-test',fullName:'Kiểm thử giao diện'}));});
 await page.route('**/api/**',route=>{
  const u=new URL(route.request().url()),p=u.pathname;let data=[];
  if(p==='/api/agencies'||p==='/api/agency')data=[agency];
  else if(p==='/api/dashboard/metrics')data=metrics;
  else if(/^\/api\/documents\/[^/]+\/items$/.test(p))data={items:Array.from({length:10},(_,i)=>({id:`00000000-0000-0000-0000-${String(i+1).padStart(12,'0')}`,code:`MT-${i+1}`,itemType:u.searchParams.get('itemType')||'Goal',title:'Nội dung mục tiêu hoặc nhiệm vụ cần theo dõi tại cơ quan chủ trì',leadAgencyName:'Bộ Khoa học và Công nghệ',calculatedStatus:'NotStarted',status:'NotStarted',subItems:[]})),totalCount:40,pageNumber:1,totalPages:4};
  else if(p.includes('/items')||p==='/api/user'||p==='/api/legaldocuments'||p.includes('import-history'))data={items:[],totalCount:0,total:0,pageNumber:1,pageSize:10,totalPages:1};
  else if(p==='/api/digital-news')data={items:[],total:0,page:1,pageSize:10};
  else if(p==='/api/digital-news/settings')data={enabled:false,sources:[],keywords:[]};
  else if(p==='/api/ai-data/catalog')data={configured:true,metrics:[],goals:[]};
  else if(p==='/api/ai-provider')data={enabled:false,baseUrl:'',model:'',hasApiKey:false};
  else if(p==='/api/ai-alerts')data={feeds:[],configured:true,allowedHosts:[]};
  else if(p.startsWith('/api/ai-inbox'))data={items:[],total:0};
  else if(p.startsWith('/api/ai-data/')||p.startsWith('/api/ai-alerts/'))data={items:[],total:0};
  return route.fulfill({json:data});
 });

 await page.goto(base);await page.locator('.executive-dashboard').waitFor();
 for(const route of ['dashboard','goals-list','tasks-list','reports','legal-documents','agency-plans','digital-news','ai-data','ai-alerts','ai-settings','agencies','units','users']) {
  await page.goto(base + '/#' + route, {waitUntil:'networkidle'});
  await page.locator('#main-content h1, #main-content h2, #main-content h3, #main-content h4').first().waitFor();
  await page.waitForLoadState('networkidle');
  if(process.env.LAPTOP_PINNED){await page.locator('.app-sidebar .nav-brand button').click();await page.waitForFunction(()=>document.querySelector('.app-sidebar').getBoundingClientRect().width>=250);}
  assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1),`${route} ${width} viewport overflow`);
  const main=page.locator('#main-content');
  if(['goals-list','tasks-list'].includes(route)){const next=main.getByRole('button',{name:'Trang sau ›',exact:true});if(width<768)await next.scrollIntoViewIfNeeded();const r=await next.boundingBox();assert(r.y+r.height<=height,`${route}: pagination below viewport`);}
  const filter=main.getByRole('button',{name:/^(Lọc nâng cao|Bộ lọc nâng cao)/}).first();
  if(await filter.isVisible()) {
   await filter.click();
   const panel=page.locator('.v-popper-filter-overlay .ui-overlay-panel').filter({visible:true});
   await panel.waitFor();
   await page.evaluate(()=>new Promise(resolve=>requestAnimationFrame(()=>requestAnimationFrame(resolve))));
   if(width===1920)assert.equal(await page.locator('.v-popper-filter-compact').count(),0,`${route}: large desktop should keep original popover`);
   const bounds=await panel.boundingBox();
   assert(bounds.x>=0 && bounds.y>=64 && bounds.x+bounds.width<=width+1 && bounds.y+bounds.height<=height-8,`${route}: filter clipped ${JSON.stringify(bounds)}`);
   const apply=panel.getByRole('button',{name:'Áp dụng lọc',exact:true});
   const footer=await apply.boundingBox();
   assert(footer.y+footer.height<=height-8,`${route}: apply offscreen`);
   const select=panel.getByRole('combobox').first();
   if(await select.isVisible()) {
    await select.click();
    const search=page.getByRole('textbox',{name:'Tìm trong các lựa chọn',exact:true});
    await search.waitFor();
    const dropdown=await search.evaluate(el=>{const r=el.parentElement.parentElement.getBoundingClientRect();return {top:r.top,bottom:r.bottom,left:r.left,right:r.right};});
    assert(dropdown.top>=0 && dropdown.bottom<=height && dropdown.left>=0 && dropdown.right<=width,`${route}: select clipped ${JSON.stringify(dropdown)}`);
    await search.press('Escape');
   }
   await panel.locator('.custom-scrollbar').evaluate(el=>{el.scrollTop=el.scrollHeight;});
   const afterScroll=await apply.boundingBox();assert(Math.abs(afterScroll.y-footer.y)<1,'Filter footer moved with fields');
   await page.screenshot({animations:'disabled',path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/laptop-${route}-${width}.png`});
   await panel.getByRole('button',{name:'Đóng bảng tùy chọn',exact:true}).click();
   await panel.waitFor({state:'hidden'});
  }
  if(['goals-list','tasks-list'].includes(route)) {
   await main.getByRole('button',{name:route==='goals-list'?'Thêm mục tiêu mới':'Thêm nhiệm vụ mới',exact:true}).click();
   const modal=page.getByRole('dialog').first();await modal.waitFor();
   const box=await modal.boundingBox();assert(box.y>=0 && box.y+box.height<=height+1,`${route} modal clipped`);
   const footerButton=modal.getByRole('button').last();const footerBox=await footerButton.boundingBox();assert(footerBox.y+footerBox.height<=height,`${route} modal footer clipped`);
   await page.screenshot({animations:'disabled',path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/laptop-modal-${route}-${width}.png`});
   await modal.press('Escape');await modal.waitFor({state:'hidden'});
  }
 }
 assert.deepEqual(errors,[]);console.log(`PASS ${width}x${height}: 13 routes, filters/footer/select in viewport at 100%`);await page.close();
}
}finally{await browser.close();}
