CREATE TABLE [GiaoVien] (
    [Id] bigint NOT NULL IDENTITY,
    [MaGiaoVien] varchar(20) NOT NULL,
    [HoTen] nvarchar(150) NOT NULL,
    [NgaySinh] date NULL,
    [GioiTinh] varchar(20) NULL,
    [DienThoai] varchar(25) NOT NULL,
    [Email] nvarchar(254) NULL,
    [DiaChi] nvarchar(300) NULL,
    [TrinhDo] nvarchar(200) NOT NULL,
    [ChuyenMon] nvarchar(1000) NOT NULL,
    [CapDoCoTheDay] nvarchar(500) NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_GiaoVien] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [HoaCu] (
    [Id] bigint NOT NULL IDENTITY,
    [MaHoaCu] varchar(30) NOT NULL,
    [TenHoaCu] nvarchar(200) NOT NULL,
    [QuyCach] nvarchar(300) NOT NULL,
    [MauSac] nvarchar(100) NULL,
    [DonViCoSo] nvarchar(30) NOT NULL,
    [NhomHoaCu] varchar(20) NOT NULL,
    [GiaBanThamKhao] decimal(18,0) NOT NULL,
    [MucDatHang] decimal(18,3) NOT NULL,
    [CanhBaoAnToan] nvarchar(1000) NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_HoaCu] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [LoaiKhoaHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [MaLoai] varchar(20) NOT NULL,
    [TenLoai] nvarchar(150) NOT NULL,
    [MoTa] nvarchar(1000) NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_LoaiKhoaHoc] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [NhaCungCap] (
    [Id] bigint NOT NULL IDENTITY,
    [MaNCC] varchar(20) NOT NULL,
    [TenNCC] nvarchar(200) NOT NULL,
    [NguoiLienHe] nvarchar(150) NULL,
    [DienThoai] varchar(25) NOT NULL,
    [Email] nvarchar(254) NULL,
    [DiaChi] nvarchar(300) NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_NhaCungCap] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PhongHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [MaPhong] varchar(20) NOT NULL,
    [TenPhong] nvarchar(100) NOT NULL,
    [SucChua] smallint NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PhongHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PhongHoc_DuLieu] CHECK ([SucChua] > 0)
);
GO


CREATE TABLE [HocVien] (
    [Id] bigint NOT NULL IDENTITY,
    [MaHocVien] varchar(20) NOT NULL,
    [HoTen] nvarchar(150) NOT NULL,
    [NgaySinh] date NOT NULL,
    [GioiTinh] varchar(20) NULL,
    [DiaChi] nvarchar(300) NULL,
    [MucTieuHoc] nvarchar(1000) NULL,
    [LuuYHoTroHocTap] nvarchar(1000) NULL,
    [LienHeKhanCapTen] nvarchar(150) NOT NULL,
    [LienHeKhanCapSDT] varchar(25) NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [NhanXetDauVao] nvarchar(2000) NULL,
    [CapDoDeXuat] nvarchar(100) NULL,
    [NgayDanhGiaDauVao] date NULL,
    [GiaoVienDanhGia] bigint NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_HocVien] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HocVien_GiaoVien_GiaoVienDanhGia] FOREIGN KEY ([GiaoVienDanhGia]) REFERENCES [GiaoVien] ([Id])
);
GO


CREATE TABLE [KhoaHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [LoaiKhoaHocId] bigint NOT NULL,
    [KhoaGocId] bigint NULL,
    [MaKhoa] varchar(20) NOT NULL,
    [TenKhoa] nvarchar(200) NOT NULL,
    [PhienBan] int NOT NULL,
    [CapDo] nvarchar(100) NOT NULL,
    [TuoiToiThieu] smallint NULL,
    [YeuCauDauVao] nvarchar(1000) NULL,
    [MucTieu] nvarchar(2000) NOT NULL,
    [SoBuoi] smallint NOT NULL,
    [SoTietMoiBuoi] smallint NOT NULL,
    [PhutMoiTiet] smallint NOT NULL,
    [HocPhi] decimal(18,0) NOT NULL,
    [HieuLucTu] date NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_KhoaHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_KhoaHoc_DuLieu] CHECK ([SoBuoi] > 0 AND [SoTietMoiBuoi] > 0 AND [PhutMoiTiet] > 0 AND [HocPhi] >= 0 AND [PhienBan] > 0 AND ([TuoiToiThieu] IS NULL OR [TuoiToiThieu] >= 0)),
    CONSTRAINT [FK_KhoaHoc_KhoaHoc_KhoaGocId] FOREIGN KEY ([KhoaGocId]) REFERENCES [KhoaHoc] ([Id]),
    CONSTRAINT [FK_KhoaHoc_LoaiKhoaHoc_LoaiKhoaHocId] FOREIGN KEY ([LoaiKhoaHocId]) REFERENCES [LoaiKhoaHoc] ([Id])
);
GO


CREATE TABLE [KhuyenMai] (
    [Id] bigint NOT NULL IDENTITY,
    [MaKhuyenMai] varchar(30) NOT NULL,
    [TenKhuyenMai] nvarchar(200) NOT NULL,
    [NhomKhoanThu] varchar(20) NOT NULL,
    [LoaiGiam] varchar(20) NOT NULL,
    [GiaTriGiam] decimal(18,2) NOT NULL,
    [TranGiam] decimal(18,0) NULL,
    [TongLuot] int NOT NULL,
    [SoNgayTuXacNhanTV] smallint NULL,
    [BatDau] datetime2 NOT NULL,
    [KetThuc] datetime2 NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [KhoaApDungId] bigint NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_KhuyenMai] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_KhuyenMai_DuLieu] CHECK ([TongLuot] > 0 AND [KetThuc] > [BatDau] AND [GiaTriGiam] >= 0 AND [LoaiGiam] IN ('PHAN_TRAM','SO_TIEN') AND ([LoaiGiam] <> 'PHAN_TRAM' OR [GiaTriGiam] <= 100)),
    CONSTRAINT [FK_KhuyenMai_KhoaHoc_KhoaApDungId] FOREIGN KEY ([KhoaApDungId]) REFERENCES [KhoaHoc] ([Id])
);
GO


