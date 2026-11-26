using System.Text.Json.Nodes;
using StajFlow.Api.DTOs.Analysis;

namespace StajFlow.Api.Services;

public static class ResumeAnalysisContract
{
    public const string SchemaVersion = MatchingProfileContract.SchemaVersion;
    public static string Instruction => MatchingProfileContract.Instruction("CV");
    public static JsonNode CreateSchema() => MatchingProfileContract.CreateSchema();
    public static string CreateInput(string text) => MatchingProfileContract.CreateInput(text);
    internal static MatchingProfile? Read(string? json) => MatchingProfileContract.Read(json, true);

    public static string? Validate(string json, Func<string, string> removeIdentifiers,
        Action<string>? reportFailure = null, Action<string>? reportCorrection = null) =>
        MatchingProfileContract.Validate(json, true, removeIdentifiers, reportFailure, reportCorrection);
}
