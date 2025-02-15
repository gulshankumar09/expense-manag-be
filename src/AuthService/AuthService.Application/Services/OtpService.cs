using AuthService.Application.Interfaces;
using SharedLibrary.Interfaces;

namespace AuthService.Application.Services;

public class OtpService : IOtpService
{
    private readonly IEmailService _emailService;
    private readonly Random _random;

    public OtpService(IEmailService emailService)
    {
        _emailService = emailService;
        _random = new Random();
    }

    public string GenerateOtp()
    {
        return _random.Next(100000, 999999).ToString();
    }

    public async Task SendOtpAsync(string email, string otp)
    {
        var subject = "Verify your email";
        var body = $"Your verification code is: {otp}";
        await _emailService.SendEmailAsync(email, subject, body);
    }
}