using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Runtime;
using Orleans.Telemetry.ApplicationInsights.Tests.Grains;
using Orleans.Telemetry.ApplicationInsights.Tests.Helpers;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    [Collection(ClusterCollection.Name)]
    public class GrainMessaggingTelemetryLoggerTests
    {
        private readonly TestCluster _cluster;

        public GrainMessaggingTelemetryLoggerTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task WhenActorIsCalledShouldLogIncomingCall()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            var grainId = Guid.NewGuid();
            const string message = "Hello";

            var grain = _cluster.Client.GetGrain<ICallerGrain>(grainId);
            await grain.SendMessageToGrain(message);

            var telemetry = await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(_cluster);
            var expectedIncomingCallTelemetry = telemetry.GetIncomingGrainMessageTelemetry<ICallerGrain>(
                invocationId,
                g => g.SendMessageToGrain(message));

            Assert.NotNull(expectedIncomingCallTelemetry);
            Assert.Equal(grainId.ToString(), expectedIncomingCallTelemetry.Properties["grainId"]);
        }
        
        [Fact]
        public async Task WhenActorCallsOtherActorShouldLogOutgoingCall()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            var callerGrainId = Guid.NewGuid();
            const string message = "Hello";
            const string calledGrainCompoundKey = "5.keyExt";

            var grain = _cluster.Client.GetGrain<ICallerGrain>(callerGrainId);
            await grain.SendMessageToGrain(message);

            var telemetry = await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(_cluster);
            var expectedOutgoingCallTelemetry = telemetry.GetOutgoingGrainMessageTelemetry<ICalledGrain>(
                invocationId,
                g => g.ReceiveMessage(message));

            Assert.NotNull(expectedOutgoingCallTelemetry);
            Assert.Equal(calledGrainCompoundKey, expectedOutgoingCallTelemetry.Properties["grainId"]);
        }

        [Fact]
        public async Task WhenActorsCommunicateShouldCorrelateTelemetry()
        {
            var invocationId = Guid.NewGuid();
            RequestContext.Set("invocationId", invocationId);

            var callerGrainId = Guid.NewGuid();
            const string message = "Hello";

            var grain = _cluster.Client.GetGrain<ICallerGrain>(callerGrainId);
            await grain.SendMessageToGrain(message);

            var telemetry = (await TelemetryHelper.GetProducedTelemetryAsync<DependencyTelemetry>(_cluster)).ToList();
            var sourceInboundCall = telemetry.GetIncomingGrainMessageTelemetry<ICallerGrain>(invocationId,
                g => g.SendMessageToGrain(message));

            var destinationOutboundCall = telemetry.GetIncomingGrainMessageTelemetry<ICalledGrain>(invocationId,
                g => g.ReceiveMessage(message));

            var destinationInboundCall = telemetry.GetOutgoingGrainMessageTelemetry<ICalledGrain>(invocationId,
                g => g.ReceiveMessage(message));

            Assert.Equal(sourceInboundCall.Context.Operation.Id, destinationOutboundCall.Context.Operation.Id);
            Assert.Equal(sourceInboundCall.Context.Operation.Id, destinationInboundCall.Context.Operation.Id);
            Assert.Equal(sourceInboundCall.Id, destinationInboundCall.Context.Operation.ParentId);
            Assert.Equal(destinationInboundCall.Id, destinationOutboundCall.Context.Operation.ParentId);
        }
    }
}
