using InChambers.Core.Interfaces;
using InChambers.Core.Models.Input.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InChambers.Core.Extensions;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly UserSession _userSession;

    public SoftDeleteInterceptor(UserSession userSession) =>
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(
                eventData, result, cancellationToken);
        }

        IEnumerable<EntityEntry<ISoftDeletable>> entries =
            eventData
                .Context
                .ChangeTracker
                .Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted);

        foreach (EntityEntry<ISoftDeletable> softDeletable in entries)
        {
            softDeletable.State = EntityState.Modified;
            softDeletable.Entity.IsDeleted = true;
            softDeletable.Entity.DeletedById = _userSession.UserId;
            softDeletable.Entity.DeletedOnUtc = DateTime.UtcNow;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}