using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using SharedLibrary.Configuration;
using SharedLibrary.Interfaces;
using SharedLibrary.Exceptions;
using SharedLibrary.Constants;

namespace SharedLibrary.Services;

/// <summary>
/// Implementation of the email service using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        ValidateConfiguration();
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(_emailSettings.FromEmail))
            throw EmailException.InvalidConfiguration(
                ErrorConstants.Messages.Email.MissingFromEmail,
                new { Field = "FromEmail" });

        if (string.IsNullOrEmpty(_emailSettings.Host))
            throw EmailException.InvalidConfiguration(
                ErrorConstants.Messages.Email.MissingSmtpHost,
                new { Field = "Host" });

        if (_emailSettings.Port <= 0)
            throw EmailException.InvalidConfiguration(
                ErrorConstants.Messages.Email.InvalidSmtpPort,
                new { Field = "Port" });

        if (string.IsNullOrEmpty(_emailSettings.Username) || string.IsNullOrEmpty(_emailSettings.Password))
            throw EmailException.InvalidConfiguration(
                ErrorConstants.Messages.Email.MissingCredentials,
                new { Field = "Credentials" });
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await SendEmailAsync(new[] { to }, subject, body);
    }

    public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body)
    {
        var recipients = to.ToList();

        try
        {
            if (!recipients.Any())
                throw EmailException.InvalidRecipient(ErrorConstants.Messages.Email.EmptyRecipient);

            if (recipients.Count > _emailSettings.MaxRecipientsPerEmail)
                throw EmailException.RateLimitExceeded(
                    ErrorConstants.Messages.Email.TooManyRecipients,
                    new { MaxRecipients = _emailSettings.MaxRecipientsPerEmail });

            foreach (var recipient in recipients)
            {
                if (!IsValidEmail(recipient))
                    throw EmailException.InvalidRecipient(
                        ErrorConstants.Messages.Email.InvalidFormat,
                        new { EmailAddress = recipient });
            }

            using var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);

            foreach (var recipient in recipients)
            {
                message.To.Add(new MailAddress(recipient));
            }

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = CreateSmtpClient();

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to: {Recipients}", string.Join(", ", recipients));
            }
            catch (SmtpException ex)
            {
                HandleSmtpException(ex);
            }
        }
        catch (EmailException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to: {Recipients}", string.Join(", ", recipients));
            throw EmailException.SendingError(
                ErrorConstants.Messages.Email.SendingError,
                new { Error = ex.Message });
        }
    }

    public async Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IDictionary<string, byte[]> attachments)
    {
        try
        {
            if (!IsValidEmail(to))
                throw EmailException.InvalidRecipient(
                    ErrorConstants.Messages.Email.InvalidFormat,
                    new { EmailAddress = to });

            using var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            foreach (var attachment in attachments)
            {
                var attachmentSize = attachment.Value.Length;
                if (attachmentSize > _emailSettings.MaxAttachmentSize)
                    throw EmailException.AttachmentError(
                        ErrorConstants.Messages.Email.AttachmentTooLarge,
                        new { FileName = attachment.Key, Size = attachmentSize, MaxSize = _emailSettings.MaxAttachmentSize });

                using var stream = new MemoryStream(attachment.Value);
                message.Attachments.Add(new Attachment(stream, attachment.Key));
            }

            using var client = CreateSmtpClient();

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email with attachments sent successfully to: {To}", to);
            }
            catch (SmtpException ex)
            {
                HandleSmtpException(ex);
            }
        }
        catch (EmailException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachments to: {To}", to);
            throw EmailException.SendingError(
                ErrorConstants.Messages.Email.SendingError,
                new { Error = ex.Message });
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
        {
            EnableSsl = _emailSettings.EnableSsl,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            Timeout = _emailSettings.TimeoutSeconds * 1000 // Convert to milliseconds
        };

        return client;
    }

    private void HandleSmtpException(SmtpException ex)
    {
        var details = new { ex.StatusCode, Source = ex.Source };

        switch (ex.StatusCode)
        {
            case SmtpStatusCode.ClientNotPermitted:
                throw EmailException.AuthenticationError(
                    ErrorConstants.Messages.Email.AuthenticationError,
                    details);

            case SmtpStatusCode.MailboxBusy:
            case SmtpStatusCode.MailboxUnavailable:
                throw EmailException.RateLimitExceeded(
                    ErrorConstants.Messages.Email.MailboxUnavailable,
                    details);

            case SmtpStatusCode.GeneralFailure when ex.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase):
                throw EmailException.SmtpConnectionError(
                    ErrorConstants.Messages.Email.SslRequired,
                    details);

            default:
                throw EmailException.SendingError(
                    ErrorConstants.Messages.Email.SendingError,
                    details);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}