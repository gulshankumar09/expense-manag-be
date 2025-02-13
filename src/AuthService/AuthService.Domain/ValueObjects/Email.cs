using System.Text.RegularExpressions;
using SharedLibrary.Security;

namespace AuthService.Domain.ValueObjects;

public class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email cannot be empty");

        // Sanitize the input to prevent XSS
        var sanitizedEmail = XssSanitizer.StripHtml(email);

        // Normalize the email
        sanitizedEmail = sanitizedEmail.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(sanitizedEmail))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (sanitizedEmail.Length > 254) // Maximum allowed length for email addresses
            throw new ArgumentException("Email is too long", nameof(email));

        return new Email(sanitizedEmail);
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is Email other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Email? left, Email? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Email? left, Email? right) => !(left == right);
}