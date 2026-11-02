namespace MyThuat.Api.Models;
public sealed class ThongBaoLop:EntityBase
{
    public long LopHocId{get;set;}
    public long TaiKhoanId{get;set;}
    public string MaYeuCau{get;set;}="";
    public string TieuDe{get;set;}="";
    public string NoiDung{get;set;}="";
    public int SoNguoiNhan{get;set;}
    public int SoEmail{get;set;}
}
