import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const srcDir = fileURLToPath(new URL('../src/', import.meta.url));

test('Unified SearchInput Component Verification', async (t) => {
  const searchInputPath = path.join(srcDir, 'components', 'SearchInput.vue');
  const code = await fs.readFile(searchInputPath, 'utf8');

  await t.test('has proper props, emits and standard single-line height', () => {
    assert.ok(code.includes("default: 500"), 'Default debounce time must be 500ms');
    assert.ok(code.includes("ui-single-line"), 'Must use standard ui-single-line class');
    assert.ok(code.includes("focus:ring-2 focus:ring-blue-500"), 'Must use standard blue focus ring');
    assert.ok(code.includes("emit('search'"), 'Emits search on debounce or enter/clear');
    assert.ok(code.includes("emit('update:modelValue'"), 'Emits update:modelValue for 2-way binding');
    assert.ok(code.includes("handleClear"), 'Has clear handler');
    assert.ok(code.includes("handleEnter"), 'Has Enter key flush handler');
  });

  const screensToCheck = [
    'features/document/DocumentFilters.vue',
    'views/AgencyPlansView.vue',
    'views/ExecutiveReportsView.vue',
    'views/LegalDocumentsView.vue',
    'views/UserManagementView.vue',
    'views/UrgeHistoryView.vue',
    'components/AgencyManagement.vue',
    'components/DynamicPlanningGrid.vue',
    'views/AiGoalKeywordsView.vue',
    'views/DocumentListView.vue',
    'components/ImportHistoryAudit.vue',
    'components/UnitManagement.vue',
    'views/DigitalNewsView.vue',
    'components/ItemDetailModal.vue',
    'features/dashboard/AgencyDrilldownDialog.vue',
    'features/dashboard/AgencyPerformanceBoard.vue',
    'features/dashboard/AgencyWorkQueue.vue',
    'features/dashboard/ProvinceProgressMap.vue',
    'features/dashboard/SummaryStatusDialog.vue',
    'features/digital-news/DigitalPlanSettings.vue'
  ];

  for (const relativePath of screensToCheck) {
    await t.test(`Screen ${relativePath} uses SearchInput`, async () => {
      const fullPath = path.join(srcDir, relativePath);
      const content = await fs.readFile(fullPath, 'utf8');
      assert.ok(content.includes('SearchInput'), `${relativePath} must import and use SearchInput component`);
    });
  }

  await t.test('Views implement switchMap / request versioning pattern against race conditions', async () => {
    const agencyPlans = await fs.readFile(path.join(srcDir, 'views', 'AgencyPlansView.vue'), 'utf8');
    assert.ok(agencyPlans.includes('agencyPlansRequestId'), 'AgencyPlansView must implement request versioning');

    const legalDocs = await fs.readFile(path.join(srcDir, 'views', 'LegalDocumentsView.vue'), 'utf8');
    assert.ok(legalDocs.includes('legalDocRequestId'), 'LegalDocumentsView must implement request versioning');

    const userMgmt = await fs.readFile(path.join(srcDir, 'views', 'UserManagementView.vue'), 'utf8');
    assert.ok(userMgmt.includes('userFetchRequestId'), 'UserManagementView must implement request versioning');

    const urgeHistory = await fs.readFile(path.join(srcDir, 'views', 'UrgeHistoryView.vue'), 'utf8');
    assert.ok(urgeHistory.includes('urgeFetchRequestId'), 'UrgeHistoryView must implement request versioning');

    const aiKeywords = await fs.readFile(path.join(srcDir, 'views', 'AiGoalKeywordsView.vue'), 'utf8');
    assert.ok(aiKeywords.includes('aiKeywordsRequestId'), 'AiGoalKeywordsView must implement request versioning');

    const execReports = await fs.readFile(path.join(srcDir, 'views', 'ExecutiveReportsView.vue'), 'utf8');
    assert.ok(execReports.includes('reportFetchRequestId'), 'ExecutiveReportsView must implement request versioning');
  });
});
