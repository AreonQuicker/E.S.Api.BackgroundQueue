using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using E.S.Api.BackgroundQueue.Interfaces;
using E.S.Api.BackgroundQueue.Models;
using Microsoft.AspNetCore.Http;

namespace E.S.Api.BackgroundQueue.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<(string token, Func<CancellationToken, QueueItem, ValueTask> workItem)>
            _queue;

        public BackgroundTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity) {FullMode = BoundedChannelFullMode.DropOldest};
            _queue =
                Channel.CreateBounded<(string, Func<CancellationToken, QueueItem, ValueTask>)>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(IHttpContextAccessor httpContextAccessor,
            Func<CancellationToken, QueueItem, ValueTask> workItem)
        {
            string header = httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];

            if (workItem is null) throw new ArgumentNullException(nameof(workItem));

            await _queue.Writer.WriteAsync((header, workItem));
        }

        public async ValueTask QueueBackgroundWorkItemAsync(
            IServiceProvider serviceProvider,
            Func<CancellationToken, QueueItem, ValueTask> workItem)
        {
            var httpContextAccessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            string header = httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];

            if (workItem is null) throw new ArgumentNullException(nameof(workItem));

            await _queue.Writer.WriteAsync((header, workItem));
        }

        public async ValueTask QueueBackgroundWorkItemAsync(string header,
            Func<CancellationToken, QueueItem, ValueTask> workItem)
        {
            if (workItem is null) throw new ArgumentNullException(nameof(workItem));

            await _queue.Writer.WriteAsync((header, workItem));
        }

        public async ValueTask QueueBackgroundWorkItemAsync(
            Func<CancellationToken, QueueItem, ValueTask> workItem)
        {
            if (workItem is null) throw new ArgumentNullException(nameof(workItem));

            await _queue.Writer.WriteAsync((null, workItem));
        }

        public async ValueTask<(string token, Func<CancellationToken, QueueItem, ValueTask> workItem)>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            var workItem =
                await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}