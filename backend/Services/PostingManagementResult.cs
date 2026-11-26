using StajFlow.Api.DTOs.PostingManagement;

namespace StajFlow.Api.Services;

public enum PostingManagementError { NotFound, VersionConflict, InvalidRequirement, InvalidDeadline, InvalidDescription }

public record PostingManagementResult(CompanyPostingDto? Posting, PostingManagementError? Error = null);
