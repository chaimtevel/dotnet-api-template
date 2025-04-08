using Shared.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Extensions;

public static class SerializationExtensions
{
    public static JsonSerializerOptions SetGlobalMvcSettings(this JsonSerializerOptions options)
    {
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        options.DictionaryKeyPolicy = options.PropertyNamingPolicy;
        options.PropertyNameCaseInsensitive = true;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new FlurlUrlConverter());

        return options;
    }
}
