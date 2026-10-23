# Ưu đãi khóa học và mã cá nhân

- `KhuyenMai.TuDongApDung=true`: ưu đãi công khai, bắt buộc `KhoaApDungId`, nhóm `HOC_PHI`, không giới hạn thành viên mới. API tự chọn mức giảm có lợi nhất nếu nhiều chiến dịch cùng hiệu lực. Không cần nhập mã; không cộng nhiều chiến dịch công khai với nhau.
- `TuDongApDung=false` (mặc định): mã riêng người dùng tự nhập. Không xuất mã này ở API danh sách công khai. Việc ẩn mã không thay thế cơ chế cấp mã theo chủ tài khoản: các mã mẫu hiện là mã dùng chung cho người được phát, chưa có bảng sở hữu voucher cá nhân.
- Một ưu đãi khóa + một mã nhập tay được cộng dồn, phần trăm cùng tính trên học phí gốc. Ví dụ 1.000.000đ − 10% khóa − 10% mã = 800.000đ. Mã số tiền/trần giảm giữ nguyên điều kiện; tổng giảm không vượt học phí. Họa cụ tính riêng.
- Báo giá trả `giamKhoaHoc`, `giamMaCaNhan`, `tienGiam` tổng và `tongPhaiTra`. Khi lập phiếu, API kiểm tra lại và chốt cả hai dòng `ApDungUuDai`; `KhoanThuDangKy.TienGiamChot` là tổng giảm. Không tính lại phiếu lịch sử.
- Lượt giữ/dùng được theo dõi cho cả hai ưu đãi. Các điều kiện hiệu lực, hạn mức và khóa vẫn được kiểm tra phía server.

## Nâng cấp database có sẵn

Chạy từ thư mục repo sau khi dừng API cũ:

```powershell
dotnet run --project backend/MyThuat/MyThuat.Api -- --upgrade-promotions
```

Lệnh sao lưu COPY_ONLY và xác minh trước khi chạy `008_TachUuDaiCongKhai.sql`. Các mã cũ giữ `TuDongApDung=false`. Có khóa Junior hoạt động thì tạo chiến dịch công khai Junior 10% riêng, không biến voucher JUNIOR10 thành ưu đãi công khai. Chạy lại không tạo trùng hoặc đổi hạn ưu đãi cũ.

Quản trị có thể tạo chiến dịch khác qua `/api/quan-ly/khuyen-mai`, đặt `TuDongApDung=true`, khóa áp dụng, thời gian, loại giảm, giá trị và hạn mức. Mã kỹ thuật của chiến dịch không hiển thị cho khách. Khi phát mã riêng, giữ `TuDongApDung=false`. WinForms chưa thêm giao diện cấu hình trường mới.

Kiểm thử: SQL Server tạm kiểm tra không lộ voucher, ưu đãi tự động, cộng 10% + 10%, chặn tổng âm, không nhập chiến dịch công khai như mã, phiếu thực tế khớp báo giá và idempotency.
