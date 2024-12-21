using SharedLibrary.Domain;
using AuthService.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class User : IdentityUser //: BaseEntity
{
    public PersonName Name { get; set; }
    public string? GoogleId { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false; // default value is false
    public bool IsActive { get; set; } = true; // default value is true

    public User() : base()
    {
        Email = "temp@temp.com";
        Name = PersonName.Create("Temp", "User");
    }

    public User(string email, string passwordHash, PersonName name)
        : base()
    {
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
    }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        var user = new User(
            email,
            passwordHash,
            PersonName.Create(firstName, lastName)
        );
        return user;
    }

    public static User CreateWithGoogle(string email, string googleId, string firstName, string lastName)
    {
        var user = new User(
            email,
            Guid.NewGuid().ToString(), // Temporary password for Google users
            PersonName.Create(firstName, lastName)
        );
        user.GoogleId = googleId;
        user.EmailConfirmed = true; // Google users are pre-verified
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
        EmailConfirmed = true;
        VerificationToken = null;
        VerificationTokenExpiry = null;
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
} 