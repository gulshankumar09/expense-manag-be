using TransactionService.Domain.Entities;

namespace TransactionService.Application.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task AddAsync(Transaction transaction);
    Task SaveChangesAsync();
}