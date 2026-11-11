import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import XLSX from 'xlsx-js-style';
import {
  buildGoalsWorksheet,
  buildTasksWorksheet,
  parseUnifiedProgressReportFile,
  parseVNNumber
} from '../src/utils/unifiedProgressReportExcel.js';

console.log('=== TEST SUITE: BÁO CÁO TIẾN ĐỘ EXCEL EXPORT & IMPORT ===');

// 0. TEST VIETNAMESE NUMBER PARSER
console.log('0. Testing parseVNNumber utility...');
assert.equal(parseVNNumber(null), null);
assert.equal(parseVNNumber(''), null);
assert.equal(parseVNNumber('—'), null);
assert.equal(parseVNNumber(100), 100);
assert.equal(parseVNNumber('100'), 100);
assert.equal(parseVNNumber('10.000.000'), 10000000);
assert.equal(parseVNNumber('7.000.000'), 7000000);
assert.equal(parseVNNumber('10,000,000'), 10000000);
assert.equal(parseVNNumber('41.2%'), 41.2);
assert.equal(parseVNNumber('41,2%'), 41.2);
assert.equal(parseVNNumber('85%'), 85);
assert.equal(parseVNNumber('1.234,56'), 1234.56);
console.log('   parseVNNumber passed all cases (thousands dots, commas, percentages, decimals).');

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
      sequenceNumber: 1,
      code: 'MT-08',
      title: 'Tỉ lệ bộ, ngành, địa phương đạt cấp độ 3 mức độ trưởng thành về quản trị dữ liệu.',
      unitName: '%',
      targetPhase1: '100',
      targetPhase2: '',
      actual2025: 50,
      estimate2026: null,
      plan2027: 100,
      plan2028: '—',
      plan2029: '—',
      plan2030: '—',
      notes: 'Ghi chú ban đầu',
      applicableYears: [2025, 2026, 2027]
    },
    {
      id: '26290000-0000-0000-0000-000000000024',
      documentId: '26290000-0000-0000-0000-000000002629',
      documentNumber: '2629/QĐ-TTg',
      sequenceNumber: 2,
      code: 'MT-24',
      title: 'Đào tạo tập huấn kỹ năng số cơ bản cho người lao động',
      unitName: 'Số lượng',
      targetPhase1: '10000000',
      targetPhase2: '',
      actual2025: null,
      estimate2026: null,
      plan2027: 7000000,
      plan2028: 8000000,
      plan2029: 9000000,
      plan2030: 10000000,
      notes: null,
      applicableYears: [2026, 2027, 2028, 2029, 2030]
    },
    {
      id: '12660000-0000-0000-0000-000000000004',
      documentId: '12660000-0000-0000-0000-000000001266',
      documentNumber: '1266/QĐ-TTg',
      sequenceNumber: 1,
      code: 'MT-04',
      title: 'Tỉ lệ cung cấp dữ liệu số phục vụ chỉ đạo điều hành',
      unitName: '%',
      targetPhase1: '80',
      targetPhase2: '',
      actual2025: null,
      estimate2026: null,
      plan2027: '',
      plan2028: '',
      plan2029: '',
      plan2030: 80,
      notes: null,
      applicableYears: [2026, 2027, 2028, 2029, 2030]
    }
  ],
  tasks: [
    {
      id: '26290000-0000-0000-0000-000000000101',
      documentId: '26290000-0000-0000-0000-000000002629',
      documentNumber: '2629/QĐ-TTg',
      sequenceNumber: 1,
      code: 'NV-01',
      title: 'Xây dựng Trung tâm dữ liệu quốc gia',
      coordinatingAgencies: 'Bộ TT&TT, Bộ Quốc phòng',
      dueDate: '31/12/2026',
      status: 'NotStarted',
      statusLabel: 'Chưa thực hiện',
      deliverables: 'Hệ thống CSDL hoàn thành',
      notes: null
    }
  ]
};

