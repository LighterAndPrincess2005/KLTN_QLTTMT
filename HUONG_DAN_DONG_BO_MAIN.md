# Bản ghép main và WinForms/API

- Nền GitHub `main`: `a4d3bbc4552818594b8dd718372a6ffedee25b95` (02/11/2026).
- Đã áp dụng các thay đổi của ZIP `KLTN_QLTTMT_WinForms_API_ma_thong_nhat.zip` trên nền này. Các tệp mới ở main về SePay, thông báo, quản lý lớp giáo viên, tìm kiếm và các script `010`–`012` được giữ nguyên.
- Mã nguồn và database là hai phần khác nhau. Main không chứa `.bak`, `.mdf`, `.bacpac` hay file database. Các tệp `MainData.cs`, `DemoData.cs`, `PaymentDemoSetup.cs` chỉ là lệnh tạo/nâng cấp dữ liệu khi được chạy rõ ràng, không phải bản sao database nhóm trưởng.

## Kiểm tra trước khi chạy

Mở PowerShell trong `backend\MyThuat` của thư mục vừa giải nén, chạy:

```powershell
dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --db-info
```

Chạy cùng lệnh trên máy nhóm trưởng. So sánh máy SQL, tên database và số lượng thành viên, học viên, lớp, buổi học, đăng ký. LocalDB `(localdb)\MSSQLLocalDB` thuộc tài khoản Windows trên từng máy; hai máy cùng tên `MyThuatDb` vẫn có thể có dữ liệu hoàn toàn khác nhau.

Nếu muốn **cùng dữ liệu thật**, nhóm trưởng phải xuất backup/database từ SQL Server, bạn khôi phục vào SQL Server của mình rồi cấu hình `MyThuat.Api/appsettings.Local.json` trỏ đúng database đã khôi phục. Giữ bản sao lưu database hiện tại trước khi khôi phục, và chuyển ảnh/cấu hình riêng nếu ứng dụng cần. Không chạy `--seed-demo` hoặc `--prepare-main` với mục đích thay thế backup: hai lệnh đó chỉ tạo dữ liệu mẫu.

## Nâng cấp database đang có lên mã mới

Sau khi xác nhận đúng database và sao lưu, dừng API, chạy từng lệnh từ `backend\MyThuat`:

```powershell
dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --initialize

dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --upgrade-codes
dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --upgrade-class-inventory
dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --upgrade-notifications
dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --upgrade-class-notices
dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --prepare-payments
```

Các script nâng cấp `010`, `011`, `012` có kiểm tra bảng đã tồn tại. `--prepare-payments` cũng bổ sung lớp trải nghiệm và địa chỉ phòng mẫu theo `docs/sepay.md`; chỉ chạy sau khi đã xem dữ liệu hiện có và muốn áp dụng phần đó. Nếu database đã được nhóm trưởng nâng cấp, không cần chạy lại chỉ để đồng bộ bản ghi. Lệnh `--initialize` không tải dữ liệu từ GitHub.

## Chạy ba phần

Từ `backend\MyThuat`: `dotnet run --project MyThuat.Api\MyThuat.Api.csproj -- --urls http://127.0.0.1:5080`.
Từ thư mục gốc, trong hai terminal khác: `dotnet run --project LhlArt\LhlArt.csproj -c Release` và `npm.cmd install`, `npm.cmd run dev`.

Mã của các bản ghi cũ không bị đổi; bản ghi mới được API cấp mã cùng quy tắc cho web và WinForms. Dữ liệu mẫu `MAIN_`/`DEMO_` vẫn được giữ. Trong WinForms mới, quản trị có nút **Nguồn dữ liệu đang dùng**; dùng nút này hoặc `--db-info` để đối chiếu database trực tiếp.
