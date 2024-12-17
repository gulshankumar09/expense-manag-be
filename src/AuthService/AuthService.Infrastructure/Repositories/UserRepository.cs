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

    //public override async Task<User> GetByIdAsync(Guid id)
    //{
    //    return await _authContext.Users.FindAsync(id);
    //}

    //public override async Task<IEnumerable<User>> GetAllAsync()
    //{
    //    return await _authContext.Users.ToListAsync();
    //}

    //public override async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
    //{
    //    return await _authContext.Users.Where(predicate).ToListAsync();
    //}

    public override async Task AddAsync(User entity)
    {
        await _authContext.Users.AddAsync(entity);
        await _authContext.SaveChangesAsync();
    }


    public async Task<User> GetByEmailAsync(string email)
    {
        return await _authContext.Users
            .FirstOrDefaultAsync(u => string.Equals(u.Email.Value, email, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"User with email {email} not found");
    }
}