using Newtonsoft.Json;

namespace Outbox.Web.Models
{
    public class ConnectMessage
    {
        public Schema Schema { get; set; }
        public Payload Payload { get; set; }

        public OutboxMessage ParseOutboxMessage()
        {
            return new OutboxMessage
            {
                Id = Payload.Id,
                Message = Payload.Message,
                CreatedDateTime = Payload.CreatedDateTime
            };
        }
    }

    public class Schema
    {
    }

    public class Payload
    {
        public Guid Id { get; set; }
        public string? Message { get; set; }

        [JsonConverter(typeof(UTCDateTimeConverter))]
        public DateTime CreatedDateTime { get; set; }
    }

    public class UTCDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
