IF COL_LENGTH(N'dbo.PhieuKho', N'LopHocId') IS NULL
    ALTER TABLE dbo.PhieuKho ADD LopHocId bigint NULL
        CONSTRAINT FK_PhieuKho_LopHoc_LopHocId REFERENCES dbo.LopHoc(Id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.PhieuKho') AND name = N'IX_PhieuKho_LopHocId_LoaiPhieu_TrangThai')
    EXEC(N'CREATE INDEX IX_PhieuKho_LopHocId_LoaiPhieu_TrangThai ON dbo.PhieuKho(LopHocId, LoaiPhieu, TrangThai)');
