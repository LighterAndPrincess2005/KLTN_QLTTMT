-- Run explicitly after reviewing affected rows and exporting a backup.
-- No deletion, no elevation of read-only links, no activation of blocked records.
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;
DECLARE @now datetime2=CONVERT(datetime2,SYSDATETIMEOFFSET() AT TIME ZONE 'SE Asia Standard Time');
DECLARE @targets TABLE(Id bigint PRIMARY KEY,HocVienId bigint,ThanhVienId bigint,ActorId bigint,TruocJson nvarchar(max));
INSERT INTO @targets
SELECT dd.Id,dd.HocVienId,dd.ThanhVienId,a.Id,
 (SELECT dd.Id DaiDienId,dd.XacNhanLuc,dd.NguoiXacNhan,hv.Id HocVienId,hv.TrangThai TrangThaiHocVien,
 tv.Id ThanhVienId,tv.TrangThai TrangThaiThanhVien,tv.XacNhanThanhVienLuc FOR JSON PATH,WITHOUT_ARRAY_WRAPPER,INCLUDE_NULL_VALUES)
FROM DaiDienHocVien dd WITH(UPDLOCK,HOLDLOCK)
JOIN HocVien hv ON hv.Id=dd.HocVienId
JOIN ThanhVien tv ON tv.Id=dd.ThanhVienId
CROSS APPLY(SELECT TOP 1 tk.Id FROM TaiKhoan tk WHERE tk.ThanhVienId=tv.Id AND tk.TrangThai='HOAT_DONG' ORDER BY tk.Id) a
WHERE dd.QuyenDaiDien IN('DAY_DU','DANG_KY')
 AND dd.HieuLucTu<=@now AND(dd.HieuLucDen IS NULL OR dd.HieuLucDen>@now)
 AND hv.TrangThai IN('CHO_XAC_NHAN','HOAT_DONG') AND tv.TrangThai IN('CHO_XAC_NHAN','HOAT_DONG')
 AND(dd.XacNhanLuc IS NULL OR hv.TrangThai='CHO_XAC_NHAN' OR tv.TrangThai='CHO_XAC_NHAN');
IF(SELECT COUNT(*) FROM @targets)>20 THROW 51000,'More than 20 profiles affected. Review targets before migration.',1;
UPDATE hv SET TrangThai='HOAT_DONG',UpdatedAt=@now FROM HocVien hv WHERE hv.TrangThai='CHO_XAC_NHAN' AND EXISTS(SELECT 1 FROM @targets t WHERE t.HocVienId=hv.Id);
UPDATE tv SET TrangThai='HOAT_DONG',XacNhanThanhVienLuc=COALESCE(tv.XacNhanThanhVienLuc,@now),UpdatedAt=@now FROM ThanhVien tv WHERE tv.TrangThai='CHO_XAC_NHAN' AND EXISTS(SELECT 1 FROM @targets t WHERE t.ThanhVienId=tv.Id);
UPDATE dd SET XacNhanLuc=COALESCE(dd.XacNhanLuc,@now),NguoiXacNhan=COALESCE(dd.NguoiXacNhan,t.ActorId),UpdatedAt=@now FROM DaiDienHocVien dd JOIN @targets t ON t.Id=dd.Id;
INSERT INTO NhatKyThayDoi(TaiKhoanId,LoaiDoiTuong,IdDoiTuong,HanhDong,TruocJson,SauJson,ThoiDiem,CorrelationId,Kenh,CreatedAt,UpdatedAt)
SELECT t.ActorId,'DaiDienHocVien',t.Id,'BO_CHO_DUYET_HO_SO_CU',t.TruocJson,N'{"cheDo":"TU_PHUC_VU"}',@now,CONVERT(varchar(36),NEWID()),'MIGRATION',@now,@now FROM @targets t;
SELECT COUNT(*) HoSoDaChuyen FROM @targets;
COMMIT TRANSACTION;
