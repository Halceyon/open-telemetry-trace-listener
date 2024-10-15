# Open Telemetry Trace Listener

This project is a trace listener for Open Telemetry, a set of APIs and libraries used for distributed tracing and observability in applications. The trace listener allows you to capture and send trace data to various backends for analysis and monitoring.

## Installation

To use the Open Telemetry Trace Listener in your .NET Framework project, you can install it via NuGet.
Simply run the following command in the NuGet Package Manager Console:

```powershell
Install-Package OpenTelemetry.TraceListener
```

## Usage with System.Diagnostics

Add the trace listener to the system.diagnostics section of your App.config or Web.config file.

```xml
<configuration>
  <system.diagnostics>
    <trace>
      <listeners>
        <add name="OpenTelemetryTraceListener"
             type="OpenTelemetry.TraceListener, OpenTelemetry.TraceListener"
             initializeData="your-backend-endpoint" />
      </listeners>
    </trace>
  </system.diagnostics>
</configuration>
```

You can now use the System.Diagnostics.Trace class to log trace information. The Open Telemetry Trace Listener will capture these traces and send them to the configured backend.

```cs
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        Trace.WriteLine("This is a trace message.");
        Trace.TraceInformation("This is an informational message.");
        Trace.TraceWarning("This is a warning message.");
        Trace.TraceError("This is an error message.");
    }
}
```

## Example

To set up logging your existing System.Diagnostics traces to a [Seq](https://datalust.co/seq) server, follow these steps:

### Add the Trace Provider to the Application Start:

```cs
private TracerProvider _tracerProvider;

protected void Application_Start(object sender, EventArgs e)
{
    var endpoint = ConfigurationManager.AppSettings["OpenTelemetry:EndPoint"] ?? "http://localhost:5341";
    var apiKey = ConfigurationManager.AppSettings["OpenTelemetry:ApiKey"] ?? "";
    var sourceName = ConfigurationManager.AppSettings["OpenTelemetry:SourceName"] ?? "Web";

    _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddAspNetInstrumentation()
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: sourceName))
            .AddSource($"{sourceName}.Logs")
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri($"{_endpoint}/ingest/otlp/v1/traces");
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                options.Headers = $"X-Seq-ApiKey={_apiKey}";
            })
        .Build();
}
```

### Dispose the trace provider on application end
```cs
protected void Application_End(object sender, EventArgs e)
{
    _tracerProvider?.Dispose();
}
```