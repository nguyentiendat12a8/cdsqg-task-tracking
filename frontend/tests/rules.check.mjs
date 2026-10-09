import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';
const root = fileURLToPath(new URL('../src/', import.meta.url));
async function files(directory) {
  const entries = await fs.readdir(directory, { withFileTypes: true });
  return (await Promise.all(entries.map(e => e.isDirectory() ? files(path.join(directory, e.name)) : path.join(directory, e.name)))).flat();
}
for (const file of await files(root)) {
  if (!/\.(vue|js)$/.test(file)) continue;
  const source = await fs.readFile(file, 'utf8');
  // Calendar month/year pickers are internal native controls with their own focus model.
  if (file.endsWith('.vue') && !file.endsWith(`${path.sep}DatePicker.vue`)) {
    assert.ok(!/<select\b/.test(source), `Use SearchableSelect for business dropdowns: ${file}`);
  }
  if (!file.includes(`${path.sep}shared${path.sep}`)) {
    assert.ok(!/^\s*(?:export )?function (?:formatDate|formatDateTime|formatFileName|getStatusLabel)\(/m.test(source), `Use shared formatter/status module: ${file}`);
  }
  if (file.includes(`${path.sep}views${path.sep}`)) {
    assert.ok(!/import\s+.*?from\s+['"]xlsx(?:-js-style)?(?:\/[^'"]*)?['"]/.test(source), `Load Excel through the adapter: ${file}`);
  }
}
console.log('Passed: no duplicated canonical format/status functions; views do not eagerly import Excel.');
