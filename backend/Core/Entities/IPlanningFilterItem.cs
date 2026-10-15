namespace Cdsqg.Core.Entities;

// Shared scalar shape for domain SQL filtering and evaluated list DTO filtering.
public interface IPlanningFilterItem
{
    DateTime? StartDate { get; }
    DateTime? DueDate { get; }
    bool IsOngoing { get; }
    bool IsGeneralTask { get; }
    Guid? AssignedAgencyId { get; }
    string? Section { get; }
    string? Group { get; }
}
