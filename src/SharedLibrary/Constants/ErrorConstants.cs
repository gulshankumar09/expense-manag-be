namespace SharedLibrary.Constants;

/// <summary>
/// Constants for error codes and messages used throughout the application
/// </summary>
public static class ErrorConstants
{
    /// <summary>
    /// Error codes used in the application
    /// </summary>
    public static class Codes
    {
        public const string BadRequestCode = "BAD_REQUEST";
        public const string UnauthorizedCode = "UNAUTHORIZED";
        public const string ForbiddenCode = "FORBIDDEN";
        public const string NotFoundCode = "NOT_FOUND";
        public const string ConflictCode = "CONFLICT";
        public const string ValidationCode = "VALIDATION_ERROR";
        public const string InternalErrorCode = "INTERNAL_ERROR";
    }

    /// <summary>
    /// Error messages used in the application
    /// </summary>
    public static class Messages
    {
        public const string InvalidCredentials = "Invalid email or password.";
        public const string UnauthorizedAccess = "You are not authorized to access this resource.";
        public const string UserNotFound = "User not found.";
        public const string EmailAlreadyExists = "Email already exists.";
        public const string InvalidToken = "Invalid or expired token.";
        public const string InternalServerError = "An unexpected error occurred. Please try again later.";
        public const string ValidationError = "One or more validation errors occurred.";
        public const string InvalidRequest = "Invalid request.";
        public const string ResourceNotFound = "The requested resource was not found.";
        public const string EmailNotVerified = "Please verify your email first.";
        public const string AccountLocked = "Account is locked. Try again later.";
        public const string InvalidOldPassword = "Current password is incorrect.";
        public const string PasswordMismatch = "The new password and confirmation password do not match.";
        public const string InvalidRefreshToken = "Invalid or expired refresh token.";

        // Conflict Messages
        public static class Conflict
        {
            public const string ResourceAlreadyExists = "The resource already exists.";
            public const string DuplicateEntry = "A duplicate entry was found.";
            public const string ConcurrentModification = "The resource was modified by another user.";
            public const string RoleAlreadyAssigned = "The role is already assigned to this user.";
            public const string UserAlreadyActive = "The user account is already active.";
        }
    }

    public static class ValidationErrors
    {
        public const string RequiredField = "This field is required";
        public const string InvalidEmail = "Invalid email format";
        public const string InvalidPassword = "Password does not meet requirements";
        public const string InvalidPhoneNumber = "Invalid phone number format";
    }
}