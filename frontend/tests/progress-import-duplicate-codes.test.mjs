import assert from 'node:assert/strict';
import XLSX from 'xlsx-js-style';
import {buildTasksWorksheet,parseUnifiedProgressReportFile} from '../src/utils/unifiedProgressReportExcel.js';
import {reconcileProgressReportImport} from '../src/utils/progressReportImportDiff.js';
const tasks=[{id:'a',documentId:'doc-a',documentNumber:'2629/QĐ-TTg',code:'NV-03',title:'Nhiệm vụ 2629',status:'Completed',statusLabel:'Đã hoàn thành',notes:'Ghi chú A'}, {id:'b',documentId:'doc-b',documentNumber:'1266/QĐ-TTg',code:'NV-03',title:'Nhiệm vụ 1266',status:'InProgress',statusLabel:'Đang thực hiện',notes:'Ghi chú B'}];
const ws=buildTasksWorksheet(tasks,'Bộ Công an','11/11/2026',XLSX).ws;
const parse=async sheet=>{const wb=XLSX.utils.book_new();XLSX.utils.book_append_sheet(wb,sheet,'Nhiệm vụ');const bytes=XLSX.write(wb,{type:'buffer',bookType:'xlsx'});return parseUnifiedProgressReportFile({name:'report.xlsx',arrayBuffer:async()=>bytes},{existingGoals:[],existingTasks:tasks});};
let parsed=await parse(ws);assert.deepEqual(parsed.tasks.map(t=>t.id),['a','b']);assert.equal(reconcileProgressReportImport(parsed,[],tasks).tasks.length,0);
// Older files without hidden IDs still match by the decision section.
const rows=XLSX.utils.sheet_to_json(ws,{header:1});const legacy=XLSX.utils.aoa_to_sheet(rows.map(r=>r.slice(0,8)));
parsed=await parse(legacy);assert.deepEqual(parsed.tasks.map(t=>t.id),['a','b']);assert.equal(reconcileProgressReportImport(parsed,[],tasks).tasks.length,0);
const bRow=rows.findIndex(r=>r[8]==='b');ws[`G${bRow+1}`]={t:'s',v:'Đã hoàn thành'};
parsed=await parse(ws);const changed=reconcileProgressReportImport(parsed,[],tasks);assert.equal(changed.tasks.length,1);assert.equal(changed.tasks[0].id,'b');assert(changed.preview.tasks[0].cells.status.changed);
console.log('PASS unchanged duplicate task codes across two decisions produce zero changes; only edited decision is highlighted');
