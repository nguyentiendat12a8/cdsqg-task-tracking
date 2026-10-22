import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{for(const width of [1440,390]){
 const page=await browser.newPage({viewport:{width,height:900}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.route('**/api/**',route=>{const p=new URL(route.request().url()).pathname;let data=[];
  if(p==='/api/user')data={items:[{id:'u1',username:'review',fullName:'Người dùng kiểm thử',role:2,isActive:true}],totalCount:1,totalPages:1};
  if(p==='/api/units')data={items:[{id:'unit1',name:'Sản phẩm',dataType:'Number'}],totalCount:1,totalPages:1};
  return route.fulfill({json:data});
 });
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 await page.goto(process.env.FRONTEND_TEST_URL||'http://127.0.0.1:5173');
 for(const route of ['units','users']){
  await page.evaluate(route=>{location.hash=route},route);
  const edit=page.getByTitle(route==='units'?'Sửa':'Sửa thông tin',{exact:true});await edit.waitFor();
  await edit.scrollIntoViewIfNeeded();
  assert.ok(await edit.evaluate(el=>el.classList.contains('ui-action-primary')));
  const color=await edit.evaluate(el=>({button:getComputedStyle(el).color,svg:getComputedStyle(el.querySelector('svg')).color}));assert.equal(color.button,color.svg);assert.equal(color.button,'rgb(48, 79, 142)');
  const del=page.getByTitle(route==='units'?'Xóa':'Xóa tài khoản',{exact:true});assert.ok(await del.evaluate(el=>el.classList.contains('ui-action-danger')));
  if(route==='users'){const reset=page.getByTitle('Đặt lại mật khẩu',{exact:true});assert.equal(await reset.locator('svg').count(),1);assert.equal(await reset.locator('svg').evaluate(el=>getComputedStyle(el).color),'rgb(133, 86, 16)');}
 }
 await page.evaluate(async()=>{
  document.querySelector('#app').__vue_app__?.unmount();
  const {createApp,h,FloatingVue}=await import('/tests/refactor-fixture.js');
  const {default:Table}=await import('/src/features/document/DocumentItemsTable.vue');
  const {default:ActionButton}=await import('/src/components/ActionButton.vue');
  const {getStatusLabel,getStatusBadgeClass}=await import('/src/shared/statusPresentation.js');
  window.actionCalls=[];const call=name=>()=>window.actionCalls.push(name);
  const props={authState:{isAdmin:{value:true}},filterItemType:'Goal',paginatedPrimaryList:[{id:'g1',code:'MT-01',title:'Mục tiêu kiểm thử',calculatedStatus:'NotStarted'}],formatDateRange:()=>'',getStatusLabel,getStatusBadgeClass,formatProgressDisplay:()=>'',isGeneralTaskOrAllAgencies:()=>false,sortBy:'code',sortOrder:'asc',handleSort:()=>{},canUpdateProgress:()=>true,isCoordinatingOnly:()=>false,openProgressModal:call('progress'),isLeadAgencyFilteredByBKHCN:false,canAssignTask:()=>true,openAssignModalFromList:call('assign'),openNotificationModal:call('notify'),openItemDetailModal:()=>{},hasProgress:()=>false,handleDeleteItem:call('delete'),openEditModal:call('edit')};
  const app=createApp({render:()=>h('main',{style:'width:100%;overflow:auto;padding:16px'},[h(Table,props),h(ActionButton,{kind:'create',disabled:true,onClick:call('disabled')},()=> 'Thêm bị khóa'),h('button',{class:'ui-action ui-action-primary',disabled:true,onClick:call('disabled')},[h('svg',{width:16,height:16,stroke:'currentColor'},[h('path',{d:'M2 2L14 14'})]),'Thao tác bị khóa'])])});app.use(FloatingVue);app.mount('#app');
 });
 const progress=page.getByTitle('Cập nhật tiến độ và minh chứng',{exact:true});await progress.scrollIntoViewIfNeeded();await progress.click();assert.deepEqual(await page.evaluate(()=>window.actionCalls),['progress']);
 assert.equal(await progress.locator('svg').evaluate(el=>getComputedStyle(el).color),'rgb(48, 79, 142)');
 const more=page.getByRole('button',{name:'Thao tác khác',exact:true});await more.click();
 const edit=page.getByTitle('Chỉnh Sửa',{exact:true}),del=page.getByTitle('Xóa',{exact:true});await edit.waitFor();
 assert.equal(await edit.evaluate(el=>getComputedStyle(el).color),'rgb(48, 79, 142)');assert.equal(await del.evaluate(el=>getComputedStyle(el).color),'rgb(150, 52, 73)');
 await edit.hover();await page.waitForFunction(()=>document.querySelector('button[title="Chỉnh Sửa"]').getAnimations().every(a=>a.playState!=='running'));
 assert.equal(await edit.evaluate(el=>getComputedStyle(el).backgroundColor),'rgb(229, 237, 250)');await edit.click();assert.deepEqual(await page.evaluate(()=>window.actionCalls),['progress','edit']);
 const disabled=page.getByRole('button',{name:'Thao tác bị khóa',exact:true});assert.ok(await disabled.isDisabled());assert.equal(await disabled.locator('svg').evaluate(el=>getComputedStyle(el).color),'rgb(148, 163, 184)');assert.ok(await page.getByRole('button',{name:'Thêm bị khóa',exact:true}).isDisabled());
 assert.deepEqual(errors,[]);console.log(`${width}px: actual unit/user icons, password SVG, progress callback, dropdown colors/hover, edit callback and disabled controls passed.`);await page.close();
}}finally{await browser.close();}
