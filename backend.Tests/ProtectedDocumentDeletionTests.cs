using Cdsqg.Api.Controllers;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Cdsqg.Tests;

public class ProtectedDocumentDeletionTests
{
    [Theory]
    [InlineData("1266/QĐ-TTg", ItemTypeEnum.Goal)]
    [InlineData("1266/QĐ-TTg", ItemTypeEnum.Task)]
    [InlineData("2629/QĐ-TTg", ItemTypeEnum.Goal)]
    [InlineData("2629/QĐ-TTg", ItemTypeEnum.Task)]
    public async Task DeleteItemRejectsFixedDocumentItems(string documentNumber, ItemTypeEnum itemType)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(options);
        var document = new Document { DocumentNumber = documentNumber };
        var item = new GoalTaskItem { DocumentId = document.Id, ItemType = itemType, Code = "TEST", Title = "Test" };
        db.AddRange(document, item);
        await db.SaveChangesAsync();

        var result = await new GoalTaskItemController(db).DeleteItem(item.Id);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.True(await db.GoalTaskItems.AnyAsync(i => i.Id == item.Id));
    }

    [Fact]
    public async Task DeleteItemStillAllowsUnreportedItemFromOtherDocument()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(options);
        var document = new Document { DocumentNumber = "9999/QĐ-TTg" };
        var item = new GoalTaskItem { DocumentId = document.Id, ItemType = ItemTypeEnum.Task, Code = "TEST", Title = "Test" };
        db.AddRange(document, item);
        await db.SaveChangesAsync();

        var result = await new GoalTaskItemController(db).DeleteItem(item.Id);

        Assert.IsType<OkObjectResult>(result);
        Assert.False(await db.GoalTaskItems.AnyAsync(i => i.Id == item.Id));
    }
}