CREATE TABLE [NoiDungBuoi] (
    [Id] bigint NOT NULL IDENTITY,
    [KhoaHocId] bigint NOT NULL,
    [ThuTu] smallint NOT NULL,
    [ChuDe] nvarchar(200) NOT NULL,
    [NoiDung] nvarchar(2000) NOT NULL,
    [SoTiet] smallint NOT NULL,
    [YeuCauSanPham] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_NoiDungBuoi] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NoiDungBuoi_DuLieu] CHECK ([ThuTu] > 0 AND [SoTiet] > 0),
    CONSTRAINT [FK_NoiDungBuoi_KhoaHoc_KhoaHocId] FOREIGN KEY ([KhoaHocId]) REFERENCES [KhoaHoc] ([Id])
);
GO


CREATE TABLE [DinhMucHoaCu] (
    [Id] bigint NOT NULL IDENTITY,
    [NoiDungBuoiId] bigint NOT NULL,
    [HoaCuId] bigint NOT NULL,
    [CoSoTinh] varchar(30) NOT NULL,
    [SoLuong] decimal(18,3) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DinhMucHoaCu] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DinhMucHoaCu_HoaCu_HoaCuId] FOREIGN KEY ([HoaCuId]) REFERENCES [HoaCu] ([Id]),
    CONSTRAINT [FK_DinhMucHoaCu_NoiDungBuoi_NoiDungBuoiId] FOREIGN KEY ([NoiDungBuoiId]) REFERENCES [NoiDungBuoi] ([Id])
);
GO


CREATE TABLE [ApDungUuDai] (
    [Id] bigint NOT NULL IDENTITY,
    [DangKyId] bigint NOT NULL,
    [KhuyenMaiId] bigint NOT NULL,
    [NhomKhoanThu] varchar(20) NOT NULL,
    [GiaTriGiamChot] decimal(18,0) NOT NULL,
    [HanGiuLuot] datetime2 NOT NULL,
    [TrangThaiLuot] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ApDungUuDai] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApDungUuDai_KhuyenMai_KhuyenMaiId] FOREIGN KEY ([KhuyenMaiId]) REFERENCES [KhuyenMai] ([Id])
);
GO


CREATE TABLE [BuoiHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [LopHocId] bigint NOT NULL,
    [NoiDungBuoiId] bigint NOT NULL,
    [PhongHocId] bigint NOT NULL,
    [BuoiThayTheChoId] bigint NULL,
    [ThuTuTrongLop] smallint NOT NULL,
    [LanXepLich] smallint NOT NULL,
    [BatDau] datetime2 NOT NULL,
    [KetThuc] datetime2 NOT NULL,
    [SoTiet] smallint NOT NULL,
    [PhutMoiTiet] smallint NOT NULL,
    [PhutNghi] smallint NOT NULL,
    [LyDoDoiLich] nvarchar(1000) NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BuoiHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BuoiHoc_DuLieu] CHECK ([SoTiet] > 0 AND [PhutMoiTiet] > 0 AND [PhutNghi] >= 0 AND [LanXepLich] > 0 AND [KetThuc] > [BatDau] AND DATEDIFF_BIG(SECOND, [BatDau], [KetThuc]) = (CAST([SoTiet] AS bigint) * [PhutMoiTiet] + [PhutNghi]) * 60),
    CONSTRAINT [FK_BuoiHoc_BuoiHoc_BuoiThayTheChoId] FOREIGN KEY ([BuoiThayTheChoId]) REFERENCES [BuoiHoc] ([Id]),
    CONSTRAINT [FK_BuoiHoc_NoiDungBuoi_NoiDungBuoiId] FOREIGN KEY ([NoiDungBuoiId]) REFERENCES [NoiDungBuoi] ([Id]),
    CONSTRAINT [FK_BuoiHoc_PhongHoc_PhongHocId] FOREIGN KEY ([PhongHocId]) REFERENCES [PhongHoc] ([Id])
);
GO


CREATE TABLE [ChiTietPhieuKho] (
    [Id] bigint NOT NULL IDENTITY,
    [PhieuKhoId] bigint NOT NULL,
    [HoaCuId] bigint NOT NULL,
    [ChiTietThamChieuId] bigint NULL,
    [KhoanThuDangKyId] bigint NULL,
    [MaLo] varchar(50) NULL,
    [HanSuDung] date NULL,
    [SoLuong] decimal(18,3) NOT NULL,
    [DonGiaChot] decimal(18,0) NOT NULL,
    [SoLuongSoSachTaiKho] decimal(18,3) NULL,
    [SoLuongThucTeTaiKho] decimal(18,3) NULL,
    [SoLuongSoSachDangMuon] decimal(18,3) NULL,
    [SoLuongThucTeDangMuon] decimal(18,3) NULL,
    [GhiChu] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ChiTietPhieuKho] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChiTietPhieuKho_ChiTietPhieuKho_ChiTietThamChieuId] FOREIGN KEY ([ChiTietThamChieuId]) REFERENCES [ChiTietPhieuKho] ([Id]),
    CONSTRAINT [FK_ChiTietPhieuKho_HoaCu_HoaCuId] FOREIGN KEY ([HoaCuId]) REFERENCES [HoaCu] ([Id])
);
GO


CREATE TABLE [DaiDienHocVien] (
    [Id] bigint NOT NULL IDENTITY,
    [ThanhVienId] bigint NOT NULL,
    [HocVienId] bigint NOT NULL,
    [QuanHe] varchar(30) NOT NULL,
    [LaLienHeChinh] bit NOT NULL,
    [QuyenDaiDien] nvarchar(300) NOT NULL,
    [HieuLucTu] datetime2 NOT NULL,
    [HieuLucDen] datetime2 NULL,
    [XacNhanLuc] datetime2 NULL,
    [NguoiXacNhan] bigint NULL,
    [DongYHinhAnh] bit NOT NULL,
    [DongYTacPham] bit NOT NULL,
    [ThoiDiemDongY] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DaiDienHocVien] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DaiDienHocVien_HocVien_HocVienId] FOREIGN KEY ([HocVienId]) REFERENCES [HocVien] ([Id])
);
GO


