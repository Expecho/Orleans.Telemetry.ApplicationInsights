using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Runtime;
using Orleans.Telemetry.ApplicationInsights.Tests.Grains;
using Orleans.Telemetry.ApplicationInsights.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    public class GrainMessaggingTelemetryLoggerTests : ClusterFixture
    {
        public GrainMessaggingTelemetryLoggerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task WhenGrainsIsCalledShouldLogIncomingCall()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            const string message = "Hello";

            var grain = Cluster.Client.GetGrain<ICallerGrain>(Guid.NewGuid());
            await grain.SendMessageToGrain(message);

            var telemetry = await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(Cluster);
            var expectedIncomingCallTelemetry = telemetry.GetIncomingGrainMessageTelemetry<CallerGrain>(
                invocationId,
                g => g.SendMessageToGrain(message));

            Assert.NotNull(expectedIncomingCallTelemetry);
            Assert.Equal(grain.GetGrainId().ToString(), expectedIncomingCallTelemetry.Properties["grainId"]);
        }
        
        [Fact]
        public async Task WhenGrainsCallsOtherGrainsShouldLogOutgoingCall()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            const string message = "Hello";
            
            var grain = Cluster.Client.GetGrain<ICallerGrain>(Guid.NewGuid());
            var targetGrainId = await grain.SendMessageToGrain(message);

            var telemetry = await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(Cluster);
            var expectedOutgoingCallTelemetry = telemetry.GetOutgoingGrainMessageTelemetry<ICalledGrain>(
                invocationId,
                g => g.ReceiveMessage(message));

            Assert.NotNull(expectedOutgoingCallTelemetry);
            Assert.Equal(targetGrainId, expectedOutgoingCallTelemetry.Properties["grainId"]);
        }

        [Fact]
        public async Task WhenGrainsCommunicateShouldCorrelateTelemetry()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            const string message = "Hello";

            var grain = Cluster.Client.GetGrain<ICallerGrain>(Guid.NewGuid());
            await grain.SendMessageToGrain(message);

            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(Cluster)).ToList();
            var sourceInboundCall = telemetry.GetIncomingGrainMessageTelemetry<CallerGrain>(invocationId,
                g => g.SendMessageToGrain(message));

            var destinationOutboundCall = telemetry.GetIncomingGrainMessageTelemetry<CalledGrain>(invocationId,
                g => g.ReceiveMessage(message));

            var destinationInboundCall = telemetry.GetOutgoingGrainMessageTelemetry<ICalledGrain>(invocationId,
                g => g.ReceiveMessage(message));

            Assert.Equal(sourceInboundCall.Context.Operation.Id, destinationOutboundCall.Context.Operation.Id);
            Assert.Equal(sourceInboundCall.Context.Operation.Id, destinationInboundCall.Context.Operation.Id);
            Assert.Equal(sourceInboundCall.Id, destinationInboundCall.Context.Operation.ParentId);
            Assert.Equal(destinationInboundCall.Id, destinationOutboundCall.Context.Operation.ParentId);
        }

        [Fact]
        public async Task WhenGrainRecievesReminderShouldLogIncomingCall()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            var grain = Cluster.Client.GetGrain<IRemindedGrain>(Guid.NewGuid());
            await grain.WaitForReminder();

            var telemetry = await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(Cluster);
            var expectedIncomingCallTelemetry = telemetry.GetIncomingGrainMessageTelemetry<RemindedGrain>(
                g => g.ReceiveReminder(RemindedGrain.ReminderName, new TickStatus()));

            Assert.NotNull(expectedIncomingCallTelemetry);
            Assert.Equal(grain.GetGrainId().ToString(), expectedIncomingCallTelemetry.Properties["grainId"]);
        }
    }
}
