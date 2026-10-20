from pathlib import Path
p=Path('backend/Application/AI/AiInboxService.cs');s=p.read_text(encoding='utf-8-sig');idx=s.index('    public async Task ReadAsync');s=s[:idx]+'''    public async Task ProcessQueuedAsync(Guid id,CancellationToken ct)
    {
        var options=savedSettings==null?settings.Value:await savedSettings.GetAsync(ct);if(!options.Ready)return;
        var now=DateTime.UtcNow;
        var claimed=await db.AiInboxItems.Where(i=>i.Id==id&&(i.StorageKey!=""||i.AlertFeedId!=null)&&(i.Status=="TitleQueued"||i.Status=="ReadQueued")&&(i.LeaseUntil==null||i.LeaseUntil<now))
            .ExecuteUpdateAsync(s=>s.SetProperty(i=>i.LeaseUntil,now.AddHours(2)),ct);
        if(claimed==0)return;
        var item=await db.AiInboxItems.SingleAsync(i=>i.Id==id,ct);
        try
        {
            if(item.Status=="TitleQueued"){CanonicalUrl(item.Url);await ClassifyAsync(item,ct);await db.SaveChangesAsync(ct);}
            if(item.Status=="ReadQueued")await ReadAsync(item,ct);
        }
        catch(OperationCanceledException)when(ct.IsCancellationRequested){throw;}
        catch{item.Status="Error";item.Error="Chưa xử lý được. Kiểm tra cấu hình AI, URL hoặc file rồi xếp lại hàng đợi.";}
        finally{item.LeaseUntil=null;await db.SaveChangesAsync(CancellationToken.None);}
    }
'''+s[idx:];a=s.index('                    foreach(var id in ids)');b=s.index('\n                }\n            }',a);s=s[:a]+'''                    foreach(var id in ids)await scope.ServiceProvider.GetRequiredService<AiInboxService>().ProcessQueuedAsync(id,ct);'''+s[b:];p.write_text(s,encoding='utf-8')
