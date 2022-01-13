using System.ComponentModel.DataAnnotations.Schema;

namespace Outbox.Web.Models
{
    [Table("outbox_messages_immutable")]
    public class OutboxMessageImmutable : IOutboxMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    }
}
