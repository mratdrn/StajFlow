using System.Security.Cryptography;
using System.Text.Json;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

internal sealed record PostingAnalysisSource(string Title, string? ShortDescription, string Description)
{
    public static PostingAnalysisSource From(InternshipPosting posting) => new(posting.Title.Trim(),
        posting.ShortDescription?.Trim(), posting.Description.Trim());

    public string Fingerprint() => Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
    {
        Title,
        ShortDescription,
        Description,
        Requirements = Array.Empty<object>()
    })));
}
