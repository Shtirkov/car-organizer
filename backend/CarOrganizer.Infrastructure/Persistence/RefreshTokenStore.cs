using CarOrganizer.Application.Interfaces;
using CarOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarOrganizer.Infrastructure.Persistence;

/// <summary>EF Core-backed <see cref="IRefreshTokenStore"/>.</summary>
public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Update(token);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
