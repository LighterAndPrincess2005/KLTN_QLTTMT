# Database chính và tình huống kiểm thử

Web và WinForms dùng cùng `MyThuatDb` qua API. Dữ liệu thêm mới là giả lập có liên kết thực, không phải người thật, giao dịch ngân hàng thật hoặc báo giá thương mại. Dữ liệu cũ được giữ nguyên.

## Chuẩn bị có kiểm soát

Từ thư mục repository:

```powershell
dotnet run --project backend/MyThuat/MyThuat.Api -- --prepare-main
dotnet run --project backend/MyThuat/MyThuat.Api -- --check-main
```

`--prepare-main` sao lưu SQL Server bằng COPY_ONLY/CHECKSUM và RESTORE VERIFYONLY trước khi áp dụng migration 007. Bộ MAIN được thêm trong transaction, có kiểm tra sĩ số, tuổi, tiền và lịch. Không chạy tự động khi API khởi động. Chạy lại không tạo trùng và không đổi các lịch/phiếu đã có. Đây là nâng cấp cộng thêm, không phải lệnh xóa và nạp lại database.

Database cũ cần đã có schema 002 và bản bổ sung 005/006. Database mới dùng schema 002 hiện hành. Backup, khóa bảo vệ token, ảnh biên tập và danh sách tài khoản nằm trong `backend/MyThuat/MyThuat.Api/.local/`, không đưa vào Git. Khi chuyển máy phải mang theo database, tệp ảnh và cấu hình API phù hợp; không chỉ copy mã nguồn. Không chia sẻ thư mục khóa bí mật hoặc khóa tra cứu tư vấn cho người khác.

## Tài khoản mới

Danh sách chính xác và khóa tra cứu tư vấn mẫu: `.local/MAIN_TAI_KHOAN.md`.

| Tài khoản | Tình huống |
|---|---|
| hv01–hv04 | Bản thân và nhiều hồ sơ con; có lịch học, một số có tư vấn |
| hv05–hv14 | Đã đăng ký/đã thanh toán; có tài khoản học nhiều lớp và hoàn thành khóa |
| hv15 | Đăng ký hết hạn chưa trả tiền |
| hv16 | Giữ chỗ chưa trả tiền, có mã giảm |
| hv17 | Giữ chỗ chưa thanh toán, còn nguyên công nợ |
| hv18 | Đã trả một phần, còn công nợ |
| hv19 | Có hồ sơ, chưa đăng ký lớp |
| hv20 | Có tài khoản, chưa tạo hồ sơ |
| gv01–gv06 | Có phân công; lịch khác nhau và có giáo viên dạy nhiều lớp |
| gv07 | Trợ giảng Junior |
| gv08 | Chưa được phân lớp |
| hocvu01 | Hồ sơ, tư vấn, phản hồi và đào tạo |
| thungan01 | Thu/hoàn tiền và công nợ |
| kho01 | Quản lý kho |
| quantri01 | Quản trị |

Mật khẩu khởi tạo mới được ghi trong danh sách cục bộ. Tài khoản cũ không bị đặt lại mật khẩu. Không dùng mật khẩu khởi tạo chung khi vận hành công khai.

Mật khẩu khởi tạo MAIN hiện là `Bui@2026`. Người dùng đổi tại **Tài khoản → Thông tin cá nhân → Đổi mật khẩu**, cần mật khẩu hiện tại và xác nhận mật khẩu mới. Đổi thành công thu hồi phiên cũ và yêu cầu đăng nhập lại. Nếu đã đổi, bảng hướng dẫn chỉ còn là tham chiếu mật khẩu khởi tạo, không phải mật khẩu đang dùng. Chỉ dùng `--reset-main-passwords` khi được yêu cầu đặt lại đúng bộ 32 tài khoản MAIN; lệnh sao lưu trước, không tác động tài khoản khác và không ghi mật khẩu vào nhật ký.

## Lớp và mã giảm

| Mã lớp | Tình huống lúc khởi tạo |
|---|---|
| MAIN_L01 | Foundation, 5/10 chỗ |
| MAIN_L02 | Junior, đầy 8/8 |
| MAIN_L03 | Acrylic, 9/10 |
| MAIN_L04 | Basic, trống |
| MAIN_L05 | Intermediate, có phiếu chưa thanh toán |
| MAIN_L06 | Màu nước, có phiếu trả một phần và chưa trả |
| MAIN_L07 | Tương lai, chưa đến thời điểm mở đăng ký |
| MAIN_L08 | Đang học, có điểm danh, vắng và nhận xét |
| MAIN_L09 | Đã kết thúc, có kết quả và mã xác nhận |
| MAIN_L10 | Lớp nháp, chưa phân công/xếp lịch |
| MAIN_L11 | Có phiếu hết hạn, hủy và một phiếu dùng mã hết lượt |
| MAIN_L12 | Đã qua hạn đăng ký, không hiển thị công khai |
| MAIN_L13 | Junior còn chỗ, thử JUNIOR10 |
| MAIN_L14 | Foundation, có một lượt học bù từ lớp đang học |

Các lớp giữ một khung giờ cố định theo tuần. Ngày được tính tại lần khởi tạo, không tự dịch chuyển mỗi lần mở web. Phiếu chờ sẽ hết hạn đúng thời gian và giải phóng chỗ/lượt ưu đãi; không làm dữ liệu giả tồn tại mãi bằng cách sửa hạn ngầm.

