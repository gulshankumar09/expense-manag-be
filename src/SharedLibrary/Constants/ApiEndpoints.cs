namespace SharedLibrary.Constants;

/// <summary>
/// Contains constant values for API endpoints used across the application
/// </summary>
public static class ApiEndpoints
{
    public static class Account
    {
        private const string Base = "/api/account";
        public const string VerifyOtp = $"{Base}/verify-otp";
        public const string Register = $"{Base}/register";
        public const string Login = $"{Base}/login";
        public const string RefreshToken = $"{Base}/refresh-token";
        public const string RevokeToken = $"{Base}/revoke-token";
        public const string Profile = $"{Base}/profile";
    }

    public static class Roles
    {
        private const string Base = "/api/roles";
        public const string List = $"{Base}/list";
        public const string Create = $"{Base}/create";
        public const string Assign = $"{Base}/assign";
        public const string UserRoles = $"{Base}/user";
    }
}