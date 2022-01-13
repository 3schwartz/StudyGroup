namespace Outbox.Web.Strategies
{
    public interface IOutboxStrategy
    {
        Task<bool> Forward();
    }
}
