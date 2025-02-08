using SharedLibrary.Domain;

namespace ExpenseService.Domain.Entities;

public class Expense : BaseEntity
{
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public Guid PaidByUserId { get; private set; }
    public List<ExpenseSplit> Splits { get; private set; }

    public Expense(string description, decimal amount, Guid paidByUserId)
        : base()
    {
        Description = description;
        Amount = amount;
        PaidByUserId = paidByUserId;
        Splits = new List<ExpenseSplit>();
    }
}

public class ExpenseSplit
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }

    public ExpenseSplit(Guid userId, decimal amount)
    {
        UserId = userId;
        Amount = amount;
    }

}