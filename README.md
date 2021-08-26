# Orleans.Telemetry.ApplicationInsights

Send grain telemetry to [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview). Orleans is already able to log metrics and traces to Application Insights but it does not support writing Application Insights specific telemetry types like custom events and dependendencies out-of-the-box. 

## Supported telemetry:

- Changes in grain lifecycle like (de)activations are logged as custom events.
- Changes in sile lifecycle are logged as custom events.
- Track incoming and/or outgoing grains calls as dependency telemetry.

Telemetry outputted due to grain activity is enriched with the following custom properties:

- grainId (guid/long/string/compound)
- grainType

## Configuring

Configuration is done when building the silo. It is assumed the TelemetryClient is available using Dependency Injection.

### Tracking incoming/outgoing calls

```csharp
siloBuilder.AddIncomingGrainCallFilter<IncomingCallTelemetryLogger>();
siloBuilder.AddIncomingGrainCallFilter<OutgoingCallTelemetryLogger>();
```

### Tracking grain lifecycle events

```csharp
services.AddGrainLifecycleTelemetryLogger();
```

Inject a GrainActivationTelemetryLogger in the grain to have it paticipate in the lifecycle tracking:

```csharp
public MyGrain(GrainActivationTelemetryLogger grainActivationTelemetryLogger)
{
}
```

### Tracking siloe lifecycle events

```csharp
services.AddSiloLifecycleTelemetryLogger();
```
