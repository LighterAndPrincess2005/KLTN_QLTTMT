namespace MyThuat.Api.Models;
public sealed class PhienThanhToan:EntityBase
{
    public long DangKyId{get;set;}
    public string MaThanhToan{get;set;}="";
}
public sealed class SuKienSePay:EntityBase
{
    public long MaGiaoDichSePay{get;set;}
    public long? DangKyId{get;set;}
    public decimal SoTien{get;set;}
    public string NoiDung{get;set;}="";
    public string MaThamChieu{get;set;}="";
    public string DauVanTay{get;set;}="";
    public string TrangThai{get;set;}="CHO_DOI_CHIEU";
}
