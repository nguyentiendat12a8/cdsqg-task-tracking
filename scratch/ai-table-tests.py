from pathlib import Path
p=Path('tests/ai-inbox.browser.mjs');s=p.read_text(encoding='utf-8-sig')
s=s.replace('let uploaded=false;','let uploaded=false,queued=false;').replace('isUpload:false','isUpload:true').replace("metricKeysJson:'[]'",'''metricKeysJson:'["EGDI_RANK"]' ''')
s=s.replace('Chưa rõ — cần admin xem','Cần xem lại').replace('Đã xử lý — chưa có số liệu hợp lệ','Chưa có số liệu')
s=s.replace("if(url.pathname.endsWith('/upload'))", "if(url.pathname.endsWith('/queue')){queued=true;assert.equal(route.request().postDataJSON().metricKey,'EGDI_RANK');data={message:'Đã xếp hàng xử lý.'};}\n  if(url.pathname.endsWith('/upload'))")
s=s.replace("await page.screenshot({path:'../scratch/ai-build/inbox-desktop.png'});", """assert.equal(await root.getByRole('combobox',{name:'Chỉ số đọc lại'}).count(),0);
 await root.getByRole('button',{name:'Chi tiết',exact:true}).click();
 const dialog=page.getByRole('dialog');await dialog.getByRole('heading',{name:'Chi tiết tài liệu'}).waitFor();
 await dialog.getByRole('button',{name:'Đọc lại',exact:true}).click();
 await dialog.getByRole('combobox',{name:'Chỉ số đọc lại',exact:true}).waitFor();
 await dialog.getByRole('button',{name:'Bắt đầu đọc lại'}).click();
 await dialog.getByRole('button',{name:'Đóng',exact:true}).click();assert.equal(queued,true);
 await page.screenshot({path:'../scratch/ai-build/inbox-desktop.png'});""")
p.write_text(s,encoding='utf-8')
