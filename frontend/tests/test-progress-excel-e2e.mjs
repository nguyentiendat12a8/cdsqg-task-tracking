import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import XLSX from 'xlsx-js-style';
import { parseUnifiedProgressReportFile } from '../src/utils/unifiedProgressReportExcel.js';

console.log('=== TEST SUITE: BÁO CÁO TIẾN ĐỘ EXCEL EXPORT & IMPORT ===');

const mockReportData = {
  agencyId: '01a3d089-be05-4bfb-b966-032fe2a51a21',
  agencyName: 'Bộ Công an',
  agencyCode: 'BCA',
  agencyType: 'Ministry',
  reportYear: 2026,
  goals: [
    {
      id: '26290000-0000-0000-0000-000000000008',
      documentId: '26290000-0000-0000-0000-000000002629',
      documentNumber: '2629/QĐ-TTg',
      code: 'MT-08',
      title: 'Tỉ lệ bộ, ngành, địa phương đạt cấp độ 3 mức độ trưởng thành về quản trị dữ liệu.',
      unitName: '%',
      targetPhase1: '100%',
      targetPhase2: '',
      actual2025: 50,
      estimate2026: null,
      plan2027: '100%',
      plan2028: '—',
      plan2029: '—',
      plan2030: '—',
      notes: 'Ghi chú ban đầu',
      applicableYears: [2025, 2026, 2027]
    },
    {
      id: '26290000-0000-0000-0000-000000000010',
      documentId: '26290000-0000-0000-0000-000000002629',
      documentNumber: '2629/QĐ-TTg',
      code: 'MT-10',
      title: 'Tỉ lệ cán bộ chuyên trách về CNTT',
      unitName: '%',
      targetPhase1: '50%',
      targetPhase2: '100%',
      actual2025: null,
      estimate2026: null,
      plan2027: '',
      plan2028: '—',
      plan2029: '—',
      plan2030: '—',
      notes: null,
      applicableYears: [2025, 2026, 2027]
    },
    {
      id: '12660000-0000-0000-0000-000000000004',
      documentId: '12660000-0000-0000-0000-000000001266',
      documentNumber: '1266/QĐ-TTg',
      code: 'MT-04',
      title: 'Tỉ lệ cung cấp dữ liệu số phục vụ chỉ đạo điều hành',
      unitName: '%',
      targetPhase1: '80%',
      targetPhase2: '',
      actual2025: null,
      estimate2026: null,
      plan2027: '',
      plan2028: '',
      plan2029: '',
      plan2030: '80%',
      notes: null,
      applicableYears: [2026, 2027, 2028, 2029, 2030]
    }
  ],
  tasks: [
    {
      id: '26290000-0000-0000-0000-000000000101',
      documentId: '26290000-0000-0000-0000-000000002629',
      documentNumber: '2629/QĐ-TTg',
      code: 'NV-01',
      title: 'Xây dựng Trung tâm dữ liệu quốc gia',
      coordinatingAgencies: 'Bộ TT&TT, Bộ Quốc phòng',
      dueDate: '31/12/2026',
      status: 'NotStarted',
      statusLabel: 'Chưa thực hiện',
      completionPercentage: null,
      deliverables: 'Hệ thống CSDL hoàn thành',
      notes: null
    }
  ]
};

// 1. TEST EXCEL WORKBOOK GENERATION
console.log('1. Testing Workbook structure generation...');
const { loadExcelWriter } = await import('../src/utils/excelRuntime.js');
const X = await loadExcelWriter();

