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
        using var single = new Mutex(true, "LhlArt_Api_Management", out bool first);
        if (!first)
        {
            MessageBox.Show("Ứng dụng đang mở. Hãy dùng cửa sổ hiện có.");
            return;
        }

        try
        {
            RunApi();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể mở dữ liệu. Dữ liệu cũ được giữ nguyên.\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    static void RunApi()
    {
        using var api = new LhlArt.Networking.ArtApiClient();
        var rememberedStore=new RememberedLoginStore(api.BaseUrl);
        RememberedLogin? remembered=null;
        try{remembered=rememberedStore.Read();}catch(Exception ex){MessageBox.Show("Không đọc được thông tin đăng nhập đã lưu trên Windows. Bạn vẫn có thể đăng nhập bình thường.\n"+ex.Message,"Ghi nhớ đăng nhập");}
        while (true)
        {
            using var login = new LoginForm();
            login.Text = "LHL ART | Chào mừng trở lại";
            if(remembered is not null){login.SetRememberedLogin(remembered.Username,remembered.Password);remembered=null;}
            login.LoginRequested += async (name, password) =>
            {
                login.SetBusy(true);
                try {
                    await api.Login(name, password);
                    try{if(login.RememberLogin)rememberedStore.Save(name,password);else rememberedStore.Clear();}
                    catch(Exception ex){MessageBox.Show(login,"Đăng nhập thành công nhưng không cập nhật được thông tin ghi nhớ trên Windows.\n"+ex.Message,"Ghi nhớ đăng nhập");}
                    login.LoginSucceeded();
                }
                catch (Exception ex) {
                    if(ex is LhlArt.Networking.ApiLoginRejectedException){try{rememberedStore.Clear();}catch{/* Keep the authentication error visible; never log secrets. */}}
                    login.LoginFailed(ex.Message);
                }
                finally { if (!login.IsDisposed) login.SetBusy(false); }
            };
            if (login.ShowDialog() != DialogResult.OK) {if(!login.RememberLogin)rememberedStore.Clear();break;}
            using var main = new ApiManagementForm(api);
            Application.Run(main);
            if (!main.Logout) break;
            rememberedStore.Clear();
        }
    }
}
