using System;

namespace Orleans.Telemetry.ApplicationInsights
{
    public interface IInterceptableGrainTypeContainer
    {
        bool ContainsGrain(Type grain);
    }
}