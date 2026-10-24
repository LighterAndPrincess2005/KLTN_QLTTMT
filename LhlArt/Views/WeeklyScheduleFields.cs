using LhlArt.Services;
namespace LhlArt.Views;

internal sealed class WeeklyScheduleFields
{
    readonly InputForm form;readonly Func<int> duration;readonly string dateKey;
    bool adjusting;
    public WeeklyScheduleFields(InputForm form,string dateKey,string existing,Func<int> duration,bool locked=false,DateTime? actual=null)
    {
        this.form=form;this.dateKey=dateKey;this.duration=duration;
        var parsed=ClassPlanRules.Parse(existing);var day=actual.HasValue?(int?)actual.Value.DayOfWeek:parsed.Day;
        var time=actual?.TimeOfDay??parsed.Start??new TimeSpan(9,0,0);
        form.Add("ThuHoc","Thứ học cố định",day?.ToString()??"",new[]{new Choice("","Chọn thứ học")}.Concat(Enumerable.Range(1,7).Select(i=>i%7).Select(i=>new Choice(i.ToString(),ClassPlanRules.Weekdays[i]))),readOnly:locked);
        form.Add("CaHoc","Ca học (theo giờ bắt đầu)",ClassPlanRules.Period(time),[new("SANG","Sáng · trước 12:00"),new("CHIEU","Chiều · 12:00–17:59"),new("TOI","Tối · từ 18:00")],readOnly:locked);
        form.Add("GioBatDau","Giờ bắt đầu",time,readOnly:locked);
        form.Add("CachTuan","Học mỗi bao nhiêu tuần",parsed.Weeks,readOnly:locked);
        form.Add("LichHocDuKien","Lịch học dự kiến (tự tạo)",existing,readOnly:true,multiline:true);
        form.Changed("ThuHoc",()=>{if(!locked&&int.TryParse(form.TextValue("ThuHoc"),out var selected)){adjusting=true;form.Set(dateKey,ClassPlanRules.NextDay(form.Date(dateKey),selected));adjusting=false;}Refresh();});
        form.Changed("CaHoc",()=>{if(!locked&&!adjusting){adjusting=true;form.Set("GioBatDau",form.TextValue("CaHoc") switch{"CHIEU"=>new TimeSpan(14,0,0),"TOI"=>new TimeSpan(18,30,0),_=>new TimeSpan(9,0,0)});adjusting=false;}Refresh();});
        foreach(var key in new[]{dateKey,"GioBatDau","CachTuan"})form.Changed(key,Refresh);
        Refresh();
    }
    public DateTime First=>form.Date(dateKey).Date.Add(form.Time("GioBatDau"));
    public int Weeks=>(int)form.Number("CachTuan");
    public string Description=>int.TryParse(form.TextValue("ThuHoc"),out var day)&&day is >=0 and <=6&&duration() is >0 and <1440
        ?ClassPlanRules.Describe(day,form.Time("GioBatDau"),duration(),Weeks):"Chọn thứ học và khóa học để xem lịch dự kiến.";
    public void Refresh(){form.Set("LichHocDuKien",Description);if(form.FieldValidation!=null&&!adjusting)form.ValidateFields();}
    public Dictionary<string,string> Validate()=>ClassPlanRules.ValidateSlot(int.TryParse(form.TextValue("ThuHoc"),out var day)?day:null,form.TextValue("CaHoc"),form.Time("GioBatDau"),Weeks,form.Date(dateKey),duration());
}
