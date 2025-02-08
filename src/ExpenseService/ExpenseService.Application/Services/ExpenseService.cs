using ExpenseService.Application.Interfaces;
using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.Services;

public class ExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Expense> GetExpenseByIdAsync(Guid id)
    {
        return await _expenseRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
    {
        return await _expenseRepository.GetAllAsync();
    }

    public async Task AddExpenseAsync(Expense expense)
    {
        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();
    }
}