using TransactionService.Domain.Entities;
using SharedLibrary.Repositories;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(TransactionDbContext context) : base(context)
    {
    }
} 