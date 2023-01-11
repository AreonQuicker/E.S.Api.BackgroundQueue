using E.S.Api.BackgroundQueue.Constants;
using E.S.Api.BackgroundQueue.Interfaces;
using E.S.Api.BackgroundQueue.Models;
using E.S.Logging.Enums;
using E.S.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace E.S.Api.BackgroundQueue.HostedServices;

public sealed class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        (_taskQueue, _logger) = (taskQueue, logger);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return ProcessTaskQueueAsync(stoppingToken);
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                (string header, Func<CancellationToken, QueueItem, ValueTask> workItem) workItem =
                    await _taskQueue.DequeueAsync(stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var queueItem = QueueItem.Create(scope.ServiceProvider, workItem.header);

                    await workItem.workItem(stoppingToken, queueItem);
                }
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                var errorMessage = "Failed processing background task queue item";

                _logger.LogErrorOperation(LoggerStatusEnum.EndWithError, LoggerConstant.System,
                    null, null,
                    null, errorMessage, ex);
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}