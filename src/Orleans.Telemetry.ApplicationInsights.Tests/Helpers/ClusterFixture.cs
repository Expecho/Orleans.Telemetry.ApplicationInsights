using System.Threading.Tasks;
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

        public Task DisposeAsync() => this.Cluster.DisposeAsync().AsTask();

        public Task InitializeAsync() => this.Cluster.DeployAsync();
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