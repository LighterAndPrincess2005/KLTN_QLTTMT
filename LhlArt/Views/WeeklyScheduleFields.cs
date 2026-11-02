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
        if(!locked)time=ClassPlanRules.Period(time) switch{"CHIEU"=>new TimeSpan(14,0,0),"TOI"=>new TimeSpan(18,30,0),_=>new TimeSpan(9,0,0)};
        form.Add("ThuHoc","Thứ học cố định",day?.ToString()??"",new[]{new Choice("","Chọn thứ học")}.Concat(Enumerable.Range(1,7).Select(i=>i%7).Select(i=>new Choice(i.ToString(),ClassPlanRules.Weekdays[i]))),readOnly:locked);
        form.Add("CaHoc","Ca học (giờ cố định)",ClassPlanRules.Period(time),[new("SANG","Sáng · 09:00"),new("CHIEU","Chiều · 14:00"),new("TOI","Tối · 18:30")],readOnly:locked);
        form.Add("GioBatDau","Giờ bắt đầu theo ca",time,readOnly:true);
        form.Add("CachTuan","Chu kỳ (tuần)",parsed.Weeks,readOnly:locked);
        var selectedDays=Enumerable.Range(0,7).Where(i=>ClassPlanRules.ContainsDay(ApiDataService.Normalize(existing),i))
            .OrderBy(i=>(i-(day??0)+7)%7).ToArray();
        form.Add("SoBuoiMoiTuan","Số buổi trong tuần học",Math.Max(1,selectedDays.Length),readOnly:locked);
        var weekdays=new[]{new Choice("","Chọn thứ học")}.Concat(Enumerable.Range(1,7).Select(i=>i%7).Select(i=>new Choice(i.ToString(),ClassPlanRules.Weekdays[i]))).ToArray();
        form.Add("ThuHoc2","Thứ học thứ hai",selectedDays.Length>1?selectedDays[1].ToString():"",weekdays,readOnly:locked);
        form.Add("ThuHoc3","Thứ học thứ ba",selectedDays.Length>2?selectedDays[2].ToString():"",weekdays,readOnly:locked);
        form.Add("LichHocDuKien","Lịch học dự kiến (tự tạo)",existing,readOnly:true,multiline:true);
        form.Changed("ThuHoc",()=>{if(!locked&&int.TryParse(form.TextValue("ThuHoc"),out var selected)){adjusting=true;form.Set(dateKey,ClassPlanRules.NextDay(form.Date(dateKey),selected));adjusting=false;}Refresh();});
        form.Changed("CaHoc",()=>{if(!locked&&!adjusting){adjusting=true;form.Set("GioBatDau",form.TextValue("CaHoc") switch{"CHIEU"=>new TimeSpan(14,0,0),"TOI"=>new TimeSpan(18,30,0),_=>new TimeSpan(9,0,0)});adjusting=false;}Refresh();});
        foreach(var key in new[]{dateKey,"GioBatDau","CachTuan","SoBuoiMoiTuan","ThuHoc2","ThuHoc3"})form.Changed(key,Refresh);
        Refresh();
    }
    public DateTime First=>form.Date(dateKey).Date.Add(form.Time("GioBatDau"));
    public int Weeks=>(int)form.Number("CachTuan");
    public DateTime Last=>LastFor(1);
    public int[] Days=>new[]{"ThuHoc","ThuHoc2","ThuHoc3"}.Take((int)form.Number("SoBuoiMoiTuan")).Select(x=>int.TryParse(form.TextValue(x),out var day)?day:-1).ToArray();
    public DateTime LastFor(int sessions)
    {
        var days=Days.Where(x=>x is >=0 and <=6).Distinct().OrderBy(x=>(x-(int)First.DayOfWeek+7)%7).ToArray();
        if(days.Length==0)return First;
        var index=Math.Max(0,sessions-1);
        return First.AddDays((index/days.Length)*7*Math.Max(1,Weeks)+(days[index%days.Length]-(int)First.DayOfWeek+7)%7);
    }
    public string Description=>int.TryParse(form.TextValue("ThuHoc"),out var day)&&day is >=0 and <=6&&duration() is >0 and <1440
        ?$"{string.Join(", ",Days.Where(x=>x is >=0 and <=6).Select(x=>ClassPlanRules.Weekdays[x]))} · {form.Time("GioBatDau").ToString(@"hh\:mm")}–{form.Time("GioBatDau").Add(TimeSpan.FromMinutes(duration())).ToString(@"hh\:mm")} · "+(Weeks==1?"Hàng tuần":$"Mỗi {Weeks} tuần")+$" · {Days.Length} buổi/{(Weeks==1?"tuần":$"{Weeks} tuần")}"
        :"Chọn thứ học và khóa học để xem lịch dự kiến.";
    public void Refresh(){form.Set("LichHocDuKien",Description);if(form.FieldValidation!=null&&!adjusting)form.ValidateFields();}
    public Dictionary<string,string> Validate()
    {
        var errors=ClassPlanRules.ValidateSlot(int.TryParse(form.TextValue("ThuHoc"),out var day)?day:null,form.TextValue("CaHoc"),form.Time("GioBatDau"),Weeks,form.Date(dateKey),duration());
        if(Days.Any(x=>x<0)||Days.Distinct().Count()!=Days.Length)errors[Days.Length==3?"ThuHoc3":"ThuHoc2"]="Chọn đủ các thứ học khác nhau trong tuần.";
        return errors;
    }
}
