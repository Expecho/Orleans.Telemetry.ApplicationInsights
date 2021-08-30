using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    public static class TelemetryHelper
    {
        public static async Task<IEnumerable<ITelemetry>> GetProducedTelemetryAsync(TestCluster cluster)
        {
            return await GetProducedTelemetryAsync<ITelemetry>(cluster);
        }

        public static async Task<IEnumerable<T>> GetProducedTelemetryAsync<T>(TestCluster cluster) where T : ITelemetry
        {
            var telemetryItems = new List<T>();

            foreach (var silo in cluster.GetActiveSilos().OfType<InProcessSiloHandle>())
            {
                telemetryItems.AddRange(await GetProducedTelemetryOfSiloAsync<T>(silo));
            }

            return telemetryItems.Distinct();
        }

        private static async Task<IEnumerable<T>> GetProducedTelemetryOfSiloAsync<T>(InProcessSiloHandle siloHandle) where T : ITelemetry
        {
            var serviceProvider = siloHandle.SiloHost.Services;

            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
            await telemetryClient.FlushAsync(CancellationToken.None);

            var collector = (UnitTestTelemetryCollector)serviceProvider.GetServices<ITelemetryInitializer>().First(service => service is UnitTestTelemetryCollector);
            return collector.Telemetry.OfType<T>();
        }
    }
}