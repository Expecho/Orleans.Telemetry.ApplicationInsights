using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using System;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class ClientBuilderExtensions
    {
        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The IClientBuilder</param>
        /// <returns>The IClientBuilder</returns>
        public static IClientBuilder AddOrleansApplicationInsights(this IClientBuilder clientBuilder)
        {
            return AddOrleansApplicationInsights(clientBuilder, new ClientTelemetryOptions());
        }

        /// <summary>
        /// Adds Application Insights Orleans Telemetry provider to service collection
        /// </summary>
        /// <param name="services">The IClientBuilder</param>
        /// <param name="options">The Action used to configure the options</param>
        /// <returns>The IClientBuilder</returns>
        public static IClientBuilder AddOrleansApplicationInsights(this IClientBuilder clientBuilder, Action<ClientTelemetryOptions> options)
        {
            var telemetryOptions = new ClientTelemetryOptions();
            options?.Invoke(telemetryOptions);
            return AddOrleansApplicationInsights(clientBuilder, telemetryOptions);
        }

        private static IClientBuilder AddOrleansApplicationInsights(IClientBuilder clientBuilder, ClientTelemetryOptions options)
        {
            clientBuilder.Services.AddSingleton(options.TelemetryEnabledGrainTypeContainer);

            if (options.EnableGrainMessagingTelemetry)
                clientBuilder.Services.AddSingleton<IIncomingGrainCallFilter, IncomingCallTelemetryLogger>()
                        .AddSingleton<IOutgoingGrainCallFilter, OutgoingCallTelemetryLogger>()
                        .AddSingleton<IOutgoingGrainCallFilter, TelemetryCorrelationProvider>();

            if (options.EnableGrainLifecycleTelemetry)
                clientBuilder.Services.AddTransient(sp =>
                GrainLifecycleTelemetryLogger.Create(
                    sp.GetRequiredService<IGrainContext>(),
                    sp.GetRequiredService<TelemetryClient>()
                ));

            return clientBuilder;
        }
    }
}