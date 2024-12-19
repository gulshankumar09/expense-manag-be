using AuthService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // In a real application, implement email sending logic here
        // For development, we'll just log the email
        _logger.LogInformation("Sending email to: {To}, Subject: {Subject}, Body: {Body}", 
            to, subject, body);
        await Task.CompletedTask;
    }
} 