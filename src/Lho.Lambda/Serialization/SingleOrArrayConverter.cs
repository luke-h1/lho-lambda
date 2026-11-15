using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lho.Lambda.Serialization;

// Last.fm returns "track" as a bare object when the list has exactly one
// item, and omits or nulls it for an empty chart.
public sealed class SingleOrArrayConverter<T> : JsonConverter<IReadOnlyList<T>>
{
    public override IReadOnlyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var items = new List<T>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                var item = JsonSerializer.Deserialize<T>(ref reader, options);
                if (item is not null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        var single = JsonSerializer.Deserialize<T>(ref reader, options);
        return single is null ? [] : [single];
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }

        writer.WriteEndArray();
    }
}
