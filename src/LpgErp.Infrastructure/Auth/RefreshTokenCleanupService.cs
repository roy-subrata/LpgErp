using LpgErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LpgErp.Infrastructure.Auth;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(6);

    public RefreshTokenCleanupService(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-30);
                var deleted = await context.RefreshTokens
                    .Where(rt => rt.RevokedAt != null && rt.RevokedAt < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired revoked refresh tokens", deleted);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Refresh token cleanup failed");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
    }
}
