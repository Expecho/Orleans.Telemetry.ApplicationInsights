using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Telemetry.ApplicationInsights.Tests.Grains;
using Orleans.Telemetry.ApplicationInsights.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    public class GrainLifeCycleTelemetryLoggerTests : ClusterFixture
    {
        public GrainLifeCycleTelemetryLoggerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task WhenGrainStartedShouldLogLifecycle()
        {
            var grainId = Guid.NewGuid();
            var grain = Cluster.Client.GetGrain<ILifeCycleTestGrain>(grainId);
            await grain.Activate();
            
            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<EventTelemetry>(Cluster)).ToList();
            GetLifecyceEventTelemetry(telemetry, "Orleans.Grain.ActiveStateStarted", grain.GetGrainId().ToString());
            GetLifecyceEventTelemetry(telemetry, "Orleans.Grain.SetupStateStarted", grain.GetGrainId().ToString());
        }

        [Fact]
        public async Task WhenGrainDeactivateShouldLogLifecycle()
        {
            var grainId = Guid.NewGuid();
            var grain = Cluster.Client.GetGrain<ILifeCycleTestGrain>(grainId);
            await grain.DeActivate();

            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<EventTelemetry>(Cluster, TimeSpan.FromSeconds(5))).ToList();
            GetLifecyceEventTelemetry(telemetry, "Orleans.Grain.ActiveStateEnded", grain.GetGrainId().ToString());
        }

        private static EventTelemetry GetLifecyceEventTelemetry(IEnumerable<EventTelemetry> telemetry, string stage, string grainId)
        {
            return telemetry.Single(t => 
                t.Name == stage &&
                t.Properties["grainId"] == grainId &&
                t.Properties["grainType"] == typeof(LifeCycleTestGrain).FullName);
        }
    }
}