using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace InsuranceFraudDetection.BackgroundServices
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);
        ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }

    public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;
        private readonly ILogger<BackgroundTaskQueue> _logger;

        public BackgroundTaskQueue(int capacity, ILogger<BackgroundTaskQueue> logger)
        {
            _logger = logger;
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
        {
            if (workItem is null) throw new ArgumentNullException(nameof(workItem));

            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);
            return workItem;
        }
    }
}