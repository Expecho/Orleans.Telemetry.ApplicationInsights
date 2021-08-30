using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights.Tests
{
    public class UnitTestTelemetryCollector : ITelemetryInitializer
    {
        private readonly List<ITelemetry> _telemetry = new();

        public IReadOnlyList<ITelemetry> Telemetry => _telemetry.AsReadOnly();

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is ISupportProperties item)
            {
                item.Properties["invocationId"] = RequestContext.Get("invocationId")?.ToString();
            }

            _telemetry.Add(telemetry);
        }
    }
}