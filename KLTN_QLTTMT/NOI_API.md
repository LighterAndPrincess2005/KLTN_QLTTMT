# Web LHL Art đã nối API

Web chính nằm ở `D:\long2026\KLTN_QLTTMT`; API đang sử dụng ở `D:\long2026\MyThuat`. Trong repo, bản mã nguồn backend được đặt riêng tại `backend/MyThuat`; phần web vẫn giữ vị trí hiện tại để tiếp tục mở package.json như trước.

## Mở web lần sau

1. Bấm nút **Bat_API_MyThuat** trên Desktop để bật API.
2. Mở thư mục web, chạy `run-dev.cmd`. Hoặc chạy `pnpm run dev` khi đã cấu hình Node và pnpm.
3. Mở `http://127.0.0.1:5173`. Vite tự chuyển các yêu cầu `/api` tới API cổng 5080, không cần sửa CORS khi chạy theo cách này.
4. Nếu chưa có quản trị, thiết lập lần đầu tại `http://localhost:5080`. Trung tâm cần kích hoạt quy định, tạo khóa/lịch và công bố lớp trước khi nhận đăng ký.

Database API đã có dữ liệu mẫu dựa trên bảng khóa và lớp trong CNTT-KLCN082_Tuan4.docx: 6 khóa, 6 lớp, 88 buổi học, 4 giáo viên, 2 phòng, 3 học viên. Web gọi API để đọc dữ liệu này, không dùng danh sách mẫu hardcode. Phần giới thiệu, phương pháp, góc sáng tạo, FAQ và liên hệ đều xem được khi chưa đăng nhập.

| Khóa | Buổi | Phút/buổi | Học phí |
| --- | ---: | ---: | ---: |
| Junior | 16 | 90 | 3.200.000 đ |
| Foundation 1 | 16 | 120 | 3.600.000 đ |
| Pre-Basic 1 | 16 | 120 | 3.800.000 đ |
| Basic 1 | 16 | 120 | 4.000.000 đ |
| Intermediate 1 | 16 | 120 | 4.400.000 đ |
| Acrylic nhập môn | 8 | 120 | 2.400.000 đ |

Các lớp mẫu khai giảng 17–18/10/2026, lịch từng tuần, mỗi tiết 30 phút. Khóa/lớp có mã DEMO_ để nhận diện. Hồ sơ người dùng và giáo viên là hư cấu, không phải dữ liệu khách hàng thật. Tài khoản thử được lưu riêng tại `D:\long2026\MyThuat\MyThuat.Api\.local\Tai_khoan_mau.txt`; không đưa mật khẩu vào source/ZIP.

Trên máy mới, tạo quản trị trước rồi chạy `dotnet run --project MyThuat.Api -c Release -- --seed-demo` từ thư mục MyThuat. Lệnh seed chạy trong giao dịch và chạy lại không thêm trùng. Thao tác này chỉ chạy khi gọi rõ --seed-demo, không tự thêm mỗi lần bật API. Cần có quy định đang hiệu lực để nhận đăng ký; seed không tự thay đổi quy định của quản trị.

Ảnh Web_API_co_du_lieu.png và Web_API_mobile.png được chụp từ API/database hiện tại, không dùng mock API.

## Đã nối

- Danh mục khóa/lớp từ SQL qua API, có phân trang đầy đủ, học phí và số chỗ đang giữ thực tế; không có tuổi tối đa.
- Đăng ký tài khoản, đăng nhập, đăng xuất và xử lý token hết hiệu lực. Phiên lưu trong sessionStorage của tab, không lưu mật khẩu.
- Hồ sơ học viên của người đại diện, thêm hồ sơ mới và trạng thái chờ xác minh.
- Lập phiếu đăng ký với khóa idempotency cho retry, xử lý lỗi quyền/trình độ/trùng lịch/chỗ trống do server quyết định.
- Phiếu theo quyền đại diện, tiến độ, điểm danh, báo nghỉ, phản hồi và gửi đề nghị hủy.
- Website không tự duyệt học viên, giao dịch tiền, yêu cầu hủy hoặc kết quả cuối khóa.

Hồ sơ mới cần học vụ xác minh thành viên/đại diện, kích hoạt học viên và ghi nhận đánh giá đầu vào. Các thao tác nhân viên hiện sử dụng API/Swagger; giao diện WinForms là phần riêng. Chưa có giao diện đặt họa cụ bán riêng, đặt học bù, yêu cầu chuyển lớp hoặc thanh toán trực tuyến.

## Giao diện

Thống nhất font **Be Vietnam Pro**, giữ màu kem/xanh/cam của studio. Tăng điểm nhấn ở tiêu đề, CTA, thẻ khóa/lớp; thêm minh họa SVG nội bộ. Có giao diện di động, trạng thái tải/trống/lỗi, đóng hộp thoại bằng Escape, quản lý focus và hỗ trợ giảm chuyển động. Font được tải từ Google Fonts; mất mạng sử dụng sans-serif hệ thống.

## Cấu hình triển khai

`.env.example` có `VITE_API_BASE_URL`. Khi build để triển khai, đặt URL API HTTPS và khai báo origin website trong `WebOrigins` của API, hoặc cấu hình reverse proxy `/api`. Vite proxy chỉ áp dụng server development; không mở dist/index.html qua file:// để gọi API.

Build: `pnpm run build`. Bộ kiểm tra backend: `dotnet run --project MyThuat.Checks -c Release -- --sqlserver` từ thư mục MyThuat. Backend nguồn trong repo cần restore trước ở máy mới.

## Kiểm tra ngày 02/10/2026

- TypeScript và build Vite đạt; có cảnh báo thư viện lucide về chỉ thị use client, không chặn build.
- 96 lượt kiểm tra HTTP đạt trên SQL Server LocalDB tạm; gồm danh mục/chỗ trống, phiếu của mình, chặn tài khoản giáo viên và không lộ phiếu của thành viên khác.
- Trình duyệt Edge: danh mục thật, đăng nhập lỗi, mẫu đăng ký tài khoản, Escape, font thống nhất và không tràn ở màn hình 390px.
- Luồng browser + SQL kiểm thử: đăng nhập thành viên → chọn học viên → chọn lớp → lập phiếu → xem lịch sử/tiến độ; lỗi trùng lịch được hiển thị, không có lỗi JavaScript.

Chưa kiểm thử tải đồng thời, nghiệm thu tất cả quy định hay triển khai Internet. Mã nguồn web và API được lưu chung repo, API nằm riêng trong backend/MyThuat. Database SQL và tài khoản thật không được đưa lên GitHub; máy mới cần cấu hình database và có thể chạy --seed-demo để thêm bộ mẫu.

## Codex trong VS Code

Log của extension ghi renderer_ready_timeout trong lúc VS Code đang cập nhật Codex. Bấm Ctrl+Shift+P → Developer: Reload Window để nạp phiên bản mới. Bạn đã xác nhận Codex mở lại được.


## Khám phá khóa học

Khách chưa đăng nhập có thể bấm Khám phá chương trình để xem mục tiêu, chất liệu, độ tuổi, thời lượng, học phí, nội dung từng buổi và lớp sắp khai giảng. Nội dung buổi lấy từ API /api/khoa-hoc/{id}/noi-dung. Ảnh dùng lại bộ ảnh của website trước; khu chương trình có khối giới thiệu và nút hành động nổi bật.
