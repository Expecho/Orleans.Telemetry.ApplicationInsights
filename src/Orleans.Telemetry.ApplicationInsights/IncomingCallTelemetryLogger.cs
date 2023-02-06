using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class IncomingCallTelemetryLogger : IIncomingGrainCallFilter
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILocalSiloDetails _localSiloDetails;
        private readonly IInterceptableGrainTypeContainer _grainTypeContainer;

        public IncomingCallTelemetryLogger(TelemetryClient telemetryClient, ILocalSiloDetails localSiloDetails, IInterceptableGrainTypeContainer grainTypeContainer)
        {
            _telemetryClient = telemetryClient;
            _localSiloDetails = localSiloDetails;
            _grainTypeContainer = grainTypeContainer;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            var typeName = context.Grain switch
            {
                _ when context.Grain is IRemindable remindableGrain => (context.Grain as IRemindable).GetType(),
                _ => context.InterfaceMethod.DeclaringType
            };

            if (!_grainTypeContainer.ContainsGrain(context.InterfaceMethod.DeclaringType))
            {
                await context.Invoke();
                return;
            }

            var parentTraceId = RequestContext.Get(TelemetryCorrelationProvider.ParentId)?.ToString();
            var operationId = RequestContext.Get(TelemetryCorrelationProvider.OperationId)?.ToString();

            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"{context.InterfaceMethod.DeclaringType?.FullName}.{context.InterfaceMethod.Name}"))
            {
                var grainId = context.TargetId;
                operation.Telemetry.Context.Operation.ParentId = parentTraceId;
                operation.Telemetry.Context.Operation.Id = operationId;
                operation.Telemetry.Success = true;
                operation.Telemetry.Type = "Orleans Actor MessageIn";
                operation.Telemetry.Target = $"{_localSiloDetails.ClusterId}.{_localSiloDetails.SiloAddress}.{grainId}";
                operation.Telemetry.Properties["grainId"] = grainId.ToString();
                operation.Telemetry.Properties["grainType"] = context.InterfaceMethod.DeclaringType?.FullName;

                var arguments = Enumerable.Range(0, context.Request.GetArgumentCount()).Select(context.Request.GetArgument);
                if (arguments.Any())
                    operation.Telemetry.Data = string.Join(", ", arguments);

                try
                {
                    await context.Invoke();
                }
                catch
                {
                    operation.Telemetry.Success = false;
                    throw;
                }
            }
        }
    }
}
