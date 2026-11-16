using StajFlow.Api.DTOs.PostingManagement;

namespace StajFlow.Api.Services;

public enum PostingManagementError { NotFound, VersionConflict, InvalidRequirement }

public record PostingManagementResult(CompanyPostingDto? Posting, PostingManagementError? Error = null);
