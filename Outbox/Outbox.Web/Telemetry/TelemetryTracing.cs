using System.Diagnostics;
#pragma warning disable CS8603

namespace Outbox.Web.Telemetry
{
    public static class TelemetryTracing
    {
        public static readonly string ServiceVersion = "1.0.0";
        public static readonly string ServiceName = "OutboxTracing";
        public static readonly ActivitySource TracingActivitySource = new(ServiceName);

        public static Activity StartActivityWithParentWhenContextNotNull(string identifier, ActivityContext? context)
        {
            return (context != null
                       ? TracingActivitySource.StartActivity(identifier, ActivityKind.Internal,
                           (ActivityContext) context)
                       : TracingActivitySource.StartActivity(identifier));
        }
    }
}