using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace StajFlow.Api.Services;

internal static class AnalysisJson
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
        MaxDepth = 32,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    internal static JsonNode CreateSchema<T>()
    {
        var schema = JsonOptions.GetJsonSchemaAsNode(typeof(T), new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });
        CompleteSchema(schema);
        return schema;
    }

    private static void CompleteSchema(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["type"]?.ToJsonString() is { } type && (type == "\"string\"" ||
                obj["type"] is JsonArray types && types.Any(item => item?.ToJsonString() == "\"string\"")))
            {
                obj["minLength"] = 1;
                obj["pattern"] = @"\S";
            }
            if (obj["properties"] is JsonObject properties)
            {
                obj["additionalProperties"] = false;
                obj["required"] = new JsonArray(properties.Select(property => (JsonNode?)JsonValue.Create(property.Key)).ToArray());
                if (properties["sourceQuote"] is JsonObject quote) quote["maxLength"] = 400;
                foreach (var name in new[] { "studyYear", "startYear", "endYear" })
                    if (properties[name] is JsonObject year) year["minimum"] = 1;
                if (properties["durationMonths"] is JsonObject duration) duration["minimum"] = 0;
            }
            foreach (var child in obj.ToArray()) CompleteSchema(child.Value);
        }
        else if (node is JsonArray array)
            foreach (var child in array) CompleteSchema(child);
    }

    internal static T? Read<T>(string? json) where T : class
    {
        if (json is null) return null;
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 32 });
            var nullability = new NullabilityInfoContext();
            if (!HasShape(document.RootElement, typeof(T), nullability)) return null;
            var node = JsonNode.Parse(json)!;
            NormalizeOptionalText(node, typeof(T), nullability);
            return node.Deserialize<T>(JsonOptions);
        }
        catch (Exception error) when (error is JsonException or NotSupportedException or ArgumentException or OverflowException)
        {
            return null;
        }
    }

    private static bool HasShape(JsonElement element, Type type, NullabilityInfoContext nullability)
    {
        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable is not null) return element.ValueKind == JsonValueKind.Null || HasShape(element, nullable, nullability);
        if (type == typeof(string)) return element.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(element.GetString());
        if (type == typeof(bool)) return element.ValueKind is JsonValueKind.True or JsonValueKind.False;
        if (type == typeof(int)) return element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out _);
        if (type == typeof(decimal)) return element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out _);
        if (type.IsEnum) return element.ValueKind == JsonValueKind.String && Array.IndexOf(Enum.GetNames(type), element.GetString()) >= 0;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return element.ValueKind == JsonValueKind.Array && element.EnumerateArray().All(item => HasShape(item, type.GenericTypeArguments[0], nullability));
        if (element.ValueKind != JsonValueKind.Object) return false;
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var values = element.EnumerateObject().ToArray();
        if (values.Length != properties.Length || values.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count() != values.Length) return false;
        foreach (var property in properties)
        {
            var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
            if (!element.TryGetProperty(name, out var child)) return false;
            if (child.ValueKind == JsonValueKind.Null && !property.PropertyType.IsValueType && nullability.Create(property).ReadState == NullabilityState.Nullable) continue;
            if (property.PropertyType == typeof(string) && child.ValueKind == JsonValueKind.String &&
                nullability.Create(property).ReadState == NullabilityState.Nullable) continue;
            if (!HasShape(child, property.PropertyType, nullability)) return false;
        }
        return true;
    }

    private static void NormalizeOptionalText(JsonNode node, Type type, NullabilityInfoContext nullability)
    {
        if (node is JsonArray array && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            foreach (var child in array)
                if (child is not null) NormalizeOptionalText(child, type.GenericTypeArguments[0], nullability);
        }
        else if (node is JsonObject obj)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                if (obj[name] is not { } child) continue;
                if (property.PropertyType == typeof(string) && nullability.Create(property).ReadState == NullabilityState.Nullable &&
                    string.IsNullOrWhiteSpace(child.GetValue<string>())) obj[name] = null;
                else NormalizeOptionalText(child, property.PropertyType, nullability);
            }
        }
    }
}
