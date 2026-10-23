# Mục đánh giá và cảm nhận học viên

Mục `#reviews` được thêm sau khu thành phẩm, trước các câu hỏi thường gặp. Dùng ba ảnh gốc đã tải theo yêu cầu từ mục “Nói về chúng tôi” của PPA. Nhận xét là các đoạn trích trong ảnh chụp tham khảo do người dùng cung cấp; giữ nguyên tên và không đổi tên PPA trong lời nhận xét thành LHL Art. Giao diện ghi rõ “Ảnh và nhận xét minh họa”.

Các tệp: `public/testimonials/tran-yen-lan.jpg`, `huong-giang.jpg`, `bao-chau.jpg`. Ảnh giữ nguyên ở kích thước 300 × 300, không chỉnh mặt hoặc xóa chi tiết ảnh. Nguồn gốc nằm trong `student-review-sources.json`.

Đây là phần minh họa giao diện. Không đọc hoặc công khai dữ liệu phản hồi riêng từ API `/api/phan-hoi`; không thêm điểm sao hoặc số đánh giá giả. Nút gửi phản hồi dẫn đến khu vực thành viên hiện có, nơi người đăng ký lớp gửi phản hồi qua API.

Khi có phản hồi được phép công bố của LHL Art, thay danh sách trong `src/components/StudentReviews.tsx`, dùng đúng người gửi/ảnh và bỏ nhãn minh họa cho các nội dung thật.
