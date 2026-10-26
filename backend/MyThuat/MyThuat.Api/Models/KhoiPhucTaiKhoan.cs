namespace MyThuat.Api.Models;

public sealed class KhoiPhucTaiKhoan:EntityBase
{
    public string MaYeuCau { get; set; }="";
    public long TaiKhoanId { get; set; }
    public string MucDich { get; set; }="";
    public string EmailNhan { get; set; }="";
    public string MaBam { get; set; }="";
    public int PhienBanBaoMat { get; set; }
    public short SoLanSai { get; set; }
    public DateTime HetHanLuc { get; set; }
    public DateTime? DaDungLuc { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }
}
