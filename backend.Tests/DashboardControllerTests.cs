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
    public class DashboardResponseDto
    {
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public List<AgencyStatusSummaryDto> MinistriesPerformance { get; set; } = new();
        public List<AgencyStatusSummaryDto> ProvincesPerformance { get; set; } = new();
    }

    public class DashboardControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            SeedTestData(context);
            return context;
        }

        private readonly Guid _bcaId = Guid.NewGuid();
        private readonly Guid _bttttId = Guid.NewGuid();
        private readonly Guid _tphcmId = Guid.NewGuid();

        private void SeedTestData(AppDbContext context)
        {
            var bca = new Agency { Id = _bcaId, Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };
            var btttt = new Agency { Id = _bttttId, Code = "BTTTT", Name = "Bộ Thông tin và Truyền thông", Type = AgencyTypeEnum.Ministry };
            var tphcm = new Agency { Id = _tphcmId, Code = "TPHCM", Name = "UBND TP. Hồ Chí Minh", Type = AgencyTypeEnum.Province };

            context.Agencies.AddRange(bca, btttt, tphcm);

            // Goal 1: BCA - Section 1, Group A, Year 2026-2027
            context.GoalTaskItems.Add(new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                Code = "GOAL-BCA-01",
                Title = "Mục tiêu Bộ Công An",
                ItemType = ItemTypeEnum.Goal,
                LeadAgencyId = _bcaId,
                Section = "Mục I",
                Group = "Nhóm A",
                StartDate = new DateTime(2026, 1, 1),
                DueDate = new DateTime(2027, 12, 31),
                IsOngoing = false
            });

            // Task 1: BCA - Section 1, Group B, Year 2028-2029
            context.GoalTaskItems.Add(new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                Code = "TASK-BCA-01",
                Title = "Nhiệm vụ Bộ Công An",
                ItemType = ItemTypeEnum.Task,
                LeadAgencyId = _bcaId,
                Section = "Mục I",
                Group = "Nhóm B",
                StartDate = new DateTime(2028, 1, 1),
                DueDate = new DateTime(2029, 12, 31),
                IsOngoing = false
            });

            // Goal 2: BTTTT - Section 2, Group A, Year 2026-2030
            context.GoalTaskItems.Add(new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                Code = "GOAL-BTTTT-01",
                Title = "Mục tiêu Bộ TTTT",
                ItemType = ItemTypeEnum.Goal,
                LeadAgencyId = _bttttId,
                Section = "Mục II",
                Group = "Nhóm A",
                StartDate = new DateTime(2026, 1, 1),
                DueDate = new DateTime(2030, 12, 31),
                IsOngoing = false
            });

            // Task 2: TPHCM - Section 2, Group B, Ongoing
            context.GoalTaskItems.Add(new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                Code = "TASK-TPHCM-01",
                Title = "Nhiệm vụ TPHCM Thường Xuyên",
                ItemType = ItemTypeEnum.Task,
                LeadAgencyId = _tphcmId,
                Section = "Mục II",
                Group = "Nhóm B",
                IsOngoing = true
            });

            context.SaveChanges();
        }

        private DashboardResponseDto GetResponseDto(IActionResult actionResult)
        {
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var json = JsonSerializer.Serialize(okResult.Value);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<DashboardResponseDto>(json, options)!;
        }

        [Fact]
        public async Task GetDashboardMetrics_NoFilters_ReturnsAllItems()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            var result = await controller.GetDashboardMetrics();
            var data = GetResponseDto(result);

            Assert.Equal(2, data.TotalGoals);
            Assert.Equal(2, data.TotalTasks);
            Assert.Equal(2, data.MinistriesPerformance.Count); // BCA & BTTTT
            Assert.Equal(1, data.ProvincesPerformance.Count);  // TPHCM
        }

        [Fact]
        public async Task GetDashboardMetrics_FilterByAgency_ReturnsOnlySelectedAgency()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            // Filter by BCA only
            var result = await controller.GetDashboardMetrics(agencyId: new[] { _bcaId });
            var data = GetResponseDto(result);

            Assert.Equal(1, data.TotalGoals);
            Assert.Equal(1, data.TotalTasks);

            // MinistriesPerformance must ONLY contain BCA
            Assert.Single(data.MinistriesPerformance);
            Assert.Equal("BCA", data.MinistriesPerformance.First().Code);

            // ProvincesPerformance must be empty since TPHCM was not selected
            Assert.Empty(data.ProvincesPerformance);
        }

        [Fact]
        public async Task GetDashboardMetrics_FilterByItemTypeGoals_ReturnsGoalMetrics()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            var result = await controller.GetDashboardMetrics(itemType: "Goal");
            var data = GetResponseDto(result);

            // Total goals & tasks counts remain accurate for tab badges
            Assert.Equal(2, data.TotalGoals);
            Assert.Equal(2, data.TotalTasks);

            // Performance metrics per agency reflect only Goals
            var bcaSummary = data.MinistriesPerformance.First(m => m.Code == "BCA");
            Assert.Equal(1, bcaSummary.TotalItems); // 1 Goal, 0 Tasks
            Assert.Equal(1, bcaSummary.TotalGoals);
            Assert.Equal(0, bcaSummary.TotalTasks);
        }

        [Fact]
        public async Task GetDashboardMetrics_FilterBySection_ReturnsFilteredItems()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            var result = await controller.GetDashboardMetrics(section: new[] { "Mục I" });
            var data = GetResponseDto(result);

            Assert.Equal(1, data.TotalGoals); // GOAL-BCA-01
            Assert.Equal(1, data.TotalTasks); // TASK-BCA-01
        }

        [Fact]
        public async Task GetDashboardMetrics_FilterByGroup_ReturnsFilteredItems()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            var result = await controller.GetDashboardMetrics(group: new[] { "Nhóm A" });
            var data = GetResponseDto(result);

            Assert.Equal(2, data.TotalGoals); // GOAL-BCA-01 and GOAL-BTTTT-01
            Assert.Equal(0, data.TotalTasks);
        }

        [Fact]
        public async Task GetDashboardMetrics_FilterByYearRange_ReturnsMatchingItems()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            // Filter year 2028-2029
            var result = await controller.GetDashboardMetrics(fromYear: 2028, toYear: 2029);
            var data = GetResponseDto(result);

            // TASK-BCA-01 (2028-2029) and TASK-TPHCM-01 (IsOngoing) and GOAL-BTTTT-01 (2026-2030) match
            Assert.True(data.TotalTasks >= 1);
        }

        [Fact]
        public async Task GetDashboardMetrics_FilterByOngoingOnly_ReturnsOnlyOngoingItems()
        {
            using var context = GetInMemoryDbContext();
            var controller = new DashboardController(context);

            var result = await controller.GetDashboardMetrics(isOngoing: true);
            var data = GetResponseDto(result);

            Assert.Equal(0, data.TotalGoals);
            Assert.Equal(1, data.TotalTasks); // TASK-TPHCM-01
        }
    }
}
