using LhlArt.Models;

namespace LhlArt.Services;
public partial class CenterService
{
    public static IReadOnlyList<string> Roles { get; } = Array.AsReadOnly(new[] { "Quản lý", "Học vụ", "Kế toán", "Giáo viên", "Họa cụ" });

    public void SaveAccount(string id, string username, string role, string teacherId, bool enabled, string password, bool create)
    {
        Need();
        username = username.Trim();
        Check(username.Length >= 3 && username.Length <= 40 && username.All(x => char.IsLetterOrDigit(x) || x == '_' || x == '.'), "Tên đăng nhập gồm 3–40 chữ, số, dấu chấm hoặc gạch dưới.");
        Check(Roles.Contains(role), "Chọn vai trò hợp lệ.");
        Check(!D.Accounts.Any(x => x.Id != id && x.Name.Equals(username, StringComparison.OrdinalIgnoreCase)), "Tên đăng nhập đã được sử dụng.");
        var current = D.Accounts.SingleOrDefault(x => x.Id == id);
        Check(create ? current == null : current != null, "Tài khoản không hợp lệ.");
        if (create)
            Check(password.Length >= 8, "Mật khẩu cần ít nhất 8 ký tự.");
        if (role == "Giáo viên")
        {
            Check(D.Teachers.Any(x => x.Id == teacherId), "Chọn hồ sơ giáo viên gắn với tài khoản.");
            Check(!D.Accounts.Any(x => x.Id != id && x.Role == "Giáo viên" && x.TeacherId == teacherId), "Hồ sơ giáo viên đã được gắn với tài khoản khác.");
        }
        else
            teacherId = "";
        if (!create)
        {
            Check(username == current!.Name, "Tên đăng nhập được giữ cố định để bảo toàn lịch sử.");
            Check(current.Id != User.Id || enabled && role == current.Role, "Không khóa hoặc thay đổi vai trò của tài khoản đang đăng nhập.");
            Check(current.Role != "Quản lý" || !current.Enabled || role == "Quản lý" && enabled || D.Accounts.Any(x => x.Id != id && x.Role == "Quản lý" && x.Enabled), "Cần giữ ít nhất một tài khoản quản lý đang hoạt động.");
        }

        Commit((create ? "Tạo tài khoản " : "Cập nhật tài khoản ") + username, () =>
        {
            var account = current ?? new Account
            {
                Id = id,
                Name = username,
                PasswordHash = Passwords.Hash(password)
            };
            account.Role = role;
            account.TeacherId = teacherId;
            account.Enabled = enabled;
            if (create)
                D.Accounts.Add(account);
        });
        User = D.Accounts.Single(x => x.Id == User.Id);
    }

    public void ResetAccountPassword(string id, string password)
    {
        Need();
        Check(id != User.Id, "Dùng mục Đổi mật khẩu cho tài khoản đang đăng nhập.");
        Check(password.Length >= 8, "Mật khẩu cần ít nhất 8 ký tự.");
        var account = Get<Account>(id);
        Commit("Đặt lại mật khẩu tài khoản " + account.Name, () => account.PasswordHash = Passwords.Hash(password));
    }
}
