IF OBJECT_ID(N'dbo.ThongBao',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ThongBao(
 Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThongBao PRIMARY KEY,
 ThanhVienId bigint NULL,Loai nvarchar(30) NOT NULL,TieuDe nvarchar(250) NOT NULL,NoiDung nvarchar(max) NOT NULL,
 Email nvarchar(254) NULL,DienThoai nvarchar(25) NULL,TrangThaiEmail nvarchar(20) NOT NULL,SoLanGui int NOT NULL,
 ThuLaiLuc datetime2 NULL,GuiEmailLuc datetime2 NULL,DaDocLuc datetime2 NULL,DaGoiLuc datetime2 NULL,NguoiGoiId bigint NULL,
 CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
 CREATE INDEX IX_ThongBao_ThanhVienId_CreatedAt ON dbo.ThongBao(ThanhVienId,CreatedAt);
 CREATE INDEX IX_ThongBao_TrangThaiEmail_ThuLaiLuc ON dbo.ThongBao(TrangThaiEmail,ThuLaiLuc);
END;
