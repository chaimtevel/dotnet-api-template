using Shared.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Constants;

public static class Serialization
{
    public static JsonSerializerOptions DefaultSystemJsonOptions { get; } = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    static Serialization()
    {
        DefaultSystemJsonOptions.Converters.Add(new JsonStringEnumConverter());
        DefaultSystemJsonOptions.Converters.Add(new FlurlUrlConverter());
    }
}
