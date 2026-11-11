# Website Mỹ Thuật Bụi và API dùng chung

Website React nằm tại thư mục gốc dự án. API ASP.NET Core nằm tại `backend/MyThuat/MyThuat.Api/`. WinForms trong `LhlArt/` và website cùng truy cập SQL Server thông qua API.

## Cài đặt và khởi động

Thực hiện theo [README chính](README.md) để cài thư viện, tạo database và chạy ba thành phần. Dùng các đường dẫn tương đối từ thư mục vừa giải nén.

| Thành phần | Địa chỉ khi chạy trên cùng máy |
| --- | --- |
| Website | http://127.0.0.1:5173/ |
| API và thiết lập quản trị | http://127.0.0.1:5080/ |
| Swagger trong môi trường Development | http://127.0.0.1:5080/swagger |
| Kiểm tra API | http://127.0.0.1:5080/health |

Vite chuyển yêu cầu `/api` từ website tới cổng 5080. WinForms lấy địa chỉ API từ `LhlArt/api.config.json`.

## Các luồng đã tích hợp

- Tra cứu khóa học, nội dung từng buổi, học phí, ưu đãi và lớp tuyển sinh.
- Đăng ký tài khoản có email, đăng nhập, đổi thông tin và khôi phục mật khẩu bằng mã gửi qua email.
- Tạo hồ sơ học viên, xác lập quyền đại diện, chọn lớp và lập phiếu giữ chỗ.
- Hiển thị QR chuyển khoản, báo đã chuyển khoản, đối chiếu tại quầy và nhận kết quả thanh toán từ webhook SePay.
- Xem lịch học sau khi được xác nhận vào lớp, tiến độ, điểm danh, học bù và kết quả khóa học.
- Gửi tư vấn, nhận thông báo và gửi đánh giá khóa học kèm ảnh. Đánh giá được quản trị duyệt trước khi công khai.
- Giáo viên xem lịch được phân công, điểm danh, gửi thông báo lớp, xếp lịch bù và cập nhật ảnh đại diện.

Email cần cấu hình dịch vụ gửi thư trên máy API. Thanh toán tự động cần tài khoản nhận, khóa webhook và URL HTTPS truy cập được từ SePay. Xem [khôi phục mật khẩu](docs/forgot-password.md) và [thanh toán](docs/sepay.md).

## Dữ liệu và triển khai

Mỗi máy dùng LocalDB có database riêng. Để tái hiện đúng bản demo, chuyển bản sao lưu SQL Server và các ảnh đã tải lên theo [hướng dẫn đồng bộ](HUONG_DAN_DONG_BO_MAIN.md). Các lệnh seed tạo bộ mẫu theo mã nguồn.

Khi triển khai, cấu hình URL API bằng `VITE_API_BASE_URL` trước khi build, hoặc dùng reverse proxy cho `/api`. Khai báo origin website trong `WebOrigins` của API khi chạy khác origin. Web cần máy chủ HTTP hỗ trợ điều hướng các trang React về `index.html`.
