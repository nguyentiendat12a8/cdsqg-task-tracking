import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const componentPath = fileURLToPath(new URL('../src/components/SearchInput.vue', import.meta.url));

test('SearchInput.vue exists and has required API', async () => {
  const code = await fs.readFile(componentPath, 'utf8');
  assert.ok(code.includes('modelValue'), 'Must define modelValue prop');
  assert.ok(code.includes('debounceMs'), 'Must define debounceMs prop');
  assert.ok(code.includes('emit(\'update:modelValue\''), 'Must emit update:modelValue');
  assert.ok(code.includes('emit(\'search\''), 'Must emit search');
  assert.ok(code.includes('emit(\'clear\''), 'Must emit clear');
  assert.ok(code.includes('ui-single-line'), 'Must use standard ui-single-line height class');
  assert.ok(code.includes('focus:ring-2 focus:ring-blue-500'), 'Must have consistent blue focus ring');
  assert.ok(!code.includes('text-[11px]'), 'Must not use forbidden arbitrary typography');
});
