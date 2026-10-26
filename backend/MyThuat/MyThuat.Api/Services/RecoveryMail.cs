using System.Net;
using System.Net.Mail;

namespace MyThuat.Api.Services;

public interface IRecoveryMail
{
    void EnsureConfigured();
    Task Send(string recipient,string code,bool verify,CancellationToken ct);
}
public sealed class RecoveryMail(IConfiguration config):IRecoveryMail
{
    public void EnsureConfigured()
    {
        var s=config.GetSection("RecoveryMail");
        ApiError.Require(s.GetValue<bool>("Enabled")&&!string.IsNullOrWhiteSpace(s["Username"])&&!string.IsNullOrWhiteSpace(s["AppPassword"]),
            "Chưa bật gửi email khôi phục. Trung tâm cần cấu hình Gmail trước khi sử dụng.",503);
    }
    public async Task Send(string recipient,string code,bool verify,CancellationToken ct)
    {
        EnsureConfigured();var s=config.GetSection("RecoveryMail");
        var content=RecoveryEmailTemplate.Create(code,verify);
        // Fixed Gmail endpoint prevents credentials being sent to an arbitrary host from input.
        using var smtp=new SmtpClient("smtp.gmail.com",587){EnableSsl=true,UseDefaultCredentials=false,Credentials=new NetworkCredential(s["Username"],s["AppPassword"])};
        using var mail=new MailMessage(new MailAddress(s["Username"]!,"Mỹ Thuật Bụi"),new MailAddress(recipient)){
            Subject=content.Subject,
            BodyEncoding=System.Text.Encoding.UTF8,SubjectEncoding=System.Text.Encoding.UTF8};
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(content.PlainText,System.Text.Encoding.UTF8,"text/plain"));
        var htmlView=AlternateView.CreateAlternateViewFromString(content.Html,System.Text.Encoding.UTF8,"text/html");
        var logo=new LinkedResource(RecoveryEmailTemplate.OpenLogo(),"image/jpeg"){ContentId=RecoveryEmailTemplate.LogoContentId,TransferEncoding=System.Net.Mime.TransferEncoding.Base64};
        htmlView.LinkedResources.Add(logo);mail.AlternateViews.Add(htmlView);
        await smtp.SendMailAsync(mail,ct);
    }
}
