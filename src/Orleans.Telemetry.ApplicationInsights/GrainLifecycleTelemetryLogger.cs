using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class GrainLifecycleTelemetryLogger : ILifecycleParticipant<IGrainLifecycle>
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IGrainContextAccessor _context;

        public GrainLifecycleTelemetryLogger(TelemetryClient telemetryClient, IGrainContextAccessor context)
        {
            _telemetryClient = telemetryClient;
            _context = context;
        }

        public static GrainLifecycleTelemetryLogger Create(IGrainContextAccessor context, TelemetryClient telemetryClient)
        {
            var component = new GrainLifecycleTelemetryLogger(telemetryClient, context);
            component.Participate(context.GrainContext.ObservableLifecycle);
            return component;
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe<GrainLifecycleTelemetryLogger>(GrainLifecycleStage.Activate,
                token =>
                {
                    SetCorrelationDataOnActivation();
                    return TrackLifecycleEvent("Orleans.Grain.ActiveStateStarted");
                },
                token => 
                    TrackLifecycleEvent("Orleans.Grain.ActiveStateEnded"));
            lifecycle.Subscribe<GrainLifecycleTelemetryLogger>(GrainLifecycleStage.SetupState, token => TrackLifecycleEvent("Orleans.Grain.SetupStateStarted"));
        }

        private void SetCorrelationDataOnActivation()
        {
            Activity.Current?
                .AddBaggage("grainId", _context.GrainContext.GrainId.ToString())
                .AddBaggage("grainType", _context.GrainContext.GrainInstance.GetType().FullName);
        }

        private Task TrackLifecycleEvent(string stage)
        {
            _telemetryClient.TrackEvent(stage, new Dictionary<string, string>
            {
                {"grainId", _context.GrainContext.GrainId.ToString()},
                {"grainType", _context.GrainContext.GrainInstance.GetType().FullName}
            });

            return Task.CompletedTask;
        }
    }
}