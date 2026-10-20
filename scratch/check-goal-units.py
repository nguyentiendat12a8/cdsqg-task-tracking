from pathlib import Path
p=Path('scratch/ai-pdf-probe/Program.cs');s=p.read_text(encoding='utf-8-sig');a=s.index('var options=');s=s[:a]+'''foreach(var g in await db.GoalTaskItems.Include(g=>g.Unit).Where(g=>g.Code=="MT-12"||g.Code=="MT-25").ToListAsync())Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{g.Code,g.Title,g.UnitId,UnitCode=g.Unit?.Code,UnitName=g.Unit?.Name,g.EvaluationType,g.TargetValue,g.CustomYearTargets}));
''';s=s.replace(',g.CustomYearTargets','');p.write_text(s,encoding='utf-8')
