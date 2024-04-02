// (c) 2024 @Maxylan
namespace Homie.Utilities.Converters;

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Proompted into existance through ChatGPT, to help debug serialization error deep within ASP.NET.
/// </summary>
public class MethodBaseJsonConverter : JsonConverter<MethodBase>
{
    public override MethodBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Deserialization of MethodBase instances is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, MethodBase value, JsonSerializerOptions options)
    {
        // Exclude MethodBase properties from serialization
        writer.WriteNullValue();
    }
}