# LHL ART — Quản lý trung tâm mỹ thuật sáng tạo

Website React, WinForms và ASP.NET Core API dùng chung SQL Server. API là nơi ghi dữ liệu, kiểm tra quyền, học phí, lịch và tồn kho. Website dành cho người đại diện/học viên và giáo viên; WinForms dành cho nhân viên.

## Chạy trên Windows bằng VS Code

Cài .NET 8 SDK, SQL Server Express LocalDB và Node.js 22.12 trở lên. Mở **thư mục gốc repository** trong VS Code (thư mục có `package.json`, `backend` và `LhlArt`).

Cài thư viện web một lần:

```powershell
npm ci
```

Nạp bộ dữ liệu trung tâm một lần:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Seed
```

Lệnh này tạo database **MyThuatCenter** nếu chưa có, rồi nạp dữ liệu. Database đã có bộ này thì giữ nguyên, không tạo trùng và không đặt lại mật khẩu. Database có dữ liệu khác thì lệnh dừng. Database `MyThuatDb` của nhóm được giữ nguyên. Dữ liệu không được nạp tự động khi bật ứng dụng.

Mở ba terminal riêng:

```powershell
# Terminal 1: API, giữ cửa sổ này chạy
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Api
```

```powershell
# Terminal 2: website
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Web
```

```powershell
# Terminal 3: WinForms
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service WinForms
```

- Website: http://127.0.0.1:5173
- API/Swagger: http://127.0.0.1:5080/swagger
- Kiểm tra API: http://127.0.0.1:5080/health
- WinForms mở thành cửa sổ Windows riêng.

Nếu dùng SQL Server khác, truyền cùng `-ConnectionString` khi chạy **Seed** và **Api**, ví dụ:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Seed -ConnectionString 'Server=.\SQLEXPRESS;Database=MyThuatCenter;Trusted_Connection=True;TrustServerCertificate=True'
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Api -ConnectionString 'Server=.\SQLEXPRESS;Database=MyThuatCenter;Trusted_Connection=True;TrustServerCertificate=True'
```

Không chạy API cũ trên cùng cổng 5080 khi mở bản này. Chạy trực tiếp `dotnet run` mà không dùng script vẫn đọc chuỗi kết nối cũ trong `appsettings.json`; vì vậy hãy dùng script chung để tránh hai ứng dụng trỏ nhầm database.

## Tài khoản và mật khẩu bộ dữ liệu mẫu

Các tài khoản dưới đây chỉ được tạo khi chạy **Seed** thành công. Mật khẩu dùng chung: **`LhlArt2026!HocTap`**. Đây là tài khoản công khai cho dữ liệu hư cấu trên máy phát triển; không phải mật khẩu của thành viên nhóm hay tài khoản đang dùng thật. Database chỉ lưu mật khẩu băm. Không đưa bộ tài khoản này lên môi trường phục vụ khách hàng.

| Tài khoản | Mật khẩu | Vai trò / nơi đăng nhập |
|---|---|---|
| `quantri` | `LhlArt2026!HocTap` | Quản trị · WinForms |
| `hocvu` | `LhlArt2026!HocTap` | Học vụ · WinForms |
| `thungan` | `LhlArt2026!HocTap` | Thu ngân · WinForms |
| `quanlykho` | `LhlArt2026!HocTap` | Quản lý kho · WinForms |
| `giaovien01` | `LhlArt2026!HocTap` | Giáo viên · Web và WinForms |
| `giaovien02` | `LhlArt2026!HocTap` | Giáo viên · Web và WinForms |
| `giaovien03` | `LhlArt2026!HocTap` | Giáo viên · Web và WinForms |
| `giaovien04` | `LhlArt2026!HocTap` | Giáo viên · Web và WinForms |
| `giaovien05` | `LhlArt2026!HocTap` | Giáo viên · Web và WinForms |
| `giaovien06` | `LhlArt2026!HocTap` | Giáo viên · Web và WinForms |
| `giaovien07` | `LhlArt2026!HocTap` | Trợ giảng Junior · Web và WinForms |
| `hocvien01` | `LhlArt2026!HocTap` | Học viên · Web, chỉ xem học tập |
| `hocvien02` | `LhlArt2026!HocTap` | Học viên · Web, chỉ xem học tập |
| `hocvien03` | `LhlArt2026!HocTap` | Học viên · Web, chỉ xem học tập |
| `hocvien04` | `LhlArt2026!HocTap` | Học viên · Web, chỉ xem học tập |
| `hocvien05` | `LhlArt2026!HocTap` | Học viên · Web, chỉ xem học tập |
| `hocvien06` | `LhlArt2026!HocTap` | Học viên · Web, chỉ xem học tập |
| `phuhuynh01` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh02` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh03` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh04` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh05` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh06` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh07` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh08` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh09` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh10` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh11` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh12` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh13` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh14` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh15` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh16` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh17` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh18` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh19` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh20` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh21` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh22` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh23` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh24` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh25` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh26` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh27` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh28` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh29` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh30` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh31` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh32` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh33` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh34` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh35` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh36` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh37` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh38` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh39` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh40` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh41` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh42` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh43` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh44` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh45` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh46` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh47` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh48` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh49` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh50` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh51` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh52` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh53` | `LhlArt2026!HocTap` | Người đại diện · Web |
| `phuhuynh54` | `LhlArt2026!HocTap` | Người đại diện · Web |

