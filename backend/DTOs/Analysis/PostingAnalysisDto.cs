using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Analysis;

public sealed record PostingAnalysisDto(int Id, int PostingId, int Version, string SchemaVersion, AnalysisStatus Status,
    DateTimeOffset CreatedAt, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt, AnalysisErrorCode? ErrorCode,
    string? Message, MatchingProfile? Data, bool RequiresReanalysis = false);
