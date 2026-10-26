# Quên mật khẩu qua Gmail

Email gửi dự kiến là `Vohuynhgiahuy1706@gmail.com`. Gmail này phải do người vận hành kiểm soát. Tạo tài khoản mới trên web hoặc WinForms bắt buộc email hợp lệ, nhưng không gửi mã xác minh hay email chào mừng. Chỉ thao tác Quên mật khẩu mới gửi mã đến email đã lưu của tài khoản tương ứng. Không tự gán email gửi cho mọi tài khoản. Tài khoản cũ chưa có email không bị tự sửa hoặc khóa; cần bổ sung email đúng chủ sở hữu.

## Cấu hình Gmail một lần

1. Đăng nhập đúng Gmail gửi tại https://myaccount.google.com/ .
2. Vào Bảo mật và đăng nhập, bật Xác minh 2 bước.
3. Mở https://myaccount.google.com/apppasswords . Nếu có yêu cầu, đăng nhập lại; đặt tên ứng dụng `My Thuat Bui` rồi tạo mật khẩu ứng dụng.
4. Chạy `backend/MyThuat/Cau_hinh_Gmail.cmd`, nhập mật khẩu ứng dụng 16 ký tự vào ô nhập được che. Không dùng mật khẩu Gmail thông thường; không gửi mật khẩu trong chat hoặc đưa lên Git.
5. Bí mật được lưu cục bộ trong `MyThuat.Api/.local/mail.json`, được Git bỏ qua. Đây là file cấu hình riêng có chứa bí mật; bảo vệ tài khoản Windows và không chia sẻ file. API đọc lại cấu hình khi file đổi. Khi deploy, dùng secret manager hoặc biến môi trường `RecoveryMail__Enabled`, `RecoveryMail__Username`, `RecoveryMail__AppPassword` thay vì upload cấu hình máy cá nhân.

Nếu không thấy Mật khẩu ứng dụng: kiểm tra đã bật Xác minh 2 bước; tài khoản tổ chức, Advanced Protection hoặc cách cấu hình khóa bảo mật có thể không hỗ trợ. Chưa nhập mật khẩu ứng dụng thì tính năng báo Chưa bật gửi email, không giả vờ đã gửi. Gmail có hạn mức, có thể chặn SMTP; kiểm tra bảo mật Gmail nếu dịch vụ không gửi được. Không đưa lỗi SMTP chứa thông tin nhạy cảm ra client.

Hướng dẫn chính thức: https://support.google.com/accounts/answer/185833 . API dùng SMTP Gmail `smtp.gmail.com:587` với STARTTLS. Không cần đăng ký Resend hoặc mua tên miền để dùng Gmail riêng này.

## Nâng cấp database có kiểm soát

Chạy API bản mới với `--upgrade-recovery` từ đúng content root. Lệnh tạo backup COPY_ONLY và RESTORE VERIFYONLY trước khi thêm bảng bằng `Database/009_KhoiPhucTaiKhoan.sql`. Không đổi mật khẩu, không reset dữ liệu, không tự sửa email tài khoản mẫu. Database mới được tạo từ EF có sẵn bảng mới; database đã tồn tại cần chạy migration này một lần, chạy lại an toàn.

## Luồng sử dụng chung Web và WinForms

Quên mật khẩu → chỉ nhập tên đăng nhập → hệ thống gửi mã đến email đã lưu và hiển thị địa chỉ dạng `lon***@g***.com` → nhập mã 6 chữ số nhận trong email, mật khẩu mới và xác nhận → đăng nhập lại. Người yêu cầu không được chọn email nhận khác. Mã có hiệu lực 10 phút, một lần dùng, tối đa 5 lần thử sai. Tạo mã mới làm mã trước hết hiệu lực. API giới hạn theo IP và chờ 60 giây giữa các yêu cầu thực của một tài khoản. Trong 60 giây, yêu cầu lại trả về yêu cầu trước còn hiệu lực thay vì làm người dùng mất khả năng nhập mã vừa nhận.

API không trả email đầy đủ hoặc mã OTP. Chỉ sau khi SMTP chấp nhận gửi mới trả gợi ý email che một phần; đây là đánh đổi trải nghiệm cho phép thấy một phần thông tin liên hệ của tên đăng nhập hợp lệ, nên phải giữ giới hạn tần suất. Tên không tồn tại, chưa có email hoặc gửi thất bại trả thông báo chung. Mã lưu dưới dạng băm chậm có salt, không lưu mã gốc; mật khẩu lưu bằng PasswordHasher. Hoàn tất đặt lại tăng PhienBanBaoMat để thu hồi mọi phiên cũ. Đổi email trong hồ sơ làm mã đang chờ hết hiệu lực. Không nhận email mới từ người chưa đăng nhập để thay email khôi phục của tài khoản.

Tài khoản chưa có email hoặc không còn quyền truy cập email phải liên hệ trung tâm để xác minh và hỗ trợ. Email dữ liệu mẫu `@example.invalid` không nhận thư; dùng một tài khoản thử có email thật do bạn kiểm soát. Không dùng chung mật khẩu đồ án khi triển khai công khai.

Nếu không nhận thư, kiểm tra thư rác, tên đăng nhập/email đúng, chờ 60 giây và yêu cầu lại; nhập mã từ yêu cầu mới nhất trên chính màn hình đã gửi yêu cầu. Khi SMTP gửi lỗi, mã tạo ra sẽ vô hiệu và server chỉ ghi loại lỗi cùng ID yêu cầu, không ghi mã hoặc bí mật. Thông báo chung không khẳng định email chắc chắn được gửi.

## Mẫu email có nhận diện thương hiệu

Email khôi phục dùng HTML nền kem, đầu thư xanh đậm, logo nhúng và mã 6 số có thể sao chép. Có bản văn bản thuần cho ứng dụng email không hỗ trợ HTML. Logo được đóng gói trong API, không phụ thuộc ảnh từ website hay dịch vụ ngoài.

Xem trước mà không gửi email: chạy API với `--preview-recovery-email` từ thư mục API. Tệp `.local/previews/recovery-email.html` chỉ dùng mã minh họa `000000`, không phải mã đăng nhập. Thiết kế mới áp dụng cho các email gửi sau khi cập nhật API; thư đã nhận giữ nguyên.
