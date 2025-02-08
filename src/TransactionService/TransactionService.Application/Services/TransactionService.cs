using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Transaction> GetTransactionByIdAsync(Guid id)
    {
        return await _transactionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        return await _transactionRepository.GetAllAsync();
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();
    }
}