# Dữ liệu mẫu LHL Art

Bộ mẫu dựa trên các bảng khóa/lớp trong `CNTT-KLCN082_Tuan4.docx`. Mã tạo dữ liệu nằm tại `MyThuat.Api/Services/DemoData.cs`, đi cùng API trong Git. Không cần nhận database cá nhân của người khác.

## Nạp trên máy khác

1. Cài .NET SDK theo `global.json` và SQL Server LocalDB; cấu hình kết nối nếu dùng SQL Server khác trong `MyThuat.Api/appsettings.Local.json` (file này không đưa lên Git).
2. Bật API, tạo quản trị lần đầu để database có tài khoản quản trị.
3. Mở `Nap_DuLieuMau.cmd`, hoặc chạy `dotnet run --project MyThuat.Api -c Release -- --seed-demo` từ thư mục chứa `MyThuat.sln`.
4. Tải lại web hoặc gọi `GET /api/khoa-hoc` và `GET /api/lop-hoc`.

Lệnh nạp có giao dịch, chạy lại không thêm trùng. API không tự thêm mẫu khi khởi động thông thường. Chỉ nạp mẫu vào database thử nghiệm.

| Khóa | Buổi | Tiết/buổi | Phút/tiết | Học phí |
| --- | ---: | ---: | ---: | ---: |
| Junior | 16 | 3 | 30 | 3.200.000 đ |
| Foundation 1 | 16 | 4 | 30 | 3.600.000 đ |
| Pre-Basic 1 | 16 | 4 | 30 | 3.800.000 đ |
| Basic 1 | 16 | 4 | 30 | 4.000.000 đ |
| Intermediate 1 | 16 | 4 | 30 | 4.400.000 đ |
| Acrylic nhập môn | 8 | 4 | 30 | 2.400.000 đ |

Có 6 lớp, 88 nội dung/buổi học, 4 giáo viên, 2 phòng, 1 thành viên đại diện và 3 học viên hư cấu. Mã mẫu bắt đầu bằng `DEMO_`. Lịch mẫu bắt đầu 17–18/10/2026; khi nạp sau thời điểm này, mã tự chọn cuối tuần sắp tới phù hợp. Không có tuổi tối đa.

Mật khẩu tài khoản thử được sinh riêng khi nạp và lưu trong `MyThuat.Api/.local/Tai_khoan_mau.txt` trên máy đó, không có mật khẩu chung trong Git. Chưa có phiếu thu hay giao dịch tiền giả. Đăng ký vẫn cần quy định đang hiệu lực; bộ mẫu không tự kích hoạt hay thay đổi quy định quản trị.

Thông tin khóa và đề cương là mẫu để trình diễn, không phải cam kết thương mại của trung tâm. Hồ sơ giáo viên, thành viên và học viên không phải người dùng thật.

## Mã ưu đãi kiểm thử

Sau khi có bộ mẫu và quản trị, chạy riêng `dotnet run --project MyThuat.Api -c Release -- --seed-test-promos` từ thư mục MyThuat. Lệnh không chạy lúc khởi động API thông thường, không kích hoạt quy định và không ghi đè mã đã có.

- `LHLTEST10`: giảm 10% học phí, tối đa 500.000 đ, các khóa.
- `LHLTEST200K`: giảm 200.000 đ học phí, chỉ Foundation 1 mẫu (`DEMO_KH02`).

Mỗi mã có 1.000 lượt, thời hạn 90 ngày từ ngày nạp lần đầu. Hai mã dùng cho kiểm thử; mã đã hết hạn cần được xử lý bằng mã mới theo nghiệp vụ quản trị. Không tự cộng dồn hoặc tặng ưu đãi lần hai/giới thiệu. Giá giảm được chốt khi tạo phiếu, và vẫn cần quy định hiệu lực cùng các điều kiện đăng ký khác.
