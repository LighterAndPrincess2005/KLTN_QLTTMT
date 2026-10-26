using System.Globalization;
using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;

internal sealed class ApiPeopleController(ArtApiClient api)
{
    readonly ApiDataService data = new(api);
    static string T(JsonElement row, string key) => ArtApiClient.Text(row, key);
    static long Id(JsonElement row, string key = "id") => ApiDataService.Id(row, key);
    static string Date(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    static string Stamp(DateTime value) => value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    static DateTime Birthday(JsonElement row) => DateTime.TryParse(T(row, "ngaySinh"), out var date) ? date : BusinessTime.Now.Date.AddYears(-18);
    static Choice[] Relations => [new("TU_BAN_THAN", "Bản thân"), new("CHA", "Cha"), new("ME", "Mẹ"), new("GIAM_HO", "Người giám hộ")];
    static List<Choice> People(IEnumerable<JsonElement> rows, string code) => rows.Select(x => new Choice(Id(x).ToString(), $"{T(x, "hoTen")} · {T(x, code)}")).ToList();
    static string NewCode(string prefix) => prefix + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();

    public Task Signup(IWin32Window owner)
    {
        using var form = new InputForm("Đăng ký tài khoản người học / phụ huynh");
        form.Add("TenDangNhap", "Tên đăng nhập");
        form.Add("MatKhau", "Mật khẩu", password: true);
        form.Add("LapLai", "Nhập lại mật khẩu", password: true);
        form.Add("HoTen", "Họ tên chủ tài khoản");
        form.Add("NgaySinh", "Ngày sinh", BusinessTime.Now.Date.AddYears(-18));
        form.Add("DienThoai", "Điện thoại");
        form.Add("Email", "Email (bắt buộc)");
        form.SaveAsync = async () =>
        {
            ProfileRules.Password(form.TextValue("MatKhau"), form.TextValue("LapLai"));
            ProfileRules.Birth(form.Date("NgaySinh"));
            var username = ProfileRules.Required(form.TextValue("TenDangNhap"), "Tên đăng nhập", 100);
            if (username.Length < 3) throw new InvalidOperationException("Tên đăng nhập cần ít nhất 3 ký tự.");
            var result = await api.Post("api/tai-khoan/dang-ky", new
            {
                TenDangNhap = username, MatKhau = form.TextValue("MatKhau"),
                HoTen = ProfileRules.Required(form.TextValue("HoTen"), "Họ tên"),
                DienThoai = ProfileRules.Required(form.TextValue("DienThoai"), "Điện thoại", 25),
                Email = RequiredEmail(form.TextValue("Email")), NgaySinh = Date(form.Date("NgaySinh"))
            });
            MessageBox.Show(owner, $"Đã tạo tài khoản {username}. Có thể đăng nhập website ngay.\nMã hồ sơ thành viên: {T(result, "thanhVienId")}.\nThêm hoặc liên kết học viên tại mục Hồ sơ học viên khi cần; chưa đăng ký lớp.", "Tạo tài khoản thành công");
        };
        form.ShowDialog(owner);
        return Task.CompletedTask;
    }
    static string? Optional(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    internal static string RequiredEmail(string value){
        var email=value.Trim();
        if(email.Length==0)throw new InvalidOperationException("Vui lòng nhập email.");
        if(email.Length>254||!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))throw new InvalidOperationException("Email không đúng định dạng.");
        return email.ToLowerInvariant();
    }

    public Task Member(IWin32Window owner, JsonElement? row)
    {
        using var form = new InputForm(row.HasValue ? "Sửa thông tin người liên hệ" : "Thêm người liên hệ");
        string Get(string key) => row.HasValue ? T(row.Value, key) : "";
        form.Add("MaThanhVien", "Mã thành viên", row.HasValue ? Get("maThanhVien") : NewCode("TV"), readOnly: row.HasValue);
        form.Add("HoTen", "Họ tên người liên hệ", Get("hoTen"));
        form.Add("CoNgaySinh", "Đã có ngày sinh", row.HasValue && Get("ngaySinh") != "");
        form.Add("NgaySinh", "Ngày sinh", row.HasValue ? Birthday(row.Value) : BusinessTime.Now.Date.AddYears(-18));
        form.Add("DienThoai", "Điện thoại", Get("dienThoai"));
        form.Add("Email", "Email", Get("email"));
        form.Add("DiaChi", "Địa chỉ", Get("diaChi"));
        form.Add("TrangThai", "Trạng thái sử dụng", row.HasValue ? Get("trangThai") : "HOAT_DONG", [new("HOAT_DONG", "Hoạt động"), new("NGUNG", "Ngừng sử dụng")]);
        form.Enable("NgaySinh", form.Flag("CoNgaySinh"));
        form.Changed("CoNgaySinh", () => form.Enable("NgaySinh", form.Flag("CoNgaySinh")));
        form.SaveAsync = async () =>
        {
            if (form.Flag("CoNgaySinh")) ProfileRules.Birth(form.Date("NgaySinh"));
            var body = new { MaThanhVien = form.TextValue("MaThanhVien"), HoTen = ProfileRules.Required(form.TextValue("HoTen"), "Họ tên"),
                NgaySinh = form.Flag("CoNgaySinh") ? Date(form.Date("NgaySinh")) : null,
                DienThoai = ProfileRules.Required(form.TextValue("DienThoai"), "Điện thoại", 25), Email = Optional(form.TextValue("Email")),
                DiaChi = Optional(form.TextValue("DiaChi")), TrangThai = form.TextValue("TrangThai") };
            await Save("thanh-vien", row, body);
        };
        form.ShowDialog(owner); return Task.CompletedTask;
    }

    public async Task Student(IWin32Window owner, JsonElement? row)
    {
        var members = row.HasValue ? [] : await data.All("api/quan-ly/thanh-vien");
        members = members.Where(x => T(x, "trangThai") == "HOAT_DONG").ToList();
        if (!row.HasValue && members.Count == 0) throw new InvalidOperationException("Tạo người liên hệ hoặc tài khoản trước khi thêm học viên.");
        using var form = new InputForm(row.HasValue ? "Sửa hồ sơ học viên" : "Thêm hồ sơ học viên");
        string Get(string key) => row.HasValue ? T(row.Value, key) : "";
        if (!row.HasValue)
        {
            form.Add("ThanhVienId", "Người học / người đại diện", "", People(members, "maThanhVien"));
            form.Add("QuanHe", "Quan hệ với học viên", "CHA", Relations);
        }
        form.Add("MaHocVien", "Mã học viên", row.HasValue ? Get("maHocVien") : NewCode("HV"), readOnly: true);
        form.Add("HoTen", "Họ tên học viên", Get("hoTen"));
        form.Add("NgaySinh", "Ngày sinh học viên", row.HasValue ? Birthday(row.Value) : BusinessTime.Now.Date.AddYears(-18));
        form.Add("GioiTinh", "Giới tính", Get("gioiTinh"), [new("", "Chưa cung cấp"), new("NAM", "Nam"), new("NU", "Nữ"), new("KHAC", "Khác")]);
        form.Add("DiaChi", "Địa chỉ", Get("diaChi"));
        form.Add("LienHeKhanCapTen", "Người liên hệ khẩn cấp (có thể sửa)", Get("lienHeKhanCapTen"));
        form.Add("LienHeKhanCapSDT", "Số điện thoại khẩn cấp", Get("lienHeKhanCapSDT"));
        form.Add("MucTieuHoc", "Mục tiêu học", Get("mucTieuHoc"), multiline: true);
        form.Add("LuuYHoTroHocTap", "Lưu ý hỗ trợ học tập", Get("luuYHoTroHocTap"), multiline: true);
        if (!row.HasValue)
        {
            void Autofill()
            {
                var member = members.Single(x => Id(x).ToString() == form.TextValue("ThanhVienId"));
                if (form.TextValue("QuanHe") == "TU_BAN_THAN")
                {
                    form.Set("HoTen", T(member, "hoTen"));
                    if (T(member, "ngaySinh") != "") form.Set("NgaySinh", Birthday(member));
                    form.Set("DiaChi", T(member, "diaChi"));
                }
                form.Set("LienHeKhanCapTen", T(member, "hoTen"));
                form.Set("LienHeKhanCapSDT", T(member, "dienThoai"));
            }
            form.Changed("ThanhVienId", Autofill); form.Changed("QuanHe", Autofill); Autofill();
        }
        long? createdId = null;
        form.SaveAsync = async () =>
        {
            ProfileRules.Birth(form.Date("NgaySinh"));
            if (!row.HasValue && form.TextValue("QuanHe") == "TU_BAN_THAN") ProfileRules.Self(form.Date("NgaySinh"));
            var body = new { MaHocVien = form.TextValue("MaHocVien"), HoTen = ProfileRules.Required(form.TextValue("HoTen"), "Họ tên"),
                NgaySinh = Date(form.Date("NgaySinh")), GioiTinh = Optional(form.TextValue("GioiTinh")), DiaChi = Optional(form.TextValue("DiaChi")),
                LienHeKhanCapTen = ProfileRules.Required(form.TextValue("LienHeKhanCapTen"), "Họ tên liên hệ khẩn cấp"),
                LienHeKhanCapSDT = ProfileRules.Required(form.TextValue("LienHeKhanCapSDT"), "Điện thoại khẩn cấp", 25),
                MucTieuHoc = Optional(form.TextValue("MucTieuHoc")), LuuYHoTroHocTap = Optional(form.TextValue("LuuYHoTroHocTap")),
                TrangThai = row.HasValue ? Get("trangThai") : "HOAT_DONG" };
            if (row.HasValue) { await Save("hoc-vien", row, body); return; }
            if (createdId is null)
            {
                createdId = Id(await api.Post("api/quan-ly/hoc-vien", body));
                foreach (var key in new[] {"MaHocVien", "HoTen", "NgaySinh", "GioiTinh", "DiaChi", "LienHeKhanCapTen", "LienHeKhanCapSDT", "MucTieuHoc", "LuuYHoTroHocTap"}) form.Enable(key, false);
            }
            try
            {
                var links = await data.All("api/quan-ly/dai-dien");
                if (!links.Any(x => Id(x, "hocVienId") == createdId && Id(x, "thanhVienId").ToString() == form.TextValue("ThanhVienId") && ApiDataService.ActiveLink(x)))
                    await api.Post("api/quan-ly/dai-dien", new { HocVienId = createdId, ThanhVienId = long.Parse(form.TextValue("ThanhVienId")),
                        QuanHe = form.TextValue("QuanHe"), QuyenDaiDien = "DAY_DU", LaLienHeChinh = true, HieuLucTu = Stamp(BusinessTime.Now) });
            }
            catch (Exception ex) { throw new InvalidOperationException($"Hồ sơ học viên #{createdId} đã lưu, nhưng chưa liên kết người đại diện. Bấm Xác nhận để thử liên kết lại; không tạo lại hồ sơ. Nếu đóng cửa sổ, dùng mục Người đại diện để hoàn tất.\n{ex.Message}"); }
        };
        form.ShowDialog(owner);
    }

    public async Task Representative(IWin32Window owner, JsonElement? row)
    {
        var members = await data.All("api/quan-ly/thanh-vien");
        var students = await data.All("api/quan-ly/hoc-vien");
        if (members.Count == 0 || students.Count == 0) throw new InvalidOperationException("Cần có người liên hệ và học viên trước khi liên kết.");
        using var form = new InputForm(row.HasValue ? "Sửa người đại diện" : "Liên kết người đại diện");
        string Get(string key) => row.HasValue ? T(row.Value, key) : "";
        form.Add("ThanhVienId", "Họ tên người đại diện", Get("thanhVienId"), People(members, "maThanhVien"), readOnly: row.HasValue);
        form.Add("HocVienId", "Họ tên học viên", Get("hocVienId"), People(students, "maHocVien"), readOnly: row.HasValue);
        form.Add("QuanHe", "Quan hệ với học viên", row.HasValue ? Get("quanHe") : "CHA", Relations);
        form.Add("QuyenDaiDien", "Quyền đối với hồ sơ", row.HasValue ? Get("quyenDaiDien") : "DAY_DU", [new("DAY_DU", "Toàn quyền đại diện"), new("DANG_KY", "Đăng ký học"), new("XEM_KET_QUA", "Chỉ xem kết quả")]);
        form.Add("LaLienHeChinh", "Liên hệ chính", row.HasValue && ArtApiClient.Get(row.Value, "laLienHeChinh").GetBoolean());
        form.SaveAsync = async () =>
        {
            var studentId = long.Parse(form.TextValue("HocVienId"));
            if (form.TextValue("QuanHe") == "TU_BAN_THAN") ProfileRules.Self(Birthday(students.Single(x => Id(x) == studentId)));
            var body = new { ThanhVienId = long.Parse(form.TextValue("ThanhVienId")), HocVienId = studentId, QuanHe = form.TextValue("QuanHe"),
                QuyenDaiDien = form.TextValue("QuyenDaiDien"), LaLienHeChinh = form.Flag("LaLienHeChinh"),
                HieuLucTu = row.HasValue ? Get("hieuLucTu") : Stamp(BusinessTime.Now) };
            await Save("dai-dien", row, body);
        };
        form.ShowDialog(owner);
    }

    public async Task Stop(IWin32Window owner, string route, JsonElement row)
    {
        var relation = route.EndsWith("/dai-dien");
        if (MessageBox.Show(owner, relation ? "Kết thúc quyền đại diện này? Người này sẽ không còn xem hồ sơ qua liên kết này trên web." : "Ngừng sử dụng hồ sơ này? Lịch sử đã lưu được giữ lại.",
            relation ? "Gỡ liên kết" : "Ngừng sử dụng", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        object body = relation ? new { HieuLucDen = Stamp(BusinessTime.Now), LaLienHeChinh = false } : new { TrangThai = "NGUNG" };
        await api.Put(route + "/" + Id(row), body, T(row, "rowVersion"));
    }
    Task<JsonElement> Save(string name, JsonElement? row, object body) => row.HasValue
        ? api.Put("api/quan-ly/" + name + "/" + Id(row.Value), body, T(row.Value, "rowVersion"))
        : api.Post("api/quan-ly/" + name, body);
}
