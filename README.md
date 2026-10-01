# LHL Art — giao diện website trung tâm mỹ thuật

Frontend React + TypeScript được dựng trước theo nghiệp vụ đồ án. Website hiện đã gọi API thật. Đọc [hướng dẫn nối API và chạy web](NOI_API.md). Các file dữ liệu mẫu được giữ để tham khảo nhưng không được dùng trong ứng dụng đang chạy.

## Chạy dự án

```powershell
cd D:\long2026\KLTN_QLTTMT
npm install
npm run dev
```

Nếu máy chưa cài Node.js/npm, có thể mở `preview.html` qua một web server tĩnh để xem giao diện ngay. Không mở trực tiếp bằng `file://` nếu trình duyệt chặn tài nguyên.

## Cấu trúc chính

- `src/App.tsx`: trang chủ, bộ lọc lớp và luồng đăng ký 3 bước.
- `src/data/mockData.ts`: khóa học, lớp học, đánh giá và ảnh mẫu.
- `src/services/artCenterService.ts`: lớp truy cập dữ liệu, điểm thay thế bằng API sau này.
- `src/types.ts`: kiểu dữ liệu nghiệp vụ dùng chung.
- `src/styles.css`: toàn bộ hệ thống giao diện responsive.
- `src/animations/`: hiệu ứng SVG hoa, lá và cánh hoa bay ngang màn hình.
- `src/animations/`: hiệu ứng SVG hoa, lá và cánh hoa bay ngang màn hình.
- `preview.html`: bản xem trước tĩnh, dùng khi chưa cài được thư viện npm.

## Tích hợp API sau này

Giữ nguyên chữ ký các hàm `getCourses`, `getClasses` và `submitRegistration` trong service. Thay dữ liệu mẫu bằng các lệnh `fetch`/Axios và cấu hình URL qua biến môi trường, ví dụ `VITE_API_BASE_URL`.


## Bản nối API

Đọc [NOI_API.md](NOI_API.md) để biết cách bật API, chạy web, xác minh hồ sơ và cấu hình triển khai. Mã nguồn API tách riêng ở `backend/MyThuat`.
