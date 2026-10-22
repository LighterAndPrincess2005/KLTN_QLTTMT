namespace MyThuat.Api.Models;
public sealed class BaiViet:EntityBase
{
    public string Slug{get;set;}="";public string TieuDe{get;set;}="";public string ChuDe{get;set;}="";
    public string AnhBia{get;set;}="";public string TomTat{get;set;}="";public string NoiDungJson{get;set;}="[]";
    public string BaiThucHanh{get;set;}="";public string Nguon{get;set;}="";public string ThuVienJson{get;set;}="[]";
    public string TrangThai{get;set;}="NHAP";public DateTime? DangLuc{get;set;}public long NguoiSuaId{get;set;}
}
public sealed class GioiThieuGiaoVien:EntityBase
{
    public long GiaoVienId{get;set;}public string AnhDaiDien{get;set;}="";public string GioiThieu{get;set;}="";
    public string KhuVuc{get;set;}="";public string TacPhamJson{get;set;}="[]";public bool CongKhai{get;set;}
    public GiaoVien? GiaoVien{get;set;}
}
public sealed class GioiThieuKhoaHoc:EntityBase
{
    public long KhoaHocId{get;set;}public string AnhDaiDien{get;set;}="";public string DauRaJson{get;set;}="[]";
    public string HoaCuBaoGom{get;set;}="";public string CanChuanBi{get;set;}="";public string ThanhPhamJson{get;set;}="[]";
    public KhoaHoc? KhoaHoc{get;set;}
}
public sealed class TuVan:EntityBase
{
    public string MaTraCuu{get;set;}="";public string KhoaTraCuuHash{get;set;}="";
    public long? ThanhVienId{get;set;}public long? KhoaHocId{get;set;}public string HoTen{get;set;}="";
    public string Email{get;set;}="";public string? DienThoai{get;set;}public string ChuDe{get;set;}="";
    public string TrangThai{get;set;}="MOI_GUI";public DateTime GuiLuc{get;set;}public DateTime? DocLuc{get;set;}
    public long? NguoiXuLyId{get;set;}public ThanhVien? ThanhVien{get;set;}public KhoaHoc? KhoaHoc{get;set;}
}
public sealed class TinNhanTuVan:EntityBase
{
    public long TuVanId{get;set;}public string NoiDung{get;set;}="";public bool TuNhanVien{get;set;}
    public long? TaiKhoanId{get;set;}public DateTime GuiLuc{get;set;}public TuVan? TuVan{get;set;}
}
