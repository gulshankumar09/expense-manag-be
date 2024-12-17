using Microsoft.AspNetCore.Http;

namespace SharedLibrary.Services;

public class AuditService : IAuditService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "SYSTEM_USER";
        // Or with timestamp:
        // return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? $"SYSTEM_{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}