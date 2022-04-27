using E.S.Api.BackgroundQueue.HostedServices;
using E.S.Api.BackgroundQueue.Interfaces;
using E.S.Api.BackgroundQueue.Services;
using Microsoft.Extensions.DependencyInjection;

namespace E.S.Api.BackgroundQueue
{
    public static class Init
    {
        public static void AddBackgroundQueue(this IServiceCollection services, int capacity)
        {
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue>(_ => new BackgroundTaskQueue(capacity));
        }
    }
}