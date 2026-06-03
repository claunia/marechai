using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.WinWorld;

public class MarechaiContextFactory : IDbContextFactory<MarechaiContext>
{
    readonly DbContextOptions<MarechaiContext> _options;

    public MarechaiContextFactory(DbContextOptions<MarechaiContext> options) => _options = options;

    public MarechaiContext CreateDbContext() => new(_options);

    public Task<MarechaiContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());
}
