import assert from 'node:assert/strict';
import XLSX from 'xlsx-js-style';
import {buildGoalsWorksheet,buildTasksWorksheet,parseUnifiedProgressReportFile} from '../src/utils/unifiedProgressReportExcel.js';
import {reconcileProgressReportImport} from '../src/utils/progressReportImportDiff.js';
const goals=[{id:'g2629',code:'MT-01',documentId:'d2629',documentNumber:'2629/QĐ-TTg',title:'Mục tiêu 2629',isCoordinatingAssignment:true,applicableYears:[2025,2026],actual2025:10,estimate2026:45,notes:'Phối hợp 2629',targetPhase1:100},{id:'g1266',code:'MT-01',documentId:'d1266',documentNumber:'1266/QĐ-TTg',title:'Mục tiêu 1266',isCoordinatingAssignment:true,applicableYears:[2026],estimate2026:55,notes:'Phối hợp 1266',targetPhase1:100}];
const tasks=[{id:'t',code:'NV-01',documentId:'d2629',documentNumber:'2629/QĐ-TTg',title:'Nhiệm vụ phối hợp',isCoordinatingAssignment:true,status:'InProgress',statusLabel:'Đang thực hiện',notes:'Ghi chú phối hợp'}];
const gw=buildGoalsWorksheet(goals,'Phối hợp','15/11/2026',XLSX);const tw=buildTasksWorksheet(tasks,'Phối hợp','15/11/2026',XLSX).ws;
const wb=XLSX.utils.book_new();XLSX.utils.book_append_sheet(wb,gw,'Mục tiêu');XLSX.utils.book_append_sheet(wb,tw,'Nhiệm vụ');
const rowOf=(sheet,needle)=>XLSX.utils.sheet_to_json(sheet,{header:1}).findIndex(r=>String(r[2]||'').startsWith(needle))+1;
const r1=rowOf(gw,'Mục tiêu 2629'),r2=rowOf(gw,'Mục tiêu 1266'),rt=rowOf(tw,'Nhiệm vụ phối hợp');
for(const r of [r1,r2]){assert(gw[`C${r}`].v.includes('Vai trò: Phối hợp'));for(const col of ['I','J','K','L']){assert(!gw['!reportEditableCells'].includes(`${col}${r}`));assert.equal(gw[`${col}${r}`].v,'');}}
assert(!gw['!reportEditableCells'].includes(`G${r2}`));assert(tw[`C${rt}`].v.includes('Vai trò: Phối hợp'));
const parse=async()=>{const bytes=XLSX.write(wb,{type:'buffer',bookType:'xlsx'});return parseUnifiedProgressReportFile({name:'coord.xlsx',arrayBuffer:async()=>bytes},{existingGoals:goals,existingTasks:tasks});};
let parsed=await parse();let diff=reconcileProgressReportImport(parsed,goals,tasks);assert.equal(diff.goals.length+diff.plans.length+diff.tasks.length,0);
gw[`G${r1}`]={t:'n',v:15};gw[`H${r1}`]={t:'n',v:50};gw[`H${r2}`]={t:'n',v:65};gw[`M${r1}`]={t:'s',v:'Ghi chú mới 2629'};tw[`F${rt}`]={t:'s',v:'Đã hoàn thành'};tw[`G${rt}`]={t:'s',v:'Đã phối hợp xong'};
// Even if a caller bypasses worksheet protection, cooperation cannot change shared plans.
gw[`I${r1}`]={t:'n',v:999};parsed=await parse();diff=reconcileProgressReportImport(parsed,goals,tasks);
assert.equal(diff.goals.length,3);assert.equal(diff.tasks.length,1);assert.equal(diff.plans.length,0);
assert.equal(diff.goals.find(g=>g.id==='g2629'&&g.periodYear===2025).value,15);assert.equal(diff.goals.find(g=>g.id==='g2629'&&g.periodYear===2026).value,50);assert.equal(diff.goals.find(g=>g.id==='g1266').value,65);
assert.equal(diff.tasks[0].id,'t');assert.equal(diff.tasks[0].status,'Completed');assert.equal(diff.tasks[0].summaryNotes,'Đã phối hợp xong');
console.log('PASS coordination Excel export/import: role labels, locked plans, unchanged round trip, separate decisions/years, status and notes');
