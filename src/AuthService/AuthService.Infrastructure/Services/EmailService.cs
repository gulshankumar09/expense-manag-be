using AuthService.Application.Configuration;
using AuthService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AuthService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port);
            client.EnableSsl = _emailSettings.EnableSsl;
            client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to: {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to: {To}", to);
            throw;
        }
    }
}