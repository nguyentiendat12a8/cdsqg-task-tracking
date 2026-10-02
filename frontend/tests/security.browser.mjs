import { readFile } from 'node:fs/promises';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';

// Use an existing Playwright installation; no browser/package downloads.
const require = createRequire(process.env.PLAYWRIGHT_PACKAGE || import.meta.url);
const { chromium } = require('playwright');
const browser = await chromium.launch({ channel: process.env.BROWSER_CHANNEL || 'msedge', headless: true });
try {
  const page = await browser.newPage();
  await page.route('http://security.test/**', route => route.fulfill({ contentType: 'text/html', body: '<html><body></body></html>' }));
  await page.goto('http://security.test/');
  const source = await readFile(new URL('../src/utils/sanitizeHtml.js', import.meta.url), 'utf8');
  await page.addScriptTag({ content: source.replace('export function', 'function') + '\nwindow.sanitizeHtml = sanitizeHtml;' });
  const cases = [
    '<p onclick="window.pwned=1">Hello <strong>world</strong></p>',
    '<img src=x onerror="window.pwned=1"><script>window.pwned=1</script><p>Safe</p>',
    '<a href="javascript:window.pwned=1">Link</a><a href="https://example.com">Safe link</a>',
    '<svg><a href="javascript:alert(1)">SVG</a></svg><iframe srcdoc="<script>alert(1)</script>"></iframe>',
    '<math><mtext><table><mglyph><style><!--</style><img title="--><img src=x onerror=window.pwned=1>">',
    '<div style="background:url(javascript:alert(1))"><b>Formatted</b></div>'
  ];
  for (const input of cases) {
    const result = await page.evaluate(input => {
      const html = window.sanitizeHtml(input);
      document.body.innerHTML = html;
      const unsafe = [...document.body.querySelectorAll('*')].some(el =>
        [...el.attributes].some(attr => /^on/i.test(attr.name) || attr.name === 'style')
        || ['SCRIPT','IMG','SVG','MATH','IFRAME','OBJECT','EMBED','STYLE'].includes(el.tagName)
        || (el.tagName === 'A' && el.getAttribute('href')?.startsWith('javascript:')));
      return { html, unsafe, pwned: window.pwned || 0 };
    }, input);
    assert.equal(result.unsafe, false);
    assert.equal(result.pwned, 0);
  }
  assert.equal(await page.evaluate(() => sanitizeHtml('<p>Hello <strong>world</strong></p>')), '<p>Hello <strong>world</strong></p>');

  const auth = await readFile(new URL('../src/services/auth.js', import.meta.url), 'utf8');
  await page.evaluate(() => { localStorage.setItem('cdsqg_auth_token', 'test-token'); });
  await page.addScriptTag({ content: `const ref = value => ({value}); const computed = fn => ({get value(){return fn();}});
    const getApiUrl = () => 'http://api.test';\n` + auth.replace(/^import .*;\r?\n/gm, '').replace(/export /g, '')
    + '\nwindow.apiFetch = fetchWithAuth; window.authToken = token;' });
  const headers = await page.evaluate(async () => {
    window.fetch = async (_url, options) => ({status: 200, authorization: options.headers.get('Authorization')});
    return {
      api: (await apiFetch('http://api.test/api/dashboard/metrics')).authorization,
      external: (await apiFetch('https://outside.test/api/data')).authorization,
      static: (await apiFetch('http://api.test/uploads/file.pdf')).authorization
    };
  });
  assert.equal(headers.api, 'Bearer test-token');
  assert.equal(headers.external, null);
  assert.equal(headers.static, null);
  await page.evaluate(async () => { window.fetch = async () => ({status: 401}); await apiFetch('http://api.test/api/dashboard/metrics'); });
  assert.equal(await page.evaluate(() => authToken.value), '');
  console.log('Passed: 6 malicious HTML cases, rich text preservation, API authentication, no external token leak, 401 logout.');
} finally {
  await browser.close();
}
