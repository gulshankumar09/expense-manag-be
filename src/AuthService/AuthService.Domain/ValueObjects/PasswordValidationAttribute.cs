using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AuthService.Domain.ValueObjects;

/// <summary>
/// Validates that a password meets configurable complexity rules
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PasswordValidationAttribute : ValidationAttribute
{
    private readonly int _minLength;
    private readonly int _maxLength;
    private readonly bool _requireDigit;
    private readonly bool _requireLowercase;
    private readonly bool _requireUppercase;
    private readonly bool _requireSpecialChar;
    private readonly string _specialCharacters;

    private static readonly Regex HasNumber = new(@"[0-9]+", RegexOptions.Compiled);
    private static readonly Regex HasUpperChar = new(@"[A-Z]+", RegexOptions.Compiled);
    private static readonly Regex HasLowerChar = new(@"[a-z]+", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the PasswordValidationAttribute with custom requirements
    /// </summary>
    /// <param name="minLength">Minimum length required (default: 8)</param>
    /// <param name="maxLength">Maximum length allowed (default: 128)</param>
    /// <param name="requireDigit">Require at least one digit (default: true)</param>
    /// <param name="requireLowercase">Require at least one lowercase letter (default: true)</param>
    /// <param name="requireUppercase">Require at least one uppercase letter (default: true)</param>
    /// <param name="requireSpecialChar">Require at least one special character (default: true)</param>
    /// <param name="specialCharacters">Custom set of special characters to require (default: standard set)</param>
    public PasswordValidationAttribute(
        int minLength = 6,
        int maxLength = 128,
        bool requireDigit = true,
        bool requireLowercase = true,
        bool requireUppercase = true,
        bool requireSpecialChar = true,
        string specialCharacters = "!@#$%^&*()_+=\\[{\\]};:<>|./?,-")
    {
        _minLength = minLength;
        _maxLength = maxLength;
        _requireDigit = requireDigit;
        _requireLowercase = requireLowercase;
        _requireUppercase = requireUppercase;
        _requireSpecialChar = requireSpecialChar;
        _specialCharacters = specialCharacters;
    }

    /// <summary>
    /// Validates the password against configured complexity requirements
    /// </summary>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("Password is required.");

        var password = value.ToString();
        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("Password cannot be empty or whitespace only.");

        var errors = new List<string>();

        if (password.Length < _minLength)
            errors.Add($"Password must be at least {_minLength} characters long.");

        if (password.Length > _maxLength)
            errors.Add($"Password cannot be longer than {_maxLength} characters.");

        if (_requireDigit && !HasNumber.IsMatch(password))
            errors.Add("Password must contain at least one number.");

        if (_requireUppercase && !HasUpperChar.IsMatch(password))
            errors.Add("Password must contain at least one uppercase letter.");

        if (_requireLowercase && !HasLowerChar.IsMatch(password))
            errors.Add("Password must contain at least one lowercase letter.");

        if (_requireSpecialChar && !password.Any(ch => _specialCharacters.Contains(ch)))
            errors.Add($"Password must contain at least one special character from the following: {_specialCharacters}");

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
        var requirements = new List<string>();

        requirements.Add($"be between {_minLength} and {_maxLength} characters long");

        if (_requireDigit)
            requirements.Add("contain at least one number");

        if (_requireUppercase)
            requirements.Add("contain at least one uppercase letter");

        if (_requireLowercase)
            requirements.Add("contain at least one lowercase letter");

        if (_requireSpecialChar)
            requirements.Add($"contain at least one special character from: {_specialCharacters}");

        return $"The {name} field must {string.Join(", ", requirements)}.";
    }
}