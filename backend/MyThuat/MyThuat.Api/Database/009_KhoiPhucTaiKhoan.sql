SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.KhoiPhucTaiKhoan',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.KhoiPhucTaiKhoan(
  Id bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
  MaYeuCau varchar(32) NOT NULL,
  TaiKhoanId bigint NOT NULL,
  MucDich varchar(20) NOT NULL,
  EmailNhan nvarchar(254) NOT NULL,
  MaBam varchar(512) NOT NULL,
  PhienBanBaoMat int NOT NULL,
  SoLanSai smallint NOT NULL,
  HetHanLuc datetime2 NOT NULL,
  DaDungLuc datetime2 NULL,
  CreatedAt datetime2 NOT NULL,
  UpdatedAt datetime2 NOT NULL,
  RowVersion rowversion NOT NULL,
  CONSTRAINT FK_KhoiPhucTaiKhoan_TaiKhoan FOREIGN KEY(TaiKhoanId) REFERENCES dbo.TaiKhoan(Id)
 );
 CREATE UNIQUE INDEX IX_KhoiPhucTaiKhoan_MaYeuCau ON dbo.KhoiPhucTaiKhoan(MaYeuCau);
 CREATE INDEX IX_KhoiPhucTaiKhoan_TaiKhoanId_MucDich_CreatedAt ON dbo.KhoiPhucTaiKhoan(TaiKhoanId,MucDich,CreatedAt);
END;
COMMIT;
