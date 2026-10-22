using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Models;
namespace MyThuat.Api.Data;
public static class WebsiteMapping
{
    public static void Configure(ModelBuilder m){
        var article=m.Entity<BaiViet>();article.ToTable("BaiViet");article.HasIndex(x=>x.Slug).IsUnique();
        article.Property(x=>x.Slug).HasMaxLength(180);article.Property(x=>x.TieuDe).HasMaxLength(250);article.Property(x=>x.ChuDe).HasMaxLength(100);
        article.Property(x=>x.AnhBia).HasMaxLength(1000);article.Property(x=>x.TomTat).HasMaxLength(2000);article.Property(x=>x.Nguon).HasMaxLength(1000);article.Property(x=>x.TrangThai).HasMaxLength(20);
        article.ToTable(t=>t.HasCheckConstraint("CK_BaiViet_Json","ISJSON([NoiDungJson])=1 AND ISJSON([ThuVienJson])=1 AND [TrangThai] IN ('NHAP','DA_DANG','AN')"));
        var teacher=m.Entity<GioiThieuGiaoVien>();teacher.ToTable("GioiThieuGiaoVien");teacher.HasIndex(x=>x.GiaoVienId).IsUnique();teacher.HasOne(x=>x.GiaoVien).WithMany().HasForeignKey(x=>x.GiaoVienId).OnDelete(DeleteBehavior.NoAction);
        teacher.Property(x=>x.AnhDaiDien).HasMaxLength(1000);teacher.Property(x=>x.GioiThieu).HasMaxLength(3000);teacher.Property(x=>x.KhuVuc).HasMaxLength(200);
        var course=m.Entity<GioiThieuKhoaHoc>();course.ToTable("GioiThieuKhoaHoc");course.HasIndex(x=>x.KhoaHocId).IsUnique();course.HasOne(x=>x.KhoaHoc).WithMany().HasForeignKey(x=>x.KhoaHocId).OnDelete(DeleteBehavior.NoAction);course.Property(x=>x.AnhDaiDien).HasMaxLength(1000);
        var advice=m.Entity<TuVan>();advice.ToTable("TuVan");advice.HasIndex(x=>x.MaTraCuu).IsUnique();advice.HasIndex(x=>new{x.TrangThai,x.GuiLuc});
        advice.Property(x=>x.MaTraCuu).HasMaxLength(40);advice.Property(x=>x.KhoaTraCuuHash).HasMaxLength(64);advice.Property(x=>x.HoTen).HasMaxLength(150);advice.Property(x=>x.Email).HasMaxLength(254);advice.Property(x=>x.DienThoai).HasMaxLength(25);advice.Property(x=>x.ChuDe).HasMaxLength(200);advice.Property(x=>x.TrangThai).HasMaxLength(20);
        advice.HasOne(x=>x.ThanhVien).WithMany().HasForeignKey(x=>x.ThanhVienId).OnDelete(DeleteBehavior.NoAction);advice.HasOne(x=>x.KhoaHoc).WithMany().HasForeignKey(x=>x.KhoaHocId).OnDelete(DeleteBehavior.NoAction);
        var message=m.Entity<TinNhanTuVan>();message.ToTable("TinNhanTuVan");message.Property(x=>x.NoiDung).HasMaxLength(4000);message.HasIndex(x=>new{x.TuVanId,x.GuiLuc});message.HasOne(x=>x.TuVan).WithMany().HasForeignKey(x=>x.TuVanId).OnDelete(DeleteBehavior.NoAction);
    }
}
