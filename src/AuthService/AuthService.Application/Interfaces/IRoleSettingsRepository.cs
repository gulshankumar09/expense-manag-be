using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRoleSettingsRepository
{
    Task<RoleSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(RoleSettings settings);
}