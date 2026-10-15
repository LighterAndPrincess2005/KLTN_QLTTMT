using System.Data;
using System.Globalization;
using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Views.Shared;

namespace LhlArt.Views;

public sealed class ApiManagementForm : Form
{
    readonly ArtApiClient api;
    readonly System.Windows.Forms.Timer refreshTimer = new() { Interval = 30000 };
    readonly FlowLayoutPanel nav = new(), actions = new();
    readonly DataGridView grid = new();
    readonly Label heading = new() { UseMnemonic = false }, status = new() { UseMnemonic = false };
    readonly TextBox search = new();
    readonly Button previous = new(), next = new();
    readonly Panel workspace=new(), home=new();
    readonly Label breadcrumb=new() { UseMnemonic = false };
    readonly List<(string Name,string Route,bool Paged)> destinations=new();
    readonly TableLayoutPanel body;
    List<JsonElement> records = new();
    string route = "", title = "";
    int page = 1, total;
    bool paged, busy, mayAttend;
    Dictionary<string,string> teacherOptions=new();
    public bool Logout { get; private set; }
    public ApiManagementForm(ArtApiClient client)
    {
        api=client; Text="LHL ART | Không gian quản lý";
        Size=new Size(1440,920); MinimumSize=new Size(1100,740); StartPosition=FormStartPosition.CenterScreen;
        Font=Theme.Font(); BackColor=Theme.Paper; AutoScaleMode=AutoScaleMode.Dpi;
        nav.Dock=DockStyle.Left; nav.Width=248; nav.AutoScroll=true; nav.FlowDirection=FlowDirection.TopDown;
        nav.WrapContents=false; nav.BackColor=Color.White; nav.Padding=new Padding(18,20,12,12);
        var identity=new Panel {Width=208,Height=76,Margin=new Padding(0,0,0,12)};
        var logo=Theme.Logo();logo.Size=new Size(55,55);identity.Controls.Add(logo);
        identity.Controls.Add(new Label {Text="LHL ART",Location=new Point(68,6),Size=new Size(136,25),Font=Theme.Font(15,FontStyle.Bold),ForeColor=Theme.Teal});
        identity.Controls.Add(new Label {Text="FINE ART CENTER",Location=new Point(69,34),Size=new Size(136,22),Font=Theme.Font(8),ForeColor=Theme.Muted});nav.Controls.Add(identity);
        nav.Controls.Add(new Label {Text="KHÔNG GIAN LÀM VIỆC",Width=208,Height=28,Font=Theme.Font(8,FontStyle.Bold),ForeColor=Theme.Muted});
        var homeButton=NavButton("Trang chủ","Tổng quan");homeButton.Tag="home";
        homeButton.Click+=async(_,_)=>await Run(ShowHome);nav.Controls.Add(homeButton);
        workspace.Dock=DockStyle.Fill;
        var shell=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Padding=new Padding(28,0,28,22)};
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));shell.RowStyles.Add(new RowStyle(SizeType.Absolute,86));shell.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var top=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,310));
        breadcrumb.Text="Không gian làm việc  /  Trang chủ";breadcrumb.Dock=DockStyle.Fill;breadcrumb.TextAlign=ContentAlignment.MiddleLeft;breadcrumb.ForeColor=Theme.Muted;breadcrumb.Font=Theme.Font(10);
        var profile=new Label {Text=api.Username+"   ●\n"+RoleName(api.Role),Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleRight,ForeColor=Theme.Teal,Font=Theme.Font(10,FontStyle.Bold)};
        top.Controls.Add(breadcrumb,0,0);top.Controls.Add(profile,1,0);shell.Controls.Add(top,0,0);shell.Controls.Add(workspace,0,1);
        body=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(0),RowCount=5,ColumnCount=1,Visible=false};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{68f,62f,66f}) body.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
        body.RowStyles.Add(new RowStyle(SizeType.Percent,100)); body.RowStyles.Add(new RowStyle(SizeType.Absolute,45));
        heading.Dock=DockStyle.Fill; heading.Font=Theme.Font(22,FontStyle.Bold); heading.ForeColor=Theme.Navy;heading.UseMnemonic=false;breadcrumb.UseMnemonic=false;
        search.PlaceholderText="Tìm kiếm trong danh sách…";search.AccessibleName="Tìm kiếm trong danh sách"; search.TextChanged+=(_,_)=>Bind();
        actions.Dock=DockStyle.Fill; actions.AutoScroll=true;
        Theme.Grid(grid);grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells;
        var bottom=new FlowLayoutPanel {Dock=DockStyle.Fill}; status.AutoSize=true; status.Padding=new Padding(0,8,20,0);
        previous.Text="← Trang trước"; previous.AutoSize=true; previous.Click+=async(_,_)=>await Run(async()=>{page--;await LoadData();});
        next.Text="Trang sau →"; next.AutoSize=true; next.Click+=async(_,_)=>await Run(async()=>{page++;await LoadData();});
        bottom.Controls.AddRange([status,previous,next]);
        var searchBox=new StudioInput(search,"Tìm kiếm",true) {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,12)};
        var gridCard=new StudioCard {Dock=DockStyle.Fill,Padding=new Padding(12),Margin=new Padding(0)};gridCard.Controls.Add(grid);
        body.Controls.Add(heading,0,0);body.Controls.Add(searchBox,0,1);body.Controls.Add(actions,0,2);body.Controls.Add(gridCard,0,3);body.Controls.Add(bottom,0,4);
        home.Dock=DockStyle.Fill;home.AutoScroll=true;workspace.Controls.Add(body);workspace.Controls.Add(home);
        Controls.Add(shell);Controls.Add(nav);
        if(api.Has(32)) Item("Tổng quan","api/bao-cao/tong-quan");
        if(api.Has(2))
        {
            Item("Thành viên / phụ huynh","api/quan-ly/thanh-vien",true);
            Item("Học viên từ web","api/quan-ly/hoc-vien",true);
            Item("Quyền đại diện","api/quan-ly/dai-dien",true);
            Item("Phản hồi & đề nghị","api/phan-hoi");
        }
        if(api.Has(1))
        {
            Item("Loại khóa học","api/quan-ly/loai-khoa-hoc",true); Item("Nội dung từng buổi","api/quan-ly/noi-dung-buoi",true);
            Item("Khóa học","api/quan-ly/khoa-hoc",true); Item("Lớp học","api/quan-ly/lop-hoc",true);
            Item("Giáo viên","api/quan-ly/giao-vien",true);Item("Phòng học","api/quan-ly/phong-hoc",true);
            Item("Yêu cầu hủy / chuyển","api/thay-doi");
        }
        if(api.Has(2)||api.Has(4)) Item("Đăng ký & học phí","api/dang-ky/quan-ly",true);
        if(api.Has(8)) Item("Lịch học & điểm danh","api/lich-hoc");
        if(api.Has(16)) {Item("Họa cụ","api/quan-ly/hoa-cu",true);Item("Nhà cung cấp","api/quan-ly/nha-cung-cap",true);Item("Tồn kho","api/kho/ton");Item("Phiếu kho","api/kho/phieu");}
        if(api.Has(4)) Item("Thu tiền chờ đối chiếu","api/giao-dich/cho-doi-chieu");
        if(api.Has(64)) Item("Tài khoản nhân viên","api/tai-khoan");
        if(api.Has(32)&&api.Has(4))
        {
            var chart=NavButton("Biểu đồ doanh thu","Báo cáo");chart.Click+=async(_,_)=>await Run(async()=>{
                var result=await api.Read("api/bao-cao/doanh-thu-thang");
                var values=result.EnumerateArray().Select(x=>new Models.MonthlyFigures(ArtApiClient.Get(x,"thang").GetDateTime(),ArtApiClient.Get(x,"hocPhi").GetDecimal(),ArtApiClient.Get(x,"hoanHocPhi").GetDecimal(),ArtApiClient.Get(x,"hoaCu").GetDecimal())).ToArray();
                using var dialog=new Form {Text="Doanh thu trung tâm",Size=new Size(1040,640),StartPosition=FormStartPosition.CenterParent};dialog.Controls.Add(new StatisticsView(values));dialog.ShowDialog(this);
            });nav.Controls.Add(chart);
        }
        if(api.Has(128)) Item("Nhật ký thao tác","api/bao-cao/nhat-ky");
        var signOut=NavButton("Đăng xuất","Đăng xuất");signOut.Margin=new Padding(0,18,0,0);signOut.Click+=async(_,_)=>await Run(async()=>{
            await api.Post("api/tai-khoan/dang-xuat",new{}); Logout=true;Close(); });nav.Controls.Add(signOut);
        Shown+=async(_,_)=>{await Run(ShowHome);refreshTimer.Start();};
        refreshTimer.Tick+=async(_,_)=>{if(!busy&&ActiveForm==this&&!search.Focused)await Run(home.Visible?ShowHome:LoadData);};
        FormClosed+=(_,_)=>refreshTimer.Dispose();
    }
    static string RoleName(string role)=>role switch {"QUAN_TRI"=>"Quản trị","HOC_VU"=>"Học vụ","THU_NGAN"=>"Thu ngân","GIAO_VIEN"=>"Giáo viên","QUAN_LY_KHO"=>"Quản lý kho",_=>role};
    void Item(string name,string path,bool pagination=false)
    {
        destinations.Add((name,path,pagination));
        var button=NavButton(name,NavIcon(name));button.Tag=path;button.Click+=async(_,_)=>await Run(()=>OpenPage(name,path,pagination));nav.Controls.Add(button);
    }
    static string NavIcon(string name)=>name.Contains("Lịch")?"Lịch học":name.Contains("phí")||name.Contains("tiền")?"Thực thu học phí":name.Contains("viên")||name.Contains("diện")?"Học viên":name.Contains("kho")||name.Contains("Họa")||name.Contains("cung cấp")?"Họa cụ & kho":name.Contains("Tài khoản")?"Quản trị":name.Contains("Nhật ký")?"Báo cáo":name.Contains("Khóa")?"Khóa học":name.Contains("Lớp")?"Lớp học":"Tổng quan";
    static ActionButton NavButton(string name,string icon)=>new(){Text=name,IconKey=icon,Navigation=true,Width=208,Height=39,AutoSize=false,Margin=new Padding(0,2,0,2),BackColor=Color.White,ForeColor=Theme.Muted,Font=Theme.Font(9.5f),Cursor=Cursors.Hand,FlatStyle=FlatStyle.Flat};
    void SelectNav(string key)
    {
        foreach(var button in nav.Controls.OfType<ActionButton>()){button.IsSelected=button.Tag?.ToString()==key;button.Invalidate();}
    }
    async Task OpenPage(string name,string path,bool pagination)
    {
        home.Visible=false;body.Visible=true;body.BringToFront();SelectNav(path);breadcrumb.Text="Không gian làm việc  /  "+name;
        route=path;title=name;paged=pagination;page=1;search.Text="";await LoadData();
    }
    async Task ShowHome()
    {
        body.Visible=false;home.Visible=true;home.BringToFront();SelectNav("home");breadcrumb.Text="Không gian làm việc  /  Trang chủ";
        foreach(Control control in home.Controls.Cast<Control>().ToArray())control.Dispose();
        var dashboard=new StudioDashboard(api,destinations,(name,path,pagination)=>Run(()=>OpenPage(name,path,pagination)));
        dashboard.Dock=DockStyle.Top;home.Controls.Add(dashboard);await dashboard.LoadOverview();
    }
    async Task Run(Func<Task> work)
    {
        if(busy)return;busy=true;UseWaitCursor=true;nav.Enabled=false;actions.Enabled=false;previous.Enabled=false;next.Enabled=false;
        try {await work();}
        catch(ApiSessionExpiredException ex){MessageBox.Show(this,ex.Message,"Đăng nhập lại",MessageBoxButtons.OK,MessageBoxIcon.Information);Logout=true;Close();}
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Kiểm tra thông tin",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally {busy=false;if(!IsDisposed){UseWaitCursor=false;nav.Enabled=true;actions.Enabled=true;previous.Enabled=paged&&page>1;next.Enabled=paged&&page*100<total;}}
    }
    async Task LoadData()
    {
        var selectedId=grid.CurrentRow?.DataBoundItem is DataRowView selected ? ArtApiClient.Text(records[Convert.ToInt32(selected["__index"])],"id") : "";
        JsonElement data;
        try { data=await api.Read(route+(paged?$"?trang={page}&kichThuoc=100":"")); }
        catch(ApiNotFoundException) when(route=="api/dang-ky/quan-ly")
        {
            records.Clear();total=0;paged=false;heading.Text=title;Bind();
            foreach(Control control in actions.Controls.Cast<Control>().ToArray())control.Dispose();
            Action("Làm mới",LoadData);
            actions.Controls.Add(new Label {Text="Bản API hiện tại chưa hỗ trợ danh sách đăng ký cho nhân viên.",AutoSize=true,Padding=new Padding(0,12,0,0),ForeColor=Theme.Muted});
            status.Text="Chức năng chưa được API hỗ trợ · "+api.BaseUrl;
            return;
        }
        var list=data;
        if(ArtApiClient.TryGet(data,"duLieu",out var entries)) list=entries;
        total=ArtApiClient.TryGet(data,"tongSo",out var count)?count.GetInt32():list.ValueKind==JsonValueKind.Array?list.GetArrayLength():1;
        records=list.ValueKind==JsonValueKind.Array?list.EnumerateArray().Select(x=>x.Clone()).ToList():new(){list};
        heading.Text=title;Bind(); ConfigureActions();
        foreach(DataGridViewRow row in grid.Rows)
            if(row.DataBoundItem is DataRowView dataRow && ArtApiClient.Text(records[Convert.ToInt32(dataRow["__index"])],"id")==selectedId)
            { grid.CurrentCell=row.Cells.Cast<DataGridViewCell>().FirstOrDefault(x=>x.Visible);break; }
        status.Text=$"{records.Count} dòng · Trang {page} · Tổng {total} · Dữ liệu trung tâm";
    }
    void Bind()
    {
        var table=new DataTable();table.Columns.Add("__index",typeof(int));
        var hidden=new[]{"rowVersion","createdAt","updatedAt","requestFingerprint","phuongAnHoanJson"};
        var preferred=new[]{"hoTen","tenKhoa","tenLop","maLop","maHocVien","batDau","ketThuc","trangThai","thuTuTrongLop","nhanXet","noiDung"};
        var names=records.Where(x=>x.ValueKind==JsonValueKind.Object).SelectMany(x=>x.EnumerateObject()).Select(x=>x.Name).Distinct().Where(x=>!hidden.Contains(x,StringComparer.OrdinalIgnoreCase)).OrderBy(x=>Array.FindIndex(preferred,key=>key.Equals(x,StringComparison.OrdinalIgnoreCase)) is var rank&&rank>=0?rank:100).ToArray();
        foreach(var name in names)table.Columns.Add(name,typeof(string));
        for(var i=0;i<records.Count;i++)
        {
            var row=records[i];if(!row.ToString().Contains(search.Text,StringComparison.CurrentCultureIgnoreCase))continue;
            var target=table.NewRow();target[0]=i;
            foreach(var name in names)target[name]=Display(row,name);
            table.Rows.Add(target);
        }
        grid.DataSource=table;if(grid.Columns.Contains("__index"))grid.Columns["__index"].Visible=false;
        foreach(DataGridViewColumn column in grid.Columns)
        {
            column.HeaderText=Label(column.Name);
            if(records.Any(x=>ArtApiClient.TryGet(x,column.Name,out var v)&&v.ValueKind==JsonValueKind.Number))column.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;column.MinimumWidth=column.Name.Equals("hoTen",StringComparison.OrdinalIgnoreCase)?200:100;
            if(column.Name.Equals("batDau",StringComparison.OrdinalIgnoreCase)||column.Name.Equals("ketThuc",StringComparison.OrdinalIgnoreCase))column.MinimumWidth=165;
        }
    }
    static string Display(JsonElement row,string name)
    {
        var text=ArtApiClient.Text(row,name);
        if(text.Length>=10 && text[4]=='-' && text[7]=='-' && DateTime.TryParse(text,CultureInfo.InvariantCulture,DateTimeStyles.None,out var time))
            return time.ToString(text.Length==10?"dd/MM/yyyy":"dd/MM/yyyy HH:mm",Theme.Culture);
        if(name.Equals("trangThai",StringComparison.OrdinalIgnoreCase)||name.Equals("loaiThamGia",StringComparison.OrdinalIgnoreCase))
            return text switch {"CO_MAT"=>"Có mặt","VANG"=>"Vắng","CHO_THAM_GIA"=>"Chờ tham gia","HOC_CHINH"=>"Học chính","HOC_BU"=>"Học bù","CHO_XAC_NHAN"=>"Chờ xác minh","HOAT_DONG"=>"Hoạt động","DA_XAC_NHAN"=>"Đã xác nhận","CHO_THANH_TOAN"=>"Chờ thanh toán","DA_XEP_LICH"=>"Đã xếp lịch","DA_TO_CHUC"=>"Đã tổ chức",_=>text};
        if(IsMoney(name)&&decimal.TryParse(text,NumberStyles.Number,CultureInfo.InvariantCulture,out var amount))return Theme.Money(amount);
        return text;
    }
    static bool IsMoney(string name)=>new[]{"hocPhi","hocPhiApDung","hocPhiGocChot","giaBanThamKhao","soTien","donGiaChot","thanhTienChot","daThanhToan","phaiThuHieuLuc","conThieu"}.Contains(name,StringComparer.OrdinalIgnoreCase);
    static string Label(string name)=>name.ToLowerInvariant() switch {
        "hoten"=>"Họ tên","tenlop"=>"Tên lớp","tenkhoa"=>"Khóa học","trangthai"=>"Trạng thái","ngaysinh"=>"Ngày sinh","quanhe"=>"Quan hệ","quyendaidien"=>"Quyền đại diện","batdau"=>"Bắt đầu","ketthuc"=>"Kết thúc","x acnhanluc"=>"Xác nhận lúc","xacnhanluc"=>"Xác nhận lúc","hocvien"=>"Học viên","nguoidangky"=>"Người đăng ký","madangky"=>"Mã đăng ký","ma hocvien"=>"Mã học viên","mahocvien"=>"Mã học viên","dienthoai"=>"Điện thoại","noidung"=>"Nội dung","traloi"=>"Trả lời","capdodexuat"=>"Cấp độ đề xuất","nhanxetdauvao"=>"Nhận xét đầu vào","hocvienid"=>"ID học viên","thanhvienid"=>"ID thành viên","hanguicho"=>"Hạn giữ chỗ","lapluc"=>"Ngày đăng ký",_=>System.Text.RegularExpressions.Regex.Replace(name,"(?<=[a-z])([A-Z])"," $1")};
    JsonElement Selected()=>grid.CurrentRow?.DataBoundItem is DataRowView row?records[Convert.ToInt32(row["__index"])]:throw new InvalidOperationException("Hãy chọn một dòng.");
    static long Id(JsonElement item)=>ArtApiClient.Get(item,"id").GetInt64();
    void Action(string name,Func<Task> work)
    {
        var button=Theme.Button(name);if(button is ActionButton action)action.IconKey=name=="Làm mới"?"Làm mới":name.Contains("điểm danh")?"Lịch học":name.Contains("Xác minh")?"Quản trị":"";
        button.Click+=async(_,_)=>await Run(work);actions.Controls.Add(button);
    }
    void ConfigureActions()
    {
        foreach(Control control in actions.Controls.Cast<Control>().ToArray())control.Dispose();
        Action("Làm mới",LoadData);
        var catalog=new Controllers.ApiCatalogController(api);
        if(catalog.Supports(route))
        {
            Action("Thêm",async()=>{await catalog.Edit(this,route,title,null);await LoadData();});
            Action("Sửa",async()=>{await catalog.Edit(this,route,title,Selected());await LoadData();});
        }
        Action("Xem chi tiết",()=>{using var detail=new Form {Text="Chi tiết · "+title,Size=new Size(750,650),StartPosition=FormStartPosition.CenterParent};
            var fields=new DataGridView {Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
            fields.DataSource=Selected().EnumerateObject().Where(x=>!x.Name.Equals("rowVersion",StringComparison.OrdinalIgnoreCase)).Select(x=>new{Truong=Label(x.Name),GiaTri=x.Value.ToString()}).ToList();detail.Controls.Add(fields);detail.ShowDialog(this);return Task.CompletedTask;});
        if(route=="api/quan-ly/thanh-vien"||route=="api/quan-ly/hoc-vien") Action("Xác minh hồ sơ",async()=>{
            var row=Selected();if(MessageBox.Show(this,$"Xác minh hồ sơ {ArtApiClient.Text(row,"hoTen")}?", "Xác minh",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            await api.Put(route+"/"+Id(row),new{TrangThai="HOAT_DONG"},ArtApiClient.Text(row,"rowVersion"));await LoadData();});
        if(route=="api/quan-ly/dai-dien") Action("Xác minh quyền đại diện",async()=>{
            var row=Selected();var values=Prompt("Xác minh quyền đại diện",("Quan hệ (CHA / ME / GIAM_HO / TU_BAN_THAN)",ArtApiClient.Text(row,"quanHe")),("Quyền (DAY_DU / DANG_KY / XEM_KET_QUA)",ArtApiClient.Text(row,"quyenDaiDien")));
            if(values is null)return;
            await api.Put(route+"/"+Id(row),new{QuanHe=values[0],QuyenDaiDien=values[1]},ArtApiClient.Text(row,"rowVersion"));await LoadData();});
        if(route=="api/quan-ly/hoc-vien") Action("Đánh giá đầu vào",Assess);
        if(route=="api/quan-ly/lop-hoc") Action("Xếp lịch",async()=>{
            var row=Selected();using var form=new InputForm("Xếp lịch theo tuần");
            form.Add("room","Phòng học","",await catalog.Choices("phong-hoc","TenPhong"));
            form.Add("teacher","Giáo viên chính","",await catalog.Choices("giao-vien","HoTen"));
            var assistants=new List<Choice>{new("0","Không có")};assistants.AddRange(await catalog.Choices("giao-vien","HoTen"));
            form.Add("assistant","Trợ giảng (Junior)","0",assistants);
            form.Add("start","Buổi đầu",DateTime.Parse(ArtApiClient.Text(row,"ngayKhaiGiangDuKien"),CultureInfo.InvariantCulture).AddHours(9));
            form.Add("qualified","Đã xác minh chuyên môn",false);
            if(form.ShowDialog(this)!=DialogResult.OK)return;
            var assistant=long.Parse(form.TextValue("assistant"));
            await api.Post("api/lich-hoc/lop/"+Id(row)+"/sinh-lich",new{PhongHocId=long.Parse(form.TextValue("room")),GiaoVienId=long.Parse(form.TextValue("teacher")),TroGiangId=assistant==0?(long?)null:assistant,BuoiDau=form.Date("start").ToString("yyyy-MM-ddTHH:mm:ss"),XacNhanDuChuyenMon=form.Flag("qualified")});await LoadData();
        });
        if(route=="api/thay-doi"&&api.Has(1))
        {
            Action("Duyệt / từ chối",async()=>{var row=Selected();using var form=new InputForm("Xử lý yêu cầu");form.Add("approve","Đồng ý",true);form.Add("reason","Lý do quyết định");if(form.ShowDialog(this)!=DialogResult.OK)return;await api.Post("api/thay-doi/"+Id(row)+"/duyet",new{DongY=form.Flag("approve"),LyDo=form.TextValue("reason")});await LoadData();});
            Action("Hoàn tất chuyển",async()=>{var row=Selected();if(MessageBox.Show(this,"Hoàn tất chuyển lớp theo phương án đã duyệt?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;await api.Post("api/thay-doi/"+Id(row)+"/hoan-tat-chuyen",new{});await LoadData();});
        }

        if(route=="api/phan-hoi") Action("Trả lời",async()=>{
            var row=Selected();var fields=Prompt("Trả lời phản hồi / đề nghị",("Nội dung trả lời",ArtApiClient.Text(row,"traLoi")));if(fields is null)return;
            await api.Post("api/phan-hoi/"+Id(row)+"/tra-loi",new{TraLoi=fields[0]});await LoadData();});
        if(route=="api/dang-ky/quan-ly")
        {
            Action("Xem công nợ",async()=>{var row=Selected();var debt=await api.Read("api/dang-ky/"+Id(row)+"/cong-no");ShowCharges(debt);});
            if(api.Has(4)) Action("Ghi nhận thu tiền",Receive);
        }
        if(route=="api/lich-hoc") Action("Danh sách điểm danh",async()=>{
            var row=Selected();var id=Id(row);var now=Services.BusinessTime.Now;
            mayAttend=DateTime.TryParse(ArtApiClient.Text(row,"batDau"),CultureInfo.InvariantCulture,DateTimeStyles.None,out var start)&&start<=now
                &&(api.Has(1)||start.Date==now.Date)&&ArtApiClient.Text(row,"trangThai") is "DA_XEP_LICH" or "DA_TO_CHUC";
            route=$"api/hoc-tap/buoi/{id}/danh-sach";title="Điểm danh buổi "+id;paged=false;page=1;await LoadData();});
        if(route.StartsWith("api/hoc-tap/buoi/"))
        {
            if(mayAttend)Action("Ghi điểm danh / nhận xét",Attend);
            else actions.Controls.Add(new Label{Text="Chỉ xem: buổi chưa bắt đầu hoặc ngoài ngày giáo viên được cập nhật.",AutoSize=true,Padding=new Padding(0,12,0,0),ForeColor=Theme.Muted});
        }
    }
    async Task Assess()
    {
        var row=Selected();var staff=await api.Read("api/quan-ly/giao-vien?kichThuoc=100");
        teacherOptions=ArtApiClient.Get(staff,"duLieu").EnumerateArray().Where(x=>ArtApiClient.Text(x,"trangThai")=="HOAT_DONG").ToDictionary(x=>Id(x).ToString(),x=>ArtApiClient.Text(x,"hoTen"));
        var fields=Prompt("Đánh giá đầu vào · "+ArtApiClient.Text(row,"hoTen"),("Giáo viên đánh giá",ArtApiClient.Text(row,"giaoVienDanhGia")),("Cấp độ đề xuất",ArtApiClient.Text(row,"capDoDeXuat")),("Nhận xét sau đánh giá",ArtApiClient.Text(row,"nhanXetDauVao")));
        if(fields is null)return;if(!long.TryParse(fields[0],out var teacher)||teacher<=0)throw new InvalidOperationException("Nhập ID giáo viên trong danh sách.");
        await api.Post("api/hoc-vien/"+Id(row)+"/danh-gia",new{GiaoVienId=teacher,CapDoDeXuat=fields[1],NhanXet=fields[2]});await LoadData();
    }
    void ShowCharges(JsonElement debt)
    {
        using var form=new Form {Text="Công nợ đăng ký · "+ArtApiClient.Text(debt,"trangThai"),Size=new Size(850,430),StartPosition=FormStartPosition.CenterParent};
        var table=new DataTable();foreach(var name in new[]{"id","loaiKhoan","thanhTienChot","daThanhToan","phaiThuHieuLuc","conThieu"})table.Columns.Add(name);
        foreach(var charge in ArtApiClient.Get(debt,"khoanThu").EnumerateArray())table.Rows.Add(table.Columns.Cast<DataColumn>().Select(x=>(object)Display(charge,x.ColumnName)).ToArray());
        form.Controls.Add(new DataGridView {Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,DataSource=table,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill});form.ShowDialog(this);
    }
    async Task Receive()
    {
        var row=Selected();var id=Id(row);var debt=await api.Read("api/dang-ky/"+id+"/cong-no");ShowCharges(debt);
        var fields=Prompt("Thu tiền thực tế · đăng ký "+id,("ID khoản thu từ bảng công nợ",""),("Số tiền VND (không dấu phân cách)",""),("Phương thức TIEN_MAT / CHUYEN_KHOAN","TIEN_MAT"),("Mã chứng từ duy nhất",""),("Mã tham chiếu chuyển khoản (nếu có)",""));
        if(fields is null)return;
        if(!long.TryParse(fields[0],out var charge)||charge<=0||!decimal.TryParse(fields[1],NumberStyles.None,CultureInfo.InvariantCulture,out var amount)||amount<=0)throw new InvalidOperationException("ID khoản thu và số tiền phải là số nguyên dương.");
        if(MessageBox.Show(this,$"Xác nhận đã nhận {Theme.Money(amount)} cho đăng ký {id}?", "Ghi nhận thu tiền",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        var result=await api.Post("api/giao-dich/thu",new{DangKyId=id,KhoanThuId=charge,SoTien=amount,PhuongThuc=fields[2],MaChungTu=fields[3],MaThamChieu=fields[4],IdempotencyKey="WIN-"+Guid.NewGuid().ToString("N")});
        MessageBox.Show(this,"Kết quả: "+ArtApiClient.Text(result,"trangThai")+". API chỉ xác nhận vào lớp khi đáp ứng đủ điều kiện và học phí.");await LoadData();
    }
    async Task Attend()
    {
        var row=Selected();var fields=Prompt("Điểm danh · "+ArtApiClient.Text(row,"hoTen"),("Trạng thái CO_MAT / VANG",ArtApiClient.Text(row,"trangThai")=="VANG"?"VANG":"CO_MAT"),("Số phút tham dự",ArtApiClient.Text(row,"phutThamDu")),("Điểm sản phẩm 0–10 (không bắt buộc)",ArtApiClient.Text(row,"diemSanPham")),("Nhận xét",ArtApiClient.Text(row,"nhanXet")));
        if(fields is null)return;
        short? minutes=null;decimal? score=null;
        if(!string.IsNullOrWhiteSpace(fields[1])) {if(!short.TryParse(fields[1],out var m))throw new InvalidOperationException("Số phút không hợp lệ.");minutes=m;}
        if(!string.IsNullOrWhiteSpace(fields[2])) {if(!decimal.TryParse(fields[2].Replace(',','.'),NumberStyles.Number,CultureInfo.InvariantCulture,out var d))throw new InvalidOperationException("Điểm không hợp lệ.");score=d;}
        await api.Put("api/hoc-tap/diem-danh/"+Id(row),new{TrangThai=fields[0],PhutThamDu=minutes,DiemSanPham=score,NhanXet=fields[3],SanPhamUrl=ArtApiClient.Text(row,"sanPhamUrl"),RowVersion=ArtApiClient.Text(row,"rowVersion")});await LoadData();
    }
    string[]? Prompt(string name,params (string Label,string Value)[] fields)
    {
        using var dialog=new Form {Text=name,Width=700,Height=Math.Min(780,160+fields.Length*100),StartPosition=FormStartPosition.CenterParent,Font=Theme.Font(),BackColor=Theme.Paper,MinimizeBox=false,MaximizeBox=false};
        var panel=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(20)};
        var inputs=new List<Func<string>>();
        foreach(var field in fields)
        {
            Dictionary<string,string>? choices=field.Label.StartsWith("Quan hệ")?new(){["CHA"]="Cha",["ME"]="Mẹ",["GIAM_HO"]="Người giám hộ",["TU_BAN_THAN"]="Tự đại diện"}:
                field.Label.StartsWith("Quyền")?new(){["DAY_DU"]="Toàn quyền đại diện",["DANG_KY"]="Đăng ký học",["XEM_KET_QUA"]="Chỉ xem kết quả"}:
                field.Label.StartsWith("Phương thức")?new(){["TIEN_MAT"]="Tiền mặt",["CHUYEN_KHOAN"]="Chuyển khoản"}:
                field.Label.StartsWith("Trạng thái")?new(){["CO_MAT"]="Có mặt",["VANG"]="Vắng"}:
                field.Label=="Giáo viên đánh giá"?teacherOptions:null;
            panel.Controls.Add(new Label {Text=choices is null?field.Label:field.Label.Split(" (",StringSplitOptions.None)[0].Replace(" TIEN_MAT / CHUYEN_KHOAN","").Replace(" CO_MAT / VANG",""),Width=610,Height=42,AutoEllipsis=true});
            if(choices is not null)
            {
                var input=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,DisplayMember="Value",ValueMember="Key",DataSource=choices.ToList()};
                input.SelectedValue=field.Value;inputs.Add(()=>input.SelectedValue?.ToString()??"");panel.Controls.Add(new StudioInput(input) {Width=610});
            }
            else {var input=new TextBox {Text=field.Value};inputs.Add(()=>input.Text.Trim());panel.Controls.Add(new StudioInput(input) {Width=610});}
        }
        var ok=Theme.Button("Xác nhận",true);ok.DialogResult=DialogResult.OK;var cancel=Theme.Button("Hủy");cancel.DialogResult=DialogResult.Cancel;
        var buttons=new FlowLayoutPanel {Width=610,Height=55};buttons.Controls.AddRange([ok,cancel]);panel.Controls.Add(buttons);dialog.Controls.Add(panel);dialog.AcceptButton=ok;dialog.CancelButton=cancel;
        return dialog.ShowDialog(this)==DialogResult.OK?inputs.Select(x=>x()).ToArray():null;
    }
}
