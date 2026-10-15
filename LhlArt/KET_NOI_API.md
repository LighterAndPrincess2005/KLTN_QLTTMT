# WinForms LHL Art dùng chung API với web

## Tương thích API trong KLTN_QLTTMT (15/10/2026)

Giao diện đã đồng bộ màu web: nền kem #FCFAF5, xanh #1F6B5B và cam san hô #EF7B67. Đăng nhập có ô nhập bo góc, biểu tượng và viền khi focus; trang chủ có thống kê lấy từ API theo quyền đăng nhập. Menu bên trái đánh dấu mục đang chọn. Thanh tìm kiếm, nút thao tác, bảng dữ liệu và hộp nhập liệu dùng chung phong cách. Cửa sổ nhỏ có thanh cuộn để tiếp cận đầy đủ nội dung.

- Lịch giáo viên dùng `GET /api/lich-hoc`. API tự giới hạn kết quả theo giáo viên đăng nhập; WinForms không gọi đường dẫn `api/hoc-tap/giao-vien/buoi-cua-toi` của bản API mới hơn.
- Chọn một buổi để đọc `GET /api/hoc-tap/buoi/{id}/danh-sach`. Điều kiện ngày, giờ và quyền ghi điểm danh vẫn do API kiểm tra.
- API trong `KLTN_QLTTMT/backend/MyThuat` chưa có `GET /api/dang-ky/quan-ly`. Mục Đăng ký & học phí hiển thị thông báo chưa hỗ trợ và không cho ghi thu tiền từ danh sách thiếu này. Khi API hỗ trợ đường dẫn đó, mục này hoạt động như trước.
- Kiểm tra kết nối thật: đặt `LHL_CHECK_USER` và `LHL_CHECK_PASSWORD` trong môi trường của terminal, rồi chạy `dotnet run --project ConnectionChecks -- --live-schedule`. Kiểm tra đăng nhập, lịch và danh sách điểm danh, không ghi điểm danh hay giao dịch tiền.

## Mở ứng dụng

1. Bật API Mỹ Thuật như trước. API phải trả kết quả ở `http://127.0.0.1:5080/health`.
2. Mở `BanChay_API/LhlArt.exe` hoặc chạy `CHAY_UNG_DUNG.bat` trong thư mục LhlArt.
3. Đăng nhập bằng **tài khoản nhân viên trên API**: quản trị, học vụ, thu ngân, giáo viên hoặc quản lý kho. Tài khoản phụ huynh/học viên dùng website.

Địa chỉ server nằm ở `api.config.json` cạnh file chạy. Token chỉ giữ trong bộ nhớ, không lưu mật khẩu/token ra file. Khi phiên hết hạn, ứng dụng quay về đăng nhập.

## Đã nối API

- Đăng nhập, phân quyền menu và đăng xuất.
- Tra cứu thành viên/phụ huynh, học viên, quan hệ đại diện; có phân trang 100 dòng, tìm kiếm trong trang và làm mới.
- Xác minh trạng thái thành viên/học viên; xác minh quan hệ và quyền đại diện; ghi đánh giá đầu vào với giáo viên liên kết.
- Xem đăng ký của web, công nợ từng khoản và ghi nhận **thu tiền thực tế** bằng tài khoản có quyền thu tiền. API quyết định xác nhận vào lớp khi đủ các điều kiện; không có nút ép xác nhận bỏ qua học phí.
- Xem phản hồi/đề nghị từ web và gửi câu trả lời. Trả lời đề nghị không tự đổi lịch hoặc xếp buổi bù.
- Xem lịch được phân quyền; xem danh sách điểm danh, ghi có mặt/vắng, số phút, điểm sản phẩm và nhận xét. Giáo viên chỉ ghi từ khi buổi bắt đầu trong ngày học; học vụ có quyền đào tạo chỉnh muộn. Server kiểm tra lại quyền và thời gian.
- Tra cứu khóa/lớp/giáo viên/phòng, họa cụ/nhà cung cấp, tài khoản, tổng quan và nhật ký theo quyền.

## Xác minh hồ sơ mới từ web

1. **Thành viên / phụ huynh** → chọn người → Xác minh hồ sơ, sau khi kiểm tra thông tin thực tế.
2. **Học viên từ web** → chọn học viên → Xác minh hồ sơ.
3. **Quyền đại diện** → đối chiếu ID thành viên và học viên → Xác minh quyền đại diện; chọn đúng quan hệ/quyền. Tài khoản của con chỉ được xem kết quả nếu được cấp quyền này.
4. **Học viên từ web** → Đánh giá đầu vào → chọn giáo viên, cấp độ và nhận xét sau đánh giá thực tế.
5. Người đại diện tải lại website để đăng ký lớp. Sau khi nhận đủ tiền hợp lệ, API xác nhận đăng ký theo quy định.

Các cập nhật hồ sơ gửi RowVersion để chặn ghi đè khi người khác vừa sửa. Gặp thông báo dữ liệu đã thay đổi thì làm mới, kiểm tra lại trước khi gửi.

## Phần còn lại

Chưa chuyển hết màn hình thêm/sửa danh mục, nghiệp vụ xếp lịch, xếp học bù, duyệt hủy/chuyển, phiếu kho, tài khoản nhân viên và biểu đồ của bản JSON sang chế độ API. Hiện các danh mục này chủ yếu tra cứu. Không dùng chế độ JSON để duyệt hồ sơ từ web.

File JSON cũ được giữ nguyên. Để mở riêng: `dotnet run --project LhlArt.csproj -- --local`. Bản mã cũ trước khi đổi khởi động được lưu trong `Backup_truoc_ket_noi_API`.

## Máy khác

Đổi `BaseUrl` trong `api.config.json` cạnh bản chạy sang địa chỉ máy/server chạy API. `127.0.0.1` chỉ dùng trên chính máy này. Máy chủ hiện đang nghe loopback, vì vậy chỉ đổi địa chỉ phía WinForms chưa đủ để máy khác truy cập; cần cấu hình API nghe mạng và cho phép kết nối tại máy chủ. Bản chạy cần .NET 8 Desktop Runtime trên Windows.

## Kiểm tra đã thực hiện

- WinForms build không có lỗi/cảnh báo; mở cửa sổ thật, đăng nhập tài khoản giáo viên API, đọc 6 buổi và danh sách 3 học viên từ database chung.
- 8 kiểm tra kết nối: Bearer, quyền thu tiền, dữ liệu phản hồi, RowVersion, lỗi 409, phiên hết hạn, đăng nhập lại và chặn phụ huynh vào quản lý.
- 158 tình huống HTTP API đạt trên **database SQL Server LocalDB tạm**; có kiểm tra quyền và phân trang của endpoint danh sách đăng ký mới.
- 51 kiểm tra nghiệp vụ JSON cũ vẫn đạt.
- Không tạo giao dịch thu tiền hoặc xác minh hồ sơ thật chỉ để kiểm thử. Các thao tác ghi trong màn hình quản lý cần người có tài khoản nhân viên kiểm tra hồ sơ thực tế.
