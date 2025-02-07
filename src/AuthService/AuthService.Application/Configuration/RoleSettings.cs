namespace AuthService.Application.Configuration;

public class RoleConfiguration
{
    public int MaxSuperAdminUsers { get; set; } = 1; // Default to 1 SuperAdmin
}