# Kịch bản trình diễn CNTT-KLCN082

Trình diễn theo đề cương trang 2 và 4: Website trước, WinForms sau. Dùng một phiếu ghi danh xuyên suốt để cho thấy hai nền tảng đồng bộ qua API và SQL Server.

Chuẩn bị trước một lớp đang nhận đăng ký, hồ sơ có quyền đăng ký, một học viên đã có lịch và một hồ sơ đã hoàn thành để gửi đánh giá. Dùng dữ liệu thử. Mở sẵn hai cửa sổ.

| Thứ tự | Tiêu chí | Thao tác |
| --- | --- | --- |
| 1 | Web: Trang chủ và giới thiệu khóa học | Từ Trang chủ vào Khóa học, mở một khóa và chỉ nội dung từng buổi, học phí và lịch khai giảng. |
| 2 | Web: Tìm kiếm và lọc | Tìm Junior, mở kết quả và bấm Đăng ký khóa học để sang danh sách lớp đã lọc theo Junior. |
| 3 | Web: Thành viên, tư vấn và ưu đãi | Cho xem form đăng ký, đăng nhập tài khoản thử, mở Góc tư vấn và ưu đãi học phí của khóa. Chuẩn bị sẵn một yêu cầu có phản hồi. |
| 4 | Web: Đăng ký và thanh toán | Chọn hồ sơ, lớp và xác nhận. Ghi lại ID phiếu. Nếu QR đã cấu hình, cho xem mã và nút báo chuyển khoản. Dùng lớp 0đ để minh họa xác nhận miễn phí, hoặc dùng phiếu chờ và ghi nhận khoản thu mẫu tại WinForms. |
| 5 | Web: Lịch và theo dõi khóa | Mở Góc học tập. Cho xem lịch tuần, điểm danh, nhận xét, bài thực hành và kết quả bằng hồ sơ đã có dữ liệu. |
| 6 | Web: Phản hồi | Mở hồ sơ đã hoàn thành khóa và vào Phản hồi & đánh giá. |
| 7 | WinForms: Loại khóa, khóa, lớp | Mở nhóm Đào tạo & lịch học. Cho xem các danh mục, lịch đã xếp và mã lớp tự sinh theo khóa. |
| 8 | WinForms: Học viên và giáo viên | Tìm một học viên, xem người đại diện và mở hồ sơ giáo viên đã phân công. |
| 9 | WinForms: Họa cụ | Mở danh mục, định mức theo buổi, tồn và phiếu kho. Giải thích lớp 6/8 dự trù đủ cho 8. |
| 10 | WinForms: Đăng ký và lịch | Vào Đăng ký & học phí, tra ID phiếu ở bước 4. Đối chiếu công nợ. Với khoản thu mẫu được xác nhận, mở phiếu thu rồi tải lại Web để thấy đăng ký đã xác nhận. Mở Lịch học & điểm danh để minh họa theo dõi. |
| 11 | WinForms: Ưu đãi | Mở Mã giảm giá / ưu đãi, phân biệt giảm tự động của khóa và mã học phí nhập riêng. Có thể cho xem cấu hình nhóm họa cụ. |
| 12 | WinForms: Tìm kiếm và báo cáo | Tìm tên hoặc mã trong danh mục. Mở Báo cáo thu / hoàn, chọn Từ ngày – Đến ngày, xem tổng thu, hoàn, thu ròng và bảng chi tiết. |

Kết thúc bằng sơ đồ kết nối hai ứng dụng, báo cáo Word và Excel kiểm thử. Nếu thời gian ngắn, mở sẵn dữ liệu ở mỗi màn và dành phần thao tác đầy đủ cho luồng Web ghi danh → WinForms ghi nhận khoản thu → Web hiện trạng thái và lịch.
