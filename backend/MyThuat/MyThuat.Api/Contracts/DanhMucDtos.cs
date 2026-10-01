namespace MyThuat.Api.Contracts;
public sealed record TrangKetQua<T>(int Trang, int KichThuoc, int TongSo, IReadOnlyList<T> DuLieu);
public sealed record LoaiKhoaHocDto(long Id, string MaLoai, string TenLoai, string? MoTa);
public sealed record KhoaHocDto(long Id, long LoaiKhoaHocId, string MaKhoa, string TenKhoa,
    int PhienBan, string CapDo, short? TuoiToiThieu, string? YeuCauDauVao, string MucTieu,
    short SoBuoi, short SoTietMoiBuoi, short PhutMoiTiet, decimal HocPhi);
public sealed record LopHocDto(long Id, long KhoaHocId, string MaLop, string TenLop,
    DateOnly NgayKhaiGiangDuKien, DateOnly? NgayKetThucDuKien, short SoBuoiKeHoach,
    string LichHocDuKien, short SiSoToiDa, decimal HocPhiApDung,
    DateTime MoDangKyLuc, DateTime DongDangKyLuc);
public sealed record NoiDungBuoiDto(long Id, short ThuTu, string ChuDe, string NoiDung,
    short SoTiet, string? YeuCauSanPham);
