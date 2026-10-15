# LHL ART — Quản lý Trung tâm Mỹ thuật sáng tạo

## Cập nhật kết nối API ngày 09/10/2026

Ứng dụng **mặc định dùng API** tại `http://127.0.0.1:5080/`, dùng chung tài khoản và dữ liệu với website. Không tạo tài khoản quản trị riêng ở WinForms. Chạy `CHAY_UNG_DUNG.bat` hoặc bản đã biên dịch `BanChay_API/LhlArt.exe`.

Xem **KET_NOI_API.md** để biết chức năng đã nối, cách xác minh hồ sơ/đại diện, ghi thu tiền và cấu hình máy khác. Các danh mục quản lý hiện hỗ trợ tra cứu; chưa chuyển toàn bộ biểu mẫu thêm/sửa của bản cũ sang API.

Phần tài liệu bên dưới mô tả **chế độ JSON cũ**. Muốn mở riêng chế độ này: `dotnet run --project LhlArt.csproj -- --local`. Dữ liệu JSON không tự đồng bộ vào SQL Server.

Ứng dụng Windows Forms cho công tác quản lý đào tạo, học viên, học phí và họa cụ. Mã đề tài: CNTT-KLCN082. Mã nguồn tổ chức theo MVC, mở và chạy bằng VS Code trên Windows.

## Mở và chạy

Cần Windows 10/11 và .NET 8 SDK.

1. Giải nén, mở thư mục **LhlArt** trong VS Code.
2. Chọn Terminal → New Terminal.
3. Chạy:

```powershell
dotnet build LhlArt.csproj
dotnet run --project LhlArt.csproj
```

Có thể dùng **CHAY_UNG_DUNG.bat** hoặc **Ctrl+Shift+B**. Để gỡ lỗi, cài extension C# của Microsoft rồi nhấn **F5**. WinForms mở thành cửa sổ Windows riêng.

## Tài khoản và dữ liệu ban đầu

- Nếu máy đã có dữ liệu ở bản trước, dùng tài khoản và mật khẩu hiện tại.
- Nếu cài mới, chương trình yêu cầu tạo tài khoản quản trị. Bạn tự đặt tên đăng nhập và mật khẩu ít nhất 8 ký tự.
- Không tạo sẵn tài khoản dùng chung, học viên, lớp, chứng từ hoặc số liệu thống kê.
- Nhập dữ liệu theo thứ tự phù hợp: loại khóa → khóa học; giáo viên và phòng → lớp học; học viên → đăng ký và thanh toán. Dữ liệu họa cụ, nhập kho và ưu đãi được tạo từ các mục tương ứng.

## Giao diện

- Menu trái có icon nét mảnh, trạng thái đang chọn, hiệu ứng rê chuột và nhóm mở/thu gọn.
- Tông xanh trầm, nền kem, điểm nhấn vàng đất; dùng logo LHL ART do nhóm cung cấp.
- Trang tổng quan gồm số liệu, lịch sắp tới và việc cần xử lý. Tiền học phí và họa cụ là số lũy kế, không phải doanh thu riêng của ngày hiện tại.
- Hai dòng trong mỗi mục lịch được bố trí riêng, có khoảng cách cố định; nội dung dài được rút bằng dấu ba chấm để không chồng lên giờ học.
- Tiền có đơn vị ₫ phía sau; cột số căn phải; điểm và tỷ lệ giữ định dạng riêng.
- Logo và icon nằm trong ứng dụng, không phụ thuộc font icon hay tải ảnh qua Internet.

## Tài khoản nhân viên

Vào **Quản trị → Tài khoản nhân viên** để tạo tài khoản, chọn một trong 5 vai trò, khóa/mở hoặc đặt lại mật khẩu. Tạo hồ sơ giáo viên trước khi cấp tài khoản Giáo viên và chọn hồ sơ liên kết. Dùng tên đăng nhập đã tạo; không có mật khẩu dùng chung. Không hiển thị mật khẩu băm trên bảng.

Xem **DOI_CHIEU_YEU_CAU.md** để biết chính xác chức năng đã có, giới hạn và các phần còn thiếu theo đề cương.

## Các nhóm chức năng

| Nhóm | Nội dung |
|---|---|
| Đào tạo | Loại khóa, khóa học, lớp và phòng học |
| Hồ sơ | Học viên, tư vấn và giáo viên |
| Đăng ký & học phí | Đăng ký, thu học phí, hủy và chuyển lớp |
| Theo dõi học tập | Lịch học, điểm danh và kết quả |
| Họa cụ & kho | Danh mục, nhập/xuất, phiếu bán |
| Ưu đãi | Ưu đãi học phí và họa cụ |
| Báo cáo | Giao dịch học phí, tổng hợp lớp, doanh thu họa cụ, biểu đồ 6 tháng |
| Quản trị | Tài khoản nhân viên, nhật ký và sao lưu dữ liệu |
| Tài khoản | Đổi mật khẩu, đăng xuất |

