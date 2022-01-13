namespace Outbox.Web.Configurations
{
    public class KafkaOptions
    {
        public string OutboxTopic { get; set; } = "outbox";
        public string BootstrapServer { get; set; } = "localhost:9093";
        public string ConnectTopic { get; internal set; } = "postgres-outbox-messages";
        public string GroupId { get; set; } = Guid.NewGuid().ToString();
    }
}
