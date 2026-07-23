using CarOrganizer.Application.Interfaces;
using CarOrganizer.Application.Obligations;
using CarOrganizer.Domain.Entities;

namespace CarOrganizer.Infrastructure.Obligations;

/// <summary>
/// Store-backed implementation of <see cref="IVehicleObligationService"/>. Threads the owner id
/// through an ownership check on the vehicle before touching its obligations, and owns the
/// entity/DTO mapping.
/// </summary>
public class VehicleObligationService : IVehicleObligationService
{
    private readonly IVehicleObligationStore _obligations;
    private readonly IVehicleStore _vehicles;

    public VehicleObligationService(IVehicleObligationStore obligations, IVehicleStore vehicles)
    {
        _obligations = obligations;
        _vehicles = vehicles;
    }

    public async Task<VehicleObligationResponse?> CreateAsync(Guid ownerId, Guid vehicleId, CreateVehicleObligationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        var obligation = new VehicleObligation
        {
            VehicleId = vehicleId,
            Type = request.Type,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            Cost = request.Cost,
            Provider = request.Provider,
            PolicyNumber = request.PolicyNumber,
            Notes = request.Notes,
        };

        await _obligations.AddAsync(obligation, cancellationToken);

        return ToResponse(obligation);
    }

    public async Task<IReadOnlyList<VehicleObligationResponse>?> ListAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        var obligations = await _obligations.ListByVehicleAsync(vehicleId, cancellationToken);

        return obligations.Select(ToResponse).ToArray();
    }

    public async Task<VehicleObligationResponse?> GetAsync(Guid ownerId, Guid vehicleId, Guid obligationId, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        var obligation = await _obligations.FindByIdAsync(obligationId, vehicleId, cancellationToken);

        return obligation is null ? null : ToResponse(obligation);
    }

    public async Task<VehicleObligationResponse?> UpdateAsync(Guid ownerId, Guid vehicleId, Guid obligationId, UpdateVehicleObligationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return null;
        }

        var obligation = await _obligations.FindByIdAsync(obligationId, vehicleId, cancellationToken);
        if (obligation is null)
        {
            return null;
        }

        obligation.Type = request.Type;
        obligation.ValidFrom = request.ValidFrom;
        obligation.ValidUntil = request.ValidUntil;
        obligation.Cost = request.Cost;
        obligation.Provider = request.Provider;
        obligation.PolicyNumber = request.PolicyNumber;
        obligation.Notes = request.Notes;

        await _obligations.UpdateAsync(obligation, cancellationToken);

        return ToResponse(obligation);
    }

    public async Task<bool> DeleteAsync(Guid ownerId, Guid vehicleId, Guid obligationId, CancellationToken cancellationToken = default)
    {
        if (!await OwnsVehicleAsync(ownerId, vehicleId, cancellationToken))
        {
            return false;
        }

        var obligation = await _obligations.FindByIdAsync(obligationId, vehicleId, cancellationToken);
        if (obligation is null)
        {
            return false;
        }

        await _obligations.RemoveAsync(obligation, cancellationToken);

        return true;
    }

    private async Task<bool> OwnsVehicleAsync(Guid ownerId, Guid vehicleId, CancellationToken cancellationToken) =>
        await _vehicles.FindByIdAsync(vehicleId, ownerId, cancellationToken) is not null;

    private static VehicleObligationResponse ToResponse(VehicleObligation obligation) =>
        new(
            obligation.Id,
            obligation.Type,
            obligation.ValidFrom,
            obligation.ValidUntil,
            obligation.Cost,
            obligation.Provider,
            obligation.PolicyNumber,
            obligation.Notes,
            obligation.CreatedAtUtc,
            obligation.UpdatedAtUtc);
}
