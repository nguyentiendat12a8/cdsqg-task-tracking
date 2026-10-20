from pathlib import Path
p=Path('scratch/ai-pdf-probe/Program.cs');s=p.read_text(encoding='utf-8-sig');a=s.index('var now=DateTime.UtcNow;');b=s.index('sealed class Clients',a);s=s[:a]+'''Console.WriteLine($"Status={item.Status}; leaseActive={item.LeaseUntil>DateTime.UtcNow}; leaseUntil={item.LeaseUntil:O}");
var url="/api/ai-inbox/"+item.Id+"/file";
foreach(var d in await db.AiSourceDocuments.Where(d=>d.Url==url).ToListAsync())Console.WriteLine($"Document={d.Status}; suggestions={await db.AiDataSuggestions.CountAsync(v=>v.DocumentId==d.Id)}");
'''+s[b:];p.write_text(s,encoding='utf-8')
