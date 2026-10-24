# WinForms nối API hiện tại

Nguồn giao diện: nhánh `giao-dien-bui-full`, commit `fdf058c28901c0d3d6abbe37e161df9af3fb8b7a`, thư mục `KLTN_QLTTMT/LhlArt`.
Chỉ nhập WinForms; không nhập bản sao `src`, `backend`, công cụ thiết lập tài khoản hoặc database từ nhánh đó.

## Chạy

API và database vẫn dùng bản hiện tại của nhóm. Không chạy công cụ đặt lại tài khoản/seed vì việc ráp giao diện không cần đổi dữ liệu.
`LhlArt/api.config.json` mặc định `http://127.0.0.1:5080/`. Đổi URL tại file này nếu API ở máy khác.

```powershell
dotnet run --project LhlArt/LhlArt.csproj -c Release
```

Ví dụ đăng nhập tài khoản mẫu hiện có: `hocvu01`, `thungan01`, `kho01`, `quantri01`; mật khẩu khởi tạo `Bui@2026` chỉ còn đúng nếu chưa tự đổi. Người học/phụ huynh dùng website. Không tự tạo hoặc reset tài khoản khi WinForms khởi động.

## Đã ráp

- Xác thực Bearer, phân quyền, lỗi hết phiên và If-Match/RowVersion.
- Hồ sơ người học/phụ huynh, hồ sơ học viên và đại diện; tên/ngày sinh bản thân và liên hệ khẩn cấp tự điền, vẫn sửa được trước khi lưu.
- Danh mục, khóa/lớp, lịch/điểm danh, họa cụ/kho và nghiệp vụ API sẵn có.
- Ưu đãi công khai `TuDongApDung` tách mã cá nhân; API tính tiền cuối cùng, WinForms không tự chốt mức giảm khác API.
- Ghi danh tại quầy, retry bằng idempotency key, đọc công nợ thực và thu tiền. Không đưa lớp hết chỗ vào lựa chọn mới.
- Tư vấn website `/api/tu-van` tách riêng phản hồi `/api/phan-hoi`: đọc, trả lời, đang xử lý, đóng; dùng RowVersion mới sau khi đọc. Không giả lập gửi email.
- Đổi mật khẩu bản thân, đăng nhập lại sau khi API thu hồi phiên.

## Giới hạn do API hiện tại (không sửa API trong lần này)

- Chưa có endpoint danh sách đăng ký dành nhân viên `GET /api/dang-ky/quan-ly`: WinForms cho tra cứu ID, chỉ liệt kê ID đã tra cứu/tạo trong phiên. Không đoán ID hoặc quét toàn database. Đóng ứng dụng sẽ mất danh sách ID cục bộ nhưng phiếu vẫn tồn tại trên server.
- Chưa lưu kênh đăng ký online/tại quầy: hiển thị chưa được ghi nhận, không tự gán dữ liệu lịch sử.
- Báo giá `/api/dang-ky/bao-gia` hiện yêu cầu tài khoản thành viên, không nhận người đại diện để nhân viên báo giá thay. WinForms hiển thị học phí gốc trước ghi danh và đọc công nợ sau khi API tạo phiếu; không tự giả định báo giá chính xác trước khi gửi.
- Quản lý bài viết và hồ sơ giáo viên công khai trên web chưa có màn biên tập riêng trong WinForms này. Danh mục giáo viên không đồng nghĩa tự công bố hồ sơ web.
- Chưa cấu hình email: trả lời tư vấn được lưu trên web, khách không đăng nhập cần trung tâm liên hệ qua thông tin đã để lại.

## Kiểm thử không dùng database chính

```powershell
dotnet build LhlArt/LhlArt.csproj -c Release
dotnet run --project LhlArt/ConnectionChecks -c Release
dotnet run --project LhlArt/IntegrationChecks -c Release -- --sqlserver
```

IntegrationChecks tạo SQL Server LocalDB tạm tên `LhlWinFormsChecks_*`, kiểm tra liên thông WinForms/web qua HTTP rồi dọn database tạm. Không dùng database `MyThuatDb`.

