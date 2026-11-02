import {createRequire} from 'node:module';
import assert from 'node:assert/strict';
const {chromium}=createRequire(process.env.PLAYWRIGHT_PACKAGE)('playwright');
const browser=await chromium.launch({channel:'msedge',headless:true});
try{for(const width of [1440,390])for(const [route,title] of [['agency-plans','Đầu mối liên hệ và KH CĐS'],['legal-documents','Hệ thống văn bản chuyển đổi số']]){
 const page=await browser.newPage({viewport:{width,height:950}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{localStorage.setItem('cdsqg_auth_token','fixture');localStorage.setItem('cdsqg_auth_user',JSON.stringify({role:1,username:'fixture'}));});
 await page.route('**/api/**',route=>{const url=new URL(route.request().url());let data=[];
 if(['/api/legaldocuments','/api/agencies'].includes(url.pathname)){
  const size=Number(url.searchParams.get('pageSize')||10),pageNumber=Number(url.searchParams.get('pageNumber')||1);
  const items=Array.from({length:Math.min(size,Math.max(0,21-(pageNumber-1)*size))},(_,i)=>url.pathname==='/api/legaldocuments'?{id:'legal-'+i,code:'QD-'+i,title:'Quyết định ban hành kế hoạch chuyển đổi số của cơ quan',documentType:'Quyết định',issuingAgencyName:'Bộ Khoa học và Công nghệ',issuedDate:'2026-11-01',attachments:[]}:{id:'agency-'+i,name:'Bộ Khoa học và Công nghệ '+i,type:'Ministry',code:'MIN-'+i,contacts:[]});data={items,totalCount:21,totalPages:Math.ceil(21/size)};
 }
 return route.fulfill({json:data});});
 await page.goto('http://127.0.0.1:5189/#'+route);const heading=page.getByRole('heading',{name:title,exact:true});await heading.waitFor();
 const gap=await heading.evaluate(node=>{const header=node.closest('.ui-screen-heading');return header.nextElementSibling.getBoundingClientRect().top-header.getBoundingClientRect().bottom;});assert(Math.abs(gap)<1,'Header gap: '+gap);assert.deepEqual(errors,[]);
 await page.getByText('Trang 1 / 3',{exact:true}).waitFor();
 const nextRequest=page.waitForRequest(req=>new URL(req.url()).searchParams.get('pageNumber')==='2');await page.getByRole('button',{name:'Trang sau ›',exact:true}).click();await nextRequest;await page.getByText('Trang 2 / 3',{exact:true}).waitFor();
 await page.getByRole('combobox',{name:'Số bản ghi/trang',exact:true}).click();const sizeRequest=page.waitForRequest(req=>new URL(req.url()).searchParams.get('pageSize')==='25');await page.getByRole('option',{name:'25',exact:true}).click();await sizeRequest;await page.getByText('Trang 1 / 1',{exact:true}).waitFor();
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1),false);assert.deepEqual(errors,[]);
 await page.screenshot({path:`C:/Users/84878/Documents/ChatGPT/CDSQG/scratch/${route}-header-${width}.png`});console.log(route+' '+width+'px joined header passed');await page.close();
}}finally{await browser.close();}
