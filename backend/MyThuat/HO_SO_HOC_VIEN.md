# Xóa hồ sơ học viên chưa sử dụng

`GET /api/hoc-vien/toi` trả thêm `canXoa` cho từng hồ sơ của thành viên đang đăng nhập.

`DELETE /api/hoc-vien/toi/{hocVienId}` rút hồ sơ chờ xác minh khỏi danh sách. Thành viên phải là đại diện chính duy nhất, có quyền đầy đủ và quan hệ còn hiệu lực. Hồ sơ phải đang chờ xác nhận, chưa có xác nhận đại diện, người xác nhận, giáo viên được chỉ định đánh giá, ngày/nhận xét/kết quả đầu vào hoặc lượt theo học. Hồ sơ đã xác nhận hay có khóa học bị chặn.

Kiểm tra lại các điều kiện trong giao dịch Serializable; không dựa riêng vào trạng thái nút trên web. Không có quyền truy cập hồ sơ của người khác. Trả 401 nếu chưa đăng nhập, 403 với tài khoản không liên kết thành viên, 404 nếu không có hồ sơ còn hiệu lực của người gọi, 409 nếu hồ sơ không còn đủ điều kiện rút.

Thao tác thành công trả 204, đổi `HocVien.TrangThai` thành `NGUNG`, kết thúc hiệu lực quan hệ đại diện và ghi `RUT_HO_SO` vào nhật ký. Không xóa vật lý dữ liệu hoặc lịch sử. Không đổi schema DB và không ảnh hưởng hồ sơ đã đăng ký học.

Kiểm tra: 115 tình huống HTTP qua SQL Server LocalDB tạm, gồm quyền sở hữu, tài khoản giáo viên, hồ sơ đã xác minh, đánh giá đầu vào, có lượt theo học, nhiều đại diện và quan hệ hết hiệu lực. Trên trình duyệt đã kiểm tra tạo/rút hồ sơ, hủy xác nhận xóa, hồ sơ biến mất sau tải lại, giữ hồ sơ khi API từ chối và khoảng cách nút trên màn hình 360–1440 px. Dữ liệu thử được tạo trong DB tạm; không xóa hồ sơ thật.
