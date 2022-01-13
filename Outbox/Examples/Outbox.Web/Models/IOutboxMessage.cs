namespace Outbox.Web.Models
{
    public interface IOutboxMessage
    {
        Guid Id { get; set; }
        string Message { get; set; }

        DateTime CreatedDateTime { get; set; }
    }
}
