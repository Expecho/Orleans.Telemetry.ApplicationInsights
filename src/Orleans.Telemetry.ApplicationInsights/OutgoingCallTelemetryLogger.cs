using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class OutgoingCallTelemetryLogger : IIncomingGrainCallFilter
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILocalSiloDetails _localSiloDetails;
        private readonly IInterceptableGrainTypeContainer _grainTypeContainer;

        public OutgoingCallTelemetryLogger(TelemetryClient telemetryClient, ILocalSiloDetails localSiloDetails, IInterceptableGrainTypeContainer grainTypeContainer)
        {
            _telemetryClient = telemetryClient;
            _localSiloDetails = localSiloDetails;
            _grainTypeContainer = grainTypeContainer;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (!_grainTypeContainer.ContainsGrain(context.ImplementationMethod.DeclaringType))
            {
                await context.Invoke();
                return;
            }

            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"{context.ImplementationMethod.DeclaringType?.FullName}.{context.ImplementationMethod.Name}"))
            {
                var grainId = context.Grain.GetGraindId();
                operation.Telemetry.Success = true;
                operation.Telemetry.Type = "Orleans Actor MessageOut";
                operation.Telemetry.Target = $"{_localSiloDetails.ClusterId}.{_localSiloDetails.SiloAddress}.{grainId}";
                operation.Telemetry.Properties["grainId"] = grainId.ToString();
                operation.Telemetry.Properties["grainType"] = context.ImplementationMethod.DeclaringType?.FullName;

                if (context.Arguments != null)
                    operation.Telemetry.Data = string.Join(", ", context.Arguments);

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