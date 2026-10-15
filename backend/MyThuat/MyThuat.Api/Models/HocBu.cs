namespace MyThuat.Api.Models;

// Liên kết một buổi vắng với một lượt bù.
public sealed class HocBu : EntityBase
{
    public long DiemDanhVangId { get; set; }
    public long? DiemDanhBuId { get; set; }
    public DateTime HanHoanTat { get; set; }
    public long? NguoiDuyet { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? LyDo { get; set; }

    public DiemDanh? DiemDanhVang { get; set; }
    public DiemDanh? DiemDanhBu { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
