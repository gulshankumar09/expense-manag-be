using AuthService.Domain.Entities;
using SharedLibrary.Repositories;

namespace AuthService.Application.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User> GetByEmailAsync(string email);
} 