Mục hiển thị theo vai trò; quyền vẫn được kiểm tra ở tầng xử lý.

## Thời gian và dữ liệu đang có

Ứng dụng sử dụng giờ Việt Nam (UTC+7). Đăng ký quá hạn được cập nhật khi mở hoặc làm mới các màn hình dữ liệu. Việc thu, hoàn, điểm danh, lịch và ưu đãi được kiểm tra theo thời gian hiện tại; không còn mục thay đổi đồng hồ.

Dữ liệu cài mới lưu tại `%LOCALAPPDATA%\LhlArt\data.json`. Nếu chỉ có dữ liệu từ bản trước, ứng dụng tiếp tục đọc đúng tệp cũ để giữ hồ sơ và mật khẩu. Không tự xóa hồ sơ, không thêm ưu đãi và không thay giá bán trong dữ liệu cũ. Những hồ sơ đã có từ trước vẫn xuất hiện, kể cả hồ sơ dùng để thử trước đây; hãy đối chiếu trước khi sử dụng số liệu.

Sao lưu tại **Quản trị → Sao lưu dữ liệu**. Bản ngay trước lần lưu gần nhất nằm trong `data.json.bak`. Chỉ khôi phục dữ liệu khi đã đóng ứng dụng. Hai phiên bản dùng chung cơ chế khóa một cửa sổ để tránh ghi dữ liệu đồng thời.

## Cấu trúc MVC

```text
LhlArt.csproj               Cấu hình ứng dụng Windows Forms
Program.cs                  Khởi tạo và vòng đăng nhập
Models/                     Đối tượng nghiệp vụ và dữ liệu tổng quan
Views/                      Các màn hình và biểu mẫu
Views/Shared/               Màu sắc, icon, menu và nút dùng chung
Controllers/                Nhận thao tác, chuẩn bị dữ liệu, cập nhật View
Services/                   Quy tắc nghiệp vụ, phân quyền và thời gian
Data/                       Đọc, ghi và sao lưu JSON
Assets/                     Logo nhóm
Tests/                      Dữ liệu kiểm thử và kiểm tra nghiệp vụ
```

View → Controller → Service/Model → cập nhật View. Dữ liệu kiểm thử nằm riêng trong `Tests/`, được loại khỏi bản ứng dụng khi biên dịch.

## Kiểm tra

Mã giao diện đã biên dịch bằng Roslyn của .NET SDK 8.0.408 cùng thư viện tham chiếu Windows Forms 8.0.15. Có 51 kiểm tra nghiệp vụ đạt, gồm thu/hoàn, đăng ký, chuyển lớp, tồn kho, phân quyền, bảo toàn dữ liệu cũ, cài mới trống, khởi tạo tài khoản và giờ Việt Nam. Chạy `KIEM_TRA.bat` hoặc:

```powershell
dotnet run --project Tests/SmokeTests.csproj
```

Kiểm tra dùng thư mục tạm, không sửa dữ liệu đang sử dụng. Môi trường bàn giao không mở được cửa sổ Windows; chưa xác minh trực quan giao diện trên Windows và các mức DPI thực tế. Phần biên dịch dùng Roslyn trực tiếp vì MSBuild không lấy được thông tin tiến trình trong môi trường kiểm tra.

## Phạm vi hiện tại

Ứng dụng vẫn lưu JSON trên một máy; chưa tích hợp API, SQL Server hoặc đồng bộ Website. Việc bỏ nội dung giới thiệu bản thử không thay thế các phần này.

Các giới hạn nghiệp vụ đang giữ từ phiên bản trước: chuyển lớp chỉ trước khai giảng; chưa xử lý chuyển có hoàn chênh lệch hoặc chuyển tiếp; chưa có học bù, gửi thông báo, mua hàng/kiểm kê đầy đủ và thanh toán tự động. Các thông báo từ chối phản ánh đúng phạm vi đã cài đặt.

## Tạo bản chạy Windows x64

Dùng `TAO_EXE.bat` hoặc:

```powershell
dotnet publish LhlArt.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o BanChay
```

Bản chạy nằm trong thư mục `BanChay`. Dữ liệu nghiệp vụ vẫn lưu riêng trên máy người dùng.
