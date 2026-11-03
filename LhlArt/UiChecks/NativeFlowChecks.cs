using System.Net;
using System.Text.Json;
using LhlArt.Controllers;
using LhlArt.Networking;
using LhlArt.Views;

// Component tests exercise the real modal forms against an in-memory HTTP boundary.
// They neither authenticate a user nor create records in the running center database.
internal static class NativeFlowChecks
{
    public static void Run(Action<bool,string> check)
    {
        using var handler=new FlowHandler();using var api=new ArtApiClient(handler,"http://native-flow.invalid");
        api.Login("fixture","fixture-only").GetAwaiter().GetResult();
        using var owner=new Form {ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,Location=new Point(-6000,-6000)};
        owner.Show();Application.DoEvents();
        var people=new ApiPeopleController(api);
        Modal(()=>people.Member(owner,null),async f=>{
            f.Set("HoTen","Nguyễn Kiểm Thử");f.Set("DienThoai","0900000000");f.Set("Email","native@example.invalid");
            await f.SaveAsync!();check(handler.LastMethod=="POST"&&ArtApiClient.Text(handler.LastBody,"hoTen")=="Nguyễn Kiểm Thử","Form người liên hệ tạo đúng payload Unicode qua API");
            check(ArtApiClient.Text(handler.LastBody,"ngaySinh")=="","Không bật ngày sinh thì gửi null, không tự bịa ngày");
        });
        Modal(()=>people.Member(owner,FlowHandler.Member),async f=>{
            f.Set("HoTen","Đã sửa từ WinForms");await f.SaveAsync!();
            check(handler.LastMethod=="PUT"&&handler.LastIfMatch=="AQ=="&&ArtApiClient.Text(handler.LastBody,"hoTen")=="Đã sửa từ WinForms","Form sửa người liên hệ gửi PUT kèm RowVersion đúng");
        });
        Modal(()=>people.Student(owner,null),async f=>{
            check(f.TextValue("HoTen")==""&&f.TextValue("LienHeKhanCapTen")=="Người Đại Diện Thử"&&f.TextValue("LienHeKhanCapSDT")=="0900000000","Form học viên quan hệ Cha chỉ tự điền người liên hệ và số khẩn cấp");
            f.Set("QuanHe","TU_BAN_THAN");check(f.TextValue("HoTen")=="Người Đại Diện Thử"&&f.Date("NgaySinh").Date==new DateTime(2000,1,2),"Chọn Bản thân tự điền họ tên và ngày sinh từ người liên hệ");
            f.Set("LienHeKhanCapTen","Người liên hệ khác");f.Set("LienHeKhanCapSDT","0911111111");
            check(f.TextValue("LienHeKhanCapTen")=="Người liên hệ khác","Cho phép sửa thông tin liên hệ đã tự điền");
            handler.RejectLink=true;var before=handler.StudentCreates;
            try{await f.SaveAsync!();throw new Exception("Expected relation failure");}catch(InvalidOperationException ex){check(ex.Message.Contains("đã lưu")&&ex.Message.Contains("không tạo lại"),"Lỗi liên kết sau tạo hồ sơ thông báo đúng hồ sơ đã lưu");}
            handler.RejectLink=false;await f.SaveAsync!();
            check(handler.StudentCreates==before+1&&handler.Links==1,"Thử liên kết lại không tạo thêm hồ sơ học viên trùng");
        });
        Modal(()=>people.Representative(owner,null),async f=>{
            check(f.TextValue("ThanhVienId")=="1"&&f.TextValue("HocVienId")=="2","Form người đại diện tải được danh sách tên hai phía");
            f.Set("QuanHe","ME");await f.SaveAsync!();check(ArtApiClient.Text(handler.LastBody,"quanHe")=="ME"&&ArtApiClient.Text(handler.LastBody,"quyenDaiDien")=="DAY_DU","Form liên kết gửi đúng quan hệ và quyền đã chọn");
        });
        var catalog=new ApiCatalogController(api);
        foreach(var (route,title,codeKey,nameKey) in new[]{("giao-vien","Giáo viên","MaGiaoVien","HoTen"),("phong-hoc","Phòng học","MaPhong","TenPhong"),("hoa-cu","Họa cụ","MaHoaCu","TenHoaCu"),("nha-cung-cap","Nhà cung cấp","MaNCC","TenNCC")}){
            Modal(()=>catalog.Edit(owner,"api/quan-ly/"+route,title,null),async f=>{
                f.Set(codeKey,"NATIVE_TEST");f.Set(nameKey,"Kiểm thử "+title);f.ValidateAndSave!();await f.SaveAsync!();
                check(handler.LastMethod=="POST"&&ArtApiClient.Text(handler.LastBody,nameKey)=="Kiểm thử "+title,"Form thêm "+title+" hoạt động và giữ Unicode");
            });
        }
        var promotion=new ApiPromotionController(api);
        Modal(()=>catalog.Edit(owner,"api/quan-ly/khoa-hoc","Khóa học",null),async f=>{
            check(f.FieldValidation!().ContainsKey("TienToMa")&&!f.IsFieldEnabled("MaKhoa"),"Khóa mới yêu cầu tiền tố, mã đầy đủ chỉ đọc");
            f.Set("TienToMa","JR");check(f.FieldValidation!().ContainsKey("TienToMa"),"Tiền tố khóa 2 chữ chưa hợp lệ");
            f.Set("TienToMa","jun");f.Set("TenKhoa","Junior mới");f.Set("CapDo","JUNIOR");f.Set("MucTieu","Khám phá màu sắc");
            check(f.ValidateFields(),"Tiền tố 3 chữ và thông số khóa hợp lệ");f.ValidateAndSave!();await f.SaveAsync!();
            check(ArtApiClient.Text(handler.LastBody,"tienToMa")=="JUN"&&!ArtApiClient.TryGet(handler.LastBody,"MaKhoa",out _),"WinForms chỉ gửi tiền tố JUN, không tự tạo mã khóa đầy đủ");
        });
        Modal(()=>promotion.Edit(owner,null),async f=>{
            f.Set("MaKhuyenMai","native10");f.Set("TenKhuyenMai","Ưu đãi thử");f.Set("TuDongApDung",true);
            try{await f.SaveAsync!();throw new Exception("Expected course requirement");}catch(InvalidOperationException ex){check(ex.Message.Contains("khóa học cụ thể"),"Form ưu đãi tự động bắt chọn khóa, không dùng tùy chọn mọi khóa");}
            f.Set("KhoaApDungId","3");f.Set("GiaTriGiam",101m);
            try{await f.SaveAsync!();throw new Exception("Expected percentage bound");}catch(InvalidOperationException ex){check(ex.Message.Contains("100"),"Form mã giảm chặn phần trăm vượt 100");}
            f.Set("GiaTriGiam",10m);await f.SaveAsync!();
            check(ArtApiClient.Text(handler.LastBody,"maKhuyenMai")=="NATIVE10"&&ArtApiClient.Text(handler.LastBody,"tuDongApDung")=="True","Form ưu đãi chuẩn hóa mã và gửi chế độ tự động độc lập");
        });
        Modal(()=>new ApiEnrollmentController(api).Create(owner),f=>{
            check(f.TextValue("HocVienId")=="2"&&f.TextValue("NguoiDangKyId")=="1","Form đăng ký tại quầy tự lọc đúng người đại diện của học viên");
            check(f.TextValue("LopHocId")=="4"&&f.TextValue("HocPhi").Contains("3.600.000"),"Form ghi danh tải lớp còn chỗ và học phí theo locale Việt Nam");
            return Task.CompletedTask;
        });
        check(handler.ExternalRequests==0,"Toàn bộ form nghiệp vụ dùng HTTP giả lập, không gửi tiền/email và không sửa DB chính");
    }
    static void Modal(Func<Task> open,Func<InputForm,Task> inspect)
    {
        Exception? failure=null;var handled=false;
        using var timer=new System.Windows.Forms.Timer {Interval=30};
        timer.Tick+=async(_,_)=>{
            var f=Application.OpenForms.OfType<InputForm>().FirstOrDefault();if(f is null||handled)return;handled=true;timer.Stop();
            f.Location=new Point(-6000,-6000);f.ShowInTaskbar=false;
            try{await inspect(f);}catch(Exception ex){failure=ex;}finally{f.Close();}
        };
        timer.Start();var task=open();var until=DateTime.UtcNow.AddSeconds(10);
        while(!task.IsCompleted&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(5);}
        if(!task.IsCompleted)throw new Exception("Native form test timed out");task.GetAwaiter().GetResult();
        if(failure is not null)throw failure;if(!handled)throw new Exception("No native modal was opened");
    }
    sealed class FlowHandler:HttpMessageHandler
    {
        public string LastMethod="",LastIfMatch="";public JsonElement LastBody;public int StudentCreates,Links,ExternalRequests;public bool RejectLink;
        public static JsonElement Member=>JsonSerializer.SerializeToElement(new{id=1,maThanhVien="NATIVE_TV",hoTen="Người Đại Diện Thử",ngaySinh="2000-01-02",dienThoai="0900000000",trangThai="HOAT_DONG",rowVersion="AQ=="});
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req,CancellationToken ct)
        {
            if(req.RequestUri!.Host!="native-flow.invalid"){ExternalRequests++;throw new Exception("Unexpected external request");}
            var path=req.RequestUri.AbsolutePath;object body=new{};
            if(path.EndsWith("dang-nhap"))body=new{vaiTroChinh="QUAN_TRI",accessToken="fixture-token",tenDangNhap="fixture",quyen=255};
            else if(req.Method==HttpMethod.Get){
                object[] rows=path.EndsWith("thanh-vien")?[Member]:path.EndsWith("hoc-vien")?[new{id=2,maHocVien="NATIVE_HV",hoTen="Học Viên Thử",ngaySinh="2000-01-02",trangThai="HOAT_DONG"}]:path.EndsWith("dai-dien")?[new{id=10,hocVienId=2,thanhVienId=1,hieuLucTu="2020-01-01T00:00:00",hieuLucDen=(string?)null,xacNhanLuc="2020-01-01T00:00:00",quyenDaiDien="DAY_DU"}]:path.EndsWith("loai-khoa-hoc")?[new{id=11,tenLoai="Chính khóa"}]:path.EndsWith("khoa-hoc")?[new{id=3,tenKhoa="Foundation thử",trangThai="HOAT_DONG"}]:path.EndsWith("lop-hoc")?[new{id=4,tenLop="Lớp thử",lichHocDuKien="Thứ Bảy 09:00",soChoDaGiu=1,siSoToiDa=10,hocPhiApDung=3600000m}]:[];
                body=new{tongSo=rows.Length,duLieu=rows};
            }
            else {
                LastMethod=req.Method.Method;LastIfMatch=req.Headers.TryGetValues("If-Match",out var vals)?vals.Single():"";
                LastBody=JsonDocument.Parse(await req.Content!.ReadAsStringAsync(ct)).RootElement.Clone();
                if(path.EndsWith("hoc-vien")&&req.Method==HttpMethod.Post)StudentCreates++;
                if(path.EndsWith("dai-dien")){if(RejectLink)return new HttpResponseMessage(HttpStatusCode.Conflict){Content=new StringContent("{\"title\":\"Fixture liên kết lỗi\"}")};Links++;}
                body=new{id=path.EndsWith("hoc-vien")?20:2,rowVersion="Ag=="};
            }
            return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(body))};
        }
    }
}