CREATE TABLE [DangKy] (
    [Id] bigint NOT NULL IDENTITY,
    [MaDangKy] varchar(30) NOT NULL,
    [HoSoTheoHocId] bigint NOT NULL,
    [LopHocId] bigint NOT NULL,
    [NguoiDangKyId] bigint NOT NULL,
    [DangKyNguonId] bigint NULL,
    [LapLuc] datetime2 NOT NULL,
    [HanGiuCho] datetime2 NOT NULL,
    [HieuLucTu] datetime2 NULL,
    [HieuLucDen] datetime2 NULL,
    [HocPhiGocChot] decimal(18,0) NOT NULL,
    [TrangThai] varchar(25) NOT NULL,
    [RequestFingerprint] varchar(64) NOT NULL,
    [IdempotencyKey] varchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DangKy] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DangKy_DangKy_DangKyNguonId] FOREIGN KEY ([DangKyNguonId]) REFERENCES [DangKy] ([Id])
);
GO


CREATE TABLE [KhoanThuDangKy] (
    [Id] bigint NOT NULL IDENTITY,
    [DangKyId] bigint NOT NULL,
    [LoaiKhoan] varchar(20) NOT NULL,
    [HoaCuId] bigint NULL,
    [MoTaChot] nvarchar(300) NOT NULL,
    [SoLuong] decimal(18,3) NOT NULL,
    [DonGiaChot] decimal(18,0) NOT NULL,
    [TienGiamChot] decimal(18,0) NOT NULL,
    [ThanhTienChot] decimal(18,0) NOT NULL,
    [SoLuongDaGiao] decimal(18,3) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_KhoanThuDangKy] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_KhoanThuDangKy_DuLieu] CHECK ([SoLuong] > 0 AND [DonGiaChot] >= 0 AND [TienGiamChot] >= 0 AND [ThanhTienChot] >= 0 AND [SoLuongDaGiao] >= 0 AND [SoLuongDaGiao] <= [SoLuong] AND [ThanhTienChot] = ROUND([SoLuong] * [DonGiaChot], 0) - [TienGiamChot]),
    CONSTRAINT [FK_KhoanThuDangKy_DangKy_DangKyId] FOREIGN KEY ([DangKyId]) REFERENCES [DangKy] ([Id]),
    CONSTRAINT [FK_KhoanThuDangKy_HoaCu_HoaCuId] FOREIGN KEY ([HoaCuId]) REFERENCES [HoaCu] ([Id])
);
GO


CREATE TABLE [DiemDanh] (
    [Id] bigint NOT NULL IDENTITY,
    [DangKyId] bigint NOT NULL,
    [BuoiHocId] bigint NOT NULL,
    [NoiDungDuocTinhId] bigint NOT NULL,
    [LoaiThamGia] varchar(20) NOT NULL,
    [TrangThai] varchar(25) NOT NULL,
    [BaoNghiLuc] datetime2 NULL,
    [PhutThamDu] smallint NULL,
    [NhanXet] nvarchar(2000) NULL,
    [SanPhamUrl] nvarchar(500) NULL,
    [DiemSanPham] decimal(4,2) NULL,
    [NguoiGhi] bigint NULL,
    [GhiLuc] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DiemDanh] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DiemDanh_BuoiHoc_BuoiHocId] FOREIGN KEY ([BuoiHocId]) REFERENCES [BuoiHoc] ([Id]),
    CONSTRAINT [FK_DiemDanh_DangKy_DangKyId] FOREIGN KEY ([DangKyId]) REFERENCES [DangKy] ([Id]),
    CONSTRAINT [FK_DiemDanh_NoiDungBuoi_NoiDungDuocTinhId] FOREIGN KEY ([NoiDungDuocTinhId]) REFERENCES [NoiDungBuoi] ([Id])
);
GO


CREATE TABLE [GiangDayBuoi] (
    [Id] bigint NOT NULL IDENTITY,
    [BuoiHocId] bigint NOT NULL,
    [GiaoVienId] bigint NOT NULL,
    [VaiTro] varchar(20) NOT NULL,
    [SoTietThucDay] smallint NOT NULL,
    [LaThayThe] bit NOT NULL,
    [LyDoThayThe] nvarchar(500) NULL,
    [NguoiDuyet] bigint NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_GiangDayBuoi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GiangDayBuoi_BuoiHoc_BuoiHocId] FOREIGN KEY ([BuoiHocId]) REFERENCES [BuoiHoc] ([Id]),
    CONSTRAINT [FK_GiangDayBuoi_GiaoVien_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [GiaoVien] ([Id])
);
GO


CREATE TABLE [GiaoDichTien] (
    [Id] bigint NOT NULL IDENTITY,
    [DangKyId] bigint NOT NULL,
    [KhoanThuId] bigint NULL,
    [GiaoDichThuGocId] bigint NULL,
    [YeuCauThayDoiId] bigint NULL,
    [MaChungTu] varchar(50) NULL,
    [MaThamChieu] varchar(100) NULL,
    [LoaiGiaoDich] varchar(20) NOT NULL,
    [PhuongThuc] varchar(20) NOT NULL,
    [SoTien] decimal(18,0) NOT NULL,
    [ThoiDiem] datetime2 NOT NULL,
    [NguoiXacNhan] bigint NULL,
    [TrangThai] varchar(20) NOT NULL,
    [RequestFingerprint] varchar(64) NOT NULL,
    [IdempotencyKey] varchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_GiaoDichTien] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GiaoDichTien_DuLieu] CHECK ([SoTien] > 0),
    CONSTRAINT [FK_GiaoDichTien_DangKy_DangKyId] FOREIGN KEY ([DangKyId]) REFERENCES [DangKy] ([Id]),
    CONSTRAINT [FK_GiaoDichTien_GiaoDichTien_GiaoDichThuGocId] FOREIGN KEY ([GiaoDichThuGocId]) REFERENCES [GiaoDichTien] ([Id]),
    CONSTRAINT [FK_GiaoDichTien_KhoanThuDangKy_KhoanThuId] FOREIGN KEY ([KhoanThuId]) REFERENCES [KhoanThuDangKy] ([Id])
);
GO


CREATE TABLE [HocBu] (
    [Id] bigint NOT NULL IDENTITY,
    [DiemDanhVangId] bigint NOT NULL,
    [DiemDanhBuId] bigint NULL,
    [HanHoanTat] datetime2 NOT NULL,
    [NguoiDuyet] bigint NULL,
    [TrangThai] varchar(20) NOT NULL,
    [LyDo] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_HocBu] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HocBu_DiemDanh_DiemDanhBuId] FOREIGN KEY ([DiemDanhBuId]) REFERENCES [DiemDanh] ([Id]),
    CONSTRAINT [FK_HocBu_DiemDanh_DiemDanhVangId] FOREIGN KEY ([DiemDanhVangId]) REFERENCES [DiemDanh] ([Id])
);
GO


