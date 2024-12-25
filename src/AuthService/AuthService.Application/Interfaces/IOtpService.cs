using System;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for handling One-Time Password (OTP) operations
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new One-Time Password
    /// </summary>
    /// <returns>A string containing the generated OTP</returns>
    string GenerateOtp();

    /// <summary>
    /// Sends the OTP to the specified email address
    /// </summary>
    /// <param name="email">The recipient's email address</param>
    /// <param name="otp">The OTP to send</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendOtpAsync(string email, string otp);
}
