using SharedLibrary.Repositories;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(TransactionDbContext context) : base(context)
    {
    }
}