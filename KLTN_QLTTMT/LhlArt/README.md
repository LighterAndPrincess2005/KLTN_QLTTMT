# LHL ART — WinForms quản lý trung tâm

Bản ngày 22/10/2026: lấy API và website từ `main` tại `2b8237d`, ghép WinForms của nhánh `dong-bo-api-web-winform` tại `e9eb760`, rồi sửa trong thư mục `LhlArt`. Không thay mã nguồn backend, website hoặc schema database của nhóm.

## Mở và chạy trong VS Code

Giải nén cả dự án, mở thư mục có `backend`, `src`, `LhlArt`. Máy chạy WinForms cần Windows và .NET 8 SDK. Tiếp tục dùng API, database và tài khoản đang chạy của nhóm; bản này không nạp lại dữ liệu hoặc đặt lại mật khẩu.

1. Bật **API bản main mới của nhóm** theo `../NOI_API.md`. Nếu API đã chạy thì giữ nguyên.
2. Kiểm tra `http://127.0.0.1:5080/health`.
3. Trong `LhlArt/api.config.json`, đặt `BaseUrl` đúng API mà website đang dùng.
4. Terminal tại thư mục gốc:

```powershell
dotnet run --project LhlArt/LhlArt.csproj -c Release
```

Nếu chỉ mở thư mục `LhlArt`, chạy `dotnet run --project LhlArt.csproj -c Release`, hoặc bấm `CHAY_UNG_DUNG.bat`.

Đăng nhập bằng tài khoản nhân viên hiện có. Quản trị tạo/sửa ưu đãi, quản lý tài khoản; học vụ quản lý hồ sơ và đăng ký lớp; thu ngân tra cứu đăng ký và thu tiền. Người học/phụ huynh đăng nhập website.

Ứng dụng khởi động bằng API; không mở kho JSON cũ khi truyền `--local`. Các lớp xử lý JSON cũ còn lưu trong source để tham khảo, không tham gia luồng chạy chính.

## Các mục mới

- **Đăng ký tài khoản**: tạo tài khoản người học/phụ huynh độc lập. API mới cho đăng nhập ngay, không cần duyệt và chưa tự đăng ký lớp.
- **Người học / phụ huynh**: thêm/sửa hồ sơ người liên hệ. Hồ sơ liên hệ chưa chắc đã có tài khoản đăng nhập; tạo tài khoản tại mục riêng.
- **Hồ sơ học viên**: thêm/sửa thông tin, chọn người đại diện theo họ tên, đánh giá đầu vào. Quan hệ bản thân tự lấy tên, ngày sinh nếu đã có, địa chỉ của chủ hồ sơ; liên hệ khẩn cấp là người khác, cần nhập riêng.
- **Người đại diện**: hiện tên cả người đại diện và học viên; thêm/sửa quan hệ và quyền; gỡ liên kết bằng cách kết thúc hiệu lực.
- **Tài khoản & vai trò**: quản trị xem tài khoản người học/phụ huynh/giám hộ/nhân viên, tạo nhân viên, đổi vai trò/khóa và đặt lại mật khẩu. Loại người học/phụ huynh được suy ra từ quan hệ còn hiệu lực; một người có thể vừa học vừa đại diện cho con. Người chưa liên kết được ghi rõ.
- **Mã giảm giá / ưu đãi**: tạo/sửa mã, chọn khóa, phần trăm/số tiền, mức trần, lượt, ngày áp dụng. Có nút kiểm tra danh sách công khai mà website đang đọc.
- **Đăng ký & học phí**: đăng ký tại trung tâm, tra cứu ID, xem công nợ và thu tiền. Thu tiền chọn khoản thu bằng danh sách; gửi lại cùng nội dung giữ cùng khóa chống trùng.
- Tìm tên có dấu/không dấu, chữ đ/Đ, Unicode tổ hợp; tải đủ trang từ API trước khi lọc. Không còn tìm trên chuỗi JSON thô hoặc chỉ trang hiện tại.
- Ngày sinh từ 01/01/1900 đến hôm nay theo giờ Việt Nam. Bản thân tự đại diện cần đủ 18 tuổi; tuổi tối thiểu của khóa vẫn được API kiểm tra khi ghi danh.
- Các danh mục có thêm/sửa; các mục API cho phép có **Ngừng sử dụng**. Không xóa cứng dữ liệu có lịch sử. Form giữ nội dung khi API từ chối lưu và hiển thị đúng thông báo lỗi.
- Biểu đồ thu/hoàn 6 tháng lấy endpoint `thu-hoan` đang tồn tại trong API mới. Không gọi endpoint biểu đồ của bản cũ.

Các nút thao tác ở phía trên bảng có thể cuộn ngang khi cửa sổ hẹp. Tìm kiếm không thay đổi nội dung đang gõ; ô nhập văn bản hỗ trợ Unicode/IME. Khi dùng UniKey/EVKey trên Windows, dùng bảng mã Unicode.

## Cách tạo ưu đãi 10% để website hiện

Đăng nhập WinForms bằng quản trị → **Mã giảm giá / ưu đãi → Tạo ưu đãi**:

