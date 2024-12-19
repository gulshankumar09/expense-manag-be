using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AuthDbContext _authContext;

    public UserRepository(AuthDbContext context) : base(context)
    {
        _authContext = context;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var users = await _authContext.Users
            .Where(u => !u.IsDeleted && u.IsActive)
            .ToListAsync();

        return users.FirstOrDefault(u => u.Email.Value.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}