IF OBJECT_ID(N'dbo.DanhGiaKhoaHoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhGiaKhoaHoc (
        Id bigint IDENTITY PRIMARY KEY,
        HoSoTheoHocId bigint NOT NULL UNIQUE REFERENCES dbo.HoSoTheoHoc(Id),
        NguoiGuiId bigint NOT NULL REFERENCES dbo.ThanhVien(Id),
        SoSao int NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
        NoiDung nvarchar(2000) NOT NULL,
        DongYCongKhai bit NOT NULL,
        TrangThai nvarchar(20) NOT NULL,
        Anh varbinary(max) NULL,
        LoaiAnh nvarchar(20) NULL,
        GhiChuDuyet nvarchar(500) NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        RowVersion rowversion NOT NULL
    );
END;
