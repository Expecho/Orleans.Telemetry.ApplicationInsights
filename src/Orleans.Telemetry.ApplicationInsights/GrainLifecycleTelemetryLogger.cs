using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class GrainLifecycleTelemetryLogger : ILifecycleParticipant<IGrainLifecycle>
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IGrainActivationContext _context;

        public GrainLifecycleTelemetryLogger(TelemetryClient telemetryClient, IGrainActivationContext context)
        {
            _telemetryClient = telemetryClient;
            _context = context;
        }

        public static GrainLifecycleTelemetryLogger Create(IGrainActivationContext context, TelemetryClient telemetryClient)
        {
            var component = new GrainLifecycleTelemetryLogger(telemetryClient, context);
            component.Participate(context.ObservableLifecycle);
            return component;
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe<GrainLifecycleTelemetryLogger>(GrainLifecycleStage.Activate, token => TrackLifecycleEvent("Orleans.Grain.ActiveStateStarted"), token => TrackLifecycleEvent("Orleans.Grain.ActiveStateEnded"));
            lifecycle.Subscribe<GrainLifecycleTelemetryLogger>(GrainLifecycleStage.SetupState, token => TrackLifecycleEvent("Orleans.Grain.SetupStateStarted"));
        }

        private Task TrackLifecycleEvent(string stage)
        {
            _telemetryClient.TrackEvent(stage, new Dictionary<string, string>
            {
                {"grainId", _context.GrainInstance.GetGraindId().ToString()},
                {"grainType", _context.GrainType.FullName}
            });

            return Task.CompletedTask;
        }
    }
}