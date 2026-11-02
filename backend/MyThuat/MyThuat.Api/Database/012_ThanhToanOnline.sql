IF OBJECT_ID(N'dbo.PhienThanhToan',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.PhienThanhToan(Id bigint IDENTITY PRIMARY KEY,DangKyId bigint NOT NULL,MaThanhToan varchar(20) NOT NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
 CREATE UNIQUE INDEX IX_PhienThanhToan_DangKyId ON dbo.PhienThanhToan(DangKyId);
 CREATE UNIQUE INDEX IX_PhienThanhToan_MaThanhToan ON dbo.PhienThanhToan(MaThanhToan);
END;
IF OBJECT_ID(N'dbo.SuKienSePay',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.SuKienSePay(Id bigint IDENTITY PRIMARY KEY,MaGiaoDichSePay bigint NOT NULL,DangKyId bigint NULL,SoTien decimal(18,0) NOT NULL,NoiDung nvarchar(2000) NOT NULL,MaThamChieu nvarchar(150) NOT NULL,DauVanTay varchar(64) NOT NULL,TrangThai nvarchar(30) NOT NULL,CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
 CREATE UNIQUE INDEX IX_SuKienSePay_MaGiaoDichSePay ON dbo.SuKienSePay(MaGiaoDichSePay);
END;
