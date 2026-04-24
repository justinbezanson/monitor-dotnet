using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using WebApi.Data.Entities;

namespace WebApi.Monitors.Services;

public class MonitoringService(
    ApplicationDbContext database,
    IHttpClientFactory httpClientFactory,
    ILogger<MonitoringService> logger)
{
    public async Task PerformCheckByIdAsync(Guid monitorId, CancellationToken ct)
    {
        var monitor = await database.Monitors.FirstOrDefaultAsync(m => m.Id == monitorId, ct);
        if (monitor == null) return;

        await PerformCheckAsync(monitor, ct);
    }

    public async Task PerformCheckAsync(Data.Entities.Monitor monitor, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        bool isSuccess = false;
        int? statusCode = null;
        string? errorMessage = null;

        // Determine if we should use HTTP or raw TCP
        // If it's a known non-HTTP port like Minecraft (25565), we'll try TCP first
        bool isKnownTcpPort = monitor.Port == 25565 || monitor.Port == 22 || monitor.Port == 3389;

        try
        {
            if (isKnownTcpPort)
            {
                isSuccess = await TryTcpCheckAsync(monitor, ct);
                if (isSuccess) statusCode = 0; // 0 indicates successful TCP connection
            }
            else
            {
                try 
                {
                    var client = httpClientFactory.CreateClient();
                    var uriBuilder = new UriBuilder(monitor.Url);
                    if (monitor.Port.HasValue) uriBuilder.Port = monitor.Port.Value;

                    var response = await client.GetAsync(uriBuilder.Uri, ct);
                    statusCode = (int)response.StatusCode;
                    isSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception ex) when (monitor.Port.HasValue)
                {
                    // Fallback to TCP if HTTP fails but a port is provided
                    logger.LogInformation("HTTP check failed for {MonitorId}, falling back to TCP Ping", monitor.Id);
                    isSuccess = await TryTcpCheckAsync(monitor, ct);
                    if (isSuccess) statusCode = 0;
                    else throw; // If TCP also fails, let the outer catch handle it
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            logger.LogError(ex, "Error checking monitor {MonitorId} ({MonitorName})", monitor.Id, monitor.Name);
        }
        finally
        {
            stopwatch.Stop();
        }

        var check = new MonitorCheck
        {
            Id = Guid.NewGuid(),
            MonitorId = monitor.Id,
            Timestamp = DateTime.UtcNow,
            IsSuccess = isSuccess,
            StatusCode = statusCode,
            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
            ErrorMessage = errorMessage
        };

        monitor.LastCheckedAt = check.Timestamp;
        monitor.CurrentStatus = isSuccess ? "Online" : "Offline";

        database.MonitorChecks.Add(check);
        await database.SaveChangesAsync(ct);
    }

    private async Task<bool> TryTcpCheckAsync(Data.Entities.Monitor monitor, CancellationToken ct)
    {
        try
        {
            using var client = new TcpClient();
            var host = new Uri(monitor.Url).Host;
            var port = monitor.Port ?? (monitor.Url.StartsWith("https") ? 443 : 80);
            
            var connectTask = client.ConnectAsync(host, port, ct);
            
            // Wait for connection with a timeout
            var completedTask = await Task.WhenAny(connectTask.AsTask(), Task.Delay(5000, ct));
            
            if (completedTask == connectTask.AsTask() && client.Connected)
            {
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}
