using AuthService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AuthService.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<GoogleUserResponse>(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}", cancellationToken);

            if (response == null)
                return null;

            return new GoogleUserInfo(
                response.Email,
                response.Sub,
                response.GivenName,
                response.FamilyName);
        }
        catch
        {
            return null;
        }
    }

    private record GoogleUserResponse(
        string Sub,
        string Email,
        string GivenName,
        string FamilyName);
}