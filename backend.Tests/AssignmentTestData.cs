using Cdsqg.Application.Services;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Tests;
internal static class AssignmentTestData
{
    public static async Task PrepareAsync(AppDbContext db)
    {
        foreach (var item in await db.GoalTaskItems.ToListAsync()) await AgencyAssignments.SyncAsync(db,item);
        await db.SaveChangesAsync();
    }
}