CREATE TABLE [HoSoTheoHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [HocVienId] bigint NOT NULL,
    [KhoaHocId] bigint NOT NULL,
    [QuyDinhId] bigint NOT NULL,
    [NgayBatDau] date NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_HoSoTheoHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HoSoTheoHoc_HocVien_HocVienId] FOREIGN KEY ([HocVienId]) REFERENCES [HocVien] ([Id]),
    CONSTRAINT [FK_HoSoTheoHoc_KhoaHoc_KhoaHocId] FOREIGN KEY ([KhoaHocId]) REFERENCES [KhoaHoc] ([Id])
);
GO


CREATE TABLE [KetQuaKhoaHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [HoSoTheoHocId] bigint NOT NULL,
    [SoBuoiHopLeChot] smallint NOT NULL,
    [DiemCuoiKhoaChot] decimal(4,2) NOT NULL,
    [KetLuan] varchar(20) NOT NULL,
    [NhanXetTongKet] nvarchar(2000) NULL,
    [NguoiDuyet] bigint NOT NULL,
    [ChotLuc] datetime2 NOT NULL,
    [SoXacNhan] varchar(50) NULL,
    [NgayCap] date NULL,
    [BaiCuoiKhoaUrl] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_KetQuaKhoaHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KetQuaKhoaHoc_HoSoTheoHoc_HoSoTheoHocId] FOREIGN KEY ([HoSoTheoHocId]) REFERENCES [HoSoTheoHoc] ([Id])
);
GO


CREATE TABLE [LopHoc] (
    [Id] bigint NOT NULL IDENTITY,
    [KhoaHocId] bigint NOT NULL,
    [MaLop] varchar(30) NOT NULL,
    [TenLop] nvarchar(200) NOT NULL,
    [NgayKhaiGiangDuKien] date NOT NULL,
    [NgayKhaiGiangThucTe] date NULL,
    [NgayKetThucDuKien] date NULL,
    [SoBuoiKeHoach] smallint NOT NULL,
    [LichHocDuKien] nvarchar(500) NOT NULL,
    [SiSoToiThieu] smallint NOT NULL,
    [SiSoToiDa] smallint NOT NULL,
    [HocPhiApDung] decimal(18,0) NOT NULL,
    [MoDangKyLuc] datetime2 NOT NULL,
    [DongDangKyLuc] datetime2 NOT NULL,
    [NguoiDuyet] bigint NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_LopHoc] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_LopHoc_DuLieu] CHECK ([SoBuoiKeHoach] > 0 AND [SiSoToiThieu] > 0 AND [SiSoToiDa] >= [SiSoToiThieu] AND [HocPhiApDung] >= 0 AND [DongDangKyLuc] > [MoDangKyLuc]),
    CONSTRAINT [FK_LopHoc_KhoaHoc_KhoaHocId] FOREIGN KEY ([KhoaHocId]) REFERENCES [KhoaHoc] ([Id])
);
GO


CREATE TABLE [NhatKyThayDoi] (
    [Id] bigint NOT NULL IDENTITY,
    [TaiKhoanId] bigint NOT NULL,
    [LoaiDoiTuong] varchar(50) NOT NULL,
    [IdDoiTuong] bigint NOT NULL,
    [HanhDong] varchar(50) NOT NULL,
    [TruocJson] nvarchar(max) NULL,
    [SauJson] nvarchar(max) NULL,
    [ThoiDiem] datetime2 NOT NULL,
    [CorrelationId] varchar(100) NOT NULL,
    [Kenh] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_NhatKyThayDoi] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PhanCong] (
    [Id] bigint NOT NULL IDENTITY,
    [LopHocId] bigint NOT NULL,
    [GiaoVienId] bigint NOT NULL,
    [VaiTroTrongLop] varchar(20) NOT NULL,
    [TuNgay] date NOT NULL,
    [DenNgay] date NULL,
    [XacNhanDuChuyenMon] bit NOT NULL,
    [XacNhanNhanLopLuc] datetime2 NULL,
    [NguoiDuyet] bigint NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PhanCong] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PhanCong_GiaoVien_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [GiaoVien] ([Id]),
    CONSTRAINT [FK_PhanCong_LopHoc_LopHocId] FOREIGN KEY ([LopHocId]) REFERENCES [LopHoc] ([Id])
);
GO


CREATE TABLE [PhanHoi] (
    [Id] bigint NOT NULL IDENTITY,
    [DangKyId] bigint NOT NULL,
    [NguoiGuiId] bigint NOT NULL,
    [NoiDung] nvarchar(2000) NOT NULL,
    [GuiLuc] datetime2 NOT NULL,
    [NguoiXuLy] bigint NULL,
    [HanPhanHoi] datetime2 NOT NULL,
    [TraLoi] nvarchar(2000) NULL,
    [TraLoiLuc] datetime2 NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PhanHoi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PhanHoi_DangKy_DangKyId] FOREIGN KEY ([DangKyId]) REFERENCES [DangKy] ([Id])
);
GO


CREATE TABLE [PhieuKho] (
    [Id] bigint NOT NULL IDENTITY,
    [MaPhieu] varchar(30) NOT NULL,
    [LoaiPhieu] varchar(25) NOT NULL,
    [NhaCungCapId] bigint NULL,
    [BuoiHocId] bigint NULL,
    [PhieuThamChieuId] bigint NULL,
    [LapLuc] datetime2 NOT NULL,
    [DuyetLuc] datetime2 NULL,
    [LyDo] nvarchar(1000) NULL,
    [TrangThai] varchar(20) NOT NULL,
    [NguoiLap] bigint NOT NULL,
    [NguoiDuyet] bigint NULL,
    [NguoiNhan] bigint NULL,
    [TepBienBanUrl] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PhieuKho] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PhieuKho_BuoiHoc_BuoiHocId] FOREIGN KEY ([BuoiHocId]) REFERENCES [BuoiHoc] ([Id]),
    CONSTRAINT [FK_PhieuKho_NhaCungCap_NhaCungCapId] FOREIGN KEY ([NhaCungCapId]) REFERENCES [NhaCungCap] ([Id]),
    CONSTRAINT [FK_PhieuKho_PhieuKho_PhieuThamChieuId] FOREIGN KEY ([PhieuThamChieuId]) REFERENCES [PhieuKho] ([Id])
);
GO


