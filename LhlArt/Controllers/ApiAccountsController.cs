using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;

internal sealed class ApiAccountsController(ArtApiClient api)
{
    public bool PasswordChanged {get;private set;}
    public Task ChangeOwnPassword(IWin32Window owner)
    {
        using var form=new InputForm("Đổi mật khẩu của tôi");
        form.Add("Old","Mật khẩu hiện tại",password:true);form.Add("New","Mật khẩu mới",password:true);form.Add("Repeat","Nhập lại mật khẩu mới",password:true);
        form.SaveAsync=async()=>{ProfileRules.Password(form.TextValue("New"),form.TextValue("Repeat"));await api.Post("api/tai-khoan/doi-mat-khau",new{MatKhauCu=form.TextValue("Old"),MatKhauMoi=form.TextValue("New")});PasswordChanged=true;};
        form.ShowDialog(owner);return Task.CompletedTask;
    }
    static Choice[] Roles => [new("HOC_VU", "Học vụ"), new("THU_NGAN", "Thu ngân"), new("GIAO_VIEN", "Giáo viên"), new("QUAN_LY_KHO", "Quản lý kho"), new("QUAN_TRI", "Quản trị")];
    public async Task Create(IWin32Window owner)
    {
        var teachers = await new ApiDataService(api).All("api/quan-ly/giao-vien");
        using var form = new InputForm("Tạo tài khoản nhân viên");
        form.Add("TenDangNhap", "Tên đăng nhập");form.Add("HoTen", "Họ tên nhân viên");
        form.Add("Email","Email (bắt buộc)");
        form.Add("MatKhau", "Mật khẩu", password:true);form.Add("LapLai", "Nhập lại mật khẩu", password:true);
        form.Add("VaiTro", "Vai trò", "HOC_VU", Roles);
        form.Add("GiaoVienId", "Hồ sơ giáo viên", "", teachers.Where(x=>ArtApiClient.Text(x,"trangThai")=="HOAT_DONG").Select(x=>new Choice(ApiDataService.Id(x).ToString(),ArtApiClient.Text(x,"hoTen"))));
        form.Enable("GiaoVienId", false);form.Changed("VaiTro",()=>form.Enable("GiaoVienId",form.TextValue("VaiTro")=="GIAO_VIEN"));
        form.SaveAsync=async()=>
        {
            ProfileRules.Password(form.TextValue("MatKhau"),form.TextValue("LapLai"));
            var username=ProfileRules.Required(form.TextValue("TenDangNhap"),"Tên đăng nhập",100);
            if(username.Length<3)throw new InvalidOperationException("Tên đăng nhập cần ít nhất 3 ký tự.");
            long? teacher=null;
            if(form.TextValue("VaiTro")=="GIAO_VIEN")teacher=long.TryParse(form.TextValue("GiaoVienId"),out var id)?id:throw new InvalidOperationException("Chọn hồ sơ giáo viên.");
            await api.Post("api/tai-khoan",new {TenDangNhap=username,MatKhau=form.TextValue("MatKhau"),Email=ApiPeopleController.RequiredEmail(form.TextValue("Email")),HoTen=ProfileRules.Required(form.TextValue("HoTen"),"Họ tên"),VaiTro=form.TextValue("VaiTro"),GiaoVienId=teacher,ThanhVienId=(long?)null,QuyenBoSung=0});
        };
        form.ShowDialog(owner);
    }
    public Task Edit(IWin32Window owner,JsonElement row)
    {
        var role=ArtApiClient.Text(row,"vaiTroChinh");
        using var form=new InputForm("Vai trò và trạng thái tài khoản");
        form.Add("TenDangNhap","Tên đăng nhập",ArtApiClient.Text(row,"tenDangNhap"),readOnly:true);
        // API không cho đổi liên kết thành viên / giáo viên tại endpoint cấp quyền.
        var roles=role=="THANH_VIEN"?new[]{new Choice("THANH_VIEN","Thành viên")}:Roles.Where(x=>x.Id!="GIAO_VIEN"||ApiDataService.Id(row,"giaoVienId")>0).ToArray();
        form.Add("VaiTro","Vai trò",role,roles);
        form.Add("HoatDong","Tài khoản hoạt động",ArtApiClient.Text(row,"trangThai")=="HOAT_DONG");
        form.SaveAsync=async()=>await api.Put("api/tai-khoan/"+ApiDataService.Id(row)+"/quyen",new {VaiTro=form.TextValue("VaiTro"),QuyenBoSung=ArtApiClient.Get(row,"quyenBoSung").GetInt64(),HoatDong=form.Flag("HoatDong")});
        form.ShowDialog(owner);return Task.CompletedTask;
    }
    public Task Reset(IWin32Window owner,JsonElement row)
    {
        using var form=new InputForm("Đặt lại mật khẩu · "+ArtApiClient.Text(row,"tenDangNhap"));
        form.Add("MatKhau","Mật khẩu mới",password:true);form.Add("LapLai","Nhập lại mật khẩu",password:true);form.Add("LyDo","Lý do đặt lại");
        form.SaveAsync=async()=>
        {
            ProfileRules.Password(form.TextValue("MatKhau"),form.TextValue("LapLai"));
            await api.Post("api/tai-khoan/"+ApiDataService.Id(row)+"/dat-lai-mat-khau",new {MatKhauMoi=form.TextValue("MatKhau"),LyDo=ProfileRules.Required(form.TextValue("LyDo"),"Lý do",500)});
        };
        form.ShowDialog(owner);return Task.CompletedTask;
    }
}
