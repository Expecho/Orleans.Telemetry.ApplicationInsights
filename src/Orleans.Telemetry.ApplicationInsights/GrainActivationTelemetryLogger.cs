using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class GrainActivationTelemetryLogger : ILifecycleParticipant<IGrainLifecycle>
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IGrainActivationContext _context;

        public GrainActivationTelemetryLogger(TelemetryClient telemetryClient, IGrainActivationContext context)
        {
            _telemetryClient = telemetryClient;
            _context = context;
        }

        public static GrainActivationTelemetryLogger Create(IGrainActivationContext context, TelemetryClient telemetryClient)
        {
            var component = new GrainActivationTelemetryLogger(telemetryClient, context);
            component.Participate(context.ObservableLifecycle);
            return component;
        }

        public void Participate(IGrainLifecycle lifecycle)
        {
            lifecycle.Subscribe<GrainActivationTelemetryLogger>(GrainLifecycleStage.Activate, OnActivate, OnDeactivate);
            lifecycle.Subscribe<GrainActivationTelemetryLogger>(GrainLifecycleStage.SetupState, OnSetup);
        }

        private Task OnActivate(CancellationToken ct)
        {
            TrackLifecycleEvent("Orleans.ActivateGrain");
            return  Task.CompletedTask;
        }

        private Task OnSetup(CancellationToken ct)
        {
            TrackLifecycleEvent("Orleans.SetupGrain");
            return Task.CompletedTask;
        }

        private Task OnDeactivate(CancellationToken ct)
        {
            TrackLifecycleEvent("Orleans.DeactivateGrain");
            return Task.CompletedTask;
        }

        private void TrackLifecycleEvent(string stage)
        {
            _telemetryClient.TrackEvent(stage, new Dictionary<string, string>
            {
                {"grainId", _context.GrainInstance.GetGraindId().ToString()},
                {"grainType", _context.GrainType.FullName}
            });
        }
    }
}