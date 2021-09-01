using System;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Helpers
{
    public sealed class ClusterFixture : IDisposable
    {
        public ClusterFixture()
        {
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
        }

        public TestCluster Cluster { get; }
    }

    [CollectionDefinition(Name)]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = "ClusterCollection";
    }

    public class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ITelemetryChannel, InMemoryChannel>()
                        .AddApplicationInsightsTelemetryWorkerService(options =>
                        {
                            options.DeveloperMode = true;
                            options.EnableAdaptiveSampling = false;
                            options.EnableEventCounterCollectionModule = false;
                            options.EnableHeartbeat = false;
                            options.EnablePerformanceCounterCollectionModule = false;
                            options.EnableQuickPulseMetricStream = false;
                        })
                        .AddSingleton<IInterceptableGrainTypeContainer>(_ => 
                            new DefaultInterceptableGrainTypeContainer(Assembly.GetExecutingAssembly()))
                        .AddGrainLifecycleTelemetryLogger()
                        .AddSiloLifecycleTelemetryLogger()
                        .AddSingleton<IOutgoingGrainCallFilter, OutgoingCallTelemetryLogger>()
                        .AddSingleton<ITelemetryInitializer, UnitTestTelemetryCollector>();
                })
                .AddGrainMessagingTelemetryLogger();
        }
    }
}