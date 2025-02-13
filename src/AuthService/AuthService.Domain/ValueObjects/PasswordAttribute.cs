using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AuthService.Domain.ValueObjects;

/// <summary>
/// Validates that a password meets the required complexity rules
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PasswordAttribute : ValidationAttribute
{
    private const int MinLength = 8;
    private const int MaxLength = 128;
    private static readonly Regex HasNumber = new(@"[0-9]+", RegexOptions.Compiled);
    private static readonly Regex HasUpperChar = new(@"[A-Z]+", RegexOptions.Compiled);
    private static readonly Regex HasLowerChar = new(@"[a-z]+", RegexOptions.Compiled);
    private static readonly Regex HasSymbols = new(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]", RegexOptions.Compiled);

    public PasswordAttribute() : base("Password does not meet complexity requirements.")
    {
    }

    /// <summary>
    /// Validates the password against complexity requirements
    /// </summary>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("Password is required.");

        var password = value.ToString();
        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("Password cannot be empty or whitespace only.");

        var errors = new List<string>();

        if (password.Length < MinLength)
            errors.Add($"Password must be at least {MinLength} characters long.");

        if (password.Length > MaxLength)
            errors.Add($"Password cannot be longer than {MaxLength} characters.");

        if (!HasNumber.IsMatch(password))
            errors.Add("Password must contain at least one number.");

        if (!HasUpperChar.IsMatch(password))
            errors.Add("Password must contain at least one uppercase letter.");

        if (!HasLowerChar.IsMatch(password))
            errors.Add("Password must contain at least one lowercase letter.");

        if (!HasSymbols.IsMatch(password))
            errors.Add("Password must contain at least one special character.");

        if (errors.Any())
        {
            return new ValidationResult(
                string.Join(" ", errors),
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Formats the error message
    /// </summary>
    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be between {MinLength} and {MaxLength} characters long and contain at least one number, " +
               "one uppercase letter, one lowercase letter, and one special character.";
    }
}