Kết quả lần ráp: build Release 0 lỗi/0 cảnh báo; 30 ConnectionChecks và 53 IntegrationChecks SQL Server thành công. Kiểm thử HTTP kiểm tra dữ liệu đọc lại qua endpoint web; kiểm thử bổ sung 25 trường hợp trên biểu mẫu Windows thật với API giả lập (không dùng database chính), gồm nhập 25/16 buổi, lỗi đỏ, nút xác nhận khi sai, tự sửa lỗi, payload, ảnh render, vị trí đăng xuất khi cuộn/đổi cỡ cửa sổ, logo tải lại và đăng xuất thực sự gọi API. Không thay thế việc người dùng kiểm tra mọi màn hình ở DPI thực tế.

## Tạo / sửa lớp và xếp lịch

- Chọn khóa tự điền số buổi và học phí; sai số buổi sẽ có chữ đỏ nêu rõ tên khóa, số đang nhập và số đúng.
- Chọn thứ cố định, ca sáng/chiều/tối, giờ bắt đầu và khoảng tuần. Chọn thứ tự căn ngày khai giảng tới ngày phù hợp; mô tả lịch được tạo tự động.
- Số buổi, sĩ số, học phí, thời điểm mở/đóng và khoảng cách 48 giờ tới buổi đầu được kiểm tra tại ô nhập. Lỗi API cũng giữ form và hiển thị màu đỏ thay vì chỉ đóng/mở hộp thông báo chung.
- Lưu lớp nháp trước; sau đó dùng Xếp lịch để chọn phòng/giáo viên/trợ giảng và tạo buổi thực tế. Thứ/ca/giờ dự kiến được nạp vào bước này. API vẫn kiểm tra ngày nghỉ, lịch phòng, giáo viên và học viên.
- Lớp đã có buổi thực tế không được sửa riêng mô tả thứ/giờ trên form lớp; phải dời buổi theo nghiệp vụ API để tránh sai lịch web/điểm danh. Khóa của lớp đã tạo được giữ cố định; tạo lớp mới nếu cần chuyển chương trình.
- Kiểm thử giao diện: `dotnet run --project LhlArt/UiChecks/LhlArt.UiChecks.csproj -c Release`. Thêm đường dẫn PNG ở sau `--` để lưu ảnh biểu mẫu lỗi (không hiển thị trên desktop).

Bản chạy cố định mới nhất: `LhlArt/.local/current/LhlArt.exe`. Đóng bản cũ sau khi xử lý nội dung chưa lưu rồi mở bản này; không tự tắt cửa sổ người dùng để cập nhật. Các lần build sau dùng lại đường dẫn này khi ứng dụng đã đóng. Logo chỉ tải lại dữ liệu API, không nạp lại mã giao diện.

Thanh bên tách logo cố định, menu cuộn riêng và khu vực tài khoản cố định ở đáy. Logo (cả hình/tên) hỗ trợ bấm hoặc dùng bàn phím để về Trang chủ và tải dữ liệu mới từ API. Đăng xuất/đổi mật khẩu không nằm trong vùng menu cuộn. Cửa sổ tự giới hạn trong vùng làm việc của màn hình khi mở để tránh đáy nằm ngoài màn hình thấp hoặc DPI lớn.

## Cuộn chuột và nút Lưu

Con lăn trên ô số, ngày/giờ và danh sách đang đóng chỉ cuộn biểu mẫu, không chỉnh giá trị. Vẫn nhập bàn phím hoặc bấm mũi tên tăng/giảm; khi chủ động mở danh sách, con lăn vẫn duyệt danh sách.

Form thêm/sửa lớp có **Lưu** (ghi và giữ form mở) cạnh **Xác nhận** (ghi rồi đóng). Lưu lần đầu ghi nhận ID và RowVersion trả về; các lần lưu sau cập nhật cùng lớp bằng PUT, không POST tạo trùng. Lỗi vẫn giữ nội dung và hiển thị màu đỏ. Kiểm thử giao diện tăng lên **32 trường hợp**, gồm thông điệp con lăn ở ô số con đang focus, cuộn form, nút tăng số, Lưu giữ mở và Xác nhận dùng RowVersion mới.

## Menu nhóm thu gọn

