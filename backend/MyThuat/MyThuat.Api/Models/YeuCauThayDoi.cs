namespace MyThuat.Api.Models;

// Yêu cầu hủy hoặc chuyển có phương án và quyết định.
public sealed class YeuCauThayDoi : EntityBase
{
    public long DangKyNguonId { get; set; }
    public long? LopDichId { get; set; }
    public long? DangKyDichId { get; set; }
    public long NguoiYeuCauId { get; set; }
    public string LoaiYeuCau { get; set; } = string.Empty;
    public string NguyenNhan { get; set; } = string.Empty;
    public DateTime GuiLuc { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public short? SoBuoiDaSuDungChot { get; set; }
    public decimal? SoTienHoanDuKien { get; set; }
    public decimal? GiaTriChuyenDuKien { get; set; }
    public DateTime? HanThucHien { get; set; }
    public long? NguoiDuyet { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public string? LyDoQuyetDinh { get; set; }
    public DateTime? HoanTatLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string PhuongAnHoanJson { get; set; } = "{}";
    public string RequestFingerprint { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public decimal? GiaTriChuyenThucTe { get; set; }

    public DangKy? DangKyNguon { get; set; }
    public LopHoc? LopDich { get; set; }
    public DangKy? DangKyDich { get; set; }
    public ThanhVien? NguoiYeuCau { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
