import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const p=await browser.newPage();let releaseB;
 await p.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 await p.route('**/api/**',async r=>{const u=new URL(r.request().url());if(u.pathname==='/api/agencies')return r.fulfill({json:[{id:'a',name:'Bộ Công an'},{id:'b',name:'Cơ quan B'}]});if(u.pathname==='/api/planning/agency-progress-report'){const id=u.searchParams.get('agencyId');if(id==='b')await new Promise(resolve=>releaseB=resolve);return r.fulfill({json:{agencyId:id,goals:[],tasks:[{id,code:'NV-01',documentNumber:'2629/QĐ-TTg',title:'Nhiệm vụ '+id}],totalGoals:0,totalTasks:1,totalCount:1,totalPages:1,page:1}});}return r.fulfill({json:[]});});
 await p.goto('http://127.0.0.1:5189/#reports-progress');await p.locator('#main-content').getByRole('button',{name:/Nhiệm vụ/}).first().click();await p.getByText('Nhiệm vụ a',{exact:true}).waitFor();
 const select=p.locator('#main-content').getByRole('combobox').first();await select.click();await p.getByRole('option',{name:'Cơ quan B',exact:true}).click();await p.waitForRequest(()=>false,{timeout:100}).catch(()=>{});
 await select.click();await p.getByRole('option',{name:'Bộ Công an',exact:true}).click();await p.getByText('Nhiệm vụ a',{exact:true}).waitFor();releaseB();await p.waitForTimeout(300);assert.equal(await p.getByText('Nhiệm vụ b',{exact:true}).count(),0);assert(await p.getByText('Nhiệm vụ a',{exact:true}).isVisible());
 console.log('PASS task report stays scoped to latest selected agency after out-of-order responses');
}finally{await browser.close();}
