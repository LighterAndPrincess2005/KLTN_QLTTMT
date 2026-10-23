using System.ComponentModel;
using LhlArt.Models;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;
public partial class MainController
{
    void Accounts()
    {
        CenterService.Check(s.Is(), "Chỉ quản lý được sử dụng chức năng này.");
        var data = s.D.Accounts.OrderBy(x => x.Name).Select(x => new AccountRow(x.Id, x.Name, x.Role, s.D.Teachers.FirstOrDefault(t => t.Id == x.TeacherId)?.Name ?? "—", x.Enabled ? "Đang hoạt động" : "Đã khóa")).ToList();
        Show("Tài khoản nhân viên", "", data, Accounts);
        Button("Thêm tài khoản", () => EditAccount(true));
        Button("Sửa quyền / trạng thái", () => EditAccount(false));
        Button("Đặt lại mật khẩu", () =>
        {
            var row = Selected<AccountRow>();
            using var form = new InputForm("Đặt lại mật khẩu · " + row.Name);
            form.Add("password", "Mật khẩu mới (ít nhất 8 ký tự)", "", password: true);
            form.Add("repeat", "Nhập lại mật khẩu", "", password: true);
            form.ValidateAndSave = () =>
            {
                CenterService.Check(form.TextValue("password") == form.TextValue("repeat"), "Hai mật khẩu chưa khớp.");
                s.ResetAccountPassword(row.Id, form.TextValue("password"));
            };
            if (form.ShowDialog(view) == DialogResult.OK)
            {
                Accounts();
                MessageBox.Show(view, "Đã đặt lại mật khẩu.", "Tài khoản");
            }
        });
    }

    void EditAccount(bool create)
    {
        var account = create ? new Account
        {
            Id = CenterService.NewId("TK"),
            Role = "Học vụ"
        }

        : s.Get<Account>(Selected<AccountRow>().Id);
        using var form = new InputForm(create ? "Thêm tài khoản nhân viên" : "Quyền và trạng thái · " + account.Name);
        form.Add("name", "Tên đăng nhập", account.Name, readOnly: !create);
        form.Add("role", "Vai trò", account.Role, CenterService.Roles.Select(x => new Choice(x, x == "Họa cụ" ? "Phụ trách họa cụ" : x)));
        form.Add("teacher", "Hồ sơ giáo viên", account.TeacherId, new[] { new Choice("", "Không áp dụng") }.Concat(s.D.Teachers.Select(x => new Choice(x.Id, x.Id + " · " + x.Name))));
        form.Add("enabled", "Cho phép đăng nhập", account.Enabled);
        if (create)
        {
            form.Add("password", "Mật khẩu (ít nhất 8 ký tự)", "", password: true);
            form.Add("repeat", "Nhập lại mật khẩu", "", password: true);
        }

        form.ValidateAndSave = () =>
        {
            if (create)
                CenterService.Check(form.TextValue("password") == form.TextValue("repeat"), "Hai mật khẩu chưa khớp.");
            s.SaveAccount(account.Id, form.TextValue("name"), form.TextValue("role"), form.TextValue("teacher"), form.Flag("enabled"), create ? form.TextValue("password") : "", create);
        };
        if (form.ShowDialog(view) == DialogResult.OK)
            Accounts();
    }

    record AccountRow([property: DisplayName("Mã")] string Id, [property: DisplayName("Tên đăng nhập")] string Name, [property: DisplayName("Vai trò")] string Role, [property: DisplayName("Giáo viên liên kết")] string Teacher, [property: DisplayName("Trạng thái")] string Status);
}
