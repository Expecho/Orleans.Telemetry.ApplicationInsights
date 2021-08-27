using System.Diagnostics;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class TelemetryCorrelationProvider : IOutgoingGrainCallFilter
    {
        internal const string ParentId = "ParentId";
        internal const string OperationId = "OperationId";

        private readonly IInterceptableGrainTypeContainer _grainTypeContainer;

        public TelemetryCorrelationProvider(IInterceptableGrainTypeContainer grainTypeContainer)
        {
            _grainTypeContainer = grainTypeContainer;
        }

        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            if (_grainTypeContainer.ContainsGrain(context.InterfaceMethod.DeclaringType))
            {
                RequestContext.Set(ParentId, Activity.Current?.SpanId);
                RequestContext.Set(OperationId, Activity.Current?.RootId);
            }

            await context.Invoke();
        }
    }
}