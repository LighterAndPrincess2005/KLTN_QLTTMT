-- Existing vouchers remain private/manual. No historic registrations are repriced.
IF COL_LENGTH('dbo.KhuyenMai','TuDongApDung') IS NULL
ALTER TABLE dbo.KhuyenMai ADD TuDongApDung bit NOT NULL CONSTRAINT DF_KhuyenMai_TuDongApDung DEFAULT(0);
IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.ApDungUuDai') AND name='IX_ApDungUuDai_DangKyId_NhomKhoanThu')
DROP INDEX IX_ApDungUuDai_DangKyId_NhomKhoanThu ON dbo.ApDungUuDai;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.ApDungUuDai') AND name='IX_ApDungUuDai_DangKyId_NhomKhoanThu_KhuyenMaiId')
CREATE UNIQUE INDEX IX_ApDungUuDai_DangKyId_NhomKhoanThu_KhuyenMaiId ON dbo.ApDungUuDai(DangKyId,NhomKhoanThu,KhuyenMaiId);
