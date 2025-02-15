using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Constants;
using SharedLibrary.Repositories;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository(AuthDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User> GetByEmailAsync(string email)
    {
        var users = await context.Users
            .Where(u => !u.IsDeleted && u.IsActive)
            .ToListAsync();

        return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ??
               throw new KeyNotFoundException(ErrorConstants.Messages.UserNotFound);
    }
}