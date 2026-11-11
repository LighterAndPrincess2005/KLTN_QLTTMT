# API quản lý trung tâm Mỹ Thuật Bụi

Mã nguồn API nằm tại `backend/MyThuat/MyThuat.Api/`. API dùng C# ASP.NET Core, Entity Framework Core và SQL Server, phục vụ website và ứng dụng WinForms của khóa luận CNTT-KLCN082.

## Chạy API

1. Cài đặt và chạy theo [README chính](../../README.md), gồm thiết lập database và khởi động API tại `http://127.0.0.1:5080`. Nút Desktop được tạo bởi `Tao_Nut_Desktop.cmd` là lựa chọn bổ sung.
2. Lần đầu tạo tài khoản quản trị trên trang vừa mở. Tự chọn mật khẩu từ 8 ký tự, có chữ và số; nên dùng mật khẩu dài, riêng biệt khi vận hành thật. Bộ tài khoản MAIN được khởi tạo riêng theo lệnh, không phải mật khẩu mặc định của API.
3. Đăng nhập, sao chép token, mở Swagger → Authorize → dán token để gọi API có phân quyền.
4. Khi cần tắt API, bấm `Tat_API.cmd` trong thư mục project. Đóng tab trình duyệt chỉ đóng trang, API vẫn chạy.

Có thể chạy bằng `dotnet run` từ mã nguồn sau khi cài SDK và cấu hình SQL Server. Nếu dùng bản publish và nút Desktop, publish lại khi thay đổi code và tạo lại nút khi đổi thư mục.

## Quyền mặc định

| Vai trò | Quyền |
|---|---|
| QUAN_TRI | Tất cả; quản lý tài khoản, nhật ký và duyệt phương án tài chính |
| HOC_VU | Khóa, lớp, lịch, hồ sơ học viên, điểm danh và báo cáo học vụ |
| THU_NGAN | Thu tiền, công nợ, hoàn tiền theo phương án đã duyệt |
| GIAO_VIEN | Điểm danh và nhận xét trong lớp/buổi được phân công |
| QUAN_LY_KHO | Chứng từ nhập, cấp, trả, dự trù và tồn họa cụ |
| THANH_VIEN | Hồ sơ/đăng ký được xác minh quyền đại diện; tiến độ và phản hồi của mình |

Quản trị cấp thêm quyền nghiệp vụ cho nhân viên khi cần. Quyền quản lý tài khoản và nhật ký luôn yêu cầu vai trò quản trị. API kiểm tra quyền trên server, bao gồm quyền sở hữu hồ sơ và phân công giáo viên; không phụ thuộc nút ẩn trên giao diện.

## Các nhóm nghiệp vụ

- Danh mục công khai: loại khóa, khóa, đề cương từng buổi và lớp tuyển sinh.
- Quản lý: giáo viên, học viên, thành viên/đại diện, phòng, khóa có phiên bản, lớp có tên, số buổi, số tiết, thời lượng; khuyến mãi và quy định.
- Lịch học: sinh lịch theo nội dung khóa; kiểm tra trùng phòng, giáo viên, học viên; phân công, đổi lịch, thay giáo viên, hoãn/hủy lớp.
- Đăng ký: kiểm tra đại diện, đầu vào, tuổi tối thiểu, cửa sổ đăng ký, sĩ số; giữ chỗ có hạn, ưu đãi và khoản thu họa cụ.
- Tài chính: thu từng phần, xác nhận đủ tiền, ghi nhận tiền chờ đối chiếu, công nợ và hoàn tiền thực tế theo quyết định. API không tự giả lập giao dịch ngân hàng.
- Thay đổi: yêu cầu hủy/chuyển, quản trị duyệt, giữ lớp đích, bù chênh lệch và hoàn tất chuyển. Giữ lịch sử lớp nguồn.
- Học tập: điểm danh, báo nghỉ, học bù, nhận xét, tiến độ, đề xuất/chốt kết quả.
- Kho: nhập, tiêu hao, bán riêng, cấp/trả dụng cụ, hỏng mất, dự trù, kiểm kê, điều chỉnh có chứng từ.
- Phản hồi, báo cáo và nhật ký thay đổi.

Website và WinForms đã kết nối các endpoint nghiệp vụ. API có tích hợp SMTP cho khôi phục mật khẩu và thông báo, cùng webhook SePay để ghi nhận chuyển khoản. Cần cấu hình dịch vụ tương ứng trên máy chạy API. Quản trị xem và kích hoạt quy định trung tâm trước khi nhận đăng ký.

## Quy ước gọi API

- Swagger tại `http://localhost:5080/swagger` liệt kê endpoint và mẫu dữ liệu.
- Gọi bằng HTTP/JSON; gửi `Authorization: Bearer <accessToken>`. Token 30 phút. Đăng xuất/đổi mật khẩu/khóa tài khoản thu hồi token cũ.
- Mật khẩu được băm; không trả PasswordHash. Nhật ký không lưu mật khẩu hoặc token.
- Cập nhật danh mục bằng PUT kèm `If-Match` là RowVersion Base64 vừa đọc. Khi nhận 409, tải lại dữ liệu trước khi sửa.
- Đăng ký và giao dịch gửi khóa idempotency riêng cho từng thao tác; retry giữ nguyên khóa và nội dung. Đổi nội dung phải dùng khóa mới.
- Thời gian nghiệp vụ theo Việt Nam; tiền là VND nguyên, số lượng kho tối đa 3 chữ số thập phân.
- 401: chưa đăng nhập/token hết hiệu lực; 403: thiếu quyền; 400: dữ liệu không hợp lệ; 409: xung đột hoặc ràng buộc nghiệp vụ.

