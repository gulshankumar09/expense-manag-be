using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.Interfaces;

public interface IExpenseRepository
{
    Task<Expense> GetByIdAsync(Guid id);
    Task<IEnumerable<Expense>> GetAllAsync();
    Task AddAsync(Expense expense);
    Task SaveChangesAsync();
} 