| Mã | Phạm vi/điều kiện |
|---|---|
| JUNIOR10 | Junior giảm 10%, trần 400.000đ |
| MAUNUOC200 | Màu nước giảm 200.000đ |
| BUIMOI15 | Mọi khóa, thành viên trong 7 ngày từ xác nhận, giảm 15%, trần 500.000đ |
| BUICHUNG5 | Mọi khóa, giảm 5%, trần 250.000đ |
| DAHETHAN | Đã hết hạn, phải từ chối |
| HETLUOT | Đã hết lượt, phải từ chối |
| HOACU10 | Chỉ khoản họa cụ, không dùng giảm học phí |

Báo giá không giữ chỗ hoặc giữ lượt mã; tạo phiếu kiểm tra lại trong transaction. Không cộng dồn hai mã cùng nhóm khoản thu. Học phí và số tiền giảm được chốt trên khoản thu, không sửa học phí gốc của khóa để làm khuyến mãi.

## Kho

24 SKU, gồm giấy A4/giấy màu nước, chì, gôm, sáp, màu nước, acrylic theo từng màu, cọ, bảng pha, giá vẽ, tạp dề, canvas, đất nặn, bộ bán riêng, sổ và vật tư phụ. Đơn vị cơ sở, giá giả lập và mức đặt hàng riêng theo SKU. Tồn được tính từ chứng từ, không có trường tồn bị nhập tùy ý.

Có phiếu nhập, xuất giấy, cấp cọ dùng chung, trả cọ, hỏng một cọ, một cọ còn đang mượn, dự trữ 200 tờ giấy và một SKU tồn thấp để thử cảnh báo. Định mức liên kết nội dung buổi. Phải lấy giá nhà cung cấp xác nhận trước khi vận hành mua hàng thật.

## API bổ sung dùng chung cho Web/WinForms

- `POST /api/tu-van`: khách gửi tư vấn, nhận mã + khóa tra cứu; khóa chỉ lưu dạng SHA256 ở DB, không đưa vào URL.
- `POST /api/tu-van/tra-cuu`, `/bo-sung`: tra cứu/gửi thêm bằng mã + khóa. Có giới hạn tần suất.
- `GET /api/tu-van/toi`, `/toi/{id}`: thành viên xem yêu cầu của mình; `POST /toi/{id}/bo-sung` gửi tiếp bằng tài khoản.
- `GET /api/tu-van`: nhân viên có quyền Hồ sơ, phân trang và lọc trạng thái. `GET /{id}` ghi nhận đã đọc; `POST /{id}/tra-loi`, `PUT /{id}/trang-thai` cần RowVersion.
- `/api/phan-hoi`: phản hồi liên quan đăng ký đã có; không ép khách chưa ghi danh dùng API này.
- `/api/noi-dung/bai-viet`: bài đã đăng; `/quan-ly/bai-viet` và `PUT /quan-ly/bai-viet/{id}` cho nhân viên. ID 0 tạo mới, ID khác cập nhật có RowVersion. Bài nháp không công bố.
- `/api/noi-dung/giao-vien`: chỉ hồ sơ được chọn công khai và giáo viên hoạt động; không lộ số điện thoại/ngày sinh/email nhân sự. Quản lý ở `/quan-ly/giao-vien`.
- `/api/noi-dung/khoa-hoc`, `PUT /quan-ly/khoa-hoc/{id}`: ảnh, đầu ra, vật tư gồm trong học phí, chuẩn bị và bộ hình minh họa.
- `POST /api/tep/cong-khai`: nhân viên tải ảnh biên tập công khai PNG/JPEG/WebP tối đa 5 MB; không dùng cho hồ sơ cá nhân. DB lưu URL, tệp nằm ngoài mã nguồn. Cần sao lưu cả tệp.
- `POST /api/dang-ky/bao-gia`: kiểm tra mã theo khóa/tài khoản, tính tổng tiền và hạn giữ chỗ dự kiến. Không ghi đăng ký.
- `DELETE /api/hoc-vien/toi/{id}`: chỉ hồ sơ của mình chưa dùng, không có người đại diện khác. Hồ sơ có lịch sử không xóa.
- `GET /api/hoc-tap/giao-vien/buoi-cua-toi`: lịch thực của giáo viên đăng nhập.
- `POST /api/hoc-tap/de-nghi`: gửi đề nghị học bù/đổi lịch đến nhân viên; không tự xếp bù hoặc thay lịch.

Web có hộp xử lý tư vấn `/ho-tro` cho học vụ/quản trị. Chưa viết thêm màn hình WinForms trong đợt này; WinForms có thể tích hợp các hợp đồng API trên. Bài viết và hồ sơ công khai có API quản trị; chưa có trình soạn bài viết trực quan trên web.

## Trước khi vận hành thật

Bản hiện tại là triển khai cục bộ đã kiểm thử, không phải chứng nhận sẵn sàng sản xuất. Cần đổi toàn bộ mật khẩu mẫu; kiểm thử tải/bảo mật và phục hồi backup; HTTPS, CORS và cấu hình host đúng môi trường; dịch vụ email/SMS để khôi phục tài khoản/thông báo; chính sách dữ liệu/đồng ý công bố ảnh được trung tâm duyệt. Thanh toán hiện do thu ngân đối chiếu tiền mặt/chuyển khoản, chưa tích hợp cổng thanh toán hay webhook ngân hàng. Không được tự xác nhận thanh toán từ nút trên web. Không coi ảnh/người/tài chính giả lập là thành tích hoặc giao dịch thật.
