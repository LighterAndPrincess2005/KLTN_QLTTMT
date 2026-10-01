namespace MyThuat.Api.Services;
public static class BusinessClock
{
    // Mọi datetime nghiệp vụ trong schema dùng giờ Việt Nam.
    public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);
    public static DateOnly Today => DateOnly.FromDateTime(Now);
    public static DateTime AddWorkingDays(DateTime value,int days,IReadOnlyCollection<DateOnly> holidays)
    {
        while(days>0)
        {
            value=value.AddDays(1);
            if(value.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)&&!holidays.Contains(DateOnly.FromDateTime(value)))days--;
        }
        return value;
    }
}
