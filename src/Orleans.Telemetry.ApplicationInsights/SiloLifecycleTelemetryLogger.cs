using Orleans.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class SiloLifecycleTelemetryLogger : ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILocalSiloDetails _localSiloDetails;

        public SiloLifecycleTelemetryLogger(TelemetryClient telemetryClient, ILocalSiloDetails localSiloDetails)
        {
            _telemetryClient = telemetryClient;
            _localSiloDetails = localSiloDetails;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.Active, token => TrackLifecycleEvent("Orleans.Silo.ActiveStageStarted"), token => TrackLifecycleEvent("Orleans.Silo.ActiveStageEnded"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.AfterRuntimeGrainServices, token => TrackLifecycleEvent("Orleans.Silo.AfterRuntimeGrainServicesStageStarted"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.ApplicationServices, token => TrackLifecycleEvent("Orleans.Silo.ApplicationServicesStageStarted"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.BecomeActive, token => TrackLifecycleEvent("Orleans.Silo.BecomeActiveStageStarted"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.RuntimeGrainServices, token => TrackLifecycleEvent("Orleans.Silo.RuntimeGrainServicesStageStarted"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.RuntimeInitialize, token => TrackLifecycleEvent("Orleans.Silo.RuntimeInitializeStageStarted"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.RuntimeServices, token => TrackLifecycleEvent("Orleans.Silo.RuntimeServicesStageStarted"));
            lifecycle.Subscribe<SiloLifecycleTelemetryLogger>(ServiceLifecycleStage.AfterRuntimeGrainServices, token => TrackLifecycleEvent("Orleans.Silo.AfterRuntimeGrainServicesStageStarted"));
        }

        private Task TrackLifecycleEvent(string stage)
        {
            _telemetryClient.TrackEvent(stage, new Dictionary<string, string>
            {
                {"siloAddress", _localSiloDetails.SiloAddress.ToString() },
                {"siloName", _localSiloDetails.Name },
                {"clusterId", _localSiloDetails.ClusterId }
            });

            return Task.CompletedTask;
        }
    }
}
