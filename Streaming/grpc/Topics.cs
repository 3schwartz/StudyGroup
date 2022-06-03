using System.Collections.Concurrent;

namespace grpc
{
    public class Topics
    {
        private readonly ConcurrentQueue<GrpcService.Shared.Droid> droidTopics;

        public Topics()
        {
            droidTopics = new ConcurrentQueue<GrpcService.Shared.Droid>();
        }

        public void Enqueue(GrpcService.Shared.Droid droid)
        {
            droidTopics.Enqueue(droid);
        }

        public bool TryDequeue(out GrpcService.Shared.Droid droid)
        {
            return droidTopics.TryDequeue(out droid);
        }
    }
}
