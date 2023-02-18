using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.TestingHost;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.Telemetry.ApplicationInsights.Tests.Helpers
{
    public class ClusterFixture : IAsyncLifetime
    {
        public ClusterFixture(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;

            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
            Cluster = builder.Build();
        }

        public TestCluster Cluster { get; }

        public ITestOutputHelper TestOutputHelper { get; }

        public Task DisposeAsync() => Cluster.DisposeAsync().AsTask();

        public Task InitializeAsync() => Cluster.DeployAsync();
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
                        .AddSingleton<ITelemetryInitializer, UnitTestTelemetryCollector>();
                })
                .AddOrleansApplicationInsights(options =>
                {
                    options.TelemetryEnabledGrainTypeContainer =
                        new DefaultTelemetryEnabledGrainTypeContainer(Assembly.GetExecutingAssembly());
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddDebug()
                        .AddConsole();
                })
                .UseInMemoryReminderService();
        }
    }
}