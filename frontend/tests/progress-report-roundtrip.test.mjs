// Xuất biểu mẫu báo cáo tiến độ rồi nhập lại: mã trùng giữa 2 quyết định, dòng nhóm, mục tiêu gộp, dòng phối hợp.
import assert from 'node:assert/strict';
import XLSX from 'xlsx-js-style';
import { buildGoalsWorksheet, buildTasksWorksheet, parseUnifiedProgressReportFile, reportTemplateGoals } from '../src/utils/unifiedProgressReportExcel.js';
import { encodeProgressWorkbook } from '../src/utils/progressWorkbook.js';
import { reconcileProgressReportImport } from '../src/utils/progressReportImportDiff.js';

const D26 = '26290000-0000-0000-0000-000000002629', D12 = '12660000-0000-0000-0000-000000001266';
const goal = (o) => ({ unitName: '%', notes: null, actual2025: null, estimate2026: null, plan2027: '', plan2028: '', plan2029: '', plan2030: '', subCodes: [o.code], subGoalIds: [o.id], ...o });
export const goals = [
  goal({ id: 'g26-1', documentId: D26, documentNumber: '2629/QĐ-TTg', sequenceNumber: 1, code: 'MT-BCA-01', title: 'Cấp độ 3', group: 'Nhóm X', targetPhase1: '100', applicableYears: [2025, 2026, 2027], plan2028: '—', plan2029: '—', plan2030: '—' }),
  goal({ id: 'g26-2', documentId: D26, documentNumber: '2629/QĐ-TTg', sequenceNumber: 2, code: 'MT-BCA-02\nMT-BCA-07', subCodes: ['MT-BCA-02', 'MT-BCA-07'], subGoalIds: ['g26-2', 'g26-7'], title: 'Cán bộ chuyên trách', group: 'Nhóm X', targetPhase1: '50', targetPhase2: '100', applicableYears: [2025, 2026, 2027, 2028, 2029, 2030] }),
  goal({ id: 'g26-3', documentId: D26, documentNumber: '2629/QĐ-TTg', sequenceNumber: 3, code: 'MT-BCA-03', title: 'Dữ liệu mở', group: 'Nhóm Y', targetPhase2: '100', applicableYears: [2028, 2029, 2030], plan2027: '—' }),
  goal({ id: 'g12-c', documentId: D12, documentNumber: '1266/QĐ-TTg', sequenceNumber: 1, code: 'MT-BTP-01', title: 'Hồ sơ số hóa', group: 'Nhóm I', targetPhase1: '100', isCoordinatingAssignment: true, applicableYears: [2026], plan2027: '—', plan2028: '—', plan2029: '—', plan2030: '—' }),
  goal({ id: 'g12-1', documentId: D12, documentNumber: '1266/QĐ-TTg', sequenceNumber: 2, code: 'MT-BCA-01', title: 'Hệ thống kết nối', group: 'Nhóm I', targetPhase1: '100', applicableYears: [2026, 2027, 2028, 2029, 2030] }),
  goal({ id: 'g12-2', documentId: D12, documentNumber: '1266/QĐ-TTg', sequenceNumber: 3, code: 'MT-BCA-02', title: 'CSDL dùng chung', group: 'Nhóm I', targetPhase1: '100', applicableYears: [2026, 2027, 2028, 2029, 2030] }),
  goal({ id: 'g12-3', documentId: D12, documentNumber: '1266/QĐ-TTg', sequenceNumber: 4, code: 'MT-BCA-03', title: 'Nền tảng số', group: null, targetPhase1: '10.000.000', unitName: 'Lượt người', applicableYears: [2026, 2027, 2028, 2029, 2030] })
];
const task = (o) => ({ status: 'NotStarted', statusLabel: 'Chưa thực hiện', dueDate: '31/12/2026', deliverables: 'Báo cáo', notes: null, completionPercentage: 0, ...o });
export const tasks = [
  task({ id: 't26-1', documentId: D26, documentNumber: '2629/QĐ-TTg', sequenceNumber: 1, code: 'NV-BCA-01', title: 'Luật An ninh dữ liệu', group: 'Hoàn thiện thể chế' }),
  task({ id: 't26-2', documentId: D26, documentNumber: '2629/QĐ-TTg', sequenceNumber: 2, code: 'NV-BCA-02', title: 'Bộ chỉ số', group: 'Phát triển dữ liệu số' }),
  task({ id: 't12-1', documentId: D12, documentNumber: '1266/QĐ-TTg', sequenceNumber: 1, code: 'NV-BCA-01', title: 'Luật ANDL (1266)', group: 'Nhóm I: Hoàn thiện thể chế số' }),
  task({ id: 't12-2', documentId: D12, documentNumber: '1266/QĐ-TTg', sequenceNumber: 2, code: 'NV-BCA-02', title: 'Nghị định', group: null, isCoordinatingAssignment: true })
];

