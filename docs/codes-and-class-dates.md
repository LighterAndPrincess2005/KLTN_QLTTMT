# Cấp mã và khóa ngày lớp

## Mã tự sinh

- Khóa mới trên WinForms: nhập tiền tố 3–4 chữ A–Z, không nhập mã đầy đủ. `jun` được chuẩn hóa thành `JUN`, sinh `JUN-0001`. Một khóa hoàn toàn mới cùng tiền tố nhận số tiếp theo. Phiên bản liên kết `KhoaGocId` vẫn giữ mã khóa gốc và có số phiên bản riêng.
- Chọn khóa có mã `JUN-0001` khi tạo lớp: API cấp `JUN-L0001`, `JUN-L0002`… Bộ đếm lớp dùng chung trong tiền tố, không bắt đầu lại cho từng phiên bản khóa.
- Học viên `HV-0001`, giáo viên `GV-0001`; người liên hệ, phòng, họa cụ và đăng ký cũng dùng bộ đếm bền vững. WinForms và web gọi cùng API.
- Mã cũ không đổi. Với khóa cũ chưa có mã tiền tố mới, lớp dùng tiền tố theo cấp độ: JUN, FND, PRE, BAS, INT, ADV; cấp độ khác dùng LOP.
- Từ khi nâng cấp, bộ đếm `BoDemMa` không bị xóa khi xóa hồ sơ. Lần đầu một tiền tố được dùng, hệ thống tiếp nối số lớn nhất còn trong dữ liệu; các lần sau dùng số đã cấp lưu riêng. Không thể suy ra các mã đã bị xóa trước khi có bộ đếm này. Không xóa/reset bảng bộ đếm để làm sạch dữ liệu kiểm thử.
- Cấp mã và lưu dữ liệu trong cùng transaction; SQL Server khóa riêng tiền tố bằng UPDLOCK/HOLDLOCK. Mã chỉ trả cho khách khi thao tác đã hoàn tất. Ràng buộc duy nhất trên mã vẫn được giữ. API cũ không gửi tiền tố khóa được giữ tương thích với tiền tố KHOA.

## Ngày và trạng thái

- Ngày khai giảng thực tế chỉ được ghi khi quản trị chuyển từ Đang tuyển sinh sang Đang học, đã đủ học viên xác nhận và đến ngày buổi đầu. Lưu/sửa thông tin lớp Đang học không đổi ngày hoặc người duyệt lịch sử.
- Không sửa trực tiếp ngày khai giảng thực tế. Dữ liệu cũ đã sai ngày cần đối chiếu lịch sử để sửa có kiểm soát; bản nâng cấp không tự đoán ngày đúng.
- Lớp đã có lịch không cho đổi trực tiếp khóa, số buổi, mô tả thứ/giờ, ngày đầu hoặc ngày kết thúc. Muốn đổi dùng luồng hoãn/dời buổi có lý do và thông báo.
- Lớp đã công bố hoặc có đăng ký không sửa trực tiếp cửa sổ mở/đóng tuyển sinh.
- Luồng trạng thái: Nháp → Đang tuyển sinh → Đang học → Đã kết thúc. Có thể lưu cùng trạng thái; không quay ngược hoặc nhảy bước. Hoãn/hủy dùng nghiệp vụ riêng.

## Nâng cấp database hiện có

Từ `backend/MyThuat/MyThuat.Api` chạy:

```powershell
dotnet run -c Release -- --upgrade-codes
```

Lệnh sao lưu COPY_ONLY/CHECKSUM và VERIFYONLY trước khi tạo bảng bằng `Database/013_BoDemMa.sql`. Không đổi mã, không xóa hồ sơ, không sửa ngày lịch sử. Database mới tạo bằng `--initialize` đã có bảng qua model hiện hành. Sau nâng cấp cần chạy đúng binary API mới và WinForms mới; chỉ copy mã nguồn không thay bản exe đang mở.

## Kiểm thử

`LhlArt/IntegrationChecks` trên SQL Server tạm kiểm tra cấp mã không nhập tay, tiền tố 3/4 chữ, tiền tố sai, xóa rồi tạo lại, cấp mã đồng thời, giữ ngày khai giảng, khóa ngày lịch, người duyệt và chuyển trạng thái. `LhlArt/UiChecks` kiểm tra control thật cùng HTTP giả lập, bao gồm form tiền tố mã khóa. Không gửi email/giao dịch ngân hàng thật và không sửa database chính trong bộ kiểm tra.
