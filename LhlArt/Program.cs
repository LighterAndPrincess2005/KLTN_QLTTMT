using LhlArt.Data;
using LhlArt.Views;
using LhlArt.Controllers;
using System.Globalization;
using LhlArt.Services;

namespace LhlArt;
internal static class Program
{
    [STAThread]
    static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("vi-VN");
        ApplicationConfiguration.Initialize();
        using var single = new Mutex(true, "MyThuatDemo_CNTT_KLCN082", out bool first);
        if (!first)
        {
            MessageBox.Show("Ứng dụng đang mở. Hãy dùng cửa sổ hiện có.");
            return;
        }

        try
        {
            if (!Environment.GetCommandLineArgs().Contains("--local"))
            {
                RunApi();
                return;
            }
            var service = new CenterService(new JsonStore());
            if (!AccountSetupController.Run(service))
                return;
            while (true)
            {
                using var login = new LoginForm();
                var loginController = new LoginController(service, login);
                if (login.ShowDialog() != DialogResult.OK)
                    break;
                using var main = new MainForm(service.User.Name, service.User.Role);
                var mainController = new MainController(service, main);
                Application.Run(main);
                if (!main.Logout)
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể mở dữ liệu. Dữ liệu cũ được giữ nguyên.\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    static void RunApi()
    {
        using var api = new LhlArt.Networking.ArtApiClient();
        while (true)
        {
            using var login = new LoginForm();
            login.Text = "LHL ART | Chào mừng trở lại";
            login.LoginRequested += async (name, password) =>
            {
                login.SetBusy(true);
                try { await api.Login(name, password); login.LoginSucceeded(); }
                catch (Exception ex) { login.LoginFailed(ex.Message); }
                finally { if (!login.IsDisposed) login.SetBusy(false); }
            };
            if (login.ShowDialog() != DialogResult.OK) break;
            using var main = new ApiManagementForm(api);
            Application.Run(main);
            if (!main.Logout) break;
        }
    }
}
