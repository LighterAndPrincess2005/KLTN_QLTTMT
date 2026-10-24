using System.Text.RegularExpressions;
namespace LhlArt.Services;

public static class ClassPlanRules
{
    public static readonly string[] Weekdays=["Chủ nhật","Thứ Hai","Thứ Ba","Thứ Tư","Thứ Năm","Thứ Sáu","Thứ Bảy"];
    public static DateTime NextDay(DateTime date,int day)=>date.Date.AddDays((day-(int)date.DayOfWeek+7)%7);
    public static string Period(TimeSpan time)=>time.Hours<12?"SANG":time.Hours<18?"CHIEU":"TOI";
    public static (int? Day,TimeSpan? Start,int Weeks) Parse(string text)
    {
        var normalized=ApiDataService.Normalize(text);int? day=null;
        for(var i=0;i<Weekdays.Length;i++)if(normalized.Contains(ApiDataService.Normalize(Weekdays[i]))||(i>0&&Regex.IsMatch(normalized,@"\bthu\s+"+(i+1)+@"\b"))){day=i;break;}
        var time=Regex.Match(normalized,@"(?<!\d)([01]?\d|2[0-3])[:h]([0-5]\d)");
        var weeks=Regex.Match(normalized,@"moi\s+(\d+)\s+tuan");
        return(day,time.Success?new TimeSpan(int.Parse(time.Groups[1].Value),int.Parse(time.Groups[2].Value),0):null,
            weeks.Success&&int.TryParse(weeks.Groups[1].Value,out var interval)&&interval is >=1 and <=52?interval:1);
    }
    public static string Describe(int day,TimeSpan start,int minutes,int weeks)=>
        $"{Weekdays[day]} · {start:hh\\:mm}–{start.Add(TimeSpan.FromMinutes(minutes)):hh\\:mm} · "+(weeks==1?"Hàng tuần":$"Mỗi {weeks} tuần");
    public static Dictionary<string,string> ValidateSlot(int? day,string period,TimeSpan start,int weeks,DateTime date,int minutes)
    {
        var errors=new Dictionary<string,string>();
        if(day is null or <0 or >6)errors["ThuHoc"]="Hãy chọn thứ học cố định của lớp.";
        else if((int)date.DayOfWeek!=day)errors["NgayKhaiGiangDuKien"]=$"Ngày {date:dd/MM/yyyy} không phải {Weekdays[day.Value]}. Chọn lại ngày hoặc thứ học.";
        if(period is not("SANG" or "CHIEU" or "TOI"))errors["CaHoc"]="Hãy chọn ca sáng, chiều hoặc tối.";
        else if(Period(start)!=period)errors["GioBatDau"]="Giờ bắt đầu không thuộc ca đã chọn: sáng trước 12:00, chiều 12:00–17:59, tối từ 18:00.";
        if(minutes<=0||minutes>=24*60)errors["LichHocDuKien"]="Thời lượng buổi học phải lớn hơn 0 và nhỏ hơn 24 giờ; kiểm tra đề cương khóa.";
        else if(start.TotalMinutes+minutes>=24*60)errors["GioBatDau"]="Buổi học phải kết thúc trong cùng ngày. Hãy chọn giờ sớm hơn.";
        if(weeks is <1 or >52)errors["CachTuan"]="Khoảng cách giữa hai buổi phải từ 1 đến 52 tuần.";
        return errors;
    }
    public static Dictionary<string,string> ValidateClass(string course,int expected,decimal planned,decimal minimum,decimal maximum,decimal tuition,DateTime opens,DateTime closes,DateTime first)
    {
        var errors=new Dictionary<string,string>();
        if(expected>0&&planned!=expected)errors["SoBuoiKeHoach"]=$"Đang là {planned:0} buổi, nhưng khóa {course} quy định {expected} buổi. Số buổi của lớp phải khớp khóa.";
        if(first.Year<1900)errors["NgayKhaiGiangDuKien"]="Ngày khai giảng phải từ năm 1900 trở đi.";
        if(opens.Year<1900)errors["MoDangKyLuc"]="Ngày mở đăng ký phải từ năm 1900 trở đi.";
        if(closes.Year<1900)errors["DongDangKyLuc"]="Ngày đóng đăng ký phải từ năm 1900 trở đi.";
        if(minimum<1||minimum>short.MaxValue)errors["SiSoToiThieu"]="Sĩ số tối thiểu phải từ 1 đến 32.767.";
        if(maximum<minimum||maximum>short.MaxValue)errors["SiSoToiDa"]="Sĩ số tối đa phải lớn hơn hoặc bằng sĩ số tối thiểu và không quá 32.767.";
        if(tuition<0)errors["HocPhiApDung"]="Học phí không được âm.";
        if(opens>=closes){errors["MoDangKyLuc"]="Thời điểm mở phải trước thời điểm đóng đăng ký.";errors["DongDangKyLuc"]="Thời điểm đóng phải sau thời điểm mở đăng ký.";}
        if(closes>first.AddHours(-48))errors["DongDangKyLuc"]=$"Cần đóng đăng ký ít nhất 48 giờ trước buổi đầu: chậm nhất {first.AddHours(-48):dd/MM/yyyy HH:mm}.";
        return errors;
    }
}