## Database và mã nguồn

Các bảng nghiệp vụ có khóa ngoại, chỉ mục, RowVersion và thời điểm tạo/cập nhật. Không có tuổi tối đa, lớp phổ thông hoặc trường đang học của học viên. Chuyên môn/năng lực giảng dạy nằm trong thuộc tính giáo viên.

`MyThuat.Api/Database/002_SchemaHoanThien.sql` là script schema hiện tại để xem xét. `001_SchemaBanDau.sql` chỉ là bản lưu cũ, không dùng để tạo database cho phiên bản này. Khi database mới chưa tồn tại, nút Desktop gọi `--initialize` để tạo bằng model hiện tại. Database cũ không bị xóa. API tự bổ sung cột ngày sinh còn thiếu của `ThanhVien` (cho phép NULL, giữ nguyên dữ liệu). Các khác biệt schema khác vẫn cần xem log để xử lý riêng. Không chạy script tạo bảng lên database đang có dữ liệu.

Mã nguồn ở Controllers, Contracts, Services, Models, Data và Security. Mở `MyThuat.sln` trong Visual Studio 2022 để sửa. Ứng dụng target .NET 8. Tệp `global.json` trong thư mục này yêu cầu SDK 9.0.304 và cho phép các bản vá cùng nhánh theo `latestPatch`.

Sửa code xong cần publish lại trước khi nút Desktop dùng bản mới:

```powershell
Set-Location backend/MyThuat
dotnet build MyThuat.sln -c Release
dotnet publish MyThuat.Api/MyThuat.Api.csproj -c Release --no-restore -o MyThuat.Api/.local/publish
```

Tắt API trước khi publish. Chạy `dotnet run --project MyThuat.Checks -c Release -- --sqlserver` bằng tài khoản Windows sở hữu LocalDB để kiểm tra. Bộ kiểm tra dùng database tạm có tên MyThuatChecks_..., không dùng MyThuatDb.

## Cấu hình và dữ liệu cần giữ

Chuỗi mặc định: `Server=(localdb)\MSSQLLocalDB;Database=MyThuatDb;Trusted_Connection=True;TrustServerCertificate=True`. Có thể ghi đè trong `MyThuat.Api/appsettings.Local.json`. LocalDB và cơ chế bảo vệ khóa gắn với tài khoản Windows hiện tại: đổi người dùng/máy cần cấu hình lại và sao lưu/chuyển database đúng cách.

Database không nằm trong file ZIP mã nguồn. Sao lưu database riêng; giữ khóa `.local/keys` của bản đang sử dụng. ZIP bàn giao loại bỏ khóa, token dừng, log và cấu hình riêng. Sau khi giải nén ở máy khác, cần .NET SDK/runtime phù hợp, SQL Server LocalDB, kết nối NuGet lần restore đầu và tạo lại nút Desktop.

Web chạy ở origin khác phải khai báo danh sách `WebOrigins` trong cấu hình. WinForms gọi HTTP trực tiếp. Bản Desktop chỉ nghe loopback của máy; đưa lên Internet cần HTTPS, server SQL và cấu hình triển khai riêng.

Nhật ký: `MyThuat.Api/.local/khoi-dong.log`, `api.log`, `api-error.log`. Nếu báo lỗi, xem các file này để biết bước thất bại.


## Nạp dữ liệu mẫu từ Word

Bộ mẫu DemoData có 6 khóa, 6 lớp và 88 buổi cùng phòng, giáo viên và học viên mẫu. Tạo quản trị trước, rồi chạy `Nap_DuLieuMau.cmd`. Xem [DU_LIEU_MAU.md](DU_LIEU_MAU.md) để biết học phí, lịch và cách nạp trên máy khác. Mật khẩu sinh riêng trên máy nạp, không đưa lên Git.

## Đối chiếu dữ liệu và mã hồ sơ

Chạy `dotnet run --project MyThuat.Api/MyThuat.Api.csproj -- --db-info` trong thư mục `backend/MyThuat` trên cả hai máy để so tên máy SQL, tên database và số bản ghi. Trong WinForms, quản trị có nút **Nguồn dữ liệu đang dùng** để xem đúng API và số liệu máy mình. Bản mới hiện chữ `bản 02.11.2026` trên thanh tiêu đề để phân biệt cửa sổ đang chạy từ thư mục cũ. LocalDB mặc định là database theo tài khoản Windows trên từng máy; ZIP mã nguồn không chứa dữ liệu của máy nhóm trưởng. Nếu hai máy cho kết quả khác nhau, cần nhóm trưởng sao lưu/chuyển database và cấu hình cùng nguồn dữ liệu; không nạp dữ liệu mẫu thay dữ liệu thật.

Mã mới được cấp tại API cho cả WinForms và web: loại khóa `LK-`, khóa `KHOA-`, lớp `LOP-`, giáo viên `GV-`, phòng `PH-`, thành viên `TV-`, học viên `HV-`, họa cụ `HC-`, nhà cung cấp `NCC-`, đăng ký `DK-` và số tăng dần tối thiểu bốn chữ số. Phiên bản khóa mới dùng lại mã khóa gốc và tăng số phiên bản. Mã cũ `MAIN_`, `DEMO_` hoặc mã đã có khác giữ nguyên để bảo toàn dữ liệu liên kết. Mã ưu đãi do người quản trị đặt để người học nhập; mã tra cứu, phiếu nghiệp vụ và mã tài khoản tuân theo luồng nghiệp vụ riêng.
