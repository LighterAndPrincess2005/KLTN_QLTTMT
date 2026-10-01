namespace MyThuat.Api.Models;

// Bộ chính sách vận hành có phiên bản và hiệu lực.
public sealed class QuyDinh : EntityBase
{
    public string MaBoQuyDinh { get; set; } = string.Empty;
    public int SoPhienBan { get; set; }
    public DateTime HieuLucTu { get; set; }
    public DateTime? HieuLucDen { get; set; }
    public string NoiDungJson { get; set; } = string.Empty;
    public long NguoiDuyet { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
