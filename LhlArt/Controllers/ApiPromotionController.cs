using System.Globalization;
using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;

internal sealed class ApiPromotionController(ArtApiClient api)
{
    public async Task Edit(IWin32Window owner, JsonElement? row)
    {
        if (api.Role != "QUAN_TRI") throw new InvalidOperationException("Chỉ quản trị được tạo hoặc sửa ưu đãi.");
        var courses = await new ApiDataService(api).All("api/quan-ly/khoa-hoc");
        var options = new List<Choice> { new("0", "Tất cả khóa học (mã nhập riêng)") };
        options.AddRange(courses.Select(x => new Choice(ApiDataService.Id(x).ToString(), ArtApiClient.Text(x, "tenKhoa"))));
        string Text(string key, string fallback = "") => row.HasValue && ArtApiClient.Text(row.Value, key) is { Length: > 0 } value ? value : fallback;
        decimal Number(string key, decimal fallback = 0) => decimal.TryParse(Text(key), CultureInfo.InvariantCulture, out var number) ? number : fallback;
        DateTime Time(string key, DateTime fallback) => DateTime.TryParse(Text(key), CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) ? time : fallback;
        using var form = new InputForm(row.HasValue ? "Sửa mã giảm giá / ưu đãi" : "Tạo mã giảm giá / ưu đãi");
        form.Add("MaKhuyenMai", "Mã ưu đãi", Text("maKhuyenMai"));
        form.Add("TenKhuyenMai", "Tên chương trình", Text("tenKhuyenMai"));
        form.Add("TuDongApDung", "Tự áp dụng và hiển thị trên web", Text("tuDongApDung").Equals("true", StringComparison.OrdinalIgnoreCase));
        form.Add("KhoaApDungId", "Khóa học áp dụng", Text("khoaApDungId", "0"), options);
        form.Add("NhomKhoanThu", "Áp dụng cho", Text("nhomKhoanThu", "HOC_PHI"), [new("HOC_PHI", "Học phí"), new("HOA_CU", "Họa cụ")]);
        form.Add("LoaiGiam", "Cách giảm", Text("loaiGiam", "PHAN_TRAM"), [new("PHAN_TRAM", "Phần trăm (%)"), new("SO_TIEN", "Số tiền (₫)")]);
        form.Add("GiaTriGiam", "Giá trị giảm (% hoặc ₫)", Number("giaTriGiam", 10));
        form.Add("CoTran", "Giới hạn số tiền giảm", Text("tranGiam") != "");
        form.Add("TranGiam", "Giảm tối đa", Number("tranGiam"));
        form.Add("TongLuot", "Tổng lượt sử dụng", (int)Number("tongLuot", 100));
        form.Add("GioiHanNgay", "Giới hạn ngày từ khi tạo thành viên", Text("soNgayTuXacNhanTV") != "");
        form.Add("SoNgayTuXacNhanTV", "Số ngày áp dụng cho thành viên mới", (int)Number("soNgayTuXacNhanTV", 30));
        form.Add("BatDau", "Bắt đầu (giờ Việt Nam)", Time("batDau", BusinessTime.Now));
        form.Add("KetThuc", "Kết thúc (giờ Việt Nam)", Time("ketThuc", BusinessTime.Now.AddDays(30)));
        form.Add("TrangThai", "Trạng thái", Text("trangThai", "HOAT_DONG"), [new("HOAT_DONG", "Hoạt động"), new("NGUNG", "Ngừng áp dụng")]);
        void Mode()
        {
            var automatic = form.Flag("TuDongApDung");
            if (automatic) { form.Set("NhomKhoanThu", "HOC_PHI"); form.Set("GioiHanNgay", false); }
            form.Enable("NhomKhoanThu", !automatic); form.Enable("GioiHanNgay", !automatic);
            form.Enable("SoNgayTuXacNhanTV", !automatic && form.Flag("GioiHanNgay"));
            form.Enable("TranGiam", form.Flag("CoTran"));
        }
        form.Changed("TuDongApDung", Mode); form.Changed("GioiHanNgay", Mode); form.Changed("CoTran", Mode); Mode();
        form.SaveAsync = async () =>
        {
            var code = ProfileRules.Required(form.TextValue("MaKhuyenMai"), "Mã ưu đãi", 30).ToUpperInvariant();
            if (code.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-'))) throw new InvalidOperationException("Mã ưu đãi dùng chữ A–Z, số, dấu gạch ngang hoặc gạch dưới. Tên chương trình có thể viết tiếng Việt.");
            var automatic = form.Flag("TuDongApDung"); var course = long.Parse(form.TextValue("KhoaApDungId"));
            if (automatic && course == 0) throw new InvalidOperationException("Ưu đãi tự động cần chọn một khóa học cụ thể.");
            var amount = form.Number("GiaTriGiam");
            if (amount <= 0 || form.TextValue("LoaiGiam") == "PHAN_TRAM" && amount > 100 || form.TextValue("LoaiGiam") == "SO_TIEN" && amount != decimal.Truncate(amount))
                throw new InvalidOperationException("Mức giảm phải lớn hơn 0; phần trăm không quá 100; số tiền phải nguyên đồng.");
            if (form.Number("TongLuot") <= 0 || form.Flag("CoTran") && form.Number("TranGiam") <= 0 || form.Date("KetThuc") <= form.Date("BatDau"))
                throw new InvalidOperationException("Kiểm tra tổng lượt, mức giảm tối đa và ngày kết thúc.");
            if (form.Flag("GioiHanNgay") && form.Number("SoNgayTuXacNhanTV") is <= 0 or > 32767) throw new InvalidOperationException("Giới hạn ngày cần từ 1 đến 32767.");
            var body = new { MaKhuyenMai = code, TenKhuyenMai = ProfileRules.Required(form.TextValue("TenKhuyenMai"), "Tên chương trình", 200),
                TuDongApDung = automatic, KhoaApDungId = course == 0 ? (long?)null : course, NhomKhoanThu = form.TextValue("NhomKhoanThu"),
                LoaiGiam = form.TextValue("LoaiGiam"), GiaTriGiam = amount, TranGiam = form.Flag("CoTran") ? form.Number("TranGiam") : (decimal?)null,
                TongLuot = (int)form.Number("TongLuot"), SoNgayTuXacNhanTV = form.Flag("GioiHanNgay") ? (short?)form.Number("SoNgayTuXacNhanTV") : null,
                BatDau = form.Date("BatDau").ToString("yyyy-MM-ddTHH:mm:ss"), KetThuc = form.Date("KetThuc").ToString("yyyy-MM-ddTHH:mm:ss"), TrangThai = form.TextValue("TrangThai") };
            if (row.HasValue) await api.Put("api/quan-ly/khuyen-mai/" + ApiDataService.Id(row.Value), body, ArtApiClient.Text(row.Value, "rowVersion"));
            else await api.Post("api/quan-ly/khuyen-mai", body);
        };
        form.ShowDialog(owner);
    }
    public async Task CheckPublic(IWin32Window owner, JsonElement row)
    {
        var data = await api.Read("api/khuyen-mai");
        var name = ArtApiClient.Text(row, "tenKhuyenMai");
        var found = data.EnumerateArray().Any(x => ArtApiClient.Text(x, "tenKhuyenMai") == name && ApiDataService.Id(x, "khoaApDungId") == ApiDataService.Id(row, "khoaApDungId"));
        MessageBox.Show(owner, found ? "Ưu đãi đang có trong danh sách công khai mà website đọc. Tải lại trang web để cập nhật; khi có nhiều ưu đãi, API chọn mức giảm có lợi nhất."
            : "Ưu đãi chưa có trong danh sách công khai. Kiểm tra: tự áp dụng, đúng khóa, đang hoạt động, đã tới ngày bắt đầu và còn lượt. Mã nhập riêng không hiện trên thẻ khóa học.", "Hiển thị trên website");
    }
}
