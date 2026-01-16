using Microsoft.Extensions.Options;
using MimeKit;
using Razor.Templating.Core;
using MailKit.Net.Smtp;
using Kernel.Application;

namespace Infrastructure;

public class MailkitService : IEmailService
{
    private readonly EmailSettings _settings;

    public MailkitService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(SendEmailRequest request)
    {
        // Render template HTML bằng Razor Templating
        string body = await RazorTemplateEngine.RenderAsync($"/EmailTemplates/{request.Template}.cshtml", request.Model);

        // Tạo message
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.From));
        message.To.Add(MailboxAddress.Parse(request.ToEmail));
        message.Subject = request.Subject;

        if (request.CcEmails != null)
        {
            foreach (var ccEmail in request.CcEmails)
            {
                message.Cc.Add(MailboxAddress.Parse(ccEmail));
            }
        }

        if (request.BccEmails != null)
        {
            foreach (var bccEmail in request.BccEmails)
            {
                message.Bcc.Add(MailboxAddress.Parse(bccEmail));
            }
        }

        var builder = new BodyBuilder { HtmlBody = body };
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl);
        await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