Menu gộp thành Hồ sơ & tư vấn, Đào tạo & lịch học, Đăng ký & học phí, Họa cụ & kho, Báo cáo và Quản trị. Chỉ hiển thị nhóm có chức năng tài khoản được phép dùng. Bấm tiêu đề bung/thu gọn; mở nhóm mới tự đóng nhóm trước. Mục đang xem được tô màu và nhóm chứa mục đó tự mở; về Trang chủ thu gọn tất cả. Logo, đổi mật khẩu và Đăng xuất vẫn cố định ngoài vùng cuộn.

Kiểm thử giao diện mới nhất: **41 trường hợp thành công**, bổ sung mở/đóng nhóm, chọn đúng mục con, phân quyền thu ngân/kho/giáo viên và menu thu gọn không cần cuộn dài ở Trang chủ. Đã render kiểm tra cả trạng thái đóng và mở nhóm.

## Xác nhận xếp lịch và tự mở tuyển sinh

Không giữ UseWaitCursor trên cửa sổ quản lý trong suốt thời gian mở dialog. Khi gửi, nút hiện Đang lưu và thông báo tiến trình xếp lịch / công bố; khóa gửi trùng, trả lại nút nếu API lỗi. Không giả định chuột xoay luôn đồng nghĩa SQL bị treo: timeout API vẫn báo lỗi để người dùng kiểm tra kết quả trước khi gửi lại.

Với QUAN_TRI, xếp lịch thành công sẽ đọc lại lớp lấy RowVersion mới và PUT trạng thái DANG_TUYEN_SINH. API tiếp tục kiểm tra đủ lịch, giáo viên, khóa công khai và hạn đóng đăng ký 48 giờ. Học vụ vẫn chỉ tạo lịch theo phân quyền hiện có, không vượt quyền công bố. Nếu lịch đã lưu nhưng công bố lỗi, form nói rõ và khóa thông số lịch; Xác nhận lại chỉ thử công bố, không tạo thêm buổi. Nếu đóng form lúc đó, có thể sửa trạng thái lớp trong màn Cập nhật lớp để công bố lịch đã lưu.

Kết quả mới nhất: build 0 lỗi / 0 cảnh báo, **49 kiểm thử giao diện** và **54 kiểm thử liên thông SQL Server** thành công, gồm tự chuyển trạng thái và đọc lại lớp từ web. Chỉ sử dụng database kiểm thử tạm, không sửa dữ liệu chính.

## Tiêu đề tiếng Việt dùng chung

ApiColumnLabels cung cấp tên có dấu cho 240 trường dữ liệu từ model API, dùng chung cho bảng danh sách và Xem chi tiết. Các trạng thái lớp, tư vấn và lượt ưu đãi được dịch ở bước hiển thị; tên trường gửi API, mã định danh và dữ liệu database không thay đổi. UiChecks kiểm tra bao phủ nhãn trên toàn bộ trường scalar trong thư mục Models để phát hiện trường mới chưa có bản dịch.

## Ghi nhớ đăng nhập trên Windows

Màn đăng nhập có tùy chọn Ghi nhớ đăng nhập trên máy này, mặc định tắt. Chỉ sau khi API xác thực thành công mới lưu tên và mật khẩu vào Windows Credential Manager của người dùng Windows hiện tại, tách theo địa chỉ API. Không ghi mật khẩu/token vào JSON, repository hay nhật ký.

Mở ứng dụng lần sau tự gửi thông tin đã lưu tới API để xác thực lại; không giữ quyền offline hoặc bỏ qua kiểm tra tài khoản. Tắt ứng dụng bằng nút đóng giữ ghi nhớ. Đăng xuất, đổi mật khẩu thành công hoặc phiên hết hạn khiến ứng dụng quay lại đăng nhập sẽ xóa ghi nhớ. API từ chối mật khẩu đã lưu cũng xóa thông tin cũ; lỗi mạng chỉ hiện lỗi để thử lại, không đăng nhập vòng lặp. Có thể bỏ chọn ghi nhớ trên màn đăng nhập. Chỉ dùng tùy chọn này trên máy Windows cá nhân hoặc máy có tài khoản Windows riêng.

Kiểm thử giao diện mới nhất: 61 trường hợp thành công, gồm lưu/đọc/xóa credential thử trong kho Windows (đã dọn sau kiểm tra), tách theo API, tự gửi đăng nhập một lần và che mật khẩu. API/backend/database không thay đổi trong cập nhật này.
