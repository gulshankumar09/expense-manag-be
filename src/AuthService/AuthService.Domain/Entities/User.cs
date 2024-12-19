using SharedLibrary.Domain;
using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities;

public class User : BaseEntity
{
    public Email Email { get; private set; }
    public Password PasswordHash { get; private set; }
    public PersonName Name { get; private set; }

    // Required by EF Core
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
    }

    // Factory method for creating a new user
    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        return new User(
            Email.Create(email),
            Password.Create(passwordHash),
            PersonName.Create(firstName, lastName)
        );
    }
} 