CREATE TABLE [QuyDinh] (
    [Id] bigint NOT NULL IDENTITY,
    [MaBoQuyDinh] varchar(30) NOT NULL,
    [SoPhienBan] int NOT NULL,
    [HieuLucTu] datetime2 NOT NULL,
    [HieuLucDen] datetime2 NULL,
    [NoiDungJson] nvarchar(max) NOT NULL,
    [NguoiDuyet] bigint NOT NULL,
    [TrangThai] varchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_QuyDinh] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_QuyDinh_DuLieu] CHECK (ISJSON([NoiDungJson]) = 1)
);
GO


CREATE TABLE [TaiKhoan] (
    [Id] bigint NOT NULL IDENTITY,
    [GiaoVienId] bigint NULL,
    [ThanhVienId] bigint NULL,
    [TenDangNhap] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [HoTenNhanVien] nvarchar(150) NULL,
    [DienThoaiNhanVien] varchar(25) NULL,
    [Email] nvarchar(254) NULL,
    [VaiTroChinh] varchar(30) NOT NULL,
    [QuyenBoSung] bigint NOT NULL,
    [EmailXacNhanLuc] datetime2 NULL,
    [TokenXacNhanHash] varchar(128) NULL,
    [TokenXacNhanHetHan] datetime2 NULL,
    [KhoaDenLuc] datetime2 NULL,
    [TrangThai] varchar(20) NOT NULL,
    [PhienBanBaoMat] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_TaiKhoan] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_TaiKhoan_VaiTro] CHECK ([VaiTroChinh] IN ('QUAN_TRI','HOC_VU','THU_NGAN','GIAO_VIEN','QUAN_LY_KHO','THANH_VIEN') AND [QuyenBoSung] >= 0 AND [PhienBanBaoMat] > 0),
    CONSTRAINT [FK_TaiKhoan_GiaoVien_GiaoVienId] FOREIGN KEY ([GiaoVienId]) REFERENCES [GiaoVien] ([Id])
);
GO


CREATE TABLE [ThanhVien] (
    [Id] bigint NOT NULL IDENTITY,
    [MaThanhVien] varchar(20) NOT NULL,
    [HoTen] nvarchar(150) NOT NULL,
    [DienThoai] varchar(25) NOT NULL,
    [Email] nvarchar(254) NULL,
    [DiaChi] nvarchar(300) NULL,
    [XacNhanThanhVienLuc] datetime2 NULL,
    [TrangThai] varchar(20) NOT NULL,
    [NhuCauTuVan] nvarchar(2000) NULL,
    [KetQuaTuVan] nvarchar(2000) NULL,
    [HanPhanHoiTuVan] datetime2 NULL,
    [NguoiTuVan] bigint NULL,
    [TrangThaiTuVan] varchar(20) NULL,
    [DongYQuangBa] bit NOT NULL,
    [ThoiDiemDongYQuangBa] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ThanhVien] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThanhVien_TaiKhoan_NguoiTuVan] FOREIGN KEY ([NguoiTuVan]) REFERENCES [TaiKhoan] ([Id])
);
GO


CREATE TABLE [YeuCauThayDoi] (
    [Id] bigint NOT NULL IDENTITY,
    [DangKyNguonId] bigint NOT NULL,
    [LopDichId] bigint NULL,
    [DangKyDichId] bigint NULL,
    [NguoiYeuCauId] bigint NOT NULL,
    [LoaiYeuCau] varchar(20) NOT NULL,
    [NguyenNhan] varchar(20) NOT NULL,
    [GuiLuc] datetime2 NOT NULL,
    [LyDo] nvarchar(1000) NOT NULL,
    [SoBuoiDaSuDungChot] smallint NULL,
    [SoTienHoanDuKien] decimal(18,0) NULL,
    [GiaTriChuyenDuKien] decimal(18,0) NULL,
    [HanThucHien] datetime2 NULL,
    [NguoiDuyet] bigint NULL,
    [DuyetLuc] datetime2 NULL,
    [LyDoQuyetDinh] nvarchar(1000) NULL,
    [HoanTatLuc] datetime2 NULL,
    [TrangThai] varchar(25) NOT NULL,
    [PhuongAnHoanJson] nvarchar(max) NOT NULL,
    [RequestFingerprint] varchar(64) NOT NULL,
    [IdempotencyKey] varchar(100) NOT NULL,
    [GiaTriChuyenThucTe] decimal(18,0) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_YeuCauThayDoi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_YeuCauThayDoi_DangKy_DangKyDichId] FOREIGN KEY ([DangKyDichId]) REFERENCES [DangKy] ([Id]),
    CONSTRAINT [FK_YeuCauThayDoi_DangKy_DangKyNguonId] FOREIGN KEY ([DangKyNguonId]) REFERENCES [DangKy] ([Id]),
    CONSTRAINT [FK_YeuCauThayDoi_LopHoc_LopDichId] FOREIGN KEY ([LopDichId]) REFERENCES [LopHoc] ([Id]),
    CONSTRAINT [FK_YeuCauThayDoi_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]),
    CONSTRAINT [FK_YeuCauThayDoi_ThanhVien_NguoiYeuCauId] FOREIGN KEY ([NguoiYeuCauId]) REFERENCES [ThanhVien] ([Id])
);
GO


CREATE UNIQUE INDEX [IX_ApDungUuDai_DangKyId_NhomKhoanThu] ON [ApDungUuDai] ([DangKyId], [NhomKhoanThu]);
GO


CREATE INDEX [IX_ApDungUuDai_KhuyenMaiId] ON [ApDungUuDai] ([KhuyenMaiId]);
GO


CREATE INDEX [IX_BuoiHoc_BuoiThayTheChoId] ON [BuoiHoc] ([BuoiThayTheChoId]);
GO


CREATE UNIQUE INDEX [IX_BuoiHoc_LopHocId_NoiDungBuoiId] ON [BuoiHoc] ([LopHocId], [NoiDungBuoiId]) WHERE [TrangThai] IN ('DA_XEP_LICH', 'DA_TO_CHUC');
GO


