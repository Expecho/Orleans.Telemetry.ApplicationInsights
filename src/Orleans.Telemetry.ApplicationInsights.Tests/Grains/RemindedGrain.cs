using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Grains
{
    public interface IRemindedGrain : IGrainWithGuidKey
    {
        Task WaitForReminder();
        Task ReceiveReminder(string reminderName, TickStatus status);
    }

    [Reentrant]
    public class RemindedGrain : Grain, IRemindedGrain, IRemindable
    {
        private readonly TaskCompletionSource _taskCompletionSource;

        public const string ReminderName = "TestReminder";


        public RemindedGrain()
        {
            _taskCompletionSource = new TaskCompletionSource();
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this.RegisterOrUpdateReminder(ReminderName, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));

            return base.OnActivateAsync(cancellationToken);
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            _taskCompletionSource.TrySetResult();

            return Task.CompletedTask;
        }

        public async Task WaitForReminder()
        {
            await _taskCompletionSource.Task;
        }
    }
}