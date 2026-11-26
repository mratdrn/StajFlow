using System.Text.Json.Nodes;
using StajFlow.Api.DTOs.Analysis;

namespace StajFlow.Api.Services;

internal static class PostingAnalysisContract
{
    internal const string SchemaVersion = MatchingProfileContract.SchemaVersion;
    internal static string Instruction => MatchingProfileContract.Instruction("JOB");
    internal static JsonNode CreateSchema() => MatchingProfileContract.CreateSchema();
    internal static string CreateInput(PostingAnalysisSource source) => MatchingProfileContract.CreateInput(string.Join("\n\n",
        new[] { source.Title, source.ShortDescription, source.Description }.Where(value => !string.IsNullOrWhiteSpace(value))));
    internal static MatchingProfile? Read(string? json) => MatchingProfileContract.Read(json, false);

    internal static string? Validate(string json, Action<string>? reportFailure = null,
        Action<string>? reportCorrection = null) => MatchingProfileContract.Validate(json, false, null, reportFailure, reportCorrection);
}
