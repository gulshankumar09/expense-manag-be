using SharedLibrary.Domain;

namespace TransactionService.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public TransactionStatus Status { get; private set; }

    public Transaction(Guid fromUserId, Guid toUserId, decimal amount, string description)
        : base()
    {
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        Description = description;
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