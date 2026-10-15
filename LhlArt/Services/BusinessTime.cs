namespace LhlArt.Services;
public static class BusinessTime
{
    static readonly TimeZoneInfo Vietnam = TimeZoneInfo.CreateCustomTimeZone("Vietnam", TimeSpan.FromHours(7), "Giờ Việt Nam", "Giờ Việt Nam");
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Vietnam);
}