CREATE UNIQUE INDEX [IX_BuoiHoc_LopHocId_NoiDungBuoiId_LanXepLich] ON [BuoiHoc] ([LopHocId], [NoiDungBuoiId], [LanXepLich]);
GO


CREATE INDEX [IX_BuoiHoc_NoiDungBuoiId] ON [BuoiHoc] ([NoiDungBuoiId]);
GO


CREATE INDEX [IX_BuoiHoc_PhongHocId] ON [BuoiHoc] ([PhongHocId]);
GO


CREATE INDEX [IX_ChiTietPhieuKho_ChiTietThamChieuId] ON [ChiTietPhieuKho] ([ChiTietThamChieuId]);
GO


CREATE INDEX [IX_ChiTietPhieuKho_HoaCuId] ON [ChiTietPhieuKho] ([HoaCuId]);
GO


CREATE INDEX [IX_ChiTietPhieuKho_KhoanThuDangKyId] ON [ChiTietPhieuKho] ([KhoanThuDangKyId]);
GO


CREATE INDEX [IX_ChiTietPhieuKho_PhieuKhoId] ON [ChiTietPhieuKho] ([PhieuKhoId]);
GO


CREATE INDEX [IX_DaiDienHocVien_HocVienId] ON [DaiDienHocVien] ([HocVienId]);
GO


CREATE INDEX [IX_DaiDienHocVien_NguoiXacNhan] ON [DaiDienHocVien] ([NguoiXacNhan]);
GO


CREATE UNIQUE INDEX [IX_DaiDienHocVien_ThanhVienId_HocVienId_HieuLucTu] ON [DaiDienHocVien] ([ThanhVienId], [HocVienId], [HieuLucTu]);
GO


CREATE INDEX [IX_DangKy_DangKyNguonId] ON [DangKy] ([DangKyNguonId]);
GO


CREATE INDEX [IX_DangKy_HoSoTheoHocId] ON [DangKy] ([HoSoTheoHocId]);
GO


CREATE UNIQUE INDEX [IX_DangKy_IdempotencyKey] ON [DangKy] ([IdempotencyKey]);
GO


CREATE INDEX [IX_DangKy_LopHocId] ON [DangKy] ([LopHocId]);
GO


CREATE UNIQUE INDEX [IX_DangKy_MaDangKy] ON [DangKy] ([MaDangKy]);
GO


CREATE INDEX [IX_DangKy_NguoiDangKyId] ON [DangKy] ([NguoiDangKyId]);
GO


CREATE INDEX [IX_DiemDanh_BuoiHocId] ON [DiemDanh] ([BuoiHocId]);
GO


CREATE UNIQUE INDEX [IX_DiemDanh_DangKyId_BuoiHocId] ON [DiemDanh] ([DangKyId], [BuoiHocId]);
GO


CREATE INDEX [IX_DiemDanh_NguoiGhi] ON [DiemDanh] ([NguoiGhi]);
GO


CREATE INDEX [IX_DiemDanh_NoiDungDuocTinhId] ON [DiemDanh] ([NoiDungDuocTinhId]);
GO


CREATE INDEX [IX_DinhMucHoaCu_HoaCuId] ON [DinhMucHoaCu] ([HoaCuId]);
GO


CREATE UNIQUE INDEX [IX_DinhMucHoaCu_NoiDungBuoiId_HoaCuId_CoSoTinh] ON [DinhMucHoaCu] ([NoiDungBuoiId], [HoaCuId], [CoSoTinh]);
GO


CREATE UNIQUE INDEX [IX_GiangDayBuoi_BuoiHocId] ON [GiangDayBuoi] ([BuoiHocId]) WHERE [VaiTro] = 'CHINH';
GO


CREATE UNIQUE INDEX [IX_GiangDayBuoi_BuoiHocId_GiaoVienId_VaiTro] ON [GiangDayBuoi] ([BuoiHocId], [GiaoVienId], [VaiTro]);
GO


CREATE INDEX [IX_GiangDayBuoi_GiaoVienId] ON [GiangDayBuoi] ([GiaoVienId]);
GO


CREATE INDEX [IX_GiangDayBuoi_NguoiDuyet] ON [GiangDayBuoi] ([NguoiDuyet]);
GO


CREATE INDEX [IX_GiaoDichTien_DangKyId] ON [GiaoDichTien] ([DangKyId]);
GO


CREATE INDEX [IX_GiaoDichTien_GiaoDichThuGocId] ON [GiaoDichTien] ([GiaoDichThuGocId]);
GO


CREATE UNIQUE INDEX [IX_GiaoDichTien_IdempotencyKey] ON [GiaoDichTien] ([IdempotencyKey]);
GO


CREATE INDEX [IX_GiaoDichTien_KhoanThuId] ON [GiaoDichTien] ([KhoanThuId]);
GO


CREATE UNIQUE INDEX [IX_GiaoDichTien_MaChungTu_KhoanThuId_LoaiGiaoDich] ON [GiaoDichTien] ([MaChungTu], [KhoanThuId], [LoaiGiaoDich]) WHERE [MaChungTu] IS NOT NULL AND [KhoanThuId] IS NOT NULL AND [LoaiGiaoDich] IS NOT NULL;
GO


CREATE INDEX [IX_GiaoDichTien_NguoiXacNhan] ON [GiaoDichTien] ([NguoiXacNhan]);
GO


CREATE UNIQUE INDEX [IX_GiaoDichTien_PhuongThuc_MaThamChieu_KhoanThuId_LoaiGiaoDich] ON [GiaoDichTien] ([PhuongThuc], [MaThamChieu], [KhoanThuId], [LoaiGiaoDich]) WHERE [PhuongThuc] IS NOT NULL AND [MaThamChieu] IS NOT NULL AND [KhoanThuId] IS NOT NULL AND [LoaiGiaoDich] IS NOT NULL;
GO


CREATE INDEX [IX_GiaoDichTien_YeuCauThayDoiId] ON [GiaoDichTien] ([YeuCauThayDoiId]);
GO


CREATE UNIQUE INDEX [IX_GiaoVien_MaGiaoVien] ON [GiaoVien] ([MaGiaoVien]);
GO


CREATE UNIQUE INDEX [IX_HoaCu_MaHoaCu] ON [HoaCu] ([MaHoaCu]);
GO


