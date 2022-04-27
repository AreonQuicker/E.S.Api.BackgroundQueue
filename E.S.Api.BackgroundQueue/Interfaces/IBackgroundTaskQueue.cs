using System;
using System.Threading;
using System.Threading.Tasks;
using E.S.Api.BackgroundQueue.Models;
using Microsoft.AspNetCore.Http;

namespace E.S.Api.BackgroundQueue.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(IHttpContextAccessor httpContextAccessor,
            Func<CancellationToken, QueueItem, ValueTask> workItem);

        ValueTask QueueBackgroundWorkItemAsync(IServiceProvider serviceProvider,
            Func<CancellationToken, QueueItem, ValueTask> workItem);

        ValueTask QueueBackgroundWorkItemAsync(string header,
            Func<CancellationToken, QueueItem, ValueTask> workItem);

        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, QueueItem, ValueTask> workItem);

        ValueTask<(string token, Func<CancellationToken, QueueItem, ValueTask> workItem)> DequeueAsync(
            CancellationToken cancellationToken);
    }
}