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
        var data=new ApiDataService(api);var course=await api.Read("api/quan-ly/khoa-hoc/"+ApiDataService.Id(row,"khoaHocId"));
        var rooms=(await data.All("api/quan-ly/phong-hoc")).Where(x=>T(x,"trangThai")=="HOAT_DONG").ToList();
        var teachers=(await data.All("api/quan-ly/giao-vien")).Where(x=>T(x,"trangThai")=="HOAT_DONG").ToList();
        var content=(await data.All("api/quan-ly/noi-dung-buoi")).Where(x=>ApiDataService.Id(x,"khoaHocId")==ApiDataService.Id(course)).OrderBy(x=>ApiDataService.Id(x,"thuTu")).FirstOrDefault();
        using var form=new InputForm("Xếp lịch theo thứ và ca học");
        form.Add("room","Phòng học","",new[]{new Choice("","Chọn phòng")}.Concat(rooms.Select(x=>new Choice(T(x,"id"),T(x,"tenPhong")))));
        form.Add("teacher","Giáo viên chính","",new[]{new Choice("","Chọn giáo viên")}.Concat(teachers.Select(x=>new Choice(T(x,"id"),T(x,"hoTen")))));
        form.Add("assistant","Trợ giảng (bắt buộc với Junior)","0",new[]{new Choice("0","Không có")}.Concat(teachers.Select(x=>new Choice(T(x,"id"),T(x,"hoTen")))));
        form.Add("NgayKhaiGiangDuKien","Ngày buổi học đầu tiên",D(row,"ngayKhaiGiangDuKien",BusinessTime.Now.Date.AddDays(21)));
        form.Add("PhutNghi","Phút nghỉ trong mỗi buổi",0);
        var duration=(int)ApiDataService.Id(content,"soTiet")*(int)ApiDataService.Id(course,"phutMoiTiet");
        var slot=new WeeklyScheduleFields(form,"NgayKhaiGiangDuKien",T(row,"lichHocDuKien"),()=>duration+(int)form.Number("PhutNghi"));
        form.Add("qualified","Đã xác minh giáo viên đủ chuyên môn",false);
        form.Add("ScheduleNote","Lưu ý","API sẽ kiểm tra trùng phòng, giáo viên, học viên và ngày nghỉ. Ngày nghỉ có thể khiến buổi thực tế được chuyển sang tuần kế tiếp. "+(api.Role=="QUAN_TRI"?"Xác nhận thành công sẽ tự chuyển lớp sang Đang tuyển sinh.":"Tài khoản học vụ tạo lịch; quản trị công bố lớp theo phân quyền API."),readOnly:true,multiline:true);
        form.FieldValidation=()=>{
            var errors=slot.Validate();
            var room=rooms.FirstOrDefault(x=>T(x,"id")==form.TextValue("room"));
            if(form.TextValue("room")=="")errors["room"]="Hãy chọn phòng học hoạt động.";
            else if(ApiDataService.Id(room,"sucChua")<ApiDataService.Id(row,"siSoToiDa"))errors["room"]="Sức chứa phòng thấp hơn sĩ số tối đa của lớp.";
            if(form.TextValue("teacher")=="")errors["teacher"]="Hãy chọn giáo viên chính.";
            if(T(course,"capDo").Equals("JUNIOR",StringComparison.OrdinalIgnoreCase)&&form.TextValue("assistant")=="0")errors["assistant"]="Lớp Junior bắt buộc có trợ giảng.";
            if(form.TextValue("assistant")==form.TextValue("teacher"))errors["assistant"]="Trợ giảng và giáo viên chính phải là hai người khác nhau.";
            if(!form.Flag("qualified"))errors["qualified"]="Cần xác nhận giáo viên đủ chuyên môn trước khi xếp lịch.";
            if(slot.First<=BusinessTime.Now)errors["NgayKhaiGiangDuKien"]="Buổi đầu phải nằm trong tương lai.";
            if(form.Number("PhutNghi") is <0 or >120)errors["PhutNghi"]="Phút nghỉ phải từ 0 đến 120.";
            if(T(row,"trangThai")!="NHAP")errors["LichHocDuKien"]="Chỉ sinh lịch lần đầu cho lớp nháp chưa có buổi học. Lớp đã có lịch cần dời buổi.";
            if(slot.First.AddHours(-48)<D(row,"dongDangKyLuc",DateTime.MinValue))errors["NgayKhaiGiangDuKien"]="Ngày buổi đầu quá gần hạn đóng đăng ký. Cần cách tối thiểu 48 giờ để công bố lớp.";
            return errors;
        };
        form.ServerErrorFields=ex=>{
            var text=ex.Message.ToLowerInvariant();var key=text.Contains("trợ giảng")?"assistant":text.Contains("giáo viên")?"teacher":text.Contains("phòng")?"room":text.Contains("ngày")||text.Contains("học viên")?"NgayKhaiGiangDuKien":"LichHocDuKien";
            return new Dictionary<string,string>{{key,ex.Message}};
        };
        foreach(var key in new[]{"room","teacher","assistant","qualified"})form.Changed(key,()=>form.ValidateFields());
        form.Changed("PhutNghi",slot.Refresh);
        var workflow=new ClassScheduleWorkflow(api,ApiDataService.Id(row));
        form.SaveAsync=async()=>{
            try{await workflow.Save(new{PhongHocId=long.Parse(form.TextValue("room")),GiaoVienId=long.Parse(form.TextValue("teacher")),
                TroGiangId=form.TextValue("assistant")=="0"?(long?)null:long.Parse(form.TextValue("assistant")),BuoiDau=slot.First.ToString("yyyy-MM-ddTHH:mm:ss"),CachTuan=slot.Weeks,PhutNghi=(int)form.Number("PhutNghi"),XacNhanDuChuyenMon=form.Flag("qualified")},form.SetSaveStatus);}
            finally{if(workflow.ScheduleCreated)foreach(var key in new[]{"room","teacher","assistant","NgayKhaiGiangDuKien","PhutNghi","ThuHoc","CaHoc","GioBatDau","CachTuan","qualified"})form.Enable(key,false);}
        };
        form.ValidateFields();form.ShowDialog(owner);
    }
    internal InputForm CreateForm(List<JsonElement> courses,JsonElement? row,List<JsonElement> sessions)
    {
        var form=new InputForm(row.HasValue?"Cập nhật lớp học":"Thêm lớp học");
        form.ShowSaveButton=true;JsonElement? savedRecord=row;
        var existing=sessions.Count>0;var first=sessions.OrderBy(x=>T(x,"batDau")).FirstOrDefault();
        DateTime? actual=existing?DateTime.Parse(T(first,"batDau"),CultureInfo.InvariantCulture):null;
        var savedCourse=row.HasValue?T(row.Value,"khoaHocId"):"";
        var available=courses.Where(x=>T(x,"trangThai")=="HOAT_DONG"||T(x,"id")==savedCourse).ToList();
        form.Add("KhoaHocId","Khóa học",savedCourse,new[]{new Choice("","Chọn khóa học")}.Concat(available.Select(x=>new Choice(T(x,"id"),T(x,"tenKhoa")))),readOnly:row.HasValue);
        JsonElement Course()=>available.FirstOrDefault(x=>T(x,"id")==form.TextValue("KhoaHocId"));
        int CourseNumber(string key)=>int.TryParse(T(Course(),key),out var value)?value:0;
        int Duration()=>CourseNumber("soTietMoiBuoi")*CourseNumber("phutMoiTiet");
        form.Add("MaLop","Mã lớp",row.HasValue?T(row.Value,"maLop"):"",readOnly:row.HasValue);
        form.Add("TenLop","Tên lớp",row.HasValue?T(row.Value,"tenLop"):"");
        form.Add("NgayKhaiGiangDuKien","Ngày buổi học đầu tiên",actual??D(row,"ngayKhaiGiangDuKien",BusinessTime.Now.Date.AddDays(21)),readOnly:existing);
        form.Add("SoBuoiKeHoach","Số buổi kế hoạch",(int)N(row,"soBuoiKeHoach",16));
        var actualDuration=existing?(int)(DateTime.Parse(T(first,"ketThuc"),CultureInfo.InvariantCulture)-actual!.Value).TotalMinutes:0;
        var slot=new WeeklyScheduleFields(form,"NgayKhaiGiangDuKien",row.HasValue?T(row.Value,"lichHocDuKien"):"",()=>existing?actualDuration:Duration(),existing,actual);
        form.Add("ScheduleNote","Lưu ý về lịch",existing
            ?"Lớp đã có lịch thực tế: không sửa thứ/giờ chỉ bằng mô tả. Muốn đổi phải dời buổi để API kiểm tra phòng, giáo viên và học viên."
            :"Đây là khung giờ dự kiến. Sau khi lưu lớp nháp, dùng Xếp lịch để chọn phòng và giáo viên, tạo các buổi thực tế. Chỉ công bố sau khi đủ lịch.",readOnly:true,multiline:true);
        form.Add("SiSoToiThieu","Sĩ số tối thiểu",(int)N(row,"siSoToiThieu",6));
        form.Add("SiSoToiDa","Sĩ số tối đa",(int)N(row,"siSoToiDa",12));
        form.Add("HocPhiApDung","Học phí áp dụng",N(row,"hocPhiApDung",0));
        form.Add("MoDangKyLuc","Mở đăng ký lúc",D(row,"moDangKyLuc",BusinessTime.Now.Date));
        form.Add("DongDangKyLuc","Đóng đăng ký lúc",D(row,"dongDangKyLuc",form.Date("NgayKhaiGiangDuKien").AddDays(-3)));
        form.Add("TrangThai","Trạng thái",row.HasValue?T(row.Value,"trangThai"):"NHAP",[new("NHAP","Bản nháp"),new("DANG_TUYEN_SINH","Đang tuyển sinh"),new("DANG_HOC","Đang học"),new("DA_KET_THUC","Đã kết thúc")]);
        form.FieldValidation=()=>{
            var failures=ClassPlanRules.ValidateClass(T(Course(),"tenKhoa"),CourseNumber("soBuoi"),form.Number("SoBuoiKeHoach"),form.Number("SiSoToiThieu"),form.Number("SiSoToiDa"),form.Number("HocPhiApDung"),form.Date("MoDangKyLuc"),form.Date("DongDangKyLuc"),slot.First);
            foreach(var pair in slot.Validate())failures[pair.Key]=pair.Value;
            if(form.TextValue("KhoaHocId")=="")failures["KhoaHocId"]="Hãy chọn khóa học hoạt động trước.";
            if(string.IsNullOrWhiteSpace(form.TextValue("MaLop")))failures["MaLop"]="Mã lớp không được để trống.";
            else if(form.TextValue("MaLop").Length>30)failures["MaLop"]="Mã lớp không quá 30 ký tự.";
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
        void CourseChanged(){form.Set("SoBuoiKeHoach",CourseNumber("soBuoi"));form.Set("HocPhiApDung",decimal.TryParse(T(Course(),"hocPhi"),NumberStyles.Number,CultureInfo.InvariantCulture,out var fee)?fee:0m);slot.Refresh();}
        form.Changed("KhoaHocId",CourseChanged);
        foreach(var key in new[]{"MaLop","TenLop","SoBuoiKeHoach","SiSoToiThieu","SiSoToiDa","HocPhiApDung","MoDangKyLuc","DongDangKyLuc","TrangThai"})form.Changed(key,()=>form.ValidateFields());
        form.SaveAsync=async()=>{
            var body=new{KhoaHocId=long.Parse(form.TextValue("KhoaHocId")),MaLop=form.TextValue("MaLop"),TenLop=form.TextValue("TenLop"),
                NgayKhaiGiangDuKien=form.Date("NgayKhaiGiangDuKien").ToString("yyyy-MM-dd"),SoBuoiKeHoach=(int)form.Number("SoBuoiKeHoach"),LichHocDuKien=existing?T(row!.Value,"lichHocDuKien"):slot.Description,
                SiSoToiThieu=(int)form.Number("SiSoToiThieu"),SiSoToiDa=(int)form.Number("SiSoToiDa"),HocPhiApDung=form.Number("HocPhiApDung"),
                MoDangKyLuc=form.Date("MoDangKyLuc").ToString("yyyy-MM-ddTHH:mm:ss"),DongDangKyLuc=form.Date("DongDangKyLuc").ToString("yyyy-MM-ddTHH:mm:ss"),TrangThai=form.TextValue("TrangThai")};
            savedRecord=savedRecord.HasValue?await api.Put("api/quan-ly/lop-hoc/"+ApiDataService.Id(savedRecord.Value),body,T(savedRecord.Value,"rowVersion"))
                :await api.Post("api/quan-ly/lop-hoc",body);
            form.Enable("MaLop",false);form.Enable("KhoaHocId",false);
        };
        form.ValidateFields();return form;
    }
}
