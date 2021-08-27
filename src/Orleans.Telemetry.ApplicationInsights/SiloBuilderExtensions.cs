using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class SiloBuilderExtensions
    {
        public static ISiloBuilder AddGrainMessagingTelemetryLogger(this ISiloBuilder builder)
        {
            return builder
                .ConfigureServices(services => services.AddSingleton<IOutgoingGrainCallFilter, TelemetryCorrelationProvider>())
                .ConfigureServices(services => services.AddSingleton<IIncomingGrainCallFilter, IncomingCallTelemetryLogger>());
        }
    }
}