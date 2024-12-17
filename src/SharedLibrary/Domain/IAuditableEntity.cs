namespace SharedLibrary.Domain;

public interface IAuditableEntity
{
    string CreatedBy { get; }
    DateTime CreatedAt { get; }
    string UpdatedBy { get; }
    DateTime? UpdatedAt { get; }
    bool IsDeleted { get; }
}