| Trường | Ví dụ |
|---|---|
| Mã ưu đãi | `SACMAU10` |
| Tên chương trình | Ưu đãi khóa Sắc màu 10% |
| Tự áp dụng và hiển thị trên web | Chọn Có |
| Khóa học áp dụng | Chọn đúng khóa đã công bố |
| Áp dụng cho | Học phí |
| Cách giảm | Phần trăm (%) |
| Giá trị giảm | 10 |
| Giới hạn số tiền giảm | Bỏ chọn nếu không có trần |
| Tổng lượt | 100 |
| Bắt đầu / kết thúc | Khoảng thời gian chứa hiện tại |
| Trạng thái | Hoạt động |

Lưu → **Kiểm tra trên web** → tải lại website. API chỉ công khai ưu đãi tự áp dụng, đúng khóa, trong thời hạn và còn lượt. Nếu có nhiều ưu đãi khóa, API chọn mức giảm có lợi nhất.

Nếu muốn mã khách tự nhập, bỏ chọn **Tự áp dụng và hiển thị trên web**. Mã này dùng ở bước đăng ký, không hiện như giảm giá công khai trên thẻ khóa. Ưu đãi đã được sử dụng không được sửa theo API hiện tại; tạo mã mới.

## Kiểm tra thao tác qua lại

1. Tạo tài khoản bằng mục **Đăng ký tài khoản**, dùng tên đăng nhập chưa có. Đăng nhập tài khoản đó trên web: không cần duyệt.
2. WinForms → **Hồ sơ học viên → Thêm**, chọn người vừa tạo, quan hệ, ngày sinh và liên hệ khẩn cấp. Lưu rồi tải lại phần hồ sơ trên web: phải thấy học viên.
3. Sửa tên học viên có dấu trong WinForms → tải lại web: tên phải đổi đúng.
4. WinForms → **Đăng ký & học phí → Đăng ký tại trung tâm**, chọn học viên, người đại diện và lớp. Lưu ID đăng ký được báo sau khi tạo; web của người đại diện phải thấy đăng ký này.
5. Chọn đăng ký → **Ghi nhận thu tiền**, chọn khoản thu, nhập tiền, mã chứng từ riêng và xác nhận thực nhận. Khi đủ tiền, web đọc trạng thái **Đã xác nhận**. Chỉ dùng giao dịch giả trên database kiểm thử.
6. Web tạo đăng ký khác → lấy ID đăng ký từ kết quả API/phiếu → WinForms **Tra cứu ID đăng ký**: xem được cùng công nợ. API `main` hiện chưa có danh sách tổng dành cho nhân viên, nên chưa thể tự liệt kê toàn bộ đăng ký từ web.
7. Kiểm tra ưu đãi theo hướng dẫn ở trên. Web hiện tại cần tải lại trang để đọc bản mới; bản bàn giao này không sửa cơ chế làm mới của web.

## Phần cần nhóm API bổ sung

Xem [YEU_CAU_API.md](YEU_CAU_API.md). Hai phần chưa thể hoàn tất chỉ bằng WinForms:

- API chưa có `GET /api/dang-ky/quan-ly`: màn hình dùng tra cứu ID và các phiếu vừa tạo/tra cứu trong phiên, **không phải toàn bộ đăng ký của trung tâm**.
- API chưa lưu kênh đăng ký online/offline: cột ghi **Chưa được API ghi nhận**. Không suy đoán từ tài khoản, mã đăng ký hoặc khóa idempotency. Sau khi API trả `kenhDangKy`, WinForms sẽ hiển thị tên tương ứng.

API danh mục cũng không có DELETE quản trị. Vì vậy dùng ngừng sử dụng hoặc gỡ liên kết; không có nút xóa cứng giả. Những thao tác nghiệp vụ như phiếu kho nhiều dòng, hoàn tiền, xếp học bù vẫn theo phạm vi bản cũ, chưa bổ sung trong đợt này.

## Build và kiểm tra

Chạy từ thư mục gốc dự án:

```powershell
dotnet build LhlArt/LhlArt.csproj -c Release
dotnet run --project LhlArt/ConnectionChecks -c Release
dotnet run --project LhlArt/IntegrationChecks -c Release
```

Kiểm tra với SQL Server LocalDB trên Windows:

```powershell
dotnet run --project LhlArt/IntegrationChecks -c Release -- --sqlserver
```

Bộ kiểm tra tích hợp tạo database tạm tên ngẫu nhiên rồi xóa chính database đó; không dùng database của nhóm. Không chạy các lệnh reset/seed nếu chỉ muốn cập nhật WinForms. Kết quả thực tế: [KIEM_TRA_BAN_CAP_NHAT.md](KIEM_TRA_BAN_CAP_NHAT.md).

## Tổ chức mã

`Views` hiển thị, `Controllers/Api*Controller` xử lý biểu mẫu và gọi nghiệp vụ, `Services/ApiDataService` tìm kiếm/phân trang, `Services/ApiDisplayService` nối tên và phân loại hiển thị, `Networking/ArtApiClient` truyền HTTP/JSON. Mọi thay đổi dữ liệu đều qua API, không ghi SQL trực tiếp.
