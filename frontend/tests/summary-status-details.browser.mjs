import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium }=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
  const page=await browser.newPage({viewport:{width:1440,height:900}});
  const requests=[], errors=[]; let legacy=false, forbidden=false; page.on('pageerror',e=>errors.push(e.message));
  const agencies=Array.from({length:17},(_,index)=>({id:`a${index+1}`,name:`Bộ ${String(index+1).padStart(2,'0')}`,code:`M${index+1}`,type:'Ministry'}));
  const row={id:'shared-task',code:'MT-1',title:'Mục tiêu giao chung',itemType:'Goal',isGeneralTask:true,unitName:'%',calculatedStatus:'NotStarted'};
  const assigned=agencies.map((a,index)=>({...row,leadAgencyId:a.id,leadAgencyName:a.name,progressAgencyId:a.id,latestProgressPercent:index}));
  await page.route('**/api/**',async route=>{
    const url=new URL(route.request().url());let data=[];
    if(url.pathname==='/api/agencies')data=agencies;
    if(url.pathname==='/api/dashboard/metrics') {
      if(url.searchParams.has('detailMode')) {
        if(legacy)return route.fulfill({json:{createdMetrics:{},goalStatusSummary:{}}});
        if(forbidden)return route.fulfill({status:403,json:{}});
        requests.push(url.searchParams.toString());
        let rows=url.searchParams.get('detailMode')==='assignments'?assigned:[{...row,id:'specific-task',code:'MT-2',leadAgencyName:'Bộ 01',isGeneralTask:false}];
        if(url.searchParams.get('detailStatus')==='CompletedOnTime')rows=[];
        if(url.searchParams.get('search'))rows=rows.filter(r=>r.leadAgencyName.includes(url.searchParams.get('search')));
        data={items:rows,totalCount:rows.length,totalPages:rows.length?1:0};
      } else data={createdMetrics:{totalGoals:1,goalsSpecific:1,goalsGeneral:0,goalStatusSummary:{notStarted:1}},goalStatusSummary:{notStarted:17},ministriesPerformance:agencies.map(a=>({...a,agencyId:a.id,totalGoals:1,totalItems:1,goalNotStarted:1})),provincesPerformance:[],othersPerformance:[]};
    }
    await route.fulfill({json:data});
  });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  await page.evaluate(async()=>{
    const f=await import('/tests/refactor-fixture.js');f.authState.user.value={role:1};f.authState.token.value='';document.querySelector('#app').style.display='none';
    const root=document.createElement('div');root.id='summary-detail-test';document.body.append(root);
    const app=f.createApp({render:()=>f.h(f.Dashboard)});app.directive('accessible-dialog',f.accessibleDialog);app.mount(root);
  });
  const root=page.locator('#summary-detail-test');
  await root.getByRole('button',{name:'Xem Chưa thực hiện — danh sách',exact:true}).click();
  await root.getByText('1 bản ghi',{exact:true}).waitFor();
  assert.equal(await root.locator('tbody tr').count(),1);
  await root.getByRole('button',{name:'Đóng danh sách thống kê',exact:true}).click();
  await root.getByRole('button',{name:'Xem Chưa thực hiện — lượt giao theo cơ quan',exact:true}).click();
  await root.getByText('17 bản ghi',{exact:true}).waitFor();
  assert.equal(await root.locator('tbody tr').count(),17);
  assert.equal(await root.locator('tbody').getByText('MT-1',{exact:true}).count(),17);
  await root.locator('tbody').getByText('Bộ 17',{exact:true}).waitFor();
  await root.getByLabel('Tìm trong danh sách thống kê').fill('Bộ 17');
  await root.getByText('1 bản ghi',{exact:true}).waitFor();
  assert.equal(await root.locator('tbody tr').count(),1);
  const historyRequest=page.waitForRequest(request=>request.url().includes('/progress-history'));
  await root.locator('tbody').getByRole('button',{name:'Chi tiết',exact:true}).click();
  assert.equal(new URL((await historyRequest).url()).searchParams.get('agencyId'),'a17');
  await root.getByRole('button',{name:'✕',exact:true}).first().click();
  await page.setViewportSize({width:390,height:844});
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
  await root.getByRole('button',{name:'Đóng danh sách thống kê',exact:true}).click();
  await root.getByRole('button',{name:'Xem Hoàn thành (đúng hạn) — danh sách',exact:true}).click();
  await root.getByText('Không có bản ghi phù hợp.',{exact:true}).waitFor();
  legacy=true;
  await root.getByRole('button',{name:'Đóng danh sách thống kê',exact:true}).click();
  await root.getByRole('button',{name:'Xem Chưa thực hiện — danh sách',exact:true}).click();
  await root.getByText('BE đang chạy chưa hỗ trợ danh sách thống kê chi tiết.',{exact:false}).waitFor();
  assert.equal(await root.getByText('0 bản ghi',{exact:true}).count(),0);
  legacy=false;forbidden=true;
  await root.getByRole('button',{name:'Thử lại',exact:true}).click();
  await root.getByText('Tài khoản không có quyền xem danh sách thống kê chi tiết.',{exact:false}).waitFor();
  assert.ok(requests.some(q=>q.includes('detailMode=assignments')&&q.includes('detailStatus=NotStarted')));
  assert.deepEqual(errors,[]);
  console.log('Passed first chart single items, second chart 17 same-code recipient rows, search, zero status and mobile overflow. API mocked.');
} finally {await browser.close();}
