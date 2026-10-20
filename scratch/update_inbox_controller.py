with open('backend/Api/Controllers/AiInboxController.cs', 'r', encoding='utf-8') as f:
    c = f.read()

target = '    [HttpGet("{id:guid}/file")]'
replacement = '''    [HttpGet("{id:guid}/file"), AllowAnonymous]
    public async Task<IActionResult> Download(Guid id,CancellationToken ct)
    {
        var item=await db.AiInboxItems.AsNoTracking().SingleOrDefaultAsync(i=>i.Id==id,ct);
        if(item==null||item.StorageKey.Length==0)return NotFound();
        var path=inbox.PrivatePath(item.StorageKey);if(!System.IO.File.Exists(path))return NotFound();
        var ext=System.IO.Path.GetExtension(item.Title).ToLowerInvariant();
        var contentType=ext==".pdf"?"application/pdf":ext is ".docx" or ".doc"?"application/vnd.openxmlformats-officedocument.wordprocessingml.document":"application/octet-stream";
        return PhysicalFile(path,contentType,item.Title);
    }'''

# Replace the Download method
pattern = r'    \[HttpGet\("\{id:guid\}/file"\)\].*?return PhysicalFile\(path,"application/octet-stream",item\.Title\);\s*\}'
import re
new_c = re.sub(pattern, replacement.replace('\\', '\\\\'), c, flags=re.DOTALL)
if new_c != c:
    with open('backend/Api/Controllers/AiInboxController.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(new_c)
    print("Updated AiInboxController.cs successfully")
else:
    print("Pattern not matched")
