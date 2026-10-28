using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services;

public sealed class PendingOrdersGaugeService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingOrdersGaugeService> _logger;

    public PendingOrdersGaugeService(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingOrdersGaugeService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepo>();
                var pendingCount = await orderRepo.CountPendingOrders(stoppingToken);
                OrderTelemetry.SetPendingOrders(pendingCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to refresh the pending order metric");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
