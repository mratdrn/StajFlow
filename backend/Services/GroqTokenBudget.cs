using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;
using StajFlow.Api.Configurations;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class GroqTokenBudget(IOptions<GroqOptions> options)
{
    private readonly TiktokenTokenizer tokenizer = TiktokenTokenizer.CreateForEncoding("o200k_base");

    public LlmTokenBudget Estimate(LlmJsonRequest request, JsonNode schema)
    {
        var content = request.CreateUserMessage();
        var tokens = tokenizer.CountTokens(request.CreateInstruction()) + tokenizer.CountTokens(content) +
            tokenizer.CountTokens(schema.ToJsonString()) + options.Value.TokenSafetyMargin;
        return new(tokens, request.OutputTokens(options.Value.MaxOutputTokens));
    }
}
