using Cdsqg.Application.AI;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Api.Controllers;

[ApiController]
[Route("api/ai-goal-keywords")]
[Authorize(Roles = "Admin")]
public sealed class AiGoalKeywordsController(AppDbContext db) : ControllerBase
{
    public sealed record AiGoalKeywordInput(
        Guid? DocumentId,
        string GoalCode,
        string GoalTitle,
        string KeywordsVi,
        string? KeywordsEn,
        string ExpectedUnit,
        string ExtractionPrompt
    );

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? documentId,
        [FromQuery] string? search,
        [FromQuery] string? unit,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 100000);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = db.AiGoalKeywords.AsNoTracking();

        if (documentId.HasValue && documentId.Value != Guid.Empty)
        {
            query = query.Where(k => k.DocumentId == documentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(unit))
        {
            var u = unit.Trim().ToLower();
            query = query.Where(k => k.ExpectedUnit.ToLower() == u);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(k =>
                k.GoalCode.ToLower().Contains(term) ||
                k.GoalTitle.ToLower().Contains(term) ||
                k.KeywordsVi.ToLower().Contains(term) ||
                k.KeywordsEn.ToLower().Contains(term) ||
                k.ExpectedUnit.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(k => k.GoalCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Danh sách văn bản hỗ trợ
        var availableDocuments = await db.Documents
            .AsNoTracking()
            .OrderBy(d => d.DocumentNumber)
            .Select(d => new
            {
                d.Id,
                d.DocumentNumber,
                d.Name
            })
            .ToListAsync(ct);

        // Danh sách mục tiêu trong hệ thống để gợi ý liên kết trên giao diện
        var goalsQuery = db.GoalTaskItems
            .Include(g => g.Unit)
            .Include(g => g.Document)
            .AsNoTracking()
            .Where(g => g.ItemType == ItemTypeEnum.Goal);

        if (documentId.HasValue && documentId.Value != Guid.Empty)
        {
            goalsQuery = goalsQuery.Where(g => g.DocumentId == documentId.Value);
        }

        var availableGoals = await goalsQuery
            .OrderBy(g => g.Code)
            .Select(g => new
            {
                g.Id,
                g.DocumentId,
                DocumentNumber = g.Document != null ? g.Document.DocumentNumber : "",
                g.Code,
                g.Title,
                EvaluationType = g.EvaluationType.ToString(),
                Unit = g.Unit != null ? g.Unit.Name : ""
            })
            .ToListAsync(ct);

        return Ok(new
        {
            items,
            total,
            page,
            pageSize,
            availableDocuments,
            availableGoals
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await db.AiGoalKeywords.AsNoTracking().SingleOrDefaultAsync(k => k.Id == id, ct);
        if (item == null) return NotFound(new { message = "Không tìm thấy bộ từ khóa mục tiêu." });
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AiGoalKeywordInput input, CancellationToken ct)
    {
        var docId = input.DocumentId.HasValue && input.DocumentId.Value != Guid.Empty
            ? input.DocumentId.Value
            : Guid.Parse("12660000-0000-0000-0000-000000001266");

        var goalCode = input.GoalCode?.Trim().ToUpperInvariant() ?? "";
        var goalTitle = input.GoalTitle?.Trim() ?? "";
        var keywordsVi = input.KeywordsVi?.Trim() ?? "";
        var keywordsEn = input.KeywordsEn?.Trim() ?? "";
        var expectedUnit = input.ExpectedUnit?.Trim() ?? "";
        var prompt = input.ExtractionPrompt?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(goalCode) || goalCode.Length > 20)
            return BadRequest(new { message = "Mã mục tiêu là bắt buộc và tối đa 20 ký tự." });
        if (string.IsNullOrWhiteSpace(goalTitle) || goalTitle.Length > 500)
            return BadRequest(new { message = "Tiêu đề mục tiêu là bắt buộc và tối đa 500 ký tự." });
        if (string.IsNullOrWhiteSpace(keywordsVi))
            return BadRequest(new { message = "Từ khóa tiếng Việt là bắt buộc." });
        if (string.IsNullOrWhiteSpace(expectedUnit) || expectedUnit.Length > 50)
            return BadRequest(new { message = "Đơn vị kỳ vọng là bắt buộc và tối đa 50 ký tự." });
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest(new { message = "Câu lệnh bóc tách AI là bắt buộc." });

        if (await db.AiGoalKeywords.AnyAsync(k => k.DocumentId == docId && k.GoalCode == goalCode, ct))
            return Conflict(new { message = $"Mã mục tiêu '{goalCode}' đã có cấu hình từ khóa cho văn bản này." });

        var entity = new AiGoalKeyword
        {
            Id = Guid.NewGuid(),
            DocumentId = docId,
            GoalCode = goalCode,
            GoalTitle = goalTitle,
            KeywordsVi = keywordsVi,
            KeywordsEn = keywordsEn,
            ExpectedUnit = expectedUnit,
            ExtractionPrompt = prompt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.AiGoalKeywords.Add(entity);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AiGoalKeywordInput input, CancellationToken ct)
    {
        var entity = await db.AiGoalKeywords.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (entity == null) return NotFound(new { message = "Không tìm thấy bộ từ khóa mục tiêu cần cập nhật." });

        var docId = input.DocumentId.HasValue && input.DocumentId.Value != Guid.Empty
            ? input.DocumentId.Value
            : entity.DocumentId;

        var goalCode = input.GoalCode?.Trim().ToUpperInvariant() ?? "";
        var goalTitle = input.GoalTitle?.Trim() ?? "";
        var keywordsVi = input.KeywordsVi?.Trim() ?? "";
        var keywordsEn = input.KeywordsEn?.Trim() ?? "";
        var expectedUnit = input.ExpectedUnit?.Trim() ?? "";
        var prompt = input.ExtractionPrompt?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(goalCode) || goalCode.Length > 20)
            return BadRequest(new { message = "Mã mục tiêu là bắt buộc và tối đa 20 ký tự." });
        if (string.IsNullOrWhiteSpace(goalTitle) || goalTitle.Length > 500)
            return BadRequest(new { message = "Tiêu đề mục tiêu là bắt buộc và tối đa 500 ký tự." });
        if (string.IsNullOrWhiteSpace(keywordsVi))
            return BadRequest(new { message = "Từ khóa tiếng Việt là bắt buộc." });
        if (string.IsNullOrWhiteSpace(expectedUnit) || expectedUnit.Length > 50)
            return BadRequest(new { message = "Đơn vị kỳ vọng là bắt buộc và tối đa 50 ký tự." });
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest(new { message = "Câu lệnh bóc tách AI là bắt buộc." });

        if (await db.AiGoalKeywords.AnyAsync(k => k.DocumentId == docId && k.GoalCode == goalCode && k.Id != id, ct))
            return Conflict(new { message = $"Mã mục tiêu '{goalCode}' đã trùng với bản ghi khác trong cùng văn bản." });

        entity.DocumentId = docId;
        entity.GoalCode = goalCode;
        entity.GoalTitle = goalTitle;
        entity.KeywordsVi = keywordsVi;
        entity.KeywordsEn = keywordsEn;
        entity.ExpectedUnit = expectedUnit;
        entity.ExtractionPrompt = prompt;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await db.AiGoalKeywords.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (entity == null) return NotFound(new { message = "Không tìm thấy bộ từ khóa mục tiêu cần xóa." });

        db.AiGoalKeywords.Remove(entity);
        await db.SaveChangesAsync(ct);

        return Ok(new { message = "Đã xóa bộ từ khóa mục tiêu thành công." });
    }
}
