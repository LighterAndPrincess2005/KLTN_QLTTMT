using System.Globalization;
using System.Net;
using System.Text.Json;
using LhlArt.Controllers;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;
using LhlArt.Views.Shared;

internal static class Program
{
    static int passed;
    static void Check(bool value,string text){if(!value)throw new Exception(text);Console.WriteLine($"PASS {++passed}: {text}");}
    static JsonElement Json(object value)=>JsonSerializer.SerializeToElement(value);
    [STAThread]
    static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture=CultureInfo.GetCultureInfo("vi-VN");Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);Application.EnableVisualStyles();
        using var handler=new Handler();using var api=new ArtApiClient(handler,"http://ui-check.invalid");api.Login("admin","Test2026!").GetAwaiter().GetResult();
        var course=Json(new{id=1,tenKhoa="Foundation 1",soBuoi=16,soTietMoiBuoi=3,phutMoiTiet=30,hocPhi=3600000.00m,trangThai="HOAT_DONG"});
        var start=ClassPlanRules.NextDay(BusinessTime.Now.AddDays(30),6);
        using(var form=new ApiClassController(api).CreateForm([course],null,[])){
            form.Set("KhoaHocId","1");Check(form.Number("SoBuoiKeHoach")==16,"Chọn khóa tự điền 16 buổi");Check(form.Number("HocPhiApDung")==3600000m,"Học phí không bị sai dấu thập phân theo locale vi-VN");
            form.Set("MaLop","UI_TEST");form.Set("TenLop","Lớp thử giao diện");form.Set("NgayKhaiGiangDuKien",start);form.Set("ThuHoc","6");form.Set("GioBatDau",new TimeSpan(9,0,0));form.Set("MoDangKyLuc",BusinessTime.Now.Date);form.Set("DongDangKyLuc",start.AddDays(-3));
            Check(form.ValidateFields(),"Thứ Bảy ca sáng 09:00–10:30 và thông số hợp lệ");
            form.Set("SoBuoiKeHoach",25);Check(!form.ValidateFields()&&form.FieldError("SoBuoiKeHoach").Contains("25")&&form.FieldError("SoBuoiKeHoach").Contains("16")&&form.FieldError("SoBuoiKeHoach").Contains("Foundation 1"),"Hiện lỗi đỏ ngay dưới ô 25 buổi, nêu khóa và 16 buổi cần nhập");
            if(args.Length>0){form.Size=new Size(800,1050);form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-3000,-3000);form.ShowInTaskbar=false;form.Show();Application.DoEvents();((Button)form.AcceptButton!).PerformClick();Check(handler.Writes==0&&!form.IsDisposed&&form.Number("SoBuoiKeHoach")==25,"Bấm Xác nhận khi sai: không gửi API, không đóng form và giữ nội dung");using var screenshot=new Bitmap(form.Width,form.Height);form.DrawToBitmap(screenshot,new Rectangle(Point.Empty,form.Size));screenshot.Save(args[0]);form.Hide();}
            form.Set("SoBuoiKeHoach",16);Check(form.FieldError("SoBuoiKeHoach")=="","Sửa về 16: cảnh báo biến mất ngay");
            form.Set("CaHoc","CHIEU");Check(form.Time("GioBatDau")==new TimeSpan(14,0,0)&&form.TextValue("LichHocDuKien").Contains("14:00"),"Chọn ca chiều cập nhật giờ và mô tả lịch");
            form.Set("GioBatDau",new TimeSpan(9,0,0));Check(form.FieldError("GioBatDau")!="","Cảnh báo giờ sáng không thuộc ca chiều");form.Set("CaHoc","SANG");
            form.Set("SiSoToiDa",2);Check(form.FieldError("SiSoToiDa")!="","Cảnh báo sĩ số tối đa nhỏ hơn tối thiểu");form.Set("SiSoToiDa",12);
            form.Set("HocPhiApDung",-1m);Check(form.FieldError("HocPhiApDung")!="","Cảnh báo học phí âm");form.Set("HocPhiApDung",3600000m);
            form.Set("DongDangKyLuc",start);Check(form.FieldError("DongDangKyLuc").Contains("48"),"Cảnh báo đóng đăng ký quá sát ngày học");form.Set("DongDangKyLuc",start.AddDays(-3));
            form.Set("ThuHoc","1");Check(form.Date("NgayKhaiGiangDuKien").DayOfWeek==DayOfWeek.Monday,"Chọn Thứ Hai tự căn ngày khai giảng kế tiếp");
            form.Set("CachTuan",0);Check(form.FieldError("CachTuan")!="","Cảnh báo khoảng tuần không hợp lệ");form.Set("CachTuan",1);
            form.Set("GioBatDau",new TimeSpan(23,30,0));Check(form.FieldError("GioBatDau")!="","Không cho buổi học vượt qua ngày mới");
            form.Set("GioBatDau",new TimeSpan(9,0,0));form.Set("CachTuan",2);Check(form.ValidateFields(),"Sửa hết lỗi: biểu mẫu hợp lệ trước khi lưu");
            handler.AllowSave=true;form.SaveAsync!().GetAwaiter().GetResult();
            var savedPayload=handler.Saved!.Value;Check(ArtApiClient.Text(savedPayload,"lichHocDuKien").Contains("Mỗi 2 tuần")&&ArtApiClient.Text(savedPayload,"soBuoiKeHoach")=="16"&&!ArtApiClient.TryGet(savedPayload,"ThuHoc",out _),"Payload gửi API giữ đúng 16 buổi, lịch đã chọn và không thêm cột API không hỗ trợ");
        }
        var saved=Json(new{id=7,khoaHocId=1,maLop="CLASS",tenLop="Lớp đã có lịch",ngayKhaiGiangDuKien=start.ToString("yyyy-MM-dd"),soBuoiKeHoach=16,lichHocDuKien="Thứ Bảy · 09:00–10:30 · Hàng tuần",siSoToiThieu=6,siSoToiDa=12,hocPhiApDung=3600000m,moDangKyLuc=BusinessTime.Now.Date.ToString("s"),dongDangKyLuc=start.AddDays(-3).ToString("s"),trangThai="NHAP",rowVersion="AQ=="});
        using(var form=new ApiClassController(api).CreateForm([course],saved,[Json(new{lopHocId=7,batDau=start.AddHours(9).ToString("s"),ketThuc=start.AddHours(10.5).ToString("s")})])){
            Check(!form.IsFieldEnabled("ThuHoc")&&!form.IsFieldEnabled("GioBatDau")&&!form.IsFieldEnabled("NgayKhaiGiangDuKien")&&form.TextValue("LichHocDuKien").Contains("09:00"),"Lớp có lịch lấy giờ thực tế và khóa sửa mô tả thứ/giờ riêng lẻ");
        }
        Check(handler.Writes==1,"Chỉ một lần lưu hợp lệ vào API giả lập, không đụng database thật");
        using(var form=new ApiClassController(api).CreateForm([course],null,[])){
            form.Set("KhoaHocId","1");form.Set("MaLop","SAVE_TEST");form.Set("TenLop","Lớp thử lưu");form.Set("NgayKhaiGiangDuKien",start);form.Set("ThuHoc","6");form.Set("MoDangKyLuc",BusinessTime.Now.Date);form.Set("DongDangKyLuc",start.AddDays(-3));
            form.Size=new Size(800,740);form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-4000,-4000);form.ShowInTaskbar=false;form.Show();Application.DoEvents();
            using(var guard=new FormMouseWheelGuard(form,install:false)){
                var scroll=(Panel)form.Controls.Find("form-scroll",true).Single();var number=(NumericUpDown)form.Controls.Find("SoBuoiKeHoach",true).Single();number.Focus();Application.DoEvents();
                var oldY=scroll.AutoScrollPosition.Y;var numericChild=number.Controls.OfType<TextBox>().First();var wheel=Message.Create(numericChild.Handle,0x020A,new IntPtr(-120<<16),IntPtr.Zero);
                Check(guard.PreFilterMessage(ref wheel)&&number.Value==16&&scroll.AutoScrollPosition.Y<oldY,"Lăn trên ô số (kể cả ô đang nhập): cuộn form, không đổi 16 buổi");
                var day=(ComboBox)form.Controls.Find("ThuHoc",true).Single();var selected=day.SelectedIndex;wheel=Message.Create(day.Handle,0x020A,new IntPtr(-120<<16),IntPtr.Zero);guard.PreFilterMessage(ref wheel);Check(day.SelectedIndex==selected,"Lăn trên danh sách đóng không tự đổi thứ học");
                var time=(DateTimePicker)form.Controls.Find("GioBatDau",true).Single();var oldTime=time.Value;wheel=Message.Create(time.Handle,0x020A,new IntPtr(120<<16),IntPtr.Zero);guard.PreFilterMessage(ref wheel);Check(time.Value==oldTime,"Lăn trên giờ học không tự đổi giờ/phút");
                var oldValue=number.Value;number.UpButton();Check(number.Value==oldValue+1,"Nút mũi tên tăng số vẫn sử dụng được");form.Set("SoBuoiKeHoach",16);
            }
            var save=(Button)form.Controls.Find("save-continue",true).Single();var confirm=(Button)form.Controls.Find("confirm-save",true).Single();var writes=handler.Writes;
            Check(save.Visible&&save.Parent==confirm.Parent,"Có nút Lưu ngay cạnh Xác nhận");save.PerformClick();Pump(()=>handler.Writes>writes&&save.Enabled);
            Check(!form.IsDisposed&&form.Visible&&handler.LastMethod=="POST","Lưu tạo một lớp và giữ biểu mẫu mở");
            form.Set("TenLop","Lớp đã sửa sau lần lưu đầu");confirm.PerformClick();Pump(()=>form.IsDisposed);
            Check(handler.Writes==writes+2&&handler.LastMethod=="PUT"&&handler.LastIfMatch!="","Xác nhận sau Lưu cập nhật cùng lớp bằng RowVersion mới, không tạo trùng");
        }
        using(var main=new ApiManagementForm(api,fitToScreen:false)){
            main.StartPosition=FormStartPosition.Manual;main.Location=new Point(-5000,-5000);main.ShowInTaskbar=false;main.Show();Application.DoEvents();
            var logout=(Button)main.Controls.Find("sign-out",true).Single();var brand=(Button)main.Controls.Find("sidebar-home",true).Single();var menu=(FlowLayoutPanel)main.Controls.Find("sidebar-menu",true).Single();
            var groups=menu.Controls.OfType<CollapsibleSidebarGroup>().ToList();
            Check(groups.Count==6&&groups.All(group=>!group.Expanded),"Menu quản trị gộp thành 6 nhóm, mặc định thu gọn");
            var training=groups.Single(group=>group.Name=="nav-group-training");var profiles=groups.Single(group=>group.Name=="nav-group-profiles");
            ((Button)main.Controls.Find("nav-toggle-training",true).Single()).PerformClick();Application.DoEvents();Check(training.Expanded,"Bấm nhóm Đào tạo mở các mục con");
            ((Button)main.Controls.Find("nav-toggle-profiles",true).Single()).PerformClick();Application.DoEvents();Check(profiles.Expanded&&!training.Expanded,"Mở nhóm Hồ sơ tự thu gọn nhóm trước");
            ((Button)main.Controls.Find("nav-toggle-profiles",true).Single()).PerformClick();Check(!profiles.Expanded,"Bấm lại để đóng dropdown");
            ((Button)main.Controls.Find("nav-toggle-training",true).Single()).PerformClick();Application.DoEvents();
            foreach(var size in new[]{new Size(1100,740),new Size(1440,920)}){
                main.Size=size;Application.DoEvents();var onForm=main.RectangleToClient(logout.RectangleToScreen(logout.ClientRectangle));
                Check(main.ClientRectangle.Contains(onForm)&&logout.Parent!.ClientRectangle.Contains(logout.Bounds),"Nút Đăng xuất nằm trọn trong cửa sổ "+size);
                var oldPosition=logout.PointToScreen(Point.Empty);menu.AutoScrollPosition=new Point(0,5000);Application.DoEvents();
                Check(oldPosition==logout.PointToScreen(Point.Empty),"Cuộn menu không làm di chuyển nút Đăng xuất "+size);
            }
            main.FitWorkArea(new Rectangle(-5000,-5000,1280,720));Application.DoEvents();Check(main.Height<=720&&main.Width<=1280,"Cửa sổ tự vừa vùng làm việc, không giấu đáy ở màn hình thấp / DPI lớn");
            var typeButton=training.Buttons.Single(button=>button.Tag?.ToString()=="api/quan-ly/loai-khoa-hoc");typeButton.PerformClick();Application.DoEvents();Check(handler.CatalogReads>0&&training.Expanded&&typeButton.IsSelected,"Bấm mục con vẫn mở đúng dữ liệu API và đánh dấu mục đang xem");
            if(args.Length>0){using var expandedShot=new Bitmap(main.Width,main.Height);main.DrawToBitmap(expandedShot,new Rectangle(Point.Empty,main.Size));expandedShot.Save(Path.Combine(Path.GetDirectoryName(args[0])!,"sidebar-expanded.png"));}
            var before=handler.OverviewReads;brand.PerformClick();Application.DoEvents();Check(handler.OverviewReads>before,"Bấm logo thực sự gọi API tải lại dữ liệu Trang chủ");
            Check(groups.All(group=>!group.Expanded)&&!menu.VerticalScroll.Visible,"Về Trang chủ thu gọn các nhóm, menu vừa vùng nhìn mà không cần cuộn dài");
            if(args.Length>0){using var screenshot=new Bitmap(main.Width,main.Height);main.DrawToBitmap(screenshot,new Rectangle(Point.Empty,main.Size));screenshot.Save(Path.Combine(Path.GetDirectoryName(args[0])!,"sidebar-fixed.png"));}
            logout.PerformClick();Application.DoEvents();Check(handler.SignedOut&&main.Logout,"Nút Đăng xuất gọi API và trả ứng dụng về đăng nhập");
        }
        foreach(var (role,rights,expectedGroup) in new[]{("THU_NGAN",4L,"enrollment"),("QUAN_LY_KHO",16L,"inventory"),("GIAO_VIEN",8L,"training")}){
            using var roleHandler=new Handler{Role=role,Rights=rights};using var roleApi=new ArtApiClient(roleHandler,"http://role-check.invalid");roleApi.Login("staff","Test2026!").GetAwaiter().GetResult();
            using var roleForm=new ApiManagementForm(roleApi,fitToScreen:false);var roleMenu=(FlowLayoutPanel)roleForm.Controls.Find("sidebar-menu",true).Single();var visibleGroups=roleMenu.Controls.OfType<CollapsibleSidebarGroup>().ToList();
            Check(visibleGroups.Count==1&&visibleGroups[0].Name=="nav-group-"+expectedGroup,"Nhóm dropdown giữ đúng phân quyền "+role);
        }
        using(var scheduleHandler=new ScheduleHandler())
        using(var scheduleApi=new ArtApiClient(scheduleHandler,"http://schedule.invalid")){
            scheduleApi.Login("admin","test").GetAwaiter().GetResult();
            var workflow=new ClassScheduleWorkflow(scheduleApi,7);var messages=new List<string>();
            var pending=workflow.Save(new{PhongHocId=1},messages.Add);
            Check(!pending.IsCompleted,"Xếp lịch API chậm trả về Task chưa hoàn tất, không chặn luồng giao diện");
            Pump(()=>pending.IsCompleted);pending.GetAwaiter().GetResult();
            Check(scheduleHandler.Generated==1&&scheduleHandler.Published==1&&scheduleHandler.FreshVersion,"Xếp lịch rồi tự công bố bằng RowVersion mới đọc từ API");
            Check(messages.Count==2&&messages[1].Contains("Đang tuyển sinh"),"Hiển thị riêng tiến trình xếp lịch và công bố");
            scheduleHandler.FailPublish=true;var retryWorkflow=new ClassScheduleWorkflow(scheduleApi,7);
            var failed=retryWorkflow.Save(new{},_=>{});Pump(()=>failed.IsCompleted);
            try{failed.GetAwaiter().GetResult();throw new Exception("Expected publication failure");}catch(InvalidOperationException ex){Check(ex.Message.Contains("Lịch học đã được lưu")&&retryWorkflow.ScheduleCreated,"Lỗi công bố nêu rõ lịch đã lưu, không báo nhầm mất lịch");}
            var generated=scheduleHandler.Generated;scheduleHandler.FailPublish=false;
            var retry=retryWorkflow.Save(new{},_=>{});Pump(()=>retry.IsCompleted);retry.GetAwaiter().GetResult();
            Check(scheduleHandler.Generated==generated&&scheduleHandler.Published==3,"Thử lại chỉ công bố lớp, không sinh lịch trùng");
            scheduleHandler.FailGenerate=true;var rejected=new ClassScheduleWorkflow(scheduleApi,7).Save(new{},_=>{});var publications=scheduleHandler.Published;Pump(()=>rejected.IsCompleted);
            try{rejected.GetAwaiter().GetResult();throw new Exception("Expected schedule failure");}catch(InvalidOperationException){Check(scheduleHandler.Published==publications,"Xếp lịch thất bại không công bố lớp");}
        }
        using(var pendingForm=new InputForm("Kiểm thử tiến trình")){
            var completion=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);pendingForm.SaveAsync=()=>completion.Task;
            pendingForm.StartPosition=FormStartPosition.Manual;pendingForm.Location=new Point(-5000,-5000);pendingForm.ShowInTaskbar=false;pendingForm.Show();Application.DoEvents();
            var confirm=(Button)pendingForm.AcceptButton!;confirm.PerformClick();Application.DoEvents();
            Check(!confirm.Enabled&&confirm.Text=="Đang lưu…"&&!pendingForm.UseWaitCursor,"Chờ API hiện trạng thái rõ, chặn gửi trùng và không giữ chuột xoay");
            completion.SetResult();Pump(()=>pendingForm.IsDisposed);Check(pendingForm.IsDisposed,"API hoàn tất: form đóng bình thường, không mắc kẹt trạng thái lưu");
        }
        var modelDirectory=Path.Combine(Directory.GetCurrentDirectory(),"backend","MyThuat","MyThuat.Api","Models");
        var scalarNames=Directory.GetFiles(modelDirectory,"*.cs").SelectMany(path=>System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(path),@"public (?:string|bool|DateTime|DateOnly|TimeOnly|long|int|short|decimal|byte\[\])\?? (\w+) \{ get; set; \}").Select(match=>match.Groups[1].Value)).Distinct().ToList();
        var missingLabels=scalarNames.Where(name=>!ApiColumnLabels.TryGet(name,out _)).ToList();
        Check(missingLabels.Count==0,"Tên tiếng Việt dùng chung bao phủ "+scalarNames.Count+" trường dữ liệu API; thiếu: "+string.Join(", ",missingLabels));
        Check(ApiManagementForm.Label("LaLienHeChinh")=="Liên hệ chính"&&ApiManagementForm.Label("hieuLucTu")=="Có hiệu lực từ"&&ApiManagementForm.Label("DongYHinhAnh")=="Đồng ý sử dụng hình ảnh","Các cột Người đại diện dùng tiếng Việt có dấu, không phụ thuộc chữ hoa thường");
        Check(ApiManagementForm.Label("SoBuoiKeHoach")=="Số buổi kế hoạch"&&ApiManagementForm.Label("NhaCungCapId")=="ID nhà cung cấp"&&ApiManagementForm.Label("LoaiGiaoDich")=="Loại giao dịch"&&ApiManagementForm.Label("XacNhanDuChuyenMon")=="Đã xác nhận đủ chuyên môn","Cột lớp, kho, thu chi và phân công đều dùng bộ nhãn có dấu");
        var translatedRow=Json(new{trangThai="DANG_TUYEN_SINH",trangThaiTuVan="DA_TRA_LOI",maLop="MAIN_L01"});
        Check(ApiManagementForm.Display(translatedRow,"trangThai")=="Đang tuyển sinh"&&ApiManagementForm.Display(translatedRow,"trangThaiTuVan")=="Đã trả lời"&&ApiManagementForm.Display(translatedRow,"maLop")=="MAIN_L01","Trạng thái có dấu nhưng mã lớp và dữ liệu gốc không bị đổi");
        var rememberStore=new RememberedLoginStore("http://remember-check.invalid/"+Guid.NewGuid().ToString("N"));
        try{
            Check(rememberStore.Read() is null,"Chưa chọn ghi nhớ: không có thông tin tự đăng nhập");
            rememberStore.Save("nhanvien_thu","Test credential only 2026!");var remembered=rememberStore.Read();
            Check(remembered?.Username=="nhanvien_thu"&&remembered.Password=="Test credential only 2026!","Windows Credential Manager lưu và đọc lại đúng thông tin bằng kho của Windows");
            var otherServer=new RememberedLoginStore("http://other-server.invalid/"+Guid.NewGuid().ToString("N"));Check(otherServer.Read() is null,"Không dùng tài khoản đã lưu cho API khác");
            rememberStore.Clear();rememberStore.Clear();Check(rememberStore.Read() is null,"Xóa ghi nhớ và xóa lại an toàn, lần sau không tự đăng nhập");
        }finally{rememberStore.Clear();}
        using(var login=new LoginForm()){
            Check(!login.RememberLogin,"Ghi nhớ đăng nhập mặc định tắt, người dùng chủ động chọn");
            login.SetRememberedLogin("test_staff","Test password 2026!");var requests=0;
            login.LoginRequested+=(name,password)=>{requests++;Check(name=="test_staff"&&password=="Test password 2026!","Tự đăng nhập dùng lại thông tin đã lưu để xác thực API, không bỏ qua mật khẩu");};
            login.StartPosition=FormStartPosition.Manual;login.Location=new Point(-5000,-5000);login.ShowInTaskbar=false;login.Show();Pump(()=>requests==1);Application.DoEvents();
            var passwordBox=(TextBox)login.Controls.Find("login-password",true).Single();var rememberBox=(CheckBox)login.Controls.Find("remember-login",true).Single();
            Check(login.RememberLogin&&passwordBox.UseSystemPasswordChar&&rememberBox.Visible,"Mật khẩu đã lưu vẫn được che và tùy chọn ghi nhớ hiện rõ");
            if(args.Length>0){using var shot=new Bitmap(login.Width,login.Height);login.DrawToBitmap(shot,new Rectangle(Point.Empty,login.Size));shot.Save(Path.Combine(Path.GetDirectoryName(args[0])!,"login-remember.png"));}
            rememberBox.Checked=false;Check(!login.RememberLogin,"Người dùng có thể bỏ chọn ghi nhớ");login.Close();
        }
        foreach(var invalidEmail in new[]{"","   ","email-khong-hop-le"}){
            var blockedEmail=false;try{ApiPeopleController.RequiredEmail(invalidEmail);}catch(InvalidOperationException){blockedEmail=true;}
            Check(blockedEmail,"WinForms chặn email thiếu hoặc sai định dạng trước khi gửi API");
        }
        Check(ApiPeopleController.RequiredEmail("TEST@example.invalid")=="test@example.invalid","Email hợp lệ được chuẩn hóa trước khi tạo tài khoản");
        Console.WriteLine($"ALL {passed} WINDOWS FORM CHECKS PASSED");
    }
    static void Pump(Func<bool> done){var until=DateTime.UtcNow.AddSeconds(5);while(!done()&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(5);}if(!done())throw new Exception("Timed out waiting for form save");}
    sealed class ScheduleHandler:HttpMessageHandler
    {
        public int Generated,Published;public bool FreshVersion,FailGenerate,FailPublish;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){
            await Task.Delay(30,ct);var path=request.RequestUri!.AbsolutePath;
            var response="{}";var code=HttpStatusCode.OK;
            if(path.EndsWith("dang-nhap"))response="{\"vaiTroChinh\":\"QUAN_TRI\",\"accessToken\":\"test\",\"tenDangNhap\":\"admin\",\"quyen\":255}";
            else if(request.Method==HttpMethod.Post){Generated++;if(FailGenerate){code=HttpStatusCode.Conflict;response="{\"message\":\"Trùng lịch phòng\"}";}}
            else if(request.Method==HttpMethod.Get)response="{\"id\":7,\"trangThai\":\"NHAP\",\"rowVersion\":\"Ag==\"}";
            else if(request.Method==HttpMethod.Put){Published++;FreshVersion=request.Headers.GetValues("If-Match").Single()=="Ag==";var body=await request.Content!.ReadAsStringAsync(ct);if(!body.Contains("DANG_TUYEN_SINH"))throw new Exception("Wrong publication state");if(FailPublish){code=HttpStatusCode.Conflict;response="{\"message\":\"Phiên bản đã thay đổi\"}";}}
            return new HttpResponseMessage(code){Content=new StringContent(response)};
        }
    }
    sealed class Handler:HttpMessageHandler
    {
        public int Writes;
        public bool AllowSave;public JsonElement? Saved;
        public string LastMethod="",LastIfMatch="";
        public int OverviewReads;public bool SignedOut;
        public int CatalogReads;public string Role="QUAN_TRI";public long Rights=255;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){
            if(request.RequestUri!.AbsolutePath.EndsWith("bao-cao/tong-quan")){OverviewReads++;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"hocVien\":20,\"giaoVien\":8,\"lopTuyenSinh\":6,\"lopDangHoc\":4}")});}
            if(request.Method==HttpMethod.Get&&request.RequestUri.AbsolutePath.EndsWith("quan-ly/loai-khoa-hoc")){CatalogReads++;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"tongSo\":1,\"duLieu\":[{\"id\":1,\"maLoai\":\"CORE\",\"tenLoai\":\"Chính khóa\",\"moTa\":\"Chương trình chính\",\"trangThai\":\"HOAT_DONG\"}]}")});}
            if(request.RequestUri.AbsolutePath.EndsWith("dang-xuat")){SignedOut=true;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));}
            if(!request.RequestUri!.AbsolutePath.EndsWith("dang-nhap")){
                Writes++;if(!AllowSave)throw new Exception("Unexpected API write");LastMethod=request.Method.Method;LastIfMatch=request.Headers.TryGetValues("If-Match",out var values)?values.Single():"";Saved=JsonDocument.Parse(request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult()).RootElement.Clone();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new{id=10,rowVersion=Convert.ToBase64String(BitConverter.GetBytes(Writes))}))});
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new{vaiTroChinh=Role,accessToken="ui-only",tenDangNhap="admin",quyen=Rights}))});
        }
    }
}
