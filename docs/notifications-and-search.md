# Tìm kiếm, bố cục và thông báo

Ô tìm kiếm nằm ngay dưới thanh điều hướng. Gõ có dấu hoặc không dấu, dùng mũi tên để chọn rồi Enter, hoặc bấm gợi ý. Kết quả gồm mục trên trang, khóa học, lớp đang công bố và bài viết. Ví dụ `tư`, `tu van`, `chức tư` đều dẫn đến Góc tư vấn ở trang chủ. Kết quả không chứa hồ sơ cá nhân.

Đã kiểm tra bố cục 12 đường dẫn trên màn hình 1440, 900 và 390 px, các nhóm thẻ, trường nhập, nút và khoảng cách sau khung chuẩn bị học. Kiểm tra này không thay thế kiểm thử thủ công mọi tình huống dữ liệu.

## Thông báo nghiệp vụ

- Khi nhân viên trả lời tư vấn từ web hoặc WinForms, API ghi câu trả lời và thông báo trong cùng lần lưu. Email đến địa chỉ đã lưu trên yêu cầu tư vấn. Thành viên liên kết yêu cầu cũng nhận thông báo trên web; khách chưa đăng nhập vẫn nhận email.
- Khi dời buổi học qua API, thông báo ghi lịch cũ, mới, phòng và lý do. Người nhận là thành viên đang hoạt động có quan hệ đại diện đã xác nhận, còn hiệu lực, với học viên đã xác nhận đăng ký tham gia buổi đó. Thông báo được ghi cùng giao dịch đổi lịch; không gửi cho phiếu chờ thanh toán.
- Thông báo web nằm dưới thanh tìm kiếm sau đăng nhập, có thể đánh dấu đã đọc. Chỉ thành viên nhận mới được xem nội dung của mình.
- Quản trị/học vụ có mục **Theo dõi thông báo & liên hệ**. Trạng thái email và ghi nhận cuộc gọi tách riêng. `Gọi` mở ứng dụng gọi điện trên thiết bị; `Ghi nhận đã gọi` chỉ bấm sau khi nhân viên thực sự gọi. Chưa tích hợp cuộc gọi tự động hoặc SMS.

## Cấu hình và cập nhật database

Sử dụng cấu hình Gmail cục bộ `RecoveryMail` sẵn có, không đưa mật khẩu ứng dụng vào Git. Từ thư mục API, chạy bản mới với `--upgrade-notifications`. Lệnh sao lưu COPY_ONLY, xác minh bản sao rồi chạy `Database/010_ThongBao.sql` để thêm bảng và chỉ mục. Không sửa/xóa dữ liệu hiện có. Database tạo mới bằng EF có sẵn bảng.

Hàng đợi email lưu ở `ThongBao`; worker chạy mỗi 30 giây. Sau khi lỗi sẽ đợi 5 phút rồi thử lại, tối đa 3 lượt thông thường; lỗi vẫn giữ thông báo trên web. Nhân viên có thể thử gửi lại sau trạng thái `GUI_LOI`. Email mẫu có đuôi `.invalid` hoặc trống được đánh dấu không có email dùng được. Hệ thống không gửi thư chào mừng hoặc xác minh khi tạo tài khoản.

`DA_GUI` nghĩa là SMTP đã chấp nhận thư, không bảo đảm người nhận đã đọc hoặc thư vào Inbox. Mã trạng thái `DANG_GUI` có lease 5 phút để phục hồi nếu API bị tắt giữa lúc gửi. SMTP không có tính chất exactly-once: nếu máy dừng ngay sau SMTP chấp nhận và trước khi ghi trạng thái, có thể có thư lặp khi phục hồi. Nhật ký không ghi nội dung, email người nhận hoặc mật khẩu.

## Kiểm thử

`dotnet run --project LhlArt/IntegrationChecks -c Release -- --sqlserver` dùng database tạm, mail giả lập. Kiểm tra phân quyền đọc, không tạo trùng khi phiên bản cũ bị từ chối, lưu đánh dấu đọc/cuộc gọi, gửi lỗi và thử lại, đổi lịch tạo thông báo đúng người nhận. Không gửi email thật trong kiểm thử này.
