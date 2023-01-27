using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Telemetry.ApplicationInsights.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    public class SiloLifeCycleTelemetryLoggerTests : ClusterFixture
    {
        public SiloLifeCycleTelemetryLoggerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task WhenSiloStartedShouldLogLifecycle()
        {
            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<EventTelemetry>(Cluster)).ToList();
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
                t.Properties["siloAddress"] == Cluster.Primary.SiloAddress.ToString() &&
                t.Properties["siloName"] == Cluster.Primary.Name &&
                t.Properties["clusterId"] == Cluster.Options.ClusterId);
        }
    }
}