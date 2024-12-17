namespace AuthService.Domain.ValueObjects;

public class Password
{
    public string Hash { get; }

    private Password(string hash)
    {
        Hash = hash;
    }

    public static Password Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentNullException(nameof(hash));

        return new Password(hash);
    }
} 