# Cập nhật theo reportweb.docx ngày 17 tháng 10 năm 2026

## Các trang riêng

Lịch tuần tổng hợp là màn hình mặc định trong `/hoc-tap`. Mỗi học viên có lịch riêng gộp từ tất cả lượt học đã xác nhận. Bảng bảy ngày chia sáng/chiều/tối; các lớp có màu riêng, buổi học bù có nhãn. Có điều hướng tuần, chọn ngày, lọc lớp/loại buổi, dạng danh sách và hộp chi tiết buổi học. Trên màn hình nhỏ mặc định là danh sách; bảng tuần vẫn có thể cuộn ngang. Nhãn trùng giờ dựa vào giờ bắt đầu/kết thúc thực tế, không dựa vào chuỗi lịch dự kiến. Buổi hủy và phiếu chưa thanh toán không đưa vào lịch. Nếu một nguồn dữ liệu lỗi, hiển thị cảnh báo rằng lịch chưa đầy đủ và cho thử lại.

API `/api/hoc-tap/ho-so/:id/tien-do` bổ sung mã đăng ký, lớp thực tế, giờ kết thúc, thứ tự buổi, chủ đề, phòng và giáo viên phụ trách theo ngày phân công. Cần khởi động lại API để phiên chạy nhận các trường bổ sung. Không tự dựng các buổi học từ chuỗi lịch tuần hoặc bịa giáo viên/phòng khi chưa có dữ liệu.

Trang `/hoc-tap` được thiết kế thành dashboard riêng, không gắn vào trang chủ. Menu Học tập chuyển thẳng đến trang này; việc chọn học viên và lớp diễn ra trong cột bên trái. Đăng nhập thông thường cũng chuyển đến dashboard, thay vì mở popup chọn học viên. Tám tab gồm Tổng quan, Lịch và điểm danh, Nhận xét, Bài thực hành, Kết quả, Học bù, Đổi lịch/lớp và Phản hồi. Tổng quan lấy tiến độ và buổi tiếp theo từ dữ liệu API; không tự tạo nhận xét, điểm hoặc chứng nhận. Phiếu chờ thanh toán luôn có mục theo dõi, kể cả khi đã có lớp xác nhận. Chọn học viên cập nhật tham số `hoc-vien` để F5 vẫn giữ lựa chọn. Chưa đăng nhập chỉ hiện lời mời đăng nhập trong trang, không bật popup tự động.

- `/gioi-thieu`: giới thiệu, lịch sử, giảng viên và dấu mốc.
- `/khoa-hoc`: danh sách khóa, học phí, ưu đãi và phân trang.
- `/khoa-hoc/:id`: nội dung từng buổi, thời lượng, điều kiện, hình tham khảo theo chất liệu và lớp của khóa.
- `/lop-hoc`: tìm lớp theo bộ lọc.
- `/phuong-phap`: cách dạy và hình ảnh thực hành.
- `/goc-sang-tao`: hình ảnh và cảm nhận học viên.
- `/hoc-tap`, `/giao-vien`: các khu vực riêng cho người dùng tương ứng.

Menu sử dụng đường dẫn thật, tải trang riêng. Trạng thái đăng nhập được khôi phục và không tự mở popup khi tải lại. Logo luôn về trang chủ. Tham số `demo=1` được giữ khi điều hướng để thử giao diện; lựa chọn học viên được truyền bằng `hoc-vien` và vẫn đối chiếu với hồ sơ tài khoản.

Khi triển khai lên hosting, cấu hình fallback các đường dẫn giao diện không khớp tệp về `index.html`; `/api` vẫn chuyển đến backend. Vite dev và Vite preview đã hỗ trợ fallback.

Lịch sử năm 2014, số cơ sở, cộng đồng và danh sách giảng viên tham khảo từ https://mythuatbui.edu.vn/ve-bui. Không sử dụng con số “14+ năm” trên trang nguồn vì không nhất quán với mốc 2014. Ảnh lịch sử: `/uploads/2026/09/hanh-trinh-bui-di-qua.webp`. Ảnh chân dung ba giảng viên được lưu trong `public/bui/teacher-duong.jpg`, `teacher-kiet.jpg`, `teacher-thanh.jpg` từ các ảnh có tên tương ứng trên trang Về Bụi.