CREATE UNIQUE INDEX [IX_HocBu_DiemDanhBuId] ON [HocBu] ([DiemDanhBuId]) WHERE [DiemDanhBuId] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_HocBu_DiemDanhVangId] ON [HocBu] ([DiemDanhVangId]);
GO


CREATE INDEX [IX_HocBu_NguoiDuyet] ON [HocBu] ([NguoiDuyet]);
GO


CREATE INDEX [IX_HocVien_GiaoVienDanhGia] ON [HocVien] ([GiaoVienDanhGia]);
GO


CREATE UNIQUE INDEX [IX_HocVien_MaHocVien] ON [HocVien] ([MaHocVien]);
GO


CREATE INDEX [IX_HoSoTheoHoc_HocVienId] ON [HoSoTheoHoc] ([HocVienId]);
GO


CREATE INDEX [IX_HoSoTheoHoc_KhoaHocId] ON [HoSoTheoHoc] ([KhoaHocId]);
GO


CREATE INDEX [IX_HoSoTheoHoc_QuyDinhId] ON [HoSoTheoHoc] ([QuyDinhId]);
GO


CREATE UNIQUE INDEX [IX_KetQuaKhoaHoc_HoSoTheoHocId] ON [KetQuaKhoaHoc] ([HoSoTheoHocId]);
GO


CREATE INDEX [IX_KetQuaKhoaHoc_NguoiDuyet] ON [KetQuaKhoaHoc] ([NguoiDuyet]);
GO


CREATE UNIQUE INDEX [IX_KetQuaKhoaHoc_SoXacNhan] ON [KetQuaKhoaHoc] ([SoXacNhan]) WHERE [SoXacNhan] IS NOT NULL;
GO


CREATE INDEX [IX_KhoaHoc_KhoaGocId] ON [KhoaHoc] ([KhoaGocId]);
GO


CREATE INDEX [IX_KhoaHoc_LoaiKhoaHocId] ON [KhoaHoc] ([LoaiKhoaHocId]);
GO


CREATE UNIQUE INDEX [IX_KhoaHoc_MaKhoa_PhienBan] ON [KhoaHoc] ([MaKhoa], [PhienBan]);
GO


CREATE UNIQUE INDEX [IX_KhoanThuDangKy_DangKyId] ON [KhoanThuDangKy] ([DangKyId]) WHERE [LoaiKhoan] = 'HOC_PHI';
GO


CREATE INDEX [IX_KhoanThuDangKy_HoaCuId] ON [KhoanThuDangKy] ([HoaCuId]);
GO


CREATE INDEX [IX_KhuyenMai_KhoaApDungId] ON [KhuyenMai] ([KhoaApDungId]);
GO


CREATE UNIQUE INDEX [IX_KhuyenMai_MaKhuyenMai] ON [KhuyenMai] ([MaKhuyenMai]);
GO


CREATE UNIQUE INDEX [IX_LoaiKhoaHoc_MaLoai] ON [LoaiKhoaHoc] ([MaLoai]);
GO


CREATE INDEX [IX_LopHoc_KhoaHocId] ON [LopHoc] ([KhoaHocId]);
GO


CREATE UNIQUE INDEX [IX_LopHoc_MaLop] ON [LopHoc] ([MaLop]);
GO


CREATE INDEX [IX_LopHoc_NguoiDuyet] ON [LopHoc] ([NguoiDuyet]);
GO


CREATE UNIQUE INDEX [IX_NhaCungCap_MaNCC] ON [NhaCungCap] ([MaNCC]);
GO


CREATE INDEX [IX_NhatKyThayDoi_TaiKhoanId] ON [NhatKyThayDoi] ([TaiKhoanId]);
GO


CREATE UNIQUE INDEX [IX_NoiDungBuoi_KhoaHocId_ThuTu] ON [NoiDungBuoi] ([KhoaHocId], [ThuTu]);
GO


CREATE INDEX [IX_PhanCong_GiaoVienId] ON [PhanCong] ([GiaoVienId]);
GO


CREATE INDEX [IX_PhanCong_LopHocId] ON [PhanCong] ([LopHocId]);
GO


CREATE INDEX [IX_PhanCong_NguoiDuyet] ON [PhanCong] ([NguoiDuyet]);
GO


CREATE INDEX [IX_PhanHoi_DangKyId] ON [PhanHoi] ([DangKyId]);
GO


CREATE INDEX [IX_PhanHoi_NguoiGuiId] ON [PhanHoi] ([NguoiGuiId]);
GO


CREATE INDEX [IX_PhanHoi_NguoiXuLy] ON [PhanHoi] ([NguoiXuLy]);
GO


CREATE INDEX [IX_PhieuKho_BuoiHocId] ON [PhieuKho] ([BuoiHocId]);
GO


CREATE UNIQUE INDEX [IX_PhieuKho_MaPhieu] ON [PhieuKho] ([MaPhieu]);
GO


CREATE INDEX [IX_PhieuKho_NguoiDuyet] ON [PhieuKho] ([NguoiDuyet]);
GO


CREATE INDEX [IX_PhieuKho_NguoiLap] ON [PhieuKho] ([NguoiLap]);
GO


CREATE INDEX [IX_PhieuKho_NguoiNhan] ON [PhieuKho] ([NguoiNhan]);
GO


CREATE INDEX [IX_PhieuKho_NhaCungCapId] ON [PhieuKho] ([NhaCungCapId]);
GO


CREATE INDEX [IX_PhieuKho_PhieuThamChieuId] ON [PhieuKho] ([PhieuThamChieuId]);
GO


CREATE UNIQUE INDEX [IX_PhongHoc_MaPhong] ON [PhongHoc] ([MaPhong]);
GO


CREATE UNIQUE INDEX [IX_QuyDinh_MaBoQuyDinh_SoPhienBan] ON [QuyDinh] ([MaBoQuyDinh], [SoPhienBan]);
GO


CREATE INDEX [IX_QuyDinh_NguoiDuyet] ON [QuyDinh] ([NguoiDuyet]);
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_Email] ON [TaiKhoan] ([Email]) WHERE [Email] IS NOT NULL AND [EmailXacNhanLuc] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_GiaoVienId] ON [TaiKhoan] ([GiaoVienId]) WHERE [GiaoVienId] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_TenDangNhap] ON [TaiKhoan] ([TenDangNhap]);
GO


