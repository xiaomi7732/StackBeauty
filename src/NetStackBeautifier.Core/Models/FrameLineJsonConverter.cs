using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetStackBeautifier.Core;

public class FrameLineJsonConverter : JsonConverter<IFrameLine>
{
    private const string TypeDiscriminatorConstant = "TypeDiscriminator";
    private const string ValuePropertyName = "TypeValue";
    private enum TypeDiscriminator
    {
        FrameRawText = 1,
        FrameItem = 2,
    }

    public override IFrameLine? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // At the start of an object
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        // The very first property has to be the type discriminator
        if (!reader.Read()
            || reader.TokenType != JsonTokenType.PropertyName
            || !string.Equals(reader.GetString(), TypeDiscriminatorConstant, options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new JsonException("The very first property should be " + TypeDiscriminatorConstant);
        }

        // The type discriminator is a number
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"The type discriminator suppose to be a number. Actual: {reader.TokenType}");
        }

        IFrameLine? result;
        TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
        CheckTypeValue(ref reader);
        switch (typeDiscriminator)
        {
            case TypeDiscriminator.FrameRawText:
                result = JsonSerializer.Deserialize<FrameRawText>(ref reader, options);
                break;
            case TypeDiscriminator.FrameItem:
                result = (FrameItem?)JsonSerializer.Deserialize<FrameItem>(ref reader, options);
                break;
            default:
                throw new NotSupportedException($"Unsupported type discriminator value: {typeDiscriminator}");
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException();
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, IFrameLine value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value is FrameRawText frameRawText)
        {
            writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.FrameRawText);
            writer.WritePropertyName(ValuePropertyName);
            JsonSerializer.Serialize(writer, frameRawText);
        }
        else if (value is FrameItem frameItem)
        {
            writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.FrameItem);
            writer.WritePropertyName(ValuePropertyName);
            JsonSerializer.Serialize(writer, frameItem);
        }
        else
        {
            throw new NotSupportedException("Not support to serialize type" + value.GetType());
        }

        writer.WriteEndObject();
    }

    private void CheckTypeValue(ref Utf8JsonReader reader)
    {
        if (!reader.Read() || reader.GetString() != ValuePropertyName)
        {
            throw new JsonException();
        }
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
    }
}