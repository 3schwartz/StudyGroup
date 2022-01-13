using System.ComponentModel.DataAnnotations.Schema;

namespace Outbox.Web.Models
{
    [Table("outbox_messages_connect")]
    public class OutboxMessageConnect : IOutboxMessage
    {
        public Guid Id { get; set; } 
        public string Message { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    }
}
