using Microsoft.Extensions.Hosting;
using Serilog;
using service;

namespace worker;

public class SyncWorker(IEmailService emailService) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Initializing Gmail connection");
        await emailService.InitializeAsync();

        Log.Information("Sync worker started — interval {Interval}", Interval);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                Log.Information("Sync started at {Time}", DateTimeOffset.Now);
                await emailService.CacheEmailsAsync(new WorkerCacheOptions());
                Log.Information("Sync completed at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Sync failed");
            }
        }
    }
}
