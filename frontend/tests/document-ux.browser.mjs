import { createRequire } from 'node:module';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({channel:'msedge',headless:true});
try {
for (const width of [1440,390]) for (const type of ['Goal','Task']) {
const page = await browser.newPage({viewport:{width,height:950}});
await page.route('**/api/**',r=>r.fulfill({json:r.request().url().includes('/items')?{items:[{id:'ux-row',itemType:type,code:'MT-01',title:'Mục tiêu kiểm tra bàn phím',leadAgencyName:'Cơ quan kiểm tra',calculatedStatus:'NotStarted',subItems:[]}],totalCount:1,totalPages:1}:[]}));
await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5189');
await page.evaluate(async type=>{
const f=await import('/tests/refactor-fixture.js'); f.authState.user.value={role:1,username:'fixture'};
const root=document.createElement('div');document.body.replaceChildren(root);
const app=f.createApp({render:()=>f.h(f.Document,{filterItemType:type})});app.use(f.FloatingVue);app.directive('accessible-dialog',f.accessibleDialog);app.directive('accessible-data',f.accessibleData);app.mount(root);
},type);
const trigger=page.getByRole('button',{name:'Tính năng',exact:true});await trigger.click();
await page.getByRole('button',{name:'Xuất Excel danh sách',exact:true}).waitFor();
await page.waitForFunction(()=>document.activeElement?.textContent.includes('Xuất Excel báo cáo tiến độ'));
const panel=await page.getByRole('dialog',{name:'Tính năng Excel'}).boundingBox();
const anchor=await trigger.boundingBox();
if(panel.x<0 || panel.x+panel.width>width+1 || panel.y < anchor.y+anchor.height-1) throw Error('menu overlaps header or viewport '+JSON.stringify({panel,anchor}));

for(const name of ['Xuất Excel báo cáo tiến độ','Nhập Excel báo cáo tiến độ']) if(!await page.getByRole('button',{name,exact:true}).isVisible())throw Error(name);
await page.getByRole('button',{name:'Xuất Excel danh sách',exact:true}).focus();await page.keyboard.press('Escape');
if(await trigger.getAttribute('aria-expanded')!=='false')throw Error('Escape did not close');
await trigger.click();await page.mouse.click(5,900);
if(await trigger.getAttribute('aria-expanded')!=='false')throw Error('outside did not close');
const nameHeader=page.locator('th').filter({hasText:type==='Goal'?'Tên mục tiêu':'Tên nhiệm vụ'});
await nameHeader.press('Enter');
if(await nameHeader.getAttribute('aria-sort')!=='ascending')throw Error('sort Enter');
await nameHeader.press('Space');
if(await nameHeader.getAttribute('aria-sort')!=='descending')throw Error('sort Space');
if(width===390 && await page.locator('tbody td').nth(1).evaluate(e=>getComputedStyle(e).position)==='sticky')throw Error('sticky title mobile');
await page.getByRole('button',{name:/Xem chi tiết MT-01/}).press('Enter');
await page.getByRole('dialog').first().waitFor();
console.log('PASS',type,width);await page.close();
}
}finally{await browser.close();}
