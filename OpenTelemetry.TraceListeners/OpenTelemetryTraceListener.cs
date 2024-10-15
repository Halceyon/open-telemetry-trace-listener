using System.Configuration;
using System.Diagnostics;

namespace OpenTelemetry.TraceListeners
{
    public class OpenTelemetryTraceListener : TraceListener
    {
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

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            var activityName = GetActivityName(source);
            CreateActivity(activityName, source, eventType, id, message);
            return;
        }

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

        private string GetActivityName(string source)
        {
            return string.Format("Log: {0}", source);
        }

        private ActivitySource GetActivitySource()
        {
            var sourceName = ConfigurationManager.AppSettings["OpenTelemetry:SourceName"] ?? "Web";
            return new ActivitySource($"{sourceName}.Logs");
        }
    }
}