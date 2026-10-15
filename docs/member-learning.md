# Khu học tập thành viên

Chọn **Học tập trên thanh trên cùng → học viên có lớp đã xác nhận → khóa/lớp**. `LearningStudentPicker.tsx` mở hộp chọn, chỉ hiện hồ sơ có phiếu DA_XAC_NHAN. `MemberArea.tsx` nhóm dữ liệu bằng `hocVienId` và có 5 mục: lịch/điểm danh, nhận xét giáo viên, học bù, đổi lịch/lớp và phản hồi. Dữ liệu lấy từ API thật qua `artCenterService`; không thêm dữ liệu học tập giả vào frontend.

Phiếu xác nhận và quyền đại diện cho đăng ký cho phép báo nghỉ, đề nghị học bù, đề nghị đổi lịch và chuyển lớp. API kiểm tra lại quyền, điều kiện, thời hạn và trùng yêu cầu. Đề nghị chỉ tạo hồ sơ chờ xử lý; trung tâm vẫn phải xếp lịch hoặc duyệt chuyển lớp. Các lịch đã xếp và câu trả lời hiện theo đúng nguồn API.

Phản hồi gửi từ web được lưu riêng cho phiếu đăng ký, không tự xuất bản vào mục cảm nhận trên trang chủ. Các yêu cầu học bù/đổi lịch lưu tương thích trong `PhanHoi`, service bỏ tiền tố dành riêng và đưa chúng về đúng tab.

Nhận xét từng buổi chỉ hiện nội dung đánh giá khi có thời điểm ghi của giáo viên/trung tâm (`ghiLuc`); lý do báo nghỉ không bị coi là nhận xét. Điểm chưa có hiện “Chưa có điểm sản phẩm”.

Tài khoản riêng của con dùng liên kết đã xác minh với đúng hồ sơ, quyền XEM_KET_QUA. Web đọc `canRegister` từ quyền đại diện và chặn bước đăng ký; server vẫn kiểm tra quyền. Phụ huynh có thể đại diện nhiều con, con chỉ đọc hồ sơ được liên kết.

`TeacherCorner.tsx` là trang `/#teacher`. Giáo viên đăng nhập tự vào trang này; có lịch được phân công, bộ lọc, danh sách điểm danh, nhận xét/điểm và hoàn thành buổi. Endpoint lịch giáo viên lọc trên server theo giáo viên gắn với tài khoản. Giáo viên không chỉnh được buổi tương lai; điều chỉnh muộn cần học vụ.

Thông tin hướng dẫn xử lý của trung tâm: project API có `HOC_TAP_THANH_VIEN.md`. Bản kiểm thử 08/10/2026: 153 tình huống HTTP SQL Server LocalDB đạt; trình duyệt thử ba con, hồ sơ trùng tên, tài khoản con, giáo viên ghi nhận xét và phụ huynh đọc đúng kết quả, năm mục học tập, bàn phím và màn hình 360–1440 px. Tất cả giao dịch kiểm thử dùng database tạm.

Database đang chạy đã bổ sung bộ tài khoản kiểm thử góc học tập theo yêu cầu: một giáo viên và ba tài khoản con, ba phiếu đã xác nhận trong lớp kiểm thử miễn phí. Phụ huynh mẫu cũ chọn được cả ba con. Từng con chỉ đọc hồ sơ của mình, giáo viên chỉ được phân công lớp kiểm thử riêng. Khóa kiểm thử không nằm trong danh mục bán công khai; không có giao dịch thu tiền giả. Mật khẩu nằm trong `.local` của API và hướng dẫn kiểm thử ngoài repo.
