using System.Diagnostics;
using Outbox.Web.Models;

namespace Outbox.Web.Records
{
    public record ConcurrentStrategyRecord(OutboxMessageConcurrent Message, ActivityContext? ActivityContext);
}