// 1. TEST EXCEL BUILDER FUNCTIONS
console.log('1. Testing buildGoalsWorksheet & buildTasksWorksheet generation...');
const wsGoals = buildGoalsWorksheet(mockReportData.goals, mockReportData.agencyName, '10/11/2026', XLSX);
const { ws: wsTasks, statusCells } = buildTasksWorksheet(mockReportData.tasks, mockReportData.agencyName, '10/11/2026', XLSX);

// Verify Sheet 1 (Goals) structure
assert.equal(wsGoals['A4'].v, 'STT');
assert.equal(wsGoals['B4'].v, 'Mã MT');
assert.equal(wsGoals['C4'].v, 'Tên mục tiêu / Chỉ tiêu');
assert.equal(wsGoals['D4'].v, 'ĐVT', 'Must be ĐVT instead of Đơn vị tính');
assert.equal(wsGoals['E4'].v, 'Kết quả theo QĐ');
assert.equal(wsGoals['G4'].v, 'Kết quả thực hiện');
assert.equal(wsGoals['I4'].v, 'Mục tiêu theo từng năm (đến 2030)', 'Must match web view header');
assert.equal(wsGoals['E5'].v, 'Giai đoạn 1');
assert.equal(wsGoals['F5'].v, 'Giai đoạn 2');
assert.equal(wsGoals['G5'].v, 'Kết quả 2025');
assert.equal(wsGoals['H5'].v, 'Ước tính 2026');
assert.equal(wsGoals['I5'].v, '2027', 'No Kế hoạch prefix');
assert.equal(wsGoals['L5'].v, '2030');

// Verify Section I (Row 6, index 5)
assert.equal(wsGoals['A6'].v, 'I');
assert.ok(wsGoals['B6'].v.includes('QUYẾT ĐỊNH SỐ 2629/QĐ-TTg'));
assert.equal(wsGoals['E6'].v, '2025 - 2027');
assert.equal(wsGoals['F6'].v, '2028 - 2030');
assert.equal(wsGoals['G6'].v, '', 'Section cols G-M must be empty');

// Verify numeric formatting
assert.equal(wsGoals['E7'].t, 'n', 'Target 100 should be numeric');
assert.equal(wsGoals['E7'].v, 100);
assert.equal(wsGoals['E7'].z, '#,##0');

assert.equal(wsGoals['E8'].t, 'n', 'Target 10000000 should be numeric');
assert.equal(wsGoals['E8'].v, 10000000);
assert.equal(wsGoals['E8'].z, '#,##0');

console.log('   Sheet 1 (Mục tiêu) structure & formatting verified 100%!');

// Verify Sheet 2 (Tasks) structure
assert.equal(wsTasks['A4'].v, 'STT');
assert.equal(wsTasks['B4'].v, 'Mã NV', 'Must NOT have Căn cứ QĐ column!');
assert.equal(wsTasks['C4'].v, 'Tên nhiệm vụ');
assert.equal(wsTasks['D4'].v, 'Hạn hoàn thành');
assert.equal(wsTasks['F4'].v, 'TRẠNG THÁI THỰC HIỆN');
assert.equal(wsTasks['E4'].v, 'Sản phẩm bàn giao');
assert.equal(wsTasks['G4'].v, 'GHI CHÚ / KHÓ KHĂN & KIẾN NGHỊ');
assert.equal(wsTasks['H4'].v, 'ID Nhiệm Vụ (Ẩn)');
assert.equal(wsTasks['I4'].v, 'ID Quyết Định (Ẩn)');

// Verify Task Section I
assert.equal(wsTasks['A5'].v, 'I');
assert.ok(wsTasks['B5'].v.includes('QUYẾT ĐỊNH SỐ 2629/QĐ-TTg'));

// Verify Task Data row (Row 6)
assert.equal(wsTasks['A6'].v, 1);
assert.equal(wsTasks['B6'].v, 'NV-01');
assert.equal(wsTasks['F6'].v, 'Chưa thực hiện');

// Verify validation dropdown coordinate
assert.ok(statusCells.includes('F6'), 'Status dropdown validation must target column F!');

console.log('   Sheet 2 (Nhiệm vụ) structure verified with 7 visible columns!');