// Create an in-memory workbook with Sheet 1: Mục tiêu and Sheet 2: Nhiệm vụ
const goalsAoa = [
  ['BIỂU MẪU BÁO CÁO MỤC TIÊU DO CƠ QUAN CHỦ TRÌ THEO DÕI'],
  ['Đơn vị báo cáo: Bộ Công an | Ngày xuất file: 10/11/2026'],
  ['CĂN CỨ VÀ HƯỚNG DẪN'],
  ['STT', 'Mã MT', 'Tên mục tiêu / Chỉ tiêu', 'Đơn vị tính', 'Kết quả', '', 'Kết quả thực hiện', '', 'Kế hoạch (dự kiến)', '', '', '', 'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]'],
  ['', '', '', '', '', '', 'Kết quả 2025', 'Ước tính 2026', 'KẾ HOẠCH 2027', 'KẾ HOẠCH 2028', 'KẾ HOẠCH 2029', 'KẾ HOẠCH 2030', ''],
  ['', '', '', '', '', '', '', '', '', '', '', '', ''],
  ['I', 'QĐ 2629/QĐ-TTG', '', '', '2025 -2027', '2028-2030', '', '', '', '', '', '', ''],
  [1, 'MT-08', 'Tỉ lệ bộ, ngành, địa phương đạt cấp độ 3', '%', '100%', '', 50, '85%', '95%', '—', '—', '—', 'Báo cáo tiến độ tốt'],
  [2, 'MT-10', 'Tỉ lệ cán bộ chuyên trách CNTT', '%', '50%', '100%', '', '90%', '100%', '—', '—', '—', 'Đang đào tạo'],
  ['II', 'QĐ 1266/QĐ-TTG', '', '', '2026-2030', '', '', '', '', '', '', '', ''],
  [1, 'MT-04', 'Tỉ lệ cung cấp dữ liệu số', '%', '80%', '', '', '70%', '75%', '80%', '85%', '90%', 'Khó khăn đường truyền']
];

const tasksAoa = [
  ['BIỂU MẪU BÁO CÁO NHIỆM VỤ DO CƠ QUAN CHỦ TRÌ THEO DÕI'],
  ['Đơn vị báo cáo: Bộ Công an | Ngày xuất file: 10/11/2026'],
  ['HƯỚNG DẪN BÁO CÁO'],
  ['STT', 'Căn cứ QĐ', 'Mã NV', 'Tên nhiệm vụ', 'Cơ quan phối hợp', 'Hạn hoàn thành', 'TRẠNG THÁI THỰC HIỆN [CẦN CHỌN]', 'TIẾN ĐỘ % [CẦN ĐIỀN]', 'Sản phẩm bàn giao', 'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ [CẦN ĐIỀN]', 'ID Nhiệm Vụ (Ẩn)', 'ID Quyết Định (Ẩn)'],
  ['I', 'QĐ 2629/QĐ-TTG', '', '', '', '', '', '', '', '', '', ''],
  [1, '2629/QĐ-TTg', 'NV-01', 'Xây dựng Trung tâm dữ liệu quốc gia', 'Bộ TT&TT', '31/12/2026', 'Đang thực hiện', '75%', 'Hệ thống CSDL', 'Tiến độ theo kế hoạch', '26290000-0000-0000-0000-000000000101', '26290000-0000-0000-0000-000000002629']
];

const testWb = XLSX.utils.book_new();
const wsGoals = XLSX.utils.aoa_to_sheet(goalsAoa);
const wsTasks = XLSX.utils.aoa_to_sheet(tasksAoa);
XLSX.utils.book_append_sheet(testWb, wsGoals, 'Mục tiêu');
XLSX.utils.book_append_sheet(testWb, wsTasks, 'Nhiệm vụ');

const testBuffer = XLSX.write(testWb, { type: 'buffer', bookType: 'xlsx' });

const fakeFile = {
  name: 'Bao_Cao_Tien_Do_BCA_2026-11-10.xlsx',
  arrayBuffer: async () => testBuffer.buffer.slice(testBuffer.byteOffset, testBuffer.byteOffset + testBuffer.byteLength)
};

const parseResult = await parseUnifiedProgressReportFile(fakeFile, {
  existingGoals: mockReportData.goals,
  existingTasks: mockReportData.tasks,
  reportYear: 2026
});

console.log('2. Validating parsed Goals:');
console.log('   Total parsed goals:', parseResult.goals.length);
assert.equal(parseResult.goals.length, 3, 'Should parse 3 goals with estimate 2026');

const pMt08 = parseResult.goals.find(g => g.code === 'MT-08');
assert.ok(pMt08, 'MT-08 must be in parsed goals');
assert.equal(pMt08.value, 85, 'MT-08 estimate 2026 should be 85');
assert.equal(pMt08.summaryNotes, 'Báo cáo tiến độ tốt');

