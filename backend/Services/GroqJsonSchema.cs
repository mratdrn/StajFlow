using System.Text.Json.Nodes;

namespace StajFlow.Api.Services;

internal static class GroqJsonSchema
{
    internal static JsonNode Create(JsonNode source)
    {
        var schema = source.DeepClone();
        Normalize(schema);
        return schema;
    }

    private static void Normalize(JsonNode? node)
    {
        if (node is not JsonObject schema) return;
        foreach (var name in new[] { "pattern", "minLength", "maxLength", "minimum", "maximum", "default" })
            schema.Remove(name);
        if (schema["enum"] is JsonArray && schema["type"] is null) schema["type"] = "string";
        foreach (var name in new[] { "properties", "$defs", "definitions" })
            if (schema[name] is JsonObject properties)
                foreach (var property in properties) Normalize(property.Value);
        Normalize(schema["items"]);
        if (schema["additionalProperties"] is JsonObject additional) Normalize(additional);
        foreach (var name in new[] { "anyOf", "allOf", "oneOf" })
            if (schema[name] is JsonArray variants)
                foreach (var variant in variants) Normalize(variant);
    }
}
