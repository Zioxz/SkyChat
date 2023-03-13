using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coflnet.Sky.Chat
{
    public partial class Startup
    {
        public class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.SpecifyKind(DateTime.Parse(reader.GetString()), DateTimeKind.Utc);
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
            }
        }
    }
}
