using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>EF Core-backed <see cref="IReminderStore"/>.</summary>
public class ReminderStore : IReminderStore
{
    private readonly AppDbContext _dbContext;

    public ReminderStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        _dbContext.Reminders.Add(reminder);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Reminder?> FindByIdAsync(Guid id, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reminders
            .SingleOrDefaultAsync(r => r.Id == id && r.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<IReadOnlyList<Reminder>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // Open reminders first, then soonest due. DueDate is null for mileage-only reminders, so the
        // explicit null key keeps them last on every provider rather than relying on Postgres/InMemory defaults.
        return await _dbContext.Reminders
            .Where(r => r.VehicleId == vehicleId)           
            .OrderBy(r => r.IsCompleted)
            .ThenBy(r => r.DueDate == null)
            .ThenBy(r => r.DueDate)
            .ThenBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        _dbContext.Reminders.Remove(reminder);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        _dbContext.Reminders.Update(reminder);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
