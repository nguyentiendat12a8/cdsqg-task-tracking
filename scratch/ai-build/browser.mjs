import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const page=await browser.newPage({viewport:{width:1280,height:800}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.route('**/api/**',route=>{const p=new URL(route.request().url()).pathname;let data=[];
 if(p.endsWith('/catalog'))data={configured:false,metrics:[{key:'EGDI_RANK',name:'EGDI',unit:'rank',definition:'UN Survey',goalCodes:['MT-12','MT-25']}],goals:[{id:'goal1',code:'MT-12',title:'Chính phủ điện tử'}],allowedHosts:['desapublications.un.org']};
 if(p.endsWith('/suggestions'))data={total:11,items:[{id:'suggestion',documentId:'doc',metricKey:'EGDI_RANK',dataYear:2024,periodLabel:'UN 2024',value:71,unit:'rank',scope:'Việt Nam',quote:'Việt Nam xếp hạng 71',page:1,status:'Pending',sourceTitle:'UN report',sourceUrl:'https://desapublications.un.org/'}]};
 return route.fulfill({json:data});});
 await page.goto('http://127.0.0.1:5201');
 await page.evaluate(async()=>{const f=await import('/tests/refactor-fixture.js');const {default:View}=await import('/src/views/AiDataView.vue');f.authState.user.value={role:1,username:'admin'};f.authState.token.value='fixture';document.querySelector('#app').style.display='none';const node=document.createElement('main');node.id='ai-test';node.style.padding='16px';document.body.append(node);const app=f.createApp(View);app.directive('accessible-dialog',f.accessibleDialog);app.mount(node);});
 const root=page.locator('#ai-test');await root.getByText('Chưa có mục tiêu trong danh mục',{exact:false}).waitFor();
 await root.getByRole('button',{name:'Đề xuất và kiểm chứng',exact:true}).click();await root.getByText('Trang 1 / 2').waitFor();
 await root.getByRole('button',{name:'Kiểm chứng',exact:true}).click();
 assert.equal(await root.getByRole('button',{name:'Duyệt dữ liệu kiểm chứng',exact:true}).isDisabled(),true);
 await page.screenshot({path:'scratch/ai-build/review-desktop.png'});
 await page.setViewportSize({width:390,height:844});
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'scratch/ai-build/review-mobile.png'});
 assert.deepEqual(errors,[]);console.log('AI browser checks passed: catalog missing goal, pagination, review guard, desktop/mobile, no runtime errors.');
} finally {await browser.close();}
