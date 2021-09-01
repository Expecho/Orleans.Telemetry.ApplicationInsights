using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Telemetry.ApplicationInsights.Tests.Helpers;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    [Collection(ClusterCollection.Name)]
    public class SiloLifeCycleTelemetryLoggerTests
    {
        private readonly TestCluster _cluster;

        public SiloLifeCycleTelemetryLoggerTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task WhenSiloStartedShouldLogLifecycle()
        {
            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<EventTelemetry>(_cluster)).ToList();
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.ActiveStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.AfterRuntimeGrainServicesStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.ApplicationServicesStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.BecomeActiveStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.RuntimeGrainServicesStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.RuntimeInitializeStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.RuntimeServicesStageStarted");
            GetLifecyceEventTelemetry(telemetry, "Orleans.Silo.AfterRuntimeGrainServicesStageStarted");
        }

        private EventTelemetry GetLifecyceEventTelemetry(IEnumerable<EventTelemetry> telemetry, string stage)
        {
            return telemetry.First(t =>
                t.Name == stage &&
                t.Properties["siloAddress"] == _cluster.Primary.SiloAddress.ToString() &&
                t.Properties["siloName"] == _cluster.Primary.Name &&
                t.Properties["clusterId"] == _cluster.Options.ClusterId);
        }
    }
}