using Prometheus;

namespace Outbox.Web.Metrics
{
    public static class PrometheusMetrics
    {
        public static readonly Histogram SequentiallyStrategyPublishHistogram = Prometheus.Metrics.CreateHistogram(
            "sequentially_strategy_publish", "Duration sequentially_strategy_publish",
            new HistogramConfiguration
            {
                LabelNames = new[] { "batchSize" }
            });

        public static readonly Histogram StateMetricHistogram = Prometheus.Metrics.CreateHistogram(
            "outbox_strategy", "Duration Histogram with injection of error state",
            new HistogramConfiguration
            {
                LabelNames = new[] {"strategy", "doneWork", "error"}
            });
    }
}
