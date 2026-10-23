using System.Text.Json;
using LhlArt.Networking;

namespace LhlArt.Services;

public sealed class ApiDisplayService(ArtApiClient api)
{
    public async Task<List<JsonElement>> Enrich(string route, List<JsonElement> rows)
    {
        var data = new ApiDataService(api);
        if (route is "api/quan-ly/thanh-vien" or "api/quan-ly/hoc-vien" or "api/quan-ly/dai-dien" or "api/tai-khoan")
        {
            if (!api.Has(2)) return rows;
            var members = route.EndsWith("/thanh-vien") ? rows : await data.All("api/quan-ly/thanh-vien");
            var students = route.EndsWith("/hoc-vien") ? rows : await data.All("api/quan-ly/hoc-vien");
            var links = route.EndsWith("/dai-dien") ? rows : await data.All("api/quan-ly/dai-dien");
            var memberNames = members.ToDictionary(x => ApiDataService.Id(x), x => ArtApiClient.Text(x, "hoTen"));
            var studentNames = students.ToDictionary(x => ApiDataService.Id(x), x => ArtApiClient.Text(x, "hoTen"));
            string M(long id) => memberNames.GetValueOrDefault(id, $"Thành viên #{id}");
            string S(long id) => studentNames.GetValueOrDefault(id, $"Học viên #{id}");
            return rows.Select(row => route switch
            {
                "api/quan-ly/dai-dien" => ApiDataService.With(row, ("tenNguoiDaiDien", M(ApiDataService.Id(row, "thanhVienId"))), ("tenHocVien", S(ApiDataService.Id(row, "hocVienId")))),
                "api/quan-ly/hoc-vien" => ApiDataService.With(row, ("nguoiDaiDien", string.Join("; ", links.Where(x => ApiDataService.Id(x, "hocVienId") == ApiDataService.Id(row) && ApiDataService.ActiveLink(x))
                    .Select(x => M(ApiDataService.Id(x, "thanhVienId")) + " (" + Relation(ArtApiClient.Text(x, "quanHe")) + ")")))),
                "api/quan-ly/thanh-vien" => ApiDataService.With(row, ("loaiTaiKhoan", ApiDataService.MemberKind(ApiDataService.Id(row), links))),
                "api/tai-khoan" => ApiDataService.With(row,
                    ("hoTen", ApiDataService.Id(row, "thanhVienId") is > 0 and var member ? M(member) : ArtApiClient.Text(row, "hoTenNhanVien")),
                    ("loaiTaiKhoan", ArtApiClient.Text(row, "vaiTroChinh") == "THANH_VIEN" ? ApiDataService.MemberKind(ApiDataService.Id(row, "thanhVienId"), links) : Role(ArtApiClient.Text(row, "vaiTroChinh")))),
                _ => row
            }).ToList();
        }
        if (route == "api/dang-ky/quan-ly") return rows.Select(x => ApiDataService.With(x, ("kenhHienThi", ApiDataService.Channel(x)))).ToList();
        if (route == "api/quan-ly/khuyen-mai")
        {
            var courses = await data.All("api/quan-ly/khoa-hoc");
            return rows.Select(x => ApiDataService.With(x, ("tenKhoa", courses.Where(c => ApiDataService.Id(c) == ApiDataService.Id(x, "khoaApDungId")).Select(c => ArtApiClient.Text(c, "tenKhoa")).FirstOrDefault() ?? "Tất cả khóa học"),
                ("cachApDung", ArtApiClient.Text(x, "tuDongApDung").Equals("true", StringComparison.OrdinalIgnoreCase) ? "Tự động trên website" : "Nhập mã khi đăng ký"))).ToList();
        }
        return rows;
    }
    public static string Relation(string value) => value switch { "TU_BAN_THAN" => "Bản thân", "CHA" => "Cha", "ME" => "Mẹ", "GIAM_HO" => "Người giám hộ", _ => value };
    public static string Role(string value) => value switch { "QUAN_TRI" => "Quản trị", "HOC_VU" => "Học vụ", "THU_NGAN" => "Thu ngân", "GIAO_VIEN" => "Giáo viên", "QUAN_LY_KHO" => "Quản lý kho", "THANH_VIEN" => "Thành viên", _ => value };
}
