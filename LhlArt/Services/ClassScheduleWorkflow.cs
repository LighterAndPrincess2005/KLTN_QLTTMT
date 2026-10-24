using LhlArt.Networking;

namespace LhlArt.Services;

// Keep the successful first step when publication fails: retry must not generate duplicate sessions.
internal sealed class ClassScheduleWorkflow(ArtApiClient api,long classId)
{
    public bool ScheduleCreated { get; private set; }
    public async Task Save(object request,Action<string> progress)
    {
        if(!ScheduleCreated)
        {
            progress("Đang kiểm tra trùng lịch và tạo các buổi học. Vui lòng chờ phản hồi API…");
            await api.Post($"api/lich-hoc/lop/{classId}/sinh-lich",request);
            ScheduleCreated=true;
        }
        // Publication remains subject to the API's existing administrator approval policy.
        if(api.Role!="QUAN_TRI")return;
        try
        {
            progress("Đã xếp lịch. Đang chuyển lớp sang Đang tuyển sinh…");
            var current=await api.Read($"api/quan-ly/lop-hoc/{classId}");
            if(ArtApiClient.Text(current,"trangThai")=="DANG_TUYEN_SINH")return;
            if(ArtApiClient.Text(current,"trangThai")!="NHAP")throw new InvalidOperationException("Trạng thái lớp đã thay đổi. Hãy tải lại danh sách.");
            var version=ArtApiClient.Text(current,"rowVersion");
            if(string.IsNullOrEmpty(version))throw new InvalidOperationException("API chưa trả RowVersion mới của lớp.");
            await api.Put($"api/quan-ly/lop-hoc/{classId}",new{TrangThai="DANG_TUYEN_SINH"},version);
        }
        catch(Exception ex) when(ex is not ApiSessionExpiredException)
        {
            throw new InvalidOperationException("Lịch học đã được lưu, nhưng chưa công bố lớp: "+ex.Message+"\nBấm Xác nhận để thử công bố lại; không tạo lại lịch.",ex);
        }
    }
}
