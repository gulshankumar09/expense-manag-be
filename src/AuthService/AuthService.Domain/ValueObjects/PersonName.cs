namespace AuthService.Domain.ValueObjects;

public class PersonName
{
    public string FirstName { get; }
    public string LastName { get; }

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static PersonName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentNullException(nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentNullException(nameof(lastName));

        // Add validation rules for names (e.g., length, allowed characters)
        if (firstName.Length > 50)
            throw new ArgumentException("First name cannot exceed 50 characters", nameof(firstName));

        if (lastName.Length > 50)
            throw new ArgumentException("Last name cannot exceed 50 characters", nameof(lastName));

        return new PersonName(firstName.Trim(), lastName.Trim());
    }

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() => FullName;
}