`phuhuynh01`, `10`, `19`, `28`, `37`, `46` có thêm một học viên đã xác minh, chưa đăng ký để thử ghi danh khóa tương ứng. Ví dụ **phuhuynh01 → Nguyễn Tuệ An → lớp Junior đang tuyển sinh**. `phuhuynh04` có học viên đang học Junior; `hocvien01` xem nhận xét của học viên đầu tiên trong lớp đó. `giaovien01` dạy Junior, `giaovien07` là trợ giảng.

## Dữ liệu trung tâm

| Nhóm | Số lượng / nội dung |
|---|---|
| Chương trình | 2 nhóm, 6 khóa, 88 nội dung buổi |
| Lớp | 18 lớp: 6 đã kết thúc, 6 đang học, 6 tuyển sinh |
| Lịch | 264 buổi, 6 phòng, 7 giáo viên gồm trợ giảng Junior |
| Hồ sơ | 114 học viên; 54 người đại diện và 6 thành viên học viên |
| Đăng ký | 108 phiếu: 96 xác nhận, 6 còn thiếu học phí, 6 hết hạn |
| Thanh toán | 102 khoản thu học phí, 36 khoản thu họa cụ; từng giao dịch gắn đúng khoản thu |
| Học tập | Điểm danh, nhận xét, 36 kết quả cuối khóa, 6 lịch học bù |
| Hỗ trợ | 12 phản hồi: đã trả lời và chờ xử lý |
| Kho | 12 mã họa cụ, 3 nhà cung cấp, nhập kho, xuất tiêu hao và 36 phiếu giao canvas |
| Ưu đãi | NHAPHOC10 giảm 10% học phí, tối đa 400.000 ₫; HOACU15 giảm 15% họa cụ, tối đa 50.000 ₫ |

Lịch được tính theo ngày nạp dữ liệu; ca sáng dành cho lớp đang học, ca chiều dành cho đợt mới để không trùng phòng/giáo viên. Lệnh chạy lại giữ nguyên lịch đã nạp. Thông tin liên hệ dùng số giả và miền `example.invalid`; tên, hồ sơ và giao dịch đều hư cấu. Màn hình không chèn nhãn “demo” vào tên khóa hoặc tên người.

## Kiểm tra đồng bộ

1. Web: đăng nhập `phuhuynh01`, chọn Nguyễn Tuệ An và lớp Junior đang nhận đăng ký, gửi phiếu.
2. WinForms: đăng nhập `thungan`, vào **Đăng ký & học phí**, bấm **Làm mới**. Phiếu vừa gửi từ web có trong danh sách.
3. Chọn phiếu → **Xem công nợ** → **Ghi nhận thu tiền**. Nhập đúng ID khoản thu, số tiền còn thiếu và mã chứng từ riêng. Chỉ ghi nhận khi thực tế đã nhận tiền; trong database mẫu có thể dùng số tiền giả để kiểm thử.
4. Web cập nhật danh sách sau tối đa 30 giây hoặc khi quay lại tab. Phiếu đủ học phí chuyển sang **Đã xác nhận**, có lịch học. Mở **Học phí và thanh toán** → **Cập nhật học phí** để đối chiếu tiền.
5. Gửi phản hồi từ web; nhân viên trả lời trong WinForms; người đại diện bấm **Làm mới** tại khu học tập để xem trả lời.
6. Giáo viên đăng nhập web hoặc WinForms để xem cùng lịch và danh sách học viên. Điểm danh/nhận xét vẫn chịu giới hạn ngày giờ và phân công của API.

