import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
import {readProductProgress} from '../src/utils/progressProducts.js';
const XLSX=createRequire(import.meta.url)('xlsx-js-style');
const task={code:'NV01',deliverables:[{id:'p1'},{id:'p2'}]};
const header=['STT','Mã Nhiệm Vụ','Mã Sản Phẩm','Tên Sản Phẩm','Trạng Thái Hiện Tại','TRẠNG THÁI MỚI [CẦN CHỌN]'];
const first=[1,'NV01','p1','SP1','Chưa thực hiện','Đã hoàn thành'];
const second=[2,'NV01','p2','SP2','Chưa thực hiện','Đang thực hiện'];
function book(rows){return {Sheets:{'Sản phẩm đầu ra':XLSX.utils.aoa_to_sheet([header,...rows])}}}
assert.equal(readProductProgress({Sheets:{}},task,XLSX).error,'Thiếu sheet Sản phẩm đầu ra');
assert.deepEqual(readProductProgress(book([first,second]),task,XLSX).deliverables,[{id:'p1',currentStatus:'Completed'},{id:'p2',currentStatus:'Drafting'}]);
assert.ok(readProductProgress(book([first]),task,XLSX).error);
assert.ok(readProductProgress(book([first,first]),task,XLSX).error);
assert.ok(readProductProgress(book([first,[2,'NV01','UNKNOWN','','','Đã hoàn thành']]),task,XLSX).error);
assert.ok(readProductProgress(book([first,[2,'NV01','p2','','','Đã hoàn thành tự nhập thêm']]),task,XLSX).error);
assert.equal(readProductProgress(book([first]),{code:'NV01',deliverables:[{id:'p1'}]},XLSX).deliverables.length,1);
assert.deepEqual(readProductProgress(book([[...first.slice(0,5),''],[...second.slice(0,5),'  ']]),task,XLSX),{skipped:true});
assert.ok(readProductProgress(book([[...first.slice(0,5),''],second]),task,XLSX).error);
console.log('Passed: one/multiple products, exact 3 states, missing/duplicate/unknown IDs, missing sheet.');

const mergedSheet=XLSX.utils.aoa_to_sheet([['Mã','Sản Phẩm Đầu Ra','TIẾN ĐỘ THỰC HIỆN MỚI','Mã Sản Phẩm'],['NV01','SP1','Đã hoàn thành','p1'],['','SP2','Đang thực hiện','p2']]);
mergedSheet['!merges']=[{s:{r:1,c:0},e:{r:2,c:0}}];
const mergedBook={SheetNames:['Báo Cáo Tiến Độ'],Sheets:{'Báo Cáo Tiến Độ':mergedSheet}};
assert.deepEqual(readProductProgress(mergedBook,task,XLSX).deliverables,[{id:'p1',currentStatus:'Completed'},{id:'p2',currentStatus:'Drafting'}]);
delete mergedSheet['!merges'];
assert.ok(readProductProgress(mergedBook,task,XLSX).error);
console.log('Passed merged inline product rows and missing merge validation.');
