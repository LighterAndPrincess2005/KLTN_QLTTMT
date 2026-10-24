using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views;

namespace LhlArt.Controllers;

// Consultation threads are separate from feedback/requests of enrolled students.
internal sealed class ApiConsultationController(ArtApiClient api)
{
    public async Task Open(IWin32Window owner,JsonElement row)
    {
        var id=ApiDataService.Id(row);
        var detail=await api.Read($"api/tu-van/{id}"); // Marks read and returns the fresh RowVersion.
        var messages=ArtApiClient.Get(detail,"tinNhan").EnumerateArray().Select(message=>
            $"{(ArtApiClient.Get(message,"tuNhanVien").GetBoolean()?"Trung tâm":"Khách hàng")} · {ArtApiClient.Text(message,"guiLuc")}\r\n{ArtApiClient.Text(message,"noiDung")}");
        using var form=new InputForm("Tư vấn · "+ArtApiClient.Text(row,"hoTen"));
        form.Add("Contact","Liên hệ",ArtApiClient.Text(row,"email")+" · "+ArtApiClient.Text(row,"dienThoai"),readOnly:true);
        form.Add("History","Cuộc trao đổi",string.Join("\r\n\r\n",messages),multiline:true,readOnly:true);
        if(ArtApiClient.Text(detail,"trangThai")=="DA_DONG"){
            form.Add("State","Trạng thái","Đã đóng · chỉ xem lịch sử",readOnly:true);form.ShowDialog(owner);return;
        }
        form.Add("Action","Thao tác","REPLY",[new("REPLY","Gửi phản hồi trên website"),new("DANG_XU_LY","Đánh dấu đang xử lý"),new("DA_DONG","Đóng yêu cầu")]);
        form.Add("Reply","Nội dung trả lời","",multiline:true);
        form.Changed("Action",()=>form.Enable("Reply",form.TextValue("Action")=="REPLY"));
        form.Add("Note","Kênh phản hồi","Phản hồi được lưu trên web. Chưa có dịch vụ gửi email; không tự gửi thư từ địa chỉ liên hệ.",readOnly:true,multiline:true);
        form.SaveAsync=async()=>{
            var version=ArtApiClient.Text(detail,"rowVersion");
            if(form.TextValue("Action")=="REPLY")await api.Post($"api/tu-van/{id}/tra-loi",new{NoiDung=ProfileRules.Required(form.TextValue("Reply"),"Nội dung",4000),RowVersion=version});
            else await api.Put($"api/tu-van/{id}/trang-thai",new{TrangThai=form.TextValue("Action"),RowVersion=version});
        };
        form.ShowDialog(owner);
    }
}
