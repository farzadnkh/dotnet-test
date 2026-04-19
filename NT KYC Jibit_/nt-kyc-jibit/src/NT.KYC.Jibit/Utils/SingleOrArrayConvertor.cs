using System.Collections;
using Newtonsoft.Json;

namespace NT.KYC.Jibit.Utils;

public class SingleOrArrayConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(List<>);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var itemType = objectType.GetGenericArguments()[0];

        switch (reader.TokenType)
        {
            case JsonToken.StartArray:
                return serializer.Deserialize(reader, objectType);
            case JsonToken.StartObject:
            {
                var instance = serializer.Deserialize(reader, itemType);
                var list = (IList)Activator.CreateInstance(objectType)!;
                list.Add(instance);
                return list;
            }
            case JsonToken.None:
            case JsonToken.StartConstructor:
            case JsonToken.PropertyName:
            case JsonToken.Comment:
            case JsonToken.Raw:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Null:
            case JsonToken.Undefined:
            case JsonToken.EndObject:
            case JsonToken.EndArray:
            case JsonToken.EndConstructor:
            case JsonToken.Date:
            case JsonToken.Bytes:
            default:
                return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}