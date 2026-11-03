using System.Globalization;
using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;

internal sealed class ApiClassController(ArtApiClient api)
{
    static string T(JsonElement row,string key)=>ArtApiClient.Text(row,key);
    static DateTime D(JsonElement? row,string key,DateTime fallback)=>row.HasValue&&DateTime.TryParse(T(row.Value,key),CultureInfo.InvariantCulture,DateTimeStyles.None,out var value)?value:fallback;
    static decimal N(JsonElement? row,string key,decimal fallback)=>row.HasValue&&decimal.TryParse(T(row.Value,key),CultureInfo.InvariantCulture,out var value)?value:fallback;
    public async Task Edit(IWin32Window owner,JsonElement? row)
    {
        var data=new ApiDataService(api);var courses=await data.All("api/quan-ly/khoa-hoc");
        var sessions=row.HasValue?(await data.All("api/lich-hoc",false)).Where(x=>ApiDataService.Id(x,"lopHocId")==ApiDataService.Id(row.Value)).ToList():[];
        using var form=CreateForm(courses,row,sessions);form.ShowDialog(owner);
    }
    public async Task Generate(IWin32Window owner,JsonElement row)
    {
        var classId=ApiDataService.Id(row);
        row=await api.Read("api/quan-ly/lop-hoc/"+classId);
        if(T(row,"trangThai")!="NHAP")
        {
            MessageBox.Show(owner,"Chỉ xếp lịch lần đầu cho lớp nháp. Với lớp đã công bố, xem lịch hiện tại hoặc dùng chức năng dời buổi.","Lớp đã công bố");
            return;
        }
        var data=new ApiDataService(api);
        if((await data.All("api/lich-hoc",false)).Any(x=>ApiDataService.Id(x,"lopHocId")==classId))
        {
            MessageBox.Show(owner,"Lớp nháp đã có lịch. Mở Sửa lớp để công bố tuyển sinh hoặc dùng chức năng dời buổi nếu cần đổi lịch.","Lớp đã có lịch");
            return;
        }
        var plan=T(row,"lichHocDuKien");
        var parsed=ClassPlanRules.Parse(plan);
        var firstDay=D(row,"ngayKhaiGiangDuKien",DateTime.MinValue);
        var days=Enumerable.Range(0,7).Where(i=>ClassPlanRules.ContainsDay(ApiDataService.Normalize(plan),i)).ToArray();
        var start=firstDay.Date.Add(parsed.Start??TimeSpan.Zero);
        if(parsed.Start is null||start<=BusinessTime.Now||!days.Contains((int)start.DayOfWeek)||
            start.AddHours(-48)<D(row,"dongDangKyLuc",DateTime.MinValue))
        {
            MessageBox.Show(owner,"Mở Sửa lớp để cập nhật ngày, ca học hoặc hạn đăng ký. Buổi đầu phải ở tương lai và cách hạn đóng đăng ký ít nhất 48 giờ.","Cần cập nhật lịch dự kiến");
            return;
        }
        var course=await api.Read("api/quan-ly/khoa-hoc/"+ApiDataService.Id(row,"khoaHocId"));
        var rooms=(await data.All("api/quan-ly/phong-hoc")).Where(x=>T(x,"trangThai")=="HOAT_DONG").ToList();
        var teachers=(await data.All("api/quan-ly/giao-vien")).Where(x=>T(x,"trangThai")=="HOAT_DONG").ToList();
        var content=(await data.All("api/quan-ly/noi-dung-buoi")).Where(x=>ApiDataService.Id(x,"khoaHocId")==ApiDataService.Id(course)).OrderBy(x=>ApiDataService.Id(x,"thuTu")).FirstOrDefault();
        using var form=new InputForm("Xếp lịch lớp học"){CompactColumns=true};
        form.AddSection("1. Lịch đã lưu ở lớp");
        var duration=(int)ApiDataService.Id(content,"soTiet")*(int)ApiDataService.Id(course,"phutMoiTiet");
        var finish=start.AddMinutes(duration);
        form.Add("LichHocDuKien","Lịch dự kiến",$"{start:dd/MM/yyyy} · {plan}",readOnly:true,multiline:true);
        if(duration<=0||finish.Date!=start.Date)
            throw new InvalidOperationException("Thời lượng khóa chưa hợp lệ. Kiểm tra nội dung buổi học trước khi xếp lịch.");
        var availabilityPath=FormattableString.Invariant(
            $"api/lich-hoc/tra-ca?batDau={start:yyyy-MM-ddTHH:mm:ss}&ketThuc={finish:yyyy-MM-ddTHH:mm:ss}");
        var availability=await api.Read(availabilityPath);
        var conflicts=ArtApiClient.Get(availability,"buoiHoc").EnumerateArray()
            .Where(x=>ArtApiClient.Get(x,"trungCa").GetBoolean()).ToArray();
        var busyRooms=conflicts.Select(x=>ApiDataService.Id(x,"phongHocId")).ToHashSet();
        var busySessions=conflicts.Select(x=>ApiDataService.Id(x)).ToHashSet();
        var busyTeachers=ArtApiClient.Get(availability,"giangDay").EnumerateArray()
            .Where(x=>busySessions.Contains(ApiDataService.Id(x,"buoiHocId")))
            .Select(x=>ApiDataService.Id(x,"giaoVienId")).ToHashSet();
        var closed=ArtApiClient.Get(availability,"ngayNghi").GetBoolean();
        var capacity=ApiDataService.Id(row,"siSoToiDa");
        var freeRooms=rooms.Where(x=>!closed&&!busyRooms.Contains(ApiDataService.Id(x))&&ApiDataService.Id(x,"sucChua")>=capacity).ToArray();
        var freeTeachers=teachers.Where(x=>!closed&&!busyTeachers.Contains(ApiDataService.Id(x))).ToArray();
        form.AddAction("Xem phòng trống và lịch giáo viên",()=>{
            using var preview=new ScheduleAvailabilityForm(availability,start,finish,rooms,teachers,capacity,form.Set);
            preview.ShowDialog(form);
            return Task.CompletedTask;
        });
        form.AddSection("2. Chọn phòng và người dạy");
        form.Add("room","Phòng học","",new[]{new Choice("",freeRooms.Length==0?"Không có phòng phù hợp":"Chọn phòng trống")}.Concat(freeRooms.Select(x=>new Choice(T(x,"id"),T(x,"tenPhong")))));
        form.Add("teacher","Giáo viên chính","",new[]{new Choice("",freeTeachers.Length==0?"Không có giáo viên rảnh":"Chọn giáo viên rảnh")}.Concat(freeTeachers.Select(x=>new Choice(T(x,"id"),T(x,"hoTen")))));
        form.Add("assistant","Trợ giảng (bắt buộc với Junior)","0",new[]{new Choice("0","Không có")}.Concat(freeTeachers.Select(x=>new Choice(T(x,"id"),T(x,"hoTen")))));
        form.Add("qualified","Đã xác minh giáo viên đủ chuyên môn",false);
        form.Add("ScheduleNote","Lưu ý","API kiểm tra trùng lịch và ngày nghỉ. "+(api.Role=="QUAN_TRI"?"Khi công bố tuyển sinh, kho giữ họa cụ theo sĩ số tối đa và định mức các buổi. Nếu thiếu, lịch đã lưu và lớp vẫn là bản nháp.":"Tài khoản học vụ tạo lịch. Quản trị công bố lớp."),readOnly:true,multiline:true);
        form.FieldValidation=()=>{
            var errors=new Dictionary<string,string>();
            var room=rooms.FirstOrDefault(x=>T(x,"id")==form.TextValue("room"));
            if(form.TextValue("room")=="")errors["room"]="Hãy chọn phòng học hoạt động.";
            else if(ApiDataService.Id(room,"sucChua")<ApiDataService.Id(row,"siSoToiDa"))errors["room"]="Sức chứa phòng thấp hơn sĩ số tối đa của lớp.";
            if(form.TextValue("teacher")=="")errors["teacher"]="Hãy chọn giáo viên chính.";
            if(T(course,"capDo").Equals("JUNIOR",StringComparison.OrdinalIgnoreCase)&&form.TextValue("assistant")=="0")errors["assistant"]="Lớp Junior bắt buộc có trợ giảng.";
            if(form.TextValue("assistant")==form.TextValue("teacher"))errors["assistant"]="Trợ giảng và giáo viên chính phải là hai người khác nhau.";
            if(!form.Flag("qualified"))errors["qualified"]="Cần xác nhận giáo viên đủ chuyên môn trước khi xếp lịch.";
            if(start<=BusinessTime.Now)errors["LichHocDuKien"]="Buổi đầu phải nằm trong tương lai. Cập nhật trong Sửa lớp.";
            return errors;
        };
        form.ServerErrorFields=ex=>{
            var text=ex.Message.ToLowerInvariant();var key=text.Contains("trợ giảng")?"assistant":text.Contains("giáo viên")?"teacher":text.Contains("phòng")?"room":"LichHocDuKien";
            return new Dictionary<string,string>{{key,ex.Message}};
        };
        foreach(var key in new[]{"room","teacher","assistant","qualified"})form.Changed(key,()=>form.ValidateFields());
        var workflow=new ClassScheduleWorkflow(api,classId);
        form.SaveAsync=async()=>{
            try{await workflow.Save(new{PhongHocId=long.Parse(form.TextValue("room")),GiaoVienId=long.Parse(form.TextValue("teacher")),
                TroGiangId=form.TextValue("assistant")=="0"?(long?)null:long.Parse(form.TextValue("assistant")),BuoiDau=start.ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture),CachTuan=parsed.Weeks,NgayHocTrongTuan=days,PhutNghi=0,XacNhanDuChuyenMon=form.Flag("qualified")},form.SetSaveStatus);}
            finally{if(workflow.ScheduleCreated)foreach(var key in new[]{"room","teacher","assistant","qualified"})form.Enable(key,false);}
        };
        form.ValidateFields();form.ShowDialog(owner);
    }
    internal InputForm CreateForm(List<JsonElement> courses,JsonElement? row,List<JsonElement> sessions)
    {
        var form=new InputForm(row.HasValue?"Cập nhật lớp học":"Thêm lớp học"){CompactColumns=true};
        form.ShowSaveButton=true;JsonElement? savedRecord=row;
        var existing=sessions.Count>0;var first=sessions.OrderBy(x=>T(x,"batDau")).FirstOrDefault();
        DateTime? actual=existing?DateTime.Parse(T(first,"batDau"),CultureInfo.InvariantCulture):null;
        var savedCourse=row.HasValue?T(row.Value,"khoaHocId"):"";
        var available=courses.Where(x=>T(x,"trangThai")=="HOAT_DONG"||T(x,"id")==savedCourse).ToList();
        form.AddSection("1. Thông tin lớp");
        form.Add("KhoaHocId","Khóa học",savedCourse,new[]{new Choice("","Chọn khóa học")}.Concat(available.Select(x=>new Choice(T(x,"id"),T(x,"tenKhoa")))),readOnly:row.HasValue);
        JsonElement Course()=>available.FirstOrDefault(x=>T(x,"id")==form.TextValue("KhoaHocId"));
        int CourseNumber(string key)=>int.TryParse(T(Course(),key),out var value)?value:0;
        int Duration()=>CourseNumber("soTietMoiBuoi")*CourseNumber("phutMoiTiet");
        form.Add("SoBuoiKeHoach","Số buổi theo khóa",(int)N(row,"soBuoiKeHoach",16),readOnly:true);
        form.Add("TenLop","Tên lớp",row.HasValue?T(row.Value,"tenLop"):"");
        form.Add("MaLop","Mã lớp",row.HasValue?T(row.Value,"maLop"):"Tự sinh khi lưu",readOnly:true);
        if(!row.HasValue)form.ShowField("MaLop",false);
        form.AddSection("2. Lịch học dự kiến");
        form.Add("NgayKhaiGiangDuKien","Ngày học đầu tiên",actual??D(row,"ngayKhaiGiangDuKien",BusinessTime.Now.Date.AddDays(21)),readOnly:existing);
        var actualDuration=existing?(int)(DateTime.Parse(T(first,"ketThuc"),CultureInfo.InvariantCulture)-actual!.Value).TotalMinutes:0;
        var slot=new WeeklyScheduleFields(form,"NgayKhaiGiangDuKien",row.HasValue?T(row.Value,"lichHocDuKien"):"",()=>existing?actualDuration:Duration(),existing,actual);
        form.Add("NgayKetThucDuKien","Kết thúc dự kiến",row.HasValue&&T(row.Value,"ngayKetThucDuKien")!=""?D(row,"ngayKetThucDuKien",slot.LastFor((int)form.Number("SoBuoiKeHoach"))):slot.LastFor((int)form.Number("SoBuoiKeHoach")),readOnly:true);
        foreach(var key in new[]{"NgayKhaiGiangDuKien","ThuHoc","ThuHoc2","ThuHoc3","CachTuan","SoBuoiMoiTuan"})form.Changed(key,()=>form.Set("NgayKetThucDuKien",slot.LastFor((int)form.Number("SoBuoiKeHoach"))));
        form.Add("ScheduleNote","Lưu ý về lịch",existing
            ?"Lớp đã có lịch thực tế: không sửa thứ/giờ chỉ bằng mô tả. Muốn đổi phải dời buổi để API kiểm tra phòng, giáo viên và học viên."
            :"Lịch lặp hằng tuần theo các thứ đã chọn. Sau khi lưu lớp nháp, chọn Xếp lịch để bố trí phòng và giáo viên.",readOnly:true,multiline:true);
        var capacityLocked=row.HasValue&&T(row.Value,"trangThai")!="NHAP";
        form.AddSection("3. Tuyển sinh và học phí");
        form.Add("SiSoToiThieu","Sĩ số tối thiểu",(int)N(row,"siSoToiThieu",6),readOnly:capacityLocked);
        form.Add("SiSoToiDa","Sĩ số tối đa",(int)N(row,"siSoToiDa",12),readOnly:capacityLocked);
        var published=row.HasValue&&T(row.Value,"trangThai")!="NHAP";
        form.Add("MoDangKyLuc","Mở đăng ký lúc",D(row,"moDangKyLuc",BusinessTime.Now.Date),readOnly:published);
        form.Add("DongDangKyLuc","Đóng đăng ký lúc",D(row,"dongDangKyLuc",form.Date("NgayKhaiGiangDuKien").AddDays(-3)),readOnly:published);
        form.Add("HocPhiApDung","Học phí áp dụng",N(row,"hocPhiApDung",0));
        var currentState=row.HasValue?T(row.Value,"trangThai"):"NHAP";
        var nextStates=currentState switch {
            "NHAP"=>existing?new[]{"NHAP","DANG_TUYEN_SINH"}:new[]{"NHAP"},
            "DANG_TUYEN_SINH"=>new[]{"DANG_TUYEN_SINH","DANG_HOC"},
            "DANG_HOC"=>new[]{"DANG_HOC","DA_KET_THUC"},_=>new[]{currentState}};
        form.Add("TrangThai","Trạng thái",currentState,nextStates.Select(state=>new Choice(state,state switch {"NHAP"=>"Bản nháp","DANG_TUYEN_SINH"=>"Đang tuyển sinh","DANG_HOC"=>"Đang học","DA_KET_THUC"=>"Đã kết thúc",_=>state})));
        form.FieldValidation=()=>{
            var failures=ClassPlanRules.ValidateClass(T(Course(),"tenKhoa"),CourseNumber("soBuoi"),form.Number("SoBuoiKeHoach"),form.Number("SiSoToiThieu"),form.Number("SiSoToiDa"),form.Number("HocPhiApDung"),form.Date("MoDangKyLuc"),form.Date("DongDangKyLuc"),slot.First);
            foreach(var pair in slot.Validate())failures[pair.Key]=pair.Value;
            if(form.TextValue("KhoaHocId")=="")failures["KhoaHocId"]="Hãy chọn khóa học hoạt động trước.";
            if(string.IsNullOrWhiteSpace(form.TextValue("TenLop")))failures["TenLop"]="Tên lớp không được để trống.";
            else if(form.TextValue("TenLop").Length>200)failures["TenLop"]="Tên lớp không quá 200 ký tự.";
            if(!row.HasValue&&slot.First<=BusinessTime.Now)failures["NgayKhaiGiangDuKien"]="Buổi đầu của lớp mới phải nằm trong tương lai.";
            var state=form.TextValue("TrangThai");
            if(state!="NHAP"&&api.Role!="QUAN_TRI")failures["TrangThai"]="Chỉ quản trị được duyệt trạng thái công bố/vận hành lớp. Học vụ lưu bản nháp.";
            if(!existing&&state!="NHAP")failures["TrangThai"]="Hãy lưu bản nháp và Xếp lịch đủ các buổi trước khi công bố.";
            return failures;
        };
        form.ServerErrorFields=ex=>{
            var text=ex.Message.ToLowerInvariant();var key=text.Contains("48 giờ")?"DongDangKyLuc":text.Contains("sĩ số")||text.Contains("sức chứa")?"SiSoToiDa":text.Contains("học phí")?"HocPhiApDung":text.Contains("khóa")&&!text.Contains("lớp")?"KhoaHocId":text.Contains("đề cương")||text.Contains("số buổi")?"SoBuoiKeHoach":text.Contains("lịch")||text.Contains("giáo viên")||text.Contains("công bố")||text.Contains("khai giảng")||text.Contains("duyệt")?"TrangThai":"";
            return key==""?new Dictionary<string,string>():new Dictionary<string,string>{{key,ex.Message}};
        };
        void CourseChanged(){var count=CourseNumber("soBuoi");if(count>0)form.Set("SoBuoiKeHoach",count);form.Set("HocPhiApDung",decimal.TryParse(T(Course(),"hocPhi"),NumberStyles.Number,CultureInfo.InvariantCulture,out var fee)?fee:0m);slot.Refresh();form.Set("NgayKetThucDuKien",slot.LastFor(count));}
        form.Changed("KhoaHocId",CourseChanged);
        foreach(var key in new[]{"MaLop","TenLop","SoBuoiKeHoach","SiSoToiThieu","SiSoToiDa","HocPhiApDung","MoDangKyLuc","DongDangKyLuc","TrangThai"})form.Changed(key,()=>form.ValidateFields());
        form.SaveAsync=async()=>{
            var body=new{KhoaHocId=long.Parse(form.TextValue("KhoaHocId")),TenLop=form.TextValue("TenLop"),
                NgayKhaiGiangDuKien=form.Date("NgayKhaiGiangDuKien").ToString("yyyy-MM-dd"),SoBuoiKeHoach=(int)form.Number("SoBuoiKeHoach"),LichHocDuKien=existing?T(row!.Value,"lichHocDuKien"):slot.Description,
                SiSoToiThieu=(int)form.Number("SiSoToiThieu"),SiSoToiDa=(int)form.Number("SiSoToiDa"),HocPhiApDung=form.Number("HocPhiApDung"),
                MoDangKyLuc=form.Date("MoDangKyLuc").ToString("yyyy-MM-ddTHH:mm:ss"),DongDangKyLuc=form.Date("DongDangKyLuc").ToString("yyyy-MM-ddTHH:mm:ss"),TrangThai=form.TextValue("TrangThai")};
            savedRecord=savedRecord.HasValue?await api.Put("api/quan-ly/lop-hoc/"+ApiDataService.Id(savedRecord.Value),body,T(savedRecord.Value,"rowVersion"))
                :await api.Post("api/quan-ly/lop-hoc",body);
            form.Set("MaLop",T(savedRecord.Value,"maLop"));
            form.ShowField("MaLop",true);
            form.Enable("MaLop",false);form.Enable("KhoaHocId",false);
        };
        form.ValidateFields();return form;
    }
}
