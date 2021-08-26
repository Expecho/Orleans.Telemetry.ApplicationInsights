using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGrainLifecycleTelemetryLogger(this IServiceCollection services)
        {
            return services.AddTransient(sp =>
                GrainLifecycleTelemetryLogger.Create(
                    sp.GetRequiredService<IGrainActivationContext>(),
                    sp.GetRequiredService<TelemetryClient>()
                ));
        }

        public static IServiceCollection AddSiloLifecycleTelemetryLogger(this IServiceCollection services)
        {
            return services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>, SiloLifecycleTelemetryLogger>();
        }
    }
}