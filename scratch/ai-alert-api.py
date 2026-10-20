from pathlib import Path
p=Path('backend/Api/Controllers/AiInboxController.cs');s=p.read_text(encoding='utf-8-sig').replace('bool uploadsOnly=false,CancellationToken','bool uploadsOnly=false,bool alertsOnly=false,string? status=null,CancellationToken')
s=s.replace('        if(uploadsOnly)q=q.Where(i=>i.StorageKey!="");','        if(uploadsOnly)q=q.Where(i=>i.StorageKey!="");\n        if(alertsOnly)q=q.Where(i=>i.AlertFeedId!=null);\n        if(!string.IsNullOrEmpty(status))q=q.Where(i=>i.Status==status);')
s=s.replace('new{i.Id,i.Title,i.Url,i.Origin,i.ReceivedAt', 'new{i.Id,i.Title,i.Url,i.Origin,i.ReceivedAt,i.PublishedAt,i.AlertFeedId')
s=s.replace('if(item.StorageKey.Length==0)return Conflict(new{message="Đọc link Google Alerts đang tạm dừng. Giai đoạn này hãy nạp file tài liệu."});','if(item.StorageKey.Length==0&&item.AlertFeedId==null)return Conflict(new{message="Link cần được tiếp nhận từ nguồn RSS đã cấu hình."});\n        if(item.StorageKey.Length==0){try{AiInboxService.CanonicalUrl(item.Url);}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}}')
s=s.replace('if(input.TitleOnly&&item.StorageKey.Length==0)', 'if(input.TitleOnly&&item.StorageKey.Length==0)')
p.write_text(s,encoding='utf-8')
p=Path('backend/Api/Security/ApiPermissionFilter.cs');s=p.read_text(encoding='utf-8-sig').replace('or "AiProvider"','or "AiProvider" or "AiAlerts"');p.write_text(s,encoding='utf-8')
