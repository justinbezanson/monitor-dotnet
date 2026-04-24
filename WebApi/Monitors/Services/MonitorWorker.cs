using Microsoft.EntityFrameworkCore;
using WebApi.Monitors.Services;

namespace WebApi.Monitors.Services;

public class MonitorWorker(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<MonitorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Get interval from configuration, default to 15 seconds
        var intervalSeconds = configuration.GetValue<int>("Monitoring:WorkerIntervalSeconds", 15);
        var interval = TimeSpan.FromSeconds(intervalSeconds);

        logger.LogInformation("MonitorWorker started with interval {Interval} seconds", intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformAllChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during monitoring cycle");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task PerformAllChecksAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Find monitors that are enabled and due for a check
        var now = DateTime.UtcNow;
        var monitors = await database.Monitors
            .Where(m => m.IsEnabled)
            .ToListAsync(ct);

        var dueMonitors = monitors.Where(m => 
            m.LastCheckedAt == null || 
            m.LastCheckedAt.Value.AddSeconds(m.IntervalSeconds) <= now)
            .ToList();

        if (dueMonitors.Count == 0) return;

        logger.LogInformation("Checking {Count} monitors...", dueMonitors.Count);

        // Run checks in parallel but each with its own scope to avoid DbContext concurrency issues
        var tasks = dueMonitors.Select(async m => 
        {
            try
            {
                using var checkScope = serviceProvider.CreateScope();
                var monitoringService = checkScope.ServiceProvider.GetRequiredService<MonitoringService>();
                
                // Re-fetch or attach the monitor to the new context if needed, 
                // but MonitoringService.PerformCheckAsync expects the entity.
                // We'll pass the ID and let it handle its own data access.
                await monitoringService.PerformCheckByIdAsync(m.Id, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking monitor {MonitorId}", m.Id);
            }
        });

        await Task.WhenAll(tasks);
    }
}
