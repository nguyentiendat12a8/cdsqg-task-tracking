using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

// Explicit maintenance for the old 2629 importer which treated every unknown name as ALL_AGENCIES.
// Never run on startup: existing assignment snapshots are otherwise immutable on reads.
public static class Seed2629AssignmentRepair
{
    public static async Task<int> RunAsync(AppDbContext db)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var items = await db.GoalTaskItems.Where(i => i.DocumentId == Guid.Parse("26290000-0000-0000-0000-000000002629")).ToListAsync();
        if (items.Count == 0) { await tx.CommitAsync(); return 0; }
        var agencies = await db.Agencies.ToListAsync();
        Agency Ensure(string code, string name, AgencyTypeEnum type = AgencyTypeEnum.Ministry)
        {
            var matches = agencies.Where(a => a.Code == code || string.Equals(a.Name.Trim(), name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count > 1) throw new InvalidOperationException($"Trùng danh mục cơ quan: {name}");
            if (matches.Count == 1) return matches[0];
            var agency = new Agency { Code = code, Name = name, Type = type };
            db.Agencies.Add(agency); agencies.Add(agency); return agency;
        }
        var bca = Ensure("BCA", "Bộ Công an");
        var vpcp = Ensure("VPCP", "Văn phòng Chính phủ");
        var bkhcn = Ensure("BKHCN", "Bộ Khoa học và Công nghệ");
        var bnv = Ensure("BNV", "Bộ Nội vụ");
        var btc = Ensure("BTC", "Bộ Tài chính");
        var bcy = Ensure("BCYCP", "Ban Cơ yếu Chính phủ", AgencyTypeEnum.Other);
        var all = Ensure("ALL_AGENCIES", "Các bộ, ngành, địa phương", AgencyTypeEnum.Special);
        var ministries = Ensure("ALL_MINISTRIES_DIRECT", "Các bộ, ngành", AgencyTypeEnum.Special);
        await db.SaveChangesAsync();
        int repaired = 0;
        foreach (var item in items)
        {
            var text = (item.LeadAgencyDisplayText ?? "").Trim();
            Agency lead; Agency? second = null;
            switch (text.ToLowerInvariant())
            {
                case "văn phòng chính phủ": lead = vpcp; break;
                case "bộ khoa học và công nghệ": lead = bkhcn; break;
                case "bộ nội vụ": lead = bnv; break;
                case "bộ tài chính": lead = btc; break;
                case "ban cơ yếu chính phủ": lead = bcy; break;
                case "bộ công an": case "bộ công an (trung tâm dữ liệu quốc gia)": lead = bca; break;
                case "các bộ, ngành": lead = ministries; break;
                case "các bộ, ngành, địa phương": lead = all; break;
                case "bộ khoa học và công nghệ, bộ công an": lead = bkhcn; second = bca; break;
                case "bộ khoa học và công nghệ; bộ nội vụ": lead = bkhcn; second = bnv; break;
                default:
                    if (text.StartsWith("Bộ Công an, Bộ Khoa học và Công nghệ với các tiêu chuẩn", StringComparison.OrdinalIgnoreCase)
                        && text.Contains("các bộ, ngành", StringComparison.OrdinalIgnoreCase)) lead = ministries;
                    else throw new InvalidOperationException($"Chưa có ánh xạ chính xác cho {item.Code}; dừng sửa dữ liệu.");
                    break;
            }
            var legacy = text.Contains("Bộ Công an", StringComparison.OrdinalIgnoreCase) ? bca.Id : all.Id;
            // Do not overwrite a manual reassignment made after the original seed.
            if (item.LeadAgencyId != legacy && item.LeadAgencyId != lead.Id) continue;
            if (item.AssignedAgencyId.HasValue && item.AssignedAgencyId != second?.Id) continue;
            item.LeadAgencyId = lead.Id;
            item.AssignedAgencyId = second?.Id;
            await AgencyAssignments.SyncAsync(db, item, refreshCoordination: false);
            repaired++;
        }
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return repaired;
    }
}
