﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public class IncomingCallTelemetryLogger : IIncomingGrainCallFilter
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IOptions<ClusterOptions> _clusterOptions;
        private readonly ITelemetryEnabledGrainTypeContainer _grainTypeContainer;

        public IncomingCallTelemetryLogger(TelemetryClient telemetryClient, IOptions<ClusterOptions> clusterOptions, ITelemetryEnabledGrainTypeContainer grainTypeContainer)
        {
            _telemetryClient = telemetryClient;
            _clusterOptions = clusterOptions;
            _grainTypeContainer = grainTypeContainer;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            var typeName = context.Grain.GetType().FullName;

            if (!_grainTypeContainer.IncludeInTelemetry(context.InterfaceMethod.DeclaringType) && context.Grain is not IRemindable)
            {
                await context.Invoke();
                return;
            }

            var parentTraceId = RequestContext.Get(TelemetryCorrelationProvider.ParentId)?.ToString();
            var operationId = RequestContext.Get(TelemetryCorrelationProvider.OperationId)?.ToString();

            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"{typeName}.{context.InterfaceMethod.Name}"))
            {
                var grainId = context.TargetId;
                operation.Telemetry.Context.Operation.ParentId = parentTraceId;
                operation.Telemetry.Context.Operation.Id = operationId;
                operation.Telemetry.Success = true;
                operation.Telemetry.Type = "Orleans Actor MessageIn";
                operation.Telemetry.Target = $"{_clusterOptions.Value.ClusterId}.{(context.Grain as Grain)?.RuntimeIdentity}.{grainId}";
                operation.Telemetry.Properties["grainId"] = grainId.ToString();
                operation.Telemetry.Properties["grainType"] = typeName;

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
