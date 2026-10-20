using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Application.AI;
// Shared by uploads and RSS, persisted across processes and restarts.
public sealed class AiInboxQueueGate(AppDbContext db)
{
    public async Task<Guid?> TryAcquireAsync(DateTime now,CancellationToken ct)
    {
        var owner=Guid.NewGuid();
        var claimed=await db.Set<AiInboxQueueState>().Where(s=>s.Id==1&&s.NextReadAt<=now&&(s.LeaseUntil==null||s.LeaseUntil<=now))
            .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Owner,(Guid?)owner).SetProperty(x=>x.LeaseUntil,now.AddHours(2)),ct);
        return claimed==1?owner:null;
    }
    public Task CompleteAsync(Guid owner,DateTime finishedAt,CancellationToken ct)=>db.Set<AiInboxQueueState>().Where(s=>s.Id==1&&s.Owner==owner)
        .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Owner,(Guid?)null).SetProperty(x=>x.LeaseUntil,(DateTime?)null).SetProperty(x=>x.NextReadAt,finishedAt.AddMinutes(1)),ct);
}
