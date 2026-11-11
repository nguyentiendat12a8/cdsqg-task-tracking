import assert from 'node:assert/strict';
import {
  getAgencyTypeRank,
  compareAgenciesByStandardOrder,
  sortAgenciesByStandardOrder,
  isSpecialAgency,
  isMinistryAgency,
  isProvinceAgency
} from '../src/shared/agencyPresentation.js';

console.log('Testing agency sorting standard order...');

// 1. Rank categorization tests
const special1 = { id: '1', code: 'ALL_AGENCIES', name: 'Tất cả cơ quan', type: 5 };
const special2 = { id: '2', code: 'ALL_MINISTRIES', name: 'Khối các bộ ngành', type: 'Special' };
const special3 = { id: '3', code: 'ALL_PROVINCES', name: 'Khối các địa phương', type: 5 };

const ministry1 = { id: '4', code: 'BCA', name: 'Bộ Công an', type: 1 };
const ministry2 = { id: '5', code: 'BCY', name: 'Ban Cơ yếu Chính phủ', type: 'Ministry' };
const ministry3 = { id: '6', code: 'BCT', name: 'Bộ Công Thương', type: 1 };
const ministry4 = { id: '7', code: 'BKHCN', name: 'Bộ Khoa học và Công nghệ', type: 1 };

const province1 = { id: '8', code: 'AG', name: 'An Giang', type: 2 };
const province2 = { id: '9', code: 'HN', name: 'Thành phố Hà Nội', type: 'Province' };
const province3 = { id: '10', code: 'BN', name: 'Thành phố Bắc Ninh', type: 2 };

const subUnit1 = { id: '11', code: 'CDS', name: 'Cục Chuyển đổi số quốc gia', parentId: ministry4.id, type: 3 };

assert.equal(getAgencyTypeRank(special1), 1, 'Special agency must be rank 1');
assert.equal(getAgencyTypeRank(special2), 1, 'Special agency must be rank 1');
assert.equal(getAgencyTypeRank(ministry1), 2, 'Ministry agency must be rank 2');
assert.equal(getAgencyTypeRank(ministry2), 2, 'Ban Co yeu must be rank 2');
assert.equal(getAgencyTypeRank(province1), 3, 'Province agency must be rank 3');
assert.equal(getAgencyTypeRank(province2), 3, 'Province agency must be rank 3');
assert.equal(getAgencyTypeRank(subUnit1), 4, 'Sub-agency must be rank 4');

// 2. Sorting mixed list
const mixed = [province1, ministry1, special1, subUnit1, ministry3, province3, ministry2, special2, ministry4, province2];
const sorted = sortAgenciesByStandardOrder(mixed);
const sortedNames = sorted.map(a => a.name);

// Verify hierarchy:
// Rank 1: Khối các bộ ngành, Tất cả cơ quan
// Rank 2: Ban Cơ yếu Chính phủ, Bộ Công an, Bộ Công Thương, Bộ Khoa học và Công nghệ
// Rank 3: An Giang, Thành phố Bắc Ninh, Thành phố Hà Nội
// Rank 4: Cục Chuyển đổi số quốc gia
assert.deepEqual(sortedNames, [
  'Khối các bộ ngành',
  'Tất cả cơ quan',
  'Ban Cơ yếu Chính phủ',
  'Bộ Công an',
  'Bộ Công Thương',
  'Bộ Khoa học và Công nghệ',
  'An Giang',
  'Thành phố Bắc Ninh',
  'Thành phố Hà Nội',
  'Cục Chuyển đổi số quốc gia'
]);

// 3. Verify dropdown without special (excludeSpecial = true)
const withoutSpecial = [province1, ministry1, subUnit1, ministry3, province3, ministry2, ministry4, province2];
const sortedWithoutSpecial = sortAgenciesByStandardOrder(withoutSpecial);
const namesWithoutSpecial = sortedWithoutSpecial.map(a => a.name);

assert.deepEqual(namesWithoutSpecial, [
  'Ban Cơ yếu Chính phủ',
  'Bộ Công an',
  'Bộ Công Thương',
  'Bộ Khoa học và Công nghệ',
  'An Giang',
  'Thành phố Bắc Ninh',
  'Thành phố Hà Nội',
  'Cục Chuyển đổi số quốc gia'
]);

// Notice: In the user's screenshot, An Giang was appearing BEFORE Bộ Công an.
// In the sorted list, Bộ Công an (Ministry) is at index 1 and An Giang (Province) is at index 4!
assert.ok(namesWithoutSpecial.indexOf('Bộ Công an') < namesWithoutSpecial.indexOf('An Giang'),
  'Bộ Công an (Bộ ngành) must appear before An Giang (Địa phương)');

console.log('Passed all agency standard order sorting tests!');
