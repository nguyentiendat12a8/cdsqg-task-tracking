import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium} = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({channel:'msedge',headless:true});
try {
  const page = await browser.newPage({viewport:{width:390,height:844}});
  const errors=[]; page.on('pageerror',e => errors.push(e.message));
  await page.route('**/api/**',r=>r.fulfill({contentType:'application/json',body:'{"items":[],"totalCount":0}'}));
  await page.goto('http://127.0.0.1:5201');
  await page.evaluate(async () => {
    const {createApp,h,ref,withDirectives}=await import('/node_modules/.vite/deps/vue.js');
    const Select=(await import('/src/components/SearchableSelect.vue')).default;
    const DatePicker=(await import('/src/components/DatePicker.vue')).default;
    const Progress=(await import('/src/components/ProgressUpdateModal.vue')).default;
    const FloatingVue=(await import('/node_modules/.vite/deps/floating-vue.js')).default;
    const {accessibleDialog}=await import('/src/directives/accessibleDialog.js');
    const {accessibleData}=await import('/src/directives/accessibleData.js');
    const {authState}=await import('/src/services/auth.js');
    authState.user.value={role:2,agencyId:'00000000-0000-0000-0000-000000000001'};
    authState.token.value='fixture';
    document.querySelector('#app').style.display='none';
    const root=document.createElement('div');document.body.appendChild(root);
    window.testSelected=ref(null);window.testDate=ref('2026-10-08');window.testModal=ref(false);
    const app=createApp({render(){
      const table=h('table',{style:'width:1600px'},[
        h('thead',[h('tr',[h('th','Mã'),h('th','Nội dung')])]),
        h('tbody',[h('tr',[h('td','NV001'),h('td','Văn bản dài '.repeat(100))])])
      ]);
      return withDirectives(h('main', {style:'padding:12px'}, [
      h(Select,{label:'Cơ quan thực hiện',isMulti:false,options:Array.from({length:200},(_,i)=>({value:i,label:`Cơ quan ${i} — `+'Tên đơn vị rất dài '.repeat(12)})),modelValue:window.testSelected.value,'onUpdate:modelValue':v=>window.testSelected.value=v}),
      h(DatePicker,{modelValue:window.testDate.value,'onUpdate:modelValue':v=>window.testDate.value=v}),
      h('div',{style:'overflow-x:auto;width:350px'},[table])]),[[accessibleData]]);
    }});
    app.use(FloatingVue);app.directive('accessible-dialog',accessibleDialog);app.mount(root);
    const modalRoot=document.createElement('div');document.body.appendChild(modalRoot);
    const modal=createApp({render(){return h(Progress,{isOpen:window.testModal.value,taskId:'00000000-0000-0000-0000-000000000002',taskTitle:'Nhiệm vụ dài '.repeat(20),hasPendingApproval:true,onClose:()=>window.testModal.value=false});}});
    modal.use(FloatingVue);modal.directive('accessible-dialog',accessibleDialog);modal.mount(modalRoot);
  });
  const select=page.getByRole('combobox',{name:'Cơ quan thực hiện'});
  await select.focus();await page.keyboard.press('Enter');
  await page.getByPlaceholder('Tìm nhanh...').fill('Cơ quan 199');
  await page.keyboard.press('ArrowDown');await page.keyboard.press('Enter');
  assert.equal(await page.evaluate(()=>testSelected.value),199);
  await select.focus();await page.keyboard.press('Enter');await page.getByPlaceholder('Tìm nhanh...').fill('Không tồn tại');
  assert.equal(await page.getByRole('option').count(),0);
  await page.keyboard.press('Escape');
  const date=page.getByRole('combobox',{name:'dd/mm/yyyy'});
  await date.focus();await page.keyboard.press('Enter');
  await page.getByRole('button',{name:'2026-10-08',exact:true}).focus();
  await page.keyboard.press('ArrowRight');await page.keyboard.press('Enter');
  assert.equal(await page.evaluate(()=>testDate.value),'2026-10-09');
  await page.getByRole('region',{name:/cuộn ngang/}).focus();await page.keyboard.press('ArrowRight');
  assert.equal(await page.locator('th').first().getAttribute('scope'),'col');
  await page.evaluate(()=>testModal.value=true);
  await page.locator('[aria-modal="true"]').waitFor();
  await page.getByRole('button',{name:/Gửi Báo Cáo Tiến Độ/}).waitFor();
  assert.equal(await page.getByRole('button',{name:/Gửi Báo Cáo Tiến Độ/}).isDisabled(),true);
  await page.keyboard.press('Escape');await page.locator('[aria-modal="true"]').waitFor({state:'hidden'});
  const feedback=await page.evaluate(async()=>{
    const {fetchWithAuth}=await import('/src/services/auth.js');const messages=[];
    window.addEventListener('api-feedback',e=>messages.push(e.detail));
    const original=window.fetch;
    for(const status of [403,429,500]){window.fetch=async()=>new Response('{}',{status});await fetchWithAuth('http://localhost:5000/api/review');}
    window.fetch=original;return messages;
  });
  assert.equal(feedback.length,3);
  assert.deepEqual(errors,[]);
  await page.screenshot({path:'../scratch/data-accessibility-review.png',fullPage:true});
  let resetRequests=0;
  await page.route('**/api/auth/reset-password',r=>{resetRequests++;return r.fulfill({status:501,contentType:'application/json',body:'{"message":"Tính năng đang phát triển"}'});});
  await page.goto('about:blank');
  await page.goto('http://127.0.0.1:5201/#reset-password?token='+ 'A'.repeat(64));
  await page.getByText('Tính năng đang phát triển',{exact:true}).waitFor();
  assert.equal(await page.locator('input').count(),0);
  assert.equal(resetRequests,0);
  console.log('Passed: 200 long agency labels, empty search, keyboard select/date, table semantics, pending report disabled, modal Escape, 403/429/500 feedback.');
  console.log('Passed: recovery feature placeholder, no password fields or reset requests.');
} finally {await browser.close();}
