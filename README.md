
# OpenTelemetry Trace Listener

This repository provides a trace listener for OpenTelemetry, a set of APIs and libraries used to achieve distributed tracing and observability within applications. The trace listener enables you to capture and transmit trace data from `System.Diagnostics` traces and logs to various backends for analysis and monitoring.

## Installation

To integrate the OpenTelemetry Trace Listener into your .NET Framework project, install the package via NuGet by running the following command in the NuGet Package Manager Console:

```powershell
Install-Package OpenTelemetry.TraceListener
```

## Usage with System.Diagnostics

To configure the OpenTelemetry Trace Listener, add it to the `system.diagnostics` section in your `App.config` or `Web.config` file:

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

After configuration, you can log trace information using the `System.Diagnostics.Trace` class. The OpenTelemetry Trace Listener will capture these traces and send them to the specified backend.

```csharp
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

## Example: Logging to Seq

To log your existing `System.Diagnostics` traces to a [Seq](https://datalust.co/seq) server, follow the steps below.

1. Install the necessary OpenTelemetry NuGet packages:

   ```powershell
   Install-Package OpenTelemetry.Exporter.Console
   Install-Package OpenTelemetry.Exporter.OpenTelemetryProtocol
   Install-Package OpenTelemetry.Instrumentation.AspNet -Version 1.9.0-beta.1
   Install-Package OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule -Version 1.9.0-beta.1
   Install-Package OpenTelemetry.TraceListener
   ```

2. Add the trace provider to the `Application_Start` event in your application:

   ```csharp
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
               options.Endpoint = new Uri($"{endpoint}/ingest/otlp/v1/traces");
               options.Protocol = OtlpExportProtocol.HttpProtobuf;
               options.Headers = $"X-Seq-ApiKey={apiKey}";
           })
           .Build();
   }
   ```

3. Update your `Web.config` to include the shared listener within the `system.diagnostics` section, and assign it to the sources you wish to log:

   ```xml
   <system.diagnostics>
       <sharedListeners>
           <add name="OpenTelemetryTraceListener"
                type="OpenTelemetry.TraceListener, OpenTelemetry.TraceListener"
                initializeData="Example.Api" />
       </sharedListeners>
       <sources>
           <source name="Bus" switchValue="All">
               <listeners>
                   <clear />
                   <add name="sqldatabase" />
                   <add name="TelemetryListener" />
               </listeners>
           </source>
           <source name="Web" switchValue="Information,Warning,Error,Critical">
               <listeners>
                   <clear />
                   <add name="sqldatabase" />
                   <add name="TelemetryListener" />
               </listeners>
           </source>
       </sources>
       <trace autoflush="true" indentsize="0">
           <listeners>
               <clear />
               <add name="OpenTelemetryTraceListener"
                    type="OpenTelemetry.TraceListener, OpenTelemetry.TraceListener"
                    initializeData="Example.Api" />
           </listeners>
       </trace>
   </system.diagnostics>
   ```

4. Ensure proper disposal of the trace provider during application shutdown:

   ```csharp
   protected void Application_End(object sender, EventArgs e)
   {
       _tracerProvider?.Dispose();
   }
   ```

## Contributing

We welcome contributions to improve this project. Please feel free to submit issues, pull requests, or feature suggestions.

## License

This project is licensed under the [MIT License](LICENSE).
