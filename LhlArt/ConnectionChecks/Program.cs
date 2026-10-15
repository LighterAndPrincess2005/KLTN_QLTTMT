using System.Net;
using System.Text.Json;
using LhlArt.Networking;

if(args.Contains("--live-schedule"))
{
    var name=Environment.GetEnvironmentVariable("LHL_CHECK_USER")??throw new Exception("Thiếu LHL_CHECK_USER");
    var password=Environment.GetEnvironmentVariable("LHL_CHECK_PASSWORD")??throw new Exception("Thiếu LHL_CHECK_PASSWORD");
    using var live=new ArtApiClient(baseUrl:Environment.GetEnvironmentVariable("LHL_CHECK_API_URL")??"http://127.0.0.1:5080");
    await live.Login(name,password);
    if(live.Role!="GIAO_VIEN")throw new Exception("Cần tài khoản giáo viên để kiểm tra lịch được phân quyền.");
    var schedule=await live.Read("api/lich-hoc");
    if(schedule.ValueKind!=JsonValueKind.Array||schedule.GetArrayLength()==0)throw new Exception("Không có lịch để kiểm tra.");
    var rosters=0;
    foreach(var session in schedule.EnumerateArray())
    {
        _=ArtApiClient.Get(session,"batDau").GetDateTime();
        _=ArtApiClient.Get(session,"trangThai").GetString();
        var id=ArtApiClient.Get(session,"id").GetInt64();
        var roster=await live.Read($"api/hoc-tap/buoi/{id}/danh-sach");
        if(roster.ValueKind!=JsonValueKind.Array)throw new Exception("Danh sách điểm danh sai định dạng.");
        foreach(var row in roster.EnumerateArray())
        {
            _=ArtApiClient.Get(row,"id").GetInt64();
            _=ArtApiClient.Get(row,"hoTen").GetString();
            _=ArtApiClient.Get(row,"rowVersion").GetString();
        }
        rosters++;
    }
    Console.WriteLine($"PASS: Lịch giáo viên có {schedule.GetArrayLength()} buổi; đọc được {rosters} danh sách điểm danh. Không ghi điểm danh hoặc thay đổi dữ liệu nghiệp vụ.");
    return;
}

var queue = new Queue<Func<HttpRequestMessage,HttpResponseMessage>>();
using var client = new ArtApiClient(new Handler(queue),"http://127.0.0.1:5080");
void Require(bool value,string message) {if(!value)throw new Exception(message);Console.WriteLine("PASS: "+message);}
HttpResponseMessage Json(string body,HttpStatusCode status=HttpStatusCode.OK)=>new(status){Content=new StringContent(body)};
queue.Enqueue(r=>Json("{\"accessToken\":\"test-token\",\"vaiTroChinh\":\"HOC_VU\",\"tenDangNhap\":\"hocvu\",\"quyen\":43}"));
await client.Login("hocvu","TestOnly123456!");
Require(client.Has(2)&&!client.Has(4),"Quyền học vụ không có thu tiền");
queue.Enqueue(r=>{Require(r.Headers.Authorization?.ToString()=="Bearer test-token","Gửi Bearer trên request dữ liệu");return Json("{\"duLieu\":[{\"id\":12}],\"tongSo\":1}");});
var records=await client.Read("api/quan-ly/hoc-vien");Require(ArtApiClient.Get(records,"duLieu")[0].GetProperty("id").GetInt32()==12,"Dữ liệu JSON còn đọc được sau khi response được giải phóng");
queue.Enqueue(r=>{Require(r.Headers.GetValues("If-Match").Single()=="AQID","Gửi RowVersion để tránh ghi đè hồ sơ");return Json("{\"thongBao\":\"Hồ sơ đã thay đổi\"}",HttpStatusCode.Conflict);});
try{await client.Put("api/quan-ly/hoc-vien/12",new{TrangThai="HOAT_DONG"},"AQID");throw new Exception("Expected conflict");}catch(InvalidOperationException ex){Require(ex.Message.Contains("Hồ sơ đã thay đổi"),"Hiển thị lỗi xung đột của API");}
queue.Enqueue(r=>Json("{\"message\":\"Hết phiên\"}",HttpStatusCode.Unauthorized));
try{await client.Read("api/tai-khoan/toi");throw new Exception("Expected expired token");}catch(InvalidOperationException ex){Require(ex.Message.Contains("Đăng nhập lại"),"Phiên hết hạn yêu cầu đăng nhập lại");}
queue.Enqueue(r=>Json("",HttpStatusCode.NotFound));
try{await client.Read("api/dang-ky/quan-ly");throw new Exception("Expected missing endpoint");}catch(ApiNotFoundException ex){Require(ex.Message.Contains("404")&&ex.Message.Contains("api/dang-ky/quan-ly"),"Phân biệt đường dẫn chưa hỗ trợ với lỗi mất kết nối API");}
queue.Enqueue(r=>{Require(r.Headers.Authorization is null,"Đăng nhập lại không gửi token cũ");return Json("{\"accessToken\":\"parent-token\",\"vaiTroChinh\":\"THANH_VIEN\",\"tenDangNhap\":\"parent\",\"quyen\":0}");});
try{await client.Login("parent","TestOnly123456!");throw new Exception("Expected parent rejection");}catch(InvalidOperationException ex){Require(ex.Message.Contains("nhân viên")&&client.Permissions==0,"Tài khoản phụ huynh không vào chế độ quản lý");}
Console.WriteLine("Connection checks completed without live database writes.");
sealed class Handler(Queue<Func<HttpRequestMessage,HttpResponseMessage>> queue):HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)=>Task.FromResult(queue.Dequeue()(request));
}
