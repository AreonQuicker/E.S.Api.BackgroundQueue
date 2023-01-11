using System.Runtime.CompilerServices;
using E.S.Api.BackgroundQueue.Models;
using Microsoft.AspNetCore.Http;

namespace E.S.Api.BackgroundQueue.Interfaces;

public interface IMultiThreadedBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(IHttpContextAccessor httpContextAccessor,
        Func<CancellationToken, QueueItem, ValueTask> workItem, [CallerMemberName] string callerMemberName = "");

    ValueTask QueueBackgroundWorkItemAsync(IServiceProvider serviceProvider,
        Func<CancellationToken, QueueItem, ValueTask> workItem, [CallerMemberName] string callerMemberName = "");

    ValueTask<(string token, string name, Func<CancellationToken, QueueItem, ValueTask> workItem)> DequeueAsync(
        CancellationToken cancellationToken);
}