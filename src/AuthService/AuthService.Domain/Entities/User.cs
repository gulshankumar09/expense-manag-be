using SharedLibrary.Domain;
using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities;

public class User : BaseEntity
{
    public Email Email { get; private set; }
    public Password PasswordHash { get; private set; }
    public PersonName Name { get; private set; }
    public string? GoogleId { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public string? VerificationToken { get; private set; }
    public DateTime? VerificationTokenExpiry { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    protected User() : base()
    {
        Email = Email.Create("temp@temp.com");
        PasswordHash = Password.Create("temp");
        Name = PersonName.Create("Temp", "User");
    }

    public User(Email email, Password passwordHash, PersonName name)
        : base()
    {
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
        IsEmailVerified = false;
    }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        var user = new User(
            Email.Create(email),
            Password.Create(passwordHash),
            PersonName.Create(firstName, lastName)
        );
        return user;
    }

    public static User CreateWithGoogle(string email, string googleId, string firstName, string lastName)
    {
        var user = new User(
            Email.Create(email),
            Password.Create(Guid.NewGuid().ToString()), // Temporary password for Google users
            PersonName.Create(firstName, lastName)
        );
        user.GoogleId = googleId;
        user.IsEmailVerified = true; // Google users are pre-verified
        return user;
    }

    public void SetVerificationToken(string token, TimeSpan expiry)
    {
        VerificationToken = token;
        VerificationTokenExpiry = DateTime.UtcNow.Add(expiry);
    }

    public void SetRefreshToken(string token, TimeSpan expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = DateTime.UtcNow.Add(expiry);
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        VerificationToken = null;
        VerificationTokenExpiry = null;
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
} 