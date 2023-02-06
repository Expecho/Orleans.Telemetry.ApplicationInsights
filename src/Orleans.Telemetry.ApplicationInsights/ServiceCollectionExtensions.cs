using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using System;
using System.Reflection;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansApplicationInsights(this IServiceCollection services)
        {
            return AddOrleansApplicationInsights(services, new TelemetryOptions());
        }

        public static IServiceCollection AddOrleansApplicationInsights(this IServiceCollection services, Action<TelemetryOptions> options)
        {
            var telemetryOptions = new TelemetryOptions();
            options?.Invoke(telemetryOptions);
            return AddOrleansApplicationInsights(services, telemetryOptions);
        }

        private static IServiceCollection AddOrleansApplicationInsights(IServiceCollection services, TelemetryOptions options)
        {
            services.AddSingleton(options.InterceptableGrainTypeContainer);

            if (options.EnableGrainMessagingTelemetry)
                services.AddSingleton<IIncomingGrainCallFilter, IncomingCallTelemetryLogger>()
                        .AddSingleton<IOutgoingGrainCallFilter, OutgoingCallTelemetryLogger>()
                        .AddSingleton<IOutgoingGrainCallFilter, TelemetryCorrelationProvider>();

            if (options.EnableSiloLifecycleTelemetry)
                services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>, SiloLifecycleTelemetryLogger>();

            if (options.EnableGrainLifecycleTelemetry)
                services.AddTransient(sp =>
                GrainLifecycleTelemetryLogger.Create(
                    sp.GetRequiredService<IGrainContextAccessor>(),
                    sp.GetRequiredService<TelemetryClient>()
                ));

            return services;
        }
    }

    public class TelemetryOptions
    {
        public bool EnableSiloLifecycleTelemetry { get; set; } = true;
        public bool EnableGrainLifecycleTelemetry { get; set; } = true;
        public bool EnableGrainMessagingTelemetry { get; set; } = true;
        public IInterceptableGrainTypeContainer InterceptableGrainTypeContainer { get; set; } = new DefaultInterceptableGrainTypeContainer(Assembly.GetExecutingAssembly());
    }
}