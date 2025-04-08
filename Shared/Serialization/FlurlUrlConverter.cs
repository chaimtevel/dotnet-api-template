using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Serialization;

public class FlurlUrlConverter : JsonConverter<Flurl.Url>
{
    public override Flurl.Url Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return new Flurl.Url(value);
        }
        else
        {
            return new Flurl.Url();
        }
    }

    public override void Write(Utf8JsonWriter writer, Flurl.Url value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
