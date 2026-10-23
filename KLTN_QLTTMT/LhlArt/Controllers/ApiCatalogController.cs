using System.Globalization;
using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Views;

namespace LhlArt.Controllers;

// Biểu mẫu API dùng chung; dữ liệu và quyền được kiểm tra lại tại máy chủ.
internal sealed class ApiCatalogController(ArtApiClient api)
{
    record Field(string Key, string Label, object Value, string? Lookup = null, string? Name = null, string[]? Options = null);
    static Field Text(string key,string label,string value="") => new(key,label,value);
    static Field Number(string key,string label,decimal value=0) => new(key,label,key is "HocPhi" or "HocPhiApDung" or "GiaBanThamKhao" or "MucDatHang" ? (object)value : (int)value);
    static Field Link(string key,string label,string route,string name) => new(key,label,"",route,name);
    static Field State(string initial="HOAT_DONG",params string[] options) => new("TrangThai","Trạng thái",initial,Options:options.Length>0?options:["HOAT_DONG","NGUNG","NHAP"]);
    static Field[] Fields(string route) => route.Split('/').Last() switch
    {
        "loai-khoa-hoc" => [Text("MaLoai","Mã loại"),Text("TenLoai","Tên loại"),Text("MoTa","Mô tả"),State()],
        "khoa-hoc" => [Link("LoaiKhoaHocId","Loại khóa","loai-khoa-hoc","TenLoai"),Text("MaKhoa","Mã khóa"),Text("TenKhoa","Tên khóa"),Number("PhienBan","Phiên bản",1),Text("CapDo","Cấp độ"),Number("TuoiToiThieu","Tuổi tối thiểu",6),Text("YeuCauDauVao","Yêu cầu đầu vào"),Text("MucTieu","Mục tiêu"),Number("SoBuoi","Số buổi",16),Number("SoTietMoiBuoi","Số tiết mỗi buổi",4),Number("PhutMoiTiet","Phút mỗi tiết",30),Number("HocPhi","Học phí"),new("HieuLucTu","Ngày hiệu lực",DateTime.Today),State("NHAP","NHAP","HOAT_DONG","NGUNG")],
        "noi-dung-buoi" => [Link("KhoaHocId","Khóa học","khoa-hoc","TenKhoa"),Number("ThuTu","Thứ tự buổi",1),Text("ChuDe","Chủ đề"),Text("NoiDung","Nội dung"),Number("SoTiet","Số tiết",4),Text("YeuCauSanPham","Yêu cầu sản phẩm")],
        "lop-hoc" => [Link("KhoaHocId","Khóa học","khoa-hoc","TenKhoa"),Text("MaLop","Mã lớp"),Text("TenLop","Tên lớp"),new("NgayKhaiGiangDuKien","Ngày khai giảng",DateTime.Today.AddDays(21)),Number("SoBuoiKeHoach","Số buổi kế hoạch",16),Text("LichHocDuKien","Mô tả lịch học"),Number("SiSoToiThieu","Sĩ số tối thiểu",6),Number("SiSoToiDa","Sĩ số tối đa",12),Number("HocPhiApDung","Học phí áp dụng"),new("MoDangKyLuc","Mở đăng ký lúc",DateTime.Today),new("DongDangKyLuc","Đóng đăng ký lúc",DateTime.Today.AddDays(18)),State("NHAP","NHAP","DANG_TUYEN_SINH","DANG_HOC","DA_KET_THUC")],
        "giao-vien" => [Text("MaGiaoVien","Mã giáo viên"),Text("HoTen","Họ tên"),Text("DienThoai","Điện thoại"),Text("TrinhDo","Trình độ"),Text("ChuyenMon","Chuyên môn"),Text("CapDoCoTheDay","Cấp độ có thể dạy"),State()],
        "phong-hoc" => [Text("MaPhong","Mã phòng"),Text("TenPhong","Tên phòng"),Number("SucChua","Sức chứa",12),State()],
        "hoa-cu" => [Text("MaHoaCu","Mã họa cụ"),Text("TenHoaCu","Tên họa cụ"),Text("QuyCach","Quy cách"),Text("DonViCoSo","Đơn vị"),new("NhomHoaCu","Nhóm họa cụ","TIEU_HAO",Options:["TIEU_HAO","DUNG_CHUNG","BAN_RIENG"]),Number("GiaBanThamKhao","Giá bán"),Number("MucDatHang","Mức đặt hàng",10),State()],
        "nha-cung-cap" => [Text("MaNCC","Mã nhà cung cấp"),Text("TenNCC","Tên nhà cung cấp"),Text("DienThoai","Điện thoại"),State()],
        _ => []
    };
    public bool CanStop(string route) => new[]{"loai-khoa-hoc","khoa-hoc","giao-vien","phong-hoc","hoa-cu","nha-cung-cap"}.Contains(route.Split('/').Last());
    public async Task Stop(IWin32Window owner,string route,JsonElement row)
    {
        if(MessageBox.Show(owner,"Ngừng sử dụng mục này? Lịch sử đã phát sinh được giữ lại.","Ngừng sử dụng",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        await api.Put(route+"/"+ArtApiClient.Text(row,"id"),new{TrangThai="NGUNG"},ArtApiClient.Text(row,"rowVersion"));
    }
    public bool Supports(string route) => route.StartsWith("api/quan-ly/") && Fields(route).Length > 0;
    public async Task<List<Choice>> Choices(string route,string name)
    {
        var result=new List<Choice>();
        for(int page=1;;page++)
        {
            var data=await api.Read($"api/quan-ly/{route}?trang={page}&kichThuoc=100");
            var rows=ArtApiClient.Get(data,"duLieu").EnumerateArray().ToArray();
            result.AddRange(rows.Select(x=>new Choice(ArtApiClient.Text(x,"id"),ArtApiClient.Text(x,name))));
            if(rows.Length==0||result.Count>=ArtApiClient.Get(data,"tongSo").GetInt32())break;
        }
        return result;
    }
    public async Task Edit(IWin32Window owner,string route,string title,JsonElement? row)
    {
        var fields=Fields(route);
        using var form=new InputForm((row.HasValue?"Cập nhật ":"Thêm ")+title.ToLowerInvariant());
        foreach(var f in fields)
        {
            object value=f.Value;
            if(row.HasValue&&ArtApiClient.TryGet(row.Value,f.Key,out var existing)&&existing.ValueKind!=JsonValueKind.Null)
                value=f.Value is int?existing.GetInt32():f.Value is decimal?existing.GetDecimal():f.Value is DateTime?DateTime.Parse(existing.GetString()!,CultureInfo.InvariantCulture):existing.ToString();
            IEnumerable<Choice>? choices=f.Options?.Select(x=>new Choice(x,x switch {"HOAT_DONG"=>"Hoạt động","NGUNG"=>"Ngừng","NHAP"=>"Bản nháp","DANG_TUYEN_SINH"=>"Đang tuyển sinh","DANG_HOC"=>"Đang học","DA_KET_THUC"=>"Đã kết thúc","TIEU_HAO"=>"Vật tư tiêu hao","DUNG_CHUNG"=>"Dụng cụ dùng chung","BAN_RIENG"=>"Bán riêng",_=>x}));
            if(f.Lookup is not null)choices=await Choices(f.Lookup,f.Name!);
            form.Add(f.Key,f.Label,value,choices);
        }
        Dictionary<string,object?>? body=null;
        form.ValidateAndSave=()=>
        {
            body=new();
            foreach(var f in fields)
            {
                object value=f.Value is int?(object)(int)form.Number(f.Key):f.Value is decimal?form.Number(f.Key):f.Value is DateTime?(f.Key=="HieuLucTu"||f.Key=="NgayKhaiGiangDuKien"?form.Date(f.Key).ToString("yyyy-MM-dd"):form.Date(f.Key).ToString("yyyy-MM-ddTHH:mm:ss")):form.TextValue(f.Key);
                if(f.Lookup is not null)value=long.Parse(form.TextValue(f.Key),CultureInfo.InvariantCulture);
                body[f.Key]=value;
            }
        };
        form.SaveAsync=async()=>
        {
            if(body is null)return;
            if(row.HasValue)await api.Put(route+"/"+ArtApiClient.Text(row.Value,"id"),body,ArtApiClient.Text(row.Value,"rowVersion"));
            else await api.Post(route,body);
        };
        form.ShowDialog(owner);
    }
}
