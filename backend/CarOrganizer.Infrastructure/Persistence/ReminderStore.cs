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

    public async Task<Reminder?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reminders
            .Include(r => r.Vehicle)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reminder>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reminders
            .Where(r => r.VehicleId == vehicleId)
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