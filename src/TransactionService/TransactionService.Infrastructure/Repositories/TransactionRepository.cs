using TransactionService.Domain.Entities;
using SharedLibrary.Repositories;

namespace TransactionService.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(TransactionDbContext context) : base(context)
    {
    }
} 