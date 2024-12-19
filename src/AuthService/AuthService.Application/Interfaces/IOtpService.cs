using System;

namespace AuthService.Application.Interfaces;

public interface IOtpService
{
    string GenerateOtp();
    Task SendOtpAsync(string email, string otp);
}
