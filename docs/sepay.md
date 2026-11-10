# Thanh toán chuyển khoản SePay

API dùng C#/.NET. QR và nút báo đã chuyển khoản hoạt động khi đã cấu hình ngân hàng và số tài khoản nhận. Nhân viên đối chiếu và ghi nhận tiền trên WinForms. Để tự động xác nhận chuyển khoản, cấu hình thêm webhook HTTPS và khóa xác thực.

## Bật tích hợp

1. Liên kết đúng tài khoản ACB trong SePay. Kiểm tra tên người nhận tại ngân hàng.
2. Triển khai API lên HTTPS công khai. Không dùng địa chỉ `127.0.0.1` làm webhook. Nếu dùng đường hầm để phát triển, chỉ công khai API và không công khai SQL Server; thay URL khi đường hầm đổi.
3. Trong SePay → Tích hợp → Webhooks → Thêm webhook, dùng tên `Học phí Mỹ Thuật Bụi`, URL `https://<API>/api/thanh-toan/sepay/webhook` và định dạng JSON. Chọn sự kiện Tiền vào và tài khoản nhận học phí, rồi bật gửi lại khi lỗi.
4. Chọn xác thực API Key và tạo một khóa ngẫu nhiên. Chạy `tools/configure-sepay.ps1` trên máy chạy API và nhập cùng khóa tại lời nhắc được che. Script ghi `.local/payment.json`, thư mục này không lên Git. Khi triển khai máy chủ, đặt `SePay__Enabled=true`, `SePay__BankCode`, `SePay__AccountNumber`, `SePay__AccountName` và `SePay__WebhookKey` trong cấu hình riêng của máy chủ.
5. Mã chuyển khoản dạng `BUI` + 12 ký tự hex. Có thể để webhook nhận toàn bộ tiền vào của tài khoản đã chọn: API tự đọc mã trong nội dung. Nếu bật lọc tiền tố BUI trong SePay, cần cấu hình nhận diện mã tương ứng tại SePay trước, tránh bỏ sót giao dịch.
6. Chỉ bật sau khi đã đối chiếu URL và khóa hai phía. Không bấm Gửi thử hoặc chuyển tiền trong đợt này theo yêu cầu chủ dự án. Giao dịch thật và khả năng nhận webhook từ Internet chưa được nghiệm thu.

## Luồng và đối soát

- Web gọi `POST /api/thanh-toan/{dangKyId}/phien`, nhận QR theo số dư do API tính. QR dùng `https://vietqr.app/img`. Web thăm dò trạng thái API mỗi 4 giây khi trang hiển thị; không gọi API giao dịch SePay để thăm dò.
- Sau khi chuyển tiền, học viên bấm **Xác nhận đã chuyển khoản**. Web hiển thị **Đã gửi · Chờ trung tâm duyệt** và giữ trạng thái này khi tải lại. WinForms: **Đăng ký & học phí → Học viên báo chuyển khoản → Đối chiếu và ghi nhận tiền**; nhập mã chứng từ, mã tham chiếu ngân hàng và xác nhận đã thực nhận. Khi thu đủ, đăng ký được xác nhận và rời danh sách chờ duyệt. Bấm nút trên web không tự ghi nhận tiền.
- Webhook xác thực `Authorization: Apikey <khóa>`. API kiểm tra số tài khoản, chiều tiền, số tiền nguyên VND, mã thanh toán và hạn giữ chỗ; duy nhất hóa ID giao dịch để không cộng tiền hai lần.
- Thiếu tiền: ghi nhận phần đã thu và vẫn chờ thanh toán. Đủ tiền trong hạn: xác nhận đăng ký; web và WinForms đọc cùng dữ liệu thu tiền/đăng ký.
- Tiền thừa, đến muộn hoặc vào phiếu đã xác nhận: đưa về khoản chờ đối chiếu, không tự tăng chỗ. Mã không khớp: giữ sự kiện để đối soát, không gán đại học viên. Nhân viên có quyền Thu tiền xem `GET /api/thanh-toan/doi-soat`; khoản chưa phân bổ có thể xem trong mục Thu tiền chờ đối chiếu của WinForms. Chưa bổ sung màn quản lý sự kiện SePay riêng trên WinForms.
- Lớp 0đ xác nhận ngay bằng luồng miễn phí; không sinh QR, không tạo giao dịch ngân hàng. Đây không phải phép thử SePay.

## Cập nhật dữ liệu

Sau khi sao lưu và build API: chạy `dotnet <MyThuat.Api.dll> --prepare-payments` từ thư mục dự án API. Lệnh sao lưu SQL Server có xác minh, áp dụng `012_ThanhToanOnline.sql`, chuẩn hóa địa chỉ phòng MAIN và thêm lớp trải nghiệm nếu chưa tồn tại; không xóa tài khoản hoặc đặt lại đăng ký.

## Bằng chứng kiểm thử

`dotnet run --project LhlArt/IntegrationChecks -c Release -- --sqlserver`: 110 kiểm tra đạt, trong đó 17 kiểm tra thanh toán dùng webhook giả lập và SQL Server tạm. Các kiểm tra này không gửi giao dịch SePay hoặc email thật. Chưa kiểm thử chuyển khoản thật, webhook qua Internet, tải lớn hoặc tự động hoàn tiền ngân hàng.

Tài liệu đối chiếu: https://developer.sepay.vn/vi/sepay-webhooks/tao-webhook và https://developer.sepay.vn/vi/sepay-webhooks/tao-qr-va-form-thanh-toan (02/11/2026).

## Demo đồ án không chuyển tiền thật

Khi chạy với dữ liệu dành riêng cho demo, đặt `"DemoPayments": true` trong `api.config.json` cạnh tệp chạy WinForms rồi mở lại ứng dụng. Phiếu mô phỏng tự điền chứng từ `DEMO-PT-...` và tham chiếu `DEMO-CK-...`. Bấm Xác nhận ghi khoản thu mẫu vào dữ liệu đăng ký để minh họa đồng bộ với web. Phiếu in có nhãn MÔ PHỎNG ĐỒ ÁN. Khi dùng dữ liệu thật, để mặc định hoặc đặt `"DemoPayments": false` để yêu cầu chứng từ và xác nhận thực nhận.
