namespace SharedLibrary.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;
    public bool IsActive { get; protected set; } = true;

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void SetCreatedBy(string userId)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(string userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete(string userId)
    {
        IsDeleted = true;
        SetUpdatedBy(userId);
    }

    public void Deactivate(string userId)
    {
        IsActive = false;
        SetUpdatedBy(userId);
    }

    public void Activate(string userId)
    {
        IsActive = true;
        SetUpdatedBy(userId);
    }
}