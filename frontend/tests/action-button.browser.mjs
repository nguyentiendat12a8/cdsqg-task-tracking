import { createRequire } from 'node:module';
import assert from 'node:assert/strict';
const { chromium } = createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
  await page.goto(process.env.FRONTEND_TEST_URL || 'http://127.0.0.1:5173');
  await page.evaluate(async () => {
    const { createApp, h } = await import('/tests/refactor-fixture.js');
    const { default: Button } = await import('/src/components/ActionButton.vue');
    document.querySelector('#app').style.display = 'none';
    const root = document.createElement('div'); root.id = 'action-test'; document.body.appendChild(root);
    window.actionClicks = 0;
    createApp({ render: () => h('form', { onSubmit: event => { event.preventDefault(); window.actionClicks++; } }, [
      ...['export', 'import', 'create'].map(kind => h(Button, { kind, onClick: () => window.actionClicks++ }, () => kind)),
      h(Button, { disabled: true }, () => 'disabled'),
      h(Button, { type: 'submit', kind: 'create' }, () => 'submit')
    ]) }).mount(root);
  });
  for (const width of [1440, 390]) {
    await page.setViewportSize({ width, height: 900 });
    for (const name of ['export', 'import', 'create']) {
      const button = page.getByRole('button', { name, exact: true });
      assert.equal(Math.round((await button.boundingBox()).height), 36);
      assert.equal(await button.locator('svg').count(), 1);
      await button.click();
    }
  }
  assert.equal(await page.getByRole('button', { name: 'disabled' }).isDisabled(), true);
  await page.getByRole('button', { name: 'submit', exact: true }).click();
  assert.equal(await page.evaluate(() => window.actionClicks), 7, 'Native events and form submit preserved');
  console.log('Passed shared action button height, icons, disabled, events and form submit on desktop/mobile.');
} finally { await browser.close(); }
