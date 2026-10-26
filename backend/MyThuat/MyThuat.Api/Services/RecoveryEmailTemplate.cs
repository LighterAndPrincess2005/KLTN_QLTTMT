using System.Text.RegularExpressions;

namespace MyThuat.Api.Services;

public sealed record RecoveryEmailContent(string Subject,string PlainText,string Html);

public static class RecoveryEmailTemplate
{
    public const string LogoResource="MyThuat.Email.Logo.jpg";
    public const string LogoContentId="bui-brand-logo";
    public static RecoveryEmailContent Create(string code,bool verify=false)
    {
        if(!Regex.IsMatch(code,"^[0-9]{6}$"))throw new ArgumentException("Mã email phải gồm 6 chữ số.",nameof(code));
        var title=verify?"Xác minh email của bạn":"Đặt lại mật khẩu";
        var action=verify?"xác minh email":"đặt lại mật khẩu";
        var intro=verify?"Dùng mã dưới đây để xác minh email tài khoản của bạn.":"Chúng tôi nhận được yêu cầu đặt lại mật khẩu tài khoản của bạn. Dùng mã dưới đây để tiếp tục.";
        var plain=$"Mỹ Thuật Bụi\n{title}\n\nChào bạn,\n{intro}\n\nMã xác nhận: {code}\n\nMã có hiệu lực 10 phút và chỉ dùng một lần. Quay lại màn hình vừa yêu cầu trên web hoặc WinForms để nhập mã. Không chia sẻ mã cho bất kỳ ai.\n\nNếu bạn không yêu cầu {action}, hãy bỏ qua email này.\nEmail tự động từ hệ thống Mỹ Thuật Bụi.";
        var html=$$"""
        <!doctype html>
        <html lang="vi"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><meta name="color-scheme" content="light"><title>{{title}} · Mỹ Thuật Bụi</title>
        <style>@media only screen and (max-width:600px){.outer-pad{padding:16px 10px!important}.email-content{padding:28px 22px!important}.brand-pad{padding:24px 22px!important}.email-title{font-size:27px!important}.otp-code{font-size:32px!important;letter-spacing:7px!important}.otp-pad{padding:24px 12px!important} }</style></head>
        <body style="margin:0;padding:0;background-color:#f3f0e7;color:#263d35;font-family:Arial,Helvetica,sans-serif;-webkit-text-size-adjust:100%;">
        <div style="display:none;font-size:1px;line-height:1px;color:#f3f0e7;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">Mã bảo mật dùng một lần cho yêu cầu {{action}}. Có hiệu lực trong 10 phút.</div>
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f3f0e7;"><tr><td align="center" class="outer-pad" style="padding:40px 16px;">
        <table role="presentation" width="600" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:600px;border:1px solid #e3dfd3;border-radius:22px;background-color:#fffdf8;overflow:hidden;">
        <tr><td class="brand-pad" style="padding:28px 36px;background-color:#194f43;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0"><tr>
          <td width="64" valign="middle" style="width:64px;"><img src="cid:{{LogoContentId}}" alt="Mỹ Thuật Bụi" width="60" height="60" style="display:block;width:60px;height:60px;border:0;border-radius:14px;background-color:#fffdf8;"></td>
          <td valign="middle" style="padding-left:16px;"><p style="margin:0;color:#fffdf8;font-size:21px;line-height:1.4;font-weight:700;">Mỹ Thuật Bụi</p><p style="margin:5px 0 0;color:#d5e3d7;font-size:12px;line-height:1.5;">Học vẽ cùng Bụi</p></td>
          <td width="32" align="right" valign="top" style="width:32px;color:#f4c67a;font-size:26px;line-height:1;">✦</td>
          </tr></table>
        </td></tr>
        <tr><td class="email-content" style="padding:34px 36px 30px;">
          <p style="margin:0 0 12px;font-size:11px;line-height:1.5;font-weight:700;letter-spacing:1.6px;color:#ad4d2b;">BẢO MẬT TÀI KHOẢN</p>
          <h1 class="email-title" style="margin:0 0 22px;color:#194f43;font-size:30px;line-height:1.25;font-weight:700;">{{title}}</h1>
          <p style="margin:0 0 10px;font-size:15px;line-height:1.8;">Chào bạn,</p>
          <p style="margin:0 0 26px;color:#52665d;font-size:14px;line-height:1.8;">{{intro}}</p>
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f2f5ec;border:1px dashed #aec5b5;border-radius:16px;">
          <tr><td align="center" class="otp-pad" style="padding:25px 18px;">
            <p style="margin:0 0 15px;color:#5c7164;font-size:11px;line-height:1.4;font-weight:700;letter-spacing:2px;">MÃ XÁC NHẬN CỦA BẠN</p>
            <p class="otp-code" dir="ltr" style="margin:0;color:#194f43;font-family:Consolas,'Courier New',monospace;font-size:42px;font-weight:700;line-height:1.25;letter-spacing:10px;white-space:nowrap;">{{code}}</p>
            <p style="margin:16px 0 0;color:#536a59;font-size:12px;line-height:1.7;">Hiệu lực <strong>10 phút</strong> · Chỉ sử dụng <strong>1 lần</strong></p>
          </td></tr></table>
          <p style="margin:22px 0;color:#52665d;font-size:13px;line-height:1.8;">Quay lại màn hình <strong style="color:#194f43;">Quên mật khẩu</strong> trên website hoặc ứng dụng WinForms và nhập mã này để tiếp tục. Bạn có thể sao chép trực tiếp 6 chữ số ở trên.</p>
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#fff3e5;border-radius:12px;"><tr><td style="padding:15px 18px;border-left:3px solid #cf8151;">
            <p style="margin:0 0 5px;color:#874221;font-size:13px;font-weight:700;line-height:1.6;">Giữ mã xác nhận riêng tư</p>
            <p style="margin:0;color:#725542;font-size:12px;line-height:1.8;">Không chia sẻ mã cho bất kỳ ai, kể cả người tự xưng là nhân viên trung tâm.</p>
          </td></tr></table>
          <p style="margin:23px 0 0;color:#6d796e;font-size:12px;line-height:1.8;">Nếu bạn không yêu cầu {{action}}, hãy bỏ qua email này. Không cần trả lời thư.</p>
        </td></tr>
        <tr><td align="center" style="padding:20px 24px;border-top:1px solid #e9e5dc;background-color:#faf8f0;"><p style="margin:0;color:#466151;font-size:12px;line-height:1.6;font-weight:700;">Mỹ Thuật Bụi</p><p style="margin:5px 0 0;color:#7c8578;font-size:11px;line-height:1.7;">Email tự động về bảo mật tài khoản</p></td></tr>
        </table>
        <p style="margin:18px 0 0;color:#838a7d;font-size:11px;line-height:1.7;">Một chút màu. Một chút bạn.</p>
        </td></tr></table></body></html>
        """;
        return new($"{title} Mỹ Thuật Bụi",plain,html);
    }
    public static Stream OpenLogo()=>typeof(RecoveryEmailTemplate).Assembly.GetManifestResourceStream(LogoResource)
        ??throw new InvalidOperationException("Thiếu logo nhúng của email.");
    public static async Task WritePreview(string root)
    {
        var content=Create("000000");using var logo=OpenLogo();using var memory=new MemoryStream();await logo.CopyToAsync(memory);
        var html=content.Html.Replace("cid:"+LogoContentId,"data:image/jpeg;base64,"+Convert.ToBase64String(memory.ToArray()));
        html=html.Replace("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"background-color:#f3f0e7;\">",
            "<p style=\"margin:0;padding:12px;text-align:center;font:12px Arial;color:#874221;\">BẢN XEM TRƯỚC · MÃ 000000 CHỈ ĐỂ MINH HỌA</p><table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"background-color:#f3f0e7;\">");
        var folder=Path.Combine(root,".local","previews");Directory.CreateDirectory(folder);var file=Path.Combine(folder,"recovery-email.html");await File.WriteAllTextAsync(file,html);Console.WriteLine("Đã tạo bản xem trước với mã minh họa: "+file);
    }
}
