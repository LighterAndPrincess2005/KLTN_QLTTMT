# Thanh toán chuyển khoản SePay

API dùng C#/.NET; không cần chọn hoặc chép ví dụ PHP/Node.js. Ngân hàng nhận đã khai báo riêng trên máy: ACB, 36266017, Lê Hoàng Bảo Long. Chưa bật nhận tiền tự động khi chưa có webhook HTTPS.

## Bật tích hợp

1. Liên kết đúng tài khoản ACB trong SePay. Kiểm tra tên người nhận tại ngân hàng.
2. Triển khai API lên HTTPS công khai. Không dùng địa chỉ `127.0.0.1` làm webhook. Nếu dùng đường hầm để phát triển, chỉ công khai API và không công khai SQL Server; thay URL khi đường hầm đổi.
3. Trong SePay → Tích hợp → Webhooks → Thêm webhook: tên `Học phí Mỹ Thuật Bụi`; URL `https://<API>/api/thanh-toan/sepay/webhook`; JSON; sự kiện Tiền vào; chọn riêng ACB 36266017; bật gửi lại khi lỗi.
4. Chọn xác thực API Key. Tự đặt một khóa ngẫu nhiên dài, không dùng mật khẩu tài khoản/ngân hàng. Chạy `tools/configure-sepay.ps1` trên máy chạy API, nhập cùng khóa tại lời nhắc được che. Script ghi `.local/payment.json`, thư mục này không lên Git. Nếu triển khai máy chủ, đặt các biến `SePay__Enabled=true`, `SePay__BankCode=ACB`, `SePay__AccountNumber=36266017`, `SePay__AccountName`, `SePay__WebhookKey` trong kho cấu hình bí mật của máy chủ.
5. Mã chuyển khoản dạng `BUI` + 12 ký tự hex. Có thể để webhook nhận toàn bộ tiền vào của tài khoản đã chọn: API tự đọc mã trong nội dung. Nếu bật lọc tiền tố BUI trong SePay, cần cấu hình nhận diện mã tương ứng tại SePay trước, tránh bỏ sót giao dịch.
6. Chỉ bật sau khi đã đối chiếu URL và khóa hai phía. Không bấm Gửi thử hoặc chuyển tiền trong đợt này theo yêu cầu chủ dự án. Giao dịch thật và khả năng nhận webhook từ Internet chưa được nghiệm thu.

## Luồng và đối soát

- Web gọi `POST /api/thanh-toan/{dangKyId}/phien`, nhận QR theo số dư do API tính. QR dùng `https://vietqr.app/img`. Web thăm dò trạng thái API mỗi 4 giây khi trang hiển thị; không gọi API giao dịch SePay để thăm dò.
- Webhook xác thực `Authorization: Apikey <khóa>`. API kiểm tra số tài khoản, chiều tiền, số tiền nguyên VND, mã thanh toán và hạn giữ chỗ; duy nhất hóa ID giao dịch để không cộng tiền hai lần.
- Thiếu tiền: ghi nhận phần đã thu và vẫn chờ thanh toán. Đủ tiền trong hạn: xác nhận đăng ký; web và WinForms đọc cùng dữ liệu thu tiền/đăng ký.
- Tiền thừa, đến muộn hoặc vào phiếu đã xác nhận: đưa về khoản chờ đối chiếu, không tự tăng chỗ. Mã không khớp: giữ sự kiện để đối soát, không gán đại học viên. Nhân viên có quyền Thu tiền xem `GET /api/thanh-toan/doi-soat`; khoản chưa phân bổ có thể xem trong mục Thu tiền chờ đối chiếu của WinForms. Chưa bổ sung màn quản lý sự kiện SePay riêng trên WinForms.
- Lớp 0đ xác nhận ngay bằng luồng miễn phí; không sinh QR, không tạo giao dịch ngân hàng. Đây không phải phép thử SePay.

## Cập nhật dữ liệu

Sau khi sao lưu và build API: chạy `dotnet <MyThuat.Api.dll> --prepare-payments` từ thư mục dự án API. Lệnh sao lưu SQL Server có xác minh, áp dụng `012_ThanhToanOnline.sql`, chuẩn hóa địa chỉ phòng MAIN và thêm lớp trải nghiệm nếu chưa tồn tại; không xóa tài khoản hoặc đặt lại đăng ký.

## Bằng chứng kiểm thử

`dotnet run --project LhlArt/IntegrationChecks -c Release -- --sqlserver`: 110 kiểm tra đạt, trong đó 17 kiểm tra thanh toán dùng webhook giả lập và SQL Server tạm. Các kiểm tra này không gửi giao dịch SePay hoặc email thật. Chưa kiểm thử chuyển khoản thật, webhook qua Internet, tải lớn hoặc tự động hoàn tiền ngân hàng.

Tài liệu đối chiếu: https://developer.sepay.vn/vi/sepay-webhooks/tao-webhook và https://developer.sepay.vn/vi/sepay-webhooks/tao-qr-va-form-thanh-toan (02/11/2026).
