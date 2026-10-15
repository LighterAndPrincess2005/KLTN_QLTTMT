# Đối chiếu yêu cầu — LHL ART

Nguồn: đề cương `CNTT-KLCN082_ManhThienLy(20261008-153630).pdf`, mục 5–6 và thang điểm ở trang 2–4; ảnh yêu cầu người dùng gửi ngày 08/10/2026. Tên giảng viên và thông tin của đề cương chỉ là thông tin đối chiếu, không đưa vào giao diện sản phẩm.

## Kết luận

Có phần triển khai cho tất cả nhóm chức năng WinForms được liệt kê trong ảnh và đề cương, ở mức chức năng cơ bản. Điều này không đồng nghĩa đã hoàn thành toàn bộ đề tài. Gói hiện tại chưa chứa Website, API hay cơ sở dữ liệu SQL dùng chung, và một số nghiệp vụ chi tiết của báo cáo vẫn còn thiếu.

Ảnh tách quản lý học viên và giáo viên thành hai dòng; PDF gộp thành một nhóm. Hai cách liệt kê không làm thay đổi phạm vi chức năng.

## Phần mềm Windows

| Yêu cầu | Chức năng hiện có | Điểm còn giới hạn |
|---|---|---|
| Loại khóa, khóa học, lớp học | Thêm/sửa/xóa danh mục có kiểm tra liên kết; tạo lịch, duyệt tuyển sinh, chốt khai giảng | Chưa có ngừng sử dụng danh mục; hoãn/hủy lớp và kiểm tra lịch rảnh/chuyên môn đầy đủ |
| Thông tin học viên | Hồ sơ, đại diện, liên hệ, xác minh, nhu cầu tư vấn; tìm kiếm | Chưa tách tài khoản thành viên và quan hệ nhiều học viên theo mô hình Website |
| Thông tin giáo viên | Hồ sơ tên, điện thoại, chuyên môn; phân công lớp và liên kết tài khoản | Chưa quản lý bằng cấp, lịch rảnh và toàn bộ lịch sử phân công |
| Các loại họa cụ | Danh mục, đơn vị, giá, ngưỡng tồn, phiếu nhập/xuất, bán hàng và tồn kho | Chưa có nhà cung cấp, lô/hạn dùng, dự toán mua, thu hồi dụng cụ và kiểm kê đầy đủ |
| Đăng ký, lịch và theo dõi khóa học | Giữ chỗ; thu học phí; đổi lịch; điểm danh, nhận xét; nhập và duyệt kết quả; yêu cầu hủy/chuyển | Chưa có học bù; chuyển sau khai giảng, chuyển có hoàn chênh lệch và chuyển tiếp chưa được hỗ trợ |
| Khuyến mãi, ưu đãi | Danh mục ưu đãi học phí/họa cụ; phần trăm, trần giảm, kỳ áp dụng và số lượt | Quy tắc áp dụng theo phần đã cài đặt; chưa có quản trị chiến dịch nâng cao |
| Tìm kiếm, thống kê, báo cáo | Tìm theo các cột; lọc giao dịch theo ngày; tổng hợp lớp; báo cáo họa cụ; xuất CSV; biểu đồ 6 tháng | Chưa có bộ lọc kết hợp cho mọi danh mục, bộ báo cáo in đa dạng hoặc đồng bộ báo cáo từ Website |
| Đăng nhập, phân quyền | 5 vai trò; đổi mật khẩu; quản lý tài khoản; khóa/mở; đặt lại mật khẩu; nhật ký | Phân quyền ứng dụng một máy, chưa phải xác thực API nhiều người dùng |

## Phần còn thiếu để đáp ứng toàn đề cương

| Yêu cầu của toàn hệ thống | Tình trạng trong gói này |
|---|---|
| Website giới thiệu và tìm kiếm khóa | Chưa có |
| Đăng ký thành viên, nhận tư vấn và ưu đãi trên Website | Chưa có |
| Đăng ký khóa và thanh toán trên Website | Chưa có; thu tiền hiện nằm trong WinForms |
| Nhận lịch, theo dõi khóa và phản hồi trên Website | Chưa có |
| API dùng chung | Chưa tích hợp |
| SQL Server/MySQL và ràng buộc dữ liệu | Chưa tích hợp; hiện dùng JSON cục bộ |
| Đồng bộ Windows–Website | Chưa có |
| Kiểm thử giao diện và triển khai trên Windows | Đã kiểm tra biên dịch, chưa chạy kiểm tra trực quan trong môi trường bàn giao |

