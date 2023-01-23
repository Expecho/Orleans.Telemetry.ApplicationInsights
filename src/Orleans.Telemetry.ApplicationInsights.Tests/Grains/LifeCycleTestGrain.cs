using System.Threading.Tasks;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Grains
{
    public interface ILifeCycleTestGrain : IGrainWithGuidKey
    {
        Task Activate();
        Task DeActivate();
    }

    public class LifeCycleTestGrain : Grain, ILifeCycleTestGrain
    {
        public LifeCycleTestGrain(GrainLifecycleTelemetryLogger grainLifecycleTelemetryLogger)
        {

        }

        public Task Activate()
        {
            return Task.CompletedTask;
        }

        public Task DeActivate()
        {
            DeactivateOnIdle();

            return Task.CompletedTask;
        }
    }
}