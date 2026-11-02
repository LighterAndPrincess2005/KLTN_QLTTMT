using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public interface INotificationMail{Task Send(ThongBao message,CancellationToken ct);}
public sealed class NotificationMail(IConfiguration config):INotificationMail
{
    public async Task Send(ThongBao m,CancellationToken ct)
    {
        var s=config.GetSection("RecoveryMail");
        if(!s.GetValue<bool>("Enabled")||string.IsNullOrWhiteSpace(s["Username"])||string.IsNullOrWhiteSpace(s["AppPassword"]))throw new InvalidOperationException("Chưa cấu hình email.");
        using var smtp=new SmtpClient("smtp.gmail.com",587){EnableSsl=true,UseDefaultCredentials=false,Credentials=new NetworkCredential(s["Username"],s["AppPassword"])};
        using var mail=new MailMessage(new MailAddress(s["Username"]!,"Mỹ Thuật Bụi"),new MailAddress(m.Email!)){Subject=m.TieuDe,SubjectEncoding=Encoding.UTF8,BodyEncoding=Encoding.UTF8};
        var text=m.NoiDung+"\n\nMỹ Thuật Bụi · Thông tin gửi riêng cho người nhận.\nĐây là email thông báo tự động.";
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text,Encoding.UTF8,"text/plain"));
        var html="<!doctype html><html lang='vi'><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1'><body style='margin:0;background:#f3f0e7;padding:24px 12px;font:15px Arial,sans-serif;color:#274739'><div style='max-width:580px;margin:auto;background:#fffdf8;border:1px solid #dddccd;border-radius:18px;overflow:hidden'><div style='padding:24px;background:#194f43;color:white;font-size:22px;font-weight:bold'>Mỹ Thuật Bụi</div><div style='padding:28px'><h1 style='font-size:24px;line-height:1.5;margin:0 0 20px'>"+WebUtility.HtmlEncode(m.TieuDe)+"</h1><div style='line-height:1.9;overflow-wrap:anywhere'>"+WebUtility.HtmlEncode(m.NoiDung).Replace("\n","<br>")+"</div></div><div style='padding:20px;background:#edf3e9;font-size:12px;line-height:1.8'>Thông tin gửi riêng cho người nhận · Email thông báo tự động.</div></div></body></html>";
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html,Encoding.UTF8,"text/html"));
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct);timeout.CancelAfter(TimeSpan.FromSeconds(25));
        await smtp.SendMailAsync(mail,timeout.Token);
    }
}
public sealed class NotificationWorker(IServiceScopeFactory scopes,ILogger<NotificationWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested){
            try{await Deliver(stoppingToken);}catch(OperationCanceledException)when(stoppingToken.IsCancellationRequested){break;}
            catch(Exception e){logger.LogWarning("Notification worker: {ErrorType}",e.GetType().Name);}
            try{await Task.Delay(TimeSpan.FromSeconds(30),stoppingToken);}catch(OperationCanceledException){break;}
        }
    }
    public async Task Deliver(CancellationToken ct)
    {
        using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<MyThuatDbContext>();var now=BusinessClock.Now;
        var rows=await db.Set<ThongBao>().Where(x=>((x.TrangThaiEmail=="CHO_GUI"&&x.SoLanGui<3)||x.TrangThaiEmail=="DANG_GUI")&&(x.ThuLaiLuc==null||x.ThuLaiLuc<=now)).OrderBy(x=>x.Id).Take(10).ToListAsync(ct);
        foreach(var row in rows){
            // Optimistic concurrency claims a five-minute lease, including recovery after a process crash.
            row.TrangThaiEmail="DANG_GUI";row.ThuLaiLuc=now.AddMinutes(5);row.SoLanGui++;
            try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){db.Entry(row).State=EntityState.Detached;continue;}
            try{await scope.ServiceProvider.GetRequiredService<INotificationMail>().Send(row,ct);row.TrangThaiEmail="DA_GUI";row.GuiEmailLuc=BusinessClock.Now;row.ThuLaiLuc=null;}
            catch(OperationCanceledException)when(ct.IsCancellationRequested){throw;}
            catch(Exception e){row.TrangThaiEmail=row.SoLanGui>=3?"GUI_LOI":"CHO_GUI";row.ThuLaiLuc=BusinessClock.Now.AddMinutes(5);logger.LogWarning("Notification {Id}: {ErrorType}",row.Id,e.GetType().Name);}
            await db.SaveChangesAsync(ct);
        }
    }
}
