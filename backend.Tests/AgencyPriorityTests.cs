using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Cdsqg.Api.Controllers;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Tests
{
    public class AgencyPriorityTests
    {
        private static string GetPropString(JsonElement el, string propName)
        {
            if (el.TryGetProperty(propName, out var val)) return val.GetString() ?? "";
            var camel = char.ToLowerInvariant(propName[0]) + propName.Substring(1);
            if (el.TryGetProperty(camel, out var cVal)) return cVal.GetString() ?? "";
            return "";
        }

        [Fact]
        public async Task GetAllAgencies_WhenPrioritizeContactsTrue_PutsAgenciesWithContactsFirst()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var ag1 = new Agency
            {
                Id = Guid.NewGuid(),
                Code = "AG1",
                Name = "An Giang",
                Type = AgencyTypeEnum.Province,
                ContactPersons = new List<AgencyContactPerson>() // no contacts
            };

            var ag2 = new Agency
            {
                Id = Guid.NewGuid(),
                Code = "BCT",
                Name = "Bộ Công Thương",
                Type = AgencyTypeEnum.Ministry,
                ContactPersons = new List<AgencyContactPerson>
                {
                    new AgencyContactPerson { Name = "Nguyễn Văn A", Position = "Chuyên viên" }
                }
            };

            var ag3 = new Agency
            {
                Id = Guid.NewGuid(),
                Code = "BCA",
                Name = "Bộ Công an",
                Type = AgencyTypeEnum.Ministry,
                ContactPersons = new List<AgencyContactPerson>() // no contacts
            };

            var ag4 = new Agency
            {
                Id = Guid.NewGuid(),
                Code = "BKHCN",
                Name = "Bộ Khoa học và Công nghệ",
                Type = AgencyTypeEnum.Ministry,
                ContactPersons = new List<AgencyContactPerson>
                {
                    new AgencyContactPerson { Name = "Trần Thị B", Position = "Trưởng phòng" }
                }
            };

            context.Agencies.AddRange(ag1, ag2, ag3, ag4);
            await context.SaveChangesAsync();

            var controller = new AgencyController(context, null!);

            // 1. Without prioritizeContacts: alphabetical order by name
            var resDefault = await controller.GetAllAgencies(excludeSpecial: true);
            var okDefault = Assert.IsType<OkObjectResult>(resDefault);
            using (var docDefault = JsonDocument.Parse(JsonSerializer.Serialize(okDefault.Value)))
            {
                var namesDefault = docDefault.RootElement.EnumerateArray()
                    .Select(e => GetPropString(e, "Name"))
                    .ToList();
                Assert.Equal(new[] { "An Giang", "Bộ Công an", "Bộ Công Thương", "Bộ Khoa học và Công nghệ" }, namesDefault);
            }

            // 2. With prioritizeContacts = true: contacts first, then no contacts
            var resPrioritized = await controller.GetAllAgencies(excludeSpecial: true, prioritizeContacts: true);
            var okPrioritized = Assert.IsType<OkObjectResult>(resPrioritized);
            using (var docPrioritized = JsonDocument.Parse(JsonSerializer.Serialize(okPrioritized.Value)))
            {
                var namesPrioritized = docPrioritized.RootElement.EnumerateArray()
                    .Select(e => GetPropString(e, "Name"))
                    .ToList();
                Assert.Equal(new[] { "Bộ Công Thương", "Bộ Khoa học và Công nghệ", "An Giang", "Bộ Công an" }, namesPrioritized);
            }

            // 3. Paged query with prioritizeContacts = true
            var resPaged = await controller.GetAllAgencies(excludeSpecial: true, prioritizeContacts: true, pageNumber: 1, pageSize: 2);
            var okPaged = Assert.IsType<OkObjectResult>(resPaged);
            using (var docPaged = JsonDocument.Parse(JsonSerializer.Serialize(okPaged.Value)))
            {
                var namesPaged = docPaged.RootElement.GetProperty("items").EnumerateArray()
                    .Select(e => GetPropString(e, "Name"))
                    .ToList();
                Assert.Equal(new[] { "Bộ Công Thương", "Bộ Khoa học và Công nghệ" }, namesPaged);
            }
        }
    }
}
