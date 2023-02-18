using System.Reflection;

namespace Orleans.Telemetry.ApplicationInsights
{
    /// <summary>
    /// ClientTelemetryOptions defines the custom behavior of the features to add
    /// </summary>
    public class ClientTelemetryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the grain lifecycle events should be logged as custom events. Defaults to true.
        /// </summary>
        public bool EnableGrainLifecycleTelemetry { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether messaging between grains should be logged as dependency calls. Defaults to true.
        /// </summary>
        public bool EnableGrainMessagingTelemetry { get; set; } = true;

        /// <summary>
        /// Gets or sets an instance of ITelemetryEnabledGrainTypeContainer that specifies which grains are included in the generated telemetry. Defaults to a new instance of DefaultTelemetryEnabledGrainTypeContainer
        /// </summary>
        public ITelemetryEnabledGrainTypeContainer TelemetryEnabledGrainTypeContainer { get; set; } = new DefaultTelemetryEnabledGrainTypeContainer(Assembly.GetExecutingAssembly());
    }
}