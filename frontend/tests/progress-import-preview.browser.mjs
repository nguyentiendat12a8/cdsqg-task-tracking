import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
import XLSX from 'xlsx-js-style';
import {buildGoalsWorksheet,buildTasksWorksheet} from '../src/utils/unifiedProgressReportExcel.js';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const goals=[{id:'a',documentNumber:'2629/QĐ-TTg',code:'MT-08',title:'Mục tiêu A',applicableYears:[2025,2026,2027],actual2025:null,estimate2026:41.2,plan2027:'100',notes:''},{id:'b',documentNumber:'2629/QĐ-TTg',code:'MT-10',title:'Mục tiêu B',applicableYears:[2025,2026,2027],actual2025:null,estimate2026:36.5,plan2027:'50',notes:''}];
const tasks=[{id:'t',code:'NV-01',documentNumber:'2629/QĐ-TTg',title:'Nhiệm vụ giữ nguyên',status:'InProgress',statusLabel:'Đang thực hiện',notes:''}];
const ws=buildGoalsWorksheet(goals,'Bộ Công an','11/11/2026',XLSX),wb=XLSX.utils.book_new();
const rows=XLSX.utils.sheet_to_json(ws,{header:1});for(let i=0;i<rows.length;i++){if(rows[i][1]==='MT-08')ws[`G${i+1}`]={t:'n',v:40};if(rows[i][1]==='MT-10')ws[`G${i+1}`]={t:'n',v:30};}
XLSX.utils.book_append_sheet(wb,ws,'Mục tiêu');XLSX.utils.book_append_sheet(wb,buildTasksWorksheet(tasks,'Bộ Công an','11/11/2026',XLSX).ws,'Nhiệm vụ');
const buffer=XLSX.write(wb,{type:'buffer',bookType:'xlsx'});
const b=await chromium.launch({channel:'msedge',headless:true});
try{for(const viewport of [{width:1366,height:768},{width:390,height:844}]){
const writes=[];const p=await b.newPage({viewport});await p.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
await p.route('**/api/**',r=>{const path=new URL(r.request().url()).pathname;if(r.request().method()==='POST')writes.push({path,body:r.request().postDataJSON()});return r.fulfill({json:path==='/api/agencies'?[{id:'agency',name:'Bộ Công an'}]:path==='/api/planning/agency-progress-report'?{goals,tasks,totalGoals:2,totalTasks:1,totalCount:2,totalPages:1}:[]});});
await p.goto('http://127.0.0.1:5189/#reports-progress');await p.getByRole('button',{name:'Nhập báo cáo',exact:true}).click();await p.locator('input[type=file]').setInputFiles({name:'report.xlsx',mimeType:'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',buffer});
await p.getByText('Chỉ cập nhật dữ liệu thay đổi.',{exact:false}).waitFor();
assert.equal(await p.locator('td.bg-amber-100').count(),2);assert(await p.getByText('→ 40',{exact:true}).isVisible());assert(await p.getByText('→ 30',{exact:true}).isVisible());
assert.equal(await p.locator('body').evaluate(e=>e.scrollWidth>innerWidth),false);
await p.getByRole('button',{name:'Xác nhận cập nhật',exact:true}).click();await p.getByText('Cập nhật dữ liệu thành công!',{exact:true}).waitFor();assert.equal(writes.length,1);assert.equal(writes[0].body.items.length,2);assert(writes[0].body.items.every(i=>i.periodYear===2025));await p.close();}
console.log('PASS desktop/mobile upload preview highlights exactly two changed 2025 cells');}finally{await b.close();}