function workbook() {
  const wb = XLSX.utils.book_new();
  const g = buildGoalsWorksheet(goals, 'Bộ Công an', '14/11/2026', XLSX);
  const { ws: t, statusCells } = buildTasksWorksheet(tasks, 'Bộ Công an', '14/11/2026', XLSX);
  XLSX.utils.book_append_sheet(wb, g, 'Mục tiêu'); XLSX.utils.book_append_sheet(wb, t, 'Nhiệm vụ');
  return { wb, g, t, statusCells };
}
const findRow = (ws, col, text, nth = 0) => { const range = XLSX.utils.decode_range(ws['!ref']); let k = 0; for (let r = 0; r <= range.e.r; r++) if (String(ws[XLSX.utils.encode_cell({ r, c: col })]?.v ?? '') === text && k++ === nth) return r; throw new Error('Không thấy dòng ' + text); };
const set = (ws, r, c, v) => { ws[XLSX.utils.encode_cell({ r, c })] = { t: typeof v === 'number' ? 'n' : 's', v }; };
async function roundTrip(edit) {
  const { wb, g, t, statusCells } = workbook();
  edit?.(g, t);
  const bytes = encodeProgressWorkbook(wb, [[], statusCells], XLSX);
  const file = { name: 'bao-cao.xlsx', arrayBuffer: async () => bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength) };
  const parsed = await parseUnifiedProgressReportFile(file, { existingGoals: goals, existingTasks: tasks, reportYear: 2026 });
  return reconcileProgressReportImport(parsed, goals, tasks);
}

// 1. File vừa xuất, chưa sửa gì: không có thay đổi, không cảnh báo.
{
  const { wb, g, t } = workbook();
  assert.equal(String(g[XLSX.utils.encode_cell({ r: findRow(g, 0, 'Nhóm X'), c: 1 })]?.v ?? ''), '', 'Dòng nhóm phải để trống cột Mã');
  findRow(g, 0, 'Chưa phân nhóm'); findRow(t, 0, 'Chưa phân nhóm'); findRow(t, 0, 'Hoàn thiện thể chế');
  const res = await roundTrip();
  assert.deepEqual([res.goals.length, res.plans.length, res.tasks.length], [0, 0, 0], 'File chưa sửa không được sinh thay đổi');
  assert.deepEqual(res.warnings, [], 'File chưa sửa không được có cảnh báo: ' + res.warnings.join(' | '));
}