CREATE UNIQUE INDEX [IX_TaiKhoan_ThanhVienId] ON [TaiKhoan] ([ThanhVienId]) WHERE [ThanhVienId] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_ThanhVien_MaThanhVien] ON [ThanhVien] ([MaThanhVien]);
GO


CREATE INDEX [IX_ThanhVien_NguoiTuVan] ON [ThanhVien] ([NguoiTuVan]);
GO


CREATE INDEX [IX_YeuCauThayDoi_DangKyDichId] ON [YeuCauThayDoi] ([DangKyDichId]);
GO


CREATE INDEX [IX_YeuCauThayDoi_DangKyNguonId] ON [YeuCauThayDoi] ([DangKyNguonId]);
GO


CREATE UNIQUE INDEX [IX_YeuCauThayDoi_IdempotencyKey] ON [YeuCauThayDoi] ([IdempotencyKey]);
GO


CREATE INDEX [IX_YeuCauThayDoi_LopDichId] ON [YeuCauThayDoi] ([LopDichId]);
GO


CREATE INDEX [IX_YeuCauThayDoi_NguoiDuyet] ON [YeuCauThayDoi] ([NguoiDuyet]);
GO


CREATE INDEX [IX_YeuCauThayDoi_NguoiYeuCauId] ON [YeuCauThayDoi] ([NguoiYeuCauId]);
GO


ALTER TABLE [ApDungUuDai] ADD CONSTRAINT [FK_ApDungUuDai_DangKy_DangKyId] FOREIGN KEY ([DangKyId]) REFERENCES [DangKy] ([Id]);
GO


ALTER TABLE [BuoiHoc] ADD CONSTRAINT [FK_BuoiHoc_LopHoc_LopHocId] FOREIGN KEY ([LopHocId]) REFERENCES [LopHoc] ([Id]);
GO


ALTER TABLE [ChiTietPhieuKho] ADD CONSTRAINT [FK_ChiTietPhieuKho_KhoanThuDangKy_KhoanThuDangKyId] FOREIGN KEY ([KhoanThuDangKyId]) REFERENCES [KhoanThuDangKy] ([Id]);
GO


ALTER TABLE [ChiTietPhieuKho] ADD CONSTRAINT [FK_ChiTietPhieuKho_PhieuKho_PhieuKhoId] FOREIGN KEY ([PhieuKhoId]) REFERENCES [PhieuKho] ([Id]);
GO


ALTER TABLE [DaiDienHocVien] ADD CONSTRAINT [FK_DaiDienHocVien_TaiKhoan_NguoiXacNhan] FOREIGN KEY ([NguoiXacNhan]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [DaiDienHocVien] ADD CONSTRAINT [FK_DaiDienHocVien_ThanhVien_ThanhVienId] FOREIGN KEY ([ThanhVienId]) REFERENCES [ThanhVien] ([Id]);
GO


ALTER TABLE [DangKy] ADD CONSTRAINT [FK_DangKy_HoSoTheoHoc_HoSoTheoHocId] FOREIGN KEY ([HoSoTheoHocId]) REFERENCES [HoSoTheoHoc] ([Id]);
GO


ALTER TABLE [DangKy] ADD CONSTRAINT [FK_DangKy_LopHoc_LopHocId] FOREIGN KEY ([LopHocId]) REFERENCES [LopHoc] ([Id]);
GO


ALTER TABLE [DangKy] ADD CONSTRAINT [FK_DangKy_ThanhVien_NguoiDangKyId] FOREIGN KEY ([NguoiDangKyId]) REFERENCES [ThanhVien] ([Id]);
GO


ALTER TABLE [DiemDanh] ADD CONSTRAINT [FK_DiemDanh_TaiKhoan_NguoiGhi] FOREIGN KEY ([NguoiGhi]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [GiangDayBuoi] ADD CONSTRAINT [FK_GiangDayBuoi_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [GiaoDichTien] ADD CONSTRAINT [FK_GiaoDichTien_TaiKhoan_NguoiXacNhan] FOREIGN KEY ([NguoiXacNhan]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [GiaoDichTien] ADD CONSTRAINT [FK_GiaoDichTien_YeuCauThayDoi_YeuCauThayDoiId] FOREIGN KEY ([YeuCauThayDoiId]) REFERENCES [YeuCauThayDoi] ([Id]);
GO


ALTER TABLE [HocBu] ADD CONSTRAINT [FK_HocBu_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [HoSoTheoHoc] ADD CONSTRAINT [FK_HoSoTheoHoc_QuyDinh_QuyDinhId] FOREIGN KEY ([QuyDinhId]) REFERENCES [QuyDinh] ([Id]);
GO


ALTER TABLE [KetQuaKhoaHoc] ADD CONSTRAINT [FK_KetQuaKhoaHoc_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [LopHoc] ADD CONSTRAINT [FK_LopHoc_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [NhatKyThayDoi] ADD CONSTRAINT [FK_NhatKyThayDoi_TaiKhoan_TaiKhoanId] FOREIGN KEY ([TaiKhoanId]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [PhanCong] ADD CONSTRAINT [FK_PhanCong_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [PhanHoi] ADD CONSTRAINT [FK_PhanHoi_TaiKhoan_NguoiXuLy] FOREIGN KEY ([NguoiXuLy]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [PhanHoi] ADD CONSTRAINT [FK_PhanHoi_ThanhVien_NguoiGuiId] FOREIGN KEY ([NguoiGuiId]) REFERENCES [ThanhVien] ([Id]);
GO


ALTER TABLE [PhieuKho] ADD CONSTRAINT [FK_PhieuKho_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [PhieuKho] ADD CONSTRAINT [FK_PhieuKho_TaiKhoan_NguoiLap] FOREIGN KEY ([NguoiLap]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [PhieuKho] ADD CONSTRAINT [FK_PhieuKho_TaiKhoan_NguoiNhan] FOREIGN KEY ([NguoiNhan]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [QuyDinh] ADD CONSTRAINT [FK_QuyDinh_TaiKhoan_NguoiDuyet] FOREIGN KEY ([NguoiDuyet]) REFERENCES [TaiKhoan] ([Id]);
GO


ALTER TABLE [TaiKhoan] ADD CONSTRAINT [FK_TaiKhoan_ThanhVien_ThanhVienId] FOREIGN KEY ([ThanhVienId]) REFERENCES [ThanhVien] ([Id]);
GO