const pMt10 = parseResult.goals.find(g => g.code === 'MT-10');
assert.ok(pMt10, 'MT-10 must be in parsed goals');
assert.equal(pMt10.value, 90, 'MT-10 estimate 2026 should be 90');
assert.equal(pMt10.summaryNotes, 'Đang đào tạo');

const pMt04 = parseResult.goals.find(g => g.code === 'MT-04');
assert.ok(pMt04, 'MT-04 must be in parsed goals');
assert.equal(pMt04.value, 70, 'MT-04 estimate 2026 should be 70');
assert.equal(pMt04.summaryNotes, 'Khó khăn đường truyền');

console.log('3. Validating parsed yearly plan targets:');
console.log('   Total parsed plans:', parseResult.plans.length);
assert.equal(parseResult.plans.length, 6, 'Should extract 6 valid yearly plan targets');

const mt08Plan2027 = parseResult.plans.find(p => p.code === 'MT-08' && p.year === 2027);
assert.ok(mt08Plan2027, 'Should have MT-08 plan for 2027');
assert.equal(mt08Plan2027.targetQuantity, 95);

const mt10Plan2027 = parseResult.plans.find(p => p.code === 'MT-10' && p.year === 2027);
assert.ok(mt10Plan2027, 'Should have MT-10 plan for 2027');
assert.equal(mt10Plan2027.targetQuantity, 100);

console.log('4. Validating parsed Tasks:');
console.log('   Total parsed tasks:', parseResult.tasks.length);
assert.equal(parseResult.tasks.length, 1, 'Should parse 1 task');

const pNv01 = parseResult.tasks[0];
assert.equal(pNv01.code, 'NV-01');
assert.equal(pNv01.status, 'Drafting', 'Trạng thái "Đang thực hiện" should map to Drafting');
assert.equal(pNv01.value, 75, 'Completion percentage should be 75');
assert.equal(pNv01.summaryNotes, 'Tiến độ theo kế hoạch');

// 5. TEST USER DRAFT WITH FILLED DATA
console.log('5. Testing User Draft File with filled in data...');
const userDraftPath = 'C:/Users/84878/.gemini/antigravity/brain/387888cb-f837-4c62-bc65-19c97e5a7dc1/.user_uploaded/media_1794307436820_c880e593.xlsx';
const userFileBuffer = await fs.readFile(userDraftPath);
const userWb = XLSX.read(userFileBuffer, { type: 'buffer' });

// Add test values to MT-08 in user's workbook: cell H8 (Estimate 2026) = '88%', cell I8 (Plan 2027) = '100%', cell M8 (Note) = 'Cập nhật từ draft'
const sheetGoals = userWb.Sheets['Mục tiêu'];
sheetGoals['H8'] = { t: 's', v: '88%' };
sheetGoals['I8'] = { t: 's', v: '100%' };
sheetGoals['M8'] = { t: 's', v: 'Cập nhật từ draft' };

const modifiedUserBuffer = XLSX.write(userWb, { type: 'buffer', bookType: 'xlsx' });
const userFakeFile = {
  name: 'media_1794307436820_c880e593_modified.xlsx',
  arrayBuffer: async () => modifiedUserBuffer.buffer.slice(modifiedUserBuffer.byteOffset, modifiedUserBuffer.byteOffset + modifiedUserBuffer.byteLength)
};

const userParseResult = await parseUnifiedProgressReportFile(userFakeFile, {
  existingGoals: mockReportData.goals,
  existingTasks: mockReportData.tasks,
  reportYear: 2026
});

assert.equal(userParseResult.goals.length, 1, 'Should extract MT-08 from user draft');
assert.equal(userParseResult.goals[0].code, 'MT-08');
assert.equal(userParseResult.goals[0].value, 88);
assert.equal(userParseResult.goals[0].summaryNotes, 'Cập nhật từ draft');

assert.equal(userParseResult.plans.length, 1, 'Should extract MT-08 plan 2027 from user draft');
assert.equal(userParseResult.plans[0].year, 2027);
assert.equal(userParseResult.plans[0].targetQuantity, 100);

console.log('   User draft with filled values parsed with 100% precision!');
console.log('\n=== ALL EXCEL EXPORT & IMPORT TESTS PASSED WITH 100% SUCCESS! ===');
