using System.Runtime.CompilerServices;
using System.Threading.Channels;
using E.S.Api.BackgroundQueue.Interfaces;
using E.S.Api.BackgroundQueue.Models;
using Microsoft.AspNetCore.Http;

namespace E.S.Api.BackgroundQueue.Services;

public class MultiThreadedBackgroundTaskQueue : IMultiThreadedBackgroundTaskQueue
{
    private readonly Channel<(string token, string name, Func<CancellationToken, QueueItem, ValueTask> workItem)>
        _multiThreadedQueue;

    public MultiThreadedBackgroundTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity) {FullMode = BoundedChannelFullMode.DropOldest};
        _multiThreadedQueue =
            Channel.CreateBounded<(string, string, Func<CancellationToken, QueueItem, ValueTask>)>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(IHttpContextAccessor httpContextAccessor,
        Func<CancellationToken, QueueItem, ValueTask> workItem, [CallerMemberName] string callerMemberName = "")
    {
        if (workItem is null) throw new ArgumentNullException(nameof(workItem));

        string header = httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];

        await _multiThreadedQueue.Writer.WriteAsync((header, callerMemberName, workItem));
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        IServiceProvider serviceProvider,
        Func<CancellationToken, QueueItem, ValueTask> workItem, [CallerMemberName] string callerMemberName = "")
    {
        if (workItem is null) throw new ArgumentNullException(nameof(workItem));

        var httpContextAccessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        string header = httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];

        await _multiThreadedQueue.Writer.WriteAsync((header, callerMemberName, workItem));
    }

    public async ValueTask<(string token, string name, Func<CancellationToken, QueueItem, ValueTask> workItem)>
        DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem =
            await _multiThreadedQueue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}