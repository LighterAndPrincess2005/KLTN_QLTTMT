using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Models;
namespace MyThuat.Api.Data;

public class MyThuatDbContext(DbContextOptions<MyThuatDbContext> options) : DbContext(options)
{
    public DbSet<LoaiKhoaHoc> LoaiKhoaHoc => Set<LoaiKhoaHoc>();
    public DbSet<KhoaHoc> KhoaHoc => Set<KhoaHoc>();
    public DbSet<NoiDungBuoi> NoiDungBuoi => Set<NoiDungBuoi>();
    public DbSet<LopHoc> LopHoc => Set<LopHoc>();
    public DbSet<PhongHoc> PhongHoc => Set<PhongHoc>();
    public DbSet<GiaoVien> GiaoVien => Set<GiaoVien>();
    public DbSet<PhanCong> PhanCong => Set<PhanCong>();
    public DbSet<BuoiHoc> BuoiHoc => Set<BuoiHoc>();
    public DbSet<GiangDayBuoi> GiangDayBuoi => Set<GiangDayBuoi>();
    public DbSet<ThanhVien> ThanhVien => Set<ThanhVien>();
    public DbSet<HocVien> HocVien => Set<HocVien>();
    public DbSet<DaiDienHocVien> DaiDienHocVien => Set<DaiDienHocVien>();
    public DbSet<HoSoTheoHoc> HoSoTheoHoc => Set<HoSoTheoHoc>();
    public DbSet<DangKy> DangKy => Set<DangKy>();
    public DbSet<KhoanThuDangKy> KhoanThuDangKy => Set<KhoanThuDangKy>();
    public DbSet<KhuyenMai> KhuyenMai => Set<KhuyenMai>();
    public DbSet<ApDungUuDai> ApDungUuDai => Set<ApDungUuDai>();
    public DbSet<GiaoDichTien> GiaoDichTien => Set<GiaoDichTien>();
    public DbSet<YeuCauThayDoi> YeuCauThayDoi => Set<YeuCauThayDoi>();
    public DbSet<DiemDanh> DiemDanh => Set<DiemDanh>();
    public DbSet<HocBu> HocBu => Set<HocBu>();
    public DbSet<KetQuaKhoaHoc> KetQuaKhoaHoc => Set<KetQuaKhoaHoc>();
    public DbSet<PhanHoi> PhanHoi => Set<PhanHoi>();
    public DbSet<HoaCu> HoaCu => Set<HoaCu>();
    public DbSet<NhaCungCap> NhaCungCap => Set<NhaCungCap>();
    public DbSet<DinhMucHoaCu> DinhMucHoaCu => Set<DinhMucHoaCu>();
    public DbSet<PhieuKho> PhieuKho => Set<PhieuKho>();
    public DbSet<ChiTietPhieuKho> ChiTietPhieuKho => Set<ChiTietPhieuKho>();
    public DbSet<TaiKhoan> TaiKhoan => Set<TaiKhoan>();
    public DbSet<QuyDinh> QuyDinh => Set<QuyDinh>();
    public DbSet<NhatKyThayDoi> NhatKyThayDoi => Set<NhatKyThayDoi>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = Services.BusinessClock.Now;
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    {
        var e = modelBuilder.Entity<LoaiKhoaHoc>();
        e.ToTable("LoaiKhoaHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaLoai).HasColumnType("varchar(20)").IsRequired();
        e.HasIndex(x => x.MaLoai).IsUnique();
        e.Property(x => x.TenLoai).HasColumnType("nvarchar(150)").IsRequired();
        e.Property(x => x.MoTa).HasColumnType("nvarchar(1000)");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<KhoaHoc>();
        e.ToTable("KhoaHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.LoaiKhoaHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.LoaiKhoaHoc).WithMany().HasForeignKey(x => x.LoaiKhoaHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.KhoaGocId).HasColumnType("bigint");
        e.HasOne(x => x.KhoaGoc).WithMany().HasForeignKey(x => x.KhoaGocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.MaKhoa).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.TenKhoa).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.PhienBan).HasColumnType("int").IsRequired();
        e.Property(x => x.CapDo).HasColumnType("nvarchar(100)").IsRequired();
        e.Property(x => x.TuoiToiThieu).HasColumnType("smallint");
        e.Property(x => x.YeuCauDauVao).HasColumnType("nvarchar(1000)");
        e.Property(x => x.MucTieu).HasColumnType("nvarchar(2000)").IsRequired();
        e.Property(x => x.SoBuoi).HasColumnType("smallint").IsRequired();
        e.Property(x => x.SoTietMoiBuoi).HasColumnType("smallint").IsRequired();
        e.Property(x => x.PhutMoiTiet).HasColumnType("smallint").IsRequired();
        e.Property(x => x.HocPhi).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.HieuLucTu).HasColumnType("date").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<NoiDungBuoi>();
        e.ToTable("NoiDungBuoi");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.KhoaHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.KhoaHoc).WithMany().HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.ThuTu).HasColumnType("smallint").IsRequired();
        e.Property(x => x.ChuDe).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.NoiDung).HasColumnType("nvarchar(2000)").IsRequired();
        e.Property(x => x.SoTiet).HasColumnType("smallint").IsRequired();
        e.Property(x => x.YeuCauSanPham).HasColumnType("nvarchar(1000)");
    }
    {
        var e = modelBuilder.Entity<LopHoc>();
        e.ToTable("LopHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.KhoaHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.KhoaHoc).WithMany().HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.MaLop).HasColumnType("varchar(30)").IsRequired();
        e.HasIndex(x => x.MaLop).IsUnique();
        e.Property(x => x.TenLop).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.NgayKhaiGiangDuKien).HasColumnType("date").IsRequired();
        e.Property(x => x.NgayKhaiGiangThucTe).HasColumnType("date");
        e.Property(x => x.NgayKetThucDuKien).HasColumnType("date");
        e.Property(x => x.SoBuoiKeHoach).HasColumnType("smallint").IsRequired();
        e.Property(x => x.LichHocDuKien).HasColumnType("nvarchar(500)").IsRequired();
        e.Property(x => x.SiSoToiThieu).HasColumnType("smallint").IsRequired();
        e.Property(x => x.SiSoToiDa).HasColumnType("smallint").IsRequired();
        e.Property(x => x.HocPhiApDung).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.MoDangKyLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.DongDangKyLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint");
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<PhongHoc>();
        e.ToTable("PhongHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaPhong).HasColumnType("varchar(20)").IsRequired();
        e.HasIndex(x => x.MaPhong).IsUnique();
        e.Property(x => x.TenPhong).HasColumnType("nvarchar(100)").IsRequired();
        e.Property(x => x.SucChua).HasColumnType("smallint").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<GiaoVien>();
        e.ToTable("GiaoVien");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaGiaoVien).HasColumnType("varchar(20)").IsRequired();
        e.HasIndex(x => x.MaGiaoVien).IsUnique();
        e.Property(x => x.HoTen).HasColumnType("nvarchar(150)").IsRequired();
        e.Property(x => x.NgaySinh).HasColumnType("date");
        e.Property(x => x.GioiTinh).HasColumnType("varchar(20)");
        e.Property(x => x.DienThoai).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.Email).HasColumnType("nvarchar(254)");
        e.Property(x => x.DiaChi).HasColumnType("nvarchar(300)");
        e.Property(x => x.TrinhDo).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.ChuyenMon).HasColumnType("nvarchar(1000)").IsRequired();
        e.Property(x => x.CapDoCoTheDay).HasColumnType("nvarchar(500)").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<PhanCong>();
        e.ToTable("PhanCong");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.LopHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.LopHoc).WithMany().HasForeignKey(x => x.LopHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.GiaoVienId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.GiaoVien).WithMany().HasForeignKey(x => x.GiaoVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.VaiTroTrongLop).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.TuNgay).HasColumnType("date").IsRequired();
        e.Property(x => x.DenNgay).HasColumnType("date");
        e.Property(x => x.XacNhanDuChuyenMon).HasColumnType("bit").IsRequired();
        e.Property(x => x.XacNhanNhanLopLuc).HasColumnType("datetime2");
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
    }
    {
        var e = modelBuilder.Entity<BuoiHoc>();
        e.ToTable("BuoiHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.LopHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.LopHoc).WithMany().HasForeignKey(x => x.LopHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NoiDungBuoiId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NoiDungBuoi).WithMany().HasForeignKey(x => x.NoiDungBuoiId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.PhongHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.PhongHoc).WithMany().HasForeignKey(x => x.PhongHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.BuoiThayTheChoId).HasColumnType("bigint");
        e.HasOne(x => x.BuoiThayTheCho).WithMany().HasForeignKey(x => x.BuoiThayTheChoId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.ThuTuTrongLop).HasColumnType("smallint").IsRequired();
        e.Property(x => x.LanXepLich).HasColumnType("smallint").IsRequired();
        e.Property(x => x.BatDau).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.KetThuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.SoTiet).HasColumnType("smallint").IsRequired();
        e.Property(x => x.PhutMoiTiet).HasColumnType("smallint").IsRequired();
        e.Property(x => x.PhutNghi).HasColumnType("smallint").IsRequired();
        e.Property(x => x.LyDoDoiLich).HasColumnType("nvarchar(1000)");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<GiangDayBuoi>();
        e.ToTable("GiangDayBuoi");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.BuoiHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.BuoiHoc).WithMany().HasForeignKey(x => x.BuoiHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.GiaoVienId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.GiaoVien).WithMany().HasForeignKey(x => x.GiaoVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.VaiTro).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.SoTietThucDay).HasColumnType("smallint").IsRequired();
        e.Property(x => x.LaThayThe).HasColumnType("bit").IsRequired();
        e.Property(x => x.LyDoThayThe).HasColumnType("nvarchar(500)");
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
    }
    {
        var e = modelBuilder.Entity<ThanhVien>();
        e.ToTable("ThanhVien");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaThanhVien).HasColumnType("varchar(20)").IsRequired();
        e.HasIndex(x => x.MaThanhVien).IsUnique();
        e.Property(x => x.HoTen).HasColumnType("nvarchar(150)").IsRequired();
        e.Property(x => x.DienThoai).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.Email).HasColumnType("nvarchar(254)");
        e.Property(x => x.DiaChi).HasColumnType("nvarchar(300)");
        e.Property(x => x.XacNhanThanhVienLuc).HasColumnType("datetime2");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.NhuCauTuVan).HasColumnType("nvarchar(2000)");
        e.Property(x => x.KetQuaTuVan).HasColumnType("nvarchar(2000)");
        e.Property(x => x.HanPhanHoiTuVan).HasColumnType("datetime2");
        e.Property(x => x.NguoiTuVan).HasColumnType("bigint");
        e.HasOne(x => x.NguoiTuVanTaiKhoan).WithMany().HasForeignKey(x => x.NguoiTuVan).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TrangThaiTuVan).HasColumnType("varchar(20)");
        e.Property(x => x.DongYQuangBa).HasColumnType("bit").IsRequired();
        e.Property(x => x.ThoiDiemDongYQuangBa).HasColumnType("datetime2");
    }
    {
        var e = modelBuilder.Entity<HocVien>();
        e.ToTable("HocVien");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaHocVien).HasColumnType("varchar(20)").IsRequired();
        e.HasIndex(x => x.MaHocVien).IsUnique();
        e.Property(x => x.HoTen).HasColumnType("nvarchar(150)").IsRequired();
        e.Property(x => x.NgaySinh).HasColumnType("date").IsRequired();
        e.Property(x => x.GioiTinh).HasColumnType("varchar(20)");
        e.Property(x => x.DiaChi).HasColumnType("nvarchar(300)");
        e.Property(x => x.MucTieuHoc).HasColumnType("nvarchar(1000)");
        e.Property(x => x.LuuYHoTroHocTap).HasColumnType("nvarchar(1000)");
        e.Property(x => x.LienHeKhanCapTen).HasColumnType("nvarchar(150)").IsRequired();
        e.Property(x => x.LienHeKhanCapSDT).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.NhanXetDauVao).HasColumnType("nvarchar(2000)");
        e.Property(x => x.CapDoDeXuat).HasColumnType("nvarchar(100)");
        e.Property(x => x.NgayDanhGiaDauVao).HasColumnType("date");
        e.Property(x => x.GiaoVienDanhGia).HasColumnType("bigint");
        e.HasOne(x => x.GiaoVienDanhGiaHoSo).WithMany().HasForeignKey(x => x.GiaoVienDanhGia).OnDelete(DeleteBehavior.NoAction);
    }
    {
        var e = modelBuilder.Entity<DaiDienHocVien>();
        e.ToTable("DaiDienHocVien");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.ThanhVienId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.ThanhVien).WithMany().HasForeignKey(x => x.ThanhVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.HocVienId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.HocVien).WithMany().HasForeignKey(x => x.HocVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.QuanHe).HasColumnType("varchar(30)").IsRequired();
        e.Property(x => x.LaLienHeChinh).HasColumnType("bit").IsRequired();
        e.Property(x => x.QuyenDaiDien).HasColumnType("nvarchar(300)").IsRequired();
        e.Property(x => x.HieuLucTu).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.HieuLucDen).HasColumnType("datetime2");
        e.Property(x => x.XacNhanLuc).HasColumnType("datetime2");
        e.Property(x => x.NguoiXacNhan).HasColumnType("bigint");
        e.HasOne(x => x.NguoiXacNhanTaiKhoan).WithMany().HasForeignKey(x => x.NguoiXacNhan).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.DongYHinhAnh).HasColumnType("bit").IsRequired();
        e.Property(x => x.DongYTacPham).HasColumnType("bit").IsRequired();
        e.Property(x => x.ThoiDiemDongY).HasColumnType("datetime2");
    }
    {
        var e = modelBuilder.Entity<HoSoTheoHoc>();
        e.ToTable("HoSoTheoHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.HocVienId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.HocVien).WithMany().HasForeignKey(x => x.HocVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.KhoaHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.KhoaHoc).WithMany().HasForeignKey(x => x.KhoaHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.QuyDinhId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.QuyDinh).WithMany().HasForeignKey(x => x.QuyDinhId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NgayBatDau).HasColumnType("date").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<DangKy>();
        e.ToTable("DangKy");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaDangKy).HasColumnType("varchar(30)").IsRequired();
        e.HasIndex(x => x.MaDangKy).IsUnique();
        e.Property(x => x.HoSoTheoHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.HoSoTheoHoc).WithMany().HasForeignKey(x => x.HoSoTheoHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LopHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.LopHoc).WithMany().HasForeignKey(x => x.LopHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NguoiDangKyId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiDangKy).WithMany().HasForeignKey(x => x.NguoiDangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.DangKyNguonId).HasColumnType("bigint");
        e.HasOne(x => x.DangKyNguon).WithMany().HasForeignKey(x => x.DangKyNguonId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LapLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.HanGiuCho).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.HieuLucTu).HasColumnType("datetime2");
        e.Property(x => x.HieuLucDen).HasColumnType("datetime2");
        e.Property(x => x.HocPhiGocChot).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.IdempotencyKey).HasColumnType("varchar(100)").IsRequired();
        e.HasIndex(x => x.IdempotencyKey).IsUnique();
    }
    {
        var e = modelBuilder.Entity<KhoanThuDangKy>();
        e.ToTable("KhoanThuDangKy");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DangKyId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.DangKy).WithMany().HasForeignKey(x => x.DangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LoaiKhoan).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.HoaCuId).HasColumnType("bigint");
        e.HasOne(x => x.HoaCu).WithMany().HasForeignKey(x => x.HoaCuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.MoTaChot).HasColumnType("nvarchar(300)").IsRequired();
        e.Property(x => x.SoLuong).HasColumnType("decimal(18,3)").IsRequired();
        e.Property(x => x.DonGiaChot).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.TienGiamChot).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.ThanhTienChot).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.SoLuongDaGiao).HasColumnType("decimal(18,3)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<KhuyenMai>();
        e.ToTable("KhuyenMai");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaKhuyenMai).HasColumnType("varchar(30)").IsRequired();
        e.HasIndex(x => x.MaKhuyenMai).IsUnique();
        e.Property(x => x.TenKhuyenMai).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.NhomKhoanThu).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.LoaiGiam).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.GiaTriGiam).HasColumnType("decimal(18,2)").IsRequired();
        e.Property(x => x.TranGiam).HasColumnType("decimal(18,0)");
        e.Property(x => x.TongLuot).HasColumnType("int").IsRequired();
        e.Property(x => x.SoNgayTuXacNhanTV).HasColumnType("smallint");
        e.Property(x => x.BatDau).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.KetThuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.KhoaApDungId).HasColumnType("bigint");
        e.HasOne(x => x.KhoaApDung).WithMany().HasForeignKey(x => x.KhoaApDungId).OnDelete(DeleteBehavior.NoAction);
    }
    {
        var e = modelBuilder.Entity<ApDungUuDai>();
        e.ToTable("ApDungUuDai");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DangKyId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.DangKy).WithMany().HasForeignKey(x => x.DangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.KhuyenMaiId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.KhuyenMai).WithMany().HasForeignKey(x => x.KhuyenMaiId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NhomKhoanThu).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.GiaTriGiamChot).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.HanGiuLuot).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.TrangThaiLuot).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<GiaoDichTien>();
        e.ToTable("GiaoDichTien");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DangKyId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.DangKy).WithMany().HasForeignKey(x => x.DangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.KhoanThuId).HasColumnType("bigint");
        e.HasOne(x => x.KhoanThu).WithMany().HasForeignKey(x => x.KhoanThuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.GiaoDichThuGocId).HasColumnType("bigint");
        e.HasOne(x => x.GiaoDichThuGoc).WithMany().HasForeignKey(x => x.GiaoDichThuGocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.YeuCauThayDoiId).HasColumnType("bigint");
        e.HasOne(x => x.YeuCauThayDoi).WithMany().HasForeignKey(x => x.YeuCauThayDoiId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.MaChungTu).HasColumnType("varchar(50)");
        e.Property(x => x.MaThamChieu).HasColumnType("varchar(100)");
        e.Property(x => x.LoaiGiaoDich).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.PhuongThuc).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.SoTien).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.ThoiDiem).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.NguoiXacNhan).HasColumnType("bigint");
        e.HasOne(x => x.NguoiXacNhanTaiKhoan).WithMany().HasForeignKey(x => x.NguoiXacNhan).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.IdempotencyKey).HasColumnType("varchar(100)").IsRequired();
        e.HasIndex(x => x.IdempotencyKey).IsUnique();
    }
    {
        var e = modelBuilder.Entity<YeuCauThayDoi>();
        e.ToTable("YeuCauThayDoi");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DangKyNguonId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.DangKyNguon).WithMany().HasForeignKey(x => x.DangKyNguonId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LopDichId).HasColumnType("bigint");
        e.HasOne(x => x.LopDich).WithMany().HasForeignKey(x => x.LopDichId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.DangKyDichId).HasColumnType("bigint");
        e.HasOne(x => x.DangKyDich).WithMany().HasForeignKey(x => x.DangKyDichId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NguoiYeuCauId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiYeuCau).WithMany().HasForeignKey(x => x.NguoiYeuCauId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LoaiYeuCau).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.NguyenNhan).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.GuiLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.LyDo).HasColumnType("nvarchar(1000)").IsRequired();
        e.Property(x => x.SoBuoiDaSuDungChot).HasColumnType("smallint");
        e.Property(x => x.SoTienHoanDuKien).HasColumnType("decimal(18,0)");
        e.Property(x => x.GiaTriChuyenDuKien).HasColumnType("decimal(18,0)");
        e.Property(x => x.HanThucHien).HasColumnType("datetime2");
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint");
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.DuyetLuc).HasColumnType("datetime2");
        e.Property(x => x.LyDoQuyetDinh).HasColumnType("nvarchar(1000)");
        e.Property(x => x.HoanTatLuc).HasColumnType("datetime2");
        e.Property(x => x.TrangThai).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.IdempotencyKey).HasColumnType("varchar(100)").IsRequired();
        e.HasIndex(x => x.IdempotencyKey).IsUnique();
        e.Property(x => x.GiaTriChuyenThucTe).HasColumnType("decimal(18,0)");
    }
    {
        var e = modelBuilder.Entity<DiemDanh>();
        e.ToTable("DiemDanh");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DangKyId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.DangKy).WithMany().HasForeignKey(x => x.DangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.BuoiHocId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.BuoiHoc).WithMany().HasForeignKey(x => x.BuoiHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NoiDungDuocTinhId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NoiDungDuocTinh).WithMany().HasForeignKey(x => x.NoiDungDuocTinhId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LoaiThamGia).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.TrangThai).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.BaoNghiLuc).HasColumnType("datetime2");
        e.Property(x => x.PhutThamDu).HasColumnType("smallint");
        e.Property(x => x.NhanXet).HasColumnType("nvarchar(2000)");
        e.Property(x => x.SanPhamUrl).HasColumnType("nvarchar(500)");
        e.Property(x => x.DiemSanPham).HasColumnType("decimal(4,2)");
        e.Property(x => x.NguoiGhi).HasColumnType("bigint");
        e.HasOne(x => x.NguoiGhiTaiKhoan).WithMany().HasForeignKey(x => x.NguoiGhi).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.GhiLuc).HasColumnType("datetime2");
    }
    {
        var e = modelBuilder.Entity<HocBu>();
        e.ToTable("HocBu");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DiemDanhVangId).HasColumnType("bigint").IsRequired();
        e.HasIndex(x => x.DiemDanhVangId).IsUnique();
        e.HasOne(x => x.DiemDanhVang).WithMany().HasForeignKey(x => x.DiemDanhVangId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.DiemDanhBuId).HasColumnType("bigint");
        e.HasIndex(x => x.DiemDanhBuId).IsUnique().HasFilter("[DiemDanhBuId] IS NOT NULL");
        e.HasOne(x => x.DiemDanhBu).WithMany().HasForeignKey(x => x.DiemDanhBuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.HanHoanTat).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint");
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.LyDo).HasColumnType("nvarchar(500)");
    }
    {
        var e = modelBuilder.Entity<KetQuaKhoaHoc>();
        e.ToTable("KetQuaKhoaHoc");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.HoSoTheoHocId).HasColumnType("bigint").IsRequired();
        e.HasIndex(x => x.HoSoTheoHocId).IsUnique();
        e.HasOne(x => x.HoSoTheoHoc).WithMany().HasForeignKey(x => x.HoSoTheoHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.SoBuoiHopLeChot).HasColumnType("smallint").IsRequired();
        e.Property(x => x.DiemCuoiKhoaChot).HasColumnType("decimal(4,2)").IsRequired();
        e.Property(x => x.KetLuan).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.NhanXetTongKet).HasColumnType("nvarchar(2000)");
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.ChotLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.SoXacNhan).HasColumnType("varchar(50)");
        e.Property(x => x.NgayCap).HasColumnType("date");
        e.Property(x => x.BaiCuoiKhoaUrl).HasColumnType("nvarchar(500)");
    }
    {
        var e = modelBuilder.Entity<PhanHoi>();
        e.ToTable("PhanHoi");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.DangKyId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.DangKy).WithMany().HasForeignKey(x => x.DangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NguoiGuiId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiGui).WithMany().HasForeignKey(x => x.NguoiGuiId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NoiDung).HasColumnType("nvarchar(2000)").IsRequired();
        e.Property(x => x.GuiLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.NguoiXuLy).HasColumnType("bigint");
        e.HasOne(x => x.NguoiXuLyTaiKhoan).WithMany().HasForeignKey(x => x.NguoiXuLy).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.HanPhanHoi).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.TraLoi).HasColumnType("nvarchar(2000)");
        e.Property(x => x.TraLoiLuc).HasColumnType("datetime2");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<HoaCu>();
        e.ToTable("HoaCu");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaHoaCu).HasColumnType("varchar(30)").IsRequired();
        e.HasIndex(x => x.MaHoaCu).IsUnique();
        e.Property(x => x.TenHoaCu).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.QuyCach).HasColumnType("nvarchar(300)").IsRequired();
        e.Property(x => x.MauSac).HasColumnType("nvarchar(100)");
        e.Property(x => x.DonViCoSo).HasColumnType("nvarchar(30)").IsRequired();
        e.Property(x => x.NhomHoaCu).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.GiaBanThamKhao).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.MucDatHang).HasColumnType("decimal(18,3)").IsRequired();
        e.Property(x => x.CanhBaoAnToan).HasColumnType("nvarchar(1000)");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<NhaCungCap>();
        e.ToTable("NhaCungCap");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaNCC).HasColumnType("varchar(20)").IsRequired();
        e.HasIndex(x => x.MaNCC).IsUnique();
        e.Property(x => x.TenNCC).HasColumnType("nvarchar(200)").IsRequired();
        e.Property(x => x.NguoiLienHe).HasColumnType("nvarchar(150)");
        e.Property(x => x.DienThoai).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.Email).HasColumnType("nvarchar(254)");
        e.Property(x => x.DiaChi).HasColumnType("nvarchar(300)");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<DinhMucHoaCu>();
        e.ToTable("DinhMucHoaCu");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.NoiDungBuoiId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NoiDungBuoi).WithMany().HasForeignKey(x => x.NoiDungBuoiId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.HoaCuId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.HoaCu).WithMany().HasForeignKey(x => x.HoaCuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.CoSoTinh).HasColumnType("varchar(30)").IsRequired();
        e.Property(x => x.SoLuong).HasColumnType("decimal(18,3)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<PhieuKho>();
        e.ToTable("PhieuKho");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaPhieu).HasColumnType("varchar(30)").IsRequired();
        e.HasIndex(x => x.MaPhieu).IsUnique();
        e.Property(x => x.LoaiPhieu).HasColumnType("varchar(25)").IsRequired();
        e.Property(x => x.NhaCungCapId).HasColumnType("bigint");
        e.HasOne(x => x.NhaCungCap).WithMany().HasForeignKey(x => x.NhaCungCapId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.BuoiHocId).HasColumnType("bigint");
        e.HasOne(x => x.BuoiHoc).WithMany().HasForeignKey(x => x.BuoiHocId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.PhieuThamChieuId).HasColumnType("bigint");
        e.HasOne(x => x.PhieuThamChieu).WithMany().HasForeignKey(x => x.PhieuThamChieuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LapLuc).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.DuyetLuc).HasColumnType("datetime2");
        e.Property(x => x.LyDo).HasColumnType("nvarchar(1000)");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.NguoiLap).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiLapTaiKhoan).WithMany().HasForeignKey(x => x.NguoiLap).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint");
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.NguoiNhan).HasColumnType("bigint");
        e.HasOne(x => x.NguoiNhanTaiKhoan).WithMany().HasForeignKey(x => x.NguoiNhan).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TepBienBanUrl).HasColumnType("nvarchar(500)");
    }
    {
        var e = modelBuilder.Entity<ChiTietPhieuKho>();
        e.ToTable("ChiTietPhieuKho");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.PhieuKhoId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.PhieuKho).WithMany().HasForeignKey(x => x.PhieuKhoId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.HoaCuId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.HoaCu).WithMany().HasForeignKey(x => x.HoaCuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.ChiTietThamChieuId).HasColumnType("bigint");
        e.HasOne(x => x.ChiTietThamChieu).WithMany().HasForeignKey(x => x.ChiTietThamChieuId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.KhoanThuDangKyId).HasColumnType("bigint");
        e.HasOne(x => x.KhoanThuDangKy).WithMany().HasForeignKey(x => x.KhoanThuDangKyId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.MaLo).HasColumnType("varchar(50)");
        e.Property(x => x.HanSuDung).HasColumnType("date");
        e.Property(x => x.SoLuong).HasColumnType("decimal(18,3)").IsRequired();
        e.Property(x => x.DonGiaChot).HasColumnType("decimal(18,0)").IsRequired();
        e.Property(x => x.SoLuongSoSachTaiKho).HasColumnType("decimal(18,3)");
        e.Property(x => x.SoLuongThucTeTaiKho).HasColumnType("decimal(18,3)");
        e.Property(x => x.SoLuongSoSachDangMuon).HasColumnType("decimal(18,3)");
        e.Property(x => x.SoLuongThucTeDangMuon).HasColumnType("decimal(18,3)");
        e.Property(x => x.GhiChu).HasColumnType("nvarchar(1000)");
    }
    {
        var e = modelBuilder.Entity<TaiKhoan>();
        e.ToTable("TaiKhoan");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.GiaoVienId).HasColumnType("bigint");
        e.HasIndex(x => x.GiaoVienId).IsUnique().HasFilter("[GiaoVienId] IS NOT NULL");
        e.HasOne(x => x.GiaoVien).WithMany().HasForeignKey(x => x.GiaoVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.ThanhVienId).HasColumnType("bigint");
        e.HasIndex(x => x.ThanhVienId).IsUnique().HasFilter("[ThanhVienId] IS NOT NULL");
        e.HasOne(x => x.ThanhVien).WithMany().HasForeignKey(x => x.ThanhVienId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TenDangNhap).HasColumnType("nvarchar(100)").IsRequired();
        e.HasIndex(x => x.TenDangNhap).IsUnique();
        e.Property(x => x.PasswordHash).HasColumnType("nvarchar(500)").IsRequired();
        e.Property(x => x.HoTenNhanVien).HasColumnType("nvarchar(150)");
        e.Property(x => x.DienThoaiNhanVien).HasColumnType("varchar(25)");
        e.Property(x => x.Email).HasColumnType("nvarchar(254)");
        e.Property(x => x.VaiTroChinh).HasColumnType("varchar(30)").IsRequired();
        e.Property(x => x.QuyenBoSung).HasColumnType("bigint").IsRequired();
        e.Property(x => x.EmailXacNhanLuc).HasColumnType("datetime2");
        e.Property(x => x.TokenXacNhanHash).HasColumnType("varchar(128)");
        e.Property(x => x.TokenXacNhanHetHan).HasColumnType("datetime2");
        e.Property(x => x.KhoaDenLuc).HasColumnType("datetime2");
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
        e.Property(x => x.PhienBanBaoMat).HasColumnType("int").IsRequired();
    }
    {
        var e = modelBuilder.Entity<QuyDinh>();
        e.ToTable("QuyDinh");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.MaBoQuyDinh).HasColumnType("varchar(30)").IsRequired();
        e.Property(x => x.SoPhienBan).HasColumnType("int").IsRequired();
        e.Property(x => x.HieuLucTu).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.HieuLucDen).HasColumnType("datetime2");
        e.Property(x => x.NoiDungJson).HasColumnType("nvarchar(max)").IsRequired();
        e.Property(x => x.NguoiDuyet).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.NguoiDuyetTaiKhoan).WithMany().HasForeignKey(x => x.NguoiDuyet).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TrangThai).HasColumnType("varchar(20)").IsRequired();
    }
    {
        var e = modelBuilder.Entity<NhatKyThayDoi>();
        e.ToTable("NhatKyThayDoi");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnType("bigint").IsRequired();
        e.Property(x => x.TaiKhoanId).HasColumnType("bigint").IsRequired();
        e.HasOne(x => x.TaiKhoan).WithMany().HasForeignKey(x => x.TaiKhoanId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.LoaiDoiTuong).HasColumnType("varchar(50)").IsRequired();
        e.Property(x => x.IdDoiTuong).HasColumnType("bigint").IsRequired();
        e.Property(x => x.HanhDong).HasColumnType("varchar(50)").IsRequired();
        e.Property(x => x.TruocJson).HasColumnType("nvarchar(max)");
        e.Property(x => x.SauJson).HasColumnType("nvarchar(max)");
        e.Property(x => x.ThoiDiem).HasColumnType("datetime2").IsRequired();
        e.Property(x => x.CorrelationId).HasColumnType("varchar(100)").IsRequired();
        e.Property(x => x.Kenh).HasColumnType("varchar(20)").IsRequired();
    }
    modelBuilder.Entity<KhoaHoc>().HasIndex(x => new { x.MaKhoa, x.PhienBan }).IsUnique();
    modelBuilder.Entity<NoiDungBuoi>().HasIndex(x => new { x.KhoaHocId, x.ThuTu }).IsUnique();
    modelBuilder.Entity<BuoiHoc>().HasIndex(x => new { x.LopHocId, x.NoiDungBuoiId, x.LanXepLich }).IsUnique();
    modelBuilder.Entity<GiangDayBuoi>().HasIndex(x => new { x.BuoiHocId, x.GiaoVienId, x.VaiTro }).IsUnique();
    modelBuilder.Entity<DaiDienHocVien>().HasIndex(x => new { x.ThanhVienId, x.HocVienId, x.HieuLucTu }).IsUnique();
    modelBuilder.Entity<ApDungUuDai>().HasIndex(x => new { x.DangKyId, x.NhomKhoanThu }).IsUnique();
    modelBuilder.Entity<DiemDanh>().HasIndex(x => new { x.DangKyId, x.BuoiHocId }).IsUnique();
    modelBuilder.Entity<DinhMucHoaCu>().HasIndex(x => new { x.NoiDungBuoiId, x.HoaCuId, x.CoSoTinh }).IsUnique();
    modelBuilder.Entity<QuyDinh>().HasIndex(x => new { x.MaBoQuyDinh, x.SoPhienBan }).IsUnique();
    modelBuilder.Entity<KhoaHoc>().ToTable(t => t.HasCheckConstraint("CK_KhoaHoc_DuLieu", "[SoBuoi] > 0 AND [SoTietMoiBuoi] > 0 AND [PhutMoiTiet] > 0 AND [HocPhi] >= 0 AND [PhienBan] > 0 AND ([TuoiToiThieu] IS NULL OR [TuoiToiThieu] >= 0)"));
    modelBuilder.Entity<NoiDungBuoi>().ToTable(t => t.HasCheckConstraint("CK_NoiDungBuoi_DuLieu", "[ThuTu] > 0 AND [SoTiet] > 0"));
    modelBuilder.Entity<LopHoc>().ToTable(t => t.HasCheckConstraint("CK_LopHoc_DuLieu", "[SoBuoiKeHoach] > 0 AND [SiSoToiThieu] > 0 AND [SiSoToiDa] >= [SiSoToiThieu] AND [HocPhiApDung] >= 0 AND [DongDangKyLuc] > [MoDangKyLuc]"));
    modelBuilder.Entity<PhongHoc>().ToTable(t => t.HasCheckConstraint("CK_PhongHoc_DuLieu", "[SucChua] > 0"));
    modelBuilder.Entity<BuoiHoc>().ToTable(t => t.HasCheckConstraint("CK_BuoiHoc_DuLieu", "[SoTiet] > 0 AND [PhutMoiTiet] > 0 AND [PhutNghi] >= 0 AND [LanXepLich] > 0 AND [KetThuc] > [BatDau] AND DATEDIFF_BIG(SECOND, [BatDau], [KetThuc]) = (CAST([SoTiet] AS bigint) * [PhutMoiTiet] + [PhutNghi]) * 60"));
    modelBuilder.Entity<GiaoDichTien>().ToTable(t => t.HasCheckConstraint("CK_GiaoDichTien_DuLieu", "[SoTien] > 0"));
    modelBuilder.Entity<KhoanThuDangKy>().ToTable(t => t.HasCheckConstraint("CK_KhoanThuDangKy_DuLieu", "[SoLuong] > 0 AND [DonGiaChot] >= 0 AND [TienGiamChot] >= 0 AND [ThanhTienChot] >= 0 AND [SoLuongDaGiao] >= 0 AND [SoLuongDaGiao] <= [SoLuong] AND [ThanhTienChot] = ROUND([SoLuong] * [DonGiaChot], 0) - [TienGiamChot]"));
    modelBuilder.Entity<QuyDinh>().ToTable(t => t.HasCheckConstraint("CK_QuyDinh_DuLieu", "ISJSON([NoiDungJson]) = 1"));
    modelBuilder.Entity<GiaoDichTien>().HasIndex(x => new { x.MaChungTu, x.KhoanThuId, x.LoaiGiaoDich }).IsUnique().HasFilter("[MaChungTu] IS NOT NULL AND [KhoanThuId] IS NOT NULL AND [LoaiGiaoDich] IS NOT NULL");
    modelBuilder.Entity<GiaoDichTien>().HasIndex(x => new { x.PhuongThuc, x.MaThamChieu, x.KhoanThuId, x.LoaiGiaoDich }).IsUnique().HasFilter("[PhuongThuc] IS NOT NULL AND [MaThamChieu] IS NOT NULL AND [KhoanThuId] IS NOT NULL AND [LoaiGiaoDich] IS NOT NULL");

    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        modelBuilder.Entity(entity.ClrType).Property("CreatedAt").HasColumnType("datetime2");
        modelBuilder.Entity(entity.ClrType).Property("UpdatedAt").HasColumnType("datetime2");
        modelBuilder.Entity(entity.ClrType).Property("RowVersion").IsRowVersion();
    }
    modelBuilder.Entity<KetQuaKhoaHoc>().HasIndex(x => x.SoXacNhan).IsUnique().HasFilter("[SoXacNhan] IS NOT NULL");
    modelBuilder.Entity<KhoanThuDangKy>().HasIndex(x => x.DangKyId).IsUnique().HasFilter("[LoaiKhoan] = 'HOC_PHI'");
    modelBuilder.Entity<BuoiHoc>().HasIndex(x => new { x.LopHocId, x.NoiDungBuoiId }).IsUnique()
        .HasFilter("[TrangThai] IN ('DA_XEP_LICH', 'DA_TO_CHUC')");
    modelBuilder.Entity<KhuyenMai>().ToTable(t => t.HasCheckConstraint("CK_KhuyenMai_DuLieu",
        "[TongLuot] > 0 AND [KetThuc] > [BatDau] AND [GiaTriGiam] >= 0 AND [LoaiGiam] IN ('PHAN_TRAM','SO_TIEN') AND ([LoaiGiam] <> 'PHAN_TRAM' OR [GiaTriGiam] <= 100)"));
    modelBuilder.Entity<TaiKhoan>().ToTable(t => t.HasCheckConstraint("CK_TaiKhoan_VaiTro",
        "[VaiTroChinh] IN ('QUAN_TRI','HOC_VU','THU_NGAN','GIAO_VIEN','QUAN_LY_KHO','THANH_VIEN') AND [QuyenBoSung] >= 0 AND [PhienBanBaoMat] > 0"));

    modelBuilder.Entity<DangKy>().Property(x=>x.RequestFingerprint).HasColumnType("varchar(64)").IsRequired();
    modelBuilder.Entity<GiaoDichTien>().Property(x=>x.RequestFingerprint).HasColumnType("varchar(64)").IsRequired();
    modelBuilder.Entity<YeuCauThayDoi>().Property(x=>x.PhuongAnHoanJson).HasColumnType("nvarchar(max)").IsRequired();
    modelBuilder.Entity<YeuCauThayDoi>().Property(x=>x.RequestFingerprint).HasColumnType("varchar(64)").IsRequired();
    modelBuilder.Entity<TaiKhoan>().HasIndex(x=>x.Email).IsUnique().HasFilter("[Email] IS NOT NULL AND [EmailXacNhanLuc] IS NOT NULL");
    modelBuilder.Entity<GiangDayBuoi>().HasIndex(x=>x.BuoiHocId).IsUnique().HasFilter("[VaiTro] = 'CHINH'");
    }
}
