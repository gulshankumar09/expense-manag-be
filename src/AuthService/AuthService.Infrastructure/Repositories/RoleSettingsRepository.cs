using Microsoft.EntityFrameworkCore;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;

namespace AuthService.Infrastructure.Repositories;

public class RoleSettingsRepository : IRoleSettingsRepository
{
    private readonly AuthDbContext _context;

    public RoleSettingsRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<RoleSettings> GetSettingsAsync()
    {
        var settings = await _context.RoleSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new RoleSettings { Id = 1, MaxSuperAdminUsers = 1 };
            await _context.RoleSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(RoleSettings settings)
    {
        var existingSettings = await _context.RoleSettings.FirstOrDefaultAsync();
        if (existingSettings == null)
        {
            await _context.RoleSettings.AddAsync(settings);
        }
        else
        {
            existingSettings.MaxSuperAdminUsers = settings.MaxSuperAdminUsers;
            _context.RoleSettings.Update(existingSettings);
        }
        await _context.SaveChangesAsync();
    }
}