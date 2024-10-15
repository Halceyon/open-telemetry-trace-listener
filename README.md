# Open Telemetry Trace Listener

This project is a trace listener for Open Telemetry, a set of APIs and libraries used for distributed tracing and observability in applications. The trace listener allows you to capture and send trace data to various backends for analysis and monitoring.

## Installation

To use the Open Telemetry Trace Listener in your .NET Framework project, you can install it via NuGet. Simply run the following command in the NuGet Package Manager Console:

```powershell
Install-Package OpenTelemetry.TraceListener
```

## Usage with System.Diagnostics

To use the Open Telemetry Trace Listener with existing System.Diagnostics trace logging, follow these steps:

Configure the Trace Listener in your configuration file: Add the trace listener to the system.diagnostics section of your App.config or Web.config file.

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

Use System.Diagnostics.Trace in your code: You can now use the System.Diagnostics.Trace class to log trace information. The Open Telemetry Trace Listener will capture these traces and send them to the configured backend.

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