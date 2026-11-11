import assert from 'node:assert/strict';
import fs from 'node:fs';
import XLSX from 'xlsx-js-style';
import {buildGoalsWorksheet,buildTasksWorksheet} from '../src/utils/unifiedProgressReportExcel.js';
import {encodeProgressWorkbook} from '../src/utils/progressWorkbook.js';
const notes='Báo cáo đánh giá mức độ trưởng thành quản trị dữ liệu của các cơ quan nhà nước; cần bảo đảm hiển thị đầy đủ dấu tiếng Việt và nội dung dài trong ô. '.repeat(3);
const goals=[{id:'g1',documentNumber:'2629/QĐ-TTg',code:'MT-10MT-21',title:'Tỷ lệ bộ, ngành, địa phương đạt cấp độ 3 mức độ trưởng thành về quản trị dữ liệu.',unitName:'%',targetPhase1:100,applicableYears:[2025,2026,2027],actual2025:0,estimate2026:null,plan2027:100,notes}, {id:'g2',documentNumber:'1266/QĐ-TTg',code:'MT-01',title:'Mục tiêu quyết định 1266',unitName:'%',targetPhase1:100,applicableYears:[2026,2027,2028,2029,2030],actual2025:99,estimate2026:0}];
const ws=buildGoalsWorksheet(goals,'Bộ Công an','10/11/2026',XLSX);
const task=buildTasksWorksheet([{id:'t1',documentNumber:'2629/QĐ-TTg',code:'NV-01',title:'Nhiệm vụ kiểm tra',statusLabel:'Đang thực hiện',notes:''}],'Bộ Công an','10/11/2026',XLSX);
const wb=XLSX.utils.book_new();XLSX.utils.book_append_sheet(wb,ws,'Mục tiêu');XLSX.utils.book_append_sheet(wb,task.ws,'Nhiệm vụ');
const rows=XLSX.utils.sheet_to_json(ws,{header:1});const r1=rows.findIndex(r=>String(r[1]).includes('MT-10')),r2=rows.findIndex(r=>r[1]==='MT-01');
assert.equal(ws[`B${r1+1}`].v,'MT-10\nMT-21');assert(ws[`B${r1+1}`].s.alignment.wrapText);assert(ws['!reportEditableCells'].includes(`G${r1+1}`));assert.equal(ws[`H${r1+1}`].v,'—');assert.equal(ws[`J${r1+1}`].v,'');assert.equal(ws[`G${r2+1}`].v,'');assert.equal(ws[`H${r2+1}`].v,0);assert(ws['!rows'][r1].hpt>80);
const bytes=encodeProgressWorkbook(wb,[[],task.statusCells],XLSX);fs.mkdirSync('scratch',{recursive:true});fs.writeFileSync('scratch/protected-progress-report.xlsx',bytes);
const zip=XLSX.CFB.read(bytes,{type:'array'});const read=path=>new TextDecoder().decode(XLSX.CFB.find(zip,zip.FullPaths.find(p=>p.endsWith(path))).content);
const styles=read('/xl/styles.xml');const xf=/color/.test(styles)&&/<cellXfs\b[^>]*>([\s\S]*?)<\/cellXfs>/.exec(styles)[1].match(/<xf\b[^>]*?\/>|<xf\b[^>]*>[\s\S]*?<\/xf>/g);
for(let index=0;index<2;index++){const sheet=wb.Sheets[wb.SheetNames[index]],xml=read(`/xl/worksheets/sheet${index+1}.xml`);assert.match(xml,/<sheetProtection\b[^>]*password="[A-F0-9]+"[^>]*sheet="1"/);const editable=new Set(sheet['!reportEditableCells']);for(const match of xml.matchAll(/<c\b([^>]*)>/g)){const ref=/\br="([^"]+)"/.exec(match[1])[1];const style=Number(/\bs="(\d+)"/.exec(match[1])?.[1]||0);assert.equal(/<protection\b[^>]*locked="0"/.test(xf[style]),editable.has(ref),ref+' protection');}assert(!styles.includes('FEF08A'));assert(!styles.includes('FEFCE8'));}
assert.match(read('/xl/worksheets/sheet2.xml'),/dataValidation[^>]*sqref="F6"/);assert.equal(task.ws.E4.v,'Sản phẩm bàn giao');assert.equal(task.ws.F4.v,'TRẠNG THÁI THỰC HIỆN');
for(const sheet of [ws,task.ws]){const rows=XLSX.utils.sheet_to_json(sheet,{header:1});for(let i=0;i<rows.length;i++)if(['I','II'].includes(rows[i][0]))assert.equal(sheet[`A${i+1}`].s.alignment.horizontal,'center');}
assert.equal(ws[`J${r1+1}`].s.fill.fgColor.rgb,ws[`E${r1+1}`].s.fill.fgColor.rgb);
assert.equal(ws[`C${r1+1}`].s.fill.fgColor.rgb.slice(-6), 'FFFFFF');
assert.equal(ws[`E${r1+1}`].s.fill.fgColor.rgb.slice(-6), 'F8F9FB');
assert.equal(ws[`H${r1+1}`].s.fill.fgColor.rgb.slice(-6), 'EAF1FC');
assert.notEqual(ws[`C${r1+1}`].s.fill.fgColor.rgb.slice(-6), ws[`J${r1+1}`].s.fill.fgColor.rgb);
const original=XLSX.utils.book_new();XLSX.utils.book_append_sheet(original,XLSX.utils.aoa_to_sheet([['Unrelated']]),'Other');const oldBytes=encodeProgressWorkbook(original,[[]],XLSX);const oldZip=XLSX.CFB.read(oldBytes,{type:'array'});assert(!new TextDecoder().decode(XLSX.CFB.find(oldZip,oldZip.FullPaths.find(p=>p.endsWith('/xl/worksheets/sheet1.xml'))).content).includes('sheetProtection'));
console.log('PASS report colors, blank locked cells, dash editable cells, heights, all XML locks and opt-in isolation');

// Editing historical results imports the correct year; excluded years are ignored.
ws[`G${r1+1}`] = {t:'n',v:33};ws[`H${r1+1}`] = {t:'n',v:44};ws[`G${r2+1}`] = {t:'n',v:99};
const edited=XLSX.write(wb,{type:'buffer',bookType:'xlsx'});
const {parseUnifiedProgressReportFile}=await import('../src/utils/unifiedProgressReportExcel.js');
const parsed=await parseUnifiedProgressReportFile({name:'edited.xlsx',arrayBuffer:async()=>edited.buffer.slice(edited.byteOffset,edited.byteOffset+edited.byteLength)},{existingGoals:goals});
assert.equal(parsed.goals.find(g=>g.id==='g1'&&g.periodYear===2025).value,33);
assert.equal(parsed.goals.find(g=>g.id==='g1'&&g.periodYear===2026).value,44);
assert(!parsed.goals.some(g=>g.id==='g2'&&g.periodYear===2025));
console.log('PASS 2025/2026 import separation, merged code lines, phase colors, centered sections and product/status order');

assert(!parsed.warnings.some(w=>/QUYẾT ĐỊNH SỐ/.test(w)), 'Decision group headings must not be treated as goal rows');

for(const sheet of [ws,task.ws]) {
 const text=JSON.stringify(XLSX.utils.sheet_to_json(sheet,{header:1}));
 assert(text.includes('2629/QĐ-TTg NGÀY 01/12/2025'));
 assert(text.includes('1266/QĐ-TTg NGÀY 14/07/2026') || sheet===task.ws);
 assert(!text.includes('10/12/2024')&&!text.includes('24/10/2024'));
}
