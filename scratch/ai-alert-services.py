from pathlib import Path
p=Path('backend/Application/AI/AiInboxService.cs');s=p.read_text(encoding='utf-8-sig');a=s.index('        var count=0;');b=s.index('    public string PrivatePath',a)
s=s[:a]+'''        var count=0;
        foreach(var row in values.DistinctBy(r=>r.Url))if(await ImportEntryAsync(row,null,null,"Google Alerts",ct))count++;
        return count;
    }
    public async Task<bool> ImportEntryAsync(AlertInput row,Guid? feedId,DateTime? publishedAt,string origin,CancellationToken ct)
    {
        var url=CanonicalUrl(row.Url);var title=row.Title.Trim();
        if(title.Length is <3 or >1000)throw new InvalidOperationException("Tiêu đề phải từ 3–1.000 ký tự.");
        var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));var id=Guid.NewGuid();var now=DateTime.UtcNow;
        // A unique URL hash plus ON CONFLICT makes overlapping feeds/manual imports idempotent.
        var added=await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "AiInboxItems" ("Id","Url","UrlHash","Title","Origin","ReceivedAt","Status","MetricKeysJson","Reason","Error","Model","StorageKey","LeaseUntil","AlertFeedId","PublishedAt")
            VALUES ({id},{url},{hash},{title},{origin},{now},'TitleQueued','[]','','','','',NULL,{feedId},{publishedAt})
            ON CONFLICT ("UrlHash") DO NOTHING
            """,ct);
        return added==1;
    }
'''+s[b:]
s=s.replace('Where(i=>i.StorageKey!=""&&(i.Status=="TitleQueued"||i.Status=="ReadQueued")','Where(i=>(i.StorageKey!=""||i.AlertFeedId!=null)&&(i.Status=="TitleQueued"||i.Status=="ReadQueued")')
s=s.replace('Where(i=>i.Id==id&&(i.LeaseUntil==null||i.LeaseUntil<now))','Where(i=>i.Id==id&&(i.Status=="TitleQueued"||i.Status=="ReadQueued")&&(i.LeaseUntil==null||i.LeaseUntil<now))')
s=s.replace('if(item.Status=="TitleQueued")await service.ClassifyAsync(item,ct);else await service.ReadAsync(item,ct);','if(item.Status=="TitleQueued"){await service.ClassifyAsync(item,ct);await db.SaveChangesAsync(ct);}if(item.Status=="ReadQueued")await service.ReadAsync(item,ct);')
s=s.replace('else doc=await reader.ReadAsync(item.Url,ct);', 'else {CanonicalUrl(item.Url);doc=await reader.ReadAsync(item.Url,ct);}')
s=s.replace('ct,reprocess:true);','ct,reprocess:item.StorageKey.Length>0);')
p.write_text(s,encoding='utf-8')
p=Path('backend/Program.cs');s=p.read_text(encoding='utf-8-sig');needle='builder.Services.AddScoped<Cdsqg.Application.AI.AiInboxService>();';assert needle in s
s=s.replace(needle,needle+'''
builder.Services.AddHttpClient("ai-alert-feed",client=>client.Timeout=TimeSpan.FromSeconds(30)).ConfigurePrimaryHttpMessageHandler(Cdsqg.Application.AI.AiSourceReader.CreatePublicHandler);
builder.Services.AddScoped<Cdsqg.Application.AI.IAiAlertFeedReader,Cdsqg.Application.AI.AiAlertFeedReader>();
builder.Services.AddScoped<Cdsqg.Application.AI.AiAlertService>();
builder.Services.AddHostedService<Cdsqg.Application.AI.AiAlertWorker>();''');p.write_text(s,encoding='utf-8')
