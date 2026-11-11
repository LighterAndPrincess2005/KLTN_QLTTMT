# Đánh giá và cảm nhận học viên

Học viên gửi đánh giá sau khi hoàn thành khóa học tại khu vực **Học tập**. Biểu mẫu có số sao, nội dung, ảnh thành phẩm và lựa chọn đồng ý công khai.

Quản trị xử lý trong WinForms → **Hồ sơ & tư vấn → Duyệt đánh giá khóa học**. Màn hình cho phép xem ảnh, duyệt, từ chối và ẩn đánh giá đã công bố. API công khai chỉ trả các đánh giá đủ điều kiện hiển thị.

Website lấy đánh giá từ `GET /api/danh-gia-khoa/cong-khai` và hiển thị trong mục **Đánh giá & cảm nhận học viên**. Tên hiển thị lấy từ trường `tenNguoiDangCongKhai`; khi thiếu tên, giao diện dùng “Học viên Mỹ Thuật Bụi”. Mỗi nhóm có tối đa ba đánh giá, có nút chuyển nhóm khi danh sách dài.

## Nội dung có sẵn trên giao diện

Ba cảm nhận của Trần Yến Lan, Hương Giang và Bảo Châu trong `StudentReviews.tsx` được giữ cùng danh sách đánh giá từ API. Các ảnh nằm trong `public/testimonials/`. Nguồn của bộ ảnh và cảm nhận này là PPA, được ghi tại [student-review-sources.json](student-review-sources.json).

## Phân biệt với tư vấn và đề nghị

Tư vấn khóa học dùng `/api/tu-van`. Phản hồi và đề nghị nghiệp vụ dùng `/api/phan-hoi`. Đánh giá khóa học công khai dùng `/api/danh-gia-khoa` và có bước duyệt riêng.
