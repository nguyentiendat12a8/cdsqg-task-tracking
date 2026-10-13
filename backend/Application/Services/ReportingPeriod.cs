namespace Cdsqg.Application.Services;

public static class ReportingPeriod
{
    public static int CurrentYear => DateTime.UtcNow.AddHours(7).Year;

    public static void ValidateYear(int year)
    {
        if (year is < 1900 or > 9999)
            throw new InvalidOperationException("Năm báo cáo không hợp lệ.");
        if (year > CurrentYear)
            throw new InvalidOperationException("Năm báo cáo không được lớn hơn năm hiện tại.");
    }
}
