using System;

namespace E.S.Api.BackgroundQueue.Models
{
    public class QueueItem
    {
        public IServiceProvider ServiceProvider { get; init; }

        public string Header { get; init; }

        public static QueueItem Create(IServiceProvider scopeServiceProvider, object header)
        {
            return new QueueItem
            {
                ServiceProvider = scopeServiceProvider,
                Header = header?.ToString()
            };
        }
    }
}