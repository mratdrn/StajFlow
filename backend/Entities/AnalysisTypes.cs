using System.Text.Json.Serialization;

namespace StajFlow.Api.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<AnalysisStatus>))]
public enum AnalysisStatus { Pending, Processing, Completed, Failed }

[JsonConverter(typeof(JsonStringEnumConverter<AnalysisErrorCode>))]
public enum AnalysisErrorCode
{
    FileNotFound, InvalidPdf, EncryptedPdf, NoText, UnreadableText, FileTooLarge,
    ReadFailed, ProviderUnavailable, InvalidResponse, ProcessingFailed, Cancelled,
    AuthenticationFailed, ModelUnavailable, QuotaExceeded, InputTooLong, ConfigurationError, ContentBlocked
}

[JsonConverter(typeof(JsonStringEnumConverter<CriterionCategory>))]
public enum CriterionCategory { TechnicalSkill, Project, Experience, Education }

[JsonConverter(typeof(JsonStringEnumConverter<EvidenceStatus>))]
public enum EvidenceStatus { EvidenceFound, DeclarationOnly, NoEvidence }

[JsonConverter(typeof(JsonStringEnumConverter<ExperienceType>))]
public enum ExperienceType { Unspecified, Work, Internship }

[JsonConverter(typeof(JsonStringEnumConverter<EvaluationStatus>))]
public enum EvaluationStatus { Calculated, NotCalculated }

[JsonConverter(typeof(JsonStringEnumConverter<EvaluationUnavailableReason>))]
public enum EvaluationUnavailableReason { AnalysisPending, AnalysisFailed, InsufficientData, NoMeasurableCriteria }

[JsonConverter(typeof(JsonStringEnumConverter<CriterionMatchStatus>))]
public enum CriterionMatchStatus { Matched, NoEvidence }

[JsonConverter(typeof(JsonStringEnumConverter<RequiredCriteriaStatus>))]
public enum RequiredCriteriaStatus { AllMatched, MissingEvidence, NoRequiredCriteria, Unavailable }
