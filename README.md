# Orleans.Telemetry.ApplicationInsights

Send grain and silo telemetry to [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview). Orleans is already able to log metrics and traces to Application Insights but it does not support writing Application Insights specific telemetry types like custom events and dependendencies out-of-the-box. This package addresses that.

Related packages:
- Metrics publisher: [Microsoft.Orleans.OrleansTelemetryConsumers.AI](https://www.nuget.org/packages/Microsoft.Orleans.OrleansTelemetryConsumers.AI/)
- Trace publisher: [Microsoft.Extensions.Logging.ApplicationInsights](https://www.nuget.org/packages/Microsoft.Extensions.Logging.ApplicationInsights)

## Supported telemetry:

- Changes in grain lifecycle like (de)activations are logged as custom events.
- Changes in sile lifecycle are logged as custom events.
- Track grain calls as dependency telemetry.

Telemetry outputted due to grain activity is enriched with the following custom properties:

- grainId (guid/long/string/compound)
- grainType

## Usage

Configuration is done when building the silo. It is assumed the TelemetryClient is available using Dependency Injection.

### Tracking calls between grains

```csharp
siloBuilder.AddGrainMessagingTelemetryLogger();
```

### Tracking grain lifecycle events

```csharp
services.AddGrainLifecycleTelemetryLogger();
```

Inject a GrainActivationTelemetryLogger in the grain to have it paticipate in the lifecycle tracking:

```csharp
public MyGrain(GrainLifecycleTelemetryLogger grainLifecycleTelemetryLogger)
{
}
```

### Tracking silo lifecycle events

```csharp
services.AddSiloLifecycleTelemetryLogger();
```

The following properties are added to the event telemetry: siloName, siloAddress and clusterId.
