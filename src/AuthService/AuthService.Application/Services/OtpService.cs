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
        var body = GenerateVerificationEmailBody(otp);//$"Your verification code is: {otp}";
        await _emailService.SendEmailAsync(email, subject, body);
    }

    private string GenerateVerificationEmailBody(string verificationCode)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        margin: 0;
                        padding: 20px;
                    }}
                    .container {{
                        background-color: #ffffff;
                        border-radius: 5px;
                        padding: 20px;
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                    }}
                    h1 {{
                        color: #333333;
                    }}
                    p {{
                        color: #555555;
                    }}
                    .verification-code {{
                        font-size: 24px;
                        font-weight: bold;
                        color: #0073e6; /* AWS blue color */
                    }}
                    a {{
                        color: #0073e6; /* AWS blue color */
                        text-decoration: none;
                    }}
                    a:hover {{
                        text-decoration: underline;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>Verify Your Identity</h1>
                    <p>Hello,</p>
                    <p>We identified unusual activity in a recent sign-in attempt from your SPLITTER account. If you initiated the request to sign into SPLITTER, please enter the following code to verify your identity and complete your sign-in.</p>
                    <h2 class='verification-code'>{verificationCode}</h2>
                    <p>(This code will expire 10 minutes after it was sent.)</p>
                    <p>If you did not initiate the request to sign in to SPLITTER, we strongly advise you to change your account password. Additionally, we encourage you to enable multi-factor authentication (MFA) to add an additional layer of protection to your SPLITTER account.</p>
                    <p>For more information, visit <a href='https://www.google.com'>this link</a>.</p>
                </div>
            </body>
            </html>";
    }
}