namespace SharedLibrary.Constants;

/// <summary>
/// Contains constant values for error codes and messages used across the application
/// </summary>
public static class ErrorConstants
{
    public static class Codes
    {
        // Validation Errors (1000-1999)
        public const int ValidationErrorCode = 400;
        public const string ValidationError = "VALIDATION_ERROR";

        // Not Found Errors (2000-2999)
        public const int NotFoundCode = 404;
        public const string NotFound = "NOT_FOUND";

        // Authorization Errors (3000-3999)
        public const int UnauthorizedCode = 401;
        public const string Unauthorized = "UNAUTHORIZED";

        // Conflict Errors (4000-4999)
        public const int ConflictCode = 409;
        public const string Conflict = "CONFLICT";

        // Bad Request Errors (5000-5999)
        public const int BadRequestCode = 400;
        public const string BadRequest = "BAD_REQUEST";

        // Internal Server Errors (6000-6999)
        public const int InternalErrorCode = 500;
        public const string InternalError = "INTERNAL_ERROR";

        // User Related Errors (7000-7999)
        public const int UserNotFoundCode = 7000;
        public const int EmailExistsCode = 7001;
        public const int EmailNotVerifiedCode = 7002;
        public const int InvalidCredentialsCode = 7003;

        // Token Related Errors (8000-8999)
        public const int InvalidTokenCode = 8000;
        public const int ExpiredTokenCode = 8001;
        public const int RevokedTokenCode = 8002;

        // Role Related Errors (9000-9999)
        public const int RoleAssignmentFailedCode = 9000;
        public const int RoleExistsCode = 9001;
        public const int RoleNotFoundCode = 9002;
    }

    public static class Messages
    {
        public const string UserNotFound = "User not found";
        public const string EmailAlreadyExists = "Email already registered";
        public const string EmailNotVerified = "Email not verified";
        public const string InvalidCredentials = "Invalid email or password";
        public const string UnauthorizedAccess = "You are not authorized to perform this action";
        public const string InvalidToken = "Invalid or expired token";
        public const string InvalidInput = "Invalid input data";
        public const string RoleAssignmentFailed = "Failed to assign role";
        public const string RoleAlreadyExists = "Role already exists";
        public const string InternalServerError = "An internal server error occurred";
    }

    public static class ValidationErrors
    {
        public const string RequiredField = "This field is required";
        public const string InvalidEmail = "Invalid email format";
        public const string InvalidPassword = "Password does not meet requirements";
        public const string PasswordMismatch = "Passwords do not match";
        public const string InvalidPhoneNumber = "Invalid phone number format";
    }
}