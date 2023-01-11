using E.S.Api.BackgroundQueue.Constants;
using E.S.Api.BackgroundQueue.Interfaces;
using E.S.Api.BackgroundQueue.Models;
using E.S.Logging.Enums;
using E.S.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace E.S.Api.BackgroundQueue.HostedServices;

public sealed class MultiThreadedQueuedHostedService : BackgroundService
{
    private readonly ILogger<MultiThreadedQueuedHostedService> _logger;
    private readonly int _numberOfThreads;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMultiThreadedBackgroundTaskQueue _taskQueue;

    public MultiThreadedQueuedHostedService(
        IMultiThreadedBackgroundTaskQueue taskQueue,
        ILogger<MultiThreadedQueuedHostedService> logger,
        IServiceProvider serviceProvider,
        int numberOfThreads = 5)
    {
        _serviceProvider = serviceProvider;
        (_taskQueue, _logger) = (taskQueue, logger);
        _numberOfThreads = numberOfThreads;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Spawn of multiple threads to process the queue
        var tasks = new List<Task>();

        for (var i = 0; i < _numberOfThreads; i++)
        {
            var newTask = Task.Factory.StartNew(() => ProcessTaskQueueAsync(i, stoppingToken),
                stoppingToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);

            tasks.Add(newTask);
        }

        await Task.WhenAll(tasks);
    }

    private async Task ProcessTaskQueueAsync(int numberThread, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            (string header, string name, Func<CancellationToken, QueueItem, ValueTask> workItem) workItem =
                await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                _logger.LogInformationOperation(LoggerStatusEnum.Start, LoggerConstant.System,
                    workItem.name, null,
                    null, "Processing background task queue item");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var queueItem = QueueItem.Create(scope.ServiceProvider, workItem.header);

                    await workItem.workItem(stoppingToken, queueItem);
                }

                _logger.LogInformationOperation(LoggerStatusEnum.Start, LoggerConstant.System,
                    workItem.name, null,
                    null, "Complete processing background task queue item");

                await Task.Delay(100, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                var errorMessage = "Failed processing background task queue item";

                _logger.LogErrorOperation(LoggerStatusEnum.EndWithError, LoggerConstant.System,
                    workItem.name, null,
                    null, errorMessage, ex);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}