Không sao chép dữ liệu giữa web và file JSON. Hai ứng dụng đọc/ghi một API và một database. WinForms tự làm mới màn hình đang xem mỗi 30 giây khi cửa sổ đang hoạt động; không làm mới khi đang mở hộp nhập liệu. Đây là cập nhật định kỳ, chưa dùng SignalR.

## Những phần đã nối

- Đăng nhập và phân quyền chung; học viên/người đại diện không vào WinForms quản lý.
- Khóa/lớp, hồ sơ, xác minh đại diện, đánh giá đầu vào và ghi danh.
- Danh sách đăng ký nhân viên có phân trang và tìm kiếm; công nợ, thu học phí và chống ghi trùng khi retry.
- Lịch giáo viên, điểm danh, nhận xét, kết quả, học bù, đề nghị đổi lịch và phản hồi trên website dùng API đầy đủ từ nhánh Lê Long.
- WinForms thêm/sửa loại khóa, khóa, nội dung buổi, lớp, giáo viên, phòng, họa cụ và nhà cung cấp; xếp lịch theo tuần; duyệt/từ chối yêu cầu hủy/chuyển theo quyền API.
- Tồn kho và chứng từ kho; biểu đồ học phí/hoàn phí/doanh thu họa cụ 6 tháng lấy từ API.

Khóa học tạo ở trạng thái **Bản nháp**, nhập đủ nội dung buổi rồi quản trị mới công bố. Lớp phải có lịch và đủ giáo viên (Junior có trợ giảng) trước khi công bố. Khóa đã dùng không sửa trực tiếp phiên bản; API yêu cầu tạo phiên bản mới. Hồ sơ và chứng từ có lịch sử không xóa cứng.

Các màn hình tạo/duyệt phiếu kho nhiều dòng, hoàn tiền, xếp học bù và quản trị tài khoản đầy đủ chưa được chuyển hết sang WinForms; các API tương ứng vẫn sử dụng qua Swagger. Chưa tích hợp cổng thanh toán ngân hàng hay gửi email/SMS. Bản JSON cũ chỉ mở bằng `--local` để tham khảo và **không đồng bộ** với hệ thống này.

## Cấu trúc

- `src/`: website React/TypeScript; `src/services/artCenterService.ts` gọi API.
- `backend/MyThuat/MyThuat.Api/`: Controllers, Services, Models, Data và hợp đồng API.
- `LhlArt/`: WinForms; `Networking/ArtApiClient.cs` gọi API, `Controllers/ApiCatalogController.cs` xử lý biểu mẫu danh mục, `Views/` hiển thị.
- `backend/MyThuat/MyThuat.Checks/`: kiểm tra HTTP, quyền, học phí, học tập và bộ dữ liệu liên thông.
- `scripts/Start-Center.ps1`: chạy các thành phần trên cùng cấu hình database.

## Kiểm tra mã nguồn

```powershell
npm run build
dotnet build LhlArt/LhlArt.csproj -c Release
powershell -ExecutionPolicy Bypass -File scripts/Start-Center.ps1 -Service Check
powershell -ExecutionPolicy Bypass -File scripts/Check-SqlServer.ps1
```

Bộ kiểm tra dùng database riêng tên ngẫu nhiên, không ghi vào database đang vận hành. Xem [biên bản kiểm tra](docs/INTEGRATION_CHECKS.md) để biết kết quả thực tế và giới hạn của môi trường bàn giao.

## Dùng nhiều máy

Chạy một API trung tâm, cho website và WinForms trỏ vào cùng địa chỉ. WinForms cấu hình `LhlArt/api.config.json`; web development dùng `VITE_API_PROXY_TARGET`, web đã build dùng `VITE_API_BASE_URL`. API cần cấu hình địa chỉ lắng nghe, `AllowedHosts` và `WebOrigins` phù hợp; triển khai ngoài máy cá nhân dùng HTTPS. `127.0.0.1` luôn là chính máy đang chạy, không phải máy chủ của nhóm. Không đưa chuỗi kết nối chứa mật khẩu thật vào Git.

## Nguồn hợp nhất

Bản này phát triển từ nhánh `winf` (`f34cc27`), giữ website hiện có của nhánh `api/main` (`64fcc15`) và khôi phục các API học tập/hồ sơ từ `Lê-Long` (`1b12834`). Toàn bộ ba thành phần đặt trong cùng nhánh tích hợp để tránh chạy frontend mới với backend cũ.
