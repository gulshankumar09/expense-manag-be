using Microsoft.AspNetCore.Http;

namespace SharedLibrary.Services.Audit;

public class AuditService(IHttpContextAccessor httpContextAccessor) : IAuditService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "SYSTEM_USER";
        // Or with timestamp:
        // return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? $"SYSTEM_{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}