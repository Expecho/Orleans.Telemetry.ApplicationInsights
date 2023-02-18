using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class OutgoingCallTelemetryLogger : IOutgoingGrainCallFilter
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IOptions<ClusterOptions> _clusterOptions;
        private readonly ITelemetryEnabledGrainTypeContainer _grainTypeContainer;

        public OutgoingCallTelemetryLogger(TelemetryClient telemetryClient, IOptions<ClusterOptions> clusterOptions, ITelemetryEnabledGrainTypeContainer grainTypeContainer)
        {
            _telemetryClient = telemetryClient;
            _clusterOptions = clusterOptions;
            _grainTypeContainer = grainTypeContainer;
        }

        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            if (!_grainTypeContainer.IncludeInTelemetry(context.InterfaceMethod.DeclaringType))
            {
                await context.Invoke();
                return;
            }

            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"{context.InterfaceMethod.DeclaringType?.FullName}.{context.InterfaceMethod.Name}"))
            {
                var grainId = context.TargetId;
                operation.Telemetry.Success = true;
                operation.Telemetry.Type = "Orleans Actor MessageOut";
                operation.Telemetry.Target = $"{_clusterOptions.Value.ClusterId}.{(context.Grain as Grain)?.RuntimeIdentity}.{grainId}";
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