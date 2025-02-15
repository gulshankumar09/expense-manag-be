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
        // General error codes
        public const string BadRequestCode = "BAD_REQUEST";
        public const string UnauthorizedCode = "UNAUTHORIZED";
        public const string ForbiddenCode = "FORBIDDEN";
        public const string NotFoundCode = "NOT_FOUND";
        public const string ConflictCode = "CONFLICT";
        public const string ValidationCode = "VALIDATION_ERROR";
        public const string InternalErrorCode = "INTERNAL_ERROR";
        public const string UserNotVerified = "USER_NOT_VERIFIED";

        // Email-related error codes
        public const string EmailConfigError = "EMAIL_CONFIG_ERROR";
        public const string SmtpConnectionError = "SMTP_CONNECTION_ERROR";
        public const string EmailAuthError = "EMAIL_AUTH_ERROR";
        public const string InvalidEmailRecipient = "INVALID_EMAIL_RECIPIENT";
        public const string EmailRateLimit = "EMAIL_RATE_LIMIT";
        public const string EmailSendingError = "EMAIL_SENDING_ERROR";
        public const string EmailTemplateError = "EMAIL_TEMPLATE_ERROR";
        public const string EmailAttachmentError = "EMAIL_ATTACHMENT_ERROR";
    }

    /// <summary>
    /// Error messages used in the application
    /// </summary>
    public static class Messages
    {
        // General error messages
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

        // Email-related error messages
        public static class Email
        {
            // Configuration errors
            public const string ConfigurationError = "Email service configuration error.";
            public const string MissingFromEmail = "Sender email address is not configured.";
            public const string MissingSmtpHost = "SMTP host is not configured.";
            public const string InvalidSmtpPort = "Invalid SMTP port configuration.";
            public const string MissingCredentials = "SMTP credentials are not configured.";

            // Connection errors
            public const string SmtpConnectionError = "Failed to connect to email server.";
            public const string SslRequired = "SSL/TLS connection is required.";
            public const string TimeoutError = "Connection to email server timed out.";

            // Authentication errors
            public const string AuthenticationError = "Failed to authenticate with email server.";
            public const string InvalidCredentials = "Invalid SMTP credentials.";

            // Recipient errors
            public const string InvalidRecipient = "Invalid recipient email address.";
            public const string EmptyRecipient = "Recipient email address cannot be empty.";
            public const string InvalidFormat = "Invalid email format.";
            public const string RecipientNotFound = "Recipient email address not found.";

            // Rate limiting
            public const string RateLimitExceeded = "Email sending limit exceeded.";
            public const string QuotaExceeded = "Email quota has been exceeded.";
            public const string TooManyRecipients = "Too many recipients in single email.";

            // Sending errors
            public const string SendingError = "Failed to send email.";
            public const string DeliveryFailed = "Email delivery failed.";
            public const string MailboxUnavailable = "Recipient mailbox is unavailable.";
            public const string MessageTooLarge = "Email message size exceeds limit.";

            // Template errors
            public const string TemplateError = "Failed to process email template.";
            public const string TemplateMissing = "Email template not found.";
            public const string TemplateInvalid = "Invalid email template format.";
            public const string MissingTemplateData = "Required template data is missing.";

            // Attachment errors
            public const string AttachmentError = "Failed to process email attachment.";
            public const string AttachmentTooLarge = "Attachment size exceeds limit.";
            public const string InvalidAttachmentType = "Invalid attachment file type.";
            public const string AttachmentNotFound = "Attachment file not found.";
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