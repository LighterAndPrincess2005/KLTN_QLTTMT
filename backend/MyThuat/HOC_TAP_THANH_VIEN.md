# Khu học tập dành cho học viên/người đại diện

Trên web: **Học tập trên thanh trên cùng → chọn học viên có lớp đã xác nhận → chọn khóa/lớp**. Có thể bấm **Đổi học viên** trong góc học tập để chọn con khác. Hộp chọn chỉ hiện học viên có ít nhất một phiếu `DA_XAC_NHAN`; học viên chưa có lớp không xuất hiện. Hồ sơ học viên đã xác minh chưa đồng nghĩa đã có đăng ký vào lớp.

| Mục trên web | Thông tin và thao tác |
|---|---|
| Lịch & điểm danh | Ngày giờ, phòng, chủ đề, buổi chính/bù, có mặt/vắng; báo nghỉ buổi sắp tới |
| Nhận xét giáo viên | Nhận xét, điểm sản phẩm, liên kết bài thực hành từng buổi và kết quả cuối khóa |
| Học bù | Buổi vắng gốc, lịch bù đã xếp, lớp bù, hạn hoàn tất; gửi và theo dõi đề nghị |
| Đổi lịch / lớp | Đề nghị đổi lịch một buổi hoặc chuyển lớp cùng khóa; xem trạng thái và câu trả lời |
| Phản hồi | Gửi ý kiến về khóa/lớp; xem lịch sử và trả lời của trung tâm |

## API

- `GET /api/dang-ky/toi`: thêm `hocVienId`, `khoaHocId`, `coTheQuanLy`. Web nhóm theo ID học viên, không ghép bằng tên; hai học viên trùng tên vẫn có dữ liệu riêng.
- `GET /api/hoc-tap/ho-so/{id}/tien-do`: ngày giờ/chủ đề/phòng, người ghi nhận xét, thời điểm ghi; bổ sung danh sách `hocBu`. Phạm vi là toàn bộ lượt theo học, gồm lịch sử chuyển lớp.
- `POST /api/hoc-tap/diem-danh/{id}/bao-nghi`: báo nghỉ; không tự ghi nhận vắng hoặc đặt bù.
- `POST /api/hoc-tap/de-nghi`: gửi đề nghị, body:

```json
{
  "DiemDanhId": 123,
  "LoaiYeuCau": "HOC_BU",
  "LyDo": "Xin học bù buổi đã vắng.",
  "KhungGioMongMuon": "Chiều Thứ Bảy"
}
```

`LoaiYeuCau` nhận `HOC_BU` hoặc `DOI_LICH`. ID trên chỉ là ví dụ, cần lấy từ tiến độ của học viên. Thành viên phải có quyền đại diện đã xác minh cho thao tác đăng ký và phiếu đang hiệu lực. Học bù kiểm tra buổi chính đã vắng, báo nghỉ đủ hạn, số lượt và hạn học bù. Đổi lịch chỉ nhận buổi sắp tới của đăng ký đang hiệu lực. Không nhận trùng yêu cầu cùng loại/buổi đang chờ xử lý; không cho truy cập học viên của người khác.

- `GET/POST /api/phan-hoi`: lịch sử/gửi phản hồi thông thường.
- `GET/POST /api/thay-doi`: xem/gửi yêu cầu chuyển hoặc hủy lớp theo quy trình hiện có.

## Trung tâm xử lý

1. Học vụ/quản trị xem `GET /api/phan-hoi`, đọc lý do và khung giờ đề nghị.
2. Xếp học bù bằng `POST /api/hoc-tap/hoc-bu` với ID điểm danh vắng và buổi đích. API kiểm tra nội dung, phiên bản, chỗ, lịch, thời hạn và số lượt. Khi đặt thành công, đề nghị chờ xử lý tương ứng được trả lời với lịch bù thực tế.
3. Dời lịch lớp bằng `POST /api/lich-hoc/buoi/{id}/doi-lich`, kèm RowVersion hiện tại. Đây là thay đổi lịch chung của buổi đó: cần xem xét các học viên, giáo viên và phòng. Khi dời thành công, đề nghị đổi lịch tương ứng được trả lời với lịch mới. Muốn đổi lớp riêng một học viên thì dùng quy trình chuyển lớp.
4. Nếu cần tư vấn hoặc từ chối, dùng `POST /api/phan-hoi/{id}/tra-loi`. Trả lời một đề nghị không tự đặt bù hoặc đổi lịch.
5. Giáo viên ghi điểm danh/nhận xét bằng `PUT /api/hoc-tap/diem-danh/{id}` trong buổi được phân công. Web chỉ coi nội dung có `GhiLuc` là nhận xét đã được giáo viên/trung tâm ghi; lý do báo nghỉ không được trình bày như đánh giá giáo viên.
6. Thành viên bấm **Cập nhật** hoặc tải lại web để xem kết quả xử lý.

Giao diện quản lý WinForms chưa được xây đầy đủ; hiện các thao tác trung tâm dùng Swagger/API theo quyền.

## Cách lưu tương thích database hiện tại

Đề nghị lưu trong bảng `PhanHoi` hiện có, với tiền tố nội bộ `[HOC_BU:<DiemDanhId>]` hoặc `[DOI_LICH:<DiemDanhId>]`. Phần hiển thị web bỏ tiền tố, tách lịch sử theo từng mục. Endpoint gửi phản hồi thông thường chặn hai tiền tố dành riêng này để không giả tạo đề nghị. Lịch thực tế vẫn thuộc `BuoiHoc`, `DiemDanh`, `HocBu`; không suy ra việc đã duyệt chỉ từ câu trả lời. Không tạo thêm bảng và không thay đổi schema trong lần cập nhật này.

