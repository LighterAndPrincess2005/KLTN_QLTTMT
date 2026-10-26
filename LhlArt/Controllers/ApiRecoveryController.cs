using System.Net.Mail;
using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Views;

namespace LhlArt.Controllers;

internal sealed class ApiRecoveryController(ArtApiClient api)
{
    public async Task<bool> Open(IWin32Window owner)
    {
        JsonElement result=default;
        using(var request=new InputForm("Quên mật khẩu")){
            request.Add("name","Tên đăng nhập");
            request.Add("note","Lưu ý","Chỉ gửi email khi yêu cầu quên mật khẩu. Nếu tài khoản chưa có email hoặc mất quyền truy cập email, hãy liên hệ trung tâm.",readOnly:true,multiline:true);
            request.FieldValidation=()=>{
                var errors=new Dictionary<string,string>();if(string.IsNullOrWhiteSpace(request.TextValue("name")))errors["name"]="Nhập tên đăng nhập.";
                return errors;};
            request.SaveAsync=async()=>result=await api.Post("api/tai-khoan/quen-mat-khau",new{TenDangNhap=request.TextValue("name")});
            if(request.ShowDialog(owner)!=DialogResult.OK)return false;
        }
        using var reset=new InputForm("Nhập mã và đặt mật khẩu mới");
        reset.Add("note","Thông báo",ArtApiClient.Text(result,"thongBao"),readOnly:true,multiline:true);
        reset.Add("code","Mã 6 chữ số trong email");reset.Add("password","Mật khẩu mới",password:true);reset.Add("confirm","Nhập lại mật khẩu",password:true);
        reset.FieldValidation=()=>{
            var errors=new Dictionary<string,string>();var password=reset.TextValue("password");
            if(!System.Text.RegularExpressions.Regex.IsMatch(reset.TextValue("code"),"^[0-9]{6}$"))errors["code"]="Nhập đủ 6 chữ số của mã. Mã chỉ dùng một lần và có hạn 10 phút.";
            if(password.Length is <8 or >128||!password.Any(char.IsLetter)||!password.Any(char.IsDigit))errors["password"]="Mật khẩu cần 8–128 ký tự, có chữ và số.";
            if(password!=reset.TextValue("confirm"))errors["confirm"]="Mật khẩu xác nhận chưa khớp.";return errors;};
        reset.SaveAsync=async()=>await api.Post("api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=ArtApiClient.Text(result,"maYeuCau"),Ma=reset.TextValue("code"),MatKhauMoi=reset.TextValue("password")});
        return reset.ShowDialog(owner)==DialogResult.OK;
    }
}
