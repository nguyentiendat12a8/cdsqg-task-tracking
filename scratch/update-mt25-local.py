from pathlib import Path
p=Path('scratch/ai-pdf-probe/Program.cs');s=p.read_text(encoding='utf-8-sig');s=s[:s.index('foreach(var g')]+'''
await using var tx=await db.Database.BeginTransactionAsync();
var units=await db.Units.Where(u=>u.Code=="SL").ToListAsync();
if(units.Count!=1)throw new InvalidOperationException("Can dung mot don vi SL; dung cap nhat de tranh UnitId null.");
var affected=await db.Database.ExecuteSqlRawAsync("""
UPDATE "GoalTaskItems"
SET "UnitId" = (SELECT "Id" FROM "Units" WHERE "Code" = 'SL')
WHERE "Code" = 'MT-25';
""");
Console.WriteLine("Updated rows: "+affected);
foreach(var g in await db.GoalTaskItems.AsNoTracking().Include(g=>g.Unit).Where(g=>g.Code=="MT-25").ToListAsync())Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{g.Code,UnitCode=g.Unit!.Code,UnitName=g.Unit.Name}));
await tx.CommitAsync();
''';p.write_text(s,encoding='utf-8')
