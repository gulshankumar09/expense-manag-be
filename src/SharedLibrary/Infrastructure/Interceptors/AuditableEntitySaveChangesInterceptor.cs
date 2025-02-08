using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedLibrary.Domain;
using SharedLibrary.Services.Audit;

namespace SharedLibrary.Infrastructure.Interceptors;

/// <summary>
/// Interceptor to handle auditing of entities during save changes in DbContext.
/// </summary>
public class AuditableEntitySaveChangesInterceptor(IAuditService auditService) : SaveChangesInterceptor
{
    private readonly IAuditService _auditService = auditService;

    /// <summary>
    /// Intercepts the saving changes operation to update audit information.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <param name="result">The result of the save changes operation.</param>
    /// <returns>The interception result.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Asynchronously intercepts the saving changes operation to update audit information.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <param name="result">The result of the save changes operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The interception result.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates the entities with audit information.
    /// </summary>
    /// <param name="context">The DbContext.</param>
    private void UpdateEntities(DbContext context)
    {
        if (context == null) return;

        var userId = _auditService.GetCurrentUserId();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedBy(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedBy(userId);
            }
        }
    }
}
