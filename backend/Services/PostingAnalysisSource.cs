using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

internal sealed record PostingAnalysisSource(string Title, string? ShortDescription, string Description,
    PostingSourceRequirement[] Requirements)
{
    public static PostingAnalysisSource From(InternshipPosting posting) => new(posting.Title.Trim(),
        posting.ShortDescription?.Trim(), posting.Description.Trim(), posting.Requirements
            .Select(requirement => new PostingSourceRequirement(requirement.Id, requirement.Description.Trim(), requirement.Type))
            .OrderBy(requirement => requirement.Description, StringComparer.Ordinal)
            .ThenBy(requirement => requirement.Type).ThenBy(requirement => requirement.Id).ToArray());

    public string ToInput() => JsonSerializer.Serialize(this, JsonSerializerOptions.Web);

    public string Fingerprint() => Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
    {
        Title,
        ShortDescription,
        Description,
        Requirements = Requirements.Select(requirement => new { requirement.Description, requirement.Type }).Distinct()
    })));

    public bool ContainsQuote(string quote)
    {
        var normalized = CompatibilityTerms.Normalize(quote);
        return new[] { Title, ShortDescription, Description }
            .Any(field => field is not null && CompatibilityTerms.Normalize(field).Contains(normalized, StringComparison.Ordinal));
    }
}

internal sealed record PostingSourceRequirement(int Id, string Description, [property: JsonIgnore] RequirementType Type)
{
    public string Priority => Type.ToString();
}
