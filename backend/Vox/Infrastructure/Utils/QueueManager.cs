using System.Collections.Concurrent;

namespace Vox.Infrastructure.Utils
{
    public interface IQueueManager
    {
        Task<T> Enqueue<T>(Func<Task<T>> workItem);
        Task Enqueue(Func<Task> workItem);
    }

    public class QueueManager : IQueueManager
    {
        private readonly ConcurrentQueue<Func<Task>> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public QueueManager()
        {
            Task.Run(ProcessQueue);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> workItem)
        {
            var tcs = new TaskCompletionSource<T>();

            void Wrapper()
            {
                workItem().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        tcs.SetException(task.Exception!.InnerExceptions);
                    else if (task.IsCanceled)
                        tcs.SetCanceled();
                    else
                        tcs.SetResult(task.Result);
                });
            }

            _queue.Enqueue(() => { Wrapper(); return Task.CompletedTask; });
            _signal.Release();
            return tcs.Task;
        }

        public Task Enqueue(Func<Task> workItem)
        {
            var tcs = new TaskCompletionSource<object?>();

            _queue.Enqueue(async () =>
            {
                try
                {
                    await workItem();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            _signal.Release();
            return tcs.Task;
        }

        private async Task ProcessQueue()
        {
            while (true)
            {
                await _signal.WaitAsync();
                if (_queue.TryDequeue(out var workItem))
                {
                    await workItem();
                }
            }
        }
    }
}
