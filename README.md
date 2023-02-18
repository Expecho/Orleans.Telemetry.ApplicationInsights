# Orleans.Telemetry.ApplicationInsights

Send grain and silo telemetry to [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview). Orleans is already able to log metrics and traces to Application Insights but it does not support writing Application Insights specific telemetry types like custom events and dependencies out-of-the-box. This package addresses that.

## NuGet

[NuGet package for Orleans](https://www.nuget.org/packages/Orleans.Telemetry.ApplicationInsights/)

## Supported Orleans versions

The most recent version of this package works with Orleans >= 7. For the older versions of this document see [Orleans older versions](/docs/index.MD).

## Supported telemetry:

- Changes in grain lifecycle like (de)activations are logged as custom events.
- Changes in sile lifecycle are logged as custom events.
- Track grain calls as dependency telemetry.

Telemetry outputted due to grain activity is enriched with the following custom properties:

- grainId (guid/long/string/compound)
- grainType

## Usage

Configuration is done when building the silo. It is assumed the TelemetryClient is available using Dependency Injection, for examply by adding the [Microsoft.ApplicationInsights.WorkerService](https://www.nuget.org/packages/Microsoft.ApplicationInsights.WorkerService) NuGet package.

Configure the integration using the `AddOrleansApplicationInsights` extension method:

```csharp
siloBuilder
    .ConfigureServices(services =>
    {
        ...
        services
            .AddApplicationInsightsTelemetryWorkerService()
        ...
    }
    .AddOrleansApplicationInsights();
```

`AddOrleansApplicationInsights` can also be called with an overload to configure the options of the telemetry provider using `TelemetryOptions`.

```csharp
.AddOrleansApplicationInsights(options =>
{
    options.TelemetryEnabledGrainTypeContainer = 
        new DefaultTelemetryEnabledGrainTypeContainer(Assembly.GetExecutingAssembly());
})
```

The `TelemetryEnabledGrainTypeContainer` determines which grains are included in the telemetry. In the above example all grains found in any of the assemblies are included in the telemetry. To create your own container implement `ITelemetryEnabledGrainTypeContainer`.

## Tracking grain lifecycle events

Inject a GrainActivationTelemetryLogger in the grain to have it participate in the lifecycle tracking:

```csharp
public MyGrain(GrainLifecycleTelemetryLogger grainLifecycleTelemetryLogger)
{
}
```