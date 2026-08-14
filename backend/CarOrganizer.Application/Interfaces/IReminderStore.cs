using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Application.Interfaces;

/// <summary>Persistence for reminders. Implemented in the Infrastructure layer.</summary>
public interface IReminderStore
{
    Task AddAsync(Reminder reminder, CancellationToken cancellationToken = default);

    Task UpdateAsync(Reminder reminder, CancellationToken cancellationToken = default);
    
    Task RemoveAsync(Reminder reminder, CancellationToken cancellationToken = default);

    Task<Reminder?> FindByIdAsync(Guid id, Guid vehicleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reminder>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
