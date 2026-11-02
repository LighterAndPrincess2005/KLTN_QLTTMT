IF OBJECT_ID(N'dbo.ThongBaoLop',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ThongBaoLop(
 Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThongBaoLop PRIMARY KEY,
 LopHocId bigint NOT NULL,TaiKhoanId bigint NOT NULL,MaYeuCau nvarchar(64) NOT NULL,
 TieuDe nvarchar(150) NOT NULL,NoiDung nvarchar(3000) NOT NULL,SoNguoiNhan int NOT NULL,SoEmail int NOT NULL,
 CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,RowVersion rowversion NOT NULL);
 CREATE UNIQUE INDEX IX_ThongBaoLop_TaiKhoanId_MaYeuCau ON dbo.ThongBaoLop(TaiKhoanId,MaYeuCau);
END;
