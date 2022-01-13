using System.ComponentModel.DataAnnotations.Schema;

namespace Outbox.Web.Models
{
    [Table("outbox_messages")]
    public class OutboxMessage : IOutboxMessage
    {
        public Guid Id { get; set; } 
        public string Message { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    }
}
