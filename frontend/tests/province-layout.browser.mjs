import { createRequire } from 'node:module';
import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const XLSX = createRequire(import.meta.url)('xlsx-js-style');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
const base = process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5201';
const agency = { id: '11111111-1111-1111-1111-111111111111', name: 'Cơ quan kiểm thử tiếng Việt', code: 'TEST', type: 'Ministry', parentId: null };
const goal = { id: '22222222-2222-2222-2222-222222222222', taskId: '22222222-2222-2222-2222-222222222222', itemType: 'Goal', code: 'MT001', section: 'Mục A', title: 'Mục tiêu kiểm thử tiếng Việt', leadAgencyId: agency.id, leadAgencyName: agency.name, calculatedStatus: 'NotStarted', status: 'NotStarted', startDate: '2026-01-01', dueDate: '2030-12-31', latestProgressValue: null, unitName: '%', subItems: [] };
const task = { ...goal, id: '33333333-3333-3333-3333-333333333333', taskId: '33333333-3333-3333-3333-333333333333', itemType: 'Task', code: 'NV001', title: 'Nhiệm vụ kiểm thử tiếng Việt' };
const status = { notStarted: 1, totalGoals: 1, totalTasks: 1 };
const metrics = { totalGoals: 1, totalTasks: 1, createdMetrics: { totalGoals: 1, totalTasks: 1, goalsSpecific: 1, tasksSpecific: 1, goalStatusSummary: status, taskStatusSummary: status }, goalStatusSummary: status, taskStatusSummary: status, ministriesPerformance: [{ ...agency, agencyId: agency.id, totalItems: 1, totalGoals: 1, totalTasks: 1, notStarted: 1 }], provincesPerformance: [{ ...agency, name: "An Giang", agencyId: agency.id, type: "Province", totalItems: 1, totalGoals: 1, totalTasks: 1, goalCompletionRate: 0, taskCompletionRate: 0 }], othersPerformance: [] };
try {
  metrics.createdMetrics.totalGoals = 6;
  metrics.createdMetrics.totalTasks = 5;
  metrics.createdMetrics.goalsSpecific = 5; metrics.createdMetrics.goalsGeneral = 1;
  metrics.createdMetrics.tasksSpecific = 4; metrics.createdMetrics.tasksGeneral = 1;
  metrics.createdMetrics.goalStatusSummary = { notStarted:1, inProgressOnTime:1, expiringSoon:1, completedOnTime:1, completedOverdue:1 };
  metrics.createdMetrics.taskStatusSummary = { notStarted:2, inProgressOverdue:1, completedOverdue:1 };
  metrics.overviewMetrics = {
    goalStatusSummary: { notStarted:1, inProgressOnTime:1, inProgressOverdue:1, expiringSoon:1, completedOnTime:1, completedOverdue:1 },
    taskStatusSummary: { notStarted:2, inProgressOnTime:0, inProgressOverdue:1, expiringSoon:0, completedOnTime:0, completedOverdue:2 }
  };
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 }, acceptDownloads: true });
  page.setDefaultTimeout(10000);
  const errors = [], warnings = [], requests = [];
  page.on('pageerror', e => { errors.push(e.message); console.error('Browser error:', e.message); });
  page.on('console', m => { if (m.type() === 'warning' && m.text().includes('[Vue warn]')) warnings.push(m.text()); });
  page.on('request', r => requests.push(r.url()));
  let failItems = false, lastEdit, lastCreate;
  let exportMultiplePages = false;
  await page.route('**/api/**', async route => {
    const url = new URL(route.request().url());
    let data = [];
    if (route.request().method() === 'POST' && url.pathname === '/api/planning/items') {
      lastCreate = route.request().postDataJSON();
      return route.fulfill({ contentType: 'application/json', body: '{}' });
    }
    if (route.request().method() === 'PUT' && url.pathname.startsWith('/api/planning/items/')) {
      lastEdit = route.request().postDataJSON();
      return route.fulfill({ contentType: 'application/json', body: '{}' });
    }
    if (url.pathname === '/api/agencies') data = [agency];
    else if (url.pathname === '/api/dashboard/metrics') data = url.searchParams.has('parentAgencyId') ? { ministriesPerformance: [] } : metrics;
    else if (url.pathname === '/api/dashboard/agency-items') data = [goal, task];
    else if (url.pathname.match(/\/api\/documents\/.*\/items/)) {
      if (failItems) return route.fulfill({ status: 500, contentType: 'application/json', body: '{}' });
      data = { items: url.searchParams.get('itemType') === 'Goal' ? [goal] : [task], totalCount: 1, totalPages: 1 };
      if (exportMultiplePages && url.searchParams.get('pageSize') === '100') {
        const page = Number(url.searchParams.get('pageNumber'));
        data = { items: page === 1 ? Array.from({ length: 100 }, (_, i) => ({ ...goal, id: `export-${i}`, title: `Dòng xuất ${i}` })) : [{ ...goal, title: 'Dòng xuất cuối trang hai' }], totalCount: 101, totalPages: 2 };
      }
      const search = url.searchParams.get('search');
      if (search === 'old' || search === 'new') {
        data.items = [{ ...goal, title: search + ' response' }];
        await new Promise(resolve => setTimeout(resolve, search === 'old' ? 700 : 10));
      }
    } else if (url.pathname.includes('grid')) data = { items: [], agencies: [], baselines: [] };
    else if (url.pathname.includes('dashboard-stats')) data = {};
    await route.fulfill({ contentType: 'application/json', body: JSON.stringify(data) });
  });
  await page.goto(base);
  await page.evaluate(async () => {
    const fixture = await import('/tests/refactor-fixture.js');
    fixture.authState.user.value = { role: 1, username: 'review', fullName: 'Kiểm thử' };
    fixture.authState.token.value = 'fixture-token';
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'review-root'; root.className = 'ui-density-compact'; document.body.appendChild(root);
    window.reviewMode = fixture.ref('dashboard');
    window.reviewType = fixture.ref('Goal');
    const app = fixture.createApp({ render: () => fixture.h(window.reviewMode.value === 'dashboard' ? fixture.Dashboard : fixture.Document, { filterItemType: window.reviewType.value }) });
    app.use(fixture.FloatingVue);
    app.directive('accessible-dialog', fixture.accessibleDialog);
    app.directive('accessible-data', fixture.accessibleData);
    app.mount(root);
  });

  const root=page.locator('#review-root');
  assert.equal(await page.evaluate(() => getComputedStyle(document.documentElement).getPropertyValue('--color-blue-600').trim()), '#315ead');
  assert.equal(await page.evaluate(() => getComputedStyle(document.documentElement).getPropertyValue('--color-slate-900').trim()), '#18273b');
  const panel=root.locator('#agency-panel-provinces');
  const map=root.getByTestId('province-map-panel');
  const chart=panel.getByTestId('province-chart-panel');
  await map.waitFor();
  assert.equal(await panel.getByRole('button',{name:'Bản đồ',exact:true}).count(),0);
  assert.equal(await panel.getByRole('button',{name:'Biểu đồ cột',exact:true}).getAttribute('aria-pressed'),'true');
  const svg=map.locator('svg[aria-label="Bản đồ 34 tỉnh thành Việt Nam"]');
  const search=map.locator('#province-map-search');
  assert.equal(await search.isVisible(),true,'Province list shown beside map initially');
  await search.fill('An Giang');
  await map.getByRole('button',{name:'Thu gọn',exact:true}).click();
  assert.equal(await search.isVisible(),false);
  await map.getByRole('button',{name:'Xem chi tiết',exact:true}).click();
  assert.equal(await search.inputValue(),'An Giang','Collapse preserves search');
  const desktopMap=await map.boundingBox(), desktopChart=await chart.boundingBox();
  assert.ok(desktopMap.y < desktopChart.y, 'Map moved above the agency block');
  assert.equal(await panel.getByTestId('province-map-panel').count(),0,'No duplicate map in province chart block');
  assert.equal(await root.getByText('Thống kê mục tiêu',{exact:true}).count(),1,'Restored item donut');
  assert.equal(await root.locator('section[aria-label="Các chỉ số điều hành"]').count(),0,'Removed four cards');
  assert.equal(await root.getByTestId('admin-overview-left').locator('[style*="conic-gradient"]').count(),2,'Two donut charts on left');
  await page.evaluate(() => window.scrollTo(0,0));
  await page.screenshot({path:'tests/admin-overview-desktop.png'});
  const canvasBox = await map.getByTestId('province-map-canvas').boundingBox();
  const directoryBox = await map.getByTestId('province-map-directory').boundingBox();
  assert.ok(directoryBox.x >= canvasBox.x + canvasBox.width - 2, 'Directory is right of map');
  const left = await root.getByTestId('admin-overview-left').boundingBox();
  assert.ok(Math.abs(left.height-desktopMap.height)<2,'Map matches total height of both donut charts');
  assert.ok((await svg.boundingBox()).height>300,'Map fills available height');
  assert.ok(Math.abs(left.width-desktopMap.width)<2,'Equal overview column widths');
  assert.ok(desktopMap.x>=left.x+left.width,'Map is right of the two donuts');
  const statsChart = root.getByTestId('admin-overview-left').getByText('Thống kê mục tiêu giao cho các Bộ, ngành, địa phương và đơn vị khác',{exact:true});
  assert.ok((await statsChart.boundingBox()).y>(await root.getByText('Thống kê mục tiêu',{exact:true}).boundingBox()).y);
  const originalView=await svg.getAttribute('viewBox');
  await map.getByRole('button',{name:'Phóng to bản đồ',exact:true}).click();
  assert.notEqual(await svg.getAttribute('viewBox'),originalView);
  const zoomedView=await svg.getAttribute('viewBox');
  await svg.scrollIntoViewIfNeeded();
  const area=await svg.boundingBox();
  await page.mouse.move(area.x+area.width/2,area.y+area.height/2);
  await page.mouse.down();await page.mouse.move(area.x+area.width/2+40,area.y+area.height/2+30,{steps:5});await page.mouse.up();
  assert.notEqual(await svg.getAttribute('viewBox'),zoomedView,'Dragging moves viewport');
  const pannedView=await svg.getAttribute('viewBox');
  await panel.getByRole('button',{name:'Biểu đồ tròn',exact:true}).click();
  assert.equal(await map.isVisible(),true);
  assert.equal(await svg.getAttribute('viewBox'),pannedView,'Chart switch preserves zoom and pan');
  await map.getByRole('button',{name:'Đặt lại bản đồ',exact:true}).click();
  assert.equal(await svg.getAttribute('viewBox'),originalView);
  await svg.focus();await page.keyboard.press('ArrowRight');
  assert.notEqual(await svg.getAttribute('viewBox'),originalView);
  await page.keyboard.press('Home');assert.equal(await svg.getAttribute('viewBox'),originalView);
  assert.equal(await search.inputValue(),'An Giang');
  await panel.getByRole('button',{name:'Biểu đồ cột',exact:true}).click();
  await panel.screenshot({path:'tests/province-layout-desktop.png'});
  await root.getByRole('button',{name:'📋 Nhiệm vụ',exact:true}).click();
  await root.getByText('Bản đồ hoàn thành nhiệm vụ',{exact:true}).waitFor();
  await root.getByText('Thống kê nhiệm vụ',{exact:true}).waitFor();
  assert.equal(await root.getByTestId('admin-overview-left').locator('[style*="conic-gradient"]').count(),2);
  await root.getByRole('button',{name:'🎯 Mục tiêu',exact:true}).click();
  await root.getByText('Bản đồ hoàn thành mục tiêu',{exact:true}).waitFor();
  await page.setViewportSize({width:390,height:844});
  const mobileCanvas = await map.getByTestId('province-map-canvas').boundingBox();
  const mobileDirectory = await map.getByTestId('province-map-directory').boundingBox();
  assert.ok(mobileDirectory.y >= mobileCanvas.y + mobileCanvas.height - 2,'Mobile directory stacks under map');
  await page.evaluate(() => window.scrollTo(0,0));
  await page.screenshot({path:'tests/admin-overview-mobile.png'});
  assert.equal(await map.isVisible(),true);
  assert.ok((await map.boundingBox()).y < (await chart.boundingBox()).y);
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth > innerWidth),false);
  await panel.screenshot({path:'tests/province-layout-mobile.png'});
  assert.deepEqual(errors,[]);
  console.log('Passed two stacked donut charts on left, no four cards, map/directory on right, zoom/pan/filter and mobile layout. API mocked.');
} finally { await browser.close(); }
