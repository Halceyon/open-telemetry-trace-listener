using System.Configuration;
using System.Diagnostics;

namespace Diagnostics.OpenTelemetry
{
    /// <summary>
    /// Represents a trace listener that integrates with OpenTelemetry.
    /// </summary>
    public class OpenTelemetryTraceListener : System.Diagnostics.TraceListener
    {
        /// <summary>
        /// Writes trace information, including the message, to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The name of the trace source.</param>
        /// <param name="eventType">One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args"/> array.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
            {
                return;
            }
            var activityName = GetActivityName(source);
            var message = format;

            if ((args?.Length ?? 0) > 0)
            {
                message = string.Format(format, args);
            }
            CreateActivity(activityName, source, eventType, id, message, format, args);
            return;
        }

        /// <summary>
        /// Writes trace information, including the message, to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The name of the trace source.</param>
        /// <param name="eventType">One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The trace message to write.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            var activityName = GetActivityName(source);
            CreateActivity(activityName, source, eventType, id, message);
            return;
        }

        /// <summary>
        /// Writes a message to the listener specific output.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void Write(string message)
        {
            var activity = Activity.Current;
            if (activity == null)
            {
                CreateActivity("log", "log", TraceEventType.Information, 0, message);
                return;
            }
            activity.SetBaggage("message", message);
            return;
        }

        /// <summary>
        /// Writes a message followed by a line terminator to the listener specific output.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
            var activity = Activity.Current;
            if (activity == null)
            {
                CreateActivity("log", "log", TraceEventType.Information, 0, message);
                return;
            }
            activity.SetBaggage("message", message);
            return;
        }

        /// <summary>
        /// Creates an activity and sets the necessary tags based on the provided parameters.
        /// </summary>
        /// <param name="activityName">The name of the activity.</param>
        /// <param name="source">The name of the trace source.</param>
        /// <param name="eventType">One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The trace message.</param>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the <paramref name="args"/> array.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        private void CreateActivity(string activityName, string source, TraceEventType eventType, int id, string message, string format = null, params object[] args)
        {
            using (var activity = GetActivitySource().StartActivity(activityName, ActivityKind.Client))
            {
                activity?.Start();
                activity?.SetTag("source", source);
                activity?.SetTag("message", message);
                activity?.SetTag("eventType", eventType.ToString());
                activity?.SetTag("id", id);
                if (!string.IsNullOrEmpty(format))
                {
                    activity?.SetTag("format", format);
                }
                if (args?.Length > 0)
                {
                    activity?.SetTag("args", args);
                }

                if (eventType == TraceEventType.Error || eventType == TraceEventType.Critical || eventType == TraceEventType.Warning)
                {
                    activity?.SetStatus(ActivityStatusCode.Error);
                }
                else if (eventType == TraceEventType.Information)
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else if (eventType == TraceEventType.Verbose)
                {
                    activity?.SetStatus(ActivityStatusCode.Unset);
                }
                activity?.Stop();
            }
        }

        /// <summary>
        /// Gets the activity name based on the provided source.
        /// </summary>
        /// <param name="source">The name of the trace source.</param>
        /// <returns>The activity name.</returns>
        private string GetActivityName(string source)
        {
            return string.Format("Log: {0}", source);
        }

        /// <summary>
        /// Gets the activity source based on the provided configuration in OpenTelemetry:SourceName suffixed by ".Logs".
        /// </summary>
        /// <returns>The activity source.</returns>
        private ActivitySource GetActivitySource()
        {
            var sourceName = ConfigurationManager.AppSettings["OpenTelemetry:SourceName"] ?? "Web";
            return new ActivitySource($"{sourceName}.Logs");
        }
    }
}