Các yêu cầu tài liệu/UML, cơ sở dữ liệu và thuyết trình trong thang điểm nằm ngoài phạm vi gói mã nguồn WinForms này.

## Tài khoản và vai trò

Bản cài mới chỉ yêu cầu tạo một tài khoản quản trị ban đầu. Đây không phải ứng dụng chỉ dành cho admin. Quản lý vào **Quản trị → Tài khoản nhân viên → Thêm tài khoản** để tạo tài khoản khác, tự đặt mật khẩu và chọn vai trò.

| Vai trò | Phạm vi chính |
|---|---|
| Quản lý | Toàn bộ chức năng; phê duyệt lớp/yêu cầu/kết quả; quản lý tài khoản, ưu đãi, nhật ký, sao lưu |
| Học vụ | Danh mục đào tạo, học viên/giáo viên, đăng ký, lịch, điểm danh/kết quả, tiếp nhận yêu cầu; xem ưu đãi |
| Kế toán | Thu–hoàn học phí, hoàn tất yêu cầu đã duyệt; xem giao dịch, báo cáo và biểu đồ |
| Giáo viên | Lịch, điểm danh, nhận xét và kết quả trong phạm vi lớp được phân công |
| Phụ trách họa cụ | Danh mục, nhập/xuất, bán họa cụ, tồn và lịch sử; xem ưu đãi |

Tài khoản giáo viên phải liên kết một hồ sơ giáo viên có sẵn. Không cho hai tài khoản giáo viên dùng chung một hồ sơ. Tài khoản đã khóa không đăng nhập được. Không cho tự khóa hoặc tự đổi vai trò của tài khoản đang đăng nhập; không xóa tài khoản để giữ lịch sử. Mật khẩu băm không hiển thị trong bảng hoặc CSV tài khoản.

## Bố cục đã chỉnh trong lần này

- Chia đều thẻ thống kê ở từng hàng, không để ô cuối hàng trống.
- Tổng quan thay đổi theo vai trò: lịch học, giao dịch kế toán hoặc phiếu kho.
- Thêm nút mở nhanh và thanh sĩ số/tồn kho có số liệu.
- Điểm màu tại phần dư bên phải thanh thao tác khi còn đủ chỗ; không đè lên nút.
- Tính chiều cao thanh nút theo kích thước điều khiển và DPI, thay cho số hàng ước lượng cố định.
- Căn riêng hai dòng tiêu đề/chi tiết; vùng tiền tự dành thêm chiều rộng.
- Biểu đồ thu và hoàn học phí, bán họa cụ: dùng số giao dịch trong sáu tháng gần nhất tới thời điểm hiện tại; không cộng giá trị chuyển lớp thành tiền thu mới. Rê chuột lên cột để xem số tiền.
- Giữ logo, icon và màu sắc nhận diện; không đưa thông tin giảng viên, đề cương hoặc dữ liệu giả vào phần trang trí sản phẩm.

## Kiểm tra và giới hạn xác minh

Đã biên dịch toàn bộ mã giao diện bằng Roslyn .NET 8 cùng thư viện Windows Forms. Có 51 kiểm tra nghiệp vụ đạt, bao gồm các luồng cũ, cài mới trống, giờ Việt Nam, quản lý tài khoản, chặn quyền sai và số liệu biểu đồ.

Môi trường kiểm tra là Linux. Chưa mở cửa sổ thực trên Windows để xác minh bố cục ở 100%, 125%, 150% hoặc các độ phân giải khác. Các sửa bố cục được rà soát theo mã nguồn và lỗi đã thấy trong ảnh ở lần trước; không xem đây là kết quả kiểm thử giao diện Windows đầy đủ.
