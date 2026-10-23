# Điểm nối cần bổ sung trong API của nhóm

Đối chiếu `main` tại commit `2b8237d`, ngày 22/10/2026. Bản sửa này chỉ thay WinForms. Tài liệu dưới đây là yêu cầu phối hợp, không phải endpoint đã được thêm vào API.

## 1. Danh sách đăng ký cho nhân viên

Cần bổ sung `GET /api/dang-ky/quan-ly?trang=1&kichThuoc=100`, trả về envelope `trang`, `kichThuoc`, `tongSo`, `duLieu`. Phân quyền HoSo hoặc ThuTien, không công khai và không cho tài khoản thành viên lấy toàn bộ.

Mỗi dòng cần tối thiểu:

- `id`, `maDangKy`, `hocVienId`, `hocVien`, `lopHocId`, `tenLop`, `tenKhoa`.
- `nguoiDangKyId`, `nguoiDangKy` (họ tên), `lapLuc`, `hanGiuCho`.
- `trangThai`, `hocPhiGocChot`, `kenhDangKy`, `rowVersion`.

Giữ thứ tự phân trang ổn định. Không trả password/token/requestFingerprint. Cần có cả đăng ký web và đăng ký tại quầy. WinForms đã gọi endpoint này; khi gặp 404 mới chuyển sang tra cứu từng ID bằng `GET /api/dang-ky/{id}`.

## 2. Kênh đăng ký

Hiện `DangKy` không có trường kênh; nhật ký luôn ghi `Kenh="API"`; `IdempotencyKey` bị băm. Không thể suy ra chắc chắn online/offline từ dữ liệu đang có.

Cần thống nhất cột `KenhDangKy` và migration bảo toàn lịch sử:

- `ONLINE`: người học/phụ huynh tự đăng ký trên web.
- `OFFLINE`: nhân viên tiếp nhận trực tiếp tại trung tâm.
- `KHONG_RO` hoặc null: bản ghi cũ không đủ bằng chứng; không tự gán tất cả thành online.

Server phải ghi kênh theo ngữ cảnh được xác thực / quy tắc nghiệp vụ đã thống nhất; không tin header do client tự đặt. Trả trường này trong danh sách quản trị và chi tiết. Nếu nhân viên có thể hỗ trợ đăng ký từ xa, cần bổ sung lựa chọn và kiểm tra quyền cho trường hợp đó; không mặc định mọi tài khoản nhân viên đều là offline.

WinForms đã có cột hiển thị hỗ trợ giá trị ONLINE/WEB và OFFLINE/WINFORMS. Chưa gửi trường mới vì DTO API hiện không nhận và không lưu.

## 3. Xóa hồ sơ/danh mục

`ManagementControllerBase` chỉ có GET, POST, PUT. Không có DELETE cho nhân viên. WinForms đang dùng `TrangThai=NGUNG` với các hồ sơ/danh mục cho phép; đại diện dùng `HieuLucDen` để gỡ quyền. Muốn xóa thật bản ghi chưa sử dụng phải có endpoint kiểm tra khóa ngoại/lịch sử trên server; không làm bằng SQL từ WinForms.

## 4. API mới đã hỗ trợ — WinForms đã nối

- `POST /api/tai-khoan/dang-ky`: tài khoản hoạt động ngay; không cần duyệt.
- `GET/POST/PUT /api/quan-ly/thanh-vien`, `/hoc-vien`, `/dai-dien`: lưu hồ sơ và quan hệ; API đặt xác nhận quan hệ khi lưu.
- `POST /api/dang-ky`: nhân viên có quyền HoSo ghi danh thay cho người đại diện, vẫn kiểm tra quan hệ, tuổi, sĩ số và thời hạn.
- `POST /api/giao-dich/thu`: cập nhật học phí và xác nhận đăng ký khi đủ điều kiện; không dùng trạng thái tài khoản thay cho trạng thái đăng ký.
- `GET/POST/PUT /api/quan-ly/khuyen-mai`, `GET /api/khuyen-mai`: phân biệt TuDongApDung và mã nhập riêng. Đã có KhoaApDungId để áp ưu đãi cho một khóa.

Không cần sửa website để đọc ưu đãi công khai theo hợp đồng hiện tại; tải lại trang để nhận dữ liệu mới. Muốn tự cập nhật khi WinForms thay đổi thì nhóm web cần bổ sung refresh/polling hoặc cơ chế thông báo; việc này nằm ngoài phần WinForms.
