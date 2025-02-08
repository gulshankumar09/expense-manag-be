namespace AuthService.Domain.ValueObjects;

public class Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}