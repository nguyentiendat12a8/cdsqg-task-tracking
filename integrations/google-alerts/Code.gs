/** Bound Google Apps Script: Google Alerts email -> private Sheet -> CDSQG inbox.
 * Script Properties: CDSQG_API_BASE_URL (HTTPS), CDSQG_ALERTS_BRIDGE_KEY (>=32 chars).
 * No AI API key belongs in this script. Run setup() once after setting properties.
 */
const ALERT_HEADERS=['Tiêu đề','URL','Ngày nhận','Email ID','Đồng bộ','Lỗi'];
const ALERT_HOSTS=['desapublications.un.org','publicadministration.un.org','publicadministration.desa.un.org','nso.gov.vn','www.nso.gov.vn','gso.gov.vn','www.gso.gov.vn','moit.gov.vn','www.moit.gov.vn','idea.gov.vn','www.idea.gov.vn','mst.gov.vn','www.mst.gov.vn'];
function setup(){
 const book=SpreadsheetApp.getActiveSpreadsheet();if(!book)throw new Error('Cần chạy trong Apps Script gắn với Google Sheets.');
 PropertiesService.getScriptProperties().setProperty('CDSQG_SHEET_ID',book.getId());
 const sheet=book.getSheetByName('Google Alerts')||book.insertSheet('Google Alerts');
 if(sheet.getLastRow()===0){sheet.appendRow(ALERT_HEADERS);sheet.setFrozenRows(1);sheet.getRange(1,1,1,6).setFontWeight('bold');sheet.setColumnWidth(1,420);sheet.setColumnWidth(2,480);}
 if(!ScriptApp.getProjectTriggers().some(t=>t.getHandlerFunction()==='syncAlerts'))ScriptApp.newTrigger('syncAlerts').timeBased().everyHours(1).create();
}
function plainText(html){return html.replace(/<[^>]*>/g,'').replace(/&amp;/g,'&').replace(/&quot;/g,'"').replace(/&#39;|&apos;/g,"'").replace(/&nbsp;/g,' ').replace(/&#(\d+);/g,(_,n)=>String.fromCodePoint(Number(n))).replace(/\s+/g,' ').trim();}
function officialUrl(href){
 let url=href.replace(/&amp;/g,'&');
 if(/^https:\/\/(?:www\.)?google\.[^/]+\/url\?/i.test(url)){const match=url.match(/[?&](?:url|q)=([^&]+)/);if(match){try{url=decodeURIComponent(match[1]);}catch{return null;}}}
 const match=url.match(/^https:\/\/([^/:?#]+)(?::443)?(?:[/?#]|$)/i);
 if(!match||!ALERT_HOSTS.includes(match[1].toLowerCase()))return null;
 return url.split('#')[0].replace(/([?&])utm_[^=&]+=[^&]*/gi,'$1').replace(/[?&]$/,'');
}
function syncAlerts(){
 const lock=LockService.getScriptLock();if(!lock.tryLock(1000))return;
 try{
  const props=PropertiesService.getScriptProperties(),id=props.getProperty('CDSQG_SHEET_ID');if(!id)throw new Error('Chạy setup trước.');
  const sheet=SpreadsheetApp.openById(id).getSheetByName('Google Alerts');
  const known=new Set(sheet.getLastRow()>1?sheet.getRange(2,2,sheet.getLastRow()-1,1).getValues().flat():[]);
  const done=GmailApp.getUserLabelByName('CDSQG-Alerts-Processed')||GmailApp.createLabel('CDSQG-Alerts-Processed');
  // Read only Alerts messages, never send mail. At most 30 threads per hourly run.
  const threads=GmailApp.search('from:googlealerts-noreply@google.com -label:CDSQG-Alerts-Processed',0,30);
  for(const thread of threads){
   for(const mail of thread.getMessages()){
    if(!/googlealerts-noreply@google\.com/i.test(mail.getFrom()))continue;
    const html=mail.getBody(),pattern=/<a\b[^>]*href=["']([^"']+)["'][^>]*>([\s\S]*?)<\/a>/gi;let m;
    while((m=pattern.exec(html))!==null){const url=officialUrl(m[1]),title=plainText(m[2]);if(!url||title.length<3||known.has(url))continue;
     // Escape spreadsheet formula prefixes from external titles.
     sheet.appendRow([/^[=+@-]/.test(title)?"'"+title:title,url,mail.getDate(),mail.getId(),'Chờ đồng bộ','']);known.add(url);
    }
   }
   thread.addLabel(done);
  }
  const base=(props.getProperty('CDSQG_API_BASE_URL')||'').replace(/\/$/,''),key=props.getProperty('CDSQG_ALERTS_BRIDGE_KEY')||'';
  if(!base||!key)return; // Sheet collection also works without a configured backend bridge.
  if(!/^https:\/\//i.test(base)||key.length<32)throw new Error('URL API cần HTTPS; khóa kết nối cần ít nhất 32 ký tự.');
  const rows=sheet.getDataRange().getValues();let sent=0;
  for(let i=1;i<rows.length&&sent<50;i++){
   if(rows[i][4]==='Đã đồng bộ')continue;
   const response=UrlFetchApp.fetch(base+'/api/ai-alerts-bridge',{method:'post',contentType:'application/json',headers:{'X-Alerts-Key':key},payload:JSON.stringify([{title:String(rows[i][0]).replace(/^'(?=[=+@-])/,'').slice(0,1000),url:rows[i][1]}]),muteHttpExceptions:true,followRedirects:false});
   const code=response.getResponseCode();
   sheet.getRange(i+1,5,1,2).setValues([[code>=200&&code<300?'Đã đồng bộ':'Chờ đồng bộ',code>=200&&code<300?'':'HTTP '+code]]);sent++;
   if(code===401||code===429||code>=500)break;
  }
 }finally{lock.releaseLock();}
}
