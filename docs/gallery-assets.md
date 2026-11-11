# Quản lý ảnh website Mỹ Thuật Bụi

## Ảnh khóa học và giáo viên

WinForms → **Đào tạo & lịch học → Khóa học → Ảnh trên website** cho phép tải ảnh bìa, bổ sung ảnh thành phẩm và chọn **Ảnh có sẵn** từ khóa khác. Việc chọn lại ảnh sử dụng cùng đường dẫn đã lưu.

Giáo viên cập nhật ảnh của mình tại **Góc giáo viên → Hồ sơ công khai → Lưu ảnh**. API xác định giáo viên từ phiên đăng nhập. Ảnh cập nhật ngay trên hồ sơ đang công khai; hồ sơ đang ẩn giữ trạng thái ẩn.

Ảnh tải lên hỗ trợ PNG, JPEG và WebP, tối đa 5 MB. Tệp được lưu tại `backend/MyThuat/MyThuat.Api/.local/public-media/` và phục vụ qua `/api/tep/cong-khai/`. Chuyển database sang máy khác cần gửi kèm các tệp ảnh được bản ghi tham chiếu.

## Nguồn ảnh có sẵn

Ảnh tham khảo từ Mỹ Thuật Bụi trong `public/bui/` được ghi nguồn tại [danh sách ảnh](image-sources-refresh.md). Logo của dự án nằm trong `Anhminhhoa/logo.jpg` và bản dùng trên website cần đối chiếu với các thành phần giao diện hiện hành.

Bộ ảnh PPA trước đây trong `public/gallery/` có nguồn tại `ppa-gallery-sources.json`. Bộ tranh tạo bằng công cụ tạo ảnh gồm `garden.webp`, `ocean.webp`, `city.webp` và `still-life.webp` là tài sản của giai đoạn giao diện trước. Các thành phần công khai đã chuyển sang bộ ảnh Bụi theo đợt cập nhật được ghi trong `image-sources-refresh.md`.

Các ảnh tham khảo cần giữ thông tin nguồn. Khi thay bằng ảnh học viên của hệ thống, dùng đúng ảnh và người gửi đã đồng ý công khai.