Thành phẩm trang khóa học hiện là hình tham khảo theo chất liệu, không khẳng định thuộc một lớp cụ thể khi chưa có dữ liệu sản phẩm công khai liên kết theo khóa từ API.

Nguồn yêu cầu: `D:\long2026\reportweb.docx`, gồm nội dung góp ý và 11 ảnh chụp.

## Giao diện

- Đổi nhận diện sang Mỹ Thuật Bụi, logo chính thức ở header, footer, favicon và các nhãn.
- Tiêu đề “Chạm vào màu sắc.” nằm trên một dòng; co chữ theo chiều rộng vùng nội dung.
- Nút “Chọn lớp của bạn” có nền cam, chữ trắng.
- Nội dung từng buổi ghi số tiết và số phút từ thời lượng tiết của API.
- Chương trình, khóa công bố và lịch lớp đều phân trang 6 mục. Bộ lọc chương trình dùng “Chính khóa” thay “Core”.
- Lịch lớp lọc theo ngày học, tình trạng/chỗ trống, khóa học, tháng khai giảng; có tùy chọn xem lớp sắp khai giảng và chưa mở đăng ký.
- Ưu đãi thật lấy từ `GET /api/khuyen-mai`; điều kiện và số tiền giảm cuối cùng vẫn được API kiểm tra khi đăng ký. Dữ liệu trình diễn có banner và mã BUIMOI10, không phải ưu đãi được xác minh của Bụi.

## Hồ sơ và trình diễn

- Người đại diện được tự điền tên và điện thoại liên hệ từ tài khoản. API `/api/tai-khoan/toi` bổ sung các trường này.
- Chọn bản thân tự điền tên, ngày sinh đã nhập khi tạo tài khoản trên trình duyệt, tên liên hệ; điện thoại khẩn cấp để trống. Nếu tài khoản cũ chưa có ngày sinh thì người dùng bổ sung, không tự suy đoán.
- Phiên bản API hiện có vẫn xác minh hồ sơ và thanh toán. Để chạy thử xuyên suốt ngay, mở `http://127.0.0.1:5173/?demo=1` hoặc bấm “Dùng thử không cần duyệt” trên thanh đầu trang khi chạy phát triển.
- Chế độ trình diễn lưu hồ sơ và đăng ký trong sessionStorage, xác nhận vào lớp ngay, không ghi nhận giao dịch tiền thật. Chỉ hoạt động trong Vite development; build production dùng API.
- Trong trình diễn có thể đăng nhập bằng tên và mật khẩu bất kỳ không rỗng; tạo tài khoản vẫn dùng quy tắc biểu mẫu.
- Muốn dùng các trường hồ sơ và danh sách ưu đãi mới với dữ liệu thật cần khởi động lại API sau khi build.

## Nguồn hình ảnh

Các ảnh sau tải từ https://mythuatbui.edu.vn/ vào `public/bui/` để giao diện không phụ thuộc đường dẫn ảnh từ xa:

| Tệp | Đường dẫn nguồn trên website Bụi |
| --- | --- |
| logo.png | /icon-logo.png |
| children.webp | /uploads/2026/09/hoc-vien-bui-nho-tai-lop-hoc-ve-my-thuat-bui.webp |
| classroom.webp | /uploads/2026/09/hoc-vien-bui-lon-tai-trung-tam-day-ve-my-thuat-bui.webp |
| teacher.webp | /uploads/2026/09/giao-vien-my-thuat-bui-luon-tan-tam-va-kien-nhan-voi-hoc-vien.webp |
| watercolor.jpg | /uploads/wordpress/2026/08/loi-thuong-gap-khi-hoc-ve-mau-nuoc.jpg |
| poster.jpg | /uploads/wordpress/2026/08/mau-poster.jpg |
| oil.jpg | /uploads/wordpress/2026/08/tranh-son-dau-1.jpg |
| acrylic.jpg | /uploads/wordpress/2026/08/tranh-acrylic-1.jpg |
| sketch.jpg | /uploads/wordpress/2018/06/ky-hoa-07.jpg |

Giữ ảnh thật về đất sét hiện có vì chưa tìm được ảnh cùng chất liệu phù hợp trên nguồn khảo sát. Những ảnh đánh giá và góc sáng tạo hiện có được giữ theo yêu cầu. Tên chương trình, học phí và chính sách lấy từ dữ liệu dự án, không tự biến thành dữ liệu được Bụi xác nhận.
