namespace Cdsqg.Application.Services;
public static class DigitalNewsPublicationWindow {
 public static DateOnly Today=>DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
 public static DateOnly MonthStart(DateTime utcNow){var vn=utcNow.AddHours(7);return new(vn.Year,vn.Month,1);}
 public static DateOnly DefaultFrom=>MonthStart(DateTime.UtcNow);
 public static void Validate(DateOnly from,DateOnly? to){if(from.Year<2000||from>Today||to<from||to>Today)throw new InvalidOperationException("Ngày đăng từ phải từ năm 2000 đến hôm nay; ngày đến không trước ngày từ và không sau hôm nay.");}
 public static bool Includes(DateTime? published,DateOnly from,DateOnly? to){if(published==null)return false;var day=DateOnly.FromDateTime(published.Value.AddHours(7));return day>=from&&day<=(to??Today);}
}
