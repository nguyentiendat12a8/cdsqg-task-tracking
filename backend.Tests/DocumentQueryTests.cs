using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Xunit;

namespace Cdsqg.Tests;

public class DocumentQueryTests
{
    [Fact]
    public void ServerQueryPreservesExpandedAgencyFilterAndExplicitSort()
    {
        var ministries = Guid.Parse("00000000-0000-0000-0000-000000009998");
        var alternate = Guid.Parse("00000000-0000-0000-0000-000000009995");
        var items = new List<PlanningGridItemDto> {
            new() { Code = "MT-02", Title = "Z", ItemType = "Goal", LeadAgencyId = alternate, LeadAssignmentAgencyIds = new() { ministries } },
            new() { Code = "MT-01", Title = "A", ItemType = "Goal", LeadAgencyId = ministries, LeadAssignmentAgencyIds = new() { ministries } },
            new() { Code = "NV-01", ItemType = "Task", LeadAgencyId = Guid.NewGuid() }
        };
        var result = DocumentItemQuery.Apply(items, new() {
            ItemType = "Goal", SelectedAgencyIds = new() { ministries }, SortBy = "title", SortOrder = "asc"
        });
        Assert.Equal(new[] { "A", "Z" }, result.Select(i => i.Title));
    }

    [Fact]
    public void ChildViewDoesNotInheritGeneralItemsOrUnrelatedAgency()
    {
        var child = Guid.NewGuid(); var other = Guid.NewGuid();
        var scope = new AgencyViewScope(new() { child }, false);
        Assert.False(scope.CanView(new() { IsGeneralTask = true, LeadAgencyId = other, LeadAgencyCode = "ALL_AGENCIES" }));
        Assert.True(scope.CanView(new() { LeadAgencyId = other, AssignedAgencyId = child, LeadAssignmentAgencyIds = new() { child } }));
        Assert.False(scope.CanView(new() { LeadAgencyId = other }));
    }

    [Fact]
    public void DeadlineYearFilterAndOngoingModeKeepExistingServerContract()
    {
        var items = new List<PlanningGridItemDto> {
            new() { Code = "NV-01", DueDate = new DateTime(2028, 1, 1), StartDate = new DateTime(2026, 1, 1) },
            new() { Code = "NV-02", IsOngoing = true }
        };
        Assert.Empty(DocumentItemQuery.Apply(items, new() { FromYear = 2026, ToYear = 2026 }));
        Assert.Equal("NV-02", Assert.Single(DocumentItemQuery.Apply(items, new() { OnlyOngoing = true, FromYear = 2026, ToYear = 2026 })).Code);
    }

    [Fact]
    public void OverdueStatusFilterMatchesDeadlineStatus()
    {
        var items = new List<PlanningGridItemDto> {
            new() { Code = "NV-01", CalculatedStatus = "NotStarted", DeadlineStatus = "Overdue" },
            new() { Code = "NV-02", CalculatedStatus = "InProgress", DeadlineStatus = "Overdue" },
            new() { Code = "NV-03", CalculatedStatus = "NotStarted", DeadlineStatus = "OnTime" },
            new() { Code = "NV-04", CalculatedStatus = "Completed", DeadlineStatus = "CompletedOnTime" }
        };

        var overdueOnly = DocumentItemQuery.Apply(items, new() { SelectedStatuses = new() { "Overdue" } });
        Assert.Equal(new[] { "NV-01", "NV-02" }, overdueOnly.Select(i => i.Code));

        var notStartedOrOverdue = DocumentItemQuery.Apply(items, new() { SelectedStatuses = new() { "NotStarted", "Overdue" } });
        Assert.Equal(new[] { "NV-01", "NV-02", "NV-03" }, notStartedOrOverdue.Select(i => i.Code));
    }

    [Fact]
    public void ExecutionDateRangeFiltersCompletionDate()
    {
        var items = new List<PlanningGridItemDto> {
            new() { Code = "NV-01", DueDate = new DateTime(2026, 11, 15) },
            new() { Code = "NV-02", DueDate = new DateTime(2026, 12, 31, 23, 59, 59) },
            new() { Code = "NV-03", DueDate = new DateTime(2026, 10, 31) },
            new() { Code = "NV-04", DueDate = new DateTime(2027, 1, 1) },
            new() { Code = "NV-05", DueDate = null }
        };

        var range = DocumentItemQuery.Apply(items, new() {
            ExecutionFrom = new DateTime(2026, 11, 1),
            ExecutionTo = new DateTime(2026, 12, 31)
        });
        Assert.Equal(new[] { "NV-01", "NV-02" }, range.Select(i => i.Code));
    }
}
