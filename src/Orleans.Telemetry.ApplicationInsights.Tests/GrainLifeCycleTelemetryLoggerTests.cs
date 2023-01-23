using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Telemetry.ApplicationInsights.Tests.Grains;
using Orleans.Telemetry.ApplicationInsights.Tests.Helpers;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    [Collection(ClusterCollection.Name)]
    public class GrainLifeCycleTelemetryLoggerTests
    {
        private readonly TestCluster _cluster;

        public GrainLifeCycleTelemetryLoggerTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task WhenGrainStartedShouldLogLifecycle()
        {
            var grainId = Guid.NewGuid();
            var grain = _cluster.Client.GetGrain<ILifeCycleTestGrain>(grainId);
            await grain.Activate();
            
            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<EventTelemetry>(_cluster)).ToList();
            GetLifecyceEventTelemetry(telemetry, "Orleans.Grain.ActiveStateStarted", grainId);
            GetLifecyceEventTelemetry(telemetry, "Orleans.Grain.SetupStateStarted", grainId);
        }

        [Fact]
        public async Task WhenGrainDeactivateShouldLogLifecycle()
        {
            var grainId = Guid.NewGuid();
            var grain = _cluster.Client.GetGrain<ILifeCycleTestGrain>(grainId);
            await grain.DeActivate();

            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<EventTelemetry>(_cluster, TimeSpan.FromSeconds(5))).ToList();
            GetLifecyceEventTelemetry(telemetry, "Orleans.Grain.ActiveStateEnded", grainId);
        }

        private static EventTelemetry GetLifecyceEventTelemetry(IEnumerable<EventTelemetry> telemetry, string stage, Guid grainId)
        {
            return telemetry.Single(t => 
                t.Name == stage &&
                t.Properties["grainId"] == grainId.ToString() &&
                t.Properties["grainType"] == typeof(LifeCycleTestGrain).FullName);
        }
    }
}