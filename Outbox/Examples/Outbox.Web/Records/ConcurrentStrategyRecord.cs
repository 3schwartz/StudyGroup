using System.Diagnostics;
using Outbox.Web.Models;

namespace Outbox.Web.Records
{
    public record StrategyRecord(IOutboxMessage Message, ActivityContext? ActivityContext);
}
