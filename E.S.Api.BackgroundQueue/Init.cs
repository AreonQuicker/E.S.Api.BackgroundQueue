using E.S.Api.BackgroundQueue.HostedServices;
using E.S.Api.BackgroundQueue.Interfaces;
using E.S.Api.BackgroundQueue.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace E.S.Api.BackgroundQueue;

public static class Init
{
    public static void AddBackgroundQueue(this IServiceCollection services, int capacity)
    {
        services.AddSingleton<IBackgroundTaskQueue>(_ => new BackgroundTaskQueue(capacity));
        services.AddHostedService<QueuedHostedService>();
    }

    public static void AddMultiThreadedBackgroundQueue(this IServiceCollection services, int capacity,
        int numberOfThreads = 5)
    {
        services.AddSingleton<IMultiThreadedBackgroundTaskQueue>(
            _ => new MultiThreadedBackgroundTaskQueue(capacity));

        services.AddHostedService(s =>
            new MultiThreadedQueuedHostedService(s.GetRequiredService<IMultiThreadedBackgroundTaskQueue>(),
                s.GetRequiredService<ILogger<MultiThreadedQueuedHostedService>>(),
                s.GetRequiredService<IServiceProvider>(), numberOfThreads));
    }
}