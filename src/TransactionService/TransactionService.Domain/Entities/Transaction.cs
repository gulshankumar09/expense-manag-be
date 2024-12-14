namespace TransactionService.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TransactionStatus Status { get; private set; }

    public Transaction(Guid fromUserId, Guid toUserId, decimal amount, string description)
    {
        Id = Guid.NewGuid();
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        Status = TransactionStatus.Pending;
    }
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
} 