Phản hồi học tập ở khu thành viên là dữ liệu riêng theo tài khoản; phần cảm nhận minh họa trên trang chủ là nội dung riêng, không tự xuất bản ý kiến thành viên.

## Tài khoản riêng của con

Không chia sẻ tài khoản phụ huynh. Dùng tài khoản riêng liên kết với đúng học viên qua `DaiDienHocVien`, quan hệ `TU_BAN_THAN`, quyền `XEM_KET_QUA`. Tài khoản dùng vai trò `THANH_VIEN` hiện có; quyền với học viên được xác định trên server bằng liên kết đã xác minh.

Quản trị tạo/xác minh hồ sơ thành viên của tài khoản, thêm liên kết với `HocVienId` có sẵn và `LaLienHeChinh=false` để giữ phụ huynh là liên hệ chính. Không tạo học viên mới thay cho hồ sơ con đã học. Không cấp `DAY_DU` hoặc `DANG_KY` cho tài khoản chỉ xem học tập. Thành viên tự đăng ký không được tự xác nhận liên kết hay nhận hồ sơ bằng cách nhập tên.

Tài khoản con xem lịch, điểm danh và nhận xét của mình, có thể gửi ý kiến trong phạm vi API cho phép. API chặn đăng ký lớp, báo nghỉ, đề nghị học bù/đổi lịch nếu chỉ có `XEM_KET_QUA`. Không hiện nút đăng ký thêm lớp; form đăng ký cũng chặn bước tiếp theo cho hồ sơ không có quyền đăng ký.

## Góc giáo viên

Tài khoản `GIAO_VIEN` đăng nhập được đưa tới `/#teacher`; thanh trên cùng có **Góc giáo viên**. Trang dùng logo, font và màu LHL Art, tách khỏi nội dung đăng ký công khai. Giáo viên vẫn có thể về trang giới thiệu.

- `GET /api/hoc-tap/giao-vien/buoi-cua-toi`: chỉ các buổi có phân công cho giáo viên của tài khoản; trả tên khóa/lớp, chủ đề, giờ, phòng, vai trò và số học viên. Tối đa 500 buổi.
- `GET /api/hoc-tap/buoi/{id}/danh-sach`: danh sách học viên đã xác nhận, có ID, ngày sinh, trạng thái, báo nghỉ và RowVersion.
- Giáo viên chọn buổi → điểm danh có mặt/vắng → ghi số phút, điểm và nhận xét từng học viên → lưu. Có thể thêm liên kết bài thực hành HTTP/HTTPS.
- Giáo viên chỉ sửa sau khi buổi bắt đầu và trong ngày học. Buổi tương lai/điều chỉnh muộn hiện kết quả để xem; trường hợp chỉnh muộn cần học vụ.
- Hoàn thành buổi chỉ mở sau giờ kết thúc và khi tất cả học viên đã được điểm danh. Server kiểm tra lại trước khi ghi.

Quản trị tạo tài khoản giáo viên bằng `POST /api/tai-khoan`, vai trò `GIAO_VIEN` và `GiaoVienId` đúng. Tên giáo viên trong danh mục chưa đồng nghĩa đã có tài khoản đăng nhập. Buổi phải được phân công thì mới xuất hiện.

Kiểm tra 08/10/2026: 153 tình huống HTTP SQL Server LocalDB đạt; giao diện đã thử phụ huynh có ba con, hai hồ sơ trùng tên, tài khoản con chỉ xem đúng hồ sơ, giáo viên ghi nhận xét/điểm và phụ huynh đọc đúng kết quả. Kiểm thử dùng database tạm, không tạo tài khoản con/giáo viên hoặc phiếu học tập giả vào database đang chạy.

## Bộ tài khoản dùng thử trên máy này

Sau yêu cầu tạo tài khoản mẫu, đã nạp bằng CLI `--seed-demo-learning`: giáo viên `gv_lhl_demo`, học viên `hv_baoan_demo`, `hv_minhkhang_demo`, `hv_khanhlinh_demo`. Mật khẩu lưu riêng tại `MyThuat.Api/.local/Tai_khoan_hoc_tap_mau.txt`, không đưa vào Git hoặc web public.

Ba học viên dùng hồ sơ mẫu đã có, có phiếu DA_XAC_NHAN trong lớp kiểm thử miễn phí. Phụ huynh mẫu cũ xem được cả ba con. Có điểm danh/nhận xét mẫu, buổi trong ngày nạp để giáo viên thử ghi và ví dụ học bù cho Minh Khang. Giáo viên mẫu mới được phân công riêng cho các lớp này.

Khóa và bản chụp chính sách kiểm thử giữ NHAP để không đưa vào danh mục bán hoặc thay quy định chung. Học phí 0; không tạo giao dịch tiền. Lệnh chạy lại giữ nguyên tài khoản/mật khẩu và những dữ liệu đã thử; không tự dời lịch cho ngày mới. Đây là dữ liệu minh họa theo yêu cầu, không phải hồ sơ học tập thật.

```powershell
dotnet run --project MyThuat.Api -c Release -- --seed-demo-learning
```

Nạp trên máy khác cần bộ mẫu từ Word đã có. Mặc định sinh mật khẩu ngẫu nhiên, đọc từ file `.local` trên máy nạp.
