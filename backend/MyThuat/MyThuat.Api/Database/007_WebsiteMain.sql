SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.BaiViet',N'U') IS NULL
CREATE TABLE dbo.BaiViet(Id bigint IDENTITY PRIMARY KEY,Slug nvarchar(180) NOT NULL UNIQUE,TieuDe nvarchar(250) NOT NULL,ChuDe nvarchar(100) NOT NULL,AnhBia nvarchar(1000) NOT NULL,TomTat nvarchar(2000) NOT NULL,NoiDungJson nvarchar(max) NOT NULL,BaiThucHanh nvarchar(max) NOT NULL,Nguon nvarchar(1000) NOT NULL,ThuVienJson nvarchar(max) NOT NULL,TrangThai nvarchar(20) NOT NULL,DangLuc datetime2 NULL,NguoiSuaId bigint NOT NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL,CONSTRAINT CK_BaiViet_Json CHECK(ISJSON(NoiDungJson)=1 AND ISJSON(ThuVienJson)=1 AND TrangThai IN ('NHAP','DA_DANG','AN')));
IF OBJECT_ID(N'dbo.GioiThieuGiaoVien',N'U') IS NULL
CREATE TABLE dbo.GioiThieuGiaoVien(Id bigint IDENTITY PRIMARY KEY,GiaoVienId bigint NOT NULL UNIQUE REFERENCES dbo.GiaoVien(Id),AnhDaiDien nvarchar(1000) NOT NULL,GioiThieu nvarchar(3000) NOT NULL,KhuVuc nvarchar(200) NOT NULL,TacPhamJson nvarchar(max) NOT NULL,CongKhai bit NOT NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
IF OBJECT_ID(N'dbo.GioiThieuKhoaHoc',N'U') IS NULL
CREATE TABLE dbo.GioiThieuKhoaHoc(Id bigint IDENTITY PRIMARY KEY,KhoaHocId bigint NOT NULL UNIQUE REFERENCES dbo.KhoaHoc(Id),AnhDaiDien nvarchar(1000) NOT NULL,DauRaJson nvarchar(max) NOT NULL,HoaCuBaoGom nvarchar(max) NOT NULL,CanChuanBi nvarchar(max) NOT NULL,ThanhPhamJson nvarchar(max) NOT NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
IF OBJECT_ID(N'dbo.TuVan',N'U') IS NULL
BEGIN
CREATE TABLE dbo.TuVan(Id bigint IDENTITY PRIMARY KEY,MaTraCuu nvarchar(40) NOT NULL UNIQUE,KhoaTraCuuHash nvarchar(64) NOT NULL,ThanhVienId bigint NULL REFERENCES dbo.ThanhVien(Id),KhoaHocId bigint NULL REFERENCES dbo.KhoaHoc(Id),HoTen nvarchar(150) NOT NULL,Email nvarchar(254) NOT NULL,DienThoai nvarchar(25) NULL,ChuDe nvarchar(200) NOT NULL,TrangThai nvarchar(20) NOT NULL,GuiLuc datetime2 NOT NULL,DocLuc datetime2 NULL,NguoiXuLyId bigint NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
CREATE INDEX IX_TuVan_TrangThai_GuiLuc ON dbo.TuVan(TrangThai,GuiLuc);
END;
IF OBJECT_ID(N'dbo.TinNhanTuVan',N'U') IS NULL
BEGIN
CREATE TABLE dbo.TinNhanTuVan(Id bigint IDENTITY PRIMARY KEY,TuVanId bigint NOT NULL REFERENCES dbo.TuVan(Id),NoiDung nvarchar(4000) NOT NULL,TuNhanVien bit NOT NULL,TaiKhoanId bigint NULL,GuiLuc datetime2 NOT NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
CREATE INDEX IX_TinNhanTuVan_TuVanId_GuiLuc ON dbo.TinNhanTuVan(TuVanId,GuiLuc);
END;
COMMIT;
