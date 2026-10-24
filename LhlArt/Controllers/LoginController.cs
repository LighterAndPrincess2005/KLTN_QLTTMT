using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;
public sealed class LoginController
{
    public LoginController(CenterService service, LoginForm view)
    {
        view.LoginRequested += (username, password) =>
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                view.LoginFailed("Vui lòng nhập tên đăng nhập và mật khẩu.");
                return;
            }

            try
            {
                if (service.Login(username, password))
                    view.LoginSucceeded();
                else
                    view.LoginFailed("Tên đăng nhập hoặc mật khẩu chưa đúng.");
            }
            catch (Exception ex)
            {
                view.LoginFailed(ex.Message);
            }
        };
    }
}
