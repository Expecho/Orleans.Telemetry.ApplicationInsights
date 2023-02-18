using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using System;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class SiloBuilderExtensions
    {
        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The ISiloBuilder</param>
        /// <returns>The ISiloBuilder</returns>
        public static ISiloBuilder AddOrleansApplicationInsights(this ISiloBuilder siloBuilder)
        {
            return AddOrleansApplicationInsights(siloBuilder, new TelemetryOptions());
        }

        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The ISiloBuilder</param>
        /// <param name="options">The Action used to configure the options</param>
        /// <returns>The ISiloBuilder</returns>
        public static ISiloBuilder AddOrleansApplicationInsights(this ISiloBuilder services, Action<TelemetryOptions> options)
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
        private static ISiloBuilder AddOrleansApplicationInsights(ISiloBuilder siloBuilder, TelemetryOptions options)
        {
            siloBuilder.Services.AddSingleton(options.TelemetryEnabledGrainTypeContainer);

            if (options.EnableGrainMessagingTelemetry)
                siloBuilder.Services.AddSingleton<IIncomingGrainCallFilter, IncomingCallTelemetryLogger>()
                        .AddSingleton<IOutgoingGrainCallFilter, OutgoingCallTelemetryLogger>()
                        .AddSingleton<IOutgoingGrainCallFilter, TelemetryCorrelationProvider>();

            if (options.EnableSiloLifecycleTelemetry)
                siloBuilder.Services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>, SiloLifecycleTelemetryLogger>();

            if (options.EnableGrainLifecycleTelemetry)
                siloBuilder.Services.AddTransient(sp =>
                GrainLifecycleTelemetryLogger.Create(
                    sp.GetRequiredService<IGrainContext>(),
                    sp.GetRequiredService<TelemetryClient>()
                ));

            return siloBuilder;
        }
    }
}