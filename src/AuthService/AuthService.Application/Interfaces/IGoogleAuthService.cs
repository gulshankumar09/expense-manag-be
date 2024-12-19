namespace AuthService.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken);
}

public record GoogleUserInfo(string Email, string GoogleId, string FirstName, string LastName); 