import assert from 'node:assert/strict';
import { itemInvolvesBKHCN, bkhcnSubAgencies, subAgencyOptionsFor, subAgencyLabel } from '../src/shared/subAgencies.js';

const bkhcn = { id: 'B', code: 'BKHCN', name: 'Bộ Khoa học và Công nghệ', parentId: null };
const bca = { id: 'C', code: 'BCA', name: 'Bộ Công an', parentId: null };
const all = { id: 'ALL', code: 'ALL_AGENCIES', name: 'Các bộ, ngành, địa phương', parentId: null };
const cuc = { id: 'B1', name: 'Cục Chuyển đổi số quốc gia', parentId: 'B', type: 3 };
const vien = { id: 'B2', name: 'Viện Công nghệ số', parentId: 'B', type: 3 };
const bcaChild = { id: 'C1', name: 'Cục A05', parentId: 'C', type: 3 };
const unitGroup = { id: 'BG', code: 'ALL_UNITS_BKHCN', name: 'Tất cả cơ quan, đơn vị thuộc Bộ', parentId: 'B', type: 'Special' };
const agencies = [bkhcn, bca, all, cuc, vien, bcaChild, unitGroup];

// Bộ KH&CN chủ trì, đồng chủ trì, hoặc thuộc nhóm nhiệm vụ chung.
assert.equal(itemInvolvesBKHCN({ leadAgencyId: 'B', leadAgencyName: bkhcn.name }, agencies), true);
assert.equal(itemInvolvesBKHCN({ leadAgencyId: 'C', leadAgencyName: bca.name, assignedAgencyId: 'B' }, agencies), true);
assert.equal(itemInvolvesBKHCN({ leadAgencyId: 'ALL', leadAgencyName: all.name, isGeneralTask: true }, agencies), true);
assert.equal(itemInvolvesBKHCN({ leadAgencyId: 'C', leadAgencyName: bca.name }, agencies), false);

// Nhóm "Tất cả cơ quan, đơn vị thuộc Bộ" đứng đầu danh sách chọn.
assert.deepEqual(bkhcnSubAgencies(agencies).map(a => a.id), ['BG', 'B1', 'B2']);
// Bộ Công an chủ trì, Bộ KH&CN đồng chủ trì: chọn được cả đơn vị của hai bộ.
assert.deepEqual(subAgencyOptionsFor(agencies, { leadAgencyId: 'C', assignedAgencyId: 'B' }).map(o => o.value), ['C1', 'BG', 'B1', 'B2']);
assert.deepEqual(subAgencyOptionsFor(agencies, { leadAgencyId: 'C' }).map(o => o.value), ['C1']);
assert.deepEqual(subAgencyOptionsFor(agencies, { leadAgencyId: 'ALL', isGeneralTask: true }).map(o => o.value), ['BG', 'B1', 'B2']);
assert.equal(subAgencyLabel({ subAgencyNames: ['Cục Chuyển đổi số quốc gia', 'Viện Công nghệ số'] }), 'Cục Chuyển đổi số quốc gia, Viện Công nghệ số');
assert.equal(subAgencyLabel({}), '');
console.log('sub-agencies: ok');
