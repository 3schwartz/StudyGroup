using Models;

namespace SignalRShared
{
    public class DroidAddedResponse
    {
        public Droid Droid { get; set; }
        public DateTime Timestamp { get; set; }

        public DroidAddedResponse(DateTime timestamp, Droid droid)
        {
            Timestamp = timestamp;
            Droid = droid;
        }
    }
}
