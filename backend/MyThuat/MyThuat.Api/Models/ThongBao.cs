namespace MyThuat.Api.Models;
public sealed class ThongBao:EntityBase
{
    public long? ThanhVienId{get;set;}
    public string Loai{get;set;}="";
    public string TieuDe{get;set;}="";
    public string NoiDung{get;set;}="";
    public string? Email{get;set;}
    public string? DienThoai{get;set;}
    public string TrangThaiEmail{get;set;}="CHO_GUI";
    public int SoLanGui{get;set;}
    public DateTime? ThuLaiLuc{get;set;}
    public DateTime? GuiEmailLuc{get;set;}
    public DateTime? DaDocLuc{get;set;}
    public DateTime? DaGoiLuc{get;set;}
    public long? NguoiGoiId{get;set;}
}
