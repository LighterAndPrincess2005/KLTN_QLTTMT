using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;

internal sealed class ApiEnrollmentController(ArtApiClient api)
{
    public async Task<JsonElement?> Create(IWin32Window owner)
    {
        var data = new ApiDataService(api);
        var students = (await data.All("api/quan-ly/hoc-vien")).Where(x => ArtApiClient.Text(x, "trangThai") == "HOAT_DONG").ToList();
        var members = (await data.All("api/quan-ly/thanh-vien")).Where(x => ArtApiClient.Text(x, "trangThai") == "HOAT_DONG").ToList();
        var links = (await data.All("api/quan-ly/dai-dien")).Where(x => ApiDataService.ActiveLink(x) && ArtApiClient.Text(x, "xacNhanLuc") != "" && ArtApiClient.Text(x, "quyenDaiDien") is "DAY_DU" or "DANG_KY").ToList();
        var classes = await data.All("api/lop-hoc");
        if (students.Count == 0 || classes.Count == 0) throw new InvalidOperationException("Cần hồ sơ học viên hoạt động và lớp đang tuyển sinh.");
        using var form = new InputForm("Đăng ký lớp tại trung tâm");
        form.Add("HocVienId", "Học viên", "", students.Select(x => new Choice(ApiDataService.Id(x).ToString(), ArtApiClient.Text(x, "hoTen") + " · " + ArtApiClient.Text(x, "maHocVien"))));
        form.Add("NguoiDangKyId", "Người đăng ký / đại diện", "", Array.Empty<Choice>());
        form.Add("LopHocId", "Lớp học", "", classes.Select(x => new Choice(ApiDataService.Id(x).ToString(), ArtApiClient.Text(x, "tenLop") + " · " + ArtApiClient.Text(x, "lichHocDuKien"))));
        form.Add("MaGiamHocPhi", "Mã giảm học phí (nếu có)");
        void Representatives()
        {
            var ids = links.Where(x => ApiDataService.Id(x, "hocVienId").ToString() == form.TextValue("HocVienId")).Select(x => ApiDataService.Id(x, "thanhVienId")).ToHashSet();
            form.Options("NguoiDangKyId", members.Where(x => ids.Contains(ApiDataService.Id(x))).Select(x => new Choice(ApiDataService.Id(x).ToString(), ArtApiClient.Text(x, "hoTen"))));
        }
        form.Changed("HocVienId", Representatives); Representatives();
        var key = Guid.NewGuid().ToString("N"); string? previousBody = null; JsonElement? saved = null;
        form.SaveAsync = async () =>
        {
            if (string.IsNullOrEmpty(form.TextValue("NguoiDangKyId"))) throw new InvalidOperationException("Chưa có người đại diện đang hoạt động và được quyền đăng ký. Liên kết tại mục Người đại diện trước.");
            var values = new { HocVienId = long.Parse(form.TextValue("HocVienId")), LopHocId = long.Parse(form.TextValue("LopHocId")),
                NguoiDangKyId = long.Parse(form.TextValue("NguoiDangKyId")), MaGiamHocPhi = string.IsNullOrWhiteSpace(form.TextValue("MaGiamHocPhi")) ? null : form.TextValue("MaGiamHocPhi").ToUpperInvariant() };
            var fingerprint = JsonSerializer.Serialize(values);
            if (previousBody != null && previousBody != fingerprint) key = Guid.NewGuid().ToString("N");
            previousBody = fingerprint;
            saved = await api.Post("api/dang-ky", new { values.HocVienId, values.LopHocId, values.NguoiDangKyId, values.MaGiamHocPhi, IdempotencyKey = "WIN-" + key });
        };
        form.ShowDialog(owner);
        if (saved.HasValue) MessageBox.Show(owner, $"Đã ghi danh. ID đăng ký: {ApiDataService.Id(saved.Value)}\nMã đăng ký: {ArtApiClient.Text(saved.Value, "maDangKy")}\nTài khoản người đại diện xem được đăng ký trên website sau khi tải lại.", "Đã lưu vào trung tâm");
        return saved;
    }
    public Task<JsonElement?> Lookup(IWin32Window owner)
    {
        using var form = new InputForm("Tra cứu đăng ký");
        form.Add("Id", "ID đăng ký trên website / phiếu", 1);
        JsonElement? result = null;
        form.SaveAsync = async () =>
        {
            var id = (long)form.Number("Id"); if (id <= 0) throw new InvalidOperationException("ID đăng ký phải lớn hơn 0.");
            result = ArtApiClient.Get(await api.Read("api/dang-ky/" + id), "dangKy").Clone();
        };
        form.ShowDialog(owner); return Task.FromResult(result);
    }
}
