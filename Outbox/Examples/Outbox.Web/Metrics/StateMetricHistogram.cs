using System.Diagnostics;

namespace Outbox.Web.Metrics
{
    public class StateMetricHistogram : IDisposable
    {
        private readonly string strategy;
        private string error = "none";
        private bool doneWork = true;
        private Stopwatch stopwatch;

        public StateMetricHistogram(string strategy)
        {
            this.strategy = strategy;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void SetDoneWork(bool doneWork)
        {
            this.doneWork = doneWork;
        }

        public void SetError(Exception exception)
        {
            error = nameof(Exception);
        }

        public void Dispose()
        {
            stopwatch.Stop();
            PrometheusMetrics.StateMetricHistogram
                .WithLabels(strategy, doneWork.ToString(), error)
                .Observe(stopwatch.ElapsedMilliseconds);
        }
    }
}