// 2. Mã trùng giữa 2 quyết định: sửa dòng MT-BCA-01 của QĐ 1266 phải cập nhật đúng mục tiêu 1266.
{
  const res = await roundTrip((g) => { set(g, findRow(g, 1, 'MT-BCA-01', 1), 7, 55); set(g, findRow(g, 1, 'MT-BCA-01', 1), 8, 60); });
  assert.deepEqual(res.goals.map(x => [x.id, x.periodYear, x.value]), [['g12-1', 2026, 55]]);
  assert.deepEqual(res.plans.map(x => [x.taskId, x.year, x.targetQuantity, x.documentId]), [['g12-1', 2027, 60, D12]]);
}
// 3. Dòng 1266 MT-BCA-02 không được rơi vào mục tiêu gộp MT-BCA-02/MT-BCA-07 của QĐ 2629.
{
  const res = await roundTrip((g) => set(g, findRow(g, 1, 'MT-BCA-02'), 7, 70));
  assert.deepEqual(res.goals.map(x => [x.id, x.periodYear, x.value]), [['g12-2', 2026, 70]]);
}
// 4. Mục tiêu gộp 2629: 2027 về mã giai đoạn 1, 2028+ về mã giai đoạn 2.
{
  const res = await roundTrip((g) => { const r = findRow(g, 1, 'MT-BCA-02\nMT-BCA-07'); set(g, r, 8, 40); set(g, r, 10, 80); set(g, r, 6, '12,5'); });
  assert.deepEqual(res.plans.map(x => [x.taskId, x.year, x.targetQuantity]), [['g26-2', 2027, 40], ['g26-7', 2029, 80]]);
  assert.deepEqual(res.goals.map(x => [x.id, x.periodYear, x.value]), [['g26-2', 2025, 12.5]]);
}
// 5. Dòng phối hợp: chỉ nhận 2026; ô không áp dụng bị bỏ qua.
{
  const res = await roundTrip((g) => { const r = findRow(g, 1, 'MT-BTP-01'); set(g, r, 7, 90); set(g, r, 8, 95); });
  assert.deepEqual(res.goals.map(x => [x.id, x.periodYear, x.value]), [['g12-c', 2026, 90]]);
  assert.equal(res.plans.length, 0);
}
// 6. Số có dấu chấm hàng nghìn + đơn vị không phải %.
{
  const res = await roundTrip((g) => set(g, findRow(g, 1, 'MT-BCA-03', 1), 7, '1.500.000'));
  assert.deepEqual(res.goals.map(x => [x.id, x.value]), [['g12-3', 1500000]]);
}
// 7. Nhiệm vụ: mã trùng giữa 2 quyết định, trạng thái + ghi chú.
{
  const res = await roundTrip((g, t) => { const r = findRow(t, 1, 'NV-BCA-01', 1); set(t, r, 5, 'Đã hoàn thành'); set(t, r, 6, 'Xong'); set(t, findRow(t, 1, 'NV-BCA-02', 1), 5, 'Đang thực hiện'); });
  assert.deepEqual(res.tasks.map(x => [x.id, x.status, x.summaryNotes]), [['t12-1', 'Completed', 'Xong'], ['t12-2', 'Drafting', null]]);
  assert.deepEqual(res.warnings, []);
}
// 8. Chỉ ghi chú ở mục tiêu (không nhập số): phải báo cho người dùng biết, không lặng lẽ bỏ qua.
{
  const res = await roundTrip((g) => set(g, findRow(g, 1, 'MT-BCA-01', 1), 12, 'Khó khăn về kinh phí'));
  assert.ok(res.goals.length === 1 || res.warnings.some(w => /ghi chú/i.test(w)), 'Ghi chú mục tiêu không kèm số liệu bị bỏ qua mà không cảnh báo');
}
// 9. Mục tiêu EGDI (LHQ đánh giá: STT gốc MT-12, MT-25 của QĐ 1266) không đưa vào biểu mẫu; STT đánh lại liền mạch.
{
  const list = [
    { documentNumber: '1266/QĐ-TTg', originalCode: 'MT-11', code: 'MT-BKHCN-06', sequenceNumber: 1 },
    { documentNumber: '1266/QĐ-TTg', originalCode: 'MT-12', code: 'MT-BKHCN-07', sequenceNumber: 2 },
    { documentNumber: '1266/QĐ-TTg', originalCode: 'MT-13', code: 'MT-BKHCN-08', sequenceNumber: 3 },
    { documentNumber: '1266/QĐ-TTg', originalCode: 'MT-25', code: 'MT-BKHCN-13', sequenceNumber: 4 },
    { documentNumber: '2629/QĐ-TTg', originalCode: 'MT-12', code: 'MT-BKHCN-04', sequenceNumber: 1 }
  ];
  assert.deepEqual(reportTemplateGoals(list).map(g => [g.code, g.sequenceNumber]), [['MT-BKHCN-06', 1], ['MT-BKHCN-08', 2], ['MT-BKHCN-04', 1]]);
}
console.log('progress-report round-trip: OK');
