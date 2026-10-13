using Cdsqg.Application.Services;
using Xunit;

namespace Cdsqg.Tests;
public class ReportingPeriodTests
{
    [Fact]
    public void AcceptsCurrentAndPastYearsAndRejectsFutureYears()
    {
        ReportingPeriod.ValidateYear(ReportingPeriod.CurrentYear);
        ReportingPeriod.ValidateYear(ReportingPeriod.CurrentYear - 1);
        var error = Assert.Throws<InvalidOperationException>(() => ReportingPeriod.ValidateYear(ReportingPeriod.CurrentYear + 1));
        Assert.Contains("không được lớn hơn", error.Message);
        Assert.Throws<InvalidOperationException>(() => ReportingPeriod.ValidateYear(0));
    }
}
