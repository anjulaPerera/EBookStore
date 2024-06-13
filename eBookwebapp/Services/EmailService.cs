using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using eBookwebapp.Models;

public class EmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, false);
            await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
