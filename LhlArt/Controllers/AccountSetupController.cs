using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;
public static class AccountSetupController
{
    public static bool Run(CenterService service)
    {
        if (service.D.Accounts.Count > 0)
            return true;
        using var form = new InputForm("Thiết lập tài khoản quản trị");
        form.Add("user", "Tên đăng nhập", "");
        form.Add("password", "Mật khẩu (ít nhất 8 ký tự)", "", password: true);
        form.Add("repeat", "Nhập lại mật khẩu", "", password: true);
        form.ValidateAndSave = () =>
        {
            CenterService.Check(form.TextValue("password") == form.TextValue("repeat"), "Hai mật khẩu chưa khớp.");
            service.InitializeAdministrator(form.TextValue("user"), form.TextValue("password"));
        };
        return form.ShowDialog() == DialogResult.OK;
    }
}
