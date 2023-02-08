using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using System;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The IServiceCollection</param>
        /// <returns>The IServiceCollection</returns>
        public static IServiceCollection AddOrleansApplicationInsights(this IServiceCollection services)
        {
            return AddOrleansApplicationInsights(services, new TelemetryOptions());
        }

        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The IServiceCollection</param>
        /// <param name="options">The Action used to configure the options</param>
        /// <returns>The IServiceCollection</returns>
        public static IServiceCollection AddOrleansApplicationInsights(this IServiceCollection services, Action<TelemetryOptions> options)
        {
            var telemetryOptions = new TelemetryOptions();
            options?.Invoke(telemetryOptions);
            return AddOrleansApplicationInsights(services, telemetryOptions);
        }

        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The IServiceCollection</param>
        /// <param name="options">The Options instance used to configure with</param>
        /// <returns>The IServiceCollection</returns>
        private static IServiceCollection AddOrleansApplicationInsights(IServiceCollection services, TelemetryOptions options)
        {
            services.AddSingleton(options.TelemetryEnabledGrainTypeContainer);

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
}