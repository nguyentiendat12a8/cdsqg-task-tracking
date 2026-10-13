import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage();
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  const result = await page.evaluate(async () => {
    const { createApp, ref, authState } = await import('/tests/refactor-fixture.js');
    const { useDashboardMetrics } = await import('/src/features/dashboard/useDashboardMetrics.js');
    const { useLegalDashboard } = await import('/src/features/dashboard/useLegalDashboard.js');
    const originalFetch = window.fetch;
    let aborted = 0;
    window.fetch = (url, options = {}) => {
      if (!String(url).includes('/dashboard/metrics') && !String(url).includes('/legaldocuments/dashboard-stats')) return originalFetch(url, options);
      const query = new URL(url, location.href).searchParams;
      const marker = query.get('reportYear') || query.get('documentType');
      return new Promise((resolve, reject) => {
        const cancel = () => { clearTimeout(timer); aborted++; reject(new DOMException('Aborted', 'AbortError')); };
        const timer = setTimeout(() => {
          options.signal?.removeEventListener('abort', cancel);
          resolve(new Response(JSON.stringify({ totalGoals: Number(marker), totalCount: marker, ministriesPerformance: [], provincesPerformance: [], othersPerformance: [] }), { status:200 }));
        }, marker === '2026' || marker === 'Luật' ? 250 : 10);
        options.signal?.addEventListener('abort', cancel, { once:true });
      });
    };
    authState.user.value = { role:1 }; authState.token.value = '';
    const root = document.createElement('div'); document.body.appendChild(root);
    let metrics, legal;
    const year = ref(2026), agencies = ref([]);
    const app = createApp({ setup() {
      metrics = useDashboardMetrics({ reportYear:year, agencies, dashboardFilter:ref('goals'), selectedAgencyIds:ref([]), selectedSubAgencyIds:ref([]), selectedScopes:ref([]), selectedSections:ref([]), selectedGroups:ref([]), fromYear:ref(null), toYear:ref(null), isOngoingOnly:ref(false), isSpecialAgencyCode:()=>false });
      legal = useLegalDashboard({ agencies, isSpecialAgencyCode:()=>false });
      return () => null;
    } });
    app.mount(root);
    const oldMetrics = metrics.loadDashboardMetrics();
    legal.legalFilterDocumentType.value = 'Luật';
    const oldLegal = legal.loadLegalDashboardStats();
    year.value = 2027;
    legal.legalFilterDocumentType.value = 'Thông tư';
    await Promise.all([oldMetrics, oldLegal, metrics.loadDashboardMetrics(), legal.loadLegalDashboardStats()]);
    const values = { year:metrics.metrics.value.totalGoals, type:legal.legalStats.value.totalCount, error:metrics.loadError.value, loading:metrics.isLoading.value || legal.isLegalStatsLoading.value };
    year.value = 2026; legal.legalFilterDocumentType.value = 'Luật';
    const pending = [metrics.loadDashboardMetrics(), legal.loadLegalDashboardStats()];
    app.unmount(); await Promise.all(pending);
    window.fetch = originalFetch;
    return { ...values, aborted };
  });
  assert.equal(result.year, 2027);
  assert.equal(result.type, 'Thông tư');
  assert.equal(result.error, '');
  assert.equal(result.loading, false);
  assert.ok(result.aborted >= 6, 'Superseded and unmounted requests are cancelled');
  console.log('Passed cancellation, latest year/filter result, loading state and unmount cleanup. Fetch mocked.');
} finally { await browser.close(); }
