using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services.Interfaces;

public interface ILlmService
{
    Task<LlmJsonResult> GenerateJsonAsync(LlmJsonRequest request, CancellationToken cancellationToken);
}

public sealed class LlmJsonRequest(string instruction, string text, JsonNode schema, Func<string, string?> validate,
    string textPropertyName = "resumeText")
{
    [JsonIgnore]
    public string Instruction { get; } = instruction;
    [JsonIgnore]
    public string Text { get; } = text;
    [JsonIgnore]
    public JsonNode Schema { get; } = schema;
    [JsonIgnore]
    public Func<string, string?> Validate { get; } = validate;
    [JsonIgnore]
    public string TextPropertyName { get; } = textPropertyName;
}

public sealed class LlmJsonResult
{
    private LlmJsonResult(string? json, AnalysisErrorCode? errorCode) => (Json, ErrorCode) = (json, errorCode);
    [JsonIgnore]
    public string? Json { get; }
    public AnalysisErrorCode? ErrorCode { get; }
    public static LlmJsonResult Success(string json) => new(json, null);
    public static LlmJsonResult Failure(AnalysisErrorCode error) => new(null, error);
}