// 2. TEST PARSING THE GENERATED EXCEL FILE AFTER USER ENTERS DATA
console.log('2. Simulating user filling data into exported workbook & parsing...');
const testWb = XLSX.utils.book_new();
XLSX.utils.book_append_sheet(testWb, wsGoals, 'Mục tiêu');
XLSX.utils.book_append_sheet(testWb, wsTasks, 'Nhiệm vụ');

// User fills in:
// - MT-08 (row 7): H7 (Ước tính 2026) = '85%', M7 = 'Tiến độ rất tốt'
// - MT-24 (row 8): H8 = '7.000.000' (with dots!), I8 = '7.500.000', M8 = 'Kế hoạch đã ban hành'
// - MT-04 (row 10): H10 = '75%'
// - NV-01 (row 6 on sheet 2): F6 = 'Đang thực hiện', H6 = 'Đang hoàn thiện thiết kế'
wsGoals['H7'] = { t: 's', v: '85%' };
wsGoals['M7'] = { t: 's', v: 'Tiến độ rất tốt' };

wsGoals['H8'] = { t: 's', v: '7.000.000' };
wsGoals['I8'] = { t: 's', v: '7.500.000' };
wsGoals['M8'] = { t: 's', v: 'Kế hoạch đã ban hành' };

wsGoals['H10'] = { t: 's', v: '75%' };

wsTasks['F6'] = { t: 's', v: 'Đang thực hiện' };
wsTasks['G6'] = { t: 's', v: 'Đang hoàn thiện thiết kế' };

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

assert.equal(parseResult.goals.filter(g=>g.periodYear===2026).length, 3, 'Should parse 3 goals with estimate 2026');

const pMt08 = parseResult.goals.find(g => g.periodYear === 2026 && g.code === 'MT-08');
assert.ok(pMt08);
assert.equal(pMt08.value, 85);
assert.equal(pMt08.summaryNotes, 'Tiến độ rất tốt');

const pMt24 = parseResult.goals.find(g => g.periodYear === 2026 && g.code === 'MT-24');
assert.ok(pMt24);
assert.equal(pMt24.value, 7000000, 'Must parse 7.000.000 with thousand dot correctly to 7000000');
assert.equal(pMt24.summaryNotes, 'Kế hoạch đã ban hành');

const pMt04 = parseResult.goals.find(g => g.periodYear === 2026 && g.code === 'MT-04');
assert.ok(pMt04);
assert.equal(pMt04.value, 75);

// Check yearly plan targets
const pMt24Plan2027 = parseResult.plans.find(p => p.code === 'MT-24' && p.year === 2027);
assert.ok(pMt24Plan2027);
assert.equal(pMt24Plan2027.targetQuantity, 7500000, 'Must parse 7.500.000 correctly');

// Check parsed task
assert.equal(parseResult.tasks.length, 1);
const pNv01 = parseResult.tasks[0];
assert.equal(pNv01.code, 'NV-01');
assert.equal(pNv01.status, 'Drafting', 'Đang thực hiện -> Drafting');
assert.equal(pNv01.summaryNotes, 'Đang hoàn thiện thiết kế');

console.log('   Parsed newly exported and edited workbook with 100% precision!');

// 3. TEST USER DRAFT WITH FILLED DATA
console.log('3. Testing User Draft File with filled in data...');
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

assert.equal(userParseResult.goals.filter(g=>g.periodYear===2026).length, 1, 'Should extract MT-08 from user draft');
assert.equal(userParseResult.goals.find(g=>g.periodYear===2026).code, 'MT-08');
assert.equal(userParseResult.goals.find(g=>g.periodYear===2026).value, 88);
assert.equal(userParseResult.goals.find(g=>g.periodYear===2026).summaryNotes, 'Cập nhật từ draft');

assert.equal(userParseResult.plans.length, 1, 'Should extract MT-08 plan 2027 from user draft');
assert.equal(userParseResult.plans[0].year, 2027);
assert.equal(userParseResult.plans[0].targetQuantity, 100);

console.log('   User draft with filled values parsed with 100% precision!');
console.log('\n=== ALL EXCEL EXPORT & IMPORT TESTS PASSED WITH 100% SUCCESS! ===');
